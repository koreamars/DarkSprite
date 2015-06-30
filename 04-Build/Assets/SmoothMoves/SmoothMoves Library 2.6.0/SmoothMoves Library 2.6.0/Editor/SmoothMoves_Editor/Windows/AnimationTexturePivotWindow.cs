using UnityEngine;
using UnityEditor;

namespace SmoothMoves
{
    static public class AnimationTexturePivotWindow
    {
        static private bool _visible;
        static private Rect _areaRect;
        static private TexturePivotWindow _texturePivotWindow;

        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }
        static public Rect AreaRect { get { return _areaRect; } set { _areaRect = value; } }
        static public bool Visible 
        { 
            get 
            { 
                return _visible; 
            } 
            set 
            {
                _visible = value;

                AnimationWindow.RepositionAfterResize = true;
            } 
        }

        static public void OnEnable()
        {
            _visible = false;
            _texturePivotWindow = new TexturePivotWindow();

            _texturePivotWindow.OnEnable();
        }

        static public void OnGUI()
        {
            if (!_visible)
                return;

            Vector2 pivotOffset;
            Vector2 oldPivotOffset;
            bool useDefaultPivot;
            bool oldUseDefaultPivot; 
            bool somethingChanged = false;
            Vector2 defaultPivotOffset;

            GUILayout.BeginArea(_areaRect, GUIContent.none, Style.windowRectBackgroundStyle);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(GUILayout.Height(25.0f));
            GUILayout.Space(BoneAnimationDataEditorWindow.PADDING);
            GUILayout.Label("Set Pivot", Style.normalLabelStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X"))
            {
                Visible = false;
                editor.SetNeedsRepainted();
            }
            GUILayout.EndHorizontal();

            if (TimelineWindow.MultipleKeyframesSelected)
            {
                GUILayout.Space(40.0f);
                GUILayout.Label("  Too many keys selected to show pivot.", Style.normalLabelStyle);

                GUILayout.EndVertical();
                GUILayout.EndArea();
                return;
            }

            if (TimelineWindow.NoKeyframesSelected)
            {
                GUILayout.Space(40.0f);
                GUILayout.Label("  No keyframes selected to show pivot.", Style.normalLabelStyle);

                GUILayout.EndVertical();
                GUILayout.EndArea();
                return;
            }

            if (!TimelineWindow.FirstSelectedKeyframe.usePivotOffset)
            {
                GUILayout.Space(40.0f);
                GUILayout.Label("  This keyframe does not have a pivot offset key.", Style.normalLabelStyle);

                GUILayout.EndVertical();
                GUILayout.EndArea();
                return;
            }


            pivotOffset = TimelineWindow.FirstSelectedKeyframe.pivotOffset;
            oldPivotOffset = pivotOffset;
            useDefaultPivot = TimelineWindow.FirstSelectedKeyframe.useDefaultPivot;
            oldUseDefaultPivot = useDefaultPivot;

            defaultPivotOffset = Vector2.zero;
            if (useDefaultPivot)
            {
                if (LastKeyframe.atlas != null)
                {
                    defaultPivotOffset = LastKeyframe.atlas.LookupDefaultPivotOffset(LastKeyframe.textureIndex);
                }
            }

            somethingChanged = _texturePivotWindow.OnGUI(ref pivotOffset,
                                                                    Event.current,
                                                                    new Rect(0, 25.0f, _areaRect.width, _areaRect.height - 25.0f),
                                                                    15.0f,
                                                                    true,
                                                                    ref useDefaultPivot,
                                                                    defaultPivotOffset
                                                                    );

            if (somethingChanged)
            {
                editor.SetWillBeDirty();
                
                TimelineWindow.FirstSelectedKeyframe.useDefaultPivot = useDefaultPivot;
                if (!useDefaultPivot)
                {
                    TimelineWindow.FirstSelectedKeyframe.pivotOffset = pivotOffset;
                }

                editor.SetNeedsRepainted();
                somethingChanged = false;
            }

            GUILayout.EndVertical();


            GUILayout.EndArea();
        }

        static public void SetTexturePivotWindow()
        {
            if (TimelineWindow.OneKeyframeSelected)
            {
                if (LastKeyframe.atlas == null || LastKeyframe.textureGUID == "" || LastKeyframe.keyframeType == KeyframeSM.KEYFRAME_TYPE.TransformOnly)
                {
                    _texturePivotWindow.SetBlankTexture();
                }
                else
                {
                    if (TextureManager.TextureDictionary.ContainsKey(LastKeyframe.textureGUID))
                    {
                        _texturePivotWindow.SetTextureByGUID(LastKeyframe.textureGUID);
                    }
                    else
                    {
                        _texturePivotWindow.SetBlankTexture();
                    }
                }
            }
            else
            {
                _texturePivotWindow.SetBlankTexture();
            }
        }

        static public void GetInput(Event evt)
        {
            if (evt.type == EventType.MouseDown && _areaRect.Contains(evt.mousePosition))
            {
                editor.KeyboardFocus = BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.TexturePivot;
            }
        }

        static public void LostFocus()
        {
        }
    }
}
