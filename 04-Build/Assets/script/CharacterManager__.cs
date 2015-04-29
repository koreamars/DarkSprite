using UnityEngine;
using System.Collections;

public class CharacterManager : MonoBehaviour {

	private static CharacterManager instance;  
	private static GameObject container;  
	public static CharacterManager GetInstance()  
	{  
		if( !instance )  
		{  
			container = new GameObject();  
			container.name = "CharacterManager";  
			instance = container.AddComponent(typeof(CharacterManager)) as CharacterManager;  
		}  
		return instance;  
	}
}
