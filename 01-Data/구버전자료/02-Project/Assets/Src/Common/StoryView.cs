using UnityEngine;
using System.Collections;

public class StoryView : MonoBehaviour {

	public bool isTest;
	//public GameObject NPCField;
	public GUISkin StorySkin;
	public Texture2D _Texture;
	public GameObject BlackBg;

	private Rect _WindowRect;
	private Rect _BtnRect;
	private string _NpcName = "";
	private string _StoryString = "";
	private GUIStyle _TextAreaStyle;
	private GUIStyle _TitleAreaStyle;
	private GUIStyle _ThumbStyle;
	private GUIStyle _TextBGStyle;
	private GUIStyle _TouchIconStyle;
	private Texture _TouchIconTexture;

	private bool _IsShowWindow;

	private int _MainFont = 28;
	private int _TitleFont = 32;
	private int _lineGap = 20;

	private int _LeftGap = 0;
	private int _DefaultLeftGap = 200;

	private float _Scale = 0;

	public delegate void BtnCallBack();
	private BtnCallBack _Callback;

	private GameObject _StageTitleTxt;
	private GameObject _StageMessageTxt;

	private bool keyTestValue = false;

	private Color _NPCNameColor;
	private Color _CommentNormal;
	private Color _CommentMC;

	private byte _NPCPos = 0;

	private Texture2D _NPCTexture;
	private Texture2D _TextBgTexture;
	private GUIStyle _WindowGUIStyle;

	private GameObject _NPC1;
	private GameObject _NPC2;
	private GameObject _NPC3;
	private SpriteRenderer _Npc1Renderer;
	private SpriteRenderer _Npc2Renderer;
	private SpriteRenderer _Npc3Renderer;

	private bool _IsClick = false;
	private float _Timer = 0;
	private float _ScreenGap = 0;

	void Awake() {

		BlackBg.renderer.enabled = false;
		int windowHeight = Screen.height / 3;
		//_WindowRect = new Rect(20, (Screen.height - windowHeight) - 20, Screen.width - 40, windowHeight);
		_WindowRect = new Rect(0, 0, Screen.width, Screen.height);

		_BtnRect = new Rect(0, 0, Screen.width, Screen.height);

		_Scale = Screen.height / 800f;
		_ScreenGap = 200f * (Screen.height / 800f);

		_MainFont = (int)((float)(_MainFont) * _Scale);
		_TitleFont = (int)((float)(_TitleFont) * _Scale);
		_lineGap = (int)((float)(_lineGap) * _Scale);

		_NPCNameColor = Color.white;
		_NPCNameColor.r = 1f;
		_NPCNameColor.g = 0.6f;
		_NPCNameColor.b = 0f;
		_CommentNormal = Color.white;
		_CommentMC = Color.white;
		_CommentMC.r = 0.7f;
		_CommentMC.g = 0.7f;
		_CommentMC.b = 0.7f;
	}

	// Use this for initialization
	void Start () {
		//NPCField.renderer.enabled = false; 
		_TextBgTexture = Resources.Load<Sprite>("Common/Black50Box").texture;
		_TouchIconTexture = Resources.Load<Sprite>("Common/TouchMark").texture;
	}

	void Update () {
		if(Input.GetKey(KeyCode.Space)) {
			if(keyTestValue == false) {
				if(_Callback != null) _Callback();
				keyTestValue = true;
			}

		} else {
			keyTestValue = false;
		}

		if(_IsClick == false) _Timer += Time.deltaTime;
		if(_Timer > 0.3f) {
			_Timer = 0;
			_IsClick = true;
		}
		//print("Input.GetKey(KeyCode.Space) : " + Input.GetKey(KeyCode.A));
	}

