using UnityEngine;
using System.Collections;
using System.Linq;

public class FightScene : MonoBehaviour {

	public bool IsTest;
	// 기본 객체 자원.
	public GameObject FightUnitField;
	public GameObject ProgressField;
	public GameObject WeaponListField;

	public GameObject FadeBlack;

	public AudioSource FightBGM;

	// 정보 객체 자원.
	private MemberData _MemberData;
	private GearData _GearData;
	private UserData _UserData;
	private WarzoneData _WarzoneData;

	// 주요 필드 객체 자원.
	private FightUnitField _FightUnitField;
	private BattleProgress _BattleProgress;

	// 주요 사용 정보 자원.
	private UnitDataBoxModel[] _ProgressData;
	private Unit _CurrentUnit;
	private ArrayList _EnemyList;

	// 주요 사용 변수들.
	private short _SelectGhostID;
	private bool _SelectIsEnemy;

	private short _TargetID;

	// 시스템 정보.
	private float _DefaultRightX;

	private GameObject _SkillBtn1;
	private GameObject _SkillBtn2;
	private GameObject _SkillBtn3;

	private GameObject _DelayObject;

	private short _RewardPayCount = 0;
	private byte _RewardCoreCount = 0;
	private byte _RewardChipCount = 0;

	private bool _IsSlotClick = false;

	private bool _AutoFight = false;
	private GameObject _AutoBtn;
	private short _AutoAmmoGearId = 0;

	private GameObject _LoadingMark;

