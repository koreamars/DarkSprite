using UnityEngine;
using System.Collections;

public class StoryViewTest : MonoBehaviour {

	public int StartStoryStep = 0;
	// Use this for initialization
	void Start () {

		UserData.getInstence();
		SystemData.GetInstance();
		//StoryData.getInstence();
		StartCoroutine(StoryData.getInstence().init());
	}

	void OnGUI() {
		if(GUI.Button(new Rect(0, 0, 100, 30), "view start")) 
			StoryData.getInstence().UpdateStoryStep(StartStoryStep);	
	}

}
