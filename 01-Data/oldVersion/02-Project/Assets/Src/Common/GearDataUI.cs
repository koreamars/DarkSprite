using UnityEngine;
using System.Collections;

public class GearDataUI : MonoBehaviour {

	public bool isTest;
	public int testGearId = 1;
	
	private GameObject _GearThumb;
	private TextMesh _GearName;
	private TextMesh _DataTitle1;
	private TextMesh _DataTitle2;
	private TextMesh _DataTitle3;
	private TextMesh _DataTitle4;
	private TextMesh _DataTitle5;
	private TextMesh _DataValue1;
	private TextMesh _DataValue2;
	private TextMesh _DataValue3;
	private TextMesh _DataValue4;
	private TextMesh _DataValue5;
	private GameObject _ThumbnailBg;
	
	void Start() {
		if(isTest) {
			
			UserData.getInstence();
			GearData.getInstence();
			LocalData.getInstence().AllLoad();
			
			init();
			GearUpdate((short)(testGearId));
		}
	}
	
	public void init() {
		_GearThumb = this.gameObject.transform.FindChild("GearThumbnail").gameObject;
		_GearName = this.gameObject.transform.FindChild("GearName").GetComponent<TextMesh>();
		_DataTitle1 = this.gameObject.transform.FindChild("Txt1Title").GetComponent<TextMesh>();
		_DataTitle2 = this.gameObject.transform.FindChild("Txt2Title").GetComponent<TextMesh>();
		_DataTitle3 = this.gameObject.transform.FindChild("Txt3Title").GetComponent<TextMesh>();
		_DataTitle4 = this.gameObject.transform.FindChild("Txt4Title").GetComponent<TextMesh>();
		_DataTitle5 = this.gameObject.transform.FindChild("Txt5Title").GetComponent<TextMesh>();
		_DataValue1 = this.gameObject.transform.FindChild("Txt1").GetComponent<TextMesh>();
		_DataValue2 = this.gameObject.transform.FindChild("Txt2").GetComponent<TextMesh>();
		_DataValue3 = this.gameObject.transform.FindChild("Txt3").GetComponent<TextMesh>();
		_DataValue4 = this.gameObject.transform.FindChild("Txt4").GetComponent<TextMesh>();
		_DataValue5 = this.gameObject.transform.FindChild("Txt5").GetComponent<TextMesh>();
		_ThumbnailBg = this.gameObject.transform.FindChild("ThumbnailBg").gameObject;

	}
	
	public void GearUpdate(short gearId) {
		
		ScriptData scriptData = ScriptData.getInstence();
		
		_DataTitle1.text = "";
		_DataTitle2.text = "";
		_DataTitle3.text = "";
		_DataTitle4.text = "";
		_DataTitle5.text = "";
		_DataValue1.text = "";
		_DataValue2.text = "";
		_DataValue3.text = "";
		_DataValue4.text = "";
		_DataValue5.text = "";

		Gear gear = GearData.getInstence().GetGearByID(gearId);

		if(gear != null) {
			_GearName.text = scriptData.GetGameScript(gear.scriptId).script;
			SetSpriteChange(_GearThumb, "UnitResource/Thumbnail/" + gear.thumbnailURI);

			switch(gear.gearType){
			case GearType.Suit:
				_DataTitle1.text = scriptData.GetGameScript(160117).script;
				_DataTitle2.text = scriptData.GetGameScript(160119).script;

				_DataValue1.text = CheckValue(gear.spendIA, true, false);
				_DataValue2.text = CheckValue(gear.addMP, true, true);

				break;
			case GearType.Body:
				_DataTitle1.text = scriptData.GetGameScript(160116).script;
				_DataTitle2.text = scriptData.GetGameScript(160118).script;
				_DataTitle3.text = scriptData.GetGameScript(160120).script;
				
				_DataValue1.text = CheckValue(gear.addIA, false, false);
				_DataValue2.text = CheckValue(gear.addHP, true, true);
				_DataValue3.text = CheckValue(gear.spendMP, false, false);

				break;
			case GearType.Engine:
				_DataTitle1.text = scriptData.GetGameScript(160117).script;
				_DataTitle2.text = scriptData.GetGameScript(160120).script;
				
				_DataValue1.text = CheckValue(gear.spendIA, true, false);
				_DataValue2.text = CheckValue(gear.spendMP, false, false);

				break;
			case GearType.Weapon_Gun:
			case GearType.Weapon_Missle:
			case GearType.Weapon_Rocket:
				_DataTitle1.text = scriptData.GetGameScript(160116).script;
				_DataTitle2.text = scriptData.GetGameScript(160120).script;
				_DataTitle3.text = "<color=#ff3300>" + scriptData.GetGameScript(160121).script + "</color>";
				if(gear.maxIAD > 0) {
					_DataTitle4.text = "<color=#ff3300>" + scriptData.GetGameScript(160122).script + "</color>";
					_DataTitle5.text = scriptData.GetGameScript(160125).script;
				} else {
					_DataTitle4.text = scriptData.GetGameScript(160125).script;
				}

				_DataValue1.text = CheckValue(gear.addIA, false, true);
				_DataValue2.text = CheckValue(gear.spendMP, false, false);
				_DataValue3.text = "<color=#ff3300>" + gear.minAP + "~" + gear.maxAP + "</color>";
				if(gear.maxIAD > 0) {
					_DataValue4.text = "<color=#ff3300>" + gear.minIAD + "~" + gear.maxIAD + "</color>";
					_DataValue5.text = gear.ammo.ToString();
				} else {
					_DataValue4.text = scriptData.GetGameScript(160124).script;
				}

				break;
			}

			_ThumbnailBg.renderer.enabled = true;

			
		} else {
			SetSpriteChange(_GearThumb, "");
			
			_GearName.text = "";

			_ThumbnailBg.renderer.enabled = false;

		}
	}

