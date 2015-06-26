using UnityEngine;
using System.Collections;

public class ClassMark : MonoBehaviour {

	public GameObject MarkState;
	public Vector2 MarkScale;
	public string ClassID;
	public int SortingNum;
	// Use this for initialization

	private GameObject _ClassMarkObj;

	void Start () {

		//if(ClassID == null) ClassID = "001";
		/*
		_ClassMarkObj = Instantiate(Resources.Load<GameObject>("ClassMark/ClassMark" + ClassID)) as GameObject;
		_ClassMarkObj.transform.parent = this.transform;

		_ClassMarkObj.transform.localScale = MarkScale;
		_ClassMarkObj.transform.position = MarkPosition;
		*/
		MarkState.transform.localScale = MarkScale;
		
		SpriteRenderer renderer = (SpriteRenderer)MarkState.GetComponent ("SpriteRenderer");
		if(renderer != null) {
			renderer.sortingOrder = SortingNum;
			renderer.sprite = Resources.Load<Sprite>("ClassMark/ClassMark" + ClassID);
		}
	}

	public void SetEnabled(bool state) {
		MarkState.GetComponent<Renderer>().enabled = state;
	}

	public void SetChangeClass(string id) {
		ClassID = id;
		SpriteRenderer renderer = (SpriteRenderer)MarkState.GetComponent ("SpriteRenderer");
		renderer.sprite = Resources.Load<Sprite>("ClassMark/ClassMark" + ClassID);

	}

	public void SetSortingNum(int num) {
		SortingNum = num;
		MarkState.GetComponent<Renderer>().sortingOrder = SortingNum;
	}

}
