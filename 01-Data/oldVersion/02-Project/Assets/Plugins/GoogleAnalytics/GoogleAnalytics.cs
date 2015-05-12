using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoogleAnalytics : MonoBehaviour {
	
	public string propertyID;
	
	public static GoogleAnalytics instance;
	
	public string bundleID;
	public string appName;
	public string appVersion;
	
	private string screenRes;
	private string clientID;
	
	void Awake()
	{
		if(instance)
			DestroyImmediate(gameObject);
		else
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
	}
	
	void Start() 
	{
		screenRes = Screen.width + "x" + Screen.height;

		if(Application.platform == RuntimePlatform.WindowsEditor)
		{
			propertyID = "UA-44211594-5";
		}
		
		#if UNITY_IPHONE
		clientID = iPhoneSettings.uniqueIdentifier;
		#else
		clientID = SystemInfo.deviceUniqueIdentifier;
		#endif

	}
	
	public void LogScreen(string title)
	{

		//if(Application.platform == RuntimePlatform.WindowsEditor) return;

		Debug.Log ("Google Analytics - Screen --> " + title);
		
		title = WWW.EscapeURL(title);

		var url = "http://www.google-analytics.com/collect?v=1&ul=en-us&t=appview&sr="+screenRes+"&an="+WWW.EscapeURL(appName)+"&a=448166238&tid="+propertyID+"&aid="+bundleID+"&cid="+WWW.EscapeURL(clientID)+"&_u=.sB&av="+appVersion+"&_v=ma1b3&cd="+title+"&qt=2500&z=185&el=" + WWW.EscapeURL(clientID);
		
		StartCoroutine( Process(new WWW(url)) );
	}
	
	/*  MOBILE EVENT TRACKING:  https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide */
	public void LogEvent(string titleCat, string titleAction)
	{
		//if(Application.platform == RuntimePlatform.WindowsEditor) return;

		Debug.Log ("Google Analytics - Event --> " + titleAction);
		
		titleCat = WWW.EscapeURL(titleCat);
		titleAction = WWW.EscapeURL(titleAction);
		
		var url = "http://www.google-analytics.com/collect?v=1&ul=en-us&t=event&sr="+screenRes+"&an="+WWW.EscapeURL(appName)+"&a=448166238&tid="+propertyID+"&aid="+bundleID+"&cid="+WWW.EscapeURL(clientID)+"&_u=.sB&av="+appVersion+"&_v=ma1b3&ec="+titleCat+"&ea="+titleAction+"&qt=2500&z=185&el=" + WWW.EscapeURL(clientID);
		
		StartCoroutine( Process(new WWW(url)) );
	}
	
	
	
	private IEnumerator Process(WWW www)
	{
		yield return www;
		
		if(www.error == null)
		{
			if (www.responseHeaders.ContainsKey("STATUS"))
			{
				if (www.responseHeaders["STATUS"] == "HTTP/1.1 200 OK")	
				{
					Debug.Log ("GA Success");
				}else{
					Debug.LogWarning(www.responseHeaders["STATUS"]);	
				}
			}else{
				Debug.LogWarning("Event failed to send to Google");	
			}
		}else{
			Debug.LogWarning(www.error.ToString());	
		}
		
		www.Dispose();
	}
	
	
}