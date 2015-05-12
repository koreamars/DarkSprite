using UnityEngine;
using System.Collections;

public class IntroScene : MonoBehaviour {

	public GameObject IntroImg;
	public GameObject blackBox;
	public AudioSource IntroBGM;
	public GUISkin IntroSkin;

	private string _Comment = "";
	private short _CommentId = 1;
	private float _CommentDelay = 1f;

	private bool _IsBtnClick = false;

	private Rect _WindowRect;
	private Texture _ThumbStyle;
	private float _ScreenGap = 0;
	private float _ScaleGap = 0;

	private GameObject _loadingMark;

	// Use this for initialization
	void Start () {

		_ScreenGap = 200f * (Screen.height / 800f);
		_ScaleGap = Screen.height / 800f;
		SystemData.GetInstance();

		_loadingMark = SystemData.GetInstance().GetLoadingMark();

		if(GoogleAnalytics.instance) GoogleAnalytics.instance.appVersion = SystemData.BuildVersion;

		StartCoroutine(init());
		SystemData.GetInstance().HideBanner();
	}

	private IEnumerator init() {
		if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogScreen("Game Start");

		yield return new WaitForEndOfFrame();

		yield return StartCoroutine(NetworkData.getInstence().LoadGameData());

		// 시스템 버전 설정.
		if(NetworkData.getInstence().results != null) {
			SystemData.GetInstance().SetSystemLoadData(NetworkData.getInstence().results.text);
		}

		if(SystemData.GetInstance().GameServiceType == ServiceType.ALPHA || SystemData.GetInstance().GameServiceType == ServiceType.BETA) {
			if(GoogleAnalytics.instance) GoogleAnalytics.instance.propertyID = SystemData.GetInstance().TestGoogleId;
		}

		yield return StartCoroutine(MissionData.getInstence().init());

		yield return StartCoroutine(StoryData.getInstence().init());

		//LocalData.getInstence().AllClear();
		LocalData.getInstence().AllLoad();

		Destroy(_loadingMark);

		if(LocalData.getInstence().IntroComplete) {
			//if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogScreen("Game Start");
			SceneChange();
		} else {
			if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogScreen("Intro Start");
			if(UserData.getInstence().Option_BGM) {
				IntroBGM.Play();
				IntroBGM.loop = true;
			}

			yield return StartCoroutine(StoryData.getInstence().CheckStoryModel(50000));

			iTween.ColorTo(blackBox, iTween.Hash("a", 0f, "time", 1f, "easetype", iTween.EaseType.linear));
			
			iTween.MoveTo(IntroImg, iTween.Hash("delay", 0.2f, "y", 9.0f, "time", (_CommentDelay * 3) * 13, "easetype", iTween.EaseType.linear));
			//iTween.MoveTo(blackBox, iTween.Hash("x", 0, "delay", 2f, "oncomplete", "IntroComplete", "oncompletetarget", this.gameObject));

			MainStoryModel mainStoryModel = StoryData.getInstence().GetMainStoryModelById(50000);

			_CommentId = mainStoryModel.storyId;
			StartCoroutine(ShowComment(mainStoryModel.storyId));

			_ThumbStyle = Resources.Load<Sprite>("Common/TouchMark").texture;

		}
		//SystemData.GoogleAnlyticsCS.LogScreen("Game Start");
	}

	// 대사 노출 시작.
	private IEnumerator ShowComment(short id) {

		StoryDataModel storyDataModel = StoryData.getInstence().GetStoryDataModelById(id);

		_Comment = storyDataModel.Comment;
		yield return new WaitForSeconds(_CommentDelay / 3);
		ShowBtn();
	}

	private void ShowBtn() {
		_IsBtnClick = true;
	}

	private void OnButtonEvent() {
		if(_IsBtnClick) EndShowComment();

	}

	private void EndShowComment() {
		_IsBtnClick = false;
		_CommentId ++;

		StoryDataModel storyDataModel = StoryData.getInstence().GetStoryDataModelById(_CommentId);

		if(storyDataModel != null && storyDataModel.ViewType == StoryViewType.NpcType) {
			StartCoroutine(ShowComment(_CommentId));
		} else {
			_Comment = "";
			IntroComplete();
		}

	}


	private void IntroComplete() {
		UserData.getInstence().StoryStepId = 1;
		iTween.ColorTo(blackBox, iTween.Hash("a", 1f, "oncomplete", "SceneChange", "oncompletetarget", this.gameObject
		                                     , "easetype", iTween.EaseType.linear));
	}

	private void SceneChange() {
		LocalData.getInstence().IntroComplete = true;
		LocalData.getInstence().AllSave();
		Application.LoadLevel("DarkSpriteMain");
	}

	void OnGUI() {

		GUI.skin = IntroSkin;
		GUI.skin.label.alignment = TextAnchor.UpperCenter;
		GUI.skin.label.fontSize = (int)(30f * (Screen.height / 800f));

		if(GUI.Button(new Rect(0, 0, Screen.width, Screen.height), "")) {
			OnButtonEvent();
		}
		_WindowRect = GUILayout.Window(0, new Rect(0, Screen.height - _ScreenGap, Screen.width, Screen.height), DoStoryWindow, "");
	}

	void DoStoryWindow(int windowId) {
		GUILayout.Label("");
		if(_IsBtnClick) {
			GUILayout.BeginArea(new Rect(Screen.width - _ScreenGap, 0, _ScreenGap, _ScreenGap));
			GUILayout.Box(_ThumbStyle, GUILayout.Width(_ScreenGap), GUILayout.Height(_ScreenGap));
			GUILayout.EndArea();
		}

		GUILayout.BeginArea(new Rect(0, 30 * _ScaleGap, Screen.width, 300));
		GUILayout.Label(_Comment);
		GUILayout.EndArea();
	}

}
