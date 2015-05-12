using UnityEngine;
using System.Collections;

public class CloudEffect : MonoBehaviour {

	public bool IsMyTeam;

	private float _Timer;
	private float _defaultSpeed = 6f;
	private float _defaultScale = 7.8f;

	private byte _cloudCount = 0;

	// Use this for initialization
	void Start () {
	
	}

	void Update () {
		_Timer += Time.deltaTime;
		if(_Timer > 1) {
			CreateCloud();
			_Timer = 0;
		}
	}

	private void CreateCloud() {

		float startX = this.gameObject.transform.position.x;

		int scaleRandom = UnityEngine.Random.Range(70, 100);
		float speed = (scaleRandom / 100f) * _defaultSpeed;
		float scale = (scaleRandom / 100f) * _defaultScale;
		float startY = (scaleRandom / 100f) * -30f;

		Color cloudColor = Color.white;
		cloudColor.a = scaleRandom / 100f;

		float endX = 0;
		string objName;
		if(IsMyTeam) {
			objName = "Unit";
			endX = -10f;
		} else {
			objName = "Enemy";
			endX = 15f;
			startY -= 13f;
		}

		GameObject cloudObj = SetAddSpriteRenderer("BattleScene/Cloud0" + _cloudCount, cloudColor, 0, objName);
		cloudObj.renderer.sortingOrder = -20;
		cloudObj.transform.position = new Vector2(startX, 22f + startY);
		cloudObj.transform.localScale = new Vector3(scale, scale, scale);
		iTween.MoveTo(cloudObj, iTween.Hash("x", endX, "y", 32f + startY, "speed", speed, "oncomplete", "CloudMoveEnd", "oncompletetarget", this.gameObject
		                                    , "oncompleteparams", cloudObj, "easetype", iTween.EaseType.linear));

		_cloudCount ++;
		if(_cloudCount > 4) _cloudCount = 0;
	}

	private GameObject SetAddSpriteRenderer(string uri, Color color, int sortNum, string layerName) {
		GameObject targetobj = new GameObject();
		targetobj.layer = LayerMask.NameToLayer(layerName);
		targetobj.AddComponent<SpriteRenderer>();
		SpriteRenderer renderer = targetobj.GetComponent<SpriteRenderer>();
		renderer.sortingOrder = sortNum;
		renderer.color = color;
		renderer.sprite = Resources.Load<Sprite>(uri);
		
		return targetobj;
	}

	private void CloudMoveEnd(GameObject cloudObj) {
		Destroy(cloudObj);
	}
}
