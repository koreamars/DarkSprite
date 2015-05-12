using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;

public class MainScene : MonoBehaviour {

	public GameObject BlackBox;
	public GameObject PopupBg;
	public GameObject PopupTownMask;
	public GameObject GameMessage;
	public GameObject MissionView;

	public AudioSource MainBGM;
	public AudioSource PopupClickAudio;

	private short _PopupType = 0;

	private float _PopupY = 0f;
	
	private MainMap _MainMap;
	private GameObject _GamedataView;
	private GameObject _GameMenu;

	private GameObject _HQPanel;
	private GameObject _CommonPanel;
	private GameObject _ResearchPanel;
	private GameObject _FactoryPanel;
	private GameObject _CorpsPanel;
	private GameObject _HangarPanel;
	private GameObject _ShopPanel;
	private GameObject _OptionPanel;

	private Vector2 _SymbolCenter;

	private GameObject _UnitSelectPopup;

	private DarkSprite _DarkSprite;
	private UserData _UserData;
	private WarzoneData _WarzoneData;

	private Camera _ResearchCamera;

	private bool _isAirship;

	private GameObject _NPCObject;
	private GameObject _StoryChargeObj;
	private GameObject _StoryPopupObj;

	private GameObject _LoadingMark;
	private float _OrthographicSize = 5f;

	void Awake () {

		System.GC.Collect();

		_DarkSprite = DarkSprite.getInstence();

		float windowScale = (float)(Screen.width) / (float)(Screen.height);

		if(windowScale < 1.5f) {
			Camera UICamera = GameObject.Find("UICamera").camera;
			Camera ResearchCamera = GameObject.Find("ResearchCamera").camera;
			_OrthographicSize = 5f * (2.5f - windowScale);
			UICamera.orthographicSize = _OrthographicSize;

		}
		print("UICamera.orthographicSize : " + windowScale);
	}

	void Start () {

		if(GoogleAnalytics.instance) GoogleAnalytics.instance.appVersion = SystemData.BuildVersion;

		if(SystemData.GetInstance().GameServiceType == ServiceType.ALPHA || SystemData.GetInstance().GameServiceType == ServiceType.BETA) {
			if(GoogleAnalytics.instance) GoogleAnalytics.instance.propertyID = SystemData.GetInstance().TestGoogleId;
		}

		_isAirship = false;

		_WarzoneData = WarzoneData.getInstence();
		SystemData.GetInstance();

		_LoadingMark = SystemData.GetInstance().GetLoadingMark();

		SystemData.GetInstance().SystemMessage = GameMessage;
		SystemData.GetInstance().IsMainBtnClick = false;
		MemberData.getInstence();
		_UserData = UserData.getInstence();

		StoryData.getInstence();

		_UserData.FristUpdate(init);
		//init();

		SystemData.GetInstance().HideBanner();
	}

	private void init() {
		_WarzoneData.CurrentFightType = FightType.Idle;
		
		
		GameObject classObj = new GameObject();
		_MainMap = classObj.AddComponent<MainMap>();
		
		_GamedataView = Instantiate(Resources.Load<GameObject>("MainScene/GameDataView")) as GameObject;
		_GamedataView.transform.position = new Vector2(0, _OrthographicSize);
		_GameMenu = Instantiate(Resources.Load<GameObject>("MainScene/GameMenu")) as GameObject;
		_GameMenu.transform.position = new Vector2(0, -4.2f);
		_GameMenu.GetComponent<GameMenu>().SetCallBack(OpenPopup);
		_GameMenu.GetComponent<GameMenu>().DataUpdate();
		_GamedataView.GetComponent<GameDataView>().SetPopupOpenEvent(OpenPopup);
		
		_DarkSprite.GameDataView = _GamedataView;
		_DarkSprite.MainMapData = _MainMap;
		_DarkSprite.MainScene = this;
		
		_SymbolCenter = new Vector2(-3f, 1f);
		
		GameObject.Find("CharacterCamera").GetComponent<Camera>().enabled = false;
		_ResearchCamera = GameObject.Find("ResearchCamera").GetComponent<Camera>();
		float scale = _OrthographicSize - 4f;
		print("scale : " + scale);
		_ResearchCamera.orthographicSize = _OrthographicSize;
		_ResearchCamera.rect = new Rect(0.46f, 0f, 0.48f, 1f);
		
		_DarkSprite.GameDataView.GetComponent<GameDataView>().UpdateUserData();
		_ResearchCamera.enabled = false;
		
		if(_UserData.Option_BGM) {
			MainBGM.Play();
			MainBGM.loop = true;
		}

		iTween.ColorTo(PopupBg, iTween.Hash("a", 0, "time", 0f));
		iTween.ColorTo(PopupTownMask, iTween.Hash("a", 0, "time", 0f));
		iTween.ColorTo(BlackBox, iTween.Hash("a", 0, "time", 1f, "oncomplete", "OnSceneComplete", "oncompletetarget", this.gameObject));

		Destroy(_LoadingMark);
	}


