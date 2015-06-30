using UnityEngine;

namespace SmoothMoves
{
    public class ValueLabel
    {
        public Rect rect;
        public string text;

        public ValueLabel(string textString, float width, float height)
        {
            rect = new Rect(0, 0, width, height);
            text = textString;
        }

        public void OnGUI(GUIStyle style)
        {
            GUI.Label(rect, text, style);
        }
    }
}
