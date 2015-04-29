using UnityEngine;
using System.Xml;
using System.Collections;

public class StoryData : MonoBehaviour {

	private static StoryData _instence;

	private UserData _UserData;
	private ScriptData _ScriptData;

	private MainStoryModel[] _MainStoryData;
	private StoryDataModel[] _StoryLinesData;
	private StoryDataModel[] _MemberStoryData;

	private GameObject _StoryViewWindow;
	private StoryView _StoryView;
	private int _CurrentStoryId;		// 현재 진행중인 스토리 스텝. (userdata와는 틀림);
	private short _CurrentStoryDataId;	// 현재 진행중인 스토리 데이터 ID;

	private ArrayList _readyStepList;	// 대기 중인 스토리 정보 리스트.

	private byte _CurrentAct;
	private byte _CurrentScene;

	public static StoryData getInstence()	{
		if(_instence == null) 
		{
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<StoryData>();
			DontDestroyOnLoad(_instence);
		}
		
		return _instence;
	}
	
	public IEnumerator init() {
		yield return new WaitForEndOfFrame();
		_UserData = UserData.getInstence();
		_ScriptData = ScriptData.getInstence();

		// 메인 스토리 정보를 가져옵니다.
		TextAsset storytextAsset = (TextAsset)Resources.Load("XMLData/MainStoryData",typeof(TextAsset));
		XmlDocument storyXmlDoc = new XmlDocument();
		storyXmlDoc.LoadXml(storytextAsset.text);

		int xmlIndex = 0;

		_MainStoryData = new MainStoryModel[storyXmlDoc["MainStoryData"].ChildNodes.Count];

		int loadstep = 0;
		foreach(XmlElement xmlElement in storyXmlDoc["MainStoryData"]) {
			MainStoryModel mainStoryModel = new MainStoryModel();
			mainStoryModel.id = System.Convert.ToInt32(xmlElement["id"].InnerText);
			if(xmlIndex == 0) loadstep = mainStoryModel.id;
			mainStoryModel.storyTitleId  = System.Convert.ToInt32(xmlElement["event"].InnerText);
			mainStoryModel.storyId = System.Convert.ToInt16(xmlElement["storyId"].InnerText);
			mainStoryModel.act = System.Convert.ToByte(xmlElement["act"].InnerText);
			mainStoryModel.scene = System.Convert.ToByte(xmlElement["scene"].InnerText);
			_MainStoryData[xmlIndex] = mainStoryModel;

			xmlIndex ++;
		}

		MainStoryModel currentMainStoryModel = GetMainStoryModelById(loadstep);

		yield return StartCoroutine(LoadStoryData(currentMainStoryModel.act, currentMainStoryModel.scene));

		_readyStepList = new ArrayList();
	}

	private IEnumerator LoadStoryData(byte act, byte scene) {
		yield return new WaitForEndOfFrame();
		print("act : " + act + " scene : " + scene);
		// 서브 대사 정보를 가져옵니다.
		TextAsset sublinestextAsset = (TextAsset)Resources.Load("XMLData/StoryData/StoryData-Act" + act + "-Scene" + scene ,typeof(TextAsset));
		XmlDocument sublinesXmlDoc = new XmlDocument();
		sublinesXmlDoc.LoadXml(sublinestextAsset.text);
		
		int xmlIndex = 0;

		if(act == 0 && scene == 3) {
			_MemberStoryData = new StoryDataModel[sublinesXmlDoc["SceneStoryData"].ChildNodes.Count];
		} else {
			_StoryLinesData = new StoryDataModel[sublinesXmlDoc["SceneStoryData"].ChildNodes.Count];
		}

		foreach(XmlElement xmlElement in sublinesXmlDoc["SceneStoryData"]) {
			StoryDataModel storyDataModel = new StoryDataModel();
			storyDataModel.id = System.Convert.ToInt16(xmlElement["id"].InnerText);
			storyDataModel.NpcType = System.Convert.ToByte(xmlElement["NpcType"].InnerText);
			storyDataModel.NpcPositionType = System.Convert.ToByte(xmlElement["NpcPositionType"].InnerText);
			storyDataModel.ViewType = System.Convert.ToByte(xmlElement["ViewType"].InnerText);
			storyDataModel.StoryStep = System.Convert.ToInt32(xmlElement["StoryStep"].InnerText);
			storyDataModel.Comment = xmlElement["Comment"].InnerText;
			if(act == 0 && scene == 3) {
				_MemberStoryData[xmlIndex] = storyDataModel;
			} else {
				_StoryLinesData[xmlIndex] = storyDataModel;
			}
			
			xmlIndex ++;
		}

		_CurrentAct = act;
		_CurrentScene = scene;

		yield return 0;
	}

	/** 해당에 맞는 스토리를 진행합니다. - 주로 미션 갱신시. */
	public void UpdateStoryStep(int storyStepid) {
		_readyStepList.Add(storyStepid);
		if(_readyStepList.Count > 1) return;

		StartCoroutine(CheckStoryStep(storyStepid));
	}

