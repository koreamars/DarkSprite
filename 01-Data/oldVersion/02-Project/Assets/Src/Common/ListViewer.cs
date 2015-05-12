using UnityEngine;
using System.Collections;

public class ListViewer : MonoBehaviour {

	public static byte UserGearType = 0;
	public static byte DefaultGearType = 1;
	public static byte UserMemberType = 2;
	public static byte DefaultMemberType = 3;

	public bool IsTest;
	public GameObject LeftArrowBtn;
	public GameObject RightArrowBtn;
	public GameObject CloseBtn;

	private SystemData _systemData;
	private GameObject[] listObj;

	private byte _currentPageN = 0;
	private int _maxPageN = 1;

	public delegate void BtnCallBack(int id);
	private BtnCallBack _Callback;
	public delegate void CloseCallBack();
	private CloseCallBack _CloseCallback;

	private GameObject _TitleTextObj;

	private int callbackId = 0;

	void Start() {
		if(IsTest) {
			StartCoroutine(TestInit());
		}
	}

	private IEnumerator TestInit() {

		yield return new WaitForEndOfFrame();
		
		yield return StartCoroutine(MissionData.getInstence().init());
		
		LocalData.getInstence().AllLoad();
		DarkSprite.getInstence();
		SystemData.GetInstance();
		MemberData.getInstence();
		UserData.getInstence();
		
		Init(ListViewer.UserGearType, TestCallback, TestCloseCallback);

	}

	private void TestCallback(int id) {
		print("TestCallback : " + id);
	}

	private void TestCloseCallback() {
		print("TestCloseCallback");
	}

	public void Init(byte type, BtnCallBack onCallBack, CloseCallBack onCloseCallBack) {

		_Callback = new BtnCallBack(onCallBack);
		_CloseCallback = new CloseCallBack(onCloseCallBack);

		_systemData = SystemData.GetInstance();

		string titleStr = ScriptData.getInstence().GetGameScript(150161).script;
		_TitleTextObj = CustomTextMesh.SetAddTextMesh(titleStr, 20, TextAnchor.UpperCenter, Color.white, 50, "Alert");
		_TitleTextObj.transform.parent = this.gameObject.transform;
		_TitleTextObj.transform.position = new Vector2(0f, 4f);

		// 버튼 설정.
		LeftArrowBtn.transform.position = new Vector2(_systemData.screenLeftX + 0.5f, 0f);
		RightArrowBtn.transform.position = new Vector2(_systemData.screenRightX - 0.5f, 0f);
		LeftArrowBtn.GetComponent<ButtonEvent>().SetCallBack(OnLeftArrowBtn);
		RightArrowBtn.GetComponent<ButtonEvent>().SetCallBack(OnRightArrowBtn);
		CloseBtn.transform.position = new Vector2(_systemData.screenRightX - 1f, 3.78f);
		CloseBtn.GetComponent<ButtonEvent>().SetCallBack(OnCloseBtn);

		float windowX = (_systemData.screenLeftX * -1f) + _systemData.screenRightX;

		float slotWidth = (windowX - 2f) / 5;
		float posX = (slotWidth * 2f) * -1 ;

		listObj = new GameObject[5];

		for(byte i = 0; i < 5; i++) {
			GameObject obj = Instantiate(Resources.Load<GameObject>("Common/ListViewObject")) as GameObject;
			obj.transform.position = new Vector2(posX + (slotWidth * i), 0f);
			obj.GetComponent<ListViewObject>().Init(ListViewObject.InvenGearType, OnSlotClick);
			listObj[i] = obj;
		}

		SetUserGearByDarkGear();
	}

	/** 객체 삭제 */
	public void DestoryObject() {
		foreach(GameObject obj in listObj) {
			Destroy(obj);
		}
	}

	/** 사용자 장비 정보 노출 */
	private void SetUserGearByDarkGear() {

		ArrayList ownDarkGearList = new ArrayList();

		// 다크기어로 만들 수 있는 기어 확인.
		foreach(OwnGear newOwnGear in UserData.getInstence().UserOwnGearList) {
			Gear newGear = GearData.getInstence().GetGearByID(newOwnGear.gearId);
			if(newGear.upNextId > 0 && newOwnGear.ownCount > 0) ownDarkGearList.Add(newOwnGear);
			_maxPageN = ownDarkGearList.Count;
		}

		if(ownDarkGearList.Count == 0) _TitleTextObj.GetComponent<TextMesh>().text = ScriptData.getInstence().GetGameScript(150162).script;

		GameObject GearDataViewObj;
		OwnGear ownGear;
		byte pageN = 0;
		for(byte i = 0; i < 5; i++) {
			pageN = (byte)(_currentPageN + i);

			GearDataViewObj = listObj[i];
			if(ownDarkGearList.Count > pageN) {
				ownGear = ownDarkGearList[pageN] as OwnGear;
				if(GearDataViewObj != null) {
					GearDataViewObj.GetComponent<ListViewObject>().UpdateData(ListViewObject.InvenGearType, ownGear.gearId);
				} 
			} else {
				GearDataViewObj.GetComponent<ListViewObject>().ClearData();
			}
		}
	}

	private void OnLeftArrowBtn() {
		if(_currentPageN == 0) return;
		_currentPageN -= 5;
		print("OnLeftArrowBtn " + _currentPageN);
		SetUserGearByDarkGear();
	}

	private void OnRightArrowBtn() {
		if((_currentPageN + 5) > _maxPageN) return;
		_currentPageN += 5;
		print("OnRightArrowBtn " + _currentPageN);
		SetUserGearByDarkGear();
	}

	private void OnCloseBtn() {
		print("OnCloseBtn");
		if(_CloseCallback != null) _CloseCallback();
	}

	private void OnSlotClick(int id) {

		callbackId = id;
		if(_Callback != null) _Callback(callbackId);
	}
}
