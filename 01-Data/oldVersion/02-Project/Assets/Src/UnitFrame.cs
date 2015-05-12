using UnityEngine;
using System.Collections;

public class UnitFrame : MonoBehaviour {

	public bool isTest;
	public GameObject UnitFrameObj;
	public GameObject AirFrame;
	public GameObject SuitObj;
	public GameObject Engine;
	public GameObject Weapon;

	public AudioSource ExplosionAudio;

	private short _MemberId;
	private int _SortNum;

	private Animator _Animator;

	private Transform AirFrameFront;
	private Transform AirFrameBack;
	private Transform AirFrameWing;
	private Transform EngineFrame;

	private WeaponController Weapon1;
	private WeaponController Weapon2;
	private WeaponController Weapon3;
	private WeaponController Weapon4;

	private GameObject Weapon1Obj;
	private GameObject Weapon2Obj;
	private GameObject Weapon3Obj;
	private GameObject Weapon4Obj;

	// 바디 스케일 
	private float[] _BodyScale = new float[3]{1f, 0.9f, 1.1f};
	// 머리 포지션.
	private Vector2[] _HeadPos = new Vector2[3]{new Vector2(-0.25f, 1.13f), new Vector2(-0.25f, 1.07f), new Vector2(-0.25f, 1.17f)};

	private GameObject _SmokeObject;

	// 메모리 관리.
	private bool _IsMemorySave;			// 메모리를 세이브 할지 유무 결정.

	void Awake () {
		_Animator = UnitFrameObj.GetComponent<Animator>();

	}

	void Start() {
		if(isTest) {

			StartCoroutine(TestInit());
		}
	}

	private IEnumerator TestInit() {
		yield return new WaitForEndOfFrame();
		
		yield return StartCoroutine(MissionData.getInstence().init());
		
		LocalData.getInstence().AllLoad();
		DarkSprite.getInstence();
		SystemData.GetInstance();
		MemberData.getInstence();
		UserData.getInstence();
		ArrayList testlist = UserData.getInstence().UserMemberList;
		
		Member member = testlist[1] as Member;
		member.SuitGearId = 32;
		member.BodyGearId = 33;
		member.EngineGearId = 34;
		member.Weapon1GearId = 35;
		//member.Weapon2GearId = 13;
		//member.Weapon3GearId = 14;
		member.Weapon2GearId = 36;
		member.Weapon3GearId = 36;
		StartCoroutine(init(member.id, 1));
	}

	public IEnumerator init(short memberId, int sortNum) {

		yield return new WaitForEndOfFrame();

		_SortNum = sortNum;

		AirFrameFront = AirFrame.GetComponent<Transform>().FindChild("front") as Transform;
		AirFrameBack = AirFrame.GetComponent<Transform>().FindChild("back") as Transform;
		AirFrameWing = AirFrame.GetComponent<Transform>().FindChild("wing") as Transform;

		EngineFrame = Engine.GetComponent<Transform>();

		yield return StartCoroutine(UpdateMemberData(memberId));

	}

	/** 바디 정보를 저장할 설정. */
	public void SetBodyMemorySave(bool isSave) {
		_IsMemorySave = isSave;
	}

	public short GetMemberModelId() {
		return _MemberId;
	}

	public void SetSorting(int num) {

		_SortNum = num;

		AirFrameWing.renderer.sortingOrder = _SortNum + 11;

		if(Weapon3 != null) Weapon3.SetSorting(_SortNum + 10);

		Weapon1.SetSorting(_SortNum + 9);

		AirFrameFront.renderer.sortingOrder = _SortNum + 8;

		// 바디 (7 , -3)

		AirFrameBack.renderer.sortingOrder = _SortNum - 5;

		EngineFrame.renderer.sortingOrder = _SortNum - 6;

		Weapon2.SetSorting(_SortNum - 6);

		if(Weapon4 != null) Weapon4.SetSorting(_SortNum - 7);

	}