	private void OnSceneComplete() {
		//StoryData.getInstence().UpdateStoryStep(SceneType.MainScene);
		if(SystemData.GetInstance().firstStoryShow == false) {
			SystemData.GetInstance().firstStoryShow = true;
			// 메인 미션 체크. (튜터리얼 용)
			if(UserData.getInstence().StoryStepId >= 3 && UserData.getInstence().StoryStepId <= 4) {
				UserTown userTown = WarzoneData.getInstence().GetUserTownByID(1);
				userTown.isInvasion = true;
				userTown.lastInvasionEndTime = SystemData.GetInstance().getCurrentTime();
				long invasionTime = (SystemData.GetInstance().TownInvasionDelay * SystemData.GetInstance().millisecondNum);
				userTown.lastInvasionEndTime += invasionTime;
				GhostTown attackGhostTown = WarzoneData.getInstence().GetGhostTownByRootId(userTown.id, false);
				userTown.invasionGhostClose = 100;
				userTown.invasionGhostTownId = attackGhostTown.id;
				_MainMap.UpdateTownSymbols();
				LocalData.getInstence().UserTownDataSave();
			}
			// 데이터 보정 기능.
			if(UserData.getInstence().StoryStepId <= 0) {

				GoogleAnalytics.instance.LogEvent("Error", "Main StoryStepId Error");

				DefaultMission defaultMission;
				foreach(UserMission userMission in _UserData.UserMissionList) {
					defaultMission = MissionData.getInstence().GetDefaultMission(userMission.defaultMissionId);
					if(defaultMission != null) {
						if(defaultMission.StartStoryStep > 0 && defaultMission.StartStoryStep < 100000) {
							UserData.getInstence().StoryStepId = defaultMission.StartStoryStep;
						}
						if(defaultMission.EndStoryStep > 0 && defaultMission.EndStoryStep < 100000) {
							UserData.getInstence().StoryStepId = defaultMission.EndStoryStep;
						} 
					}
				}
				LocalData.getInstence().UserStoryStepSave();
			}
		} 

		GuideArrowManager.getInstence().ShowArrow(SceneType.MainScene);
		MissionData.getInstence().AddMissionGoal(MissionGoalType.MainScene_Appear, 1);

		float rePosition = 0f;
		if(_OrthographicSize > 5f) rePosition = _OrthographicSize - 5f;
		float missionViewX = 0f - rePosition;
		MissionView.transform.position = new Vector2(missionViewX, _OrthographicSize - 5f);
		MissionData.getInstence().ShowUpdateMissionView();

		// 1막3장 종료시 과금 팝업 유무.
		if(_UserData.StoryStepId >= 49 && SystemData.GetInstance().IsStoryCharge == false) {
			_StoryChargeObj = new GameObject();
			_StoryChargeObj.name = "StoryOpenIcon";
			_StoryChargeObj.AddComponent<SpriteRenderer>();
			_StoryChargeObj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Common/Shop/StoryOpenIcon");
			_StoryChargeObj.transform.localScale = new Vector2(1.4f, 1.4f);
			float storyChargeX = SystemData.GetInstance().screenRightX - 1f + rePosition;
			_StoryChargeObj.transform.position = new Vector2(storyChargeX, (3.5f + (_OrthographicSize - 5f)));
			_StoryChargeObj.AddComponent<BoxCollider2D>();
			_StoryChargeObj.AddComponent<ButtonEvent>().SetCallBack(OnStoryChargeObj);
		}

		SystemData.GetInstance().IsMainBtnClick = true;

		return;
		// 메인 미션 체크. (튜터리얼 용)
		/*
		if(SystemData.GetInstance().firstStoryShow == false && UserData.getInstence().StoryStep > 1 && UserData.getInstence().StoryStep < 22) {
			MissionData.getInstence().MainStoryCheck();
			SystemData.GetInstance().firstStoryShow = true;
			if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogScreen("Game Start");

			if(UserData.getInstence().StoryStep >= 2) {
				UserTown userTown = WarzoneData.getInstence().GetUserTownByID(1);
				userTown.isInvasion = true;
				userTown.lastInvasionEndTime = SystemData.GetInstance().getCurrentTime();
				long invasionTime = (SystemData.GetInstance().TownInvasionDelay * SystemData.GetInstance().millisecondNum);
				userTown.lastInvasionEndTime += invasionTime;
				GhostTown attackGhostTown = WarzoneData.getInstence().GetGhostTownByRootId(userTown.id, false);
				userTown.invasionGhostClose = 100;
				userTown.invasionGhostTownId = attackGhostTown.id;
				_MainMap.UpdateTownSymbols();
			}
		}
		*/
	}

