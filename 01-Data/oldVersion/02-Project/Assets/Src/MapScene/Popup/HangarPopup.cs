using UnityEngine;
using System.Collections;

public class HangarPopup : MonoBehaviour {

	public bool isTest;
	public GameObject CloseBtn;
	public GameObject SubTitleTxt;
	public GameObject DetailView;
	public GameObject MemberScrollMenu;
	public GameObject GearScrollMenu;

	public GameObject GearMenu1;
	public GameObject GearMenu2;
	public GameObject GearMenu3;
	public GameObject GearMenu4;
	public GameObject GearMenu5;
	public GameObject GearMenu6;

	public GameObject ViewTypeBtn;
	public GameObject GearViewBtn;

	private UserData _UserData;
	private ScriptData _ScriptData;
	private MemberData _MemberData;
	private GearData _GearData;

	private ScrollMenberMenu _MemberScrollMenu;
	private ScrollMenu _GearScrollMenu;
	private GameObject _DefaultCraft;
	private UnitFrame _UnitFrame;
	private bool _isUnitFrameSet;
	private GameObject _MemberDataUIObj;
	private MemberDataUI _MemberDataUI;

	private GameObject _SuitGearDataUIObj;
	private GameObject _BodyGearDataUIObj;
	private GameObject _EngineGearDataUIObj;
	private GameObject _Weapon1GearDataUIObj;
	private GameObject _Weapon2GearDataUIObj;
	private GameObject _Weapon3GearDataUIObj;

	private GameObject _UnitNameTxt;
	private GameObject _MemberCommentObj;

	private short _SelectMemberId = 0;
	private byte _SelectGearType = 0;
		
	public delegate void CloseEvent();
	private CloseEvent _CloseCallback;

	void Start() {
		
		CloseBtn.GetComponent<ButtonEvent>().SetCallBack(OnPopupClose);
		SubTitleTxt.GetComponent<Renderer>().sortingOrder = 2;

		if(isTest) {
			StartCoroutine(TestInit());
		}
	}

	private IEnumerator TestInit() {
		yield return new WaitForEndOfFrame();
		yield return StartCoroutine(MissionData.getInstence().init());
		yield return StartCoroutine(StoryData.getInstence().init());
		LocalData.getInstence().AllLoad();
		DarkSprite.getInstence();
		SystemData.GetInstance();
		MemberData.getInstence();
		UserData.getInstence();
		yield return StartCoroutine(init());
		SetPopupStart();
	}
	
	public void SetCloseEventCallBack(CloseEvent onCloseEvent) {
		_CloseCallback = new CloseEvent(onCloseEvent);
		
	}
	
	
	private void OnPopupClose() {
		if(_CloseCallback != null) _CloseCallback();
	}

