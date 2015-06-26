using UnityEngine;
using System.Collections;

public class EndPopup : MonoBehaviour {

	public bool isTest;

	public GameObject GameResultTitle;
	public GameObject GameRewrdTitle;
	public GameObject MemberDataTitle;

	private GameObject CloseBtn;
	private GameObject DarkGearBtn;

	private ScriptData _ScriptData;
	private short _SelectMemberId;
	private ArrayList _MemberIdList;
	private int _GetCoreCount;

	private int _UnitExp = 0;

	void Start() {
		if(isTest) {
			LocalData.getInstence().AllLoad();

			byte testCount = 5;
			UnitDataBoxModel[] testList = new UnitDataBoxModel[testCount];
			for(byte i = 0; i < testCount; i++) {
				testList[i] = new UnitDataBoxModel();
				testList[i].id = (byte)(i + 1);
				testList[i].modelId = 1;
				testList[i].currentHP = 1000;
				testList[i].currentMP = 50;
				testList[i].maxHP = 100;
				testList[i].maxMP = 1000;
				testList[i].ActNum = 22;
				testList[i].type = 0;
			}

			init(testList, 100, 2, 0);

			this.gameObject.transform.position = new Vector2(0f, 0f);
			this.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

			iTween.ScaleFrom(this.gameObject, iTween.Hash("scale", new Vector3(2f, 2f, 2f), "time", 0.5f, "easetype", iTween.EaseType.easeOutCirc));
			iTween.ColorFrom(this.gameObject, iTween.Hash("a", 0, "time", 0.5f));
		}
	}

	public void init(UnitDataBoxModel[] ResultDataList, short rewardPayCount, byte rewardCoreCount, byte rewardChipCount) {

		_ScriptData = ScriptData.getInstence();

		bool isWin = false;
		byte ghostCount = 0;
		ArrayList memberids = new ArrayList();
		foreach(UnitDataBoxModel boxModel in ResultDataList) {
			if(boxModel.type == 0) {
				if(boxModel.currentHP > 0) isWin = true;
				memberids.Add(boxModel.modelId);
			} else {
				Ghost ghost = WarzoneData.getInstence().GetGhostByGhostId(boxModel.modelId);
				DefaultGhost defaultGhost = WarzoneData.getInstence().GetDefaultGhostByGhostId(ghost.defaultId);
				ghostCount += (byte)(defaultGhost.closeValue / 100);
			}
		}

		_MemberIdList = memberids;
		_GetCoreCount = (int)(rewardCoreCount);

		_UnitExp = ghostCount * SystemData.GetInstance().FightUnitExp / memberids.Count;
		if(SystemData.GetInstance().IsMemberXpPlus == true) {
			_UnitExp += 1000;
		}

		string googleMsg = "";
		if(isWin) {
			googleMsg = "Win";
			GameResultTitle.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(120600).script;
		} else {
			googleMsg = "Lost";
			GameResultTitle.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(120601).script;
		}

		if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogScreen("Fight End - " + googleMsg);

		GameRewrdTitle.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(130600).script;
		MemberDataTitle.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(130601).script;

		GameObject coreObj;
		byte coreCount = rewardCoreCount;
		byte chipCount = rewardChipCount;
		float delay = 1;
		byte count = (byte)(rewardCoreCount + rewardChipCount);
		float posX = ((count - 1) * 1f) / 2f * -1f;
		for(byte i = 0; i < (rewardCoreCount + rewardChipCount); i ++) {
			coreObj = new GameObject();
			coreObj.AddComponent<SpriteRenderer>();
			coreObj.transform.position = new Vector2(posX,2.08f);
			SpriteRenderer renderer = coreObj.GetComponent<SpriteRenderer>();
			coreObj.layer = LayerMask.NameToLayer("Alert");
			coreObj.transform.parent = this.gameObject.transform;
			renderer.sortingOrder = 100;
			if(coreCount > 0) {	// 코어.
				renderer.sprite = Resources.Load<Sprite>("Common/CoreSymbol");
				coreCount --;
			} else {		// 파편.
				renderer.sprite = Resources.Load<Sprite>("Common/CoreChipSymbol");
				chipCount --;
			}

			iTween.ColorFrom(coreObj, iTween.Hash("r",10,"g",10,"b",10,"a", 0,"delay", delay ,"time", 0.6f));

			posX += 1f;
			delay += 0.5f;

		}

		if(rewardPayCount > 0) {
			GameObject rewardPayObj = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
			rewardPayObj.GetComponent<OutLineFont>().SetString("+ " + rewardPayCount.ToString());
			rewardPayObj.GetComponent<OutLineFont>().SetSortLayer("Alert");
			rewardPayObj.GetComponent<OutLineFont>().SetSort(100);
			rewardPayObj.GetComponent<OutLineFont>().SetFontSize(24);
			rewardPayObj.GetComponent<OutLineFont>().SetAlign(TextAnchor.MiddleLeft);
			Color fontColor = Color.white;
			fontColor.r = 1;
			fontColor.g = 0.5f;
			fontColor.b = 0;
			rewardPayObj.GetComponent<OutLineFont>().SetFontColor(fontColor);
			rewardPayObj.GetComponent<OutLineFont>().SetLineColor(Color.white);
			rewardPayObj.transform.position = new Vector2(-0.32f + this.gameObject.transform.position.x,1.28f);
			rewardPayObj.transform.parent = this.gameObject.transform;
			iTween.ColorFrom(rewardPayObj, iTween.Hash("r",10,"g",10,"b",10,"a", 0,"delay", delay ,"time", 0.6f));

			GameObject rewardPayImg = new GameObject();
			SpriteRenderer payImgRenderer = rewardPayImg.AddComponent<SpriteRenderer>();
			payImgRenderer.sprite = Resources.Load<Sprite>("Common/PaySymbol");
			payImgRenderer.transform.position = new Vector3(-0.93f + this.gameObject.transform.position.x,1.31f,0f);
			payImgRenderer.transform.parent = this.gameObject.transform;
			rewardPayImg.layer = LayerMask.NameToLayer("Alert");
			iTween.ColorFrom(rewardPayImg, iTween.Hash("r",10,"g",10,"b",10,"a", 0,"delay", delay ,"time", 0.6f));
		}

		ShowMembers(memberids, rewardCoreCount);

		CloseBtn = this.gameObject.transform.FindChild("EndPopupCloseBtn").gameObject;
		CloseBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(160158).script, 50, Color.white);
		CloseBtn.GetComponent<CommonBtn>().SetClick(OnCloseBtnClick);
		DarkGearBtn = this.gameObject.transform.FindChild("DarkGearPopupBtn").gameObject;

