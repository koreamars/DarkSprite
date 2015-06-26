using UnityEngine;
using System.Collections;

public class UnitSelectPopup : MonoBehaviour {

	public bool isTest;
	public GameObject CloseBtn;
	public GameObject SelectList;
	public GameObject TitleTxt;
	public GameObject PopupBg;
	public int SortingNum;
	public Color ErrorFontColor;

	private TextMesh _TitleTxtMesh;
	private ScrollMenu _ScrollMenu;
	private ButtonEvent _CloseCommonBtn;

	private UserData _UserData;

	public delegate void CloseCallBack();
	private CloseCallBack _CloseCallBack;

	public delegate void SelectCallBack(int id);
	private SelectCallBack _SelectCallBack;

	private GameObject _EngageBtn;
	private GameObject _FightDataObj;
	private GameObject _FightCommentObj;

	private ListMenuModel[] _ListMenuModel;

	private GameObject[] _MemberDataUIList;

	private byte _currentUnitId;
	private int _memberTotalCost = 0;

	void Awake () {
		_UserData = UserData.getInstence();

		_TitleTxtMesh = TitleTxt.GetComponent<TextMesh>();
		_ScrollMenu = SelectList.GetComponent<ScrollMenu>();
		_CloseCommonBtn = CloseBtn.GetComponent<ButtonEvent>();
		_CloseCommonBtn.SetCallBack(OnCloseEvent);

		PopupBg.GetComponent<Renderer>().sortingOrder = SortingNum - 2;
		CloseBtn.GetComponent<Renderer>().sortingOrder = SortingNum;
		TitleTxt.GetComponent<Renderer>().sortingOrder = SortingNum;
		_ScrollMenu.sortingNum = SortingNum;

		_TitleTxtMesh.text = ScriptData.getInstence().GetGameScript(110005).script;

		if(isTest) {
			LocalData.getInstence().AllLoad();
			Init(0, 10, testFunc1, testFunc2);
		}
	}

	public void testFunc1() {

	}

	public void testFunc2(int id) {
		
	}

