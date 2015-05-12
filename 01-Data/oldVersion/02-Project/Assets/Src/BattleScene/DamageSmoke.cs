using UnityEngine;
using System.Collections;

public class DamageSmoke : MonoBehaviour {

	public bool isTest;

	private float motionSpeed = 200f;
	private GameObject _TargetObj;

	private Color _NormalDamageColor;

	private float _Timer;
	private bool _OnSmoke;
	private bool _IsEnemy;

	// Use this for initialization
	void Start () {

		_NormalDamageColor = Color.white;
		_NormalDamageColor.a = 0.5f;

		if(isTest) SetSmoke(new GameObject(), true, _NormalDamageColor);
	}

	void Update() {
		_Timer += Time.deltaTime;
		if(_Timer > 0.05f) {
			if(_OnSmoke == true) CreateSmoke();
			_Timer = 0;
		}
	}

	public void SetSmoke(GameObject targetObj, bool isEnemy, Color smokeColor) {
		_TargetObj = targetObj;
		_OnSmoke = true;
		_IsEnemy = isEnemy;
		_NormalDamageColor = smokeColor;
	}


	private void CreateSmoke() {
		GameObject smokeObj = new GameObject();
		float scale = _TargetObj.transform.localScale.x * 0.1f;
		float currentSpeed = motionSpeed * scale;
		float endX;
		float endY;

		if(_IsEnemy) {
			endX = (smokeObj.transform.position.x * scale) + 18;
			endY = (smokeObj.transform.position.y * scale) - 5;
			smokeObj.layer = LayerMask.NameToLayer("Enemy");
		} else {
			endX = (smokeObj.transform.position.x * scale) - 8;
			endY = (smokeObj.transform.position.y * scale) + 6;
			smokeObj.layer = LayerMask.NameToLayer("Unit");
		}
		//smokeObj.transform.position = new Vector2(_TargetObj.transform.position.x - 0.8f, _TargetObj.transform.position.y - 0.4f);
		smokeObj.transform.position = new Vector2(_TargetObj.transform.position.x - (6.3f * scale), _TargetObj.transform.position.y - (7.6f * scale));
		smokeObj.transform.parent = this.transform;
		//float scale = UnityEngine.Random.Range(3, 5);
		//scale = _TargetObj.transform.localScale.x;
		smokeObj.transform.localScale = new Vector2(scale * 5f, scale * 5f);
		float randomRoc = UnityEngine.Random.Range(0, 180) * 1f;
		smokeObj.transform.localRotation = Quaternion.Euler(0f, 0f,randomRoc);

		if(smokeObj.GetComponent ("SpriteRenderer") == null) smokeObj.AddComponent<SpriteRenderer>();
		SpriteRenderer renderer = (SpriteRenderer)smokeObj.GetComponent ("SpriteRenderer");
		renderer.material.color = _NormalDamageColor;
		renderer.sprite = Resources.Load<Sprite>("UnitResource/EffectImg/Smoke");
		
		iTween.MoveTo(smokeObj, iTween.Hash("x", endX, "y", endY, "speed", currentSpeed, "easetype", iTween.EaseType.linear
		                                    , "oncomplete", "SmokeEnd", "oncompletetarget", this.gameObject
		                                    , "oncompleteparams", smokeObj));
	}


	private void SmokeEnd(GameObject smokeObj) {
		Destroy(smokeObj);
		//CreateSmoke();
	}

	public void DestroySmoke() {
		_OnSmoke = false;
	}

}
