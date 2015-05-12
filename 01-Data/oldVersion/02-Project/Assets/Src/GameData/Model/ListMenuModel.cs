using UnityEngine;
using System.Collections;

public class ListMenuModel {

	public short id;
	public int scriptId;	// 버튼으로 노출할 스크립트의 아이디.
	public short optionalId;	// 추가로 사용할 아이디.
	public string scriptString; 	// scriptId가 없을 경우 대체 되는 스크립트.
	public Color fontColor = Color.white;
	public Color outLineColor = Color.black;
	public bool isClick = true;
}
