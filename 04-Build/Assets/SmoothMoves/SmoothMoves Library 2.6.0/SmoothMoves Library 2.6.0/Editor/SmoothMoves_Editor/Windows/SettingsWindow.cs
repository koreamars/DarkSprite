using UnityEngine;
using UnityEditor;

namespace SmoothMoves
{
	static class SettingsWindow
	{
        public const float SETTINGS_WINDOW_WIDTH = 300.0f;
        public const float SETTINGS_WINDOW_HEIGHT = 300.0f;

        public const float DEFAULT_ANIMATION_GRID_SIZE = 50.0f;
        public const float DEFAULT_ANIMATION_NON_SELECTION_DARKEN_FACTOR = 0.5f;

        public enum KEYFRAME_PROPERTY_COPYPASTE_MASK
        {
            UserTrigger = 1,
            Type = 2,
            Atlas = 4,
            Texture = 8,
            Pivot = 16,
            Depth = 32,
            Collider = 64,
            LocalPosition = 128,
            LocalRotation = 256,
            LocalScale = 512,
            ImageScale = 1024,
            Color = 2048
        }

        static private Rect _areaRect; 
        static private bool _visible;

//        static private bool _newAutoUpdateScene;
        static private bool _newAnimationDrawGizmoLabels;
        static private bool _newAnimationDrawGrid;
        static private float _newAnimationGridSize;
        static private float _newAnimationNonSelectionDarkenFactor;
        static private bool _newAnimationShowSelectedBoneBounds;
        static private int _newAnimationAxisThickness;
        static private KEYFRAME_PROPERTY_COPYPASTE_MASK _newTimelinePropertyCopyPasteMask;
        static private float _newDepth0ZOffset;
        static private float _newDepthMaxZOffset;

        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }
        static public Rect AreaRect { get { return _areaRect; } set { _areaRect = value; } }
        static public bool Visible { get { return _visible; } }
        static public bool AnimationDrawGrid 
        { 
            get 
            { 
                return (PlayerPrefs.GetInt("SmoothMoves_Editor_DrawAnimationGrid", 1) == 1 ? true : false); 
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_Editor_DrawAnimationGrid", (value ? 1 : 0));
            }
        }
        static public bool AnimationDrawGizmoLabels 
        { 
            get 
            { 
                return (PlayerPrefs.GetInt("SmoothMoves_Editor_DrawAnimationGizmos", 1) == 1 ? true : false); 
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_Editor_DrawAnimationGizmos", (value ? 1 : 0));
            }
        }
        static public float AnimationGridSize
        {
            get
            {
                return PlayerPrefs.GetFloat("SmoothMoves_Editor_AnimationGridSize", DEFAULT_ANIMATION_GRID_SIZE);
            }
            set
            {
                PlayerPrefs.SetFloat("SmoothMoves_Editor_AnimationGridSize", Mathf.Clamp(value, 0, value));
            }
        }

        static public float AnimationNonSelectionDarkenFactor
        {
            get
            {
                return PlayerPrefs.GetFloat("SmoothMoves_Editor_AnimationNonSelectionDarkenFactor", DEFAULT_ANIMATION_NON_SELECTION_DARKEN_FACTOR);
            }
            set
            {
                PlayerPrefs.SetFloat("SmoothMoves_Editor_AnimationNonSelectionDarkenFactor", Mathf.Clamp01(value));
            }
        }

        static public bool AnimationShowSelectedBoneBounds
        {
            get
            {
                return (PlayerPrefs.GetInt("SmoothMoves_Editor_AnimationShowSelectedBoneBounds", 0) == 1 ? true : false);
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_Editor_AnimationShowSelectedBoneBounds", (value ? 1 : 0));
            }
        }

        static public int AnimationAxisThickness
        {
            get
            {
                return PlayerPrefs.GetInt("SmoothMoves_Editor_AnimationAxisThickness", 1);
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_Editor_AnimationAxisThickness", value);
            }
        }

