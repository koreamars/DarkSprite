using UnityEngine;
using System.Collections;

public class CommonBtn : MonoBehaviour {

	public int btnId;
	public string BtnName;
	public int SortingNum;
	public Color TxtColor;
	public Color FontOutLineColor;
	public GameObject OnbtnImg;
	public GameObject OffBtnImg;
	public GameObject SelectBtnImg;

	private GameObject BtnNameTxt;

	private bool _IsSelect = false;
	private bool _IsEnable = true;

	public delegate void BtnDownCallBack(int id);
	private BtnDownCallBack _BtnDownCallback;
	public delegate void BtnCallBack(int id);
	private BtnCallBack _Callback;
	public delegate void DragCallBack(int id, Vector2 move);
	private DragCallBack _DragCallBack;

	private Vector2 _MouseMove;

	// Use this for initialization
	void Awake () {
		if(BtnNameTxt == null) {
			BtnNameTxt = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
			BtnNameTxt.transform.parent = this.gameObject.transform;
			//BtnNameTxt.GetComponent<OutLineFont>().SetString("");

		}

		Init(btnId, BtnName, SortingNum, TxtColor);
		SetBtnSelect(false);
	}

	public void SetClick(BtnCallBack onCallBack) {
		_Callback = new BtnCallBack(onCallBack);
	}

	public void SetDownClick(BtnDownCallBack onDownCallBack) {
		_BtnDownCallback = new BtnDownCallBack(onDownCallBack);
	}

	public void SetDragCallBack(DragCallBack onDragCallBack) {
		_DragCallBack = new DragCallBack(onDragCallBack);
		
	}

	public void Init(int id, string name, int sortNum, Color color) {
		btnId = id;
		BtnName = name;
		if(BtnNameTxt != null) {
			BtnNameTxt.GetComponent<OutLineFont>().SetString(BtnName);
			BtnNameTxt.transform.position = this.gameObject.transform.position;
			BtnNameTxt.transform.localScale = new Vector2(1, 1);
			BtnNameTxt.GetComponent<OutLineFont>().SetSortLayer(LayerMask.LayerToName(this.gameObject.layer));
			BtnNameTxt.GetComponent<OutLineFont>().SetSort(sortNum);
			FontOutLineColor.a = 1f;
			BtnNameTxt.GetComponent<OutLineFont>().SetLineColor(FontOutLineColor);
			BtnNameTxt.GetComponent<OutLineFont>().SetFontSize(20);
			BtnNameTxt.GetComponent<OutLineFont>().SetLineSize(1.5f);
		}
		TxtColor = Color.yellow;
		TxtColor.r = color.r;
		TxtColor.g = color.g;
		TxtColor.b = color.b;

		if(OnbtnImg != null) OnbtnImg.GetComponent<Renderer>().sortingOrder = sortNum - 1;
		if(OffBtnImg != null) OffBtnImg.GetComponent<Renderer>().sortingOrder = sortNum - 1;
		if(SelectBtnImg != null) SelectBtnImg.GetComponent<Renderer>().sortingOrder = sortNum - 1;
		SortingNum = sortNum;

		SetTxtColor(TxtColor);
	}

	/**
	 * 버튼 선택 표시 유무
	 * */
	public void SetBtnSelect(bool isSelect)
	{
		_IsSelect = isSelect;
		if(_IsSelect == true)
		{
			if(OffBtnImg != null) OffBtnImg.GetComponent<Renderer>().enabled = false;
			if(OnbtnImg != null) OnbtnImg.GetComponent<Renderer>().enabled = false;
			if(SelectBtnImg != null) SelectBtnImg.GetComponent<Renderer>().enabled = true;
		}
		else
		{
			if(OffBtnImg != null) OffBtnImg.GetComponent<Renderer>().enabled = true;
			if(OnbtnImg != null) OnbtnImg.GetComponent<Renderer>().enabled = false;
			if(SelectBtnImg != null) SelectBtnImg.GetComponent<Renderer>().enabled = false;
		}
	}

	public void SetTextAnchor(TextAnchor anchor) {
		if(BtnNameTxt != null) {
			BtnNameTxt.GetComponent<OutLineFont>().SetAlign(anchor);
			if(anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.UpperLeft || anchor == TextAnchor.LowerLeft) {
				BtnNameTxt.transform.position = new Vector2(-1.6f + this.gameObject.transform.position.x, this.gameObject.transform.position.y);
			} else {
				//BtnNameTxt.transform.position = new Vector2(0f, 0f);
			}
		}
	}

