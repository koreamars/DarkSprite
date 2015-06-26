using UnityEngine;
using System.Collections;

public class UnitBody : MonoBehaviour {

	public bool isTest;
	public GameObject NudeBodyObj;

	private GameObject HeadObj;

	private Transform NudeBody;
	private Transform NudeLeftArm;
	private Transform NudeRightArm;
	private Transform NudeLeftHand;
	private Transform NudeRightHand;
	private Transform NudeLeftLeg;
	private Transform NudeRightLeg;
	private Transform NudeLeftThigh;
	private Transform NudeRightThigh;

	public Texture2D CaptureBodyTexture;
	private Camera rdCam;
	private Camera MainCam;

	private short _currentMemberId;
	private short _TestMemberCount = 0;

	public int TestDefaultID = 1;
	public int TestBodyGearID = 1;

	// 바디 스케일 
	private float[] _BodyScale = new float[3]{1f, 0.9f, 1.1f};
	// 머리 포지션.
	private Vector2[] _HeadPos = new Vector2[3]{new Vector2(-0.25f, 1.13f), new Vector2(-0.25f, 1.07f), new Vector2(-0.25f, 1.17f)};

	private GameObject TestObj;

	Vector3[,] _PoseData;

	// 메모리 관리.
	public bool IsMemorySave;			// 정보를 저장할지 결정.
	private ArrayList _SaveMemberIds;	// 저장되는 기체의 맴버의 ID;
	private ArrayList _SaveUnitBodys;	// 저장되는 기체의 정보.

	void Start() {
		if(isTest) {

			StartCoroutine(TestInit());

		}
	}

	private IEnumerator TestInit() {
		yield return new WaitForEndOfFrame();

		yield return StartCoroutine(MissionData.getInstence().init());
		
		LocalData.getInstence().AllLoad();
		DarkSprite.getInstence();
		SystemData.GetInstance();
		MemberData.getInstence();
		UserData.getInstence();
		
		TestObj = GameObject.Find("TestObj");
		
		ArrayList testlist = UserData.getInstence().UserMemberList;
		
		Member member = testlist[1] as Member;
		member.DefaultId = (short)(TestDefaultID);
		member.SuitGearId = (short)(TestBodyGearID);
	}

	private IEnumerator GetUnitBody(short memberId, short sortNum) {

		UserData.getInstence().IsGUIShow = false;
		print("IsMemorySave : " + IsMemorySave);

		yield return new WaitForEndOfFrame();

		Member member = UserData.getInstence().GetMemberById(memberId);

		if(member == null) {
			yield return 0;
			print("member null.");
			UserData.getInstence().IsGUIShow = true;
			yield break;
		}

		NudeBodyObj.transform.localScale = new Vector2(1f, 1f);
		SetFieldObject();

		_PoseData = MemberData.getInstence()._UnitPoseData;

		FaceData faceData = MemberData.getInstence().GetFaceDataById(member.DefaultId);
		Gear suitGear = GearData.getInstence().GetGearByID(member.SuitGearId);

		SetBodySorting(sortNum);
		SetBodyPose(faceData);
		//SetBodyColor(faceData);
		SetBodyGearChange(faceData, suitGear, memberId);

		int bodytypeNum = faceData.BodyScaleType;

		HeadObj = Instantiate(Resources.Load<GameObject>("UnitResource/CharacterHead")) as GameObject;
		HeadObj.GetComponent<CharacterHead>().init(member.DefaultId);
		HeadObj.transform.parent = this.transform;
		HeadObj.transform.localScale = new Vector2(-0.85f, 0.85f);
		HeadObj.GetComponent<CharacterHead>().SetSortingNum(15);

		HeadObj.transform.position = new Vector2(this.transform.position.x + _HeadPos[bodytypeNum].x, this.transform.position.y + _HeadPos[bodytypeNum].y);
		NudeBodyObj.transform.localScale = new Vector2(_BodyScale[bodytypeNum], _BodyScale[bodytypeNum]);

		yield return 1;

		UserData.getInstence().IsGUIShow = true;

	}

