using UnityEngine;
using UnityEditor;
using System;

namespace SmoothMoves
{
    static public class BoneWindow
    {
        public const float MIN_WIDTH = 200.0f;
        public const float MAX_WIDTH = 300.0f;
        public const float BONE_COLOR_WIDTH = 8.0f;
        public const float BONE_COLOR_HEIGHT = 20.0f;

        private const float BONE_LABEL_WIDTH = 120.0f;
        private const float BONE_LABEL_HEIGHT = 20.0f;
        private const float BONE_LABEL_Y_SPACER = 3.0f;
        private const float BONE_LIST_TREE_INDENT = 20.0f;
        private const int FIRST_EDITABLE_BONE = 1;

        private enum HIERARCHY
        {
            NotSet = -1,
            Value = 0,
            Blank = 1,
            LastChild = 2,
            MiddleChild = 3,
            Vertical = 4
        }

        static private DraggableBone _draggableBone;

        static private int _visibleBoneCount;
        static private int _firstVisibleBone;
        static private int _clickedBoneIndex;
        static private int _lastClickedBoneIndex;
        static private int _editBoneIndex;
        static private Vector2 _boneListScrollPosition;
        static private int _dragBoneIndex;
        static private Vector2 _dragBoneStart;
        static private Vector2 _dragBonePosition;
        static private bool _draggingBone;
        static private int _dropTargetBoneIndex;
        static private double _lastScrollTime;
        static private bool _boneNeedsMoving;
        static private int _moveBoneIndex;
        static private int _moveBoneToIndex;
        static private bool _boneListNeedsScrolled;
        static private int _scrollBoneListTo;
        static private int _selectedBoneDataIndex;
        static private int _addBoneParentBoneNodeIndex;
        static private bool _focusTextbox;
        static private bool _unfocusTextbox;
        static private int _editOnRepaint;

        static private Rect _areaRect;
        static private Rect _rootBoneRect;
        static private Rect _boneListRect;
        static private Rect _labelRect;

        static private string _editBoneName;
        static private string _originalEditBoneName;
        static private bool _duplicateNameWarning;

        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }
        static public Rect AreaRect { get { return _areaRect; } set { _areaRect = value; } }
        static public float BoneWindowX 
        { 
            get 
            {
                return PlayerPrefs.GetFloat("SmoothMoves_Editor_BoneWindowX", 200.0f); 
            } 
            set 
            { 
                PlayerPrefs.SetFloat("SmoothMoves_Editor_BoneWindowX", Mathf.Min(Mathf.Max(value, MIN_WIDTH), MAX_WIDTH));
            } 
        }
        static public int SelectedBoneDataIndex { get { return _selectedBoneDataIndex; } set { _selectedBoneDataIndex = value; } }
        static public int FirstVisibleBone { get { return _firstVisibleBone; } set { _firstVisibleBone = value; } }
        static public int VisibleBoneCount { get { return _visibleBoneCount; } }
		static public bool BoneListNeedsScrolled { get { return _boneListNeedsScrolled; } set { _boneListNeedsScrolled = value; } }
		static public int ScrollBoneListTo { get { return _scrollBoneListTo; } set { _scrollBoneListTo = value; } }

        static public void OnEnable()
        {
            _firstVisibleBone = 1;
            _dragBoneIndex = -1;
            _addBoneParentBoneNodeIndex = -1;
            _editBoneIndex = -1;
            _editOnRepaint = -1;
        }

        static public void SetRects()
        {
            _rootBoneRect = new Rect(
                                     BoneAnimationDataEditorWindow.VERTICAL_SCROLL_SLIDER_WIDTH + BoneAnimationDataEditorWindow.PADDING,
                                     BoneAnimationDataEditorWindow.TOOLBAR_HEIGHT - 25.0f,
                                     _boneListRect.width - 60.0f,
                                     25.0f
                                     );

            _boneListRect = new Rect(
                                     BoneAnimationDataEditorWindow.VERTICAL_SCROLL_SLIDER_WIDTH + BoneAnimationDataEditorWindow.PADDING,
                                     BoneAnimationDataEditorWindow.TOOLBAR_HEIGHT + BoneAnimationDataEditorWindow.PADDING,
                                     _areaRect.width - (BoneAnimationDataEditorWindow.PADDING) - BoneAnimationDataEditorWindow.VERTICAL_SCROLL_SLIDER_WIDTH,
                                     _areaRect.height - (BoneAnimationDataEditorWindow.TOOLBAR_HEIGHT + (BoneAnimationDataEditorWindow.PADDING * 2.0f)) - BoneAnimationDataEditorWindow.SCROLLBAR_HEIGHT
                                     );

            _labelRect = new Rect(0, 0, _areaRect.width, 20.0f);

            _visibleBoneCount = Mathf.FloorToInt(_boneListRect.height / TimelineWindow.TIMELINE_FRAME_HEIGHT);
        }

