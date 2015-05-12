using UnityEngine;
using System.Collections;

public class ListViewObject : MonoBehaviour {

	public static byte InvenGearType = 0;	// 보유 장비 형.
	public static byte MemberType = 1;

	public GameObject Thumbnail;
	public GameObject OutLine;
	public bool IsTest;

	private GameObject TitleTxt = null;

	public delegate void BtnCallBack(int id);
	private BtnCallBack _Callback;

	private byte _CurrentType = 0;
	private int callbackId = 0;

	private ArrayList _DataTxtList;

	void Start() {
		if(IsTest == true) {
			Init(ListViewObject.InvenGearType, TestCallback);
			UpdateData(ListViewObject.InvenGearType, 5);
		}
	}

	public void TestCallback(int id) {
		print("TestCallback : " + id);
	}

	public void Init(byte type, BtnCallBack onCallBack) {
		_CurrentType = type;
		_Callback = new BtnCallBack(onCallBack);
	}

	public void UpdateData(byte type, short id) {

		_CurrentType = type;

		if(_DataTxtList != null && _DataTxtList.Count > 0) {
			foreach(GameObject txtObj in _DataTxtList) {
				Destroy(txtObj);
			}
		}

		_DataTxtList = new ArrayList();
		if(_CurrentType == ListViewObject.InvenGearType) {	// 장비형
			ShowGearData((short)(id));
		} else {								// 맴버형.
			ShowMemberData((short)(id));
		}
		OutLine.renderer.enabled = true;
	}

	public void ClearData() {
		SpriteRenderer renderer = (SpriteRenderer)Thumbnail.GetComponent ("SpriteRenderer");
		renderer.sprite = Resources.Load<Sprite>("");
		SetTitleText("");
		OutLine.renderer.enabled = false;

		if(_DataTxtList != null && _DataTxtList.Count > 0) {
			foreach(GameObject txtObj in _DataTxtList) {
				Destroy(txtObj);
			}
		}
	}

	/** 장비 정보를 보여줌. */
	private void ShowGearData(short id) {

		Gear gear = GearData.getInstence().GetGearByID(id);

		callbackId = (int)(id);

		SpriteRenderer renderer = (SpriteRenderer)Thumbnail.GetComponent ("SpriteRenderer");
		renderer.sprite = Resources.Load<Sprite>("UnitResource/Thumbnail/" + gear.thumbnailURI);

		string gearName = ScriptData.getInstence().GetGameScript(gear.scriptId).script;
		SetTitleText(gearName);
		if(_CurrentType == ListViewObject.InvenGearType) ShowInvenTypeGearData(id);
	}

	/** 맴버 정보를 보여줌 */
	private void ShowMemberData(short id) {
		DefaultMember defaultMember = MemberData.getInstence().GetDefaultMemberByID(id);

		SpriteRenderer renderer = (SpriteRenderer)Thumbnail.GetComponent ("SpriteRenderer");
		renderer.sprite = Resources.Load<Sprite>("MemberImg/Member" + defaultMember.thumbId);
	}

	/** 타이틀을 노출합니다. */
	private void SetTitleText(string text) {
		if(TitleTxt == null) {
			TitleTxt = CustomTextMesh.SetAddTextMesh(text, 18, TextAnchor.MiddleCenter, Color.white, 5, "Alert");
			TitleTxt.name = "TitleText";
			TitleTxt.transform.parent = this.gameObject.transform;
			TitleTxt.transform.position = new Vector2(this.gameObject.transform.position.x, 1.21f);
		} else {
			TitleTxt.GetComponent<TextMesh>().text = text;
		}


	}

	private void ShowInvenTypeGearData(short id) {
		Gear gear = GearData.getInstence().GetGearByID(id);
		OwnGear ownGear = UserData.getInstence().GetOwnGearByGearId(id);

		GameObject ownCountTxt = GetText("보유량 : " + ownGear.ownCount);
		ownCountTxt.transform.position = new Vector2(this.gameObject.transform.position.x, -1.21f);
		_DataTxtList.Add(ownCountTxt);

	}

	private GameObject GetText(string text) {
		GameObject txtObj = CustomTextMesh.SetAddTextMesh(text, 16, TextAnchor.MiddleCenter, Color.white, 5, "Alert");
		txtObj.transform.parent = this.gameObject.transform;
		return txtObj;
	}

	void OnMouseDown() {
		if(_Callback != null) _Callback(callbackId);
	}

}
