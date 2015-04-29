using UnityEngine;
using System;
using System.Collections;

public class HQPopup : MonoBehaviour {

	public bool isTest;
	public GameObject CloseBtn;

	public GameObject MainMenu1;
	public GameObject MainMenu2;
	public GameObject MainMenu3;
	public GameObject MainMenu4;
	public GameObject ScrollMenu;
	public GameObject MemberScrollMenu;

	public GameObject BaseDataView;
	public GameObject DataSelectBtn1;
	public GameObject DataSelectBtn2;
	public GameObject ClassMark;

	public GameObject SearchImg;
	public GameObject SearchIcon;

	public Color TitleFontColor;
	public Color TitleFontLineColor;
	public Color SubTitleFontColor;
	public Color SubTitleFontLineColor;
	public Color DataBtnColor;

	public GUISkin guiSkin;

	private GameObject SubTitleTxt;
	private GameObject TitleThumbnail;
	private GameObject DataTitleTxt;
	private OutLineFont DataTitleFont;

	private GameObject _DataSelectBtn3;
	private GameObject _DataSelectBtn4;

	private ScrollMenu _ScrollMenu;
	private ScrollMenberMenu _ScrollMemberMenu;

	private int _MainIndex = 0;
	private int _SubIndex = 0;

	private GameObject[] MainBtnList;
	private string[] _MainBtnNames;
	private ListMenuModel[] _ListMenuData;
	private ListMemberModel[] _ListMemberData;

	public delegate void CloseEvent();
	private CloseEvent _CloseCallback;

	private UserData _UserData;
	private WarzoneData _WarzoneData;
	private ScriptData _ScriptData;
	private SystemData _SystemData;
	private MemberData _MemberData;

	private GameObject _MemberDataUIObj;
	private MemberDataUI _MemberDataUI;

	private ArrayList _UserTowns;

	private byte _SearchIconMoveCount;

	private GameObject _RewardViewer;

	private int[] _BaseScriptList;
	private int[,] _BaseDataScriptList;

	private bool _OnTime;
	private float _Timer;

	private GameObject _MemberCommentObj;
	private string _MemberComment = "";
	private float _WidthScale;
	private float _HeightScale;

	private GameObject _MemberDataUIPop;

	void Awake() {

		// 1024 X 600
		_WidthScale = Screen.width / 1024f;
		_HeightScale = Screen.height / 600f;

		DarkSprite.getInstence();

		_BaseScriptList = new int[3];
		_BaseScriptList[0] = 130104;
		_BaseScriptList[1] = 130105;
		_BaseScriptList[2] = 130106;

		_BaseDataScriptList = new int[3,4];
		_BaseDataScriptList[0,0] = 150100;
		_BaseDataScriptList[0,1] = 160101;
		_BaseDataScriptList[0,2] = 160102;
		_BaseDataScriptList[0,3] = 160100;

		_BaseDataScriptList[1,0] = 150101;
		_BaseDataScriptList[1,1] = 160101;
		_BaseDataScriptList[1,2] = 160102;
		_BaseDataScriptList[1,3] = 160100;

		_BaseDataScriptList[2,0] = 150102;
		_BaseDataScriptList[2,1] = 160101;
		_BaseDataScriptList[2,2] = 160102;
		_BaseDataScriptList[2,3] = 160100;

		_SystemData = SystemData.GetInstance();
		_UserData = UserData.getInstence();
		_WarzoneData = WarzoneData.getInstence();
		_ScriptData = ScriptData.getInstence();
		_MemberData = MemberData.getInstence();

		_MainBtnNames = new string[]{_ScriptData.GetGameScript(120101).script, _ScriptData.GetGameScript(120103).script
			, _ScriptData.GetGameScript(120102).script, _ScriptData.GetGameScript(120100).script};

		CloseBtn.GetComponent<ButtonEvent>().SetCallBack(OnPopupClose);
		MainBtnList = new GameObject[]{MainMenu1, MainMenu2, MainMenu3, MainMenu4};
		int btnIndex = 0;
		foreach(GameObject btn in MainBtnList)
		{
			btn.GetComponent<CommonBtn>().BtnName = _MainBtnNames[btnIndex];
			btn.GetComponent<CommonBtn>().SetClick(OnMainMenuClick);
			btnIndex ++;
		}

		_ScrollMenu = ScrollMenu.GetComponent<ScrollMenu>();
		_ScrollMenu.SetMenuClick(OnListMenuClick);
		_ScrollMemberMenu = MemberScrollMenu.GetComponent<ScrollMenberMenu>();
		_ScrollMemberMenu.SetMenuClick(OnListMenuClick);

		DataSelectBtn1.GetComponent<CommonBtn>().SetClick(OnDataBtnClick);
		DataSelectBtn2.GetComponent<CommonBtn>().Init(0, "", 10, DataBtnColor);
		DataSelectBtn2.GetComponent<CommonBtn>().SetClick(OnDataBtn2Click);

		DataTitleTxt = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
		DataTitleFont = DataTitleTxt.GetComponent<OutLineFont>();
		DataTitleFont.SetString("");
		DataTitleFont.SetAlign(TextAnchor.MiddleLeft);
		DataTitleTxt.transform.parent = this.gameObject.transform;
		DataTitleTxt.transform.position = new Vector2(-6.98f, 0.26f);
		DataTitleFont.SetFontSize(22);
		DataTitleFont.SetLineSize(2);
		SubTitleFontColor.a = 1;
		DataTitleFont.SetFontColor(SubTitleFontColor);
		TitleFontLineColor.a = 1;
		DataTitleFont.SetLineColor(TitleFontLineColor);

		TitleThumbnail = new GameObject();
		TitleThumbnail.AddComponent<SpriteRenderer>();
		TitleThumbnail.transform.parent = this.gameObject.transform;
		TitleThumbnail.transform.position = new Vector2(-6.04f, -1.03f);
		TitleThumbnail.transform.localScale = new Vector2(0.75f, 0.75f);

		SubTitleTxt = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
		SubTitleTxt.name = "titleTxt";
		SubTitleTxt.GetComponent<OutLineFont>().SetString("");
		SubTitleTxt.GetComponent<OutLineFont>().SetFontSize(24);
		SubTitleTxt.GetComponent<OutLineFont>().SetLineSize(1.5f);
		SubTitleTxt.GetComponent<OutLineFont>().SetAlign(TextAnchor.MiddleLeft);
		TitleFontColor.a = 1;
		SubTitleTxt.GetComponent<OutLineFont>().SetFontColor(TitleFontColor);
		TitleFontLineColor.a = 1;
		SubTitleTxt.GetComponent<OutLineFont>().SetLineColor(TitleFontLineColor);
		SubTitleTxt.transform.parent = this.gameObject.transform;
		SubTitleTxt.transform.position = new Vector2(1.24f, 4.19f);

		SearchImg.renderer.sortingOrder = 10;
		SearchIcon.renderer.sortingOrder = 11;
		SearchImg.renderer.enabled = false;
		SearchIcon.renderer.enabled = false;
		SearchImg.GetComponent<BoxCollider>().enabled = false;

		_RewardViewer = Instantiate(Resources.Load<GameObject>("Common/RewardViewer")) as GameObject;
		_RewardViewer.transform.parent = this.gameObject.transform;
	}