	public IEnumerator init() {

		yield return new WaitForEndOfFrame();

		_UserData = UserData.getInstence();
		_ScriptData = ScriptData.getInstence();
		_MemberData = MemberData.getInstence();
		_GearData = GearData.getInstence();

		_DefaultCraft = Instantiate(Resources.Load<GameObject>("UnitResource/UnitFrame")) as GameObject;
		_DefaultCraft.transform.parent = this.gameObject.transform;
		_DefaultCraft.transform.position = new Vector2(-2f, -10f);
		_UnitFrame = _DefaultCraft.GetComponent<UnitFrame>();
		_UnitFrame.SetBodyMemorySave(false);

		SubTitleTxt.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(110007).script;
		SubTitleTxt.GetComponent<TextMesh>().text += " <size=18>(" + _UserData.UserMemberList.Count + "/";
		SubTitleTxt.GetComponent<TextMesh>().text += _UserData.GetBaseSoildersByLevel(_UserData.BaseLevel) + ")</size>";

		_MemberScrollMenu = MemberScrollMenu.GetComponent<ScrollMenberMenu>();
		_MemberScrollMenu.sortingNum = 5;
		_MemberScrollMenu.SetMenuClick(OnMemberlistClick);
		_GearScrollMenu = GearScrollMenu.GetComponent<ScrollMenu>();
		_GearScrollMenu.SetMenuClick(OnGearListClick);

		string btnName1 = _ScriptData.GetGameScript(120700).script;
		GearMenu1.GetComponent<CommonBtn>().Init(GearSlotType.Suit, btnName1, 25, Color.white);
		GearMenu1.GetComponent<CommonBtn>().SetClick(OnGearMenuClick);
		string btnName2 = _ScriptData.GetGameScript(120701).script;
		GearMenu2.GetComponent<CommonBtn>().Init(GearSlotType.Body, btnName2, 25, Color.white);
		GearMenu2.GetComponent<CommonBtn>().SetClick(OnGearMenuClick);
		string btnName3 = _ScriptData.GetGameScript(120702).script;
		GearMenu3.GetComponent<CommonBtn>().Init(GearSlotType.Engine, btnName3, 25, Color.white);
		GearMenu3.GetComponent<CommonBtn>().SetClick(OnGearMenuClick);
		string btnName4 = _ScriptData.GetGameScript(120703).script;
		GearMenu4.GetComponent<CommonBtn>().Init(GearSlotType.Weapon_Default, btnName4, 25, Color.white);
		GearMenu4.GetComponent<CommonBtn>().SetClick(OnGearMenuClick);
		string btnName5 = _ScriptData.GetGameScript(120704).script;
		GearMenu5.GetComponent<CommonBtn>().Init(GearSlotType.Weapon_Slot1, btnName5, 25, Color.white);
		GearMenu5.GetComponent<CommonBtn>().SetClick(OnGearMenuClick);
		string btnName6 = _ScriptData.GetGameScript(120705).script;
		GearMenu6.GetComponent<CommonBtn>().Init(GearSlotType.Weapon_Slot2, btnName6, 25, Color.white);
		GearMenu6.GetComponent<CommonBtn>().SetClick(OnGearMenuClick);
		string btnName7 = _ScriptData.GetGameScript(120707).script;
		ViewTypeBtn.GetComponent<CommonBtn>().Init(7, btnName7, 15, Color.white);
		ViewTypeBtn.GetComponent<CommonBtn>().SetClick(OnDetailViewClick);
		string btnName8 = _ScriptData.GetGameScript(130506).script;
		GearViewBtn.GetComponent<CommonBtn>().Init(7, btnName8, 15, Color.white);
		GearViewBtn.GetComponent<CommonBtn>().SetClick(OnShowUnitGearView);

		OnViewChange(0);

		Member member = _UserData.UserMemberList[0] as Member;

		_MemberDataUIObj = Instantiate(Resources.Load<GameObject>("Common/MemberDataUI")) as GameObject;
		_MemberDataUIObj.transform.parent = DetailView.transform;
		_MemberDataUIObj.transform.localScale = new Vector2(0.70f, 0.70f);
		_MemberDataUIObj.transform.position = new Vector2(DetailView.transform.position.x + 2.12f, DetailView.transform.position.y + 3.25f);
		_MemberDataUI = _MemberDataUIObj.GetComponent<MemberDataUI>();
		_MemberDataUI.init(0);
		_MemberDataUI.MemberUpdate(member.id, false);
		_SelectMemberId = member.id;

		float uiStartPocY = 3.8f + this.gameObject.transform.position.y;
		float uiGapY = 1.4f;
		_SuitGearDataUIObj = GetGearDataUI(uiStartPocY, GearMenu1);
		uiStartPocY -= uiGapY;
		_BodyGearDataUIObj = GetGearDataUI(uiStartPocY, GearMenu2);
		uiStartPocY -= uiGapY;
		_EngineGearDataUIObj = GetGearDataUI(uiStartPocY, GearMenu3);
		uiStartPocY -= uiGapY;
		_Weapon1GearDataUIObj = GetGearDataUI(uiStartPocY, GearMenu4);
		uiStartPocY -= uiGapY;
		_Weapon2GearDataUIObj = GetGearDataUI(uiStartPocY, GearMenu5);
		uiStartPocY -= uiGapY;
		_Weapon3GearDataUIObj = GetGearDataUI(uiStartPocY, GearMenu6);

		UpdateGearDataUI(_SuitGearDataUIObj, 1);
		UpdateGearDataUI(_BodyGearDataUIObj, 2);
		UpdateGearDataUI(_EngineGearDataUIObj, 3);
		UpdateGearDataUI(_Weapon1GearDataUIObj, 4);
		UpdateGearDataUI(_Weapon2GearDataUIObj, 4);
		UpdateGearDataUI(_Weapon3GearDataUIObj, 4);

		yield return StartCoroutine(StoryData.getInstence().CheckStoryModel(50002));

		ShowUnitName(_SelectMemberId);
		ShowMemberDetailView(_SelectMemberId);
		ShowGearView(_SelectMemberId, false, false);
	}

