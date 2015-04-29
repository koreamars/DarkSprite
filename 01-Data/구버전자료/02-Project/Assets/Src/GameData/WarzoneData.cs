using UnityEngine;
using System.Xml;
using System.Collections;

/*
 * 게임내의 마을 및 전투 지역 정보를 갑니다.
 * 마을 , 둥지, 항공기 보호 정보.
 * */

public class WarzoneData : MonoBehaviour {

	public byte CurrentFightUnitId;	// 이번에 전투에 참가할 아군 Unit ID;
	public byte CurrentTownId;		// 이번에 전투할 Town ID;
	public byte CurrentFightType;			// 전투 형태를 정의.
	public byte CurrentTownType;		// 전투하는 지역의 형태 마을 혹은 둥지.

	private Town[] _DefaultTownData;
	public ArrayList _UserTownData;
	public ArrayList _GhostTownData;
	private DefaultGhost[] _DefaultGhostData;
	private ArrayList _GhostData;
	public AirshipDefense[] _AirshipdefenseData;

	private static WarzoneData _instence;

	private SystemData _SystemData;


	public static WarzoneData getInstence()	{
		if(_instence == null) 
		{
			//_instence = new DataController();
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<WarzoneData>();
			DontDestroyOnLoad(_instence);
		}
		
		return _instence;
	}

	void Awake() {

		_SystemData = SystemData.GetInstance();

		CurrentFightUnitId = 1;
		CurrentTownType = TownStateType.Town;
		CurrentFightType = FightType.Idle;

		// 마을 정보 초기화.
		TextAsset textAsset = (TextAsset)Resources.Load("XMLData/TownData",typeof(TextAsset));
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(textAsset.text);

		_DefaultTownData = new Town[xmlDoc["townData"].ChildNodes.Count];
		_UserTownData = new ArrayList();
		_GhostTownData = new ArrayList();
		_GhostData = new ArrayList();

		int xmlIndex = 0;
		foreach(XmlElement xmlElement in xmlDoc["townData"]) {
			Town town = new Town();
			town.id = System.Convert.ToByte(xmlElement["id"].InnerText);
			town.soldierCount = System.Convert.ToByte(xmlElement["soldierCount"].InnerText);
			town.position = new Vector2(System.Convert.ToSingle(xmlElement["positionX"].InnerText), System.Convert.ToSingle(xmlElement["positionY"].InnerText));
			town.townNameId = System.Convert.ToInt32(xmlElement["nameScriptId"].InnerText);
			town.isView = xmlElement["defaultView"].InnerText == "1";
			town.state = System.Convert.ToByte(xmlElement["defaultState"].InnerText);
			town.resident = System.Convert.ToInt16(xmlElement["resident"].InnerText);
			town.ghostClose = System.Convert.ToInt16(xmlElement["ghostClose"].InnerText);
			town.maxClose = System.Convert.ToInt16(xmlElement["maxClose"].InnerText);
			town.root = System.Convert.ToByte(xmlElement["root"].InnerText);
			_DefaultTownData[xmlIndex] = town;

			xmlIndex ++;
		}

		// 기본 고스트 정보.
		TextAsset ghostTextAsset = (TextAsset)Resources.Load("XMLData/GhostData",typeof(TextAsset));
		XmlDocument ghostXmlDoc = new XmlDocument();
		ghostXmlDoc.LoadXml(ghostTextAsset.text);
		
		_DefaultGhostData = new DefaultGhost[ghostXmlDoc["GhostData"].ChildNodes.Count];

		xmlIndex = 0;
		foreach(XmlElement xmlElement in ghostXmlDoc["GhostData"]) {

			DefaultGhost defaultGhost = new DefaultGhost();
			defaultGhost.id = System.Convert.ToByte(xmlElement["id"].InnerText);
			defaultGhost.HP = System.Convert.ToInt16(xmlElement["HP"].InnerText);
			defaultGhost.IA = System.Convert.ToInt16(xmlElement["IA"].InnerText);
			defaultGhost.WeaponId = System.Convert.ToByte(xmlElement["weaponId"].InnerText);
			defaultGhost.coreDrop = System.Convert.ToInt16(xmlElement["coreDrop"].InnerText);
			defaultGhost.chipDrop = System.Convert.ToInt16(xmlElement["chipDrop"].InnerText);
			defaultGhost.resourceURI = xmlElement["resourceURI"].InnerText.ToString();
			defaultGhost.closeValue = System.Convert.ToInt16(xmlElement["closeValue"].InnerText);

			_DefaultGhostData[xmlIndex] = defaultGhost;

			xmlIndex ++;
		}

		// 항공기 보호 정보.
		TextAsset airshipTextAsset = (TextAsset)Resources.Load("XMLData/AirshipDefenseData",typeof(TextAsset));
		XmlDocument airshipXmlDoc = new XmlDocument();
		airshipXmlDoc.LoadXml(airshipTextAsset.text);
		
		_AirshipdefenseData = new AirshipDefense[airshipXmlDoc["AirshipDefenseData"].ChildNodes.Count];
		
		xmlIndex = 0;
		foreach(XmlElement xmlElement in airshipXmlDoc["AirshipDefenseData"]) {
			
			AirshipDefense airshipDefense = new AirshipDefense();
			airshipDefense.id = System.Convert.ToByte(xmlElement["id"].InnerText);
			airshipDefense.rewardCost = System.Convert.ToInt16(xmlElement["rewardCost"].InnerText);
			airshipDefense.ghostClose = System.Convert.ToInt16(xmlElement["ghostClose"].InnerText);
			airshipDefense.lockTownCount = System.Convert.ToByte(xmlElement["lockTownCount"].InnerText);
			airshipDefense.ScriptId = System.Convert.ToInt32(xmlElement["ScriptId"].InnerText);
			
			_AirshipdefenseData[xmlIndex] = airshipDefense;
			
			xmlIndex ++;
		}

	}