	public void SetSortLayer(string sortName) {
		BtnNameTxt.GetComponent<OutLineFont>().SetSortLayer(sortName);
		if(OnbtnImg != null) OnbtnImg.layer = LayerMask.NameToLayer(sortName);
		if(OffBtnImg != null) OffBtnImg.layer = LayerMask.NameToLayer(sortName);
		if(SelectBtnImg != null) SelectBtnImg.layer = LayerMask.NameToLayer(sortName);
	}

	public void SetTxtColor(Color color) {
		TxtColor.r = color.r;
		TxtColor.g = color.g;
		TxtColor.b = color.b;
		if(BtnNameTxt != null) BtnNameTxt.GetComponent<OutLineFont>().SetFontColor(TxtColor);
	}

	public void SetTxtOutColor(Color color) {
		if(BtnNameTxt != null) BtnNameTxt.GetComponent<OutLineFont>().SetLineColor(color);
	}

	public void SetTxtSize(byte size) {
		if(BtnNameTxt != null) BtnNameTxt.GetComponent<OutLineFont>().SetFontSize(size);
	}

	public Vector2 GetBtnSize() {

		return new Vector2(OffBtnImg.GetComponent<Renderer>().bounds.size.x, OffBtnImg.GetComponent<Renderer>().bounds.size.y);
	}

	public void SetAlpha(float alpha) {
		if(OnbtnImg != null) OnbtnImg.GetComponent<Renderer>().material.color = new Color(1, 1, 1, alpha);
		if(OffBtnImg != null) OffBtnImg.GetComponent<Renderer>().material.color = new Color(1, 1, 1, alpha);
		if(SelectBtnImg != null) SelectBtnImg.GetComponent<Renderer>().material.color = new Color(1, 1, 1, alpha);
		TxtColor.a = alpha;
		if(BtnNameTxt != null) BtnNameTxt.GetComponent<OutLineFont>().SetFontColor(TxtColor);
	}

	public void SetBtnName(string name) {
		if(BtnNameTxt != null) BtnNameTxt.GetComponent<OutLineFont>().SetString(name);
	}

	public void SetEnabled(bool state) {
		if(state) {
			_IsEnable = true;
			if(BtnNameTxt != null) {
				BtnNameTxt.GetComponent<OutLineFont>().SetEnable(_IsEnable);
			}
			SetBtnSelect(_IsSelect);
		} else {
			_IsEnable = false;
			if(BtnNameTxt != null) BtnNameTxt.GetComponent<OutLineFont>().SetEnable(_IsEnable);
			if(OffBtnImg != null) OffBtnImg.GetComponent<Renderer>().enabled = false;
			if(OnbtnImg != null) OnbtnImg.GetComponent<Renderer>().enabled = false;
			if(SelectBtnImg != null) SelectBtnImg.GetComponent<Renderer>().enabled = false;
		}
	}

	void OnMouseDown ()
	{
		print("OnMouseDown");
		if(_IsEnable == false) return;
		_MouseMove = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		if(SelectBtnImg != null) SelectBtnImg.GetComponent<Renderer>().enabled = false;
		if(OnbtnImg != null) {
			if(OffBtnImg != null) OffBtnImg.GetComponent<Renderer>().enabled = false;
			OnbtnImg.GetComponent<Renderer>().enabled = true;
		} else {
			if(OffBtnImg != null) OffBtnImg.GetComponent<Renderer>().enabled = true;
		}
		if(_BtnDownCallback != null) _BtnDownCallback(btnId);
	}

	void OnMouseUp ()
	{
		print("OnMouseUp");
		if(_IsEnable == false) return;
		if(_IsSelect == true)
		{
			if(OffBtnImg != null) OffBtnImg.GetComponent<Renderer>().enabled = false;
			if(OnbtnImg != null) OnbtnImg.GetComponent<Renderer>().enabled = false;
			if(SelectBtnImg != null) SelectBtnImg.GetComponent<Renderer>().enabled = true;
		}
		else
		{
			if(OffBtnImg != null) OffBtnImg.GetComponent<Renderer>().enabled = false;
			if(OnbtnImg != null) OnbtnImg.GetComponent<Renderer>().enabled = true;
			if(SelectBtnImg != null) SelectBtnImg.GetComponent<Renderer>().enabled = false;
		}
		SetBtnSelect(false);
		if(_Callback != null) _Callback(btnId);
	}

	void OnMouseDrag() {
		if(_IsEnable == false) return;
		Vector2 CurrentMove = new Vector2(_MouseMove.x - Input.mousePosition.x, _MouseMove.y - Input.mousePosition.y);
		if(_DragCallBack != null) _DragCallBack(btnId, CurrentMove);
	}
}
