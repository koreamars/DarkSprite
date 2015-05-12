using UnityEngine;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections;

public class LocalData : MonoBehaviour {

	private static LocalData _instence;

	private MemberData _MemberData;
	private UserData _UserData;
	private WarzoneData _WarzoneData;
	private SystemData _SystemData;

	public bool IntroComplete = false;

	public static LocalData getInstence()	{
		if(_instence == null) 
		{
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<LocalData>();
			DontDestroyOnLoad(_instence);
		}
		
		return _instence;
	}

	void Awake() {
		_WarzoneData = WarzoneData.getInstence();
		_SystemData = SystemData.GetInstance();
		_MemberData = MemberData.getInstence();
		_UserData = UserData.getInstence();
		ScriptData.getInstence();
	}

	/** 모든 사용자 정보를 저장 합니다. */
	public void AllSave() {

		PlayerPrefs.SetInt("FristSave", (IntroComplete) ? 1 : 0);
		PlayerPrefs.SetInt("GameBGM", (_UserData.Option_BGM) ? 1 : 0);
		PlayerPrefs.SetInt("GameSound", (_UserData.Option_Sound) ? 1 : 0);
		PlayerPrefs.SetInt("IsStoryCharge", (_SystemData.IsStoryCharge) ? 1 : 0);
		PlayerPrefs.SetInt("UserMoney", _UserData.UserMoney);
		PlayerPrefs.SetInt("UserCores", _UserData.UserCores);
		PlayerPrefs.SetInt("UserChips", _UserData.UserChips);
		PlayerPrefs.SetInt("UserBannerStandByCount", _UserData.UserBannerStandByCount);
		PlayerPrefs.SetInt("BaseLevel", _UserData.BaseLevel);
		PlayerPrefs.SetInt("StoryStep", _UserData.StoryStepId);
		PlayerPrefs.SetInt("ResearchLevel", _UserData.ResearchLevel);
		PlayerPrefs.SetInt("FactoryLevel", _UserData.FactoryLevel);
		PlayerPrefs.SetString("MemberList", MemberDataToString());
		PlayerPrefs.SetString("UnitList", UnitDataToString());
		PlayerPrefs.SetString("UserTownList", UserTownDataToString());
		PlayerPrefs.SetString("GhostTownList", GhostTownDataToString());
		PlayerPrefs.SetString("UserResearchList", UserResearchDataToString());
		PlayerPrefs.SetString("UserOwnGearList", UserOwnGearDataToString());
		PlayerPrefs.SetString("UserMakeGearList", MakeOwnGearDataToString());
		PlayerPrefs.SetString("UserMissionList", UserMissionDataToString());
		PlayerPrefs.SetString("UserCandidateMemberIds", UserCandidateMemberIdsToString());
		PlayerPrefs.SetString("SingleCountValues", _UserData.SingleCountValues);

		PlayerPrefs.Save();
	}

