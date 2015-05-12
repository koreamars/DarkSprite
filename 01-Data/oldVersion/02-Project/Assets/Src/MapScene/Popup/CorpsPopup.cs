using UnityEngine;
using System.Collections;
using System;

public class CorpsPopup : MonoBehaviour {

	public bool IsTest;
	public GameObject CloseBtn;
	public GameObject SubTitleTxt;
	public GameObject ListMenu;
	public GameObject MemberListMenu;
	public GameObject MemberThumb1;
	public GameObject MemberThumb2;
	public GameObject MemberThumb3;
	public GameObject MemberThumb4;
	public GameObject MemberThumb5;
	public GameObject MemberSelectBox;
	public GameObject MainMemberThumb;
	public GameObject MemberDataTxt;
	public GameObject GearDataTxt;
	public GameObject OptionBtn1;
	public GameObject OptionBtn2;
	public GameObject OptionBtn3;
	public GameObject SubTitleTxt2;
	public GameObject ListBtn;

	public Color NewUnitColor;
	
	public delegate void CloseEvent();
	private CloseEvent _CloseCallback;

	private SystemData _SystemData;
	private UserData _UserData;
	private ScriptData _ScriptData;
	private MemberData _MemberData;
	private GearData _GearData;

	private GameObject _MemberDataUIObj;
	private MemberDataUI _MemberDataUI;

	private ScrollMenu _ScrollMenu;
	private ScrollMenberMenu _ScrollMemberMenu;
	private ListMenuModel[] _ListMenuData;
	private ListMemberModel[] _ListMemberData;

	private byte _CurrentUnitId = 0;
	private short _CurrentMemberId = 0;
	private byte _CurrentSelectSlot = 1;

	void Start() {

		if(IsTest) {
			LocalData.getInstence().AllLoad();
			DarkSprite.getInstence();
			init();
		}
	}

	public void init() {

		MemberSelectBox.renderer.sortingOrder = 10;

		_SystemData = SystemData.GetInstance();
		_MemberData = MemberData.getInstence();
		_ScriptData = ScriptData.getInstence();
		_UserData = UserData.getInstence();
		_GearData = GearData.getInstence();

		CloseBtn.GetComponent<ButtonEvent>().SetCallBack(OnPopupClose);
		SubTitleTxt.renderer.sortingOrder = 2;
		
		OptionBtn1.GetComponent<CommonBtn>().SetClick(OnOptionBtn1Click);
		OptionBtn1.GetComponent<CommonBtn>().Init(0, "", 10, Color.white);
		OptionBtn2.GetComponent<CommonBtn>().SetClick(OnUnitDelete);
		OptionBtn2.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(120513).script, 10, Color.white);
		OptionBtn3.GetComponent<CommonBtn>().SetClick(OnOptionBtn3Click);

		MemberThumb1.GetComponent<CommonBtn>().SetClick(OnMemberSlotClick);
		MemberThumb1.GetComponent<CommonBtn>().btnId = 1;
		MemberThumb2.GetComponent<CommonBtn>().SetClick(OnMemberSlotClick);
		MemberThumb2.GetComponent<CommonBtn>().btnId = 2;
		MemberThumb3.GetComponent<CommonBtn>().SetClick(OnMemberSlotClick);
		MemberThumb3.GetComponent<CommonBtn>().btnId = 3;
		MemberThumb4.GetComponent<CommonBtn>().SetClick(OnMemberSlotClick);
		MemberThumb4.GetComponent<CommonBtn>().btnId = 4;
		MemberThumb5.GetComponent<CommonBtn>().SetClick(OnMemberSlotClick);
		MemberThumb5.GetComponent<CommonBtn>().btnId = 5;
		
		_ScrollMenu = ListMenu.GetComponent<ScrollMenu>();
		_ScrollMenu.SetMenuClick(OnUnitListClick);
		_ScrollMenu.sortingNum = 2;
		_ScrollMemberMenu = MemberListMenu.GetComponent<ScrollMenberMenu>();
		_ScrollMemberMenu.sortingNum = 2;
		_ScrollMemberMenu.SetMenuClick(OnMemberListClick);