	public IEnumerator UpdateMemberData(short memberId) {

		yield return new WaitForEndOfFrame();

		_MemberId = memberId;

		UnitDestroy();

		Member member = UserData.getInstence().GetMemberById(_MemberId);
		if(member == null) {
			yield break;
		}

		Gear suitGear = GearData.getInstence().GetGearByID(member.SuitGearId);
		Gear bodyGear = GearData.getInstence().GetGearByID(member.BodyGearId);
		Gear engineGear = GearData.getInstence().GetGearByID(member.EngineGearId);
		Gear weapon1Gear = GearData.getInstence().GetGearByID(member.Weapon1GearId);
		Gear weapon2Gear = GearData.getInstence().GetGearByID(member.Weapon2GearId);
		Gear weapon3Gear = GearData.getInstence().GetGearByID(member.Weapon3GearId);

		SetSpriteChange(AirFrameBack, "airframe/" + bodyGear.resourceURI, "back");
		SetSpriteChange(AirFrameFront, "airframe/" + bodyGear.resourceURI, "front");
		SetSpriteChange(AirFrameWing, "airframe/" + bodyGear.resourceURI, "wing");

		SetSpriteChange(EngineFrame, "Engine/" + engineGear.resourceURI, "Engine");

		Weapon1Obj = SetWeapon("UnitResource/Weapon/" + weapon1Gear.resourceURI, new Vector2(0.5f, 0.5f), new Vector2(-1.84f, -1.6f), _SortNum + 16, weapon1Gear.gearType, true);
		Weapon1 = Weapon1Obj.GetComponent<WeaponController>();
		Weapon2Obj = SetWeapon("UnitResource/Weapon/" + weapon1Gear.resourceURI, new Vector2(0.5f, 0.5f), new Vector2(0.88f, -1.1f), _SortNum + 2, weapon1Gear.gearType, false);
		Weapon2 = Weapon2Obj.GetComponent<WeaponController>();
		if(weapon2Gear != null) {
			Weapon3Obj = SetWeapon("UnitResource/Weapon/" + weapon2Gear.resourceURI, new Vector2(0.5f, 0.5f), new Vector2(-3.07f, -0.72f), _SortNum + 17, weapon2Gear.gearType, true);
			Weapon3 = Weapon3Obj.GetComponent<WeaponController>();
		}
		if(weapon3Gear != null) {
			Weapon4Obj = SetWeapon("UnitResource/Weapon/" + weapon3Gear.resourceURI, new Vector2(0.5f, 0.5f), new Vector2(0.53f, -0.03f), _SortNum + 1, weapon3Gear.gearType, true);
			Weapon4 = Weapon4Obj.GetComponent<WeaponController>();
		}

		yield return StartCoroutine(SetBodyTexture(member.id));

		SetSorting(_SortNum);

	}

	private IEnumerator SetBodyTexture(short memberId) {
		yield return new WaitForEndOfFrame();

		yield return StartCoroutine(UserData.getInstence().SetUnitBodyData(memberId, (short)(_SortNum), _IsMemorySave));

		GameObject UnitBody = UserData.getInstence().GetUnitBody();
		UnitBody.transform.parent = SuitObj.transform;
		UnitBody.transform.position = UnitFrameObj.transform.position;
		UnitBody.transform.localScale = new Vector2(0.5f, 0.5f);
	}

	private void SpriteColorChange(GameObject sprite, Color color) {
		SpriteRenderer renderer = sprite.GetComponent<SpriteRenderer>();
		if(color.a == 0) color.a = 1;
		renderer.color = color;
	}

	private GameObject SetWeapon(string Uri, Vector2 scale, Vector2 position, int sortNum, byte gearType, bool isSound) {
		GameObject WeaponObj = Instantiate(Resources.Load<GameObject>(Uri)) as GameObject;
		WeaponController weaponController = WeaponObj.GetComponent<WeaponController>();
		weaponController.init(sortNum, gearType, isSound);
		//weaponController.init(sortNum, GearType.Weapon_Rocket, isSound);
		WeaponObj.transform.parent = Weapon.transform;

		Vector2 weaponPos = new Vector2(Weapon.transform.position.x + position.x, Weapon.transform.position.y + position.y);
		WeaponObj.transform.position = weaponPos;
		WeaponObj.transform.localScale = scale;

		return WeaponObj;
	}

	private void SetSpriteChange(Transform targetObj, string uri, string targeturi) {
		SpriteRenderer renderer = (SpriteRenderer)targetObj.GetComponent ("SpriteRenderer");
		renderer.sprite = Resources.Load<Sprite>("UnitResource/" + uri + "/" + targeturi);
	}

