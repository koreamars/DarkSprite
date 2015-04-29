using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;

public class AdmobSripteTest : MonoBehaviour {

	BannerView bannerView;

	// Use this for initialization
	void Start () {
		AdSize adSize = new AdSize(360, 50);
		// Create a 320x50 banner at the top of the screen.
		bannerView = new BannerView("ca-app-pub-4325367879357929/7581160494", AdSize.Banner, AdPosition.Top);
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();
		// Load the banner with the request.
		bannerView.LoadAd(request);
		
		bannerView.Show();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
