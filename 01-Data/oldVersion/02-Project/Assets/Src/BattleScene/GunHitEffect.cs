using UnityEngine;
using System.Collections;

public class GunHitEffect : MonoBehaviour {

	// Use this for initialization
	public void StartFx(string layerName) {
		GameObject hitAniObj = this.gameObject.transform.FindChild("HitEffectObject").gameObject;
		hitAniObj.layer = LayerMask.NameToLayer(layerName);
		StartCoroutine(DelayDelete());
	}

	private IEnumerator DelayDelete() {
		yield return new WaitForSeconds(1f);
		Destroy(this.gameObject);
	}

}
