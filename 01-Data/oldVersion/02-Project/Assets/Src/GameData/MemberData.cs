using UnityEngine;
using System.Xml;
using System.Collections;

/*
 * 게임내의 사용자 대원들의 정보를 갑니다.
 * 
 * */

public class MemberData : MonoBehaviour {

	private DefaultMember[] _DefaultMembers;
	private FaceData[] _FaceDatas;
	private ArrayList _CurrentMemberList;
	private ClassModel[] _ClassDataList;

	public Vector3[,] _UnitPoseData;

	private static MemberData _instence;

	
	public static MemberData getInstence()	{
		if(_instence == null) 
		{
			//_instence = new DataController();
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<MemberData>();
			DontDestroyOnLoad(_instence);
		}
		
		return _instence;
	}

	void Awake() {

		int xmlIndex = 0;
		// 기본 계급 데이터 초기화.
		TextAsset classAsset = (TextAsset)Resources.Load("XMLData/ClassData",typeof(TextAsset));
		XmlDocument classXmlDoc = new XmlDocument();
		classXmlDoc.LoadXml(classAsset.text);

		_ClassDataList = new ClassModel[classXmlDoc["ClassData"].ChildNodes.Count];

		foreach(XmlElement xmlElement in classXmlDoc["ClassData"]) {
			_ClassDataList[xmlIndex] = new ClassModel();
			_ClassDataList[xmlIndex].id = System.Convert.ToByte(xmlElement["id"].InnerText);
			_ClassDataList[xmlIndex].Markid = xmlElement["markId"].InnerText.ToString();
			_ClassDataList[xmlIndex].exp = System.Convert.ToInt32(xmlElement["exp"].InnerText);
			_ClassDataList[xmlIndex].scoutCost = System.Convert.ToInt32(xmlElement["scoutCost"].InnerText);
			_ClassDataList[xmlIndex].HP = System.Convert.ToInt16(xmlElement["HP"].InnerText);
			_ClassDataList[xmlIndex].MP = System.Convert.ToInt16(xmlElement["MP"].InnerText);
			_ClassDataList[xmlIndex].IA = System.Convert.ToInt16(xmlElement["IA"].InnerText);
			_ClassDataList[xmlIndex].scriptId = System.Convert.ToInt32(xmlElement["scriptId"].InnerText);
			_ClassDataList[xmlIndex].healCost = System.Convert.ToInt32(xmlElement["healCost"].InnerText);
			_ClassDataList[xmlIndex].healChipCount = System.Convert.ToByte(xmlElement["healChipCount"].InnerText);
			_ClassDataList[xmlIndex].rescueCost = System.Convert.ToInt32(xmlElement["rescueCost"].InnerText);
			_ClassDataList[xmlIndex].RescueCoreCount = System.Convert.ToByte(xmlElement["RescueCoreCount"].InnerText);

			xmlIndex ++;
		}

		// 기본 맴버 데이터 초기화.
		TextAsset textAsset = (TextAsset)Resources.Load("XMLData/DefaultMemberData",typeof(TextAsset));
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(textAsset.text);
		
		_DefaultMembers = new DefaultMember[xmlDoc["defaultMemberData"].ChildNodes.Count];
		
		xmlIndex = 0;
		foreach(XmlElement xmlElement in xmlDoc["defaultMemberData"]) {
			_DefaultMembers[xmlIndex] = new DefaultMember();
			_DefaultMembers[xmlIndex].Id = System.Convert.ToByte(xmlElement["id"].InnerText);
			_DefaultMembers[xmlIndex].townId = System.Convert.ToByte(xmlElement["townId"].InnerText);
			_DefaultMembers[xmlIndex].unlockTownLevel = System.Convert.ToByte(xmlElement["unlockTownLevel"].InnerText);
			_DefaultMembers[xmlIndex].classN = System.Convert.ToByte(xmlElement["class"].InnerText);
			_DefaultMembers[xmlIndex].thumbId = xmlElement["thumbId"].InnerText.ToString();
			_DefaultMembers[xmlIndex].nameId = System.Convert.ToInt32(xmlElement["nameId"].InnerText);

			xmlIndex ++;
		}

		// 기본 얼굴 데이터 초기화.
		TextAsset faceAsset = (TextAsset)Resources.Load("XMLData/CharacterFaceData",typeof(TextAsset));
		XmlDocument faceXmlDoc = new XmlDocument();
		faceXmlDoc.LoadXml(faceAsset.text);
		
		_FaceDatas = new FaceData[faceXmlDoc["CharacterFaceData"].ChildNodes.Count];
		
		xmlIndex = 0;
		foreach(XmlElement xmlElement in faceXmlDoc["CharacterFaceData"]) {
			_FaceDatas[xmlIndex] = new FaceData();
			_FaceDatas[xmlIndex].id = System.Convert.ToInt16(xmlElement["id"].InnerText);
			_FaceDatas[xmlIndex].FaceR = System.Convert.ToSingle(xmlElement["FaceR"].InnerText);
			_FaceDatas[xmlIndex].FaceG = System.Convert.ToSingle(xmlElement["FaceG"].InnerText);
			_FaceDatas[xmlIndex].FaceB = System.Convert.ToSingle(xmlElement["FaceB"].InnerText);
			_FaceDatas[xmlIndex].LipR = System.Convert.ToSingle(xmlElement["LipR"].InnerText);
			_FaceDatas[xmlIndex].LipG = System.Convert.ToSingle(xmlElement["LipG"].InnerText);
			_FaceDatas[xmlIndex].LipB = System.Convert.ToSingle(xmlElement["LipB"].InnerText);
			_FaceDatas[xmlIndex].EyeR = System.Convert.ToSingle(xmlElement["EyeR"].InnerText);
			_FaceDatas[xmlIndex].EyeG = System.Convert.ToSingle(xmlElement["EyeG"].InnerText);
			_FaceDatas[xmlIndex].EyeB = System.Convert.ToSingle(xmlElement["EyeB"].InnerText);
			_FaceDatas[xmlIndex].HairR = System.Convert.ToSingle(xmlElement["HairR"].InnerText);
			_FaceDatas[xmlIndex].HairG = System.Convert.ToSingle(xmlElement["HairG"].InnerText);
			_FaceDatas[xmlIndex].HairB = System.Convert.ToSingle(xmlElement["HairB"].InnerText);
			_FaceDatas[xmlIndex].EyeUri = xmlElement["EyeUri"].InnerText;
			_FaceDatas[xmlIndex].EyeLineUri = xmlElement["EyeLineUri"].InnerText;
			_FaceDatas[xmlIndex].HairUri = xmlElement["HairUri"].InnerText;
			_FaceDatas[xmlIndex].LipUri = xmlElement["LipUri"].InnerText;
			_FaceDatas[xmlIndex].BodyScaleType = System.Convert.ToByte(xmlElement["BodyScale"].InnerText);
			_FaceDatas[xmlIndex].ChestType = System.Convert.ToByte(xmlElement["ChestType"].InnerText);
			_FaceDatas[xmlIndex].PoseType = System.Convert.ToByte(xmlElement["PoseType"].InnerText);
			
			xmlIndex ++;
		}

		// 캐릭터 포즈 정보 초기화.
		_UnitPoseData = new Vector3[6,8];

		// 기본 포즈.
		_UnitPoseData[0,0] = new Vector3(-1.76982f, -4.942778f, 26.05168f);	// 왼쪽 허벅지.
		_UnitPoseData[0,1] = new Vector3(-0.8838974f, -4.770328f, 4.313354f);	// 오른쪽 허벅지.
		_UnitPoseData[0,2] = new Vector3(-2.542605f, -8.758736f, 0f);	// 왼쪽 다리.
		_UnitPoseData[0,3] = new Vector3(-1.861806f, -7.520075f, 342.8547f);	// 오른쪽 다리.
		_UnitPoseData[0,4] = new Vector3(-2.260157f, -0.7311051f, 0f);	// 왼쪽 팔.
		_UnitPoseData[0,5] = new Vector3(0.3078436f, -0.1931434f, 0f);	// 오른쪽 팔.
		_UnitPoseData[0,6] = new Vector3(-1.8124f, -3.131952f, 327.8025f);	// 왼쪽 손.
		_UnitPoseData[0,7] = new Vector3(1.116538f, -2.3661f, 321.4666f);	// 오른쪽 손.

		// 포즈 1.
		_UnitPoseData[1,0] = new Vector3(-1.648112f, -4.980494f, 31.27071f);	// 왼쪽 허벅지.
		_UnitPoseData[1,1] = new Vector3(-0.5450702f, -4.855452f, 12.92508f);	// 오른쪽 허벅지.
		_UnitPoseData[1,2] = new Vector3(-2.160396f, -8.700426f, 0f);	// 왼쪽 다리.
		_UnitPoseData[1,3] = new Vector3(-0.8472062f, -7.746376f, 0f);	// 오른쪽 다리.
		_UnitPoseData[1,4] = new Vector3(-2.260157f, -0.7311051f, 0f);	// 왼쪽 팔.
		_UnitPoseData[1,5] = new Vector3(0.135494f, -0.2503456f, 344.6827f);	// 오른쪽 팔.
		_UnitPoseData[1,6] = new Vector3(-2.062444f, -3.325826f, 317.4847f);	// 왼쪽 손.
		_UnitPoseData[1,7] = new Vector3(0.8320043f, -2.301512f, 329.1636f);	// 오른쪽 손.

		// 포즈 2.
		_UnitPoseData[2,0] = new Vector3(-2.68752f, -4.608355f, -5.341072f);	// 왼쪽 허벅지.
		_UnitPoseData[2,1] = new Vector3(-0.3762188f, -4.777517f, 20.34534f);	// 오른쪽 허벅지.
		_UnitPoseData[2,2] = new Vector3(-4.303523f, -8.193892f, 0f);	// 왼쪽 다리.
		_UnitPoseData[2,3] = new Vector3(-0.7692751f, -7.720417f, -8.507446f);	// 오른쪽 다리.
		_UnitPoseData[2,4] = new Vector3(-2.338271f, -0.8112382f, 0f);	// 왼쪽 팔.
		_UnitPoseData[2,5] = new Vector3(0.0835106f, -0.2602504f, 340.9582f);	// 오른쪽 팔.
		_UnitPoseData[2,6] = new Vector3(-1.336157f, -2.362131f, 0f);	// 왼쪽 손.
		_UnitPoseData[2,7] = new Vector3(0.4652669f, -2.457564f, 317.3772f);	// 오른쪽 손.

		// 포즈 3.
		_UnitPoseData[3,0] = new Vector3(-1.916396f, -5.046492f, 22.71073f);	// 왼쪽 허벅지.
		_UnitPoseData[3,1] = new Vector3(-1.411062f, -4.824814f, 348.7668f);	// 오른쪽 허벅지.
		_UnitPoseData[3,2] = new Vector3(-2.747495f, -8.838097f, 0f);	// 왼쪽 다리.
		_UnitPoseData[3,3] = new Vector3(-2.941711f, -7.448858f, 339.9222f);	// 오른쪽 다리.
		_UnitPoseData[3,4] = new Vector3(-2.112709f, -0.7521698f, 8.827717f);	// 왼쪽 팔.
		_UnitPoseData[3,5] = new Vector3(0.02854764f, -0.2452531f, 339.7047f);	// 오른쪽 팔.
		_UnitPoseData[3,6] = new Vector3(-2.065228f, -3.54203f, 306.0742f);	// 왼쪽 손.
		_UnitPoseData[3,7] = new Vector3(0.5282376f, -2.363142f, 325.6659f);	// 오른쪽 손.

		// 포즈 4.
		_UnitPoseData[4,0] = new Vector3(-1.144545f, -4.798933f, 49.69807f);	// 왼쪽 허벅지.
		_UnitPoseData[4,1] = new Vector3(-0.1075488f, -4.69571f, 28.83477f);	// 오른쪽 허벅지.
		_UnitPoseData[4,2] = new Vector3(-0.3018299f, -8.514938f, 22.67217f);	// 왼쪽 다리.
		_UnitPoseData[4,3] = new Vector3(-0.01096547f, -7.611777f, 0.8499041f);	// 오른쪽 다리.
		_UnitPoseData[4,4] = new Vector3(-2.166559f, -0.8419983f, 7.360397f);	// 왼쪽 팔.
		_UnitPoseData[4,5] = new Vector3(0.2065678f, -0.2710397f, 345.3521f);	// 오른쪽 팔.
		_UnitPoseData[4,6] = new Vector3(-2.496665f, -3.591778f, 293.873f);	// 왼쪽 손.
		_UnitPoseData[4,7] = new Vector3(0.9508123f, -2.241758f, 335.1962f);	// 오른쪽 손.
	}