	private void OnStoryChargeObj() {
		print("OnStoryChargeObj");
		_StoryPopupObj = Instantiate(Resources.Load<GameObject>("Common/Shop/StoryOpenPopup")) as GameObject;
		_StoryPopupObj.GetComponent<StoryOpenPopup>().SetMainBtnUpdate(MainStoryBtnUpdate);
	}

	private void MainStoryBtnUpdate() {
		if(_StoryChargeObj != null) {
			Destroy(_StoryChargeObj);
		}
	}

	public void GameMenuUpdate() {
		_GameMenu.GetComponent<GameMenu>().DataUpdate();
	}

	/**
	 * 팝업 컨트롤
	 * */
	public void OpenPopup(short type)
	{
		GameLog.Log("type:" + type);

		if(SystemData.GetInstance().IsMainBtnClick == false) return;
		if(_PopupType == type) return;

		//if(_PopupType > 0 && type > 0) return;

		if(type == 0) {
			_GameMenu.transform.position = new Vector2(0, -4.2f);
			_GamedataView.transform.position = new Vector2(0, _OrthographicSize);
			GameMessage.transform.position = new Vector2(0, 0);
			MissionView.transform.position = new Vector2(0, _OrthographicSize - 5f);

			if(_StoryChargeObj != null) {
				_StoryChargeObj.renderer.enabled = true;
				_StoryChargeObj.GetComponent<BoxCollider2D>().enabled = true;
			}

		} else {
			_GameMenu.transform.position = new Vector2(0, -8.2f);
			_GamedataView.transform.position = new Vector2(0, -10f);
			GameMessage.transform.position = new Vector2(0, -10f);
			MissionView.transform.position = new Vector2(0, -10f);

			if(_StoryChargeObj != null) {
				_StoryChargeObj.renderer.enabled = false;
				_StoryChargeObj.GetComponent<BoxCollider2D>().enabled = false;
			}
		}

		GameMenuUpdate();

		prevPopupDestroy();

		_ResearchCamera.enabled = false;


		_PopupType = type;
		switch(type)
		{
		case MainPopupType.HQPopup:
			ShowNPCObj(NPCType.Tassa);
			_MainMap.UpdateTownMove(1);
			_HQPanel = Instantiate(Resources.Load<GameObject>("MainScene/Popup/HQPopup")) as GameObject;
			_HQPanel.transform.position = new Vector2(0, _PopupY);
			_HQPanel.GetComponent<HQPopup>().SetCloseEventCallBack(OnPopupClose);
			_HQPanel.GetComponent<HQPopup>().init();
			PopupOpenMove(_HQPanel);
			iTween.ColorTo(PopupTownMask, iTween.Hash("a", 0.7f, "time", 0.3f));
			break;
		case MainPopupType.CommoPopup:
			ShowNPCObj(NPCType.Ria);
			_MainMap.UpdateTownMove(1);
			_CommonPanel = Instantiate(Resources.Load<GameObject>("MainScene/Popup/CommoPopup")) as GameObject;
			_CommonPanel.transform.position = new Vector2(0, _PopupY);
			_CommonPanel.GetComponent<CommoPopup>().SetCloseEventCallBack(OnPopupClose);
			_CommonPanel.GetComponent<CommoPopup>().SetFightStartEventCallBack(OnFightStartReady);
			_CommonPanel.GetComponent<CommoPopup>().init();
			PopupOpenMove(_CommonPanel);
			iTween.ColorTo(PopupTownMask, iTween.Hash("a", 0.7f, "time", 0.3f));
			break;
		case MainPopupType.ResearchPopup:
			ShowNPCObj(NPCType.Sara);
			_ResearchCamera.enabled = true;
			_MainMap.UpdateScaleMove(_MainMap.GetDefaultScale(), 0f, 0f);
			_ResearchPanel = Instantiate(Resources.Load<GameObject>("MainScene/Popup/ResearchPopup")) as GameObject;
			_ResearchPanel.transform.position = new Vector2(0, _PopupY);
			_ResearchPanel.GetComponent<ResearchPopup>().SetCloseEventCallBack(OnPopupClose);
			_ResearchPanel.GetComponent<ResearchPopup>().init();
			PopupOpenMove(_ResearchPanel);
			iTween.ColorTo(PopupBg, iTween.Hash("a", 0.7f, "time", 0.3f));
			break;
		case MainPopupType.FactoryPopup:
			ShowNPCObj(NPCType.Kity);
			_MainMap.UpdateScaleMove(_MainMap.GetDefaultScale(), 0f, 0f);
			_FactoryPanel = Instantiate(Resources.Load<GameObject>("MainScene/Popup/FactoryPopup")) as GameObject;
			_FactoryPanel.transform.position = new Vector2(0, _PopupY);
			_FactoryPanel.GetComponent<FactoryPopup>().SetCloseEventCallBack(OnPopupClose);
			_FactoryPanel.GetComponent<FactoryPopup>().init();
			PopupOpenMove(_FactoryPanel);
			iTween.ColorTo(PopupBg, iTween.Hash("a", 0.7f, "time", 0.3f));
			break;
		case MainPopupType.CorpsPopup:
			_MainMap.UpdateScaleMove(_MainMap.GetDefaultScale(), 0f, 0f);
			_CorpsPanel = Instantiate(Resources.Load<GameObject>("MainScene/Popup/CorpsPopup")) as GameObject;
			_CorpsPanel.transform.position = new Vector2(0, _PopupY);
			_CorpsPanel.GetComponent<CorpsPopup>().SetCloseEventCallBack(OnPopupClose);
			_CorpsPanel.GetComponent<CorpsPopup>().init();
			PopupOpenMove(_CorpsPanel);
			iTween.ColorTo(PopupBg, iTween.Hash("a", 0.7f, "time", 0.3f));
			break;
		case MainPopupType.FightPopup:
			_MainMap.UpdateScaleMove(_MainMap.GetDefaultScale(), 0f, 0f);
			_UnitSelectPopup = Instantiate(Resources.Load<GameObject>("MainScene/Popup/UnitSelectPopup")) as GameObject;
			_UnitSelectPopup.transform.position = new Vector2(0, _PopupY);
			UnitSelectPopup popup = _UnitSelectPopup.GetComponent<UnitSelectPopup>();

			UserTown userTown = _WarzoneData.GetUserTownByID((byte)(_WarzoneData.CurrentTownId));

			if(_isAirship == false) {
				if(userTown == null) {
					_WarzoneData.CurrentTownType = TownStateType.Nest;
				} else {
					_WarzoneData.CurrentTownType = TownStateType.Town;
				}
			} else {
				_WarzoneData.CurrentTownType = TownStateType.Airship;
			}

			popup.Init(0, 2, OnUnitPopupClose, OnUnitPopupSelect);
			PopupOpenMove(_UnitSelectPopup);
			iTween.ColorTo(PopupBg, iTween.Hash("a", 0.7f, "time", 0.3f));
			break;
		case MainPopupType.HangarPopup:
			_MainMap.UpdateScaleMove(_MainMap.GetDefaultScale(), 0f, 0f);
			_HangarPanel = Instantiate(Resources.Load<GameObject>("MainScene/Popup/HangarPopup")) as GameObject;
			_HangarPanel.transform.position = new Vector2(0, _PopupY);
			_HangarPanel.GetComponent<HangarPopup>().SetCloseEventCallBack(OnPopupClose);
			StartCoroutine(_HangarPanel.GetComponent<HangarPopup>().init());
			PopupOpenMove(_HangarPanel);
			iTween.ColorTo(PopupBg, iTween.Hash("a", 0.7f, "time", 0.3f));
			break;
		case MainPopupType.ShopPopup:
			_MainMap.UpdateScaleMove(_MainMap.GetDefaultScale(), 0f, 0f);
			_ShopPanel = Instantiate(Resources.Load<GameObject>("MainScene/Popup/ShopPopup")) as GameObject;
			_ShopPanel.transform.position = new Vector2(0, _PopupY);
			_ShopPanel.GetComponent<ShopPopup>().SetCloseEventCallBack(OnPopupClose);
			_ShopPanel.GetComponent<ShopPopup>().init();
			PopupOpenMove(_ShopPanel);
			iTween.ColorTo(PopupBg, iTween.Hash("a", 0.7f, "time", 0.3f));
			break;
		case MainPopupType.OptionPopup:
			_MainMap.UpdateScaleMove(_MainMap.GetDefaultScale(), 0f, 0f);
			_OptionPanel = Instantiate(Resources.Load<GameObject>("MainScene/Popup/OptionPopup")) as GameObject;
			_OptionPanel.transform.position = new Vector2(0, _PopupY);
			_OptionPanel.GetComponent<OptionPopup>().SetCloseEventCallBack(OnPopupClose);
			_OptionPanel.GetComponent<OptionPopup>().init();
			PopupOpenMove(_OptionPanel);
			iTween.ColorTo(PopupBg, iTween.Hash("a", 0.7f, "time", 0.3f));
			break;
		default:
			MissionData.getInstence().AddMissionGoal(MissionGoalType.MainScene_Appear, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.MainScene);
			_MainMap.UpdateScaleMove(_MainMap.GetDefaultScale(), 0f, 0f);
			_PopupType = 0;
			iTween.ColorTo(PopupBg, iTween.Hash("a", 0, "time", 0.1f));
			iTween.ColorTo(PopupTownMask, iTween.Hash("a", 0, "time", 0.1f));

			MissionData.getInstence().ShowUpdateMissionView();

			break;
		}
	}

