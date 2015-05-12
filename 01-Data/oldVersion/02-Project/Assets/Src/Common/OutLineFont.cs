using UnityEngine;
using System.Collections;

public class OutLineFont : MonoBehaviour {

	public bool isTest;

	private GameObject _MainTextField;
	private GameObject _LineTextField1;
	private GameObject _LineTextField2;
	private GameObject _LineTextField3;
	private GameObject _LineTextField4;
	private GameObject _LineTextField5;
	private GameObject _LineTextField6;
	private GameObject _LineTextField7;
	private GameObject _LineTextField8;

	private float _OutlineSize = 1f;
	private Color _MainColor = Color.white;
	private Color _LineColor = Color.black;
	private int _SortNumber = 0;
	private TextAnchor _FontAnchor = TextAnchor.MiddleCenter;
	private string _TextStr = "";
	private byte _FontSize = 12;

	// Use this for initialization
	void Start () {
		if(isTest) {
			SetString("outlinefont");
		}
	}

	public void SetString(string text) {
		if(_MainTextField == null) CreateFont();
		SetSort(_SortNumber);
		SetFontColor(_MainColor);
		SetLineColor(_LineColor);
		SetLineSize(_OutlineSize);
		SetAlign(_FontAnchor);
		SetFontSize(_FontSize);

		_TextStr = text;

		_MainTextField.GetComponent<TextMesh>().text = _TextStr;
		_LineTextField1.GetComponent<TextMesh>().text = _TextStr;
		_LineTextField2.GetComponent<TextMesh>().text = _TextStr;
		_LineTextField3.GetComponent<TextMesh>().text = _TextStr;
		_LineTextField4.GetComponent<TextMesh>().text = _TextStr;
		_LineTextField5.GetComponent<TextMesh>().text = _TextStr;
		_LineTextField6.GetComponent<TextMesh>().text = _TextStr;
		_LineTextField7.GetComponent<TextMesh>().text = _TextStr;
		_LineTextField8.GetComponent<TextMesh>().text = _TextStr;
	}

	public void SetFontSize(byte size) {
		if(_MainTextField == null) CreateFont();
		_FontSize = size;
		_MainTextField.GetComponent<TextMesh>().fontSize  = _FontSize;
		_LineTextField1.GetComponent<TextMesh>().fontSize = _FontSize;
		_LineTextField2.GetComponent<TextMesh>().fontSize = _FontSize;
		_LineTextField3.GetComponent<TextMesh>().fontSize = _FontSize;
		_LineTextField4.GetComponent<TextMesh>().fontSize = _FontSize;
		_LineTextField5.GetComponent<TextMesh>().fontSize = _FontSize;
		_LineTextField6.GetComponent<TextMesh>().fontSize = _FontSize;
		_LineTextField7.GetComponent<TextMesh>().fontSize = _FontSize;
		_LineTextField8.GetComponent<TextMesh>().fontSize = _FontSize;
	}

	public void SetSortLayer(string layerName) {
		if(_MainTextField == null) CreateFont();
		_MainTextField.layer = LayerMask.NameToLayer(layerName);
		_LineTextField1.layer = LayerMask.NameToLayer(layerName);
		_LineTextField2.layer = LayerMask.NameToLayer(layerName);
		_LineTextField3.layer = LayerMask.NameToLayer(layerName);
		_LineTextField4.layer = LayerMask.NameToLayer(layerName);
		_LineTextField5.layer = LayerMask.NameToLayer(layerName);
		_LineTextField6.layer = LayerMask.NameToLayer(layerName);
		_LineTextField7.layer = LayerMask.NameToLayer(layerName);
		_LineTextField8.layer = LayerMask.NameToLayer(layerName);
	}

	/** 텍스트 뎁스를 소팅합니다 */
	public void SetSort(int sortNum) {
		if(_MainTextField == null) CreateFont();
		_SortNumber = sortNum;
		_MainTextField.renderer.sortingOrder = _SortNumber + 1;
		_LineTextField1.renderer.sortingOrder = _SortNumber;
		_LineTextField2.renderer.sortingOrder = _SortNumber;
		_LineTextField3.renderer.sortingOrder = _SortNumber;
		_LineTextField4.renderer.sortingOrder = _SortNumber;
		_LineTextField5.renderer.sortingOrder = _SortNumber;
		_LineTextField6.renderer.sortingOrder = _SortNumber;
		_LineTextField7.renderer.sortingOrder = _SortNumber;
		_LineTextField8.renderer.sortingOrder = _SortNumber;
	}

	/** 폰트 컬러 설정 */
	public void SetFontColor(Color fontColor) {
		if(_MainTextField == null) CreateFont();
		_MainColor = fontColor;
		_MainTextField.GetComponent<TextMesh>().color = _MainColor;
	}

	/** 라인 컬러 설정 */
	public void SetLineColor(Color lineColor) {
		if(_MainTextField == null) CreateFont();
		_LineColor = lineColor;
		_LineTextField1.GetComponent<TextMesh>().color = _LineColor;
		_LineTextField2.GetComponent<TextMesh>().color = _LineColor;
		_LineTextField3.GetComponent<TextMesh>().color = _LineColor;
		_LineTextField4.GetComponent<TextMesh>().color = _LineColor;
		_LineTextField5.GetComponent<TextMesh>().color = _LineColor;
		_LineTextField6.GetComponent<TextMesh>().color = _LineColor;
		_LineTextField7.GetComponent<TextMesh>().color = _LineColor;
		_LineTextField8.GetComponent<TextMesh>().color = _LineColor;
	}

