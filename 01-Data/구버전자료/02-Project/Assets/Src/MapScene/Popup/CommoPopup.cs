using UnityEngine;
using System.Collections;
using System;

public class CommoPopup : MonoBehaviour {

	public bool isTest;
	public GameObject CloseBtn;
	public GameObject MainMenu1;
	public GameObject MainMenu2;
	public GameObject MainMenu3;
	public GameObject ListMenu;
	public GameObject StartBtn;
	public GameObject DetailView;
	public int SortingNum;

	public Color DefaultBtnColor;
	public Color TitleColor;
	public Color TitleOutLineColor;

	private OutLineFont _DataTitleFont;
	private OutLineFont _SubTitleText;

	public delegate void CloseEvent();
	private CloseEvent _CloseCallback;
	public delegate void FightStartEvent(int id, bool isAirship);
	private FightStartEvent _FightStartEventFunc;

	private GameObject[] _MainMenuObjList;
	private string[] _MainMenuNames;
	private byte _CurrentMainMenuNum = 0;
	private byte _CurrentSelectTownId = 0;

	private ScrollMenu _ScrollMenu;
	private GameObject _RewardViewerObj;

	private ListMenuModel[] _TownListData;
	private ListMenuModel[] _GhostListData;

	private WarzoneData _WarzoneData;
	private ScriptData _ScriptData;

	private float _Timer = 0;

	void Awake() {
		if(isTest) {
			LocalData.getInstence().AllLoad();
			init();
		}
	}

	void Update() {
		_Timer += Time.deltaTime;
		if(_Timer > 1) {
			ShowDataView();
			_Timer = 0;
		}
	}

	public void init() {

		_ScriptData = ScriptData.getInstence();
		_WarzoneData = WarzoneData.getInstence();

		GameObject DataTitleObj = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
		_DataTitleFont = DataTitleObj.GetComponent<OutLineFont>();
		_DataTitleFont.SetString("");
		_DataTitleFont.SetAlign(TextAnchor.MiddleLeft);
		DataTitleObj.transform.parent = this.gameObject.transform;
		DataTitleObj.transform.position = new Vector2(-6.98f, 0.31f);
		_DataTitleFont.SetFontSize(22);
		_DataTitleFont.SetLineSize(2);
		TitleOutLineColor.a = 1f;
		_DataTitleFont.SetLineColor(TitleOutLineColor);

		_MainMenuNames = new string[]{_ScriptData.GetGameScript(120200).script, _ScriptData.GetGameScript(120201).script
			, _ScriptData.GetGameScript(120202).script};
		
		CloseBtn.GetComponent<ButtonEvent>().SetCallBack(OnPopupClose);

		_ScrollMenu = ListMenu.GetComponent<ScrollMenu>();
		_ScrollMenu.SetMenuClick(OnTownClick);

		_MainMenuObjList = new GameObject[]{MainMenu1, MainMenu2, MainMenu3};

		CommonBtn commonBtn;
		int btnNum = 0;
		foreach(GameObject btnObj in _MainMenuObjList)
		{
			commonBtn = btnObj.GetComponent<CommonBtn>();
			commonBtn.Init(btnNum, _MainMenuNames[btnNum], SortingNum, DefaultBtnColor);
			commonBtn.SetClick(OnMainMenuClick);

			int storyStep = UserData.getInstence().StoryStepId;

			if(btnNum == 1 && storyStep < 15) btnObj.GetComponent<CommonBtn>().SetEnabled(false);
			if(btnNum == 2 && storyStep < 15) btnObj.GetComponent<CommonBtn>().SetEnabled(false);
			//if(btnNum == 3 && storyStep < 4) btnObj.GetComponent<CommonBtn>().SetEnabled(false);

			btnNum ++;
		}

		GameObject TitleTxtObj = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
		TitleTxtObj.name = "titleTxt";
		TitleTxtObj.GetComponent<OutLineFont>().SetString("");
		TitleTxtObj.GetComponent<OutLineFont>().SetFontSize(24);
		TitleTxtObj.GetComponent<OutLineFont>().SetLineSize(1.5f);
		TitleTxtObj.GetComponent<OutLineFont>().SetAlign(TextAnchor.MiddleLeft);
		TitleColor.a = 1;
		TitleTxtObj.GetComponent<OutLineFont>().SetFontColor(TitleColor);
		TitleOutLineColor.a = 1;
		TitleTxtObj.GetComponent<OutLineFont>().SetLineColor(TitleOutLineColor);
		TitleTxtObj.transform.parent = this.gameObject.transform;
		TitleTxtObj.transform.position = new Vector2(1.24f, 4.19f);
		_SubTitleText = TitleTxtObj.GetComponent<OutLineFont>();

		_RewardViewerObj = Instantiate(Resources.Load<GameObject>("Common/RewardViewer")) as GameObject;
		_RewardViewerObj.transform.parent = this.gameObject.transform;

		StartBtn.GetComponent<CommonBtn>().SetClick(OnStartClick);

		SelectMainMenu(_CurrentMainMenuNum);

	}

