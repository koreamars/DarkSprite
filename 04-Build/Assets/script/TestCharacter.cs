using UnityEngine;
using System.Collections;

public class TestCharacter : MonoBehaviour {

	private CharacterManager _characterManager;
	// Use this for initialization
	void Start () {
		_characterManager = CharacterManager.GetInstance ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