	/** 기어뷰 생성 */
	private GameObject GetGearDataUI(float pocY, GameObject btnObj) {
		GameObject uiObj = Instantiate(Resources.Load<GameObject>("Common/GearDataUI")) as GameObject;
		uiObj.transform.parent = this.gameObject.transform;
		uiObj.transform.localScale = new Vector2(0.6f, 0.6f);
		uiObj.transform.position = new Vector2(this.gameObject.transform.position.x - 3.39f, pocY);
		uiObj.GetComponent<GearDataUI>().init();

		btnObj.transform.position = new Vector2(uiObj.transform.position.x - 2.6f, uiObj.transform.position.y + 0.2f);
		btnObj.transform.localScale = new Vector2(0.75f, 0.75f);
		return uiObj;
	}

	/** 기어뷰 데이터 갱신 */
	private void UpdateGearDataUI(GameObject gearUI, short gearId) {
		gearUI.GetComponent<GearDataUI>().GearUpdate(gearId);
	}

	public void SetPopupStart() {

		ShowMemberList();
	}

	private IEnumerator SetCraftField() {
		yield return new WaitForEndOfFrame();

		GameObject loadingMark = SystemData.GetInstance().GetLoadingMark();

		if(_isUnitFrameSet == true) {
			yield return StartCoroutine(_UnitFrame.UpdateMemberData(_SelectMemberId));
		} else {
			yield return StartCoroutine(_UnitFrame.init(_SelectMemberId, 5));
			_isUnitFrameSet = true;
		}
		_DefaultCraft.transform.position = new Vector2(-2.68f, 2.01f);

		Destroy(loadingMark);
	}

	private void ShowMemberList() {
		ArrayList MemberList = _UserData.UserMemberList;
		MemberList.Sort(new MemberClassSort());
		ListMemberModel[] listData = new ListMemberModel[MemberList.Count];

		int index = 0;
		foreach(Member member in MemberList) {
			listData[index] = new ListMemberModel();
			listData[index].id = (byte)(index + 1);
			listData[index].optionalId = member.id;
			listData[index].ClassId = member.ClassId;
			listData[index].scriptString = ScriptData.getInstence().GetMemberNameByMemberId(member.id);

			index ++;
		}

		_MemberScrollMenu.SetScrollData(listData, 0);
		_MemberScrollMenu.SetScrollView();

	}

	private void ShowGearView(short memberId, bool isMovie, bool isGearShow) {

		if(memberId == 0) return;
		_MemberDataUI.MemberUpdate(memberId, false);

		if(isGearShow == false) {
			Member member = _UserData.GetMemberById(memberId);

			UpdateGearDataUI(_SuitGearDataUIObj, member.SuitGearId);
			UpdateGearDataUI(_BodyGearDataUIObj, member.BodyGearId);
			UpdateGearDataUI(_EngineGearDataUIObj, member.EngineGearId);
			UpdateGearDataUI(_Weapon1GearDataUIObj, member.Weapon1GearId);
			UpdateGearDataUI(_Weapon2GearDataUIObj, member.Weapon2GearId);
			UpdateGearDataUI(_Weapon3GearDataUIObj, member.Weapon3GearId);
		} else {
			UpdateGearDataUI(_SuitGearDataUIObj, 0);
			UpdateGearDataUI(_BodyGearDataUIObj, 0);
			UpdateGearDataUI(_EngineGearDataUIObj, 0);
			UpdateGearDataUI(_Weapon1GearDataUIObj, 0);
			UpdateGearDataUI(_Weapon2GearDataUIObj, 0);
			UpdateGearDataUI(_Weapon3GearDataUIObj, 0);
		}

		if(isMovie == false) return;

		iTween.ColorTo(_MemberDataUIObj, iTween.Hash("r", 5, "g", 5, "b", 5, "time", 0.1f
		                                             , "oncomplete", "GearUpdateView", "oncompletetarget", this.gameObject));
	}