	IEnumerator Start () {

		if(GoogleAnalytics.instance) GoogleAnalytics.instance.appVersion = SystemData.BuildVersion;

		if(SystemData.GetInstance().GameServiceType == ServiceType.ALPHA || SystemData.GetInstance().GameServiceType == ServiceType.BETA) {
			if(GoogleAnalytics.instance) GoogleAnalytics.instance.propertyID = SystemData.GetInstance().TestGoogleId;
		}

		yield return new WaitForEndOfFrame();

		_LoadingMark = SystemData.GetInstance().GetLoadingMark();

		NotificationManager.getInstence().CancelAllNoti();

		_DefaultRightX = SystemData.GetInstance().screenRightX - 1.5f;

		_WarzoneData = WarzoneData.getInstence();
		_UserData = UserData.getInstence();
		_MemberData = MemberData.getInstence();
		_GearData = GearData.getInstence();

		if(IsTest) {
			LocalData.getInstence().AllLoad();
			_WarzoneData.CurrentTownType = TownStateType.Airship;
			_WarzoneData.CurrentTownId = 2;
		}

		if(_UserData.Option_BGM) {
			FightBGM.Play();
			FightBGM.loop = true;
		}

		_CurrentUnit = _UserData.GetUnitById(_WarzoneData.CurrentFightUnitId);

		if(_WarzoneData.CurrentTownType == TownStateType.Town) {
			UserTown userTown = _WarzoneData.GetUserTownByID(_WarzoneData.CurrentTownId);

			_WarzoneData.CurrentFightType = FightType.DefenseFighting;	// 마을 방어 전투 모드로 변경.
			_EnemyList = _WarzoneData.GetGhostDataByGhostClose(userTown.invasionGhostClose);

		} else if (_WarzoneData.CurrentTownType == TownStateType.Airship) {
			_WarzoneData.CurrentFightType = FightType.AirshipDefense;	// 항공기 방어 전투 모드로 변경.
			AirshipDefense airshipDefense = _WarzoneData.GetAirshipDefenseById(_WarzoneData.CurrentTownId);
			_EnemyList = _WarzoneData.GetGhostDataByGhostClose(airshipDefense.ghostClose);
		} else {
			_WarzoneData.CurrentFightType = FightType.AttackFighting;	// 둥지 공격 전투 모드로 변경.
			GhostTown ghostTown = _WarzoneData.GetGhostTownByTownId(_WarzoneData.CurrentTownId);
			//ghostTown.ghostClose
			Town defaultTown = _WarzoneData.GetDefaultTownData(_WarzoneData.CurrentTownId);
			_EnemyList = _WarzoneData.GetGhostDataByGhostClose(defaultTown.maxClose);
		}

		_FightUnitField = FightUnitField.GetComponent<FightUnitField>();
		_BattleProgress = ProgressField.GetComponent<BattleProgress>();
		_BattleProgress.SetSlotClickCallback(OnEnemySelect);

		//ProgressField.transform.position = new Vector2(0f, -1.8f);

		int memberCount = 0;
		foreach (byte memberId in _CurrentUnit.memberList) {
			if(memberId > 0)  memberCount ++;
		}

		_ProgressData = new UnitDataBoxModel[memberCount + _EnemyList.Count];

		byte unitIndex = 0;
		Member member;
		DefaultMember defaultMember;
		Gear gear;
		foreach(byte memberId in _CurrentUnit.memberList) {
			if(memberId > 0) {
				member = _UserData.GetMemberById(memberId) as Member;
				defaultMember = _MemberData.GetDefaultMemberByID(member.DefaultId);
				_GearData.UpdateMemberGearSpec(member);
				_ProgressData[unitIndex] = new UnitDataBoxModel();
				_ProgressData[unitIndex].id = unitIndex;
				_ProgressData[unitIndex].modelId = member.id;
				_ProgressData[unitIndex].type = 0;
				_ProgressData[unitIndex].imgName = "MemberImg/Member" + defaultMember.thumbId;
				_ProgressData[unitIndex].maxHP = member.MaxHP;
				_ProgressData[unitIndex].maxMP = member.MaxMP;
				_ProgressData[unitIndex].currentHP = member.CurrentHP;
				_ProgressData[unitIndex].currentMP = member.CurrentMP;
				_ProgressData[unitIndex].ActNum = member.TotalIA;
				_ProgressData[unitIndex].MaxActNum = member.TotalIA;
				gear = _GearData.GetGearByID(member.Weapon1GearId);
				_ProgressData[unitIndex].Ammo1 = gear.ammo;
				gear = _GearData.GetGearByID(member.Weapon2GearId);
				if(gear != null) _ProgressData[unitIndex].Ammo2 = gear.ammo;
				gear = _GearData.GetGearByID(member.Weapon3GearId);
				if(gear != null) _ProgressData[unitIndex].Ammo3 = gear.ammo;

				unitIndex ++;
			}
		}

		Ghost ghost;
		DefaultGhost defaultGhost;
		foreach(byte ghostId in _EnemyList) {
			ghost = _WarzoneData.GetGhostByGhostId(ghostId);
			defaultGhost = _WarzoneData.GetDefaultGhostByGhostId(ghost.defaultId);
			_ProgressData[unitIndex] = new UnitDataBoxModel();
			_ProgressData[unitIndex].id = unitIndex;
			_ProgressData[unitIndex].modelId = ghost.id;
			_ProgressData[unitIndex].type = 1;
			_ProgressData[unitIndex].currentHP = ghost.currentHP;
			_ProgressData[unitIndex].maxHP = ghost.currentHP;
			_ProgressData[unitIndex].currentMP = 1;
			_ProgressData[unitIndex].maxMP = 1;
			_ProgressData[unitIndex].imgName = "GhostImg/Thumbnail/" + defaultGhost.resourceURI;
			_ProgressData[unitIndex].ActNum = defaultGhost.IA;
			_ProgressData[unitIndex].MaxActNum = defaultGhost.IA;

			unitIndex ++;
		}
		_ProgressData = _ProgressData.OrderBy(UnitDataBoxModel => UnitDataBoxModel.ActNum).ToArray();

		_BattleProgress.SetProgressSlot(_ProgressData);

		_FightUnitField.SetOpeningCallback(MyUnitFieldOpenCom);

		yield return StartCoroutine(_FightUnitField.init(_CurrentUnit.memberList));

		_FightUnitField.SetOpening(_CurrentUnit.memberList[0]);

		iTween.ColorTo(FadeBlack, iTween.Hash("a", 0, "time", 2f));

		Destroy(_LoadingMark);

		if(_UserData.UserBannerStandByCount <= 0) {
			SystemData.GetInstance().ShowBanner();
		} else {
			if(_UserData.UserBannerStandByCount < SystemData.GetInstance().UserMaxBannerCount) {
				_UserData.UserBannerStandByCount -= 1;
				LocalData.getInstence().UserResourceSave();
			}

		}

		GuideArrowManager.getInstence().ShowArrow(SceneType.FightScene);

	}

	/**
	 * 아군 진영의 등장 연출 완료.
	 * */
	private void MyUnitFieldOpenCom() {
		//iTween.MoveTo(ProgressField, iTween.Hash("y", 0, "speed", 2f, "oncomplete", "ShowField", "oncompletetarget", this.gameObject));
		_BattleProgress.SetSlotOpeningFunc(ShowField);

		if(_UserData.StoryStepId < 10) {
			//GameObject firstGuide = Instantiate(Resources.Load<GameObject>("BattleScene/FirstFightGuide")) as GameObject;
			//GameObject storyViewWindow = Instantiate(Resources.Load<GameObject>("Common/StoryView")) as GameObject;
			//StoryView storyView = _StoryViewWindow.GetComponent<StoryView>();
			//storyView.ShowStoryWindow(5, 1, "", "", OnFirstGuideClick, 2);
			StoryData.getInstence().UpdateStoryStep(60001);
		}
	}

