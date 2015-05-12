using UnityEngine;
using System.Collections;

public class DarkSprite : MonoBehaviour {

	private static DarkSprite _instence;

	public GameObject GameDataView;
	public MainMap MainMapData;
	public MainScene MainScene;

	public static DarkSprite getInstence(){
		if(_instence == null) 
		{
			//_instence = new DataController();
			GameObject instence = new GameObject();
			_instence = instence.AddComponent<DarkSprite>();
		}
		
		return _instence;
	}


}