	private void prevPopupDestroy()
	{
		hideNPCObj();
		switch(_PopupType)
		{
		case MainPopupType.HQPopup:
			if(_HQPanel != null)
			{
				Destroy(_HQPanel);
				_HQPanel = null;
			}
			break;
		case MainPopupType.CommoPopup:
			if(_CommonPanel != null)
			{
				Destroy(_CommonPanel);
				_CommonPanel = null;
			}
			break;
		case MainPopupType.ResearchPopup:
			if(_ResearchPanel != null)
			{
				Destroy(_ResearchPanel);
				_ResearchPanel = null;
			}
			break;
		case MainPopupType.FactoryPopup:
			if(_FactoryPanel != null)
			{
				Destroy(_FactoryPanel);
				_FactoryPanel = null;
			}
			break;
		case MainPopupType.CorpsPopup:
			if(_CorpsPanel != null)
			{
				Destroy(_CorpsPanel);
				_CorpsPanel = null;
			}
			break;
		case MainPopupType.FightPopup:
			if(_UnitSelectPopup != null)
			{
				Destroy(_UnitSelectPopup);
				_UnitSelectPopup = null;
			}
			break;
		case MainPopupType.HangarPopup:
			if(_HangarPanel != null)
			{
				Destroy(_HangarPanel);
				_HangarPanel = null;
			}
			break;
		case MainPopupType.ShopPopup:
			if(_ShopPanel != null)
			{
				Destroy(_ShopPanel);
				_ShopPanel = null;
			}
			break;
		case MainPopupType.OptionPopup:
			if(_OptionPanel != null)
			{
				Destroy(_OptionPanel);
				_OptionPanel = null;
			}
			break;
		}
	}

