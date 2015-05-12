using UnityEngine;
using System.Collections;
using SmoothMoves;

public class BoneTest : MonoBehaviour {

	// Use this for initialization
	public GameObject thePrefab;

	void Start () {
		//GameObject prefab = Resources.Load ("boneTest") as GameObject;
		// Resources/Prefabs/Bullet.prefab 로드
		for (byte i = 0; i < 6; i ++) {
			GameObject testBone = (GameObject) Instantiate(thePrefab);
			testBone.transform.position = new Vector2 (((i * 5f) - 13f), 3.5f);
			BoneAnimation ani = testBone.GetComponent<BoneAnimation>();
			ani.CrossFade("Idle");
			//Transform aniTransform = ani.GetSpriteTransform("Body") as Transform;
			ani.SetBoneColor("Body", Color.black, 0.5f);
			int clipIdx = ani.GetAnimationClipIndex("Idle");
			ani.SwapAnimationBoneTexture(clipIdx, "skirt", "New Atlas", "skirt", "New Atlas 1", "skirtTest");
		}
	}

}
