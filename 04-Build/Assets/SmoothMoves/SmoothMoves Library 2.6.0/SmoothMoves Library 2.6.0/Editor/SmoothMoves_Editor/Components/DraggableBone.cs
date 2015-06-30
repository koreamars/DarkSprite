using UnityEngine;
using System.Collections;

namespace SmoothMoves
{
    public class DraggableBone
    {
        private Vector2 _size;

        public string boneName;
        public Vector2 position;

        public DraggableBone(string bName, Vector2 pos)
        {
            boneName = bName;
            position = pos;
        }

        public void OnGUI()
        {
			if (_size != Vector2.zero)
			{
	            Rect drawRect = new Rect(position.x - (_size.x * 0.5f), position.y - (_size.y * 0.5f), _size.x, _size.y);
	
	            GUILayout.BeginArea(drawRect, boneName, Style.setValueFaintStyle);
	
	            GUILayout.EndArea();
			}
        }

        public void Hide(bool hide)
        {
            _size.x = (hide ? 0 : 100.0f);
            _size.y = (hide ? 0 : 20.0f);
        }
    }
}
