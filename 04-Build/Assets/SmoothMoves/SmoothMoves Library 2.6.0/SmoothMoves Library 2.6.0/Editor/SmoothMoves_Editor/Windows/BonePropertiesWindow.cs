using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SmoothMoves
{
    static public class BonePropertiesWindow
    {
        private const float BONE_EDITOR_TEXTURE_FORCE_SIZE = 60.0f;
        private const float MIN_WIDTH = 240.0f;
        private const float LABEL_WIDTH = 55.0f;
        private const float CURVE_BUTTON_WIDTH = 30.0f;
        private const float TOGGLE_WIDTH = 13.0f;
        private const float CURVE_FIELD_WIDTH = 30.0f;
        private const float SLIDER_WIDTH = 110.0f;

        private enum CURVE_CHANGE
        {
            None,
            AddedKeyframe,
            DeletedKeyframe,
            ChangedProperty
        }

        static private string _lastBoneEditorTextureGUID;
        static private Rect _areaRect;
        static private Rect _bonePropertiesBoneNameRect;
        static private Texture2D _texture;
        static private Vector2 _scrollPosition;

        static private AnimationCurveEditorWindow _animationCurveEditor;
        static private BoneCurves _currentBoneCurves;
        static private AnimationClipBone _currentBone;
        static private KeyframeSM.CURVE_PROPERTY _currentCurveProperty;

        static private bool _atlasListBuilt = false;
        static private int _selectedAtlasIndex = -1;
        static private List<TextureAtlas> _atlasList;
        static private List<string> _atlasNameList;
        static private List<string> _atlasGUIDList;
		
        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }
        static public Rect AreaRect { get { return _areaRect; } set { _areaRect = value; } }
        static public string LastBoneEditorTextureGUID { get { return _lastBoneEditorTextureGUID; } set { _lastBoneEditorTextureGUID = value; } }
		static public float BonePropertiesWindowX 
        { 
            get 
            {
                return PlayerPrefs.GetFloat("SmoothMoves_Editor_BonePropertiesWindowX", MIN_WIDTH); 
            } 
            set 
            { 
                PlayerPrefs.SetFloat("SmoothMoves_Editor_BonePropertiesWindowX", Mathf.Max(value, MIN_WIDTH));
            } 
        }

        static public void OnEnable()
        {
            _texture = null;
            _currentCurveProperty = KeyframeSM.CURVE_PROPERTY.None;
            _currentBone = null;
            _currentBoneCurves = null;
            _atlasListBuilt = false;
        }
		
        static public void SetRects()
        {
            _bonePropertiesBoneNameRect = new Rect(
                                               BoneAnimationDataEditorWindow.PADDING,
                                               BoneAnimationDataEditorWindow.PADDING,
                                               _areaRect.width - (BoneAnimationDataEditorWindow.PADDING * 2.0f),
                                               50.0f
                                               );
        }

        static public void SetTexture(Texture2D tex)
        {
            _texture = tex;
        }

        static public void OnGUI()
        {
            GUILayout.BeginArea(_areaRect, GUIContent.none, Style.windowRectBackgroundStyle);

            if (ClipBrowserWindow.CurrentClip != null && TimelineWindow.OneKeyframeSelected)
            {
                KeyframeSM keyframe;
                float time;
                AnimationClipSM clip;

                if (BoneWindow.SelectedBoneDataIndex < editor.boneAnimationData.boneDataList.Count &&
                    BoneWindow.SelectedBoneDataIndex != -1)
                {
                    keyframe = TimelineWindow.FirstSelectedKeyframe;
                    _currentBoneCurves = AnimationHelper.GetBoneDataCurves(keyframe.boneDataIndex);

                    if (_currentBoneCurves != null)
                    {
                        Style.OnGUI();

                        _currentBone = editor.boneAnimationData.GetAnimationClipBoneFromBoneDataIndex(ClipBrowserWindow.SelectedAnimationClipIndex, keyframe.boneDataIndex);
                        clip = ClipBrowserWindow.CurrentClip;

                        time = TimelineWindow.FrameTime;

                        if (keyframe != TimelineWindow.LastKeyframeSelected)
                        {
                            if (keyframe != null)
                            {
                                AnimationTexturePivotWindow.SetTexturePivotWindow();

                                if (keyframe.useTextureGUID && keyframe.textureGUID != "")
                                {
                                    int textureIndex = Mathf.Max(TextureManager.GetTextureIndex(LastKeyframe.atlas, keyframe.textureGUID), -1);
                                    TextureSelectionWindow.TextureSelectionScrollPosition = new Vector2(0, textureIndex * (BoneAnimationDataEditorWindow.SELECT_TEXTURE_SIZE + TextureSelectionWindow.TEXTURE_LABEL_SIZE + TextureSelectionWindow.TEXTURE_SELECTION_SPACING));
                                }
                            }

                            editor.SetNeedsRepainted();
                        }

                        TimelineWindow.LastKeyframeSelected = keyframe;


                        GUIHelper.DrawBox(_bonePropertiesBoneNameRect, Style.selectedInformationStyle, true);

                        // Begin overall grouping
                        GUILayout.BeginVertical();

                        DrawKeyframeProperties(keyframe);

                        // Start main properties scroll
                        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);

                        DrawUserTrigger(clip, keyframe);
                        DrawKeyframeType(clip, keyframe);
                        DrawAtlas(clip, keyframe);
                        DrawTextureAndPivot(clip, keyframe);

                        _lastBoneEditorTextureGUID = LastKeyframe.textureGUID;

                        DrawDepth(clip, keyframe);

                        DrawCollider(clip, keyframe);
                        DrawLocalPositionGroup(clip, keyframe, time);
                        DrawLocalRotationGroup(clip, keyframe, time);
                        DrawLocalScaleGroup(clip, keyframe, time);
                        DrawImageScaleGroup(clip, keyframe, time);
                        DrawColorGroup(clip, keyframe, time);

                        GUILayout.EndScrollView();
                        // End properties scroll

                        GUILayout.EndVertical();
                        // End Overall grouping

                    }
                }
            }

            GUILayout.EndArea();
        }

        static private void DrawKeyframeProperties(KeyframeSM keyframe)
        {
            // Animation Name
            GUILayout.BeginHorizontal();
            GUILayout.Space(BoneAnimationDataEditorWindow.PADDING);
            GUILayout.Label("Animation:", Style.selectedInformationFieldStyle, GUILayout.Width(70.0f), GUILayout.Height(15.0f));
            GUILayout.Label(ClipBrowserWindow.CurrentClip.animationName, Style.selectedInformationValueStyle, GUILayout.Height(15.0f));
            GUILayout.EndHorizontal();
            // End Animation Name

            // Bone Name
            GUILayout.BeginHorizontal();
            GUILayout.Space(BoneAnimationDataEditorWindow.PADDING);
            GUILayout.Label("Bone:", Style.selectedInformationFieldStyle, GUILayout.Width(70.0f), GUILayout.Height(15.0f));
            GUILayout.Label(editor.boneAnimationData.boneDataList[BoneWindow.SelectedBoneDataIndex].boneName, Style.selectedInformationValueStyle, GUILayout.Height(15.0f));
            GUILayout.EndHorizontal();
            // End Bone Name

            // Frame
            GUILayout.BeginHorizontal();
            GUILayout.Space(BoneAnimationDataEditorWindow.PADDING);
            GUILayout.Label("Frame:", Style.selectedInformationFieldStyle, GUILayout.Width(70.0f), GUILayout.Height(15.0f));
            GUILayout.Label(keyframe.frame.ToString(), Style.selectedInformationValueStyle, GUILayout.Height(15.0f));
            GUILayout.EndHorizontal();
            // End Frame

            GUILayout.Space(10.0f);
        }

        static private void DrawUserTrigger(AnimationClipSM clip, KeyframeSM keyframe)
        {
            // if this is not the root bone
            if (keyframe.boneDataIndex > 0)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Space(5.0f);

                bool newUserTriggerCallback = GUILayout.Toggle(keyframe.userTriggerCallback, "User Trigger", Style.normalToggleStyle);
                if (newUserTriggerCallback != keyframe.userTriggerCallback)
                {
                    editor.SetWillBeDirty();

                    keyframe.userTriggerCallback = newUserTriggerCallback;

                    TimelineWindow.SetFrameGridNeedsRebuilt();
                }
                GUILayout.EndHorizontal();

                if (newUserTriggerCallback)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(5.0f);
                    GUILayout.Label("Tag:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                    string newTag = GUILayout.TextField(keyframe.userTriggerTag);
                    GUILayout.Space(5.0f);
                    if (newTag != keyframe.userTriggerTag)
                    {
                        editor.SetWillBeDirty();
                        keyframe.userTriggerTag = newTag;

                        clip.SetMaxKeyframe();

                        TimelineWindow.SetFrameGridNeedsRebuilt();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(10.0f);
                }

            }
        }

        static private void DrawKeyframeType(AnimationClipSM clip, KeyframeSM keyframe)
        {
            // if this is not the root bone
            if (keyframe.boneDataIndex > 0)
            {
                // Keyframe type start
                bool newUse;

                GUILayout.BeginHorizontal();

                GUILayout.Space(5.0f);

                if (keyframe.frame > 0)
                {
                    newUse = GUILayout.Toggle(keyframe.useKeyframeType, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                    if (newUse != keyframe.useKeyframeType)
                    {
                        editor.SetWillBeDirty();

                        keyframe.useKeyframeType = newUse;
                        if (newUse)
                        {
                            keyframe.keyframeType = LastKeyframe.keyframeType;
                            TimelineWindow.UpdateLastKeyframe();
                        }

                        clip.SetMaxKeyframe();

                        TimelineWindow.SetFrameGridNeedsRebuilt();
                    }
                }
                else
                {
                    newUse = true;
                }

                if (newUse)
                {
                    GUILayout.Label("Type:", Style.normalLabelStyle);
                    KeyframeSM.KEYFRAME_TYPE newKeyframeType = (KeyframeSM.KEYFRAME_TYPE)EditorGUILayout.EnumPopup(keyframe.keyframeType);
                    if (newKeyframeType != keyframe.keyframeType)
                    {
                        editor.SetWillBeDirty();

                        keyframe.keyframeType = newKeyframeType;
                        TimelineWindow.UpdateLastKeyframe();
                    }
                }
                else
                {
                    GUILayout.Label("No Type Key", Style.normalLabelStyle);
                }
                GUILayout.EndHorizontal();
                // End keyframe type

                GUILayout.Space(10.0f);
            }
        }

        static private void DrawAtlas(AnimationClipSM clip, KeyframeSM keyframe)
        {
            // if this is not the root bone
            if (keyframe.boneDataIndex > 0)
            {
                if (LastKeyframe.keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                {
                    bool newUse;

                    // Atlas
                    GUILayout.BeginHorizontal();

                    GUILayout.Space(5.0f);

                    if (keyframe.frame > 0)
                    {
                        newUse = GUILayout.Toggle(keyframe.useAtlas, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                        if (newUse != keyframe.useAtlas)
                        {
                            editor.SetWillBeDirty();

                            keyframe.useAtlas = newUse;
                            if (newUse)
                            {
                                keyframe.atlas = LastKeyframe.atlas;
                                GetAtlasLists(keyframe.atlas);
                                TimelineWindow.UpdateLastKeyframe();
                            }

                            clip.SetMaxKeyframe();

                            TimelineWindow.SetFrameGridNeedsRebuilt();
                        }
                    }
                    else
                    {
                        newUse = true;
                    }

                    if (newUse)
                    {
                        if (!_atlasListBuilt)
                            GetAtlasLists(keyframe.atlas);
                        else
                            SetSelectedAtlasIndex(keyframe.atlas);

                        GUILayout.Label("Atlas:", Style.normalLabelStyle);
                        int newAtlasIndex = EditorGUILayout.Popup(_selectedAtlasIndex, _atlasNameList.ToArray());
                        if (newAtlasIndex != _selectedAtlasIndex)
                        {
                            editor.SetWillBeDirty();

                            _selectedAtlasIndex = newAtlasIndex;
                            keyframe.atlas = _atlasList[_selectedAtlasIndex];
                            LastKeyframe.atlas = keyframe.atlas;

                            _lastBoneEditorTextureGUID = "";
                            TimelineWindow.UpdateLastKeyframe();

                            TextureManager.GenerateTextureDictionary(true);
                            if (TextureSelectionWindow.Visible)
                            {
                                editor.SetNeedsRepainted();
                            }

                            if (keyframe.atlas == null)
                            {
                                _texture = null;
                            }
                        }





                        //GUILayout.Label("Atlas:", Style.normalLabelStyle);

                        //TextureAtlas newAtlas = (TextureAtlas)EditorGUILayout.ObjectField(keyframe.atlas, typeof(TextureAtlas), false);
                        //if (newAtlas != keyframe.atlas)
                        //{
                        //    editor.SetWillBeDirty();

                        //    keyframe.atlas = newAtlas;
                        //    _lastBoneEditorTextureGUID = "";
                        //    TimelineWindow.UpdateLastKeyframe();

                        //    TextureManager.GenerateTextureDictionary(true);
                        //    if (TextureSelectionWindow.Visible)
                        //    {
                        //        editor.SetNeedsRepainted();
                        //    }

                        //    if (newAtlas == null)
                        //    {
                        //        _texture = null;
                        //    }
                        //}
                    }
                    else
                    {
                        GUILayout.Label("No Atlas Key", Style.normalLabelStyle);
                    }

                    GUILayout.EndHorizontal();
                    // End Atlas

                    GUILayout.Space(10.0f);
                }
            }
        }

        static private void GetAtlasLists(TextureAtlas atlas)
        {
            DirectoryInfo di;
            FileInfo[] allFiles;
            int assetPathIndex;
            string assetPath;
            TextureAtlas ad;

            if (_atlasNameList == null)
            {
                _atlasNameList = new List<string>();
            }
            else
            {
                _atlasNameList.Clear();
            }

            if (_atlasList == null)
            {
                _atlasList = new List<TextureAtlas>();
            }
            else
            {
                _atlasList.Clear();
            }

            if (_atlasGUIDList == null)
            {
                _atlasGUIDList = new List<string>();
            }
            else
            {
                _atlasGUIDList.Clear();
            }

            _atlasList.Add(null);
            _atlasNameList.Add("< None >");
            _atlasGUIDList.Add("");

            di = new DirectoryInfo(Application.dataPath);
            allFiles = di.GetFiles("*.asset", SearchOption.AllDirectories);
            foreach (FileInfo file in allFiles)
            {
                assetPathIndex = file.FullName.IndexOf("Assets/");

                if (assetPathIndex == -1)
                {
                    assetPathIndex = file.FullName.IndexOf(@"Assets\");
                }

                if (assetPathIndex >= 0)
                {
                    assetPath = file.FullName.Substring(assetPathIndex, file.FullName.Length - assetPathIndex);
                    ad = (TextureAtlas)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextureAtlas));

                    if (ad != null)
                    {
                        _atlasList.Add(ad);
                        _atlasNameList.Add(ad.name);
                        _atlasGUIDList.Add(AssetDatabase.AssetPathToGUID(assetPath));
                    }
                }

            }

            _atlasListBuilt = true;

            SetSelectedAtlasIndex(atlas);
        }

        static private void SetSelectedAtlasIndex(TextureAtlas atlas)
        {
            if (atlas != null && _atlasGUIDList != null)
            {
                string guid;
                string assetPath;

                assetPath = AssetDatabase.GetAssetPath(atlas);
                guid = AssetDatabase.AssetPathToGUID(assetPath);

                _selectedAtlasIndex = _atlasGUIDList.IndexOf(guid);

                if (_selectedAtlasIndex == -1)
                {
                    _selectedAtlasIndex = 0;
                }
            }
            else
            {
                _selectedAtlasIndex = 0;
            }
        }

        static public void DrawTextureAndPivot(AnimationClipSM clip, KeyframeSM keyframe)
        {
            // if this is not the root bone
            if (keyframe.boneDataIndex > 0)
            {
                if (LastKeyframe.keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                {
                    bool newUse;

                    if (LastKeyframe.textureGUID != _lastBoneEditorTextureGUID)
                    {
                        editor.SetWillBeDirty();

                        if (LastKeyframe.textureGUID != "" && LastKeyframe.atlas != null)
                        {
                            if (TextureManager.TextureDictionary.ContainsKey(LastKeyframe.textureGUID))
                            {
                                if (TextureManager.GetTextureIndex(LastKeyframe.atlas, LastKeyframe.textureGUID) == -1)
                                    _texture = null;
                                else
                                    _texture = TextureManager.TextureDictionary[LastKeyframe.textureGUID].texture;
                            }
                        }
                    }

                    // Texture and Pivot
                    GUILayout.BeginHorizontal();

                    GUILayout.Space(5.0f);

                    if (keyframe.frame > 0)
                    {
                        newUse = GUILayout.Toggle(keyframe.useTextureGUID, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                        if (newUse != keyframe.useTextureGUID)
                        {
                            editor.SetWillBeDirty();

                            keyframe.useTextureGUID = newUse;
                            if (newUse)
                            {
                                keyframe.textureGUID = LastKeyframe.textureGUID;
                                TimelineWindow.UpdateLastKeyframe();
                            }

                            clip.SetMaxKeyframe();

                            TimelineWindow.SetFrameGridNeedsRebuilt();
                        }
                    }
                    else
                    {
                        newUse = true;
                    }

                    if (newUse)
                    {
                        GUILayout.Space(BoneAnimationDataEditorWindow.PADDING * 2.0f);
                        if (GUILayout.Button(_texture, GUILayout.Width(BonePropertiesWindow.BONE_EDITOR_TEXTURE_FORCE_SIZE), GUILayout.Height(BonePropertiesWindow.BONE_EDITOR_TEXTURE_FORCE_SIZE)))
                        {
                            if (TextureSelectionWindow.Visible)
                            {
                                TextureSelectionWindow.Visible = false;
                            }
                            else
                            {
                                int textureIndex = Mathf.Min(TextureManager.GetTextureIndex(LastKeyframe.atlas, keyframe.textureGUID), 0);
                                TextureSelectionWindow.TextureSelectionScrollPosition = new Vector2(0, textureIndex * BoneAnimationDataEditorWindow.SELECT_TEXTURE_SIZE);
                                TextureSelectionWindow.Visible = true;
                            }

                            editor.SetNeedsRepainted();
                        }
                    }
                    else
                    {
                        GUILayout.Label("No Texture Key", Style.wordWrapStyle, GUILayout.Width(60.0f));
                    }

                    GUILayout.Space(10.0f);

                    if (keyframe.frame > 0)
                    {
                        newUse = GUILayout.Toggle(keyframe.usePivotOffset, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                        if (newUse != keyframe.usePivotOffset)
                        {
                            editor.SetWillBeDirty();

                            keyframe.usePivotOffset = newUse;
                            if (newUse)
                            {
                                keyframe.pivotOffset = LastKeyframe.pivotOffset;
                                keyframe.useDefaultPivot = LastKeyframe.useDefaultPivot;
                                TimelineWindow.UpdateLastKeyframe();
                            }

                            clip.SetMaxKeyframe();

                            TimelineWindow.SetFrameGridNeedsRebuilt();
                        }
                    }
                    else
                    {
                        newUse = true;
                    }

                    if (newUse)
                    {
                        if (GUILayout.Button(Resources.pivotButton, GUILayout.Width(BonePropertiesWindow.BONE_EDITOR_TEXTURE_FORCE_SIZE), GUILayout.Height(BonePropertiesWindow.BONE_EDITOR_TEXTURE_FORCE_SIZE)))
                        {
                            if (AnimationTexturePivotWindow.Visible)
                            {
                                AnimationTexturePivotWindow.Visible = false;
                            }
                            else
                            {
                                AnimationTexturePivotWindow.SetTexturePivotWindow();
                                AnimationTexturePivotWindow.Visible = true;
                            }

                            editor.SetNeedsRepainted();
                        }
                    }
                    else
                    {
                        GUILayout.Label("No Pivot Key", Style.wordWrapStyle, GUILayout.Width(60.0f));
                    }
                    GUILayout.EndHorizontal();
                    // End Texture and Pivot

                    GUILayout.Space(10.0f);
                }
            }
        }

        static public void DrawDepth(AnimationClipSM clip, KeyframeSM keyframe)
        {
            // if this is not the root bone
            if (keyframe.boneDataIndex > 0)
            {
                if (LastKeyframe.keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                {
                    bool newUse;

                    // Depth
                    GUILayout.BeginHorizontal();

                    GUILayout.Space(5.0f);

                    if (keyframe.frame > 0)
                    {
                        newUse = GUILayout.Toggle(keyframe.useDepth, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                        if (newUse != keyframe.useDepth)
                        {
                            editor.SetWillBeDirty();

                            keyframe.useDepth = newUse;
                            if (newUse)
                            {
                                keyframe.depth = LastKeyframe.depth;
                                TimelineWindow.UpdateLastKeyframe();
                            }

                            clip.SetMaxKeyframe();

                            TimelineWindow.SetFrameGridNeedsRebuilt();
                        }
                    }
                    else
                    {
                        newUse = true;
                    }

                    if (newUse)
                    {
                        GUILayout.Label("Depth:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));

                        string[] depths = new string[KeyframeSM.MAX_DEPTH + 1];
                        for (int d = 0; d <= KeyframeSM.MAX_DEPTH; d++)
                        {
                            depths[d] = "Depth " + d.ToString();
                        }
                        int newDepth = EditorGUILayout.Popup(keyframe.depth, depths);
                        if (newDepth != keyframe.depth)
                        {
                            editor.SetWillBeDirty();
                            keyframe.depth = newDepth;
                        }
                    }
                    else
                    {
                        GUILayout.Label("No Depth Key", Style.normalLabelStyle);
                    }
                    GUILayout.EndHorizontal();
                    // End Depth

                    GUILayout.Space(10.0f);
                }
            }
        }

        static private void DrawCollider(AnimationClipSM clip, KeyframeSM keyframe)
        {
            bool newUse;

            // Collider
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            GUILayout.Space(5.0f);

            if (keyframe.frame > 0)
            {
                newUse = GUILayout.Toggle(keyframe.useCollider, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                if (newUse != keyframe.useCollider)
                {
                    editor.SetWillBeDirty();

                    keyframe.useCollider = newUse;
                    if (newUse)
                    {
                        keyframe.collider.type = LastKeyframe.collider.type;
                        keyframe.collider.center = LastKeyframe.collider.center;
                        keyframe.collider.boxSize = LastKeyframe.collider.boxSize;
                        keyframe.collider.sphereRadius = LastKeyframe.collider.sphereRadius;
                        keyframe.collider.layer = LastKeyframe.collider.layer;
                        keyframe.collider.useAnimationLayer = LastKeyframe.collider.useAnimationLayer;
                        keyframe.collider.isTrigger = LastKeyframe.collider.isTrigger;
                        keyframe.collider.tag = LastKeyframe.collider.tag;
                        TimelineWindow.UpdateLastKeyframe();
                    }

                    clip.SetMaxKeyframe();

                    TimelineWindow.SetFrameGridNeedsRebuilt();
                }
            }
            else
            {
                newUse = true;
            }

            if (newUse)
            {
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();

                GUILayout.Label("Collider:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));

                ColliderSM.COLLIDER_TYPE newColliderType = (ColliderSM.COLLIDER_TYPE)EditorGUILayout.EnumPopup(keyframe.collider.type);
                if (newColliderType != keyframe.collider.type)
                {
                    editor.SetWillBeDirty();
                    keyframe.collider.type = newColliderType;
                }

                GUILayout.EndHorizontal();

                if (keyframe.collider.type != ColliderSM.COLLIDER_TYPE.None)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Tag:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                    string newTag = GUILayout.TextField(keyframe.collider.tag);
                    if (newTag != keyframe.collider.tag)
                    {
                        editor.SetWillBeDirty();
                        keyframe.collider.tag = newTag;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();

                    bool newIsTrigger = GUILayout.Toggle(keyframe.collider.isTrigger, "Is Trigger?", Style.normalToggleStyle);
                    if (newIsTrigger != keyframe.collider.isTrigger)
                    {
                        editor.SetWillBeDirty();
                        keyframe.collider.isTrigger = newIsTrigger;
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Layer:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));

                    bool newUseAnimationLayer = GUILayout.Toggle(keyframe.collider.useAnimationLayer, "Use Animation Layer", Style.normalToggleStyle);
                    if (newUseAnimationLayer != keyframe.collider.useAnimationLayer)
                    {
                        editor.SetWillBeDirty();
                        keyframe.collider.useAnimationLayer = newUseAnimationLayer;
                    }

                    GUILayout.EndHorizontal();

                    if (!newUseAnimationLayer)
                    {
                        GUILayout.BeginHorizontal();

                        GUILayout.Space(LABEL_WIDTH);

                        int newColliderLayer = EditorGUILayout.LayerField(keyframe.collider.layer);
                        if (newColliderLayer != keyframe.collider.layer)
                        {
                            editor.SetWillBeDirty();
                            keyframe.collider.layer = newColliderLayer;
                        }

                        GUILayout.EndHorizontal();
                    }

                    float newColliderCenterX;
                    float newColliderCenterY;
                    float newColliderCenterZ;

                    GUILayout.BeginVertical();
                    GUILayout.Label("Collider Center:", Style.normalLabelStyle);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("X: ", Style.normalLabelStyle, GUILayout.Width(20.0f));
                    newColliderCenterX = EditorGUILayout.FloatField(keyframe.collider.center.x, GUILayout.Width(40.0f));
                    GUILayout.Label("Y: ", Style.normalLabelStyle, GUILayout.Width(20.0f));
                    newColliderCenterY = EditorGUILayout.FloatField(keyframe.collider.center.y, GUILayout.Width(40.0f));
                    GUILayout.Label("Z: ", Style.normalLabelStyle, GUILayout.Width(20.0f));
                    newColliderCenterZ = EditorGUILayout.FloatField(keyframe.collider.center.z, GUILayout.Width(40.0f));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    Vector3 newColliderCenter = new Vector3(newColliderCenterX, newColliderCenterY, newColliderCenterZ);
                    if (newColliderCenter != keyframe.collider.center)
                    {
                        editor.SetWillBeDirty();
                        keyframe.collider.center = newColliderCenter;
                    }

                    switch (keyframe.collider.type)
                    {
                        case ColliderSM.COLLIDER_TYPE.Box:
                            float newColliderBoxSizeX;
                            float newColliderBoxSizeY;
                            float newColliderBoxSizeZ;

                            GUILayout.BeginVertical();
                            GUILayout.Label("Collider Box Size:", Style.normalLabelStyle);
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("X: ", Style.normalLabelStyle, GUILayout.Width(20.0f));
                            newColliderBoxSizeX = EditorGUILayout.FloatField(keyframe.collider.boxSize.x, GUILayout.Width(40.0f));
                            GUILayout.Label("Y: ", Style.normalLabelStyle, GUILayout.Width(20.0f));
                            newColliderBoxSizeY = EditorGUILayout.FloatField(keyframe.collider.boxSize.y, GUILayout.Width(40.0f));
                            GUILayout.Label("Z: ", Style.normalLabelStyle, GUILayout.Width(20.0f));
                            newColliderBoxSizeZ = EditorGUILayout.FloatField(keyframe.collider.boxSize.z, GUILayout.Width(40.0f));
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();

                            Vector3 newColliderBoxSize = new Vector3(newColliderBoxSizeX, newColliderBoxSizeY, newColliderBoxSizeZ);
                            if (newColliderBoxSize != keyframe.collider.boxSize)
                            {
                                editor.SetWillBeDirty();
                                keyframe.collider.boxSize = newColliderBoxSize;
                            }
                            break;

                        case ColliderSM.COLLIDER_TYPE.Sphere:
                            GUILayout.Label("Collider Sphere Radius:", Style.normalLabelStyle);

                            float newColliderSphereRadius = EditorGUILayout.FloatField("", keyframe.collider.sphereRadius);
                            if (newColliderSphereRadius != keyframe.collider.sphereRadius)
                            {
                                editor.SetWillBeDirty();
                                keyframe.collider.sphereRadius = newColliderSphereRadius;
                            }
                            break;
                    }

                    if (_texture != null && LastKeyframe.keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                    {
                        if (GUILayout.Button("Match Texture Size"))
                        {
                            switch (keyframe.collider.type)
                            {
                                case ColliderSM.COLLIDER_TYPE.Box:
                                    editor.SetWillBeDirty();
                                    keyframe.collider.boxSize = new Vector3(_texture.width * keyframe.imageScale.val.x, _texture.height * keyframe.imageScale.val.y, 0);
                                    break;

                                case ColliderSM.COLLIDER_TYPE.Sphere:
                                    float halfTextureWidth = (_texture.width * keyframe.imageScale.val.x) / 2.0f;
                                    float halfTextureHeight = (_texture.height * keyframe.imageScale.val.y) / 2.0f;
                                    keyframe.collider.sphereRadius = Mathf.Sqrt((halfTextureWidth * halfTextureWidth) + (halfTextureHeight * halfTextureHeight));
                                    break;
                            }
                        }
                    }
                }

                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.Label("No Collider Key", Style.normalLabelStyle);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            // End Collider

            GUILayout.Space(15.0f);
        }

        static private void DrawLocalPositionGroup(AnimationClipSM clip, KeyframeSM keyframe, float time)
        {
            bool newUse;
            Vector3 newPosition;

            // Position Group
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Space(LABEL_WIDTH);
            GUILayout.Label("    Local Position", Style.normalLabelStyle);
            GUILayout.EndHorizontal();

            // Position X
            GUILayout.BeginHorizontal();

            GUILayout.Space(5.0f);

            if (_currentBoneCurves.localPositionXNeedsTwoKeys)
            {
                GUIContent guiContent = new GUIContent(Resources.warning, "You must supply at least two keyframes for localPosition.x to generate an animation curve");
                GUILayout.Label(guiContent, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
            }

            if (keyframe.frame > 0)
            {
                newUse = GUILayout.Toggle(keyframe.localPosition3.useX, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                if (newUse != keyframe.localPosition3.useX)
                {
                    editor.SetWillBeDirty();

                    keyframe.localPosition3.useX = newUse;
                    if (newUse)
                    {
                        keyframe.localPosition3.val.x = _currentBoneCurves.localPositionXCurve.Evaluate(time);
                        keyframe.localPosition3.inTangentX = AnimationHelper.CalculateTangent(_currentBoneCurves.localPositionXCurve, (float)keyframe.frame, keyframe.localPosition3.val.x);
                        keyframe.localPosition3.outTangentX = keyframe.localPosition3.inTangentX;
                    }

                    //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                    //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
                    AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                    clip.SetMaxKeyframe();

                    TimelineWindow.SetFrameGridNeedsRebuilt();
                }
            }
            else
            {
                newUse = true;
            }

            if (newUse)
            {
                GUILayout.Label("X:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                newPosition.x = EditorGUILayout.FloatField(keyframe.localPosition3.val.x);
                if (newPosition.x != keyframe.localPosition3.val.x)
                {
                    if (AnimationWindow.Action == AnimationWindow.ACTION.None)
                    {
                        editor.SetWillBeDirty();
                    }

                    keyframe.localPosition3.val.x = newPosition.x;

                    MoveKey(keyframe, _currentBoneCurves.localPositionXCurve, KeyframeSM.CURVE_PROPERTY.LocalPositionX);
                }
                if (_currentBoneCurves.localPositionXCurve.keys.Length > 1)
                {
                    CurveField(KeyframeSM.CURVE_PROPERTY.LocalPositionX);
                }
            }
            else
            {
                GUILayout.Label("No Position X Key", Style.normalLabelStyle);
            }

            GUILayout.EndHorizontal();
            // End Position X

            // Position Y
            GUILayout.BeginHorizontal();

            GUILayout.Space(5.0f);

            if (_currentBoneCurves.localPositionYNeedsTwoKeys)
            {
                GUIContent guiContent = new GUIContent(Resources.warning, "You must supply at least two keyframes for localPosition.y to generate an animation curve");
                GUILayout.Label(guiContent, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
            }

            if (keyframe.frame > 0)
            {
                newUse = GUILayout.Toggle(keyframe.localPosition3.useY, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                if (newUse != keyframe.localPosition3.useY)
                {
                    editor.SetWillBeDirty();

                    keyframe.localPosition3.useY = newUse;
                    if (newUse)
                    {
                        keyframe.localPosition3.val.y = _currentBoneCurves.localPositionYCurve.Evaluate(time);
                        keyframe.localPosition3.inTangentY = AnimationHelper.CalculateTangent(_currentBoneCurves.localPositionYCurve, (float)keyframe.frame, keyframe.localPosition3.val.y);
                        keyframe.localPosition3.outTangentY = keyframe.localPosition3.inTangentY;
                    }

                    //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                    //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
                    AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                    clip.SetMaxKeyframe();

                    TimelineWindow.SetFrameGridNeedsRebuilt();
                }
            }
            else
            {
                newUse = true;
            }

            if (newUse)
            {
                GUILayout.Label("Y:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                newPosition.y = EditorGUILayout.FloatField(keyframe.localPosition3.val.y);
                if (newPosition.y != keyframe.localPosition3.val.y)
                {
                    if (AnimationWindow.Action == AnimationWindow.ACTION.None)
                        editor.SetWillBeDirty();

                    keyframe.localPosition3.val.y = newPosition.y;

                    MoveKey(keyframe, _currentBoneCurves.localPositionYCurve, KeyframeSM.CURVE_PROPERTY.LocalPositionY);
                }
                if (_currentBoneCurves.localPositionYCurve.keys.Length > 1)
                {
                    CurveField(KeyframeSM.CURVE_PROPERTY.LocalPositionY);
                }
            }
            else
            {
                GUILayout.Label("No Position Y Key", Style.normalLabelStyle);
            }

            GUILayout.EndHorizontal();
            // End Position Y


            // Position Z is no longer used in Unity 5. Instead, the z position is used by the depth setting to control the ordering of
            // bones. Unity 5 changed the way the submesh draw order was handled, taking away the ability to sort by the submesh index.
            // As a result, multiple materials would not sort properly. Using the z position with an orthographic camera will mitigate this
            // bug in Unity somewhat.

            //// Position Z
            //GUILayout.BeginHorizontal();

            //GUILayout.Space(5.0f);

            //if (_currentBoneCurves.localPositionZNeedsTwoKeys)
            //{
            //    GUIContent guiContent = new GUIContent(Resources.warning, "You must supply at least two keyframes for localPosition.z to generate an animation curve");
            //    GUILayout.Label(guiContent, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
            //}

            //if (keyframe.frame > 0)
            //{
            //    newUse = GUILayout.Toggle(keyframe.localPosition3.useZ, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
            //    if (newUse != keyframe.localPosition3.useZ)
            //    {
            //        editor.SetWillBeDirty();

            //        keyframe.localPosition3.useZ = newUse;
            //        if (newUse)
            //        {
            //            keyframe.localPosition3.val.z = _currentBoneCurves.localPositionZCurve.Evaluate(time);
            //            keyframe.localPosition3.inTangentZ = AnimationHelper.CalculateTangent(_currentBoneCurves.localPositionZCurve, (float)keyframe.frame, keyframe.localPosition3.val.z);
            //            keyframe.localPosition3.outTangentZ = keyframe.localPosition3.inTangentZ;
            //        }

            //        //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
            //        //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
            //        AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

            //        clip.SetMaxKeyframe();

            //        TimelineWindow.SetFrameGridNeedsRebuilt();
            //    }
            //}
            //else
            //{
            //    newUse = true;
            //}

            //if (newUse)
            //{
            //    GUILayout.Label("Z:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
            //    newPosition.z = EditorGUILayout.FloatField(keyframe.localPosition3.val.z);
            //    if (newPosition.z != keyframe.localPosition3.val.z)
            //    {
            //        if (AnimationWindow.Action == AnimationWindow.ACTION.None)
            //            editor.SetWillBeDirty();

            //        keyframe.localPosition3.val.z = newPosition.z;

            //        MoveKey(keyframe, _currentBoneCurves.localPositionZCurve, KeyframeSM.CURVE_PROPERTY.LocalPositionZ);
            //    }
            //    if (_currentBoneCurves.localPositionZCurve.keys.Length > 1)
            //    {
            //        CurveField(KeyframeSM.CURVE_PROPERTY.LocalPositionZ);
            //    }
            //}
            //else
            //{
            //    GUILayout.Label("No Position Z Key", Style.normalLabelStyle);
            //}

            //GUILayout.EndHorizontal();
            //// End Position Z

            GUILayout.EndVertical();
            // End Position Group

            GUILayout.Space(10.0f);
        }

        static public void DrawLocalRotationGroup(AnimationClipSM clip, KeyframeSM keyframe, float time)
        {
            bool newUse;

            // Local Rotation Group
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Space(LABEL_WIDTH);
            GUILayout.Label("    Local Rotation", Style.normalLabelStyle);
            GUILayout.EndHorizontal();

            // Local Rotation
            GUILayout.BeginHorizontal();

            GUILayout.Space(5.0f);

            if (_currentBoneCurves.localRotationNeedsTwoKeys)
            {
                GUIContent guiContent = new GUIContent(Resources.warning, "You must supply at least two keyframes for localRotation to generate an animation curve");
                GUILayout.Label(guiContent, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
            }

            if (keyframe.frame > 0)
            {
                newUse = GUILayout.Toggle(keyframe.localRotation.use, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                if (newUse != keyframe.localRotation.use)
                {
                    editor.SetWillBeDirty();

                    keyframe.localRotation.use = newUse;
                    if (newUse)
                    {
                        keyframe.localRotation.val = _currentBoneCurves.localRotationCurve.Evaluate(time);
                        keyframe.localRotation.inTangent = AnimationHelper.CalculateTangent(_currentBoneCurves.localRotationCurve, (float)keyframe.frame, keyframe.localRotation.val);
                        keyframe.localRotation.outTangent = keyframe.localRotation.inTangent;
                    }

                    //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                    //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
                    AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                    clip.SetMaxKeyframe();

                    TimelineWindow.SetFrameGridNeedsRebuilt();
                }
            }
            else
            {
                newUse = true;
            }

            if (newUse)
            {
                GUILayout.Label("Degrees:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                float newRotation = EditorGUILayout.FloatField(keyframe.localRotation.val);
                if (newRotation != keyframe.localRotation.val)
                {
                    if (AnimationWindow.Action == AnimationWindow.ACTION.None)
                        editor.SetWillBeDirty();

                    keyframe.localRotation.val = newRotation;

                    MoveKey(keyframe, _currentBoneCurves.localRotationCurve, KeyframeSM.CURVE_PROPERTY.LocalRotation);
                }
                GUILayout.FlexibleSpace();
                if (_currentBoneCurves.localRotationCurve.keys.Length > 1)
                {
                    CurveField(KeyframeSM.CURVE_PROPERTY.LocalRotation);
                }
            }
            else
            {
                GUILayout.Label("No Local Rotation Key", Style.normalLabelStyle);
                GUILayout.FlexibleSpace();
            }

            GUILayout.EndHorizontal();
            // End Local Rotation

            GUILayout.EndVertical();
            // End Local Rotation Group

            GUILayout.Space(10.0f);
        }

        static public void DrawLocalScaleGroup(AnimationClipSM clip, KeyframeSM keyframe, float time)
        {
            bool newUse;
            Vector3 newScale;

            // Scale Group
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Space(LABEL_WIDTH);
            GUILayout.Label("    Local Scale", Style.normalLabelStyle);
            GUILayout.EndHorizontal();

            // Scale X
            GUILayout.BeginHorizontal();

            GUILayout.Space(5.0f);

            if (_currentBoneCurves.localScaleNeedsTwoKeys)
            {
                GUIContent guiContent = new GUIContent(Resources.warning, "You must supply at least two keyframes for localScale x, y, or z to generate an animation curve");
                GUILayout.Label(guiContent, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
            }

            if (keyframe.frame > 0)
            {
                newUse = GUILayout.Toggle(keyframe.localScale3.useX, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                if (newUse != keyframe.localScale3.useX)
                {
                    editor.SetWillBeDirty();

                    keyframe.localScale3.useX = newUse;
                    if (newUse)
                    {
                        keyframe.localScale3.val.x = _currentBoneCurves.localScaleXCurve.Evaluate(time);
                        keyframe.localScale3.inTangentX = AnimationHelper.CalculateTangent(_currentBoneCurves.localScaleXCurve, (float)keyframe.frame, keyframe.localScale3.val.x);
                        keyframe.localScale3.outTangentX = keyframe.localScale3.inTangentX;
                    }

                    //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                    //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
                    AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                    clip.SetMaxKeyframe();

                    TimelineWindow.SetFrameGridNeedsRebuilt();
                }
            }
            else
            {
                newUse = true;
            }

            if (newUse)
            {
                GUILayout.Label("X:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                newScale.x = EditorGUILayout.FloatField(keyframe.localScale3.val.x);
                if (newScale.x != keyframe.localScale3.val.x)
                {
                    if (AnimationWindow.Action == AnimationWindow.ACTION.None)
                        editor.SetWillBeDirty();

                    keyframe.localScale3.val.x = newScale.x;

                    MoveKey(keyframe, _currentBoneCurves.localScaleXCurve, KeyframeSM.CURVE_PROPERTY.LocalScaleX);
                }
                if (_currentBoneCurves.localScaleXCurve.keys.Length > 1)
                {
                    CurveField(KeyframeSM.CURVE_PROPERTY.LocalScaleX);
                }
            }
            else
            {
                GUILayout.Label("No Scale X Key", Style.normalLabelStyle);
            }

            GUILayout.EndHorizontal();
            // End Scale X

            // Scale Y
            GUILayout.BeginHorizontal();

            GUILayout.Space(5.0f);

            if (_currentBoneCurves.localScaleNeedsTwoKeys)
            {
                GUIContent guiContent = new GUIContent(Resources.warning, "You must supply at least two keyframes for localScale x, y, or z to generate an animation curve");
                GUILayout.Label(guiContent, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
            }

            if (keyframe.frame > 0)
            {
                newUse = GUILayout.Toggle(keyframe.localScale3.useY, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                if (newUse != keyframe.localScale3.useY)
                {
                    editor.SetWillBeDirty();

                    keyframe.localScale3.useY = newUse;
                    if (newUse)
                    {
                        keyframe.localScale3.val.y = _currentBoneCurves.localScaleYCurve.Evaluate(time);
                        keyframe.localScale3.inTangentY = AnimationHelper.CalculateTangent(_currentBoneCurves.localScaleYCurve, (float)keyframe.frame, keyframe.localScale3.val.y);
                        keyframe.localScale3.outTangentY = keyframe.localScale3.inTangentY;
                    }

                    //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                    //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
                    AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                    clip.SetMaxKeyframe();

                    TimelineWindow.SetFrameGridNeedsRebuilt();
                }
            }
            else
            {
                newUse = true;
            }

            if (newUse)
            {

                GUILayout.Label("Y:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                newScale.y = EditorGUILayout.FloatField(keyframe.localScale3.val.y);
                if (newScale.y != keyframe.localScale3.val.y)
                {
                    if (AnimationWindow.Action == AnimationWindow.ACTION.None)
                        editor.SetWillBeDirty();

                    keyframe.localScale3.val.y = newScale.y;

                    MoveKey(keyframe, _currentBoneCurves.localScaleYCurve, KeyframeSM.CURVE_PROPERTY.LocalScaleY);
                }
                if (_currentBoneCurves.localScaleYCurve.keys.Length > 1)
                {
                    CurveField(KeyframeSM.CURVE_PROPERTY.LocalScaleY);
                }
            }
            else
            {
                GUILayout.Label("No Scale Y Key", Style.normalLabelStyle);
            }

            GUILayout.EndHorizontal();
            // End Scale Y

            // Scale Z
            GUILayout.BeginHorizontal();

            GUILayout.Space(5.0f);

            if (_currentBoneCurves.localScaleNeedsTwoKeys)
            {
                GUIContent guiContent = new GUIContent(Resources.warning, "You must supply at least two keyframes for localScale x, y, or z to generate an animation curve");
                GUILayout.Label(guiContent, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
            }

            if (keyframe.frame > 0)
            {
                newUse = GUILayout.Toggle(keyframe.localScale3.useZ, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                if (newUse != keyframe.localScale3.useZ)
                {
                    editor.SetWillBeDirty();

                    keyframe.localScale3.useZ = newUse;
                    if (newUse)
                    {
                        keyframe.localScale3.val.z = _currentBoneCurves.localScaleZCurve.Evaluate(time);
                        keyframe.localScale3.inTangentZ = AnimationHelper.CalculateTangent(_currentBoneCurves.localScaleZCurve, (float)keyframe.frame, keyframe.localScale3.val.z);
                        keyframe.localScale3.outTangentZ = keyframe.localScale3.inTangentZ;
                    }

                    //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                    //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
                    AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                    clip.SetMaxKeyframe();

                    TimelineWindow.SetFrameGridNeedsRebuilt();
                }
            }
            else
            {
                newUse = true;
            }

            if (newUse)
            {
                GUILayout.Label("Z:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                newScale.z = EditorGUILayout.FloatField(keyframe.localScale3.val.z);
                if (newScale.z != keyframe.localScale3.val.z)
                {
                    if (AnimationWindow.Action == AnimationWindow.ACTION.None)
                        editor.SetWillBeDirty();

                    keyframe.localScale3.val.z = newScale.z;

                    MoveKey(keyframe, _currentBoneCurves.localScaleZCurve, KeyframeSM.CURVE_PROPERTY.LocalScaleZ);
                }
                if (_currentBoneCurves.localScaleZCurve.keys.Length > 1)
                {
                    CurveField(KeyframeSM.CURVE_PROPERTY.LocalScaleZ);
                }
            }
            else
            {
                GUILayout.Label("No Scale Z Key", Style.normalLabelStyle);
            }

            GUILayout.EndHorizontal();
            // End Scale Z

            GUILayout.EndVertical();
            // End Scale Group

            GUILayout.Space(10.0f);
        }

        static public void DrawImageScaleGroup(AnimationClipSM clip, KeyframeSM keyframe, float time)
        {
            if (keyframe.boneDataIndex > 0)
            {
                if (LastKeyframe.keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                {
                    bool newUse;
                    Vector2 newImageScale;

                    // Image Scale Group
                    GUILayout.BeginVertical();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(LABEL_WIDTH);
                    GUILayout.Label("    Image Scale", Style.normalLabelStyle);
                    GUILayout.EndHorizontal();

                    // Image Scale X
                    GUILayout.BeginHorizontal();

                    GUILayout.Space(5.0f);

                    if (_currentBoneCurves.imageScaleNeedsTwoKeys)
                    {
                        GUIContent guiContent = new GUIContent(Resources.warning, "You must supply at least two keyframes for imageScale x or y to generate an animation curve");
                        GUILayout.Label(guiContent, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
                    }

                    if (keyframe.frame > 0)
                    {
                        newUse = GUILayout.Toggle(keyframe.imageScale.useX, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                        if (newUse != keyframe.imageScale.useX)
                        {
                            editor.SetWillBeDirty();

                            keyframe.imageScale.useX = newUse;
                            if (newUse)
                            {
                                keyframe.imageScale.val.x = _currentBoneCurves.imageScaleXCurve.Evaluate(time);
                                keyframe.imageScale.inTangentX = AnimationHelper.CalculateTangent(_currentBoneCurves.imageScaleXCurve, (float)keyframe.frame, keyframe.imageScale.val.x);
                                keyframe.imageScale.outTangentX = keyframe.imageScale.inTangentX;
                            }

                            //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                            //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
                            AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                            clip.SetMaxKeyframe();

                            TimelineWindow.SetFrameGridNeedsRebuilt();
                        }
                    }
                    else
                    {
                        newUse = true;
                    }

                    if (newUse)
                    {
                        GUILayout.Label("X:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                        newImageScale.x = EditorGUILayout.FloatField(keyframe.imageScale.val.x);
                        if (newImageScale.x != keyframe.imageScale.val.x)
                        {
                            if (AnimationWindow.Action == AnimationWindow.ACTION.None)
                                editor.SetWillBeDirty();

                            keyframe.imageScale.val.x = newImageScale.x;

                            MoveKey(keyframe, _currentBoneCurves.imageScaleXCurve, KeyframeSM.CURVE_PROPERTY.ImageScaleX);
                        }
                        if (_currentBoneCurves.imageScaleXCurve.keys.Length > 1)
                        {
                            CurveField(KeyframeSM.CURVE_PROPERTY.ImageScaleX);
                        }
                    }
                    else
                    {
                        GUILayout.Label("No Image Scale X Key", Style.normalLabelStyle);
                    }

                    GUILayout.EndHorizontal();
                    // End Image Scale X

                    // Image Scale Y
                    GUILayout.BeginHorizontal();

                    GUILayout.Space(5.0f);

                    if (_currentBoneCurves.imageScaleNeedsTwoKeys)
                    {
                        GUIContent guiContent = new GUIContent(Resources.warning, "You must supply at least two keyframes for imageScale x or y to generate an animation curve");
                        GUILayout.Label(guiContent, GUILayout.Width(Resources.warning.width), GUILayout.Height(Resources.warning.height));
                    }

                    if (keyframe.frame > 0)
                    {
                        newUse = GUILayout.Toggle(keyframe.imageScale.useY, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                        if (newUse != keyframe.imageScale.useY)
                        {
                            editor.SetWillBeDirty();

                            keyframe.imageScale.useY = newUse;
                            if (newUse)
                            {
                                keyframe.imageScale.val.y = _currentBoneCurves.imageScaleYCurve.Evaluate(time);
                                keyframe.imageScale.inTangentY = AnimationHelper.CalculateTangent(_currentBoneCurves.imageScaleYCurve, (float)keyframe.frame, keyframe.imageScale.val.y);
                                keyframe.imageScale.outTangentY = keyframe.imageScale.inTangentY;
                            }

                            //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                            //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
                            AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                            clip.SetMaxKeyframe();

                            TimelineWindow.SetFrameGridNeedsRebuilt();
                        }
                    }
                    else
                    {
                        newUse = true;
                    }

                    if (newUse)
                    {
                        GUILayout.Label("Y:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                        newImageScale.y = EditorGUILayout.FloatField(keyframe.imageScale.val.y);
                        if (newImageScale.y != keyframe.imageScale.val.y)
                        {
                            if (AnimationWindow.Action == AnimationWindow.ACTION.None)
                                editor.SetWillBeDirty();

                            keyframe.imageScale.val.y = newImageScale.y;

                            MoveKey(keyframe, _currentBoneCurves.imageScaleYCurve, KeyframeSM.CURVE_PROPERTY.ImageScaleY);
                        }
                        if (_currentBoneCurves.imageScaleYCurve.keys.Length > 1)
                        {
                            CurveField(KeyframeSM.CURVE_PROPERTY.ImageScaleY);
                        }
                    }
                    else
                    {
                        GUILayout.Label("No Image Scale Y Key", Style.normalLabelStyle);
                    }

                    GUILayout.EndHorizontal();
                    // End Image Scale Y

                    GUILayout.EndVertical();
                    // End Image Scale Group

                    GUILayout.Space(20.0f);
                }
            }
        }

        static public void DrawColorGroup(AnimationClipSM clip, KeyframeSM keyframe, float time)
        {
            // if this is not the root bone
            if (keyframe.boneDataIndex > 0 && LastKeyframe.keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
            {
                bool newUse;
                Color newColor;

                // Color Group
                GUILayout.BeginVertical();

                // Color
                GUILayout.BeginHorizontal();

                GUILayout.Space(5.0f);

                if (keyframe.frame > 0)
                {
                    newUse = GUILayout.Toggle(keyframe.color.use, GUIContent.none, GUILayout.Width(TOGGLE_WIDTH));
                    if (newUse != keyframe.color.use)
                    {
                        editor.SetWillBeDirty();

                        keyframe.color.use = newUse;
                        if (newUse)
                        {
                            Color c = new Color(_currentBoneCurves.colorRCurve.Evaluate(time),
                                                    _currentBoneCurves.colorGCurve.Evaluate(time),
                                                    _currentBoneCurves.colorBCurve.Evaluate(time),
                                                    _currentBoneCurves.colorACurve.Evaluate(time));

                            keyframe.color.val = c;

                            keyframe.color.inTangentR = AnimationHelper.CalculateTangent(_currentBoneCurves.colorRCurve, (float)keyframe.frame, keyframe.color.val.r);
                            keyframe.color.outTangentR = keyframe.color.inTangentR;

                            keyframe.color.inTangentG = AnimationHelper.CalculateTangent(_currentBoneCurves.colorGCurve, (float)keyframe.frame, keyframe.color.val.g);
                            keyframe.color.outTangentG = keyframe.color.inTangentG;

                            keyframe.color.inTangentB = AnimationHelper.CalculateTangent(_currentBoneCurves.colorBCurve, (float)keyframe.frame, keyframe.color.val.b);
                            keyframe.color.outTangentB = keyframe.color.inTangentR;

                            keyframe.color.inTangentA = AnimationHelper.CalculateTangent(_currentBoneCurves.colorACurve, (float)keyframe.frame, keyframe.color.val.a);
                            keyframe.color.outTangentA = keyframe.color.inTangentA;

                            keyframe.color.inTangentBlendWeight = AnimationHelper.CalculateTangent(_currentBoneCurves.colorBlendWeightCurve, (float)keyframe.frame, keyframe.color.blendWeight);
                            keyframe.color.outTangentBlendWeight = keyframe.color.inTangentBlendWeight;

                            keyframe.color.blendWeight = _currentBoneCurves.colorBlendWeightCurve.Evaluate(time);
                        }

                        //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
                        //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
                        AnimationHelper.refreshAnimationEditorWindowPostCycle = true;

                        clip.SetMaxKeyframe();

                        TimelineWindow.SetFrameGridNeedsRebuilt();
                    }
                }
                else
                {
                    newUse = true;
                }

                if (newUse)
                {
                    GUILayout.BeginVertical();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Color:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                    newColor = EditorGUILayout.ColorField(keyframe.color.val);
                    if (newColor != keyframe.color.val)
                    {
                        editor.SetWillBeDirty();

                        keyframe.color.val = newColor;

                        MoveKey(keyframe, _currentBoneCurves.colorRCurve, KeyframeSM.CURVE_PROPERTY.ColorR);
                        MoveKey(keyframe, _currentBoneCurves.colorGCurve, KeyframeSM.CURVE_PROPERTY.ColorG);
                        MoveKey(keyframe, _currentBoneCurves.colorBCurve, KeyframeSM.CURVE_PROPERTY.ColorB);
                        MoveKey(keyframe, _currentBoneCurves.colorACurve, KeyframeSM.CURVE_PROPERTY.ColorA);
                        MoveKey(keyframe, _currentBoneCurves.colorBlendWeightCurve, KeyframeSM.CURVE_PROPERTY.ColorBlend);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5.0f);

                    // Red
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Red:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                    float newRed = GUILayout.HorizontalSlider(newColor.r, 0, 1.0f, GUILayout.Width(SLIDER_WIDTH));
                    if (newRed != newColor.r)
                    {
                        editor.SetWillBeDirty();

                        newColor.r = newRed;
                        keyframe.color.val = newColor;

                        MoveKey(keyframe, _currentBoneCurves.colorRCurve, KeyframeSM.CURVE_PROPERTY.ColorR);
                    }
                    if (_currentBoneCurves.colorRCurve.keys.Length > 1)
                    {
                        CurveField(KeyframeSM.CURVE_PROPERTY.ColorR);
                    }
                    GUILayout.EndHorizontal();

                    // Green
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Green:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                    float newGreen = GUILayout.HorizontalSlider(newColor.g, 0, 1.0f, GUILayout.Width(SLIDER_WIDTH));
                    if (newGreen != newColor.g)
                    {
                        editor.SetWillBeDirty();

                        newColor.g = newGreen;
                        keyframe.color.val = newColor;

                        MoveKey(keyframe, _currentBoneCurves.colorGCurve, KeyframeSM.CURVE_PROPERTY.ColorG);
                    }
                    if (_currentBoneCurves.colorGCurve.keys.Length > 1)
                    {
                        CurveField(KeyframeSM.CURVE_PROPERTY.ColorG);
                    }
                    GUILayout.EndHorizontal();

                    // Blue
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Blue:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                    float newBlue = GUILayout.HorizontalSlider(newColor.b, 0, 1.0f, GUILayout.Width(SLIDER_WIDTH));
                    if (newBlue != newColor.b)
                    {
                        editor.SetWillBeDirty();

                        newColor.b = newBlue;
                        keyframe.color.val = newColor;

                        MoveKey(keyframe, _currentBoneCurves.colorBCurve, KeyframeSM.CURVE_PROPERTY.ColorB);
                    }
                    if (_currentBoneCurves.colorBCurve.keys.Length > 1)
                    {
                        CurveField(KeyframeSM.CURVE_PROPERTY.ColorB);
                    }
                    GUILayout.EndHorizontal();

                    // Alpha
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Alpha:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                    float newAlpha = GUILayout.HorizontalSlider(newColor.a, 0, 1.0f, GUILayout.Width(SLIDER_WIDTH));
                    if (newAlpha != newColor.a)
                    {
                        editor.SetWillBeDirty();

                        newColor.a = newAlpha;
                        keyframe.color.val = newColor;

                        MoveKey(keyframe, _currentBoneCurves.colorACurve, KeyframeSM.CURVE_PROPERTY.ColorA);
                    }
                    if (_currentBoneCurves.colorACurve.keys.Length > 1)
                    {
                        CurveField(KeyframeSM.CURVE_PROPERTY.ColorA);
                    }
                    GUILayout.EndHorizontal();

                    // Blend
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Blend:", Style.normalLabelStyle, GUILayout.Width(LABEL_WIDTH));
                    float newColorBlendingWeight = GUILayout.HorizontalSlider(keyframe.color.blendWeight, 0, 1.0f, GUILayout.Width(SLIDER_WIDTH));
                    if (newColorBlendingWeight != keyframe.color.blendWeight)
                    {
                        editor.SetWillBeDirty();

                        keyframe.color.blendWeight = newColorBlendingWeight;

                        MoveKey(keyframe, _currentBoneCurves.colorBlendWeightCurve, KeyframeSM.CURVE_PROPERTY.ColorBlend);
                    }
                    if (_currentBoneCurves.colorBlendWeightCurve.keys.Length > 1)
                    {
                        CurveField(KeyframeSM.CURVE_PROPERTY.ColorBlend);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(2.0f);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("None"))
                    {
                        if (keyframe.color.blendWeight != 0)
                        {
                            editor.SetWillBeDirty();

                            keyframe.color.blendWeight = 0;
                            MoveKey(keyframe, _currentBoneCurves.colorBlendWeightCurve, KeyframeSM.CURVE_PROPERTY.ColorBlend);
                        }
                    }
                    if (GUILayout.Button("Half / Half"))
                    {
                        if (keyframe.color.blendWeight != 0.5f)
                        {
                            editor.SetWillBeDirty();

                            keyframe.color.blendWeight = 0.5f;
                            MoveKey(keyframe, _currentBoneCurves.colorBlendWeightCurve, KeyframeSM.CURVE_PROPERTY.ColorBlend);
                        }
                    }
                    if (GUILayout.Button("Full"))
                    {
                        if (keyframe.color.blendWeight != 1.0f)
                        {
                            editor.SetWillBeDirty();

                            keyframe.color.blendWeight = 1.0f;
                            MoveKey(keyframe, _currentBoneCurves.colorBlendWeightCurve, KeyframeSM.CURVE_PROPERTY.ColorBlend);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.Label("No Color Key", Style.normalLabelStyle);
                    GUILayout.FlexibleSpace();
                }

                GUILayout.EndHorizontal();
                // End Color

                GUILayout.EndVertical();
                // End Color Group
            }

            GUILayout.Space(10.0f);
        }

		static private void MoveKey(KeyframeSM keyframe, AnimationCurve curve, KeyframeSM.CURVE_PROPERTY curveProperty)
		{
			Keyframe key;
			int keyIndex;
			
			keyIndex = AnimationHelper.GetKeyIndex(curve, (int)keyframe.frame);
			key = curve.keys[keyIndex];
			
			switch (curveProperty)
			{
				case KeyframeSM.CURVE_PROPERTY.LocalPositionX:
					key.value = keyframe.localPosition3.val.x;
					break;

				case KeyframeSM.CURVE_PROPERTY.LocalPositionY:
                    key.value = keyframe.localPosition3.val.y;
					break;

                case KeyframeSM.CURVE_PROPERTY.LocalPositionZ:
                    key.value = keyframe.localPosition3.val.z;
                    break;
                
                case KeyframeSM.CURVE_PROPERTY.LocalRotation:
					key.value = keyframe.localRotation.val;
					break;

                case KeyframeSM.CURVE_PROPERTY.LocalScaleX:
                    key.value = keyframe.localScale3.val.x;
                    break;

                case KeyframeSM.CURVE_PROPERTY.LocalScaleY:
                    key.value = keyframe.localScale3.val.y;
                    break;

                case KeyframeSM.CURVE_PROPERTY.LocalScaleZ:
                    key.value = keyframe.localScale3.val.z;
                    break;

				case KeyframeSM.CURVE_PROPERTY.ImageScaleX:
					key.value = keyframe.imageScale.val.x;
					break;

				case KeyframeSM.CURVE_PROPERTY.ImageScaleY:
					key.value = keyframe.imageScale.val.y;
					break;

                case KeyframeSM.CURVE_PROPERTY.ColorR:
                    key.value = keyframe.color.val.r;
                    break;

                case KeyframeSM.CURVE_PROPERTY.ColorG:
                    key.value = keyframe.color.val.g;
                    break;

                case KeyframeSM.CURVE_PROPERTY.ColorB:
                    key.value = keyframe.color.val.b;
                    break;

                case KeyframeSM.CURVE_PROPERTY.ColorA:
                    key.value = keyframe.color.val.a;
                    break;

                case KeyframeSM.CURVE_PROPERTY.ColorBlend:
                    key.value = keyframe.color.blendWeight;
                    break;

            }
			
			curve.MoveKey(keyIndex, key);
			
			AnimationHelper.AdjustLinearTangents(curve, _currentBone, curveProperty);
            //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
            //AnimationHelper.AddBoneDataIndexToRefreshList(keyframe.boneDataIndex);
            AnimationHelper.refreshAnimationEditorWindowPostCycle = true;
		}

        static private void CurveField(KeyframeSM.CURVE_PROPERTY curveProperty)
        {
            Event evt = Event.current;
            Rect newRect = GUILayoutUtility.GetRect(CURVE_FIELD_WIDTH, 15.0f);

            AnimationCurve oldCurve = AnimationHelper.GetAnimationCurveFromProperty(_currentBoneCurves, curveProperty);
            
            EditorGUIUtility.DrawCurveSwatch(newRect, oldCurve, null, Style.GetStyleBackgroundColor(Style.setValueStyle), Style.GetStyleBackgroundColor(Style.windowRectDarkBackgroundStyle));
			
            if (evt.type == EventType.MouseDown)
            {
                if (newRect.Contains(evt.mousePosition))
                {
                    _animationCurveEditor = AnimationCurveEditorWindow.ShowEditor();

                    if (_animationCurveEditor != null)
                    {
                        _currentCurveProperty = curveProperty;

                        _animationCurveEditor.SetAnimationCurve(ref oldCurve,
                                                                ClipBrowserWindow.SelectedAnimationClipIndex, 
                                                                _currentBone.boneDataIndex,
                                                                _currentCurveProperty, 
                                                                AnimationCurveEditorChanged, 
                                                                AnimationCurveEditorAddKey,
                                                                AnimationCurveEditorRemoveKey
                                                                );
                        _animationCurveEditor.Repaint();
                    }
                }
            }
        }

        static public void AnimationCurveEditorChanged(ref AnimationCurve animationCurve, Keyframe key)
        {
            editor.SetWillBeDirty();

            ChangeKeyframeProperty(ref _currentBoneCurves, ref _currentBone, ref animationCurve, key, _currentCurveProperty);

            TimelineWindow.SetFrameGridNeedsRebuilt();

            editor.Dirty();
            editor.Repaint();
        }

        static public void AnimationCurveEditorAddKey(ref AnimationCurve animationCurve, Keyframe key)
        {
            editor.SetWillBeDirty();

            AddKeyframe(ref _currentBoneCurves, ref _currentBone, ref animationCurve, key, _currentCurveProperty);

            TimelineWindow.SetFrameGridNeedsRebuilt();
            
            editor.Dirty();
            editor.Repaint();
        }

        static public void AnimationCurveEditorRemoveKey(ref AnimationCurve animationCurve, int keyIndex, int frame)
        {
            editor.SetWillBeDirty();

            RemoveKeyframe(ref _currentBoneCurves, ref _currentBone, ref animationCurve, keyIndex, frame, _currentCurveProperty);

            TimelineWindow.SetFrameGridNeedsRebuilt();
            
            editor.Dirty();
            editor.Repaint();
        }

        static private void ChangeKeyframeProperty(ref BoneCurves boneCurves, 
                                                    ref AnimationClipBone bone, 
                                                    ref AnimationCurve oldCurve, 
                                                    Keyframe key, 
                                                    KeyframeSM.CURVE_PROPERTY curveProperty)
        {
            KeyframeSM keyframe;

            keyframe = bone.GetKeyframe(Mathf.FloorToInt(key.time));
			
			if (keyframe != null)
			{
	            switch (curveProperty)
	            {
	                case KeyframeSM.CURVE_PROPERTY.LocalPositionX:
	                    keyframe.localPosition3.val.x = key.value;
                        keyframe.localPosition3.inTangentX = key.inTangent;
                        keyframe.localPosition3.outTangentX = key.outTangent;
                        keyframe.localPosition3.tangentModeX = key.tangentMode;
	                    break;
	
	                case KeyframeSM.CURVE_PROPERTY.LocalPositionY:
                        keyframe.localPosition3.val.y = key.value;
                        keyframe.localPosition3.inTangentY = key.inTangent;
                        keyframe.localPosition3.outTangentY = key.outTangent;
                        keyframe.localPosition3.tangentModeY = key.tangentMode;
	                    break;

                    case KeyframeSM.CURVE_PROPERTY.LocalPositionZ:
                        keyframe.localPosition3.val.z = key.value;
                        keyframe.localPosition3.inTangentZ = key.inTangent;
                        keyframe.localPosition3.outTangentZ = key.outTangent;
                        keyframe.localPosition3.tangentModeZ = key.tangentMode;
                        break;
                    
                    case KeyframeSM.CURVE_PROPERTY.LocalRotation:
	                    keyframe.localRotation.val = key.value;
	                    keyframe.localRotation.inTangent = key.inTangent;
	                    keyframe.localRotation.outTangent = key.outTangent;
	                    keyframe.localRotation.tangentMode = key.tangentMode;

                        AnimationHelper.BakeQuaternionCurves(ref oldCurve,
	                                                    ref boneCurves.localRotationXCurve,
	                                                    ref boneCurves.localRotationYCurve,
	                                                    ref boneCurves.localRotationZCurve,
	                                                    ref boneCurves.localRotationWCurve);
	                    break;

                    case KeyframeSM.CURVE_PROPERTY.LocalScaleX:
                        keyframe.localScale3.val.x = key.value;
                        keyframe.localScale3.inTangentX = key.inTangent;
                        keyframe.localScale3.outTangentX = key.outTangent;
                        keyframe.localScale3.tangentModeX = key.tangentMode;
                        break;

                    case KeyframeSM.CURVE_PROPERTY.LocalScaleY:
                        keyframe.localScale3.val.y = key.value;
                        keyframe.localScale3.inTangentY = key.inTangent;
                        keyframe.localScale3.outTangentY = key.outTangent;
                        keyframe.localScale3.tangentModeY = key.tangentMode;
                        break;

                    case KeyframeSM.CURVE_PROPERTY.LocalScaleZ:
                        keyframe.localScale3.val.z = key.value;
                        keyframe.localScale3.inTangentZ = key.inTangent;
                        keyframe.localScale3.outTangentZ = key.outTangent;
                        keyframe.localScale3.tangentModeZ = key.tangentMode;
                        break;
	
	                case KeyframeSM.CURVE_PROPERTY.ImageScaleX:
	                    keyframe.imageScale.val.x = key.value;
	                    keyframe.imageScale.inTangentX = key.inTangent;
	                    keyframe.imageScale.outTangentX = key.outTangent;
	                    keyframe.imageScale.tangentModeX = key.tangentMode;
	                    break;
	
	                case KeyframeSM.CURVE_PROPERTY.ImageScaleY:
	                    keyframe.imageScale.val.y = key.value;
	                    keyframe.imageScale.inTangentY = key.inTangent;
	                    keyframe.imageScale.outTangentY = key.outTangent;
	                    keyframe.imageScale.tangentModeY = key.tangentMode;
	                    break;

                    case KeyframeSM.CURVE_PROPERTY.ColorR:
                        keyframe.color.val.r = key.value;
                        keyframe.color.inTangentR = key.inTangent;
                        keyframe.color.outTangentR = key.outTangent;
                        keyframe.color.tangentModeR = key.tangentMode;
                        break;

                    case KeyframeSM.CURVE_PROPERTY.ColorG:
                        keyframe.color.val.g = key.value;
                        keyframe.color.inTangentG = key.inTangent;
                        keyframe.color.outTangentG = key.outTangent;
                        keyframe.color.tangentModeG = key.tangentMode;
                        break;

                    case KeyframeSM.CURVE_PROPERTY.ColorB:
                        keyframe.color.val.b = key.value;
                        keyframe.color.inTangentB = key.inTangent;
                        keyframe.color.outTangentB = key.outTangent;
                        keyframe.color.tangentModeB = key.tangentMode;
                        break;

                    case KeyframeSM.CURVE_PROPERTY.ColorA:
                        keyframe.color.val.a = key.value;
                        keyframe.color.inTangentA = key.inTangent;
                        keyframe.color.outTangentA = key.outTangent;
                        keyframe.color.tangentModeA = key.tangentMode;
                        break;

                    case KeyframeSM.CURVE_PROPERTY.ColorBlend:
                        keyframe.color.blendWeight = key.value;
                        keyframe.color.inTangentBlendWeight = key.inTangent;
                        keyframe.color.outTangentBlendWeight = key.outTangent;
                        keyframe.color.tangentModeBlendWeight = key.tangentMode;
                        break;
                }
			}
        }

        static private void AddKeyframe(ref BoneCurves boneCurves, ref AnimationClipBone bone, ref AnimationCurve oldCurve, Keyframe key, KeyframeSM.CURVE_PROPERTY curveProperty)
        {
            int newFrame;

            newFrame = (int)key.time;
            AddKeyframe(ref bone, newFrame, key.value, key.inTangent, key.outTangent, curveProperty);

            switch (curveProperty)
            {
                case KeyframeSM.CURVE_PROPERTY.LocalRotation:
                    AnimationHelper.BakeQuaternionCurves(ref oldCurve,
                                                                        ref boneCurves.localRotationXCurve,
                                                                        ref boneCurves.localRotationYCurve,
                                                                        ref boneCurves.localRotationZCurve,
                                                                        ref boneCurves.localRotationWCurve);
                    break;
            }
        }

        static private void AddKeyframe(ref AnimationClipBone bone, int newFrame, float value, float inTan, float outTan, KeyframeSM.CURVE_PROPERTY curveProperty)
        {
            KeyframeSM keyframe;

            keyframe = bone.GetKeyframe(newFrame);
            if (keyframe == null)
            {
                keyframe = bone.AddKeyframe(bone.boneDataIndex, newFrame, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.Blank);
            }

            switch (curveProperty)
            {
                case KeyframeSM.CURVE_PROPERTY.LocalPositionX:
                    keyframe.localPosition3.useX = true;
                    keyframe.localPosition3.val.x = value;
                    keyframe.localPosition3.inTangentX = inTan;
                    keyframe.localPosition3.outTangentX = outTan;
                    break;

                case KeyframeSM.CURVE_PROPERTY.LocalPositionY:
                    keyframe.localPosition3.useY = true;
                    keyframe.localPosition3.val.y = value;
                    keyframe.localPosition3.inTangentY = inTan;
                    keyframe.localPosition3.outTangentY = outTan;
                    break;

                case KeyframeSM.CURVE_PROPERTY.LocalPositionZ:
                    keyframe.localPosition3.useZ = true;
                    keyframe.localPosition3.val.z = value;
                    keyframe.localPosition3.inTangentZ = inTan;
                    keyframe.localPosition3.outTangentZ = outTan;
                    break;
                
                case KeyframeSM.CURVE_PROPERTY.LocalRotation:
                    keyframe.localRotation.use = true;
                    keyframe.localRotation.val = value;
                    keyframe.localRotation.inTangent = inTan;
                    keyframe.localRotation.outTangent = outTan;
                    break;

                case KeyframeSM.CURVE_PROPERTY.LocalScaleX:
                    keyframe.localScale3.useX = true;
                    keyframe.localScale3.val.x = value;
                    keyframe.localScale3.inTangentX = inTan;
                    keyframe.localScale3.outTangentX = outTan;
                    break;

                case KeyframeSM.CURVE_PROPERTY.LocalScaleY:
                    keyframe.localScale3.useY = true;
                    keyframe.localScale3.val.y = value;
                    keyframe.localScale3.inTangentY = inTan;
                    keyframe.localScale3.outTangentY = outTan;
                    break;

                case KeyframeSM.CURVE_PROPERTY.LocalScaleZ:
                    keyframe.localScale3.useZ = true;
                    keyframe.localScale3.val.z = value;
                    keyframe.localScale3.inTangentZ = inTan;
                    keyframe.localScale3.outTangentZ = outTan;
                    break;

                case KeyframeSM.CURVE_PROPERTY.ImageScaleX:
                    keyframe.imageScale.useX = true;
                    keyframe.imageScale.val.x = value;
                    keyframe.imageScale.inTangentX = inTan;
                    keyframe.imageScale.outTangentX = outTan;
                    break;

                case KeyframeSM.CURVE_PROPERTY.ImageScaleY:
                    keyframe.imageScale.useY = true;
                    keyframe.imageScale.val.y = value;
                    keyframe.imageScale.inTangentY = inTan;
                    keyframe.imageScale.outTangentY = outTan;
                    break;

                case KeyframeSM.CURVE_PROPERTY.ColorR:
                    keyframe.color.use = true;
                    keyframe.color.val.r = value;
                    keyframe.color.inTangentR = inTan;
                    keyframe.color.outTangentR = outTan;
                    break;

                case KeyframeSM.CURVE_PROPERTY.ColorG:
                    keyframe.color.use = true;
                    keyframe.color.val.g = value;
                    keyframe.color.inTangentG = inTan;
                    keyframe.color.outTangentG = outTan;
                    break;

                case KeyframeSM.CURVE_PROPERTY.ColorB:
                    keyframe.color.use = true;
                    keyframe.color.val.b = value;
                    keyframe.color.inTangentB = inTan;
                    keyframe.color.outTangentB = outTan;
                    break;

                case KeyframeSM.CURVE_PROPERTY.ColorA:
                    keyframe.color.use = true;
                    keyframe.color.val.a = value;
                    keyframe.color.inTangentA = inTan;
                    keyframe.color.outTangentA = outTan;
                    break;

                case KeyframeSM.CURVE_PROPERTY.ColorBlend:
                    keyframe.color.use = true;
                    keyframe.color.blendWeight = value;
                    keyframe.color.inTangentBlendWeight = inTan;
                    keyframe.color.outTangentBlendWeight = outTan;
                    break;
            }

            ClipBrowserWindow.CurrentClip.SetMaxKeyframe();
        }

        static private void RemoveKeyframe(ref BoneCurves boneCurves, ref AnimationClipBone bone, ref AnimationCurve oldCurve, int keyIndex, int oldFrame, KeyframeSM.CURVE_PROPERTY curveProperty)
        {
            RemoveKeyframe(ref bone, oldFrame, curveProperty);

            switch (curveProperty)
            {
                case KeyframeSM.CURVE_PROPERTY.LocalRotation:
                    boneCurves.localRotationXCurve.RemoveKey(keyIndex);
                    boneCurves.localRotationYCurve.RemoveKey(keyIndex);
                    boneCurves.localRotationZCurve.RemoveKey(keyIndex);
                    boneCurves.localRotationWCurve.RemoveKey(keyIndex);
                    break;
            }
        }

        static private void RemoveKeyframe(ref AnimationClipBone bone, int oldFrame, KeyframeSM.CURVE_PROPERTY curveProperty)
        {
            KeyframeSM keyframe;

            keyframe = bone.GetKeyframe(oldFrame);
			
			if (keyframe != null)
			{
	            switch (curveProperty)
	            {
	                case KeyframeSM.CURVE_PROPERTY.LocalPositionX:
	                    keyframe.localPosition3.useX = false;
                        keyframe.localPosition3.val.x = 0;
	                    break;
	
	                case KeyframeSM.CURVE_PROPERTY.LocalPositionY:
                        keyframe.localPosition3.useY = false;
                        keyframe.localPosition3.val.y = 0;
	                    break;

                    case KeyframeSM.CURVE_PROPERTY.LocalPositionZ:
                        keyframe.localPosition3.useZ = false;
                        keyframe.localPosition3.val.z = 0;
                        break;
                    
                    case KeyframeSM.CURVE_PROPERTY.LocalRotation:
	                    keyframe.localRotation.use = false;
	                    keyframe.localRotation.val = 0;
	                    break;

                    case KeyframeSM.CURVE_PROPERTY.LocalScaleX:
                        keyframe.localScale3.useX = false;
                        keyframe.localScale3.val.x = 0;
                        break;

                    case KeyframeSM.CURVE_PROPERTY.LocalScaleY:
                        keyframe.localScale3.useY = false;
                        keyframe.localScale3.val.y = 0;
                        break;

                    case KeyframeSM.CURVE_PROPERTY.LocalScaleZ:
                        keyframe.localScale3.useZ = false;
                        keyframe.localScale3.val.z = 0;
                        break;
	
	                case KeyframeSM.CURVE_PROPERTY.ImageScaleX:
	                    keyframe.imageScale.useX = false;
	                    keyframe.imageScale.val.x = 0;
	                    break;
	
	                case KeyframeSM.CURVE_PROPERTY.ImageScaleY:
	                    keyframe.imageScale.useY = false;
	                    keyframe.imageScale.val.y = 0;
	                    break;
	
                    case KeyframeSM.CURVE_PROPERTY.ColorR:
                        keyframe.color.use = false;
                        keyframe.color.val.r = 0;
                        break;

                    case KeyframeSM.CURVE_PROPERTY.ColorG:
                        keyframe.color.use = false;
                        keyframe.color.val.g = 0;
                        break;

                    case KeyframeSM.CURVE_PROPERTY.ColorB:
                        keyframe.color.use = false;
                        keyframe.color.val.b = 0;
                        break;

                    case KeyframeSM.CURVE_PROPERTY.ColorA:
                        keyframe.color.use = false;
                        keyframe.color.val.a = 0;
                        break;

                    case KeyframeSM.CURVE_PROPERTY.ColorBlend:
                        keyframe.color.use = false;
                        keyframe.color.blendWeight = 0;
                        break;
	            }
	
	            if (!keyframe.BeingUsed)
	            {
	                bone.RemoveKeyframe(oldFrame);
	                ClipBrowserWindow.CurrentClip.SetMaxKeyframe();
	            }
			}
        }

        static public void GetInput(Event evt)
        {
            if (evt.type == EventType.MouseDown && _areaRect.Contains(evt.mousePosition))
            {
                editor.KeyboardFocus = BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.BoneProperties;
            }
        }

        static public void LostFocus()
        {
        }
    }
}
