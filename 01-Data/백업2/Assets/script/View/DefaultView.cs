using UnityEngine;
using System.Collections;

public class DefaultView : MonoBehaviour {

	public delegate void ShowEndCall();
	public ShowEndCall showEndCall = null;

	protected UnitModel currentModel;

	public virtual void SetModel(UnitModel model) {
		currentModel = model;
	}

	public virtual void Show(ShowEndCall endcall) {
		showEndCall = endcall;
	}

	public virtual void Hide() {
	}

	public virtual void Destory() {
	}
}