	/** 최초 가이드 화면. */
	private void OnFirstGuideClick() {

	}


	/**
	 * 적군 진영의 적 선택 이벤트.
	 * */
	private void OnEnemySelect(byte modelId) {
		if(_AutoFight == true) return;
		if(_IsSlotClick == false) return;
		foreach(UnitDataBoxModel boxModel in _ProgressData) {
			if(boxModel.id == modelId) {
				_TargetID = boxModel.modelId;
			}
			boxModel.isTarget = false;
		}
		GameObject progressSlot = _BattleProgress.GetSlotObjByID(modelId);
		progressSlot.GetComponent<UnitDataBox>().Model.isTarget = true;

		UpdateProgressField(false);

	}

// 메인 프로세스. //=================================================================================================

	private void ShowField() {
		SelectUnit();
		ShowAutoFightBtn();
	}

	private IEnumerator GameEndCheck() {
		yield return new WaitForSeconds(0.5f);
		byte enemyCount = 0;
		byte unitCount = 0;

		foreach(UnitDataBoxModel boxModel in _ProgressData) {
			if(!boxModel.isOut) boxModel.ActNum -= SystemData.GetInstance().TurnActPoint;
			if(boxModel.ActNum <= 0) {
				Member member = _UserData.GetMemberById(boxModel.modelId);
				if(boxModel.type == 0) {	// 대원인 경우.
					boxModel.ActNum = member.TotalIA;
				} else {	// 고스트 인 경우.
					Ghost ghost = _WarzoneData.GetGhostByGhostId(boxModel.modelId);
					DefaultGhost defaultGhost = _WarzoneData.GetDefaultGhostByGhostId(ghost.defaultId);
					boxModel.ActNum = defaultGhost.IA;
					if(boxModel.ActNum == 0) {
						Debug.LogError("Act가 초기화 되지 않음.");
					}
				}

			}
			if(boxModel.isOut == false && boxModel.type == 0) unitCount ++;
			if(boxModel.isOut == false && boxModel.type == 1) enemyCount ++;
		}

		UpdateProgressField(true);

		if(enemyCount > 0 && unitCount > 0) {
			ShowField();
		} else {
			FightGameEnd();
		}

	}
	
// 메인 프로세스. //=================================================================================================

// 추가 프로세스. //=================================================================================================
	// 사용자에게 사용가능한 무장을 보여줍니다.
	private void ShowMyUnitWeapon() {
		ShowWeaponBtn(true);
		_IsSlotClick = true;
	}

	// 무기를 발사합니다.
	private void WeaponSlot0fire(int weaponId) {

		UnitDataBoxModel selectModel = GetUnitUnitDataBoxModel();
		if(selectModel != null && selectModel.Ammo1 != 0) {
			selectModel.Ammo1 -= 1;
		} else {
			return;
		}

		ShowWeaponBtn(false);
		if(_DelayObject == null) _DelayObject = new GameObject();

		_FightUnitField.UnitWeaponFire(0);
		StartCoroutine(MyUnitFireCom(weaponId));
	}

	// 무기를 발사합니다.
	private void WeaponSlot1fire(int weaponId) {

		UnitDataBoxModel selectModel = GetUnitUnitDataBoxModel();
		if(selectModel != null && selectModel.Ammo3 != 0) {
			selectModel.Ammo3 -= 1;
		} else {
			return;
		}

		ShowWeaponBtn(false);
		if(_DelayObject == null) _DelayObject = new GameObject();
		
		_FightUnitField.UnitWeaponFire(2);
		StartCoroutine(MyUnitFireCom(weaponId));
	}

	// 무기를 발사합니다.
	private void WeaponSlot2fire(int weaponId) {

		UnitDataBoxModel selectModel = GetUnitUnitDataBoxModel();
		if(selectModel != null && selectModel.Ammo2 != 0) {
			selectModel.Ammo2 -= 1;
		} else {
			return;
		}

		ShowWeaponBtn(false);
		if(_DelayObject == null) _DelayObject = new GameObject();
		
		_FightUnitField.UnitWeaponFire(1);
		StartCoroutine(MyUnitFireCom(weaponId));
	}

	private IEnumerator MyUnitFireCom(int weaponId) {
		_IsSlotClick = false;
		yield return new WaitForSeconds(0.5f);
		_FightUnitField.SetUnitOut(false);
		StartCoroutine(UnitHit((byte)(weaponId), true, _TargetID));
	}

