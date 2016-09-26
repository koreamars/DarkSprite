using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FrameTest : MonoBehaviour {

    public Text fpsTextField;

    private float deltaTime = 0.0f;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        //string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        float msec = deltaTime * 1000.0f;
        fpsTextField.text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
	}
}
