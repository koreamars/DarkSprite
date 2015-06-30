using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SmoothMoves
{
    class SetAtlasesWindow
    {
        public const float SET_ATLASES_WINDOW_WIDTH = 300.0f;
        public const float SET_ATLASES_WINDOW_HEIGHT = 150.0f;

        static private bool _toggleKeyframeType;
        static private KeyframeSM.KEYFRAME_TYPE _keyframeType;

        static private bool _toggleAtlas;
        static private TextureAtlas _atlas;

        static private Rect _areaRect;
        static private bool _visible;

        static private bool _atlasListBuilt = false;
        static private int _selectedAtlasIndex = -1;
        static private List<TextureAtlas> _atlasList;
        static private List<string> _atlasNameList;
        static private List<string> _atlasGUIDList;

        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }
        static public Rect AreaRect { get { return _areaRect; } set { _areaRect = value; } }
        static public bool Visible { get { return _visible; } set { _visible = value; } }
        static public float WindowWidth
        {
            get
            {
                return SET_ATLASES_WINDOW_WIDTH;
            }
        }

        static public float WindowHeight
        {
            get
            {
                return SET_ATLASES_WINDOW_HEIGHT;
            }
        }

        static public void Reset()
        {
            _toggleKeyframeType = false;
            _keyframeType = KeyframeSM.KEYFRAME_TYPE.TransformOnly;

            _toggleAtlas = false;
            _atlas = null;
        }

        static public void OnEnable()
        {
            _toggleKeyframeType = false;
            _toggleAtlas = false;
            _atlas = null;
            _atlasListBuilt = false;
        }

        static public void OnGUI()
        {
            if (_visible && TimelineWindow.MultipleKeyframesSelected)
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

                GUILayout.Label("Set Atlases", Style.centeredTextStyle, GUILayout.Width(_areaRect.width - 40.0f));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", GUILayout.Width(30.0f)))
                {
                    _visible = false;
                    editor.SetNeedsRepainted();
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(20.0f);

                GUILayout.BeginHorizontal();
                _toggleKeyframeType = GUILayout.Toggle(_toggleKeyframeType, "Keyframe Type:", Style.normalToggleStyle);
                if (_toggleKeyframeType)
                {
                    _keyframeType = (KeyframeSM.KEYFRAME_TYPE)EditorGUILayout.EnumPopup(_keyframeType);
                }
                GUILayout.EndHorizontal();

                if (_keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                {
                    GUILayout.BeginHorizontal();
                    _toggleAtlas = GUILayout.Toggle(_toggleAtlas, "Atlas:", Style.normalToggleStyle);
                    if (_toggleAtlas)
                    {
                        if (!_atlasListBuilt)
                            GetAtlasLists(_atlas);
                        else
                            SetSelectedAtlasIndex(_atlas);

                        int newAtlasIndex = EditorGUILayout.Popup(_selectedAtlasIndex, _atlasNameList.ToArray());
                        if (newAtlasIndex != _selectedAtlasIndex)
                        {
                            _selectedAtlasIndex = newAtlasIndex;
                            _atlas = _atlasList[_selectedAtlasIndex];
                        }
                        //_atlas = (TextureAtlas)EditorGUILayout.ObjectField(_atlas, typeof(TextureAtlas), false);
                    }
                    else
                    {
                        _atlas = null;
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Update Selected Keyframes", GUILayout.Width(200.0f)))
                {
                    editor.SetWillBeDirty(); ;
                    
                    KeyframeSM keyframe;
                    for (int keyframeIndex = 0; keyframeIndex < TimelineWindow.SelectedKeyframeCount; keyframeIndex++)
                    {
                        keyframe = TimelineWindow.GetSelectedKeyframeAtIndex(keyframeIndex);

                        if (_toggleKeyframeType)
                        {
                            keyframe.useKeyframeType = true;
                            keyframe.keyframeType = _keyframeType;

                            if (keyframe.keyframeType == KeyframeSM.KEYFRAME_TYPE.TransformOnly)
                            {
                                keyframe.Reset(AnimationClipBone.DEFAULT_SETTING.All);
                            }
                        }

                        if (_toggleAtlas && keyframe.keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                        {
                            keyframe.useAtlas = true;
                            keyframe.atlas = _atlas;
                        }
                    }

                    _visible = false;
                    TimelineWindow.SetFrameGridNeedsRebuilt();
                    editor.SetNeedsRepainted();

                    TextureManager.GenerateTextureDictionary(true);
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.EndArea();
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

        static public void GetInput(Event evt)
        {

        }
    }
}
