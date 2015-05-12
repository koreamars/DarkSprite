using UnityEngine;
using System.Collections;


public class MainMap : MonoBehaviour {

	private float _ScaleTime = 0.5f;
	private float _DefaultScale = 1f;
	private float _ZoomInScale = 1.2f;
	private float _defaultSBScale = 0.7f;

	private GameObject _MainBg;
	//private GameObject _BaseCampObj;
	private GameObject _MapStage;

	private GameObject[] _SBList;

	private byte _CurrentTownId = 0;

	private float _Timer;

	// Use this for initialization
	void Awake() {
		GameLog.Log("MapController");

	}

	void Update() {

		_Timer += Time.deltaTime;
		bool symbolU = false;
		if(_Timer > 1) {
			symbolU = true;
			_Timer = 0;
		}


		if(_SBList != null) {
			foreach(GameObject townSB in _SBList) {
				if(townSB == null) continue;
				Town town = WarzoneData.getInstence().GetDefaultTownData(townSB.GetComponent<MapSymbol>().TownId);
				GhostTown ghostTown = WarzoneData.getInstence().GetGhostTownByTownId(town.id);
				if(symbolU) townSB.GetComponent<MapSymbol>().SetTownSymbol(townSB.GetComponent<MapSymbol>().TownId);
				Linedrow(townSB, town, ghostTown);
			}
		}
	}

	// Use this for initialization
	void Start () {
		GameLog.Log("MapController.Start");

		_MapStage = Instantiate(Resources.Load<GameObject>("MainScene/MapStage")) as GameObject;

		_MainBg = Instantiate(Resources.Load<GameObject>("MainScene/BgMap")) as GameObject;
		_MainBg.transform.parent = _MapStage.transform;

		/*
		_BaseCampObj = Instantiate(Resources.Load<GameObject>("MainScene/MapSymbol/BaseCampPrefab")) as GameObject;
		_BaseCampObj.transform.localScale = new Vector2(_defaultSBScale, _defaultSBScale);
		_BaseCampObj.transform.parent = _MapStage.transform;
		_BaseCampObj.renderer.sortingOrder = -5;
		*/

		_MapStage.transform.localScale = new Vector2(_DefaultScale, _DefaultScale);

		deleteTown();
		CreateTown();
	}

	private void CreateTown() {
		Town[] townList = WarzoneData.getInstence().GetDefaultTownList();
		_SBList = new GameObject[townList.Length];
		byte index = 0;
		foreach(Town town in townList) {

			UserTown userTown = WarzoneData.getInstence().GetUserTownByID(town.id);
			GhostTown ghostTown = null;
			if(userTown == null) {
				ghostTown = WarzoneData.getInstence().GetGhostTownByTownId(town.id);
			}

			if(ghostTown != null && ghostTown.isView == false) continue;

			GameObject townSB = Instantiate(Resources.Load<GameObject>("MainScene/MapSymbol"), new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
			townSB.name = "town" + town.id;
			_SBList[index] = townSB;
			townSB.GetComponent<MapSymbol>().init(town.id, 1);

			townSB.transform.parent = _MapStage.transform;
			townSB.transform.localScale = new Vector2(_defaultSBScale, _defaultSBScale);
			townSB.transform.position = new Vector3(town.position.x, town.position.y, 0f);

			Linedrow(townSB, town, ghostTown);

			index ++;
		}
	}

	private void Linedrow(GameObject townSB, Town town, GhostTown ghostTown) {

		Town prevTown;
		if(town.root != town.id) {
			prevTown = WarzoneData.getInstence().GetDefaultTownData(town.root);
			if(prevTown == null) return;
			LineRenderer townSBLineRenderer = townSB.GetComponent<LineRenderer>();
			townSBLineRenderer.SetVertexCount(2);
			townSBLineRenderer.SetWidth(0.05f, 0.05f);

			Color lineColor = Color.white;
			if(ghostTown != null) {

				lineColor.r = 1f;
				lineColor.g = 0.5f;
				lineColor.b = 0f;

				townSBLineRenderer.SetColors(lineColor, lineColor);
			} else {

				lineColor.r = 0.8f;
				lineColor.g = 0.8f;
				lineColor.b = 0.8f;

				townSBLineRenderer.SetColors(lineColor, lineColor);
			}

			float mapScale = _MapStage.transform.localScale.x;
			Vector3 mapVecotor = _MapStage.transform.position;
			float mapPosX = mapVecotor.x;
			float mapPosY = mapVecotor.y;

			float startPosx = (town.position.x * mapScale) + mapPosX;
			float startPosy = (town.position.y * mapScale) + mapPosY;
			float endPosx = (prevTown.position.x * mapScale) + mapPosX;
			float endPosy = (prevTown.position.y * mapScale) + mapPosY;

			Vector3 startVector = transform.TransformDirection(new Vector3(startPosx, startPosy, 0f));
			Vector3 endVector = transform.TransformDirection(new Vector3(endPosx, endPosy, 0f));

			townSBLineRenderer.SetPosition(0, startVector);
			townSBLineRenderer.SetPosition(1, endVector);
		}

	}

	private void deleteTown() {
		if(_SBList != null) {
			foreach(GameObject townSB in _SBList) {
				if(townSB != null) Destroy(townSB);
			}
		}
		_SBList = null;
	}

	public void UpdateTownSymbols() {
		deleteTown();
		_MapStage.transform.localScale = new Vector3(_DefaultScale, _DefaultScale, _DefaultScale);
		_MapStage.transform.position = new Vector3(0f, 0f, 0f);
		CreateTown();
		if(_CurrentTownId > 0) {
			//_MapStage.transform.localScale = new Vector3(_ZoomInScale, _ZoomInScale, _ZoomInScale);
		}
	}

	public void UpdateTownMove(byte TownId) {
		_CurrentTownId = TownId;
		if(TownId > 0) {
			Town town = WarzoneData.getInstence().GetDefaultTownData(TownId);

			UpdateScaleMove(_ZoomInScale, town.position.x * _ZoomInScale * -1, town.position.y * _ZoomInScale * -1);
			
		} else {
			UpdateScaleMove(_DefaultScale, 0, 0);
		}
	}

	public void UpdateScaleMove(float scale, float x, float y) {
		iTween.MoveTo(_MapStage, iTween.Hash("x", x, "y", y, "time", _ScaleTime, "easetype", iTween.EaseType.easeOutSine));
		iTween.ScaleTo(_MapStage, iTween.Hash("x", scale, "y", scale, "time", _ScaleTime, "easetype", iTween.EaseType.easeOutSine));
	}

	public float GetDefaultScale() {
		return _DefaultScale;
	}

}
