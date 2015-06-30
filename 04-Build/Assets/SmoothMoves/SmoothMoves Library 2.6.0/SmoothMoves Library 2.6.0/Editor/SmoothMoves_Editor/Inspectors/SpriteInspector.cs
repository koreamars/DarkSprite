using UnityEngine;
using UnityEditor;

namespace SmoothMoves
{
    [CustomEditor(typeof(Sprite))]
    public class SpriteInspector : Editor
    {
        private Sprite _sprite;

        //private MeshRenderer _renderer;
        private TextureSelectComponent _textureSelectComponent;

        static public void UpdateAllSpritesInScene()
        {
            UnityEngine.Object[] oList = Object.FindObjectsOfType(typeof(Sprite));
            foreach (Object o in oList)
            {
                ((Sprite)o).Initialize();
                EditorUtility.SetDirty(((Sprite)o).gameObject);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_sprite != null)
            {
                Style.Reset();
                Resources.LoadTextures();
                _textureSelectComponent.OnGUI();

                string[] sizeModeStrings = new string[2];
                sizeModeStrings[0] = "Relative Scale";
                sizeModeStrings[1] = "Absolute Size";
                GUILayout.BeginHorizontal();
                Sprite.SIZE_MODE newSizeMode = (Sprite.SIZE_MODE)GUILayout.SelectionGrid((int)_sprite.sizeMode, sizeModeStrings, 2);
                if (newSizeMode != _sprite.sizeMode)
                {
                    _sprite.Initialize();
                    _sprite.SetSizeMode(newSizeMode);
                    UpdateInstances();
                }
                GUILayout.EndHorizontal();

                switch (newSizeMode)
                {
                    case Sprite.SIZE_MODE.RelativeScale:
                        Vector2 oldScale = _sprite.scale;
                        Vector2 newScale = oldScale;

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(" Scale X ", GUILayout.Width(100.0f));
                        newScale.x = EditorGUILayout.FloatField(oldScale.x);
                        if (newScale.x != oldScale.x)
                        {
                            _sprite.Initialize();
                            _sprite.SetScale(newScale);
                            UpdateInstances();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(" Scale Y ", GUILayout.Width(100.0f));
                        newScale.y = EditorGUILayout.FloatField(oldScale.y);
                        if (newScale.y != oldScale.y)
                        {
                            _sprite.Initialize();
                            _sprite.SetScale(newScale);
                            UpdateInstances();
                        }
                        GUILayout.EndHorizontal();

                        break;

                    case Sprite.SIZE_MODE.Absolute:
                        Vector2 oldSize = _sprite.size;
                        Vector2 newSize = oldSize;

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(" Width ", GUILayout.Width(100.0f));
                        newSize.x = EditorGUILayout.FloatField(oldSize.x);
                        if (newSize.x != oldSize.x)
                        {
                            _sprite.Initialize();
                            _sprite.SetSize(newSize);
                            UpdateInstances();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(" Height ", GUILayout.Width(100.0f));
                        newSize.y = EditorGUILayout.FloatField(oldSize.y);
                        if (newSize.y != oldSize.y)
                        {
                            _sprite.Initialize();
                            _sprite.SetSize(newSize);
                            UpdateInstances();
                        }
                        GUILayout.EndHorizontal();
                        break;
                }

                Color newColor = EditorGUILayout.ColorField(" Color ", _sprite.color);
                if (newColor != _sprite.color)
                {
                    _sprite.Initialize();
                    _sprite.SetColor(newColor);
                    UpdateInstances();
                }

                GUILayout.Space(15.0f);

                if (PrefabUtility.GetPrefabType(_sprite.gameObject) == PrefabType.PrefabInstance)
                {
                    if (GUILayout.Button("Disconnect from Prefab"))
                    {
                        // disconnect this instance from its prefab parent

                        PrefabUtility.DisconnectPrefabInstance(_sprite.gameObject);
                        _sprite.DisconnectInstance();
                        EditorUtility.SetDirty(_sprite.gameObject);
                    }

                    GUILayout.Space(15.0f);
                }
                else //if (PrefabUtility.GetPrefabType(_sprite.gameObject) == PrefabType.None)
                {
                    if (GUILayout.Button("Create Separate Mesh"))
                    {
                        // if this sprite has been duplicated, then the mesh object
                        // is probably shared with the other object it was copied from.
                        // seperating the mesh will allow modifications to be independant
                        // between the two objects.

                        PrefabUtility.DisconnectPrefabInstance(_sprite.gameObject);
                        _sprite.DisconnectInstance();
                        EditorUtility.SetDirty(_sprite.gameObject);
                    }

                    GUILayout.Space(15.0f);
                }
            }
        }

        public void OnEnable()
        {

            _sprite = (Sprite)target;

            if (_textureSelectComponent == null)
            {
                _textureSelectComponent = new TextureSelectComponent();

                if (_sprite != null)
                    _textureSelectComponent.SetAtlasChangedDelegate(_sprite.gameObject, SetAtlas);
                _textureSelectComponent.SetTextureChangedDelegate(SetTextureGUID);
                _textureSelectComponent.SetPivotOffsetChangedDelegate(SetPivotOffset);
            }

            if (_sprite != null)
            {
                _sprite.Initialize();

                _textureSelectComponent.atlas = _sprite.atlas;
                _textureSelectComponent.textureGUID = _sprite.textureGUID;
                _textureSelectComponent.Initialize();

                _textureSelectComponent.OnEnable();
                _textureSelectComponent.SetAtlas(_sprite.atlas);
                _textureSelectComponent.SetTextureGUID(_sprite.textureGUID);
                _textureSelectComponent.SetPivotOffset(_sprite.pivotOffsetOverride, _sprite.useDefaultPivot);

                EditorUtility.SetDirty(_sprite.gameObject);
            }
        }

        public void OnDisable()
        {
            _sprite = null;
        }

        public void SetAtlas(TextureSelectComponent textureSelectComponent, TextureAtlas atlas)
        {
            if (_sprite != null)
            {
                bool updateInstances = false;

                if (atlas != _sprite.atlas)
                {
                    updateInstances = true;
                }

                //if (atlas != null)
                //    _renderer.material = atlas.material;

                _sprite.SetAtlas(atlas);
                _sprite.SetTextureGUID(textureSelectComponent.textureGUID);
                //_sprite.atlas = atlas;

                if (updateInstances)
                    UpdateInstances();

                EditorUtility.SetDirty(_sprite);

                Repaint();
            }
        }

        public void SetTextureGUID(TextureSelectComponent textureSelectComponent, string textureGUID)
        {
            if (_sprite != null)
            {
                bool updateInstances = false;

                if (textureGUID != _sprite.textureGUID)
                {
                    updateInstances = true;
                }

                _sprite.SetTextureGUID(textureGUID);

                if (updateInstances)
                    UpdateInstances();

                EditorUtility.SetDirty(_sprite);

                Repaint();
            }
        }

        public void SetPivotOffset(TextureSelectComponent textureSelectComponent, Vector2 pivotOffset, bool useDefaultPivot)
        {
            if (_sprite != null)
            {
                bool updateInstances = false;

                if (pivotOffset != _sprite.pivotOffsetOverride || useDefaultPivot != _sprite.useDefaultPivot)
                {
                    updateInstances = true;
                }

                _sprite.useDefaultPivot = useDefaultPivot;

                _sprite.SetPivotOffset(pivotOffset, useDefaultPivot);

                textureSelectComponent.pivotOffset = _sprite.pivotOffsetOverride;
                textureSelectComponent.useDefaultPivot = _sprite.useDefaultPivot;

                if (updateInstances)
                    UpdateInstances();

                EditorUtility.SetDirty(_sprite);

                Repaint();
            }
        }

        private void UpdateInstances()
        {
            if (_sprite != null)
            {
                if (PrefabUtility.GetPrefabType(_sprite.gameObject) == PrefabType.Prefab)
                {
                    // if this sprite is a prefab for other sprites, 
                    // then get its children and update them to look like this prefab

                    EditorUtility.SetDirty(_sprite.gameObject);

                    //Object[] oList = Object.FindSceneObjectsOfType(typeof(Sprite));
                    Object[] oList = Object.FindObjectsOfType(typeof(Sprite));

                    foreach (Object o in oList)
                    {
                        if (PrefabUtility.GetPrefabType(o) == PrefabType.PrefabInstance)
                        {
                            if (PrefabUtility.GetPrefabParent(o) == (Object)_sprite)
                            {
                                ((Sprite)o).Copy(_sprite, true);
                            }
                        }
                    }
                }
                else if (PrefabUtility.GetPrefabType(_sprite.gameObject) == PrefabType.PrefabInstance)
                {
                    // if this sprite is an instance of a prefab,
                    // then get its parent prefab and all the sibling instances that share the parent prefab
                    // and update them to look like this instance.

                    EditorUtility.SetDirty(_sprite.gameObject);

                    //Object[] oList = Object.FindObjectsOfTypeIncludingAssets(typeof(Sprite));
                    Object[] oList = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(Sprite));

                    foreach (Object o in oList)
                    {
                        if (PrefabUtility.GetPrefabType(o) == PrefabType.Prefab)
                        {
                            if (PrefabUtility.GetPrefabParent((Object)_sprite) == o)
                            {
                                // we don't need to copy the mesh on the prefab since it won't store anyway
                                ((Sprite)o).Copy(_sprite, false);
                            }
                        }
                        else if (PrefabUtility.GetPrefabType(o) == PrefabType.PrefabInstance)
                        {
                            if (PrefabUtility.GetPrefabParent((Object)_sprite) == PrefabUtility.GetPrefabParent(o))
                            {
                                // we copy the mesh on the sibling instance
                                ((Sprite)o).Copy(_sprite, true);
                            }
                        }
                    }
                }
            }
        }
    }
}
