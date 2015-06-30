using UnityEngine;
using UnityEditor;

namespace SmoothMoves
{
    public class DisplayTexture
    {
        public const int NULL_SIZE = 40;
        public const float LABEL_HEIGHT = 15.0f;
        public const float PIVOT_RECT_HALF_SIZE = 5.0f;

        private const float EDGE_BUFFER = 3.0f;

        private Rect selectedRect;
        private Rect labelRect;
        private Rect pivotRect;
        private GUIStyle labelStyle;

        public Texture2D texture;
        public string guid;
        public string textureName;
        public string texturePath;
        public int index;
        public Vector2 position;
        public float scale = 1.0f;
        public bool selected;
        public Vector2 forceSize;
        public Rect drawRect;

        public DisplayTexture(Texture2D tex, string id, string name, string path, int idx, Vector2 pos)
        {
            Initialize(tex, id, name, path, idx, pos, Vector2.zero);
        }

        private void Initialize(Texture2D tex, string id, string name, string path, int idx, Vector2 pos, Vector2 force)
        {
            texture = tex;
            guid = id;
            textureName = name;
            texturePath = path;
            index = idx;
            position = pos;
            forceSize = force;
        }

        public void OnGUI(bool showPivot, Vector2 pivot)
        {
            if (texture == null)
            {
                drawRect = new Rect(position.x, position.y, NULL_SIZE * scale, NULL_SIZE * scale);
            }
            else
            {
                if (forceSize == Vector2.zero)
                {
                    drawRect = new Rect(position.x, position.y, texture.width * scale, texture.height * scale);
                }
                else
                {
                    drawRect = new Rect(position.x, position.y, forceSize.x, forceSize.y);
                }
            }

            labelRect = new Rect(drawRect.x, drawRect.yMax + 2.0f, drawRect.width, LABEL_HEIGHT - 2.0f);

            pivotRect = new Rect(drawRect.x + (drawRect.width / 2.0f) + (drawRect.width * pivot.x) - PIVOT_RECT_HALF_SIZE,
                                 drawRect.y + (drawRect.height / 2.0f) - (drawRect.height * pivot.y) - PIVOT_RECT_HALF_SIZE,
                                 PIVOT_RECT_HALF_SIZE * 2.0f,
                                 PIVOT_RECT_HALF_SIZE * 2.0f);

            if (selected)
            {
                selectedRect = new Rect(drawRect.x - EDGE_BUFFER, drawRect.y - EDGE_BUFFER, drawRect.width + (EDGE_BUFFER * 2.0f), drawRect.height + (EDGE_BUFFER * 2.0f) + LABEL_HEIGHT);

                //Style.PushBackgroundColor(Style.selectionDoneStyle);
                GUI.Box(selectedRect, GUIContent.none, Style.selectionDoneStyle);
                //Style.PopBackgroundColor();

                labelStyle = Style.selectTextureLabelOnStyle;
            }
            else
            {
                labelStyle = Style.selectTextureLabelOffStyle;
            }

            if (texture != null)
            {
                GUI.DrawTexture(drawRect, texture);
            }

            GUI.Label(labelRect, textureName, labelStyle);

            if (showPivot)
                GUI.Box(pivotRect, GUIContent.none, Style.setValueFaintStyle);
        }
    }
}
