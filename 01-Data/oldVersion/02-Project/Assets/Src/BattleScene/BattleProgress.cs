using UnityEngine;
using System.Collections;
using System.Linq;

public class BattleProgress : MonoBehaviour {

	public Camera MainCamera;

	public AudioSource ExplosionAudio;
	public AudioSource GunHitAudio;

	public bool IsTest;

	private UnitDataBoxModel[] _SlotModelList;
	private GameObject[] _SlotObjectList;

	private float _DefaultLeftX;
	private float _DefaultRightX;
	private float _SlotGap = 1.6f;

	public delegate void SlotObjectClick(byte id);
	private SlotObjectClick _SlotObjectClick;

	public delegate void SlotOpeningFunc();
	private SlotOpeningFunc _SlotOpeningFunc;

	private GameObject _HitSlotObj;
	private float _hitMovePoint;

	void Awake() {

		_DefaultLeftX = SystemData.GetInstance().screenLeftX + 1f;
		_DefaultRightX = SystemData.GetInstance().screenRightX - 1f;

		if(IsTest) {

			UnitDataBoxModel[] sampleData = new UnitDataBoxModel[5];

			for(byte i =0; i<5; i++) {
				sampleData[i] = new UnitDataBoxModel();
				sampleData[i].id = i;
				sampleData[i].currentHP = 10;
				sampleData[i].currentMP = 10;
				sampleData[i].maxHP = 20;
				sampleData[i].maxMP = 10;
				sampleData[i].imgName = "Member00" + (i + 1);
				//sampleData[i].ActNum = System.Convert.ToInt32(UnityEngine.Random.Range(1, 10));
				sampleData[i].ActNum = System.Convert.ToInt32(UnityEngine.Random.Range(1, 10));
				if(i == 0) sampleData[i].isSelect = true;
				if(i == 4) sampleData[i].isOut = true;

			}

			SetProgressSlot(sampleData);
		}

	}

	public void SetSlotClickCallback(SlotObjectClick OnSlotObjectClick) {
		_SlotObjectClick = new SlotObjectClick(OnSlotObjectClick);
	}

	public void SetProgressSlot(UnitDataBoxModel[] data) {

		_SlotModelList = data;

		GameObject slot;
		byte index = 0;
		byte myIndex = 0;
		byte enemyIndex = 0;

		if(_SlotObjectList == null) {
			_SlotObjectList = new GameObject[_SlotModelList.Length];

			foreach(UnitDataBoxModel dataBoxModel in _SlotModelList) {
				slot = Instantiate(Resources.Load<GameObject>("BattleScene/UnitDataBox")) as GameObject;
				slot.transform.parent = this.transform;
				dataBoxModel.turnIndex = index;
				//slot.transform.position = new Vector2(_DefaultLeftX + (index * _SlotGap), -6f);
				if(dataBoxModel.type == 0) {
					slot.transform.position = new Vector2(_DefaultLeftX - 2f, 3.3f - (_SlotGap * myIndex));
					myIndex ++;
				} else {
					slot.transform.position = new Vector2(_DefaultRightX + 2f, 3.3f - (_SlotGap * enemyIndex));
					enemyIndex ++;
				}
				slot.GetComponent<UnitDataBox>().id = dataBoxModel.id;
				slot.GetComponent<UnitDataBox>().init(dataBoxModel, 20, OnSlotClick);
				_SlotObjectList[index] = slot;
				index++;
			}
		} else {
			float selectGap = 0f;
			foreach(UnitDataBoxModel dataBoxModel in _SlotModelList) {

				dataBoxModel.turnIndex = index;

				GameObject targetObj = GetSlotObjByID(dataBoxModel.id);
				targetObj.GetComponent<UnitDataBox>().update(dataBoxModel);
				selectGap = 0.1f * index;
				if(dataBoxModel.isSelect == true) selectGap = selectGap - 0.5f;
				if(dataBoxModel.currentHP == 0) selectGap = 0.1f * _SlotModelList.Length;
				if(dataBoxModel.type == 0) {
					iTween.MoveTo(targetObj, iTween.Hash("x", _DefaultLeftX + 1f - selectGap, "speed", 1f));
				} else {
					iTween.MoveTo(targetObj, iTween.Hash("x", _DefaultRightX - 1f + selectGap, "speed", 1f));
				}

				index++;
			}
		}
	}

	public void SetSlotOpeningFunc(SlotOpeningFunc OnSlotOpeningFunc) {
		_SlotOpeningFunc = new SlotOpeningFunc(OnSlotOpeningFunc);
		SlotOpen();
	}

	private void SlotOpen() {
		byte index = 0;
		foreach(GameObject slotObj in _SlotObjectList) {
			if(index == 0) {
				if(slotObj.GetComponent<UnitDataBox>().Model.type == 0) {
					iTween.MoveTo(slotObj, iTween.Hash("x", _DefaultLeftX, "speed", 2f, "oncomplete", "SlotEndOpening", "oncompletetarget", this.gameObject));
				} else {
					iTween.MoveTo(slotObj, iTween.Hash("x", _DefaultRightX, "speed", 2f, "oncomplete", "SlotEndOpening", "oncompletetarget", this.gameObject));
				}
			} else {
				if(slotObj.GetComponent<UnitDataBox>().Model.type == 0) {
					iTween.MoveTo(slotObj, iTween.Hash("x", _DefaultLeftX, "speed", 2f));
				} else {
					iTween.MoveTo(slotObj, iTween.Hash("x", _DefaultRightX, "speed", 2f));
				}
			}

			index ++;
		}
	}

