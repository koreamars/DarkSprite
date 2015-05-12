using UnityEngine;
using System.Collections;

public class OptionPopup : MonoBehaviour {

	public bool isTest;
	public GameObject CloseBtn;

	public GameObject TitleTxt;

	public GameObject Option1Btn;
	public GameObject Option2Btn;
	public GameObject Option3Btn;
	public GameObject OptionTxt1;
	public GameObject OptionTxt2;

	private GameObject _VerViewObj = null;
	
	public delegate void CloseEvent();
	private CloseEvent _CloseCallback;

	private ScriptData _ScriptData;
	private UserData _UserData;

	private bool isCheat = false;
	private Rect windowRect;
	private Texture2D _Texture;
	
	void Awake() {
		DarkSprite.getInstence();

		_Texture = Resources.Load<Sprite>("Common/BlackBox").texture;
	}
	
	void Start() {
		if(isTest) {
			LocalData.getInstence().AllLoad();
			init();
		}
	}
	
	public void init() {

		_ScriptData = ScriptData.getInstence();
		_UserData = UserData.getInstence();

		TitleTxt.GetComponent<TextMesh>().text = ScriptData.getInstence().GetGameScript(120900).script;
		
		CloseBtn.GetComponent<ButtonEvent>().SetCallBack(OnPopupClose);

		Option1Btn.GetComponent<CommonBtn>().Init(0, _ScriptData.GetGameScript(130900).script, 1, Color.white);
		Option2Btn.GetComponent<CommonBtn>().Init(1, _ScriptData.GetGameScript(130901).script, 1, Color.white);
		Option1Btn.GetComponent<CommonBtn>().SetClick(OnOptionMenuClick);
		Option2Btn.GetComponent<CommonBtn>().SetClick(OnOptionMenuClick);

		if(SystemData.GetInstance().GameServiceType == ServiceType.ALPHA || Application.platform == RuntimePlatform.WindowsEditor) {
			Option3Btn.GetComponent<CommonBtn>().Init(2, "Open Cheat", 1, Color.white);
			Option3Btn.GetComponent<CommonBtn>().SetClick(OnCheatWindow);
		} else {
			Option3Btn.GetComponent<CommonBtn>().SetEnabled(false);
		}

		OptionTxtView();

	}