	public void SetSorting(int sortNum) {
		_GearThumb.renderer.sortingOrder = sortNum;
		_GearName.gameObject.renderer.sortingOrder = sortNum;
		_DataTitle1.gameObject.renderer.sortingOrder = sortNum;
		_DataTitle2.gameObject.renderer.sortingOrder = sortNum;
		_DataTitle3.gameObject.renderer.sortingOrder = sortNum;
		_DataTitle4.gameObject.renderer.sortingOrder = sortNum;
		_DataTitle5.gameObject.renderer.sortingOrder = sortNum;
		_DataValue1.gameObject.renderer.sortingOrder = sortNum;
		_DataValue2.gameObject.renderer.sortingOrder = sortNum;
		_DataValue3.gameObject.renderer.sortingOrder = sortNum;
		_DataValue4.gameObject.renderer.sortingOrder = sortNum;
		_DataValue5.gameObject.renderer.sortingOrder = sortNum;
		_ThumbnailBg.renderer.sortingOrder = sortNum - 1;
	}

	public void SetSortLayer(string layerName) {
		_GearThumb.layer = LayerMask.NameToLayer(layerName);
		_ThumbnailBg.layer = LayerMask.NameToLayer(layerName);
		_DataTitle1.gameObject.layer = LayerMask.NameToLayer(layerName);
		_DataTitle2.gameObject.layer = LayerMask.NameToLayer(layerName);
		_DataTitle3.gameObject.layer = LayerMask.NameToLayer(layerName);
		_DataTitle4.gameObject.layer = LayerMask.NameToLayer(layerName);
		_DataTitle5.gameObject.layer = LayerMask.NameToLayer(layerName);
		_DataValue1.gameObject.layer = LayerMask.NameToLayer(layerName);
		_DataValue2.gameObject.layer = LayerMask.NameToLayer(layerName);
		_DataValue3.gameObject.layer = LayerMask.NameToLayer(layerName);
		_DataValue4.gameObject.layer = LayerMask.NameToLayer(layerName);
		_DataValue5.gameObject.layer = LayerMask.NameToLayer(layerName);
	}

	private string CheckValue(short value, bool upType, bool markType) {
		string returnStr = "";
		string colorType = "#ffffff";
		string markState = "";

		if(value > 0) {
			if(upType) {
				colorType = "#00cc66";
			} else {
				colorType = "#ff3300";
			}
			if(markType) {
				markState = "+";
			} else {
				markState = "-";
			}
		}

		returnStr = "<color=" + colorType + ">" + markState + "" + value + "</color>";

		return returnStr;
	}

	private void SetSpriteChange(GameObject obj, string uri) {
		SpriteRenderer renderer = (SpriteRenderer)obj.GetComponent ("SpriteRenderer");
		if(renderer != null) renderer.sprite = Resources.Load<Sprite>(uri);
		
	}
}
