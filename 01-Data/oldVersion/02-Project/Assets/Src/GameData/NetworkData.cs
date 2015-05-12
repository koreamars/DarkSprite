using System; 
using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class NetworkData : MonoBehaviour {

	private static NetworkData _instence;

	public WWW results = null;

	public static NetworkData getInstence()	{
		if(_instence == null) 
		{
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<NetworkData>();
			DontDestroyOnLoad(_instence);
		}

		return _instence;
	}

	public IEnumerator LoadGameData() {
		yield return new WaitForEndOfFrame();

		//var url = "http://www.clana.kr/DarkSprite/GameData.txt";
		print("LoadGameData !!");

		//WWW results = GET("http://www.clana.kr/DarkSprite/GameData.txt");
		results = new WWW ("http://www.clana.kr/DarkSprite/GameData.txt");

		yield return StartCoroutine (WaitForRequest (results));
	}

	public WWW GET(string url)
	{
		
		WWW www = new WWW (url);
		StartCoroutine (WaitForRequest (www));
		return www; 
	}

	public WWW POST(string url, Dictionary<string,string> post)
	{
		WWWForm form = new WWWForm();
		foreach(KeyValuePair<String,String> post_arg in post)
		{
			form.AddField(post_arg.Key, post_arg.Value);
		}
		WWW www = new WWW(url, form);
		
		StartCoroutine(WaitForRequest(www));
		return www; 
	}

	private IEnumerator WaitForRequest(WWW www)
	{
		yield return www;
		
		// check for errors
		if (www.error == null) {
			Debug.Log("WWW Ok!: " + www.text);
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}
	}

	/** 카톡 API */
	public void SendKakao(string paramsStr, string code) {
		if(GoogleAnalytics.instance) GoogleAnalytics.instance.LogEvent("KakaoSend", code);
		Application.OpenURL("http://www.clana.kr/DarkSprite/kakaoWeb.php?" + paramsStr);
	}
}