	private void OptionTxtView() {
		if(_UserData.Option_BGM) {
			OptionTxt1.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160156).script;
		} else {
			OptionTxt1.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160157).script;
		}

		if(_UserData.Option_Sound) {
			OptionTxt2.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160156).script;
		} else {
			OptionTxt2.GetComponent<TextMesh>().text = _ScriptData.GetGameScript(160157).script;
		}

		if(_VerViewObj != null) return;

		string verStr = "ver : " + SystemData.BuildVersion;
		if(SystemData.GetInstance().GameServiceType == ServiceType.BETA) verStr += " (beta)";
		if(SystemData.GetInstance().GameServiceType == ServiceType.ALPHA) verStr += " (alpha)";

		_VerViewObj = CustomTextMesh.SetAddTextMesh(verStr, 12, TextAnchor.MiddleLeft, Color.white, 
		                                                      20, "GamePopup");
		_VerViewObj.transform.parent = this.gameObject.transform;
		_VerViewObj.transform.position = new Vector2(-2.66f, -1.95f);
	}

	private void OnOptionMenuClick(int id) {
		if(id == 0) {
			if(_UserData.Option_BGM) {
				_UserData.Option_BGM = false;
				if(DarkSprite.getInstence().MainScene != null) DarkSprite.getInstence().MainScene.MainBGM.Stop();
			} else {
				_UserData.Option_BGM = true;
				if(DarkSprite.getInstence().MainScene != null) {
					DarkSprite.getInstence().MainScene.MainBGM.Play();
					DarkSprite.getInstence().MainScene.MainBGM.loop = true;
				}
			}
			OptionTxtView();
			LocalData.getInstence().UserOptionDataSave();
		} else if(id == 1) {
			if(_UserData.Option_Sound) {
				_UserData.Option_Sound = false;
			} else {
				_UserData.Option_Sound = true;
			}
			OptionTxtView();
			LocalData.getInstence().UserOptionDataSave();
		} else {
			LocalData.getInstence().AllClear();
			Application.LoadLevel("DarkSpriteIntro");
		}
	}
	
	public void SetCloseEventCallBack(CloseEvent onCloseEvent) {
		_CloseCallback = new CloseEvent(onCloseEvent);
		
	}

	private void OnCheatWindow(int id) {
		windowRect = new Rect((Screen.width / 2) - 300, (Screen.height / 2) - 300, 600, 600);
		isCheat = true;
	}

	
	private void OnPopupClose() {
		if(_CloseCallback != null) _CloseCallback();
	}

	void OnGUI() {
		GUI.skin.window.normal.background = _Texture;
		GUI.skin.window.onNormal.background = _Texture;
		GUI.skin.button.fontSize = 24;
		GUI.skin.label.fontSize = 24;
		if(isCheat == true) {
			windowRect = GUI.Window(0, windowRect, DoMyWindow, "Cheat Window");
		}

	}

	void DoMyWindow(int windowID) {
		if (GUI.Button(new Rect(60, 20, 250, 60), "창닫기"))
			isCheat = false;

		if (GUI.Button(new Rect(20, 100, 280, 60), "모든 데이터 초기화.")) {
			isCheat = false;
			LocalData.getInstence().AllClear();
			Application.LoadLevel("DarkSpriteIntro");

		}


		if (GUI.Button(new Rect(310, 100, 280, 60), "전투 후 코어 강제 지급")) {
			if(SystemData.GetInstance().IsGetFightCore == true) {
				SystemData.GetInstance().IsGetFightCore = false;
			} else {
				SystemData.GetInstance().IsGetFightCore = true;
			}
		}
		GUI.Label(new Rect(310, 160, 280, 40), "지급 상태 : " + SystemData.GetInstance().IsGetFightCore);



		if (GUI.Button(new Rect(20, 220, 280, 60), "결제 검증 단계")) {
			if(SystemData.GetInstance().IsShopPurchase == true) {
				SystemData.GetInstance().IsShopPurchase = false;
			} else {
				SystemData.GetInstance().IsShopPurchase = true;
			}
		}
		GUI.Label(new Rect(20, 280, 280, 40), "검증을 하는가? : " + SystemData.GetInstance().IsShopPurchase);



		if (GUI.Button(new Rect(310, 220, 280, 60), "경험치 1000xp 강제 지급")) {
			if(SystemData.GetInstance().IsMemberXpPlus == true) {
				SystemData.GetInstance().IsMemberXpPlus = false;
			} else {
				SystemData.GetInstance().IsMemberXpPlus = true;
			}
		}
		GUI.Label(new Rect(310, 280, 280, 40), "강제 지급합니까? : " + SystemData.GetInstance().IsMemberXpPlus);



		if (GUI.Button(new Rect(20, 340, 280, 60), "항공기 보호 미션 완료.")) {
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Aircraft_Defense_Complete, 1);
			isCheat = false;
		}

		if (GUI.Button(new Rect(310, 340, 280, 60), "5개 코어파편 강제 지급.")) {
			_UserData.UserChips += 5;
			BillModel billmodel = new BillModel();
			billmodel.corechip = 5;
			billmodel.corechipPlus = true;
			_UserData.UpdatePayData(billmodel, new Vector2(0f, 0f));
		}

		if (GUI.Button(new Rect(20, 410, 280, 60), "마을방어 미션 완료.")) {
			MissionData.getInstence().AddMissionGoal(MissionGoalType.Town_Defense_Com, 1);
			isCheat = false;
		}
	}
}