	private void EnemyShow(int weaponId) {
		_FightUnitField.SetSelectEnemy(true, _SelectGhostID, 0, 0);
		StartCoroutine(SetEnemyFire(weaponId));
	}

	private IEnumerator SetEnemyFire(int weaponId) {
		yield return new WaitForSeconds(1f);
		_FightUnitField.UnitWeaponFire(0);
		if(_DelayObject == null) _DelayObject = new GameObject();
		StartCoroutine(ShowEnemyFire(weaponId));

	}

	private IEnumerator ShowEnemyFire(int weaponId) {
		yield return new WaitForSeconds(0.3f);
		_FightUnitField.SetUnitOut(false);
		StartCoroutine(UnitHit((byte)(weaponId), false, _TargetID));

	}


// 추가 프로세스. //=================================================================================================


// 주요 단위 프로세스. //--------------------------------------------------------------------------------------------
	/** 현재 진행중인 객체의 UnitDataBoxModel 정보를 반환함. */
	private UnitDataBoxModel GetUnitUnitDataBoxModel() {
		UnitDataBoxModel selectBoxModel = null;
		
		foreach(UnitDataBoxModel boxModel in _ProgressData) {
			if(boxModel.modelId == _SelectGhostID && boxModel.type == 0) {
				selectBoxModel = boxModel;
			}
			
		}
		return selectBoxModel;
	}

	private void ShowAutoFightBtn() {
		if(_AutoBtn == null) {
			_AutoBtn = Instantiate(Resources.Load<GameObject>("Common/FightSkillBtn")) as GameObject;
			_AutoBtn.GetComponent<CommonBtn>().Init(0, ScriptData.getInstence().GetGameScript(160163).script, 10, Color.white);
			_AutoBtn.GetComponent<CommonBtn>().SetSortLayer("UI");
			_AutoBtn.GetComponent<CommonBtn>().SetClick(OnAutoFightBtn);
			_AutoBtn.GetComponent<CommonBtn>().SetTxtSize(12);
			_AutoBtn.transform.position = new Vector2(SystemData.GetInstance().screenLeftX + 1.8f, -4.50f);
			_AutoBtn.transform.localScale = new Vector2(1.5f, 1.5f);
			_AutoBtn.transform.parent = this.gameObject.transform;

			iTween.MoveFrom(_AutoBtn, iTween.Hash("y", -6.15f, "speed", 8f));

		}
	}

	private void OnAutoFightBtn(int id) {
		if(_AutoFight == true) {
			_AutoFight = false;
		} else {
			_AutoFight = true;
			if(_IsSlotClick == true) {
				WeaponSlot0fire(_AutoAmmoGearId);
			}
		}
		_AutoBtn.GetComponent<CommonBtn>().SetBtnSelect(_AutoFight);

	}

	/** 
	 * 유닛 선택 처리
	 * */
	private void SelectUnit() {

		foreach(UnitDataBoxModel boxModel in _ProgressData) {
			boxModel.isSelect = false;
			boxModel.isTarget = false;
		}

		_SelectGhostID = _ProgressData[0].modelId;
		_SelectIsEnemy = (_ProgressData[0].type == 1);
		_ProgressData[0].isSelect = true;
		_ProgressData[0].ActNum = 0;

		if(_SelectIsEnemy) {	// 적인 경우.

			SelectRandomTarget(false);

			Ghost ghost = _WarzoneData.GetGhostByGhostId(_SelectGhostID);
			DefaultGhost defaultGhost = _WarzoneData.GetDefaultGhostByGhostId(ghost.defaultId);

			//SetEnemyFire(defaultGhost.WeaponId);
			EnemyShow(defaultGhost.WeaponId);

		} else {	// 아군인 경우.
			_FightUnitField.SetSelectUnit(true, _SelectGhostID, 0);
			SelectRandomTarget(true);
			ShowMyUnitWeapon();
		}

		UpdateProgressField(false);
	}

