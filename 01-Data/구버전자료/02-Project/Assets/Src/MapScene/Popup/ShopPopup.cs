using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Prime31;

public class ShopPopup : MonoBehaviour {

	public bool isTest;
	public GameObject CloseBtn;

	public GameObject TitleTxt;

	public GameObject CoreBtn1;
	public GameObject CoreBtn2;
	public GameObject CoreBtn3;
	public GameObject CoreBtn4;

	public GameObject PayBtn1;
	public GameObject PayBtn2;
	public GameObject PayBtn3;
	public GameObject PayBtn4;

	public Color ValueFontColor;
	public Color ValueOutlineColor;

	public delegate void CloseEvent();
	private CloseEvent _CloseCallback;

	private ShopData _ShopData;
	private GameObject[] _BtnList;

	private string _GoogleLog = "";
	private string _errorMsg = "";
	private byte _productid = 0;

	private GameObject _ShopComment;

	void Awake() {
		DarkSprite.getInstence();

	}

	void Start() {
		if(isTest) {
			LocalData.getInstence().AllLoad();
			init();
		}
	}

#if UNITY_ANDROID

	void OnEnable()
	{

		GoogleIABManager.purchaseSucceededEvent += ShopAPICallComplete;
		GoogleIABManager.purchaseFailedEvent += ShopAPICallFailure;
		GoogleIABManager.consumePurchaseSucceededEvent += ShopAPIConsumeComplete;
		GoogleIABManager.consumePurchaseFailedEvent += ShopAPIConsumeFailure;
	}
	
	
	void OnDisable()
	{
		GoogleIABManager.purchaseSucceededEvent -= ShopAPICallComplete;
		GoogleIABManager.purchaseFailedEvent -= ShopAPICallFailure;
		GoogleIABManager.consumePurchaseSucceededEvent -= ShopAPIConsumeComplete;
		GoogleIABManager.consumePurchaseFailedEvent -= ShopAPIConsumeFailure;
	}

#endif
	
	public void init() {

		_ShopData = ShopData.getInstence();

		TitleTxt.GetComponent<TextMesh>().text = ScriptData.getInstence().GetGameScript(120800).script;

		_BtnList = new GameObject[8]{CoreBtn1, CoreBtn2, CoreBtn3, CoreBtn4, PayBtn1, PayBtn2, PayBtn3, PayBtn4};

		CloseBtn.GetComponent<ButtonEvent>().SetCallBack(OnPopupClose);

		ShopValueModel[] modelList = _ShopData.ShopDataList;

		byte index = 1;
		foreach(GameObject btn in _BtnList) {
			ShopValueModel model = modelList[index - 1];
			SetBtn(model.id, "x" + model.value, model.wonCost, btn);

			index ++;
		}

		if(UserData.getInstence().UserBannerStandByCount <= 0) {
			string commentstr = ScriptData.getInstence().GetGameScript(150149).script;
			_ShopComment = CustomTextMesh.SetAddTextMesh(commentstr, 14, TextAnchor.MiddleCenter, Color.white, 30, "GamePopup");
			_ShopComment.transform.parent = this.gameObject.transform;
			_ShopComment.transform.position = new Vector2(0f, -2.51f);
		}

		ShopAPIAllConsume();

#if UNITY_ANDROID

		var key = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAg/xyXZxAl+U2OS3t3uXeXdylwEfLHBMYELCG+h4U8ispNqnh9sw20UudIoE5EkYKhw//N9qUtB/Zz+eDgXQUZoMiHZbR0Dor45X/m29yAWlwiWMGXPnXnwr/yrog5G+vBoD74e2KVRMn53VXIU2FLEfBJot8J0LeQNVDG3A/J1q502iKzovBAoza4fykH9VxC9JqUL3dSztk/WmWqVaDJr/eXLwa9i6PWecNlcB+kWxf81lgy9tuGj9iZAYQiUiumJkWJ5FvV4UufD1zL6w1AGZWE9EMW7E+WOBal19lngPW/PeehjqaYPas4XpMi8y/r4sxAj+dnxJYtnr/z43QdwIDAQAB";
		GoogleIAB.init( key );

#endif

	}

