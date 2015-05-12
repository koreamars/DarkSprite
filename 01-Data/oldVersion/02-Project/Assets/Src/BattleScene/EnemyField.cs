using UnityEngine;
using System.Collections;

public class EnemyField : MonoBehaviour {

	public GameObject BgObject;
	public Camera EnemyCamera;
	public GameObject DamageTxt;
	public bool isTest;

	private ArrayList _EnemyList;
	private Vector2[] _EnemyPosition;
	private Vector2[] _EnemyScale;

	private float _ScreenY = 13f;

	private byte _CurrentEnemyIndex = 0;

	public delegate void SelectCallBack(byte index);
	private SelectCallBack _SelectCallback;

	private WarzoneData _WarzoneData;

	void Awake () {

		_WarzoneData = WarzoneData.getInstence();

		_EnemyPosition = new Vector2[]{new Vector2(3.5f, -1.2f)
			, new Vector2(6.4f, 0.5f)
			, new Vector2(3.4f, 1.5f)
			, new Vector2(6f, 3f)
			, new Vector2(3.5f, 4f)};
		_EnemyScale = new Vector2[]{new Vector2(0.45f, 0.45f)
			, new Vector2(0.40f, 0.40f)
			, new Vector2(0.35f, 0.35f)
			, new Vector2(0.30f, 0.30f)
			, new Vector2(0.25f, 0.25f)};


		_EnemyPosition = new Vector2[]{ new Vector2(3.4f, 1.5f)
			, new Vector2(6.4f, 0.5f)
			, new Vector2(6f, 3f)
			, new Vector2(3.5f, -1.2f)
			, new Vector2(3.5f, 4f)};
		_EnemyScale = new Vector2[]{ new Vector2(0.35f, 0.35f)
			, new Vector2(0.40f, 0.40f)
			, new Vector2(0.30f, 0.30f)
			, new Vector2(0.45f, 0.45f)
			, new Vector2(0.25f, 0.25f)};

		DamageTxt.renderer.sortingOrder = 10;

		if(isTest) {

			ArrayList dataList = WarzoneData.getInstence().GetGhostDataByGhostClose(1000);

			init(dataList, null);
		}


	}

	public void init(ArrayList enemyList, SelectCallBack OnSelectCallback) {

		_SelectCallback = new SelectCallBack(OnSelectCallback);

		GameObject enemyObj;
		
		_EnemyList = new ArrayList();
		
		for (short i = 0; i < enemyList.Count; i++) {
			Ghost ghost = _WarzoneData.GetGhostByGhostId((byte)(enemyList[i]));
			DefaultGhost defaultGhost = _WarzoneData.GetDefaultGhostByGhostId(ghost.defaultId);
			enemyObj = Instantiate(Resources.Load<GameObject>("UnitResource/Enemy")) as GameObject;
			enemyObj.transform.position = new Vector2(_EnemyPosition[i].x, (_EnemyPosition[i].y - _ScreenY));
			enemyObj.transform.localScale = _EnemyScale[i];
			enemyObj.GetComponent<EnemyFrame>().SetEnemyId((byte)(ghost.id));
			enemyObj.GetComponent<EnemyFrame>().SetCallBack(EnemyClick);
			_EnemyList.Add(enemyObj);
			
		}
	}

	public void SelectEnemyByModelId(short ghostId) {
		if(_CurrentEnemyIndex > 0)
		{
			GameObject prevEnemyObj = _EnemyList[_CurrentEnemyIndex - 1] as GameObject;
			prevEnemyObj.GetComponent<EnemyFrame>().EnemySelectMark(false);
		}

		byte index = 1;
		foreach(GameObject enemyObj in _EnemyList) {
			if(enemyObj.GetComponent<EnemyFrame>().GetEnemyID() == ghostId) break;
			index ++;
		}

		_CurrentEnemyIndex = index;
		GameObject currentEnemyObj = _EnemyList[_CurrentEnemyIndex - 1] as GameObject;
		currentEnemyObj.GetComponent<EnemyFrame>().EnemySelectMark(true);

		if(_SelectCallback != null) _SelectCallback(currentEnemyObj.GetComponent<EnemyFrame>().GetEnemyID());

	}

	public void SetEnemyHitAni(byte type, short damage) {

		DamageTxt.renderer.enabled = true;
		GameObject enemyObj = _EnemyList[_CurrentEnemyIndex - 1] as GameObject;
		enemyObj.GetComponent<EnemyFrame>().SetHitAni(type);

		Ghost ghost = _WarzoneData.GetGhostByGhostId(enemyObj.GetComponent<EnemyFrame>().GetEnemyID());

		if(ghost.currentHP < (ghost.maxHP / 3f)) {
			enemyObj.GetComponent<EnemyFrame>().SetDamageSmoke(true);
		}

		DamageTxt.GetComponent<TextMesh>().text = "-" + damage;
		DamageTxt.transform.position = enemyObj.transform.position;

		iTween.ColorUpdate(DamageTxt, iTween.Hash("a", 1f, "time", 0f));
		iTween.MoveTo(DamageTxt, iTween.Hash("y", DamageTxt.transform.position.y + 1f));
		iTween.ColorTo(DamageTxt, iTween.Hash("a", 0f, "oncomplete", "DamageTxtMoveEnd", "oncompletetarget", this.gameObject));
	}

	public void SetEnemyFireAni() {
		GameObject currentEnemyObj = _EnemyList[_CurrentEnemyIndex - 1] as GameObject;
		currentEnemyObj.GetComponent<EnemyFrame>().SetFireAni();
	}

	public void SetEnemyOut(short ghostId) {
		foreach(GameObject enemyObj in _EnemyList) {
			if(enemyObj.GetComponent<EnemyFrame>().GetEnemyID() == ghostId) {
				enemyObj.GetComponent<EnemyFrame>().SetUnitOut();
				break;
			}
		}
	}

	private void DamageTxtMoveEnd() {
		//iTween.ColorUpdate(DamageTxt, iTween.Hash("a", 1f, "time", 0f));
	}
		 

	private void EnemyClick(byte id) {

		if(_CurrentEnemyIndex > 0)
		{
			GameObject prevEnemyObj = _EnemyList[_CurrentEnemyIndex - 1] as GameObject;
			prevEnemyObj.GetComponent<EnemyFrame>().EnemySelectMark(false);
		}

		byte index = 1;
		foreach(GameObject enemyObj in _EnemyList) {
			if(enemyObj.GetComponent<EnemyFrame>().GetEnemyID() == id) {
				_CurrentEnemyIndex = index;
				enemyObj.GetComponent<EnemyFrame>().EnemySelectMark(true);
				break;
			}

			index ++;
		}

		if(_SelectCallback != null) _SelectCallback(id);
	}
}