	/**
	 * 사용자 무장 선택 버튼의 노출 유무 
	 * @state : 노출 유무.
	 * */
	private void ShowWeaponBtn(bool state) {
		if(state) {
			Member member = _UserData.GetMemberById(_SelectGhostID);
			
			if(_SkillBtn1 != null) Destroy(_SkillBtn1);
			if(_SkillBtn2 != null) Destroy(_SkillBtn2);
			if(_SkillBtn3 != null) Destroy(_SkillBtn3);

			WeaponListField.transform.position = new Vector2(0, -6.15f);
			
			Gear wearponGear;
			string weaponName;
			UnitDataBoxModel selectBoxModel = null;

			foreach(UnitDataBoxModel boxModel in _ProgressData) {
				if(boxModel.modelId == _SelectGhostID && boxModel.type == 0) {
					selectBoxModel = boxModel;
				}

			}

			_AutoAmmoGearId = member.Weapon1GearId;

			string ammoCount = "";
			if(member.Weapon1GearId > 0) {
				wearponGear = _GearData.GetWeaponGearByID(member.Weapon1GearId);
				_SkillBtn1 = Instantiate(Resources.Load<GameObject>("Common/FightSkillBtn")) as GameObject;
				_SkillBtn1.transform.position = new Vector2(_DefaultRightX, -5.89f);
				_SkillBtn1.transform.localScale = new Vector2(1.5f, 1.5f);
				_SkillBtn1.transform.parent = WeaponListField.transform;
				if(selectBoxModel.Ammo1 >= 0 && selectBoxModel != null) ammoCount = " X" + selectBoxModel.Ammo1;
				weaponName = ScriptData.getInstence().GetGameScript(wearponGear.scriptId).script + ammoCount;
				_SkillBtn1.GetComponent<CommonBtn>().Init(member.Weapon1GearId, weaponName, -1, Color.white);
				if(selectBoxModel.Ammo1 != 0 && _AutoFight == false) _SkillBtn1.GetComponent<CommonBtn>().SetClick(WeaponSlot0fire);
				_SkillBtn1.GetComponent<CommonBtn>().SetTxtSize(12);
			}
			ammoCount = "";
			if(member.Weapon2GearId > 0) {
				wearponGear = _GearData.GetWeaponGearByID(member.Weapon2GearId);
				_SkillBtn2 = Instantiate(Resources.Load<GameObject>("Common/FightSkillBtn")) as GameObject;
				_SkillBtn2.transform.position = new Vector2(_DefaultRightX - 3.5f, -5.89f);
				_SkillBtn2.transform.localScale = new Vector2(1.5f, 1.5f);
				_SkillBtn2.transform.parent = WeaponListField.transform;
				if(selectBoxModel.Ammo2 >= 0 && selectBoxModel != null) ammoCount = " X" + selectBoxModel.Ammo2;
				weaponName = ScriptData.getInstence().GetGameScript(wearponGear.scriptId).script + ammoCount;
				_SkillBtn2.GetComponent<CommonBtn>().Init(member.Weapon2GearId, weaponName, -1, Color.white);
				if(selectBoxModel.Ammo2 != 0 && _AutoFight == false) _SkillBtn2.GetComponent<CommonBtn>().SetClick(WeaponSlot2fire);
				_SkillBtn2.GetComponent<CommonBtn>().SetTxtSize(12);
			}
			ammoCount = "";
			if(member.Weapon3GearId > 0) {
				wearponGear = _GearData.GetWeaponGearByID(member.Weapon3GearId);
				_SkillBtn3 = Instantiate(Resources.Load<GameObject>("Common/FightSkillBtn")) as GameObject;
				_SkillBtn3.transform.position = new Vector2(_DefaultRightX - 7f, -5.89f);
				_SkillBtn3.transform.localScale = new Vector2(1.5f, 1.5f);
				_SkillBtn3.transform.parent = WeaponListField.transform;
				if(selectBoxModel.Ammo3 >= 0 && selectBoxModel != null) ammoCount = " X" + selectBoxModel.Ammo3;
				weaponName = ScriptData.getInstence().GetGameScript(wearponGear.scriptId).script + ammoCount;
				_SkillBtn3.GetComponent<CommonBtn>().Init(member.Weapon3GearId, weaponName, -1, Color.white);
				if(selectBoxModel.Ammo3 != 0 && _AutoFight == false) _SkillBtn3.GetComponent<CommonBtn>().SetClick(WeaponSlot1fire);
				_SkillBtn3.GetComponent<CommonBtn>().SetTxtSize(12);
			}

			iTween.MoveTo(WeaponListField, iTween.Hash("y", -4.75f, "speed", 8f, "oncomplete", "OpenBtnsEnd", "oncompletetarget", this.gameObject));
		} else {
			iTween.MoveTo(WeaponListField, iTween.Hash("y", -7.05f, "speed", 8f, "oncomplete", "OpenBtnsEnd", "oncompletetarget", this.gameObject));
		}
	}

	private void OpenBtnsEnd() {
		if(_AutoFight == true && _IsSlotClick == true) {
			StartCoroutine(AutoAmmoFire());
		}
	}

	private IEnumerator AutoAmmoFire() {
		yield return new WaitForSeconds(0.5f);
		WeaponSlot0fire(_AutoAmmoGearId);
		yield return 0;
	}