	/** 아웃 라인 설정 */
	public void SetLineSize(float lineSize) {
		if(_MainTextField == null) CreateFont();
		_OutlineSize = lineSize;
		float thisLineSize = _OutlineSize / 50f;

		float posX = _MainTextField.transform.position.x;
		float posY = _MainTextField.transform.position.y;

		_LineTextField1.transform.position = new Vector3(posX, posY + thisLineSize, 0);
		_LineTextField2.transform.position = new Vector3(posX + thisLineSize, posY + thisLineSize, 0);
		_LineTextField3.transform.position = new Vector3(posX + thisLineSize, posY, 0);
		_LineTextField4.transform.position = new Vector3(posX + thisLineSize, posY - thisLineSize, 0);
		_LineTextField5.transform.position = new Vector3(posX, posY - thisLineSize, 0);
		_LineTextField6.transform.position = new Vector3(posX - thisLineSize, posY - thisLineSize, 0);
		_LineTextField7.transform.position = new Vector3(posX - thisLineSize, posY, 0);
		_LineTextField8.transform.position = new Vector3(posX - thisLineSize, posY + thisLineSize, 0);
	}

	public void SetAlign(TextAnchor textAnchor) {
		if(_MainTextField == null) CreateFont();
		_FontAnchor = textAnchor;
		_MainTextField.GetComponent<TextMesh>().anchor = _FontAnchor;
		_LineTextField1.GetComponent<TextMesh>().anchor = _FontAnchor;
		_LineTextField2.GetComponent<TextMesh>().anchor = _FontAnchor;
		_LineTextField3.GetComponent<TextMesh>().anchor = _FontAnchor;
		_LineTextField4.GetComponent<TextMesh>().anchor = _FontAnchor;
		_LineTextField5.GetComponent<TextMesh>().anchor = _FontAnchor;
		_LineTextField6.GetComponent<TextMesh>().anchor = _FontAnchor;
		_LineTextField7.GetComponent<TextMesh>().anchor = _FontAnchor;
		_LineTextField8.GetComponent<TextMesh>().anchor = _FontAnchor;
	}

	public void SetEnable(bool isEnable) {
		if(_MainTextField == null) CreateFont();
		_MainTextField.renderer.enabled = isEnable;
		_LineTextField1.renderer.enabled = isEnable;
		_LineTextField2.renderer.enabled = isEnable;
		_LineTextField3.renderer.enabled = isEnable;
		_LineTextField4.renderer.enabled = isEnable;
		_LineTextField5.renderer.enabled = isEnable;
		_LineTextField6.renderer.enabled = isEnable;
		_LineTextField7.renderer.enabled = isEnable;
		_LineTextField8.renderer.enabled = isEnable;
	}

	public float GetTextWidth() {
		return _MainTextField.renderer.bounds.size.x;
	}

	/** 텍스트 생성. */
	private void CreateFont() {
		if(_MainTextField == null) _MainTextField = Instantiate(Resources.Load<GameObject>("DefaultFont"), new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
		if(_LineTextField1 == null) _LineTextField1 = Instantiate(Resources.Load<GameObject>("DefaultFont"), new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
		if(_LineTextField2 == null) _LineTextField2 = Instantiate(Resources.Load<GameObject>("DefaultFont"), new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
		if(_LineTextField3 == null) _LineTextField3 = Instantiate(Resources.Load<GameObject>("DefaultFont"), new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
		if(_LineTextField4 == null) _LineTextField4 = Instantiate(Resources.Load<GameObject>("DefaultFont"), new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
		if(_LineTextField5 == null) _LineTextField5 = Instantiate(Resources.Load<GameObject>("DefaultFont"), new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
		if(_LineTextField6 == null) _LineTextField6 = Instantiate(Resources.Load<GameObject>("DefaultFont"), new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
		if(_LineTextField7 == null) _LineTextField7 = Instantiate(Resources.Load<GameObject>("DefaultFont"), new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
		if(_LineTextField8 == null) _LineTextField8 = Instantiate(Resources.Load<GameObject>("DefaultFont"), new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
		_MainTextField.transform.parent = this.gameObject.transform;
		_LineTextField1.transform.parent = this.gameObject.transform;
		_LineTextField2.transform.parent = this.gameObject.transform;
		_LineTextField3.transform.parent = this.gameObject.transform;
		_LineTextField4.transform.parent = this.gameObject.transform;
		_LineTextField5.transform.parent = this.gameObject.transform;
		_LineTextField6.transform.parent = this.gameObject.transform;
		_LineTextField7.transform.parent = this.gameObject.transform;
		_LineTextField8.transform.parent = this.gameObject.transform;

		_MainTextField.transform.localScale = new Vector3(1f, 1f, 1f);
		_LineTextField1.transform.localScale = new Vector3(1f, 1f, 1f);
		_LineTextField2.transform.localScale = new Vector3(1f, 1f, 1f);
		_LineTextField3.transform.localScale = new Vector3(1f, 1f, 1f);
		_LineTextField4.transform.localScale = new Vector3(1f, 1f, 1f);
		_LineTextField5.transform.localScale = new Vector3(1f, 1f, 1f);
		_LineTextField6.transform.localScale = new Vector3(1f, 1f, 1f);
		_LineTextField7.transform.localScale = new Vector3(1f, 1f, 1f);
		_LineTextField8.transform.localScale = new Vector3(1f, 1f, 1f);

	}


}
