using UnityEngine;
using System.Xml;
using System.Collections;

public class ShopData : MonoBehaviour {

	private static ShopData _instence;

	public ShopValueModel[] ShopDataList;

	public delegate void RequestComplete(bool isSeccess);
	public RequestComplete _RequestComplete;


	public static ShopData getInstence(){
		if(_instence == null) 
		{
			//_instence = new DataController();
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<ShopData>();
			DontDestroyOnLoad(_instence);
		}
		
		return _instence;
	}

	void Awake () {
		// 과금 정보 초기화.
		TextAsset textAsset = (TextAsset)Resources.Load("XMLData/ShopValueData",typeof(TextAsset));
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(textAsset.text);
		
		ShopDataList = new ShopValueModel[xmlDoc["ShopValueData"].ChildNodes.Count];

		
		int xmlIndex = 0;
		foreach(XmlElement xmlElement in xmlDoc["ShopValueData"]) {
			ShopValueModel model = new ShopValueModel();
			model.id = System.Convert.ToByte(xmlElement["id"].InnerText);
			model.androidId = xmlElement["androidId"].InnerText;
			model.type = System.Convert.ToByte(xmlElement["type"].InnerText);
			model.value = System.Convert.ToInt32(xmlElement["value"].InnerText);
			model.dollarCost = System.Convert.ToSingle(xmlElement["dollarCost"].InnerText);
			model.wonCost = System.Convert.ToInt32(xmlElement["wonCost"].InnerText);

			ShopDataList[xmlIndex] = model;
			
			xmlIndex ++;
		}
	}

	public ShopValueModel GetShopValueModelByID(byte id) {

		foreach(ShopValueModel model in ShopDataList) {
			if(model.id == id) return model;
		}

		return null;
	}

	/** 결제 요청 */
	public void SetChargeRequest(string modelId, RequestComplete OnRequestComplete) {
		_RequestComplete = new RequestComplete(OnRequestComplete);

		OnShopRequestComplete(true);
	}

	/** 결제 요청 완료 */
	private void OnShopRequestComplete(bool isSuccess) {

		_RequestComplete(isSuccess);
	}



}
