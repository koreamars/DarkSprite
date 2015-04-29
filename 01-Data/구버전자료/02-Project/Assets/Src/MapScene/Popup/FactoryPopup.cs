using UnityEngine;
using System.Collections;

public class FactoryPopup : MonoBehaviour {

	public bool IsTest;
	public GameObject CloseBtn;
	public GameObject ScrollMenu;
	public GameObject SubMenuBtn1;
	public GameObject SubMenuBtn2;
	public GameObject SubMenuBtn3;
	public GameObject SubMenuBtn4;
	public GameObject CreateBtn;
	public GameObject DeleteBtn;
	public GameObject Thumbnail;
	public GameObject DataTxt1;
	public GameObject DataTxt2;
	public GameObject DataTxt3;
	public GameObject DataTxt4;
	public GameObject DataTxt5;
	public GameObject DataTxt6;
	public GameObject DataTxt7;

	public Color TitleFontColor;
	public Color TitleFontLineColor;
	public Color SubTitleFontColor;
	public Color SubTitleFontLineColor;

	private UserData _UserData;
	private ScriptData _ScriptData;
	private GearData _GearData;
	private SystemData _SystemData;

	private OutLineFont DataTitleFont;
	private GameObject SubTitleTxt;
	
	public delegate void CloseEvent();
	private CloseEvent _CloseCallback;

	private int _MainIndex;
	private int _CurrentSelectId = 0;
	private short _CurrentListIndex = 0;
	private GameObject _RewardViewer;

	private GameObject _GearDataUIObj;
	private GearDataUI _GearDataUI;

	public Color _CleateBtnColor;
	public Color _DeleteBtnColor;
	public Color _CancelBtnColor;

	private ScrollMenu _ScrollMenu;
	private ListMenuModel[] _ListMenuData;

	private bool _OnTime;
	private float _Timer;

	private int _CurrentOwnCount = 0;

	void Awake() {
		_MainIndex = 0;

		_SystemData = SystemData.GetInstance();
		_UserData = UserData.getInstence();
		_ScriptData = ScriptData.getInstence();
		_GearData = GearData.getInstence();
	}

	void Start() {
		if(IsTest) init();
	}

	void Update() {
		if(_ListMenuData.Length > 0) {
			_Timer += Time.deltaTime;
			if(_Timer > 1) {
				UpdateListMenu();
				ShowGearData();
				_Timer = 0;
			}
		}
	}

	public void init() {

		_RewardViewer = Instantiate(Resources.Load<GameObject>("Common/RewardViewer")) as GameObject;
		_RewardViewer.transform.parent = this.gameObject.transform;

		_GearDataUIObj = Instantiate(Resources.Load<GameObject>("Common/GearDataUI")) as GameObject;
		_GearDataUIObj.transform.parent = this.gameObject.transform; 
		_GearDataUIObj.transform.localScale = new Vector2(0.9f, 0.9f);
		_GearDataUIObj.transform.position = new Vector2(-5.59f, -1.25f);
		_GearDataUI = _GearDataUIObj.GetComponent<GearDataUI>();
		_GearDataUI.init();

		/*
		GameObject dataTitleTxt = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
		DataTitleFont = dataTitleTxt.GetComponent<OutLineFont>();
		DataTitleFont.SetString("");
		DataTitleFont.SetAlign(TextAnchor.UpperLeft);
		DataTitleFont.SetLineColor(SubTitleFontLineColor);
		dataTitleTxt.transform.parent = this.gameObject.transform;
		dataTitleTxt.transform.position = new Vector2(-6.68f, 0.52f);
		DataTitleFont.SetFontSize(22);
		DataTitleFont.SetLineSize(2);
		*/

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


		_ScrollMenu = ScrollMenu.GetComponent<ScrollMenu>();
		_ScrollMenu.SetMenuClick(OnListMenuClick);
		
		CloseBtn.GetComponent<ButtonEvent>().SetCallBack(OnPopupClose);

		string btnName = "";
		btnName = _ScriptData.GetGameScript(120403).script;
		SubMenuBtn1.GetComponent<CommonBtn>().Init(1, btnName, 1, TitleFontColor);
		SubMenuBtn1.GetComponent<CommonBtn>().SetClick(OnMainMenuClick);
		btnName = _ScriptData.GetGameScript(120400).script;
		SubMenuBtn2.GetComponent<CommonBtn>().Init(2, btnName, 1, TitleFontColor);
		SubMenuBtn2.GetComponent<CommonBtn>().SetClick(OnMainMenuClick);
		btnName = _ScriptData.GetGameScript(120401).script;
		SubMenuBtn3.GetComponent<CommonBtn>().Init(3, btnName, 1, TitleFontColor);
		SubMenuBtn3.GetComponent<CommonBtn>().SetClick(OnMainMenuClick);
		btnName = _ScriptData.GetGameScript(120402).script;
		SubMenuBtn4.GetComponent<CommonBtn>().Init(4, btnName, 1, TitleFontColor);
		SubMenuBtn4.GetComponent<CommonBtn>().SetClick(OnMainMenuClick);
		CreateBtn.GetComponent<CommonBtn>().SetClick(OnCreateBtnClick);
		DeleteBtn.GetComponent<CommonBtn>().SetClick(OnDeleteBtnClick);

		ShowOwnGearData();

		UpdateView();
		ShowGearData();
	}
	
	
	public void SetCloseEventCallBack(CloseEvent onCloseEvent) {
		_CloseCallback = new CloseEvent(onCloseEvent);
		
	}

