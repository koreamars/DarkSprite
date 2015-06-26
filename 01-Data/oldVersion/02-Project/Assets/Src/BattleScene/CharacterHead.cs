using UnityEngine;
using System.Collections;

public class CharacterHead : MonoBehaviour {

	public bool isTest;

	public GameObject BaseLip;
	public GameObject BaseEye;
	public GameObject BaseEyeBg;
	public GameObject BaseEyeline;
	public GameObject BaseHair;
	public GameObject BaseFaceMask;
	public GameObject BaseEyeLight;

	public Color FaceColor;
	public Color EyeColor;
	public Color LipColor;
	public Color HairColor;

	public int testHairNum = 1;

	private string colordata = "";

	void Start() {

		if(isTest) {
			init((short)(testHairNum));
			SetSortingNum(0);
		}
	}

	public void init(short defaultMemberId) {

		FaceData faceData = MemberData.getInstence().GetFaceDataById(defaultMemberId);
		if(faceData == null) return;

		Transform FrontHair = BaseHair.transform.FindChild("FrontHair");
		Transform BackHair = BaseHair.transform.FindChild("BackHair");
		Transform LightHair = BaseHair.transform.FindChild("LightHair");
		Transform Accessory = BaseHair.transform.FindChild("Accessory");

		SpriteTextureChange(BaseEye, "UnitResource/Head/Eye/" + faceData.EyeUri);
		SpriteTextureChange(BaseEyeBg, "UnitResource/Head/Eye/" + faceData.EyeUri + "Bg");
		SpriteTextureChange(BaseEyeLight, "UnitResource/Head/Eye/" + faceData.EyeUri + "Light");
		SpriteTextureChange(BaseEyeline, "UnitResource/Head/EyeLine/" + faceData.EyeLineUri);
		SpriteTextureChange(BaseLip, "UnitResource/Head/Mouth/" + faceData.LipUri);

		SpriteTextureChange(FrontHair.gameObject, "UnitResource/Head/Hair/" + faceData.HairUri + "/FrontHair");
		SpriteTextureChange(BackHair.gameObject, "UnitResource/Head/Hair/" + faceData.HairUri + "/BackHair");
		SpriteTextureChange(LightHair.gameObject, "UnitResource/Head/Hair/" + faceData.HairUri + "/LightHair");
		SpriteTextureChange(Accessory.gameObject, "UnitResource/Head/Hair/" + faceData.HairUri + "/Accessory");

		FaceColor = Color.white;
		FaceColor.r = faceData.FaceR;
		FaceColor.g = faceData.FaceG;
		FaceColor.b = faceData.FaceB;

		LipColor = Color.white;
		LipColor.r = faceData.LipR;
		LipColor.g = faceData.LipG;
		LipColor.b = faceData.LipB;

		EyeColor = Color.white;
		EyeColor.r = faceData.EyeR;
		EyeColor.g = faceData.EyeG;
		EyeColor.b = faceData.EyeB;

		HairColor = Color.white;
		HairColor.r = faceData.HairR;
		HairColor.g = faceData.HairG;
		HairColor.b = faceData.HairB;

		SetFaceColor(FaceColor);
		SetLipColor(LipColor);
		SetEyeColor(EyeColor);
		SetHairColor(HairColor);

		if(isTest) {
			if(faceData.BodyScaleType == 0) {
				this.gameObject.transform.position = new Vector2(0f, 0f);
				this.gameObject.transform.localScale = new Vector2(-1f, 1f);
			} else if (faceData.BodyScaleType == 1) {	// 작음.
				this.gameObject.transform.position = new Vector2(0f, -0.2f);
				this.gameObject.transform.localScale = new Vector2(-0.9f, 0.9f);
			} else {	// 큰.
				this.gameObject.transform.position = new Vector2(0f, 0.2f);
				this.gameObject.transform.localScale = new Vector2(-1.05f, 1.05f);
			}
		}
	}

	private void RandomFace() {
		FaceRandom();
		SetFaceColor(FaceColor);
		
		LipRandom();
		SetLipColor(LipColor);
		
		EyeRandom();
		SetEyeColor(EyeColor);
		
		HairRandom();
		SetHairColor(HairColor);
	}

	private void SetRandomColor() {
		SetFaceColor(FaceColor);
		SetLipColor(LipColor);
		SetEyeColor(EyeColor);
		SetHairColor(HairColor);
	}

	private void FaceRandom () {
		FaceColor = Color.white;
		float FaceRandom = UnityEngine.Random.Range(80, 100) / 100f;
		FaceColor.r = FaceRandom + 0.2f;
		FaceColor.g = FaceRandom;
		FaceColor.b = FaceRandom;
	}

