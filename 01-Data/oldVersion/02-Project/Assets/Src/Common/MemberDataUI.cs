using UnityEngine;
using System.Collections;

public class MemberDataUI : MonoBehaviour {

	public bool isTest;

	private GameObject _MemberThumb;
	private GameObject _MemberClass;
	private TextMesh _MemberName;
	private TextMesh _MemberClassName;
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
	private GameObject _StateSymbol;

	void Start() {
		if(isTest) {

			UserData.getInstence();
			LocalData.getInstence().AllLoad();

			init(0);
			Member member = UserData.getInstence().UserMemberList[1] as Member;
			MemberUpdate(member.id, false);
		}
	}

	public void init(int sortNum) {
		_MemberThumb = this.gameObject.transform.FindChild("Member001").gameObject;
		_MemberClass = this.gameObject.transform.FindChild("ClassMark001").gameObject;
		_MemberName = this.gameObject.transform.FindChild("MemberName").GetComponent<TextMesh>();
		_MemberClassName = this.gameObject.transform.FindChild("MemberClass").GetComponent<TextMesh>();
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
		_StateSymbol = this.gameObject.transform.FindChild("StateSymbol").gameObject;

		_MemberThumb.GetComponent<Renderer>().sortingOrder = sortNum + 1;
		_MemberClass.GetComponent<Renderer>().sortingOrder = sortNum;
		_MemberName.GetComponent<Renderer>().sortingOrder = sortNum;
		_MemberClassName.GetComponent<Renderer>().sortingOrder = sortNum;
		_DataTitle1.GetComponent<Renderer>().sortingOrder = sortNum;
		_DataTitle2.GetComponent<Renderer>().sortingOrder = sortNum;
		_DataTitle3.GetComponent<Renderer>().sortingOrder = sortNum;
		_DataTitle4.GetComponent<Renderer>().sortingOrder = sortNum;
		_DataTitle5.GetComponent<Renderer>().sortingOrder = sortNum;
		_DataValue1.GetComponent<Renderer>().sortingOrder = sortNum;
		_DataValue2.GetComponent<Renderer>().sortingOrder = sortNum;
		_DataValue3.GetComponent<Renderer>().sortingOrder = sortNum;
		_DataValue4.GetComponent<Renderer>().sortingOrder = sortNum;
		_DataValue5.GetComponent<Renderer>().sortingOrder = sortNum;
		_ThumbnailBg.GetComponent<Renderer>().sortingOrder = sortNum;
		_StateSymbol.GetComponent<Renderer>().sortingOrder = sortNum;

	}

	public void SetSortLayer(string layerName) {
		_MemberThumb.layer = LayerMask.NameToLayer(layerName);
		_MemberClass.layer = LayerMask.NameToLayer(layerName);
		_MemberName.gameObject.layer = LayerMask.NameToLayer(layerName);
		_MemberClassName.gameObject.layer = LayerMask.NameToLayer(layerName);
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
		_ThumbnailBg.layer = LayerMask.NameToLayer(layerName);
		_StateSymbol.layer = LayerMask.NameToLayer(layerName);
	}