	private void SetBtn(byte id, string str, int cost, GameObject btnObj) {
		btnObj.GetComponent<CommonBtn>().Init(id, str, 1, Color.white);
		btnObj.GetComponent<CommonBtn>().SetTxtSize(18);
		btnObj.GetComponent<CommonBtn>().SetClick(OnShopClick);

		GameObject valueTxt = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
		valueTxt.transform.parent = this.gameObject.transform;
		valueTxt.GetComponent<OutLineFont>().SetString(cost + "원");
		valueTxt.GetComponent<OutLineFont>().SetSort(10);
		valueTxt.GetComponent<OutLineFont>().SetFontSize(16);
		ValueFontColor.a = 1f;
		valueTxt.GetComponent<OutLineFont>().SetFontColor(ValueFontColor);
		ValueOutlineColor.a = 1f;
		valueTxt.GetComponent<OutLineFont>().SetLineColor(ValueOutlineColor);
		valueTxt.transform.position = new Vector3(btnObj.transform.position.x, btnObj.transform.position.y - 0.5f, 0f);
	}

	private void OnShopClick(int id) {
		ShopAPICall(id);
		/*
		ShopValueModel model = _ShopData.GetShopValueModelByID((byte)(id));

		BillModel billModel = new BillModel();
		if(model.type == 1) {	// 코어.
			UserData.getInstence().UserCores += model.value;
			billModel.core = model.value;
			billModel.corePlus = true;
		} else {	// 자금.
			UserData.getInstence().UserMoney += model.value;
			billModel.money = model.value;
			billModel.moneyPlus = true;
		}
		UserData.getInstence().UpdatePayData(billModel, new Vector2(0f, 0f));
		*/
	}

// 결제 관련 =======================================================================================
	// 결제 콜.
	private void ShopAPICall(int id) {

		if(_productid > 0) return;

		_productid = (byte)(id);

		ShopValueModel model = _ShopData.GetShopValueModelByID(_productid);

		if(Application.platform == RuntimePlatform.WindowsEditor) {
			ShopPurchaseComplete();
		} else {

#if UNITY_ANDROID

			if(SystemData.GetInstance().IsShopPurchase == true) {
				//GoogleIAB.purchaseProduct( model.androidId, model.id.ToString());
				_GoogleLog = model.androidId;
				GoogleIAB.purchaseProduct(model.androidId, model.androidId);
			} else {
				ShopPurchaseComplete();
			}
		

#endif
		}
	}

#if UNITY_ANDROID

	private void ShopAPIAllConsume() {

		string[] strList = new string[_ShopData.ShopDataList.Length];

		ShopValueModel[] modelList = _ShopData.ShopDataList;

		int index = 0;
		foreach(ShopValueModel model in modelList) {
			strList[index] = model.androidId;
			index ++;
		}

		GoogleIAB.consumeProducts(strList);
	}

// 소모 아이템 체크 완료. ====================================================
	// 상품 소모 완료.
	private void ShopAPIConsumeComplete(GooglePurchase purchase) {

	}

	// 상품 소모 실패.
	private void ShopAPIConsumeFailure(string error) {

		_errorMsg += _productid + "/" + _GoogleLog + " = \n" + error;

	}


// 소모 아이템 체크 완료. ====================================================

	// 결제 완료.
	private void ShopAPICallComplete(GooglePurchase purchase ) {

		ShopValueModel model = _ShopData.GetShopValueModelByID(_productid);
		GoogleIAB.consumeProduct( model.androidId);
		ShopPurchaseComplete();
	}


	// 결제 실패.
	private void ShopAPICallFailure(string error ) {

		ShopValueModel model = _ShopData.GetShopValueModelByID(_productid);
		GoogleIAB.consumeProduct(model.androidId);

		_errorMsg += model.androidId + "/" + _GoogleLog + " ! \n" + error;
		_productid = 0;
	}

#endif

	private void ShopPurchaseComplete() {

		ShopValueModel model = _ShopData.GetShopValueModelByID(_productid);

		BillModel billModel = new BillModel();
		if(model.type == 1) {	// 코어.
			UserData.getInstence().UserCores += model.value;
			billModel.core = model.value;
			billModel.corePlus = true;
		} else {	// 자금.
			UserData.getInstence().UserMoney += model.value;
			billModel.money = model.value;
			billModel.moneyPlus = true;
		}

		if(UserData.getInstence().UserBannerStandByCount < SystemData.GetInstance().UserMaxBannerCount) {
			UserData.getInstence().UserBannerStandByCount += SystemData.GetInstance().UserMaxBannerCount;
		}
		UserData.getInstence().UpdatePayData(billModel, new Vector2(0f, 0f));
		_productid = 0;
	}


// 결제 관련 =======================================================================================

	public void SetCloseEventCallBack(CloseEvent onCloseEvent) {
		_CloseCallback = new CloseEvent(onCloseEvent);
		
	}

	private void OnPopupClose() {
		if(_CloseCallback != null) _CloseCallback();
	}

	void OnGUI() {
		if(SystemData.GetInstance().GameServiceType == ServiceType.ALPHA) GUI.Label(new Rect(10, 200, 1000, 200), _errorMsg);
	}
}
