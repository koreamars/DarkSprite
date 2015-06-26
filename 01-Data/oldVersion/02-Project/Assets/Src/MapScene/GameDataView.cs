using UnityEngine;
using System.Collections;

public class GameDataView : MonoBehaviour {

	public GameObject CoinTxt;
	public GameObject CoreTxt;
	public GameObject DebrisTxt;

	public GameObject OptionBtn;
	public GameObject ShopBtn;

	private UserData _UserData;

	public delegate void PopupOpenEvent(short type);
	private PopupOpenEvent _PopupOpenEvent;

	void Awake () {
		_UserData = UserData.getInstence();
		
		GameLog.Log("GameDataView");
		CoinTxt.GetComponent<Renderer>().sortingOrder = -4;
		CoreTxt.GetComponent<Renderer>().sortingOrder = -4;
		DebrisTxt.GetComponent<Renderer>().sortingOrder = -4;

	}

	void Start() {
		OptionBtn.GetComponent<CommonBtn>().Init(0, ScriptData.getInstence().GetGameScript(120900).script, -3, Color.white);
		OptionBtn.GetComponent<CommonBtn>().SetClick(OnOptionClick);
		ShopBtn.GetComponent<CommonBtn>().Init(0, ScriptData.getInstence().GetGameScript(120800).script, -3, Color.white);
		/*
		if(Application.platform == RuntimePlatform.WindowsEditor) {
			ShopBtn.GetComponent<CommonBtn>().SetClick(OnShopPopupOpen); 
		} else {
			ShopBtn.GetComponent<CommonBtn>().SetEnabled(false);
		}
		*/
		ShopBtn.GetComponent<CommonBtn>().SetClick(OnShopPopupOpen); 
	}

	public void SetPopupOpenEvent(PopupOpenEvent onPopupOpenEvent) {
		_PopupOpenEvent = new PopupOpenEvent(onPopupOpenEvent);
		
	}

	public void UpdateUserData() {
		CoinTxt.GetComponent<TextMesh>().text = _UserData.UserMoney + "";
		CoreTxt.GetComponent<TextMesh>().text = _UserData.UserCores + "";
		DebrisTxt.GetComponent<TextMesh>().text = _UserData.UserChips + "";

	}

	private void OnOptionClick(int id) {
		//LocalData.getInstence().AllClear();
		//Application.LoadLevel("DarkSpriteIntro");
		if(_PopupOpenEvent != null) _PopupOpenEvent(MainPopupType.OptionPopup);
	}

	private void OnShopPopupOpen(int id) {
		if(_PopupOpenEvent != null) _PopupOpenEvent(MainPopupType.ShopPopup);
	}

}
