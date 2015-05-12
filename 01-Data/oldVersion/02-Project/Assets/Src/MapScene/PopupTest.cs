using UnityEngine;
using System.Collections;

public class PopupTest : MonoBehaviour {

	public GUISkin guiSkin;

	public Texture2D icon;

	//private float windowY = -500f;
	private float windowX;

	void Start() {
		windowX = Screen.width + 25;
	}


	void DoMyWindow(int windowID) {

		//GUILayout.Label("", "test");
		//GUILayout.TextArea("asdlfjasldfjklas");
		//GUILayout.Box(icon);
		GUILayout.Label(icon, GUILayout.Width(100f));
		GUILayout.BeginArea(new Rect(20, 30, 200, 450));
		GUILayout.Label(icon);
		GUILayout.Space(20);

		//GUILayout.BeginVertical();
		GUILayout.Button("Short Button", GUILayout.ExpandWidth(false));
		GUILayout.Button("Very very long Button");

		GUILayout.EndArea();
		//GUILayout.EndVertical();


	}

	public void OnGUI()
	{
		GUI.skin = guiSkin;

		GUILayout.Window(0, new Rect(windowX, 20, 300, 400), DoMyWindow, "");

		if(windowX < (Screen.width / 2))
		{
			windowX = (Screen.width / 2);
		}
		else
		{
			windowX = windowX - 25;
		}

		//GUI.Button (new Rect (10,10,100,100), new GUIContent ("", icon, "This is the tooltip"));
		//GUI.Label (new Rect (10,80,100,100), GUI.tooltip);

	}
}