	void Start() {
		if(isTest) {
			StartCoroutine(MissionData.getInstence().init());
			LocalData.getInstence().AllLoad();
			init();
		}
	}

	public void init() {

		SelectMainMenu();
		GameObject btn = MainBtnList[0] as GameObject;
		btn.GetComponent<CommonBtn>().SetBtnSelect(true);

		byte btnIndex = 0;
		int storyStep = UserData.getInstence().StoryStepId;
		foreach(GameObject btnObj in MainBtnList) {
			if(btnObj == null) continue;
			if(btnIndex == 1 && storyStep < 11) btnObj.GetComponent<CommonBtn>().SetEnabled(false); // 대원 관리.
			if(btnIndex == 2 && storyStep < 13) btnObj.GetComponent<CommonBtn>().SetEnabled(false);	// 대원 모집.
			if(btnIndex == 3 && storyStep < 20) btnObj.GetComponent<CommonBtn>().SetEnabled(false);	// 기지 개발.

			btnIndex ++;
		}

		_MemberDataUIObj = Instantiate(Resources.Load<GameObject>("Common/MemberDataUI")) as GameObject;
		_MemberDataUIObj.transform.parent = this.gameObject.transform;
		_MemberDataUIObj.transform.localScale = new Vector2(0.75f, 0.75f);
		_MemberDataUIObj.transform.position = new Vector2(-5.91f, -1.15f);
		_MemberDataUI = _MemberDataUIObj.GetComponent<MemberDataUI>();
		_MemberDataUI.init(0);
		_MemberDataUI.MemberUpdate(0, false);

		StartCoroutine(StoryData.getInstence().CheckStoryModel(50002));
	}

	void Update() {
		if(_OnTime) {
			_Timer += Time.deltaTime;
			if(_Timer > 1) {
				OnScreenUpate();
				_Timer = 0;
			}
		}
	}


	public void SetCloseEventCallBack(CloseEvent onCloseEvent) {
		_CloseCallback = new CloseEvent(onCloseEvent);

	}


	private void OnPopupClose() {
		if(_CloseCallback != null) _CloseCallback();
	}

	// 메인 메뉴 선택.
	private void SelectMainMenu() {

		HideMemberComment();

		string subComment = "";

		_ListMenuData = null;
		_ListMemberData = null;
		_UserTowns = null;
		_OnTime = false;
		int index = 0;
		if(_MainIndex == 3) {	// 기지개발

			_SubIndex = 0;

			ScrollMenu.transform.position = new Vector2(3.98f, 0.59f);
			MemberScrollMenu.transform.position = new Vector2(20f, 0.59f);
			BaseDataView.transform.position = new Vector2(-6.75f, -1.11f);

			_ListMenuData = new ListMenuModel[_BaseScriptList.Length];
			foreach(int scriptId in _BaseScriptList) {
				_ListMenuData[index] = new ListMenuModel();
				_ListMenuData[index].id = (byte)(index + 1);
				_ListMenuData[index].scriptId = scriptId;

				index ++;
			}

			SelectSubMenu1();

			_ScrollMenu.SetScrollData(_ListMenuData, 0);
			_ScrollMenu.SetScrollView();
				
		} else if (_MainIndex == 0) {	// 세금징수

			_SubIndex = 0;

			ScrollMenu.transform.position = new Vector2(3.98f, 0.59f);
			MemberScrollMenu.transform.position = new Vector2(20f, 0.59f);
			BaseDataView.transform.position = new Vector2(-6.75f, -1.11f);

			_UserTowns = _WarzoneData.GetUserTownList();
			_ListMenuData = new ListMenuModel[_UserTowns.Count];
			foreach(UserTown userTown in _UserTowns) {
				_ListMenuData[index] = new ListMenuModel();
				_ListMenuData[index].id = (byte)(index + 1);
				_ListMenuData[index].scriptId = _WarzoneData.GetDefaultTownData(userTown.id).townNameId;

				index ++;
			}

			SelectSubMenu2();

			_ScrollMenu.SetScrollData(_ListMenuData, 0);
			_ScrollMenu.SetScrollView();

		} else if (_MainIndex == 2) {	// 대원모집

			ArrayList defaultMemberIds = _MemberData.GetRandomMemberidsByCore(0);
			_ListMemberData = new ListMemberModel[defaultMemberIds.Count];

			MemberScrollMenu.transform.position = new Vector2(20f, 0.59f);
			ScrollMenu.transform.position = new Vector2(20f, 0.59f);
			BaseDataView.transform.position = new Vector2(-4.89f, -1.3f);

			DefaultMember defaultMember;

			foreach(byte memberId in defaultMemberIds) {

				defaultMember = _MemberData.GetDefaultMemberByID(memberId);
				_ListMemberData[index] = new ListMemberModel();
				_ListMemberData[index].id = (byte)(index + 1);
				_ListMemberData[index].optionalId = memberId;
				_ListMemberData[index].scriptString = _ScriptData.GetDefaultMemberName(defaultMember.nameId);
				_ListMemberData[index].ClassId = defaultMember.classN;

				_MemberComment = "...";

				index ++;
			}

			bool isMemberUIMovie = true;
			if(_SubIndex == 0) isMemberUIMovie = false;
			if(_ListMemberData.Length > 0) {
				_SubIndex = _ListMemberData[0].optionalId;
				//TitleThumbnail.transform.position = new Vector2(-5.6f, 2);
				//TitleThumbnail.transform.localScale = new Vector2(2f, 2f);
			} else {
				_SubIndex = 0;
			}

			if(isMemberUIMovie == true) {
				iTween.ColorTo(_MemberDataUIObj, iTween.Hash("r", 5, "g", 5, "b", 5, "time", 0.1f
				                                             , "oncomplete", "SetMemberDataUpdateCom", "oncompletetarget", this.gameObject));
			} else {
				SelectSubMenu3();
			}
			//SelectSubMenu3();

			//_ScrollMemberMenu.SetScrollData(_ListMemberData, 0);
			//_ScrollMemberMenu.SetScrollView();

		} else if (_MainIndex == 1) {	// 대원관리

			MemberScrollMenu.transform.position = new Vector2(3.98f, 0.59f);
			ScrollMenu.transform.position = new Vector2(20f, 0.59f);
			BaseDataView.transform.position = new Vector2(-6.75f, -1.89f);

			subComment = " <size=18>(" + _UserData.UserMemberList.Count + "/";
			subComment += _UserData.GetBaseSoildersByLevel(_UserData.BaseLevel) + ")</size>";

			/*
			ArrayList stateMemberList = new ArrayList();
			foreach(Member member in _UserData.UserMemberList) {
				if(member.state != MemberStateType.Ready) {
					stateMemberList.Add(member);
				}
			}
			*/
			ArrayList stateMemberList = _UserData.UserMemberList;

			//stateMemberList.Sort(new MemberClassSort());
			//stateMemberList.Sort(new MemberOutListMPSort());
			stateMemberList.Sort(new MemberOutListMPAndHPSort());
			//stateMemberList.Sort(new MemberOutListHPSort());

			_ListMemberData = new ListMemberModel[stateMemberList.Count];

			Color fontColor = Color.white;
			foreach(Member member in stateMemberList) {
				_ListMemberData[index] = new ListMemberModel();
				_ListMemberData[index].id = (byte)(index + 1);
				_ListMemberData[index].optionalId = member.id;
				if(member.state == MemberStateType.Wound) {
					fontColor.r = 0.8f;
					fontColor.g = 0.6f;
					fontColor.b = 0f;
				} else if (member.state == MemberStateType.Mia) {
					fontColor.r = 1f;
					fontColor.g = 0.4f;
					fontColor.b = 0f;
				} else {
					fontColor.r = 0.3f;
					fontColor.g = 0.8f;
					fontColor.b = 0.9f;
				}
				_ListMemberData[index].scriptString = _ScriptData.GetMemberNameByMemberId(member.id);
				_ListMemberData[index].fontColor = fontColor;
				_ListMemberData[index].ClassId = _UserData.GetMemberById(member.id).ClassId;

				index ++;
			}

			if(_ListMemberData.Length > 0) {
				_SubIndex = _ListMemberData[0].optionalId;
			} else {
				_SubIndex = 0;
			}

			//TitleThumbnail.transform.position = new Vector2(-5.6f, 2);
			//TitleThumbnail.transform.localScale = new Vector2(2f, 2f);

			_ScrollMemberMenu.SetScrollData(_ListMemberData, 0);
			_ScrollMemberMenu.SetScrollView();

			SelectSubMenu4();

		} else {	// 대원관리

			_ListMenuData = new ListMenuModel[0];
		}


		MainBtnList[_MainIndex].GetComponent<CommonBtn>().SetBtnSelect(true);
		//SubTitleTxt.GetComponent<TextMesh>().text = _MainBtnNames[_MainIndex];
		SubTitleTxt.GetComponent<OutLineFont>().SetString(_MainBtnNames[_MainIndex] + subComment);
	}

