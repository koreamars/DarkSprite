using UnityEngine;
using System.Collections;

public class ResearchSlot : MonoBehaviour {

	public GameObject SlotBg;
	public GameObject Thumbnail;
	public GameObject TitleText;
	public GameObject TitleTextBg;
	public GameObject BlackBg;

	private Research _Research;
	private int _BtnId;

	public delegate void BtnCallBack(int id);
	private BtnCallBack _Callback;
	public delegate void DragCallBack(int id, Vector2 move);
	private DragCallBack _DragCallBack;

	private Vector2 _MouseMove;
	private bool _IsUnlock;
	private bool _IsSelect;

	private Color _DefaultColor;
	private Color _ThumbAlpha;

	void Awake() {
		_DefaultColor = Color.white;
		_ThumbAlpha = Color.white;
	}

	public void init(int btnId, Research research, byte sortNum) {
		_BtnId = btnId;
		_Research = research;
		BlackBg.GetComponent<Renderer>().sortingOrder = sortNum;
		SlotBg.GetComponent<Renderer>().sortingOrder = sortNum + 3;
		Thumbnail.GetComponent<Renderer>().sortingOrder = sortNum + 1;
		TitleText.GetComponent<Renderer>().sortingOrder = sortNum + 3;
		TitleTextBg.GetComponent<Renderer>().sortingOrder = sortNum + 2;
		SlotBg.GetComponent<Renderer>().material.color = _DefaultColor;
		UpdateSlot();
		SpriteRenderer renderer = (SpriteRenderer)Thumbnail.GetComponent ("SpriteRenderer");
		Gear gear = GearData.getInstence().GetGearByID(_Research.gearId);
		if(gear != null) {
			renderer.sprite = Resources.Load<Sprite>("UnitResource/Thumbnail/" + gear.thumbnailURI);
		} else {
			renderer.sprite = Resources.Load<Sprite>("MainScene/Research/Research-CoreMake");
			TitleText.GetComponent<TextMesh>().text = ScriptData.getInstence().GetGameScript(160152).script;
		}
		//160152

	}

	public void SetClick(BtnCallBack onCallBack) {
		_Callback = new BtnCallBack(onCallBack);
	}

	public void SetDragCallBack(DragCallBack onDragCallBack) {
		_DragCallBack = new DragCallBack(onDragCallBack);
		
	}

	public Research GetResearch() {
		return _Research;
	}

	public void UpdateSlot() {
		Color textColor = Color.white;
		Gear gear = GearData.getInstence().GetGearByID(_Research.gearId);
		if(UserData.getInstence().ResearchLevel < _Research.unlockLevel) {
			TitleText.GetComponent<TextMesh>().text = ScriptData.getInstence().GetGameScript(160126).script;
			textColor.r = 1f;
			textColor.g = 0.5f;
			textColor.b = 0f;
			_IsUnlock = false;
			_DefaultColor = Color.gray;
			_ThumbAlpha.a = 0.5f;
		} else {
			if(gear != null) {
				UserResearch userResearch = UserData.getInstence().GetUserResearchByReserch(_Research);
				if(userResearch != null) {
					if(userResearch.isComplete) {
						//TitleText.GetComponent<TextMesh>().text = ScriptData.getInstence().GetGameScript(gear.scriptId).script;
						TitleText.GetComponent<TextMesh>().text = ScriptData.getInstence().GetGameScript(150107).script;
						_DefaultColor.r = 1f;
						_DefaultColor.g = 0.4f;
						_DefaultColor.b = 0f;
					} else {
						TitleText.GetComponent<TextMesh>().text = ScriptData.getInstence().GetGameScript(gear.scriptId).script;
						_DefaultColor.r = 1f;
						_DefaultColor.g = 0.9f;
						_DefaultColor.b = 0.6f;
					}
				} else {
					TitleText.GetComponent<TextMesh>().text = ScriptData.getInstence().GetGameScript(gear.scriptId).script;
				}
				
				_ThumbAlpha.a = 1f;
				
				_IsUnlock = true;
			} else {

			}

		}
		TitleText.GetComponent<TextMesh>().color = textColor;
		SlotBg.GetComponent<Renderer>().material.color = _DefaultColor;
		SpriteRenderer renderer = (SpriteRenderer)Thumbnail.GetComponent ("SpriteRenderer");
		renderer.color = _ThumbAlpha;
	}

	public void SelectSlot(bool isSelect) {
		Color color = Color.white;
		if(_IsUnlock == false && _BtnId > 0) return;

		_IsSelect = isSelect;
		if(isSelect) {
			color.r = 1f;
			color.g = 0.5f;
			color.b = 1f;
			SlotBg.GetComponent<Renderer>().material.color = color;
			//color.a = 1;
		} else {
			SlotBg.GetComponent<Renderer>().material.color = _DefaultColor;
			//color.a = 0;
		}

	}

	void OnMouseDown () {
		_MouseMove = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
	}

	void OnMouseUp () {
		if(_Callback != null) _Callback(_BtnId);
	}

	void OnMouseDrag () {
		Vector2 CurrentMove = new Vector2(_MouseMove.x - Input.mousePosition.x, _MouseMove.y - Input.mousePosition.y);
		if(_DragCallBack != null) _DragCallBack(_BtnId, CurrentMove);
	}

}
