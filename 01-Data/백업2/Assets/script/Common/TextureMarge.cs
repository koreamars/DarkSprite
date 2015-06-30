using UnityEngine;
using System.Collections;

public class TextureMarge : MonoBehaviour {

	public static Texture2D SetTextureMarge(Texture2D texture1, Texture2D texture2, Vector2 startPos) {

		Texture2D merge = new Texture2D(texture1.width, texture1.height);
		string _test = "";
		int startX = (int)startPos.x;
		int startY = (int)startPos.y;
		int endX = startX + texture2.width;
		int endY = startY + texture2.height;

		bool _testBool = false;
		for (int y = 0; y < texture1.height; y++)
		{
			for (int x = 0; x < texture1.width; x++)
			{
				Color one = texture1.GetPixel(x, y);
				if(x >= startX && y >= startY && x < endX && y < endY) {
					Color two = texture2.GetPixel(x - startX, y - startY);
					if(_testBool == false) {
						_testBool = true;
					}
					if(two.a > 0) {
						Color mergeColor = new Color();
						mergeColor.a = one.a + two.a;
						if(two.a < 0.2f) {
							mergeColor.r = (one.r * one.a) + (two.r * two.a);
							mergeColor.g = (one.g * one.a) + (two.g * two.a);
							mergeColor.b = (one.b * one.a) + (two.b * two.a);
						} else {
							mergeColor.r = two.r;
							mergeColor.g = two.g;
							mergeColor.b = two.b;
						}
						merge.SetPixel(x, y, mergeColor);
					} else {
						merge.SetPixel(x, y, one);
					}
				} else {
					merge.SetPixel(x, y, one);
				}
			}
		}

		merge.Apply();

		return merge;
	}

	public static Texture2D SetChangeColor(Texture2D targetTexture, Color color) {

		Texture2D merge = new Texture2D(targetTexture.width, targetTexture.height);

		for (int y = 0; y < targetTexture.height; y++)
		{
			for (int x = 0; x < targetTexture.width; x++)
			{
				Color one = targetTexture.GetPixel(x, y);
				if(one.a > 0) {
					Color newColor = Color.white;
					newColor.r = ColorMerge(one.r, color.r);
					newColor.g = ColorMerge(one.g, color.g) - 0.05f;
					newColor.b = ColorMerge(one.b, color.b);
					//newColor.r = color.r;
					//newColor.g = color.g;
					//newColor.b = color.b;
					newColor.a = one.a;
					merge.SetPixel(x, y, newColor);
				} else {
					merge.SetPixel(x, y, one);
				}

			}
		}
		
		merge.Apply();
		
		return merge;
	}

	private static float ColorMerge(float baseValue, float updateValue) {
		float retureValue = 0f;
		float updateV = 1f - updateValue;
		//if(baseValue > 1f) baseValue = 1f;
		retureValue = baseValue - updateV;
		if(retureValue < 0f) retureValue = 0f;

		return retureValue;
	}
}