	private IEnumerator CheckStoryStep(int storyStepid) {

		yield return new WaitForEndOfFrame();

		MainStoryModel mainStoryModel = GetMainStoryModelById(storyStepid);

		if(mainStoryModel == null) {
			print("StoryData : MainStoryModel null.");
			yield break;
		}

		yield return StartCoroutine(CheckStoryModel(storyStepid));

		if(_StoryViewWindow == null) _StoryViewWindow = Instantiate(Resources.Load<GameObject>("Common/StoryView")) as GameObject;
		_StoryView = _StoryViewWindow.GetComponent<StoryView>();

		_CurrentStoryId = mainStoryModel.id;
		if(mainStoryModel.storyTitleId > 0) {	// 타이틀이 노출이 필요 한 경우.
			ShowStoryTitleMovie(mainStoryModel);
		} else {
			StoryDataModel storyDataModel = GetStoryDataModelById(mainStoryModel.storyId);
			ShowStoryViewWindow(storyDataModel);
		}

	}

	/** 화면에 스토리를 보여줌. */
	private void ShowStoryViewWindow(StoryDataModel storyDataModel) {
		_CurrentStoryDataId = storyDataModel.id;

		string npcName = "";
		switch(storyDataModel.NpcType) {
		case NPCType.Ria:
			npcName = _ScriptData.GetGameScript(190100).script;
			break;
		case NPCType.Sara:
			npcName = _ScriptData.GetGameScript(190101).script;
			break;
		case NPCType.Kity:
			npcName = _ScriptData.GetGameScript(190102).script;
			break;
		case NPCType.Tassa:
			npcName = _ScriptData.GetGameScript(190103).script;
			break;
		case NPCType.Kris:
			npcName = _ScriptData.GetGameScript(190104).script;
			break;
		}
		
		byte npcType = storyDataModel.NpcType;
		byte npcPos = storyDataModel.NpcPositionType;
		byte viewType = storyDataModel.ViewType;

		string storyStr = storyDataModel.Comment;
		_StoryView.ShowStoryWindow(npcType, npcPos, npcName, storyStr, OnStoryViewClick, viewType);
	}

	/** 각 막이나 장의 시작을 알리는 효과 노출 */
	private void ShowStoryTitleMovie(MainStoryModel mainStoryModel) {

		_CurrentStoryDataId = mainStoryModel.storyId;
		string act = mainStoryModel.act + _ScriptData.GetGameScript(210100).script;
		string scene = mainStoryModel.scene + _ScriptData.GetGameScript(210201).script;
		string name = _ScriptData.GetGameScript(mainStoryModel.storyTitleId).script;

		_StoryView.ShowStageView(act + " " + scene, name, ShowStoryViewComplete);
		if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("StoryStep", mainStoryModel.act + "-" + mainStoryModel.scene);
	}

	/** 타이틀 모션 종료 */
	private void ShowStoryViewComplete() {
		StoryDataModel storyDataModel = GetStoryDataModelById(_CurrentStoryDataId);
		ShowStoryViewWindow(storyDataModel);
	}

	//private void 

	// getter =========================================================================================================

	public MainStoryModel GetMainStoryModelById(int id) {
		foreach(MainStoryModel model in _MainStoryData) {
			if(model.id == id) return model;
		}
		
		return null;
	}

	public StoryDataModel GetStoryDataModelById(short id) {
		foreach(StoryDataModel model in _StoryLinesData) {
			if(model.id == id) return model;
		}
		
		return null;
	}

	public StoryDataModel GetMemberStoryModelByid(short npcId) {
		foreach(StoryDataModel model in _MemberStoryData) {
			if(model.NpcType == npcId) return model;
		}
		
		return null;
	}

	/** 해당 MainStoryModel의 아이디 맞는 StoryData를 찾고 없으면 로드함. */
	public IEnumerator CheckStoryModel(int storyStepid) {
		//print("CheckStoryModel : " + storyStepid);
		yield return new WaitForEndOfFrame();

		MainStoryModel mainStoryModel = GetMainStoryModelById(storyStepid);

		if(mainStoryModel.act != _CurrentAct || mainStoryModel.scene != _CurrentScene) {
			yield return StartCoroutine(LoadStoryData(mainStoryModel.act, mainStoryModel.scene));
		} else {
			yield return 0;
		}
	}

	// getter =========================================================================================================

	private void OnStoryViewClick() {

		StoryDataModel storyDataModel = GetStoryDataModelById((short)(_CurrentStoryDataId + 1));

		if(storyDataModel != null && _CurrentStoryId == storyDataModel.StoryStep) {
			ShowStoryViewWindow(storyDataModel);
		} else {
			_StoryViewWindow.GetComponent<StoryView>().HideStoryWindow();
			//_UserData.StoryStep += 1;
			LocalData.getInstence().UserStoryStepSave();
			if(DarkSprite.getInstence().MainScene != null) DarkSprite.getInstence().MainScene.GameMenuUpdate();

			_readyStepList.RemoveAt(0);

			if(_readyStepList.Count > 0) {
				int nextStoryStepID = (int)(_readyStepList[0]);
				MainStoryModel mainStoryModel = GetMainStoryModelById(nextStoryStepID);
				if(mainStoryModel == null) return;
				StartCoroutine(CheckStoryStep(nextStoryStepID));
			} else {
				Destroy(_StoryViewWindow);
			}
		}

	}

}