	/** 모든 사용자 정보를 불러 옵니다. */
	public void AllLoad() {
		IntroComplete = (PlayerPrefs.GetInt("FristSave") == 1);

		if(!IntroComplete) {
			AllClear();
		} else {

			_UserData.Option_BGM = (PlayerPrefs.GetInt("GameBGM") == 1);
			_UserData.Option_Sound = (PlayerPrefs.GetInt("GameSound") == 1);
			_SystemData.IsStoryCharge = (PlayerPrefs.GetInt("IsStoryCharge") == 1);

			_UserData.UserMoney = PlayerPrefs.GetInt("UserMoney");
			_UserData.UserCores = PlayerPrefs.GetInt("UserCores");
			_UserData.UserChips = PlayerPrefs.GetInt("UserChips");

			_UserData.BaseLevel = (byte)(PlayerPrefs.GetInt("BaseLevel"));
			_UserData.ResearchLevel = (byte)(PlayerPrefs.GetInt("ResearchLevel"));
			_UserData.FactoryLevel = (byte)(PlayerPrefs.GetInt("FactoryLevel"));

			_UserData.StoryStepId = (int)(PlayerPrefs.GetInt("StoryStep"));

			_UserData.SingleCountValues = PlayerPrefs.GetString("SingleCountValues");
			if(_UserData.SingleCountValues.Length == 0) _UserData.SingleCountValues = "000000000000";

			// 유저 베너 노출 대기 카운트.
			_UserData.UserBannerStandByCount = (int)(PlayerPrefs.GetInt("UserBannerStandByCount"));

			// 맴버 정보.
			_UserData.UserMemberList = MemberDataToMemberList(PlayerPrefs.GetString("MemberList"));

			// 네임 보정 로직.
			bool isSumNameOver = false;
			foreach(Member member in _UserData.UserMemberList) {
				if(member.NameId > 100) {
					isSumNameOver = true;
					break;
				}
			}

			if(isSumNameOver == true) {
				foreach(Member member in _UserData.UserMemberList) {
					if(member.NameId > 100) {
						member.NameId = _UserData.GetMemberNameCount(member.DefaultId);
					}
				}
			}

			// 부대 정보.
			_UserData.UserUnitList = UnitDataToUnitList(PlayerPrefs.GetString("UnitList"));

			// 보유 마을 정보.
			_WarzoneData._UserTownData = TownDataToUserTownList(PlayerPrefs.GetString("UserTownList"));

			// 고스트 둥지 정보.
			_WarzoneData._GhostTownData = GhostDataToGhostTownList(PlayerPrefs.GetString("GhostTownList"));

			// 보유 기술 정보.
			_UserData.UserResearchList = ResearchDataToResearchList(PlayerPrefs.GetString("UserResearchList"));

			// 보유 장비 정보.
			_UserData.UserOwnGearList = OwnGearDataToOwnGearList(PlayerPrefs.GetString("UserOwnGearList"));

			// 보유 미션 정보.
			_UserData.UserMissionList = MissionDataToUserMissionList(PlayerPrefs.GetString("UserMissionList"));

			// 미션 데이터 보정. - 지난 미션 갱신 처리.
			foreach(UserMission userMission in _UserData.UserMissionList) {
				DefaultMission defaultMission = MissionData.getInstence().GetDefaultMission(userMission.defaultMissionId);
				if(defaultMission.MissionGoal1Type == MissionGoalType.Target_GhostTown_Unlock ||
				   defaultMission.MissionGoal2Type == MissionGoalType.Target_GhostTown_Unlock) {
					foreach(UserTown userTown in _WarzoneData._UserTownData) {
						MissionData.getInstence().AddMissionGoal(MissionGoalType.Target_GhostTown_Unlock, userTown.id);
					}
				}
			}

			// 제작 중인  장비 정보.
			_UserData.UserMakeGearList = MakeGearDataToMakeGearList(PlayerPrefs.GetString("UserMakeGearList"));

			// 후보 대원 아이디 목록.
			_UserData.UserCandidateMemberIds = CandidateMembersToCandidateMemberData(PlayerPrefs.GetString("UserCandidateMemberIds"));
		}

		//AllClear();
	}

	public void UserStoryStepSave() {
		PlayerPrefs.SetInt("StoryStep", _UserData.StoryStepId);
		PlayerPrefs.SetInt("IsStoryCharge", (_SystemData.IsStoryCharge) ? 1 : 0);
		PlayerPrefs.Save();
	}

	public void UserResourceSave() {
		PlayerPrefs.SetInt("UserMoney", _UserData.UserMoney);
		PlayerPrefs.SetInt("UserCores", _UserData.UserCores);
		PlayerPrefs.SetInt("UserChips", _UserData.UserChips);
		PlayerPrefs.SetInt("UserBannerStandByCount", _UserData.UserBannerStandByCount);
		PlayerPrefs.SetString("SingleCountValues", _UserData.SingleCountValues);
		PlayerPrefs.Save();
	}

	public void UserBaseLevelSave() {
		PlayerPrefs.SetInt("BaseLevel", _UserData.BaseLevel);
		PlayerPrefs.SetInt("ResearchLevel", _UserData.ResearchLevel);
		PlayerPrefs.SetInt("FactoryLevel", _UserData.FactoryLevel);
		PlayerPrefs.Save();
	}

	public void UserMemberDataSave() {
		PlayerPrefs.SetString("MemberList", MemberDataToString());
		PlayerPrefs.Save();
	}

	public void UserUnitDataSave() {
		PlayerPrefs.SetString("UnitList", UnitDataToString());
		PlayerPrefs.Save();
	}

	public void UserTownDataSave() {
		PlayerPrefs.SetString("UserTownList", UserTownDataToString());
		PlayerPrefs.Save();
	}

	public void GhostTownDataSave() {
		PlayerPrefs.SetString("GhostTownList", GhostTownDataToString());
		PlayerPrefs.Save();
	}

