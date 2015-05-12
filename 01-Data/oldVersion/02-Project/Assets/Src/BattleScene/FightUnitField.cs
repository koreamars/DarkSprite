using UnityEngine;
using System.Collections;

public class FightUnitField : MonoBehaviour {

	public bool isTest;
	public GameObject BG;

	private GameObject _UserUnitObj;
	private GameObject _EnemyUnitObj;
	private UnitFrame _UnitFrame;
	private EnemyFrame _EnemyFrame;

	private bool _IsMyUnit;
	private bool _IsTurn;
	private short _MemberId;
	private short _EnemyId;
	private short _CurrentDamage;
	private byte _WeaponType;

	public delegate void OpeningCallback();
	private OpeningCallback _OpeningCallback;

	public delegate void AppearCompleteCallback();
	private AppearCompleteCallback _AppearCompleteCallback;

	public delegate void DisappearCompleteCallback();
	private DisappearCompleteCallback _DisappearCompleteCallback;

	private Vector2 _UnitOutPos = new Vector2(-12f, 6.5f);
	private Vector2 _UnitInPos = new Vector2(0f, 2.0f);
	private Vector2 _EnemyOutPos = new Vector2(14f, 8f);
	private Vector2 _EnemyInPos = new Vector2(0f, 2f);

	private GameObject _StoryViewWindow;

	private GameObject _perTxt;

	// Use this for initialization
	void Start () {

		if(isTest) {
			MemberData.getInstence();
			UserData.getInstence();
			GearData.getInstence();
			ScriptData.getInstence();
			UnitData.getInstence();
			LocalData.getInstence().AllLoad();
			WarzoneData.getInstence().GetGhostDataByGhostClose(200);

			short[] testArray = new short[5]{1, 0, 0, 0, 0};
			StartCoroutine(init(testArray));
		}
	}

	/** 초기화 */
	public IEnumerator init(short[] memberIds) {

		yield return new WaitForEndOfFrame();

		if(UserData.getInstence().UserBannerStandByCount <= 0) {
			_UnitInPos = new Vector2(0f, 1.6f);
		} else {
			_UnitInPos = new Vector2(0f, 2.0f);
		}

		byte memberCount = 0;
		byte loadMemberCount = 0;
		foreach(short memberId in memberIds) {
			if(memberId > 0) memberCount ++;
		}

		_EnemyUnitObj = Instantiate(Resources.Load<GameObject>("UnitResource/Enemy")) as GameObject;
		_EnemyFrame = _EnemyUnitObj.GetComponent<EnemyFrame>();
		_EnemyUnitObj.transform.position = _EnemyOutPos;


		_UserUnitObj = Instantiate(Resources.Load<GameObject>("UnitResource/UnitFrame")) as GameObject;
		_UserUnitObj.transform.position = _UnitOutPos;
		_UnitFrame = _UserUnitObj.GetComponent<UnitFrame>();
		_UnitFrame.SetBodyMemorySave(true);

		_perTxt = CustomTextMesh.SetAddTextMesh("0/" + memberCount, 12, TextAnchor.MiddleCenter, Color.white, 101, "Alert");
		_perTxt.transform.position = new Vector2(0f, -0.5f);
		yield return StartCoroutine(_UnitFrame.init((short)(memberIds[0]), 0));
		if(memberIds[0] > 0) loadMemberCount ++;
		_perTxt.GetComponent<TextMesh>().text = loadMemberCount + "/" + memberCount;
		yield return StartCoroutine(_UnitFrame.UpdateMemberData((short)(memberIds[1])));
		if(memberIds[1] > 0) loadMemberCount ++;
		_perTxt.GetComponent<TextMesh>().text = loadMemberCount + "/" + memberCount;
		yield return StartCoroutine(_UnitFrame.UpdateMemberData((short)(memberIds[2])));
		if(memberIds[2] > 0) loadMemberCount ++;
		_perTxt.GetComponent<TextMesh>().text = loadMemberCount + "/" + memberCount;
		yield return StartCoroutine(_UnitFrame.UpdateMemberData((short)(memberIds[3])));
		if(memberIds[3] > 0) loadMemberCount ++;
		_perTxt.GetComponent<TextMesh>().text = loadMemberCount + "/" + memberCount;
		yield return StartCoroutine(_UnitFrame.UpdateMemberData((short)(memberIds[4])));
		if(memberIds[4] > 0) loadMemberCount ++;
		_perTxt.GetComponent<TextMesh>().text = loadMemberCount + "/" + memberCount;
		Destroy(_perTxt);
	}