	private void SetMemberDataUpdateCom() {
		SelectSubMenu3();
		iTween.ColorTo(_MemberDataUIObj, iTween.Hash("r", 1, "g", 1, "b", 1, "time", 0.1f));
	}

	/**
	 * 기지 개발 관련 정보 노출
	 * */
	private void SelectSubMenu1() {

		if(_DataSelectBtn3 != null) Destroy(_DataSelectBtn3);
		if(_DataSelectBtn4 != null) Destroy(_DataSelectBtn4);

		_MemberDataUI.MemberUpdate(0, false);

		int subDataTitle = _BaseScriptList[_SubIndex];

		string value1 = "";
		string value2 = "";
		string value3 = "";

		if(_SubIndex == 0) {
			value1 = _UserData.GetBaseSoildersByLevel(_UserData.BaseLevel) + GetStringByScriptId(160103);
			value2 = _UserData.GetBaseSoildersByLevel((byte)(_UserData.BaseLevel + 1)) + GetStringByScriptId(160103);
			value3 = _UserData.GetBasePayByLevel((byte)(_UserData.BaseLevel + 1)) + "";
			if(_UserData.BaseLevel > _SystemData.BaseUpgradeMax) {
				value2 =  "MAX"; value3 = "0";
			}
		} else if(_SubIndex == 1) {
			value1 = _UserData.ResearchLevel + GetStringByScriptId(160104);
			value2 = (_UserData.ResearchLevel + 1) + GetStringByScriptId(160104);
			value3 = _UserData.GetResearchPayByLevel((byte)(_UserData.ResearchLevel + 1)) + "";
			if(_UserData.ResearchLevel > _SystemData.ResearchUpgradeMax) {
				value2 =  "MAX"; value3 = "0";
			}
		} else {
			value1 = _UserData.GetFactoryLineByLevel(_UserData.FactoryLevel) + GetStringByScriptId(160105);
			value2 = _UserData.GetFactoryLineByLevel((byte)(_UserData.FactoryLevel + 1)) + GetStringByScriptId(160105);
			value3 = _UserData.GetFactoryPayByLevel((byte)(_UserData.FactoryLevel + 1)) + "";
			if(_UserData.FactoryLevel > _SystemData.FactoryUpgradeMax) {
				value2 =  "MAX"; value3 = "0";
			}
		}

		DataTitleFont.SetString(_ScriptData.GetGameScript(subDataTitle).script);
		Transform dataTxtline1 = BaseDataView.transform.FindChild("DataText1");
		Transform dataTxtline2 = BaseDataView.transform.FindChild("DataText2");
		Transform dataTxtline3 = BaseDataView.transform.FindChild("DataText3");
		Transform dataTxtline4 = BaseDataView.transform.FindChild("DataText4");
		string txtline2 = "";
		string txtline3 = "";
		string txtline4 = "";
		if(value1 == "MAX") {
			txtline2 = GetStringByScriptId(_BaseDataScriptList[_SubIndex, 1]) + " : " + value1;
		} else {
			txtline2 = GetStringByScriptId(_BaseDataScriptList[_SubIndex, 1]) + " : " + value1;
		}

		if(value2 == "MAX") {
			txtline3 = "<color=#ff7700>최대치까지 개발되였습니다.</color>";
		} else {
			txtline3 = GetStringByScriptId(_BaseDataScriptList[_SubIndex, 2]) + " : " + value2;
		}

		if(value3 == "0") {
			txtline4 = "";
		} else {
			txtline4 = GetStringByScriptId(_BaseDataScriptList[_SubIndex, 3]) + " : ";
		}

		dataTxtline1.GetComponent<TextMesh>().text =  GetStringByScriptId(_BaseDataScriptList[_SubIndex, 0]);
		dataTxtline2.GetComponent<TextMesh>().text =  txtline2;
		dataTxtline3.GetComponent<TextMesh>().text =  txtline3;
		dataTxtline4.GetComponent<TextMesh>().text =  txtline4;

		UserData.getInstence().GetBaseSoildersByLevel(1);

		DataSelectBtn1.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(130100).script, 10, DataBtnColor);
		DataSelectBtn1.GetComponent<CommonBtn>().SetEnabled(true);
		//DataSelectBtn.GetComponent<CommonBtn>().Init(0, "-" + value3, 10, DataBtnColor);
		DataSelectBtn2.GetComponent<CommonBtn>().SetEnabled(false);

