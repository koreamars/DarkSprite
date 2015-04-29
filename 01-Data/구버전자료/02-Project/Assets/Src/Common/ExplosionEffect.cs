using UnityEngine;
using System.Collections;

public class ExplosionEffect : MonoBehaviour {

	public bool isTest;

	// Use this for initialization
	void Start () {
		//CreateSmoke();
	}

	public void CreateSmoke(Vector2 StartPos, bool isEnemy) {
		//GameObject smokeObj;
		//this.transform.position = StartPos;
		/*
		float delayCount = 11.8f;
		for(byte i = 0; i < 5; i ++) {
			smokeObj = new GameObject();
			float posX = UnityEngine.Random.Range(0, 25);
			float posY = UnityEngine.Random.Range(0, 25);
			float scale = UnityEngine.Random.Range(2, 4);
			posX = posX * 0.1f + StartPos.x - 2f;
			posY = posY * 0.1f + StartPos.y - 2f;
			scale = scale * 0.2f;
			smokeObj.transform.localScale = new Vector2(scale, scale);
			
			smokeObj.transform.position = new Vector2(posX, posY);
			smokeObj.transform.parent = this.transform;
			smokeObj.AddComponent<SpriteRenderer>();
			SpriteRenderer renderer = (SpriteRenderer)smokeObj.GetComponent ("SpriteRenderer");
			Color smokeColor = Color.white;
			smokeColor.a = 0.3f;
			smokeColor.r = 1f;
			smokeColor.g = (float)(UnityEngine.Random.Range(8, 10) * 0.1f);
			smokeColor.b = 0.5f;
			renderer.material.color = smokeColor;
			renderer.sprite = Resources.Load<Sprite>("UnitResource/EffectImg/Smoke");
			renderer.sortingOrder = 120;

			float endX = -8;
			float endY = 6;
			smokeObj.layer = LayerMask.NameToLayer("Unit");
			if(isEnemy) {
				endX = 15;
				endY = -3;
				smokeObj.layer = LayerMask.NameToLayer("Enemy");
			}

			if(i == 0) {
				iTween.MoveTo(smokeObj, iTween.Hash("x", endX, "y", endY, "speed", delayCount, "easetype", iTween.EaseType.linear
				                                    , "oncomplete", "SmokeEnd", "oncompletetarget", this.gameObject));
			} else {
				iTween.MoveTo(smokeObj, iTween.Hash("x", endX, "y", endY, "speed", delayCount, "easetype", iTween.EaseType.linear));
			}
			iTween.FadeTo(smokeObj, iTween.Hash("alpha", 0f));
		}
		*/
	}

	private void SmokeEnd() {
		if(isTest == false) {
			Destroy(this.gameObject);
		}
	}

	void OnGUI() {
		if(isTest) {
			if(GUI.Button(new Rect(10, 10, 100, 30), "test")) {
				CreateSmoke(new Vector2(0, 0), false);
			}
		}
	}
}