	private void GearUpdateView() {
		iTween.ColorTo(_MemberDataUIObj, iTween.Hash("r", 1, "g", 1, "b", 1, "time", 0.1f));
	}

	private void OnGearMenuClick(int id) {

		OnViewChange(2);

		_UserData.GetMakeGearList();

		ArrayList gearIdList = null;

		_SelectGearType = (byte)(id);

		Member member = _UserData.GetMemberById(_SelectMemberId);
		short ownGearId = 0;

		switch(id) {
		case GearSlotType.Suit:		// 슈트
			gearIdList = _UserData.GetOwnGearByGearType(GearType.Suit);
			ownGearId = member.SuitGearId;
			GuideArrowManager.getInstence().ShowArrow(SceneType.HangarPopup_Suit);
			break;
		case GearSlotType.Body:		// 기체.
			gearIdList = _UserData.GetOwnGearByGearType(GearType.Body);
			ownGearId = member.BodyGearId;
			GuideArrowManager.getInstence().ShowArrow(SceneType.HangarPopup_Airframe);
			break;
		case GearSlotType.Engine:		// 엔진.
			gearIdList = _UserData.GetOwnGearByGearType(GearType.Engine);
			ownGearId = member.EngineGearId;
			GuideArrowManager.getInstence().ShowArrow(SceneType.HangarPopup_Engine);
			break;
		case GearSlotType.Weapon_Default:		// 기본 무장.
			gearIdList = _UserData.GetOwnGearByGearType(GearType.Weapon_Gun);
			//gearIdList.AddRange(_UserData.GetOwnGearByGearType(GearType.Weapon_Rocket));
			//gearIdList.AddRange(_UserData.GetOwnGearByGearType(GearType.Weapon_Missle));
			ownGearId = member.Weapon1GearId;
			GuideArrowManager.getInstence().ShowArrow(SceneType.HangarPopup_Weapon1);
			break;
		case GearSlotType.Weapon_Slot1:		// 1차 무장.
			gearIdList = _UserData.GetOwnGearByGearType(GearType.Weapon_Gun);
			gearIdList.AddRange(_UserData.GetOwnGearByGearType(GearType.Weapon_Rocket));
			gearIdList.AddRange(_UserData.GetOwnGearByGearType(GearType.Weapon_Missle));
			ownGearId = member.Weapon2GearId;
			GuideArrowManager.getInstence().ShowArrow(SceneType.HangarPopup_Weapon2);
			break;
		case GearSlotType.Weapon_Slot2:		// 2차 무장.
			gearIdList = _UserData.GetOwnGearByGearType(GearType.Weapon_Gun);
			gearIdList.AddRange(_UserData.GetOwnGearByGearType(GearType.Weapon_Rocket));
			gearIdList.AddRange(_UserData.GetOwnGearByGearType(GearType.Weapon_Missle));
			ownGearId = member.Weapon3GearId;
			GuideArrowManager.getInstence().ShowArrow(SceneType.HangarPopup_Weapon3);
			break;
		}
		ListMenuModel[] gearList = GetGearListModel(gearIdList);

		short selectIndex = 0;
		foreach(ListMenuModel listMenuModel in gearList) {
			if(listMenuModel.id == ownGearId) break;
			selectIndex ++;
		}

		/*
		if(_GearDataUIObj == null) {
			_GearDataUIObj = Instantiate(Resources.Load<GameObject>("Common/GearDataUI")) as GameObject;
			_GearDataUIObj.transform.parent = this.gameObject.transform;
			_GearDataUIObj.transform.localScale = new Vector2(0.7f, 0.7f);
			_GearDataUIObj.transform.position = new Vector2(this.gameObject.transform.position.x  - 6.46f, 
			                                                this.gameObject.transform.position.y  + 0.73f);
			_GearDataUI = _GearDataUIObj.GetComponent<GearDataUI>();
			_GearDataUI.init();
			_GearDataUI.SetSorting(10);

		}*/


		//_GearDataUI.GearUpdate(ownGearId);

		_GearScrollMenu.sortingNum = 10;
		_GearScrollMenu.SetScrollData(gearList, selectIndex);
		_GearScrollMenu.SetScrollView();

	}