        static public bool AnimationContrastDark
        {
            get
            {
                return (PlayerPrefs.GetInt("SmoothMoves_Editor_ContrastDark", 1) == 1 ? true : false);
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_Editor_ContrastDark", (value ? 1 : 0));
            }
        }

        static public bool AnimationScaleGizmoImage
        {
            get
            {
                return (PlayerPrefs.GetInt("SmoothMoves_Editor_ScaleGizmoImage", 1) == 1 ? true : false);
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_Editor_ScaleGizmoImage", (value ? 1 : 0));
            }
        }

        static public KEYFRAME_PROPERTY_COPYPASTE_MASK TimelinePropertyCopyPasteMask
        {
            get
            {
                return (KEYFRAME_PROPERTY_COPYPASTE_MASK)(PlayerPrefs.GetInt("SmoothMoves_Editor_TimelinePropertyCopyPasteMask", (int)0xFFF));
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_Editor_TimelinePropertyCopyPasteMask", (int)value);
            }
        }

        static public float Depth0ZOffset
        {
            get
            {
                return PlayerPrefs.GetFloat("SmoothMoves_Editor_Depth0ZOffset", 0);
            }
            set
            {
                PlayerPrefs.SetFloat("SmoothMoves_Editor_Depth0ZOffset", value);
            }
        }

        static public float DepthMaxZOffset
        {
            get
            {
                return PlayerPrefs.GetFloat("SmoothMoves_Editor_DepthMaxZOffset", 0.05f);
            }
            set
            {
                PlayerPrefs.SetFloat("SmoothMoves_Editor_DepthMaxZOffset", value);
            }
        }

        //static public bool AutoUpdateScene
        //{
        //    get
        //    {
        //        return (PlayerPrefs.GetInt("SmoothMoves_Editor_AutoUpdateScene", 0) == 1 ? true : false);
        //    }
        //    set
        //    {
        //        bool oldAutoUpdate = AutoUpdateScene;

        //        PlayerPrefs.SetInt("SmoothMoves_Editor_AutoUpdateScene", (value ? 1 : 0));

        //        if (value && !oldAutoUpdate)
        //        {
        //            MeshUtilities.AutoUpdateBoneAnimation(editor.boneAnimationData);
        //        }
        //    }
        //}

        static public float WindowWidth
        {
            get
            {
                return SETTINGS_WINDOW_WIDTH;
            }
        }

        static public float WindowHeight
        {
            get
            {
                return SETTINGS_WINDOW_HEIGHT;
            }
        }

        static public void OnEnable()
        {
            _visible = false;
            _newAnimationGridSize = 0;
            _newAnimationNonSelectionDarkenFactor = 0.5f;
        }

