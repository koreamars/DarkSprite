using UnityEngine;
using System.Collections;

public class AlertPopup : MonoBehaviour {

	public GameObject CommentTxt1;
	public GameObject CommentTxt2;
	public GameObject CommentTxt3;
	public GameObject CommentTxt4;
	public GameObject AlertBg;
	public GameObject AlertBtn1;
	public GameObject AlertBtn2;

	private bool _IsConfirmType;

	private GameObject _BillSB;

	public delegate void ConfirmPopupCallback(bool isOk);
	private ConfirmPopupCallback _ConfirmPopupCallback;

	void Start() {
		/*
		BillModel testmodel = new BillModel();
		testmodel.money = 999999;
		testmodel.core = 9999;
		testmodel.corechip = 9999;
		init("자금이 부족합니다.","확인" ,10, testmodel);
		SetConfirmData("확인", "취소", ConfirmTest);
		*/
	}

	public void init(string comment, string btnName, byte sortingNum, BillModel billData) {

		UserData.getInstence().IsShowAlert = true;

		// 다크기어 팝업 제거.
		GameObject darkGearPopup = GameObject.Find("DarkGearPopup") as GameObject;
		if(darkGearPopup != null) Destroy(darkGearPopup);

		CommentTxt1.GetComponent<TextMesh>().text = "";
		CommentTxt2.GetComponent<TextMesh>().text = "";
		CommentTxt3.GetComponent<TextMesh>().text = "";
		CommentTxt4.GetComponent<TextMesh>().text = "";
		CommentTxt1.GetComponent<Renderer>().sortingOrder = sortingNum + 1;
		CommentTxt2.GetComponent<Renderer>().sortingOrder = sortingNum + 1;
		CommentTxt3.GetComponent<Renderer>().sortingOrder = sortingNum + 1;
		CommentTxt4.GetComponent<Renderer>().sortingOrder = sortingNum + 1;
		AlertBg.GetComponent<Renderer>().sortingOrder = sortingNum;
		AlertBg.transform.localScale = new Vector2(1.1f, 1.1f);

		AlertBtn1.GetComponent<CommonBtn>().Init(0, btnName, sortingNum + 2, Color.white);
		AlertBtn1.GetComponent<CommonBtn>().SetClick(OnBtnClick);
		AlertBtn1.transform.position = new Vector3(0f, -1.45f, -5f);

		AlertBtn2.GetComponent<CommonBtn>().Init(0, btnName, sortingNum + 2, Color.white);
		AlertBtn2.GetComponent<CommonBtn>().SetEnabled(false);
		AlertBtn2.GetComponent<CommonBtn>().SetClick(OnConfirmBtnClick);
		AlertBtn2.transform.position = new Vector3(0f, -2f, 0f);

		if(billData.core > 0 || billData.corechip > 0 || billData.money > 0) {
			GameObject rewardViewerObj = Instantiate(Resources.Load<GameObject>("Common/RewardViewer")) as GameObject;
			rewardViewerObj.transform.parent = this.gameObject.transform;
			rewardViewerObj.GetComponent<RewardViewer>().init(billData, sortingNum + 3, true);
			rewardViewerObj.GetComponent<RewardViewer>().SetLayerSort(LayerMask.NameToLayer("Alert"));
			rewardViewerObj.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
			rewardViewerObj.transform.position = new Vector2(0f, -0.5f);

		}

		_IsConfirmType = false;

		string currentComment = comment;
		currentComment = TextView(CommentTxt1, currentComment);

		if(currentComment.Length <= 0) return;

		currentComment = TextView(CommentTxt2, currentComment);

		if(currentComment.Length <= 0) return;
		
		currentComment = TextView(CommentTxt3, currentComment);

		if(currentComment.Length <= 0) return;
		
		currentComment = TextView(CommentTxt4, currentComment);

	}

	private string TextView(GameObject commentObj, string comment) {
		string returnStr = "";
		int space1Count = comment.IndexOf("<p>");
		if(space1Count >= 0) {
			commentObj.GetComponent<TextMesh>().text = comment.Substring(0, space1Count);
			returnStr = comment.Substring(space1Count + 3);
		} else {
			commentObj.GetComponent<TextMesh>().text = comment;
			returnStr = "";
		}

		return returnStr;
	}

	public void SetConfirmData(string okStr, string cancelStr, ConfirmPopupCallback OnConfirmPopupCallback) {
		_ConfirmPopupCallback = new ConfirmPopupCallback(OnConfirmPopupCallback);
		AlertBtn1.GetComponent<CommonBtn>().SetBtnName(cancelStr);
		AlertBtn2.GetComponent<CommonBtn>().SetBtnName(okStr);
		AlertBtn1.transform.position = new Vector3(1.3f, -1.45f, -5f);
		AlertBtn2.transform.position = new Vector3(-1.3f, -1.45f, -5f);
		AlertBtn2.GetComponent<CommonBtn>().SetEnabled(true);
		_IsConfirmType = true;
	}

	private void ConfirmTest(bool isTest) {
		print("isTest : " + isTest);
	}

	private void OnBtnClick(int id) {
		UserData.getInstence().IsShowAlert = false;
		if(_IsConfirmType) {
			if(_ConfirmPopupCallback != null) _ConfirmPopupCallback(false);
		}
		Destroy(this.gameObject);
	}

	private void OnConfirmBtnClick(int id) {
		UserData.getInstence().IsShowAlert = false;
		if(_ConfirmPopupCallback != null) _ConfirmPopupCallback(true);
		Destroy(this.gameObject);
	}
}