	/**
	 * 공격 대상을 선택함.
	 * */
	private void SelectRandomTarget(bool isEnemy) {
		byte enemyType = 0;
		ArrayList targetIdList = new ArrayList();
		ArrayList targetModelList = new ArrayList();
		if(isEnemy) enemyType = 1;
		foreach(UnitDataBoxModel boxModel in _ProgressData) {
			if(boxModel.type == enemyType && !boxModel.isOut) {
				//_TargetID = boxModel.modelId;
				targetIdList.Add(boxModel.modelId);
				targetModelList.Add(boxModel.id);
				//break;
			}
		}
		int randomIndex = UnityEngine.Random.Range(0, targetIdList.Count);
		_TargetID = (short)(targetIdList[randomIndex]);

		GameObject progressSlot = _BattleProgress.GetSlotObjByID((byte)(targetModelList[randomIndex]));
		progressSlot.GetComponent<UnitDataBox>().Model.isTarget = true;
		//progressSlot.GetComponent<UnitDataBox>().TargetSelect();

	}

	/**
	 * 하단 프로그래스 바의 갱신.
	 * */
	private void UpdateProgressField(bool isSort) {
		if(isSort) _ProgressData = _ProgressData.OrderBy(UnitDataBoxModel => UnitDataBoxModel.ActNum).ToArray();
		_BattleProgress.SetProgressSlot(_ProgressData);

	}

	/**
	 * 대상을 공격함.
	 * */
	private IEnumerator UnitHit(byte weaponId, bool isEnemy, short targetModelId) {

		yield return new WaitForSeconds(0.5f);

		Gear weaponGear = _GearData.GetWeaponGearByID((byte)(weaponId));
		short damage = (short)(UnityEngine.Random.Range(weaponGear.minAP, weaponGear.maxAP));
		short damageIA = (short)(UnityEngine.Random.Range(weaponGear.minIAD, weaponGear.maxIAD));

		byte progressIndex = GetModelIdByprogressIndex(isEnemy, targetModelId);
		UnitDataBoxModel boxModel = _ProgressData[progressIndex];
		boxModel.currentHP -= damage;

		if(boxModel.currentHP <= 0) {
			boxModel.currentHP = 0;
			boxModel.isOut = true;
			boxModel.ActNum = 10000;
			//UpdateProgressField(true);
			if(isEnemy) {
				GetReward(boxModel.modelId);
			}
		} else {
			//UpdateProgressField(false);
		}

		// 행동력 공격 
		boxModel.ActNum += damageIA;

		if(boxModel.type == 0) {
			Member member = _UserData.GetMemberById(boxModel.modelId);
			member.CurrentHP = boxModel.currentHP;
			member.lastHPUpdateTime = SystemData.GetInstance().getCurrentTime() + (SystemData.GetInstance().MemberHpPlusDelay * SystemData.GetInstance().millisecondNum);
		} else {
			Ghost ghost = _WarzoneData.GetGhostByGhostId(boxModel.modelId);
			ghost.currentHP = (short)(boxModel.currentHP);
		}

		// 공격 대상 연출 시작.
		if(isEnemy) {
			//_FightUnitField.SetSelectEnemy(false, boxModel.modelId, damage, weaponGear.gearType);
		} else {
			//_FightUnitField.SetSelectUnit(false, boxModel.modelId, damage);
		}
		_BattleProgress.SetSlotHitAni(boxModel.id, damage, weaponGear.gearType, damageIA);

		if(boxModel.currentHP <= 0) {
			StartCoroutine(ShowHitAniEnd(true));
		} else {
			StartCoroutine(ShowHitAniEnd(false));
		}

	}

	private IEnumerator ShowHitAniEnd(bool IsDestroy) {
		yield return new WaitForSeconds(0.4f);
		//_FightUnitField.SetUnitOut(IsDestroy);
		UpdateProgressField(false);
		StartCoroutine(GameEndCheck());
	}


// 주요 단위 프로세스. //--------------------------------------------------------------------------------------------

	private void GetReward(short ghostId) {

		// 한 전투에서 최대 획득 리워드 수 제한.
		if(SystemData.GetInstance().FightUnitRwardCount <= (_RewardChipCount + _RewardCoreCount)) {
			return;
		}

		Ghost ghost = _WarzoneData.GetGhostByGhostId(ghostId);
		DefaultGhost defaultGhost = _WarzoneData.GetDefaultGhostByGhostId(ghost.defaultId);
		int random = UnityEngine.Random.Range(0, 1000);
		if(random < defaultGhost.coreDrop) {	// 코어 획득 유무.
		//if(true) {	// 코어 획득 유무.
			_RewardCoreCount += 1;
			ShowReward(1);
		} else {
			random = UnityEngine.Random.Range(0, 1000);
			if(random < defaultGhost.chipDrop) {	// 코어 파편 획득 유무.
				_RewardChipCount += 1;
				ShowReward(0);
			}
		}

		_RewardPayCount = 0;
	}

