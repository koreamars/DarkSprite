using UnityEngine;
using System.Collections;

public class EnemyFrame : MonoBehaviour {

	public GameObject EnemyImg;
	public GameObject SelectMark;
	public bool IsTest;
	public AudioSource ExplosionAudio;
	public AudioSource LaunchAudio;
	public AudioSource GunHitAudio;

	private byte _EnemyId;

	public delegate void BtnCallBack(byte id);
	private BtnCallBack _Callback;

	private GameObject _SmokeObject;
	private Vector2 _SmokeStartPos;

	// Use this for initialization
	void Start () {
		SelectMark.renderer.enabled = false;
	}

	public void SetEnemyId(byte id) {
		_EnemyId = id;
		Ghost ghost = WarzoneData.getInstence().GetGhostByGhostId(id);
		DefaultGhost defaultGhost = WarzoneData.getInstence().GetDefaultGhostByGhostId(ghost.defaultId);

		SpriteRenderer renderer = EnemyImg.GetComponent<SpriteRenderer>();
		renderer.sprite = Resources.Load<Sprite>("GhostImg/Unit/" + defaultGhost.resourceURI);

		_SmokeStartPos = new Vector2(this.gameObject.transform.position.x + 0, this.gameObject.transform.position.y - 0);

	}

	public byte GetEnemyID() {
		return _EnemyId;
	}
	
	public void EnemySelectMark(bool isSelect) {
		SelectMark.renderer.enabled = isSelect;
	}

	public void SetCallBack(BtnCallBack onCallBack) {
		_Callback = new BtnCallBack(onCallBack);

	}

	public void SetHitAni(byte weaponType) {
		GameObject hitEffect;
		if(weaponType == GearType.Weapon_Gun) {
			hitEffect = Instantiate(Resources.Load<GameObject>("BattleScene/GunHitFX")) as GameObject;
			hitEffect.GetComponent<GunHitEffect>().StartFx("UI");
			if(UserData.getInstence().Option_Sound) GunHitAudio.Play();
			this.GetComponent<Animator>().Play("GunHit");
		} else if(weaponType == GearType.Weapon_Rocket) {
			hitEffect = Instantiate(Resources.Load<GameObject>("BattleScene/ExplosionFX")) as GameObject;
			hitEffect.GetComponent<ExplosionFX>().StartFx(false);
			if(UserData.getInstence().Option_Sound) ExplosionAudio.Play();
			this.GetComponent<Animator>().Play("RocketHit");
		} else {
			hitEffect = Instantiate(Resources.Load<GameObject>("BattleScene/ExplostionEffect")) as GameObject;
			hitEffect.GetComponent<ExplosionFX>().StartFx(false);
			if(UserData.getInstence().Option_Sound) ExplosionAudio.Play();
			this.GetComponent<Animator>().Play("RocketHit");
		}


	}

	public void SetFireAni() {
		this.GetComponent<Animator>().Play("Short");
		if(UserData.getInstence().Option_Sound) LaunchAudio.Play();
		GameObject hitEffect;
		hitEffect = Instantiate(Resources.Load<GameObject>("UnitResource/ExplostionEffect")) as GameObject;
		hitEffect.GetComponent<ExplosionEffect>().CreateSmoke(_SmokeStartPos , true);
	}

	public void SetUnitOut() {
		iTween.MoveTo(this.gameObject, iTween.Hash("delay", 0.3f,"oncomplete", "UnitOut", "oncompletetarget", this.gameObject));

	}

	private void UnitOut() {
		SetHitAni(GearType.Weapon_Rocket);
		if(UserData.getInstence().Option_Sound) ExplosionAudio.Play();
		iTween.MoveTo (this.gameObject, iTween.Hash("x", 4f, "y", -20f, "easetype", iTween.EaseType.easeInSine
		                                            , "oncomplete", "EndUnitOut", "oncompletetarget", this.gameObject
		                                            , "oncompleteparams", this.gameObject));
	}

	private void EndUnitOut(GameObject obj) {
		SetDamageSmoke(false);
	}

	public void SetDamageSmoke(bool type) {
		/*
		if(type) {
			if(_SmokeObject == null) {
				_SmokeObject = Instantiate(Resources.Load<GameObject>("UnitResource/DamageSmoke")) as GameObject;
				_SmokeObject.GetComponent<DamageSmoke>().SetSmoke(this.gameObject, true, Color.gray);
			}
		} else {
			if(_SmokeObject != null) {
				_SmokeObject.GetComponent<DamageSmoke>().DestroySmoke();
				//Destroy(_SmokeObject);
			}
		}
		*/
	}

	void OnMouseUp() {
		if(_Callback != null) _Callback(_EnemyId);
	}

	void OnGUI () {
		if(IsTest) {
			if(GUI.Button(new Rect(10, 10, 140, 30), "SetFireAni")) {
				SetFireAni();
			}
			if(GUI.Button(new Rect(10, 40, 100, 30), "hit")) {
				SetHitAni(GearType.Weapon_Gun);
			}
			if(GUI.Button(new Rect(10, 70, 100, 30), "hit2")) {
				SetHitAni(GearType.Weapon_Rocket);
			}
		}
	}
}