	public Town[] GetDefaultTownList() {
		return _DefaultTownData;
	}

	/** 사용 가능한 항공기 보호 정보를 반환 */
	public ArrayList GetOpenAirshipDefense() {

		ArrayList openList = new ArrayList();

		byte townCount = (byte)(_UserTownData.Count);
		foreach(AirshipDefense airshipDefense in _AirshipdefenseData) {
			if(airshipDefense.lockTownCount <= townCount) {
				openList.Add(airshipDefense);
			}
		}

		openList.Sort(new AirDefenseSort());

		return openList;
	}

	public AirshipDefense GetAirshipDefenseById(byte id) {
		foreach(AirshipDefense airshipDefense in _AirshipdefenseData) {
			if(airshipDefense.id == id) return airshipDefense;
		}

		return null;
	}

	/**
	 * 해당 하는 Town ID에 해상하는 Town 정보를 반환합니다.
	 * */
	public Town GetDefaultTownData(byte townId) {
		int index = 0;
		foreach(Town thisTown in _DefaultTownData) {
			if(thisTown.id == townId) return thisTown;
			index ++;
		}	

		return null;
	}

	/** 사용자의 마을 정보를 리스트로 반환 */
	public ArrayList GetUserTownList() {
		return _UserTownData;
	}

	/** id에 해당하는 사용자의 마을 정보를 반환 */
	public UserTown GetUserTownByID(byte id) {
		foreach(UserTown userTown in _UserTownData) {
			if(userTown.id == id) return userTown;
		}

		return null;
	}

	/** rootId에 해당하는 고스트 타운 정보를 반환 - isAttackType : 공격이 가능한 타운 존재시. */
	public GhostTown GetGhostTownByRootId(byte rootId, bool isAttackType) {
		foreach(Town town in _DefaultTownData) {
			if(town.root == rootId) {
				GhostTown ghostTown = GetGhostTownByTownId(town.id);
				if(isAttackType) {
					if(ghostTown != null && ghostTown.ghostClose >= _SystemData.GhostAttachLimit) return ghostTown;
				} else {
					if(ghostTown != null) return ghostTown;
				}
			}
		}

		return null;
	}

	/** id에 해당하는 마을을 UserTown에 추가 */
	public void AddUserTown(byte townId) {
		Town town = GetDefaultTownData(townId);
		UserTown userTown = new UserTown();
		userTown.id = town.id;
		userTown.resident = _SystemData.TownDefaultResident;
		userTown.lastResidentPlusTime = (_SystemData.TownNextResidentTime * _SystemData.millisecondNum) + _SystemData.getCurrentTime();
		userTown.lastInvasionEndTime = (_SystemData.GhostAttachDelay * _SystemData.millisecondNum) + _SystemData.getCurrentTime();
		userTown.lastTaxTime = _SystemData.getCurrentTime();

		// 타겟 고스트 둥지에 공격 시간 지연 처리.
		Town ghostDefaultTown;
		foreach(GhostTown ghostTown in _GhostTownData) {
			ghostDefaultTown = GetDefaultTownData(ghostTown.id);
			if(ghostDefaultTown.root == townId) {
				ghostTown.lastAttackTime = (_SystemData.GhostAttachDelay * _SystemData.millisecondNum) + _SystemData.getCurrentTime();
			}
		}

		_UserTownData.Add(userTown);

		LocalData.getInstence().UserTownDataSave();
	}

	/** townid에 해당하는 UserTown을 제거 */
	public void RemoveUserTown(byte townId) {
		UserTown userTown = GetUserTownByID(townId);
		_UserTownData.Remove(userTown);

		LocalData.getInstence().UserTownDataSave();
	}

	/** townId에 해당하는 GhostTown을 추가. */
	public void AddGhostTown(byte townId) {
		Town town = GetDefaultTownData(townId);
		GhostTown ghostTown = new GhostTown();
		ghostTown.id = town.id;
		ghostTown.ghostClose = _SystemData.GhostDefaultClose;
		ghostTown.isView = true;
		ghostTown.lastClosePlusTime = (_SystemData.GhostTownNextTime * _SystemData.millisecondNum) + _SystemData.getCurrentTime();
		ghostTown.lastAttackTime = (_SystemData.GhostAttachDelay * _SystemData.millisecondNum) + _SystemData.getCurrentTime();
		_GhostTownData.Add(ghostTown);

		LocalData.getInstence().GhostTownDataSave();
	}