	public void ShowStoryWindow(byte NpcType, byte NpcPos, string NpcName, string StoryStr, BtnCallBack onBtnCallback, byte viewType) {

		_IsClick = false;
		_Timer = 0;

		_Callback = new BtnCallBack(onBtnCallback);

		_NpcName = NpcName;
		_StoryString = StoryStr;

		_LeftGap = 0;
		float npcScale = 1.3f;

		if(_NPC1 == null){
			_NPC1 = new GameObject();
			_NPC1.layer = LayerMask.NameToLayer("Alert");
			_NPC1.transform.parent = this.gameObject.transform;
			_Npc1Renderer = _NPC1.AddComponent<SpriteRenderer>();
			_NPC1.transform.localScale = new Vector3(npcScale, npcScale, npcScale);
		} else {
			_NPC1.renderer.enabled = true;
		}

		if(_NPC2 == null){
			_NPC2 = new GameObject();
			_NPC2.layer = LayerMask.NameToLayer("Alert");
			_NPC2.transform.parent = this.gameObject.transform;
			_Npc2Renderer = _NPC2.AddComponent<SpriteRenderer>();
			_NPC2.transform.localScale = new Vector3(npcScale, npcScale, npcScale);
		} else {
			_NPC2.renderer.enabled = true;
		}

		if(_NPC3 == null){
			_NPC3 = new GameObject();
			_NPC3.layer = LayerMask.NameToLayer("Alert");
			_NPC3.transform.parent = this.gameObject.transform;
			_Npc3Renderer = _NPC3.AddComponent<SpriteRenderer>();
			_NPC3.transform.localScale = new Vector3(npcScale, npcScale, npcScale);
		} else {
			_NPC3.renderer.enabled = true;
		}

		if(NpcType == NPCType.None || viewType == StoryViewType.GuideType) {
			_Npc1Renderer.sprite = Resources.Load<Sprite>("NPC/npc00");
			_Npc2Renderer.sprite = Resources.Load<Sprite>("NPC/npc00");
			_Npc3Renderer.sprite = Resources.Load<Sprite>("NPC/npc00");
		}

		_NPC1.renderer.sortingOrder = 400;
		_NPC2.renderer.sortingOrder = 400;
		_NPC3.renderer.sortingOrder = 400;

		_NPCPos = NpcPos;

		Sprite npcSprite;
		if(viewType == StoryViewType.NpcType) {
			if(NpcType == NPCType.Kris) {
				npcSprite = Resources.Load<Sprite>("NPC/npc01");
			} else if(NpcType == NPCType.Ria) {
				npcSprite = Resources.Load<Sprite>("NPC/npc02");
			} else if (NpcType == NPCType.Tassa) {
				npcSprite = Resources.Load<Sprite>("NPC/npc03");
			} else if (NpcType == NPCType.Kity) {
				npcSprite = Resources.Load<Sprite>("NPC/npc04");
			} else if (NpcType == NPCType.Sara) {
				npcSprite = Resources.Load<Sprite>("NPC/npc05");
			} else {
				npcSprite = Resources.Load<Sprite>("NPC/npc00");
			}

			if(NpcPos == NPCPositionType.Center) {
				_NPC1.renderer.sortingOrder = 500;
				_NPC1.transform.position = new Vector3(0f, -2.36f, 0f);
				_Npc1Renderer.sprite = npcSprite;
			} else if(NpcPos == NPCPositionType.Right) {
				_NPC2.renderer.sortingOrder = 500;
				_NPC2.transform.position = new Vector3(4.99f, -2.36f, 0f);
				_Npc2Renderer.sprite = npcSprite;
			} else if(NpcPos == NPCPositionType.Left) {
				_NPC3.renderer.sortingOrder = 500;
				_NPC3.transform.localScale = new Vector3(npcScale * -1f, npcScale, npcScale);
				_NPC3.transform.position = new Vector3(-4f, -2.36f, 0f);
				_Npc3Renderer.sprite = npcSprite;
			} else {
				//NPCField.renderer.enabled = false;
			}

		} else {
			_NpcName = "";

			npcSprite = Resources.Load<Sprite>("Common/GuideImage/GuideImg" + NpcType);

			_NPC1.renderer.sortingOrder = 500;
			_NPC1.transform.position = new Vector3(0f, -2.36f, 0f);
			_Npc1Renderer.sprite = npcSprite;
		}

		/*
		if(NpcType == NPCType.Kris) {
			_NPCTexture = Resources.Load<Sprite>("NPC/npc01").texture;
		} else if(NpcType == NPCType.Ria) {
			_NPCTexture = Resources.Load<Sprite>("NPC/npc02").texture;
		} else if (NpcType == NPCType.Tassa) {
			_NPCTexture = Resources.Load<Sprite>("NPC/npc03").texture;
		} else if (NpcType == NPCType.Kity) {
			_NPCTexture = Resources.Load<Sprite>("NPC/npc04").texture;
		} else if (NpcType == NPCType.Sara) {
			_NPCTexture = Resources.Load<Sprite>("NPC/npc05").texture;
		} else {
			_NPCTexture = null;
		}
		*/

		_NPCPos = NpcPos;

		_IsShowWindow = true;

		BlackBg.renderer.enabled = true;
	}