	private ListMenuModel[] GetGearListModel(ArrayList data) {
		ListMenuModel[] gearList = new ListMenuModel[data.Count + 1];

		gearList[0] = new ListMenuModel();

		switch(_SelectGearType) {
		case GearSlotType.Suit:
			gearList[0].id = 1;
			gearList[0].scriptString = _ScriptData.GetGameScript(160166).script;
			break;
		case GearSlotType.Body:
			gearList[0].id = 2;
			gearList[0].scriptString = _ScriptData.GetGameScript(160166).script;
			break;
		case GearSlotType.Engine:
			gearList[0].id = 3;
			gearList[0].scriptString = _ScriptData.GetGameScript(160166).script;
			break;
		case GearSlotType.Weapon_Default:
			gearList[0].id = 4;
			gearList[0].scriptString = _ScriptData.GetGameScript(160137).script;
			break;
		case GearSlotType.Weapon_Slot1:
			gearList[0].id = 0;
			gearList[0].scriptString = _ScriptData.GetGameScript(160140).script;
			break;
		case GearSlotType.Weapon_Slot2:
			gearList[0].id = 0;
			gearList[0].scriptString = _ScriptData.GetGameScript(160140).script;
			break;
		}

		byte index = 1;
		foreach(short gearId in data) {
			Gear gear = _GearData.GetGearByID(gearId);
			print("gearId : " + gearId);
			OwnGear ownGear = _UserData.GetOwnGearByGearId(gearId);
			gearList[index] = new ListMenuModel();
			gearList[index].id = gearId;
			string gearName = _ScriptData.GetGameScript(gear.scriptId).script;
			gearList[index].scriptString = gearName + " (x" + ownGear.ownCount + ")";
			
			index ++;
		}

		return gearList;
	}


	private void OnMemberlistClick(int memberId) {
		Member member = _UserData.GetMemberById((byte)(memberId));
		ShowMemberDetailView(member.id);
		ShowGearView(member.id, false, false);
	}

	private void OnGearListClick(int GearId) {
		print("OnGearListClick : " + GearId);
		OwnGear ownGear = _UserData.GetOwnGearByGearId((byte)(GearId));
		Member member = _UserData.GetMemberById(_SelectMemberId);
		OwnGear prevGear = null;
		print("ownGear : " + ownGear);
		short prevGearId = 0;
		if(ownGear != null) {
			if(ownGear.ownCount <= 0) return;

			switch(_SelectGearType) {
			case GearSlotType.Suit:
				if(member.SuitGearId != ownGear.gearId) {
					prevGearId = member.SuitGearId;
					member.SuitGearId = ownGear.gearId;
				} else {
					return;
				}
				break;
			case GearSlotType.Body:
				if(member.BodyGearId != ownGear.gearId) {
					prevGearId = member.BodyGearId;
					member.BodyGearId = ownGear.gearId;
				} else {
					return;
				}
				break;
			case GearSlotType.Engine:
				if(member.EngineGearId != ownGear.gearId) {
					prevGearId = member.EngineGearId;
					member.EngineGearId = ownGear.gearId;
				} else {
					return;
				}
				break;
			case GearSlotType.Weapon_Default:
				if(member.Weapon1GearId != ownGear.gearId) {
					prevGearId = member.Weapon1GearId;
					member.Weapon1GearId = ownGear.gearId;
				} else {
					return;
				}
				break;
			case GearSlotType.Weapon_Slot1:
				if(member.Weapon2GearId != ownGear.gearId) {
					prevGearId = member.Weapon2GearId;
					member.Weapon2GearId = ownGear.gearId;
				} else {
					return;
				}
				break;
			case GearSlotType.Weapon_Slot2:
				if(member.Weapon3GearId != ownGear.gearId) {
					prevGearId = member.Weapon3GearId;
					member.Weapon3GearId = ownGear.gearId;
				} else {
					return;
				}
				break;
			}

			ownGear.ownCount -= 1;

			MissionData.getInstence().AddMissionGoal(MissionGoalType.Hangar_Gear_Change, 1);
		} else {

			switch(_SelectGearType) {
			case GearSlotType.Suit:
				prevGearId = member.SuitGearId;
				member.SuitGearId = 1;
				break;
			case GearSlotType.Body:
				prevGearId = member.BodyGearId;
				member.BodyGearId = 2;
				break;
			case GearSlotType.Engine:
				prevGearId = member.EngineGearId;
				member.EngineGearId = 3;
				break;
			case GearSlotType.Weapon_Default:
				prevGearId = member.Weapon1GearId;
				member.Weapon1GearId = 4;
				break;
			case GearSlotType.Weapon_Slot1:
				prevGearId = member.Weapon2GearId;
				member.Weapon2GearId = 0;
				break;
			case GearSlotType.Weapon_Slot2:
				prevGearId = member.Weapon3GearId;
				member.Weapon3GearId = 0;
				break;
			}

		}

		// 이전에 착용한 장비 복원.
		if(prevGearId > 4) {
			prevGear = _UserData.GetOwnGearByGearId(prevGearId);
			if(prevGear != null) {
				prevGear.ownCount += 1;
			} else {
				Gear newGear = _GearData.GetGearByID(prevGearId);
				if(newGear != null) {
					OwnGear newOwnGear = new OwnGear();
					newOwnGear.gearId = newGear.id;
					newOwnGear.ownCount = 1;
					_UserData.UserOwnGearList.Add(newOwnGear);
				}

			}
		}

		LocalData.getInstence().UserOwnGearDataSave();
		LocalData.getInstence().UserMemberDataSave();

		ShowGearView(_SelectMemberId, true, false);
		OnGearMenuClick(_SelectGearType);

		//_ViewType = 2;
		//OnViewChange(0);
	}

