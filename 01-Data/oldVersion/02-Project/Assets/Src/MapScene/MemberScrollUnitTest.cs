using UnityEngine;
using System.Collections;

public class MemberScrollUnitTest : MonoBehaviour {
	public GameObject ListMenu;
	
	private ScrollMenberMenu _ScrollMenu;
	
	private ListMemberModel[] TestListData;
	
	// Use this for initialization
	void Start () {
		
		byte testCount = 15;
		TestListData = new ListMemberModel[testCount];
		for(byte i=0; i<testCount; i++) {
			TestListData[i] = new ListMemberModel();
			TestListData[i].id = i;
			TestListData[i].ClassId = 2;
			TestListData[i].scriptId = 100000;
		}
		
		_ScrollMenu = ListMenu.GetComponent<ScrollMenberMenu>();
		
		_ScrollMenu.SetScrollData(TestListData, 0);
		_ScrollMenu.SetScrollView();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
