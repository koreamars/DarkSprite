using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SmoothMoves
{
    static public class TextureSelectionWindow
    {
        static public float TEXTURE_SELECTION_SPACING = 15.0f;
        static public float TEXTURE_LABEL_SIZE = 15.0f;

        static private Rect _areaRect;
        static private Vector2 _textureSelectionScrollPosition;
        static private bool _visible;

        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }
        static public Rect AreaRect { get { return _areaRect; } set { _areaRect = value; } }
        static public Vector2 TextureSelectionScrollPosition { get { return _textureSelectionScrollPosition; } set { _textureSelectionScrollPosition = value; } }
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
        }

        static public void OnGUI()
        {
            if (!_visible)
                return;

            GUILayout.BeginArea(_areaRect, GUIContent.none, Style.windowRectBackgroundStyle);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(GUILayout.Height(25.0f));
            GUILayout.Space(BoneAnimationDataEditorWindow.PADDING);
            GUILayout.Label("Select Texture", Style.normalLabelStyle);
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

                GUILayout.BeginHorizontal();
                GUILayout.Space(10.0f);
                GUILayout.Label("Too many keys selected to show the texture.", Style.wordWrapStyle);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.EndArea();

                return;
            }

            if (TimelineWindow.NoKeyframesSelected)
            {
                GUILayout.Space(40.0f);

                GUILayout.BeginHorizontal();
                GUILayout.Space(10.0f);
                GUILayout.Label("No keyframe selected to show the texture.", Style.wordWrapStyle);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.EndArea();

                return;
            }

            if (LastKeyframe.atlas == null)
            {
                GUILayout.Space(40.0f);

                GUILayout.BeginHorizontal();
                GUILayout.Space(10.0f);
                GUILayout.Label("No atlas has been set in previous keyframes.", Style.wordWrapStyle);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.EndArea();

                return;
            }

            if (!TimelineWindow.FirstSelectedKeyframe.useTextureGUID)
            {
                GUILayout.Space(40.0f);

                GUILayout.BeginHorizontal();
                GUILayout.Space(10.0f);
                GUILayout.Label("This keyframe does not have a texture key.", Style.wordWrapStyle);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.EndArea();

                return;
            }


            _textureSelectionScrollPosition = GUILayout.BeginScrollView(_textureSelectionScrollPosition, false, false);

            KeyframeSM keyframe;
            int textureIndex = 0;
            GUIStyle currentStyle;
            GUIStyle currentLabelStyle;
            Texture2D texture;
            string textureName;
            GUIContent guiContent;

            keyframe = TimelineWindow.FirstSelectedKeyframe;

            if (LastKeyframe.keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
            {
                if (TextureManager.AtlasTextureDictionary.ContainsKey(LastKeyframe.atlas))
                {
                    foreach (string textureGUID in TextureManager.AtlasTextureDictionary[LastKeyframe.atlas].textureGUIDs)
                    {
                        texture = TextureManager.TextureDictionary[textureGUID].texture;
                        textureName = TextureManager.GetAssetNameFromPath(AssetDatabase.GUIDToAssetPath(textureGUID), false);

                        if (LastKeyframe.textureGUID == textureGUID)
                        {
                            currentStyle = Style.selectedTextureStyle;
                            currentLabelStyle = Style.selectTextureLabelOnStyle;
                        }
                        else
                        {
                            currentStyle = Style.unSelectedTextureStyle;
                            currentLabelStyle = Style.selectTextureLabelOffStyle;
                        }

                        if (GUILayout.Button(texture, currentStyle, GUILayout.Width(BoneAnimationDataEditorWindow.SELECT_TEXTURE_SIZE), GUILayout.Height(BoneAnimationDataEditorWindow.SELECT_TEXTURE_SIZE)))
                        {
                            editor.SetWillBeDirty();

                            BonePropertiesWindow.LastBoneEditorTextureGUID = textureGUID;
                            if (TextureManager.TextureDictionary.ContainsKey(textureGUID))
                            {
                                keyframe.textureGUID = textureGUID;
                                TimelineWindow.UpdateLastKeyframe();
                                BonePropertiesWindow.SetTexture(TextureManager.TextureDictionary[textureGUID].texture);
                                editor.SetNeedsRepainted();
                            }

                            AnimationTexturePivotWindow.SetTexturePivotWindow();

                        }

                        guiContent = new GUIContent(textureName, textureName);
                        GUILayout.Label(guiContent, currentLabelStyle, GUILayout.Width(BoneAnimationDataEditorWindow.SELECT_TEXTURE_SIZE), GUILayout.Height(TEXTURE_LABEL_SIZE));

                        GUILayout.Space(TEXTURE_SELECTION_SPACING);

                        textureIndex++;
                    }
                }
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUILayout.EndArea();
        }

        static public void GetInput(Event evt)
        {
            if (evt.type == EventType.MouseDown && _areaRect.Contains(evt.mousePosition))
            {
                editor.KeyboardFocus = BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.TextureSelection;
            }
        }

        static public void LostFocus()
        {
        }
    }
}
