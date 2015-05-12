using UnityEngine;
using System.Collections;

public class FirstFightGuide : MonoBehaviour {

	private GameObject _TextField1;
	private GameObject _TextField2;
	private GameObject _TextField3;
	private GameObject _TextField4;
	private GameObject _TextField5;
	private GameObject _Bg;

	private int sortNum = 100;

	// Use this for initialization
	void Start () {

		float parentX = this.gameObject.transform.position.x;
		float parentY = this.gameObject.transform.position.y;
		float parentZ = this.gameObject.transform.position.z;

		ScriptData scriptData = ScriptData.getInstence();

		_Bg = this.gameObject.transform.FindChild("fightGuideImg").gameObject;
		_Bg.renderer.sortingOrder = sortNum - 1;
		_Bg.layer = LayerMask.NameToLayer("UI");

		_TextField1 = CustomTextMesh.SetAddTextMesh(scriptData.GetGameScript(150140).script, 18, TextAnchor.MiddleRight, Color.white, sortNum, "UI");
		_TextField2 = CustomTextMesh.SetAddTextMesh(scriptData.GetGameScript(150141).script, 18, TextAnchor.MiddleLeft, Color.white, sortNum, "UI");
		_TextField3 = CustomTextMesh.SetAddTextMesh(scriptData.GetGameScript(150143).script, 18, TextAnchor.MiddleRight, Color.white, sortNum, "UI");
		_TextField4 = CustomTextMesh.SetAddTextMesh(scriptData.GetGameScript(150144).script, 18, TextAnchor.MiddleLeft, Color.white, sortNum, "UI");
		_TextField5 = CustomTextMesh.SetAddTextMesh(scriptData.GetGameScript(150142).script, 18, TextAnchor.MiddleCenter, Color.white, sortNum, "UI");

		_TextField1.transform.parent = this.gameObject.transform;
		_TextField2.transform.parent = this.gameObject.transform;
		_TextField3.transform.parent = this.gameObject.transform;
		_TextField4.transform.parent = this.gameObject.transform;
		_TextField5.transform.parent = this.gameObject.transform;

		_TextField1.transform.position = new Vector3(parentX + 0.04f, parentY + 1.84f, parentZ);
		_TextField2.transform.position = new Vector3(parentX + 0.3f, parentY + 2.58f, parentZ);
		_TextField3.transform.position = new Vector3(parentX + 0.8f, parentY + -2.08f, parentZ);
		_TextField4.transform.position = new Vector3(parentX + 0.72f, parentY + 0.31f, parentZ);
		_TextField5.transform.position = new Vector3(parentX + 3.57f, parentY + -2.77f, parentZ);
	}

	void OnMouseDown() {
		Destroy(this.gameObject);
	}

}
