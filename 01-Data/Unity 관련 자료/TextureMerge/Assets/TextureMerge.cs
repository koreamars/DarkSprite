using UnityEngine;
using System.Collections;

public class TextureMerge : MonoBehaviour
{
	public Texture2D texture1;
	public Texture2D texture2;
	public GameObject gameobject;

	void Start()
	{
		Texture2D merge = new Texture2D(512, 512);

		for (int y = 0; y < texture1.height; y++)
		{
			for (int x = 0; x < texture1.width; x++)
			{
				Color one = texture1.GetPixel(x, y);
				Color two = texture2.GetPixel(x, y);
				
				if( one.a != 0 )
					merge.SetPixel(x, y, one);
				if ( two.a != 0 )
					merge.SetPixel(x, y, two);
			}
	   }

		merge.Apply();

		gameobject.GetComponent<Renderer>().material.mainTexture = merge;
	}
}