	public void SetCloseEventCallBack(CloseEvent onCloseEvent) {
		_CloseCallback = new CloseEvent(onCloseEvent);
		
	}

	public void SetFightStartEventCallBack(FightStartEvent onFightStartEvent) {
		_FightStartEventFunc = new FightStartEvent(onFightStartEvent);
		
	}

	private void OnPopupClose() {
		if(_CloseCallback != null) _CloseCallback();
	}

	private void OnMainMenuClick(int id) {
		SelectMainMenu((byte)(id));
		if(id == 0) {	// 마을 방어.
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Common_TownDefense_Click, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.CommoPopup);
		} else if (id == 1) {	// 둥지 공격.
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Common_AttackGhost_Click, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.CommoPopup_GhostAttack);
		} else {	// 항공기 보호.
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Common_AircraftDefense_Click, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.CommoPopup_Aircraft);
		}
	}

	private void SelectMainMenu(byte num) {

		_TownListData = null;
		short townListCount = 0;
		_CurrentSelectTownId = 0;
		if(num == 0) {	// 마을 리스트
			ArrayList UserTownList = _WarzoneData.GetInvasionTownList();
			_TownListData = new ListMenuModel[UserTownList.Count];

			foreach(UserTown userTown in UserTownList) {
				_TownListData[townListCount] = new ListMenuModel();
				_TownListData[townListCount].id = userTown.id;
				_TownListData[townListCount].scriptId = _WarzoneData.GetDefaultTownData(userTown.id).townNameId;

				townListCount ++;
			}

			if(_TownListData.Length > 0) _CurrentSelectTownId = (byte)(_TownListData[0].id);

		} else if (num == 1) {	// 둥지 리스트.
			_TownListData = new ListMenuModel[0];
			ArrayList GhostTownList = _WarzoneData.GetShowGhostTownList();
			_TownListData = new ListMenuModel[GhostTownList.Count];

			//GhostTown[] tghostTownList = GhostTownList.ToArray() as GhostTown[];
			//print("ghostTownList : " + tghostTownList);
			//tghostTownList = tghostTownList.OrderBy(GhostTown => GhostTown.id).ToArray();
			GhostTownList.Sort(new GhostTownSort());

			foreach(GhostTown ghostTown in GhostTownList) {
				_TownListData[townListCount] = new ListMenuModel();
				_TownListData[townListCount].id = ghostTown.id;
				_TownListData[townListCount].scriptId = _WarzoneData.GetDefaultTownData(ghostTown.id).townNameId;
				//string townName = _ScriptData.GetGameScript(_WarzoneData.GetDefaultTownData(ghostTown.id).townNameId).script;
				//_TownListData[townListCount].scriptString = townName + " : " + ghostTown.id;

				townListCount ++;
			}

			if(_TownListData.Length > 0) _CurrentSelectTownId = (byte)(_TownListData[0].id);

		} else {
			_TownListData = new ListMenuModel[_WarzoneData.GetOpenAirshipDefense().Count];

			foreach(AirshipDefense airshipDefense in _WarzoneData.GetOpenAirshipDefense()) {
				_TownListData[townListCount] = new ListMenuModel();
				_TownListData[townListCount].id = airshipDefense.id;
				_TownListData[townListCount].scriptId = airshipDefense.ScriptId;

				townListCount ++;
			}

			if(_TownListData.Length > 0) _CurrentSelectTownId = (byte)(_TownListData[0].id);
		}

		if(_TownListData.Length > 0) _WarzoneData.CurrentTownId = (byte)_TownListData[0].id;

		_MainMenuObjList[_CurrentMainMenuNum].GetComponent<CommonBtn>().SetBtnSelect(false);
		_CurrentMainMenuNum = num;
		_MainMenuObjList[_CurrentMainMenuNum].GetComponent<CommonBtn>().SetBtnSelect(true);

		_SubTitleText.SetString(_MainMenuNames[_CurrentMainMenuNum]);
		_ScrollMenu.SetScrollData(_TownListData, 0);
		_ScrollMenu.SetScrollView();

		if(_TownListData.Length > 0) {
			StartBtn.GetComponent<CommonBtn>().btnId = _TownListData[0].id;
		} else {
			StartBtn.GetComponent<CommonBtn>().btnId = 0;
		}

		ShowDataView();
	}

	private void ShowDataView() {

		TextMesh DataTxt1 = DetailView.transform.FindChild("DataTxt1").GetComponent<TextMesh>();
		TextMesh DataTxt2 = DetailView.transform.FindChild("DataTxt2").GetComponent<TextMesh>();
		TextMesh DataTxt3 = DetailView.transform.FindChild("DataTxt3").GetComponent<TextMesh>();
		TextMesh DataTxt4 = DetailView.transform.FindChild("DataTxt4").GetComponent<TextMesh>();

		DataTxt1.text = "";
		DataTxt2.text = "";
		DataTxt3.text = "";
		DataTxt4.text = "";
		_DataTitleFont.SetString("");

		BillModel billModel = new BillModel();
		if(_CurrentSelectTownId == 0) {
			ShowGhostThumb(0);
			_RewardViewerObj.GetComponent<RewardViewer>().init(billModel, 10, true);
			return;
		}

		Town defaultTown = null;
		if(_CurrentMainMenuNum == 0) {	// 마을 정보.
			UserTown userTown = _WarzoneData.GetUserTownByID(_CurrentSelectTownId);
			if(userTown == null) {
				ShowGhostThumb(0);
				_RewardViewerObj.GetComponent<RewardViewer>().init(billModel, 10, true);
				return;
			}
			long lastTime = userTown.lastInvasionEndTime - SystemData.GetInstance().getCurrentTime();
			if(lastTime < 0) return;

			defaultTown = _WarzoneData.GetDefaultTownData(_CurrentSelectTownId);
			_DataTitleFont.SetString(_ScriptData.GetGameScript(defaultTown.townNameId).script);
			DataTxt1.text = _ScriptData.GetGameScript(160108).script + " : " + userTown.resident;
			string damageComment = _ScriptData.GetGameScript(160162).script;
			DataTxt2.text = _ScriptData.GetGameScript(160155).script + " (" + damageComment + ":" + (userTown.invasionGhostClose / 2) + ")";
			ShowGhostThumb(userTown.invasionGhostClose);
			DataTxt3.text = _ScriptData.GetGameScript(150118).script + " " + _ScriptData.GetGameScript(160109).script;
			string lastTimeStr = SystemData.GetInstance().GetTimeStrByTime(lastTime);
			DataTxt4.text = lastTimeStr;

		} else if (_CurrentMainMenuNum == 1) {	// 둥지 정보.
			GhostTown ghostTown = _WarzoneData.GetGhostTownByTownId(_CurrentSelectTownId);
			defaultTown = _WarzoneData.GetDefaultTownData(_CurrentSelectTownId);
			ShowGhostThumb(defaultTown.maxClose);
			_DataTitleFont.SetString(_ScriptData.GetGameScript(defaultTown.townNameId).script);
			DataTxt1.text = _ScriptData.GetGameScript(160145).script + " : " + ghostTown.ghostClose;
			DataTxt2.text = _ScriptData.GetGameScript(160155).script;

		} else if (_CurrentMainMenuNum == 2) {	// 항공기 보호 정보.
			AirshipDefense airshipDefense = _WarzoneData.GetAirshipDefenseById(_CurrentSelectTownId);
			_DataTitleFont.SetString(_ScriptData.GetGameScript(airshipDefense.ScriptId).script);
			ShowGhostThumb(airshipDefense.ghostClose);
			DataTxt1.text = _ScriptData.GetGameScript(160145).script + " : " + airshipDefense.ghostClose;
			DataTxt2.text = _ScriptData.GetGameScript(160155).script;
			DataTxt3.text = _ScriptData.GetGameScript(160153).script;
			billModel.money = airshipDefense.rewardCost;
			billModel.moneyPlus = true;
			_RewardViewerObj.transform.position = new Vector3(-3.82f, -3.23f, 0f);
		}

		_RewardViewerObj.transform.localScale = new Vector3(1f, 1f, 1f);
		_RewardViewerObj.GetComponent<RewardViewer>().init(billModel, 10, true);
		_RewardViewerObj.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

		if(DarkSprite.getInstence() != null && defaultTown != null) {
			if(DarkSprite.getInstence().MainMapData != null) DarkSprite.getInstence().MainMapData.UpdateTownMove(defaultTown.id);
		}
	}

	private void ShowGhostThumb(short close) {

		ArrayList ghostList = null;
		if(close > 0) {
			ghostList = _WarzoneData.GetGhostDataByGhostClose(close);
		} 

		string thumbUri = "";
		SpriteRenderer ghostThumb;
		for(int i = 0; i < 5; i++) {
			thumbUri = "";
			if(ghostList != null && (ghostList.Count - 1) >= i) {
				byte ghostId = (byte)(ghostList[i]);
				Ghost ghost = _WarzoneData.GetGhostByGhostId(ghostId);
				DefaultGhost defaultGhost = _WarzoneData.GetDefaultGhostByGhostId(ghost.defaultId);
				if(defaultGhost != null) thumbUri = defaultGhost.resourceURI;

			}

			ghostThumb = DetailView.transform.FindChild("ghost0" + (i + 1)).GetComponent<SpriteRenderer>();
			ghostThumb.sprite = Resources.Load<Sprite>("GhostImg/Thumbnail/" + thumbUri);
		}

	}

	private void OnStartClick(int id) {
		if(id == 0) return;
		if(_FightStartEventFunc != null) {
			if(_CurrentMainMenuNum == 2) {
				_FightStartEventFunc(id, true);
			} else {
				_FightStartEventFunc(id, false);
			}
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Common_OperationStart_Click, 1);
		} 
	}

	private void OnTownClick(int id) {
		StartBtn.GetComponent<CommonBtn>().btnId = id;
		_CurrentSelectTownId = (byte)(id);
		ShowDataView();
	}

	public class GhostTownSort : IComparer
	{
		public int Compare(object x, object y)
		{
			// reverse the arguments
			byte g1 = ((GhostTown)x).id;
			byte g2 = ((GhostTown)y).id;

			if (g1 < g2)
				return -1;
			else
				return 0;
		}
		
	}

}