	public void UserResearchDataSave() {
		PlayerPrefs.SetString("UserResearchList", UserResearchDataToString());
		PlayerPrefs.Save();
	}

	public void UserOwnGearDataSave() {
		PlayerPrefs.SetString("UserOwnGearList", UserOwnGearDataToString());
		PlayerPrefs.Save();
	}

	public void UserMissionDataSave() {
		PlayerPrefs.SetString("UserMissionList", UserMissionDataToString());
		PlayerPrefs.Save();
	}

	public void UserMakeGearDataSave() {
		PlayerPrefs.SetString("UserMakeGearList", MakeOwnGearDataToString());
		PlayerPrefs.Save();
	}

	public void UserCandidateMembersSave() {
		PlayerPrefs.SetString("UserCandidateMemberIds", UserCandidateMemberIdsToString());
		PlayerPrefs.Save();
	}

	public void UserOptionDataSave() {
		PlayerPrefs.SetInt("GameBGM", (_UserData.Option_BGM) ? 1 : 0);
		PlayerPrefs.SetInt("GameSound", (_UserData.Option_Sound) ? 1 : 0);
		PlayerPrefs.Save();
	}

	/** 모든 사용자 정보를 초기화 합니다. */
	public void AllClear() {

		IntroComplete = false;

		_UserData.UserMoney = 6000;
		_UserData.UserCores = 4;
		_UserData.UserChips = 8;
		_UserData.BaseLevel = 1;
		_UserData.ResearchLevel = 1;
		_UserData.FactoryLevel = 1;
		_UserData.UserBannerStandByCount = _SystemData.UserDefaultBannerCount;

		_UserData.Option_BGM = true;
		_UserData.Option_Sound = true;

		_UserData.UserMemberList = new ArrayList();
		_UserData.UserUnitList = new ArrayList();
		_WarzoneData._UserTownData = new ArrayList();
		_WarzoneData._GhostTownData = new ArrayList();
		_UserData.UserResearchList = new ArrayList();
		_UserData.UserOwnGearList = new ArrayList();
		_UserData.UserMissionList = new ArrayList();
		_UserData.UserMakeGearList = new ArrayList();
		_UserData.UserCandidateMemberIds = new ArrayList();

		_SystemData.IsStoryCharge = false;

		MissionData.getInstence().AddMission(1);
		
		// 초기 부대 및 맴버 등록.
		DefaultMember[] defaultMembers = _MemberData.GetDefaultMembers();
		short memberId =  0;
		int memberCost = 0;
		_UserData.UserMoney += 10000;
		foreach(DefaultMember defaultMember in defaultMembers) {
			if(defaultMember.townId == 0) {
				memberId = _UserData.AddMember(defaultMember.Id);
				ClassModel classmodel = _MemberData.GetClassModelByClassId(defaultMember.classN);
				memberCost = classmodel.scoutCost;
				_UserData.UserMoney += memberCost;

				if(defaultMember.Id == 1) {
					short[] firstUnitMembers = new short[]{memberId, 0, 0, 0, 0};
					_UserData.AddUnit(firstUnitMembers);
				}
			}
		}

		_UserData.UserMoney -= 10000;

		// 초기 마을 정보 설정.
		foreach(Town town in _WarzoneData.GetDefaultTownList()) {
			if(town.state == TownStateType.Nest) {
				_WarzoneData.AddGhostTown(town.id);
				GhostTown ghostTown = _WarzoneData.GetGhostTownByTownId(town.id);
				ghostTown.ghostClose = town.ghostClose;
				ghostTown.isView = town.isView;
			} else {
				_WarzoneData.AddUserTown(town.id);
				UserTown userTown = _WarzoneData.GetUserTownByID(town.id);
				userTown.resident = town.resident;
				userTown.lastTaxTime -= _SystemData.TownTaxCollectdelay * _SystemData.millisecondNum;
			}
		}

		_UserData.UserResearchList = new ArrayList();
		_UserData.UserOwnGearList = new ArrayList();
		_UserData.UserMakeGearList = new ArrayList();

		AllSave();
	}

	// 데이터를 문자 화 ========================================================================================