	private void SetFieldObject() {
		NudeBody = NudeBodyObj.GetComponent<Transform>().FindChild("body") as Transform;
		NudeLeftLeg = NudeBodyObj.GetComponent<Transform>().FindChild("left-leg") as Transform;
		NudeRightLeg = NudeBodyObj.GetComponent<Transform>().FindChild("right-leg") as Transform;
		NudeLeftThigh = NudeBodyObj.GetComponent<Transform>().FindChild("left-thigh") as Transform;
		NudeRightThigh = NudeBodyObj.GetComponent<Transform>().FindChild("right-thigh") as Transform;
		NudeLeftArm = NudeBodyObj.GetComponent<Transform>().FindChild("left-arm") as Transform;
		NudeRightArm = NudeBodyObj.GetComponent<Transform>().FindChild("right-arm") as Transform;
		NudeLeftHand = NudeBodyObj.GetComponent<Transform>().FindChild("left-hand") as Transform;
		NudeRightHand = NudeBodyObj.GetComponent<Transform>().FindChild("right-hand") as Transform;

		if(HeadObj != null) Destroy(HeadObj);

	}

	private void SetBodySorting(short sortNum) {
		NudeBody.GetComponent<Renderer>().sortingOrder = sortNum;
		NudeLeftLeg.GetComponent<Renderer>().sortingOrder = sortNum + 3;
		NudeRightLeg.GetComponent<Renderer>().sortingOrder = sortNum - 3;
		NudeLeftThigh.GetComponent<Renderer>().sortingOrder = sortNum + 2;
		NudeRightThigh.GetComponent<Renderer>().sortingOrder = sortNum - 3;
		NudeLeftArm.GetComponent<Renderer>().sortingOrder = sortNum + 5;
		NudeRightArm.GetComponent<Renderer>().sortingOrder = sortNum - 3;
		NudeLeftHand.GetComponent<Renderer>().sortingOrder = sortNum + 6;
		NudeRightHand.GetComponent<Renderer>().sortingOrder = sortNum - 4;

	}

	private void SetBodyGearChange(FaceData faceData, Gear suitGear, short memberId) {

		int chestNum = faceData.ChestType;
		bool IsSetBtmap = false;
		// 메모리 저장.
		if(IsMemorySave == true) {
			int memberIndex = -1;
			if(_SaveMemberIds != null) {
				memberIndex = _SaveMemberIds.IndexOf(memberId);
			}
			if(memberIndex < 0) {	// 정보가 없을 경우.
				IsSetBtmap = true;
			} else {		// 정보가 존재시.

				Sprite[] bodyData = _SaveUnitBodys[memberIndex] as Sprite[];

				SetSpriteInChange(NudeBody.gameObject, bodyData[0] as Sprite);
				SetSpriteInChange(NudeLeftLeg.gameObject, bodyData[1] as Sprite);
				SetSpriteInChange(NudeRightLeg.gameObject, bodyData[2] as Sprite);
				SetSpriteInChange(NudeLeftThigh.gameObject, bodyData[3] as Sprite);
				SetSpriteInChange(NudeRightThigh.gameObject, bodyData[4] as Sprite);
				SetSpriteInChange(NudeLeftArm.gameObject, bodyData[5] as Sprite);
				SetSpriteInChange(NudeRightArm.gameObject, bodyData[6] as Sprite);
				SetSpriteInChange(NudeLeftHand.gameObject, bodyData[7] as Sprite);
				SetSpriteInChange(NudeRightHand.gameObject, bodyData[8] as Sprite);

				return;
			}

		} else {	// 메모리 초기화.
			_SaveMemberIds = new ArrayList();
			_SaveUnitBodys = new ArrayList();

			IsSetBtmap = true;
		}

		if(IsSetBtmap == true) {
			SetBitmapChange(NudeBody, "Suit/nudebody/body" + chestNum, "Suit/" + suitGear.resourceURI + "/body" + chestNum, faceData);
			SetBitmapChange(NudeLeftLeg, "Suit/nudebody/left-leg", "Suit/" + suitGear.resourceURI + "/left-leg", faceData);
			SetBitmapChange(NudeRightLeg, "Suit/nudebody/right-leg", "Suit/" + suitGear.resourceURI + "/right-leg", faceData);
			SetBitmapChange(NudeLeftThigh, "Suit/nudebody/left-thigh", "Suit/" + suitGear.resourceURI + "/left-thigh", faceData);
			SetBitmapChange(NudeRightThigh, "Suit/nudebody/right-thigh", "Suit/" + suitGear.resourceURI + "/right-thigh", faceData);
			SetBitmapChange(NudeLeftArm, "Suit/nudebody/left-arm", "Suit/" + suitGear.resourceURI + "/left-arm", faceData);
			SetBitmapChange(NudeRightArm, "Suit/nudebody/right-arm", "Suit/" + suitGear.resourceURI + "/right-arm", faceData);
			SetBitmapChange(NudeLeftHand, "Suit/nudebody/left-hand", "Suit/" + suitGear.resourceURI + "/left-hand", faceData);
			SetBitmapChange(NudeRightHand, "Suit/nudebody/right-hand", "Suit/" + suitGear.resourceURI + "/right-hand", faceData);
		}

		// 메모리 저장.
		if(IsMemorySave == true) {
			if(_SaveMemberIds == null) {
				_SaveMemberIds = new ArrayList();
				_SaveUnitBodys = new ArrayList();
			}
			_SaveMemberIds.Add(memberId);

			Sprite[] bodyData = new Sprite[9];
			bodyData[0] = NudeBody.GetComponent<SpriteRenderer>().sprite;
			bodyData[1] = NudeLeftLeg.GetComponent<SpriteRenderer>().sprite;
			bodyData[2] = NudeRightLeg.GetComponent<SpriteRenderer>().sprite;
			bodyData[3] = NudeLeftThigh.GetComponent<SpriteRenderer>().sprite;
			bodyData[4] = NudeRightThigh.GetComponent<SpriteRenderer>().sprite;
			bodyData[5] = NudeLeftArm.GetComponent<SpriteRenderer>().sprite;
			bodyData[6] = NudeRightArm.GetComponent<SpriteRenderer>().sprite;
			bodyData[7] = NudeLeftHand.GetComponent<SpriteRenderer>().sprite;
			bodyData[8] = NudeRightHand.GetComponent<SpriteRenderer>().sprite;

			_SaveUnitBodys.Add(bodyData);
		}

	}