	public void SetOpening(short leaderId) {
		_MemberId = leaderId;

		_UserUnitObj.transform.position = new Vector2(12f, -10f);
		BG.transform.position = new Vector2(4f, 0.8f);

		StartCoroutine(LeaderOpening());
	}

	private IEnumerator LeaderOpening() {
		yield return new WaitForEndOfFrame();
		
		yield return StartCoroutine(_UnitFrame.UpdateMemberData(_MemberId));

		yield return StartCoroutine(StoryData.getInstence().CheckStoryModel(50001));

		_UserUnitObj.transform.localScale = new Vector2(1.6f, 1.6f);
		
		iTween.MoveTo(_UserUnitObj, iTween.Hash("x", 0f, "y", -1.5f, "time", 2.5f, "oncomplete", "OpeningStep1"
		                                        , "oncompletetarget", this.gameObject, "easetype", iTween.EaseType.easeOutBack));
		iTween.MoveTo(BG, iTween.Hash("x", 0f, "time", 7.5f, "easetype", iTween.EaseType.easeOutCirc));
	}

	private void OpeningStep1() {
		if(_StoryViewWindow == null) _StoryViewWindow = Instantiate(Resources.Load<GameObject>("Common/StoryView")) as GameObject;
		StoryView storyView = _StoryViewWindow.GetComponent<StoryView>();

		Member member = UserData.getInstence().GetMemberById(_MemberId);
		DefaultMember defaultMember = MemberData.getInstence().GetDefaultMemberByID(member.DefaultId);
		string thumbUri = defaultMember.thumbId;
		string nameStr = ScriptData.getInstence().GetMemberNameByMemberId(member.id);
		int mentRandomValue = UnityEngine.Random.Range(0, 3);

		StoryDataModel storyDataModel = StoryData.getInstence().GetStoryDataModelById(defaultMember.Id);

		string message = storyDataModel.Comment;

		storyView.ShowMessageWindow("MemberImg/Member" + thumbUri, nameStr, message, OnStoryViewClick);

	}

	private void OnStoryViewClick() {
		_StoryViewWindow.GetComponent<StoryView>().HideStoryWindow();
		Destroy(_StoryViewWindow);
		StartCoroutine(OpeningStep2());
	}

	private IEnumerator OpeningStep2() {
		yield return new WaitForSeconds(0.5f);

		iTween.MoveTo(_UserUnitObj, iTween.Hash("x", -16f, "y", 5.5f, "time", 2f, "oncomplete", "OpeningStep3"
		                                        , "oncompletetarget", this.gameObject, "easetype", iTween.EaseType.easeInBack));
	}

	private void OpeningStep3() {
		_UserUnitObj.transform.position = _UnitOutPos;
		_UserUnitObj.transform.localScale = new Vector2(0.5f, 0.5f);
		if(_OpeningCallback != null) _OpeningCallback();
	}

	public void SetAppearCompleteCallback(AppearCompleteCallback OnAppear) {
		_AppearCompleteCallback = new AppearCompleteCallback(OnAppear);
	}

	public void SetDisappearCompleteCallback(DisappearCompleteCallback OnDisappear) {
		_DisappearCompleteCallback = new DisappearCompleteCallback(OnDisappear);
	}