	public void ShowMessageWindow(string thumb, string NpcName, string StoryStr, BtnCallBack onBtnCallback) {
		//int windowHeight = Screen.height / 3;
		//_WindowRect = new Rect(20, (Screen.height - windowHeight) - 20, Screen.width - 40, windowHeight);

		_Callback = new BtnCallBack(onBtnCallback);
		_NpcName = NpcName;
		_StoryString = StoryStr;
		_LeftGap = (int)((float)(_DefaultLeftGap) * _Scale);
		_Texture = Resources.Load<Sprite>(thumb).texture;

		_IsShowWindow = true;
	}

	public void ShowStageView(string title, string message, BtnCallBack onEndCallback) {
		_Callback = new BtnCallBack(onEndCallback);

		BlackBg.renderer.enabled = true;
		Color BlackBgColor = Color.white;
		BlackBgColor.a = 1f;
		BlackBg.renderer.material.color = BlackBgColor;
		iTween.ColorFrom(BlackBg, iTween.Hash("a", 0f));

		_StageTitleTxt = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
		_StageMessageTxt = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;

		_StageTitleTxt.GetComponent<OutLineFont>().SetString(title);
		_StageMessageTxt.GetComponent<OutLineFont>().SetString(message);

		_StageTitleTxt.GetComponent<OutLineFont>().SetSortLayer("Alert");
		_StageMessageTxt.GetComponent<OutLineFont>().SetSortLayer("Alert");

		_StageTitleTxt.GetComponent<OutLineFont>().SetSort(550);
		_StageMessageTxt.GetComponent<OutLineFont>().SetSort(550);

		_StageTitleTxt.transform.position = new Vector2(0f, 0.8f);

		//_StageTitleTxt.GetComponent<OutLineFont>().SetFontColor(Color.gray);
		_StageTitleTxt.GetComponent<OutLineFont>().SetFontSize(22);
		_StageMessageTxt.GetComponent<OutLineFont>().SetFontSize(32);

		iTween.ColorFrom(_StageTitleTxt, iTween.Hash("delay", 0.3f, "a", 0f));
		iTween.ColorFrom(_StageMessageTxt, iTween.Hash("delay", 0.3f, "a", 0f, "oncomplete", "HideStageView", "oncompletetarget", this.gameObject));

	}

	private void HideStageView() {
		float delay = 1f;
		iTween.ColorTo(BlackBg, iTween.Hash("a", 0f, "delay", delay + 0.5f
		                                    , "oncomplete", "HideStageViewComplete", "oncompletetarget", this.gameObject));
		iTween.ColorTo(_StageTitleTxt, iTween.Hash("delay", delay, "a", 0f));
		iTween.ColorTo(_StageMessageTxt, iTween.Hash("delay", delay, "a", 0f));

	}

	private void HideStageViewComplete() {
		Color BlackBgColor = Color.white;
		BlackBgColor.a = 1f;
		BlackBg.renderer.material.color = BlackBgColor;
		BlackBg.renderer.enabled = false;
		Destroy(_StageTitleTxt);
		Destroy(_StageMessageTxt);
		_StageTitleTxt = null;
		_StageMessageTxt = null;

		if(_Callback != null) _Callback();
	}

