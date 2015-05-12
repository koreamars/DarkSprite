using UnityEngine;
using System.Collections;

public class PayAni : MonoBehaviour {

	public bool IsTest;

	private int Value;
	private bool isPlus;

	private GameObject PayObject;

	void Start() {
		if(IsTest) {
			BillModel billTest = new BillModel();
			billTest.core = 10;
			AniStart(billTest);
		}
	}

	public void AniStart(BillModel billModel) {

		PayObject = Instantiate(Resources.Load<GameObject>("Common/RewardViewer")) as GameObject;
		PayObject.transform.parent = this.gameObject.transform;
		PayObject.transform.position = this.gameObject.transform.position;
		PayObject.transform.localScale = new Vector3(1f, 1f, 1f);
		PayObject.GetComponent<RewardViewer>().init(billModel, 100, true);
		PayObject.GetComponent<RewardViewer>().SetLayerSort(LayerMask.NameToLayer("Alert"));

		iTween.MoveTo(PayObject, iTween.Hash("y", 1f,"speed" ,1f , "easetype", iTween.EaseType.linear
		                                  , "oncomplete", "OnAniComplete", "oncompletetarget", this.gameObject));
		iTween.ColorTo(PayObject, iTween.Hash("a", 0f));
	}

	private void OnAniComplete() {
		Destroy(this.gameObject);
	}
}