	public void MemberUpdate(short memberId, bool isDefault) {

		ScriptData scriptData = ScriptData.getInstence();

		_DataTitle1.text = scriptData.GetGameScript(160133).script;
		_DataTitle2.text = scriptData.GetGameScript(160112).script;
		_DataTitle3.text = scriptData.GetGameScript(160123).script;
		_DataTitle4.text = scriptData.GetGameScript(160132).script;
		_DataTitle5.text = scriptData.GetGameScript(160113).script;

		Member member = null;
		DefaultMember defaultMember = null;
		ClassModel classModel;
		if(isDefault) {
			defaultMember = MemberData.getInstence().GetDefaultMemberByID(memberId);
		} else {
			member = UserData.getInstence().GetMemberById(memberId);
			if(member != null) defaultMember = MemberData.getInstence().GetDefaultMemberByID(member.DefaultId);
		}

		if(member != null) {
			classModel = MemberData.getInstence().GetClassModelByClassId(member.ClassId);
			_MemberName.text = scriptData.GetMemberNameByMemberId(member.id);
			_MemberClassName.text = scriptData.GetGameScript(classModel.scriptId).script;
			SetSpriteChange(_MemberThumb, "MemberImg/Member" + defaultMember.thumbId);
			SetSpriteChange(_MemberClass, "ClassMark/ClassMark" + classModel.Markid);

			if(member.ClassId >= SystemData.GetInstance().MemberMaxClass) {
				_DataValue1.text = "<color=#ffcc00>MAX</color>";
			} else {
				_DataValue1.text = member.Exp + "/" + classModel.exp;
			}

			//float hpCount = (float)(member.CurrentHP) / (float)(member.MaxHP);
			string hpColor = "#ffffff";
			if(member.CurrentHP <= 20) {
				hpColor = "#ff0000";
			} else if (member.CurrentHP > 20 && member.CurrentHP <= 30) {
				hpColor = "#ffc600";

			}
			GearData.getInstence().UpdateMemberGearSpec(member);
			_DataValue2.text = "<color=" + hpColor + ">" + member.CurrentHP + "/" + member.MaxHP + "</color>";
			byte warCount = UMPLevelCheck(member.UMP, member.CurrentMP);
			string umpColor = "#99ccff";
			string mpColor = "#ffffff";
			if(warCount < 1) {
				umpColor = "#ff0000";
				mpColor = "#ff0000";
			} else if(warCount == 1) {
				umpColor = "#ffc600";
				mpColor = "#ffc600";
			} 
			_DataValue3.text = "<color=" + mpColor + ">" + member.CurrentMP + "/" + member.MaxMP + "</color>";
			_DataValue4.text = "<color=" + umpColor + ">" + member.UMP.ToString() + "(" + warCount + ")</color>";
			_DataValue5.text = member.TotalIA.ToString();

			_ThumbnailBg.GetComponent<Renderer>().enabled = true;

			string stateStr = "";
			if(member.state == MemberStateType.Wound) {
				stateStr = "Common/EndGameSymbol02";
			} else if (member.state == MemberStateType.Mia) {
				stateStr = "Common/EndGameSymbol03";
			}

			SetSpriteChange(_StateSymbol, stateStr);

		} else {
			if(defaultMember != null) {
				classModel = MemberData.getInstence().GetClassModelByClassId(defaultMember.classN);
				_MemberName.text = scriptData.GetDefaultMemberName(defaultMember.nameId);
				_MemberClassName.text = scriptData.GetGameScript(classModel.scriptId).script;
				SetSpriteChange(_MemberThumb, "MemberImg/Member" + defaultMember.thumbId);
				SetSpriteChange(_MemberClass, "ClassMark/ClassMark" + classModel.Markid);

				_DataTitle1.text = scriptData.GetGameScript(160112).script;
				_DataTitle2.text = scriptData.GetGameScript(160123).script;
				_DataTitle3.text = scriptData.GetGameScript(160113).script;
				_DataTitle4.text = "";
				_DataTitle5.text = "";
				_DataValue1.text = classModel.HP.ToString();
				_DataValue2.text = classModel.MP.ToString();
				_DataValue3.text = classModel.IA.ToString();
				_DataValue4.text = "";
				_DataValue5.text = "";

				_ThumbnailBg.GetComponent<Renderer>().enabled = true;

			} else {
				SetSpriteChange(_MemberThumb, "MemberImg/");
				SetSpriteChange(_MemberClass, "ClassMark/ClassMark");
				
				_MemberName.text = "";
				_MemberClassName.text = "";
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
				
				_ThumbnailBg.GetComponent<Renderer>().enabled = false;
			}

			SetSpriteChange(_StateSymbol, "");
		}
	}

	private byte UMPLevelCheck(int umpValue, int currentMP) {
		if(umpValue == 0) return 0;
		byte umpcount = (byte)(currentMP / umpValue);
		return umpcount;
	}

	private void SetSpriteChange(GameObject obj, string uri) {
		SpriteRenderer renderer = (SpriteRenderer)obj.GetComponent ("SpriteRenderer");
		if(renderer != null) renderer.sprite = Resources.Load<Sprite>(uri);

	}
}
