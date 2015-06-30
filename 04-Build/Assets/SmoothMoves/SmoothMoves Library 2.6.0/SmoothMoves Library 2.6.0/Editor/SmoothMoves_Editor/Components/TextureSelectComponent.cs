using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace SmoothMoves
{
    public delegate void AtlasChangedDelegate(TextureSelectComponent source, TextureAtlas newAtlas);
    public delegate void TextureChangedDelegate(TextureSelectComponent source, string newTextureGUID);
    public delegate void PivotOffsetChangedDelegate(TextureSelectComponent source, Vector2 newPivotOffset, bool useDefaultPivot);

    public class TextureSelectComponent
    {
        private const float TEXTURE_FORCE_SIZE = 60.0f;

        public int _selectedAtlasIndex = -1;
        public List<TextureAtlas> _atlasList;
        public List<string> _atlasNameList;
        public List<string> _atlasGUIDList;

        private Texture2D _texture;
        private AtlasChangedDelegate _atlasChangedDelegate;
        private TextureChangedDelegate _textureChangedDelegate;
        private PivotOffsetChangedDelegate _pivotOffsetChangedDelegate;
        //private TextureAtlasEditorWindow _atlasEditor;
        private TextureSelectComponentPivotEditorWindow _pivotEditor;
        private List<string> _textureGUIDs;
        private List<string> _textureNames;
        private int _selectedTextureGUIDIndex;

        public int index;
        public GameObject sourceObject;
        public TextureAtlas atlas;
        public string textureGUID;
        public bool showPivotOffset;
        public Vector2 pivotOffset;
        public bool useDefaultPivot;

        public TextureSelectComponent()
        {
            index = 0;
            showPivotOffset = true;

            useDefaultPivot = true;
        }

        public TextureSelectComponent(int idx, bool showPivot)
        {
            index = idx;
            showPivotOffset = showPivot;

            useDefaultPivot = true;
        }

        public void OnEnable()
        {
            BuildLists(atlas);
            _selectedTextureGUIDIndex = GetTextureGUIDIndex(textureGUID);
            SetAtlas(atlas);

            GetAtlasList();
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical();

            //GetAtlasList();
            if (_atlasGUIDList != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(" Atlas:");
                int newAtlasIndex = EditorGUILayout.Popup(_selectedAtlasIndex, _atlasNameList.ToArray());
                if (newAtlasIndex != _selectedAtlasIndex)
                {
                    atlas = _atlasList[newAtlasIndex];
                    _selectedAtlasIndex = newAtlasIndex;

                    SetAtlas(atlas);
                }
                GUILayout.EndHorizontal();
            }

            //GUILayout.BeginHorizontal();
            //GUILayout.Label(" Texture Atlas:");
            //TextureAtlas newAtlas = (TextureAtlas)EditorGUILayout.ObjectField(atlas, typeof(TextureAtlas), false);
            //if (newAtlas != atlas)
            //{
            //    SetAtlas(newAtlas);
            //}
            //GUILayout.EndHorizontal();

            if (_textureNames != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(" Texture:");
                int newTextureGUIDIndex = EditorGUILayout.Popup(_selectedTextureGUIDIndex, _textureNames.ToArray());
                if (newTextureGUIDIndex != _selectedTextureGUIDIndex)
                {
                    SetTextureGUIDIndex(newTextureGUIDIndex);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(_texture, GUILayout.Width(TEXTURE_FORCE_SIZE), GUILayout.Height(TEXTURE_FORCE_SIZE));
            GUILayout.Space(10.0f);
            if (showPivotOffset)
            {
                GUILayout.BeginVertical();

                //bool newUsePivot = GUILayout.Toggle(usePivot, "Use Pivot", GUILayout.Height(20.0f));
                //if (newUsePivot != usePivot)
                //{
                //    usePivot = newUsePivot;
                //    SetPivotOffset(usePivot, pivotOffset);
                //}

                //if (usePivot)
                //{
                if (GUILayout.Button(Resources.pivotButton, GUILayout.Width(TEXTURE_FORCE_SIZE), GUILayout.Height(TEXTURE_FORCE_SIZE)))
                {
                    if (TextureSelectComponentPivotEditorWindow.Instance == null)
                        TextureSelectComponentPivotEditorWindow.ShowEditor();

                    TextureSelectComponentPivotEditorWindow.Instance.SetTextureSelectComponent(this);
                }
                //}
                //else
                //{
                //    GUILayout.FlexibleSpace();
                //}

                GUILayout.EndVertical();

            }

            //if (GUILayout.Button(Resources.pivotButton, GUILayout.Width(TEXTURE_FORCE_SIZE), GUILayout.Height(TEXTURE_FORCE_SIZE)))
            //{
            //    if (TextureSelectComponentPivotEditorWindow.Instance == null)
            //        TextureSelectComponentPivotEditorWindow.ShowEditor();

            //    TextureSelectComponentPivotEditorWindow.Instance.SetTextureSelectComponent(this);
            //}
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void GetAtlasList()
        {
            DirectoryInfo di;
            FileInfo[] allFiles;
            int assetPathIndex;
            string assetPath;
            TextureAtlas atlas;

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
                    atlas = (TextureAtlas)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextureAtlas));

                    if (atlas != null)
                    {
                        _atlasList.Add(atlas);
                        _atlasNameList.Add(atlas.name);
                        _atlasGUIDList.Add(AssetDatabase.AssetPathToGUID(assetPath));
                    }
                }

            }
        }

        //public void OnGUI()
        //{
        //GUILayout.BeginVertical();

        //GUILayout.BeginVertical(Style.antiSelectionStyle);

        //GUILayout.BeginHorizontal();

        //GUILayout.Label(" Texture Atlas ");
        //TextureAtlas newAtlas = (TextureAtlas)EditorGUILayout.ObjectField(atlas, typeof(TextureAtlas), false);
        //if (newAtlas != atlas)
        //{
        //    SetAtlas(newAtlas);
        //}
        //GUILayout.EndHorizontal();

        //if (textureGUID != _lastTextureGUID)
        //{
        //    if (textureGUID != "")
        //    {
        //        _texture = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(textureGUID), typeof(Texture2D));
        //    }
        //    else
        //    {
        //        _texture = null;
        //    }
        //}

        //GUILayout.BeginHorizontal();
        //GUILayout.FlexibleSpace();
        //if (GUILayout.Button(_texture, GUILayout.Width(TEXTURE_FORCE_SIZE), GUILayout.Height(TEXTURE_FORCE_SIZE)))
        //{
        //    GetAtlasEditor().SetTextureSelectComponent(this);
        //}

        //if (showPivotOffset)
        //{
        //    GUILayout.BeginVertical();

        //    //bool newUsePivot = GUILayout.Toggle(usePivot, "Use Pivot", GUILayout.Height(20.0f));
        //    //if (newUsePivot != usePivot)
        //    //{
        //    //    usePivot = newUsePivot;
        //    //    SetPivotOffset(usePivot, pivotOffset);
        //    //}

        //    //if (usePivot)
        //    //{
        //        if (GUILayout.Button(Resources.pivotButton, GUILayout.Width(TEXTURE_FORCE_SIZE), GUILayout.Height(TEXTURE_FORCE_SIZE)))
        //        {
        //            GetPivotEditor().SetTextureSelectComponent(this);
        //        }
        //    //}
        //    //else
        //    //{
        //    //    GUILayout.FlexibleSpace();
        //    //}

        //    GUILayout.EndVertical();

        //}
        //GUILayout.Space(20.0f);
        //GUILayout.EndHorizontal();

        //_lastTextureGUID = textureGUID;

        //GUILayout.EndVertical();

        //GUILayout.Space(5.0f);

        //GUILayout.EndVertical();
        //}

        public void Initialize()
        {
            GetAtlasList();

            if (_atlasList != null)
            {
                _selectedAtlasIndex = _atlasList.IndexOf(atlas);
            }
        }

        public void SetAtlasChangedDelegate(GameObject source, AtlasChangedDelegate atlasChangedDelegate)
        {
            sourceObject = source;
            _atlasChangedDelegate = atlasChangedDelegate;
        }

        public void SetTextureChangedDelegate(TextureChangedDelegate textureChangedDelegate)
        {
            _textureChangedDelegate = textureChangedDelegate;
        }

        public void SetPivotOffsetChangedDelegate(PivotOffsetChangedDelegate pivotOffsetChangedDelegate)
        {
            _pivotOffsetChangedDelegate = pivotOffsetChangedDelegate;
        }

        //public void SetAtlas(TextureAtlas newAtlas)
        //{
        //    atlas = newAtlas;

        //    if (_atlasChangedDelegate != null)
        //        _atlasChangedDelegate(this, atlas);

        //    if (TextureAtlasEditorWindow.Instance != null)
        //        TextureAtlasEditorWindow.Instance.SetTextureSelectComponent(this);

        //    //if (TexturePivotEditorWindow.Instance != null)
        //    //    TexturePivotEditorWindow.Instance.SetTextureSelectComponent(this);

        //    if (atlas != null)
        //    {
        //        if (!atlas.textureGUIDs.Contains(textureGUID))
        //        {
        //            _texture = null;
        //        }
        //    }
        //}

        //public void SetTextureGUID(string guid)
        //{
        //    textureGUID = guid;
        //    if (textureGUID != _lastTextureGUID)
        //    {
        //        if (textureGUID != "")
        //        {
        //            _texture = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(textureGUID), typeof(Texture2D));
        //        }
        //        else
        //        {
        //            _texture = null;
        //        }

        //        if (_textureChangedDelegate != null)
        //            _textureChangedDelegate(this, textureGUID);
        //    }

        //    _lastTextureGUID = textureGUID;

        //    if (TextureAtlasEditorWindow.Instance != null)
        //        TextureAtlasEditorWindow.Instance.SetTextureSelectComponent(this);

        //    //if (TexturePivotEditorWindow.Instance != null)
        //    //    TexturePivotEditorWindow.Instance.SetTextureSelectComponent(this);
        //}

        public void BuildLists(TextureAtlas atlas)
        {
            if (atlas == null)
                return;

            if (_textureGUIDs == null)
                _textureGUIDs = new List<string>();
            else
                _textureGUIDs.Clear();

            if (_textureNames == null)
                _textureNames = new List<string>();
            else
                _textureNames.Clear();

            SortedDictionary<string, string> sortedTextures = new SortedDictionary<string, string>();
            int textureIndex;

            for (textureIndex = 0; textureIndex < atlas.textureGUIDs.Count; textureIndex++)
            {
                sortedTextures.Add(TextureManager.GetAssetNameFromPath(AssetDatabase.GUIDToAssetPath(atlas.textureGUIDs[textureIndex]), false), atlas.textureGUIDs[textureIndex]);
            }

            foreach (KeyValuePair<string, string> kvp in sortedTextures)
            {
                _textureNames.Add(kvp.Key);
                _textureGUIDs.Add(kvp.Value);
            }
        }

        public void SetAtlas(TextureAtlas newAtlas)
        {
            if (newAtlas != null)
            {
                BuildLists(newAtlas);

                if (_selectedTextureGUIDIndex == -1)
                {
                    if (newAtlas.textureGUIDs.Count > 0)
                        _selectedTextureGUIDIndex = 0;
                    else
                        return;
                }

                try
                {
                    textureGUID = _textureGUIDs[_selectedTextureGUIDIndex];
                }
                catch
                {
                    _selectedTextureGUIDIndex = 0;
                    textureGUID = _textureGUIDs[_selectedTextureGUIDIndex];
                }

                if (!_textureGUIDs.Contains(textureGUID))
                {
                    _texture = null;
                }
                else
                {
                    _texture = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(textureGUID), typeof(Texture2D));

                    _selectedTextureGUIDIndex = GetTextureGUIDIndex(textureGUID);

                    atlas = newAtlas;

                    if (TextureSelectComponentPivotEditorWindow.Instance != null)
                    {
                        TextureSelectComponentPivotEditorWindow.Instance.SetTextureSelectComponent(this);
                    }

                    if (_atlasChangedDelegate != null)
                        _atlasChangedDelegate(this, atlas);
                }
            }
            else
            {
                _texture = null;

                if (_textureGUIDs != null)
                    _textureGUIDs.Clear();
            }
        }

        private int GetTextureGUIDIndex(string guid)
        {
            if (_textureGUIDs == null)
                return -1;

            return _textureGUIDs.IndexOf(guid);
        }

        public void SetTextureGUID(string guid)
        {
            SetTextureGUIDIndex(GetTextureGUIDIndex(guid));
        }

        private void SetTextureGUIDIndex(int guidIndex)
        {
            if (_textureGUIDs != null)
            {
                if (guidIndex > -1 && guidIndex < _textureGUIDs.Count)
                {
                    _selectedTextureGUIDIndex = guidIndex;
                    textureGUID = _textureGUIDs[_selectedTextureGUIDIndex];

                    if (!_textureGUIDs.Contains(textureGUID))
                    {
                        _texture = null;
                    }
                    else
                    {
                        _texture = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(textureGUID), typeof(Texture2D));

                        if (TextureSelectComponentPivotEditorWindow.Instance != null)
                        {
                            TextureSelectComponentPivotEditorWindow.Instance.SetTextureSelectComponent(this);
                        }

                        if (_textureChangedDelegate != null)
                            _textureChangedDelegate(this, textureGUID);
                    }
                }
                else
                {
                    _selectedTextureGUIDIndex = 0;
                    textureGUID = "";
                    _texture = null;
                }
            }
            else
            {
                _selectedTextureGUIDIndex = 0;
                textureGUID = "";
                _texture = null;
            }
        }

        public void SetPivotOffset(Vector2 pivot, bool useDefault)
        {
            pivotOffset = pivot;
            useDefaultPivot = useDefault;

            //if (useDefaultPivot)
            //{
            //    if (atlas != null)
            //    {
            //        pivotOffset = atlas.LookupDefaultPivotOffset(textureGUID);
            //    }
            //    else
            //    {
            //        pivotOffset = Vector2.zero;
            //    }
            //}

            if (_pivotOffsetChangedDelegate != null)
                _pivotOffsetChangedDelegate(this, pivotOffset, useDefault);

            //if (TexturePivotEditorWindow.Instance != null)
            //    TexturePivotEditorWindow.Instance.SetTextureSelectComponent(this);
        }

        //private TextureAtlasEditorWindow GetAtlasEditor()
        //{
        //    if (_atlasEditor == null)
        //        _atlasEditor = TextureAtlasEditorWindow.ShowEditor();

        //    return _atlasEditor;
        //}

        private TextureSelectComponentPivotEditorWindow GetPivotEditor()
        {
            if (_pivotEditor == null)
                _pivotEditor = TextureSelectComponentPivotEditorWindow.ShowEditor();

            return _pivotEditor;
        }
    }
}
