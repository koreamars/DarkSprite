using UnityEngine;
using System.Collections;

public class CustomTextMesh : MonoBehaviour {

	public static GameObject SetAddTextMesh(string str, int fontSize, TextAnchor anchor, Color color, int sortNum, string targetLayer) {
		GameObject targetobj = Instantiate(Resources.Load<GameObject>("DefaultFont")) as GameObject;
		targetobj.layer = LayerMask.NameToLayer(targetLayer);
		TextMesh textMesh = targetobj.GetComponent<TextMesh>();
		textMesh.text = str;
		textMesh.anchor = anchor;
		textMesh.color = color;
		textMesh.fontSize = fontSize;
		targetobj.renderer.sortingOrder = sortNum;
		
		return targetobj;
	}
}