        static public void ShowWindow()
        {
            _visible = true;

//            _newAutoUpdateScene = AutoUpdateScene;
            _newAnimationDrawGizmoLabels = AnimationDrawGizmoLabels;
            _newAnimationDrawGrid = AnimationDrawGrid;
            _newAnimationGridSize = AnimationGridSize;
            _newAnimationNonSelectionDarkenFactor = AnimationNonSelectionDarkenFactor;
            _newAnimationShowSelectedBoneBounds = AnimationShowSelectedBoneBounds;
            _newAnimationAxisThickness = AnimationAxisThickness;
            _newTimelinePropertyCopyPasteMask = TimelinePropertyCopyPasteMask;
            _newDepth0ZOffset = Depth0ZOffset;
            _newDepthMaxZOffset = DepthMaxZOffset;
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

                GUILayout.BeginArea(_areaRect);

                GUILayout.BeginVertical(Style.windowRectBackgroundStyle);

                GUILayout.BeginHorizontal();

                GUILayout.Label("Settings", Style.centeredTextStyle, GUILayout.Width(_areaRect.width - 40.0f));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", GUILayout.Width(30.0f)))
                {
                    _visible = false;
                    editor.SetNeedsRepainted();
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(20.0f);

//                _newAutoUpdateScene = GUILayout.Toggle(_newAutoUpdateScene, "Auto-Update Scene");
                _newAnimationDrawGizmoLabels = GUILayout.Toggle(_newAnimationDrawGizmoLabels, "Show Gizmo Labels", Style.normalToggleStyle);
                _newAnimationShowSelectedBoneBounds = GUILayout.Toggle(_newAnimationShowSelectedBoneBounds, "Show Selected Bone Bounds", Style.normalToggleStyle);
                _newAnimationDrawGrid = GUILayout.Toggle(_newAnimationDrawGrid, "Show Grid", Style.normalToggleStyle);

                if (_newAnimationDrawGrid)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("  Grid Size:", Style.normalLabelStyle, GUILayout.Width(80.0f));
                    GUILayout.FlexibleSpace();
                    _newAnimationGridSize = EditorGUILayout.FloatField(_newAnimationGridSize, GUILayout.Width(100.0f));
                    // Don't allow zero or negative pixel grid size if showing the grid
                    // or the editor will lock up
                    if (_newAnimationGridSize <= 0)
                    {
                        _newAnimationGridSize = DEFAULT_ANIMATION_GRID_SIZE;
                    }

                    GUILayout.Label("px", Style.normalLabelStyle, GUILayout.Width(20.0f));

                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(6.0f);

                GUILayout.BeginHorizontal();
                GUILayout.Label("  Non-Selected Bone Darken:", Style.normalLabelStyle);
                _newAnimationNonSelectionDarkenFactor = GUILayout.HorizontalSlider(_newAnimationNonSelectionDarkenFactor, 0, 1.0f, GUILayout.Width(80.0f));
                GUILayout.Space(20.0f);
                GUILayout.EndHorizontal();

                GUILayout.Space(6.0f);

                GUILayout.BeginHorizontal();
                GUILayout.Label("  Axis Thickness:", Style.normalLabelStyle);
                string[] axisThicknesses = new string[11];
                for (int d = 0; d <= 10; d++)
                {
                    axisThicknesses[d] = d.ToString() + " px";
                }
                _newAnimationAxisThickness = EditorGUILayout.Popup(_newAnimationAxisThickness, axisThicknesses);
                GUILayout.Space(20.0f);
                GUILayout.EndHorizontal();

                GUILayout.Space(6.0f);

                GUILayout.BeginHorizontal();
                GUILayout.Label("  Copy / Paste:", Style.normalLabelStyle);
                _newTimelinePropertyCopyPasteMask = (KEYFRAME_PROPERTY_COPYPASTE_MASK)EditorGUILayout.EnumMaskField(_newTimelinePropertyCopyPasteMask);
                GUILayout.Space(20.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("  Depth 0 Z Offset:", Style.normalLabelStyle);
                _newDepth0ZOffset = EditorGUILayout.FloatField(_newDepth0ZOffset, GUILayout.Width(60f));
                GUILayout.Space(20.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("  Depth Max Z Offset:", Style.normalLabelStyle);
                _newDepthMaxZOffset = EditorGUILayout.FloatField(_newDepthMaxZOffset, GUILayout.Width(60f));
                GUILayout.Space(20.0f);
                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Update", GUILayout.Width(70.0f)))
                {
                    //AutoUpdateScene = _newAutoUpdateScene;
                    AnimationDrawGizmoLabels = _newAnimationDrawGizmoLabels;
                    AnimationDrawGrid = _newAnimationDrawGrid;
                    AnimationGridSize = _newAnimationGridSize;
                    AnimationNonSelectionDarkenFactor = _newAnimationNonSelectionDarkenFactor;
                    AnimationShowSelectedBoneBounds = _newAnimationShowSelectedBoneBounds;
                    AnimationAxisThickness = _newAnimationAxisThickness;
                    TimelinePropertyCopyPasteMask = _newTimelinePropertyCopyPasteMask;
                    Depth0ZOffset = _newDepth0ZOffset;
                    DepthMaxZOffset = _newDepthMaxZOffset;

                    _visible = false;
                    editor.SetNeedsRepainted();
                }

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.EndArea();
            }
        }

        static public void GetInput(Event evt)
        {
        }
	}
}
