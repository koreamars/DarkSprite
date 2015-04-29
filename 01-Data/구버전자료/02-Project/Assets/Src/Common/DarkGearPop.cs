using UnityEngine;
using System.Collections;

public class DarkGearPop : MonoBehaviour {

	public static byte BaseType = 0;
	public static byte FightType = 1;

	public GameObject CoreMergingBtn;
	public GameObject GetMark;
	public GameObject CancelBtn;
	public GameObject CloseBtn;
	public GameObject KakaoBtn;
	public GameObject BgObject;
	public bool IsTest;
	
	private GameObject _BillSB;
	private GameObject _titleTxt;
	private GameObject _GearDataViewer;
	private GameObject _RewardViewer;
	private GameObject _SucessPTxt;
	
	public delegate void CloseCallback(bool isComplete, bool isSuccess);
	private CloseCallback _CloseCallback;

	private short _CurrentDarkGearId = 0;
	private byte _CurrentType = 0;
	private int _SortNum = 0;
	private string _NameStr;
	private string _ThumbUrl;
	private bool _IsComplete = false;	// 시도 유무 판단.
	private bool _IsSuccess = false;	// 코어머징 성공 유무.

	private BillModel _CurrentBillModel;
	
	void Start() {

		if(IsTest) {
			init(10, 10, 0);
		}
	}

	public void init(short gearId, byte sortingNum, byte type) {

		_CurrentDarkGearId = gearId;
		_CurrentType = type;
		_SortNum = sortingNum;

		string btnName = ScriptData.getInstence().GetGameScript(160106).script;
		string cancelBtnName = ScriptData.getInstence().GetGameScript(160107).script;
		
		CoreMergingBtn.GetComponent<CommonBtn>().Init(0, btnName, _SortNum + 2, Color.white);
		CoreMergingBtn.GetComponent<CommonBtn>().SetClick(OnSetDarkGearClick);
		CoreMergingBtn.transform.position = new Vector3(-1.24f, -2.14f, -5f);

		CancelBtn.GetComponent<CommonBtn>().Init(0, cancelBtnName, _SortNum + 2, Color.white);
		CancelBtn.GetComponent<CommonBtn>().SetClick(PopupClose);
		CancelBtn.transform.position = new Vector3(1.24f, -2.14f, -5f);

		KakaoBtn.GetComponent<ButtonEvent>().SetCallBack(OnKakaoBtn);
		KakaoBtn.renderer.enabled = false;
		KakaoBtn.collider.enabled = false;
		KakaoBtn.transform.position = new Vector3(0f, -1.99f, -5f);

		CloseBtn.GetComponent<ButtonEvent>().SetCallBack(OnCloseBtn);
		CloseBtn.renderer.enabled = false;
		CloseBtn.collider.enabled = false;
		CloseBtn.transform.position = new Vector3(2.96f, 1.8f, -5f);

		GetMark.renderer.enabled = false;

		KakaoBtn.renderer.sortingOrder = _SortNum + 1;
		CloseBtn.renderer.sortingOrder = _SortNum + 1;
		BgObject.renderer.sortingOrder = _SortNum;
		GetMark.renderer.sortingOrder = _SortNum + 13;

		ShowGearView();
	}

