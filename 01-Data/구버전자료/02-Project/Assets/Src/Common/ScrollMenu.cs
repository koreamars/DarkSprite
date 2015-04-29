using UnityEngine;
using System.Collections;

public class ScrollMenu : MonoBehaviour {

	public bool isTest;
	public int sortingNum;
	public GameObject BtnObject;
	public GameObject ListBg;
	public Color BtnColor;
	public Color OutLineColor;
	public GameObject BtnListStage;

	internal GameObject[] _BtnObjList;

	private ScriptData _ScriptData;

	private float _BtnYgap = 0;
	private float _CurrentDragY = 0f;
	private float _LimBtnY = 0;
	private float _MaxBtnY = 0;
	private float _BtnX;
	private float _BtnY;
	private Vector2 _BtnScale = new Vector2(0.83f, 0.83f);

	private float _DefaultBtnY = 2.45f;
	private float _ScrollLitYGap = 2.66f;
	private float _ScrollMaxYGap = -2.7f;
	private float _DefaultMoveMaxY = 4.92f;

	internal float _ScrollViewLitY;
	internal float _ScrollViewMaxY;

	internal ListMenuModel[] _ListMenuData;

	private bool _IsDrag;
	private short _CurrentSelectIndex = -1;

	public delegate void BtnCallBack(int id);
	private BtnCallBack _MenuCallback;

	private float BtnListStageY;

	private float _OriBtnX = 0;
	private float _OriBtnY = 0;

	public bool isMemberType = false;

	// Use this for initialization
	public virtual void Awake () {

		_ScriptData = ScriptData.getInstence();

		ListBg.renderer.sortingOrder = sortingNum - 2;

		Color btnTxtColor = Color.white;
		btnTxtColor.r = BtnColor.r;
		btnTxtColor.g = BtnColor.g;
		btnTxtColor.b = BtnColor.b;

		_OriBtnX = BtnListStage.transform.position.x;
		_OriBtnY = BtnListStage.transform.position.y;

		_BtnX = BtnListStage.transform.position.x;
		BtnListStageY = BtnListStage.transform.position.y;

		_ScrollViewLitY = _ScrollLitYGap + BtnListStageY;
		_ScrollViewMaxY = _ScrollMaxYGap + BtnListStageY;

		if(isTest && isMemberType == false) {
			byte count = 15;
			ListMenuModel[] testData = new ListMenuModel[count];
			for(byte i = 0; i < count; i++){
				testData[i] = new ListMenuModel();
				testData[i].id = i;
				testData[i].scriptString = "test : " + i;
			}
			SetScrollData(testData, 1);
			SetScrollView();
		}

	}

	public void SetMenuClick(BtnCallBack onCallBack) {
		_MenuCallback = new BtnCallBack(onCallBack);
	}

	private void OnMouseDrag(int id, Vector2 moveVector)
	{

		if(_CurrentDragY == 0) _CurrentDragY = BtnListStage.transform.position.y;

		float moveY = _CurrentDragY - (moveVector.y / 50);

		if(moveY > _LimBtnY) moveY = _LimBtnY;
		if(moveY < _MaxBtnY) moveY = _MaxBtnY;


		if(moveVector.y != 0 && (moveY != 0 || moveY != _LimBtnY)) 
		{
			BtnListStage.transform.position = new Vector2(_BtnX, moveY);
		}

		print("moveVector.y : " + moveVector.y);
		if(moveVector.y < -2 || moveVector.y > 2)
		{
			_IsDrag = true;
			SetScrollView();
		}

	}

	private void OnMouseClick(int id) {
		//print("OnMouseClick");
		_CurrentDragY = 0; 

		if(_IsDrag == false) {
			short btnIndex = 0;
			if(_CurrentSelectIndex >= 0 && _BtnObjList.Length > _CurrentSelectIndex) {
				_BtnObjList[_CurrentSelectIndex].GetComponent<CommonBtn>().SetBtnSelect(false);
			}

			short optionalId = 0;
			bool isClick = false;

			foreach(ListMenuModel menuData in _ListMenuData){
				if(menuData.id == id) {
					if(menuData.isClick == false) {
						if(_BtnObjList.Length > 0 && _BtnObjList[_CurrentSelectIndex] != null) {
							_BtnObjList[_CurrentSelectIndex].GetComponent<CommonBtn>().SetBtnSelect(true);
						}
						break;
					}
					optionalId = menuData.optionalId;
					_CurrentSelectIndex = btnIndex;
					_BtnObjList[btnIndex].GetComponent<CommonBtn>().SetBtnSelect(true);
					isClick = menuData.isClick;
					break;
				}

				btnIndex ++;
			}

			if(optionalId > 0) {
				if(_MenuCallback != null && isClick == true) _MenuCallback(optionalId);
			} else {
				if(_MenuCallback != null && isClick == true) _MenuCallback(id);
			}

		} else {
			if(_BtnObjList.Length < _CurrentSelectIndex) _BtnObjList[_CurrentSelectIndex].GetComponent<CommonBtn>().SetBtnSelect(true);
		}
		_IsDrag = false;
	}