	/** 리워드 획득시 노출을 시켜줍니다. */
	private void ShowReward(byte type) {
		GameObject CoreSymbol = new GameObject();
		CoreSymbol.AddComponent<SpriteRenderer>();
		float posX = SystemData.GetInstance().screenRightX - ((_RewardCoreCount + _RewardChipCount) * 0.8f);
		CoreSymbol.transform.position = new Vector2(posX, 4.46f);
		SpriteRenderer renderer = CoreSymbol.GetComponent<SpriteRenderer>();
		//CoreSymbol.layer = LayerMask.NameToLayer("Enemy");
		renderer.sortingOrder = 100;
		if(type == 1) {	// 코어.
			renderer.sprite = Resources.Load<Sprite>("Common/CoreSymbol");
		} else {		// 파편.
			renderer.sprite = Resources.Load<Sprite>("Common/CoreChipSymbol");
		}

		iTween.MoveFrom(CoreSymbol, iTween.Hash("x", 0f,"y", 0f, "time", 0.6f, "easetype", iTween.EaseType.easeInBack));
		iTween.ColorFrom(CoreSymbol, iTween.Hash("a", 0, "time", 0.6f));
		iTween.ScaleFrom(CoreSymbol, iTween.Hash("scale", new Vector3(0, 0, 0)));
	}

	private byte GetModelIdByprogressIndex(bool isEnemy, short modelId) {

		byte isEnemyType = 0;
		if(isEnemy) isEnemyType = 1;

		byte index = 0;
		foreach(UnitDataBoxModel boxModel in _ProgressData) {
			if(boxModel.type == isEnemyType && boxModel.modelId == modelId) {
				break;
			}
			index ++;
		}
		return index;
	}

	/**
	 * 현재 프로세스의 첫번째 유닛이 적인지 아닌지 반환.
	 * */
	private bool IsEnemy() {
		return true;
	}

	private void FightGameEnd() {
		Unit unit = _UserData.GetUnitById(_WarzoneData.CurrentFightUnitId);
		bool isWin = false;
		foreach(UnitDataBoxModel boxModel in _ProgressData) {
			if(boxModel.type == 0) {
				if(boxModel.currentHP > 0) {
					isWin = true;
					break;
				}
			} 
		}

		if(isWin) {	// 전투 승리시.
			if(_WarzoneData.CurrentFightType == FightType.DefenseFighting) {
				// 마을 방어 시.
				UserTown userTown = _WarzoneData.GetUserTownByID(_WarzoneData.CurrentTownId);
				if(userTown != null) {
					string prevGhostClose = userTown.invasionGhostClose.ToString();
					userTown.isInvasion = false;
					userTown.invasionGhostTownId = 0;
					userTown.invasionGhostClose = 0;
					long nextTime = (SystemData.GetInstance().GhostAttachDelay * SystemData.GetInstance().millisecondNum) + SystemData.GetInstance().getCurrentTime();
					userTown.lastInvasionEndTime = nextTime;

					LocalData.getInstence().UserTownDataSave();

					MissionData.getInstence().AddMissionGoal(MissionGoalType.Town_Defense_Com, 1);

					if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("FightEnd-TownDefense", userTown.id + "-" + prevGhostClose);

				} else {
					Debug.LogError("마을 정보가 없습니다. townId : " + _WarzoneData.CurrentTownId);
				}

			} else if (_WarzoneData.CurrentFightType == FightType.AirshipDefense) {
				// 항공기 방어.
				AirshipDefense airshipDefense = _WarzoneData.GetAirshipDefenseById(_WarzoneData.CurrentTownId);

				_RewardPayCount = airshipDefense.rewardCost;

				MissionData.getInstence().AddMissionGoal(MissionGoalType.Aircraft_Defense_Complete, 1);

				if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("FightEnd-AircraftDefense", airshipDefense.id.ToString());

			} else {
				// 둥지 공격 시.
				GhostTown ghostTown = _WarzoneData.GetGhostTownByTownId(_WarzoneData.CurrentTownId);
				// 공격력 연산
				short ghostTownDamage = 0;
				Member thisMember;
				foreach(short memberid in unit.memberList) {
					if(memberid > 0) {
						thisMember = _UserData.GetMemberById(memberid);
						ghostTownDamage += (short)(thisMember.ClassId * SystemData.GetInstance().TownAttackPoint);
					}
				}

				ghostTown.ghostClose -= ghostTownDamage;
				long nextPlusTime = (SystemData.GetInstance().GhostTownNextTime * SystemData.GetInstance().millisecondNum) + SystemData.GetInstance().getCurrentTime();
				ghostTown.lastClosePlusTime = nextPlusTime;
				if(ghostTown.ghostClose <= 0) {

					_WarzoneData.AddUserTown(ghostTown.id);
					_WarzoneData.RemoveGhostTown(ghostTown.id);

					MissionData.getInstence().AddMissionGoal(MissionGoalType.GhostTown_Unlock, 1);
					MissionData.getInstence().AddMissionGoal(MissionGoalType.Target_GhostTown_Unlock, ghostTown.id);

					if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("FightEnd-GhostTownClear", ghostTown.id.ToString());
				}

				LocalData.getInstence().UserTownDataSave();
				LocalData.getInstence().GhostTownDataSave();

				if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("FightEnd-GhostAttack", ghostTown.id + "-" + ghostTown.ghostClose);
			}
		}

		// 타운 정보 갱신.
		if(_WarzoneData.CurrentFightType == FightType.AirshipDefense || _WarzoneData.CurrentFightType == FightType.AttackFighting) {
			if(_UserData.StoryStepId > 22) _UserData.SetFightEndUpdateTown();
		}

		if(UserData.getInstence().StoryStepId == 26 && _RewardCoreCount == 0) {
			_RewardCoreCount += 1;
			print("튜토리얼용 코어 강제 지급.");
		}
		if(SystemData.GetInstance().IsGetFightCore == true) _RewardCoreCount += 5;
		if(_RewardCoreCount > 0) MissionData.getInstence().AddMissionGoal(MissionGoalType.Get_Core, _RewardCoreCount);

		_ProgressData.OrderBy(UnitDataBoxModel => UnitDataBoxModel.modelId).ToArray();

		ShowGameEndPopup();
		MissionData.getInstence().AddMissionGoal(MissionGoalType.Fight_Complete, 1);



		if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("GetFightReward", _RewardPayCount+ "-" + _RewardCoreCount + "-" + _RewardChipCount);
	}