	private void LipRandom () {
		LipColor = Color.white;
		float LipRandom = UnityEngine.Random.Range(80, 100) / 100f;
		LipColor.r = LipRandom + 0.2f;
		LipColor.g = LipRandom;
		LipColor.b = LipRandom;
	}

	private void EyeRandom () {
		EyeColor = Color.white;
		EyeColor.r = UnityEngine.Random.Range(0, 100) / 100f;
		EyeColor.g = UnityEngine.Random.Range(0, 100) / 100f;
		EyeColor.b = UnityEngine.Random.Range(0, 100) / 100f;
	}

	private void HairRandom () {
		HairColor = Color.white;
		HairColor.r = UnityEngine.Random.Range(0, 100) / 100f;
		HairColor.g = UnityEngine.Random.Range(0, 100) / 100f;
		HairColor.b = UnityEngine.Random.Range(0, 100) / 100f;
	}

	public void SetFaceColor(Color color) {
		SpriteColorChange(BaseFaceMask, color);

		colordata += "face : " + color.ToString() + "\n";
	}

	public void SetLipColor(Color color) {
		SpriteColorChange(BaseLip, color);
		colordata += "Lip : " + color.ToString() + "\n";
	}

	public void SetEyeColor(Color color) {
		SpriteColorChange(BaseEye, color);
		colordata += "Eye : " + color.ToString() + "\n";
	}

	public void SetHairColor(Color color) {

		Transform FrontHair = BaseHair.transform.FindChild("FrontHair");
		Transform BackHair = BaseHair.transform.FindChild("BackHair");
		//Transform LightHair = BaseHair.transform.FindChild("LightHair");

		SpriteColorChange(BaseEyeline, color);

		SpriteColorChange(FrontHair.gameObject, color);
		SpriteColorChange(BackHair.gameObject, color);

		colordata += "hair : " + color.ToString() + "\n";
	}

	public void SetSortingNum(int sortNum) {
		Transform FrontHair = BaseHair.transform.FindChild("FrontHair");
		Transform BackHair = BaseHair.transform.FindChild("BackHair");
		Transform LightHair = BaseHair.transform.FindChild("LightHair");
		Transform Accessory = BaseHair.transform.FindChild("Accessory");

		Accessory.GetComponent<Renderer>().sortingOrder = sortNum + 5;
		LightHair.GetComponent<Renderer>().sortingOrder = sortNum + 4;
		FrontHair.GetComponent<Renderer>().sortingOrder = sortNum + 3;
		BackHair.GetComponent<Renderer>().sortingOrder = sortNum;

		BaseFaceMask.GetComponent<Renderer>().sortingOrder = sortNum + 2;
		BaseLip.GetComponent<Renderer>().sortingOrder = sortNum + 4;
		BaseEye.GetComponent<Renderer>().sortingOrder = sortNum + 5;
		BaseEyeBg.GetComponent<Renderer>().sortingOrder = sortNum + 4;
		BaseEyeline.GetComponent<Renderer>().sortingOrder = sortNum + 4;
		BaseEyeLight.GetComponent<Renderer>().sortingOrder = sortNum + 6;
	}

	private void SpriteTextureChange(GameObject sprite, string uri) {
		SpriteRenderer renderer = sprite.GetComponent<SpriteRenderer>();
		if(renderer == null) return;
		renderer.sprite = Resources.Load<Sprite>(uri);
		if(renderer.sprite == null) return;
		Texture2D tex = renderer.sprite.texture;
	}

	private void SpriteColorChange(GameObject sprite, Color color) {
		SpriteRenderer renderer = sprite.GetComponent<SpriteRenderer>();
		if(color.a == 0) color.a = 1;
		renderer.color = color;
	}

	void OnGUI() {
		if(isTest) {
			if(GUI.Button(new Rect(0, 0, 120, 30), "changeColor")) {
				colordata = "";
				RandomFace();
			}

			if(GUI.Button(new Rect(0, 40, 120, 30), "HairColor")) {
				colordata = "";
				HairRandom();
				SetRandomColor();
			}

			if(GUI.Button(new Rect(0, 80, 120, 30), "LipColor")) {
				colordata = "";
				LipRandom();
				SetRandomColor();
			}

			if(GUI.Button(new Rect(0, 120, 120, 30), "FaceColor")) {
				colordata = "";
				FaceRandom();
				SetRandomColor();
			}

			if(GUI.Button(new Rect(0, 160, 120, 30), "EyeColor")) {
				colordata = "";
				EyeRandom();
				SetRandomColor();
			}

			if(GUI.Button(new Rect(0, 200, 120, 30), "UpdateColor")) {
				colordata = "";
				SetRandomColor();
			}



			GUI.TextArea(new Rect(0, Screen.height - 140f, Screen.width, 140f), colordata);
		}

	}
}