	public void ShowGearView() {
		Gear gear = GearData.getInstence().GetGearByID(_CurrentDarkGearId);

		_CurrentBillModel = new BillModel();

		if(gear != null) {
			if(_CurrentType == DarkGearPop.BaseType) {
				_CurrentBillModel.core = gear.makeResource;
				if(_CurrentBillModel.core <= 0) _CurrentBillModel.corechip = 2;
				_CurrentBillModel.money = gear.makeCost;
			} else {
				_CurrentBillModel.core = 1;
			}
		}

		if(_GearDataViewer == null) {
			string titleStr = ScriptData.getInstence().GetGameScript(150156).script;
			_titleTxt = CustomTextMesh.SetAddTextMesh(titleStr, 18, TextAnchor.UpperCenter, Color.white, _SortNum + 1, "Alert");
			_titleTxt.transform.position = new Vector2(0f, 2.19f);
			_titleTxt.transform.parent = this.gameObject.transform;

			_GearDataViewer = Instantiate(Resources.Load<GameObject>("Common/GearDataUI")) as GameObject;
			_GearDataViewer.GetComponent<GearDataUI>().init();
			_GearDataViewer.GetComponent<GearDataUI>().SetSorting(_SortNum + 2);
			_GearDataViewer.GetComponent<GearDataUI>().SetSortLayer("Alert");
			_GearDataViewer.transform.localScale = new Vector2(0.8f, 0.8f);
			_GearDataViewer.transform.position = new Vector2(-1.75f, -0.19f);
			_GearDataViewer.transform.parent = this.gameObject.transform;

			_SucessPTxt = CustomTextMesh.SetAddTextMesh("성공확률 : 20%", 15, TextAnchor.MiddleCenter, Color.white, _SortNum + 1, "Alert");
			_SucessPTxt.transform.position = new Vector2(0f, -1.57f);
			_SucessPTxt.transform.parent = this.gameObject.transform;
		}
		_GearDataViewer.GetComponent<GearDataUI>().GearUpdate(_CurrentDarkGearId);

		string SuccessTxt = ScriptData.getInstence().GetGameScript(160169).script;
		if(_CurrentType == DarkGearPop.BaseType) {
			_SucessPTxt.GetComponent<TextMesh>().text = SuccessTxt + " : " + SystemData.GetInstance().DarkgearSuccessValue + "%";
		} else {
			_SucessPTxt.GetComponent<TextMesh>().text = SuccessTxt + " : 50%";
		}

		if(gear != null && gear.upNextId > 0) {
			_RewardViewer = Instantiate(Resources.Load<GameObject>("Common/RewardViewer")) as GameObject;
			_RewardViewer.transform.parent = this.gameObject.transform;
			_RewardViewer.GetComponent<RewardViewer>().init(_CurrentBillModel, _SortNum + 1, true);
			_RewardViewer.GetComponent<RewardViewer>().SetLayerSort(LayerMask.NameToLayer("Alert"));
			_RewardViewer.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
			_RewardViewer.transform.position = new Vector2(0f, 1.14f);
		}
	}

	public void SetCloseCallback(CloseCallback OnCloseCallback) {
		_CloseCallback = new CloseCallback(OnCloseCallback);
	}

	/** 다크 기어 시작 */
	private void OnSetDarkGearClick(int id) {

		// 필요 자원 확인.
		if(IsTest == false) {
			if(UserData.getInstence().UserMoney < _CurrentBillModel.money) {
				_CurrentBillModel.moneyPlus = false;
				UserData.getInstence().SetAlert(ScriptData.getInstence().GetGameScript(150109).script, _CurrentBillModel);
				PopupClose(0);
				return;
			}
			if(UserData.getInstence().UserCores < _CurrentBillModel.core) {
				_CurrentBillModel.corePlus = false;
				UserData.getInstence().SetAlert(ScriptData.getInstence().GetGameScript(150109).script, _CurrentBillModel);
				PopupClose(0);
				return;
			}
			if(UserData.getInstence().UserChips < _CurrentBillModel.corechip) {
				_CurrentBillModel.corechipPlus = false;
				UserData.getInstence().SetAlert(ScriptData.getInstence().GetGameScript(150109).script, _CurrentBillModel);
				PopupClose(0);
				return;
			}
		}

		CoreMergingBtn.GetComponent<CommonBtn>().SetEnabled(false);
		if(_RewardViewer != null) {
			Destroy(_RewardViewer);
			_RewardViewer = null;
		}

		iTween.ColorTo(_GearDataViewer, iTween.Hash("r", 10f, "g", 10f, "b", 10f, "time", 0.5f
		                                            , "oncomplete", "OnCompleteDarkGear", "oncompletetarget", this.gameObject));

	}

