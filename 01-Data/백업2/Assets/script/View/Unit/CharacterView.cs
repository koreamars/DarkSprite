using DragonBones;
using DragonBones.Factorys;
using DragonBones.Display;
using DragonBones.Animation;
using DragonBones.Objects;
using DragonBones.Textures;
using UnityEngine;
using System.Collections;

public class CharacterView : DefaultView {

	public UnityFactory unityFactory = null;

	private Armature _currentArmature;

	public override void Show (DefaultView.ShowEndCall endcall)
	{
		base.Show (endcall);

		StartCoroutine (showArmature ());
	}

	public override void Hide ()
	{
		base.Hide ();
	}

	public override void Destory ()
	{
		base.Destory ();
	}

	private IEnumerator showArmature () {			

		if (unityFactory == null) yield return new WaitForEndOfFrame ();


		if(_currentArmature != null) {
			GameObject delObject = ((_currentArmature.Display as UnityArmatureDisplay).Display as GameObject);
			Destroy(delObject);
		}
		_currentArmature = unityFactory.BuildArmature ("BaseUnitAni", null, "BaseUnitAni", "BaseUnitAni");
		_currentArmature.AdvanceTime (0f);
		WorldClock.Clock.Add (_currentArmature);
		_currentArmature.Animation.GotoAndPlay ("idle", -1, -1, 0);
		
		GameObject newObject = ((_currentArmature.Display as UnityArmatureDisplay).Display as GameObject);
		newObject.name = "Armature";
		newObject.transform.parent = this.transform;
		newObject.transform.position = new Vector3 (0, 0, 0);

		if (showEndCall != null)
			showEndCall ();
		
		yield return new WaitForEndOfFrame ();
	}

}