	/** 하단 메뉴 선텍 상태 설정 */
	private void SetMainMenu() {
		SubMenuBtn1.GetComponent<CommonBtn>().SetBtnSelect(false);
		SubMenuBtn2.GetComponent<CommonBtn>().SetBtnSelect(false);
		SubMenuBtn3.GetComponent<CommonBtn>().SetBtnSelect(false);
		SubMenuBtn4.GetComponent<CommonBtn>().SetBtnSelect(false);

		CreateBtn.transform.position = new Vector2(-1.42f, -4.08f);
		DeleteBtn.transform.position = new Vector2(-4.7f, -4.08f);
		CreateBtn.GetComponent<CommonBtn>().FontOutLineColor = SubTitleFontLineColor;
		DeleteBtn.GetComponent<CommonBtn>().FontOutLineColor = SubTitleFontLineColor;
		switch(_MainIndex) {
		case 1:		// 슈트.
			if(_ListMenuData.Length == 0) {
				CreateBtn.transform.position = new Vector2(5.48f, -23.02f);
				DeleteBtn.transform.position = new Vector2(5.48f, -23.02f);
			}
			//SubTitleTxt.GetComponent<OutLineFont>().SetString(_ScriptData.GetGameScript(120403).script);
			SubMenuBtn1.GetComponent<CommonBtn>().SetBtnSelect(true);
			CreateBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(130400).script, 1, _CleateBtnColor);
			DeleteBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(130402).script, 1, _DeleteBtnColor);

