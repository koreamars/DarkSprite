using UnityEngine;
using System.Collections;

public class DarkGearPopup : MonoBehaviour {

	public bool isTest;

	private GameObject CloseBtn;
	private GameObject GearListField;

	private ScriptData _ScriptData;

	private int _CoreCount;

	private short _CurrentMemberId;
	private int _SelectGearIndex;
	private ArrayList _GearObjList;
	private ArrayList _GearSlotNList;
	private ArrayList _CoreObjList;

	private GameObject _DarkGearPopup;

	void Start() {
		if(isTest) {
			LocalData.getInstence().AllLoad();
			MemberData.getInstence();
			ArrayList testMemberList = new ArrayList();
			Unit unit = UserData.getInstence().GetUnitById(1);
			foreach(short memberId in unit.memberList) {
				if(memberId > 0) testMemberList.Add(memberId);
			}
			init(testMemberList, 2);
		}
	}

	public void init(ArrayList memberList, int coreCount) {

		_CoreCount = coreCount;

		_ScriptData = ScriptData.getInstence();

		ShowCore();
		ShowMembers(memberList);

		CloseBtn = this.gameObject.transform.FindChild("CloseBtn").gameObject;
		CloseBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(160158).script, 50, Color.white);
		CloseBtn.GetComponent<CommonBtn>().SetClick(OnCloseBtnClick);

		GameObject titleTxtObj = this.gameObject.transform.FindChild("TitleTxt").gameObject;
		titleTxtObj.GetComponent<TextMesh>().text = ScriptData.getInstence().GetGameScript(130601).script;

		if(UserData.getInstence().StoryStepId == 27) CloseBtn.GetComponent<CommonBtn>().SetEnabled(false);

