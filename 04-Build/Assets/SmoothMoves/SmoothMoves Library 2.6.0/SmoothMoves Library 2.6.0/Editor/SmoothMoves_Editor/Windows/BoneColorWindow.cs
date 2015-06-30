using System;
using UnityEngine;
using UnityEditor;

namespace SmoothMoves
{
    class BoneColorWindow
    {
        private const float COLOR_SELECTOR_WIDTH = 80.0f;
        private const float BLEND_WEIGHT_SLIDER_WIDTH = 120.0f;
        private const float BONE_WEIGHT_SCROLL_SENSITIVITY = 0.01f;

        public const float BONE_COLOR_WINDOW_WIDTH = 350.0f;
        public const float BONE_COLOR_WINDOW_HEIGHT = 100.0f;

        static private BoneData _boneData;
        static private Rect _areaRect;
        static private bool _visible;

        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }
        static public Rect AreaRect { get { return _areaRect; } set { _areaRect = value; } }
        static public bool Visible { get { return _visible; } }
        static public float WindowWidth
        {
            get
            {
                return BONE_COLOR_WINDOW_WIDTH;
            }
        }

        static public float WindowHeight
        {
            get
            {
                return BONE_COLOR_WINDOW_HEIGHT;
            }
        }

        static public void Reset(BoneData boneData)
        {
            _boneData = boneData;

            _visible = true;
        }

        static public void OnEnable()
        {
        }

        static public void OnGUI()
        {
            if (_visible)
            {
                Style.OnGUI();

                Rect r = new Rect(_areaRect.x - 10.0f,
                                  _areaRect.y - 10.0f,
                                  _areaRect.width + 20.0f,
                                  _areaRect.height + 20.0f);
                GUI.Box(r, GUIContent.none);

                GUILayout.BeginArea(_areaRect, Style.windowRectBackgroundStyle);

                GUIHelper.DrawBox(new Rect(0, 30.0f, _areaRect.width, _areaRect.height - 60.0f),
                                                Style.windowRectBackgroundStyle,
                                                true);

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();

                GUILayout.Label("Blend [" + _boneData.boneName + "] Bone Color", Style.centeredTextStyle, GUILayout.Width(_areaRect.width - 40.0f));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", GUILayout.Width(30.0f)))
                {
                    _visible = false;
                    editor.SetNeedsRepainted();
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(10.0f);

                GUILayout.BeginHorizontal();

                GUILayout.Space(25.0f);

                GUILayout.Space(2.0f);
                GUILayout.Label("Mesh Color", Style.normalLabelStyle, GUILayout.Width(COLOR_SELECTOR_WIDTH));
                GUILayout.Space(25.0f);
                GUILayout.Label("Blend Weight", Style.normalLabelStyle, GUILayout.Width(100.0f));
                GUILayout.Label("Bone Color", Style.normalLabelStyle, GUILayout.Width(COLOR_SELECTOR_WIDTH));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Space(25.0f);

                Color newMeshColor = EditorGUILayout.ColorField(editor.boneAnimationData.meshColor, GUILayout.Width(COLOR_SELECTOR_WIDTH));
                if (newMeshColor != editor.boneAnimationData.meshColor)
                {
                    editor.SetWillBeDirty();
                    
                    editor.boneAnimationData.meshColor = newMeshColor;
                }

                float newBlendingWeight = GUILayout.HorizontalSlider(_boneData.boneColor.blendingWeight, 0, 1.0f, GUILayout.Width(BLEND_WEIGHT_SLIDER_WIDTH));
                if (newBlendingWeight != _boneData.boneColor.blendingWeight)
                {
                    editor.SetWillBeDirty();
                    
                    _boneData.boneColor.blendingWeight = newBlendingWeight;
                }

                Color newBoneColor = EditorGUILayout.ColorField(_boneData.boneColor.color, GUILayout.Width(COLOR_SELECTOR_WIDTH));
                if (newBoneColor != _boneData.boneColor.color)
                {
                    editor.SetWillBeDirty();
                    
                    _boneData.boneColor.color = newBoneColor;
                }

                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();

                GUILayout.Space(25.0f);

                if (GUILayout.Button("None", GUILayout.Width(COLOR_SELECTOR_WIDTH)))
                {
                    editor.SetWillBeDirty();
                    
                    _boneData.boneColor.blendingWeight = 0;
                }

                if (GUILayout.Button("Half / Half", GUILayout.Width(BLEND_WEIGHT_SLIDER_WIDTH)))
                {
                    editor.SetWillBeDirty();
                    
                    _boneData.boneColor.blendingWeight = 0.5f;
                }

                if (GUILayout.Button("Full", GUILayout.Width(COLOR_SELECTOR_WIDTH)))
                {
                    editor.SetWillBeDirty();
                    
                    _boneData.boneColor.blendingWeight = 1.0f;
                }

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.EndArea();
            }
        }

        static public void GetInput(Event evt)
        {
            if (_visible)
            {
                if (evt.type == EventType.ScrollWheel)
                {
                    editor.SetWillBeDirty();
                    
                    _boneData.boneColor.blendingWeight = Mathf.Clamp01(_boneData.boneColor.blendingWeight + (-evt.delta.y * BONE_WEIGHT_SCROLL_SENSITIVITY));
                    editor.SetNeedsRepainted();
                    evt.Use();
                }
            }
        }
    }
}
