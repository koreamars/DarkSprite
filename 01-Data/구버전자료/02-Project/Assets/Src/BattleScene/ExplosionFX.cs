using UnityEngine;
using System.Collections;

public class ExplosionFX : MonoBehaviour {

	// Use this for initialization
	public void StartFx (bool type) {

		GameObject EffectObj = this.gameObject.transform.FindChild("EffectObj").gameObject;

		EffectObj.transform.localScale = new Vector2(0f, 0f);
		//float point = -10f;
		//if(type == false) point = 10f;
		iTween.MoveTo(EffectObj, iTween.Hash("time", 0.6f, "oncomplete", "MoveComplete", "oncompletetarget", this.gameObject
		                                           , "easetype", iTween.EaseType.easeInCirc));
		iTween.ScaleTo(EffectObj, iTween.Hash("x", 1f, "y", 1f, "time", 0.3f));
		iTween.ColorTo(EffectObj, iTween.Hash("r", 0f, "g", 0f, "b", 0f, "a", 0f, "time", 0.4f));
		iTween.RotateTo(EffectObj, new Vector3(0,0,90),1);
	}

	private void MoveComplete() {
		Destroy(this.gameObject);
	}

}