	public DefaultMember[] GetDefaultMembers() {
		return _DefaultMembers;
	}

	public DefaultMember GetDefaultMemberByID(short id) {
		foreach(DefaultMember member in _DefaultMembers) {
			if(member.Id == id) {
				return member;
			}
		}
		
		return null;
	}

	public ArrayList GetDefaultMemberIdsByUserTown(UserTown userTown) {
		ArrayList memberList = new ArrayList();
		byte memberId;
		short resident = userTown.resident;
		byte townId = userTown.id;
		foreach(DefaultMember defaultMember in _DefaultMembers) {
			if(townId == defaultMember.townId) {
				memberId = defaultMember.Id;
				memberList.Add(memberId);
			}
		}

		return memberList;
	}

	public ArrayList GetDefaultMemberIdsByUnlock() {

		ArrayList memberList = new ArrayList();
		UserTown userTown;
		foreach(DefaultMember defaultMember in _DefaultMembers) {
			if(UserData.getInstence().GetMemberById(defaultMember.Id) != null) {
				continue;
			}
			userTown = WarzoneData.getInstence().GetUserTownByID(defaultMember.townId);
			if(userTown != null && userTown.resident > ((defaultMember.unlockTownLevel - 1) * 500)) {
				memberList.Add(defaultMember.Id);
			}
		}


		return memberList;
	}

