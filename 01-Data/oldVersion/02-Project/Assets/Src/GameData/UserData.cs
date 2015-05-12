using UnityEngine;
using System.Xml;
using System.Collections;
using GoogleMobileAds.Api;

public class UserData : MonoBehaviour {

	private static UserData _instence;

	// 보유 자산.
	/** 사용자 자산 */
	public int UserMoney = 0;
	/** 사용자 보유 코어 */
	public int UserCores = 0;
	/** 사용자 보유 코어 파편 */
	public int UserChips = 0;

	/** 보유 부대 정보. */
	public ArrayList UserUnitList;
	/** 보유 대원 정보. */
	public ArrayList UserMemberList;
	/** 오픈된 기술 정보. */
	public ArrayList UserResearchList;
	/** 제작중인 장비 정보. */
	public ArrayList UserMakeGearList;
	/** 보유중인 장비 정보. */
	public ArrayList UserOwnGearList;

	// 기지 상태 정보.
	/** 기지레벨 - 보유 가능 부대수 증가 */
	public byte BaseLevel = 1;
	/** 연구소레벨 - 연구 가능 종류 확장 */
	public byte ResearchLevel = 1;
	/** 공장레벨 - 생산 보유량 증가 */
	public byte FactoryLevel = 1;
	/** 스토리 진행 스텝 ID */
	public int StoryStepId = 0;

	/** 유저 베너 대기 카운트. */
	public int UserBannerStandByCount;

	public ArrayList UserCandidateMemberIds;	// 후보 대원 정보.

	public ArrayList UserMissionList;		// 사용자 미션 정보.

	public bool IsShowAlert = false;

	public int TestData = 0;

	public string SingleCountValues = "000000000000";	// 유저별 한번만 보여주는 정보.

	/** 세금 징수 가능 유무 */
	public bool IsGetTax = false;

	// 기지 상태 가이드 정보.
	private int[,] _BaseLevelGuide;
	private int[,] _ResearchLevelGuide;
	private int[,] _FactoryLevelGuide;

	private SystemData _SystemData;
	private ScriptData _ScriptData;

	private float _Timer;

	// 옵션 정보.
	public bool Option_BGM = false;
	public bool Option_Sound = true;

	public GUISkin AlertSkin;
	private GUIStyle _AlertButtnGUIStyle;
	private GUIStyle _AlertTextGUIStyle;

	public delegate void FristUpdateComplete();
	private FristUpdateComplete _FristUpdateComplete;

	public delegate void ConfirmPopupCallback(bool isOk);
	private ConfirmPopupCallback _ConfirmPopupCallback;

	private int _CurrentFPS = 0;
	private long _CurrentMemory = 0;

	public bool IsGUIShow = true;

	private GameObject _UnitBodyObj;

	private short _PlayStep = 17;

	public static UserData getInstence()	{
		if(_instence == null) 
		{
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<UserData>();
			DontDestroyOnLoad(_instence);
		}

		return _instence;
	}
	