		// 코어머징 버튼 노출.
		bool isUpGear = false;
		foreach(short memberId in memberids) {
			if(CheckUpNextGear(memberId) == true) isUpGear = true;
		}

		if(UserData.getInstence().StoryStepId != 27) {
			if(rewardCoreCount > 0 && isUpGear && UserData.getInstence().StoryStepId > 25) {
				DarkGearBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(160159).script, 50, Color.white);
				DarkGearBtn.GetComponent<CommonBtn>().SetClick(OnDarkGearBtnClick);
			} else {
				CloseBtn.transform.position = new Vector2(0f, CloseBtn.transform.position.y);
				DarkGearBtn.GetComponent<CommonBtn>().SetEnabled(false);
			}
		} else {	// 튜토리얼용.
			CloseBtn.GetComponent<CommonBtn>().SetEnabled(false);
			DarkGearBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(160159).script, 50, Color.white);
			DarkGearBtn.GetComponent<CommonBtn>().SetClick(OnDarkGearBtnClick);
		}

		UserData.getInstence().UserCores += rewardCoreCount;
		UserData.getInstence().UserChips += rewardChipCount;
		UserData.getInstence().UserMoney += rewardPayCount;

		LocalData.getInstence().UserResourceSave();
		LocalData.getInstence().UserMemberDataSave();

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

	private void ShowMembers(ArrayList memberList, int coreCount) {

		GameObject memberObj;
		GameObject classObj;
		GameObject memberName;
		GameObject expField;
		GameObject expTitle;
		GameObject expTotal;
		GameObject stateMark;

		Member member;
		DefaultMember defaultMember;
		byte stateType = 0;	// 1:진급, 2:부상, 3:MIA
		float posX = ((memberList.Count - 1) * 3f) / 2f * -1f;
		foreach(short memberId in memberList) {
			stateType = 0;
			member = UserData.getInstence().GetMemberById(memberId);
			defaultMember = MemberData.getInstence().GetDefaultMemberByID(member.DefaultId);

			ClassModel nextClass = MemberData.getInstence().GetNextLClassModel(member.ClassId);
			ClassModel currentClass = MemberData.getInstence().GetClassModelByClassId(member.ClassId);

			// 경험치 갱신.
			UserData.getInstence().UpdateMemberExp(member, _UnitExp);

			if(member.Exp >= currentClass.exp) {
				stateType = 1;
			}

			// 맴버 이미지 노출.
			if(member.CurrentHP == 0 && member.DefaultId > 1) {	// 부상.
				if(UserData.getInstence().UserMemberList.Count > 5) { 
					stateType = 2;
					member.state = MemberStateType.Wound;
					memberObj = SetAddSpriteRenderer("MemberImg/Member" + defaultMember.thumbId, Color.grey, 100);
				} else {
					member.CurrentHP = 1;
					memberObj = SetAddSpriteRenderer("MemberImg/Member" + defaultMember.thumbId, Color.white, 100);
				}
			} else {
				memberObj = SetAddSpriteRenderer("MemberImg/Member" + defaultMember.thumbId, Color.white, 100);
			}

			memberObj.transform.position = new Vector2(posX, -0.78f);
			memberObj.transform.localScale = new Vector2(0.75f, 0.75f);
			memberObj.transform.parent = this.gameObject.transform;
			// 계급장 표시.
			ClassModel classModel = MemberData.getInstence().GetClassModelByClassId(member.ClassId);
			classObj = SetAddSpriteRenderer("ClassMark/ClassMark" + classModel.Markid, Color.white, 100);
			classObj.transform.position = new Vector2(memberObj.transform.position.x - 0.44f, -1.20f);
			classObj.transform.localScale = new Vector2(0.7f, 0.7f);
			classObj.transform.parent = this.gameObject.transform;
			classObj.GetComponent<Renderer>().sortingOrder = 101;

			// 맴버 이름.
			memberName = SetAddTextMesh(_ScriptData.GetMemberNameByMemberId(member.id), 13, TextAnchor.MiddleCenter, Color.white);
			memberName.transform.position = new Vector2(memberObj.transform.position.x - 0f, -1.9f);
			memberName.transform.parent = this.gameObject.transform;
			// 경험치 타이틀.
			Color expColor = Color.white;
			expColor.r = 0.1f;
			expColor.g = 1.0f;
			expColor.b = 0.3f;
			string exp = "+" + _UnitExp + "Exp";
			expField = SetAddTextMesh(exp, 13, TextAnchor.MiddleCenter, expColor);
			expField.name = "expField";
			expField.transform.position = new Vector2(memberObj.transform.position.x - 0f, -2.60f);
			expField.transform.parent = this.gameObject.transform;

			string exptitle = _ScriptData.GetGameScript(160141).script;
			expTitle = SetAddTextMesh(exptitle, 13, TextAnchor.MiddleCenter, expColor);
			expTitle.name = "expTitle";
			expTitle.transform.position = new Vector2(memberObj.transform.position.x - 0f, -2.30f);
			expTitle.transform.parent = this.gameObject.transform;

			Color totalExpColor = Color.white;
			totalExpColor.r = 0.7f;
			totalExpColor.g = 1f;
			totalExpColor.b = 0.3f;
			string expTotalStr = "Exp " + member.Exp + "/" + currentClass.exp;
			if(member.ClassId >= SystemData.GetInstance().MemberMaxClass) {
				expTotalStr = "Exp MAX";
			}
			expTotal = SetAddTextMesh(expTotalStr, 13, TextAnchor.MiddleCenter, totalExpColor);
			expTotal.name = "totalExp";
			expTotal.transform.position = new Vector2(memberObj.transform.position.x - 0f, -3.00f);
			expTotal.transform.parent = this.gameObject.transform;

			// 상태 표시.
			stateMark = null;
			if(stateType == 1) {	// 진급
				stateMark = SetAddSpriteRenderer("Common/EndGameSymbol01", Color.white, 101);
			} else if (stateType == 2) {	// 부상.
				stateMark = SetAddSpriteRenderer("Common/EndGameSymbol02", Color.white, 101);
			} else if (stateType == 3) {	// 실종.
				stateMark = SetAddSpriteRenderer("Common/EndGameSymbol03", Color.white, 101);
			} 
			if(stateMark != null) {
				stateMark.name = "stateMark";
				stateMark.transform.position = new Vector2(memberObj.transform.position.x - 0.2f, -0.58f);
				stateMark.transform.localScale = new Vector2(1.3f, 1.3f);
				stateMark.transform.parent = this.gameObject.transform;
			}

			posX += 3f;

		}

	}

	private short GetDarkGearId(short memberId) {
		ArrayList darkGearList = new ArrayList();
		Member member = UserData.getInstence().GetMemberById(memberId);

		short gearId = CheckDarkGearId(member.SuitGearId);
		if(gearId > 0) darkGearList.Add(gearId);

		gearId = CheckDarkGearId(member.BodyGearId);
		if(gearId > 0) darkGearList.Add(gearId);

		gearId = CheckDarkGearId(member.EngineGearId);
		if(gearId > 0) darkGearList.Add(gearId);

		gearId = CheckDarkGearId(member.Weapon1GearId);
		if(gearId > 0) darkGearList.Add(gearId);

		gearId = CheckDarkGearId(member.Weapon2GearId);
		if(gearId > 0) darkGearList.Add(gearId);

		gearId = CheckDarkGearId(member.Weapon3GearId);
		if(gearId > 0) darkGearList.Add(gearId);

		if(darkGearList.Count == 0) return 0;

		int randomIndex = UnityEngine.Random.Range(0, darkGearList.Count);
		short returnGearId = (short)(darkGearList[randomIndex]);

		return returnGearId;

	}

	private short CheckDarkGearId(short gearId) {
		if(gearId == 0) return 0;
		Gear gear = GearData.getInstence().GetGearByID(gearId);
		return gear.upNextId;
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


	private void OnConfirmClick(bool isConfirm) {
		if(isConfirm) {

		}
	}

	private void OnCloseBtnClick(int btnId) {
		Application.LoadLevel("DarkSpriteMain");
	}

	private void OnDarkGearBtnClick(int btnId) {
		GameObject darkGearPopup = Instantiate(Resources.Load<GameObject>("BattleScene/DarkGearPopup")) as GameObject;
		darkGearPopup.name = "DarkGearPopup";
		darkGearPopup.GetComponent<DarkGearPopup>().init(_MemberIdList, _GetCoreCount);
		Destroy(this.gameObject);
	}
}