			GuideArrowManager.getInstence().ShowArrow(SceneType.FactoryPopup_Suit);
			break;
		case 2:		// 기체.
			if(_ListMenuData.Length == 0) {
				CreateBtn.transform.position = new Vector2(5.48f, -23.02f);
				DeleteBtn.transform.position = new Vector2(5.48f, -23.02f);
			}
			//SubTitleTxt.GetComponent<OutLineFont>().SetString(_ScriptData.GetGameScript(120400).script);
			SubMenuBtn2.GetComponent<CommonBtn>().SetBtnSelect(true);
			CreateBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(130400).script, 1, _CleateBtnColor);
			DeleteBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(130402).script, 1, _DeleteBtnColor);

			GuideArrowManager.getInstence().ShowArrow(SceneType.FactoryPopup_Airframe);
			break;
		case 3:		// 엔진.
			if(_ListMenuData.Length == 0) {
				CreateBtn.transform.position = new Vector2(5.48f, -23.02f);
				DeleteBtn.transform.position = new Vector2(5.48f, -23.02f);
			}
			//SubTitleTxt.GetComponent<OutLineFont>().SetString(_ScriptData.GetGameScript(120401).script);
			SubMenuBtn3.GetComponent<CommonBtn>().SetBtnSelect(true);
			CreateBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(130400).script, 1, _CleateBtnColor);
			DeleteBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(130402).script, 1, _DeleteBtnColor);

			GuideArrowManager.getInstence().ShowArrow(SceneType.FactoryPopup_Engine);
			break;
		case 4:		// 무장.
			if(_ListMenuData.Length == 0) {
				CreateBtn.transform.position = new Vector2(5.48f, -23.02f);
				DeleteBtn.transform.position = new Vector2(5.48f, -23.02f);
			}
			//SubTitleTxt.GetComponent<OutLineFont>().SetString(_ScriptData.GetGameScript(120402).script);
			SubMenuBtn4.GetComponent<CommonBtn>().SetBtnSelect(true);
			CreateBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(130400).script, 1, _CleateBtnColor);
			DeleteBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(130402).script, 1, _DeleteBtnColor);

			GuideArrowManager.getInstence().ShowArrow(SceneType.FactoryPopup_Weapon);
			break;
		default:
			//CreateBtn.transform.position = new Vector2(5.48f, -23.02f);
			if(_UserData.UserMakeGearList.Count == 0) {
				CreateBtn.transform.position = new Vector2(5.48f, -23.02f);
			}
			DeleteBtn.transform.position = new Vector2(5.48f, -23.02f);
			//SubTitleTxt.GetComponent<OutLineFont>().SetString(_ScriptData.GetGameScript(120404).script);
			CreateBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(130401).script, 1, _CancelBtnColor);

			GuideArrowManager.getInstence().ShowArrow(SceneType.FactoryPopup);
			break;
		}

	}

	private void UpdateListMenu() {

		ArrayList userMakeList;
		Color color = Color.white;

		switch(_MainIndex) {
		case 1:		// 슈트.
			userMakeList = GetPossibleGearList(GearType.Suit);
			userMakeList.Sort(new MarkGearSort());
			userMakeList.AddRange(AddMakeDatalist(GearType.Suit));
			GetListMenuByGearType(userMakeList);
			//if(_ListMenuData.Length > 0) _CurrentSelectId = _ListMenuData[0].id;
			//GuideArrowManager.getInstence().ShowArrow(SceneType.FactoryPopup_Suit);
			break;
		case 2:		// 기체.
			userMakeList = GetPossibleGearList(GearType.Body);
			userMakeList.Sort(new MarkGearSort());
			userMakeList.AddRange(AddMakeDatalist(GearType.Body));
			GetListMenuByGearType(userMakeList);
			//if(_ListMenuData.Length > 0) _CurrentSelectId = _ListMenuData[0].id;
			//GuideArrowManager.getInstence().ShowArrow(SceneType.FactoryPopup_Airframe);
			break;
		case 3:		// 엔진.
			userMakeList = GetPossibleGearList(GearType.Engine);
			userMakeList.Sort(new MarkGearSort());
			userMakeList.AddRange(AddMakeDatalist(GearType.Engine));
			GetListMenuByGearType(userMakeList);
			//if(_ListMenuData.Length > 0) _CurrentSelectId = _ListMenuData[0].id;
			//GuideArrowManager.getInstence().ShowArrow(SceneType.FactoryPopup_Engine);
			break;
		case 4:		// 무장.
			userMakeList = GetPossibleGearList(GearType.Weapon_Gun);
			ArrayList addMakeList1 = GetPossibleGearList(GearType.Weapon_Rocket);
			ArrayList addMakeList2 = GetPossibleGearList(GearType.Weapon_Missle);
			userMakeList.AddRange(addMakeList1);
			userMakeList.AddRange(addMakeList2);
			userMakeList.Sort(new MarkGearSort());
			userMakeList.AddRange(AddMakeDatalist(GearType.Weapon_Gun));
			userMakeList.AddRange(AddMakeDatalist(GearType.Weapon_Rocket));
			userMakeList.AddRange(AddMakeDatalist(GearType.Weapon_Missle));
			GetListMenuByGearType(userMakeList);
			//if(_ListMenuData.Length > 0) _CurrentSelectId = _ListMenuData[0].id;
			//GuideArrowManager.getInstence().ShowArrow(SceneType.FactoryPopup_Weapon);

			break;
		default:
			userMakeList = _UserData.GetMakeGearList();
			_ListMenuData = new ListMenuModel[userMakeList.Count];
			//byte index = 0;
			/*
			Gear gear;
			foreach(MakeGear makeGear in userMakeList) {
				gear = _GearData.GetGearByID(makeGear.gearId);
				_ListMenuData[index] = new ListMenuModel();
				_ListMenuData[index].id = index;
				_ListMenuData[index].optionalId = makeGear.gearId;
				string str = _ScriptData.GetGameScript(gear.scriptId).script;
				if(index == 0) {
					long lastTime = makeGear.endTime - _SystemData.getCurrentTime();
					_ListMenuData[index].scriptString = str + " (" + _SystemData.GetTimeStrByTime(lastTime)+ ")";
				} else {
					_ListMenuData[index].scriptString = str;
				}
				index ++;
			}
			*/
			GetListMenuByGearType(userMakeList);

			color = Color.white;

			//GuideArrowManager.getInstence().ShowArrow(SceneType.FactoryPopup);
			break;
		}

		_ScrollMenu.BtnColor = color;

		_ScrollMenu.SetScrollData(_ListMenuData, _CurrentListIndex);
		_ScrollMenu.SetScrollView();

	}

	private ArrayList AddMakeDatalist(byte gearType) {
		ArrayList userMakeList = _UserData.GetMakeGearList();
		ArrayList makeList = new ArrayList();

		Gear gear;
		foreach(MakeGear makeGear in userMakeList) {
			gear = _GearData.GetGearByID(makeGear.gearId);
			if(gearType == gear.gearType) makeList.Add(makeGear);
		}

		return makeList;
	}

	private void UpdateView() {

		UpdateListMenu();
		SetMainMenu();
	}

	/** 사용자중 제작 가능한 목록을 반환함 */
	private ArrayList GetPossibleGearList(byte gearType) {
		_UserData.GetIsResearching();
		ArrayList UserResearchList = _UserData.UserResearchList;
		ArrayList userMakeGearPossibleList = new ArrayList();

		Research research;
		Gear gear;
		foreach(UserResearch userResearch in UserResearchList) {
			if(userResearch.isComplete) {
				research = _GearData.GetResearchByID(userResearch.id);
				gear = _GearData.GetGearByID(research.gearId);
				if(gear.gearType == gearType) userMakeGearPossibleList.Add(gear);
			}

		}

		// 다크 기어 장비 출력.

		Research checkResearch;
		Gear darkGear;
		foreach(OwnGear ownGear in _UserData.UserOwnGearList) {
			checkResearch = _GearData.GetResearchByGearID(ownGear.gearId);
			if(checkResearch != null) continue;
			darkGear = _GearData.GetGearByID(ownGear.gearId);
			if(darkGear != null) {
				if(darkGear.gearType == gearType) userMakeGearPossibleList.Add(darkGear);
			}
		}

		return userMakeGearPossibleList;
	}

	private void GetListMenuByGearType(ArrayList gearList) {
		//gearList.Sort(new MarkGearSort());
		_ListMenuData = new ListMenuModel[gearList.Count];
		if(gearList.Count == 0) return;
		byte index = 0;
		OwnGear ownGear;
		MakeGear makeGear;
		Gear gear;
		Color makeGearColor = Color.white;
		makeGearColor.r = 1f;
		makeGearColor.g = 0.8f;
		makeGearColor.b = 0.2f;

		for(byte i = 0; i < gearList.Count; i++) {
			_ListMenuData[index] = new ListMenuModel();
			if(gearList[i] is Gear) {
				gear = gearList[i] as Gear;
				_ListMenuData[index].id = index;
				_ListMenuData[index].optionalId = gear.id;
				ownGear = _UserData.GetOwnGearByGearId(gear.id);
				byte eaCount = 0;
				if(ownGear != null) eaCount = ownGear.ownCount;
				_ListMenuData[index].scriptString = _ScriptData.GetGameScript(gear.scriptId).script + " (" + eaCount + "EA)";
			} else {
				makeGear = gearList[i] as MakeGear;
				gear = _GearData.GetGearByID(makeGear.gearId);
				_ListMenuData[index] = new ListMenuModel();
				_ListMenuData[index].id = index;
				_ListMenuData[index].optionalId = makeGear.gearId;
				_ListMenuData[index].isClick = false;
				string str = _ScriptData.GetGameScript(gear.scriptId).script;
				//if(index == 0) {
				if(true) {
					long lastTime = makeGear.endTime - _SystemData.getCurrentTime();
					_ListMenuData[index].scriptString = str + " (" + _SystemData.GetTimeStrByTime(lastTime)+ ")";
				} else {
					//_ListMenuData[index].scriptString = str;
				}
				_ListMenuData[index].fontColor = makeGearColor;
				//_ListMenuData[index].outLineColor = Color.gray;
			}
			index ++;
		}
		/*
		foreach(Gear gear in gearList) {
			_ListMenuData[index] = new ListMenuModel();
			_ListMenuData[index].id = gear.id;
			ownGear = _UserData.GetOwnGearByGearId(gear.id);
			byte eaCount = 0;
			if(ownGear != null) eaCount = ownGear.ownCount;
			_ListMenuData[index].scriptString = _ScriptData.GetGameScript(gear.scriptId).script + " (" + eaCount + "EA)";
			
			index ++;
		}
		*/
	}

		
	private void OnPopupClose() {
		if(_CloseCallback != null) _CloseCallback();
	}

	private void OnListMenuClick(int id) {
		short index = 0;
		foreach(ListMenuModel model in _ListMenuData) {
			if(model.optionalId == id) {
				_CurrentListIndex = index;
				_CurrentSelectId = model.optionalId;
				break;
			}
			index ++;
		}
		//_CurrentSelectId = id;
		ShowGearData();
	}
	
	private void OnMainMenuClick(int index) {

		//_MainIndex = 0;
		if(_MainIndex == index) {
			_MainIndex = 0;
		} else {
			_MainIndex = index;
		}
		_CurrentListIndex = 0;

		UpdateView();

		_CurrentSelectId = 0;
		if(_ListMenuData.Length > 0) {
			ListMenuModel thisModel = _ListMenuData[0];
			_CurrentSelectId = thisModel.optionalId;
		}

		ShowGearData();
	}

	/** 장비 생산 시작 */
	private void OnCreateBtnClick(int id) {
		if(_MainIndex != 0) {	// 장비 생산.

			// 보유량 계산.
			byte currentOwnCount = 0;
			foreach(OwnGear ownGear in _UserData.UserOwnGearList) {
				currentOwnCount += ownGear.ownCount;
			}

			currentOwnCount += (byte)(_UserData.UserMakeGearList.Count);

			if(currentOwnCount >= _SystemData.MaxOwnItemCount) {
				_UserData.SetAlert(_ScriptData.GetGameScript(150138).script, new BillModel());
				return;
			}

			Gear gear = _GearData.GetGearByID((short)(_CurrentSelectId));

			if(gear == null) return;

			BillModel billModel = _UserData.SetMakeGear(gear);
			if(billModel.money > 0) {
				_UserData.UpdatePayData(billModel, new Vector2(-0.65f, -3.53f));
			}

			MissionData.getInstence().AddMissionGoal(MissionGoalType.Factory_GearMake_Start, 1);
			LocalData.getInstence().UserMakeGearDataSave();

			if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("MakeGear", gear.id.ToString());

		} else {	// 장비 생산 취소.
			if(_UserData.UserMakeGearList.Count > 0) {
				MakeGear makeGear = _UserData.UserMakeGearList[0] as MakeGear;
				Gear cancelGear = _GearData.GetGearByID(makeGear.gearId);
				BillModel billModel = new BillModel();
				billModel.money = cancelGear.makeCost;
				billModel.core = cancelGear.makeResource;
				_UserData.SetConfirmPop(_ScriptData.GetGameScript(150147).script, SetMakeGearCancel, billModel);

			} else {
				_UserData.SetAlert(_ScriptData.GetGameScript(150148).script, new BillModel());
			}

		}

		ShowOwnGearData();
	}

	private void SetMakeGearCancel(bool isConfirm) {
		if(isConfirm == true) {
			if(_UserData.UserMakeGearList.Count > 0) {
				MakeGear makeGear = _UserData.UserMakeGearList[0] as MakeGear;
				long lastTime = makeGear.endTime - _SystemData.getCurrentTime();
				Gear cancelGear = _GearData.GetGearByID(makeGear.gearId);
				_UserData.UserMoney += cancelGear.makeCost;
				_UserData.UserCores += cancelGear.makeResource;

				_UserData.UserMakeGearList.Remove(makeGear);

				UpdateView();
				ShowGearData();

				// 다른 장비가 존재시 남은 시간 제거.
				foreach(MakeGear otherMakeGear in _UserData.UserMakeGearList) {
					otherMakeGear.endTime -= lastTime;
				}

				LocalData.getInstence().UserResourceSave();
				LocalData.getInstence().UserMakeGearDataSave();

			} else {
				_UserData.SetAlert(_ScriptData.GetGameScript(150148).script, new BillModel());
			}
		}
	}

	/** 보유 장비를 제거합니다. */
	private void OnDeleteBtnClick(int id) {
		//Gear gear = _GearData.GetGearByID((byte)(_CurrentSelectId));
		OwnGear ownGear = _UserData.GetOwnGearByGearId((short)(_CurrentSelectId));
		if(ownGear != null && ownGear.ownCount > 0) {
			Gear gear = _GearData.GetGearByID((short)(_CurrentSelectId));
			BillModel billmodel = new BillModel();
			billmodel.money = (int)(gear.makeCost / _SystemData.GearDeleteRewardValue);
			billmodel.core = (int)(gear.makeResource /  _SystemData.GearDeleteRewardValue);
			string gearName = _ScriptData.GetGameScript(gear.scriptId).script;
			_UserData.SetConfirmPop("'" + gearName + "' " + _ScriptData.GetGameScript(150146).script, OwnGearDelete, billmodel);
		} else {
			_UserData.SetAlert(_ScriptData.GetGameScript(150145).script, new BillModel());
		}
	}

	private void OwnGearDelete(bool isConfirm) {
		if(isConfirm) {
			Gear gear = _GearData.GetGearByID((short)(_CurrentSelectId));
			OwnGear ownGear = _UserData.GetOwnGearByGearId((short)(_CurrentSelectId));
			ownGear.ownCount -= 1;

			BillModel billModel = new BillModel();
			billModel.money = (int)(gear.makeCost / _SystemData.GearDeleteRewardValue);
			billModel.core = (int)(gear.makeResource /  _SystemData.GearDeleteRewardValue);
			_UserData.UserMoney += billModel.money;
			_UserData.UserCores += billModel.core;

			_UserData.UpdatePayData(billModel, new Vector2(0f, 0f));

			LocalData.getInstence().UserOwnGearDataSave();

			ShowOwnGearData();
		}
	}

	private void ShowOwnGearData() {
		// 보유량 계산.
		byte currentOwnCount = 0;
		foreach(OwnGear userOwnGear in _UserData.UserOwnGearList) {
			currentOwnCount += userOwnGear.ownCount;
		}
		_CurrentOwnCount = currentOwnCount + (byte)(_UserData.UserMakeGearList.Count);
		SubTitleTxt.GetComponent<OutLineFont>().SetString(_ScriptData.GetGameScript(160128).script + " " + _CurrentOwnCount + "/" + _SystemData.MaxOwnItemCount);
	}

	private void ShowGearData() {
		//DataTitleFont.SetString("");
		DataTxt1.GetComponent<TextMesh>().text = "";
		DataTxt2.GetComponent<TextMesh>().text = "";
		DataTxt3.GetComponent<TextMesh>().text = "";
		DataTxt4.GetComponent<TextMesh>().text = "";
		DataTxt5.GetComponent<TextMesh>().text = "";
		DataTxt6.GetComponent<TextMesh>().text = "";
		DataTxt7.GetComponent<TextMesh>().text = "";

		Gear gear = null;
		if(_MainIndex == 0) {
			if(_ListMenuData.Length > 0) {
				if(_ListMenuData.Length <= _CurrentSelectId) _CurrentSelectId = 0;
				if(_ListMenuData[_CurrentSelectId] == null) _CurrentSelectId = 0;
				gear = _GearData.GetGearByID((short)(_ListMenuData[_CurrentSelectId].optionalId));
			}
		} else {
			gear = _GearData.GetGearByID((short)(_CurrentSelectId));
		}

		BillModel billModel = new BillModel();

		if(gear != null) {
			Research research = null;
			if(gear != null) research = _GearData.GetResearchByGearID(gear.id);

			if(research == null) {
				CreateBtn.GetComponent<CommonBtn>().SetEnabled(false);
			} else {
				CreateBtn.GetComponent<CommonBtn>().SetEnabled(true);
			}

			SpriteRenderer renderer = (SpriteRenderer)Thumbnail.GetComponent ("SpriteRenderer");
			renderer.sprite = Resources.Load<Sprite>("UnitResource/Thumbnail/");
			//DataTitleFont.SetString(_ScriptData.GetGameScript(gear.scriptId).script);

			byte ownCount = 0;
			OwnGear ownGear = _UserData.GetOwnGearByGearId(gear.id);
			if(ownGear != null) ownCount = ownGear.ownCount;

			if(_MainIndex == 0) {
				MakeGear makeGear = _UserData.UserMakeGearList[_CurrentSelectId] as MakeGear;
				long lastTime = makeGear.endTime - _SystemData.getCurrentTime();
				DataTxt4.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160128).script + " : " + ownCount;
				DataTxt5.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160109).script + " : " + _SystemData.GetTimeStrByTime(lastTime);
			} else {
				DataTxt4.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160128).script + " : " + ownCount;
				long makeTime = gear.makeTime * _SystemData.millisecondNum;
				DataTxt5.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160129).script + " : " + _SystemData.GetTimeStrByTime(makeTime);
				DataTxt6.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160130).script + " : ";

				billModel.money = gear.makeCost;
				billModel.moneyPlus = false;
				billModel.core = gear.makeResource;
				billModel.corePlus = false;
			}

			_GearDataUI.GearUpdate(gear.id);




		} else {
			SpriteRenderer renderer = (SpriteRenderer)Thumbnail.GetComponent ("SpriteRenderer");
			renderer.sprite = Resources.Load<Sprite>("UnitResource/Thumbnail/");

			_GearDataUI.GearUpdate(0);
		}

		_RewardViewer.transform.localScale = new Vector3(1f, 1f, 1f);
		_RewardViewer.GetComponent<RewardViewer>().init(billModel, 5, false);
		_RewardViewer.transform.position = new Vector2(-4.64f, -3.33f);
		_RewardViewer.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
	}

	public class MarkGearSort : IComparer
	{
		public int Compare(object x, object y)
		{
			// reverse the arguments
			int g1 = ((Gear)x).id;
			int g2 = ((Gear)y).id;

			Research research1 = GearData.getInstence().GetResearchByGearID((short)(g1));
			if(research1 == null) g1 -= 10000;
			Research research2 = GearData.getInstence().GetResearchByGearID((short)(g2));
			if(research2 == null) g2 -= 10000;

			if (g1 > g2)
				return -1;
			else
				return 0;
		}
		
	}

}