	/** 맴버 정보 변환 */
	private string MemberDataToString() {

		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml("<MemberList></MemberList>");

		foreach(Member member in _UserData.UserMemberList) {

			XmlElement elem = XmlToXmlDocument("Member", xmlDoc, member);

			xmlDoc.DocumentElement.AppendChild(elem);

		}

		return XmlToString(xmlDoc);
	}

	/** Unit 정보 변환 */
	private string UnitDataToString() {

		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml("<UnitList></UnitList>");
		
		XmlSerializer xmlSerializer;
		StringWriter textWriter;
		
		foreach(Unit unit in _UserData.UserUnitList) {

			xmlSerializer = new XmlSerializer(unit.GetType());
			textWriter = new StringWriter();
			xmlSerializer.Serialize(textWriter, unit);

			XmlDocument unitDoc = new XmlDocument();
			unitDoc.LoadXml(textWriter.ToString());
			
			XmlElement elem = xmlDoc.CreateElement("Unit");
			XmlElement childElem;
			
			foreach(XmlNode xmlnode in unitDoc.ChildNodes[1].ChildNodes) {
				childElem = xmlDoc.CreateElement(xmlnode.Name);
				if(xmlnode.Name == "memberList") {
					string memberList = "";
					foreach(byte memberId in unit.memberList) {
						if(memberList == "") {
							memberList += memberId;
						} else {
							memberList += ":" + memberId;
						}
					}
					childElem.InnerText = memberList;
				} else {
					childElem.InnerText = xmlnode.InnerText;
				}
				elem.AppendChild(childElem);
			}
			
			xmlDoc.DocumentElement.AppendChild(elem);
			
		}

		return XmlToString(xmlDoc);
	}

	/** UserTown 정보 변환 */
	private string UserTownDataToString() {
		
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml("<UserTownList></UserTownList>");

		
		foreach(UserTown userTown in _WarzoneData.GetUserTownList()) {

			XmlElement elem = XmlToXmlDocument("UserTown", xmlDoc, userTown);
			
			xmlDoc.DocumentElement.AppendChild(elem);
			
		}
		return XmlToString(xmlDoc);
	}

	/** GhostTown 정보 변환 */
	private string GhostTownDataToString() {
		
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml("<GhostTownList></GhostTownList>");

		foreach(GhostTown ghostTown in _WarzoneData.GetGhostTownList()) {

			XmlElement elem = XmlToXmlDocument("GhostTown", xmlDoc, ghostTown);

			xmlDoc.DocumentElement.AppendChild(elem);
			
		}

		return XmlToString(xmlDoc);
	}

	/** 보유 기술 정보 변환 */
	private string UserResearchDataToString() {
		
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml("<UserResearchList></UserResearchList>");

		foreach(UserResearch userResearch in _UserData.UserResearchList) {

			XmlElement elem = XmlToXmlDocument("UserResearch", xmlDoc, userResearch);
			
			xmlDoc.DocumentElement.AppendChild(elem);
			
		}

		return XmlToString(xmlDoc);
	}

	/** 보유 장비 정보 변환 */
	private string UserOwnGearDataToString() {
		
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml("<UserOwnGearList></UserOwnGearList>");
		
		foreach(OwnGear ownGear in _UserData.UserOwnGearList) {
			
			XmlElement elem = XmlToXmlDocument("OwnGear", xmlDoc, ownGear);
			
			xmlDoc.DocumentElement.AppendChild(elem);
			
		}

		return XmlToString(xmlDoc);
	}

	/** 보유 장비 정보 변환 */
	private string UserMissionDataToString() {
		
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml("<UserMissionList></UserMissionList>");
		
		foreach(UserMission userMission in _UserData.UserMissionList) {
			
			XmlElement elem = XmlToXmlDocument("UserMission", xmlDoc, userMission);
			
			xmlDoc.DocumentElement.AppendChild(elem);
			
		}
		
		return XmlToString(xmlDoc);
	}

	/** 제작 장비 정보 변환 */
	private string MakeOwnGearDataToString() {
		
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml("<MakeOwnGearList></MakeOwnGearList>");
		
		foreach(MakeGear makeGear in _UserData.UserMakeGearList) {
			
			XmlElement elem = XmlToXmlDocument("MakeGear", xmlDoc, makeGear);
			
			xmlDoc.DocumentElement.AppendChild(elem);
			
		}

		return XmlToString(xmlDoc);
	}