	private void SetBodyPose(FaceData faceData) {
		int poseNum = faceData.PoseType;
		//int poseNum = 0;
		
		float bodyX = NudeBodyObj.transform.position.x;
		float bodyY = NudeBodyObj.transform.position.y;
		
		NudeLeftLeg.transform.position = new Vector2(bodyX + _PoseData[poseNum, 2].x / 2, bodyY + _PoseData[poseNum, 2].y / 2);
		NudeLeftLeg.transform.rotation = Quaternion.Euler(0f, 0f, _PoseData[poseNum, 2].z);
		NudeRightLeg.transform.position = new Vector2(bodyX + _PoseData[poseNum, 3].x / 2, bodyY + _PoseData[poseNum, 3].y / 2);
		NudeRightLeg.transform.rotation = Quaternion.Euler(0f, 0f, _PoseData[poseNum, 3].z);
		NudeLeftThigh.transform.position = new Vector2(bodyX + _PoseData[poseNum, 0].x / 2, bodyY + _PoseData[poseNum, 0].y / 2);
		NudeLeftThigh.transform.rotation = Quaternion.Euler(0f, 0f, _PoseData[poseNum, 0].z);
		NudeRightThigh.transform.position = new Vector2(bodyX + _PoseData[poseNum, 1].x / 2, bodyY + _PoseData[poseNum, 1].y / 2);
		NudeRightThigh.transform.rotation = Quaternion.Euler(0f, 0f, _PoseData[poseNum, 1].z);
		NudeLeftArm.transform.position = new Vector2(bodyX + _PoseData[poseNum, 4].x / 2, bodyY + _PoseData[poseNum, 4].y / 2);
		NudeLeftArm.transform.rotation = Quaternion.Euler(0f, 0f, _PoseData[poseNum, 4].z);
		NudeRightArm.transform.position = new Vector2(bodyX + _PoseData[poseNum, 5].x / 2, bodyY + _PoseData[poseNum, 5].y / 2);
		NudeRightArm.transform.rotation = Quaternion.Euler(0f, 0f, _PoseData[poseNum, 5].z);
		NudeLeftHand.transform.position = new Vector2(bodyX + _PoseData[poseNum, 6].x / 2, bodyY + _PoseData[poseNum, 6].y / 2);
		NudeLeftHand.transform.rotation = Quaternion.Euler(0f, 0f, _PoseData[poseNum, 6].z);
		NudeRightHand.transform.position = new Vector2(bodyX + _PoseData[poseNum, 7].x / 2, bodyY + _PoseData[poseNum, 7].y / 2);
		NudeRightHand.transform.rotation = Quaternion.Euler(0f, 0f, _PoseData[poseNum, 7].z);

	}