	public void Init(byte type,int SortingNum, CloseCallBack OnCloseCallBack, SelectCallBack OnSelectCallBack) {
		_CloseCallBack = new CloseCallBack(OnCloseCallBack);
		_SelectCallBack = new SelectCallBack(OnSelectCallBack);

		ArrayList unitList = _UserData.UserUnitList;
		_ListMenuModel = null;
		_ListMenuModel = new ListMenuModel[unitList.Count];
		short index = 0;
		Unit currentUnit = null;
		//int ghostDamage = 0;
		foreach(Unit unit in unitList) {
			_ListMenuModel[index] = new ListMenuModel();
			_ListMenuModel[index].id = unit.id;
			if(index == 0) {
				_currentUnitId = unit.id;
				currentUnit = unit;
			}
			byte memberCount = 0;
			int totalHP = 0;
			int currentHP = 0;
			Member member = null;
			foreach(byte memberId in unit.memberList) {
				if(memberId > 0) {
					memberCount ++;
					member = _UserData.GetMemberById(memberId);
					if(member != null) {
						totalHP += member.MaxHP;
						currentHP += member.CurrentHP;
					}
					//ghostDamage += member.ClassId * 5;

				}

			}
			if(memberCount > 0) {
				int hpState = (int)(((float)(currentHP) / (float)(totalHP)) * 100f);
				string memberCountStr = memberCount + ScriptData.getInstence().GetGameScript(160103).script;
				string btnStr = ScriptData.getInstence().GetUnitNameByUnitId(unit.id) + " <size=18>[ " + memberCountStr + " | " + hpState + "% ]</size>";
				_ListMenuModel[index].scriptString = "<size=20>" + btnStr + "</size>";
			} else {
				_ListMenuModel[index].id = 0;
				_ListMenuModel[index].scriptString = "<size=20>" + ScriptData.getInstence().GetGameScript(150117).script + "</size>";
				_ListMenuModel[index].fontColor = ErrorFontColor;
			}

			index ++;
		}

		PopupBg.GetComponent<Renderer>().sortingOrder = SortingNum - 2;
		CloseBtn.GetComponent<Renderer>().sortingOrder = SortingNum;
		TitleTxt.GetComponent<Renderer>().sortingOrder = SortingNum;
		_ScrollMenu.sortingNum = SortingNum + 1;
		_ScrollMenu.BtnColor = Color.white;
		_ScrollMenu.SetMenuClick(OnSelectClickEvent);
		_ScrollMenu.SetScrollData(_ListMenuModel, 0);
		_ScrollMenu.SetScrollView();

		_EngageBtn = Instantiate(Resources.Load<GameObject>("Common/CommonBtn02")) as GameObject;
		_EngageBtn.GetComponent<CommonBtn>().Init(0, ScriptData.getInstence().GetGameScript(130200).script, SortingNum + 5, Color.white);
		_EngageBtn.transform.parent = this.gameObject.transform;
		_EngageBtn.transform.localScale = new Vector2(0.9f, 0.9f);
		_EngageBtn.transform.position = new Vector2(0f, -4f);
		_EngageBtn.GetComponent<CommonBtn>().SetClick(OnEngageClick);

		_MemberDataUIList = new GameObject[5];
		UpdateMemberView();

		WarzoneData warzoneData = WarzoneData.getInstence();

		// 고스트 노출.
		byte currentTownType = warzoneData.CurrentTownType;

		ArrayList enemyList = null;

		string commentStr = "";

		switch(currentTownType) {
		case TownStateType.Town:	// 마을 방어.
			print("TownStateType.Town");
			UserTown userTown = warzoneData.GetUserTownByID(warzoneData.CurrentTownId);
			enemyList = warzoneData.GetGhostDataByGhostClose(userTown.invasionGhostClose);
			commentStr = ScriptData.getInstence().GetGameScript(150159).script;
			break;
		case TownStateType.Nest:	// 둥지 공격.
			print("TownStateType.Nest");
			GhostTown ghostTown = warzoneData.GetGhostTownByTownId(warzoneData.CurrentTownId);
			Town defaultTown = warzoneData.GetDefaultTownData(warzoneData.CurrentTownId);
			enemyList = warzoneData.GetGhostDataByGhostClose(defaultTown.maxClose);
			commentStr = ScriptData.getInstence().GetGameScript(150158).script;
			break;
		case TownStateType.Airship:
			print("TownStateType.Airship");
			AirshipDefense airshipDefense = warzoneData.GetAirshipDefenseById(warzoneData.CurrentTownId);
			enemyList = warzoneData.GetGhostDataByGhostClose(airshipDefense.ghostClose);
			commentStr = ScriptData.getInstence().GetGameScript(150160).script;
			break;
		}

		// 안내 멘트.
		_FightCommentObj = CustomTextMesh.SetAddTextMesh(commentStr, 14, TextAnchor.UpperRight, Color.white, 100, "Default");
		_FightCommentObj.GetComponent<TextMesh>().alignment = TextAlignment.Right;
		_FightCommentObj.transform.position = new Vector2(-3.38f, -3.00f);
		_FightCommentObj.transform.parent = this.gameObject.transform;

		if(enemyList.Count > 0) {
			//GameObject ghostObj;
			byte ghostindex = 0;
			foreach(byte ghostId in enemyList) {
				Ghost ghost = warzoneData.GetGhostByGhostId(ghostId);
				DefaultGhost defaultGhost = warzoneData.GetDefaultGhostByGhostId(ghost.defaultId);
				GameObject ghostObj = new GameObject();
				ghostObj.AddComponent<SpriteRenderer>();
				ghostObj.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("GhostImg/Thumbnail/" + defaultGhost.resourceURI);
				ghostObj.transform.position = new Vector2(-4.2f, 3.4f + (ghostindex * -1.4f));
				ghostObj.transform.localScale = new Vector2(0.75f, 0.75f);
				ghostObj.transform.parent = this.gameObject.transform;
				ghostindex ++;

				// 고스트 HP
				string ghostHpstr = ScriptData.getInstence().GetGameScript(160112).script + " : " + ghost.maxHP.ToString();
				GameObject ghostHpObj = CustomTextMesh.SetAddTextMesh(ghostHpstr, 11, TextAnchor.MiddleRight, Color.white, 100, "Default");
				ghostHpObj.transform.position = new Vector2(ghostObj.transform.position.x - 1f, ghostObj.transform.position.y + 0.25f);
				ghostHpObj.transform.parent = this.gameObject.transform;

				// 고스트 IA
				string ghostIAstr = ScriptData.getInstence().GetGameScript(160113).script + " : " + defaultGhost.IA.ToString();
				GameObject ghostIAObj = CustomTextMesh.SetAddTextMesh(ghostIAstr, 11, TextAnchor.MiddleRight, Color.white, 100, "Default");
				ghostIAObj.transform.position = new Vector2(ghostObj.transform.position.x - 1f, ghostObj.transform.position.y - 0f);
				ghostIAObj.transform.parent = this.gameObject.transform;

				// 고스트 AP
				Gear gear = GearData.getInstence().GetGearByID(defaultGhost.WeaponId);
				int minAP = gear.minAP;
				int maxAP = gear.maxAP;
				string ghostAPstr = ScriptData.getInstence().GetGameScript(160121).script + " : " + minAP + "~" + maxAP;
				GameObject ghostAPObj = CustomTextMesh.SetAddTextMesh(ghostAPstr, 11, TextAnchor.MiddleRight, Color.white, 100, "Default");
				ghostAPObj.transform.position = new Vector2(ghostObj.transform.position.x - 1f, ghostObj.transform.position.y - 0.25f);
				ghostAPObj.transform.parent = this.gameObject.transform;
			}
		}
	}

