using UnityEngine;
using System.Collections;

public class MyUnitField : MonoBehaviour {

	public GameObject BgObject;
	public Camera MyUnityCamera;
	public GameObject DamageTxt;
	public bool isTest;

	private ArrayList _UnitList;

	private float _zPosDefault = -2f;
	private float _zPosCap = 1f;
	private Vector3[] _UnitPosition;
	private Vector2[] _UnitDefaultScale;
	private Vector3[] _UnitOutPosition;

	private short _CurrentSelectN = 0;

	private int[] sortNumArray = new int[]{20, 10, 30, 40, 0};

	public delegate void OpeingEndCallBack();
	private OpeingEndCallBack _OpeningEndCallback;

	public delegate void MotionEndFunc();
	private MotionEndFunc _MotionEndFunc;

	void Awake () {

		_UnitPosition = new Vector3[]{ new Vector3(4.02f, 2.19f, _zPosDefault + (_zPosCap * 2))
			, new Vector3(7.3f, 2.8f, _zPosDefault + (_zPosCap * 3))
			, new Vector3(7.5f, 0.4f, _zPosDefault + (_zPosCap * 1))
			, new Vector3(4.5f, -0.37f, _zPosDefault)
			, new Vector3(5.32f, 4.03f, _zPosDefault + (_zPosCap * 4))};

		_UnitDefaultScale = new Vector2[]{ new Vector2(0.22f, 0.22f)
			, new Vector2(0.20f, 0.20f)
			, new Vector2(0.25f, 0.25f)
			, new Vector2(0.3f, 0.3f)
			, new Vector2(0.19f, 0.19f)};

		_UnitOutPosition = new Vector3[]{ new Vector3(-0.3f, 0.0f, 0.4f)
			, new Vector3(13.4f, 5.2f, 0.35f)
			, new Vector3(14.0f, -2.2f, 0.45f)
			, new Vector3(1.4f, -7.2f, 0.5f)
			, new Vector3(0.8f, 7.6f, 0.3f)};

		MyUnityCamera.GetComponent<Camera>().rect = new Rect (0f, 0, 1, 1);

		DamageTxt.renderer.sortingOrder = 10;
		DamageTxt.renderer.enabled = false;
	}

	void Start () {
		if(isTest)
		{
			Unit unit = new Unit();
			unit.id = 0;
			unit.memberList[0] = (byte)(1);
			unit.memberList[1] = (byte)(2);
			unit.memberList[2] = (byte)(3);
			unit.memberList[3] = (byte)(4);
			unit.memberList[4] = (byte)(5);

			init(unit);
		}
	}

	/** 전투 유닛들 초기화 */
	public IEnumerator init(Unit unit) {

		_UnitList = new ArrayList();

		for (byte i = 0; i < unit.memberList.Length; i++)
		{
			yield return new WaitForEndOfFrame();

			if(unit.memberList[i] <= 0) {
				yield return 0;
				continue;
			}

			yield return StartCoroutine(SetUnitData(unit.memberList[i], i));

			GameObject unitObj = _UnitList[i] as GameObject;

			if(i == 0) {
				//unitObj.GetComponent<UnitFrame>().SetIntroAni();
				unitObj.transform.position = new Vector3(35f, -10f, 0f);
				unitObj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
				iTween.MoveTo(unitObj, iTween.Hash("delay", 1f,"x", 7.8f, "y", -2.1f,"time", 3.5f, "oncomplete", "IntroStep2", "oncompletetarget", this.gameObject
				                                   , "easetype", iTween.EaseType.easeOutQuint, "oncompleteparams", unitObj));
			} else {
				unitObj.transform.position = new Vector3(-35f, -10f, 0f);
			}

		}

		DamageTxt.renderer.sortingOrder = 50;

	}

