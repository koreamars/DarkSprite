using UnityEngine;
using System.Collections;

public class DamageText : MonoBehaviour {

	public bool IsTest;

	private short _DamageValue = 0;
	private GameObject _DamageTxtObj;

	void Start() {
		if(IsTest) {
			this.transform.position = new Vector2(-5f, 0);
			init(1000, 0, "UI", Color.red, false);
		}
	}

	public void init(short damageValue, short sortNum, string layerName, Color OutLineColor, bool isPlus) {
		_DamageValue =damageValue;

		_DamageTxtObj = Instantiate(Resources.Load<GameObject>("OutlineFont")) as GameObject;
		OutLineFont font = _DamageTxtObj.GetComponent<OutLineFont>();
		if(isPlus == true) {
			font.SetString("+" + _DamageValue.ToString());
		} else {
			font.SetString("-" + _DamageValue.ToString());
		}
		font.SetFontColor(Color.white);
		font.SetLineColor(OutLineColor);
		font.SetFontSize(32);
		font.SetSort(sortNum);
		font.SetSortLayer(layerName);

		_DamageTxtObj.transform.parent = this.transform;
		_DamageTxtObj.transform.position = this.transform.position;

		iTween.MoveTo(_DamageTxtObj, iTween.Hash("y", this.transform.position.y + 1f,"time", 0.6f, "oncomplete", "TxtMoveEnd"
		                                        , "oncompletetarget", this.gameObject, "easetype", iTween.EaseType.linear));
		iTween.ColorTo(_DamageTxtObj, iTween.Hash("a", 0,"time", 0.6f, "easetype", iTween.EaseType.linear));
	}

	private void TxtMoveEnd() {
		Destroy(_DamageTxtObj);
		Destroy(this.gameObject);
	}
}