	private void ShowGhostTownData(int ghostDamage) {

		if(_FightDataObj != null) Destroy(_FightDataObj);

		// 둥지 공격력 표시.
		GhostTown ghostTown = WarzoneData.getInstence().GetGhostTownByTownId(WarzoneData.getInstence().CurrentTownId);
		if(ghostTown == null) return;
		string damageStr = ScriptData.getInstence().GetGameScript(160165).script + " : " + ghostDamage
			+ "<size=14>\n" + ScriptData.getInstence().GetGameScript(160145).script + " : " + ghostTown.ghostClose + "</size>";
		
		_FightDataObj = CustomTextMesh.SetAddTextMesh(damageStr, 20, TextAnchor.MiddleCenter, Color.white, 100, "Default");
		_FightDataObj.transform.position = new Vector2(0f, -3.10f);
		_FightDataObj.transform.parent = this.gameObject.transform;

	}

	private void OnCloseEvent() {
		if(_CloseCallBack != null) _CloseCallBack();
	}

	private void OnSelectClickEvent(int id) {
		if(id == 0) return;
		_currentUnitId = (byte)(id);

		UpdateMemberView();
	}

	private void UpdateMemberView() {
		Unit unit = _UserData.GetUnitById(_currentUnitId);
			 
		GameObject memberDataUIObj;
		int ghostDamage = 0;
		_memberTotalCost = 0;
		for(byte i = 0; i < 5; i++) {
			if(_MemberDataUIList[i] == null) {
				memberDataUIObj = Instantiate(Resources.Load<GameObject>("Common/MemberDataUI")) as GameObject;
				_MemberDataUIList[i] = memberDataUIObj;
			} else {
				memberDataUIObj = _MemberDataUIList[i] as GameObject;
			}
			MemberDataUI memberDataUI = memberDataUIObj.GetComponent<MemberDataUI>();
			memberDataUI.init(0);
			memberDataUIObj.transform.position = new Vector2(3.9f, 3.4f + (i * -1.7f));
			memberDataUIObj.transform.localScale = new Vector2(0.6f, 0.6f);
			memberDataUIObj.transform.parent = this.gameObject.transform;
			memberDataUI.MemberUpdate(unit.memberList[i], false);

			Member member = _UserData.GetMemberById(unit.memberList[i]);
			if(member != null) {
				ghostDamage += member.ClassId * SystemData.GetInstance().TownAttackPoint;
				_memberTotalCost += member.ClassId * SystemData.GetInstance().MemberClassCost;
			}


		}

		byte currentTownType = WarzoneData.getInstence().CurrentTownType;
		switch(currentTownType) {
		case TownStateType.Town:	// 마을 방어.
			break;
		case TownStateType.Nest:	// 둥지 공격.
			ShowGhostTownData(ghostDamage);
			break;
		case TownStateType.Airship:
			break;
		}
	}



	private void OnEngageClick(int id) {

		BillModel memberCostBill = new BillModel();
		memberCostBill.money = _memberTotalCost;
		memberCostBill.moneyPlus = false;
		if(memberCostBill.money <= _UserData.UserMoney) {
			_UserData.SetConfirmPop(ScriptData.getInstence().GetGameScript(150151).script, OnConfirm, memberCostBill);
		} else {
			_UserData.SetAlert(ScriptData.getInstence().GetGameScript(150152).script, memberCostBill);
		}
	}

	private void OnConfirm(bool isConfirm) {
		if(isConfirm == true) {
			BillModel memberCostBill = new BillModel();
			memberCostBill.money = _memberTotalCost;
			memberCostBill.moneyPlus = false;
			_UserData.UserMoney -= memberCostBill.money;
			_UserData.UpdatePayData(memberCostBill, new Vector2(0f, 0f));
			MissionData.getInstence().AddMissionGoal(MissionGoalType.UnitSelect_Fight_Start, 1);
			if(_SelectCallBack != null) _SelectCallBack(_currentUnitId);
		}
	}

}
