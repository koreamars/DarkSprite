using UnityEngine;
using System.Collections;

public class UnitDataBox : MonoBehaviour {

	public bool isTest;
	public GameObject ImgObject;
	public GameObject SelectBox;
	public GameObject UnitFrame;
	public GameObject TargetMark;
	public GameObject TurnNumMark;
	public Color OutLineColor;
	public Color TurnColor1;
	public Color TurnColor2;
	public Color TurnColor3;
	public Color TurnOutColor;
	public Color HpGreenColor;
	public Color HpYerrowColor;
	public Color HpRedColor;
	public Color AIColor;

	public byte id;

	public UnitDataBoxModel Model;

	public delegate void SlotCallback(byte id);
	private SlotCallback _SlotCallback;

	private GameObject HPCount;
	private GameObject AICount;
	private GameObject _LedBar;
	private GameObject _TurnCount;

	private short _SortNum = 0;

	void Awake() {
		if(isTest) {
			UnitDataBoxModel model = new UnitDataBoxModel();
			model.id = 0;
			model.modelId = 1;
			model.imgName = "MemberImg/Member001";
			model.ActNum = 33;
			model.currentHP = 999;
			model.currentMP = 999;
			model.MaxActNum = 99;
			model.maxHP = 999;
			model.maxMP = 999;
			model.type = 0;
			model.turnIndex = 0;
			init(model, 0, SetTestCallback);
			update(model);
		}
	}

	private void SetTestCallback(byte id) {

	}

	public void SortingNum(short num) {
		SelectBox.GetComponent<Renderer>().sortingOrder = num;
		ImgObject.GetComponent<Renderer>().sortingOrder = num + 2;
		UnitFrame.GetComponent<Renderer>().sortingOrder = num + 1;
		TargetMark.GetComponent<Renderer>().sortingOrder = num + 4;
		_LedBar.GetComponent<Renderer>().sortingOrder = num + 2;
		AICount.GetComponent<Renderer>().sortingOrder = num + 3;
		TurnNumMark.GetComponent<Renderer>().sortingOrder = num + 3;
		HPCount.GetComponent<Renderer>().sortingOrder = num + 3;
		_TurnCount.GetComponent<Renderer>().sortingOrder = num + 4;

		_SortNum = num;

		SelectBox.GetComponent<Renderer>().enabled = false;
	}

	public void init(UnitDataBoxModel model, short sortNum, SlotCallback OnSlotCallback) {

		_SlotCallback = new SlotCallback(OnSlotCallback);

		_LedBar = this.gameObject.transform.FindChild("LedBar").gameObject;
		_LedBar.GetComponent<Renderer>().material.color = Color.black;

		Model = model;
		if(model.isSelect){
			SlotSelect(true);
		} else {
			SlotSelect(false);
		}

		TargetMark.transform.position = this.gameObject.transform.position;

		Color thumbColor = Color.white;
		if(model.isOut == true) {
			thumbColor.a = 0.5f;
		} else {
			thumbColor.a = 1f;
		}
		ImgChange(model.imgName);
		ImgObject.GetComponent<Renderer>().material.color = thumbColor;

		if(AICount == null) {
			if(model.type == 0) {
				AICount = CustomTextMesh.SetAddTextMesh("", 14, TextAnchor.MiddleLeft, AIColor, 0, "UI");
				AICount.transform.position = new Vector2(this.gameObject.transform.position.x + 0.68f, this.gameObject.transform.position.y + -0.24f);
			} else {
				AICount = CustomTextMesh.SetAddTextMesh("", 14, TextAnchor.MiddleRight, AIColor, 0, "UI");
				AICount.transform.position = new Vector2(this.gameObject.transform.position.x - 0.68f, this.gameObject.transform.position.y + -0.24f);
			}
			AICount.transform.parent = this.gameObject.transform;
		}

		if(HPCount == null) {
			if(model.type == 0) {
				HPCount = CustomTextMesh.SetAddTextMesh("", 15, TextAnchor.MiddleLeft, Color.white, 0, "UI"); 
				HPCount.transform.position = new Vector2(this.gameObject.transform.position.x + 0.68f, this.gameObject.transform.position.y + -0.54f);
			} else {
				HPCount = CustomTextMesh.SetAddTextMesh("", 15, TextAnchor.MiddleRight, Color.white, 0, "UI"); 
				HPCount.transform.position = new Vector2(this.gameObject.transform.position.x - 0.68f, this.gameObject.transform.position.y + -0.54f);
			}
			HPCount.transform.parent = this.gameObject.transform;
		}

		if(_TurnCount == null) {
			_TurnCount = CustomTextMesh.SetAddTextMesh("", 15, TextAnchor.MiddleCenter, Color.white, 0, "UI"); 
			_TurnCount.GetComponent<TextMesh>().fontStyle = FontStyle.BoldAndItalic;
			_TurnCount.transform.parent = this.gameObject.transform;
			if(model.type == 0) {
				_TurnCount.transform.position = new Vector2(this.gameObject.transform.position.x + 0.52f, this.gameObject.transform.position.y + 0.58f);
			} else {
				_TurnCount.transform.position = new Vector2(this.gameObject.transform.position.x - 0.58f, this.gameObject.transform.position.y + 0.58f);
			}

		}

		//ShowActCount(model.ActNum, model.MaxActNum);
		SortingNum(sortNum);
		TargetMark.GetComponent<Renderer>().enabled = false;

	}

