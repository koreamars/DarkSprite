using UnityEngine;
using UnityEditor;
using System;

namespace SmoothMoves
{
    static public class ClipBrowserWindow
    {
        public const float MIN_WIDTH = 130.0f;

        private const float ANIMATION_CLIP_LABEL_HEIGHT = 20.0f;

        static private Rect _areaRect;
        static private Rect _clipListRect;
        static private Rect _labelRect;

        static private int _firstVisibleAnimationClip;
        static private int _visibleAnimationClipCount;

        static private int _lastClickedClipIndex;
        static private int _editClipIndex;
        static private bool _focusTextbox;
        static private bool _unfocusTextbox;

        static private string _editClipName;
        static private string _originalEditClipName;
        static private bool _duplicateNameWarning;

        static public Rect AreaRect { get { return _areaRect; } set { _areaRect = value; } }
        static public int SelectedAnimationClipIndex 
		{ 
			get 
			{
                if (editor.boneAnimationData == null)
                    return -1;
                else if (editor.boneAnimationData.animationClips == null)
                    return -1;

                int index = PlayerPrefs.GetInt("SmoothMoves_Editor_ClipBrowser_SelectedClipIndex", -1);
                int clipCount = editor.boneAnimationData.animationClips.Count;
                if (index >= clipCount)
                {
                    if (clipCount > 0)
                        index = 0;
                    else
                        index = -1;
                }
                else if (index == -1)
                {
                    if (clipCount > 0)
                    {
                        index = 0;
                    }
                }
                else if (index < -1)
                {
                    index = -1;
                }

                return index;
			} 
			set
			{
                PlayerPrefs.SetInt("SmoothMoves_Editor_ClipBrowser_SelectedClipIndex", value);
                TimelineWindow.SetFrameGridNeedsRebuilt();
			}
		}
		
        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }

        static public AnimationClipSM CurrentClip
        {
            get
            {
                if (editor == null)
                    return null;
                if (editor.boneAnimationData == null)
                    return null;
                else if (editor.boneAnimationData.animationClips == null)
                    return null;
                else if (SelectedAnimationClipIndex == -1 || SelectedAnimationClipIndex >= editor.boneAnimationData.animationClips.Count)
                    return null;
                else
                    return editor.boneAnimationData.animationClips[SelectedAnimationClipIndex];
            }
        }

        static public float ClipBrowserWidth
        {
            get
            {
                return PlayerPrefs.GetFloat("SmoothMoves_Editor_ClipBrowserWidth", 150.0f);
            }
            set
            {
                PlayerPrefs.SetFloat("SmoothMoves_Editor_ClipBrowserWidth", Mathf.Min(Mathf.Max(value, MIN_WIDTH), editor.Width - BoneWindow.AreaRect.width - TimelineWindow.AreaRect.width));
            }
        }

        static public void OnEnable()
        {
            _firstVisibleAnimationClip = 0;
            _editClipName = "";
            _editClipIndex = -1;
        }

        static public void SetRects()
        {
            _clipListRect = new Rect(
                                     BoneAnimationDataEditorWindow.PADDING,
                                     BoneAnimationDataEditorWindow.PADDING + 58.0f,
                                     _areaRect.width - (BoneAnimationDataEditorWindow.PADDING * 2.0f) - BoneAnimationDataEditorWindow.VERTICAL_SCROLL_SLIDER_WIDTH,
                                     _areaRect.height - (78.0f + (BoneAnimationDataEditorWindow.PADDING * 2.0f))
                                     );

            _labelRect = new Rect(0, 0, _areaRect.width, 20.0f);

            _visibleAnimationClipCount = Mathf.FloorToInt(_clipListRect.height / ANIMATION_CLIP_LABEL_HEIGHT);
        }

