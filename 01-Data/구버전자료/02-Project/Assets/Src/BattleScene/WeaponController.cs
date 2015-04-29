using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour {

	public bool isTest;
	public GameObject WeaponImg;
	public GameObject EffectImg;

	private Animator _Animator;
	private byte _GearType = GearType.Weapon_Gun;
	private bool _isSound;

	public void init(int sortNum, byte weaponType, bool isSound) {
		_isSound = isSound;
		_GearType = weaponType;
		SetSorting(sortNum);
	}

	public void FireAni() {
		//GameObject hitEffect = null;
		_Animator = this.GetComponent<Animator>();
		switch(_GearType) {
		case GearType.Weapon_Gun:
			_Animator.Play("Fire");
			break;
		case GearType.Weapon_Rocket:
			_Animator.Play("RocketFire");
			//hitEffect = Instantiate(Resources.Load<GameObject>("UnitResource/ExplostionEffect")) as GameObject;

			break;
		case GearType.Weapon_Missle:
			_Animator.Play("RocketFire");
			//hitEffect = Instantiate(Resources.Load<GameObject>("UnitResource/ExplostionEffect")) as GameObject;

			break;
		}

	}

	public void SetSorting(int num) {
		WeaponImg.renderer.sortingOrder = num;
		EffectImg.renderer.sortingOrder = num + 1;
	}

	public void Fire() {
		//print("Fire : " + _isSound);
		if(UserData.getInstence().Option_Sound == true && _isSound) {
			print("play audio.");
			audio.Play();
		}
	}

	void OnGUI() {
		if(isTest) {
			if(GUI.Button(new Rect(0, 0, 100, 30), "test")) {
				_isSound = true;
				_GearType = GearType.Weapon_Gun;
				FireAni();
			}
		}
	}
}