	private void ShowMemberDetailView(short memberId) {

		_SelectMemberId = memberId;

		Member member = _UserData.GetMemberById(memberId);

		_MemberDataUI.MemberUpdate(member.id, false);
		ShowUnitName(member.id);

		StoryDataModel memberData = StoryData.getInstence().GetMemberStoryModelByid(member.DefaultId);
		if(memberData == null) return;

		// 대원 정보 노출.
		if(_MemberCommentObj == null) {
			_MemberCommentObj = SetAddTextMesh(memberData.Comment, 16, TextAnchor.UpperLeft, Color.white);
			_MemberCommentObj.GetComponent<TextMesh>().alignment = TextAlignment.Left;
			_MemberCommentObj.transform.position = new Vector2(1.32f, 1.32f + this.gameObject.transform.position.y);
			_MemberCommentObj.transform.parent = this.gameObject.transform;
			_MemberCommentObj.GetComponent<Renderer>().enabled = false;
			_MemberCommentObj.name = "member comment";
			_MemberCommentObj.layer = LayerMask.NameToLayer("UI");
		} else {
			_MemberCommentObj.GetComponent<TextMesh>().text = memberData.Comment;
		}

	}

	private void OnDetailViewClick(int id) {
		if(MemberScrollMenu.transform.position.y == -20f) {
			OnViewChange(0);
		} else {
			OnViewChange(1);

		}
	}

	private void OnShowUnitGearView(int id) {
		OnViewChange(3);
	}

	private void ShowUnitName(short memberId) {
		string unitName = _ScriptData.GetGameScript(160161).script + " : " + _ScriptData.GetGameScript(160160).script;

		Member member = _UserData.GetMemberById(memberId);
		Unit memberInUnit = _UserData.GetUnitById(member.UnitId);
		if(memberInUnit != null) unitName = _ScriptData.GetGameScript(160161).script + " : " + _ScriptData.GetUnitNameByUnitId(memberInUnit.id);
		if(_UnitNameTxt == null) {
			Color unitColor = Color.white;
			unitColor.r = 1f;
			unitColor.g = 1f;
			unitColor.b = 0.8f;
			_UnitNameTxt = SetAddTextMesh(unitName, 13, TextAnchor.MiddleLeft, unitColor);
			_UnitNameTxt.transform.position = new Vector2(3.01f, 1.82f + this.gameObject.transform.position.y);
			_UnitNameTxt.transform.parent = this.gameObject.transform;
			_UnitNameTxt.layer = LayerMask.NameToLayer("UI");
		} else {
			_UnitNameTxt.GetComponent<TextMesh>().text = unitName;
		}

		_SelectMemberId = member.id;
	}
		 