        static private void PreGUIGetInput(Event evt)
        {
            if (editor.KeyboardFocus == BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.Clips)
            {
                if (evt.type == EventType.KeyDown)
                {
                    switch (evt.keyCode)
                    {
                        case KeyCode.KeypadEnter:
                        case KeyCode.Return:
                            if (_editClipIndex != -1)
                            {
                                evt.Use();
                                SelectClip(_editClipIndex);
                                StopEditingClip();
                            }
                            break;

                        case KeyCode.Escape:
                            if (_editClipIndex != -1)
                            {
                                SelectClip(_editClipIndex);
                                _editClipIndex = -1;
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
            float labelWidth;

            PreGUIGetInput(Event.current);

            GUIStyle currentAnimationClipStyle;

            GUILayout.BeginArea(_areaRect, GUIContent.none, Style.windowRectBackgroundStyle);

            GUIHelper.DrawBox(_labelRect, Style.maskStyle, true);

            GUIHelper.DrawBox(_clipListRect, Style.windowRectBackgroundStyle, true);

            GUILayout.BeginVertical();

            GUI.SetNextControlName("AnimationClipsLabel");
            GUILayout.Label("Animation Clips", Style.centeredTextStyle, GUILayout.Width(MIN_WIDTH - 20.0f));
            if (_unfocusTextbox)
            {
                GUI.FocusControl("AnimationClipsLabel");
                _unfocusTextbox = false;
            }

            GUILayout.Space(18.0f);

            GUILayout.BeginHorizontal();

            bool atLeastOneButtonVisible = false;

            if (!TimelineWindow.IsPlaying && !editor.ModalPopup)
            {
                if (GUILayout.Button(new GUIContent(Resources.buttonAddLarge, "Add Animation Clip"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonAddLarge.width), GUILayout.Height(Resources.buttonAddLarge.height)))
                {
                    AddAnimationClip();
                }

                atLeastOneButtonVisible = true;
            }

            if (SelectedAnimationClipIndex != -1 && !TimelineWindow.IsPlaying && !editor.ModalPopup)
            {

                GUILayout.Space(2.0f);

                if (GUILayout.Button(new GUIContent(Resources.buttonUp, "Move Animation Clip Up"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonUp.width), GUILayout.Height(Resources.buttonUp.height)))
                {
                    MoveSelectedClip(-1);
                }

                GUILayout.Space(2.0f);

                if (GUILayout.Button(new GUIContent(Resources.buttonDown, "Move Animation Clip Down"), Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonDown.width), GUILayout.Height(Resources.buttonDown.height)))
                {
                    MoveSelectedClip(1);
                }

                atLeastOneButtonVisible = true;
            }

            GUILayout.EndHorizontal();

            if (!atLeastOneButtonVisible)
            {
                GUILayout.Space(25.0f);
            }

            GUILayout.Space(2.0f);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(Style.windowRectDarkBackgroundStyle, GUILayout.Height(_clipListRect.height));

            for (int clipIndex = _firstVisibleAnimationClip; clipIndex < Mathf.Min(_firstVisibleAnimationClip + _visibleAnimationClipCount, editor.boneAnimationData.AnimationClipCount); clipIndex++)
            {
                GUILayout.BeginHorizontal();

                if (editor.boneAnimationData.animationClips[clipIndex].animationNeedsTwoKeyframes)
                {
                    GUIContent guiContent = new GUIContent(Resources.warning, "One or more of the bones in this animation clip has at least one property that needs two or more keyframes");
                    GUILayout.Label(guiContent, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
                    labelWidth = _clipListRect.width - Resources.warning.width - BoneAnimationDataEditorWindow.PADDING;
                }
                else
                {
                    GUILayout.Label(GUIContent.none, GUILayout.Width(0));
                    labelWidth = _clipListRect.width - BoneAnimationDataEditorWindow.PADDING; // +8.0f;
                }

                if (_editClipIndex == clipIndex)
                {
                    GUI.SetNextControlName("ClipTextbox");
                    _editClipName = EditorGUILayout.TextField(_editClipName, GUILayout.Width(_clipListRect.width - (BoneAnimationDataEditorWindow.PADDING) - 5.0f), GUILayout.Height(ANIMATION_CLIP_LABEL_HEIGHT));

                    if (_focusTextbox)
                    {
                        GUI.FocusControl("ClipTextbox");
                        _focusTextbox = false;
                    }
                }
                else
                {
                    if (clipIndex == SelectedAnimationClipIndex)
                        currentAnimationClipStyle = Style.selectedInformationStyle;
                    else
                        currentAnimationClipStyle = Style.unSelectedInformationStyle;

                    string shortAnimationName = editor.boneAnimationData.animationClips[clipIndex].animationName;
                    if (shortAnimationName.Length > 20)
                    {
                        shortAnimationName = shortAnimationName.Substring(0, 17) + "...";
                    }

                    GUIContent labelContent = new GUIContent(shortAnimationName, (clipIndex == 0 ? "Default Animation : " + editor.boneAnimationData.animationClips[clipIndex].animationName : editor.boneAnimationData.animationClips[clipIndex].animationName));

                    GUILayout.Label(labelContent,
                                    currentAnimationClipStyle,
                                    GUILayout.Width(labelWidth),
                                    GUILayout.Height(ANIMATION_CLIP_LABEL_HEIGHT));
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            if (SelectedAnimationClipIndex != -1 && !TimelineWindow.IsPlaying && !editor.ModalPopup)
            {
                int newFirstVisibleClip = Convert.ToInt32(GUILayout.VerticalSlider((float)_firstVisibleAnimationClip, 0, editor.boneAnimationData.animationClips.Count - 1, GUILayout.Height(_clipListRect.height - BoneAnimationDataEditorWindow.SCROLLBAR_HEIGHT))); //, GUILayout.Width(VERTICAL_SCROLL_SLIDER_WIDTH)));
                if (newFirstVisibleClip != _firstVisibleAnimationClip)
                {
                    _firstVisibleAnimationClip = newFirstVisibleClip;
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if (_duplicateNameWarning)
            {
                GUI.Label(_labelRect, "Clip name already exists", Style.warningStyle);
            }

            GUILayout.EndArea();
        }

        static public void GetInput(Event evt)
        {
			if (TimelineWindow.IsPlaying || editor.boneAnimationData.animationClips.Count == 0)
				return;

            Vector2 areaMousePos;
            Vector2 localMousePos;

            if (evt.type == EventType.MouseDown && _areaRect.Contains(evt.mousePosition))
            {
                editor.KeyboardFocus = BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.Clips;
            }

            if (editor.KeyboardFocus == BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.Clips)
            {
                if (evt.type == EventType.KeyDown)
                {
                    switch (evt.keyCode)
                    {
                        case KeyCode.Delete:
                        case KeyCode.Backspace:
                            if (SelectedAnimationClipIndex != -1 && _editClipIndex == -1)
                            {
                                RemoveAnimationClip(SelectedAnimationClipIndex);
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
                            if (SelectedAnimationClipIndex != -1)
                            {
                                if (_editClipIndex == -1)
                                {
                                    EditClip(SelectedAnimationClipIndex);
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
				
                if (_clipListRect.Contains(areaMousePos))
                {
                    int clickedClipIndex = -1;

                    localMousePos = areaMousePos - GUIHelper.UpperLeftCorner(_clipListRect);

                    switch (evt.type)
                    {
                        case EventType.mouseDown:
                            BoneWindow.StopEditingBone();

                            GetAnimationClipIndexFromMousePosition(localMousePos, ref clickedClipIndex);

                            if (clickedClipIndex != -1)
                            {
                                if (EditorHelper.RightMouseButton(evt))
                                {
                                    GenericMenu clipBrowserContextMenu = new GenericMenu();
                                    clipBrowserContextMenu.AddItem(new GUIContent("Delete Clip"), false, ClipBrowserContextMenuCallback_RemoveClip, clickedClipIndex);
                                    clipBrowserContextMenu.AddItem(new GUIContent("Rename Clip"), false, ClipBrowserContextMenuCallback_RenameClip, clickedClipIndex);
                                    clipBrowserContextMenu.AddSeparator("");
                                    clipBrowserContextMenu.AddItem(new GUIContent("Duplicate Entire Clip"), false, ClipBrowserContextMenuCallback_DuplicateEntireClip, clickedClipIndex);
                                    clipBrowserContextMenu.AddItem(new GUIContent("Duplicate First Frames"), false, ClipBrowserContextMenuCallback_DuplicateFirstFrames, clickedClipIndex);
                                    clipBrowserContextMenu.AddItem(new GUIContent("Duplicate Last Frames"), false, ClipBrowserContextMenuCallback_DuplicateLastFrames, clickedClipIndex);
                                    clipBrowserContextMenu.ShowAsContext();
                                    evt.Use();
                                }
                                else if (EditorHelper.LeftMouseButton(evt))
                                {
                                    SelectClip(clickedClipIndex);
                                    editor.SetNeedsRepainted();

                                    if (editor.CheckForDoubleClick(_lastClickedClipIndex == clickedClipIndex))
                                    {
                                        EditClip(clickedClipIndex);
                                    }
                                    else
                                    {
                                        if (_editClipIndex == clickedClipIndex)
                                        {
                                            // do nothing, let the mouse select the text
                                        }
                                        else
                                        {
                                            StopEditingClip();

                                            evt.Use();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                editor.SetNeedsRepainted();

                                evt.Use();
                            }

                            _lastClickedClipIndex = clickedClipIndex;
                            break;

                        case EventType.ScrollWheel:

                            int newFirstVisibleClip = Mathf.Clamp(_firstVisibleAnimationClip + Convert.ToInt16(Mathf.Sign(evt.delta.y)), 0, editor.boneAnimationData.animationClips.Count - 1);
                            if (newFirstVisibleClip != _firstVisibleAnimationClip)
                            {
                                _firstVisibleAnimationClip = newFirstVisibleClip;
                                evt.Use();
                            }
                            break;
                    }
                }
            }
        }

        static private void GetAnimationClipIndexFromMousePosition(Vector2 mousePos, ref int clipIndex)
        {
            clipIndex = Mathf.FloorToInt(mousePos.y / (ANIMATION_CLIP_LABEL_HEIGHT)) + _firstVisibleAnimationClip;
            if (clipIndex > (editor.boneAnimationData.AnimationClipCount - 1))
            {
                clipIndex = -1;
            }
        }

        static public void ClipBrowserContextMenuCallback_RemoveClip(System.Object obj)
        {
            RemoveAnimationClip(Convert.ToInt16(obj));
        }

        static public void ClipBrowserContextMenuCallback_RenameClip(System.Object obj)
        {
            EditClip(Convert.ToInt16(obj));
        }
		
		static public void ClipBrowserContextMenuCallback_DuplicateEntireClip(System.Object obj)
		{
			DuplicateClip(Convert.ToInt16(obj), AnimationClipSM.DUPLICATE_MODE.EntireClip);
		}
		
		static public void ClipBrowserContextMenuCallback_DuplicateFirstFrames(System.Object obj)
		{
			DuplicateClip(Convert.ToInt16(obj), AnimationClipSM.DUPLICATE_MODE.FirstFrames);
		}
		
		static public void ClipBrowserContextMenuCallback_DuplicateLastFrames(System.Object obj)
		{
			DuplicateClip(Convert.ToInt16(obj), AnimationClipSM.DUPLICATE_MODE.LastFrames);
		}

        static private void SelectClip(int clipIndex)
        {
            SelectedAnimationClipIndex = clipIndex;

            TimelineWindow.ResetSelectedFrames();
			TimelineWindow.SetCurrentFrame(0);
            TimelineWindow.ResetPlayDirection();
			
			TextureManager.GenerateTextureDictionary(false);
            AnimationHelper.GenerateAnimationCurves(ClipBrowserWindow.CurrentClip);

            AnimationHelper.ResetAnimationCurveEditorWindow();
        }

        static public void AddAnimationClip()
        {
            editor.SetWillBeDirty();

            editor.boneAnimationData.AddAnimationClip(GetUniqueClipName("New Clip"));

            SelectedAnimationClipIndex = editor.boneAnimationData.animationClips.Count - 1;
			StopEditingClip();

            editor.KeyboardFocus = BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.Clips;
			
			editor.SetNeedsRepainted();
        }
		
		static public void DuplicateClip(int clipIndex, AnimationClipSM.DUPLICATE_MODE duplicateMode)
		{
            editor.SetWillBeDirty();
            
            editor.boneAnimationData.DuplicateAnimationClip(clipIndex, duplicateMode, GetUniqueClipName("New Clip"));
			StopEditingClip();
			
			SelectedAnimationClipIndex = editor.boneAnimationData.animationClips.Count-1;
			
			editor.SetNeedsRepainted();
		}

        static public void RemoveAnimationClip(int clipIndex)
        {
			if (EditorUtility.DisplayDialog("Confirm Clip Deletion", "Are you sure you want to delete the animation '" + editor.boneAnimationData.animationClips[clipIndex].animationName + "'?", "Yes", "No"))
			{
                editor.SetWillBeDirty();

                editor.Focus();

	            editor.boneAnimationData.RemoveAnimationClip(clipIndex);
	
	            if (editor.boneAnimationData.animationClips.Count > 0)
                    SelectedAnimationClipIndex = editor.boneAnimationData.animationClips.Count - 1;
	            else
	                SelectedAnimationClipIndex = -1;
				
				editor.SetNeedsRepainted();
			}
			
			StopEditingClip();
        }

        static public void EditClip(int index)
        {
            _editClipIndex = index;
            _focusTextbox = true;
            _editClipName = editor.boneAnimationData.animationClips[_editClipIndex].animationName;
            _originalEditClipName = _editClipName.Trim();
            editor.SetNeedsRepainted();
        }

        static public bool StopEditingClip()
        {
            editor.SetWillBeDirty();

            if (_editClipIndex > -1 && _editClipIndex < editor.boneAnimationData.animationClips.Count)
            {
                _editClipName = _editClipName.Trim();

                if (_editClipName == "")
                {
                    editor.boneAnimationData.animationClips[_editClipIndex].animationName = GetUniqueClipName("New Clip");
                }
                else
                {
                    if (editor.boneAnimationData.AnimationClipNameExists(_editClipName) && _editClipName.ToLower() != _originalEditClipName.ToLower())
                    {
                        _duplicateNameWarning = true;
                        editor.Focus();
                        _focusTextbox = true;
                        return false;
                    }
                    else
                    {
                        editor.boneAnimationData.animationClips[_editClipIndex].animationName = _editClipName;
                    }
                }
            }

            _unfocusTextbox = true;

            _editClipIndex = -1;

			editor.SetNeedsRepainted();

            _duplicateNameWarning = false;

            return true;
        }

        static private string GetUniqueClipName(string name)
        {
            bool success = true;

            foreach (AnimationClipSM clip in editor.boneAnimationData.animationClips)
            {
                if (clip.animationName.ToLower() == name.ToLower())
                {
                    success = false;
                    break;
                }
            }

            if (!success)
            {
                name = GetUniqueClipName(EditorHelper.GenerateIncrementedIndexedName(name));
            }

            return name;
        }

        static private void MoveSelectedClip(int direction)
        {
            if (SelectedAnimationClipIndex != -1)
            {
                int selectedIndex = SelectedAnimationClipIndex;

                if (direction == -1)
                {
                    if (selectedIndex > 0)
                    {
                        editor.SetWillBeDirtyNoSnapshot();

                        SwapAnimationClips(selectedIndex, selectedIndex - 1);
                        SelectedAnimationClipIndex = selectedIndex - 1;
                    }
                }
                else if (direction == 1)
                {
                    if (selectedIndex < (editor.boneAnimationData.animationClips.Count - 1))
                    {
                        editor.SetWillBeDirtyNoSnapshot();

                        SwapAnimationClips(selectedIndex, selectedIndex + 1);
                        SelectedAnimationClipIndex = selectedIndex + 1;
                    }
                }
            }
        }

        static private void SwapAnimationClips(int indexA, int indexB)
        {
            AnimationClipSM clipTemp = editor.boneAnimationData.animationClips[indexA];
            editor.boneAnimationData.animationClips[indexA] = editor.boneAnimationData.animationClips[indexB];
            editor.boneAnimationData.animationClips[indexB] = clipTemp;
        }

        static public void LostFocus()
        {
            if (_editClipIndex != -1)
            {
                SelectClip(_editClipIndex);
                StopEditingClip();
            }
        }
    }
}
