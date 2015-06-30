using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SmoothMoves
{
    public class BoneAnimationDataEditorWindow : EditorWindow
    {
        static private BoneAnimationDataEditorWindow _instance;

        private const double PERIODIC_UPDATE_INTERVAL = 1.0;
        private const float BOTTOM_SECTION_MIN_HEIGHT = 30.0f;

        public enum TIME_MODE
        {
            Frames,
            AnimationCurves
        }

        private enum RESIZE_WINDOW_MODE
        {
            None,
            ResizeBottom,
            ResizeBonePropertiesWindow,
            ResizeBoneWindow,
            ResizeClipBrowserWindow
        }

        public enum KEYBOARD_FOCUS
        {
            None,
            BoneProperties,
            TextureSelection,
            TexturePivot,
            Animation,
            Bones,
            Timeline,
            Clips
        }

        private bool _needsRepainted;
        private bool _willBeDirty;
        private bool _needsReset;
        private RESIZE_WINDOW_MODE _resizeWindowMode;
        private float _bottomSectionY;
        private bool _atlasNeedsRebuilt;
        private string _atlasesNeedRebuilding;
        private float _deltaTime;
        private double _lastUpdateTime;
        private double _periodicUpdateTime;
        private double _clickedTime;
        private bool _startedCompiling;
        private TIME_MODE _timeMode;
        private KEYBOARD_FOCUS _keyboardFocus;
        private bool _timelineFrameGridNeedsRebuilt;
        private float _animationControlsX;
        private bool _createdSnapshot;

        private Vector2 _windowSize;

        private Rect _resizeBottomRect;
        private Rect _resizeBoneWindowRect;

        public UnityEngine.Object boneAnimationObject;
        public BoneAnimationData boneAnimationData;

        public const float MIN_WINDOW_WIDTH = 700.0f;
        public const float MIN_WINDOW_HEIGHT = 300.0f;
        public const float RECT_X_SPACER = 2.0f;
        public const float RECT_Y_SPACER = 2.0f;
        public const float TOOLBAR_HEIGHT = 70.0f;
        public const float SCROLLBAR_HEIGHT = 23.0f;
        public const float PADDING = 2.0f;
        public const float VERTICAL_SCROLL_SLIDER_WIDTH = 16.0f;
        public const float SELECT_TEXTURE_SIZE = 95.0f;
        public const float DRAG_THRESHOLD = 10.0f;
        public const float DOUBLE_CLICK_THRESHOLD = 0.3f;
        public const float MINIMUM_SCROLL_INTERVAL = 0.1f;

        static public BoneAnimationDataEditorWindow Instance { get { return _instance; } }
        public TIME_MODE TimeMode { get { return _timeMode; } }
        public float Width { get { return position.width; } }
        public KEYBOARD_FOCUS KeyboardFocus
        {
            get
            {
                return _keyboardFocus;
            }
            set
            {
                switch (_keyboardFocus)
                {
                    case KEYBOARD_FOCUS.BoneProperties:
                        BonePropertiesWindow.LostFocus();
                        break;

                    case KEYBOARD_FOCUS.TextureSelection:
                        TextureSelectionWindow.LostFocus();
                        break;

                    case KEYBOARD_FOCUS.TexturePivot:
                        AnimationTexturePivotWindow.LostFocus();
                        break;

                    case KEYBOARD_FOCUS.Animation:
                        AnimationWindow.LostFocus();
                        break;

                    case KEYBOARD_FOCUS.Bones:
                        BoneWindow.LostFocus();
                        break;

                    case KEYBOARD_FOCUS.Timeline:
                        TimelineWindow.LostFocus();
                        break;

                    case KEYBOARD_FOCUS.Clips:
                        ClipBrowserWindow.LostFocus();
                        break;
                }

                _keyboardFocus = value;
            }
        }

        public bool ModalPopup
        {
            get
            {
                return (SetAtlasesWindow.Visible || SettingsWindow.Visible || BoneColorWindow.Visible);
            }
        }

        static public BoneAnimationDataEditorWindow ShowEditorUtility()
        {
            if (_instance != null)
            {
                _instance.ShowUtility();
                return _instance;
            }

            _instance = (BoneAnimationDataEditorWindow)EditorWindow.GetWindow(typeof(BoneAnimationDataEditorWindow), true, "SmoothMoves Animation Editor v" + EditorHelper.VERSION);
            _instance.ShowUtility();

            if (EditorHelper.ShowWelcomeScreen)
                WelcomeScreen.ShowEditor();

            return _instance;
        }

        void OnEnable()
        {
            if (_instance == null)
                _instance = this;

            SetSelection();

            _needsReset = true;

            _resizeWindowMode = RESIZE_WINDOW_MODE.None;
            _timeMode = TIME_MODE.Frames;

            //Resources.OnEnable();
            TextureManager.OnEnable();

            Resources.LoadTextures();

            AnimationControlsWindow.OnEnable();
            AnimationTexturePivotWindow.OnEnable();
            AnimationWindow.OnEnable();
            BonePropertiesWindow.OnEnable();
            BoneWindow.OnEnable();
            ClipBrowserWindow.OnEnable();
            SetAtlasesWindow.OnEnable();
            BoneColorWindow.OnEnable();
            SettingsWindow.OnEnable();
            TextureSelectionWindow.OnEnable();
            TimelineWindow.OnEnable();

            //FieldInfo undoCallback = typeof(EditorApplication).GetField("undoRedoPerformed", BindingFlags.NonPublic | BindingFlags.Static);
            //undoCallback.SetValue(null, (EditorApplication.CallbackFunction)UndoPerformed);

            EditorApplication.playmodeStateChanged += OnEditorPlaymodeStateChanged;
        }

        void OnEditorPlaymodeStateChanged()
        {
            if (!(EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode))
            {
                _needsReset = true;
            }
        }

        void Update()
        {
            bool needsRepainted = false;

            if ((EditorApplication.timeSinceStartup - _periodicUpdateTime) > PERIODIC_UPDATE_INTERVAL)
            {
                SetSelection();
                if (CheckAtlasNeedsRebuilt())
                    needsRepainted = true;

                TextureManager.GenerateTextureDictionary(false);

                _periodicUpdateTime = EditorApplication.timeSinceStartup;
            }

            _deltaTime = (float)(EditorApplication.timeSinceStartup - _lastUpdateTime);
            if (TimelineWindow.FrameUpdate(_deltaTime))
                needsRepainted = true;
            _lastUpdateTime = EditorApplication.timeSinceStartup;

            if (EditorApplication.isCompiling)
            {
                _startedCompiling = true;
            }
            else if (_startedCompiling)
            {
                _startedCompiling = false;
                _needsReset = true;
            }

            if (needsRepainted)
                Repaint();

            if (_needsReset)
            {
                Reset();
                _needsReset = false;
            }
        }

        public void SetSelection()
        {
            bool somethingChanged = false;

            if (Selection.activeObject != null)
            {
                if (Selection.activeObject.GetType() == typeof(BoneAnimationData))
                {
                    if (boneAnimationData != (BoneAnimationData)Selection.activeObject)
                    {
                        boneAnimationObject = Selection.activeObject;
                        boneAnimationData = (BoneAnimationData)boneAnimationObject;

                        this.title = "SmoothMoves Animation Editor v" + EditorHelper.VERSION + " - " + boneAnimationObject.name;

                        if (boneAnimationData.UpdateDataVersion())
                        {
                            Snapshot();
                            Dirty();
                        }

                        somethingChanged = true;
                    }
                }
                //else if (boneAnimationObject != null)
                //{
                //    boneAnimationObject = null;
                //    boneAnimationData = null;
                //    somethingChanged = true;
                //}
            }
            //else if (boneAnimationObject != null)
            //{
            //    boneAnimationObject = null;
            //    boneAnimationData = null;
            //    somethingChanged = true;
            //}

            if (somethingChanged)
            {
                _needsReset = true;
            }
        }

        private bool CheckAtlasNeedsRebuilt()
        {
            bool atlasNeedsRebuilt = false;

            _atlasesNeedRebuilding = "";

            if (boneAnimationData != null && boneAnimationData.animationClips != null)
            {
                foreach (KeyValuePair<TextureAtlas, AtlasDictionaryEntry> pair in TextureManager.AtlasTextureDictionary)
                {
                    if (pair.Key.needsRebuilt == 1)
                    {
                        atlasNeedsRebuilt = true;
                        _atlasesNeedRebuilding += pair.Key.name + ", ";
                    }
                }

                if (atlasNeedsRebuilt)
                {
                    _atlasesNeedRebuilding = _atlasesNeedRebuilding.Substring(0, _atlasesNeedRebuilding.Length - 2);
                }
            }

            if (atlasNeedsRebuilt != _atlasNeedsRebuilt)
            {
                _atlasNeedsRebuilt = atlasNeedsRebuilt;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            Style.Reset();
            Resources.LoadTextures();
            TimelineWindow.ResetSelectedFrames();
            TextureManager.GenerateTextureDictionary(true);
            RecalculateTextureIndices();

            _bottomSectionY = PlayerPrefs.GetFloat("SmoothMoves_Editor_BottomSectionY", 200.0f);

            SetRects(false);

            AnimationWindow.CenterOrigin();
            AnimationHelper.GenerateAnimationCurves(ClipBrowserWindow.CurrentClip);

            SetTimeMode(_timeMode);

            Repaint();
        }



        public void OnGUI()
        {
            if (boneAnimationData == null)
                return;

            bool undoPerformed = false;
            if (Event.current.type == EventType.ValidateCommand)
            {
                if (Event.current.commandName == "UndoRedoPerformed")
                {
                    undoPerformed = true;
                }
            }

            boneAnimationData.Initialize();

            SetRects(true);

            if (EditorApplication.isPlaying)
            {
                GUILayout.BeginVertical();
                GUILayout.Space((position.height / 2.0f) - 20.0f);
                GUILayout.Label("Editing animation is disabled while the editor is playing", Style.centeredTextStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
            }
            else if (_atlasNeedsRebuilt)
            {
                GUILayout.BeginVertical();
                GUILayout.Space((position.height / 2.0f) - 20.0f);
                GUILayout.Label("The following atlases in this animation need to be rebuilt:", Style.centeredTextStyle);
                GUILayout.Label(_atlasesNeedRebuilding, Style.centeredTextStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
            }
            else
            {
                if (TimelineWindow.IsPlaying)
                {
                    AnimationWindow.OnGUI();
                    AnimationControlsWindow.OnGUI(true);
                    TimelineWindow.OnGUI();

                    GUIHelper.DrawBox(new Rect(0, position.height - _bottomSectionY, position.width, 2.0f), Style.gridBackgroundStyle, true);
                    GUIHelper.DrawBox(new Rect(_animationControlsX, AnimationControlsWindow.Height, position.width - _animationControlsX, 2.0f), Style.gridBackgroundStyle, true);
                    GUIHelper.DrawBox(new Rect(BonePropertiesWindow.BonePropertiesWindowX, 0, 2.0f, position.height - _bottomSectionY), Style.gridBackgroundStyle, true);
                    GUIHelper.DrawBox(new Rect(BoneWindow.BoneWindowX, position.height - _bottomSectionY, 2.0f, _bottomSectionY), Style.gridBackgroundStyle, true);
                    GUIHelper.DrawBox(new Rect(AnimationWindow.AreaRect.xMin, AnimationWindow.AreaRect.yMin, 2.0f, AnimationWindow.AreaRect.height), Style.gridBackgroundStyle, true);

                    AnimationWindow.GetInput(Event.current);
                }
                else
                {
                    AnimationWindow.OnGUI();
                    BonePropertiesWindow.OnGUI();
                    TextureSelectionWindow.OnGUI();
                    AnimationTexturePivotWindow.OnGUI();
                    AnimationControlsWindow.OnGUI(false);
                    BoneWindow.OnGUI();
                    TimelineWindow.OnGUI();
                    SetAtlasesWindow.OnGUI();
                    ClipBrowserWindow.OnGUI();

                    GUIHelper.DrawBox(new Rect(0, position.height - _bottomSectionY, position.width, 2.0f), Style.gridBackgroundStyle, true);
                    GUIHelper.DrawBox(new Rect(_animationControlsX, AnimationControlsWindow.Height, position.width - _animationControlsX, 2.0f), Style.gridBackgroundStyle, true);
                    GUIHelper.DrawBox(new Rect(BonePropertiesWindow.BonePropertiesWindowX, 0, 2.0f, position.height - _bottomSectionY), Style.gridBackgroundStyle, true);
                    GUIHelper.DrawBox(new Rect(BoneWindow.BoneWindowX, position.height - _bottomSectionY, 2.0f, _bottomSectionY), Style.gridBackgroundStyle, true);
                    GUIHelper.DrawBox(new Rect(AnimationWindow.AreaRect.xMin, AnimationWindow.AreaRect.yMin, 2.0f, AnimationWindow.AreaRect.height), Style.gridBackgroundStyle, true);

                    SettingsWindow.OnGUI();
                    BoneColorWindow.OnGUI();

                    if (!ModalPopup)
                    {
                        GetInput(Event.current);
                        BonePropertiesWindow.GetInput(Event.current);
                        TextureSelectionWindow.GetInput(Event.current);
                        AnimationTexturePivotWindow.GetInput(Event.current);
                        AnimationWindow.GetInput(Event.current);
                        BoneWindow.GetInput(Event.current);
                        TimelineWindow.GetInput(Event.current);
                        ClipBrowserWindow.GetInput(Event.current);
                    }
                    else if (SetAtlasesWindow.Visible)
                    {
                        SetAtlasesWindow.GetInput(Event.current);
                    }
                    else if (BoneColorWindow.Visible)
                    {
                        BoneColorWindow.GetInput(Event.current);
                    }
                    else if (SettingsWindow.Visible)
                    {
                        SettingsWindow.GetInput(Event.current);
                    }

                    if (Event.current.type == EventType.Repaint && !TimelineWindow.IsPlaying)
                    {
                        BoneWindow.MoveBones();
                        BoneWindow.ScrollBones();
                        BoneWindow.AddBonePostRepaint();
                        TimelineWindow.SelectFramePostCycle();
                        AnimationHelper.RefreshCurveEditorPostCycle();

                        if (_timelineFrameGridNeedsRebuilt)
                        {
                            TimelineWindow.SetFrameGridNeedsRebuilt();
                            _timelineFrameGridNeedsRebuilt = false;
                            _needsRepainted = true;
                        }
                    }

                    if (_willBeDirty)
                    {
                        if (boneAnimationObject != null)
                        {
                            Dirty();
                        }

                        AnimationHelper.GenerateAnimationCurves(ClipBrowserWindow.CurrentClip);

                        _willBeDirty = false;
                    }
                }

                if (_needsRepainted)
                {
                    _needsRepainted = false;
                    Repaint();
                }
            }

            if (undoPerformed)
            {
                UndoPerformed();
            }
        }

        private void SetRects(bool addCursorRects)
        {
            float clipBrowserX;

            _windowSize = new Vector2(Mathf.Max(position.width, BoneWindow.AreaRect.width + TimelineWindow.MIN_WIDTH + ClipBrowserWindow.AreaRect.width), Mathf.Max(position.height, MIN_WINDOW_HEIGHT));

            if (!ModalPopup)
            {
                switch (_resizeWindowMode)
                {
                    case RESIZE_WINDOW_MODE.ResizeBottom:
                        _bottomSectionY = Mathf.Min(Mathf.Max(_windowSize.y - Event.current.mousePosition.y, BOTTOM_SECTION_MIN_HEIGHT), _windowSize.y - BOTTOM_SECTION_MIN_HEIGHT);
                        PlayerPrefs.SetFloat("SmoothMoves_Editor_BottomSectionY", _bottomSectionY);
                        _timelineFrameGridNeedsRebuilt = true;
                        break;

                    case RESIZE_WINDOW_MODE.ResizeBoneWindow:
                        BoneWindow.BoneWindowX = Event.current.mousePosition.x;
                        _timelineFrameGridNeedsRebuilt = true;
                        break;
                }
            }

            clipBrowserX = _windowSize.x - ClipBrowserWindow.ClipBrowserWidth;

            BonePropertiesWindow.AreaRect = new Rect(
                                           0,
                                           0,
                                           BonePropertiesWindow.BonePropertiesWindowX,
                                           _windowSize.y - _bottomSectionY
                                           );

            if (TextureSelectionWindow.Visible)
            {
                TextureSelectionWindow.AreaRect = new Rect(
                                                     BonePropertiesWindow.AreaRect.xMax + RECT_X_SPACER,
                                                     0,
                                                     120.0f,
                                                     BonePropertiesWindow.AreaRect.height
                                                     );

                if (AnimationTexturePivotWindow.Visible)
                {
                    AnimationTexturePivotWindow.AreaRect = new Rect(
                                                      TextureSelectionWindow.AreaRect.xMax + RECT_X_SPACER,
                                                      0,
                                                      300.0f,
                                                      BonePropertiesWindow.AreaRect.height
                                                      );

                    _animationControlsX = AnimationTexturePivotWindow.AreaRect.xMax + RECT_X_SPACER;
                    AnimationControlsWindow.AreaRect = new Rect(
                                                          _animationControlsX,
                                                          0,
                                                          _windowSize.x - _animationControlsX,
                                                          AnimationControlsWindow.Height
                                                          );

                    AnimationWindow.AreaRect = new Rect(
                                                  AnimationTexturePivotWindow.AreaRect.xMax + RECT_X_SPACER,
                                                  AnimationControlsWindow.AreaRect.yMax + RECT_Y_SPACER,
                                                  _windowSize.x - (AnimationTexturePivotWindow.AreaRect.xMax + RECT_X_SPACER),
                                                  BonePropertiesWindow.AreaRect.height - (AnimationControlsWindow.AreaRect.height + RECT_Y_SPACER)
                                                  );
                }
                else
                {
                    _animationControlsX = TextureSelectionWindow.AreaRect.xMax + RECT_X_SPACER;
                    AnimationControlsWindow.AreaRect = new Rect(
                                                          _animationControlsX,
                                                          0,
                                                          _windowSize.x - _animationControlsX,
                                                          AnimationControlsWindow.Height
                                                          );
                    AnimationWindow.AreaRect = new Rect(
                                                  TextureSelectionWindow.AreaRect.xMax + RECT_X_SPACER,
                                                  AnimationControlsWindow.AreaRect.yMax + RECT_Y_SPACER,
                                                  _windowSize.x - (TextureSelectionWindow.AreaRect.xMax + RECT_X_SPACER),
                                                  BonePropertiesWindow.AreaRect.height - (AnimationControlsWindow.AreaRect.height + RECT_Y_SPACER)
                                                  );
                }
            }
            else
            {
                if (AnimationTexturePivotWindow.Visible)
                {
                    AnimationTexturePivotWindow.AreaRect = new Rect(
                                                      BonePropertiesWindow.AreaRect.xMax + RECT_X_SPACER,
                                                      0,
                                                      300.0f,
                                                      BonePropertiesWindow.AreaRect.height
                                                      );

                    _animationControlsX = AnimationTexturePivotWindow.AreaRect.xMax + RECT_X_SPACER;
                    AnimationControlsWindow.AreaRect = new Rect(
                                                          _animationControlsX,
                                                          0,
                                                          _windowSize.x - _animationControlsX,
                                                          AnimationControlsWindow.Height
                                                          );

                    AnimationWindow.AreaRect = new Rect(
                                                  AnimationTexturePivotWindow.AreaRect.xMax + RECT_X_SPACER,
                                                  AnimationControlsWindow.AreaRect.yMax + RECT_Y_SPACER,
                                                  _windowSize.x - (AnimationTexturePivotWindow.AreaRect.xMax + RECT_X_SPACER),
                                                  BonePropertiesWindow.AreaRect.height - (AnimationControlsWindow.AreaRect.height + RECT_Y_SPACER)
                                                  );
                }
                else
                {
                    _animationControlsX = BonePropertiesWindow.AreaRect.xMax + RECT_X_SPACER;
                    AnimationControlsWindow.AreaRect = new Rect(
                                                          _animationControlsX,
                                                          0,
                                                          _windowSize.x - _animationControlsX,
                                                          AnimationControlsWindow.Height
                                                          );

                    AnimationWindow.AreaRect = new Rect(
                                                  BonePropertiesWindow.AreaRect.xMax + RECT_X_SPACER,
                                                  AnimationControlsWindow.AreaRect.yMax + RECT_Y_SPACER,
                                                  _windowSize.x - (BonePropertiesWindow.AreaRect.xMax + RECT_X_SPACER),
                                                  BonePropertiesWindow.AreaRect.height - (AnimationControlsWindow.AreaRect.height + RECT_Y_SPACER)
                                                  );
                }
            }


            BoneWindow.AreaRect = new Rect(
                                      0,
                                      BonePropertiesWindow.AreaRect.yMax + RECT_Y_SPACER,
                                      BoneWindow.BoneWindowX,
                                      _windowSize.y - (BonePropertiesWindow.AreaRect.yMax + RECT_Y_SPACER)
                                      );

            TimelineWindow.AreaRect = new Rect(
                                         BoneWindow.AreaRect.xMax + RECT_X_SPACER,
                                          BoneWindow.AreaRect.y,
                                         clipBrowserX - RECT_X_SPACER - (BoneWindow.AreaRect.xMax + RECT_X_SPACER),
                                          BoneWindow.AreaRect.height
                                         );

            ClipBrowserWindow.AreaRect = new Rect(
                                            clipBrowserX,
                                            BoneWindow.AreaRect.y,
                                            ClipBrowserWindow.ClipBrowserWidth,
                                            BoneWindow.AreaRect.height
                                            );

            SetAtlasesWindow.AreaRect = new Rect(
                                              (position.width / 2.0f) - (SetAtlasesWindow.WindowWidth / 2.0f),
                                              AnimationWindow.AreaRect.y + 30.0f,
                                              SetAtlasesWindow.WindowWidth,
                                              SetAtlasesWindow.WindowHeight
                                              );

            BoneColorWindow.AreaRect = new Rect(
                                              BoneWindow.AreaRect.xMax + PADDING,
                                              BoneWindow.AreaRect.yMin,
                                              BoneColorWindow.WindowWidth,
                                              BoneColorWindow.WindowHeight
                                              );

            SettingsWindow.AreaRect = new Rect(
                                              (position.width / 2.0f) - (SettingsWindow.WindowWidth / 2.0f),
                                              AnimationWindow.AreaRect.y + 30.0f,
                                              SettingsWindow.WindowWidth,
                                              SettingsWindow.WindowHeight
                                              );

            _resizeBottomRect = new Rect(
                                          0,
                                          BonePropertiesWindow.AreaRect.yMax - RECT_Y_SPACER,
                                          _windowSize.x,
                                          RECT_Y_SPACER * 4.0f
                                          );

            _resizeBoneWindowRect = new Rect(
                                             BoneWindow.AreaRect.xMax,
                                             BoneWindow.AreaRect.y,
                                             RECT_X_SPACER * 4.0f,
                                             BoneWindow.AreaRect.height
                                             );

            if (addCursorRects && !ModalPopup)
            {
                EditorGUIUtility.AddCursorRect(_resizeBottomRect, MouseCursor.ResizeVertical);
                EditorGUIUtility.AddCursorRect(_resizeBoneWindowRect, MouseCursor.ResizeHorizontal);
            }

            BonePropertiesWindow.SetRects();
            AnimationWindow.SetRects();
            BoneWindow.SetRects();
            TimelineWindow.SetRects();
            ClipBrowserWindow.SetRects();
        }

        private void GetInput(Event evt)
        {
            if (_resizeBottomRect.Contains(evt.mousePosition))
            {
                switch (evt.type)
                {
                    case EventType.MouseDown:
                        _resizeWindowMode = RESIZE_WINDOW_MODE.ResizeBottom;
                        evt.Use();
                        break;

                    case EventType.MouseDrag:
                        if (_resizeWindowMode == RESIZE_WINDOW_MODE.ResizeBottom)
                        {
                            evt.Use();
                        }
                        break;
                }
            }
            else if (_resizeBoneWindowRect.Contains(evt.mousePosition))
            {
                switch (evt.type)
                {
                    case EventType.MouseDown:
                        _resizeWindowMode = RESIZE_WINDOW_MODE.ResizeBoneWindow;
                        evt.Use();
                        break;

                    case EventType.MouseDrag:
                        if (_resizeWindowMode == RESIZE_WINDOW_MODE.ResizeBoneWindow)
                        {
                            evt.Use();
                        }
                        break;
                }
            }

            if (evt.type == EventType.MouseUp && _resizeWindowMode != RESIZE_WINDOW_MODE.None)
            {
                _resizeWindowMode = RESIZE_WINDOW_MODE.None;
                evt.Use();
            }
        }

        public bool CheckForDoubleClick(bool sameIndex)
        {
            if (!sameIndex)
            {
                _clickedTime = EditorApplication.timeSinceStartup;
                return false;
            }

            bool doubleClicked = false;

            if ((EditorApplication.timeSinceStartup - _clickedTime) < DOUBLE_CLICK_THRESHOLD)
                doubleClicked = true;

            _clickedTime = EditorApplication.timeSinceStartup;

            return doubleClicked;
        }

        private void RecalculateTextureIndices()
        {
            if (boneAnimationData != null)
            {
                boneAnimationData.RecalculateTextureIndices();
            }
        }

        public void SetTimeMode(TIME_MODE timeMode)
        {
            _timeMode = timeMode;

            switch (_timeMode)
            {
                case TIME_MODE.Frames:
                    TimelineWindow.Visible = true;
                    break;

                case TIME_MODE.AnimationCurves:
                    TimelineWindow.Visible = false;
                    break;
            }
        }

        public void UndoPerformed()
        {
            AnimationHelper.GenerateAnimationCurves(ClipBrowserWindow.CurrentClip);
            TimelineWindow.SetFrameGridNeedsRebuilt();

            if (AnimationCurveEditorWindow.Instance != null)
            {
                //AnimationHelper.AddBoneDataIndexToRefreshList(AnimationCurveEditorWindow.Instance.BoneDataIndex);
                AnimationHelper.RefreshCurveEditorPostCycle();
            }

            SetNeedsRepainted();
        }

        public void SetWillBeDirtyNoSnapshot()
        {
            _willBeDirty = true;
        }
        
        public void SetWillBeDirty()
        {
            Snapshot();
            _willBeDirty = true;
        }

        private void Snapshot()
        {
            if (!_createdSnapshot)
            {
                Undo.RecordObject(boneAnimationData, "BoneAnimationData");
                //Undo.SetSnapshotTarget(boneAnimationData, "Bone Animation Data");
                //Undo.CreateSnapshot();
                //Undo.ClearSnapshotTarget();

                _createdSnapshot = true;
            }
        }

        public void SetNeedsRepainted()
        {
            _needsRepainted = true;
        }

        public void Dirty()
        {
            if (boneAnimationObject != null)
            {
                if (_createdSnapshot)
                {
                    //Undo.SetSnapshotTarget(boneAnimationData, "Bone Animation Data");
                    //Undo.RegisterSnapshot();
                    //Undo.ClearSnapshotTarget();

                    _createdSnapshot = false;
                }

                boneAnimationData.GenerateBuildID();
                EditorUtility.SetDirty(boneAnimationObject);
            }
        }

        public void RefreshDefaultPivot()
        {
            if (boneAnimationObject != null)
            {
                AnimationHelper.GenerateAnimationCurves(ClipBrowserWindow.CurrentClip);
                EditorUtility.SetDirty(boneAnimationObject);
                Repaint();
            }
        }
    }
}
