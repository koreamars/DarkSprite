using UnityEngine;
using System.Xml;
using System.Collections;

public class ScriptData : MonoBehaviour {

	private static ScriptData _instence;

	private GameScript[] _GameScriptList;

	public static ScriptData getInstence()	{
		if(_instence == null) 
		{
			//_instence = new DataController();
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<ScriptData>();
		}
		
		return _instence;
	}

	void Awake() {
		
		// 게임 기본 스크립트 정보.
		TextAsset textAsset = (TextAsset)Resources.Load("XMLData/GameDataScript",typeof(TextAsset));
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(textAsset.text);
		
		_GameScriptList = new GameScript[xmlDoc["GameDataScript"].ChildNodes.Count];
		int xmlIndex = 0;
		foreach(XmlElement xmlElement in xmlDoc["GameDataScript"]) {
			if(xmlElement["id"] != null) {
				GameScript gameScript = new GameScript();
				gameScript.id = System.Convert.ToInt32(xmlElement["id"].InnerText);
				gameScript.script = System.Convert.ToString(xmlElement["data"].InnerText);

				_GameScriptList[xmlIndex] = gameScript;
				
				xmlIndex ++;
			}
		}
		
	}

	public GameScript GetGameScript(int scriptId) {
		int index = 0;
		foreach(GameScript thisGameScript in _GameScriptList) {
			if(thisGameScript != null && thisGameScript.id == scriptId) return thisGameScript;
			index ++;
		}	
		
		return null;
	}

	public string GetMemberNameByMemberId(short memberId) {
		//print("GetMemberNameByMemberId : " + memberId);
		Member member = UserData.getInstence().GetMemberById(memberId);
		DefaultMember defaultMember = MemberData.getInstence().GetDefaultMemberByID(member.DefaultId);
		string memberName = "null";
		if (defaultMember != null) {
			if(member.NameId == 0) {
				memberName = GetDefaultMemberName(defaultMember.nameId);
			} else {
				memberName = GetDefaultMemberName(defaultMember.nameId) + "-" + member.NameId;
			}
		}

		return memberName;
	}

	public string GetDefaultMemberName(int nameId) {
		GameScript gameScript = GetGameScript(nameId);
		return gameScript.script;
	}

	public string GetUnitNameByUnitId(byte id) {
		string unitName = "";
		switch(id) {
		case 1:
			unitName = "ALPHA";
			break;
		case 2:
			unitName = "BRAVO";
			break;
		case 3:
			unitName = "CHARLIE";
			break;
		case 4:
			unitName = "DELTA";
			break;
		case 5:
			unitName = "ECHO";
			break;
		default:
			unitName = "null";
			break;
		}
		return unitName;
	}
}
