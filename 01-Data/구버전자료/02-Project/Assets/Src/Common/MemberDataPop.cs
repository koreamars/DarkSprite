using UnityEngine;
using System.Collections;

public class MemberDataPop : MonoBehaviour {
	
	public GameObject GetMark;
	public GameObject CloseBtn;
	public GameObject KakaoBtn;
	public GameObject BgObject;
	public bool IsTest;
	
	private GameObject _BillSB;
	private GameObject _titleTxt;
	private GameObject _MemberDataUIObj;
	private MemberDataUI _MemberDataUI;
	private GameObject _CommentTxt;
	
	public delegate void CloseCallback();
	private CloseCallback _CloseCallback;
	
	private short _CurrentMemberId = 0;
	private int _SortNum = 0;
	private string _NameStr;
	private string _ThumbUrl;

	private BillModel _CurrentBillModel;
	
	void Start() {
		
		if(IsTest) {
			init(1, 10);
		}
	}
	
	public void init(short memberId, byte sortingNum) {
		_CurrentMemberId = memberId;
		_SortNum = sortingNum;

		DefaultMember member = MemberData.getInstence().GetDefaultMemberByID(_CurrentMemberId);
		_NameStr = ScriptData.getInstence().GetGameScript(member.nameId).script;
		_ThumbUrl = "member/Member" + member.thumbId + ".jpg";

		// 타이틀 텍스트.
		string titleStr = ScriptData.getInstence().GetGameScript(160167).script;
		_titleTxt = CustomTextMesh.SetAddTextMesh(titleStr, 22, TextAnchor.UpperCenter, Color.white, _SortNum + 1, "Alert");
		_titleTxt.transform.parent = this.gameObject.transform;
		_titleTxt.transform.position = new Vector2(0f, 2f);

		// 코멘트.
		string commentStr = ScriptData.getInstence().GetGameScript(150154).script;
		_CommentTxt = CustomTextMesh.SetAddTextMesh(commentStr, 15, TextAnchor.UpperCenter, Color.white, _SortNum + 1, "Alert");
		_CommentTxt.transform.parent = this.gameObject.transform;
		_CommentTxt.transform.position = new Vector2(0f, 1.3f);

		_MemberDataUIObj = Instantiate(Resources.Load<GameObject>("Common/MemberDataUI")) as GameObject;
		_MemberDataUIObj.transform.parent = this.gameObject.transform;
		_MemberDataUIObj.transform.localScale = new Vector2(0.75f, 0.75f);
		_MemberDataUIObj.transform.position = new Vector2(-2f, -0.1f);
		_MemberDataUI = _MemberDataUIObj.GetComponent<MemberDataUI>();
		_MemberDataUI.init(_SortNum + 5);
		_MemberDataUI.MemberUpdate(_CurrentMemberId, true);
		_MemberDataUI.SetSortLayer("Alert");

		GetMark.renderer.sortingOrder = _SortNum + 7;
		CloseBtn.renderer.sortingOrder = _SortNum + 1;
		KakaoBtn.renderer.sortingOrder = _SortNum + 1;
		BgObject.renderer.sortingOrder = _SortNum;
		_titleTxt.renderer.sortingOrder = _SortNum + 1;
		_CommentTxt.renderer.sortingOrder = _SortNum + 1;

		CloseBtn.GetComponent<ButtonEvent>().SetCallBack(OnCloseBtn);
		KakaoBtn.GetComponent<ButtonEvent>().SetCallBack(OnKakaoBtn);
	}

	public void SetCloseCallback(CloseCallback OnCloseCallback) {
		_CloseCallback = new CloseCallback(OnCloseCallback);
	}
	
	private void OnCloseBtn() {
		PopupClose(0);
	}
	
	private void OnKakaoBtn() {
		print("OnKakaoBtn");
		if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("KakaoSend", _NameStr);
		string paramStr = "itemName=" + _NameStr + "&thumburl=http://clana.kr/DarkSprite/images/" + _ThumbUrl;
		NetworkData.getInstence().SendKakao(paramStr, _NameStr);
		Destroy(this.gameObject);
	}
	
	private void PopupClose(int id) {
		if(_CloseCallback != null) _CloseCallback();
		Destroy(this.gameObject);
	}
	
	
	
}
