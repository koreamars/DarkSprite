using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace SmoothMoves
{
    public delegate void AnimationCurveChangedDelegate(ref AnimationCurve animationCurve, Keyframe key);
    public delegate void AnimationCurveAddKeyDelegate(ref AnimationCurve animationCurve, Keyframe key);
    public delegate void AnimationCurveRemoveKeyDelegate(ref AnimationCurve animationCurve, int keyIndex, int frame);

    public class AnimationCurveEditorWindow : EditorWindow
    {
        private const float UPDATE_INTERVAL = 1.0f;
        private const float MIN_WIDTH = 502.0f;
        private const float MIN_HEIGHT = 480.0f;
        private const float TIME_INTERPOLATION_INTERVAL = 0.1f;
        private const float X_BUFFER = 30.0f;
        private const float Y_BUFFER = 30.0f;
        private const float SCROLL_WHEEL_SENSITIVITY = 0.03f;
        private const float MIN_ZOOM = 0.1f;
        private const float MAX_ZOOM = 10.0f;
        private const float X_BORDER = 40.0f;
        private const float Y_TOP_BORDER = 30.0f;
        private const float Y_BOTTOM_BORDER = 25.0f;
        private const float TOOLBAR_WIDTH = 110.0f;
        private const float GRID_TIME_INTERVALS = 10.0f;
        private const float GRID_VALUE_INTERVALS = 10.0f;
        private const float TIME_LABEL_WIDTH = 30.0f;
        private const float TIME_LABEL_HEIGHT = 23.0f;
        private const float VALUE_LABEL_WIDTH = 35.0f;
        private const float VALUE_LABLE_HEIGHT = 23.0f;
        private const float MIN_ZOOM_LABELS = 0.5f;
        private const float COLLISION_ARC_BUFFER = 10.0f;

        private enum ACTION
        {
            None,
            DragScreen,
            MoveNode,
            RotatingInTangent,
            RotatingOutTangent
        }

        static private AnimationCurveEditorWindow _instance;

        private AnimationCurve _animationCurve;
        private List<Vector2> _interpolationPoints;
        private List<Vector2> _scaledInterpolationPoints;
        private List<Rect> _scaledNodeRects;
        private List<Vector2> _scaledNodePoints;
        private Rect _curveBounds;
        private Rect _curveDrawingRect;
        private Vector2 _centerOfCurveDrawingRect;
        private Vector2 _curveOffset;
        private float _zoom;
        private Vector2 _boundRatio;
        private Rect _curveRect;
        private Rect _lastPosition;
        private int _selectedKeyframeIndex;
        private bool _needsRepainted;
        private bool _curveNeedsScaled;
        private bool _curveNeedsRebuilt;
        private bool _curveNeedsRebounded;
        private Rect[] _borderRects;
        private Rect _timeLabelRect;
        private Rect _valueLabelRect;
        private Rect _infoRect;
        private ACTION _action;
        private Vector2 _dragStartPosition;
        private Vector2 _dragStartOrigin;
        private Vector2 _scaledCurveOffset;
        private Vector2 _xAxisStart;
        private Vector2 _xAxisEnd;
        private Vector2 _yAxisStart;
        private Vector2 _yAxisEnd;
        private Vector2 _curveOrigin;
        private Vector2 _gridSpacer;
        private int _timeLabelCount;
        private List<ValueLabel> _timeLabels;
        private int _valueLabelCount;
        private List<ValueLabel> _valueLabels;
        private float _gridTimeSpacer;
        private float _gridValueSpacer;
        private Rect _gizmoTangentHandle;
        private float _gizmoTangentHalfHeight;
        private float _tangentRatio;
        private CollisionArc _tangentInArc;
        private CollisionArc _tangentOutArc;
        private AnimationCurveChangedDelegate _animationCurveChangedDelegate;
        private AnimationCurveAddKeyDelegate _animationCurveAddKeyDelegate;
        private AnimationCurveRemoveKeyDelegate _animationCurveRemoveKeyDelegate;
        private KeyframeSM.CURVE_PROPERTY _curveProperty;
        private string _animationName;
        private string _boneName;
        private bool _showAddFrame;
        private float _addDistanceSquared;
        private Rect _addFrameRect;
        private int _addFrame;
        private float _addValue;
        private Vector2 _addFramePosition;
        private int _animationIndex;
        private int _boneDataIndex;
        private Rect _currentTimeRect;
        private bool _addPointsOn;
        private AnimationClipBone _animationClipBone;

        private Color _curveColor;
        private Color _gridColor;
        private Color _gizmoOffColor;
        private Color _gizmoOnColor;
        private Color _gizmoDisabledColor;

        private bool AddPointsOn
        {
            get
            {
                return _addPointsOn;
            }
            set
            {
                _addPointsOn = value;
                PlayerPrefs.SetInt("SmoothMoves_AnimationCurveEditor_AddPointsOn", (_addPointsOn ? 1 : 0));
            }
        }

        static public AnimationCurveEditorWindow Instance
        {
            get
            {
                return _instance;
            }
        }

        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }

        public KeyframeSM.CURVE_PROPERTY CurveProperty { get { return _curveProperty; } }
        public int BoneDataIndex { get { return _boneDataIndex; } }

        static public AnimationCurveEditorWindow ShowEditor()
        {
            if (_instance != null)
            {
                _instance.Close();
            }

            _instance = (AnimationCurveEditorWindow)EditorWindow.GetWindow(typeof(AnimationCurveEditorWindow), true, "SmoothMoves Curve Editor");

            return _instance;
        }

        void OnEnable()
        {
            Style.Reset();
            //AnimationCurveResources.OnEnable();
            AnimationCurveResources.LoadTextures();

            _curveColor = Style.GetStyleBackgroundColor(Style.setValueStyle);
            _gridColor = Style.GetStyleBackgroundColor(Style.gridLineStyle);
            _gizmoOffColor = Style.GetStyleBackgroundColor(Style.selectionDoneStyle);
            _gizmoOnColor = Style.GetStyleBackgroundColor(Style.targetStyle);
            _gizmoDisabledColor = Style.Grey5;

            _selectedKeyframeIndex = -1;
            _zoom = 1.0f;
            _curveOffset = Vector2.zero;

            SetRects();

            _gizmoTangentHandle = new Rect(0, 0, AnimationCurveResources.gizmoCurveTangentHandle.width, AnimationCurveResources.gizmoCurveTangentHandle.height);
            _gizmoTangentHalfHeight = _gizmoTangentHandle.height * 0.5f;

            _gridSpacer = Vector2.one;

            _timeLabels = new List<ValueLabel>();
            _valueLabels = new List<ValueLabel>();

            _tangentInArc = new CollisionArc(-COLLISION_ARC_BUFFER, COLLISION_ARC_BUFFER, _gizmoTangentHandle.width);
            _tangentOutArc = new CollisionArc(-COLLISION_ARC_BUFFER, COLLISION_ARC_BUFFER, _gizmoTangentHandle.width);

            _addFrameRect = new Rect(0, 0, AnimationCurveResources.gizmoCurveAddNode.width, AnimationCurveResources.gizmoCurveAddNode.height);
            _addDistanceSquared = Mathf.Pow((AnimationCurveResources.gizmoCurveAddNode.width * 0.5f), 2);

            _currentTimeRect = new Rect(0, 0, 40.0f, 23.0f);

            _addPointsOn = (PlayerPrefs.GetInt("SmoothMoves_AnimationCurveEditor_AddPointsOn", 1) == 1);

            wantsMouseMove = true;

            //FieldInfo undoCallback = typeof(EditorApplication).GetField("undoRedoPerformed", BindingFlags.NonPublic | BindingFlags.Static);
            //undoCallback.SetValue(null, (EditorApplication.CallbackFunction)editor.UndoPerformed);
        }

        void OnGUI()
        {
            if (_animationCurve == null)
                return;

            if (_timeLabels == null)
                return;

            if (_timeLabels.Count == 0)
                return;

            if (EditorApplication.isPlaying)
            {
                GUILayout.BeginVertical();
                GUILayout.Space((position.height / 2.0f) - 20.0f);
                GUILayout.Label("Editing curves is disabled while the editor is playing", Style.centeredTextStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();

                return;
            }

            if (_lastPosition != position)
            {
                Rect newPosition = new Rect(position.x, position.y, Mathf.Max(position.width, MIN_WIDTH), Mathf.Max(position.height, MIN_HEIGHT));

                position = newPosition;

                if (_lastPosition != position)
                {
                    SetRects();
                    _curveNeedsScaled = true;
                }
            }

            Keyframe key;

            GUIHelper.DrawBox(_curveDrawingRect, Style.windowRectBackgroundStyle, true);

            if (_scaledInterpolationPoints == null || _scaledNodeRects == null || _scaledNodePoints == null)
                return;

            if (_curveNeedsRebuilt)
            {
                BuildCurve(_curveNeedsRebounded);
                _curveNeedsRebounded = false;
            }

            if (_curveNeedsScaled)
            {
                ScaleCurveIntoBounds();
            }

            if (_zoom > MIN_ZOOM_LABELS)
            {
                GUIHelper.DrawGridFromOrigin(_curveOrigin,
                                                            _curveDrawingRect,
                                                            _gridSpacer.x,
                                                            _gridSpacer.y,
                                                            _gridColor);
            }

            GUIHelper.DrawLine(_xAxisStart, _xAxisEnd, Style.xAxisStyle);
            GUIHelper.DrawLine(_yAxisStart, _yAxisEnd, Style.yAxisStyle);

            GUIHelper.Draw2DLines(_scaledInterpolationPoints.ToArray(), _curveColor);

            foreach (Rect r in _scaledNodeRects)
            {
                GUI.DrawTexture(r, AnimationCurveResources.gizmoCurveNode);
            }

            if (_showAddFrame)
            {
                GUI.DrawTexture(_addFrameRect, AnimationCurveResources.gizmoCurveAddNode);
            }

            if (_selectedKeyframeIndex > (_animationCurve.keys.Length - 1))
                _selectedKeyframeIndex = -1;

            if (_selectedKeyframeIndex != -1)
            {
                key = _animationCurve.keys[_selectedKeyframeIndex];

                _gizmoTangentHandle.x = _scaledNodePoints[_selectedKeyframeIndex].x;
                _gizmoTangentHandle.y = _scaledNodePoints[_selectedKeyframeIndex].y - _gizmoTangentHalfHeight;

                Style.PushColor(_gizmoOffColor);

                Matrix4x4 backupMatrix;
                backupMatrix = GUI.matrix;

                if (_selectedKeyframeIndex > 0)
                {
                    _tangentInArc.Update(_scaledNodePoints[_selectedKeyframeIndex], 180.0f - Mathf.Atan(key.inTangent * _tangentRatio) * Mathf.Rad2Deg);

                    if (_action == ACTION.RotatingInTangent)
                    {
                        Style.PushColor(_gizmoOnColor);
                    }
                    else
                    {
                        switch ((KeyframeSM.TANGENT_MODE)key.tangentMode)
                        {
                            case KeyframeSM.TANGENT_MODE.Smooth:
                                Style.PushColor(_gizmoOffColor);
                                break;

                            case KeyframeSM.TANGENT_MODE.LeftFreeRightConstant:
                            case KeyframeSM.TANGENT_MODE.LeftFreeRightFree:
                            case KeyframeSM.TANGENT_MODE.LeftFreeRightLinear:
                                Style.PushColor(Color.green);
                                break;

                            default:
                                Style.PushColor(_gizmoDisabledColor);
                                break;
                        }
                    }

                    GUIUtility.RotateAroundPivot(_tangentInArc.RotationDeg, _scaledNodePoints[_selectedKeyframeIndex]);
                    GUI.DrawTexture(_gizmoTangentHandle, AnimationCurveResources.gizmoCurveTangentHandle);
                    GUI.matrix = backupMatrix;
                    Style.PopColor();
                }

                if (_selectedKeyframeIndex < (_animationCurve.keys.Length - 1))
                {
                    _tangentOutArc.Update(_scaledNodePoints[_selectedKeyframeIndex], -Mathf.Atan(key.outTangent * _tangentRatio) * Mathf.Rad2Deg);

                    if (_action == ACTION.RotatingOutTangent)
                    {
                        Style.PushColor(_gizmoOnColor);
                    }
                    else
                    {
                        switch ((KeyframeSM.TANGENT_MODE)key.tangentMode)
                        {
                            case KeyframeSM.TANGENT_MODE.Smooth:
                                Style.PushColor(_gizmoOffColor);
                                break;

                            case KeyframeSM.TANGENT_MODE.LeftConstantRightFree:
                            case KeyframeSM.TANGENT_MODE.LeftLinearRightFree:
                            case KeyframeSM.TANGENT_MODE.LeftFreeRightFree:
                                Style.PushColor(Color.blue);
                                break;

                            default:
                                Style.PushColor(_gizmoDisabledColor);
                                break;
                        }
                    }

                    GUIUtility.RotateAroundPivot(_tangentOutArc.RotationDeg, _scaledNodePoints[_selectedKeyframeIndex]);
                    GUI.DrawTexture(_gizmoTangentHandle, AnimationCurveResources.gizmoCurveTangentHandle);
                    Style.PopColor();
                }

                GUI.matrix = backupMatrix;

                Style.PopColor();

                GUI.DrawTexture(_scaledNodeRects[_selectedKeyframeIndex], AnimationCurveResources.gizmoCurveNodeSelected);
            }

            _lastPosition = position;

            foreach (Rect borderRect in _borderRects)
            {
                GUIHelper.DrawBox(borderRect, Style.propertiesGroupStyle, false);
            }

            GUIHelper.DrawBox(_timeLabelRect, Style.windowRectBackgroundStyle, true);
            GUIHelper.DrawBox(_valueLabelRect, Style.windowRectBackgroundStyle, true);

            if (_zoom > MIN_ZOOM_LABELS)
            {
                for (int t = 0; t < _timeLabelCount; t++)
                {
                    _timeLabels[t].OnGUI(Style.centeredGreyTextStyle);
                }
                if (_addFrame != -1)
                {
                    _currentTimeRect.x = _addFramePosition.x - (_currentTimeRect.width * 0.5f);
                    _currentTimeRect.y = _curveDrawingRect.yMax;
                    GUI.Label(_currentTimeRect, _addFrame.ToString(), Style.highlightLabelStyle);
                }

                for (int t = 0; t < _valueLabelCount; t++)
                {
                    _valueLabels[t].OnGUI(Style.centeredGreyTextStyle);
                }
            }

            GUIHelper.DrawBox(_infoRect, Style.selectedInformationStyle, true);

            // overall layout
            GUILayout.BeginVertical();

            // top toolbar
            GUILayout.BeginHorizontal();
            GUILayout.Label("  Zoom: ", Style.normalLabelStyle, GUILayout.Width(50.0f));
            float newZoom = GUILayout.HorizontalSlider(_zoom, MIN_ZOOM, MAX_ZOOM, GUILayout.Width(_curveDrawingRect.width - 170.0f));
            if (newZoom != _zoom)
            {
                _zoom = newZoom;
                _curveNeedsScaled = true;
                _needsRepainted = true;
            }
            GUILayout.Label("x " + EditorHelper.RoundFloat(_zoom, 2).ToString(), Style.normalLabelStyle, GUILayout.Width(40.0f));
            if (GUILayout.Button("Fit to Window", GUILayout.Width(100.0f)))
            {
                _zoom = 1.0f;
                _curveOffset = Vector2.zero;
                _curveNeedsRebuilt = true;
                _curveNeedsRebounded = true;
            }

            bool newAddPointsOn;
            newAddPointsOn = GUILayout.Toggle(_addPointsOn, (_addPointsOn ? "Add Points On" : "Add Points Off"), GUI.skin.button);
            if (newAddPointsOn != _addPointsOn)
            {
                AddPointsOn = newAddPointsOn;

                if (!newAddPointsOn)
                {
                    _showAddFrame = false;
                }
            }

            GUILayout.EndHorizontal();
            // end top toolbar

            // main space
            GUILayout.BeginHorizontal();

            GUILayout.Space(position.width - TOOLBAR_WIDTH + 5.0f);

            // info box
            GUILayout.BeginVertical();

            GUILayout.Space(3.0f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(5.0f);
            GUILayout.Label(_animationName, Style.selectedInformationValueStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5.0f);
            GUILayout.Label(_boneName, Style.selectedInformationValueStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5.0f);
            GUILayout.Label(_curveProperty.ToString(), Style.selectedInformationValueStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(5.0f);

            if (_selectedKeyframeIndex != -1)
            {
                key = _animationCurve.keys[_selectedKeyframeIndex];

                GUILayout.BeginHorizontal();
                GUILayout.Space(5.0f);
                GUILayout.Label("Frame:", Style.selectedInformationFieldStyle, GUILayout.Width(45.0f));
                GUILayout.Space(5.0f);
                GUILayout.Label(Mathf.FloorToInt(key.time).ToString(), Style.selectedInformationValueStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(5.0f);
                GUILayout.Label("Value:", Style.selectedInformationFieldStyle, GUILayout.Width(45.0f));
                GUILayout.Space(5.0f);
                GUILayout.Label(EditorHelper.RoundFloat(key.value, 2).ToString(), Style.selectedInformationValueStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(5.0f);
                GUILayout.Label("InTan:", Style.selectedInformationFieldStyle, GUILayout.Width(45.0f));
                GUILayout.Space(5.0f);
                GUILayout.Label(Mathf.RoundToInt(Mathf.Atan(key.inTangent) * Mathf.Rad2Deg).ToString(), Style.selectedInformationValueStyle, GUILayout.Width(20.0f));
                GUILayout.Label(AnimationCurveResources.degree, GUILayout.Width(AnimationCurveResources.degree.width), GUILayout.Height(AnimationCurveResources.degree.height));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(5.0f);
                GUILayout.Label("OutTan:", Style.selectedInformationFieldStyle, GUILayout.Width(45.0f));
                GUILayout.Space(5.0f);
                GUILayout.Label(Mathf.RoundToInt(Mathf.Atan(key.outTangent) * Mathf.Rad2Deg).ToString(), Style.selectedInformationValueStyle, GUILayout.Width(20.0f));
                GUILayout.Label(AnimationCurveResources.degree, GUILayout.Width(AnimationCurveResources.degree.width), GUILayout.Height(AnimationCurveResources.degree.height));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            // info box

            GUILayout.EndHorizontal();
            // main space

            GUILayout.Space(20.0f);

            if (_selectedKeyframeIndex != -1)
            {
                key = _animationCurve.keys[_selectedKeyframeIndex];
                DrawButtons(key);
            }

            GUILayout.EndVertical();
            // overall layout

            OnInput();

            //if (Event.current.type == EventType.ValidateCommand)
            //{
            //    switch (Event.current.commandName)
            //    {
            //        case "UndoRedoPerformed":
            //            AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
            //            AnimationHelper.AddBoneDataIndexToRefreshList(_boneDataIndex);

            //            editor.UndoPerformed();

            //            editor.Repaint();
            //            break;
            //    }
            //}

            if (_needsRepainted)
            {
                Repaint();
                _needsRepainted = false;
            }
        }

        private void DrawButtons(Keyframe key)
        {
            // buttons space
            GUILayout.BeginHorizontal();

            GUILayout.Space(position.width - TOOLBAR_WIDTH + 5.0f);

            // buttons
            GUILayout.BeginVertical();

            Texture2D texture = null;

            GUILayout.BeginHorizontal();

            if (key.tangentMode == (int)KeyframeSM.TANGENT_MODE.Smooth)
                texture = AnimationCurveResources.curveEditorSmoothOn;
            else
                texture = AnimationCurveResources.curveEditorSmoothOff;
            if (GUILayout.Button(texture, Style.noBorderButtonStyle, GUILayout.Width(texture.width), GUILayout.Height(texture.height)))
            {
                editor.SetWillBeDirty();

                key.tangentMode = (int)KeyframeSM.TANGENT_MODE.Smooth;
                key.inTangent = key.outTangent;
                UpdateAnimationCurveKey(_selectedKeyframeIndex, key);
                _curveNeedsRebuilt = true;
            }

            GUILayout.Space(4.0f);

            if (key.tangentMode != (int)KeyframeSM.TANGENT_MODE.Smooth)
                texture = AnimationCurveResources.curveEditorBrokenOn;
            else
                texture = AnimationCurveResources.curveEditorBrokenOff;
            if (GUILayout.Button(texture, Style.noBorderButtonStyle, GUILayout.Width(texture.width), GUILayout.Height(texture.height)))
            {
                editor.SetWillBeDirty();

                key.tangentMode = (int)KeyframeSM.TANGENT_MODE.LeftFreeRightFree;
                UpdateAnimationCurveKey(_selectedKeyframeIndex, key);
                _needsRepainted = true;
            }

            GUILayout.EndHorizontal();

            texture = AnimationCurveResources.curveEditorFlat;
            if (GUILayout.Button(texture, Style.noBorderButtonStyle, GUILayout.Width(texture.width), GUILayout.Height(texture.height)))
            {
                switch ((KeyframeSM.TANGENT_MODE)key.tangentMode)
                {
                    case KeyframeSM.TANGENT_MODE.Smooth:
                    case KeyframeSM.TANGENT_MODE.LeftFreeRightConstant:
                    case KeyframeSM.TANGENT_MODE.LeftFreeRightFree:
                    case KeyframeSM.TANGENT_MODE.LeftFreeRightLinear:
                        editor.SetWillBeDirty();

                        key.inTangent = 0;
                        break;
                }

                switch ((KeyframeSM.TANGENT_MODE)key.tangentMode)
                {
                    case KeyframeSM.TANGENT_MODE.Smooth:
                    case KeyframeSM.TANGENT_MODE.LeftConstantRightFree:
                    case KeyframeSM.TANGENT_MODE.LeftFreeRightFree:
                    case KeyframeSM.TANGENT_MODE.LeftLinearRightFree:
                        editor.SetWillBeDirty();

                        key.outTangent = 0;
                        break;
                }
                UpdateAnimationCurveKey(_selectedKeyframeIndex, key);
                _curveNeedsRebuilt = true;
            }

            if (key.tangentMode != (int)KeyframeSM.TANGENT_MODE.Smooth)
            {
                KeyframeSM.LEFT_RIGHT_TANGENT_MODE leftTangentMode;
                KeyframeSM.LEFT_RIGHT_TANGENT_MODE rightTangentMode;

                leftTangentMode = DrawTangentButtons(true, key);
                rightTangentMode = DrawTangentButtons(false, key);

                KeyframeSM.TANGENT_MODE newTangentMode = AnimationHelper.GetTangentModeFromLeftRight(leftTangentMode, rightTangentMode);

                if (key.tangentMode != (int)newTangentMode)
                {
                    editor.SetWillBeDirty();

                    key.tangentMode = (int)newTangentMode;

                    switch (leftTangentMode)
                    {
                        case KeyframeSM.LEFT_RIGHT_TANGENT_MODE.Constant:
                            key.inTangent = Mathf.Infinity;
                            break;
                    }

                    switch (rightTangentMode)
                    {
                        case KeyframeSM.LEFT_RIGHT_TANGENT_MODE.Constant:
                            key.outTangent = Mathf.Infinity;
                            break;
                    }

                    UpdateAnimationCurveKey(_selectedKeyframeIndex, key);

                    _curveNeedsRebuilt = true;
                }
            }

            GUILayout.Space(10.0f);

            GUILayout.Label("Value:", Style.normalLabelStyle);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(AnimationCurveResources.curveEditorSetZero, Style.noBorderButtonStyle, GUILayout.Width(AnimationCurveResources.curveEditorSetZero.width), GUILayout.Height(AnimationCurveResources.curveEditorSetZero.height)))
            {
                editor.SetWillBeDirty();

                key.value = 0;
                UpdateAnimationCurveKey(_selectedKeyframeIndex, key);
                _curveNeedsRebuilt = true;
            }
            if (GUILayout.Button(AnimationCurveResources.curveEditorSetOne, Style.noBorderButtonStyle, GUILayout.Width(AnimationCurveResources.curveEditorSetOne.width), GUILayout.Height(AnimationCurveResources.curveEditorSetOne.height)))
            {
                editor.SetWillBeDirty();

                key.value = 1.0f;
                UpdateAnimationCurveKey(_selectedKeyframeIndex, key);
                _curveNeedsRebuilt = true;
            }

            if (GUILayout.Button(AnimationCurveResources.curveEditorDeleteNode, Style.noBorderButtonStyle, GUILayout.Width(AnimationCurveResources.curveEditorDeleteNode.width), GUILayout.Height(AnimationCurveResources.curveEditorDeleteNode.height)))
            {
                RemoveKeyframe(_selectedKeyframeIndex);
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private KeyframeSM.LEFT_RIGHT_TANGENT_MODE DrawTangentButtons(bool left, Keyframe key)
        {
            KeyframeSM.LEFT_RIGHT_TANGENT_MODE tangentMode = KeyframeSM.LEFT_RIGHT_TANGENT_MODE.None;

            if ((left && _selectedKeyframeIndex > 0) || (!left && (_selectedKeyframeIndex < (_animationCurve.keys.Length - 1))))
            {
                Texture2D texture;

                GUILayout.Space(10.0f);

                tangentMode = AnimationHelper.GetLeftRightTangentMode(left, key.tangentMode);

                GUILayout.Label((left ? "Left" : "Right") + " Tangent:", Style.normalLabelStyle);

                GUILayout.BeginHorizontal();

                if (tangentMode == KeyframeSM.LEFT_RIGHT_TANGENT_MODE.Free)
                    texture = AnimationCurveResources.curveEditorSideFreeOn;
                else
                    texture = AnimationCurveResources.curveEditorSideFreeOff;
                if (GUILayout.Button(texture, Style.noBorderButtonStyle, GUILayout.Width(texture.width), GUILayout.Height(texture.height)))
                {
                    tangentMode = KeyframeSM.LEFT_RIGHT_TANGENT_MODE.Free;
                }

                if (tangentMode == KeyframeSM.LEFT_RIGHT_TANGENT_MODE.Linear)
                    texture = AnimationCurveResources.curveEditorSideLinearOn;
                else
                    texture = AnimationCurveResources.curveEditorSideLinearOff;
                if (GUILayout.Button(texture, Style.noBorderButtonStyle, GUILayout.Width(texture.width), GUILayout.Height(texture.height)))
                {
                    tangentMode = KeyframeSM.LEFT_RIGHT_TANGENT_MODE.Linear;
                }

                if (tangentMode == KeyframeSM.LEFT_RIGHT_TANGENT_MODE.Constant)
                    texture = AnimationCurveResources.curveEditorSideConstantOn;
                else
                    texture = AnimationCurveResources.curveEditorSideConstantOff;
                if (GUILayout.Button(texture, Style.noBorderButtonStyle, GUILayout.Width(texture.width), GUILayout.Height(texture.height)))
                {
                    tangentMode = KeyframeSM.LEFT_RIGHT_TANGENT_MODE.Constant;
                }

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();

                if (tangentMode == KeyframeSM.LEFT_RIGHT_TANGENT_MODE.Free)
                {
                    texture = AnimationCurveResources.curveEditorFlat;
                    if (GUILayout.Button(texture, Style.noBorderButtonStyle, GUILayout.Width(texture.width), GUILayout.Height(texture.height)))
                    {
                        editor.SetWillBeDirty();

                        tangentMode = KeyframeSM.LEFT_RIGHT_TANGENT_MODE.Free;
                        if (left)
                            key.inTangent = 0;
                        else
                            key.outTangent = 0;
                        UpdateAnimationCurveKey(_selectedKeyframeIndex, key);
                        _curveNeedsRebuilt = true;
                    }
                }
            }

            return tangentMode;
        }

        private bool TangentHandleSelectable(bool inTangent, Keyframe key)
        {
            if (inTangent)
            {
                return (
                        (key.tangentMode == (int)KeyframeSM.TANGENT_MODE.LeftFreeRightConstant)
                        ||
                        (key.tangentMode == (int)KeyframeSM.TANGENT_MODE.LeftFreeRightLinear)
                        ||
                        (key.tangentMode == (int)KeyframeSM.TANGENT_MODE.LeftFreeRightFree)
                        ||
                        (key.tangentMode == (int)KeyframeSM.TANGENT_MODE.Smooth)
                        );
            }
            else
            {
                return (
                        (key.tangentMode == (int)KeyframeSM.TANGENT_MODE.LeftConstantRightFree)
                        ||
                        (key.tangentMode == (int)KeyframeSM.TANGENT_MODE.LeftLinearRightFree)
                        ||
                        (key.tangentMode == (int)KeyframeSM.TANGENT_MODE.LeftFreeRightFree)
                        ||
                        (key.tangentMode == (int)KeyframeSM.TANGENT_MODE.Smooth)
                        );
            }
        }

        private void OnInput()
        {
            Event evt = Event.current;
            Vector2 areaMousePos;
            float tan;
            Vector2 point;
            Keyframe key;
            int frame;
            float value;

            areaMousePos = evt.mousePosition;

            switch (evt.type)
            {
                case EventType.KeyDown:
                    switch (evt.keyCode)
                    {
                        case KeyCode.Delete:
                        case KeyCode.Backspace:
                            if (_selectedKeyframeIndex != -1)
                            {
                                RemoveKeyframe(_selectedKeyframeIndex);
                            }
                            break;
                    }
                    break;

                case EventType.MouseDown:
                    if (EditorHelper.LeftMouseButton(evt))
                    {
                        if (_showAddFrame)
                        {
                            if (_addFrameRect.Contains(areaMousePos))
                            {
                                AddKeyframe(_addFrame, _addValue);
                            }
                        }

                        for (int k = 0; k < _scaledNodeRects.Count; k++)
                        {
                            if (_scaledNodeRects[k].Contains(areaMousePos))
                            {
                                _action = ACTION.MoveNode;
                                _selectedKeyframeIndex = k;
                                _dragStartPosition = areaMousePos;
                                _needsRepainted = true;
                                evt.Use();
                                break;
                            }
                        }

                        if (_action == ACTION.None)
                        {
                            if (_selectedKeyframeIndex != -1)
                            {
                                if (TangentHandleSelectable(true, _animationCurve.keys[_selectedKeyframeIndex]))
                                {
                                    if (_tangentInArc.CheckCollision(areaMousePos)
                                        &&
                                        _selectedKeyframeIndex > 0)
                                    {
                                        _action = ACTION.RotatingInTangent;
                                        evt.Use();
                                    }
                                }
                                if (TangentHandleSelectable(false, _animationCurve.keys[_selectedKeyframeIndex]))
                                {
                                    if (_tangentOutArc.CheckCollision(areaMousePos)
                                                &&
                                                _selectedKeyframeIndex < (_animationCurve.keys.Length - 1))
                                    {
                                        _action = ACTION.RotatingOutTangent;
                                        evt.Use();
                                    }
                                }
                            }
                        }
                    }
                    else if (EditorHelper.MiddleMouseButton(evt))
                    {
                        if (_curveDrawingRect.Contains(areaMousePos))
                        {
                            _dragStartPosition = areaMousePos;
                            _dragStartOrigin = _curveOffset;
                            _action = ACTION.DragScreen;
                            evt.Use();
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    GetFrameValue(areaMousePos.x, out _addFrame, out _addValue);
                    GetScaledPosition((float)_addFrame, _addValue, out _addFramePosition.x, out _addFramePosition.y);

                    switch (_action)
                    {
                        case ACTION.DragScreen:
                            _curveOffset = _dragStartOrigin + ((areaMousePos - _dragStartPosition) / _zoom);
                            _curveNeedsScaled = true;
                            _needsRepainted = true;
                            evt.Use();
                            break;

                        case ACTION.MoveNode:
                            if (_selectedKeyframeIndex != -1)
                            {
                                editor.SetWillBeDirty();

                                GetFrameValue(areaMousePos.x, areaMousePos.y, out frame, out value);

                                key = _animationCurve.keys[_selectedKeyframeIndex];

                                if (frame != (int)key.time && _selectedKeyframeIndex > 0)
                                {
                                    MoveKeyFrame((int)key.time, frame, value);
                                }
                                else
                                {
                                    key.value = value;
                                    UpdateAnimationCurveKey(_selectedKeyframeIndex, key);
                                    _curveNeedsRebuilt = true;
                                }

                                evt.Use();
                            }
                            break;

                        case ACTION.RotatingInTangent:
                            editor.SetWillBeDirty();

                            point = _scaledNodePoints[_selectedKeyframeIndex];
                            tan = 0;

                            if (areaMousePos.x < point.x)
                            {
                                tan = ((areaMousePos.y - point.y) / (point.x - areaMousePos.x)) / _tangentRatio;
                            }
                            else if (areaMousePos.x >= point.x)
                            {
                                tan = Mathf.Infinity;
                            }

                            key = _animationCurve.keys[_selectedKeyframeIndex];
                            key.inTangent = tan;

                            switch ((KeyframeSM.TANGENT_MODE)_animationCurve.keys[_selectedKeyframeIndex].tangentMode)
                            {
                                case KeyframeSM.TANGENT_MODE.Smooth:
                                    key.outTangent = tan;
                                    break;
                            }

                            UpdateAnimationCurveKey(_selectedKeyframeIndex, key);
                            _curveNeedsRebuilt = true;

                            evt.Use();
                            break;

                        case ACTION.RotatingOutTangent:
                            editor.SetWillBeDirty();

                            point = _scaledNodePoints[_selectedKeyframeIndex];
                            tan = 0;

                            if (areaMousePos.x > point.x)
                            {
                                tan = ((point.y - areaMousePos.y) / (areaMousePos.x - point.x)) / _tangentRatio;
                            }
                            else if (areaMousePos.x <= point.x)
                            {
                                tan = Mathf.Infinity;
                            }

                            key = _animationCurve.keys[_selectedKeyframeIndex];
                            key.outTangent = tan;

                            switch ((KeyframeSM.TANGENT_MODE)_animationCurve.keys[_selectedKeyframeIndex].tangentMode)
                            {
                                case KeyframeSM.TANGENT_MODE.Smooth:
                                    key.inTangent = tan;
                                    break;
                            }

                            UpdateAnimationCurveKey(_selectedKeyframeIndex, key);
                            _curveNeedsRebuilt = true;

                            evt.Use();
                            break;
                    }
                    break;

                case EventType.MouseUp:
                    _action = ACTION.None;
                    evt.Use();
                    break;

                case EventType.MouseMove:
                    if (_addPointsOn)
                    {
                        if (_curveDrawingRect.Contains(areaMousePos))
                        {
                            float distanceSquared;

                            GetFrameValue(areaMousePos.x, out _addFrame, out _addValue);
                            GetScaledPosition((float)_addFrame, _addValue, out _addFramePosition.x, out _addFramePosition.y);

                            if (AnimationCurveContainsFrame(_addFrame) == -1)
                            {
                                distanceSquared = (areaMousePos - new Vector2(_addFramePosition.x, _addFramePosition.y)).sqrMagnitude;

                                if (distanceSquared <= _addDistanceSquared)
                                {
                                    _showAddFrame = true;
                                    _addFrameRect.x = _addFramePosition.x - (AnimationCurveResources.gizmoCurveAddNode.width * 0.5f);
                                    _addFrameRect.y = _addFramePosition.y - (AnimationCurveResources.gizmoCurveAddNode.height * 0.5f);
                                }
                                else
                                {
                                    _showAddFrame = false;
                                }
                            }
                            else
                            {
                                _showAddFrame = false;
                            }

                            evt.Use();
                        }
                    }
                    else
                    {
                        _showAddFrame = false;
                    }
                    break;

                case EventType.ScrollWheel:
                    _zoom = Mathf.Clamp((_zoom - (evt.delta.y * SCROLL_WHEEL_SENSITIVITY)), MIN_ZOOM, MAX_ZOOM);
                    _curveNeedsScaled = true;
                    _needsRepainted = true;
                    evt.Use();
                    break;
            }
        }

        private void MoveKeyFrame(int fromFrame, int toFrame, float value)
        {
            if (toFrame <= 0)
                return;

            int fromIndex = AnimationCurveContainsFrame(fromFrame);

            if (fromIndex == -1)
                return;

            int toIndex = AnimationCurveContainsFrame(toFrame);
            Keyframe key = _animationCurve.keys[fromIndex];

            if (toIndex == -1)
            {
                key.time = (float)toFrame;
                key.value = value;
                _animationCurve.MoveKey(fromIndex, key);

                AnimationHelper.AdjustLinearTangents(_animationCurve, _animationClipBone, _curveProperty);

                if (_animationCurveRemoveKeyDelegate != null)
                {
                    _animationCurveRemoveKeyDelegate(ref _animationCurve, fromIndex, fromFrame);
                }
                if (_animationCurveAddKeyDelegate != null)
                {
                    _animationCurveAddKeyDelegate(ref _animationCurve, key);
                }

                _selectedKeyframeIndex = AnimationCurveContainsFrame(toFrame);
            }
        }

        private int AnimationCurveContainsFrame(int frame)
        {
            for (int keyIndex = 0; keyIndex < _animationCurve.keys.Length; keyIndex++)
            {
                if (_animationCurve.keys[keyIndex].time == (float)frame)
                    return keyIndex;
            }

            return -1;
        }

        private void SetRects()
        {
            _curveDrawingRect = new Rect(X_BORDER, Y_TOP_BORDER, position.width - TOOLBAR_WIDTH - X_BORDER, position.height - (Y_TOP_BORDER + Y_BOTTOM_BORDER));
            _curveRect = new Rect(_curveDrawingRect.xMin + X_BUFFER, _curveDrawingRect.yMin + Y_BUFFER, _curveDrawingRect.width - (X_BUFFER * 2.0f), _curveDrawingRect.height - (Y_BUFFER * 2.0f));

            _centerOfCurveDrawingRect = new Vector2(_curveDrawingRect.xMin + (_curveDrawingRect.width * 0.5f),
                                                    _curveDrawingRect.yMin + (_curveDrawingRect.height * 0.5f)
                                                    );

            float widthRatio;
            float heightRatio;

            if (_curveBounds.width == 0)
                widthRatio = 1.0f;
            else
                widthRatio = _curveRect.width / _curveBounds.width;

            if (_curveBounds.height == 0)
                heightRatio = 1.0f;
            else
                heightRatio = _curveRect.height / _curveBounds.height;

            _boundRatio = new Vector2(widthRatio, heightRatio);

            _borderRects = new Rect[4];
            _borderRects[0] = new Rect(0, 0, _curveDrawingRect.xMin, position.height);
            _borderRects[1] = new Rect(0, 0, position.width, _curveDrawingRect.yMin);
            _borderRects[2] = new Rect(0, _curveDrawingRect.yMax, position.width, position.height - _curveDrawingRect.yMax);
            _borderRects[3] = new Rect(_curveDrawingRect.xMax, 0, position.width - _curveDrawingRect.xMax, position.height);

            _xAxisStart = new Vector2(_curveDrawingRect.xMin, 0);
            _xAxisEnd = new Vector2(_curveDrawingRect.xMax, 0);

            _yAxisStart = new Vector2(0, _curveDrawingRect.yMin);
            _yAxisEnd = new Vector2(0, _curveDrawingRect.yMax);

            _timeLabelRect = new Rect(_curveDrawingRect.xMin, _curveDrawingRect.yMax + 2.0f, _curveDrawingRect.width, 20.0f);
            _valueLabelRect = new Rect(2.0f, _curveDrawingRect.yMin, _curveDrawingRect.xMin - 4.0f, _curveDrawingRect.height);

            _infoRect = new Rect(position.width - TOOLBAR_WIDTH + 5.0f, _curveDrawingRect.yMin, TOOLBAR_WIDTH - 10.0f, 115.0f);
        }

        private void ScaleCurveIntoBounds()
        {
            float x, y;

            if (_curveBounds.height == 0)
                _tangentRatio = 1.0f;
            else
                _tangentRatio = _curveBounds.width / _curveBounds.height;

            _scaledCurveOffset = new Vector2(_curveOffset.x - _centerOfCurveDrawingRect.x,
                                            -_curveOffset.y - _centerOfCurveDrawingRect.y);

            GetScaledPosition(0, 0, out x, out y);
            _curveOrigin = new Vector2(x, y);
            _xAxisStart.y = y;
            _xAxisEnd.y = y;
            _yAxisStart.x = x;
            _yAxisEnd.x = x;

            _gridSpacer.x = ((_curveRect.width * _gridTimeSpacer) / _curveBounds.width) * _zoom;
            _gridSpacer.y = ((_curveRect.height * _gridValueSpacer) / _curveBounds.height) * _zoom;

            if (_scaledInterpolationPoints == null)
                _scaledInterpolationPoints = new List<Vector2>();
            else
                _scaledInterpolationPoints.Clear();

            if (_scaledNodeRects == null)
                _scaledNodeRects = new List<Rect>();
            else
                _scaledNodeRects.Clear();

            if (_scaledNodePoints == null)
                _scaledNodePoints = new List<Vector2>();
            else
                _scaledNodePoints.Clear();

            Vector2 sv = Vector2.zero;
            foreach (Vector2 v in _interpolationPoints)
            {
                GetScaledPosition(v.x, v.y, out sv.x, out sv.y);
                _scaledInterpolationPoints.Add(sv);
            }

            Rect r = new Rect();
            int texWidth = AnimationCurveResources.gizmoCurveNode.width;
            int texHeight = AnimationCurveResources.gizmoCurveNode.height;
            foreach (Keyframe key in _animationCurve.keys)
            {
                GetScaledPosition(key.time, key.value, out x, out y);
                r.xMin = x - (texWidth * 0.5f);
                r.yMin = y - (texHeight * 0.5f);
                r.width = texWidth;
                r.height = texHeight;

                _scaledNodeRects.Add(r);
                _scaledNodePoints.Add(new Vector2(x, y));
            }

            CalculateLabels(_curveOrigin, _curveDrawingRect, _gridSpacer.x, _gridSpacer.y);

            GetScaledPosition((float)_addFrame, _addValue, out _addFramePosition.x, out _addFramePosition.y);

            _curveNeedsScaled = false;
            _needsRepainted = true;
        }

        private void CalculateLabels(Vector2 origin, Rect bounds, float xInterval, float yInterval)
        {
            float x, y;
            ValueLabel timeLabel;
            ValueLabel valueLabel;
            int rightXCount;
            int topYCount;
            int bottomYCount;
            int index = 0;

            rightXCount = Mathf.CeilToInt((bounds.xMax - origin.x) / xInterval);

            for (int i = 0; i < rightXCount; i++)
            {
                x = origin.x + (i * xInterval) - (TIME_LABEL_WIDTH * 0.5f);
                y = bounds.yMax;

                if (x > bounds.xMin && (x + TIME_LABEL_WIDTH) < bounds.xMax)
                {
                    timeLabel = GetTimeLabel(index, (i * _gridTimeSpacer).ToString());
                    timeLabel.rect.x = x;
                    timeLabel.rect.y = y;

                    index++;
                }
            }

            _timeLabelCount = index;

            index = 0;

            topYCount = Mathf.CeilToInt((origin.y - bounds.yMin) / yInterval);
            bottomYCount = Mathf.CeilToInt((bounds.yMax - origin.y) / yInterval);

            for (int i = 0; i < topYCount; i++)
            {
                x = bounds.xMin;
                y = origin.y - (i * yInterval) - (VALUE_LABLE_HEIGHT * 0.5f);

                if (y > bounds.yMin && (y + TIME_LABEL_HEIGHT) < bounds.yMax)
                {
                    valueLabel = GetValueLabel(index, (i * _gridValueSpacer).ToString());
                    valueLabel.rect.x = x - (valueLabel.rect.width);
                    valueLabel.rect.y = y;

                    index++;
                }
            }
            for (int i = 0; i < bottomYCount; i++)
            {
                x = bounds.xMin;
                y = origin.y + (i * yInterval) - (VALUE_LABLE_HEIGHT * 0.5f);

                if (y > bounds.yMin && (y + TIME_LABEL_HEIGHT) < bounds.yMax)
                {
                    valueLabel = GetValueLabel(index, (-i * _gridValueSpacer).ToString());
                    valueLabel.rect.x = x - (valueLabel.rect.width);
                    valueLabel.rect.y = y;

                    index++;
                }
            }

            _valueLabelCount = index;
        }

        private ValueLabel GetTimeLabel(int index, string text)
        {
            ValueLabel timeLabel;

            if ((_timeLabels.Count - 1) < index)
            {
                timeLabel = new ValueLabel(text, TIME_LABEL_WIDTH, TIME_LABEL_HEIGHT);
                _timeLabels.Add(timeLabel);
            }
            else
            {
                timeLabel = _timeLabels[index];
                timeLabel.text = text;
            }

            return timeLabel;
        }

        private ValueLabel GetValueLabel(int index, string text)
        {
            ValueLabel valueLabel;

            if ((_valueLabels.Count - 1) < index)
            {
                valueLabel = new ValueLabel(text, VALUE_LABEL_WIDTH, VALUE_LABLE_HEIGHT);
                _valueLabels.Add(valueLabel);
            }
            else
            {
                valueLabel = _valueLabels[index];
                valueLabel.text = text;
            }

            return valueLabel;
        }

        private void GetScaledPosition(float x, float y, out float newX, out float newY)
        {
            newX = ((((x - _curveBounds.xMin) * _boundRatio.x) + _scaledCurveOffset.x) * _zoom) + _centerOfCurveDrawingRect.x + _curveRect.xMin;
            newY = _centerOfCurveDrawingRect.y - ((((y - _curveBounds.yMin) * _boundRatio.y) + _scaledCurveOffset.y) * _zoom) - _curveRect.yMin;
        }

        private void GetFrameValue(float x, out int frame, out float value)
        {
            frame = Mathf.RoundToInt(((((x - _curveRect.xMin - _centerOfCurveDrawingRect.x) / _zoom) - _scaledCurveOffset.x) / _boundRatio.x) + _curveBounds.xMin);
            value = _animationCurve.Evaluate((float)frame);
        }

        private void GetFrameValue(float x, float y, out int frame, out float value)
        {
            frame = Mathf.RoundToInt(((((x - _curveRect.xMin - _centerOfCurveDrawingRect.x) / _zoom) - _scaledCurveOffset.x) / _boundRatio.x) + _curveBounds.xMin);
            value = ((((_centerOfCurveDrawingRect.y - _curveRect.yMin - y) / _zoom) - _scaledCurveOffset.y) / _boundRatio.y) + _curveBounds.yMin;
        }

        public void ResetAnimationCurve()
        {
            _animationCurve = null;
            _animationIndex = -1;
            _animationName = "";
            _boneDataIndex = -1;
            _animationClipBone = null;
            _boneName = "";
            _selectedKeyframeIndex = -1;
            _curveProperty = KeyframeSM.CURVE_PROPERTY.None;
            _animationCurveAddKeyDelegate = null;
            _animationCurveChangedDelegate = null;
            _animationCurveRemoveKeyDelegate = null;

            Repaint();
        }

        public void SetAnimationCurve(ref AnimationCurve animationCurve,
                                        int animationIndex,
                                        int boneDataIndex,
                                        KeyframeSM.CURVE_PROPERTY curveProperty,
                                        AnimationCurveChangedDelegate animationCurveChangedDelegate,
                                        AnimationCurveAddKeyDelegate animationCurveAddKeyDelegate,
                                        AnimationCurveRemoveKeyDelegate animationCurveRemoveKeyDelegate
                                        )
        {
            _animationCurve = animationCurve;
            _animationIndex = animationIndex;
            _boneDataIndex = boneDataIndex;
            _curveProperty = curveProperty;
            _animationCurveChangedDelegate = animationCurveChangedDelegate;
            _animationCurveAddKeyDelegate = animationCurveAddKeyDelegate;
            _animationCurveRemoveKeyDelegate = animationCurveRemoveKeyDelegate;

            _animationName = editor.boneAnimationData.animationClips[_animationIndex].animationName;
            _boneName = editor.boneAnimationData.boneDataList[_boneDataIndex].boneName;
            _animationClipBone = editor.boneAnimationData.GetAnimationClipBoneFromBoneDataIndex(_animationIndex, _boneDataIndex);

            _selectedKeyframeIndex = -1;

            BuildCurve(true);
        }

        public void RefreshCurve(AnimationCurve animationCurve,
                                    int animationIndex,
                                    int boneDataIndex,
                                    KeyframeSM.CURVE_PROPERTY curveProperty)
        {
            if (
                (curveProperty == _curveProperty)
                &&
                (animationIndex == _animationIndex)
                &&
                (boneDataIndex == _boneDataIndex)
                )
            {
                _animationCurve = animationCurve;
                _selectedKeyframeIndex = -1;
                BuildCurve(true);

                Repaint();
            }
        }

        public void ChangeBone(AnimationCurve animationCurve,
                                int boneDataIndex)
        {
            if (
                (_animationIndex != -1)
                &&
                (_curveProperty != KeyframeSM.CURVE_PROPERTY.None)
                )
            {
                _animationCurve = animationCurve;
                _boneDataIndex = boneDataIndex;

                _boneName = editor.boneAnimationData.boneDataList[_boneDataIndex].boneName;
                _animationClipBone = editor.boneAnimationData.GetAnimationClipBoneFromBoneDataIndex(_animationIndex, _boneDataIndex);

                _selectedKeyframeIndex = -1;

                BuildCurve(true);

                Repaint();
            }
        }

        private void BuildCurve(bool recalculateBounds)
        {
            if (_interpolationPoints == null)
                _interpolationPoints = new List<Vector2>();
            else
                _interpolationPoints.Clear();

            if (recalculateBounds)
            {
                _curveBounds = new Rect();
            }

            Vector2 interpolationPoint;
            Keyframe key;
            float duration;
            float intervalCount;
            float time;
            float value;

            for (int k = 0; k < _animationCurve.keys.Length; k++)
            {
                key = _animationCurve.keys[k];

                if (k < (_animationCurve.keys.Length - 1))
                {
                    duration = _animationCurve.keys[k + 1].time - key.time;
                    intervalCount = (duration / TIME_INTERPOLATION_INTERVAL);
                }
                else
                {
                    intervalCount = 1;
                }

                for (int t = 0; t < intervalCount; t++)
                {
                    time = key.time + (t * TIME_INTERPOLATION_INTERVAL);
                    value = _animationCurve.Evaluate(time);
                    interpolationPoint = new Vector2(time, value);
                    _interpolationPoints.Add(interpolationPoint);

                    if (recalculateBounds)
                    {
                        if (k == 0 && t == 0)
                        {
                            _curveBounds.xMin = time;
                            _curveBounds.xMax = time;
                            _curveBounds.yMin = value;
                            _curveBounds.yMax = value;
                        }
                        else
                        {
                            if (time < _curveBounds.xMin)
                                _curveBounds.xMin = time;
                            if (time > _curveBounds.xMax)
                                _curveBounds.xMax = time;
                            if (value < _curveBounds.yMin)
                                _curveBounds.yMin = value;
                            if (value > _curveBounds.yMax)
                                _curveBounds.yMax = value;
                        }
                    }
                }
            }

            if (recalculateBounds)
            {
                if (_curveBounds.height == 0)
                {
                    _curveBounds.yMin = _curveBounds.yMin - 10.0f;
                    _curveBounds.yMax = _curveBounds.yMax + 10.0f;
                }

                SetRects();
            }

            _gridTimeSpacer = Mathf.Ceil((_curveBounds.width / GRID_TIME_INTERVALS));
            _gridValueSpacer = Mathf.Ceil((_curveBounds.height / GRID_VALUE_INTERVALS));

            _curveNeedsRebuilt = false;

            ScaleCurveIntoBounds();
        }

        private void AddKeyframe(int frame, float value)
        {
            editor.SetWillBeDirty();

            float tan = 0;

            if (frame >= 1)
            {
                tan = AnimationHelper.CalculateTangent(_animationCurve, (float)frame, value);
            }

            Keyframe key = new Keyframe((float)frame, value, tan, tan);

            _selectedKeyframeIndex = _animationCurve.AddKey(key);

            if (_animationCurveAddKeyDelegate != null)
            {
                _animationCurveAddKeyDelegate(ref _animationCurve, key);
            }

            _showAddFrame = false;

            _curveNeedsRebuilt = true;
        }

        private void RemoveKeyframe(int keyIndex)
        {
            editor.SetWillBeDirty();

            if (keyIndex <= 0 || keyIndex > (_animationCurve.keys.Length - 1))
                return;

            int frame = (int)_animationCurve.keys[keyIndex].time;

            if (EditorUtility.DisplayDialog("Confirm Key Deletion", "Are you sure you want to delete the key at frame " + frame.ToString() + "?", "Yes", "No"))
            {
                editor.Focus();

                _animationCurve.RemoveKey(keyIndex);

                if (_animationCurveRemoveKeyDelegate != null)
                {
                    _animationCurveRemoveKeyDelegate(ref _animationCurve, keyIndex, frame);
                }

                _curveNeedsRebuilt = true;
            }
        }

        private void UpdateAnimationCurveKey(int keyIndex, Keyframe key)
        {
            editor.SetWillBeDirty();

            _animationCurve.MoveKey(keyIndex, key);
            AnimationHelper.AdjustLinearTangents(_animationCurve, _animationClipBone, _curveProperty);
            key = _animationCurve.keys[keyIndex];

            if (_animationCurveChangedDelegate != null)
            {
                _animationCurveChangedDelegate(ref _animationCurve, key);
            }
        }

        public void Refresh()
        {
            RefreshCurve(_animationCurve, _animationIndex, _boneDataIndex, _curveProperty);
        }
    }
}
