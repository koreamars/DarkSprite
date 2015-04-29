using UnityEngine;
using System.Collections;

public class TextureMargeTest : MonoBehaviour {

	// Use this for initialization
	void Start () {

		GameObject testobj = new GameObject();
		//Sprite testSprite = Resources.Load<Sprite>("UnitResource/Suit/nudebody/body0");

		Texture2D Test1 = Resources.Load<Texture2D>("UnitResource/Suit/nudebody/body0") as Texture2D;
		Texture2D Test2 = Resources.Load<Texture2D>("UnitResource/Suit/Suit04/body0") as Texture2D;

		Texture2D testTexture = TextureMarge.SetTextureMarge(Test1, Test2);

		Sprite testSprite = Sprite.Create(testTexture, new Rect(0, 0, testTexture.width, testTexture.height), new Vector2(0.5f, 0.5f));

		testobj.AddComponent<SpriteRenderer>().sprite = testSprite;
	}

}