	public virtual void SetScrollView() {

		foreach(GameObject btnObj in _BtnObjList)
		{
			if(btnObj.transform.position.y < _ScrollViewLitY && btnObj.transform.position.y > _ScrollViewMaxY)
			{
				btnObj.GetComponent<CommonBtn>().SetAlpha(1);
				btnObj.transform.position = new Vector2(btnObj.transform.parent.transform.position.x, btnObj.transform.position.y);
			}
			else
			{
				btnObj.GetComponent<CommonBtn>().SetAlpha(0);
				btnObj.transform.position = new Vector2(20f, btnObj.transform.position.y);
			}
		}
	}

	public virtual void SetScrollData(ListMenuModel[] dataList, short selectIndex) {

		if(_ScriptData == null) _ScriptData = ScriptData.getInstence();

		DeleteBtns();
		_ListMenuData = dataList;
		if(selectIndex >= -1) _CurrentSelectIndex = selectIndex;
		//_OriBtnX = this.gameObject.transform.position.x;
		//_OriBtnY = this.gameObject.transform.position.y;
		BtnListStage.transform.position = new Vector2(_OriBtnX, _OriBtnY);

		CreateBtns();
	}

	internal void DeleteBtns() {
		if(_BtnObjList == null) return;
		foreach(GameObject btn in _BtnObjList)
		{
			Destroy(btn);
		}

		_BtnObjList = null;
	}

	internal void CreateBtns() {

		ListBg.renderer.sortingOrder = sortingNum - 2;

		_BtnY = _DefaultBtnY + BtnListStage.transform.position.y;

		short btnIndex = 0;
		CommonBtn btnCommonBtn;

		Color btnTxtColor = Color.white;
		btnTxtColor.r = BtnColor.r;
		btnTxtColor.g = BtnColor.g;
		btnTxtColor.b = BtnColor.b;

		_BtnObjList = new GameObject[_ListMenuData.Length];

		foreach(ListMenuModel listMenuModel in _ListMenuData)
		{
			_BtnObjList[btnIndex] = Instantiate(BtnObject, new Vector3(_BtnX, _BtnY, 0), Quaternion.identity) as GameObject;
			btnCommonBtn = _BtnObjList[btnIndex].GetComponent<CommonBtn>() as CommonBtn;
			string name = "null";
			if(listMenuModel.scriptId > 0) {
				name = _ScriptData.GetGameScript(listMenuModel.scriptId).script;
			} else {
				name = listMenuModel.scriptString;
			}

			btnCommonBtn.FontOutLineColor = OutLineColor;
			/*
			if(listMenuModel.optionalId > 0) {
				btnCommonBtn.Init(listMenuModel.optionalId, name, sortingNum, btnTxtColor);
			} else {
				btnCommonBtn.Init(listMenuModel.id, name, sortingNum, btnTxtColor);
			}*/
			btnCommonBtn.Init(listMenuModel.id, name, sortingNum, btnTxtColor);

			if(listMenuModel.fontColor != Color.white) btnCommonBtn.SetTxtColor(listMenuModel.fontColor);
			if(listMenuModel.outLineColor != Color.black) btnCommonBtn.SetTxtOutColor(listMenuModel.outLineColor);

			btnCommonBtn.SetDragCallBack(OnMouseDrag);
			btnCommonBtn.SetClick(OnMouseClick);
			btnCommonBtn.SetDownClick(OnMouseDown);
			if(btnIndex == _CurrentSelectIndex) btnCommonBtn.SetBtnSelect(true);
			_BtnObjList[btnIndex].transform.localScale = _BtnScale;
			
			_BtnObjList[btnIndex].transform.parent = BtnListStage.transform;

			if(_BtnYgap == 0) _BtnYgap = btnCommonBtn.GetBtnSize().y + 0.1f;
			_BtnY -= _BtnYgap;
			
			btnIndex ++;
		}

		_LimBtnY = (_BtnYgap * _BtnObjList.Length) - _DefaultMoveMaxY;
		//_MaxBtnY = BtnListStageY + 0.5f;
		_MaxBtnY = BtnListStageY;

		_ScrollViewLitY = _ScrollLitYGap + BtnListStage.transform.position.y;
		_ScrollViewMaxY = _ScrollMaxYGap + BtnListStage.transform.position.y;
	}

	private void OnMouseDown(int id) {

		_ScrollViewLitY = _ScrollLitYGap + BtnListStageY;
		_ScrollViewMaxY = _ScrollMaxYGap + BtnListStageY;
	}

}