	private void OnViewChange(byte nextView) {

		float scrollX = 4.01f;
		float scrollY = -1.45f;
		string btnName = "";
		switch(nextView) {	
		case 0:		// 맴버 리스트.
			btnName = _ScriptData.GetGameScript(120707).script;
			MemberScrollMenu.transform.position = new Vector2(this.gameObject.transform.position.x + scrollX, 
			                                                  this.gameObject.transform.position.y + scrollY);
			GearScrollMenu.transform.position = new Vector2(this.gameObject.transform.position.x + scrollX, 
			                                                this.gameObject.transform.position.y - 20f);
			SetBtnsEnable(true);
			ShowGearView(_SelectMemberId, false, false);
			_DefaultCraft.transform.position = new Vector2(-2f, 20f);
			if(_MemberCommentObj != null) _MemberCommentObj.GetComponent<Renderer>().enabled = false;
			break;
		case 1:		// 맴버 디테일 정보.
			btnName = _ScriptData.GetGameScript(120706).script;
			MemberScrollMenu.transform.position = new Vector2(this.gameObject.transform.position.x + scrollX, 
			                                                  this.gameObject.transform.position.y - 20f);
			GearScrollMenu.transform.position = new Vector2(this.gameObject.transform.position.x + scrollX, 
			                                                this.gameObject.transform.position.y - 20f);
			GuideArrowManager.getInstence().ShowArrow(SceneType.HangarPopup);
			if(_MemberCommentObj != null) _MemberCommentObj.GetComponent<Renderer>().enabled = true;
			break;
		case 2:		// 기어 선택창.
			btnName = _ScriptData.GetGameScript(120706).script;
			MemberScrollMenu.transform.position = new Vector2(this.gameObject.transform.position.x + scrollX, 
			                                                  this.gameObject.transform.position.y - 20f);
			GearScrollMenu.transform.position = new Vector2(this.gameObject.transform.position.x + scrollX, 
			                                                this.gameObject.transform.position.y + scrollY);
			GuideArrowManager.getInstence().ShowArrow(SceneType.HangarPopup_DetailView);
			if(_MemberCommentObj != null) _MemberCommentObj.GetComponent<Renderer>().enabled = false;
			break;
		case 3:		// 미리 보기.
			btnName = _ScriptData.GetGameScript(120706).script;
			MemberScrollMenu.transform.position = new Vector2(this.gameObject.transform.position.x + scrollX, 
			                                                  this.gameObject.transform.position.y - 20f);
			GearScrollMenu.transform.position = new Vector2(this.gameObject.transform.position.x + scrollX, 
			                                                this.gameObject.transform.position.y - 20f);

			SetBtnsEnable(false);
			ShowGearView(_SelectMemberId, false, true);
			if(_MemberCommentObj != null) _MemberCommentObj.GetComponent<Renderer>().enabled = true;
			StartCoroutine(SetCraftField());
			break;
		}


		ViewTypeBtn.GetComponent<CommonBtn>().SetBtnName(btnName);
	}

	private void SetBtnsEnable(bool isEnable) {
		GearMenu1.GetComponent<CommonBtn>().SetEnabled(isEnable);
		GearMenu2.GetComponent<CommonBtn>().SetEnabled(isEnable);
		GearMenu3.GetComponent<CommonBtn>().SetEnabled(isEnable);
		GearMenu4.GetComponent<CommonBtn>().SetEnabled(isEnable);
		GearMenu5.GetComponent<CommonBtn>().SetEnabled(isEnable);
		GearMenu6.GetComponent<CommonBtn>().SetEnabled(isEnable);
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

	public class MemberClassSort : IComparer
	{
		public int Compare(object x, object y)
		{
			// reverse the arguments
			byte g1 = ((Member)x).ClassId;
			byte g2 = ((Member)y).ClassId;
			
			if (g1 > g2)
				return -1;
			else
				return 0;
		}
		
	}
	
}
