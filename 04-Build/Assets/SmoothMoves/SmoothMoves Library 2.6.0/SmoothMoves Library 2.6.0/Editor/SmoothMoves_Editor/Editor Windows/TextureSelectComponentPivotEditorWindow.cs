using UnityEngine;
using UnityEditor;

namespace SmoothMoves
{
    public class TextureSelectComponentPivotEditorWindow : EditorWindow
    {
        private const float MIN_WIDTH = 300.0f;
        private const float MIN_HEIGHT = 300.0f;
        private const double PERIODIC_UPDATE_INTERVAL = 1.0;

        static private TextureSelectComponentPivotEditorWindow _instance;

        private TextureSelectComponent _textureSelectComponent;
        private Rect _lastPosition;

        public TexturePivotWindow texturePivotWindow;

        static public TextureSelectComponentPivotEditorWindow Instance
        {
            get
            {
                return _instance;
            }
        }

        static public TextureSelectComponentPivotEditorWindow ShowEditor()
        {
            if (_instance == null)
            {
                _instance = (TextureSelectComponentPivotEditorWindow)EditorWindow.GetWindow(typeof(TextureSelectComponentPivotEditorWindow), true, "Tex Pivot");
            }

            _instance.ShowUtility();

            Style.Reset();

            return _instance;
        }

        static public void OnEnable()
        {
        }

        static public void OnDestroy()
        {
            _instance = null;
        }

        public void SetTextureSelectComponent(TextureSelectComponent textureSelectComponent)
        {
            _textureSelectComponent = textureSelectComponent;

            if (texturePivotWindow == null)
            {
                texturePivotWindow = new TexturePivotWindow();
            }

            texturePivotWindow.OnEnable();
            if (_textureSelectComponent.atlas != null && _textureSelectComponent.textureGUID != "")
                texturePivotWindow.SetTexture((Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_textureSelectComponent.textureGUID), typeof(Texture2D)));
            else
                texturePivotWindow.SetBlankTexture();

            Repaint();
        }

        public void OnGUI()
        {
            if (_instance == null)
                return;

            if (_instance.texturePivotWindow == null)
                return;

            if (_textureSelectComponent == null)
                return;

            if (position.width != _lastPosition.width || position.height != _lastPosition.height)
            {
                Rect newPosition = new Rect(position.x, position.y, Mathf.Max(position.width, MIN_WIDTH), Mathf.Max(position.height, MIN_HEIGHT));
                position = newPosition;
            }

            Vector2 pivotOffset;
            Vector2 oldPivotOffset;
            bool useDefaultPivot;
            bool oldUseDefaultPivot;
            bool somethingChanged = false;
            Vector2 defaultPivotOffset;

            GUILayout.BeginVertical();

            pivotOffset = _textureSelectComponent.pivotOffset;
            oldPivotOffset = pivotOffset;
            useDefaultPivot = _textureSelectComponent.useDefaultPivot;
            oldUseDefaultPivot = useDefaultPivot;

            defaultPivotOffset = Vector2.zero;
            if (useDefaultPivot)
            {
                if (_textureSelectComponent.atlas != null)
                {
                    defaultPivotOffset = _textureSelectComponent.atlas.LookupDefaultPivotOffset(_textureSelectComponent.textureGUID);
                }
            }

            somethingChanged = _instance.texturePivotWindow.OnGUI(ref pivotOffset,
                                                                    Event.current,
                                                                    new Rect(0, 0, position.width, position.height),
                                                                    5.0f,
                                                                    true,
                                                                    ref useDefaultPivot,
                                                                    defaultPivotOffset
                                                                    );

            if (somethingChanged)
            {
                _textureSelectComponent.SetPivotOffset(pivotOffset, useDefaultPivot);
                Repaint();
                somethingChanged = false;
            }

            GUILayout.EndVertical();

            _lastPosition = position;
        }

        public void RefreshDefaultPivot()
        {
            if (_textureSelectComponent != null)
            {
                if (_textureSelectComponent.useDefaultPivot)
                {
                    _textureSelectComponent.SetPivotOffset(_textureSelectComponent.pivotOffset, true);
                }
            }
        }
    }
}
