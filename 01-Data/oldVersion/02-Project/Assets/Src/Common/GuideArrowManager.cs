using UnityEngine;
using System;
using System.Xml;
using System.Collections;

public class GuideArrowManager : MonoBehaviour {

	private static GuideArrowManager _instence;

	private GameObject _ArrowObject;
	private GuideArrowModel[] _GuideArrowData;

	public static GuideArrowManager getInstence()	{
		
		if(_instence == null) 
		{
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<GuideArrowManager>();
			DontDestroyOnLoad(_instence);
		}
		
		return _instence;
	}

	void Awake() {
		// 기본 장비 데이터 초기화.
		TextAsset guideArrowDataAsset = (TextAsset)Resources.Load("XMLData/GuideArrowData",typeof(TextAsset));
		XmlDocument guideArrowXmlDoc = new XmlDocument();
		guideArrowXmlDoc.LoadXml(guideArrowDataAsset.text);
		
		_GuideArrowData = new GuideArrowModel[guideArrowXmlDoc["GuideArrowData"].ChildNodes.Count];
		
		int xmlIndex = 0;
		foreach(XmlElement xmlElement in guideArrowXmlDoc["GuideArrowData"]) {
			_GuideArrowData[xmlIndex] = new GuideArrowModel();
			_GuideArrowData[xmlIndex].SceneType = System.Convert.ToByte(xmlElement["scenetype"].InnerText);
			_GuideArrowData[xmlIndex].StoryStepId = System.Convert.ToInt16(xmlElement["storystep"].InnerText);
			_GuideArrowData[xmlIndex].XPosition = System.Convert.ToSingle(xmlElement["x"].InnerText);
			_GuideArrowData[xmlIndex].YPosition = System.Convert.ToSingle(xmlElement["y"].InnerText);
			_GuideArrowData[xmlIndex].Rotation = System.Convert.ToSingle(xmlElement["rotation"].InnerText);
			
			xmlIndex ++;
		}

	}

	public void ShowArrow(byte sceneType) {
		print("ShowArrow : " + sceneType + "/" + UserData.getInstence().StoryStepId);
		SystemData.GetInstance().CurrentSceneType = sceneType;

		// 구글 로그.
		if(GoogleAnalytics.instance) {
			string sceneTypeStr = "";
			switch(sceneType) {
			case SceneType.MainScene:
				sceneTypeStr = "MainScene";
				break;
			case SceneType.FightScene:
				sceneTypeStr = "FightScene";
				break;
			case SceneType.HQPopup:
				sceneTypeStr = "HQPopup";
				break;
			case SceneType.CommoPopup:
				sceneTypeStr = "CommoPopup";
				break;
			case SceneType.CommoPopup_Aircraft:
				sceneTypeStr = "CommoPopup_Aircraft";
				break;
			case SceneType.CommoPopup_GhostAttack:
				sceneTypeStr = "CommoPopup_GhostAttack";
				break;
			case SceneType.ResearchPopup:
				sceneTypeStr = "ResearchPopup";
				break;
			case SceneType.FactoryPopup:
				sceneTypeStr = "FactoryPopup";
				break;
			case SceneType.CorpsPopup:
				sceneTypeStr = "CorpsPopup";
				break;
			case SceneType.HangarPopup:
				sceneTypeStr = "HangarPopup";
				break;
			case SceneType.FightPopup:
				sceneTypeStr = "FightPopup";
				break;
			case SceneType.ShopPopup:
				sceneTypeStr = "ShopPopup";
				break;
			case SceneType.OptionPopup:
				sceneTypeStr = "OptionPopup";
				break;
			case SceneType.FactoryPopup_Suit:
				sceneTypeStr = "FactoryPopup_Suit";
				break;
			case SceneType.FactoryPopup_Engine:
				sceneTypeStr = "FactoryPopup_Engine";
				break;
			case SceneType.FactoryPopup_Airframe:
				sceneTypeStr = "FactoryPopup_Airframe";
				break;
			case SceneType.HangarPopup_DetailView:
				sceneTypeStr = "HangarPopup_DetailView";
				break;
			case SceneType.HQPopup_CareMember:
				sceneTypeStr = "HQPopup_CareMember";
				break;
			case SceneType.HQPopup_GetMember:
				sceneTypeStr = "HQPopup_GetMember";
				break;
			case SceneType.HQPopup_Upgrade:
				sceneTypeStr = "HQPopup_Upgrade";
				break;
			case SceneType.ResearchPopup_CoreMake:
				sceneTypeStr = "ResearchPopup_CoreMake";
				break;
			}

			if(sceneTypeStr != "") GoogleAnalytics.instance.LogScreen(sceneTypeStr);
		}

		GuideArrowModel model = GetModelBySceneType(sceneType, UserData.getInstence().StoryStepId);
		if(model != null) {
			CreateArrow();
			_ArrowObject.transform.position = new Vector2(model.XPosition, model.YPosition);
			_ArrowObject.transform.rotation = Quaternion.Euler(0f, 0f, model.Rotation);
		} else {
			if(_ArrowObject != null) Destroy(_ArrowObject);
			_ArrowObject = null;
		}

	}

	public void UpdateArrow() {
		ShowArrow(SystemData.GetInstance().CurrentSceneType);
	}

	private void CreateArrow() {
		if(_ArrowObject == null) _ArrowObject = Instantiate(Resources.Load<GameObject>("Common/GuideArrow")) as GameObject;
	}

	private GuideArrowModel GetModelBySceneType(byte sceneType, int storyStep) {
		foreach(GuideArrowModel model in _GuideArrowData) {
			if(model.SceneType == sceneType && model.StoryStepId == storyStep) return model;

		}

		return null;
	}
}