	/** 후보 대원 정보 변환 */
	private string UserCandidateMemberIdsToString() {
		
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml("<UserCandidateMemberIds></UserCandidateMemberIds>");
		
		foreach(byte memberId in _UserData.UserCandidateMemberIds) {
			
			XmlElement elem = xmlDoc.CreateElement("memberId");
			elem.InnerText = memberId.ToString();

			xmlDoc.DocumentElement.AppendChild(elem);
			
		}
		
		return XmlToString(xmlDoc);
	}


	// 데이터를 문자 화 ========================================================================================

	// 문자화를 데이터화  ========================================================================================

	private ArrayList MemberDataToMemberList(string data) {

		ArrayList memberList = new ArrayList();

		XmlDocument xmlDoc = StringToXml(data);

		Member member;

		foreach(XmlElement xmlElement in xmlDoc["MemberList"]) {
			member = new Member();
			member.id = System.Convert.ToInt16(xmlElement["id"].InnerText);
			member.DefaultId = System.Convert.ToByte(xmlElement["DefaultId"].InnerText);
			member.ClassId = System.Convert.ToByte(xmlElement["ClassId"].InnerText);
			member.NameId = System.Convert.ToInt32(xmlElement["NameId"].InnerText);
			member.BodyGearId = System.Convert.ToInt16(xmlElement["BodyGearId"].InnerText);
			member.EngineGearId = System.Convert.ToInt16(xmlElement["EngineGearId"].InnerText);
			member.SuitGearId = System.Convert.ToInt16(xmlElement["SuitGearId"].InnerText);
			member.Weapon1GearId = System.Convert.ToInt16(xmlElement["Weapon1GearId"].InnerText);
			member.Weapon2GearId = System.Convert.ToInt16(xmlElement["Weapon2GearId"].InnerText);
			member.Weapon3GearId = System.Convert.ToInt16(xmlElement["Weapon3GearId"].InnerText);
			member.Thumbnail = xmlElement["Thumbnail"].InnerText;
			member.CurrentHP = System.Convert.ToInt32(xmlElement["CurrentHP"].InnerText);
			member.MaxHP = System.Convert.ToInt32(xmlElement["MaxHP"].InnerText);
			member.CurrentMP = System.Convert.ToInt32(xmlElement["CurrentMP"].InnerText);
			member.MaxMP = System.Convert.ToInt32(xmlElement["MaxMP"].InnerText);
			member.TotalIA = System.Convert.ToInt32(xmlElement["TotalIA"].InnerText);
			member.UMP = System.Convert.ToInt32(xmlElement["UMP"].InnerText);
			member.UnitId = System.Convert.ToByte(xmlElement["UnitId"].InnerText);
			member.Exp = System.Convert.ToInt32(xmlElement["Exp"].InnerText);
			member.lastHPUpdateTime = System.Convert.ToInt64(xmlElement["lastHPUpdateTime"].InnerText);
			member.lastMPUpdateTime = System.Convert.ToInt64(xmlElement["lastMPUpdateTime"].InnerText);
			member.state = System.Convert.ToByte(xmlElement["state"].InnerText);
			memberList.Add(member);
		}

		return memberList;

	}

	/** 부대 정보를 반환함 */
	private ArrayList UnitDataToUnitList(string data) {
		
		ArrayList unitList = new ArrayList();
		
		XmlDocument xmlDoc = StringToXml(data);
		
		Unit unit;
		foreach(XmlElement xmlElement in xmlDoc["UnitList"]) {
			unit = new Unit();
			unit.id = System.Convert.ToByte(xmlElement["id"].InnerText);
			unit.unitNameId = System.Convert.ToInt16(xmlElement["unitNameId"].InnerText);
			unit.memberList = new short[5];
			string memberListStr = xmlElement["memberList"].InnerText;
			string memberIdstr = "";
			byte index = 0;
			foreach(char strN in memberListStr.ToCharArray()) {
				if(strN == System.Convert.ToChar(":")) {
					unit.memberList[index] = System.Convert.ToInt16(memberIdstr);
					memberIdstr = "";
					index ++;
				} else {
					memberIdstr += strN;
				}
			}
			unit.memberList[4] = System.Convert.ToInt16(memberIdstr);
			unitList.Add(unit);
		}
		return unitList;
		
	}