		MissionData.getInstence().AddMissionGoal(MissionGoalType.CoreMerging_Appear, 1);
	}

	private void ShowCore() {
		_CoreObjList = new ArrayList();
		GameObject coreObj;
		float posX = ((_CoreCount - 1) * 1f) / 2f * -1f;
		float delay = 0f;
		for(byte i = 0; i < _CoreCount; i ++) {
			coreObj = new GameObject();
			coreObj.AddComponent<SpriteRenderer>();
			coreObj.transform.position = new Vector2(posX,0.29f);
			SpriteRenderer renderer = coreObj.GetComponent<SpriteRenderer>();
			coreObj.layer = LayerMask.NameToLayer("Alert");
			renderer.sprite = Resources.Load<Sprite>("Common/CoreSymbol");

			iTween.ColorFrom(coreObj, iTween.Hash("r",10,"g",10,"b",10,"a", 0,"delay", delay ,"time", 0.6f));

			_CoreObjList.Add(coreObj);
			
			posX += 1f;
			delay += 0.5f;
		}
	}

	private void ShowMembers(ArrayList memberList) {
		GameObject memberObj;
		GameObject memberName;

		Member member;
		DefaultMember defaultMember;
		float posX = ((memberList.Count - 1) * 3f) / 2f * -1f;
		foreach(short memberId in memberList) {
			if(memberId <= 0) continue;
			member = UserData.getInstence().GetMemberById(memberId);
			defaultMember = MemberData.getInstence().GetDefaultMemberByID(member.DefaultId);

			memberObj = SetAddSpriteRenderer("MemberImg/Member" + defaultMember.thumbId, Color.white, 100);
			memberObj.transform.position = new Vector2(posX, 2.56f);
			memberObj.transform.localScale = new Vector2(0.75f, 0.75f);
			memberObj.transform.parent = this.transform;

			// 맴버 이름.
			memberName = SetAddTextMesh(_ScriptData.GetMemberNameByMemberId(member.id), 13, TextAnchor.MiddleCenter, Color.white);
			memberName.transform.position = new Vector2(memberObj.transform.position.x - 0f, 1.67f);
			memberName.transform.parent = this.transform;

			if(CheckUpNextGear(member.id) == true) {
				GameObject getDarkBtn = Instantiate(Resources.Load<GameObject>("Common/CommonBtn01")) as GameObject;
				getDarkBtn.GetComponent<BoxCollider>().size = new Vector2(4f, 2f);
				getDarkBtn.GetComponent<CommonBtn>().Init(member.id, _ScriptData.GetGameScript(160139).script, 50, Color.white);
				getDarkBtn.GetComponent<CommonBtn>().SetSortLayer("Alert");
				getDarkBtn.GetComponent<CommonBtn>().SetClick(ShowGearList);
				getDarkBtn.transform.localScale = new Vector2(0.6f, 0.6f);
				getDarkBtn.transform.position = new Vector2(memberObj.transform.position.x, 1.13f);
				getDarkBtn.transform.parent = this.transform;
			}

			posX += 3f;
		}
	}

	private void ShowGearList(int memberId) {

		_CurrentMemberId = (short)(memberId);
		_SelectGearIndex = 0;

		if(GearListField != null) Destroy(GearListField);
		GearListField = new GameObject();

		Member member = UserData.getInstence().GetMemberById((short)(memberId));
		ArrayList gearIdList = new ArrayList();
		_GearSlotNList = new ArrayList();

		if(CheckGearIsDark(member.SuitGearId) == true) {
			gearIdList.Add(member.SuitGearId);
			_GearSlotNList.Add(1);
		}
		if(CheckGearIsDark(member.BodyGearId) == true) {
			gearIdList.Add(member.BodyGearId);
			_GearSlotNList.Add(2);
		}
		if(CheckGearIsDark(member.EngineGearId) == true) {
			gearIdList.Add(member.EngineGearId);
			_GearSlotNList.Add(3);
		}
		if(CheckGearIsDark(member.Weapon1GearId) == true) {
			gearIdList.Add(member.Weapon1GearId);
			_GearSlotNList.Add(4);
		}
		if(CheckGearIsDark(member.Weapon2GearId) == true) {
			gearIdList.Add(member.Weapon2GearId);
			_GearSlotNList.Add(5);
		}
		if(CheckGearIsDark(member.Weapon3GearId) == true) {
			gearIdList.Add(member.Weapon3GearId);
			_GearSlotNList.Add(6);
		}

		if(_GearSlotNList.Count == 0) {
			GameObject gearNonName = SetAddTextMesh(_ScriptData.GetGameScript(160140).script, 23, TextAnchor.MiddleCenter, Color.white);
			gearNonName.transform.position = new Vector2(0f, -1.28f);
			gearNonName.transform.parent = GearListField.transform;
			return;
		}

		GameObject gearObj;
		GameObject gearName;

		Gear currentGear;
		_GearObjList = new ArrayList();

		float posX = ((gearIdList.Count - 1) * 3f) / 2f * -1f;
		byte index = 0;
		foreach(short gearId in gearIdList) {
			currentGear = GearData.getInstence().GetGearByID(gearId);

			// 기어 썸네일
			gearObj = SetAddSpriteRenderer("UnitResource/Thumbnail/" + currentGear.thumbnailURI, Color.white, 100);
			gearObj.transform.position = new Vector2(posX, -1.09f);
			gearObj.transform.localScale = new Vector2(0.75f, 0.75f);
			gearObj.transform.parent = GearListField.transform;
			_GearObjList.Add(gearObj);

			// 기어 이름.
			gearName = SetAddTextMesh(_ScriptData.GetGameScript(currentGear.scriptId).script, 13, TextAnchor.MiddleCenter, Color.white);
			gearName.transform.position = new Vector2(gearObj.transform.position.x - 0f, -2.02f);
			gearName.transform.parent = GearListField.transform;

			// 머징 버튼.
			GameObject getDarkBtn = Instantiate(Resources.Load<GameObject>("Common/CommonBtn01")) as GameObject;
			getDarkBtn.GetComponent<BoxCollider>().size = new Vector2(4f, 2f);
			getDarkBtn.GetComponent<CommonBtn>().Init((int)(index), _ScriptData.GetGameScript(160159).script, 50, Color.white);
			getDarkBtn.GetComponent<CommonBtn>().SetSortLayer("Alert");
			getDarkBtn.GetComponent<CommonBtn>().SetClick(SetCoreMerging);
			getDarkBtn.transform.localScale = new Vector2(0.6f, 0.6f);
			getDarkBtn.transform.position = new Vector2(gearObj.transform.position.x, -2.54f);
			getDarkBtn.transform.parent = GearListField.transform;
			index ++;
			posX += 3f;
		}
	}

	/** 착용중인 장비 중 다크기어화 할수 있는 장비가 있는지 확인 함 */
	private bool CheckUpNextGear(short memberId) {
		Member member = UserData.getInstence().GetMemberById(memberId);
		// 슈트 체크.
		if(CheckGearIsDark(member.SuitGearId) == true) return true;

		// 기체 체크.
		if(CheckGearIsDark(member.BodyGearId) == true) return true;

		// 엔진 체크.
		if(CheckGearIsDark(member.EngineGearId) == true) return true;

		// 기본 무기 체크.
		if(CheckGearIsDark(member.Weapon1GearId) == true) return true;

		// 추가 무기 1 체크.
		if(CheckGearIsDark(member.Weapon2GearId) == true) return true;

		// 추가 무기 2 체크.
		if(CheckGearIsDark(member.Weapon3GearId) == true) return true;

		return false;
	}

	/** 해당 기어가 다크기어 업이 가능한 기어 인지 확인함. */
	private bool CheckGearIsDark(short gearId) {
		Gear gear = GearData.getInstence().GetGearByID(gearId);
		if(gear != null && gear.upNextId > 0) return true;
		return false;
	}

	private GameObject SetAddSpriteRenderer(string uri, Color color, int sortNum) {
		GameObject targetobj = new GameObject();
		targetobj.layer = LayerMask.NameToLayer("Alert");
		targetobj.AddComponent<SpriteRenderer>();
		SpriteRenderer renderer = targetobj.GetComponent<SpriteRenderer>();
		renderer.sortingOrder = sortNum;
		renderer.color = color;
		renderer.sprite = Resources.Load<Sprite>(uri);
		
		return targetobj;
	}

	private GameObject SetAddTextMesh(string str, int fontSize, TextAnchor anchor, Color color) {
		GameObject targetobj = Instantiate(Resources.Load<GameObject>("DefaultFont")) as GameObject;
		targetobj.layer = LayerMask.NameToLayer("Alert");
		TextMesh textMesh = targetobj.GetComponent<TextMesh>();
		textMesh.text = str;
		textMesh.anchor = anchor;
		textMesh.color = color;
		textMesh.fontSize = fontSize;
		targetobj.GetComponent<Renderer>().sortingOrder = 100;
		
		return targetobj;
	}

	private void SetCoreMerging(int indexId) {
		_SelectGearIndex = indexId;

		Member member = UserData.getInstence().GetMemberById(_CurrentMemberId);
		int slotNum = (int)(_GearSlotNList[_SelectGearIndex]);
		short gearId = 0;
		bool isChecking = false;
		switch(slotNum) {
		case 1:	// 슈트.
			if(CheckGearIsDark(member.SuitGearId) == true) isChecking = true;
			gearId = member.SuitGearId;
			break;
		case 2:	// 기체.
			if(CheckGearIsDark(member.BodyGearId) == true) isChecking = true;
			gearId = member.BodyGearId;
			break;
		case 3:	// 엔진.
			if(CheckGearIsDark(member.EngineGearId) == true) isChecking = true;
			gearId = member.EngineGearId;
			break;
		case 4:	// 기본 무장.
			if(CheckGearIsDark(member.Weapon1GearId) == true) isChecking = true;
			gearId = member.Weapon1GearId;
			break;
		case 5:	// 추가 무장 1.
			if(CheckGearIsDark(member.Weapon2GearId) == true) isChecking = true;
			gearId = member.Weapon2GearId;
			break;
		case 6:	// 추가 무장 2.
			if(CheckGearIsDark(member.Weapon3GearId) == true) isChecking = true;
			gearId = member.Weapon3GearId;
			break;
		}

		BillModel billmodel = new BillModel();
		if(isChecking && _CoreCount > 0) {
			billmodel.core = 1;
			//UserData.getInstence().SetConfirmPop(_ScriptData.GetGameScript(150132).script, OnConfirmClick, billmodel);

			_DarkGearPopup = Instantiate(Resources.Load<GameObject>("Common/DarkGearPopup")) as GameObject;
			_DarkGearPopup.GetComponent<DarkGearPop>().init(gearId, 200, DarkGearPop.FightType);
			_DarkGearPopup.GetComponent<DarkGearPop>().SetCloseCallback(OnConfirmClick);

		} else {
			if(_CoreCount > 0) {
				UserData.getInstence().SetAlert(_ScriptData.GetGameScript(150133).script, billmodel);
			} else {
				UserData.getInstence().SetAlert(_ScriptData.GetGameScript(150134).script, billmodel);
			}

		}

	}

	private void OnConfirmClick(bool isConfirm, bool isSuccess) {
		print("OnConfirmClick : " + isConfirm + "/" + isSuccess);
		if(isConfirm) {	// 코어머징을 시도함.
			Member member = UserData.getInstence().GetMemberById(_CurrentMemberId);

			CloseBtn.GetComponent<CommonBtn>().SetEnabled(true);

			_CoreCount -= 1;
			GameObject coreObj = _CoreObjList[_CoreCount] as GameObject;
			iTween.ColorTo(coreObj, iTween.Hash("a", 0f, "easetype", iTween.EaseType.easeInBack));
			iTween.ScaleTo(coreObj, iTween.Hash("x", 2f, "y", 2f, "easetype", iTween.EaseType.easeInBack));

			int slotNum = (int)(_GearSlotNList[_SelectGearIndex]);
			Gear gear = null;
			if(isSuccess) {	// 코어머징 성공시.
				switch(slotNum) {
				case 1:	// 슈트.
					gear = GearData.getInstence().GetGearByID(member.SuitGearId);
					member.SuitGearId = gear.upNextId;
					break;
				case 2:	// 기체.
					gear = GearData.getInstence().GetGearByID(member.BodyGearId);
					member.BodyGearId = gear.upNextId;
					break;
				case 3:	// 엔진.
					gear = GearData.getInstence().GetGearByID(member.EngineGearId);
					member.EngineGearId = gear.upNextId;
					break;
				case 4:	// 기본 무장.
					gear = GearData.getInstence().GetGearByID(member.Weapon1GearId);
					member.Weapon1GearId = gear.upNextId;
					break;
				case 5:	// 추가 무장 1.
					gear = GearData.getInstence().GetGearByID(member.Weapon2GearId);
					member.Weapon2GearId = gear.upNextId;
					break;
				case 6:	// 추가 무장 2.
					gear = GearData.getInstence().GetGearByID(member.Weapon3GearId);
					member.Weapon3GearId = gear.upNextId;
					break;
				}
			} else {
				switch(slotNum) {
				case 1:	// 슈트.
					member.SuitGearId = 1;
					break;
				case 2:	// 기체.
					member.BodyGearId = 2;
					break;
				case 3:	// 엔진.
					member.EngineGearId = 3;
					break;
				case 4:	// 기본 무장.
					member.Weapon1GearId = 4;
					break;
				case 5:	// 추가 무장 1.
					member.Weapon2GearId = 0;
					break;
				case 6:	// 추가 무장 2.
					member.Weapon3GearId = 0;
					break;
				}
			}

			ShowGearList(_CurrentMemberId);
			LocalData.getInstence().UserMemberDataSave();
		}
	}

	private void OnCloseBtnClick(int btnId) {
		Application.LoadLevel("DarkSpriteMain");
	}
}