		ListBtn.GetComponent<CommonBtn>().SetClick(OnListTypeChange);
		ListBtn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(120511).script, 2, Color.white);

		_MemberDataUIObj = Instantiate(Resources.Load<GameObject>("Common/MemberDataUI")) as GameObject;
		_MemberDataUIObj.transform.parent = this.gameObject.transform;
		_MemberDataUIObj.transform.localScale = new Vector2(1f, 1f);

		_MemberDataUI = _MemberDataUIObj.GetComponent<MemberDataUI>();
		_MemberDataUI.init(0);

		ListMenuChange(0);
		ShowUnitData();

		_MemberDataUIObj.transform.position = new Vector2(0.49f, 0.14f);
	}
	
	
	public void SetCloseEventCallBack(CloseEvent onCloseEvent) {
		_CloseCallback = new CloseEvent(onCloseEvent);
		
	}


	// 좌측 리스트 메뉴 관련. =====================================================================================

	/** 좌측 리스트 메뉴를 변경함. 0:부대리스트 1:대원리스트 */
	private void ListMenuChange(byte type) {
		TextMesh subTitle2 = SubTitleTxt2.GetComponent<TextMesh>();
		if(type == 0) {
			ListMenu.transform.position = new Vector2(-3.94f, ListMenu.transform.position.y);
			MemberListMenu.transform.position = new Vector2(20f, ListMenu.transform.position.y);
			ListBtn.GetComponent<CommonBtn>().SetBtnName(_ScriptData.GetGameScript(120511).script);
			subTitle2.text = _ScriptData.GetGameScript(120506).script;
			GuideArrowManager.getInstence().ShowArrow(SceneType.CorpsPopup);

			SetCorpList();
		} else {
			ListMenu.transform.position = new Vector2(20f, ListMenu.transform.position.y);
			MemberListMenu.transform.position = new Vector2(-3.94f, ListMenu.transform.position.y);
			ListBtn.GetComponent<CommonBtn>().SetBtnName(_ScriptData.GetGameScript(120512).script);
			subTitle2.text = _ScriptData.GetGameScript(120510).script;
			GuideArrowManager.getInstence().ShowArrow(SceneType.CorpsPopup_MemberList);

			SetMemberList();
		}
	}

	/** 부대 리스트를 보여줌. */
	private void SetCorpList() {
		byte MaxUnitCount = 5;
		ArrayList unitList = _UserData.UserUnitList;
		if(unitList.Count < MaxUnitCount) {	// 부대 생성 수 제한.
			_ListMenuData = new ListMenuModel[unitList.Count + 1];
		} else {
			_ListMenuData = new ListMenuModel[unitList.Count];
		}

		int index = 0;
		short listIndex = 0;
		foreach(Unit unit in unitList) {
			_ListMenuData[index] = new ListMenuModel();
			_ListMenuData[index].id = unit.id;
			_ListMenuData[index].scriptString = _ScriptData.GetUnitNameByUnitId(unit.id);
			
			if(_CurrentUnitId == 0 && index == 0) {
				_CurrentUnitId = unit.id;
				_CurrentSelectSlot = 1;
				_CurrentMemberId = unit.memberList[0];

			}
			if(_CurrentUnitId == unit.id) listIndex = (short)(index);
			
			index ++;
		}
		if(index < MaxUnitCount) {	// 부대 생성 수 제한.
			_ListMenuData[index] = new ListMenuModel();
			_ListMenuData[index].id = 0;
			_ListMenuData[index].scriptString = _ScriptData.GetGameScript(160131).script;
			_ListMenuData[index].fontColor = NewUnitColor;
		}
		
		_ScrollMenu.BtnColor = Color.white;
		_ScrollMenu.SetScrollData(_ListMenuData, listIndex);
		_ScrollMenu.SetScrollView();


	}
	
	/** 대원 목록을 보여줌. */
	private void SetMemberList() {
		ArrayList memberisNotUnitList = _UserData.GetUnitNotMembers();
		memberisNotUnitList.Sort(new MemberOutListHPSort());
		memberisNotUnitList.Sort(new MemberOutListMPSort());
		_ListMemberData = new ListMemberModel[memberisNotUnitList.Count];
		int index = 0;
		Member member;
		foreach(short memberid in memberisNotUnitList) {
			member = _UserData.GetMemberById(memberid);
			_ListMemberData[index] = new ListMemberModel();
			_ListMemberData[index].id = (byte)(index + 1);
			_ListMemberData[index].ClassId = member.ClassId;
			_ListMemberData[index].optionalId = member.id;
			_ListMemberData[index].scriptString = _ScriptData.GetMemberNameByMemberId(member.id);
			
			index ++;
		}
		//-3.94f
		_ScrollMemberMenu.BtnColor = Color.white;
		_ScrollMemberMenu.SetScrollData(_ListMemberData, -1);
		_ScrollMemberMenu.SetScrollView();
	}

	// 좌측 리스트 메뉴 관련. =====================================================================================


	// 유닛 정보 관련 ============================================================================================

	private void ShowMemberData() {

		TextMesh GearTitle = GearDataTxt.transform.FindChild("GearTitle").GetComponent<TextMesh>();
		TextMesh GearSuitStr = GearDataTxt.transform.FindChild("GearData1").GetComponent<TextMesh>();
		TextMesh GearBodyStr = GearDataTxt.transform.FindChild("GearData2").GetComponent<TextMesh>();
		TextMesh GearEngineStr = GearDataTxt.transform.FindChild("GearData3").GetComponent<TextMesh>();
		TextMesh GearWeapon1Str = GearDataTxt.transform.FindChild("GearData4").GetComponent<TextMesh>();
		TextMesh GearWeapon2Str = GearDataTxt.transform.FindChild("GearData5").GetComponent<TextMesh>();
		TextMesh GearWeapon3Str = GearDataTxt.transform.FindChild("GearData6").GetComponent<TextMesh>();
		GearTitle.text = "";
		GearSuitStr.text = "";
		GearBodyStr.text = "";
		GearEngineStr.text = "";
		GearWeapon1Str.text = "";
		GearWeapon2Str.text = "";
		GearWeapon3Str.text = "";
		
		
		Member member = _UserData.GetMemberById(_CurrentMemberId);
		string memberThumbURI = "";
		if(member != null) {

			_MemberDataUI.MemberUpdate(member.id, false);

			string className = _ScriptData.GetGameScript(_MemberData.GetClassModelByClassId(member.ClassId).scriptId).script;
			memberThumbURI = member.Thumbnail;
			_GearData.UpdateMemberGearSpec(member);
			
			Gear suitGear = _GearData.GetGearByID(member.SuitGearId);
			Gear bodyGear = _GearData.GetGearByID(member.BodyGearId);
			Gear engineGear = _GearData.GetGearByID(member.EngineGearId);
			Gear weapon1Gear = _GearData.GetGearByID(member.Weapon1GearId);
			Gear weapon2Gear = _GearData.GetGearByID(member.Weapon2GearId);
			Gear weapon3Gear = _GearData.GetGearByID(member.Weapon3GearId);
			
			GearTitle.text = _ScriptData.GetGameScript(160139).script;
			GearSuitStr.text = _ScriptData.GetGameScript(160134).script + " : " + _ScriptData.GetGameScript(suitGear.scriptId).script;
			GearBodyStr.text = _ScriptData.GetGameScript(160135).script + " : " + _ScriptData.GetGameScript(bodyGear.scriptId).script;
			GearEngineStr.text = _ScriptData.GetGameScript(160136).script + " : " + _ScriptData.GetGameScript(engineGear.scriptId).script;
			GearWeapon1Str.text = _ScriptData.GetGameScript(160137).script + " : " + _ScriptData.GetGameScript(weapon1Gear.scriptId).script;
			if(weapon2Gear != null) GearWeapon2Str.text = _ScriptData.GetGameScript(160138).script + " : " + _ScriptData.GetGameScript(weapon2Gear.scriptId).script;
			if(weapon3Gear != null) GearWeapon3Str.text = _ScriptData.GetGameScript(160139).script + " : " + _ScriptData.GetGameScript(weapon3Gear.scriptId).script;

			OptionBtn1.GetComponent<CommonBtn>().SetEnabled(true);
			OptionBtn1.GetComponent<CommonBtn>().SetBtnName(_ScriptData.GetGameScript(120507).script);


			if(member.UnitId != 0) {
				OptionBtn1.GetComponent<CommonBtn>().SetEnabled(true);
				OptionBtn1.GetComponent<CommonBtn>().SetBtnName(_ScriptData.GetGameScript(120508).script);
			} else {
				OptionBtn1.GetComponent<CommonBtn>().SetEnabled(true);
				OptionBtn1.GetComponent<CommonBtn>().SetBtnName(_ScriptData.GetGameScript(120507).script);
			}

		} else {
			OptionBtn1.GetComponent<CommonBtn>().SetEnabled(false);
			//OptionBtn2.GetComponent<CommonBtn>().SetEnabled(false);

			_MemberDataUI.MemberUpdate(0, false);
		}

		OptionBtn2.GetComponent<CommonBtn>().SetEnabled(true);
		OptionBtn3.GetComponent<CommonBtn>().SetEnabled(false);

	}

	// 유닛 정보 관련 ============================================================================================


	// 유닛 리스트 관련 ============================================================================================

	/** 부대 정보 노출 */
	private void ShowUnitData() {
		Unit unit = _UserData.GetUnitById(_CurrentUnitId);
		SubTitleTxt.GetComponent<TextMesh>().text = _ScriptData.GetUnitNameByUnitId(unit.id);
		
		Member member = null;
		DefaultMember defaultMember = null;
		short memberId;
		byte memberState;
		string memberThumb;
		for(byte i = 0; i < unit.memberList.Length; i++) {
			memberId = unit.memberList[i];
			member = null;
			if(memberId > 0) member = _UserData.GetMemberById(memberId);
			if(member != null) {
				defaultMember = _MemberData.GetDefaultMemberByID(member.DefaultId);
				memberThumb = "Member" + defaultMember.thumbId;
			} else {
				memberThumb = "Member000";
			}
			memberState = 0;
			if(member != null) memberState = member.state;
			switch(i) {
			case 0:
				ShowMemberThumb(MemberThumb1, memberThumb, memberState);
				break;
			case 1:
				ShowMemberThumb(MemberThumb2, memberThumb, memberState);
				break;
			case 2:
				ShowMemberThumb(MemberThumb3, memberThumb, memberState);
				break;
			case 3:
				ShowMemberThumb(MemberThumb4, memberThumb, memberState);
				break;
			case 4:
				ShowMemberThumb(MemberThumb5, memberThumb, memberState);
				break;
			}
		}
		
		ShowMemberData();
	}

	// 유닛 리스트 관련 ============================================================================================


	// 하단 유닛 설정 메뉴 관련 ============================================================================================

	// 하단 유닛 설정 메뉴 관련 ============================================================================================


	// 마우스 이벤트 ============================================================================================
	/** 편대 해산 확인 */
	private void OnUnitDelete(int btnId) {
		_UserData.SetConfirmPop(_ScriptData.GetGameScript(150137).script, DeleteUnit, new BillModel());

	}

	/** 편대 해산 */
	private void DeleteUnit(bool isConfirm) {

		if(isConfirm == false) return;
		bool isUnitDel = _UserData.RemoveUnit(_CurrentUnitId);
		
		if(isUnitDel) {

			_CurrentUnitId = 1;
			_CurrentSelectSlot = 1;
			Unit unit = _UserData.UserUnitList[0] as Unit;
			_CurrentMemberId = unit.memberList[0];

			ListMenuChange(0);
			ShowUnitData();

			MemberSelectBox.transform.position = MemberThumb1.transform.position;
			
		}
	}

	private void OnPopupClose() {
		if(_CloseCallback != null) _CloseCallback();
	}

	private void OnListTypeChange(int id) {
		if(ListMenu.transform.position.x > 10) {	// 현재 맴버 리스트.
			ListMenuChange(0);
		} else {
			ListMenuChange(1);
		}
	}

	/** 대원 등록 및 대원 해제. */
	private void OnOptionBtn1Click(int id) {

		Member member = _UserData.GetMemberById(_CurrentMemberId);
		Unit unit = _UserData.GetUnitById(_CurrentUnitId);
			 
		if(member.UnitId == 0) {	// 등록 혹은 교체.
			Member masterMember = _UserData.GetMemberById(unit.memberList[0]);
			if(_CurrentSelectSlot != 1 && masterMember != null && masterMember.ClassId <= member.ClassId) {
				_UserData.SetAlert(_ScriptData.GetGameScript(150113).script, new BillModel());
				return;
			}

			// 편대장으로의 계급이 부족합니다. 
			byte maxClass = 0;
			Member checkMember;
			if(_CurrentSelectSlot == 1) {
				byte classIndex = 0;
				foreach(short memberId in unit.memberList) {
					if(classIndex == 0) {
						classIndex ++;
						continue;
					}
					checkMember = _UserData.GetMemberById(memberId);
					if(checkMember != null) {
						if(maxClass < checkMember.ClassId) maxClass = checkMember.ClassId;
					}
					classIndex ++;
				}
				if(maxClass >= member.ClassId) {
					_UserData.SetAlert(_ScriptData.GetGameScript(150115).script, new BillModel());
					return;
				}
			}

			Member prevMember = _UserData.GetMemberById(unit.memberList[_CurrentSelectSlot - 1]);
			if(prevMember != null) prevMember.UnitId = 0;

			unit.memberList[_CurrentSelectSlot - 1] = member.id;
			member.UnitId = unit.id;
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Corps_Member_Change, 1);
		} else {	// 해제 
			if(_CurrentSelectSlot == 1) {	// 편대장 자리를 비울수 없음.
				_UserData.SetAlert(_ScriptData.GetGameScript(150112).script, new BillModel());
				return;
			} else {
				member.UnitId = 0;
				unit.memberList[_CurrentSelectSlot - 1] = 0;
			}

		}

		ListMenuChange(1);
		ShowUnitData();

		LocalData.getInstence().UserUnitDataSave();
		LocalData.getInstence().UserMemberDataSave();
	}

	/** 대원 제거 */
	private void OnOptionBtn2Click(int id) {

	}
	
	private void OnOptionBtn3Click(int id) {

		// 유닛 장비 노출.
		
	}

	/** 편대의 맴버 썸네일 클릭시 */
	private void OnMemberSlotClick(int id) {

		Unit unit = _UserData.GetUnitById(_CurrentUnitId);

		switch(id) {
		case 1:
			MemberSelectBox.transform.position = MemberThumb1.transform.position;
			break;
		case 2:
			MemberSelectBox.transform.position = MemberThumb2.transform.position;
			break;
		case 3:
			MemberSelectBox.transform.position = MemberThumb3.transform.position;
			break;
		case 4:
			MemberSelectBox.transform.position = MemberThumb4.transform.position;
			break;
		case 5:
			MemberSelectBox.transform.position = MemberThumb5.transform.position;
			break;
		}
		_CurrentSelectSlot = (byte)(id);
		_CurrentMemberId = unit.memberList[_CurrentSelectSlot - 1];

		ListMenuChange(1);
		ShowMemberData();
	}

	/** 부대 리스트 클릭시 */
	private void OnUnitListClick(int id) {
		if(id == 0) {	// 신규 편대.
			
			ArrayList userUnitList = _UserData.UserUnitList;
			foreach(Unit unit in userUnitList) {
				if(unit.memberList[0] == 0) {
					_UserData.SetAlert(_ScriptData.GetGameScript(150111).script, new BillModel());
					return;
				}
			}
			
			short[] memberIdList = new short[5];
			memberIdList[0] = 0;
			memberIdList[1] = 0;
			memberIdList[2] = 0;
			memberIdList[3] = 0;
			memberIdList[4] = 0;
			_UserData.AddUnit(memberIdList);
			Unit newUnit = _UserData.UserUnitList[_UserData.UserUnitList.Count - 1] as Unit;
			_CurrentUnitId = newUnit.id;
			
			_CurrentSelectSlot = 1;
			_CurrentMemberId = 0;

			ListMenuChange(1);

			if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("MakeUnit", userUnitList.Count.ToString());
			
		} else {
			_CurrentUnitId = (byte)(id);
			Unit unit = _UserData.GetUnitById(_CurrentUnitId);
			_CurrentSelectSlot = 1;
			_CurrentMemberId = unit.memberList[0];

		}

		MemberSelectBox.transform.position = MemberThumb1.transform.position;

		ShowUnitData();

		LocalData.getInstence().UserUnitDataSave();

	}

	/** 대원 리스트 클릭시 */
	private void OnMemberListClick(int id) {
		_CurrentMemberId = (byte)(id);
		ShowMemberData();

		GuideArrowManager.getInstence().ShowArrow(SceneType.CorpsPopup_MemberSelect);

	}

	// 마우스 이벤트 ============================================================================================


	// 기능 function ============================================================================================

	/** 대원 썸네일 노출 */
	private void ShowMemberThumb(GameObject thumbField, string str, byte markType) {

		SpriteRenderer renderer = (SpriteRenderer)thumbField.GetComponent ("SpriteRenderer");
		renderer.sprite = Resources.Load<Sprite>("MemberImg/" + str);

		GameObject stateMark = null;
		Transform stateMarkTrans = null;
		stateMarkTrans = this.gameObject.transform.FindChild(thumbField.name + "Mark") as Transform;
		if(stateMarkTrans == null) {
			stateMark = new GameObject();
			stateMark.transform.parent = this.gameObject.transform;
			stateMark.name = thumbField.name + "Mark";
			stateMark.AddComponent<SpriteRenderer>();
		} else {
			stateMark = stateMarkTrans.gameObject;
		}
		stateMark.transform.position = new Vector2(thumbField.transform.position.x - 0.2f, thumbField.transform.position.y + 0.3f);
		//stateMark.transform.localScale = new Vector2(thumbField.transform.localScale.x / 1.5f, thumbField.transform.localScale.y / 1.5f);
		SpriteRenderer markRenderer = stateMark.GetComponent<SpriteRenderer>();
		markRenderer.sortingOrder = 15;

		if(str != "Member000" && str != "") {
			if(markType == MemberStateType.Wound) {
				markRenderer.sprite = Resources.Load<Sprite>("Common/EndGameSymbol02");
			} else if(markType == MemberStateType.Mia) {
				markRenderer.sprite = Resources.Load<Sprite>("Common/EndGameSymbol03");
			} else {
				markRenderer.sprite = Resources.Load<Sprite>("");
			}
		} else {
			markRenderer.sprite = Resources.Load<Sprite>("");
		}
	}

	/** 편대에 대원을 추가함. */
	private void InsertMember() {
		Unit unit = _UserData.GetUnitById(_CurrentUnitId);
		if(_CurrentSelectSlot > 0) {

			DeleteMember();

			unit.memberList[_CurrentSelectSlot - 1] = _CurrentMemberId;
			Member member = _UserData.GetMemberById(_CurrentMemberId);
			member.UnitId = unit.id;
		}

		SetMemberList();
		ShowUnitData();

		LocalData.getInstence().UserMemberDataSave();
		LocalData.getInstence().UserUnitDataSave();
	}

	/** 편대에서 대원을 제거함. */
	private void DeleteMember() {
		Unit unit = _UserData.GetUnitById(_CurrentUnitId);
		if(_CurrentSelectSlot > 0) {
			short prevMemberId = unit.memberList[_CurrentSelectSlot - 1];
			if(prevMemberId > 0) {
				Member prevMember = _UserData.GetMemberById(prevMemberId);
				prevMember.UnitId = 0;
				unit.memberList[_CurrentSelectSlot - 1] = 0;
			}
		}

		SetMemberList();
		ShowUnitData();

		LocalData.getInstence().UserMemberDataSave();
		LocalData.getInstence().UserUnitDataSave();
	}

	public class MemberOutListMPSort : IComparer
	{
		public int Compare(object x, object y)
		{
			Member xMember = UserData.getInstence().GetMemberById((short)(x));
			Member yMember = UserData.getInstence().GetMemberById((short)(y));

			// reverse the arguments
			int g1 = xMember.CurrentMP;
			int g2 = yMember.CurrentMP;
			
			if (g1 > g2)
				return -1;
			else
				return 0;
		}
		
	}

	public class MemberOutListHPSort : IComparer
	{
		public int Compare(object x, object y)
		{
			Member xMember = UserData.getInstence().GetMemberById((short)(x));
			Member yMember = UserData.getInstence().GetMemberById((short)(y));
			
			// reverse the arguments
			int g1 = xMember.CurrentHP;
			int g2 = yMember.CurrentHP;
			
			if (g1 > g2)
				return -1;
			else
				return 0;
		}
		
	}

	// 기능 function ============================================================================================
}
