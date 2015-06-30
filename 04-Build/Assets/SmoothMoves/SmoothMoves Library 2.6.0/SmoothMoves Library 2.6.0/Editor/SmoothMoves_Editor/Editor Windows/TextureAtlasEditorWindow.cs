using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace SmoothMoves
{
    public class TextureAtlasEditorWindow : EditorWindow
    {        
        static private TextureAtlasEditorWindow _instance;

        private enum ATLAS_SIZE
        {
            Size32 = 32,
            Size64 = 64,
            Size128 = 128,
            Size256 = 256,
            Size512 = 512,
            Size1024 = 1024,
            Size2048 = 2048,
            Size4096 = 4096
        }

        private const float TEXTURE_EDGE_BUFFER = 5.0f;
        private const float SPACE_BETWEEN_TEXTURES = 5.0f;
        private const double UPDATE_INTERVAL = 1.0;
        private const float TEXTURE_PIVOT_WINDOW_WIDTH = 300.0f;
        private const int MAX_TEXTURE_SIZE = 4096;

        private Object _textureAtlasObject;
        private TextureAtlas _textureAtlas;

        private List<DisplayTexture> _displayTextures;
        private List<string> _textureGUIDs;
        private DisplayTexture _selectedTexture;
        private TexturePivotWindow _texturePivotWindow;
        private BoneAnimationDataEditorWindow _boneAnimationDataEditorWindow;

        private bool _needsRepainted;
        private bool _isDirty;
        private bool _needsRebuilt;

        private float _zoomFactor;

        private Rect _texturesRect;
        private Rect _texturePivotRect;
        private Rect _texturesScrollRect;
        private Vector2 _scrollPosition;

        private string _textureGUID;

        public string TextureGUID
        {
            get
            {
                return _textureGUID;
            }
        }

        private bool ShowPivots
        {
            get
            {
                return (PlayerPrefs.GetInt("SMAtlas.ShowPivots", 0) == 1);
            }
            set
            {
                PlayerPrefs.SetInt("SMAtlas.ShowPivots", (value ? 1 : 0));
            }
        }

        static public TextureAtlasEditorWindow Instance
        {
            get
            {

                return _instance;
            }
        }

        static public TextureAtlasEditorWindow ShowEditor()
        {
            if (_instance == null)
            {
                _instance = (TextureAtlasEditorWindow)EditorWindow.GetWindow(typeof(TextureAtlasEditorWindow), true, "Atlas Editor");
                _instance.ShowUtility();
            }

            _instance.ShowUtility();

            Resources.LoadTextures();

            return _instance;
        }

        void OnEnable()
        {
            if (_displayTextures == null)
                _displayTextures = new List<DisplayTexture>();
            else
                _displayTextures.Clear();

            if (_textureGUIDs == null)
                _textureGUIDs = new List<string>();
            else
                _textureGUIDs.Clear();

            _selectedTexture = null;

            _needsRepainted = false;
            _isDirty = false;
            _needsRebuilt = false;
            //_lastTime = 0;

            _zoomFactor = 1f;

            _texturePivotWindow = new TexturePivotWindow();

            _texturePivotWindow.OnEnable();

            Resources.LoadTextures();
        }

        void OnGUI()
        {
            Style.Reset();

            Style.OnGUI();

            if (_textureAtlas == null)
            {
                GUILayout.Space(20.0f);
                GUILayout.Label("No atlas selected", Style.centeredTextStyle);
                return;
            }

            SetRects();
            CreateGUI();
            HandleInput();

            if (_isDirty)
            {
                if (_textureAtlasObject != null)
                {
                    EditorUtility.SetDirty(_textureAtlasObject);
                }

                _isDirty = false;
            }

            if (_needsRepainted)
            {
                Repaint();
                _needsRepainted = false;
            }
        }

        private void SetRects()
        {
            _texturesRect = new Rect(10.0f, 105.0f, position.width - TEXTURE_PIVOT_WINDOW_WIDTH - 20.0f, position.height - 135.0f);
            _texturePivotRect = new Rect(_texturesRect.xMax, _texturesRect.yMin, TEXTURE_PIVOT_WINDOW_WIDTH, position.height - _texturesRect.yMin - 30.0f);
        }

        public void SetTextureAtlasAsset(Object obj)
        {
            _textureAtlasObject = obj;
            _textureAtlas = (TextureAtlas)_textureAtlasObject;

            _textureAtlas.UpdateDataVersion();

            this.title = "Atlas Editor - " + obj.name;

            if (_textureAtlas == null)
            {
                _textureAtlasObject = null;
                ClearDisplayTextures();
            }

            ClearDisplayTextures();

            _needsRebuilt = false;

            if (_textureAtlas != null)
            {
                RedisplayTextures();
                Redisplay();
                SetSelectedTexture();
            }

            _needsRepainted = true;
        }

        private void CreateGUI()
        {
            Resources.LoadTextures();

            if (!_needsRebuilt)
            {
                _needsRebuilt = CheckNeedsRebuilt();
            }

            GUIHelper.DrawBox(new Rect(10.0f, 3, position.width - 20.0f, 20), Style.selectionDoneStyle, true);
            GUIHelper.DrawBox(new Rect(10.0f, 25, position.width - 20.0f, 63), Style.windowRectBackgroundStyle, true);

            

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0f);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label(_textureAtlas.name, Style.blankStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(3.0f);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Padding", Style.normalLabelStyle, GUILayout.Width(100.0f));
            int newAtlasPadding = EditorGUILayout.IntField(_textureAtlas.padding, GUILayout.Width(100.0f));
            if (newAtlasPadding != _textureAtlas.padding)
            {
                _textureAtlas.padding = newAtlasPadding;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Atlas Size", Style.normalLabelStyle, GUILayout.Width(100.0f));
            int newMaxAtlasSize = (int)((ATLAS_SIZE)EditorGUILayout.EnumPopup((ATLAS_SIZE)_textureAtlas.maxAtlasSize, GUILayout.Width(100.0f)));
            if (newMaxAtlasSize != _textureAtlas.maxAtlasSize)
            {
                _textureAtlas.maxAtlasSize = newMaxAtlasSize;
            }
            GUILayout.EndHorizontal();

            bool newForceSquare = GUILayout.Toggle(_textureAtlas.forceSquare, "Force Square", Style.normalToggleStyle, GUILayout.Width(100.0f));
            if (newForceSquare != _textureAtlas.forceSquare)
            {
                _textureAtlas.forceSquare = newForceSquare;
            }

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();

            if (_needsRebuilt)
            {
                Style.PushBackgroundColor(Style.warningStyle);
                if (GUILayout.Button("Rebuild Atlas", GUILayout.Width(100.0f)))
                {
                    if (AllTextureNamesUnique())
                    {
                        if (CheckExistingFiles())
                        {
                            BuildAtlas();
                        }
                    }

                }
                Style.PopBackgroundColor();
            }
            else
            {
                if (GUILayout.Button("Rebuild Atlas", GUILayout.Width(100.0f)))
                {
                    if (AllTextureNamesUnique())
                    {

                        if (CheckExistingFiles())
                        {
                            BuildAtlas();
                        }
                    }
                }
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.Space(10.0f);

            GUILayout.EndHorizontal();

            GUILayout.Space(6.0f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20.0f);
            GUILayout.BeginHorizontal(Style.windowRectDarkBackgroundStyle, GUILayout.Width(position.width - 40.0f));
            GUILayout.Space(5.0f);
            if (_selectedTexture != null)
            {
                GUILayout.Label(_selectedTexture.texturePath, Style.normalLabelStyle);
            }
            else
            {
                GUILayout.Label("No Texture Selected", Style.normalLabelStyle);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            if (_needsRebuilt)
            {
                GUILayout.BeginArea(_texturesRect, Style.warningStyle);
            }
            else
            {
                GUILayout.BeginArea(_texturesRect, Style.windowRectBackgroundStyle);
            }

            CheckNeedsRedisplayed();

            Redisplay();


            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);

            GUILayout.BeginVertical();
            GUILayout.Box(GUIContent.none, GUI.skin.label, GUILayout.Height(_texturesScrollRect.height));
            GUILayout.EndVertical();

            for (int index=0; index<_displayTextures.Count; index++)
            {
                _displayTextures[index].scale = _zoomFactor;
                if (_selectedTexture == _displayTextures[index])
                    _displayTextures[index].selected = true;
                else
                    _displayTextures[index].selected = false;
                _displayTextures[index].OnGUI(ShowPivots, _textureAtlas.defaultPivotOffsets[index]);
            }

            GUILayout.EndScrollView();

            GUILayout.EndArea();

            // texture pivot
            GUILayout.BeginArea(_texturePivotRect, Style.windowRectBackgroundStyle);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(GUILayout.Height(25.0f));
            GUILayout.Space(BoneAnimationDataEditorWindow.PADDING);
            GUILayout.Label("Default Pivot", Style.normalLabelStyle);
            GUILayout.EndHorizontal();

            if (_selectedTexture != null)
            {
                Vector2 pivotOffset = _textureAtlas.defaultPivotOffsets[_selectedTexture.index];
                Vector2 oldPivotOffset = pivotOffset;
                bool somethingChanged;
                Sprite sprite;

                _texturePivotWindow.SetTexture(_selectedTexture.texture);

                bool usingDefaultPivot = false;

                somethingChanged = _texturePivotWindow.OnGUI(ref pivotOffset,
                                                            Event.current,
                                                            new Rect(0, 15.0f, TEXTURE_PIVOT_WINDOW_WIDTH, _texturePivotRect.height - 30.0f),
                                                            15.0f,
                                                            false,
                                                            ref usingDefaultPivot,
                                                            pivotOffset
                                                            );

                if (somethingChanged)
                {
                    _needsRebuilt = true;

                    _textureAtlas.defaultPivotOffsets[_selectedTexture.index] = pivotOffset;

                    GetBoneAnimationDataEditor();
                    
                    if (_boneAnimationDataEditorWindow != null)
                    {
                        _boneAnimationDataEditorWindow.RefreshDefaultPivot();
                    }
                    
                    //Object[] oList = Object.FindSceneObjectsOfType(typeof(Sprite));
                    Object[] oList = Object.FindObjectsOfType(typeof(Sprite));
                    foreach (Object o in oList)
                    {
                        sprite = (Sprite)o;
                        if (sprite.atlas == _textureAtlas)
                        {
                            if (sprite.textureGUID == _textureAtlas.textureGUIDs[_selectedTexture.index]
                                &&
                                sprite.useDefaultPivot)
                            {
                                sprite.SetPivotOffset(pivotOffset, true);
                            }
                        }
                    }

                    _isDirty = true;
                    _needsRepainted = true;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
            // end texture pivot

            GUILayout.Space(_texturesRect.height); // + 10.0f);

            GUILayout.BeginHorizontal();

            GUILayout.Space(10.0f);

            if (_displayTextures.Count > 0)
            {
                GUIContent guiContent;
                if (ShowPivots)
                {
                    guiContent = new GUIContent(Resources.buttonPivotOn, "Hide Pivots");
                }
                else
                {
                    guiContent = new GUIContent(Resources.buttonPivotOff, "Show Pivots");
                }
                if (GUILayout.Button(guiContent, Style.noBorderButtonStyle, GUILayout.Width(Resources.buttonPivotOff.width), GUILayout.Height(Resources.buttonPivotOff.height)))
                {
                    ShowPivots = !ShowPivots;
                }
            }

            GUILayout.Space(10.0f);

            int selectedIndex;

            selectedIndex = -1;
            if (_selectedTexture != null)
            {
                for (int i = 0; i < _displayTextures.Count; i++)
                {
                    if (_displayTextures[i] == _selectedTexture)
                    {
                        selectedIndex = i;
                        break;
                    }
                }
            }
            if (selectedIndex != -1)
            {
                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.margin.top = 0;
                if (GUILayout.Button("Delete Texture", style, GUILayout.Width(100.0f), GUILayout.Height(23.0f)))
                {
                    RemoveTexture(selectedIndex);
                    _needsRepainted = true;
                }
            }

            GUILayout.EndHorizontal();
        }

        private void HandleInput()
        {
            Event currentEvent;
            Vector2 relativeMousePosition;

            currentEvent = Event.current;
            relativeMousePosition = currentEvent.mousePosition -
                                    GUIHelper.UpperLeftCorner(_texturesRect) +
                                    _scrollPosition;

            switch (currentEvent.type)
            {
                case EventType.dragUpdated:
                case EventType.dragPerform:
                    if (_texturesScrollRect.Contains(relativeMousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (currentEvent.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            AppendTextures(DragAndDrop.objectReferences);
                            _needsRepainted = true;
                        }

                        currentEvent.Use();
                    }
                    break;

                case EventType.mouseDown:
                    _selectedTexture = null;
                    for (int i = 0; i < _displayTextures.Count; i++)
                    {
                        if (_displayTextures[i].drawRect.Contains(relativeMousePosition))
                        {
                            _selectedTexture = _displayTextures[i];
                            _textureGUID = _textureGUIDs[i];

                            currentEvent.Use();
                            break;
                        }
                    }
                    break;
            }
        }

        private void SetSelectedTexture()
        {
            _selectedTexture = null;

            if (_textureGUIDs == null)
                return;

            if (_displayTextures == null)
                return;
        }

        private void CheckNeedsRedisplayed()
        {
            bool needsRedisplayed = false;

            if (_textureAtlas == null)
                return;
            else if (_textureAtlas.textureGUIDs == null)
                return;
            else if (_displayTextures == null)
            {
                needsRedisplayed = true;
            }
            else if (_displayTextures.Count != _textureAtlas.textureGUIDs.Count)
            {
                needsRedisplayed = true;
            }
            else
            {
                foreach (string guid in _textureGUIDs)
                {
                    if (!_textureAtlas.textureGUIDs.Contains(guid))
                    {
                        needsRedisplayed = true;
                        break;
                    }
                }

                foreach (string guid in _textureAtlas.textureGUIDs)
                {
                    if (!_textureGUIDs.Contains(guid))
                    {
                        needsRedisplayed = true;
                        break;
                    }
                }
            }

            if (needsRedisplayed)
            {
                RedisplayTextures();
            }
        }

        private bool CheckNeedsRebuilt()
        {
            bool needsRebuilt = false;

            if (_textureAtlas != null)
            {
                if (_textureAtlas.textureGUIDs != null)
                {
                    foreach (string guid in _textureGUIDs)
                    {
                        if (!_textureAtlas.textureGUIDs.Contains(guid))
                        {
                            needsRebuilt = true;
                            break;
                        }
                    }

                    if (_textureAtlas.textureGUIDs.Count != _textureAtlas.uvs.Count)
                    {
                        needsRebuilt = true;
                    }
                    else if (_textureAtlas.textureGUIDs.Count != _textureAtlas.defaultPivotOffsets.Count)
                    {
                        needsRebuilt = true;
                    }
                }
            }

            int oldNeedsRebuilt = _textureAtlas.needsRebuilt;
            _textureAtlas.needsRebuilt = (needsRebuilt ? 1 : 0);
            if (oldNeedsRebuilt != _textureAtlas.needsRebuilt)
                _isDirty = true;

            return needsRebuilt;
        }

        private void RedisplayTextures()
        {
            Texture2D texture;
            string guid;
            bool isDirty = false;

            ClearDisplayTextures();
            _textureGUIDs.Clear();

            int i = 0;

            while (i < _textureAtlas.textureGUIDs.Count)
            {
                guid = _textureAtlas.textureGUIDs[i];
                texture = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Texture2D));

                if (texture != null)
                {
                    DisplayTexture tex = new DisplayTexture(texture, guid, TextureManager.GetAssetNameFromPath(AssetDatabase.GUIDToAssetPath(guid), false), AssetDatabase.GUIDToAssetPath(guid), i, new Vector2(0, 0));

                    _displayTextures.Add(tex);
                    _textureGUIDs.Add(guid);

                    i++;
                }
                else
                {
                    _textureAtlas.textureGUIDs.RemoveAt(i);
                    _textureAtlas.textureSizes.RemoveAt(i);
                    _textureAtlas.uvs.RemoveAt(i);
                    _textureAtlas.defaultPivotOffsets.RemoveAt(i);
                    _textureAtlas.textureNames.RemoveAt(i);
                    _textureAtlas.texturePaths.RemoveAt(i);
                    isDirty = true;
                }
            }

            if (isDirty)
            {
                EditorUtility.SetDirty(_textureAtlasObject);
            }

            Repaint();
        }

        private void Redisplay()
        {
            float leftEdge = TEXTURE_EDGE_BUFFER;
            float top = TEXTURE_EDGE_BUFFER;
            float maxRowHeight = 0;
            bool matchedSelection = false;
            DisplayTexture tex;
            int i;
            int textureIndex;

            List<SortedTexture> sortedGUIDs = new List<SortedTexture>();
            for (textureIndex = 0; textureIndex < _displayTextures.Count; textureIndex++)
            {
                sortedGUIDs.Add(new SortedTexture(TextureManager.GetAssetNameFromPath(AssetDatabase.GUIDToAssetPath(_displayTextures[textureIndex].guid), false), textureIndex));
            }
            sortedGUIDs.Sort(new SortTexturesAscending());

            foreach (SortedTexture sortedTexture in sortedGUIDs)
            {
                i = sortedTexture.textureIndex;

                tex = _displayTextures[i];

                if ((leftEdge + tex.texture.width + TEXTURE_EDGE_BUFFER + 30.0f) > _texturesRect.width)
                {
                    leftEdge = TEXTURE_EDGE_BUFFER;
                    top += maxRowHeight + SPACE_BETWEEN_TEXTURES;
                    maxRowHeight = 0;
                }

                tex.position = new Vector2(leftEdge, top);

                if (tex.texture != null)
                    leftEdge += (tex.texture.width * _zoomFactor) + SPACE_BETWEEN_TEXTURES;
                else
                    leftEdge += DisplayTexture.NULL_SIZE + SPACE_BETWEEN_TEXTURES;

                if ((tex.texture.height + DisplayTexture.LABEL_HEIGHT) > maxRowHeight)
                {
                    maxRowHeight = tex.texture.height + DisplayTexture.LABEL_HEIGHT;
                }

                if (_selectedTexture != null)
                    if (tex.index == _selectedTexture.index && tex.texture == _selectedTexture.texture)
                    {
                        _selectedTexture = tex;
                        matchedSelection = true;
                    }
            }

            _texturesScrollRect = new Rect(_texturesRect.x, 0, _texturesRect.width, Mathf.Max(top + maxRowHeight, _texturesRect.height));

            if (!matchedSelection)
                _selectedTexture = null;
        }

        private void ClearDisplayTextures()
        {
            _displayTextures.Clear();
        }

        private void AppendTextures(Object[] objs)
        {
            List<string> newTextureGUIDs = new List<string>();
            List<Texture2D> sortedNewList = new List<Texture2D>();
            string textureGUID;

            newTextureGUIDs.AddRange(_textureAtlas.textureGUIDs);

            // Now iterate through the newly dropped objects and verify they are
            // textures:
            for (int i = 0; i < objs.Length; ++i)
            {
                if (objs[i] is Texture2D)
                {
                    if (!sortedNewList.Contains((Texture2D)objs[i]))
                    {
                        sortedNewList.Add((Texture2D)objs[i]);
                    }
                }
            }

            // Now append these onto the list:
            for (int i = 0; i < sortedNewList.Count; ++i)
            {
                textureGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath((Texture2D)sortedNewList[i]));
                if (!newTextureGUIDs.Contains(textureGUID))
                {
                    newTextureGUIDs.Add(textureGUID);
                }
            }

            // create new pivots
            int newPivotCount = (newTextureGUIDs.Count - _textureAtlas.textureGUIDs.Count);
            for (int p = 0; p < newPivotCount; p++)
            {
                _textureAtlas.defaultPivotOffsets.Add(Vector3.zero);
            }

            // Now dump it back to the original array:
            _textureAtlas.textureGUIDs.Clear();
            _textureAtlas.textureGUIDs.AddRange(newTextureGUIDs);

            RedisplayTextures();

            if (_textureAtlasObject != null)
            {
                _isDirty = true;
            }
        }

        private void RemoveTexture(int index)
        {
            _textureAtlas.textureGUIDs.RemoveAt(index);
            _textureAtlas.defaultPivotOffsets.RemoveAt(index);
            RedisplayTextures();

            if (_textureAtlasObject != null)
            {
                _isDirty = true;
            }
        }

        private bool CheckExistingFiles()
        {
            string path = AssetDatabase.GetAssetPath(_textureAtlas);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(_textureAtlas)), "");
            }

            string[] filePaths;
            List<string> filesToOverwrite = new List<string>();
            
            filePaths = Directory.GetFiles(path, _textureAtlas.name + ".mat");
            foreach (string filePath in filePaths)
            {
                if (Path.GetFileNameWithoutExtension(filePath) == Path.GetFileNameWithoutExtension(_textureAtlas.name))
                {
                    filesToOverwrite.Add(Path.GetFileName(filePath));
                }
            }
            filePaths = Directory.GetFiles(path, _textureAtlas.name + ".png");
            foreach (string filePath in filePaths)
            {
                if (Path.GetFileNameWithoutExtension(filePath) == Path.GetFileNameWithoutExtension(_textureAtlas.name))
                {
                    filesToOverwrite.Add(Path.GetFileName(filePath));
                }
            }

            if (filesToOverwrite.Count > 0)
            {
                string filesToOverwriteString = "\n\n";
                foreach (string fileName in filesToOverwrite)
                {
                    filesToOverwriteString += fileName + "\n";
                }
                filesToOverwriteString += "\n";

                return (EditorUtility.DisplayDialog("Some Files Already Exist", "The following files will be overwritten when you create this atlas." + filesToOverwriteString + "Are you sure you want to do this?", "Yes", "No"));
            }
            else
            {
                return true;
            }
        }

        private bool AllTextureNamesUnique()
        {
            List<string> uniqueTextureGUIDs = new List<string>();
            List<string> uniqueTextureNames = new List<string>(); ;
            string textureName;

            for (int tg = 0; tg < _textureAtlas.textureGUIDs.Count; tg++)
            {
                if (!uniqueTextureGUIDs.Contains(_textureAtlas.textureGUIDs[tg]))
                {
                    uniqueTextureGUIDs.Add(_textureAtlas.textureGUIDs[tg]);
                }
            }

            foreach (string textureGUID in uniqueTextureGUIDs)
            {
                textureName = GetAssetNameFromPath(AssetDatabase.GUIDToAssetPath(textureGUID), false);

                if (!uniqueTextureNames.Contains(textureName))
                {
                    uniqueTextureNames.Add(textureName);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "One or more of your textures share the name [" + textureName + "]. Please make sure all texture names are unique within an atlas.", "OK");

                    return false;
                }
            }

            return true;
        }

        private void BuildAtlas()
        {
            Texture2D atlas;
            byte[] bytes;
            Rect[] atlasUVs;
            List<Texture2D> textures;
            List<string> usedTextureGUIDs;
            List<Vector2> usedDefaultPivotOffsets;
            Texture2D tex;

            if (_textureAtlas.maxAtlasSize < 32)
            {
                _textureAtlas.maxAtlasSize = 32;
            }

            // Get the asset path
            string path = AssetDatabase.GetAssetPath(_textureAtlas);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(_textureAtlas)), "");
            }

            // clear the uv list in the textureAtlas
            if (_textureAtlas.uvs == null)
                _textureAtlas.uvs = new List<Rect>();
            else
                _textureAtlas.uvs.Clear();

            // clear out the texture sizes
            if (_textureAtlas.textureSizes == null)
                _textureAtlas.textureSizes = new List<Vector2>();
            else
                _textureAtlas.textureSizes.Clear();

            // clear out texture names
            if (_textureAtlas.textureNames == null)
                _textureAtlas.textureNames = new List<string>();
            else
                _textureAtlas.textureNames.Clear();

            // clear out texture paths
            if (_textureAtlas.texturePaths == null)
                _textureAtlas.texturePaths = new List<string>();
            else
                _textureAtlas.texturePaths.Clear();

            // remove duplicate texture guids in the atlas
            usedTextureGUIDs = new List<string>();
            usedDefaultPivotOffsets = new List<Vector2>();
            for (int tg=0; tg < _textureAtlas.textureGUIDs.Count; tg++)
            {
                if (!usedTextureGUIDs.Contains(_textureAtlas.textureGUIDs[tg]))
                {
                    usedTextureGUIDs.Add(_textureAtlas.textureGUIDs[tg]);
                    usedDefaultPivotOffsets.Add(_textureAtlas.defaultPivotOffsets[tg]);
                }
            }
            // write back unique list to the texture GUID array
            _textureAtlas.textureGUIDs.Clear();
            _textureAtlas.textureGUIDs.AddRange(usedTextureGUIDs);
            _textureAtlas.defaultPivotOffsets.Clear();
            _textureAtlas.defaultPivotOffsets.AddRange(usedDefaultPivotOffsets);
            usedTextureGUIDs.Clear();
            usedTextureGUIDs = null;
            usedDefaultPivotOffsets.Clear();
            usedDefaultPivotOffsets = null;

            // get a list of textures for this atlas
            // and set the texture sizes array
            textures = new List<Texture2D>();
            foreach (string textureGUID in _textureAtlas.textureGUIDs)
            {
                tex = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(textureGUID), typeof(Texture2D));
                textures.Add(tex);
            }

            // reimport textures if they are not in the right format
            bool refresh = false;
            foreach (Texture2D texture in textures)
            {
                if (VerifyTextureImportSettings(texture))
                    refresh = true;
            }
            if (refresh)
            {
                AssetDatabase.Refresh();
            }

            foreach (Texture2D texture in textures)
            {
                _textureAtlas.textureSizes.Add(new Vector2(texture.width, texture.height));
                _textureAtlas.textureNames.Add(texture.name);
                _textureAtlas.texturePaths.Add(AssetDatabase.GetAssetPath(texture));
            }

            // Create atlas
            atlas = new Texture2D((int)ATLAS_SIZE.Size32, (int)ATLAS_SIZE.Size32);
            atlasUVs = atlas.PackTextures((Texture2D[])textures.ToArray(), _textureAtlas.padding, 4096);
            textures.Clear();
            textures = null;
            foreach (Rect uv in atlasUVs)
            {
                _textureAtlas.uvs.Add(uv);
            }

            // See if the texture needs to be made square:
            if (_textureAtlas.forceSquare && atlas.width != atlas.height)
            {
                int size = Mathf.Max(atlas.width, atlas.height);

                // Create a square texture:
                Texture2D tempTex = (Texture2D)Instantiate(atlas);
                tempTex.name = atlas.name;
                tempTex.Resize(size, size, TextureFormat.ARGB32, false);

                Color[] pixels = tempTex.GetPixels();
                Color blankPixel = new Color(0, 0, 0, 0);
                for (int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
                {
                    pixels[pixelIndex] = blankPixel;
                }
                tempTex.SetPixels(pixels);

                // Copy the contents:
                tempTex.SetPixels(0, 0, atlas.width, atlas.height, atlas.GetPixels(0), 0);
                tempTex.Apply(false);

                Rect temp;

                // Scale the UVs to account for this:
                for (int i = 0; i < _textureAtlas.uvs.Count; i++)
                {
                    temp = _textureAtlas.uvs[i];

                    // See which side we expanded:
                    if (atlas.width > atlas.height)
                    {
                        temp.y = temp.y * 0.5f;
                        temp.yMax = temp.y + (temp.height * 0.5f);
                    }
                    else
                    {
                        temp.x = temp.x * 0.5f;
                        temp.xMax = temp.x + (temp.width * 0.5f);
                    }

                    _textureAtlas.uvs[i] = temp;
                }

                if (atlas != tempTex)
                    Editor.DestroyImmediate(atlas);
                atlas = tempTex;
            }

            bytes = atlas.EncodeToPNG();
            Directory.CreateDirectory(path);
            using (FileStream fs = File.Create(path + _textureAtlas.name + ".png"))
            {
                fs.Write(bytes, 0, bytes.Length);
            }
            bytes = null;
            AssetDatabase.Refresh();
            atlas = (Texture2D)AssetDatabase.LoadAssetAtPath(path + _textureAtlas.name + ".png", typeof(Texture2D));
            VerifyAtlasImportSettings(atlas, _textureAtlas.maxAtlasSize);

            // Create material (if necessary)
            _textureAtlas.material = (Material)AssetDatabase.LoadAssetAtPath(path + _textureAtlas.name + ".mat", typeof(Material));
            if (_textureAtlas.material == null)
            {
                _textureAtlas.material = new Material(Shader.Find("Particles/Alpha Blended"));
                AssetDatabase.CreateAsset(_textureAtlas.material, path + _textureAtlas.name + ".mat");

                if (EditorHelper.LogUpdates)
                    Debug.Log("Created material [" + _textureAtlas.name + "]");
            }
            _textureAtlas.material.mainTexture = atlas;
            AssetDatabase.Refresh();

            // Update all the objects that are using this atlas
            List<Object> objList;
            Object[] o;

            objList = new List<Object>();
            o = FindObjectsOfType(typeof(Sprite));
            Sprite sprite;
            objList.AddRange(o);
            foreach (Object o2 in objList)
            {
                sprite = (Sprite)o2;

                if (sprite.atlas == _textureAtlas)
                {
                    sprite.SetAtlas(_textureAtlas);
                }
            }

            _textureAtlas.lastBuildID = System.DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 1024).ToString();

            EditorUtility.SetDirty(_textureAtlasObject);

            AssetDatabase.SaveAssets();

            if (EditorHelper.LogUpdates)
                Debug.Log("Built atlas [" + _textureAtlas.name + "]");

            _needsRebuilt = false;
            _needsRepainted = true;

            TextureManager.GeneratedDictionaries = false;

            AnimationHelper.SetAssetsNeedBuiltThatHaveAtlasInScene(_textureAtlas);

            //AnimationHelper.UpdateBoneAnimationsAndDataInCurrentScene(true);

            //EditorUtility.DisplayDialog("Atlas Built", "Atlas [" + _textureAtlas.name + "] was built. Please be sure to rebuild your BoneAnimationData, prefabs, and BoneAnimations (or turn Auto-build on in the SmoothMoves Control Panel: \"SmoothMoves > Tools > Control Panel\").", "OK");
        }

        private bool VerifyTextureImportSettings(Texture2D tex)
        {
            string texturePath;

            // Re-import so we can make sure it will be ready for atlas
            // generation:
            texturePath = AssetDatabase.GetAssetPath(tex);
            // Get the texture's importer:
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texturePath);
            if (!importer.isReadable
                ||
                importer.textureFormat != TextureImporterFormat.ARGB32
                ||
                importer.npotScale != TextureImporterNPOTScale.None
                ||
                importer.mipmapEnabled
                ||
                importer.maxTextureSize != MAX_TEXTURE_SIZE 
                ||
                importer.wrapMode != TextureWrapMode.Clamp
                ||
                importer.filterMode != FilterMode.Bilinear
                )
            {
                // Reimport it with isReadable set to true and ARGB32:
                importer.isReadable = true;
                importer.textureFormat = TextureImporterFormat.ARGB32;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = MAX_TEXTURE_SIZE;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.Refresh();

                return true;
            }

            return false;
        }

        private bool VerifyAtlasImportSettings(Texture2D tex, int maxAtlasSize)
        {
            string texturePath;

            // Re-import so we can make sure it will be ready for atlas
            // generation:
            texturePath = AssetDatabase.GetAssetPath(tex);
            // Get the texture's importer:
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texturePath);
            if (importer.maxTextureSize != maxAtlasSize || importer.wrapMode != TextureWrapMode.Clamp)
            {
                // Reimport it with isReadable set to true and ARGB32:
                importer.maxTextureSize = maxAtlasSize;
                importer.wrapMode = TextureWrapMode.Clamp;
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.Refresh();

                return true;
            }

            return false;
        }

        private BoneAnimationDataEditorWindow GetBoneAnimationDataEditor()
        {
            if (_boneAnimationDataEditorWindow == null)
            {
                if (BoneAnimationDataEditorWindow.Instance != null)
                    _boneAnimationDataEditorWindow = BoneAnimationDataEditorWindow.ShowEditorUtility();
            }

            return _boneAnimationDataEditorWindow;
        }

        static public string GetAssetNameFromPath(string path, bool showExtension)
        {
            string[] s = path.Split(@"/"[0]);
            if (s.Length > 0)
            {
                string fileName = s[s.Length - 1];

                if (!showExtension)
                {
                    int pos = fileName.IndexOf(".");
                    if (pos > -1)
                    {
                        fileName = fileName.Substring(0, pos);
                    }
                }

                return fileName;
            }

            return path;
        }
    }

    public class SortedTexture
    {
        public string textureName;
        public int textureIndex;

        public SortedTexture(string name, int index)
        {
            textureName = name;
            textureIndex = index;
        }
    }

    public class SortTexturesAscending : IComparer<SortedTexture>
    {
        int IComparer<SortedTexture>.Compare(SortedTexture a, SortedTexture b)
        {
            int compare = System.String.Compare(a.textureName, b.textureName);

            if (compare > 0)
                return 1;
            if (compare < 0)
                return -1;
            else
                return 0;
        }
    }
}
