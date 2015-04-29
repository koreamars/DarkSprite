using UnityEngine;
using System.Collections;

public class SystemMessage : MonoBehaviour {

	public bool isTest;

	private GameObject[] _SystemMegObjList;
	private ArrayList _MessageList;

	private SystemData _SystemData;

	private short _MaxCount = 10;

	// Use this for initialization
	void Start () {
		if(isTest) {
			init();
		}
	}

	/** 필드 생성 */
	public void init() {

		_SystemData = SystemData.GetInstance();

		_SystemMegObjList = new GameObject[10];

		_MessageList = new ArrayList();

		float posY = _SystemData.screenTopY - 1.2f;
		for(byte i = 0; i< 10; i ++) {
			_SystemMegObjList[i] = Instantiate(Resources.Load<GameObject>("OutlineFont"), new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
			_SystemMegObjList[i].transform.parent = this.gameObject.transform;
			_SystemMegObjList[i].transform.localScale = new Vector3(1f, 1f, 1f);

			OutLineFont outLineFont = _SystemMegObjList[i].GetComponent<OutLineFont>();
			outLineFont.SetString("systemMessage test");
			outLineFont.SetAlign(TextAnchor.MiddleRight);
			outLineFont.SetSort(-4);
			outLineFont.SetFontSize(16);


			_SystemMegObjList[i].transform.position = new Vector3(_SystemData.screenRightX - 0.1f, posY, 0f);

			posY -= 0.4f;
		}

		ShowMessage();
	}

	public void AddMessage(string text, Color color) {
		GameMessage gameMessage = new GameMessage();
		gameMessage.massge = text;
		gameMessage.color = color;
		_MessageList.Add(gameMessage);
		ShowMessage();
	}

	private void ShowMessage() {

		if(_MessageList.Count > _MaxCount) {
			_MessageList.RemoveAt(0);

		}

		for(byte i = 0; i< 10; i ++) {
			OutLineFont outLineFont = _SystemMegObjList[i].GetComponent<OutLineFont>();
			if((_MessageList.Count - 1) >= i) {
				GameMessage message = _MessageList[i] as GameMessage;
				outLineFont.SetString(message.massge);

				Color msgColor = Color.white;
				if(message.color == Color.red) {
					msgColor.r = 1f;
					msgColor.g = 0.2f;
					msgColor.b = 0f;
				} else {
					msgColor.r = 1f;
					msgColor.g = 0.8f;
					msgColor.b = 0f;
				}

				outLineFont.SetFontColor(msgColor);

			} else {
				outLineFont.SetString("");
			}
		}
	}

}
