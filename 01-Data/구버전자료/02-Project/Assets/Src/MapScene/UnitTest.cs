using UnityEngine;
using System.Collections;

public class UnitTest : MonoBehaviour {

	public GameObject ListMenu;

	private ScrollMenu _ScrollMenu;

	private ListMenuModel[] TestListData;

	// Use this for initialization
	void Start () {

		byte testCount = 15;
		TestListData = new ListMenuModel[testCount];
		for(byte i=0; i<testCount; i++) {
			TestListData[i] = new ListMenuModel();
			TestListData[i].id = i;
			TestListData[i].scriptId = 100000;
		}

		_ScrollMenu = ListMenu.GetComponent<ScrollMenu>();

		_ScrollMenu.SetScrollData(TestListData, 0);
		_ScrollMenu.SetScrollView();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