	/** 피부색 변경 */
	private void SetBodyColor(FaceData faceData) {

		int chestNum = faceData.ChestType;
		
		SetSpriteChange(NudeBody, "Suit/nudebody", "body" + chestNum);
		SetSpriteChange(NudeLeftLeg, "Suit/nudebody", "left-leg");
		SetSpriteChange(NudeRightLeg, "Suit/nudebody", "right-leg");
		SetSpriteChange(NudeLeftThigh, "Suit/nudebody", "left-thigh");
		SetSpriteChange(NudeRightThigh, "Suit/nudebody", "right-thigh");
		SetSpriteChange(NudeLeftArm, "Suit/nudebody", "left-arm");
		SetSpriteChange(NudeRightArm, "Suit/nudebody", "right-arm");
		SetSpriteChange(NudeLeftHand, "Suit/nudebody", "left-hand");
		SetSpriteChange(NudeRightHand, "Suit/nudebody", "right-hand");

	}

	/** Sprite 변경 */
	private void SetSpriteChange(Transform targetObj, string uri, string targeturi) {
		SpriteRenderer renderer = (SpriteRenderer)targetObj.GetComponent ("SpriteRenderer");
		renderer.sprite = Resources.Load<Sprite>("UnitResource/" + uri + "/" + targeturi);
	}

	/** Sprite만 교체 */
	private void SetSpriteInChange(GameObject targetObj, Sprite changeData) {
		SpriteRenderer renderer = (SpriteRenderer)targetObj.GetComponent ("SpriteRenderer");
		renderer.sprite = changeData;
	}

	/** 비트맵 합성 */
	private void SetBitmapChange(Transform targetObj, string baseuri, string updateTargeturi, FaceData faceData) {
		//print("baseuri : " + updateTargeturi);
		Color skinColor = Color.white;
		skinColor.r = faceData.FaceR;
		skinColor.g = faceData.FaceG;
		skinColor.b = faceData.FaceB;
		skinColor.a = 1f;
		Texture2D baseTexture = Resources.Load<Texture2D>("UnitResource/" + baseuri) as Texture2D;
		baseTexture = TextureMarge.SetChangeColor(baseTexture, skinColor);
		Texture2D updateTexture = Resources.Load<Texture2D>("UnitResource/" + updateTargeturi) as Texture2D;

		Texture2D newTexture = null;
		if(updateTexture != null) {
			newTexture = TextureMarge.SetTextureMarge(baseTexture, updateTexture);
		} else {
			newTexture = TextureMarge.SetTextureMarge(baseTexture, baseTexture);
		}
		Sprite testSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f));
		SpriteRenderer renderer = (SpriteRenderer)targetObj.GetComponent ("SpriteRenderer");
		renderer.sprite = testSprite;
	}

	void OnGUI() {
		//GUI.Window(0, new Rect(0, 0, Screen.width, Screen.height), DoMyWindow, "");
		if(isTest && UserData.getInstence().IsGUIShow) {
			if(GUI.Button(new Rect(0, 30, 100, 30), "test")) {
				ArrayList testList = UserData.getInstence().GetMemberList();
				Member testMember = testList[_TestMemberCount] as Member;
				testMember.DefaultId = (short)(TestDefaultID);
				testMember.SuitGearId = (short)(TestBodyGearID);
				short testMemberId = testMember.id;
				StartCoroutine(SetUnitBody(testMemberId, 0));
				_TestMemberCount ++;
				if(_TestMemberCount >= testList.Count) _TestMemberCount = 0;
			}
		}
	}

	void DoMyWindow(int windowID) {

	}

	public IEnumerator SetUnitBody(short memberId, short sortNum) {
		yield return new WaitForEndOfFrame();

		if(TestObj != null) TestObj.GetComponent<SpriteRenderer>().enabled = false;

		yield return StartCoroutine(GetUnitBody(memberId, sortNum));

		if(isTest) {
			/*
			//print("CaptureBodyTexture.height : " + CaptureBodyTexture.height);

			Sprite TestSprite = Sprite.Create(CaptureBodyTexture, new Rect(0, 0, CaptureBodyTexture.width, CaptureBodyTexture.height), new Vector2(0.5f, 0.5f));

			TestObj.GetComponent<SpriteRenderer>().enabled = true;
			TestObj.GetComponent<SpriteRenderer>().sprite = TestSprite;*/
		}
	}
}
