using UnityEngine;
using System.Collections;
using DragonBones.Animation;

public class TestCharacter : MonoBehaviour {

	private CharacterController _characterController;
	private byte _currentIndex = 0;
	// Use this for initialization
	void Start () {
		_characterController = CharacterController.GetInstance ();

		StartCoroutine (showCharater());

	}

	public IEnumerator showCharater() {
		
		yield return StartCoroutine(_characterController.Init());

		CharacterModel model = new CharacterModel ();
		GameObject characterObj = _characterController.ShowCharacterView (model, ShowEndCall);

	}

	private void ShowEndCall(byte currentIndex) {
		print ("currentIndex : " + currentIndex);
		GameObject characterObj = _characterController.GetCharacterView (currentIndex);
		print ("chracterObj : " + characterObj);
		characterObj.transform.position = new Vector3 (0, 2, 0);

		CharacterModel model = new CharacterModel ();
		_characterController.UpdateCharacterView (currentIndex, model, UpdateEnd);
	}

	private void UpdateEnd(byte currentIndex) {
		print ("update Index : " + currentIndex);
	}

}
