using UnityEngine;
using System.Collections;

public class MapSymbol : MonoBehaviour {

	public bool isTest;
	public GameObject MapSB;
	public GameObject OutLine;

	private GameObject _TownName;
	private GameObject _AttackEffect;
	private byte _SortNum;

	public byte TownId = 0;

	public Color TownColor;
	public Color GhostColor;

	void Start() {
		if(isTest) {
			WarzoneData.getInstence();
			ScriptData.getInstence();
			init(2, 0);
		}
	}

	public void init(byte townId, byte sortNum) {
		TownId = townId;
		_SortNum = sortNum;
		MapSB.GetComponent<Renderer>().sortingOrder = sortNum + 1;

		CreateTownName(townId);
		SetTownSymbol(townId);

	}

	private void CreateTownName(byte townId) {

		Town town = WarzoneData.getInstence().GetDefaultTownData(townId);
		
		if(town == null) return;

		_TownName = Instantiate(Resources.Load<GameObject>("OutlineFont"), new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
		_TownName.transform.parent = this.gameObject.transform;
		_TownName.transform.localScale = new Vector3(1f, 1f, 1f);

		_TownName.GetComponent<OutLineFont>().SetString(ScriptData.getInstence().GetGameScript(town.townNameId).script);
		_TownName.GetComponent<OutLineFont>().SetFontSize(18);
		_TownName.GetComponent<OutLineFont>().SetLineSize(2);
		_TownName.GetComponent<OutLineFont>().SetSort(_SortNum);
		_TownName.GetComponent<OutLineFont>().SetSortLayer("Bg");
		_TownName.transform.position = new Vector3(0f, -0.6f, 0f);
		_TownName.layer = LayerMask.NameToLayer("Bg");

	}

	public void SetTownSymbol(byte townId) {
		try {
			SpriteRenderer renderer = MapSB.GetComponent<SpriteRenderer>();
			Sprite sbSprite;

			UserTown userTown = WarzoneData.getInstence().GetUserTownByID(townId);

			TownColor.a = 1f;
			if(_TownName != null) _TownName.GetComponent<OutLineFont>().SetFontColor(TownColor);
			
			if(townId == 1) {
				sbSprite = Resources.Load<Sprite>("MainScene/MapSymbol/MapSymbol00");
				renderer.sprite = sbSprite;
				if(userTown != null && userTown.isInvasion) {
					AddAttackEffect();
				} else {
					DeleteAttackEffect();
				}

				return;
			}

			int residentCount;
			if(userTown != null) {
				// 유저 마을 일경우.
				residentCount = userTown.resident;
				if(residentCount > 500 && residentCount <= 1000) {
					sbSprite = Resources.Load<Sprite>("MainScene/MapSymbol/MapSymbol02");
				} else if (residentCount > 1000) {
					sbSprite = Resources.Load<Sprite>("MainScene/MapSymbol/MapSymbol03");
				} else {
					sbSprite = Resources.Load<Sprite>("MainScene/MapSymbol/MapSymbol01");
				}

				if(userTown != null && userTown.isInvasion) {
					AddAttackEffect();
				} else {
					DeleteAttackEffect();
				}

			} else {
				// 고스트 둥지 일경우.
				DeleteAttackEffect();
				GhostTown ghostTown = WarzoneData.getInstence().GetGhostTownByTownId(townId);
				if(ghostTown == null) return;
				if(ghostTown.ghostClose >= 100 && ghostTown.ghostClose < 500) {
					sbSprite = Resources.Load<Sprite>("MainScene/MapSymbol/MapSymbol05");
				} else if (ghostTown.ghostClose >= 500 && ghostTown.ghostClose < 1000) {
					sbSprite = Resources.Load<Sprite>("MainScene/MapSymbol/MapSymbol06");
				} else if (ghostTown.ghostClose >= 1000 && ghostTown.ghostClose < 1500) {
					sbSprite = Resources.Load<Sprite>("MainScene/MapSymbol/MapSymbol07");
				} else if (ghostTown.ghostClose >= 1500) {
					sbSprite = Resources.Load<Sprite>("MainScene/MapSymbol/MapSymbol08");
				} else {
					sbSprite = Resources.Load<Sprite>("MainScene/MapSymbol/MapSymbol04");
				}

				GhostColor.a = 1f;
				if(_TownName != null) _TownName.GetComponent<OutLineFont>().SetFontColor(GhostColor);

			}

			renderer.sprite = sbSprite;
		} catch (System.Exception e) {
			if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("Error", "SetTownSymbol");
			print("===============================  Exception townId : " + e.Data);
		}
	}

	private void AddAttackEffect() {

		if(_AttackEffect == null) {
			_AttackEffect = new GameObject();
			_AttackEffect.AddComponent<SpriteRenderer>();
			_AttackEffect.transform.parent = this.gameObject.transform;
		}

		_AttackEffect.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 0.1f, this.gameObject.transform.position.z);
		_AttackEffect.transform.localScale = new Vector2(1f, 1f);
		_AttackEffect.layer = LayerMask.NameToLayer("Bg");
		SpriteRenderer effectRenderer = _AttackEffect.GetComponent<SpriteRenderer>();
		effectRenderer.sortingOrder = _SortNum;
		effectRenderer.sprite = Resources.Load<Sprite>("MainScene/MapSymbol/MapSBEffect");

	}

	private void DeleteAttackEffect() {
		if(_AttackEffect != null) {
			Destroy(_AttackEffect);
		}
	}



}