	public void update(UnitDataBoxModel model) {
		Model = model;
		if(model.isSelect){
			SlotSelect(true);
		} else {
			SlotSelect(false);
		}
		Color thumbColor = Color.white;
		if(model.isOut == true) {
			thumbColor.a = 0.5f;
		} else {
			thumbColor.a = 1f;
		}
		ImgChange(model.imgName);
		ImgObject.GetComponent<Renderer>().material.color = thumbColor;
		ShowActCount(model.ActNum, model.MaxActNum);

		byte currentTurn = (byte)(model.turnIndex + 1);
		if(model.turnIndex == 0) {
			TurnNumMark.GetComponent<Renderer>().material.color = TurnColor1;
		} else if (model.turnIndex > 0 && model.turnIndex <= 2) {
			TurnNumMark.GetComponent<Renderer>().material.color = TurnColor2;
		} else {
			TurnNumMark.GetComponent<Renderer>().material.color = TurnColor3;
		}
		_TurnCount.GetComponent<TextMesh>().text = currentTurn.ToString();
		//AICount.transform.position = new Vector2(-0.51f, -0.28f);
		//HPCount.transform.position = new Vector2(-0.51f, -0.54f);

		TurnNumMark.transform.position = new Vector2(_TurnCount.transform.position.x + 0.05f, _TurnCount.transform.position.y + 0.02f);

		TargetMark.GetComponent<Renderer>().enabled = false;
		SetProgress(model.currentHP);

		if(model.isTarget) {
			TargetMark.GetComponent<Renderer>().enabled = true;
		} else {
			TargetMark.GetComponent<Renderer>().enabled = false;
		}
	}

	private void ShowActCount(int actCount, int maxCount) {

		if(actCount < 10000) {
			AICount.GetComponent<TextMesh>().text = actCount + "/" + maxCount;
		} else {
			AICount.GetComponent<TextMesh>().text = "";
		}
	}

	private void SetProgress(int current) {
		//HPCount.GetComponent<OutLineFont>().SetString(current.ToString());

		HPCount.GetComponent<TextMesh>().text = current + "/" + Model.maxHP;
		_LedBar.transform.localScale = new Vector2(1.16f * ((float)(current) / (float)(Model.maxHP)), 0.24f);
		Color textColor;
		if(current > 60) {
			textColor = HpGreenColor;
			_LedBar.GetComponent<Renderer>().material.color = HpGreenColor;
		} else if(current <= 60 && current > 20) {
			textColor = HpYerrowColor;
			_LedBar.GetComponent<Renderer>().material.color = HpYerrowColor;
		} else {
			textColor = HpRedColor;
			_LedBar.GetComponent<Renderer>().material.color = HpRedColor;
		}

		textColor.r -= 0.4f;
		textColor.g -= 0.4f;
		textColor.b -= 0.4f;
		HPCount.GetComponent<TextMesh>().color = textColor;
	}

	private void ImgChange(string imgName) {
		SpriteRenderer renderer = (SpriteRenderer)ImgObject.GetComponent ("SpriteRenderer");
		renderer.sprite = Resources.Load<Sprite>(imgName);
	}

	private void SlotSelect(bool isSelect) {
		if(isSelect){
			SelectBox.GetComponent<Renderer>().enabled = true;
			//Bg.renderer.enabled = false;
		} else {
			SelectBox.GetComponent<Renderer>().enabled = false;
			//Bg.renderer.enabled = true;
		}

	}

	public void TargetSelect() {
		TargetMark.GetComponent<Renderer>().enabled = true;
	}

	void OnMouseDown() {
		if(Model != null) {
			if(Model.type == 1 && Model.isOut == false) {
				if(_SlotCallback != null) _SlotCallback(Model.id);
			}
			
		}

	}


}
