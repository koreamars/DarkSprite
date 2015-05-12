using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {

	public GUIText GuiText;

	void Start() {
		GetAndroidStr();
	}

	void GetAndroidStr() {
		GuiText.text = "android test ///";
#if UNITY_EDITOR
		GuiText.text += "로컬 테스트다 냥~";
#elif UNITY_ANDROID
		GuiText.text += "android test ///";
		try {
			AndroidJavaClass _ajc = new AndroidJavaClass("com.kakaoplugin.KakaoTest");
			GuiText.text += " ///" + _ajc;

			string rtnstr = _ajc.CallStatic<string>("ReturnStr", "정적 메서드 안뇽~");
			GuiText.text += rtnstr + "///";
			
			int rtnint = _ajc.CallStatic<int>("ReturnInt", 999);
			GuiText.text += rtnint + "///";
			
			AndroidJavaObject _ajo = new AndroidJavaObject("com.kakaoplugin.KakaoTest");
			string Comrtnstr = _ajo.Call<string>("ComReturnStr", "걍 메서드 안뇽~");
			GuiText.text += Comrtnstr + "///";
			
			int Comrtnint = _ajo.Call<int>("ComReturnInt", 999999);
			GuiText.text += Comrtnint + "///";

		} catch (System.Exception e) {
			//if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("Error", "UpdateTown");
			//Debug.LogError(e.Data.ToString());
			GuiText.text = e.ToString();
		}

#endif

	}
}
