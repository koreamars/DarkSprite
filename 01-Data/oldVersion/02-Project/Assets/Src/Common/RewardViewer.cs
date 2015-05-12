using UnityEngine;
using System.Collections;

public class RewardViewer : MonoBehaviour {

	public Color FontOutLineColor;
	public Color PlusFontColor;
	public Color MinusFontColor;

	private float _TotalWidth = 0;
	private GameObject _BillObject;

	private GameObject CoinObject;
	private GameObject CoreObject;
	private GameObject ChipObject;
	private GameObject CoinTxtObj;
	private GameObject CoreTxtObj;
	private GameObject ChipTxtObj;

	void Awake() {
		/*
		BillModel testBill = new BillModel();
		testBill.money = 9999999;
		//testBill.core = 99999;
		testBill.corechip = 99999;
		testBill.corechipPlus = false;
		init(testBill, 1);
		*/
	}

	public void init(BillModel billModel, int sortNum, bool centerAlign) {

		_TotalWidth = 0;
		if(_BillObject != null) Destroy(_BillObject);

		_BillObject = new GameObject();
		_BillObject.transform.parent = this.gameObject.transform;

		if(CoreObject != null) Destroy(CoreObject);
		if(ChipObject != null) Destroy(ChipObject);
		if(CoinObject != null) Destroy(CoinObject);
		if(CoinTxtObj != null) Destroy(CoinTxtObj);
		if(CoreTxtObj != null) Destroy(CoreTxtObj);
		if(ChipTxtObj != null) Destroy(ChipTxtObj);

		byte index = 0;

		if(billModel.core >= 0) {
			CoreObject = CreateBillSB(0, billModel.core, index, sortNum, billModel.corePlus);
			index ++;
		}

		if(billModel.corechip >= 0) {
			ChipObject = CreateBillSB(1, billModel.corechip, index, sortNum, billModel.corechipPlus);
			index ++;
		}

		if(billModel.money >= 0) {
			CoinObject = CreateBillSB(2, billModel.money, index, sortNum, billModel.moneyPlus);
			index ++;
		}

		if(centerAlign) {
			_BillObject.transform.position = new Vector2(_TotalWidth / 2f * -1f + this.gameObject.transform.position.x, 0f + this.gameObject.transform.position.y);
		} else {
			_BillObject.transform.position = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
		}
	}

	public void SetLayerSort(int layerNum) {
		if(CoreObject != null) CoreObject.layer = layerNum;
		if(ChipObject != null) ChipObject.layer = layerNum;
		if(CoinObject != null) CoinObject.layer = layerNum;
		if(CoinTxtObj != null) CoinTxtObj.GetComponent<OutLineFont>().SetSortLayer(LayerMask.LayerToName(layerNum));
		if(CoreTxtObj != null) CoreTxtObj.GetComponent<OutLineFont>().SetSortLayer(LayerMask.LayerToName(layerNum));
		if(ChipTxtObj != null) ChipTxtObj.GetComponent<OutLineFont>().SetSortLayer(LayerMask.LayerToName(layerNum));
	}

	private GameObject CreateBillSB(byte type, int value, byte index, int sortNum, bool isPlus) {
		if(value <= 0) return null;

		SpriteRenderer renderer;
		GameObject valueTxtObj = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
		valueTxtObj.transform.parent = _BillObject.transform.transform;
		OutLineFont valueTxt = valueTxtObj.GetComponent<OutLineFont>();
		if(isPlus) {
			valueTxt.SetString("" + value.ToString());
			PlusFontColor.a = 1f;
			valueTxt.SetFontColor(PlusFontColor);
		} else {
			valueTxt.SetString("" + value.ToString());
			MinusFontColor.a = 1f;
			valueTxt.SetFontColor(MinusFontColor);
		}
		valueTxt.SetSort(sortNum);
		valueTxt.SetAlign(TextAnchor.MiddleLeft);
		valueTxt.SetFontSize(22);
		FontOutLineColor.a = 1f;
		valueTxt.SetLineColor(FontOutLineColor);
		valueTxtObj.transform.position = new Vector2(0.4f + _TotalWidth,0f);
		
		GameObject CoinSB = new GameObject();
		CoinSB.transform.parent = _BillObject.transform.transform;
		CoinSB.transform.position = new Vector2(0f + _TotalWidth, 0f);

		CoinSB.AddComponent<SpriteRenderer>();
		renderer = CoinSB.GetComponent<SpriteRenderer>();
		if(type == 0) {
			renderer.sprite = Resources.Load<Sprite>("Common/CoreSymbol");
			CoreTxtObj = valueTxtObj;
		} else if (type == 1) {
			renderer.sprite = Resources.Load<Sprite>("Common/CoreChipSymbol");
			ChipTxtObj = valueTxtObj;
		} else {
			renderer.sprite = Resources.Load<Sprite>("Common/PaySymbol");
			CoinTxtObj = valueTxtObj;
		}
		renderer.sortingOrder = sortNum;

		_TotalWidth += renderer.bounds.size.x;
		_TotalWidth += valueTxt.GetTextWidth();

		return CoinSB;
	}
}