	private void SlotEndOpening() {
		if(_SlotOpeningFunc != null) _SlotOpeningFunc();
	}

	public GameObject GetSlotObjByID(byte boxModelId) {
		foreach(GameObject slotObj in _SlotObjectList) {
			if(slotObj.GetComponent<UnitDataBox>().id == boxModelId) return slotObj;
		}
		return null;
	}

	public void SetSlotHitAni(byte boxModelId, short damage, byte type, short damageIA) {
		foreach(GameObject slotObj in _SlotObjectList) {
			if(slotObj.GetComponent<UnitDataBox>().id == boxModelId) {
				_HitSlotObj = slotObj;
				break;
			}
		}
		GameObject thumbNailObj = _HitSlotObj.transform.FindChild("Member001").gameObject;
		GameObject SlotBg = _HitSlotObj.transform.FindChild("ProgressSlot").gameObject;
		iTween.ColorTo(thumbNailObj, iTween.Hash("r", 10f, "g", 1f, "b", 1f, "time", 0.2f 
		                                        , "oncomplete", "SlotHitAniEnd", "oncompletetarget", this.gameObject));
		iTween.ColorTo(SlotBg, iTween.Hash("r", 10f, "g", 1f, "b", 1f, "time", 0.2f));


		GameObject DamageTxt = Instantiate(Resources.Load<GameObject>("BattleScene/DamageText")) as GameObject;
		DamageTxt.transform.position = _HitSlotObj.transform.position;
		DamageTxt.GetComponent<DamageText>().init(damage, 55, "UI", Color.red, false);

		if(damageIA > 0) StartCoroutine(ShowDamageIA(damageIA));

		if(_HitSlotObj.GetComponent<UnitDataBox>().Model.type == 0) {
			_hitMovePoint = -0.15f;
		} else {
			_hitMovePoint = 0.15f;
		}

		GameObject hitEffect;
		if(type == GearType.Weapon_Missle || type == GearType.Weapon_Rocket || type == GearType.GhostWeapon) {
			if(UserData.getInstence().Option_Sound) ExplosionAudio.Play();
			hitEffect = Instantiate(Resources.Load<GameObject>("BattleScene/ExplosionFX")) as GameObject;
			hitEffect.GetComponent<ExplosionFX>().StartFx(false);
			hitEffect.transform.localScale = new Vector2(0.5f, 0.5f);
			hitEffect.transform.position = _HitSlotObj.transform.position;
			ExplosionMoving();
		} else {
			if(UserData.getInstence().Option_Sound) GunHitAudio.Play();
			hitEffect = Instantiate(Resources.Load<GameObject>("BattleScene/GunHitFX")) as GameObject;
			hitEffect.GetComponent<GunHitEffect>().StartFx("UI");
			hitEffect.transform.localScale = new Vector2(0.5f, 0.5f);
			hitEffect.transform.position = new Vector2(_HitSlotObj.transform.position.x, _HitSlotObj.transform.position.y - 0.5f);
			GunHitMoving();
		}

	}

	private IEnumerator ShowDamageIA(short damageIA) {
		yield return new WaitForSeconds(0.2f);
		GameObject DamageTxt = Instantiate(Resources.Load<GameObject>("BattleScene/DamageText")) as GameObject;
		DamageTxt.transform.position = _HitSlotObj.transform.position;
		DamageTxt.GetComponent<DamageText>().init(damageIA, 55, "UI", Color.green, true);
		yield return 0;
	}

	private void SlotHitAniEnd() {
		GameObject thumbNailObj = _HitSlotObj.transform.FindChild("Member001").gameObject;
		GameObject SlotBg = _HitSlotObj.transform.FindChild("ProgressSlot").gameObject;
		iTween.ColorTo(thumbNailObj, iTween.Hash("r", 1f, "g", 1f, "b", 1f, "time", 0.2f));
		iTween.ColorTo(SlotBg, iTween.Hash("r", 1f, "g", 1f, "b", 1f, "time", 0.2f));
	}

	// 슬롯 이펙트 무빙.
	private void GunHitMoving() {
		iTween.MoveFrom(_HitSlotObj, iTween.Hash("x", _HitSlotObj.transform.position.x + _hitMovePoint, "y", _HitSlotObj.transform.position.y, "time", 0.4f
		                                         , "easetype", iTween.EaseType.easeInElastic));
	}

	private void ExplosionMoving() {
		iTween.MoveFrom(_HitSlotObj, iTween.Hash("x", _HitSlotObj.transform.position.x + _hitMovePoint, "y", _HitSlotObj.transform.position.y, "time", 0.4f
		                                         , "easetype", iTween.EaseType.easeInBack));
	}


	void OnGUI() {
		if(IsTest) {
			if(GUI.Button(new Rect(10, 10, 70, 30), "update")) {
				//_SlotModelList[1].ActNum = _SlotModelList[1].ActNum - 1;
				_SlotModelList[0].ActNum = 20;
				//_SlotObjectList[1].GetComponent<UnitDataBox>().update(_SlotModelList[1]);

				SetProgressSlot(_SlotModelList);
			}
		}
	}

	public void OnSlotClick(byte id) {
		if(_SlotObjectClick != null) _SlotObjectClick(id);
	}

	public class myReverserClass : IComparer  {
		
		// Calls CaseInsensitiveComparer.Compare with the parameters reversed. 
		int IComparer.Compare( System.Object x, System.Object y )  {
			return( (new CaseInsensitiveComparer()).Compare( y, x ) );
		}
		
	}

}
