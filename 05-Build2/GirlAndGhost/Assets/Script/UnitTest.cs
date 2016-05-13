using UnityEngine;
using System.Collections;
using SmoothMoves;

public class UnitTest : MonoBehaviour {

	private BoneAnimation _boneAnimation;
	private float _baseValue;
	private bool _chestPlus = false;
	private bool _prevType = false;
	private bool _currentType = false;
	// Use this for initialization
	void Start () {
		GameObject prefab = Resources.Load ("character/Unit") as GameObject;
		GameObject bullet = MonoBehaviour.Instantiate (prefab) as GameObject;

		_boneAnimation = bullet.GetComponent<BoneAnimation> ();
		//setChestScale (1.05f);
		//setChestScale (0.95f);
		_baseValue = 0.95f;

		//_boneAnimation.SwapTexture ("AtlasChest", "left-chest", "AtlasChest", "left-chest-01");
		//_boneAnimation.SwapTexture ("AtlasChest", "right-chest", "AtlasChest", "right-chest-01");
		//_boneAnimation.SwapTexture ("AtlasSkirt", "skirt", "AtlasSkirt", "skirt-01");
		//_boneAnimation.SwapBoneTexture("Skirt", "AtlasSkirt", "skirt-01", "AtlasSkirt", "skirt-00");

		Color bodyColor = new Color ();
		bodyColor.a = 1f;
		bodyColor.r = 0.5f;
		bodyColor.g = 0f;
		bodyColor.b = 0f;
		//_boneAnimation.SetBoneColor ("Body", bodyColor, 0.2f);
		//_boneAnimation.SetBoneColor ("Left-Chest", bodyColor, 0.2f);
		//_boneAnimation.SetBoneColor ("Right-Chest", bodyColor, 0.2f);

		//SmoothMoves.Sprite sprite = new SmoothMoves.Sprite ();
	}

	private void setChestScale(float value) {

		Transform leftChestT = _boneAnimation.GetBoneTransform ("Left-Chest-T");

		leftChestT.localScale = new Vector3 (value, value, value);
		Transform rightChestT = _boneAnimation.GetBoneTransform ("Right-Chest-T");
		rightChestT.localScale = new Vector3 (value, value, value);
	}
	
	// Update is called once per frame
	void Update () {
		if (_baseValue >= 1.02f)
			_chestPlus = true;
		if (_baseValue < 0.90f)
			_chestPlus = false;
		/*
		if(_baseValue > 0.90f) {
			_boneAnimation.SwapTexture ("MainBody", "left-chest-s", "MainBody", "left-chest");
			_boneAnimation.SwapTexture ("MainBody", "right-chest-s", "MainBody", "right-chest");
			setChestScale (_baseValue);
		} else {
			_boneAnimation.SwapTexture ("MainBody", "left-chest", "MainBody", "left-chest-s");
			_boneAnimation.SwapTexture ("MainBody", "right-chest", "MainBody", "right-chest-s");
			setChestScale (_baseValue * 1.2f);
		}
		*/

		float value = -0.001f;
		if(!_chestPlus) {
			value = 0.001f;
		}
		_baseValue += value;
	}
}
