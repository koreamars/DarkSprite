using UnityEngine;
using System.Collections;

public class ShareAlertPopup : MonoBehaviour {

	public GameObject CommentTxt1;
	public GameObject CommentTxt2;
	public GameObject CommentTxt3;
	public GameObject CommentTxt4;
	public GameObject AlertBg;
	public GameObject CloseBtn;
	public GameObject KakaoBtn;

	public delegate void CloseEvent();
	private CloseEvent _CloseCallback;

	private string _ThumbUrl = "";
	private string _NameStr = "";

	void Start() {
		//init(10, "신규대원 획득.<p>리아 테일러를 획득하였습니다.<p>이 소식을 친구들과 공유하세요.");
	}

	public void init(byte sortingNum, string comment, string thumbUrl, string nameStr) {

		UserData.getInstence().IsShowAlert = true;

		_ThumbUrl = thumbUrl;
		_NameStr = nameStr;

		CommentTxt1.GetComponent<TextMesh>().text = "";
		CommentTxt2.GetComponent<TextMesh>().text = "";
		CommentTxt3.GetComponent<TextMesh>().text = "";
		CommentTxt4.GetComponent<TextMesh>().text = "";
		CommentTxt1.renderer.sortingOrder = sortingNum + 1;
		CommentTxt2.renderer.sortingOrder = sortingNum + 1;
		CommentTxt3.renderer.sortingOrder = sortingNum + 1;
		CommentTxt4.renderer.sortingOrder = sortingNum + 1;
		AlertBg.renderer.sortingOrder = sortingNum;
		AlertBg.transform.localScale = new Vector2(1.1f, 1.1f);

		CloseBtn.GetComponent<ButtonEvent>().SetCallBack(OnCloseBtn);
		CloseBtn.renderer.sortingOrder = sortingNum;

		KakaoBtn.GetComponent<ButtonEvent>().SetCallBack(OnkakaoBtn);
		KakaoBtn.renderer.sortingOrder = sortingNum;

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

	private void OnCloseBtn() {
		Destroy(this.gameObject);
	}

	private void OnkakaoBtn() {

		if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("KakaoSend", _NameStr);

		Application.OpenURL("http://www.clana.kr/DarkSprite/kakaoWeb.php?itemName=" + _NameStr + "&thumburl=http://clana.kr/DarkSprite/images/" + _ThumbUrl);


		Destroy(this.gameObject);
	}

}