	/** 딘일 유닛 초기화 */
	private IEnumerator SetUnitData(short memberId, byte index) {
		yield return new WaitForEndOfFrame();

		GameObject unitObj = Instantiate(Resources.Load<GameObject>("UnitResource/UnitFrame"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

		yield return StartCoroutine(unitObj.GetComponent<UnitFrame>().init((byte)(memberId), (int)(sortNumArray[index])));

		unitObj.transform.localScale = _UnitDefaultScale[index];
		_UnitList.Add(unitObj);

	}

// 오프닝 =====================================================================================================
	private void IntroStep1(GameObject unitObj) {
		iTween.MoveTo(unitObj, iTween.Hash("x", 6.0f, "y", 11.8f,"speed", 1.5f, "oncomplete", "IntroStep2", "oncompletetarget", this.gameObject
		                                   , "easetype", iTween.EaseType.linear, "oncompleteparams", unitObj));
	}

	private void IntroStep2(GameObject unitObj) {
		iTween.MoveTo(unitObj, iTween.Hash("x", -5f, "y", 30f,"time", 1.5f, "oncomplete", "CameraMoveStart", "oncompletetarget", this.gameObject
		                                   , "easetype", iTween.EaseType.easeInQuint));
	}


	private void CameraMoveStart() {
		Hashtable cameraht = iTween.Hash("from", 1f, "to", 0.5f, "time", 0.5f, "onupdate", "CameraScaleMove"
		                                 , "oncomplete", "UnitOpening", "oncompletetarget", this.gameObject);
		iTween.ValueTo(this.gameObject, cameraht);
	}

	private void CameraScaleMove(float value) {
		MyUnityCamera.GetComponent<Camera>().rect = new Rect (0f, 0, value, 1);
	}

	private void UnitOpening() {

		byte index = 0;

		foreach(GameObject unitObj in _UnitList) {

			unitObj.transform.position = new Vector3(_UnitPosition[index].x - 8f, _UnitPosition[index].y + 2.5f, 0);
			unitObj.transform.localScale = _UnitDefaultScale[index];
			Hashtable vals = new Hashtable();
			vals.Add("unitObj", unitObj);
			vals.Add("isComplete", true);
			vals.Add("endPos", new Vector2(_UnitPosition[index].x, _UnitPosition[index].y));

			iTween.MoveTo(unitObj, iTween.Hash("delay", (index * 0.2f), "oncomplete", "SetUnitOpening", "oncompletetarget", this.gameObject
			                                   , "oncompleteparams", vals));
			index ++;
		}
	}

	/** 유닛을 등장 시킴 */
	private void SetUnitOpening(object paramObj) {

		Hashtable ht = (Hashtable)paramObj;
		GameObject unitObj = (GameObject)ht["unitObj"];
		bool isComplete = (bool)ht["isComplete"];
		Vector2 endPos = (Vector2)ht["endPos"];


		unitObj.GetComponent<UnitFrame>().SetFrontRunEnd();
		if(isComplete) {
			iTween.MoveTo(unitObj, iTween.Hash("x", endPos.x, "y", endPos.y
			                                   , "oncomplete", "OnUnitMoveComplete", "oncompletetarget", this.gameObject
			                                   , "oncompleteparams", unitObj,"speed", 15f, "easetype", iTween.EaseType.easeOutCirc));
		} else {
			iTween.MoveTo(unitObj, iTween.Hash("x", endPos.x, "y", endPos.y, "speed", 15f, "easetype", iTween.EaseType.easeOutCirc));
		}

	}

// 오프닝 =====================================================================================================

	public void UnitSelect(short modelId, MotionEndFunc OnMotionEndFunc) {

		OutMotionEnd();

		if(OnMotionEndFunc != null) _MotionEndFunc = new MotionEndFunc(OnMotionEndFunc);

		byte index = 0;
		foreach(GameObject unitObj in _UnitList) {
			if(unitObj.GetComponent<UnitFrame>().GetMemberModelId() == modelId) {
				break;
			}
			index ++;
		}

		_CurrentSelectN = index;
		GameObject SelectUnitObj = _UnitList[_CurrentSelectN] as GameObject;
		
		SelectInMotion(true);
	}

	public void UnitUnSelect() {
		SelectInMotion(false);
	}

	public void UnitGunFire(byte slotN) {
		GameObject SelectUnitObj = _UnitList[_CurrentSelectN] as GameObject;
		SelectUnitObj.GetComponent<UnitFrame>().SetGunFire(slotN);
	}

	public void SetOpeningEndCallback(OpeingEndCallBack onCallBack) {
		_OpeningEndCallback = new OpeingEndCallBack(onCallBack);
	}

	public void SetTargetSelect(short modelId) {
		byte index = 0;
		if(_CurrentSelectN >= 0) {
			GameObject prevUnit = _UnitList[_CurrentSelectN] as GameObject;
			if(prevUnit != null) prevUnit.GetComponent<UnitFrame>().SetSelectMark(false);
		}
		foreach(GameObject unitObj in _UnitList) {
			if(unitObj.GetComponent<UnitFrame>().GetMemberModelId() == modelId) {
				unitObj.GetComponent<UnitFrame>().SetSelectMark(true);
				break;
			}
			index ++;
		}
		_CurrentSelectN = index;

	}

	public void UnitOut(short modelId) {
		GameObject SelectUnitObj = _UnitList[_CurrentSelectN] as GameObject;
		foreach(GameObject unitObj in _UnitList) {
			if(unitObj.GetComponent<UnitFrame>().GetMemberModelId() == modelId) {
				unitObj.GetComponent<UnitFrame>().SetUnitOut();
			}
		}
	}

	public void SetTargetAttack(short damage) {
		GameObject SelectUnitObj = _UnitList[_CurrentSelectN] as GameObject;
		SelectUnitObj.GetComponent<UnitFrame>().SetHitAni();

		short memberId = SelectUnitObj.GetComponent<UnitFrame>().GetMemberModelId();
		Member member = UserData.getInstence().GetMemberById(memberId);
		if(member.CurrentHP < (member.MaxHP / 3)) {
			SelectUnitObj.GetComponent<UnitFrame>().SetDamageSmoke(true);
		}

		DamageTxt.renderer.enabled = true;
		DamageTxt.GetComponent<TextMesh>().text = "-" + damage;
		DamageTxt.transform.position = SelectUnitObj.transform.position;

		iTween.ColorUpdate(DamageTxt, iTween.Hash("a", 1f, "time", 0f));
		iTween.MoveTo(DamageTxt, iTween.Hash("y", DamageTxt.transform.position.y + 1f));
		iTween.ColorTo(DamageTxt, iTween.Hash("a", 0f, "oncomplete", "DamageTxtMoveEnd", "oncompletetarget", this.gameObject));


	}

	private void DamageTxtMoveEnd(){

	}

	private void SelectInMotion(bool isSelect) {
		GameObject SelectUnitObj = _UnitList[_CurrentSelectN] as GameObject;

		string callbackName = "";
		if(isSelect) {
			callbackName = "SelectInMotionEnd";
		} else {
			callbackName = "SelectOutMotionEnd";
		}

		SelectUnitObj.GetComponent<UnitFrame>().SetFrontRunEnd();
		iTween.MoveTo(SelectUnitObj, iTween.Hash("x", -2.7f, "easetype", iTween.EaseType.easeInCirc
		                                         , "oncomplete", callbackName, "oncompletetarget", this.gameObject));
	}

	private void OnUnitMoveComplete(GameObject unitObj) {

		if(_OpeningEndCallback != null) _OpeningEndCallback();
	}

	private void SelectInMotionEnd() {
		GameObject unitObj = _UnitList[_CurrentSelectN] as GameObject;
		unitObj.GetComponent<UnitFrame>().SetFrontRunEnd();
		unitObj.GetComponent<UnitFrame>().SetSorting(100);
		//SetAllScale(0.05f);
		iTween.MoveTo(unitObj, iTween.Hash("x", 6f, "y", 3, "oncomplete", "MotionEnd", "oncompletetarget", this.gameObject));
		unitObj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
		//MyUnityCamera.orthographicSize = 7;

		iTween.ValueTo(this.gameObject, iTween.Hash("from", MyUnityCamera.orthographicSize, "to", 7,"speed" , 5f, "onupdate", "CameraSizeMove"));

	}

	private void SelectOutMotionEnd() {
		//SetAllScale(0f);
		GameObject unitObj = _UnitList[_CurrentSelectN] as GameObject;
		unitObj.GetComponent<UnitFrame>().SetFrontRunEnd();
		unitObj.GetComponent<UnitFrame>().SetSorting(sortNumArray[_CurrentSelectN]);
		iTween.MoveTo(unitObj, iTween.Hash("x", _UnitPosition[_CurrentSelectN].x, "y", _UnitPosition[_CurrentSelectN].y
		                                   , "oncomplete", "OutMotionEnd", "oncompletetarget", this.gameObject));
		unitObj.transform.localScale = new Vector2(_UnitDefaultScale[_CurrentSelectN].x, _UnitDefaultScale[_CurrentSelectN].y);
		_CurrentSelectN = -1;
		//MyUnityCamera.orthographicSize = 5;
		iTween.ValueTo(this.gameObject, iTween.Hash("from", MyUnityCamera.orthographicSize, "to", 5,"speed" , 5f, "onupdate", "CameraSizeMove"));
	}

	private void MotionEnd() {
		if(_MotionEndFunc != null) _MotionEndFunc();
	}

	private void OutMotionEnd() {
		byte index = 0;

		foreach(GameObject unitObj in _UnitList) {
			unitObj.GetComponent<UnitFrame>().SetSelectMark(false);
			index ++;
		}
	}

	private void CameraSizeMove(float size) {
		MyUnityCamera.orthographicSize = size;
	}

	private void SetAllScale(float mScale) {
		short index = 0;
		foreach(GameObject unitObj in _UnitList) {
			if(index != _CurrentSelectN) {
				iTween.ScaleTo(unitObj, iTween.Hash("scale", new Vector3(_UnitDefaultScale[index].x - mScale, _UnitDefaultScale[index].y - mScale, 1)));
			}
			index ++;
		}

	}

	void OnGUI () {
		if(isTest) {
			if(GUI.Button(new Rect(10, 10, 60, 30), "select1"))
				UnitSelect((short)(1), testCallback);
			if(GUI.Button(new Rect(10, 50, 60, 30), "select2"))
				UnitSelect((short)(2), testCallback);
			if(GUI.Button(new Rect(10, 90, 60, 30), "select3"))
				UnitSelect((short)(3), testCallback);
			if(GUI.Button(new Rect(10, 130, 60, 30), "select4"))
				UnitSelect((short)(4), testCallback);
			if(GUI.Button(new Rect(10, 170, 60, 30), "select5"))
				UnitSelect((short)(5), testCallback);

			if(GUI.Button(new Rect(70, 10, 60, 30), "hit1"))
				SetTargetAttack(10);
			if(GUI.Button(new Rect(70, 50, 60, 30), "hit2"))
				SetTargetAttack(10);
			if(GUI.Button(new Rect(70, 90, 60, 30), "hit3"))
				SetTargetAttack(20);
			if(GUI.Button(new Rect(70, 130, 60, 30), "hit4"))
				SetTargetAttack(30);
			if(GUI.Button(new Rect(70, 170, 60, 30), "hit5"))
				SetTargetAttack(40);

			if(GUI.Button(new Rect(10, 210, 100, 30), "unSelect"))
				UnitUnSelect();

		}
	}

	private void testCallback() {

	}


}
