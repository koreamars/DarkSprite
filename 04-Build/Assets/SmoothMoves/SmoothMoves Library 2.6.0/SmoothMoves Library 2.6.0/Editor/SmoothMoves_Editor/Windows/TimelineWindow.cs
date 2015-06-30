using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace SmoothMoves
{
    static public class LastKeyframe
    {
        static public KeyframeSM.KEYFRAME_TYPE keyframeType;
        static public TextureAtlas atlas;
        static public string textureGUID;
        static public int textureIndex;
        static public Vector2 pivotOffset;
        static public bool useDefaultPivot;
        static public int depth;
        static public ColliderSM collider = new ColliderSM();
    }

    public class SlidingKeyframe
    {
        public int frame;
        public int originalFrame;
        public int boneNodeIndex;
        public int boneDataIndex;

        public SlidingKeyframe(int f, int bni, int bdi)
        {
            frame = f;
            originalFrame = frame;
            boneNodeIndex = bni;
            boneDataIndex = bdi;
        }
    }

    static public class TimelineWindow
    {
        public const float MIN_WIDTH = 450.0f;
        public const float TIMELINE_FRAME_WIDTH = 20.0f;
        public const float TIMELINE_FRAME_HEIGHT = 23.0f;
        private const int FIRST_EDITABLE_FRAME = 0;
        private const int TIMELINE_FRAME_LABEL_INTERVAL = 5;

        public enum PLAY_DIRECTION
        {
            Forward = 1,
            Reverse = -1
        }

        static private bool _draggingTime;
        static private BoneFrame _workingStartFrameSelection;
        static private BoneFrame _workingEndFrameSelection;
        static private BoneFrame _startFrameSelection;
        static private BoneFrame _endFrameSelection;
        static private List<BoneFrame> _selectedFrames;
        static private List<KeyframeSM> _selectedKeyframes;
		static private List<KeyframeSM> _clipboardKeyframes;
        static private bool _selectingFrames;
        static private KeyframeSM _lastSelectedKeyframe;
        static private BoneFrame _shiftStartBone;
        static private GUIStyle[,] _frameGrid;
        static private int _frameGridBoneCount;
        static private int _frameGridFrameCount;
        static private bool _frameGridNeedsRebuilt;
        static private int _lastSelectedTimelineFrame;

        static private PLAY_DIRECTION _playDirection;
        static private int _currentFrame;
        static private float _frameTime;
        static private bool _isPlaying;
        static private int _visibleFrameCount;
        static private int _firstVisibleFrame;
        static private bool _selectFramePostCycle;
        static private BoneFrame _selectBoneFrame;
        static private bool _visible;

        static private bool _keyframesAreSliding;
        static private List<SlidingKeyframe> _slidingKeyframes;
        static private Vector2 _slidingKeyframeStartPosition;
        static private Rect _selectionBounds;
        static private Rect _originalSelectionBounds;
        static private int _lastFrameOffset;

        static private Rect _areaRect;
        static private Rect _lastAreaRect;
        static private Rect _currentFrameLineRect;
        static private Rect _currentFrameMarkerRect;
        static private Rect _animationClipNameRect;
        static private Rect _timeLineLabelRect;
        static private Rect _timeLineRect;
        static private Rect _moveSelectedFramesBackwardRect;
        static private Rect _moveSelectedFramesForwardRect;

        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }

        static public Rect AreaRect { get { return _areaRect; } set { _areaRect = value; } }
        static public bool Visible { get { return _visible; } set { _visible = value; } }
		static public bool IsPlaying { get { return _isPlaying; } }
        static public PLAY_DIRECTION PlayDirection { get { return _playDirection; } }
        static public KeyframeSM LastKeyframeSelected { get { return _lastSelectedKeyframe; } set { _lastSelectedKeyframe = value; } }
        static public List<BoneFrame> SelectedFrames { get { return _selectedFrames; } }
        static public int SelectedKeyframeCount { get { return _selectedKeyframes.Count; } }
        static public bool OneKeyframeSelected { get { return _selectedKeyframes.Count == 1; } }
        static public bool KeyframesSelected { get { return _selectedKeyframes.Count > 0; } }
        static public bool NoKeyframesSelected { get { return _selectedKeyframes.Count == 0; } }
        static public bool MultipleKeyframesSelected { get { return _selectedKeyframes.Count > 1; } }
        static public KeyframeSM FirstSelectedKeyframe { get { if (_selectedKeyframes.Count == 0) return null; else return _selectedKeyframes[0]; } }
        static public BoneFrame LastSelectedFrame { get { if (_selectedFrames.Count == 0) return null; else return _selectedFrames[_selectedFrames.Count-1]; } }
        static public int FirstVisibleFrame 
        { 
            get 
            { 
                return _firstVisibleFrame; 
            } 
            set 
            { 
                if (_firstVisibleFrame != value)
                    SetFrameGridNeedsRebuilt();
                _firstVisibleFrame = value; 
            } 
        }
        static public bool AllSelectedKeyframesInSameFrame
        {
            get
            {
                if (_selectedKeyframes.Count == 0)
                    return false;

                int frame = _selectedKeyframes[0].frame;

                foreach (KeyframeSM keyframe in _selectedKeyframes)
                {
                    if (keyframe.frame != frame)
                        return false;
                }

                return true;
            }
        }
        static public int CurrentFrame { get { return _currentFrame; } } // set { _currentFrame = value; } }
        static public float FrameTime { get { return _frameTime; } set { _frameTime = value; } }
        static public float RealTime { get { return _frameTime / ClipBrowserWindow.CurrentClip.fps; } }
		static public string RealTimeString
		{
			get
			{
                return EditorHelper.RoundFloat(RealTime, 2).ToString();
			}
		}

        static public void OnEnable()
        {
            _selectedFrames = null;
            _selectedKeyframes = new List<KeyframeSM>();
		    _clipboardKeyframes = new List<KeyframeSM>();

            _shiftStartBone = new BoneFrame(-1, -1, -1);

            _playDirection = PLAY_DIRECTION.Forward;
            FirstVisibleFrame = 0;

            _moveSelectedFramesBackwardRect = new Rect(0, 0, Resources.moveSelectedFramesBackward_On.width, Resources.moveSelectedFramesBackward_On.height);
            _moveSelectedFramesForwardRect = new Rect(0, 0, Resources.moveSelectedFramesForward_On.width, Resources.moveSelectedFramesForward_On.height);

            SetFrameGridNeedsRebuilt();
        }


        static public void SetRects()
        {
            _animationClipNameRect = new Rect(
                                              BoneAnimationDataEditorWindow.PADDING,
                                              BoneAnimationDataEditorWindow.PADDING,
                                              280.0f,
                                              20.0f
                                              );

            _timeLineLabelRect = new Rect(
                                          BoneAnimationDataEditorWindow.PADDING,
                                          BoneAnimationDataEditorWindow.TOOLBAR_HEIGHT - TIMELINE_FRAME_HEIGHT - 1.0f,
                                          _areaRect.width - (BoneAnimationDataEditorWindow.PADDING * 2.0f),
                                          TIMELINE_FRAME_HEIGHT + 4.0f
                                          );

            _timeLineRect = new Rect(
                                     BoneAnimationDataEditorWindow.PADDING,
                                     BoneAnimationDataEditorWindow.TOOLBAR_HEIGHT + BoneAnimationDataEditorWindow.PADDING,
                                     _areaRect.width - (BoneAnimationDataEditorWindow.PADDING * 2.0f),
                                     _areaRect.height - (BoneAnimationDataEditorWindow.TOOLBAR_HEIGHT + (BoneAnimationDataEditorWindow.PADDING * 2.0f)) - BoneAnimationDataEditorWindow.SCROLLBAR_HEIGHT
                                     );

            _visibleFrameCount = Mathf.FloorToInt(_timeLineRect.width / TIMELINE_FRAME_WIDTH) - 1;
        }

        static public void OnGUI()
        {
            if (_visible)
            {
                if (_areaRect != _lastAreaRect)
                {
                    _frameGridNeedsRebuilt = true;
                }
                _lastAreaRect = _areaRect;

                if (_frameGridNeedsRebuilt)
                    BuildFrameGrid();

                GUILayout.BeginArea(_areaRect, GUIContent.none, Style.windowRectBackgroundStyle);

                GUILayout.BeginVertical();

                if (ClipBrowserWindow.CurrentClip != null)
                {
                    Style.OnGUI();

                    float newFPS;
                    WrapMode newWrapMode;
                    AnimationClipSM clip = ClipBrowserWindow.CurrentClip;

                    GUIHelper.DrawBox(_timeLineRect, Style.windowRectBackgroundStyle, true);

                    GUIHelper.DrawBox(new Rect(_timeLineRect.x,
                                                    _timeLineRect.y,
                                                    _timeLineRect.width,
                                                    (Mathf.Min(BoneWindow.FirstVisibleBone + BoneWindow.VisibleBoneCount, editor.boneAnimationData.BoneCount) - BoneWindow.FirstVisibleBone) * TIMELINE_FRAME_HEIGHT + 3.0f),
                                        Style.gridBackgroundStyle,
                                        false);

                    GUIHelper.DrawBox(_timeLineLabelRect, Style.gridBackgroundStyle, false);

                    GUIHelper.DrawBox(_animationClipNameRect, Style.selectedInformationStyle, true);

                    GUILayout.BeginVertical();

                    GUILayout.BeginHorizontal();

                    string shortAnimationName = ClipBrowserWindow.CurrentClip.animationName;
                    if (shortAnimationName.Length > 39)
                    {
                        shortAnimationName = shortAnimationName.Substring(0, 36) + "...";
                    }
                    GUIContent guiContent = new GUIContent(shortAnimationName, ClipBrowserWindow.CurrentClip.animationName);
                    GUILayout.Label(guiContent, Style.blankStyle, GUILayout.Width(_animationClipNameRect.width), GUILayout.Height(20.0f));

                    if (!editor.ModalPopup)
                    {
                        GUILayout.Label("  FPS: ", Style.normalLabelStyle, GUILayout.Width(30.0f));
                        newFPS = EditorGUILayout.FloatField(ClipBrowserWindow.CurrentClip.fps, GUILayout.Width(30.0f));
                        if (newFPS != clip.fps)
                        {
                            editor.SetWillBeDirty(); ;
                            
                            clip.SetFPS(newFPS);
                        }

                        if (!IsPlaying)
                        {
                            Dictionary<WrapMode, int> wrapModeDictionary = new Dictionary<WrapMode, int>();
                            string[] wrapModes = new string[4];
                            wrapModes[0] = WrapMode.Once.ToString();
                            wrapModes[1] = WrapMode.Loop.ToString();
                            wrapModes[2] = WrapMode.PingPong.ToString();
                            wrapModes[3] = WrapMode.ClampForever.ToString();
                            wrapModeDictionary.Add(WrapMode.Once, 0);
                            wrapModeDictionary.Add(WrapMode.Loop, 1);
                            wrapModeDictionary.Add(WrapMode.PingPong, 2);
                            wrapModeDictionary.Add(WrapMode.ClampForever, 3);
                            newWrapMode = (WrapMode)Enum.Parse(typeof(WrapMode), wrapModes[EditorGUILayout.Popup(wrapModeDictionary[clip.wrapMode], wrapModes, GUILayout.Width(90.0f))]);
                            if (newWrapMode != clip.wrapMode)
                            {
                                editor.SetWillBeDirty(); ;
                                
                                clip.wrapMode = newWrapMode;
                                _playDirection = PLAY_DIRECTION.Forward;
                            }
                        }

                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.Space(2.0f);

                    if (editor.ModalPopup || IsPlaying)
                    {
                        GUILayout.Space(18.0f);
                    }

                    if (!editor.ModalPopup && !IsPlaying)
                    {
                        GUILayout.BeginHorizontal();

                        bool newMix = GUILayout.Toggle(clip.mix, "Mix", Style.normalToggleStyle, GUILayout.Width(40.0f));
                        if (newMix != clip.mix)
                        {
                            editor.SetWillBeDirty(); ;
                            
                            clip.mix = newMix;
                        }

                        AnimationBlendMode newBlendMode = (AnimationBlendMode)EditorGUILayout.EnumPopup(clip.blendMode, GUILayout.Width(60.0f));
                        if (newBlendMode != clip.blendMode)
                        {
                            editor.SetWillBeDirty(); ;
                            
                            clip.blendMode = newBlendMode;
                        }

                        string[] layers = new string[AnimationClipSM.MAX_LAYERS + 1];
                        for (int l = 0; l <= AnimationClipSM.MAX_LAYERS; l++)
                        {
                            layers[l] = "Layer " + l.ToString();
                        }
                        int newLayer = EditorGUILayout.Popup(clip.layer, layers, GUILayout.Width(65.0f));
                        if (newLayer != clip.layer)
                        {
                            editor.SetWillBeDirty(); ;
                            
                            clip.layer = newLayer;
                        }

                        GUILayout.Space(5.0f);

                        GUILayout.Label("Weight:", Style.normalLabelStyle, GUILayout.Width(45.0f));

                        if (GUILayout.Button(new GUIContent(Resources.buttonSubtract, "Decrease Blend Weight"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonSubtract.width), GUILayout.Height(Resources.buttonSubtract.height)))
                        {
                            editor.SetWillBeDirty(); ;
                            
                            clip.blendWeight = Mathf.Round(Mathf.Clamp01((clip.blendWeight - 0.01f)) * 100.0f) / 100.0f;
                        }

                        float newBlendWeight = GUILayout.HorizontalSlider(clip.blendWeight, 0, 1.0f, GUILayout.Width(60.0f));
                        if (newBlendWeight != clip.blendWeight)
                        {
                            editor.SetWillBeDirty(); ;
                            
                            clip.blendWeight = Mathf.Round(Mathf.Clamp01(newBlendWeight) * 100.0f) / 100.0f;
                        }

                        if (GUILayout.Button(new GUIContent(Resources.buttonAdd, "Increase Blend Weight"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonAdd.width), GUILayout.Height(Resources.buttonAdd.height)))
                        {
                            editor.SetWillBeDirty(); ;
                            
                            clip.blendWeight = Mathf.Round(Mathf.Clamp01(clip.blendWeight + 0.01f) * 100.0f) / 100.0f;
                        }

                        GUILayout.Label((clip.blendWeight * 100.0f) + "%", Style.normalLabelStyle, GUILayout.Width(40.0f));

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(BoneAnimationDataEditorWindow.TOOLBAR_HEIGHT - 63.0f);


                    GUILayout.BeginHorizontal();
                    for (int frame = FirstVisibleFrame; frame <= FirstVisibleFrame + _frameGridFrameCount; frame++)
                    {
                        if (((frame % TIMELINE_FRAME_LABEL_INTERVAL) == 0) ||
                            (frame == FIRST_EDITABLE_FRAME) ||
                            (frame == (AnimationClipSM.MAX_FRAMES))
                            )
                        {
                            GUILayout.Label(frame.ToString(), Style.timelineLabelStyle, GUILayout.Width(TIMELINE_FRAME_WIDTH), GUILayout.Height(TIMELINE_FRAME_HEIGHT));
                        }
                        else
                        {
                            GUILayout.Label("", Style.centeredTextStyle, GUILayout.Width(TIMELINE_FRAME_WIDTH), GUILayout.Height(TIMELINE_FRAME_HEIGHT));
                        }
                    }
                    GUILayout.EndHorizontal();


                    GUILayout.BeginVertical();

                    GUILayout.Space(2.0f);

                    if (ClipBrowserWindow.SelectedAnimationClipIndex != -1)
                    {
                        for (int x = 0; x < _frameGridBoneCount; x++)
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Space(2.0f);

                            for (int y = 0; y < _frameGridFrameCount; y++)
                            {
                                GUILayout.Space(1.0f);

                                GUILayout.Label("",
                                                _frameGrid[x, y],
                                                GUILayout.Width(TIMELINE_FRAME_WIDTH - 1.0f),
                                                GUILayout.Height(TIMELINE_FRAME_HEIGHT - 1.0f)
                                                );
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(1.0f);
                        }
                    }


                    GUILayout.EndVertical();


                    GUILayout.FlexibleSpace();


                    if (!editor.ModalPopup && !IsPlaying)
                    {
                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button(new GUIContent(Resources.buttonDoubleArrowBackwardSmall, "Move to Frame Zero"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonDoubleArrowBackwardSmall.width), GUILayout.Height(Resources.buttonDoubleArrowBackwardSmall.height)))
                        {
                            FirstVisibleFrame = 0;
                            editor.SetNeedsRepainted();
                            SetFrameGridNeedsRebuilt();
                        }
                        GUILayout.Space(2.0f);
                        if (GUILayout.Button(new GUIContent(Resources.buttonArrowBackwardSmall, "Move Backward one Frame"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonArrowBackwardSmall.width), GUILayout.Height(Resources.buttonArrowBackwardSmall.height)))
                        {
                            FirstVisibleFrame = Mathf.Clamp(FirstVisibleFrame - 1, 0, AnimationClipSM.MAX_FRAMES - _visibleFrameCount);
                            editor.SetNeedsRepainted();
                            SetFrameGridNeedsRebuilt();
                        }

                        FirstVisibleFrame = Convert.ToInt16(GUILayout.HorizontalSlider((float)FirstVisibleFrame, FIRST_EDITABLE_FRAME, AnimationClipSM.MAX_FRAMES - _visibleFrameCount, GUILayout.Width((_visibleFrameCount * TIMELINE_FRAME_WIDTH - 120.0f - 120.0f) + (TIMELINE_FRAME_WIDTH * 0.5f))));

                        if (GUILayout.Button(new GUIContent(Resources.buttonArrowForwardSmall, "Move Forward one Frame"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonArrowForwardSmall.width), GUILayout.Height(Resources.buttonArrowForwardSmall.height)))
                        {
                            FirstVisibleFrame = Mathf.Clamp(FirstVisibleFrame + 1, 0, AnimationClipSM.MAX_FRAMES - _visibleFrameCount);
                            editor.SetNeedsRepainted();
                            SetFrameGridNeedsRebuilt();
                        }
                        GUILayout.Space(2.0f);
                        if (GUILayout.Button(new GUIContent(Resources.buttonDoubleArrowForwardSmall, "Move to Last Used Frame"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonDoubleArrowForwardSmall.width), GUILayout.Height(Resources.buttonDoubleArrowForwardSmall.height)))
                        {
                            ClampFrame(clip.maxFrame, clip.maxFrame);
                            editor.SetNeedsRepainted();
                            SetFrameGridNeedsRebuilt();
                        }

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(new GUIContent(Resources.buttonRemoveBlankKeyframes, "Delete Blank Keyframes"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonRemoveBlankKeyframes.width), GUILayout.Height(Resources.buttonRemoveBlankKeyframes.height)))
                        {
                            editor.SetWillBeDirty();
                            
                            editor.boneAnimationData.RemoveBlankKeyframes(ClipBrowserWindow.SelectedAnimationClipIndex);

                            ResetSelectedFrames();
                            editor.SetNeedsRepainted();
                            SetFrameGridNeedsRebuilt();
                        }

                        GUILayout.Space(10.0f);

                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Space(BoneAnimationDataEditorWindow.SCROLLBAR_HEIGHT);
                    }



                    GUILayout.EndVertical();

                    GUI.BeginGroup(_timeLineRect);

                    if (_currentFrame >= FirstVisibleFrame && _currentFrame <= (FirstVisibleFrame + _visibleFrameCount + 1))
                    {
                        _currentFrameLineRect.x = ((_frameTime) * TIMELINE_FRAME_WIDTH) - (FirstVisibleFrame * TIMELINE_FRAME_WIDTH);
                        _currentFrameLineRect.y = 0;
                        _currentFrameLineRect.width = 5.0f;
                        _currentFrameLineRect.height = _timeLineRect.height;

                        GUIHelper.DrawVerticalLine(new Vector2(_currentFrameLineRect.x, 0),
                                             _timeLineRect.height,
                                             4.0f,
                                             Style.targetStyle);
                        GUIHelper.DrawVerticalLine(new Vector2(_currentFrameLineRect.x + 2.0f, 0),
                                             _timeLineRect.height,
                                             1.0f,
                                             Style.whiteStyle);
                    }

                    GUI.EndGroup();

                    GUI.BeginGroup(_timeLineLabelRect);

                    _currentFrameMarkerRect.x = ((_frameTime) * TIMELINE_FRAME_WIDTH) - (FirstVisibleFrame * TIMELINE_FRAME_WIDTH) - 39.0f;
                    _currentFrameMarkerRect.y = 0;
                    _currentFrameMarkerRect.width = 80.0f;
                    _currentFrameMarkerRect.height = _timeLineLabelRect.height;

                    GUIHelper.DrawBox(_currentFrameMarkerRect, Style.currentFrameMarkerStyle, true);

                    GUI.Label(new Rect(_currentFrameMarkerRect.x + 3.0f,
                                        _currentFrameMarkerRect.y + 5.0f,
                                        _currentFrameMarkerRect.width - 6.0f,
                                        20.0f),
                                        _currentFrame.ToString() + "/" + clip.maxFrame.ToString(),
                                        Style.currentFrameMarkerStyle);

                    GUI.EndGroup();

                }

                GUILayout.EndVertical();


                if (_selectedKeyframes.Count > 0)
                {
                    GUI.DrawTexture(_moveSelectedFramesBackwardRect, (_keyframesAreSliding ? Resources.moveSelectedFramesBackward_On : Resources.moveSelectedFramesBackward_Off));
                    GUI.DrawTexture(_moveSelectedFramesForwardRect, (_keyframesAreSliding ? Resources.moveSelectedFramesForward_On : Resources.moveSelectedFramesForward_Off));
                }

                GUILayout.EndArea();
            }
        }

        static public void GetInput(Event evt)
        {
            if (_visible)
            {
                Vector2 areaMousePos;
                Vector2 localMousePos;
                int boneNodeIndex = -1;
                int boneDataIndex;
                int frame = -1;
                float time = 0;
                BoneFrame boneFrame;
                bool keyframeIsSet;
                bool useSelection;

                if (evt.type == EventType.MouseDown && _areaRect.Contains(evt.mousePosition))
                {
                    editor.KeyboardFocus = BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.Timeline;
                }

                // global arrow keys
                if (evt.type == EventType.KeyDown)
                {
                    GetSelectedUpperLeftCorner(out boneNodeIndex, out frame);

                    if (frame != -1)
                    {
                        switch (evt.keyCode)
                        {
                            case KeyCode.UpArrow:
                                boneNodeIndex = Mathf.Clamp(boneNodeIndex - 1, 1, editor.boneAnimationData.dfsBoneNodeList.Count - 1);
                                _selectFramePostCycle = true;
                                _selectBoneFrame = new BoneFrame(editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex), boneNodeIndex, frame);
                                editor.SetNeedsRepainted();
                                SetFrameGridNeedsRebuilt();
                                evt.Use();
                                break;

                            case KeyCode.DownArrow:
                                boneNodeIndex = Mathf.Clamp(boneNodeIndex + 1, 1, editor.boneAnimationData.dfsBoneNodeList.Count - 1);
                                _selectFramePostCycle = true;
                                _selectBoneFrame = new BoneFrame(editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex), boneNodeIndex, frame);
                                editor.SetNeedsRepainted();
                                SetFrameGridNeedsRebuilt();
                                evt.Use();
                                break;

                            case KeyCode.LeftArrow:
                                frame = Mathf.Clamp(frame - 1, 0, AnimationClipSM.MAX_FRAMES);
                                _selectFramePostCycle = true;
                                _selectBoneFrame = new BoneFrame(editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex), boneNodeIndex, frame);
                                editor.SetNeedsRepainted();
                                SetFrameGridNeedsRebuilt();
                                evt.Use();
                                break;

                            case KeyCode.RightArrow:
                                frame = Mathf.Clamp(frame + 1, 0, AnimationClipSM.MAX_FRAMES);
                                _selectFramePostCycle = true;
                                _selectBoneFrame = new BoneFrame(editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex), boneNodeIndex, frame);
                                editor.SetNeedsRepainted();
                                SetFrameGridNeedsRebuilt();
                                evt.Use();
                                break;

                            case KeyCode.B:
                                useSelection = false;
                                if (_selectedFrames != null && !_keyframesAreSliding)
                                {
                                    if (_selectedFrames.Count > 1)
                                    {
                                        if (IsFrameSelected(boneNodeIndex, frame))
                                        {
                                            useSelection = true;
                                        }
                                    }
                                }

                                if (useSelection)
                                {
                                    AddSelectedKeyframes(ClipBrowserWindow.SelectedAnimationClipIndex, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.Blank, true);
                                }
                                else
                                {
                                    if (_selectedFrames.Count > 0)
                                    {
                                        AddKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, (int)_selectionBounds.yMin, (int)_selectionBounds.xMin, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.Blank, true);
                                    }
                                }
                                break;

                            case KeyCode.P:
                                useSelection = false;
                                if (_selectedFrames != null && !_keyframesAreSliding)
                                {
                                    if (_selectedFrames.Count > 1)
                                    {
                                        if (IsFrameSelected(boneNodeIndex, frame))
                                        {
                                            useSelection = true;
                                        }
                                    }
                                }

                                if (useSelection)
                                {
                                    AddSelectedKeyframes(ClipBrowserWindow.SelectedAnimationClipIndex, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.PositionRotation, true);
                                }
                                else
                                {
                                    if (_selectedFrames.Count > 0)
                                    {
                                        AddKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, (int)_selectionBounds.yMin, (int)_selectionBounds.xMin, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.PositionRotation, true);
                                    }
                                }
                                break;

                            case KeyCode.T:
                                useSelection = false;
                                if (_selectedFrames != null && !_keyframesAreSliding)
                                {
                                    if (_selectedFrames.Count > 1)
                                    {
                                        if (IsFrameSelected(boneNodeIndex, frame))
                                        {
                                            useSelection = true;
                                        }
                                    }
                                }

                                if (useSelection)
                                {
                                    AddSelectedKeyframes(ClipBrowserWindow.SelectedAnimationClipIndex, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.Texture, true);
                                }
                                else
                                {
                                    if (_selectedFrames.Count > 0)
                                    {
                                        AddKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, (int)_selectionBounds.yMin, (int)_selectionBounds.xMin, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.Texture, true);
                                    }
                                }
                                break;
                        }
                    }
                }


                if (editor.KeyboardFocus == BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.Timeline)
                {
                    switch (evt.type)
                    {
                        case EventType.ValidateCommand:
                            if (evt.commandName == "Copy")
                            {
                                CopyKeyframesToClipboard();
                                evt.Use();
                            }
                            else if (evt.commandName == "Cut")
                            {
                                CutSelectedKeyframes();
                                evt.Use();
                            }
                            else if (evt.commandName == "Paste")
                            {
                                PasteClipboardKeyframes();
                                evt.Use();
                            }
                            break;

                        case EventType.KeyDown:
                            GetSelectedUpperLeftCorner(out boneNodeIndex, out frame);

                            if (frame > -1)
                            {
                                switch (evt.keyCode)
                                {
                                    case KeyCode.Delete:
                                    case KeyCode.Backspace:
                                        if (_selectedKeyframes.Count > 0)
                                        {
                                            RemoveSelectedKeyframes(ClipBrowserWindow.SelectedAnimationClipIndex);
                                        }
                                        evt.Use();
                                        break;


                                }
                            }
                            break;
                    }
                }

                if (ClipBrowserWindow.CurrentClip != null)
                {
                    if (!_isPlaying)
					{
                        if (_areaRect.Contains(evt.mousePosition))
                        {
                            areaMousePos = evt.mousePosition - GUIHelper.UpperLeftCorner(_areaRect);
							
                            if (_timeLineLabelRect.Contains(areaMousePos))
                            {
                                localMousePos = areaMousePos - GUIHelper.UpperLeftCorner(_timeLineLabelRect);

                                switch (evt.type)
                                {
                                    case EventType.MouseDown:
                                        if (EditorHelper.LeftMouseButton(evt))
                                        {
                                            GetTimeFromMousePosition(localMousePos, ref time);

                                            if (time >= 0)
                                                SetFrameTime(time);

                                            ResetSelectedFrames();

                                            _draggingTime = true;

                                            if (editor.CheckForDoubleClick(_lastSelectedTimelineFrame == _currentFrame))
                                            {
                                                SelectColumn(_currentFrame);
                                            }

                                            _lastSelectedTimelineFrame = _currentFrame;

                                            evt.Use();
                                        }
                                        else if (EditorHelper.RightMouseButton(evt))
                                        {
                                            GetBoneIndexAndFrameFromMousePosition(localMousePos, ref boneNodeIndex, ref frame);

                                            SelectColumn(frame);

                                            GenericMenu frameContextMenu = new GenericMenu();
                                            frameContextMenu.AddItem(new GUIContent("Set Blank Keyframes for Column"), false, FrameContextMenuCallback_AddSelectedKeyframes, null);
                                            frameContextMenu.AddSeparator("");
                                            frameContextMenu.AddItem(new GUIContent("Delete All Keyframes in Column"), false, FrameContextMenuCallback_RemoveSelectedKeyframes, null);
                                            frameContextMenu.AddSeparator("");
                                            frameContextMenu.AddItem(new GUIContent("Duplicate Previous Keyframes in Column"), false, FrameContextMenuCallback_DuplicateLastSelectedKeyframes, null);
                                            frameContextMenu.AddItem(new GUIContent("Duplicate First Keyframes in Column"), false, FrameContextMenuCallback_DuplicateFirstSelectedKeyframes, null);
                                            frameContextMenu.AddSeparator("");
                                            frameContextMenu.AddItem(new GUIContent("Insert Column of Frames"), false, FrameContextMenuCallback_InsertFramesAtSelection, null);
                                            frameContextMenu.AddItem(new GUIContent("Delete Column of Frames"), false, FrameContextMenuCallback_DeleteFramesAtSelection, null);
                                            frameContextMenu.ShowAsContext();
                                            evt.Use();
                                        }
                                        break;

                                    case EventType.MouseDrag:
                                        if (EditorHelper.LeftMouseButton(evt) && _draggingTime)
                                        {
                                            GetTimeFromMousePosition(localMousePos, ref time);

                                            if (time >= 0)
                                                SetFrameTime(time);

                                            evt.Use();
                                        }
                                        break;

                                    case EventType.MouseUp:
                                        _draggingTime = false;
                                        break;

                                    case EventType.ScrollWheel:
                                        AdvanceFrame(Convert.ToInt16(Mathf.Sign(-evt.delta.y)));
                                        evt.Use();
                                        break;
                                }
                            }
                            else if (_timeLineRect.Contains(areaMousePos))
                            {
                                localMousePos = areaMousePos - GUIHelper.UpperLeftCorner(_timeLineRect);

                                boneNodeIndex = -1;
                                frame = -1;

                                switch (evt.type)
                                {
                                    case EventType.MouseDown:

                                        if (_selectedKeyframes.Count > 0)
                                        {
                                            if (evt.button == 0)
                                            {
                                                if (_moveSelectedFramesBackwardRect.Contains(areaMousePos)
                                                    ||
                                                    _moveSelectedFramesForwardRect.Contains(areaMousePos)
                                                    )
                                                {
                                                    StartSliding(areaMousePos);
                                                }
                                            }
                                        }

                                        if (_keyframesAreSliding)
                                        {
                                            SetFrameGridNeedsRebuilt();
                                            editor.SetNeedsRepainted();
                                        }
                                        else
                                        {
                                            if (EditorHelper.RightMouseButton(evt))
                                            {
                                                GetBoneIndexAndFrameFromMousePosition(localMousePos, ref boneNodeIndex, ref frame);
                                                boneDataIndex = editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex);

                                                useSelection = false;
                                                if (_selectedFrames != null)
                                                {
                                                    if (_selectedFrames.Count > 1)
                                                    {
                                                        if (IsFrameSelected(boneNodeIndex, frame))
                                                        {
                                                            useSelection = true;
                                                        }
                                                        else
                                                        {
                                                            ResetSelectedFrames();
                                                        }
                                                    }
                                                }

                                                if (useSelection)
                                                {
                                                    GenericMenu frameContextMenu = new GenericMenu();
                                                    if (_selectedKeyframes.Count == 0)
                                                    {
                                                        frameContextMenu.AddItem(new GUIContent("Set Blank Keyframes"), false, FrameContextMenuCallback_AddSelectedKeyframes, null);
                                                        frameContextMenu.AddItem(new GUIContent("Set Pos - Rot Keyframes"), false, FrameContextMenuCallback_AddSelectedPositionRotationKeyframes, null);
                                                    }
                                                    if (_currentFrame > 0)
                                                    {
                                                        frameContextMenu.AddItem(new GUIContent("Duplicate Previous Keyframes"), false, FrameContextMenuCallback_DuplicateLastSelectedKeyframes, null);
                                                        frameContextMenu.AddItem(new GUIContent("Duplicate First Keyframes"), false, FrameContextMenuCallback_DuplicateFirstSelectedKeyframes, null);
                                                    }
                                                    frameContextMenu.AddSeparator("");
                                                    if (_selectedKeyframes.Count > 0)
                                                        frameContextMenu.AddItem(new GUIContent("Cut Keyframes"), false, FrameContextMenuCallback_CutKeyframesToClipboard, null);
                                                    if (_selectedKeyframes.Count > 0)
                                                        frameContextMenu.AddItem(new GUIContent("Copy Keyframes"), false, FrameContextMenuCallback_CopyKeyframesToClipboard, null);
                                                    if (_clipboardKeyframes.Count > 0)
                                                        frameContextMenu.AddItem(new GUIContent("Paste Keyframes" + (_clipboardKeyframes.Count > 1 ? "s" : "")), false, FrameContextMenuCallback_PasteKeyframesFromClipboard, null);
                                                    frameContextMenu.AddSeparator("");
                                                    if (KeyframesSelected)
                                                    {
                                                        frameContextMenu.AddItem(new GUIContent("Delete Keyframes"), false, FrameContextMenuCallback_RemoveSelectedKeyframes, null);
                                                        frameContextMenu.AddSeparator("");
                                                        frameContextMenu.AddItem(new GUIContent("Reset Transforms"), false, FrameContextMenuCallback_ResetTransform, null);
                                                        if (MultipleKeyframesSelected)
                                                        {
                                                            frameContextMenu.AddItem(new GUIContent("Set Atlases"), false, FrameContextMenuCallback_EditKeyframes, null);
                                                        }
                                                    }
                                                    frameContextMenu.AddSeparator("");
                                                    frameContextMenu.AddItem(new GUIContent("Insert Frames"), false, FrameContextMenuCallback_InsertFramesAtSelection, null);
                                                    frameContextMenu.AddItem(new GUIContent("Delete Frames"), false, FrameContextMenuCallback_DeleteFramesAtSelection, null);
                                                    frameContextMenu.ShowAsContext();
                                                    evt.Use();
                                                }
                                                else
                                                {
                                                    boneFrame = new BoneFrame(boneDataIndex, boneNodeIndex, frame);
                                                    keyframeIsSet = editor.boneAnimationData.GetKeyframeIsSet(ClipBrowserWindow.SelectedAnimationClipIndex, boneNodeIndex, frame);

                                                    SelectFrame(boneNodeIndex, frame);

                                                    GenericMenu frameContextMenu = new GenericMenu();
                                                    if (!keyframeIsSet)
                                                    {
                                                        frameContextMenu.AddItem(new GUIContent("Set Blank Keyframe"), false, FrameContextMenuCallback_AddKeyframe, boneFrame);
                                                        frameContextMenu.AddItem(new GUIContent("Set Pos - Rot Keyframe"), false, FrameContextMenuCallback_AddPositionRotationKeyframe, boneFrame);
                                                        if (_currentFrame > 0)
                                                        {
                                                            frameContextMenu.AddItem(new GUIContent("Duplicate Previous Keyframe"), false, FrameContextMenuCallback_DuplicatePreviousKeyframe, boneFrame);
                                                            frameContextMenu.AddItem(new GUIContent("Duplicate First Keyframe"), false, FrameContextMenuCallback_DuplicateFirstKeyframe, boneFrame);
                                                        }
                                                        frameContextMenu.AddSeparator("");
                                                        if (_clipboardKeyframes.Count > 0)
                                                            frameContextMenu.AddItem(new GUIContent("Paste Keyframe" + (_clipboardKeyframes.Count > 1 ? "s" : "")), false, FrameContextMenuCallback_PasteKeyframesFromClipboard, null);
                                                    }
                                                    if (keyframeIsSet)
                                                    {
                                                        if (_currentFrame > 0)
                                                        {
                                                            frameContextMenu.AddItem(new GUIContent("Duplicate Previous Keyframe"), false, FrameContextMenuCallback_DuplicatePreviousKeyframe, boneFrame);
                                                            frameContextMenu.AddItem(new GUIContent("Duplicate First Keyframe"), false, FrameContextMenuCallback_DuplicateFirstKeyframe, boneFrame);
                                                            frameContextMenu.AddSeparator("");
                                                        }
                                                        if (_selectedKeyframes.Count > 0)
                                                            frameContextMenu.AddItem(new GUIContent("Cut Keyframe"), false, FrameContextMenuCallback_CutKeyframesToClipboard, null);
                                                        if (_selectedKeyframes.Count > 0)
                                                            frameContextMenu.AddItem(new GUIContent("Copy Keyframe"), false, FrameContextMenuCallback_CopyKeyframesToClipboard, null);
                                                        if (_clipboardKeyframes.Count > 0)
                                                            frameContextMenu.AddItem(new GUIContent("Paste Keyframe" + (_clipboardKeyframes.Count > 1 ? "s" : "")), false, FrameContextMenuCallback_PasteKeyframesFromClipboard, null);
                                                        frameContextMenu.AddSeparator("");
                                                        frameContextMenu.AddItem(new GUIContent("Delete Keyframe"), false, FrameContextMenuCallback_RemoveKeyframe, boneFrame);
                                                        frameContextMenu.AddSeparator("");
                                                        frameContextMenu.AddItem(new GUIContent("Reset Transform"), false, FrameContextMenuCallback_ResetTransform, boneFrame);
                                                    }
                                                    frameContextMenu.AddSeparator("");
                                                    frameContextMenu.AddItem(new GUIContent("Insert Frame"), false, FrameContextMenuCallback_InsertFramesAtSelection, null);
                                                    frameContextMenu.ShowAsContext();
                                                    evt.Use();
                                                }
                                            }
                                            else if (evt.button == 0)
                                            {
                                                // we use evt.button == 0 here instead of EditorUtilities.LeftMouseButton(evt) because 
                                                // EditorUtilities.LeftMouseButton(evt) excludes the shift key, which we need for 
                                                // selecting a range of frames.

                                                GetBoneIndexAndFrameFromMousePosition(localMousePos, ref boneNodeIndex, ref frame);

                                                if (evt.control)
                                                {
                                                    if (IsFrameSelected(boneNodeIndex, frame))
                                                    {
                                                        RemoveFrameFromSelection(boneNodeIndex, frame);
                                                    }
                                                    else
                                                    {
                                                        AddFrameToSelection(boneNodeIndex, frame);
                                                    }

                                                    _shiftStartBone.boneNodeIndex = boneNodeIndex;
                                                    _shiftStartBone.frame = frame;
                                                }
                                                else if (evt.shift)
                                                {
                                                    if (_shiftStartBone.boneNodeIndex == -1)
                                                    {
                                                        _shiftStartBone.boneNodeIndex = boneNodeIndex;
                                                        _shiftStartBone.frame = frame;
                                                    }

                                                    StartDragFrameSelection(_shiftStartBone.boneNodeIndex, _shiftStartBone.frame);
                                                    EndDragFrameSelection(boneNodeIndex, frame);
                                                }
                                                else
                                                {
                                                    _selectingFrames = true;
                                                    StartDragFrameSelection(boneNodeIndex, frame);

                                                    _shiftStartBone.boneNodeIndex = boneNodeIndex;
                                                    _shiftStartBone.frame = frame;
                                                }

                                                evt.Use();
                                            }
                                        }
                                        break;

                                    case EventType.MouseDrag:
                                        if (_keyframesAreSliding && EditorHelper.LeftMouseButton(evt))
                                        {
                                            Vector2 slidingOffset = areaMousePos - _slidingKeyframeStartPosition;
                                            int frameOffset = Mathf.FloorToInt(slidingOffset.x / TIMELINE_FRAME_WIDTH);

                                            if (Mathf.FloorToInt(localMousePos.x / TIMELINE_FRAME_WIDTH) == (_visibleFrameCount))
                                            {
                                                if (FirstVisibleFrame < (AnimationClipSM.MAX_FRAMES - _visibleFrameCount))
                                                {
                                                    FirstVisibleFrame++;
                                                    frameOffset++;
                                                    _slidingKeyframeStartPosition.x -= TIMELINE_FRAME_WIDTH;
                                                }
                                            }
                                            else if (Mathf.FloorToInt(localMousePos.x / TIMELINE_FRAME_WIDTH) <= 0)
                                            {
                                                if (FirstVisibleFrame > 0)
                                                {
                                                    FirstVisibleFrame--;
                                                    frameOffset--;
                                                    _slidingKeyframeStartPosition.x += TIMELINE_FRAME_WIDTH;
                                                }
                                            }

                                            if (_originalSelectionBounds.xMin + frameOffset < 0)
                                            {
                                                frameOffset = -(int)_originalSelectionBounds.xMin;
                                            }
                                            else if (_originalSelectionBounds.xMax + frameOffset > AnimationClipSM.MAX_FRAMES)
                                            {
                                                frameOffset = (AnimationClipSM.MAX_FRAMES - (int)_originalSelectionBounds.xMax);
                                            }

                                            if (frameOffset != _lastFrameOffset)
                                            {
                                                ShiftSlidingKeyframes(ref frameOffset);

                                                SetFrameGridNeedsRebuilt();
                                                editor.SetNeedsRepainted();
                                            }

                                            _lastFrameOffset = frameOffset;
                                        }
                                        else
                                        {
                                            if (EditorHelper.LeftMouseButton(evt) && _draggingTime)
                                            {
                                                GetTimeFromMousePosition(localMousePos, ref time);

                                                if (time >= 0)
                                                    SetFrameTime(time);

                                                evt.Use();
                                            }
                                            else if (EditorHelper.LeftMouseButton(evt) && _selectingFrames)
                                            {
                                                GetBoneIndexAndFrameFromMousePosition(localMousePos, ref boneNodeIndex, ref frame);
                                                EndDragFrameSelection(boneNodeIndex, frame);
                                                if (Mathf.FloorToInt(localMousePos.x / TIMELINE_FRAME_WIDTH) == (_visibleFrameCount))
                                                {
                                                    if (FirstVisibleFrame < (AnimationClipSM.MAX_FRAMES - _visibleFrameCount))
                                                    {
                                                        FirstVisibleFrame++;
                                                    }
                                                }
                                                else if (Mathf.FloorToInt(localMousePos.x / TIMELINE_FRAME_WIDTH) == 0)
                                                {
                                                    if (FirstVisibleFrame > 0)
                                                    {
                                                        FirstVisibleFrame--;
                                                    }
                                                }

                                                if (Mathf.FloorToInt(localMousePos.y / TIMELINE_FRAME_HEIGHT) == (BoneWindow.VisibleBoneCount - 1))
                                                {
                                                    if (BoneWindow.FirstVisibleBone < (editor.boneAnimationData.boneDataList.Count - BoneWindow.VisibleBoneCount))
                                                    {
                                                        BoneWindow.FirstVisibleBone++;
                                                    }
                                                }
                                                else if (Mathf.FloorToInt(localMousePos.y / TIMELINE_FRAME_HEIGHT) == 0)
                                                {
                                                    if (BoneWindow.FirstVisibleBone > 1)
                                                    {
                                                        BoneWindow.FirstVisibleBone--;
                                                    }
                                                }

                                                evt.Use();
                                            }
                                        }
                                        break;

                                    case EventType.MouseUp:
                                        if (EditorHelper.LeftMouseButton(evt))
                                        {
                                            if (_keyframesAreSliding)
                                            {
                                                Vector2 slidingOffset = areaMousePos - _slidingKeyframeStartPosition;
                                                int frameOffset = Mathf.FloorToInt(slidingOffset.x / TIMELINE_FRAME_WIDTH);
                                                _lastFrameOffset = 0;

                                                if (frameOffset != 0)
                                                {
                                                    bool someOverlap = false;
                                                    List<KeyframeSM> newKeyframes = new List<KeyframeSM>();
                                                    KeyframeSM newKeyframe;
                                                    bool overlappingOnSlidingKeyframe;
                                                    KeyframeSM copyExistingKeyframe;
                                                    foreach (SlidingKeyframe slidingKeyframe in _slidingKeyframes)
                                                    {
                                                        //copyExistingKeyframe = editor.boneAnimationData.GetKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, slidingKeyframe.boneDataIndex, slidingKeyframe.frame - frameOffset);
                                                        copyExistingKeyframe = editor.boneAnimationData.GetKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, slidingKeyframe.boneDataIndex, slidingKeyframe.originalFrame);
                                                        if (copyExistingKeyframe != null)
                                                        {
                                                            newKeyframe = new KeyframeSM(-1, AnimationClipBone.DEFAULT_SETTING.Blank);
                                                            newKeyframe.CopyKeyframe(copyExistingKeyframe);
                                                            newKeyframe.frame = slidingKeyframe.frame;
                                                            newKeyframes.Add(newKeyframe);
                                                        }

                                                        // check to see if we are going to be overwriting an existing keyframe with this move
                                                        if (editor.boneAnimationData.GetKeyframeIsSet(ClipBrowserWindow.SelectedAnimationClipIndex, slidingKeyframe.boneNodeIndex, slidingKeyframe.frame))
                                                        {
                                                            // make sure this set keyframe isn't one of the original keyframes we are sliding
                                                            overlappingOnSlidingKeyframe = false;
                                                            foreach (SlidingKeyframe otherSlidingKeyframe in _slidingKeyframes)
                                                            {
                                                                if (
                                                                    (otherSlidingKeyframe.boneDataIndex == slidingKeyframe.boneDataIndex
                                                                    &&
                                                                    otherSlidingKeyframe.originalFrame == slidingKeyframe.frame)
                                                                    )
                                                                {
                                                                    overlappingOnSlidingKeyframe = true;
                                                                    break;
                                                                }
                                                            }

                                                            // this keyframe isn't overlapping one of the original sliding keyframes,
                                                            // so we flag it as overlapping
                                                            if (!overlappingOnSlidingKeyframe)
                                                            {
                                                                someOverlap = true;
                                                            }
                                                        }
                                                    }

                                                    bool resume = true;

                                                    if (someOverlap)
                                                    {
                                                        if (!EditorUtility.DisplayDialog("Warning", "Moving the selected keyframes here will overwrite other keyframes. Are you sure you want to do this?", "Yes", "No"))
                                                        {
                                                            ResetSlidingKeyframes();

                                                            resume = false;
                                                        }
                                                    }

                                                    if (resume)
                                                    {
                                                        editor.SetWillBeDirty();

                                                        // remove old keyframes
                                                        foreach (SlidingKeyframe slidingKeyframe in _slidingKeyframes)
                                                        {
                                                            // don't remove keyframe zero
                                                            if (slidingKeyframe.originalFrame != 0)
                                                            {
                                                                editor.boneAnimationData.RemoveKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, slidingKeyframe.boneNodeIndex, slidingKeyframe.originalFrame);
                                                            }
                                                        }

                                                        // move the keyframes to their new positions
                                                        for (int keyframeIndex = 0; keyframeIndex < newKeyframes.Count; keyframeIndex++)
                                                        {
                                                            newKeyframe = editor.boneAnimationData.AddKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex,
                                                                                                                editor.boneAnimationData.GetBoneNodeIndex(newKeyframes[keyframeIndex].boneDataIndex),
                                                                                                                newKeyframes[keyframeIndex].frame,
                                                                                                                AnimationClipBone.KEYFRAME_COPY_MODE.None,
                                                                                                                AnimationClipBone.DEFAULT_SETTING.Blank);
                                                            newKeyframe.CopyKeyframe(newKeyframes[keyframeIndex]);

                                                            editor.boneAnimationData.animationClips[ClipBrowserWindow.SelectedAnimationClipIndex].SetMaxKeyframe();
                                                        }

                                                        AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                                                        // update the array of selected keyframes
                                                        foreach (KeyframeSM selectedKeyframe in _selectedKeyframes)
                                                        {
                                                            if (selectedKeyframe.frame != 0)
                                                            {
                                                                selectedKeyframe.frame += frameOffset;
                                                            }
                                                        }

                                                        // update the original frame of the selected frames
                                                        foreach (BoneFrame selectedFrame in _selectedFrames)
                                                        {
                                                            selectedFrame.originalFrame = selectedFrame.frame;
                                                        }

                                                        CalculateSelectionBounds();
                                                    }
                                                }

                                                SetFrameGridNeedsRebuilt();
                                                editor.SetNeedsRepainted();

                                                _keyframesAreSliding = false;
                                            }
                                            else
                                            {
                                                if (!_draggingTime)
                                                {
                                                    GetBoneIndexAndFrameFromMousePosition(localMousePos, ref boneNodeIndex, ref frame);

                                                    if (!evt.control)
                                                    {
                                                        _selectingFrames = false;
                                                        EndDragFrameSelection(boneNodeIndex, frame);
                                                    }

                                                    evt.Use();
                                                }
                                            }
                                        }

                                        _draggingTime = false;
                                        break;

                                    case EventType.ScrollWheel:
                                        AdvanceFrame(Convert.ToInt16(Mathf.Sign(-evt.delta.y)));
                                        evt.Use();
                                        break;

                                    case EventType.KeyDown:
                                        switch (evt.keyCode)
                                        {
                                            case KeyCode.Escape:
                                                if (_keyframesAreSliding)
                                                {
                                                    ResetSlidingKeyframes();
                                                    _keyframesAreSliding = false;
                                                    _draggingTime = true;

                                                    evt.Use();
                                                }
                                                break;

                                            case KeyCode.I:
                                                InsertFramesAtSelection();
                                                break;

                                            case KeyCode.D:
                                                DeleteFramesAtSelection();
                                                break;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        static private void StartSliding(Vector2 areaMousePos)
        {
            _slidingKeyframeStartPosition = areaMousePos;
            _lastFrameOffset = 0;
            _keyframesAreSliding = true;

            _slidingKeyframes.Clear();
            foreach (KeyframeSM selectedKeyframe in _selectedKeyframes)
            {
                _slidingKeyframes.Add(new SlidingKeyframe(selectedKeyframe.frame, editor.boneAnimationData.GetBoneNodeIndex(selectedKeyframe.boneDataIndex), selectedKeyframe.boneDataIndex));
            }

            _originalSelectionBounds = _selectionBounds;
        }

        static private void ResetSlidingKeyframes()
        {
            if (_slidingKeyframes != null)
            {
                foreach (SlidingKeyframe slidingKeyframe in _slidingKeyframes)
                {
                    slidingKeyframe.frame = slidingKeyframe.originalFrame;
                }

                foreach (BoneFrame boneFrame in _selectedFrames)
                {
                    boneFrame.frame = boneFrame.originalFrame;
                }

                CalculateSelectionBounds();

                SetFrameGridNeedsRebuilt();
                editor.SetNeedsRepainted();
            }
        }

        static private void ShiftSlidingKeyframes(ref int frameCount)
        {
            if (_slidingKeyframes != null)
            {
                if (_slidingKeyframes.Count > 0)
                {
                    foreach (SlidingKeyframe slidingKeyframe in _slidingKeyframes)
                    {
                        slidingKeyframe.frame = slidingKeyframe.originalFrame + frameCount;
                    }

                    foreach (BoneFrame boneFrame in _selectedFrames)
                    {
                        boneFrame.frame = boneFrame.originalFrame + frameCount;
                    }

                    CalculateSelectionBounds();
                }
            }
        }

        static private void GetBoneIndexAndFrameFromMousePosition(Vector2 mousePos, ref int boneIndex, ref int frame)
        {
            boneIndex = Mathf.FloorToInt(mousePos.y / TIMELINE_FRAME_HEIGHT) + BoneWindow.FirstVisibleBone;
            frame = Mathf.FloorToInt(mousePos.x / TIMELINE_FRAME_WIDTH) + FirstVisibleFrame;
        }

        static private void GetTimeFromMousePosition(Vector2 mousePos, ref float time)
        {
            time = (mousePos.x / TIMELINE_FRAME_WIDTH) + (float)FirstVisibleFrame;
        }

        static public void FrameContextMenuCallback_AddKeyframe(System.Object obj)
        {
            editor.SetWillBeDirty();
            BoneFrame boneFrame = (BoneFrame)obj;
            AddKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, boneFrame.boneNodeIndex, boneFrame.frame, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.Blank, false);
        }

        static public void FrameContextMenuCallback_AddPositionRotationKeyframe(System.Object obj)
        {
            editor.SetWillBeDirty();
            BoneFrame boneFrame = (BoneFrame)obj;
            AddKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, boneFrame.boneNodeIndex, boneFrame.frame, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.PositionRotation, false);
        }

        static public void FrameContextMenuCallback_DuplicatePreviousKeyframe(System.Object obj)
        {
            editor.SetWillBeDirty();
            BoneFrame boneFrame = (BoneFrame)obj;
            AddKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, boneFrame.boneNodeIndex, boneFrame.frame, AnimationClipBone.KEYFRAME_COPY_MODE.CopyPrevious, AnimationClipBone.DEFAULT_SETTING.All, false);
        }

        static public void FrameContextMenuCallback_DuplicateFirstKeyframe(System.Object obj)
        {
            editor.SetWillBeDirty();
            BoneFrame boneFrame = (BoneFrame)obj;
            AddKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, boneFrame.boneNodeIndex, boneFrame.frame, AnimationClipBone.KEYFRAME_COPY_MODE.CopyFirst, AnimationClipBone.DEFAULT_SETTING.All, false);
        }

        static public void FrameContextMenuCallback_RemoveKeyframe(System.Object obj)
        {
            editor.SetWillBeDirty();
            BoneFrame boneFrame = (BoneFrame)obj;
            RemoveKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, boneFrame.boneNodeIndex, boneFrame.frame);
        }

        static public void FrameContextMenuCallback_AddSelectedKeyframes(System.Object obj)
        {
            editor.SetWillBeDirty();
            AddSelectedKeyframes(ClipBrowserWindow.SelectedAnimationClipIndex, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.Blank, false);
        }

        static public void FrameContextMenuCallback_AddSelectedPositionRotationKeyframes(System.Object obj)
        {
            editor.SetWillBeDirty();
            AddSelectedKeyframes(ClipBrowserWindow.SelectedAnimationClipIndex, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.PositionRotation, false);
        }

        static public void FrameContextMenuCallback_DuplicateLastSelectedKeyframes(System.Object obj)
        {
            editor.SetWillBeDirty();
            AddSelectedKeyframes(ClipBrowserWindow.SelectedAnimationClipIndex, AnimationClipBone.KEYFRAME_COPY_MODE.CopyPrevious, AnimationClipBone.DEFAULT_SETTING.All, false);
        }

        static public void FrameContextMenuCallback_DuplicateFirstSelectedKeyframes(System.Object obj)
        {
            editor.SetWillBeDirty();
            AddSelectedKeyframes(ClipBrowserWindow.SelectedAnimationClipIndex, AnimationClipBone.KEYFRAME_COPY_MODE.CopyFirst, AnimationClipBone.DEFAULT_SETTING.All, false);
        }

        //static public void FrameContextMenuCallback_InsertColumnOfFrames(System.Object obj)
        //{
        //    int atFrame = (int)obj;

        //    if (editor.boneAnimationData.animationClips[ClipBrowserWindow.SelectedAnimationClipIndex].maxFrame >= AnimationClipSM.MAX_FRAMES)
        //    {
        //        EditorUtility.DisplayDialog("Warning", "Inserting frames here would push the last keyframe beyond the limit of [" + AnimationClipSM.MAX_FRAMES.ToString() + "]", "OK");
        //        return;
        //    }

        //    editor.SetWillBeDirty();
        //    editor.boneAnimationData.InsertFrames(ClipBrowserWindow.SelectedAnimationClipIndex, atFrame, 1);

        //    AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
        //    foreach (AnimationClipBone bone in editor.boneAnimationData.animationClips[ClipBrowserWindow.SelectedAnimationClipIndex].bones)
        //    {
        //        AnimationHelper.AddBoneDataIndexToRefreshList(bone.boneDataIndex);
        //    }

        //    ResetSelectedFrames();

        //    SetFrameGridNeedsRebuilt();
        //    editor.SetNeedsRepainted();
        //}

        static public void FrameContextMenuCallback_InsertFramesAtSelection(System.Object obj)
        {
            InsertFramesAtSelection();
        }

        static public void FrameContextMenuCallback_DeleteFramesAtSelection(System.Object obj)
        {
            DeleteFramesAtSelection();
        }

        static public void FrameContextMenuCallback_RemoveSelectedKeyframes(System.Object obj)
        {
            editor.SetWillBeDirty();
            RemoveSelectedKeyframes(ClipBrowserWindow.SelectedAnimationClipIndex);
        }

        static public void FrameContextMenuCallback_ResetTransform(System.Object obj)
        {
            ResetKeyframeTransforms();
        }

        static public void FrameContextMenuCallback_EditKeyframes(System.Object obj)
        {
            EditSelectedKeyframes();
        }

        static public void FrameContextMenuCallback_CutKeyframesToClipboard(System.Object obj)
        {
            CutSelectedKeyframes();
        }
		
		static public void FrameContextMenuCallback_CopyKeyframesToClipboard(System.Object obj)
		{
            CopyKeyframesToClipboard();
		}

		static public void FrameContextMenuCallback_PasteKeyframesFromClipboard(System.Object obj)
		{
            editor.SetWillBeDirty();
            PasteClipboardKeyframes();
		}

        static public void ResetPlayDirection()
        {
            _playDirection = PLAY_DIRECTION.Forward;
        }

		static public void Play(bool rewind)
        {
            if (ClipBrowserWindow.CurrentClip.maxFrame > 0)
            {
                if (rewind)
                {
                    _currentFrame = 0;
                    _frameTime = 0;
                }

                _isPlaying = true;
            }
        }

        static public void Stop()
        {
            _isPlaying = false;
        }

        static public bool FrameUpdate(float deltaTime)
        {
            if (_isPlaying)
            {
                AdvanceTime(deltaTime);
                return true;
            }

            return false;
        }

        static public void AdvanceFrame(int frameCount)
        {
            if (ClipBrowserWindow.CurrentClip.maxFrame == 0)
                return;

            int frame;

            frame = _currentFrame + (frameCount * (int)_playDirection);

            if (frame < 0)
            {
                switch (ClipBrowserWindow.CurrentClip.wrapMode)
                {
                    case WrapMode.Loop:
                        frame = (ClipBrowserWindow.CurrentClip.maxFrame + 1) - (-frame % ClipBrowserWindow.CurrentClip.maxFrame);
                        break;

                    case WrapMode.ClampForever:
                    case WrapMode.Once:
                        frame = 0;
                        break;

                    case WrapMode.PingPong:
                        frame = Mathf.Clamp(-frame, 0, ClipBrowserWindow.CurrentClip.maxFrame);
                        _playDirection = (PLAY_DIRECTION)(-(int)_playDirection);
                        break;
                }
            }
            else if (frame > ClipBrowserWindow.CurrentClip.maxFrame)
            {
                switch (ClipBrowserWindow.CurrentClip.wrapMode)
                {
                    case WrapMode.Loop:
                        frame = (frame % (ClipBrowserWindow.CurrentClip.maxFrame + 1));
                        break;

                    case WrapMode.ClampForever:
                    case WrapMode.Once:
                        frame = ClipBrowserWindow.CurrentClip.maxFrame;
                        break;

                    case WrapMode.PingPong:
                        frame = Mathf.Clamp(ClipBrowserWindow.CurrentClip.maxFrame - (frame % ClipBrowserWindow.CurrentClip.maxFrame), 0, ClipBrowserWindow.CurrentClip.maxFrame);
                        _playDirection = (PLAY_DIRECTION)(-(int)_playDirection);
                        break;
                }
            }
            SetCurrentFrame(frame);

            //ResetSelectedFrames();

            if (frame >= (FirstVisibleFrame + _visibleFrameCount))
            {
                MakeFrameFirst(frame);
            }
            else if (frame < FirstVisibleFrame)
            {
                MakeFrameFirst(frame - _visibleFrameCount);
            }
        }

        static public void AdvanceTime(float deltaTime)
        {
            float time;

            time = _frameTime + (deltaTime * (float)_playDirection * ClipBrowserWindow.CurrentClip.fps);

            if (time < 0)
            {
                switch (ClipBrowserWindow.CurrentClip.wrapMode)
                {
                    case WrapMode.Loop:
                        time = ClipBrowserWindow.CurrentClip.maxFrame + RemainingTime(time, ClipBrowserWindow.CurrentClip.maxFrame);
                        break;

                    case WrapMode.ClampForever:
                    case WrapMode.Once:
                        time = 0;
                        break;

                    case WrapMode.PingPong:
                        time = Mathf.Clamp(-RemainingTime(time, ClipBrowserWindow.CurrentClip.maxFrame), 0, ClipBrowserWindow.CurrentClip.maxFrame);
                        _playDirection = (PLAY_DIRECTION)(-(int)_playDirection);
                        break;
                }
            }
            else if (time > ClipBrowserWindow.CurrentClip.maxFrame)
            {
                switch (ClipBrowserWindow.CurrentClip.wrapMode)
                {
                    case WrapMode.Loop:
                        time = RemainingTime(time, ClipBrowserWindow.CurrentClip.maxFrame);
                        break;

                    case WrapMode.ClampForever:
                    case WrapMode.Once:
                        time = ClipBrowserWindow.CurrentClip.maxFrame;
                        _isPlaying = false;
                        break;

                    case WrapMode.PingPong:
                        time = Mathf.Clamp(ClipBrowserWindow.CurrentClip.maxFrame - RemainingTime(time, ClipBrowserWindow.CurrentClip.maxFrame), 0, ClipBrowserWindow.CurrentClip.maxFrame);
                        _playDirection = (PLAY_DIRECTION)(-(int)_playDirection);
                        break;
                }
            }

            if (time >= ((FirstVisibleFrame + _visibleFrameCount)))
            {
                MakeTimeFirst(time);
            }
            else if (time < (FirstVisibleFrame))
            {
                MakeTimeFirst(time - (_visibleFrameCount));
            }

            SetFrameTime(time);
        }

        static private float RemainingTime(float time, float maxTime)
        {
            return ((time / maxTime) - Mathf.Round(time / maxTime)) * maxTime;
        }

        static public void SetCurrentFrame(int frame)
        {
            _currentFrame = frame;
            _frameTime = (float)_currentFrame;
        }

        static public void SetFrameTime(float time)
        {
            _frameTime = time;
            _currentFrame = Mathf.FloorToInt(time);
        }
		
		static public void SelectFrame(int boneNodeIndex, int frame)
		{
            if (ClipBrowserWindow.CurrentClip == null)
			{
				frame = -1;

				ResetWorkingSelectedFrames();
				ResetSelectedFrames();
				
				AddFrameToSelection(boneNodeIndex, frame);
			}
			else
			{
				ResetWorkingSelectedFrames();
				ResetSelectedFrames();
				
				AddFrameToSelection(boneNodeIndex, frame);
	
	            SetCurrentFrame(frame);
	
	            if (frame < FirstVisibleFrame || frame > (FirstVisibleFrame + _visibleFrameCount))
	                ClampFrame(frame, AnimationClipSM.MAX_FRAMES);
			}
		}

        static public void SelectKeyframesForBone(int boneNodeIndex)
        {
            if (ClipBrowserWindow.CurrentClip == null)
                return;

            ResetSelectedFrames();

            int clipIndex;
            int boneDataIndex;

            clipIndex = ClipBrowserWindow.SelectedAnimationClipIndex;
            boneDataIndex = editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex);

            foreach (KeyframeSM keyframe in editor.boneAnimationData.animationClips[clipIndex].bones[boneDataIndex].keyframes)
            {
                _selectedFrames.Add(new BoneFrame(keyframe.boneDataIndex, boneNodeIndex, keyframe.frame));
            }
            CalculateSelectionBounds();

            SetKeyframesFromSelectedFrames();
        }
		
		static public void AddFrameToSelection(int boneNodeIndex, int frame)
		{
			_selectedFrames.Add(new BoneFrame(editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex), boneNodeIndex, frame));
            CalculateSelectionBounds();

            if (_selectedFrames.Count == 1)
            {
                AnimationHelper.ChangeAnimationCurveEditorWindowBone(editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex));
            }

			SetKeyframesFromSelectedFrames();
            SetFrameGridNeedsRebuilt();
		}

        static public void RemoveFrameFromSelection(int boneNodeIndex, int frame)
        {
            int bdIndex = -1;

            if (_selectedFrames != null)
            {
                for (int sf = 0; sf < _selectedFrames.Count; sf++)
                {
                    if (_selectedFrames[sf].boneNodeIndex == boneNodeIndex && _selectedFrames[sf].frame == frame)
                    {
                        bdIndex = _selectedFrames[sf].boneDataIndex;

                        _selectedFrames.RemoveAt(sf);
                        break;
                    }   
                }
            }

            if (bdIndex != -1)
            {
                if (_selectedKeyframes != null)
                {
                    for (int skf = 0; skf < _selectedKeyframes.Count; skf++)
                    {
                        if (_selectedKeyframes[skf].boneDataIndex == bdIndex && _selectedKeyframes[skf].frame == frame)
                        {
                            _selectedKeyframes.RemoveAt(skf);
                            break;
                        }
                    }
                }
            }

            CalculateSelectionBounds();

            SetFrameGridNeedsRebuilt();
        }

        static private void CalculateSelectionBounds()
        {
            if (editor != null)
            {
                _selectionBounds.xMin = AnimationClipSM.MAX_FRAMES + 1;
                _selectionBounds.xMax = -1;
                _selectionBounds.yMin = editor.boneAnimationData.boneDataList.Count + 1;
                _selectionBounds.yMax = -1;

                if (_selectedKeyframes != null)
                {
                    foreach (BoneFrame boneFrame in _selectedFrames)
                    {
                        if (boneFrame.frame < _selectionBounds.xMin)
                            _selectionBounds.xMin = boneFrame.frame;

                        if (boneFrame.frame > _selectionBounds.xMax)
                            _selectionBounds.xMax = boneFrame.frame;

                        if (boneFrame.boneNodeIndex < _selectionBounds.yMin)
                            _selectionBounds.yMin = boneFrame.boneNodeIndex;

                        if (boneFrame.boneNodeIndex > _selectionBounds.yMax)
                            _selectionBounds.yMax = boneFrame.boneNodeIndex;
                    }
                }
            }
        }

        static public void StartDragFrameSelection(int boneNodeIndex, int frame)
        {
            if (boneNodeIndex < editor.boneAnimationData.BoneCount)
            {
                ResetWorkingSelectedFrames();
                _workingStartFrameSelection.Set(editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex), boneNodeIndex, frame);
            }
        }

        static public void EndDragFrameSelection(int boneNodeIndex, int frame)
        {
            if (boneNodeIndex < editor.boneAnimationData.BoneCount && _workingEndFrameSelection != null)
            {
                int boneDataIndex = editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex);

                if (frame != -1)
                    TimelineWindow.SetCurrentFrame(frame);

                _workingEndFrameSelection.Set(boneDataIndex, boneNodeIndex, frame);

                ResetSelectedFrames();

                _startFrameSelection.Copy(_workingStartFrameSelection);
                _endFrameSelection.Copy(_workingEndFrameSelection);

                if (_startFrameSelection.boneNodeIndex > _endFrameSelection.boneNodeIndex)
                {
                    _startFrameSelection.boneNodeIndex = _workingEndFrameSelection.boneNodeIndex;
                    _endFrameSelection.boneNodeIndex = _workingStartFrameSelection.boneNodeIndex;
                }
                if (_startFrameSelection.frame > _endFrameSelection.frame)
                {
                    _startFrameSelection.frame = _workingEndFrameSelection.frame;
                    _endFrameSelection.frame = _workingStartFrameSelection.frame;
                }

                for (int i = _startFrameSelection.boneNodeIndex; i <= _endFrameSelection.boneNodeIndex; i++)
                {
                    for (int j = _startFrameSelection.frame; j <= _endFrameSelection.frame; j++)
                    {
                        _selectedFrames.Add(new BoneFrame(editor.boneAnimationData.GetBoneDataIndex(i), i, j));
                    }
                }
                CalculateSelectionBounds();

                if (_selectedFrames.Count == 1)
                    AnimationHelper.ChangeAnimationCurveEditorWindowBone(boneDataIndex);

                SetKeyframesFromSelectedFrames();
            }
        }
		
        static public void ResetWorkingSelectedFrames()
        {
            if (_workingStartFrameSelection == null)
            {
                _workingStartFrameSelection = new BoneFrame(-1, -1, -1);
                _workingEndFrameSelection = new BoneFrame(-1, -1, -1);
            }
            else
            {
                _workingStartFrameSelection.Set(-1, -1, -1);
                _workingEndFrameSelection.Set(-1, -1, -1);
            }

            SetFrameGridNeedsRebuilt();
        }

        static public void ResetSelectedFrames()
        {
            if (_selectedFrames == null)
            {
                _selectedFrames = new List<BoneFrame>();
                _startFrameSelection = new BoneFrame(-1, -1, -1);
                _endFrameSelection = new BoneFrame(-1, -1, -1);
            }
            else
            {
                _selectedFrames.Clear();
                _startFrameSelection.Set(-1, -1, -1);
                _endFrameSelection.Set(-1, -1, -1);
            }
			
			if (_selectedKeyframes == null)
			{
				_selectedKeyframes = new List<KeyframeSM>();
			}
			else
			{
				_selectedKeyframes.Clear();
			}

            if (_slidingKeyframes == null)
                _slidingKeyframes = new List<SlidingKeyframe>();
            else
                _slidingKeyframes.Clear();

            CalculateSelectionBounds();

            SetFrameGridNeedsRebuilt();
        }

        static public void SetKeyframesFromSelectedFrames()
        {
            BoneWindow.SelectedBoneDataIndex = -1;
            if (_selectedKeyframes == null)
			{
				_selectedKeyframes = new List<KeyframeSM>();
			}
			else
			{
				_selectedKeyframes.Clear();
			}

            if (_selectedFrames == null)
                return;

            if (_selectedFrames.Count == 0)
                return;

            KeyframeSM keyframe;
            int selectedBoneDataIndex = -1;
            for (int frameIndex = 0; frameIndex < _selectedFrames.Count; frameIndex++)
            {
                selectedBoneDataIndex = BoneWindow.SelectedBoneDataIndex;
                keyframe = editor.boneAnimationData.GetKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex,
                                        _selectedFrames[frameIndex].boneNodeIndex,
                                        _selectedFrames[frameIndex].frame,
                                        out selectedBoneDataIndex
                                    );
                BoneWindow.SelectedBoneDataIndex = selectedBoneDataIndex;

                AddSelectedKeyframe(keyframe);
            }

            if (OneKeyframeSelected)
            {
                UpdateLastKeyframe();

                if (TextureManager.TextureDictionary.ContainsKey(LastKeyframe.textureGUID))
                {
                    if (TextureManager.GetTextureIndex(LastKeyframe.atlas, LastKeyframe.textureGUID) == -1)
                        BonePropertiesWindow.SetTexture(null);
                    else
                        BonePropertiesWindow.SetTexture(TextureManager.TextureDictionary[LastKeyframe.textureGUID].texture);
                }
                else
                {
                    BonePropertiesWindow.SetTexture(null);
                }
            }
            else
            {
                BonePropertiesWindow.SetTexture(null);
            }

            SetFrameGridNeedsRebuilt();
        }

        static public void UpdateLastKeyframe()
        {
            if (OneKeyframeSelected)
            {
                KeyframeSM keyframe = FirstSelectedKeyframe;

                AnimationClipBone animationClipBone = editor.boneAnimationData.GetAnimationClipBoneFromBoneDataIndex(ClipBrowserWindow.SelectedAnimationClipIndex,
                                                                                                                        keyframe.boneDataIndex);

                LastKeyframe.keyframeType = animationClipBone.GetPreviousKeyframeType(keyframe.frame);
                LastKeyframe.atlas = animationClipBone.GetPreviousAtlas(keyframe.frame);
                LastKeyframe.textureGUID = animationClipBone.GetPreviousTextureGUID(keyframe.frame);
                LastKeyframe.pivotOffset = animationClipBone.GetPreviousPivotOffset(keyframe.frame);
                LastKeyframe.useDefaultPivot = animationClipBone.GetPreviousUseDefaultPivot(keyframe.frame);
                LastKeyframe.depth = animationClipBone.GetPreviousDepth(keyframe.frame);
                LastKeyframe.collider = animationClipBone.GetPreviousCollider(keyframe.frame);

                if (LastKeyframe.atlas != null)
                {
                    LastKeyframe.textureIndex = LastKeyframe.atlas.GetTextureIndex(LastKeyframe.textureGUID);
                }
                else
                {
                    LastKeyframe.textureIndex = -1;
                }
            }
        }

        static public void AddSelectedKeyframe(KeyframeSM keyframe)
        {
            if (keyframe != null)
            {
                _selectedKeyframes.Add(keyframe);

                SetFrameGridNeedsRebuilt();
            }
        }

        static public bool KeyframeSelected(KeyframeSM keyframe)
        {
            return (_selectedKeyframes.Contains(keyframe));
        }

        static public void ResetKeyframeTransforms()
        {
            if (EditorUtility.DisplayDialog("Reset Transform Confirmation", "Are you sure you want to reset the transforms for the selected keyframes?", "Yes", "No"))
            {
                editor.Focus();

                if (_selectedKeyframes.Count > 0)
                {
                    editor.SetWillBeDirty();
                }

                foreach (KeyframeSM keyframe in _selectedKeyframes)
                {
                    keyframe.localPosition3.val = Vector3.zero;
                    keyframe.localRotation.val = 0;
                    keyframe.localScale3.val = Vector3.one;
                    keyframe.imageScale.val = Vector3.one;
                }

                if (_selectedKeyframes.Count > 0)
                {
                    editor.SetNeedsRepainted();
                    SetFrameGridNeedsRebuilt();
                }
            }
        }

        static public void HideBones(bool hide)
        {
            int clipIndex = ClipBrowserWindow.SelectedAnimationClipIndex;
            if (clipIndex != -1)
            {
                editor.Focus();

                if (_selectedFrames.Count > 0)
                {
                    editor.SetWillBeDirty();
                }

                foreach (BoneFrame boneFrame in _selectedFrames)
                {
                    if (hide)
                    {
                        editor.boneAnimationData.animationClips[clipIndex].bones[boneFrame.boneDataIndex].visible = false;
                    }
                    else
                    {
                        editor.boneAnimationData.animationClips[clipIndex].bones[boneFrame.boneDataIndex].visible = true;
                        editor.boneAnimationData.boneDataList[boneFrame.boneDataIndex].active = true;
                    }
                }

                if (_selectedFrames.Count > 0)
                {
                    editor.SetNeedsRepainted();
                    SetFrameGridNeedsRebuilt();
                }
            }
        }

        static public void EditSelectedKeyframes()
        {
            if (_selectedKeyframes.Count > 1)
            {
                editor.SetWillBeDirty();

                SetAtlasesWindow.Reset();
                SetAtlasesWindow.Visible = true;
            }
        }

        static public void GetSelectedUpperLeftCorner(out int boneNodeIndex, out int frame)
        {
            boneNodeIndex = -1;
            frame = -1;

            if (_selectedFrames == null)
                return;

            if (_selectedFrames.Count == 0)
                return;

            boneNodeIndex = 3000;
            frame = AnimationClipSM.MAX_FRAMES + 1;

            foreach (BoneFrame boneFrame in _selectedFrames)
            {
                if (boneFrame.boneNodeIndex < boneNodeIndex)
                    boneNodeIndex = boneFrame.boneNodeIndex;

                if (boneFrame.frame < frame)
                    frame = boneFrame.frame;
            }
        }

        static public void CopyKeyframesToClipboard()
        {
            _clipboardKeyframes.Clear();

            foreach (KeyframeSM keyframe in _selectedKeyframes)
            {
                _clipboardKeyframes.Add(keyframe);
            }
        }
		
		static public void PasteClipboardKeyframes()
		{
			if (_clipboardKeyframes.Count == 0)
				return;
			
			int clipIndex;
            int pasteToBoneNodeIndex;
            int pasteToFrame;

            GetSelectedUpperLeftCorner(out pasteToBoneNodeIndex, out pasteToFrame);

            foreach (BoneFrame boneFrame in _selectedFrames)
            {
                if (boneFrame.boneNodeIndex < pasteToBoneNodeIndex)
                    pasteToBoneNodeIndex = boneFrame.boneNodeIndex;

                if (boneFrame.frame < pasteToFrame)
                    pasteToFrame = boneFrame.frame;
            }

            int boneNodeIndex;
            bool willOverwrite = false;
            int minBoneNodeIndex = 3000;
            int minFrame = AnimationClipSM.MAX_FRAMES + 1;
			
			KeyframeSM newKeyframe;
			
			clipIndex = ClipBrowserWindow.SelectedAnimationClipIndex;
            
            // normalize the bone and frame offset
			foreach (KeyframeSM keyframe in _clipboardKeyframes)
			{
				boneNodeIndex = editor.boneAnimationData.GetBoneNodeIndex(keyframe.boneDataIndex);
				if (boneNodeIndex < minBoneNodeIndex)
					minBoneNodeIndex = boneNodeIndex;
				
				if (keyframe.frame < minFrame)
					minFrame = keyframe.frame;
			}

            foreach (KeyframeSM keyframe in _clipboardKeyframes)
            {
                boneNodeIndex = editor.boneAnimationData.GetBoneNodeIndex(keyframe.boneDataIndex);

                if (editor.boneAnimationData.GetKeyframeIsSet(clipIndex,
                                                                pasteToBoneNodeIndex + (boneNodeIndex - minBoneNodeIndex),
                                                                pasteToFrame + (keyframe.frame - minFrame)
                                                                )
                    )
                {
                    willOverwrite = true;
                    break;
                }
            }

            if (!willOverwrite ||
                (willOverwrite && EditorUtility.DisplayDialog("Paste Keyframe(s)", "Pasting keyframes here will overwrite some of your previously set keyframes. Are you sure you want to do this?", "Yes", "No")))
            {
                editor.Focus();

                SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK copyPasteMask = SettingsWindow.TimelinePropertyCopyPasteMask;
                int newFrame;
                int newBoneNodeIndex;

                foreach (KeyframeSM keyframe in _clipboardKeyframes)
                {
                    boneNodeIndex = editor.boneAnimationData.GetBoneNodeIndex(keyframe.boneDataIndex);

                    // only copy keyframes that will fit in our bone list
                    if ((pasteToBoneNodeIndex + (boneNodeIndex - minBoneNodeIndex)) < editor.boneAnimationData.dfsBoneNodeList.Count)
                    {
                        newFrame = pasteToFrame + (keyframe.frame - minFrame);
                        newBoneNodeIndex = pasteToBoneNodeIndex + (boneNodeIndex - minBoneNodeIndex);

                        newKeyframe = AddKeyframe(clipIndex,
                                                    newBoneNodeIndex,
                                                    newFrame,
                                                    AnimationClipBone.KEYFRAME_COPY_MODE.None,
                                                    AnimationClipBone.DEFAULT_SETTING.Blank,
                                                    true);

                        if (newKeyframe != null)
                        {
                            if ((copyPasteMask & SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK.UserTrigger) != 0)
                            {
                                newKeyframe.userTriggerCallback = keyframe.userTriggerCallback;
                                newKeyframe.userTriggerTag = keyframe.userTriggerTag;
                            }
                            if ((copyPasteMask & SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK.Type) != 0)
                            {
                                newKeyframe.useKeyframeType = keyframe.useKeyframeType;
                                newKeyframe.keyframeType = keyframe.keyframeType;
                            }
                            if ((copyPasteMask & SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK.Atlas) != 0)
                            {
                                newKeyframe.useAtlas = keyframe.useAtlas;
                                newKeyframe.atlas = keyframe.atlas;
                            }
                            if ((copyPasteMask & SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK.Texture) != 0)
                            {
                                newKeyframe.useTextureGUID = keyframe.useTextureGUID;
                                newKeyframe.textureGUID = keyframe.textureGUID;
                                newKeyframe.textureIndex = keyframe.textureIndex;
                            }
                            if ((copyPasteMask & SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK.Pivot) != 0)
                            {
                                newKeyframe.usePivotOffset = keyframe.usePivotOffset;
                                newKeyframe.pivotOffset = keyframe.pivotOffset;
                                newKeyframe.useDefaultPivot = keyframe.useDefaultPivot;
                            }
                            if ((copyPasteMask & SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK.Depth) != 0)
                            {
                                newKeyframe.useDepth = keyframe.useDepth;
                                newKeyframe.depth = keyframe.depth;
                            }
                            if ((copyPasteMask & SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK.Collider) != 0)
                            {
                                newKeyframe.useCollider = keyframe.useCollider;
                                newKeyframe.collider.CopyCollider(keyframe.collider);
                            }
                            if ((copyPasteMask & SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK.LocalPosition) != 0)
                            {
                                newKeyframe.localPosition3.Copy(keyframe.localPosition3, newKeyframe.frame);
                            }
                            if ((copyPasteMask & SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK.LocalRotation) != 0)
                            {
                                newKeyframe.localRotation.Copy(keyframe.localRotation, newKeyframe.frame);
                            }
                            if ((copyPasteMask & SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK.LocalScale) != 0)
                            {
                                newKeyframe.localScale3.Copy(keyframe.localScale3, newKeyframe.frame);
                            }
                            if ((copyPasteMask & SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK.ImageScale) != 0)
                            {
                                newKeyframe.imageScale.Copy(keyframe.imageScale, newKeyframe.frame);
                            }
                            if ((copyPasteMask & SettingsWindow.KEYFRAME_PROPERTY_COPYPASTE_MASK.Color) != 0)
                            {
                                newKeyframe.color.Copy(keyframe.color, newKeyframe.frame);
                            }

                            //newKeyframe.CopyKeyframePositionRotationScale(keyframe);

                            newKeyframe.boneDataIndex = editor.boneAnimationData.GetBoneDataIndex(newBoneNodeIndex);
                            newKeyframe.frame = newFrame;
                        }
                    }
                }
            }

            SetFrameGridNeedsRebuilt();
		}

        static public KeyframeSM GetSelectedKeyframeAtIndex(int index)
        {
            if (index > -1 && index < _selectedKeyframes.Count)
                return _selectedKeyframes[index];
            else
                return null;
        }

        static public int MiddleVisibleFrame
        {
            get
            {
                return (FirstVisibleFrame + _visibleFrameCount) / 2;
            }
        }

        static public void CenterFrame(int frame)
        {
            FirstVisibleFrame = Mathf.Clamp(frame - (_visibleFrameCount / 2), 0, AnimationClipSM.MAX_FRAMES - _visibleFrameCount);
        }

        static public void MakeFrameFirst(int frame)
        {
            FirstVisibleFrame = Mathf.Clamp(frame, 0, AnimationClipSM.MAX_FRAMES - _visibleFrameCount);
        }

        static public void CenterTime(float time)
        {
            int frame = Mathf.FloorToInt(time);
            CenterFrame(frame);
        }

        static public void MakeTimeFirst(float time)
        {
            int frame = Mathf.FloorToInt(time);
            MakeFrameFirst(frame);
        }

        static public void ClampFrame(int frame, int lastFrame)
        {
            FirstVisibleFrame = Mathf.Clamp(frame, 0, Mathf.Clamp(lastFrame - _visibleFrameCount, 0, AnimationClipSM.MAX_FRAMES));
        }

        static public bool IsFrameSelected(int boneIndex, int frame)
        {
            if (_selectedFrames != null)
            {
                foreach (BoneFrame boneFrame in _selectedFrames)
                {
                    if (boneFrame.boneNodeIndex == boneIndex &&
                        boneFrame.frame == frame)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static public bool IsThisASlidingKeyframe(int boneIndex, int frame)
        {
            if (_slidingKeyframes != null)
            {
                foreach (SlidingKeyframe slidingKeyframe in _slidingKeyframes)
                {
                    if (slidingKeyframe.boneNodeIndex == boneIndex &&
                        slidingKeyframe.originalFrame == frame)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static public bool IsFrameEmpty(int clipIndex, int boneIndex, int frame)
        {
            int bdIndex = -1;
            KeyframeSM keyframe = editor.boneAnimationData.GetKeyframe(clipIndex, boneIndex, frame, out bdIndex);
            if (keyframe != null)
            {
                return (!keyframe.BeingUsed);
            }

            return false;
        }

        static public void SelectColumn(int frame)
        {
            ResetWorkingSelectedFrames();
            ResetSelectedFrames();

            if (editor.boneAnimationData.BoneCount > 0)
            {
                StartDragFrameSelection(1, frame);
                EndDragFrameSelection(editor.boneAnimationData.BoneCount - 1, frame);

                editor.SetNeedsRepainted();
                SetFrameGridNeedsRebuilt();
            }
        }

        static public KeyframeSM AddKeyframe(int clipIndex, int boneNodeIndex, int frame, AnimationClipBone.KEYFRAME_COPY_MODE copyMode, AnimationClipBone.DEFAULT_SETTING defaultSetting, bool ignoreWarnings)
        {
            bool keyframeExists = false;
			bool proceed = false;

            keyframeExists = editor.boneAnimationData.GetKeyframeIsSet(clipIndex, boneNodeIndex, frame);
			
			if (ignoreWarnings)
			{
				proceed = true;
			}
			else if (keyframeExists)
			{
                if (EditorUtility.DisplayDialog("Keyframe is Already Set", "Are you sure you want to overwrite the current keyframe?", "Yes", "No"))
                {
                    editor.Focus();

                    proceed = true;
                }
			}
			else
			{
				proceed = true;
			}
			
            if (proceed)
            {
                editor.SetWillBeDirty();
                
                KeyframeSM keyframe = editor.boneAnimationData.AddKeyframe(clipIndex, boneNodeIndex, frame, copyMode, defaultSetting);

                //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
                AnimationHelper.refreshAnimationEditorWindowPostCycle = true;
                
                if (defaultSetting == AnimationClipBone.DEFAULT_SETTING.PositionRotation)
                {
                    BoneCurves boneCurves = AnimationHelper.GetBoneDataCurves(keyframe.boneDataIndex);
                    if (boneCurves != null)
                    {
                        keyframe.localPosition3.val.x = boneCurves.localPositionXCurve.Evaluate(keyframe.frame);
                        keyframe.localPosition3.inTangentX = AnimationHelper.CalculateTangent(boneCurves.localPositionXCurve, (float)keyframe.frame, keyframe.localPosition3.val.x);
                        keyframe.localPosition3.outTangentX = keyframe.localPosition3.inTangentX;

                        keyframe.localPosition3.val.y = boneCurves.localPositionYCurve.Evaluate(keyframe.frame);
                        keyframe.localPosition3.inTangentY = AnimationHelper.CalculateTangent(boneCurves.localPositionYCurve, (float)keyframe.frame, keyframe.localPosition3.val.y);
                        keyframe.localPosition3.outTangentY = keyframe.localPosition3.inTangentY;

                        keyframe.localPosition3.val.z = boneCurves.localPositionZCurve.Evaluate(keyframe.frame);
                        keyframe.localPosition3.inTangentZ = AnimationHelper.CalculateTangent(boneCurves.localPositionZCurve, (float)keyframe.frame, keyframe.localPosition3.val.z);
                        keyframe.localPosition3.outTangentZ = keyframe.localPosition3.inTangentZ;

                        keyframe.localRotation.val = boneCurves.localRotationCurve.Evaluate(keyframe.frame);
                        keyframe.localRotation.inTangent = AnimationHelper.CalculateTangent(boneCurves.localRotationCurve, (float)keyframe.frame, keyframe.localRotation.val);
                        keyframe.localRotation.outTangent = keyframe.localRotation.inTangent;
                    }
                }

                editor.SetNeedsRepainted();
                SetFrameGridNeedsRebuilt();

                _selectFramePostCycle = true;
                _selectBoneFrame = new BoneFrame(keyframe.boneDataIndex, boneNodeIndex, frame);

                return keyframe;
            }
			
			return null;
        }

        static public void RemoveKeyframe(int clipIndex, int boneNodeIndex, int frame)
        {
            if (EditorUtility.DisplayDialog("Delete Keyframe Confirmation", "Are you sure you want to delete this keyframe?", "Yes", "No"))
            {
                editor.SetWillBeDirty();
                
                editor.Focus();

                editor.boneAnimationData.RemoveKeyframe(clipIndex, boneNodeIndex, frame);

                //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                //AnimationHelper.AddBoneDataIndexToRefreshList(editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex));
                AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                ResetSelectedFrames();

                editor.SetNeedsRepainted();
                SetFrameGridNeedsRebuilt();
            }
        }

        static public void AddSelectedKeyframes(int clipIndex, AnimationClipBone.KEYFRAME_COPY_MODE copyMode, AnimationClipBone.DEFAULT_SETTING defaultSetting, bool ignoreWarnings)
        {
            bool keyframeExists = false;

            foreach (BoneFrame boneFrame in _selectedFrames)
            {
                if (editor.boneAnimationData.GetKeyframeIsSet(clipIndex, boneFrame.boneNodeIndex, boneFrame.frame))
                {
                    keyframeExists = true;
                    break;
                }
            }

            bool proceed = true;

            if (!ignoreWarnings && keyframeExists)
            {
                if (!EditorUtility.DisplayDialog("At Least One Keyframe is Already Set", "Are you sure you want to overwrite the current keyframe(s)?", "Yes", "No"))
                {
                    proceed = false;
                }
            }

            if (
                (keyframeExists && proceed)
                ||
                (!keyframeExists)
                )
            {
                editor.Focus();

                editor.boneAnimationData.AddSelectedKeyframes(clipIndex, ref _selectedFrames, copyMode, defaultSetting);

                if (defaultSetting == AnimationClipBone.DEFAULT_SETTING.PositionRotation)
                {
                    editor.SetWillBeDirty();
                    
                    KeyframeSM keyframe;
                    int bdIndex;

                    //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();

                    foreach (BoneFrame selectedFrame in _selectedFrames)
                    {
                        keyframe = editor.boneAnimationData.GetKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, selectedFrame.boneNodeIndex, selectedFrame.frame, out bdIndex);

                        //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);

                        if (keyframe != null)
                        {
                            BoneCurves boneCurves = AnimationHelper.GetBoneDataCurves(keyframe.boneDataIndex);
                            if (boneCurves != null)
                            {
                                keyframe.localPosition3.val.x = boneCurves.localPositionXCurve.Evaluate(keyframe.frame);
                                keyframe.localPosition3.inTangentX = AnimationHelper.CalculateTangent(boneCurves.localPositionXCurve, (float)keyframe.frame, keyframe.localPosition3.val.x);
                                keyframe.localPosition3.outTangentX = keyframe.localPosition3.inTangentX;

                                keyframe.localPosition3.val.y = boneCurves.localPositionYCurve.Evaluate(keyframe.frame);
                                keyframe.localPosition3.inTangentY = AnimationHelper.CalculateTangent(boneCurves.localPositionYCurve, (float)keyframe.frame, keyframe.localPosition3.val.y);
                                keyframe.localPosition3.outTangentY = keyframe.localPosition3.inTangentY;

                                keyframe.localPosition3.val.z = boneCurves.localPositionZCurve.Evaluate(keyframe.frame);
                                keyframe.localPosition3.inTangentZ = AnimationHelper.CalculateTangent(boneCurves.localPositionZCurve, (float)keyframe.frame, keyframe.localPosition3.val.z);
                                keyframe.localPosition3.outTangentZ = keyframe.localPosition3.inTangentZ;

                                keyframe.localRotation.val = boneCurves.localRotationCurve.Evaluate(keyframe.frame);
                                keyframe.localRotation.inTangent = AnimationHelper.CalculateTangent(boneCurves.localRotationCurve, (float)keyframe.frame, keyframe.localRotation.val);
                                keyframe.localRotation.outTangent = keyframe.localRotation.inTangent;
                            }
                        }
                    }

                    AnimationHelper.refreshAnimationEditorWindowPostCycle = true;
                }

                ResetSelectedFrames();

                editor.SetNeedsRepainted();
                SetFrameGridNeedsRebuilt();
            }
        }

        static public void CutSelectedKeyframes()
        {
            editor.SetWillBeDirty();
            
            _clipboardKeyframes.Clear();

            //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();

            foreach (KeyframeSM keyframe in _selectedKeyframes)
            {
                _clipboardKeyframes.Add(keyframe);

                //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
            }

            AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

            editor.boneAnimationData.RemoveSelectedKeyframes(ClipBrowserWindow.SelectedAnimationClipIndex, ref _selectedFrames);

            ResetSelectedFrames();

            editor.SetNeedsRepainted();
            SetFrameGridNeedsRebuilt();
        }

        static public void RemoveSelectedKeyframes(int clipIndex)
        {
            if (EditorUtility.DisplayDialog("Delete Keyframes Confirmation", "Are you sure you want to delete these keyframes?", "Yes", "No"))
            {
                editor.SetWillBeDirty();
                
                editor.Focus();

                editor.boneAnimationData.RemoveSelectedKeyframes(clipIndex, ref _selectedFrames);

                //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                //foreach (BoneFrame boneFrame in _selectedFrames)
                //{
                //    AnimationHelper.AddBoneDataIndexToRefreshList(boneFrame.boneDataIndex);
                //}

                AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                ResetSelectedFrames();

                editor.SetNeedsRepainted();
                SetFrameGridNeedsRebuilt();
            }
        }
		
        static public void SelectFramePostCycle()
        {
            if (_selectFramePostCycle)
            {
				if (_selectBoneFrame.frame < FirstVisibleFrame)
				{
					FirstVisibleFrame = _selectBoneFrame.frame;
				}

				if (_selectBoneFrame.frame > (FirstVisibleFrame + _visibleFrameCount))
				{
					FirstVisibleFrame = _selectBoneFrame.frame - _visibleFrameCount;
				}
				
				if (_selectBoneFrame.boneNodeIndex < BoneWindow.FirstVisibleBone)
				{
					BoneWindow.FirstVisibleBone = _selectBoneFrame.boneNodeIndex;
				}
				
				if (_selectBoneFrame.boneNodeIndex >= (BoneWindow.FirstVisibleBone + BoneWindow.VisibleBoneCount))
			    {
					BoneWindow.FirstVisibleBone = _selectBoneFrame.boneNodeIndex - BoneWindow.VisibleBoneCount + 1;
				}
				
                SelectFrame(_selectBoneFrame.boneNodeIndex, _selectBoneFrame.frame);

                _selectFramePostCycle = false;
                editor.SetNeedsRepainted();
                SetFrameGridNeedsRebuilt();
            }
        }

        static public void LostFocus()
        {
        }

        static private void BuildFrameGrid()
        {
            if (ClipBrowserWindow.SelectedAnimationClipIndex != -1)
            {
                GUIStyle currentFrameStyle;
                int firstBone = BoneWindow.FirstVisibleBone;
                int lastBone = Mathf.Min(BoneWindow.FirstVisibleBone + BoneWindow.VisibleBoneCount, editor.boneAnimationData.BoneCount);
                int firstFrame = FirstVisibleFrame;
                int lastFrame = Mathf.Min(FirstVisibleFrame + _visibleFrameCount, AnimationClipSM.MAX_FRAMES);
                bool slidingFrame;

                _frameGridBoneCount = (lastBone - firstBone);
                _frameGridFrameCount = (lastFrame - firstFrame + 1);

                if (_frameGridBoneCount <= 0 || _frameGridFrameCount <= 0)
                    return;

                _frameGrid = new GUIStyle[_frameGridBoneCount, _frameGridFrameCount];

                for (int boneNodeIndex = firstBone; boneNodeIndex < lastBone; boneNodeIndex++)
                {
                    GUILayout.BeginHorizontal();
                    for (int frame = firstFrame; frame <= lastFrame; frame++)
                    {
                        slidingFrame = false;
                        currentFrameStyle = Style.gridFrameStyle;

                        GUILayout.Space(1.0f);

                        if (_keyframesAreSliding)
                        {
                            foreach (SlidingKeyframe slidingKeyframe in _slidingKeyframes)
                            {
                                if (slidingKeyframe.frame == frame && slidingKeyframe.boneNodeIndex == boneNodeIndex)
                                {
                                    slidingFrame = true;

                                    if (_lastFrameOffset == 0)
                                    {
                                        currentFrameStyle = Style.whiteStyle;
                                    }
                                    else
                                    {
                                        if (editor.boneAnimationData.GetKeyframeIsSet(ClipBrowserWindow.SelectedAnimationClipIndex, boneNodeIndex, frame))
                                        {
                                            if (IsThisASlidingKeyframe(boneNodeIndex, frame))
                                                currentFrameStyle = Style.whiteStyle;
                                            else
                                                currentFrameStyle = Style.keyframeAlreadySetStyle;
                                        }
                                        else
                                            currentFrameStyle = Style.whiteStyle;
                                    }

                                    break;
                                }
                            }
                        }

                        if (!slidingFrame)
                        {
                            if (editor.boneAnimationData.GetKeyframeIsSet(ClipBrowserWindow.SelectedAnimationClipIndex, boneNodeIndex, frame))
                            {
                                if (_keyframesAreSliding)
                                {
                                    if (IsThisASlidingKeyframe(boneNodeIndex, frame))
                                    {
                                        slidingFrame = true;
                                        currentFrameStyle = Style.windowRectBackgroundStyle;
                                    }
                                }

                                if (!slidingFrame)
                                {
                                    if (IsFrameSelected(boneNodeIndex, frame))
                                    {
                                        if (IsFrameEmpty(ClipBrowserWindow.SelectedAnimationClipIndex, boneNodeIndex, frame))
                                        {
                                            currentFrameStyle = Style.selectedEmptyKeyframeWarningStyle;
                                        }
                                        else
                                        {
                                            currentFrameStyle = Style.setValueSelectedStyle;
                                        }
                                    }
                                    else
                                    {
                                        if (IsFrameEmpty(ClipBrowserWindow.SelectedAnimationClipIndex, boneNodeIndex, frame))
                                        {
                                            currentFrameStyle = Style.emptyKeyframeWarningStyle;
                                        }
                                        else
                                        {
                                            currentFrameStyle = Style.setValueStyle;
                                        }
                                    }
                                }
                            }
                            else if (IsFrameSelected(boneNodeIndex, frame))
                            {
                                if (_keyframesAreSliding)
                                {
                                    currentFrameStyle = Style.targetStyle;
                                }
                                else
                                {
                                    if (_selectingFrames)
                                        currentFrameStyle = Style.selectionWorkingStyle;
                                    else
                                        currentFrameStyle = Style.selectionDoneStyle;
                                }
                            }
                            else
                            {
                                if (((frame % TIMELINE_FRAME_LABEL_INTERVAL) == 0) ||
                                    (frame == FIRST_EDITABLE_FRAME) ||
                                    (frame == (AnimationClipSM.MAX_FRAMES))
                                    )
                                    currentFrameStyle = Style.gridFrameMarkerStyle;
                                else
                                    currentFrameStyle = Style.gridFrameStyle;
                            }
                        }

                        _frameGrid[boneNodeIndex - firstBone, frame - firstFrame] = currentFrameStyle;
                    }
                }
            }

            if (_selectedFrames != null)
            {
                if (_selectedFrames.Count > 0)
                {
                    if (_selectedKeyframes != null)
                    {
                        if (_selectedKeyframes.Count > 0)
                        {
                            float minY = (_selectionBounds.yMin - BoneWindow.FirstVisibleBone + 1) * TIMELINE_FRAME_HEIGHT;
                            float maxY = (_selectionBounds.yMax - BoneWindow.FirstVisibleBone + 1) * TIMELINE_FRAME_HEIGHT;
                            float y = ((maxY - minY) * 0.5f) + minY + 40.0f;

                            if ((y - _timeLineRect.yMin) <= -32)
                            {
                                y = -1000;
                            }
                            else if ((y - _timeLineRect.yMin) >= (_timeLineRect.height - 32))
                            {
                                y = -1000;
                            }

                            _moveSelectedFramesBackwardRect.x = ((_selectionBounds.xMin - _firstVisibleFrame) * (TIMELINE_FRAME_WIDTH)) - Resources.moveSelectedFramesBackward_Off.width;
                            _moveSelectedFramesBackwardRect.y = y;

                            _moveSelectedFramesForwardRect.x = (_selectionBounds.xMax + 1 - _firstVisibleFrame) * (TIMELINE_FRAME_WIDTH) + 5.0f;
                            _moveSelectedFramesForwardRect.y = y;
                        }
                        else
                        {
                            _moveSelectedFramesBackwardRect.x = -1000.0f;
                            _moveSelectedFramesForwardRect.x = -1000.0f;
                        }
                    }
                }
            }

            _frameGridNeedsRebuilt = false;
        }

        static public void SetFrameGridNeedsRebuilt()
        {
            _frameGridNeedsRebuilt = true;
        }

        static private void InsertFramesAtSelection()
        {
            if (_selectedFrames == null)
                return;

            if (_selectedFrames.Count == 0)
                return;

            int maxFrame = -1;
            int tempMaxFrame;

            foreach (AnimationClipBone bone in editor.boneAnimationData.animationClips[ClipBrowserWindow.SelectedAnimationClipIndex].bones)
            {
                tempMaxFrame = bone.GetMaxFrame();
                if (tempMaxFrame > maxFrame)
                    maxFrame = tempMaxFrame;
            }

            int atFrame;
            int lastFrame;
            int frameCount;

            atFrame = (int)_selectionBounds.xMin;
            lastFrame = (int)_selectionBounds.xMax;
            frameCount = lastFrame - atFrame + 1;

            if ((maxFrame + frameCount) > AnimationClipSM.MAX_FRAMES)
            {
                EditorUtility.DisplayDialog("Warning", "Inserting [" + frameCount.ToString() + "] frame" + (frameCount > 1 ? "s" : "") + " here would push the last keyframe [" + maxFrame.ToString() + "] beyond the limit of [" + AnimationClipSM.MAX_FRAMES.ToString() + "]", "OK");
                return;
            }

            editor.SetWillBeDirty();

            //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
            foreach (BoneFrame boneFrame in _selectedFrames)
            {
                if (boneFrame.frame == atFrame)
                {
                    editor.boneAnimationData.InsertFramesForBoneDataIndex(ClipBrowserWindow.SelectedAnimationClipIndex, boneFrame.boneDataIndex, atFrame, frameCount);
                    //AnimationHelper.AddBoneDataIndexToRefreshList(boneFrame.boneDataIndex);
                }
            }

            AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

            ResetSelectedFrames();

            SetFrameGridNeedsRebuilt();
            editor.SetNeedsRepainted();
        }

        static private void DeleteFramesAtSelection()
        {
            int atFrame;
            int lastFrame;
            int frameCount;

            atFrame = (int)_selectionBounds.xMin;
            lastFrame = (int)_selectionBounds.xMax;
            frameCount = lastFrame - atFrame + 1;

            if (atFrame == 0)
            {
                EditorUtility.DisplayDialog("Warning", "Cannot delete frame 0", "OK");
                return;
            }

            bool resume = false;

            if (_selectedKeyframes == null)
            {
                resume = true;
            }
            else
            {
                if (_selectedKeyframes.Count > 0)
                {
                    if (EditorUtility.DisplayDialog("Delete Keyframes Confirmation", "Deleting these frames will also delete the selected keyframes. Are you sure you want to delete these keyframes?", "Yes", "No"))
                    {
                        resume = true;
                    }
                }
                else
                {
                    resume = true;
                }
            }

            if (resume)
            {
                editor.SetWillBeDirty();

                editor.Focus();

                editor.boneAnimationData.RemoveSelectedKeyframes(ClipBrowserWindow.SelectedAnimationClipIndex, ref _selectedFrames);

                //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                foreach (BoneFrame boneFrame in _selectedFrames)
                {
                    if (boneFrame.frame == atFrame)
                    {
                        editor.boneAnimationData.DeleteFramesForBoneDataIndex(ClipBrowserWindow.SelectedAnimationClipIndex, boneFrame.boneDataIndex, atFrame, frameCount);
                        //AnimationHelper.AddBoneDataIndexToRefreshList(boneFrame.boneDataIndex);
                    }
                }

                AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                ResetSelectedFrames();

                editor.SetNeedsRepainted();
                SetFrameGridNeedsRebuilt();
            }
        }
    }
}
