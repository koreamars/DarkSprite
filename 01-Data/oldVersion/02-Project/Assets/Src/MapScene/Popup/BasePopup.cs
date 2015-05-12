using UnityEngine;
using System.Collections;

public class BasePopup : MonoBehaviour {

	public Texture CloseTexture;
	public Texture TitleTexture;
	public Texture LineBg;

	public delegate void ContentsScript();
	public delegate void CloseEvent();
	private ContentsScript ContentsScirptFunc;
	private CloseEvent CloseEventFunc;

	private Rect _WindowRect;

	public void BasePopupLayerOut(Rect windowRect, int windowId) {
		_WindowRect = windowRect;
		GUILayout.Window(windowId, windowRect, OnPopup, "");

	}

	public void SetContentsScirpt(ContentsScript OnContentsScript) {
		ContentsScirptFunc = new ContentsScript(OnContentsScript);
	}

	public void SetCloseEvent(CloseEvent OnCloseEvent) {
		CloseEventFunc = new CloseEvent(OnCloseEvent);
	}

	private void OnPopup(int windowID) {

		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		GUILayout.Label(TitleTexture, GUILayout.Width(280), GUILayout.Height(32));
		GUI.skin.label.alignment = TextAnchor.UpperRight;
		GUILayout.BeginArea(new Rect(_WindowRect.width - 40, 12, 30, 30));
		if(GUI.Button(new Rect(0, 0, 30, 30), CloseTexture, GUIStyle.none)) 
			OnPopupClose();
		GUILayout.EndArea();
		if(ContentsScirptFunc != null) ContentsScirptFunc();
		
	}

	private void OnPopupClose() {
		if(CloseEventFunc != null) CloseEventFunc();
	}


}