		BillModel billModel = new BillModel();
		billModel.money = System.Convert.ToInt32(value3);
		billModel.moneyPlus = false;
		_RewardViewer.transform.localScale = new Vector3(1f, 1f, 1f);
		_RewardViewer.GetComponent<RewardViewer>().init(billModel, 5, false);
		_RewardViewer.transform.position = new Vector2(-5.57f, -1.8f);
		_RewardViewer.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
	}

	/**
	 * 마을 세금 징수 정보 노출.
	 * */
	private void SelectSubMenu2() {

		if(_DataSelectBtn3 != null) Destroy(_DataSelectBtn3);
		if(_DataSelectBtn4 != null) Destroy(_DataSelectBtn4);

		if(_MemberDataUI != null) _MemberDataUI.MemberUpdate(0, false);

		DataTitleFont.SetString("");
		Transform dataTxtline1 = BaseDataView.transform.FindChild("DataText1");
		Transform dataTxtline2 = BaseDataView.transform.FindChild("DataText2");
		Transform dataTxtline3 = BaseDataView.transform.FindChild("DataText3");
		Transform dataTxtline4 = BaseDataView.transform.FindChild("DataText4");

		dataTxtline1.GetComponent<TextMesh>().text = "";
		dataTxtline2.GetComponent<TextMesh>().text = "";
		dataTxtline3.GetComponent<TextMesh>().text = "";
		dataTxtline4.GetComponent<TextMesh>().text = "";

		DataSelectBtn1.GetComponent<CommonBtn>().SetEnabled(false);

		if(_SubIndex >= _UserTowns.Count) return;

		UserTown selectTown = _UserTowns[_SubIndex] as UserTown;

		Town town = _WarzoneData.GetDefaultTownData(selectTown.id);

		if(DarkSprite.getInstence() != null) {
			if(DarkSprite.getInstence().MainMapData != null) DarkSprite.getInstence().MainMapData.UpdateTownMove(town.id);
		}

		DataTitleFont.SetString(GetStringByScriptId(town.townNameId));

		dataTxtline1.GetComponent<TextMesh>().text =  GetStringByScriptId(160108) + " : " + selectTown.resident;
		long lastTime = (selectTown.lastTaxTime + (_SystemData.TownTaxCollectdelay * _SystemData.millisecondNum)) - _SystemData.getCurrentTime();
		if(lastTime < 0) lastTime = 0;
		TimeSpan timespan = new TimeSpan(lastTime);
		string currentTime = "";
		if(lastTime == 0) {
			_OnTime = false;
			currentTime = _ScriptData.GetGameScript(160164).script;
		} else {
			_OnTime = true;
			currentTime = string.Format("{0:D2}:{1:D2}:{2:D2}", timespan.Hours, timespan.Minutes, timespan.Seconds);
		}

		string payValue = "+" + (selectTown.resident * SystemData.GetInstance().ResidentPerPay);
		dataTxtline2.GetComponent<TextMesh>().text =  GetStringByScriptId(160109) + " : " + currentTime;
		dataTxtline3.GetComponent<TextMesh>().text =  GetStringByScriptId(160153) + " : ";
		dataTxtline4.GetComponent<TextMesh>().text =  "";

		//string btnName = "+" + (selectTown.resident * SystemData.GetInstance().ResidentPerPay);
		string btnName = GetStringByScriptId(120101);
		Color btnColor = Color.gray;
		if(selectTown.lastTaxTime + (_SystemData.TownTaxCollectdelay * _SystemData.millisecondNum) > SystemData.GetInstance().getCurrentTime()) {
			DataSelectBtn1.GetComponent<CommonBtn>().Init(0, btnName, 10, btnColor);
		} else {
			DataSelectBtn1.GetComponent<CommonBtn>().Init(0, btnName, 10, DataBtnColor);
		}
		DataSelectBtn1.GetComponent<CommonBtn>().SetEnabled(true);
		DataSelectBtn2.GetComponent<CommonBtn>().SetEnabled(false);

		BillModel billModel = new BillModel();
		billModel.money = (int)(selectTown.resident * SystemData.GetInstance().ResidentPerPay);
		//billModel.money = 1220;
		billModel.moneyPlus = true;
		_RewardViewer.transform.localScale = new Vector3(1f, 1f, 1f);
		_RewardViewer.GetComponent<RewardViewer>().init(billModel, 5, false);
		_RewardViewer.transform.position = new Vector2(-5.61f, -1.36f);
		_RewardViewer.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
	}

	/**
	 * 등록 가능한 맴버 정보 노출.
	 * */
	private void SelectSubMenu3() {

		if(_DataSelectBtn3 != null) Destroy(_DataSelectBtn3);
		if(_DataSelectBtn4 != null) Destroy(_DataSelectBtn4);

		DefaultMember member = _MemberData.GetDefaultMemberByID((byte)(_SubIndex));

		Color btnColor = Color.gray;

		Transform dataTxtline1 = BaseDataView.transform.FindChild("DataText1");
		Transform dataTxtline2 = BaseDataView.transform.FindChild("DataText2");
		Transform dataTxtline3 = BaseDataView.transform.FindChild("DataText3");
		Transform dataTxtline4 = BaseDataView.transform.FindChild("DataText4");

		dataTxtline1.GetComponent<TextMesh>().text =  "";
		dataTxtline2.GetComponent<TextMesh>().text =  "";
		dataTxtline3.GetComponent<TextMesh>().text =  "";
		dataTxtline4.GetComponent<TextMesh>().text =  "";

		BillModel billModel = new BillModel();

		if(member != null) {

			_MemberDataUI.MemberUpdate(member.Id, true);

			StoryDataModel memberData = StoryData.getInstence().GetMemberStoryModelByid(member.Id);
			if(memberData != null) {
				_MemberComment = memberData.Comment;
				ShowMemberComment(memberData.Comment);
			}

			if(DarkSprite.getInstence() != null) {
				if(DarkSprite.getInstence().MainMapData != null) DarkSprite.getInstence().MainMapData.UpdateTownMove(1);
			}

			DataTitleFont.SetString(_ScriptData.GetDefaultMemberName(member.nameId));

			ClassModel classModel = _MemberData.GetClassModelByClassId(member.classN);

			Town town = _WarzoneData.GetDefaultTownData(member.townId);

			//dataTxtline1.GetComponent<TextMesh>().text =  GetStringByScriptId(160112) + " : " + classModel.HP;
			//dataTxtline2.GetComponent<TextMesh>().text =  GetStringByScriptId(160123) + " : " + classModel.MP;
			//dataTxtline3.GetComponent<TextMesh>().text =  GetStringByScriptId(160113) + " : " + classModel.IA;
			dataTxtline4.GetComponent<TextMesh>().text =  GetStringByScriptId(160100) + " : ";

			DataSelectBtn1.GetComponent<CommonBtn>().Init(0, GetStringByScriptId(130101), 10, DataBtnColor);
			DataSelectBtn1.GetComponent<CommonBtn>().SetEnabled(true);
			btnColor.r = 0.4f;
			btnColor.g = 0.6f;
			btnColor.b = 1f;
			//DataSelectBtn2.GetComponent<CommonBtn>().Init(0, GetStringByScriptId(130103), 10, btnColor);
			//DataSelectBtn2.GetComponent<CommonBtn>().SetEnabled(true);

			DataSelectBtn2.GetComponent<CommonBtn>().SetEnabled(false);

			_DataSelectBtn3 = Instantiate(Resources.Load<GameObject>("Common/CommonBtn01")) as GameObject;
			_DataSelectBtn3.transform.localScale = new Vector2(0.9f, 0.9f);
			_DataSelectBtn3.transform.position = new Vector2(-5.45f, -3.25f);
			_DataSelectBtn3.transform.parent = this.gameObject.transform;
			_DataSelectBtn3.GetComponent<CommonBtn>().Init(0, "일반 탐색", 30, Color.white);
			_DataSelectBtn3.GetComponent<CommonBtn>().SetClick(SetMemberSearch);

			_DataSelectBtn4 = Instantiate(Resources.Load<GameObject>("Common/CommonBtn01")) as GameObject;
			_DataSelectBtn4.transform.localScale = new Vector2(0.9f, 0.9f);
			_DataSelectBtn4.transform.position = new Vector2(-2.56f, -3.25f);
			_DataSelectBtn4.transform.parent = this.gameObject.transform;
			_DataSelectBtn4.GetComponent<CommonBtn>().Init(1, "정밀 탐색", 30, btnColor);
			_DataSelectBtn4.GetComponent<CommonBtn>().SetClick(SetMemberSearch);

			billModel.money = classModel.scoutCost;

		} else {
			dataTxtline1.GetComponent<TextMesh>().text =  "";
			dataTxtline2.GetComponent<TextMesh>().text =  "";
			dataTxtline3.GetComponent<TextMesh>().text =  "";
			dataTxtline4.GetComponent<TextMesh>().text =  "";

			DataSelectBtn1.GetComponent<CommonBtn>().Init(0, "-", 10, btnColor);
			DataSelectBtn2.GetComponent<CommonBtn>().SetEnabled(false);

			billModel.money = 0;
		}

		//billModel.money = 1220;
		billModel.moneyPlus = false;
		_RewardViewer.transform.localScale = new Vector3(1f, 1f, 1f);
		_RewardViewer.transform.position = new Vector2(-3.69f, -1.99f);
		_RewardViewer.GetComponent<RewardViewer>().init(billModel, 5, false);
		_RewardViewer.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

	}

	/**
	 * 관리 대원 정보.
	 * */
	private void SelectSubMenu4() {

		if(_DataSelectBtn3 != null) Destroy(_DataSelectBtn3);
		if(_DataSelectBtn4 != null) Destroy(_DataSelectBtn4);

		Member member =  _UserData.GetMemberById((byte)(_SubIndex));

		Transform dataTxtline1 = BaseDataView.transform.FindChild("DataText1");
		Transform dataTxtline2 = BaseDataView.transform.FindChild("DataText2");
		Transform dataTxtline3 = BaseDataView.transform.FindChild("DataText3");
		Transform dataTxtline4 = BaseDataView.transform.FindChild("DataText4");

		dataTxtline1.GetComponent<TextMesh>().text =  "";
		dataTxtline2.GetComponent<TextMesh>().text =  "";
		dataTxtline3.GetComponent<TextMesh>().text =  "";
		dataTxtline4.GetComponent<TextMesh>().text =  "";
		DataTitleFont.SetString("");

		if(member == null) return;

		_MemberDataUI.MemberUpdate(member.id, false);

		DataTitleFont.SetString(_ScriptData.GetMemberNameByMemberId(member.id));


		ClassModel classModel = _MemberData.GetClassModelByClassId(member.ClassId);

		DefaultMember defaultMember = _MemberData.GetDefaultMemberByID(member.DefaultId);

		if(DarkSprite.getInstence() != null) {
			if(DarkSprite.getInstence().MainMapData != null) DarkSprite.getInstence().MainMapData.UpdateTownMove(1);
		}

		Town town = _WarzoneData.GetDefaultTownData(defaultMember.townId);

		Color btnColor = Color.gray;
		string stateStr = "";
		string hpStr = member.CurrentHP + "/" + member.MaxHP;
		string mpStr = member.CurrentMP + "/" + member.MaxMP;

		BillModel billModel = new BillModel();

		if(member.state == MemberStateType.Wound) {
			// 부상.
			stateStr = "<color=#ff6600>" + GetStringByScriptId(160147) + "</color>";
			int healCost = classModel.healCost;

			billModel.money = classModel.healCost;
			billModel.moneyPlus = false;
			billModel.core = classModel.healChipCount;
			billModel.corePlus = false;

			//dataTxtline2.GetComponent<TextMesh>().text =  GetStringByScriptId(160148) + " " + GetStringByScriptId(160100) + " : " + healCost;
			//dataTxtline3.GetComponent<TextMesh>().text =  GetStringByScriptId(160130) + " : " + GetStringByScriptId(160151) + "x"  + classModel.healChipCount;

			if(classModel.healCost <= _UserData.UserMoney && classModel.healChipCount <= _UserData.UserChips) {
				btnColor.r = 0.8f;
				btnColor.g = 1f;
				btnColor.b = 0.4f;
				DataSelectBtn2.GetComponent<CommonBtn>().Init(0, GetStringByScriptId(130107), 10, btnColor);
			} else {
				DataSelectBtn2.GetComponent<CommonBtn>().Init(0, GetStringByScriptId(130107), 10, btnColor);
			}

		} else if(member.state == MemberStateType.Mia) {
			// 실종.
			stateStr = "<color=#ff0000>" + GetStringByScriptId(160146) + "</color>";
			int rescueCost = classModel.rescueCost;
			//dataTxtline2.GetComponent<TextMesh>().text =  GetStringByScriptId(160148) + " " + GetStringByScriptId(160100) + " : " + rescueCost;
			//dataTxtline3.GetComponent<TextMesh>().text =  GetStringByScriptId(160130) + " : " + GetStringByScriptId(160150) + "x" + classModel.RescueCoreCount;

			if(classModel.rescueCost <= _UserData.UserMoney && classModel.RescueCoreCount <= _UserData.UserCores) {
				btnColor.r = 0.4f;
				btnColor.g = 0.6f;
				btnColor.b = 1f;
				DataSelectBtn2.GetComponent<CommonBtn>().Init(0, GetStringByScriptId(130108), 10, btnColor);
			} else {
				DataSelectBtn2.GetComponent<CommonBtn>().Init(0, GetStringByScriptId(130108), 10, btnColor);
			}

		} else {
			// 회복.
			stateStr = GetStringByScriptId(160154);
			int rescueCost = classModel.rescueCost;
			//dataTxtline2.GetComponent<TextMesh>().text =  GetStringByScriptId(160148) + " " + GetStringByScriptId(160100) + " : " + rescueCost;
			//dataTxtline3.GetComponent<TextMesh>().text =  GetStringByScriptId(160130) + " : " + GetStringByScriptId(160150) + "x" + classModel.RescueCoreCount;

			if(member.CurrentHP < member.MaxHP || member.CurrentMP < member.MaxMP) {
				btnColor.r = 0.8f;
				btnColor.g = 1f;
				btnColor.b = 0.8f;
				DataSelectBtn2.GetComponent<CommonBtn>().Init(0, GetStringByScriptId(130109), 10, btnColor);
			} else {
				DataSelectBtn2.GetComponent<CommonBtn>().Init(0, GetStringByScriptId(130109), 10, btnColor);
			}
			billModel.core = _SystemData.MemberHealCore;
			billModel.corePlus = false;
			billModel.money = _SystemData.MemberHealMoney;
			billModel.moneyPlus = false;
		}

		//dataTxtline1.GetComponent<TextMesh>().text =  GetStringByScriptId(160115) + " : " + stateStr;
		//dataTxtline2.GetComponent<TextMesh>().text =  GetStringByScriptId(160112) + " : " + hpStr;
		//dataTxtline3.GetComponent<TextMesh>().text =  GetStringByScriptId(160123) + " : " + mpStr;
		dataTxtline4.GetComponent<TextMesh>().text =  GetStringByScriptId(160130) + " : ";

		DataSelectBtn1.GetComponent<CommonBtn>().Init(0, GetStringByScriptId(130102), 10, DataBtnColor);

		//dataTxtline4.GetComponent<TextMesh>().text =  "";

		DataSelectBtn1.GetComponent<CommonBtn>().SetEnabled(true);
		DataSelectBtn2.GetComponent<CommonBtn>().SetEnabled(true);

		_RewardViewer.transform.localScale = new Vector3(1f, 1f, 1f);
		_RewardViewer.GetComponent<RewardViewer>().init(billModel, 5, true);
		_RewardViewer.transform.position = new Vector2(-3.94f, -2.56f);
		_RewardViewer.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

	}

	private string GetStringByScriptId(int id) {
		string txt = "";
		if(id > 0) txt = _ScriptData.GetGameScript(id).script;
		return txt;
	}

	private void OnMainMenuClick(int btnIndex) {
		DataSelectBtn1.GetComponent<CommonBtn>().SetEnabled(false);
		// 이전 메뉴 복구
		if(_MainIndex != (btnIndex - 1)) MainBtnList[_MainIndex].GetComponent<CommonBtn>().SetBtnSelect(false);

		_MainIndex = btnIndex - 1;
		_SubIndex = 0;

		if(_MainIndex == 1) {	// 대원 관리.
			MissionData.getInstence().AddMissionGoal(MissionGoalType.HQ_CareMember_Click, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.HQPopup_CareMember);
		} else if (_MainIndex == 2) {	// 대원 모집.
			MissionData.getInstence().AddMissionGoal(MissionGoalType.HQ_GetMember_Click, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.HQPopup_GetMember);
		} else if (_MainIndex == 3) {	// 기지 개발.
			MissionData.getInstence().AddMissionGoal(MissionGoalType.HQ_Upgrade_Click, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.HQPopup_Upgrade);
		} else {
			MissionData.getInstence().AddMissionGoal(MissionGoalType.HQ_Tax_Click, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.HQPopup);
		}

		SelectMainMenu();
	}

	private void OnListMenuClick(int btnId) {
		if(_MainIndex == 3) {
			_SubIndex = btnId - 1;
			SelectSubMenu1();
		} else if (_MainIndex == 0) {
			_SubIndex = btnId - 1;
			SelectSubMenu2();
		} else if (_MainIndex == 2) {
			_SubIndex = btnId;
			SelectSubMenu3();
		} else {
			_SubIndex = btnId;
			SelectSubMenu4();
		}
	}

	private void OnDataBtnClick(int id) {
		BillModel billModel;
		if(_MainIndex == 3) {
			// 기지 개발.
			billModel = _UserData.UpgradeBase((byte)(_SubIndex));
			if(billModel.money > 0) {
				_UserData.UpdatePayData(billModel, new Vector2(-4f, -3.62f));
				SelectSubMenu1();
				string googleStr = "";
				if(_SubIndex == 0) {
					googleStr = "Base-" + _UserData.BaseLevel;
					MissionData.getInstence().AddMissionGoal(MissionGoalType.HQ_Upgrade_BaseUpgrade, 1);
				} else if (_SubIndex == 1) {
					googleStr = "Research-" + _UserData.ResearchLevel;
					MissionData.getInstence().AddMissionGoal(MissionGoalType.HQ_Upgrade_ResearchUpgrade, 1);
				} else {
					googleStr = "Factory-" + _UserData.FactoryLevel;
					MissionData.getInstence().AddMissionGoal(MissionGoalType.HQ_Upgrade_FactoryUpgrade, 1);
				}
				if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("Upgrade", googleStr);
			};

		} else if (_MainIndex == 0) {
			// 세금 징수.
			UserTown selectTown = _UserTowns[_SubIndex] as UserTown;
			billModel = _UserData.GetTownTax(selectTown);
			if(billModel.money > 0) {
				_UserData.UpdatePayData(billModel, new Vector2(-4f, -3.62f));
				SelectSubMenu2();
				MissionData.getInstence().AddMissionGoal(MissionGoalType.HQ_Tax_GetTax, 1);
				if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("GetTax", billModel.money + "-" + _UserData.UserMoney);
				WarzoneData.getInstence().SetTaxNotification();

			};
		} else if (_MainIndex == 2) {
			// 부대원 추가.
			billModel = new BillModel();
			int memberMaxCount = _UserData.GetBaseSoildersByLevel(_UserData.BaseLevel);
			int memberCount = _UserData.UserMemberList.Count;
			if(memberCount >= memberMaxCount) {
				_UserData.SetAlert(_ScriptData.GetGameScript(150105).script, billModel);
				return;
			} else {
				short memberId = _UserData.AddMember((byte)(_SubIndex));
				if(memberId > 0) {
					Member member = _UserData.GetMemberById(memberId);
					ClassModel classModel = _MemberData.GetClassModelByClassId(member.ClassId);
					billModel.money = classModel.scoutCost;
					billModel.moneyPlus =false;

					if(_UserData.StoryStepId > 25) {
						/*
						DefaultMember defaultMember = _MemberData.GetDefaultMemberByID(member.DefaultId);
						string shareComment = _ScriptData.GetGameScript(160167).script;
						shareComment += "<p>'" + _ScriptData.GetDefaultMemberName(defaultMember.nameId) + "' " + _ScriptData.GetGameScript(150153).script;
						shareComment += "<p>" + _ScriptData.GetGameScript(150154).script;
						string memberThumbUrl = "member/Member" + defaultMember.thumbId + ".jpg";
						_SystemData.SetSharePopup(10, shareComment, memberThumbUrl, _ScriptData.GetDefaultMemberName(defaultMember.nameId));
						*/
						ShowMemberDataPopup(member.DefaultId);
					}
				}
				if(billModel.money > 0) {
					_UserData.UpdatePayData(billModel, new Vector2(-4f, -3.62f));
					
					foreach(byte mid in _UserData.UserCandidateMemberIds) {
						if(_SubIndex == mid) {
							_UserData.UserCandidateMemberIds.Remove(mid);
							break;
						}
					}

					SelectMainMenu();
					MissionData.getInstence().AddMissionGoal(MissionGoalType.HQ_GetMember_Start, 1);
				}
			}

		} else if (_MainIndex == 1) {
			// 부대원 해고.
			Member member = _UserData.GetMemberById((byte)(_SubIndex));
			if(member.DefaultId > 1) {
				ClassModel classModel = _MemberData.GetClassModelByClassId(member.ClassId);
				billModel = new BillModel();
				billModel.money = (int)(classModel.scoutCost / 2);
				billModel.moneyPlus = true;
				_UserData.SetConfirmPop(GetStringByScriptId(150125), MemberFire, billModel);
				if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("RmoveMember", member.DefaultId.ToString());

			} else {
				_UserData.SetAlert(GetStringByScriptId(150130), new BillModel());
			}

		}

		GuideArrowManager.getInstence().UpdateArrow();
	}

	/** 신규 맴버 획득 확인 팝업 */
	private void ShowMemberDataPopup(short memberId) {
		_MemberDataUIPop = Instantiate(Resources.Load<GameObject>("Common/MemberDataPop")) as GameObject;
		_MemberDataUIPop.name = "MemberDataPop";
		_MemberDataUIPop.GetComponent<MemberDataPop>().init(memberId, 150);
	}

	private void OnDataBtn2Click(int id) {
		if (_MainIndex == 2) {	// 대원 수색.
			/*
			BillModel billModel = new BillModel();
			billModel.core = _SystemData.MemberSearchCore;
			billModel.corePlus = false;

			if(_UserData.UserCores < _SystemData.MemberSearchCore) {
				_UserData.SetAlert(GetStringByScriptId(150109), billModel);
				return;
			}

			_UserData.SetConfirmPop(GetStringByScriptId(150126), MemberDetailSearch, billModel);
			*/
		}
		if (_MainIndex == 1) {	// 대원 관리.
			Member member = _UserData.GetMemberById((byte)(_SubIndex));

			if(member.CurrentHP == member.MaxHP && member.CurrentMP == member.MaxMP) return;

			BillModel billModel = new BillModel();
			if(member.state == MemberStateType.Wound) {	// 부상 치료.
				ClassModel classModel = _MemberData.GetClassModelByClassId(member.ClassId);
				billModel.moneyPlus = false;
				billModel.money = classModel.healCost;
				billModel.core = classModel.healChipCount;
				billModel.corePlus = false;
				if(_UserData.UserMoney >= billModel.money && _UserData.UserCores >= billModel.core) {
					_UserData.SetConfirmPop(GetStringByScriptId(150128), SetMemberHeal, billModel);
				} else {
					_UserData.SetAlert(GetStringByScriptId(150109), billModel);
				}

			} else if(member.state == MemberStateType.Mia) {	// 대원 회복.
				billModel.core = _SystemData.MemberRescueCore;
				billModel.corePlus = false;
				_UserData.SetConfirmPop(GetStringByScriptId(150129), SetMemberHeal, billModel);
			} else {
				// 회복
				ClassModel classModel = _MemberData.GetClassModelByClassId(member.ClassId);
				billModel.core = _SystemData.MemberHealCore;
				billModel.corePlus = false;
				billModel.money = _SystemData.MemberHealMoney;
				billModel.moneyPlus = false;
				if(_UserData.UserMoney >= billModel.money && _UserData.UserCores >= billModel.core) {
					_UserData.SetConfirmPop(GetStringByScriptId(150127), SetMemberHeal, billModel);
				} else {
					_UserData.SetAlert(GetStringByScriptId(150109), billModel);
				}
			}

		}
	}

	// 대원 탐색. 
	private void SetMemberSearch(int id) {
		BillModel billModel = new BillModel();

		if(id == 0) {
			billModel.money = _SystemData.NormalSearchMoney;
			billModel.moneyPlus = false;
			if(_UserData.UserMoney < _SystemData.NormalSearchMoney) {
				_UserData.SetAlert(GetStringByScriptId(150003), billModel);
				return;
			}
			_UserData.SetConfirmPop(GetStringByScriptId(150163), MemberNormalSearch, billModel);
		} else {
			billModel.core = _SystemData.MemberSearchCore;
			billModel.corePlus = false;

			if(_UserData.UserCores < _SystemData.MemberSearchCore) {
				_UserData.SetAlert(GetStringByScriptId(150109), billModel);
				return;
			}
			_UserData.SetConfirmPop(GetStringByScriptId(150126), MemberDetailSearch, billModel);
		}
	}

	// 대원 회복 ====================================================================
	private void SetMemberHeal(bool isconfirm) {
		if(isconfirm) {
			BillModel billModel = new BillModel();
			Member member = _UserData.GetMemberById((byte)(_SubIndex));
			if(member.state == MemberStateType.Wound) {	// 부상 치료.
				ClassModel classModel = _MemberData.GetClassModelByClassId(member.ClassId);
				_UserData.UserCores -= classModel.healChipCount;
				_UserData.UserMoney -= classModel.healCost;
				billModel.money = classModel.healCost;
				billModel.moneyPlus = false;
				billModel.core = classModel.healChipCount;
				billModel.corePlus = false;

				member.CurrentHP = 10;
				member.state = MemberStateType.Ready;

				if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("HealMember-Wound", member.ClassId.ToString());

			} else if(member.state == MemberStateType.Mia) {	// 대원 실종.
				_UserData.UserCores -= _SystemData.MemberRescueCore;
				member.CurrentHP = 10;
				member.state = MemberStateType.Ready;
				billModel.core = _SystemData.MemberRescueCore;
				billModel.corePlus = false;

				if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("HealMember-MIA", member.ClassId.ToString());

			} else {
				// 회복
				ClassModel classModel = _MemberData.GetClassModelByClassId(member.ClassId);
				member.CurrentHP = member.MaxHP;
				member.CurrentMP = member.MaxMP;

				billModel.money = _SystemData.MemberHealMoney;
				billModel.moneyPlus = false;
				billModel.core = _SystemData.MemberHealCore;
				billModel.corePlus = false;

				_UserData.UserMoney -= billModel.money;
				_UserData.UserCores -= billModel.core;

				if(_UserData.StoryStepId == 12) {	// 튜터리얼 용.
					MainBtnList[2].GetComponent<CommonBtn>().SetEnabled(true);	// 대원 모집.
				}

				if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("CareMember", member.ClassId.ToString());
				MissionData.getInstence().AddMissionGoal(MissionGoalType.HQ_CareMember_Start, 1);

			}

			_UserData.UpdatePayData(billModel, new Vector2(0f, 0f));

			LocalData.getInstence().UserMemberDataSave();
			LocalData.getInstence().UserResourceSave();

			SelectMainMenu();

		}
	}
	// 대원 회복 ====================================================================

	// 대원 치료 ====================================================================
	private void SetMemberRescue(bool isconfirm) {
		Member member = _UserData.GetMemberById((byte)(_SubIndex));
		_UserData.UserCores -= _SystemData.MemberHealCore;
		member.CurrentHP = 10;

		SelectMainMenu();
	}
	// 대원 치료 ====================================================================

	// 대원 정밀 수색. =============================================================
	private void MemberDetailSearch(bool isconfirm) {
		print("MemberDetailSearch");
		if(isconfirm) {
			SearchImg.renderer.enabled = true;
			SearchImg.renderer.sortingOrder = 20;
			Color color = Color.white;
			color.a = 0f;
			SearchImg.renderer.material.color = color;
			iTween.ColorTo(SearchImg, iTween.Hash("a", 1f, "oncomplete", "SearchIconMove", "oncompletetarget", this.gameObject));
			SearchImg.GetComponent<BoxCollider>().enabled = true;

		}
	}

	// 대원 일반 수색. =============================================================
	private void MemberNormalSearch(bool isconfirm) {
		if(isconfirm) {
			SearchIconMoveEnd(0);
		}
	}

	private void SearchIconMove() {
		SearchIcon.renderer.enabled = true;
		SearchIcon.renderer.sortingOrder = 21;
		if(_SearchIconMoveCount == 0) {
			Color color = Color.white;
			color.a = 0f;
			SearchIcon.renderer.material.color = color;
			iTween.ColorTo(SearchIcon, iTween.Hash("a", 1f));
		}

		float xPos = (UnityEngine.Random.Range(0, 10) / 10f) - 1f;
		float yPos = (UnityEngine.Random.Range(0, 10) / 10f) - 1f;
		if(_SearchIconMoveCount < 5) {
			iTween.MoveTo(SearchIcon, iTween.Hash("x", xPos, "y", yPos, "oncomplete", "SearchIconMove", "oncompletetarget", this.gameObject));
			_SearchIconMoveCount ++;
		} else {
			iTween.MoveTo(SearchIcon, iTween.Hash("x", xPos, "y", yPos, "oncomplete", "SearchIconMoveEnd", "oncompletetarget", this.gameObject
			                                      , "oncompleteparams", (byte)(1)));
			iTween.ColorTo(SearchImg, iTween.Hash("a", 0f));
			iTween.ColorTo(SearchIcon, iTween.Hash("a", 0f));
			_SearchIconMoveCount = 0;
		}
	}

	private void SearchIconMoveEnd(byte type) {
		print("SearchIconMoveEnd : " + type);

		BillModel billModel = new BillModel();

		SearchImg.GetComponent<BoxCollider>().enabled = false;

		if(type == 0) {
			billModel.money = _SystemData.NormalSearchMoney;
			billModel.moneyPlus = false;

			_MemberData.GetRandomMemberidsByCore(1);
			_UserData.UserMoney -= _SystemData.NormalSearchMoney;
		} else {
			billModel.core = _SystemData.MemberSearchCore;
			billModel.corePlus = false;

			_MemberData.GetRandomMemberidsByCore(_SystemData.MemberSearchCore);
			_UserData.UserCores -= _SystemData.MemberSearchCore;
		}

		_UserData.UpdatePayData(billModel, new Vector2(0f, 0f));

		if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("ExploreMember", _UserData.UserCandidateMemberIds.Count.ToString());

		SelectMainMenu();

	}

	private void ShowMemberComment(string comment) {

		if(_MemberCommentObj == null) {
			_MemberCommentObj = SetAddTextMesh(comment, 16, TextAnchor.UpperLeft, Color.white);
			_MemberCommentObj.GetComponent<TextMesh>().alignment = TextAlignment.Left;

		} else {
			_MemberCommentObj.GetComponent<TextMesh>().text = comment;
		}
		_MemberCommentObj.transform.position = new Vector2(1.28f, 3.51f);
		_MemberCommentObj.transform.parent = this.gameObject.transform;
	}

	private void HideMemberComment() {
		Destroy(_MemberCommentObj);
		_MemberCommentObj = null;
	}

	private GameObject SetAddTextMesh(string str, int fontSize, TextAnchor anchor, Color color) {
		GameObject targetobj = Instantiate(Resources.Load<GameObject>("DefaultFont")) as GameObject;
		//targetobj.layer = LayerMask.NameToLayer("Alert");
		TextMesh textMesh = targetobj.GetComponent<TextMesh>();
		textMesh.text = str;
		textMesh.anchor = anchor;
		textMesh.color = color;
		textMesh.fontSize = fontSize;
		targetobj.renderer.sortingOrder = 10;
		
		return targetobj;
	}



	// 대원 정밀 수색. =============================================================

	/** 대원을 해고 함. */
	private void MemberFire(bool isConfirm) {
		if(isConfirm == false) return;
		Member member = _UserData.GetMemberById((byte)(_SubIndex));
		ClassModel classModel = _MemberData.GetClassModelByClassId(member.ClassId);
		// 자산 보충. 
		BillModel billModel = new BillModel();
		billModel.money = (int)(classModel.scoutCost / 2);
		billModel.moneyPlus = true;

		_UserData.UserMoney += billModel.money;
		_UserData.UpdatePayData(billModel, new Vector2(0f, 0f));

		// 장비 초기화
		_UserData.ResetMemberGears(member.id);

		_UserData.RemoveMember((byte)(_SubIndex));
		SelectMainMenu();
	}

	/** 대원의 상태를 갱신함. 부상, 구조, 회복 */
	private void MemberStateUpdate(bool isConfirm) {
		Member member = _UserData.GetMemberById((byte)(_SubIndex));
	}

	private void OnScreenUpate() {
		if(_MainIndex == 0) {
			SelectSubMenu2();
		}
	}

	public class MemberOutListMPSort : IComparer
	{
		public int Compare(object x, object y)
		{
			// reverse the arguments
			int g1 = ((Member)x).CurrentMP;
			int g2 = ((Member)y).CurrentMP;
			
			if (g1 < g2)
				return -1;
			else
				return 0;
		}
		
	}
	
	public class MemberOutListHPSort : IComparer
	{
		public int Compare(object x, object y)
		{
			// reverse the arguments
			int g1 = ((Member)x).CurrentHP;
			int g2 = ((Member)y).CurrentHP;
			
			if (g1 < g2)
				return -1;
			else
				return 0;
		}
		
	}

	public class MemberOutListMPAndHPSort : IComparer
	{
		public int Compare(object x, object y)
		{
			int hpGamx = ((Member)x).MaxHP - ((Member)x).CurrentHP;
			int mpGamx = ((Member)x).MaxMP - ((Member)x).CurrentMP;
			int totalGamX = hpGamx + mpGamx;
			int hpGamy = ((Member)y).MaxHP - ((Member)y).CurrentHP;
			int mpGamy = ((Member)y).MaxMP - ((Member)y).CurrentMP;
			int totalGamY = hpGamy + mpGamy;
			if (totalGamX > totalGamY)
				return -1;
			else
				return 0;
		}
		
	}

	public class MemberDefaultIdSort : IComparer
	{
		public int Compare(object x, object y)
		{
			// reverse the arguments
			int g1 = ((Member)x).DefaultId;
			int g2 = ((Member)y).DefaultId;
			
			if (g1 < g2)
				return -1;
			else
				return 0;
		}
		
	}

	public class MemberClassSort : IComparer
	{
		public int Compare(object x, object y)
		{
			// reverse the arguments
			int g1 = ((Member)x).ClassId;
			int g2 = ((Member)y).ClassId;
			
			if (g1 > g2)
				return -1;
			else
				return 0;
		}
		
	}

}