	/** 다크 기어 확인 */
	private void OnCompleteDarkGear() {

		Gear gear = GearData.getInstence().GetGearByID(_CurrentDarkGearId);

		// 자원 소비.
		if(_CurrentBillModel.money > 0) UserData.getInstence().UserMoney -= _CurrentBillModel.money;
		if(_CurrentBillModel.core > 0) UserData.getInstence().UserCores -= _CurrentBillModel.core;
		if(_CurrentBillModel.corechip > 0) UserData.getInstence().UserChips -= _CurrentBillModel.corechip;
		UserData.getInstence().UpdatePayData(_CurrentBillModel, new Vector2(0f, 0f));

		// 장비 소비.
		if(_CurrentType == DarkGearPop.BaseType) {
			OwnGear ownGear = UserData.getInstence().GetOwnGearByGearId(gear.id);
			if(ownGear.ownCount > 0) ownGear.ownCount -= 1;
		}

		if(gear.upNextId > 0) {
			if(CheckGetDarkGear()) {	// 다크 기어 성공시.

				_IsSuccess = true;
				_CurrentDarkGearId = gear.upNextId;

				GetMark.renderer.enabled = true;
				if(_CurrentType == DarkGearPop.BaseType) {
					OwnGear newOwnGear = UserData.getInstence().GetOwnGearByGearId(_CurrentDarkGearId);
					if(newOwnGear != null) {
						newOwnGear.ownCount += 1;
					} else {
						newOwnGear = new OwnGear();
						newOwnGear.gearId = _CurrentDarkGearId;
						newOwnGear.ownCount = 1;
						UserData.getInstence().UserOwnGearList.Add(newOwnGear);
					}
				}

				Gear newGearData = GearData.getInstence().GetGearByID(_CurrentDarkGearId);

				_NameStr = ScriptData.getInstence().GetGameScript(newGearData.scriptId).script;
				_ThumbUrl = "gear/" + newGearData.thumbnailURI + ".jpg";

				KakaoBtn.renderer.enabled = true;
				KakaoBtn.collider.enabled = true;
				CloseBtn.renderer.enabled = true;
				CloseBtn.collider.enabled = true;

				if(IsTest == false) MissionData.getInstence().AddMissionGoal(MissionGoalType.Get_DarkGear, 1);
				CancelBtn.GetComponent<CommonBtn>().SetEnabled(false);
				_SucessPTxt.GetComponent<TextMesh>().renderer.enabled = false;
			} else {					// 다크 기어 실패시.

				if(_CurrentType == DarkGearPop.BaseType) {
					_CurrentDarkGearId = 0;

					string failTxtStr = ScriptData.getInstence().GetGameScript(150157).script;
					_titleTxt.GetComponent<TextMesh>().text = failTxtStr;

				} else {
					switch(gear.gearType) {
					case GearType.Suit:
						_CurrentDarkGearId = 1;
						break;
					case GearType.Body:
						_CurrentDarkGearId = 2;
						break;
					case GearType.Engine:
						_CurrentDarkGearId = 3;
						break;
					case GearType.Weapon_Gun:
						_CurrentDarkGearId = 4;
						break;
					case GearType.Weapon_Missle:
					case GearType.Weapon_Rocket:
						_CurrentDarkGearId = 0;
						break;
					}
				}
				CancelBtn.GetComponent<CommonBtn>().SetEnabled(true);
			}

			ShowGearView();
		}

		_IsComplete = true;
		LocalData.getInstence().UserOwnGearDataSave();

		// 다음 기어 정보.
		CoreMergingBtn.GetComponent<CommonBtn>().SetEnabled(false);

		iTween.ColorTo(_GearDataViewer, iTween.Hash("r", 1f, "g", 1f, "b", 1f, "time", 0.5f));
	}

	private bool CheckGetDarkGear() {
		bool IsCheck = false;

		if(UserData.getInstence().StoryStepId == 28) {
			print("튜토리얼 강제 성공.");
			return true;
		}

		if(_CurrentType == DarkGearPop.BaseType) {
			if(UnityEngine.Random.Range(0, 100) <= SystemData.GetInstance().DarkgearSuccessValue) IsCheck = true;
		} else {
			if(UnityEngine.Random.Range(0, 100) <= 50) IsCheck = true;
		}

		if(IsCheck) {
			SystemData.GetInstance().DarkgearSuccessValue = 20;
		} else {
			if(SystemData.GetInstance().DarkgearSuccessValue < 80) SystemData.GetInstance().DarkgearSuccessValue += 10;
		}

		return IsCheck;
	}

	private void OnCloseBtn() {
		PopupClose(0);
	}

	private void OnKakaoBtn() {
		print("OnKakaoBtn");
		string paramStr = "itemName=" + _NameStr + "&thumburl=http://clana.kr/DarkSprite/images/" + _ThumbUrl;
		NetworkData.getInstence().SendKakao(paramStr, _NameStr);
		Destroy(this.gameObject);
	}

	private void PopupClose(int id) {
		if(_CloseCallback != null) _CloseCallback(_IsComplete, _IsSuccess);
		Destroy(this.gameObject);
	}



}