	private void ShowGameEndPopup() {

		// 대원들 HP/MP를 확인하여 노티 출력.
		int limmp = 0;
		int limhp = 0;
		int leftMp = 0;
		int leftHp = 0;
		foreach(Member member in _UserData.UserMemberList) {
			leftMp = member.MaxMP - member.CurrentMP;
			leftHp = member.MaxHP - member.CurrentHP;
			if(leftHp > leftMp) {
				if(limhp < leftHp) limhp = leftHp;
			} else {
				if(limmp < leftMp) limmp = leftMp;
			}
		}

		if(limhp > 0 || limmp > 0) {
			int hpTime = (int)(limhp / SystemData.GetInstance().MemberHpPlusCount);
			int mpTime = (int)(limmp / SystemData.GetInstance().MemberMpPlusCount);
			hpTime =  hpTime * SystemData.GetInstance().MemberHpPlusDelay;
			mpTime =  mpTime * SystemData.GetInstance().MemberMpPlusDelay;

			// 노티 실행.
			if(hpTime > mpTime) {
				hpTime += 60;
				NotificationManager.getInstence().SetMemberCareNotification(hpTime);
			} else {
				mpTime += 60;
				NotificationManager.getInstence().SetMemberCareNotification(mpTime);
			}
		}

		_WarzoneData.SetTaxNotification();	// 마을 세금 노티 연산.

		NotificationManager.getInstence().AddNotification(ScriptData.getInstence().GetGameScript(220100).script, 21600);
		NotificationManager.getInstence().AddNotification(ScriptData.getInstence().GetGameScript(220100).script, 86400);

		GameObject endPopup = Instantiate(Resources.Load<GameObject>("BattleScene/EndPopup")) as GameObject;

		endPopup.GetComponent<EndPopup>().init(_ProgressData, _RewardPayCount, _RewardCoreCount, _RewardChipCount);
		iTween.ScaleFrom(endPopup, iTween.Hash("scale", new Vector3(2f, 2f, 2f), "delay", 0.2f, "time", 0.5f, "easetype", iTween.EaseType.easeOutCirc));
		iTween.ColorFrom(endPopup, iTween.Hash("a", 0, "delay", 0.2f, "time", 0.5f));

		if(ProgressField != null) Destroy(ProgressField);
	}

	void OnGUI() {
		/*
		if(GUI.Button(new Rect(10, 10, 100, 30), "GetIndex")) {
			byte testIndex = 2;
			//GetModelIdByprogressIndex((_ProgressData[testIndex].type == 1), _ProgressData[testIndex].modelId);
			Application.LoadLevel("DarkSpriteMain");
		}
		*/
	}


}