        static private void PreGUIGetInput(Event evt)
        {
            if (editor.KeyboardFocus == BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.Bones)
            {
                if (evt.type == EventType.KeyDown)
                {
                    switch (evt.keyCode)
                    {
                        case KeyCode.KeypadEnter:
                        case KeyCode.Return:
                            if (_editBoneIndex != -1)
                            {
                                evt.Use();
                                SelectBone(_editBoneIndex);
                                StopEditingBone();
                            }
                            break;

                        case KeyCode.Escape:
                            if (_editBoneIndex != -1)
                            {
                                SelectBone(_editBoneIndex);
                                _editBoneIndex = -1;
                                _unfocusTextbox = true;
                                evt.Use();
                            }
                            break;
                    }
                }
            }
        }

        static public void OnGUI()
        {
            PreGUIGetInput(Event.current);

            GUIStyle currentBoneStyle;
            bool newMixTransform;
            bool mixing = false;

            if (ClipBrowserWindow.CurrentClip != null)
            {
                if (ClipBrowserWindow.CurrentClip.mix)
                {
                    mixing = true;
                }
            }

            CreateDraggableObjects();

            // Begin overall area
            GUILayout.BeginArea(_areaRect, GUIContent.none, Style.windowRectBackgroundStyle);

            GUIHelper.DrawBox(_labelRect, Style.maskStyle, true);

            GUIHelper.DrawBox(_boneListRect, Style.windowRectBackgroundStyle, true);

            // Begin overall vertical
            GUILayout.BeginVertical();

            GUILayout.Label("Bone Hierarchy", Style.centeredTextStyle);

            GUILayout.Space(30.0f);

            // Begin header horizontal
            GUILayout.BeginHorizontal();

            GUILayout.Space(BoneAnimationDataEditorWindow.PADDING);
            if (_draggingBone && _dropTargetBoneIndex == 0)
            {
                currentBoneStyle = Style.blankStyle;
                GUIHelper.DrawBox(_rootBoneRect, Style.targetStyle, true);
            }
            else if (IsBoneSelected(0))
            {
                currentBoneStyle = Style.blankStyle;
                GUIHelper.DrawBox(_rootBoneRect, Style.selectedInformationStyle, true);
            }
            else
            {
                currentBoneStyle = Style.blankStyle;
                GUIHelper.DrawBox(_rootBoneRect, Style.unSelectedInformationStyle, true);
            }

            GUI.SetNextControlName("RootBone");
            GUILayout.Label("     Root ", currentBoneStyle, GUILayout.Width(_boneListRect.width - 40.0f), GUILayout.Height(25.0f));
            if (_unfocusTextbox)
            {
                GUI.FocusControl("RootBone");
                _unfocusTextbox = false;
            }

            if (!TimelineWindow.IsPlaying && !editor.ModalPopup)
            {
                GUILayout.Space(5.0f);

                if (GUILayout.Button(new GUIContent(Resources.buttonAddLarge, "Add Bone as a Child to Root"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonAddLarge.width), GUILayout.Height(Resources.buttonAddLarge.height)))
                {
                    AddBone(0);
                }
            }

            GUILayout.EndHorizontal();
            // end header horizontal

            GUILayout.Space(BoneAnimationDataEditorWindow.PADDING + 1.0f);

            // begin bone list and slider horizontal
            GUILayout.BeginHorizontal();


            if (!editor.ModalPopup)
            {
                int newFirstVisibleBone = Convert.ToInt32(GUILayout.VerticalSlider((float)_firstVisibleBone, 1, editor.boneAnimationData.BoneCount - 1, GUILayout.Height(_boneListRect.height - BoneAnimationDataEditorWindow.SCROLLBAR_HEIGHT))); //, GUILayout.Width(VERTICAL_SCROLL_SLIDER_WIDTH)));
                if (newFirstVisibleBone != _firstVisibleBone)
                {
                    _firstVisibleBone = newFirstVisibleBone;
                    TimelineWindow.SetFrameGridNeedsRebuilt();
                }
            }
            else
            {
                GUILayout.Space(16.0f);
            }


            //_boneListScrollPosition = EditorGUILayout.BeginScrollView(_boneListScrollPosition, false, false, GUILayout.Width(_boneListRect.width - BoneAnimationDataEditorWindow.PADDING), GUILayout.Height(_boneListRect.height + BoneAnimationDataEditorWindow.SCROLLBAR_HEIGHT));
            _boneListScrollPosition = Vector2.zero;
            GUILayout.BeginVertical(Style.windowRectDarkBackgroundStyle, GUILayout.Width(_boneListRect.width - BoneAnimationDataEditorWindow.PADDING), GUILayout.Height(_boneListRect.height)); // + BoneAnimationDataEditorWindow.SCROLLBAR_HEIGHT));

            // begin bone list vertical
            GUILayout.BeginVertical();

            GUILayout.Space(1.0f);

            for (int boneIndex = _firstVisibleBone; boneIndex < Mathf.Min(_firstVisibleBone + _visibleBoneCount, editor.boneAnimationData.BoneCount); boneIndex++)
            {
                int clipIndex = -1;
                int boneDataIndex;
                AnimationClipBone clipBone = null;
                BoneData boneData;
                BoneCurves boneCurves;
                GUIContent guiContent;

                boneDataIndex = editor.boneAnimationData.GetBoneDataIndex(boneIndex);
                boneData = editor.boneAnimationData.GetBoneData(boneIndex);

                boneCurves = AnimationHelper.GetBoneDataCurves(boneDataIndex);

                clipIndex = ClipBrowserWindow.SelectedAnimationClipIndex;
                if (clipIndex != -1)
                {
                    clipBone = editor.boneAnimationData.animationClips[clipIndex].bones[boneDataIndex];
                }

                // bone horizontal
                GUILayout.BeginHorizontal();

                GUILayout.Space(2.0f);

                for (int d = 0; d < editor.boneAnimationData.dfsBoneNodeList[boneIndex].depth - 1; d++)
                {
                    GUILayout.Space(BONE_LIST_TREE_INDENT);
                }

                if (boneCurves != null)
                {
                    if (boneCurves.AnimationCurvesNeedTwoKeyframes)
                    {
                        guiContent = new GUIContent(Resources.warning, "At least one of your properties on this bone needs two or more keyframes to generate an animation curve");
                        GUILayout.Label(guiContent, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
                    }
                    else
                    {
                        GUILayout.Label(GUIContent.none, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
                    }
                }
                else
                {
                    GUILayout.Label(GUIContent.none, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
                }

                if (mixing)
                {
                    if (clipBone != null)
                    {
                        newMixTransform = GUILayout.Toggle(clipBone.mixTransform, GUIContent.none, GUILayout.Width(10.0f));
                        if (newMixTransform != clipBone.mixTransform)
                        {
                            editor.SetWillBeDirty(); ;
                            
                            clipBone.mixTransform = newMixTransform;
                        }
                    }
                }

                if (clipBone != null)
                {
                    if (clipBone.visible)
                    {
                        guiContent = new GUIContent(Resources.eyeOpen, "Hide Bone");
                    }
                    else
                    {
                        if (boneData.active)
                        {
                            guiContent = new GUIContent(Resources.eyeClosed, "Deactivate Bone");
                        }
                        else
                        {
                            guiContent = new GUIContent(Resources.boneInactive, "Show Bone");
                        }
                    }

                    //guiContent = new GUIContent((clipBone.visible ? Resources.eyeOpen : Resources.eyeClosed), (clipBone.visible ? "Hide Bone" : "Show Bone"));
                    if (GUILayout.Button(guiContent, Style.noBorderButtonStyle, GUILayout.Width(Resources.eyeOpen.width), GUILayout.Height(Resources.eyeOpen.height)))
                    {
                        editor.SetWillBeDirty();

                        if (clipBone.visible)
                        {
                            clipBone.visible = false;
                            boneData.active = true;
                        }
                        else
                        {
                            if (boneData.active)
                            {
                                boneData.active = false;
                            }
                            else
                            {
                                clipBone.visible = true;
                                boneData.active = true;
                            }
                        }

                        //clipBone.visible = !clipBone.visible;
                    }
                }

                if (_editBoneIndex == boneIndex)
                {
                    GUI.SetNextControlName("BoneTextbox");
                    _editBoneName = EditorGUILayout.TextField(_editBoneName, GUILayout.Width(BONE_LABEL_WIDTH), GUILayout.Height(BONE_LABEL_HEIGHT));

                    if (_focusTextbox)
                    {
                        GUI.FocusControl("BoneTextbox");
                        _focusTextbox = false;
                    }
                }
                else
                {
                    if (_dropTargetBoneIndex == boneIndex)
                    {
                        currentBoneStyle = Style.targetStyle;
                    }
                    else if (IsBoneSelected(boneIndex))
                    {
                        currentBoneStyle = Style.selectedInformationStyle;
                    }
                    else
                    {
                        if (clipBone != null)
                        {
                            if (clipBone.visible)
                            {
                                currentBoneStyle = Style.unSelectedInformationStyle;
                            }
                            else
                            {
                                if (boneData.active)
                                {
                                    currentBoneStyle = Style.disabledStyle;
                                }
                                else
                                {
                                    currentBoneStyle = Style.inactiveStyle;
                                }
                            }

                            //currentBoneStyle = (clipBone.visible ? Style.unSelectedInformationStyle : Style.disabledStyle);
                        }
                        else
                        {
                            currentBoneStyle = Style.unSelectedInformationStyle;
                        }
                    }

                    guiContent = new GUIContent(boneData.ShortenedBoneName, boneData.boneName);

                    GUILayout.Label(guiContent,
                                    currentBoneStyle,
                                    GUILayout.Width(BONE_LABEL_WIDTH),
                                    GUILayout.Height(BONE_LABEL_HEIGHT));

                    if (boneData.boneColor.blendingWeight == 0)
                    {
                        if (GUILayout.Button(GUIContent.none, Style.boneNoColorButtonStyle, GUILayout.Width(BONE_COLOR_WIDTH), GUILayout.Height(BONE_COLOR_HEIGHT)))
                        {
                            BoneColorWindow.Reset(boneData);
                        }
                    }
                    else
                    {
                        Rect fullColorRect = GUILayoutUtility.GetRect(BONE_COLOR_WIDTH, BONE_COLOR_HEIGHT);
                        Rect boneColorRect = new Rect(0, BONE_COLOR_HEIGHT * (1.0f - boneData.boneColor.blendingWeight), BONE_COLOR_WIDTH, BONE_COLOR_HEIGHT * boneData.boneColor.blendingWeight);

                        GUI.BeginGroup(fullColorRect, Style.boneNoColorButtonStyle);

                        Style.PushBackgroundColor(boneData.boneColor.color);
                        GUIHelper.DrawBox(boneColorRect, Style.boneColorButtonStyle, false);
                        Style.PopBackgroundColor();

                        GUI.EndGroup();

                        if (GUI.Button(fullColorRect, GUIContent.none, Style.blankStyle))
                        {
                            BoneColorWindow.Reset(boneData);
                        }
                    }
                }

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
                // bone horizontal

                GUILayout.Space(BONE_LABEL_Y_SPACER);
            }

            GUILayout.EndVertical();
            // end bone list vertical

            //GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            // end bone list and slider horizontal

            GUILayout.EndVertical();
            // End overall vertical

            if (_duplicateNameWarning)
            {
                GUI.Label(_labelRect, "Bone name already exists", Style.warningStyle);
            }

            GUILayout.EndArea();
            // End overall area

            DrawDraggableObjects();

            if (_editOnRepaint != -1)
            {
                EditBone(_editOnRepaint);
                editor.SetNeedsRepainted();
                _editOnRepaint = -1;
            }
        }

        static private void CreateDraggableObjects()
        {
            if (_draggableBone == null)
            {
                _draggableBone = new DraggableBone("Bone", new Vector2(-600.0f, 0));
            }
        }

        static private void DrawDraggableObjects()
        {
            _draggableBone.OnGUI();
        }

        static private void HideDraggingBone()
        {
            _dragBonePosition = Vector2.zero;
            _draggableBone.position = _dragBonePosition;
            _draggableBone.Hide(true);
        }

        static public void GetInput(Event evt)
        {
			if (TimelineWindow.IsPlaying)
				return;

            Vector2 areaMousePos;
            Vector2 localMousePos;

            if (evt.type == EventType.MouseDown && _areaRect.Contains(evt.mousePosition))
            {
                editor.KeyboardFocus = BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.Bones;
            }

            if (editor.KeyboardFocus == BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.Bones)
            {
                if (evt.type == EventType.KeyDown)
                {
                    switch (evt.keyCode)
                    {
                        case KeyCode.Escape:
                            if (_dragBoneIndex != -1)
                            {
                                HideDraggingBone();

                                _dragBoneIndex = -1;
                                _dropTargetBoneIndex = -1;

                                _draggingBone = false;

                                editor.SetNeedsRepainted();

                                evt.Use();
                            }
                            break;

                        case KeyCode.Delete:
                        case KeyCode.Backspace:
                            if (_clickedBoneIndex != -1 && _editBoneIndex == -1)
                            {
                                ConfirmDeleteBone(_clickedBoneIndex);
                                evt.Use();
                            }
                            break;
                    }
                }

                if (evt.type == EventType.KeyUp)
                {
                    switch (evt.keyCode)
                    {
                        case KeyCode.R:
                            if (_clickedBoneIndex != -1 && _editBoneIndex == -1)
                            {
                                EditBone(_clickedBoneIndex);
                                evt.Use();
                            }
                            break;

                        case KeyCode.A:
                            if (evt.control)
                            {
                                if (evt.shift)
                                {
                                    if (_clickedBoneIndex != -1 && _clickedBoneIndex < editor.boneAnimationData.dfsBoneNodeList.Count)
                                    {
                                        AddBone(_clickedBoneIndex);
                                        evt.Use();
                                    }
                                }
                                else
                                {
                                    AddBone(0);
                                    evt.Use();
                                }
                            }
                            break;
                    }
                }
            }	

            if (_areaRect.Contains(evt.mousePosition))
            {
                areaMousePos = evt.mousePosition - GUIHelper.UpperLeftCorner(_areaRect);
				
                if (_rootBoneRect.Contains(areaMousePos))
                {
                    switch (evt.type)
                    {
                        case EventType.MouseDown:
                            if (EditorHelper.LeftMouseButton(evt))
                            {
                                StopEditingBone();
                                _dragBoneIndex = -1;
                                _dropTargetBoneIndex = -1;

                                SelectBone(0);

                                editor.SetNeedsRepainted();

                                evt.Use();
                            }
                            break;

                        case EventType.MouseDrag:
                            if (_dragBoneIndex != -1)
                            {
                                _dragBonePosition = evt.mousePosition;

                                if (!_draggingBone)
                                {
                                    _draggingBone = true;
                                    _draggableBone.boneName = editor.boneAnimationData.GetBoneData(_dragBoneIndex).ShortenedBoneName;
                                    _draggableBone.Hide(false);
                                }
                                else if (_draggingBone)
                                {
                                    _draggableBone.position = _dragBonePosition;
                                    editor.SetNeedsRepainted();
                                }

                                _dropTargetBoneIndex = 0;

                                evt.Use();
                            }
                            break;

                        case EventType.mouseUp:
                            if (_dropTargetBoneIndex != -1)
                            {
                                if (_dropTargetBoneIndex != _dragBoneIndex)
                                {
                                    _boneNeedsMoving = true;
                                    _moveBoneIndex = _dragBoneIndex;
                                    _moveBoneToIndex = _dropTargetBoneIndex;

                                    _dropTargetBoneIndex = -1;

                                    TimelineWindow.ResetSelectedFrames();
                                }

                                HideDraggingBone();

                                editor.SetNeedsRepainted();
                            }
                            else
                            {
                                HideDraggingBone();

                                editor.SetNeedsRepainted();
                            }

                            _dragBoneIndex = -1;
                            _dropTargetBoneIndex = -1;
                            _draggingBone = false;

                            evt.Use();

                            break;
                    }
                }
                else if (_boneListRect.Contains(areaMousePos))
                {
                    localMousePos = areaMousePos - GUIHelper.UpperLeftCorner(_boneListRect);

                    switch (evt.type)
                    {
                        case EventType.MouseDown:
                            ClipBrowserWindow.StopEditingClip();

                            GetBoneIndexFromMousePosition(localMousePos, ref _clickedBoneIndex);
						
                            _dragBoneIndex = -1;
                            _dropTargetBoneIndex = -1;

                            if (_clickedBoneIndex != -1)
                            {
                                if (EditorHelper.LeftMouseButton(evt))
                                {
                                    SelectBone(_clickedBoneIndex);
                                    editor.SetNeedsRepainted();

                                    if (editor.CheckForDoubleClick(_lastClickedBoneIndex == _clickedBoneIndex))
                                    {
                                        _editOnRepaint = _clickedBoneIndex;
                                    }
                                    else
                                    {
                                        if (_editBoneIndex == _clickedBoneIndex)
                                        {
                                            // do nothing, let the mouse select the text
                                        }
                                        else
                                        {
                                            _dragBoneIndex = _clickedBoneIndex;
                                            _dragBoneStart = localMousePos;

											StopEditingBone();

                                            evt.Use();
                                        }
                                    }
                                }
                                else if (EditorHelper.RightMouseButton(evt))
                                {
                                    SelectBone(_clickedBoneIndex);
                                    GenericMenu boneContextMenu = new GenericMenu();
                                    boneContextMenu.AddItem(new GUIContent("Add Bone"), false, BoneContextMenuCallback_AddBone, _clickedBoneIndex);
                                    boneContextMenu.AddItem(new GUIContent("Delete Bone"), false, BoneContextMenuCallback_RemoveBone, _clickedBoneIndex);
                                    boneContextMenu.AddItem(new GUIContent("Rename Bone"), false, BoneContextMenuCallback_RenameBone, _clickedBoneIndex);
                                    boneContextMenu.AddSeparator("");
									boneContextMenu.AddItem(new GUIContent("Select all keyframes in bone"), false, BoneContextMenuCallback_SelectKeyframes, _clickedBoneIndex);
                                    boneContextMenu.AddSeparator("");
                                    boneContextMenu.AddItem(new GUIContent("Make Bone the Base"), false, BoneContextMenuCallback_RootBone, _clickedBoneIndex);
                                    boneContextMenu.ShowAsContext();
                                    evt.Use();
                                    break;
                                }
                            }
                            else
                            {
                                editor.SetNeedsRepainted();

                                evt.Use();
                            }

                            _lastClickedBoneIndex = _clickedBoneIndex;
                            break;

                        case EventType.MouseDrag:
                            if (_dragBoneIndex != -1)
                            {
                                _dragBonePosition = evt.mousePosition;

                                if ((_dragBoneStart - localMousePos).magnitude > BoneAnimationDataEditorWindow.DRAG_THRESHOLD && !_draggingBone)
                                {
                                    _draggingBone = true;
                                    _draggableBone.boneName = editor.boneAnimationData.GetBoneData(_dragBoneIndex).ShortenedBoneName;
                                    _draggableBone.Hide(false);
                                }
                                else if (_draggingBone)
                                {
                                    _draggableBone.position = _dragBonePosition;
                                    editor.SetNeedsRepainted();
                                }

                                _dropTargetBoneIndex = -1;
                                GetBoneIndexFromMousePosition(localMousePos, ref _dropTargetBoneIndex);

                                if (_dropTargetBoneIndex == _firstVisibleBone && _firstVisibleBone > 1)
                                {
                                    if (EditorApplication.timeSinceStartup - _lastScrollTime > BoneAnimationDataEditorWindow.MINIMUM_SCROLL_INTERVAL)
                                        _firstVisibleBone -= 1;

                                    _lastScrollTime = EditorApplication.timeSinceStartup;

                                    editor.SetNeedsRepainted();
                                }
                                else if (editor.boneAnimationData.dfsBoneNodeList != null)
                                {
                                    if (_dropTargetBoneIndex == (_firstVisibleBone + _visibleBoneCount - 1) && (_firstVisibleBone + _visibleBoneCount - 1) < editor.boneAnimationData.BoneCount)
                                    {
                                        if (EditorApplication.timeSinceStartup - _lastScrollTime > BoneAnimationDataEditorWindow.MINIMUM_SCROLL_INTERVAL)
                                            _firstVisibleBone += 1;

                                        _lastScrollTime = EditorApplication.timeSinceStartup;

                                        editor.SetNeedsRepainted();
                                    }
                                }

                                if (_dropTargetBoneIndex == _dragBoneIndex)
                                {
                                    _dropTargetBoneIndex = -1;
                                }
                                else if (editor.boneAnimationData.GetIsBoneDescendant(_dragBoneIndex, _dropTargetBoneIndex))
                                {
                                    _dropTargetBoneIndex = -1;
                                }

                                evt.Use();
                            }
                            break;

                        case EventType.mouseUp:
                            if (_dropTargetBoneIndex != -1)
                            {
                                if (_dropTargetBoneIndex != _dragBoneIndex)
                                {
                                    _boneNeedsMoving = true;
                                    _moveBoneIndex = _dragBoneIndex;
                                    _moveBoneToIndex = _dropTargetBoneIndex;

                                    _dropTargetBoneIndex = -1;

                                    TimelineWindow.ResetSelectedFrames();
                                }

                                HideDraggingBone();

                                editor.SetNeedsRepainted();
                            }
                            else
                            {
                                HideDraggingBone();

                                editor.SetNeedsRepainted();
                            }

                            _dragBoneIndex = -1;
                            _dropTargetBoneIndex = -1;
                            _draggingBone = false;

                            evt.Use();

                            break;

                        case EventType.ScrollWheel:

                            int newFirstVisibleBone = Mathf.Clamp(_firstVisibleBone + Convert.ToInt16(Mathf.Sign(evt.delta.y)), 1, editor.boneAnimationData.boneDataList.Count - 1);
                            if (newFirstVisibleBone != _firstVisibleBone)
                            {
                                _firstVisibleBone = newFirstVisibleBone;
                                TimelineWindow.SetFrameGridNeedsRebuilt();
                                _boneListNeedsScrolled = true;
                                _scrollBoneListTo = _firstVisibleBone;
                                evt.Use();
                            }
                            break;
                    }
                }
            }
        }

        static private void GetBoneIndexFromMousePosition(Vector2 mousePos, ref int boneIndex)
        {
            boneIndex = Mathf.FloorToInt((mousePos.y + _boneListScrollPosition.y) / (BONE_LABEL_HEIGHT + BONE_LABEL_Y_SPACER)) + _firstVisibleBone;
            if (boneIndex > (editor.boneAnimationData.BoneCount - 1))
            {
                boneIndex = -1;
            }
        }

        static public void BoneContextMenuCallback_AddBone(System.Object obj)
        {
            AddBone(Convert.ToInt32(obj));
        }

        static public void BoneContextMenuCallback_RemoveBone(System.Object obj)
        {
			ConfirmDeleteBone(Convert.ToInt16(obj));
        }

        static public void BoneContextMenuCallback_RenameBone(System.Object obj)
        {
            _clickedBoneIndex = Convert.ToInt16(obj);
            _editOnRepaint = _clickedBoneIndex;
        }
		
		static private void ConfirmDeleteBone(int boneNodeIndex)
		{
			string boneName = editor.boneAnimationData.boneDataList[editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex)].boneName;
			
			if (EditorUtility.DisplayDialog("Confirm Bone Deletion", "Are you sure you want to delete bone '" + boneName + "'. This will remove the bone from all animation clips!", "Yes", "No"))
			{
                editor.Focus();

	            RemoveBone(boneNodeIndex);
			}
		}
		
		static public void BoneContextMenuCallback_SelectKeyframes(System.Object obj)
		{
            TimelineWindow.SelectKeyframesForBone(Convert.ToInt16(obj));
		}

        static public void BoneContextMenuCallback_RootBone(System.Object obj)
        {
            editor.SetWillBeDirty();
            
            editor.boneAnimationData.MakeBonesChildrenOfBone(Convert.ToInt16(obj));

            TimelineWindow.ResetSelectedFrames();

            editor.SetNeedsRepainted();
        }

        static public void SelectBone(int boneNodeIndex)
        {
            TimelineWindow.ResetWorkingSelectedFrames();
            TimelineWindow.ResetSelectedFrames();

            TimelineWindow.SelectFrame(boneNodeIndex, TimelineWindow.CurrentFrame);

            _clickedBoneIndex = boneNodeIndex;

            AnimationHelper.ChangeAnimationCurveEditorWindowBone(editor.boneAnimationData.GetBoneDataIndex(boneNodeIndex));
        }

        static public bool IsBoneSelected(int boneNodeIndex)
        {
            if (TimelineWindow.SelectedFrames != null)
            {
                foreach (BoneFrame boneFrame in TimelineWindow.SelectedFrames)
                {
                    if (boneFrame.boneNodeIndex == boneNodeIndex)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static public void MoveBones()
        {
            if (_boneNeedsMoving)
            {
                editor.SetWillBeDirty();
                
                editor.boneAnimationData.MoveBoneToParent(_moveBoneIndex, _moveBoneToIndex);

                _boneNeedsMoving = false;
                editor.SetNeedsRepainted();
            }
        }

        static public void ScrollBones()
        {
            if (_boneListNeedsScrolled)
            {
                _firstVisibleBone = _scrollBoneListTo;

                _boneListNeedsScrolled = false;
                editor.SetNeedsRepainted();
            }
        }

        static public void AddBone(int parentBoneNodeIndex)
        {
            _addBoneParentBoneNodeIndex = parentBoneNodeIndex;
            editor.KeyboardFocus = BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.Bones;
        }

        static public void AddBonePostRepaint()
        {
            if (_addBoneParentBoneNodeIndex != -1)
            {
                editor.SetWillBeDirty(); ;
                
                int newBoneIndex;
                newBoneIndex = editor.boneAnimationData.AddBone(_addBoneParentBoneNodeIndex, GetUniqueBoneName("Bone"));

                if (newBoneIndex < _firstVisibleBone)
                {
                    _firstVisibleBone = newBoneIndex;
                }
                else if (newBoneIndex > (_firstVisibleBone + _visibleBoneCount - 1))
                {
                    _firstVisibleBone = (newBoneIndex - _visibleBoneCount + 1);
                }

                SelectBone(newBoneIndex);

                editor.SetNeedsRepainted();

                _addBoneParentBoneNodeIndex = -1;
            }
        }

        static public void RemoveBone(int boneNodeIndex)
        {
            editor.SetWillBeDirty();
            
            editor.boneAnimationData.RemoveBone(boneNodeIndex);

            int newBoneIndex = boneNodeIndex - 1;

            if (newBoneIndex == 0 || newBoneIndex == -1)
            {
                TimelineWindow.ResetSelectedFrames();
            }
            else
            {
                if (newBoneIndex < _firstVisibleBone)
                {
                    _firstVisibleBone = newBoneIndex;
                }
                else if (newBoneIndex > (_firstVisibleBone + _visibleBoneCount - 1))
                {
                    _firstVisibleBone = (newBoneIndex - _visibleBoneCount + 1);
                }

                SelectBone(newBoneIndex);
            }

            editor.SetNeedsRepainted();
        }

        static public void EditBone(int index)
        {
            _editBoneIndex = index;
            _editBoneName = editor.boneAnimationData.GetBoneData(_editBoneIndex).boneName;
            _originalEditBoneName = _editBoneName.Trim();
            _focusTextbox = true;
        }

        static public bool StopEditingBone()
        {
            editor.SetWillBeDirty();

            if (_editBoneIndex > -1 && _editBoneIndex < editor.boneAnimationData.dfsBoneNodeList.Count)
            {
                int boneDataIndex = editor.boneAnimationData.GetBoneDataIndex(_editBoneIndex);

                _editBoneName = _editBoneName.Trim();

                if (_editBoneName == "")
                {
                    editor.boneAnimationData.boneDataList[boneDataIndex].boneName = GetUniqueBoneName("Bone");
                }
                else
                {
                    if (editor.boneAnimationData.BoneNameExists(_editBoneName) && _editBoneName.ToLower() != _originalEditBoneName.ToLower())
                    {
                        _duplicateNameWarning = true;
                        editor.Focus();
                        _focusTextbox = true;
                        return false;
                    }
                    else
                    {
                        editor.boneAnimationData.boneDataList[boneDataIndex].boneName = _editBoneName;
                    }
                }
            }

            _unfocusTextbox = true;

            _editBoneIndex = -1;
			editor.SetNeedsRepainted();

            _duplicateNameWarning = false;

            return true;
        }

        static private string GetUniqueBoneName(string name)
        {
            bool success = true;

            foreach (BoneData boneData in editor.boneAnimationData.boneDataList)
            {
                if (boneData.boneName.ToLower() == name.ToLower())
                {
                    success = false;
                    break;
                }
            }

            if (!success)
            {
                name = GetUniqueBoneName(EditorHelper.GenerateIncrementedIndexedName(name));
            }

            return name;
        }

        static public void LostFocus()
        {
            if (_editBoneIndex != -1)
            {
                SelectBone(_editBoneIndex);
                StopEditingBone();
            }
        }
    }
}