	/** 보유 마을 정보를 반환 */
	private ArrayList TownDataToUserTownList(string data) {
		
		ArrayList townList = new ArrayList();
		
		XmlDocument xmlDoc = StringToXml(data);

		if(xmlDoc["UserTownList"].ChildNodes == null || xmlDoc["UserTownList"].ChildNodes.Count == 0) return townList;

		UserTown userTown;
		foreach(XmlElement xmlElement in xmlDoc["UserTownList"]) {
			userTown = new UserTown();
			userTown.id = System.Convert.ToByte(xmlElement["id"].InnerText);
			userTown.resident = System.Convert.ToInt16(xmlElement["resident"].InnerText);
			userTown.isInvasion = (xmlElement["isInvasion"].InnerText == "true");
			userTown.lastTaxTime = System.Convert.ToInt64(xmlElement["lastTaxTime"].InnerText);
			userTown.lastResidentPlusTime = System.Convert.ToInt64(xmlElement["lastResidentPlusTime"].InnerText);
			userTown.invasionGhostTownId = System.Convert.ToByte(xmlElement["invasionGhostTownId"].InnerText);
			userTown.invasionGhostClose = System.Convert.ToInt16(xmlElement["invasionGhostClose"].InnerText);
			userTown.lastInvasionEndTime = System.Convert.ToInt64(xmlElement["lastInvasionEndTime"].InnerText);

			townList.Add(userTown);
		}

		return townList;
		
	}

	/** 고스트 둥지 정보를 반환 */
	private ArrayList GhostDataToGhostTownList(string data) {
		
		ArrayList ghostTownList = new ArrayList();
		
		XmlDocument xmlDoc = StringToXml(data);
		
		if(xmlDoc["GhostTownList"].ChildNodes == null || xmlDoc["GhostTownList"].ChildNodes.Count == 0) {
			print("GhostDataToGhostTownList null");
			return ghostTownList;
		}
		
		GhostTown ghostTown;
		foreach(XmlElement xmlElement in xmlDoc["GhostTownList"]) {
			ghostTown = new GhostTown();
			ghostTown.id = System.Convert.ToByte(xmlElement["id"].InnerText);
			ghostTown.ghostClose = System.Convert.ToInt16(xmlElement["ghostClose"].InnerText);
			ghostTown.lastClosePlusTime = System.Convert.ToInt64(xmlElement["lastClosePlusTime"].InnerText);
			ghostTown.lastAttackTime = System.Convert.ToInt64(xmlElement["lastAttackTime"].InnerText);
			ghostTown.isView = (xmlElement["isView"].InnerText == "true");

			ghostTownList.Add(ghostTown);
		}

		return ghostTownList;
		
	}

	/** 보유 개발 정보를 반환 */
	private ArrayList ResearchDataToResearchList(string data) {
		
		ArrayList researchList = new ArrayList();
		
		XmlDocument xmlDoc = StringToXml(data);
		
		if(xmlDoc["UserResearchList"].ChildNodes == null || xmlDoc["UserResearchList"].ChildNodes.Count == 0) {
			print("ResearchDataToResearchList null");
			return researchList;
		}
		
		UserResearch userResearch;
		foreach(XmlElement xmlElement in xmlDoc["UserResearchList"]) {
			userResearch = new UserResearch();
			userResearch.id = System.Convert.ToByte(xmlElement["id"].InnerText);
			userResearch.startTime = System.Convert.ToInt64(xmlElement["startTime"].InnerText);
			userResearch.isComplete = (xmlElement["isComplete"].InnerText == "true");
			
			researchList.Add(userResearch);
		}
		return researchList;
		
	}

	/** 보유 장비 정보를 반환 */
	private ArrayList OwnGearDataToOwnGearList(string data) {
		
		ArrayList ownGearList = new ArrayList();
		
		XmlDocument xmlDoc = StringToXml(data);
		
		if(xmlDoc["UserOwnGearList"].ChildNodes == null || xmlDoc["UserOwnGearList"].ChildNodes.Count == 0) {
			print("OwnGearDataToOwnGearList null");
			return ownGearList;
		}
		
		OwnGear ownGear;
		foreach(XmlElement xmlElement in xmlDoc["UserOwnGearList"]) {
			ownGear = new OwnGear();
			ownGear.gearId = System.Convert.ToInt16(xmlElement["gearId"].InnerText);
			ownGear.ownCount = System.Convert.ToByte(xmlElement["ownCount"].InnerText);
			
			ownGearList.Add(ownGear);
		}

		return ownGearList;
		
	}