	public void SetFrontRun() {
		_Animator.Play("frontRun");
	}

	public void SetFrontRunEnd() {
		_Animator.Play("frontRunEnd");
	}

	public void SetIdleAni() {
		_Animator.Play("idle");
	}

	public void SetIntroAni() {
		_Animator.Play("Intro");
	}

	public void SetHitAni() {
		GameObject hitEffect = Instantiate(Resources.Load<GameObject>("BattleScene/ExplosionFX")) as GameObject;
		hitEffect.GetComponent<ExplosionFX>().StartFx(true);
		_Animator.Play("Hit");
		//hitEffect.transform.position = this.gameObject.transform.position;
		/*
		hitEffect.GetComponent<ExplosionEffect>().CreateSmoke(new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y)
		                                                      , false);
		*/
		//AudioData.getInstence().ExplosionPlay(1);
		if(UserData.getInstence().Option_Sound) ExplosionAudio.Play();
	}

	public void SetGunFire(byte slotN) {
		if(slotN == 1) {
			Weapon3.FireAni();
		} else if (slotN == 2) {
			Weapon4.FireAni();
		} else {
			Weapon1.FireAni();
			Weapon2.FireAni();
		}

	}

	public void SetUnitOut() {
		iTween.MoveTo(this.gameObject, iTween.Hash("delay", 0.45f,"oncomplete", "UnitOut", "oncompletetarget", this.gameObject));

	}

	public void SetDamageSmoke(bool type) {
		/*
		if(type) {
			if(_SmokeObject == null) {
				_SmokeObject = Instantiate(Resources.Load<GameObject>("UnitResource/DamageSmoke")) as GameObject;
				_SmokeObject.GetComponent<DamageSmoke>().SetSmoke(this.gameObject, false, Color.gray);
			}
		} else {
			if(_SmokeObject != null) {
				_SmokeObject.GetComponent<DamageSmoke>().DestroySmoke();
				Destroy(_SmokeObject);
			}
		}
		*/
	}

	private void UnitOut() {

		_Animator.Play("Hit");
		//AudioData.getInstence().ExplosionPlay(1);
		if(UserData.getInstence().Option_Sound) ExplosionAudio.Play();
		GameObject hitEffect = Instantiate(Resources.Load<GameObject>("UnitResource/ExplostionEffect")) as GameObject;
		hitEffect.GetComponent<ExplosionEffect>().CreateSmoke(new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y)
		                                                      , false);

		iTween.MoveTo (this.gameObject, iTween.Hash("x", -4f, "y", -20f, "easetype", iTween.EaseType.easeInSine));
	}

	public void SetSelectMark(bool state) {

	}

	public void UnitDestroy() {
		Destroy(Weapon1Obj);
		Destroy(Weapon2Obj);
		Destroy(Weapon3Obj);
		Destroy(Weapon4Obj);
		Destroy(Weapon1);
		Destroy(Weapon2);
		Destroy(Weapon3);
		Destroy(Weapon4);
	}
		 

	void OnGUI() {
		if(isTest == true && UserData.getInstence().IsGUIShow) {
			if (GUI.Button(new Rect(10, 10, 80, 30), "hit"))
				SetHitAni();

			if (GUI.Button(new Rect(10, 50, 80, 30), "move "))
				_Animator.Play("frontRunEnd");

			if (GUI.Button(new Rect(10, 90, 80, 30), "hit and out")) {
				SetUnitOut();
			}

			if (GUI.Button(new Rect(10, 120, 80, 30), "gun0")) {
				SetGunFire(0);
			}
			if (GUI.Button(new Rect(10, 150, 80, 30), "gun1")) {
				SetGunFire(1);
			}
			if (GUI.Button(new Rect(10, 180, 80, 30), "gun2")) {
				SetGunFire(2);
			}

			if (GUI.Button(new Rect(10, 220, 80, 30), "smoke")) {
				SetDamageSmoke(true);
			}

			if (GUI.Button(new Rect(10, 250, 80, 30), "smoke!")) {
				SetDamageSmoke(false);
			}

			if (GUI.Button(new Rect(10, 280, 80, 30), "save!")) {
				LocalData.getInstence().UserMemberDataSave();
			}
		}
	}
}