	private void ShowNPCObj(byte npcType) {
		if(_NPCObject == null) {
			_NPCObject = new GameObject();
			_NPCObject.AddComponent<SpriteRenderer>();
			_NPCObject.transform.position = new Vector2(-4f, -1.2f + (_OrthographicSize - 5f));
			float npcScale = 1.3f;
			_NPCObject.transform.localScale = new Vector2(-1f * npcScale, 1f * npcScale);
		}

		SpriteRenderer renderer = _NPCObject.GetComponent<SpriteRenderer>();
		renderer.sortingOrder = -3;

		switch(npcType){
		case NPCType.Ria:
			renderer.sprite = Resources.Load<Sprite>("NPC/npc02");
			break;
		case NPCType.Sara:
			renderer.sprite = Resources.Load<Sprite>("NPC/npc05");
			break;
		case NPCType.Kity:
			renderer.sprite = Resources.Load<Sprite>("NPC/npc04");
			break;
		case NPCType.Tassa:
			renderer.sprite = Resources.Load<Sprite>("NPC/npc03");
			break;
		default:
			renderer.sprite = Resources.Load<Sprite>("");
			break;
		}

		iTween.MoveFrom(_NPCObject, iTween.Hash("x", -8, "time", 0.5f));
	}

	private void hideNPCObj() {
		Destroy(_NPCObject);
	}