	/** townId에 해당하는 GhostTown을 제거 */
	public void RemoveGhostTown(byte townId) {
		GhostTown ghostTown = GetGhostTownByTownId(townId);
		_GhostTownData.Remove(ghostTown);

		LocalData.getInstence().GhostTownDataSave();
	}

	/** townId에 해당하는 마을을 root로 가지고 있는 마을 ID 목록을 반환 */
	public ArrayList FindRootTown(byte townId) {
		ArrayList idList = new ArrayList();
		foreach(Town town in _DefaultTownData) {
			if(town.root == townId) idList.Add(town.id);
		}
		return idList;
	}

	public ArrayList GetGhostTownList() {
		return _GhostTownData;
	}

	public ArrayList GetInvasionTownList() {

		ArrayList InvasionTownData = new ArrayList();

		foreach(UserTown userTown in _UserTownData) {
			if(userTown.isInvasion == true) InvasionTownData.Add(userTown);
		}

		return InvasionTownData;
	}

	public GhostTown GetGhostTownByTownId(byte townId) {
		foreach(GhostTown ghostTown in _GhostTownData) {
			if(ghostTown.id == townId) return ghostTown;
		}
		return null;
	}

	public ArrayList GetShowGhostTownList() {

		ArrayList ShowGhostTownData = new ArrayList();

		foreach(GhostTown ghostTown in _GhostTownData) {
			if(ghostTown.isView == true) ShowGhostTownData.Add(ghostTown);
			//ShowGhostTownData.Add(ghostTown);
		}

		return ShowGhostTownData;
	}

	/** 고스트 둥지의 밀집도에 다른 대전 상대 생성 후 ID 목록을 반환 */
	public ArrayList GetGhostDataByGhostClose(short close) {

		_GhostData = new ArrayList();
		ArrayList ghostList = new ArrayList();

		//close = 199;
		short[] ghostLevelList = new short[]{0, 0, 0, 0, 0};
		short totalClose = close;
		int levelIndex = 0;
		for(short i = close; i >= 0; i-= 100) {
			if(i > 0) {
				ghostLevelList[levelIndex] += 100;
			} 

			levelIndex ++;
			if(levelIndex >= 5) levelIndex = 0;
		}

		Ghost ghost;
		DefaultGhost defaultGhost;
		short closeValue = 0;
		for (byte i = 0; i < 5; i++) {
			closeValue = ghostLevelList[i];
			if(closeValue > 0) {
				defaultGhost = GetDefaultGhostByCloseValue(closeValue);
				ghost = new Ghost();
				ghost.id = (byte)(i + 1);
				ghost.defaultId = defaultGhost.id;
				ghost.currentHP = defaultGhost.HP;
				ghost.maxHP = defaultGhost.HP;
				_GhostData.Add(ghost);
				ghostList.Add(ghost.id);
			}
		}

		return ghostList;
	}

	/** 해당 마을을 공격하는 대전 상대 생성 후 반환 */
	public ArrayList GetGhostDataByTownId(byte TownId) {

		UserTown userTown = GetUserTownByID(TownId);
		GhostTown ghostTown = GetGhostTownByTownId(userTown.invasionGhostTownId);
		if(ghostTown != null) {
			return GetGhostDataByGhostClose(ghostTown.ghostClose);
		} else {
			print("공격하는 둥지 없보가 없음. townId : " + TownId);
		}
		return null;
	}

	public DefaultGhost GetDefaultGhostByGhostId(byte ghostid) {
		foreach(DefaultGhost ghost in _DefaultGhostData) {
			if(ghost.id == ghostid) return ghost;
		}

		return null;

	}

	public DefaultGhost GetDefaultGhostByCloseValue(short closeValue) {
		foreach(DefaultGhost ghost in _DefaultGhostData) {
			if(ghost.closeValue == closeValue) return ghost;
		}
		
		return _DefaultGhostData[_DefaultGhostData.Length - 1];
		
	}

	public Ghost GetGhostByGhostId(short ghostid) {
		foreach(Ghost ghost in _GhostData) {
			if(ghost.id == ghostid) return ghost;
		}
		return null;
	}

	/** 마을 세금 노티 연산 */
	public void SetTaxNotification() {
		long maxTime = 0;
		foreach(UserTown userTown in _UserTownData) {
			if(userTown.lastTaxTime > maxTime) maxTime = userTown.lastTaxTime;
		}

		maxTime += _SystemData.TownTaxCollectdelay * _SystemData.millisecondNum;
		maxTime -= _SystemData.getCurrentTime();
		int timeSecond = (int)(maxTime / _SystemData.millisecondNum);

		if(timeSecond > 60) {
			NotificationManager.getInstence().SetTaxNotification(timeSecond + 300);
		}
	}

	public class AirDefenseSort : IComparer
	{
		public int Compare(object x, object y)
		{
			// reverse the arguments
			int g1 = ((AirshipDefense)x).ghostClose;
			int g2 = ((AirshipDefense)y).ghostClose;
			
			if (g1 > g2)
				return -1;
			else
				return 0;
		}
		
	}
}