	public void HideStoryWindow() {
		if(_NPC1 != null) {
			_NPC1.renderer.enabled = false;
			_Npc1Renderer.sprite = Resources.Load<Sprite>("NPC/npc00");
		}
		if(_NPC2 != null) {
			_NPC2.renderer.enabled = false;
			_Npc2Renderer.sprite = Resources.Load<Sprite>("NPC/npc00");
		}
		if(_NPC3 != null) {
			_NPC3.renderer.enabled = false;
			_Npc3Renderer.sprite = Resources.Load<Sprite>("NPC/npc00");
		}
		BlackBg.renderer.enabled = false;
		_IsShowWindow = false;
	}

	private void OnWindowClick() {
		HideStoryWindow();
	}

	void OnGUI () {

		GUI.skin = StorySkin;

		_TextAreaStyle = new GUIStyle(GUI.skin.label);
		_TextAreaStyle.fontSize = _MainFont;

		_TitleAreaStyle = new GUIStyle(GUI.skin.label);
		_TitleAreaStyle.fontSize = _TitleFont;

		_ThumbStyle = new GUIStyle(GUI.skin.box);
		_ThumbStyle.normal.background = _Texture;

		_TextBGStyle = new GUIStyle(GUI.skin.box);
		_TextBGStyle.normal.background = _TextBgTexture;

		_TouchIconStyle = new GUIStyle(GUI.skin.box);
		_TouchIconStyle.normal.background = null;

		_WindowGUIStyle = new GUIStyle(GUI.skin.window);
		/*
		if(_LeftGap <= 0) {
			_WindowGUIStyle.normal.background = _TextBgTexture;
		} else {
			_WindowGUIStyle.normal.background = null;
		}
		*/
		_WindowGUIStyle.normal.background = null;


		if(_IsShowWindow == true) {
			_WindowRect = GUILayout.Window(10, _WindowRect, DoStoryWindow, "", _WindowGUIStyle);

			if (GUI.Button(_BtnRect, ""))
				if(_Callback != null && _IsClick) _Callback();
		}

		//GUI.Label(new Rect(0, 30, Screen.width, Screen.height), _NPCTexture);
		//GUILayout.Box(_Texture, GUILayout.Width(_Texture.width));

		if(isTest) {
			if (GUILayout.Button("Show")) {
				byte npcType = 1;
				byte npcPos = NPCPositionType.Right;
				string npcName = "리아";
				string storyStr = "어서오십시요. 사령관님.\n먼길 오시는데 불편하진 않으셨는지 모르겠네요.";
				ShowStoryWindow(npcType, npcPos, npcName, storyStr, OnWindowClick, StoryViewType.NpcType);
			}

			if (GUILayout.Button("Hide"))
				HideStoryWindow();

			if (GUILayout.Button("Message")) {
				string npcName = "리아";
				string storyStr = "어서오십시요. 사령관님.\n먼길 오시는데 불편하진 않으셨는지 모르겠네요.";
				ShowMessageWindow("MemberImg/Member001", npcName, storyStr, OnWindowClick);
			}

			if (GUILayout.Button("StageView")) {
				string npcName = "1막 1장";
				string storyStr = "버려진 기지";
				ShowStageView(npcName,storyStr, OnWindowClick);
			}
		}
	}