	private void PopupOpenMove(GameObject PopupPanel)
	{
		if(_UserData.Option_Sound) PopupClickAudio.Play();
		iTween.MoveFrom(PopupPanel, iTween.Hash("y", 2,"speed", 20f, "easetype", iTween.EaseType.easeOutSine
		                                        , "oncomplete", "PopupOpenComplete", "oncompletetarget", this.gameObject));
		BlackBox.renderer.sortingOrder = 1;

	}

	private void PopupOpenComplete() {
		if(_HangarPanel != null) _HangarPanel.GetComponent<HangarPopup>().SetPopupStart();

		// 미션 갱신.
		switch(_PopupType)
		{
		case MainPopupType.HQPopup:
			MissionData.getInstence().AddMissionGoal(MissionGoalType.HQ_Appear, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.HQPopup);

			break;
		case MainPopupType.CommoPopup:
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Commo_Appear, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.CommoPopup);

			break;
		case MainPopupType.ResearchPopup:
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Research_Appear, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.ResearchPopup);

			break;
		case MainPopupType.FactoryPopup:
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Factory_Appear, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.FactoryPopup);

			break;
		case MainPopupType.CorpsPopup:
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Corps_Appear, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.CorpsPopup);

			break;
		case MainPopupType.FightPopup:
			MissionData.getInstence().AddMissionGoal(MissionGoalType.UnitSelect_Appear, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.FightPopup);

			break;
		case MainPopupType.HangarPopup:
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Hangar_Appear, 1);
			GuideArrowManager.getInstence().ShowArrow(SceneType.HangarPopup);

			break;
		case MainPopupType.ShopPopup:
			GuideArrowManager.getInstence().ShowArrow(SceneType.ShopPopup);

			break;
		case MainPopupType.OptionPopup:
			GuideArrowManager.getInstence().ShowArrow(SceneType.OptionPopup);

			break;
		}

		
	}

	private void SetPopupCloseEvent(GameObject PopupPanel)
	{
		Transform[] tempTransforms = PopupPanel.GetComponentsInChildren<Transform>(); 
		
		foreach (Transform child in tempTransforms) 
		{
			if(child.name == "PopupCloseBtn")
			{
				ButtonEvent closeBtn = child.GetComponent<ButtonEvent>() as ButtonEvent;
				closeBtn.SetCallBack(OnPopupClose);
				break;
			}
		}
	}

	private void OnPopupClose(){
		OpenPopup(0);
	}

	private void OnFightStartReady(int townId, bool isAirship) {
		_isAirship = isAirship;
		_WarzoneData.CurrentTownId = (byte)(townId);
		OpenPopup(MainPopupType.FightPopup);
	}

	private void OnUnitPopupClose() {
		OpenPopup(0);
	}

	private void OnUnitPopupSelect(int id) {
		_WarzoneData.CurrentFightUnitId = (byte)(id);

		Unit unit = _UserData.GetUnitById((byte)(id));
		int[] memberSpendMp = new int[5];
		Member member;
		int index = 0;
		foreach(byte memberId in unit.memberList) {
			if(memberId > 0) {
				int spendMP = 0;
				member = _UserData.GetMemberById(memberId);
				Gear bodyGear = GearData.getInstence().GetGearByID(member.BodyGearId);
				Gear SuitGear = GearData.getInstence().GetGearByID(member.SuitGearId);
				Gear EngineGear = GearData.getInstence().GetGearByID(member.EngineGearId);
				Gear Weapon1Gear = GearData.getInstence().GetGearByID(member.Weapon1GearId);
				Gear Weapon2Gear = GearData.getInstence().GetGearByID(member.Weapon2GearId);
				Gear Weapon3Gear = GearData.getInstence().GetGearByID(member.Weapon3GearId);
				spendMP = bodyGear.spendMP + SuitGear.spendMP + EngineGear.spendMP + Weapon1Gear.spendMP;
				if(Weapon2Gear != null) spendMP += Weapon2Gear.spendMP;
				if(Weapon3Gear != null) spendMP += Weapon3Gear.spendMP;
				if(member.CurrentMP < spendMP) { 
					_UserData.SetAlert("MP가 부족한 대원이 있습니다.", new BillModel());
					return;
				}
				if(member.CurrentHP < (member.MaxHP / 3)) {
					_UserData.SetAlert("부상이 심한 대원이 있습니다.", new BillModel());
					return;
				}

				memberSpendMp[index] = spendMP;
				index ++;
			}
		}

		index = 0;
		foreach(byte memberId in unit.memberList) {
			member = _UserData.GetMemberById(memberId);
			if(member != null) {
				member.CurrentMP -= memberSpendMp[index];
				member.lastMPUpdateTime = SystemData.GetInstance().getCurrentTime() + (SystemData.GetInstance().MemberMpPlusDelay * SystemData.GetInstance().millisecondNum);
				index ++;
			}

		}

		iTween.ColorTo(BlackBox, iTween.Hash("a", 1, "time", 1f, "oncomplete", "SceneChange", "oncompletetarget", this.gameObject));
	}

	private void SceneChange() {
		Application.LoadLevel("DarkSpriteFight");
	}

}