	public void SetOpeningCallback(OpeningCallback OnOpeningCallback) {
		_OpeningCallback = new OpeningCallback(OnOpeningCallback);
	}

	/** 아군 유닛 등장 연출 */
	public void SetSelectUnit(bool IsTurn, short memberId, short damage) {
		_UserUnitObj.transform.position = _UnitOutPos;
		//_UserUnitObj.transform.localScale = new Vector2(0.5f, 0.5f);
		_IsMyUnit = true;
		_IsTurn = IsTurn;
		_MemberId = memberId;
		_CurrentDamage = damage;
		StartCoroutine(AppearMyUnit());
	}

	/** 무기 발사 */
	public void UnitWeaponFire(byte weaponSlotNum) {
		if(_IsMyUnit) {
			_UnitFrame.SetGunFire(weaponSlotNum);
		} else {
			_EnemyFrame.SetFireAni();
		}
	}
	
	/** 적군 유닛 등장 연출 */
	public void SetSelectEnemy(bool IsTurn, short enemyId, short damage, byte weaponType) {
		_EnemyUnitObj.transform.position = _EnemyOutPos;
		//_UserUnitObj.transform.localScale = new Vector2(0.5f, 0.5f);
		_IsMyUnit = false;
		_IsTurn = IsTurn;
		_EnemyId = enemyId;
		_CurrentDamage = damage;
		_WeaponType = weaponType;

		_EnemyFrame.SetEnemyId((byte)(_EnemyId));

		AppearEnemy();
	}

	/** 유닛 퇴장 연출 */
	public void SetUnitOut(bool IsDestroy) {
		if(IsDestroy) {	// 파괴 효과.
			if(_IsMyUnit) {
				iTween.MoveTo(_UserUnitObj, iTween.Hash("x", -6f, "y", -8f,"time", 0.6f, "oncomplete", "UnitOutComplete"
				                                        , "oncompletetarget", this.gameObject, "easetype", iTween.EaseType.easeInBack));
			} else {
				_UnitFrame.SetHitAni();
				iTween.MoveTo(_EnemyUnitObj, iTween.Hash("delay", 0.3f, "x", _EnemyOutPos.x, "y", -8f,"time", 0.6f, "oncomplete", "UnitOutComplete"
				                                         , "oncompletetarget", this.gameObject, "easetype", iTween.EaseType.easeInQuad));
			}
		} else {
			if(_IsMyUnit) {
				iTween.MoveTo(_UserUnitObj, iTween.Hash("x", _UnitOutPos.x, "y", _UnitOutPos.y,"time", 0.6f, "oncomplete", "UnitOutComplete"
				                                        , "oncompletetarget", this.gameObject, "easetype", iTween.EaseType.easeInQuad));
			} else {
				iTween.MoveTo(_EnemyUnitObj, iTween.Hash("x", _EnemyOutPos.x, "y", _EnemyOutPos.y,"time", 0.6f, "oncomplete", "UnitOutComplete"
				                                        , "oncompletetarget", this.gameObject, "easetype", iTween.EaseType.easeInQuad));
			}
			//iTween.MoveTo(BG, iTween.Hash("x", 0f, "time", 0.6f, "easetype", iTween.EaseType.easeInOutSine));
		}
	}

	/** 아군 유닛 등장 */
	private IEnumerator AppearMyUnit() {
		yield return new WaitForEndOfFrame();

		yield return StartCoroutine(_UnitFrame.UpdateMemberData(_MemberId));

		iTween.MoveTo(BG, iTween.Hash("x", 3f, "time", 1.3f, "easetype", iTween.EaseType.easeInOutSine));
		iTween.MoveTo(_UserUnitObj, iTween.Hash("x", _UnitInPos.x, "y", _UnitInPos.y, "time", 1.2f, "oncomplete", "MyUnitAppearCompelete"
		                                        , "oncompletetarget", this.gameObject, "easetype", iTween.EaseType.easeOutBack));

	}