	/** 대원 찾기를 통해 해당하는 대원정보를 ID로 반환함. */
	public ArrayList GetRandomMemberidsByCore(byte coreCount) {

		/** coreCount 형태
		 * 0 : 일반 확인용.
		 * 1 : 일반 재 탐색.
		 * & : 나머지는 모두 과금 대상.
		 * */
		if(coreCount > 0) {	// 과금 선택시 모두 초기화.
			UserData.getInstence().UserCandidateMemberIds = new ArrayList();
		}

		ArrayList newMemberList = UserData.getInstence().UserCandidateMemberIds;
		ArrayList candidateList = new ArrayList();
		ArrayList nextCandidateList = new ArrayList();

		//int townCount = WarzoneData.getInstence()._UserTownData.Count;
		int townCount = 1;
		int randomLevel = 0;

		townCount -= newMemberList.Count;
		if(townCount <= 0) {
			return newMemberList;
		}

		// 1차 확률 적용.
		if(coreCount > 1) {	// 과금 선택.
			randomLevel = UnityEngine.Random.Range(1, 4);
		} else {
			randomLevel = 0;
		}

		foreach(DefaultMember defaultMember in _DefaultMembers) {
			if(randomLevel == 0) {
				if(defaultMember.unlockTownLevel == 0) {
					candidateList.Add(defaultMember.Id);
					print("defaultMember.Id : " + defaultMember.Id);
				}
			} else {
				if(defaultMember.unlockTownLevel > 0 && defaultMember.unlockTownLevel <= randomLevel) {
					candidateList.Add(defaultMember.Id);
					print("defaultMember.Id : " + defaultMember.Id);
				}
			}
		}
		// 2차 확률 적용.
		for(int i = 0; i < townCount; i++) {
			int randomCount = candidateList.Count;
			print("randomCount : " + randomCount);
			int randomIndex = UnityEngine.Random.Range(0, randomCount);
			newMemberList.Add(candidateList[randomIndex]);
		}

		LocalData.getInstence().UserCandidateMembersSave();

		return newMemberList;
	}

	public ClassModel GetClassModelByClassId(byte id) {
		foreach(ClassModel classmodel in _ClassDataList) {
			if(classmodel.id == id) return classmodel;
		}
		return null;
	}

	public ClassModel GetNextLClassModel(byte classId) {
		ClassModel nextClassModel = null;

		int index = 0;
		print("_ClassDataList.Length : " + _ClassDataList.Length);
		foreach(ClassModel classmodel in _ClassDataList) {
			if(classmodel.id == classId) {
				if(_ClassDataList.Length > (index + 1)) {
					print("index : " + index);
					nextClassModel = _ClassDataList[index + 1] as ClassModel;
				}
			}
			index ++;
		}

		return nextClassModel;
	}

	public FaceData GetFaceDataById(short defaultMemberId) {

		foreach(FaceData faceData in _FaceDatas) {
			if(faceData.id == defaultMemberId) return faceData;
		}

		return null;
	}

}
