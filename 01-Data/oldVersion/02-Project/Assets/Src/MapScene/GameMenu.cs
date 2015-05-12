using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour {

	public GameObject HQBtn;
	public GameObject CommoBtn;
	public GameObject ResearchBtn;
	public GameObject FactoryBtn;
	public GameObject CorpsBtn;
	public GameObject HangarBtn;

	private GameObject _TaxSymbol;

	public delegate void MenuCallBack(short menuNum);
	private MenuCallBack _MenuCallback;

	void Start() {
		HQBtn.GetComponent<ButtonEvent>().SetCallBack(OnHQClick);
		CommoBtn.GetComponent<ButtonEvent>().SetCallBack(OnCommoClick);
		ResearchBtn.GetComponent<ButtonEvent>().SetCallBack(OnResearchClick);
		FactoryBtn.GetComponent<ButtonEvent>().SetCallBack(OnFactoryClick);
		CorpsBtn.GetComponent<ButtonEvent>().SetCallBack(OnCorpsClick);
		HangarBtn.GetComponent<ButtonEvent>().SetCallBack(OnHangarClick);

	}

	public void DataUpdate() {

		CommoBtn.renderer.enabled = true;
		CommoBtn.GetComponent<BoxCollider2D>().enabled = true;
		ResearchBtn.renderer.enabled = true;
		ResearchBtn.GetComponent<BoxCollider2D>().enabled = true;
		FactoryBtn.renderer.enabled = true;
		FactoryBtn.GetComponent<BoxCollider2D>().enabled = true;
		CorpsBtn.renderer.enabled = true;
		CorpsBtn.GetComponent<BoxCollider2D>().enabled = true;
		HangarBtn.renderer.enabled = true;
		HangarBtn.GetComponent<BoxCollider2D>().enabled = true;

		int storyStep = UserData.getInstence().StoryStepId;
		
		if(storyStep < 3) {	// 작전 지휘소.
			CommoBtn.renderer.enabled = false;
			CommoBtn.GetComponent<BoxCollider2D>().enabled = false;
		}
		if(storyStep < 5) {	// 연구소.
			ResearchBtn.renderer.enabled = false;
			ResearchBtn.GetComponent<BoxCollider2D>().enabled = false;
		}
		if(storyStep < 7) {	// 공장.
			FactoryBtn.renderer.enabled = false;
			FactoryBtn.GetComponent<BoxCollider2D>().enabled = false;
		}
		if(storyStep < 14) {	// 부대.
			CorpsBtn.renderer.enabled = false;
			CorpsBtn.GetComponent<BoxCollider2D>().enabled = false;
		}
		if(storyStep < 9) {	// 행거.
			HangarBtn.renderer.enabled = false;
			HangarBtn.GetComponent<BoxCollider2D>().enabled = false;
		}

		if(UserData.getInstence().StoryStepId > 12) SetTownTax();

	}

	public void SetTownTax() {
		if(_TaxSymbol == null) {
			_TaxSymbol = new GameObject();
			_TaxSymbol.AddComponent<SpriteRenderer>();
			_TaxSymbol.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("MainScene/MainUI/TaxSymbol");
			_TaxSymbol.transform.position = new Vector2(HQBtn.transform.position.x, HQBtn.transform.position.y + 1.4f);
			_TaxSymbol.transform.parent = this.gameObject.transform;
		}
		if(UserData.getInstence().IsGetTax == true) {
			_TaxSymbol.renderer.enabled = true;
		} else {
			_TaxSymbol.renderer.enabled = false;
		}
	}

	private void OnHQClick() {
		//GameLog.Log("OnHQClick");
		if(_MenuCallback != null) _MenuCallback(MainPopupType.HQPopup);
	}

	private void OnCommoClick() {
		//GameLog.Log("OnCommoClick");
		if(_MenuCallback != null) _MenuCallback(MainPopupType.CommoPopup);
	}

	private void OnResearchClick() {
		//GameLog.Log("OnResearchClick");
		if(_MenuCallback != null) _MenuCallback(MainPopupType.ResearchPopup);
	}

	private void OnFactoryClick() {
		//GameLog.Log("OnFactoryClick");
		if(_MenuCallback != null) _MenuCallback(MainPopupType.FactoryPopup);
	}

	private void OnCorpsClick() {
		//GameLog.Log("OnCorpsClick");
		if(_MenuCallback != null) _MenuCallback(MainPopupType.CorpsPopup);
	}

	private void OnHangarClick() {
		//GameLog.Log("OnCorpsClick");
		if(_MenuCallback != null) _MenuCallback(MainPopupType.HangarPopup);
	}

	public void SetCallBack(MenuCallBack onMenuCallBack) {
		_MenuCallback = new MenuCallBack(onMenuCallBack);

	}


}