	/** 아군 유닛 등장 연출 완료 */
	private void MyUnitAppearCompelete() {
		if(_IsTurn) {
			if(_AppearCompleteCallback != null) _AppearCompleteCallback();
		} else {
			StartCoroutine(MyUnitDamageEffect());
		}
	}

	/** 아군 유닛 히트 연출 시작 */
	private IEnumerator MyUnitDamageEffect() {

		_UnitFrame.SetHitAni();
		StartCoroutine(SetDamageTxtShow());
		yield return new WaitForSeconds(1f);
		if(_AppearCompleteCallback != null) _AppearCompleteCallback();
	}

	/** 적군 유닛 등장 연출 시작 */
	private void AppearEnemy() {

		iTween.MoveTo(BG, iTween.Hash("x", -4f, "time", 1.3f, "easetype", iTween.EaseType.easeInOutSine));

		iTween.MoveTo(_EnemyUnitObj, iTween.Hash("x", _EnemyInPos.x, "y", _EnemyInPos.y, "time", 1.2f, "oncomplete", "EnemyAppearComplete"
		                                        , "oncompletetarget", this.gameObject, "easetype", iTween.EaseType.easeOutBack));

	}

	/** 적군 유닛 등장 연출 완료 */
	private void EnemyAppearComplete() {
		if(_IsTurn) {
			if(_AppearCompleteCallback != null) _AppearCompleteCallback();
		} else {
			StartCoroutine(EnemyHitAni());
		}

	}

	/** 적군 유닛 히트 연출 시작 */
	private IEnumerator EnemyHitAni() {
		_EnemyFrame.SetHitAni(_WeaponType);
		StartCoroutine(SetDamageTxtShow());
		yield return new WaitForSeconds(1f);
		if(_AppearCompleteCallback != null) _AppearCompleteCallback();
		_AppearCompleteCallback = null;
	}

	/** 유닛 퇴장 연출 완료 */
	private void UnitOutComplete() {
		if(_DisappearCompleteCallback != null) _DisappearCompleteCallback();
		_DisappearCompleteCallback = null;
	}

	private IEnumerator SetDamageTxtShow() {
		yield return new WaitForSeconds(0.3f);
		GameObject DamageTxt = Instantiate(Resources.Load<GameObject>("BattleScene/DamageText")) as GameObject;
		DamageTxt.GetComponent<DamageText>().init(_CurrentDamage, 55, "UI", Color.red, false);
	}


	void OnGUI() {
		if(isTest) {
			if(GUI.Button(new Rect(0, 40, 160, 30), "SetSelectUnit - open")) {
				SetSelectUnit(true, (short)(UnityEngine.Random.Range(1, 5)), 0);
			}

			if(GUI.Button(new Rect(0, 80, 160, 30), "SetSelectUnit - hit")) {
				SetSelectUnit(false, (short)(UnityEngine.Random.Range(1, 5)), 100);
			}

			if(GUI.Button(new Rect(0, 120, 160, 30), "UnitWeaponFire")) {
				UnitWeaponFire(0);
			}

			if(GUI.Button(new Rect(0, 160, 160, 30), "SetSelectEnemy - open")) {
				SetSelectEnemy(true, 1, 0, 0);
			}

			if(GUI.Button(new Rect(0, 200, 160, 30), "SetSelectEnemy - hit")) {
				SetSelectEnemy(false, 1, 100, 4);
			}

			if(GUI.Button(new Rect(0, 240, 160, 30), "SetSelectEnemy - roket hit")) {
				SetSelectEnemy(false, 1, 100, 5);
			}

			if(GUI.Button(new Rect(0, 280, 160, 30), "SetUnitOut false")) {
				SetUnitOut(false);
			}

			if(GUI.Button(new Rect(0, 320, 160, 30), "SetUnitOut true")) {
				SetUnitOut(true);
			}

			if(GUI.Button(new Rect(0, 360, 160, 30), "Opening")) {
				SetOpening(1);
			}
		}
	}

}