	void Awake() {

		AlertSkin = Instantiate(Resources.Load<GUISkin>("Skin/AlertSkin")) as GUISkin;

		_SystemData = SystemData.GetInstance();
		_ScriptData = ScriptData.getInstence();

		UserUnitList = new ArrayList();
		UserMemberList = new ArrayList();
		UserResearchList = new ArrayList();
		UserMakeGearList = new ArrayList();
		UserOwnGearList = new ArrayList();

		// 기지 관련 기본 가이드 정보.
		TextAsset textAsset = (TextAsset)Resources.Load("XMLData/BaseGuide",typeof(TextAsset));
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(textAsset.text);
		
		_BaseLevelGuide = new int[xmlDoc["BaseGuide"].ChildNodes.Count, 3];
		_ResearchLevelGuide = new int[xmlDoc["BaseGuide"].ChildNodes.Count, 2];
		_FactoryLevelGuide = new int[xmlDoc["BaseGuide"].ChildNodes.Count, 3];

		int xmlIndex = 0;
		foreach(XmlElement xmlElement in xmlDoc["BaseGuide"]) {
			
			_BaseLevelGuide[xmlIndex, 0] = System.Convert.ToInt32(xmlElement["baseLevel"].InnerText);
			_BaseLevelGuide[xmlIndex, 1] = System.Convert.ToInt32(xmlElement["maxSoilders"].InnerText);
			_BaseLevelGuide[xmlIndex, 2] = System.Convert.ToInt32(xmlElement["basePay"].InnerText);

			_ResearchLevelGuide[xmlIndex, 0] = System.Convert.ToInt32(xmlElement["researchLevel"].InnerText);
			_ResearchLevelGuide[xmlIndex, 1] = System.Convert.ToInt32(xmlElement["researchPay"].InnerText);

			_FactoryLevelGuide[xmlIndex, 0] = System.Convert.ToInt32(xmlElement["factoryLevel"].InnerText);
			_FactoryLevelGuide[xmlIndex, 1] = System.Convert.ToInt32(xmlElement["factoryLine"].InnerText);
			_FactoryLevelGuide[xmlIndex, 2] = System.Convert.ToInt32(xmlElement["factoryPay"].InnerText);
			
			xmlIndex ++;
		}

		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	void Update() {
		// 안드로이드 뒤로가기 버튼 처리
		if(Application.platform == RuntimePlatform.Android)
		{
			if(Input.GetKey(KeyCode.Escape))
			{
				Application.Quit();
			}

		}

		if(UserMemberList.Count > 0) {
			_Timer += Time.deltaTime;

			if(_Timer > 1) {

				_CurrentFPS = (int)(1.0f / Time.smoothDeltaTime);
				_CurrentMemory = System.GC.GetTotalMemory(false);
				//_CurrentMemory = UnityEngine.SystemInfo.systemMemorySize + UnityEngine.SystemInfo.graphicsMemorySize;

				if(StoryStepId >= _PlayStep && WarzoneData.getInstence() != null && WarzoneData.getInstence().CurrentFightType == FightType.Idle) {
					MemberHPAndMPUpdate();
					UpdateTown();
				}
				_Timer = 0;
			}
		}
	}

	public IEnumerator SetUnitBodyData(short memberId, short sortNum, bool isMemorySave) {

		yield return new WaitForEndOfFrame();

		if(_UnitBodyObj == null) _UnitBodyObj = Instantiate(Resources.Load<GameObject>("UnitResource/UnitBody")) as GameObject;

		_UnitBodyObj.GetComponent<UnitBody>().IsMemorySave = isMemorySave;
		yield return StartCoroutine(_UnitBodyObj.GetComponent<UnitBody>().SetUnitBody(memberId, sortNum));

	}

	public GameObject GetUnitBody() {
		if(_UnitBodyObj == null) _UnitBodyObj = Instantiate(Resources.Load<GameObject>("UnitResource/UnitBody")) as GameObject;
		return _UnitBodyObj;
	}

	/** 초기에 갱신 데이터들을 업데이트 합니다. */
	public void FristUpdate(FristUpdateComplete OnComplete) {
		print("FristUpdate");
		_FristUpdateComplete = new FristUpdateComplete(OnComplete);
		bool isUpdate = UpdateTown();

		while(isUpdate == true) {
			isUpdate = UpdateTown();
			MemberHPAndMPUpdate();
		}
		isUpdate = MemberHPAndMPUpdate();
		while(isUpdate == true) {
			isUpdate = MemberHPAndMPUpdate();
		}

		// 장비 생산 연산.

		_FristUpdateComplete();
	}

	public UserMission GetUserMission(short missionId) {
		foreach(UserMission userMission in UserMissionList) {
			if(missionId == userMission.defaultMissionId) return userMission;
		}
		return null;
	}

	/**
	 * 전투 후 타운 정보를 갱신함.
	 * */
	public void SetFightEndUpdateTown() {

		// 마을 성장.
		Town defaultTown;
		byte ghostAttackTownCount = 0;
		foreach(UserTown userTown in WarzoneData.getInstence()._UserTownData) {
			defaultTown = WarzoneData.getInstence().GetDefaultTownData(userTown.id);
			int townNextResident = UnityEngine.Random.Range(2, _SystemData.TownNextResidentCount);
			userTown.resident += (short)(townNextResident);
			if(userTown.resident > _SystemData.TownMaxResidentCount) userTown.resident = _SystemData.TownMaxResidentCount;
			if(userTown.isInvasion) ghostAttackTownCount ++;
		}

		// 마을 공격.
		int attackRandom = UnityEngine.Random.Range(_SystemData.AccumulateTownAttackCount, 100);
		if(attackRandom >= 80 && ghostAttackTownCount < _SystemData.MaxSameTownAttackCount) {	// 공격 조건
			GhostTown ghostTown;
			ArrayList isAttackTownIDs = new ArrayList();
			foreach(UserTown attackUserTown in WarzoneData.getInstence()._UserTownData) {
				if(attackUserTown.isInvasion) continue;
				ghostTown = WarzoneData.getInstence().GetGhostTownByRootId(attackUserTown.id, true);
				if(ghostTown == null) continue;
				isAttackTownIDs.Add(attackUserTown.id);
				break;
			}
			if(isAttackTownIDs.Count == 0) return;	// 공격 가능한 마을이 없음.
			// 공격 처리.
			int isAttackTownIndex = UnityEngine.Random.Range(0, isAttackTownIDs.Count);
			byte isAttackTownId = (byte)(isAttackTownIDs[isAttackTownIndex]);
			UserTown isAttackTown = WarzoneData.getInstence().GetUserTownByID(isAttackTownId);
			GhostTown isAttackGhost = WarzoneData.getInstence().GetGhostTownByRootId(isAttackTownId, true);
			if(isAttackTown == null || isAttackGhost == null) return;
			isAttackTown.isInvasion = true;
			isAttackTown.invasionGhostClose = (short)(isAttackGhost.ghostClose / 4);
			long endTime = _SystemData.TownInvasionDelay * _SystemData.millisecondNum;
			isAttackTown.lastInvasionEndTime = _SystemData.getCurrentTime() + endTime;

			_SystemData.AccumulateTownAttackCount = 0;
		} else {
			_SystemData.AccumulateTownAttackCount += 20;
		}

		LocalData.getInstence().UserTownDataSave();
	}

	/** 대원들의 HP와MP를 갱신합니다.*/
	private bool MemberHPAndMPUpdate() {
		//print("MemberHPAndMPUpdate");
		bool isUpdate = false;
		bool isSave = false;
		bool isNotiCancel = true;
		foreach(Member member in UserMemberList) {
			if(member.state == MemberStateType.Wound || member.state == MemberStateType.Mia) continue;
			if(member.CurrentHP < member.MaxHP || member.CurrentMP < member.MaxMP) {
				long currentTime = _SystemData.getCurrentTime();
				if(member.CurrentHP < member.MaxHP) {
					// hp 부족시.
					if(member.lastHPUpdateTime < currentTime) {
						member.CurrentHP += _SystemData.MemberHpPlusCount;
						if(member.CurrentHP > member.MaxHP) member.CurrentHP = member.MaxHP;
						member.lastHPUpdateTime += (_SystemData.MemberHpPlusDelay * _SystemData.millisecondNum);
						isSave = true;
						if(member.lastHPUpdateTime < currentTime) isUpdate = true;
					}
				}
				if(member.CurrentMP < member.MaxMP) {
					// mp 부족시.
					if(member.lastMPUpdateTime < currentTime) {
						member.CurrentMP += _SystemData.MemberMpPlusCount;
						if(member.CurrentMP > member.MaxMP) member.CurrentMP = member.MaxMP;
						member.lastMPUpdateTime += (_SystemData.MemberMpPlusDelay * _SystemData.millisecondNum);
						isSave = true;

						if(member.lastMPUpdateTime < currentTime) isUpdate = true;
					}
				}
				isNotiCancel = false;
			} else {
				continue;
			}
		}

		// 맴버 정보가 모두 회복 되었으면 노티 제거.
		if(isNotiCancel == true) NotificationManager.getInstence().CancelMemberCareNoti();

		if(isSave) LocalData.getInstence().UserMemberDataSave();

		return isUpdate;
	}

	/** 마을 정보를 갱신합니다. */
	private bool UpdateTown() {
		try {

			bool isUpdate = false;
			bool ghostTownSave = false;
			int index = 0;

			// 마을 공격 체크.
			IsGetTax = false;
			long currentTime = _SystemData.getCurrentTime();
			long taxDelay = _SystemData.TownTaxCollectdelay * _SystemData.millisecondNum;
			byte delTownId = 0;
			foreach(UserTown userTown in WarzoneData.getInstence().GetUserTownList()) {
				Town userDefaultTown = WarzoneData.getInstence().GetDefaultTownData(userTown.id);
				if(userTown.isInvasion && userTown.lastInvasionEndTime < currentTime) {
					userTown.isInvasion = false;
					//userTown.resident -= _SystemData.TownDamage;
					userTown.resident -= (short)(userTown.invasionGhostClose / 2);
					if(userTown.resident < 0) userTown.resident = 0;

					string massge = _ScriptData.GetGameScript(userDefaultTown.townNameId).script;
					_SystemData.AddMessage(massge + _ScriptData.GetGameScript(150122).script + "(-" + (userTown.invasionGhostClose / 2) + ")", Color.yellow);
					LocalData.getInstence().UserTownDataSave();

				}



				if((userTown.lastTaxTime + taxDelay) <= currentTime) IsGetTax = true;

			}

			if(IsGetTax == true) NotificationManager.getInstence().CancelTaxNoti();

			// 둥지 성장.
			foreach(GhostTown ghostTown in WarzoneData.getInstence().GetGhostTownList()) {

				// 둥지 성장.
				index = 0;
				bool isGhostTownUpdate = true;
				while(isGhostTownUpdate) {
					isGhostTownUpdate = GhostTownUpdate(ghostTown);
					if(isGhostTownUpdate == true) ghostTownSave = true;
					index ++;
				}
			}

			if(ghostTownSave) LocalData.getInstence().GhostTownDataSave();

			return isUpdate;

		} catch (System.Exception e) {
			if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("Error", "UpdateTown");
			Debug.LogError(e.Data.ToString());
		}

		return false;
	}

	/** 마을 1회 성장 갱신함. */
	private bool UserTownUpdate(UserTown userTown) {
		bool nextUpdate = false;

		Town userDefaultTown = WarzoneData.getInstence().GetDefaultTownData(userTown.id);
		string userTownName = _ScriptData.GetGameScript(userDefaultTown.townNameId).script;

		//print("userTown.lastResidentPlusTime : " + (_SystemData.getCurrentTime() - userTown.lastResidentPlusTime));
		if(userTown.lastResidentPlusTime < _SystemData.getCurrentTime()) {
			if(userTown.resident >= _SystemData.TownMaxResidentCount) return true;
			long nextUpTime = _SystemData.TownNextResidentTime * _SystemData.millisecondNum;
			// 성장 카운트.
			short updateCount = (short)((_SystemData.getCurrentTime() - userTown.lastResidentPlusTime) / nextUpTime);
			if(updateCount < 0 || updateCount > 30000) updateCount = 30000;	// 오버 플로우 방어.
			updateCount += 1;

			userTown.lastResidentPlusTime += nextUpTime * updateCount;

			int nextResident = UnityEngine.Random.Range((int)(_SystemData.TownNextResidentCount / 2), (int)(_SystemData.TownNextResidentCount));
			userTown.resident += (short)(nextResident * updateCount);
			//print("resident : " + (short)(nextResident * updateCount));
			if(userTown.resident > _SystemData.TownMaxResidentCount) userTown.resident = _SystemData.TownMaxResidentCount;
			nextUpdate = true;
		}

		return nextUpdate;
	}

	/** 마을의 1회의 공격 유무를 갱신함. */
	private bool UserTownAttackUpdate(UserTown userTown) {
		bool nextUpdate = false;
		Town userDefaultTown = WarzoneData.getInstence().GetDefaultTownData(userTown.id);
		GhostTown attackGhostTown = WarzoneData.getInstence().GetGhostTownByRootId(userTown.id, true);
		if(attackGhostTown == null) {
			if(userTown.id >= 15) {
				attackGhostTown = new GhostTown();
				attackGhostTown.id = 100;
				attackGhostTown.ghostClose = (short)(userDefaultTown.maxClose * 1.5f);
			} else {
				//userTown.isInvasion = false;
				return nextUpdate;
			}

		}
		//if(userTown.id == 1) print("UserTownAttackUpdate : " + (_SystemData.getCurrentTime() - userTown.lastInvasionEndTime));
		//if(userTown.id == 1) print("GhostAttachDelay : " + (_SystemData.GhostAttachDelay * _SystemData.millisecondNum));

		// 다음 공격 시간 저장.
		if(userTown.lastInvasionEndTime < _SystemData.getCurrentTime()) {	// 마을 침략이 오버된 상태.
			// 침략 카운트.
			short attackCount = (short)((_SystemData.getCurrentTime() - userTown.lastInvasionEndTime) / (_SystemData.GhostAttachDelay * _SystemData.millisecondNum));
			if(attackCount < 0 || attackCount > 30000) attackCount = 30000;	// 오버 플로우 방어.
			//if(userTown.id == 1) print("attackCount : " + attackCount);
			attackCount += 1;
			userTown.resident -= (short)(_SystemData.TownDamage * attackCount);
			userTown.isInvasion = false;
			userTown.lastInvasionEndTime += (_SystemData.GhostAttachDelay * _SystemData.millisecondNum) * (attackCount);
			if(userTown.resident <= 0) {
				userTown.resident = 0;
			} 
			string massge = _ScriptData.GetGameScript(userDefaultTown.townNameId).script;
			_SystemData.AddMessage(massge + _ScriptData.GetGameScript(150122).script + "(-" + (_SystemData.TownDamage * attackCount) + ")", Color.yellow);
			nextUpdate = true;

		} else {	// 침략 중인 경우.
			long invasionTime = userTown.lastInvasionEndTime - (_SystemData.TownInvasionDelay * _SystemData.millisecondNum);
			userTown.isInvasion = false;
			userTown.invasionGhostClose = (short)(attackGhostTown.ghostClose / 3);
			userTown.invasionGhostTownId = attackGhostTown.id;
			if(invasionTime <= _SystemData.getCurrentTime()) {
				userTown.isInvasion = true;
			} else {
				userTown.isInvasion = false;
			}

		}

		return nextUpdate;
	}

	/** 고스트 둥지의 1회 성장을 설정함. */
	private bool GhostTownUpdate(GhostTown ghostTown) {
		bool nextUpdate = false;
		Town town = WarzoneData.getInstence().GetDefaultTownData(ghostTown.id);
		if(town.maxClose == ghostTown.ghostClose) return nextUpdate;

		if(ghostTown.lastClosePlusTime < _SystemData.getCurrentTime()) {
			int nextClose = UnityEngine.Random.Range((int)(_SystemData.GhostTownNextCount * 0.8f), (int)(_SystemData.GhostTownNextCount));
			ghostTown.ghostClose += (short)(nextClose);

			ghostTown.lastClosePlusTime += _SystemData.GhostTownNextTime * _SystemData.millisecondNum;

			if(town.maxClose < ghostTown.ghostClose) ghostTown.ghostClose = town.maxClose;
			nextUpdate = true;
		}
		return nextUpdate;
	}

// Getter ==========================================================================================================

	/**
	 * Id에 해당하는 부대를 반환.
	 * */
	public Unit GetUnitById(byte id) {
		foreach(Unit unit in UserUnitList) {
			if(unit.id == id) return unit;
		}
		return null;
	}

	/**
	 * Id에 해당하는 맴버를 반환.
	 * */
	public Member GetMemberById(short id) {
		foreach(Member member in UserMemberList) {
			if(member.id == id) {
				return member;
			}
		}
		return null;
	}

	public ArrayList GetMemberList() {
		return UserMemberList;
	}

	/** 부대에 소속되지 않은 맴버 리스트를 반환함. */
	public ArrayList GetUnitNotMembers() {
		ArrayList memberIdList = new ArrayList();
		foreach(Member member in UserMemberList) {
			if(member.UnitId == 0) memberIdList.Add(member.id);
		}
		return memberIdList;
	}

	/** 해당 레벨에 해당하는 최대 수용 병력 수 */
	public int GetBaseSoildersByLevel(byte level) {
		int soilderCount = 0;
		soilderCount = _BaseLevelGuide[level - 1, 1];
		return soilderCount;
	}

	/** 해당 레벨로 업그레이드에 필요한 비용을 반환 */
	public int GetBasePayByLevel(byte level) {
		int basepay = 0;
		basepay = _BaseLevelGuide[level - 1, 2];
		return basepay;
	}

	/** 기지 연구소 레벨 업그레이드에 필요한 비용을 반환. */
	public int GetResearchPayByLevel(byte level) {
		int researchpay = 0;
		researchpay = _ResearchLevelGuide[level - 1, 1];
		return researchpay;
	}

	/** 해당 레벨에 해당하는 공장 예약 수를 반환함. */
	public int GetFactoryLineByLevel(byte level) {
		int factoryline = 0;
		factoryline = _FactoryLevelGuide[level - 1, 1];
		return factoryline;
	}

	/** 해당 레벨에 해당하는 공장 레벨로 업그레이드에 필요한 비용을 반환. */
	public int GetFactoryPayByLevel(byte level) {
		int factorypay = 0;
		factorypay = _FactoryLevelGuide[level - 1, 2];
		return factorypay;
	}

	/** Research에 해당 하는 UserResearch를 반환 */
	public UserResearch GetUserResearchByReserch(Research research) {
		GetIsResearching();
		foreach(UserResearch userResearch in UserResearchList) {
			if(userResearch.id == research.id) {
				return userResearch;
			}
		}

		return null;
	}

	/** 현재 제작중인 장비 목록을 반환 또한 장비의 생산 시간을 갱신. */
	public ArrayList GetMakeGearList() {
		OwnGear ownGear;
		ArrayList delMakeGearidList = new ArrayList();
		try {
			if(UserMakeGearList.Count <= 0) return UserMakeGearList;
			foreach(MakeGear makeGear in UserMakeGearList) {
				if(makeGear.endTime < _SystemData.getCurrentTime()) {
					ownGear = GetOwnGearByGearId(makeGear.gearId);
					if(ownGear == null) {
						ownGear = new OwnGear();
						ownGear.gearId = makeGear.gearId;
						ownGear.ownCount = 1;
						UserOwnGearList.Add(ownGear);
					} else {
						ownGear.ownCount += 1;
					}
					//serMakeGearList.Remove(makeGear);
					delMakeGearidList.Add(makeGear.gearId);
				}
			}

			foreach(short gearId in delMakeGearidList) {
				MakeGear delMakeGear = GetMakerGearByGearId(gearId);
				UserMakeGearList.Remove(delMakeGear);
			}


			LocalData.getInstence().UserOwnGearDataSave();
			LocalData.getInstence().UserMakeGearDataSave();
		
		} catch (System.Exception e) {
			if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("Error", "GetMakeGearList");
			Debug.LogError(e.Message);
		}

		return UserMakeGearList;
	}

	/** 대원의 장비를 초기화 함 */
	public void ResetMemberGears(short memberId) {
		Member member = GetMemberById(memberId);
		OwnGear owngear;
		if(member.SuitGearId != 1) {
			owngear = GetOwnGearByGearId(member.SuitGearId);
			owngear.ownCount += 1;
		}

		if(member.BodyGearId != 2) {
			owngear = GetOwnGearByGearId(member.BodyGearId);
			owngear.ownCount += 1;
		}

		if(member.EngineGearId != 3) {
			owngear = GetOwnGearByGearId(member.EngineGearId);
			owngear.ownCount += 1;
		}

		if(member.Weapon1GearId != 4) {
			owngear = GetOwnGearByGearId(member.Weapon1GearId);
			owngear.ownCount += 1;
		}

		if(member.Weapon2GearId > 0) {
			owngear = GetOwnGearByGearId(member.Weapon2GearId);
			owngear.ownCount += 1;
		}

		if(member.Weapon3GearId > 0) {
			owngear = GetOwnGearByGearId(member.Weapon3GearId);
			owngear.ownCount += 1;
		}

		member.SuitGearId = 1;
		member.BodyGearId = 2;
		member.EngineGearId = 3;
		member.Weapon1GearId = 4;
		member.Weapon2GearId = 0;
		member.Weapon3GearId = 0;

		LocalData.getInstence().UserOwnGearDataSave();
	}

	/** GearId에 맞는 현재 생산중인 장비를 반환함. */
	public MakeGear GetMakerGearByGearId(short gearId) {
		foreach(MakeGear makeGear in UserMakeGearList) {
			if(makeGear.gearId == gearId) return makeGear;
		}
		return null;
	}

	/** GearId에 해당하는 OwnGear를 반환합니다. */
	public OwnGear GetOwnGearByGearId(short gearId) {

		SetOwnGearCheck();

		OwnGear returnOwnGear = null;

		foreach(OwnGear ownGear in UserOwnGearList) {

			if(ownGear.gearId == gearId) returnOwnGear = ownGear;
		}

		if(gearId == 0 || gearId == 1 || gearId == 2 || gearId == 3 || gearId == 4) return null;

		if(returnOwnGear != null) return returnOwnGear;

		// 신규 기어 등록.
		Gear gear = GearData.getInstence().GetGearByID(gearId);
		if(gear != null) {
			OwnGear newOwnGear = new OwnGear();
			newOwnGear.gearId = gearId;
			UserOwnGearList.Add(newOwnGear);
			return newOwnGear;
		}

		return null;
	}

	// OwnGear 보정.
	private void SetOwnGearCheck() {

		ArrayList delOwnGearList = new ArrayList();

		UserOwnGearList.Sort(new OwnGearIdSort());
		short ownGearId = 0;
		foreach(OwnGear ownGear in UserOwnGearList) {
			if(ownGear.gearId == 0 || ownGear.gearId == 1 || ownGear.gearId == 2 || ownGear.gearId == 3 || ownGear.gearId == 4)
			{
				delOwnGearList.Add(ownGear);	// 기본 아이템 제거.
			}
			if(ownGear.gearId == ownGearId) delOwnGearList.Add(ownGear);	// 중복 아이템 제거.
			ownGearId = ownGear.gearId;
		}

		// 보정 로직.
		foreach(OwnGear delOwnGear in delOwnGearList) {
			UserOwnGearList.Remove(delOwnGear);
		}

	}

	/** GearType에 해당하는 OwnGearID 리스트를 반환합니다. */
	public ArrayList GetOwnGearByGearType(byte gearType) {

		SetOwnGearCheck();

		ArrayList typeList = new ArrayList();
		Gear gear;
		foreach(OwnGear ownGear in UserOwnGearList) {
			gear = GearData.getInstence().GetGearByID(ownGear.gearId);
			if(gear.gearType == gearType) {
				typeList.Add(gear.id);
			}
		}
		return typeList;
	}

	/** 모든 무기 장비중 OwnGearID 리스트를 반환 */
	public ArrayList GetOwnGearIsWeapon() {
		ArrayList typeList = new ArrayList();
		Gear gear;
		foreach(OwnGear ownGear in UserOwnGearList) {
			gear = GearData.getInstence().GetGearByID(ownGear.gearId);
			if(gear.gearType == GearType.Weapon_Gun || gear.gearType == GearType.Weapon_Missle || gear.gearType == GearType.Weapon_Rocket) {
				typeList.Add(gear.id);
			}
		}
		return typeList;
	}

	/** 개발 중인 Research가 존재하는지를 반환. */
	public byte GetIsResearching() {
		byte researchId = 0;
		Research research;
		foreach(UserResearch userResearch in UserResearchList) {
			research = GearData.getInstence().GetResearchByID(userResearch.id);
			long lastTime = (userResearch.startTime + (research.researchTime * _SystemData.millisecondNum)) - SystemData.GetInstance().getCurrentTime();
			if(lastTime <= 0) userResearch.isComplete = true;
			if(lastTime > 0) researchId = userResearch.id;
		}

		return researchId;
	}


// Getter ==========================================================================================================

	/** Category에 해당하는 기지 상태를 업그레이드함. */
	public BillModel UpgradeBase(byte categoryIndex) {
		BillModel returnBill = new BillModel();
		int usertownCount = WarzoneData.getInstence()._UserTownData.Count;
		if(categoryIndex == 0) {	// 부대 개발.
			if(BaseLevel <= _SystemData.BaseUpgradeMax) {
				if(BaseLevel <= usertownCount) {
					returnBill.money = _BaseLevelGuide[BaseLevel, 2];
				} else {
					SetAlert(_ScriptData.GetGameScript(150155).script, returnBill);
					return returnBill;
				}

			} else {
				SetAlert(ScriptData.getInstence().GetGameScript(150131).script, returnBill);
				return returnBill;
			}

		} else if (categoryIndex == 1) {	// 연구 개발.
			if(ResearchLevel <= _SystemData.ResearchUpgradeMax) {
				returnBill.money = _BaseLevelGuide[ResearchLevel, 2];
			} else {
				SetAlert(ScriptData.getInstence().GetGameScript(150131).script, returnBill);
				return returnBill;
			}

		} else {	// 공장 개발.
			if(FactoryLevel <= _SystemData.FactoryUpgradeMax) {
				if(BaseLevel <= usertownCount) {
					returnBill.money = _BaseLevelGuide[FactoryLevel, 2];
				} else {
					SetAlert(_ScriptData.GetGameScript(150155).script, returnBill);
					return returnBill;
				}

			} else {
				SetAlert(ScriptData.getInstence().GetGameScript(150131).script, returnBill);
				return returnBill;
			}

		}

		if(UserMoney < returnBill.money) {
			BillModel alertBill = new BillModel();
			SetAlert(ScriptData.getInstence().GetGameScript(150003).script, alertBill);
			returnBill.money = 0;
			return returnBill;
		}

		UserMoney -= returnBill.money;
		returnBill.moneyPlus = false;
		//UpdatePayData(pay, false);
		if(DarkSprite.getInstence().GameDataView != null) DarkSprite.getInstence().GameDataView.GetComponent<GameDataView>().UpdateUserData();

		if(categoryIndex == 0) {	// 부대 개발.
			BaseLevel += 1;
			
		} else if (categoryIndex == 1) {	// 연구 개발.
			ResearchLevel += 1;
			
		} else {	// 공장 개발.
			FactoryLevel += 1;
		}
		LocalData.getInstence().UserBaseLevelSave();
		return returnBill;
	}

	/** 마을의 세금을 징수함. */
	public BillModel GetTownTax(UserTown userTown) {
		BillModel returnBill = new BillModel();
		returnBill.money = userTown.resident * _SystemData.ResidentPerPay;
		long taxTime = userTown.lastTaxTime + (_SystemData.TownTaxCollectdelay * _SystemData.millisecondNum);
		if(returnBill.money > 0) {
			if(taxTime < _SystemData.getCurrentTime()) {
				userTown.lastTaxTime = _SystemData.getCurrentTime();
				UserData.getInstence().UserMoney += returnBill.money;

				if(DarkSprite.getInstence().GameDataView != null) DarkSprite.getInstence().GameDataView.GetComponent<GameDataView>().UpdateUserData();

				LocalData.getInstence().UserResourceSave();
				LocalData.getInstence().UserTownDataSave();

			} else {
				returnBill.money = 0;
				SetAlert(ScriptData.getInstence().GetGameScript(150104).script, new BillModel());
			}

		} else {
			returnBill.money = 0;
			SetAlert(ScriptData.getInstence().GetGameScript(150104).script, new BillModel());
		}
		return returnBill;
	}

	/** 새로운 대원을 추가함. */
	public short AddMember(byte memberId) {
		DefaultMember defaultMember = MemberData.getInstence().GetDefaultMemberByID(memberId);
		if(defaultMember == null) return 0;
		ClassModel classModel = MemberData.getInstence().GetClassModelByClassId(defaultMember.classN);
		ClassModel prevClassModel = MemberData.getInstence().GetClassModelByClassId((byte)(defaultMember.classN - 1));

		int memberCost = classModel.scoutCost;

		short memberid = 0;

		if(UserMoney >= memberCost) {
			Member member = new Member();
			member.id = GetMemberNewId();
			member.DefaultId = defaultMember.Id;
			member.ClassId = defaultMember.classN;
			member.MaxHP = classModel.HP;
			member.CurrentHP = member.MaxHP;
			member.MaxMP = classModel.MP;
			member.CurrentMP = member.MaxMP;
			member.BodyGearId = 2;
			member.EngineGearId = 3;
			member.SuitGearId = 1;
			member.Weapon1GearId = 4;
			member.Weapon2GearId = 0;
			member.Weapon3GearId = 0;
			member.Thumbnail = "Member" + defaultMember.thumbId;
			member.NameId = GetMemberNameCount(defaultMember.Id);
			member.state = MemberStateType.Ready;
			if(prevClassModel != null) {
				member.Exp = prevClassModel.exp + 1;
			} else {
				member.Exp = 0;
			}

			string googleStr = member.DefaultId + "-" + UserMemberList.Count;
			if(defaultMember.classN > 2) {
				if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("GetMember-Unique", googleStr);
			} else {
				if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("GetMember-Normal", googleStr);
			}

			UserMoney -= classModel.scoutCost;
			memberid = member.id;

			UserMemberList.Add(member);
			if(DarkSprite.getInstence().GameDataView != null) DarkSprite.getInstence().GameDataView.GetComponent<GameDataView>().UpdateUserData();

		} else {
			BillModel alertBill = new BillModel();
			SetAlert(ScriptData.getInstence().GetGameScript(150003).script, alertBill);
			memberCost = 0;
			return 0;
		}
		MissionData.getInstence().AddMissionGoal(MissionGoalType.Member_TotalCount, (byte)(UserMemberList.Count));
		LocalData.getInstence().UserMemberDataSave();
		return memberid;
	}

	/** 맴버 카운트 연산 */
	public byte GetMemberNameCount(short defaultMemberid) {
		byte memberCount = 0;
		byte sumMemberCount = 0;
		ArrayList sumMemberList = new ArrayList();
		foreach(Member member in UserMemberList) {
			if(member.DefaultId == defaultMemberid) {
				sumMemberCount ++;
				sumMemberList.Add(member);
			}
		}

		if(sumMemberCount == 0) return 0;

		for(byte i = 0; i<sumMemberCount; i++) {
			bool isSum = false;
			foreach(Member member in sumMemberList) {
				if(member.NameId == i) isSum = true;
			}
			memberCount = i;
			if(isSum == false) {
				return memberCount;
			}
		}

		memberCount ++;

		return memberCount;
	}


	/** 대원을 해고 함. */
	public void RemoveMember(short memberId) {

		Member delMember = GetMemberById(memberId);

		if(delMember.UnitId > 0) {
			Unit unit = GetUnitById(delMember.UnitId);
			int index = 0;
			foreach(short mId in unit.memberList) {
				if(mId == delMember.id) {
					unit.memberList[index] = 0;
					break;
				}
				index ++;
			}
			LocalData.getInstence().UserUnitDataSave();
		}
		UserMemberList.Remove(delMember);
		LocalData.getInstence().UserMemberDataSave();
	}

	private byte GetMemberNewId() {
		byte checkIndex = 1;
		if(UserMemberList.Count > 0) {
			while(GetMemberById(checkIndex) != null) {
				checkIndex ++;
				if(checkIndex > 150) {
					Debug.LogError("ID Index Overfull.");
					break;
				}
			}

		}

		return checkIndex;
	}

	/** 부대원을 회복 시킵니다. */
	public int HealMember(short memberId, byte memberStateType) {
		Member member = GetMemberById(memberId);
		ClassModel classModel = MemberData.getInstence().GetClassModelByClassId(member.ClassId);

		int pay = 0;
		int chipcount = 0;
		int corecount = 0;
		if(memberStateType == MemberStateType.Wound) {
			// 부상 회복.
			pay = classModel.healCost;
			chipcount = classModel.healChipCount;
		} else {
			// 구조
			pay = classModel.rescueCost;
			corecount = classModel.RescueCoreCount;
		}

		if(UserMoney >=  pay) {
			UserMoney -= pay;
		} else {
			SetAlert(ScriptData.getInstence().GetGameScript(150003).script, new BillModel());
			return 0;
		}

		if(UserChips >=  chipcount && UserCores >= corecount) {
			UserChips -= chipcount;
			UserCores -= corecount;
		} else {
			SetAlert(ScriptData.getInstence().GetGameScript(150109).script, new BillModel());
			return 0;
		}

		member.state = MemberStateType.Ready;
		member.CurrentHP = (int)(member.MaxHP / 5f);
		LocalData.getInstence().UserMemberDataSave();
		return pay;
	}

	/** 신규 편대를 생성 */
	public void AddUnit(short[] memberIdList) {
		Unit lastUnit = null;
		if(UserUnitList.Count > 0) lastUnit = UserUnitList[UserUnitList.Count - 1] as Unit;

		Unit unit = new Unit();

		if(lastUnit != null) {
			unit.id = (byte)(lastUnit.id + 1);
		} else {
			unit.id = 1;
		}
		unit.memberList = new short[5];
		unit.memberList[0] = memberIdList[0];
		unit.memberList[1] = memberIdList[1];
		unit.memberList[2] = memberIdList[2];
		unit.memberList[3] = memberIdList[3];
		unit.memberList[4] = memberIdList[4];
		unit.unitNameId = unit.id;

		UserUnitList.Add(unit);

		Member member;
		foreach(byte memberId in unit.memberList) {
			member = GetMemberById(memberId);
			if(member != null) member.UnitId = unit.id;
		}
		LocalData.getInstence().UserUnitDataSave();
		LocalData.getInstence().UserMemberDataSave();
	}

	/** 편대 제거 */
	public bool RemoveUnit(byte unitId) {
		bool isDel = false;
		if(unitId == 1) {
			SetAlert(_ScriptData.GetGameScript(150135).script, new BillModel());
		} else {
			if(UserUnitList.Count == unitId) {
				Unit delUnit = GetUnitById(unitId);
				foreach(short memberid in delUnit.memberList) {
					Member member = GetMemberById(memberid);
					if(member != null) {
						member.UnitId = 0;
					}
				}

				UserUnitList.Remove(delUnit);

				LocalData.getInstence().UserUnitDataSave();
				LocalData.getInstence().UserMemberDataSave();

				isDel = true;
			} else {
				SetAlert(_ScriptData.GetGameScript(150136).script, new BillModel());
			}

		}

		return isDel;
	}

	/** 개발을 시작함. */
	public BillModel SetResearch(Research research) {
		BillModel billModel = new BillModel();
		if(UserMoney < research.researchCost || UserCores < research.coreCost) {
			SetAlert(ScriptData.getInstence().GetGameScript(150003).script, new BillModel());
			return billModel;
		}
		UserMoney -= research.researchCost;
		billModel.money = research.researchCost;
		billModel.moneyPlus = false;
		UserCores -= research.coreCost;
		billModel.core = research.coreCost;
		billModel.corePlus = false;

		UserResearch userResearch = new UserResearch();
		userResearch.id = research.id;
		userResearch.startTime = _SystemData.getCurrentTime();
		UserResearchList.Add(userResearch);

		LocalData.getInstence().UserResearchDataSave();
		return billModel;
	}

	/** 장비를 생산함. */
	public BillModel SetMakeGear(Gear gear) {

		BillModel returnBill = new BillModel();

		if(FactoryLevel <= UserMakeGearList.Count) {
			SetAlert(ScriptData.getInstence().GetGameScript(150108).script, new BillModel());
			return returnBill;
		}
		if(UserMoney < gear.makeCost) {	// 자금이 부족.
			SetAlert(ScriptData.getInstence().GetGameScript(150003).script, new BillModel());
			return returnBill;
		}
		if(gear.makeResourceType == ResourceType.Core && UserCores < gear.makeResource) {	// 자원이 부족.
			SetAlert(ScriptData.getInstence().GetGameScript(150109).script, new BillModel());
			return returnBill;
		}

		UserMoney -= gear.makeCost;

		returnBill.money = gear.makeCost;
		returnBill.moneyPlus = false;

		if(gear.makeResourceType == ResourceType.Core) {
			UserCores -= gear.makeResource;
			returnBill.core = gear.makeResource;
			returnBill.corePlus = false;
		}


		MakeGear makeGear = new MakeGear();
		makeGear.gearId = gear.id;
		if(UserMakeGearList.Count > 0) {
			MakeGear prevMakeGear = UserMakeGearList[UserMakeGearList.Count - 1] as MakeGear;
			makeGear.endTime = prevMakeGear.endTime + (gear.makeTime * _SystemData.millisecondNum);
		} else {
			makeGear.endTime = _SystemData.getCurrentTime() + (gear.makeTime * _SystemData.millisecondNum);
		}
		UserMakeGearList.Add(makeGear);

		LocalData.getInstence().UserMakeGearDataSave();
		return returnBill;
	}

	/** 경험치 증가 및 승격 */
	public bool UpdateMemberExp(Member member, int plusExp) {
		bool isClassUp = false;
		// 진급 레벨 제한.
		ClassModel memberClass = MemberData.getInstence().GetClassModelByClassId(member.ClassId);
		ClassModel maxClass = MemberData.getInstence().GetClassModelByClassId(_SystemData.MemberMaxClass);
		if(member.ClassId >= _SystemData.MemberMaxClass) {
			member.Exp = maxClass.exp - 1;
			member.ClassId = maxClass.id;
			return isClassUp;
		}
		ClassModel nextClass = MemberData.getInstence().GetNextLClassModel(member.ClassId);
		if(nextClass == null) return isClassUp;
		member.Exp += plusExp;
		if(memberClass.exp <= member.Exp) {
			member.ClassId = nextClass.id;
			isClassUp = true;
		}

		LocalData.getInstence().UserMemberDataSave();
		return isClassUp;
	}

	/** 코어칩을 이용하여 코어를 제작함. */
	public bool AddCoreByCoreChip() {
		if(UserChips >= _SystemData.CoreMakeChipCount) {
			UserChips -= _SystemData.CoreMakeChipCount;
			UserCores += 1;
		} else {
			return false;
		}
		return true;
	}

	/** 자산을 업데이트함. */
	public void UpdatePayData(BillModel billModel, Vector2 vector) {
		GameObject payAni = Instantiate(Resources.Load<GameObject>("Common/PayAni")) as GameObject;
		payAni.transform.position = vector;
		payAni.GetComponent<PayAni>().AniStart(billModel);

		if(DarkSprite.getInstence().GameDataView != null) DarkSprite.getInstence().GameDataView.GetComponent<GameDataView>().UpdateUserData();

		LocalData.getInstence().UserResourceSave();
	}

	/** 경고 창을 노출함. */
	public void SetAlert(string str, BillModel billData) {
		GameObject alertPopup = Instantiate(Resources.Load<GameObject>("Common/AlertPopup")) as GameObject;
		alertPopup.GetComponent<AlertPopup>().init(str, ScriptData.getInstence().GetGameScript(160106).script, 250, billData);
		//_AlertMeg = str;
		//_AlertBill = billData;
	}

	/** 컨펌 팝업을 노출함 */
	public void SetConfirmPop(string str, ConfirmPopupCallback OnConfirmPopupCallback, BillModel billData) {
		_ConfirmPopupCallback = new ConfirmPopupCallback(OnConfirmPopupCallback);

		GameObject alertPopup = Instantiate(Resources.Load<GameObject>("Common/AlertPopup")) as GameObject;
		alertPopup.GetComponent<AlertPopup>().init(str, ScriptData.getInstence().GetGameScript(160106).script, 200, billData);
		alertPopup.GetComponent<AlertPopup>().SetConfirmData(ScriptData.getInstence().GetGameScript(160106).script, ScriptData.getInstence().GetGameScript(160107).script , OnConfirmPopupClick);

	}

	private void OnConfirmPopupClick(bool isConfirm) {
		if(_ConfirmPopupCallback != null) _ConfirmPopupCallback(isConfirm);

	}
		 
	void OnGUI() {

		//GUI.skin = AlertSkin;

		if(IsGUIShow && _SystemData.GameServiceType == ServiceType.ALPHA) {
			GUI.Label(new Rect(0, 0, 100, 30), "<color=#ff0000><size=18> FPS : " + _CurrentFPS + "</size></color>"); 
			GUI.Label(new Rect(0, 20, 120, 30), "<color=#ff0000><size=18> " + ((int)(_CurrentMemory / 1000) / 1000f) + "MByte</size></color>");
			GUI.Label(new Rect(0, 40, 100, 30), "<color=#ff0000><size=18> data : " + TestData + "</size></color>"); 
			//GUI.Label(new Rect(0, 40, 120, 30), "<color=#ff0000><size=18> " + UnityEngine.SystemInfo.systemMemorySize + "MByte</size></color>");
		}

	}

	public class OwnGearIdSort : IComparer
	{
		public int Compare(object x, object y)
		{
			// reverse the arguments
			short g1 = ((OwnGear)x).gearId;
			short g2 = ((OwnGear)y).gearId;
			
			if (g1 > g2)
				return -1;
			else
				return 0;
		}
		
	}
}