	void DoStoryWindow(int windowId) {
		GUILayout.Label("");

		GUILayout.BeginArea(new Rect(_lineGap, 520 * _Scale, _WindowRect.width, _WindowRect.height));
		GUILayout.Box("", _TextBGStyle, GUILayout.Width(_WindowRect.width - (_lineGap * 2)), GUILayout.Height(250 * _Scale));
		GUILayout.EndArea();

		if(_LeftGap > 0) {
			GUILayout.BeginArea(new Rect(_lineGap + (20 * _Scale), _lineGap + (520 * _Scale), _WindowRect.width - (_lineGap * 2), _WindowRect.height - (_lineGap * 2)));
			GUILayout.Box("", _ThumbStyle, GUILayout.Width(180 * _Scale), GUILayout.Height(180 * _Scale));
			GUILayout.EndArea();
			_NPCPos = NPCPositionType.Left;
		}

		GUILayout.BeginArea(new Rect(_lineGap * 2 + _LeftGap, _lineGap + (520 * _Scale), _WindowRect.width - (_lineGap * 4), _WindowRect.height - (_lineGap * 2)));

		_TitleAreaStyle.normal.textColor = _NPCNameColor;

		switch(_NPCPos) {
		case NPCPositionType.Right:
			_TitleAreaStyle.alignment = TextAnchor.MiddleRight;
			_TextAreaStyle.alignment = TextAnchor.MiddleRight;
			break;
		case NPCPositionType.Center:
			_TitleAreaStyle.alignment = TextAnchor.MiddleCenter;
			_TextAreaStyle.alignment = TextAnchor.MiddleCenter;
			break;
		default:
			_TitleAreaStyle.alignment = TextAnchor.MiddleLeft;
			_TextAreaStyle.alignment = TextAnchor.MiddleLeft;
			break;
		}
		
		GUILayout.Label(_NpcName, _TitleAreaStyle);
		if(_NpcName != "") {
			_TextAreaStyle.normal.textColor = _CommentNormal;
			GUILayout.Label(_StoryString, _TextAreaStyle);
		} else {
			_TextAreaStyle.normal.textColor = _CommentMC;
			GUILayout.Label(_StoryString, _TextAreaStyle);
		}
		GUILayout.EndArea();

		if(_IsClick) {
			/*
			GUILayout.BeginArea(new Rect((Screen.width * _Scale) - (_lineGap * 2), _WindowRect.height - 150 - (_lineGap * 2), _WindowRect.width, _WindowRect.height));
			GUILayout.Box(_TouchIconTexture, _TouchIconStyle, GUILayout.Width(_ScreenGap), GUILayout.Height(_ScreenGap));
			GUILayout.EndArea();
			*/
			GUILayout.BeginArea(new Rect(Screen.width - (_ScreenGap + _lineGap), Screen.height - (_ScreenGap + _lineGap), _ScreenGap, _ScreenGap));
			GUILayout.Box(_TouchIconTexture, _TouchIconStyle);
			GUILayout.EndArea();
		}

		/*
		if(_LeftGap > 0) {
			GUILayout.BeginArea(new Rect(_lineGap, _lineGap, _WindowRect.width - (_lineGap * 2), _WindowRect.height - (_lineGap * 2)));
			GUILayout.Box("", _ThumbStyle, GUILayout.Width(180 * _Scale), GUILayout.Height(180 * _Scale));
			GUILayout.EndArea();
		}
		GUILayout.BeginArea(new Rect(_lineGap + _LeftGap, _lineGap, _WindowRect.width - (_lineGap * 2) - _LeftGap, _WindowRect.height - (_lineGap * 2)));

		_TitleAreaStyle.normal.textColor = _NPCNameColor;
		switch(_NPCPos) {
		case NPCPositionType.Left:
			_TitleAreaStyle.alignment = TextAnchor.MiddleRight;
			_TextAreaStyle.alignment = TextAnchor.MiddleRight;
			break;
		case NPCPositionType.Center:
			_TitleAreaStyle.alignment = TextAnchor.MiddleCenter;
			_TextAreaStyle.alignment = TextAnchor.MiddleCenter;
			break;
		default:
			_TitleAreaStyle.alignment = TextAnchor.MiddleLeft;
			_TextAreaStyle.alignment = TextAnchor.MiddleLeft;
			break;
		}

		GUILayout.Label(_NpcName, _TitleAreaStyle);
		if(_NpcName != "") {
			_TextAreaStyle.normal.textColor = _CommentNormal;
			GUILayout.Label(_StoryString, _TextAreaStyle);
		} else {
			_TextAreaStyle.normal.textColor = _CommentMC;
			GUILayout.Label(_StoryString, _TextAreaStyle);
		}
		GUILayout.EndArea();
		*/

	}

}
