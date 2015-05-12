using UnityEngine;
using System.Collections;

public class ButtonEvent : MonoBehaviour {

	public delegate void BtnCallBack();
	public delegate void DragCallBack(float dragX, float dragY);

	private BtnCallBack _Callback;
	private DragCallBack _DragCallBack;


	public void SetCallBack(BtnCallBack onCallBack) {
		_Callback = new BtnCallBack(onCallBack);

		/*
		public void setYYEndFunc(yyEndFunc onYYEndFunc) {
			yyEndFunction = new yyEndFunc(onYYEndFunc);
		}
		*/
	}

	public void SetDragCallBack(DragCallBack onDragCallBack) {
		_DragCallBack = new DragCallBack(onDragCallBack);

	}

	void OnMouseDown() {
		print("OnMouseDown");
		if(_Callback != null) _Callback();
	}

	void OnMouseDrag() {
		if(_DragCallBack != null) _DragCallBack(Input.mousePosition.x, Input.mousePosition.y);
	}

}
