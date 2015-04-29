using UnityEngine;
using System.Collections;

public class StoryOpenPopup : MonoBehaviour {

	public GameObject MainBg;

	private GameObject _OkayBtn;
	private GameObject _CancelBtn;

	public delegate void MainBtnUpdate();
	private MainBtnUpdate _MainBtnUpdate;

	void Start() {
		MainBg.renderer.sortingOrder = 299;

		_OkayBtn = Instantiate(Resources.Load<GameObject>("Common/CommonBtn01")) as GameObject;
		_OkayBtn.GetComponent<CommonBtn>().Init(0, "스토리팩 구매", 312, Color.white);
		_OkayBtn.GetComponent<CommonBtn>().SetDownClick(OnOkayBtn);
		_OkayBtn.transform.parent = this.gameObject.transform;
		_OkayBtn.transform.position = new Vector2(0f, -3.35f);

		_CancelBtn = new GameObject();
		_CancelBtn.AddComponent<BoxCollider>();
		_CancelBtn.GetComponent<BoxCollider>().size = new Vector3(2f,2f, 1f);
		_CancelBtn.AddComponent<SpriteRenderer>();
		_CancelBtn.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("MainScene/Popup/PopupBase/PopupCloseBtn");
		_CancelBtn.renderer.sortingOrder = 302;
		_CancelBtn.transform.parent = this.gameObject.transform;
		_CancelBtn.transform.localScale = new Vector2(1.6f, 1.6f);
		_CancelBtn.transform.position = new Vector2(4.8f, 3.4f);
		_CancelBtn.AddComponent<ButtonEvent>().SetCallBack(OnCancelBtn);

		MainBg.AddComponent<BoxCollider2D>().size = new Vector2(20f, 20f);

	}

	public void SetMainBtnUpdate(MainBtnUpdate OnMainBtnUpdate) {
		_MainBtnUpdate = new MainBtnUpdate(OnMainBtnUpdate);
	}

	private void OnOkayBtn(int id) {
		print("OnOkayBtn");
		Destroy(this.gameObject);

		ShopData.getInstence().SetChargeRequest("storypack01", OnChargeRequestCom);
	}

	private void OnChargeRequestCom(bool isSuccess) {
		print("OnChargeRequestCom");

		// 스팩 증가.
		SystemData systemdata = SystemData.GetInstance();
		systemdata.BaseUpgradeMax = 4;
		systemdata.ResearchUpgradeMax = 2;
		systemdata.FactoryUpgradeMax = 8;

		systemdata.MaxOwnItemCount = 30;

		systemdata.MemberMaxClass = 15;

		systemdata.IsStoryCharge = true;

		LocalData.getInstence().UserStoryStepSave();

		if(_MainBtnUpdate != null) _MainBtnUpdate();
	}

	private void OnCancelBtn() {
		print("OnCancelBtn");
		Destroy(this.gameObject);
	}
}