	/** 보유 미션 정보를 반환 */
	private ArrayList MissionDataToUserMissionList(string data) {
		
		ArrayList missionList = new ArrayList();
		
		XmlDocument xmlDoc = StringToXml(data);

		if(xmlDoc["UserMissionList"] == null) return missionList;
		if(xmlDoc["UserMissionList"].ChildNodes == null || xmlDoc["UserMissionList"].ChildNodes.Count == 0) {
			print("MissionDataToUserMissionList null");
			return missionList;
		}
		
		UserMission userMission;
		foreach(XmlElement xmlElement in xmlDoc["UserMissionList"]) {
			userMission = new UserMission();
			userMission.defaultMissionId = System.Convert.ToInt16(xmlElement["defaultMissionId"].InnerText);
			userMission.currentGoal1 = System.Convert.ToByte(xmlElement["currentGoal1"].InnerText);
			userMission.currentGoal2 = System.Convert.ToByte(xmlElement["currentGoal2"].InnerText);
			
			missionList.Add(userMission);
		}
		
		return missionList;
		
	}

	/** 보유 장비 정보를 반환 */
	private ArrayList MakeGearDataToMakeGearList(string data) {
		
		ArrayList makeGearList = new ArrayList();
		
		XmlDocument xmlDoc = StringToXml(data);
		
		if(xmlDoc["MakeOwnGearList"].ChildNodes == null || xmlDoc["MakeOwnGearList"].ChildNodes.Count == 0) {
			print("MakeGearDataToMakeGearList null");
			return makeGearList;
		}
		
		MakeGear makeGear;
		foreach(XmlElement xmlElement in xmlDoc["MakeOwnGearList"]) {
			makeGear = new MakeGear();
			makeGear.gearId = System.Convert.ToInt16(xmlElement["gearId"].InnerText);
			makeGear.endTime = System.Convert.ToInt64(xmlElement["endTime"].InnerText);
			
			makeGearList.Add(makeGear);
		}
		
		string XmlStr = XmlToString(xmlDoc);
		print(XmlStr);
		
		return makeGearList;
		
	}

	/** 후보 대원 정보를 반환 */
	private ArrayList CandidateMembersToCandidateMemberData(string data) {
		//print(data);
		ArrayList memberList = new ArrayList();
		
		XmlDocument xmlDoc = StringToXml(data);
		
		if(xmlDoc["UserCandidateMemberIds"] == null || xmlDoc["UserCandidateMemberIds"].ChildNodes == null || xmlDoc["UserCandidateMemberIds"].ChildNodes.Count == 0) {
			print("UserCandidateMemberIds null");
			return memberList;
		}

		foreach(XmlElement xmlElement in xmlDoc["UserCandidateMemberIds"]) {
			byte memberId = System.Convert.ToByte(xmlElement.InnerText);
			memberList.Add(memberId);
		}
		
		string XmlStr = XmlToString(xmlDoc);
		print(XmlStr);
		
		return memberList;
		
	}


	// 문자화를 데이터화  ========================================================================================


	private string XmlToString(XmlDocument XmlDoc) {

		XmlSerializer txmlSerializer = new XmlSerializer(XmlDoc.GetType());
		StringWriter ttextWriter = new StringWriter();
		
		txmlSerializer.Serialize(ttextWriter, XmlDoc);

		return ttextWriter.ToString();
	}

	private XmlDocument StringToXml(string data){
		XmlDocument xmlDoc = new XmlDocument();
		if(data == null || data == "") return xmlDoc;
		xmlDoc.LoadXml(data);

		return xmlDoc;
	}

	private XmlElement XmlToXmlDocument(string elementName, XmlDocument mainXmlDoc, object data) {

		XmlSerializer xmlSerializer = new XmlSerializer(data.GetType());
		StringWriter textWriter = new StringWriter();
		xmlSerializer.Serialize(textWriter, data);

		XmlDocument currentXmlDoc = new XmlDocument();
		currentXmlDoc.LoadXml(textWriter.ToString());
		
		XmlElement elem = mainXmlDoc.CreateElement(elementName);
		XmlElement childElem;
		
		foreach(XmlNode xmlnode in currentXmlDoc.ChildNodes[1].ChildNodes) {
			childElem = mainXmlDoc.CreateElement(xmlnode.Name);
			childElem.InnerText = xmlnode.InnerText;
			elem.AppendChild(childElem);
		}

		return elem;
	}

}
