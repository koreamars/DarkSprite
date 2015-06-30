using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SmoothMoves
{
    [CustomEditor(typeof(TextureFunctionMonoBehaviour))]
    public class TextureFunctionInspector : Editor
    {
        private TextureFunctionMonoBehaviour _baseBehaviour;
        private List<TextureSelectComponent> _textureSelectComponents;
        private List<TextureSearchReplaceComponent> _textureSearchReplaceComponents;

        protected bool showAtTop;

        public override void OnInspectorGUI()
        {
            if (_baseBehaviour != null)
            {
                Style.Reset();
                Resources.LoadTextures();

                if (showAtTop)
                {
                    DisplayTextureSelectComponents();
                    DisplayTextureSearchReplaceComponents();
                    base.OnInspectorGUI();
                }
                else
                {
                    base.OnInspectorGUI();
                    DisplayTextureSelectComponents();
                    DisplayTextureSearchReplaceComponents();
                }
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private void DisplayTextureSelectComponents()
        {
            int newTextureSelectCount = EditorGUILayout.IntField("Texture Select Count:", _baseBehaviour.textureSelectCount);
            if (newTextureSelectCount < _baseBehaviour.textureSelectCount)
            {
                _baseBehaviour.textureSelectCount = newTextureSelectCount;

                _textureSelectComponents.RemoveRange(_baseBehaviour.textureSelectCount, _textureSelectComponents.Count - _baseBehaviour.textureSelectCount);

                // clean up unused groups
                int index = _textureSelectComponents.Count;
                while (index < _baseBehaviour.textureSelectList.Count)
                {
                    if (_baseBehaviour.textureSelectList[index].atlas == null
                        &&
                        _baseBehaviour.textureSelectList[index].textureGUID == "")
                    {
                        _baseBehaviour.textureSelectList.RemoveAt(index);
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            else if (newTextureSelectCount > _baseBehaviour.textureSelectCount)
            {
                int oldCount = _baseBehaviour.textureSelectCount;

                _baseBehaviour.textureSelectCount = newTextureSelectCount;

                TextureSelectComponent textureSelectComponent;
                TextureAtlas newAtlas;
                string newTextureGUID;
                Vector2 newPivot;
                bool newUseDefaultPivot;
                TextureSelect textureSelect;
                for (int i = oldCount; i < newTextureSelectCount; i++)
                {
                    textureSelectComponent = new TextureSelectComponent(i, true);
                    textureSelectComponent.SetAtlasChangedDelegate(_baseBehaviour.gameObject, SetSelectAtlas);
                    textureSelectComponent.SetTextureChangedDelegate(SetSelectTextureGUID);
                    textureSelectComponent.SetPivotOffsetChangedDelegate(SetSelectPivotOffset);

                    if (i >= _baseBehaviour.textureSelectList.Count)
                    {
                        newAtlas = null;
                        newTextureGUID = "";
                        newPivot = Vector2.zero;
                        newUseDefaultPivot = true;

                        textureSelect = new TextureSelect();

                        textureSelect.atlas = newAtlas;
                        textureSelect.textureGUID = newTextureGUID;
                        textureSelect.pivotOffset = newPivot;
                        textureSelect.useDefaultPivot = newUseDefaultPivot;

                        _baseBehaviour.textureSelectList.Add(textureSelect);
                    }
                    else
                    {
                        textureSelect = _baseBehaviour.textureSelectList[i];

                        newAtlas = textureSelect.atlas;
                        newTextureGUID = textureSelect.textureGUID;
                        newPivot = textureSelect.pivotOffset;
                        newUseDefaultPivot = textureSelect.useDefaultPivot;
                    }

                    textureSelectComponent.SetAtlas(newAtlas);
                    textureSelectComponent.SetTextureGUID(newTextureGUID);
                    textureSelectComponent.SetPivotOffset(newPivot, newUseDefaultPivot);

                    _textureSelectComponents.Add(textureSelectComponent);
                }
            }

            for (int ts = 0; ts < _baseBehaviour.textureSelectCount; ts++)
            {
                _textureSelectComponents[ts].OnGUI();
            }
        }

        private void DisplayTextureSearchReplaceComponents()
        {
            int newTextureSearchReplaceCount = EditorGUILayout.IntField("Texture Search Replace Count:", _baseBehaviour.textureSearchReplaceCount);
            if (newTextureSearchReplaceCount < _baseBehaviour.textureSearchReplaceCount)
            {
                _baseBehaviour.textureSearchReplaceCount = newTextureSearchReplaceCount;

                _textureSearchReplaceComponents.RemoveRange(_baseBehaviour.textureSearchReplaceCount, _textureSearchReplaceComponents.Count - _baseBehaviour.textureSearchReplaceCount);

                // clean up unused groups
                int index = _textureSearchReplaceComponents.Count;
                while (index < _baseBehaviour.textureSearchReplaceList.Count)
                {
                    if (_baseBehaviour.textureSearchReplaceList[index].searchAtlas == null
                        &&
                        _baseBehaviour.textureSearchReplaceList[index].searchTextureGUID == ""
                        &&
                        _baseBehaviour.textureSearchReplaceList[index].replaceAtlas == null
                        &&
                        _baseBehaviour.textureSearchReplaceList[index].replaceTextureGUID == ""
                        )
                    {
                        _baseBehaviour.textureSearchReplaceList.RemoveAt(index);
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            else if (newTextureSearchReplaceCount > _baseBehaviour.textureSearchReplaceCount)
            {
                int oldCount = _baseBehaviour.textureSearchReplaceCount;

                _baseBehaviour.textureSearchReplaceCount = newTextureSearchReplaceCount;

                TextureSearchReplaceComponent textureSearchReplaceComponent;
                TextureAtlas newSearchAtlas;
                string newSearchTextureGUID;
                TextureAtlas newReplaceAtlas;
                string newReplaceTextureGUID;
                Vector2 newReplacePivot;
                bool newReplaceUseDefaultPivot;
                TextureSearchReplace textureSearchReplace;
                for (int i = oldCount; i < newTextureSearchReplaceCount; i++)
                {
                    textureSearchReplaceComponent = new TextureSearchReplaceComponent(i, _baseBehaviour.gameObject);
                    textureSearchReplaceComponent.SetSearchAtlasChangedDelegate(SetSearchAtlas);
                    textureSearchReplaceComponent.SetSearchTextureChangedDelegate(SetSearchTextureGUID);
                    textureSearchReplaceComponent.SetReplaceAtlasChangedDelegate(SetReplaceAtlas);
                    textureSearchReplaceComponent.SetReplaceTextureChangedDelegate(SetReplaceTextureGUID);
                    textureSearchReplaceComponent.SetReplacePivotOffsetChangedDelegate(SetReplacePivotOffset);

                    if (i >= _baseBehaviour.textureSearchReplaceList.Count)
                    {
                        newSearchAtlas = null;
                        newSearchTextureGUID = "";

                        newReplaceAtlas = null;
                        newReplaceTextureGUID = "";
                        newReplacePivot = Vector2.zero;
                        newReplaceUseDefaultPivot = false;

                        textureSearchReplace = new TextureSearchReplace();

                        textureSearchReplace.searchAtlas = newSearchAtlas;
                        textureSearchReplace.searchTextureGUID = newSearchTextureGUID;

                        textureSearchReplace.replaceAtlas = newReplaceAtlas;
                        textureSearchReplace.replaceTextureGUID = newReplaceTextureGUID;
                        textureSearchReplace.replacePivotOffset = newReplacePivot;
                        textureSearchReplace.replaceUseDefaultPivot = newReplaceUseDefaultPivot;

                        _baseBehaviour.textureSearchReplaceList.Add(textureSearchReplace);
                    }
                    else
                    {
                        textureSearchReplace = _baseBehaviour.textureSearchReplaceList[i];

                        newSearchAtlas = textureSearchReplace.searchAtlas;
                        newSearchTextureGUID = textureSearchReplace.searchTextureGUID;

                        newReplaceAtlas = textureSearchReplace.replaceAtlas;
                        newReplaceTextureGUID = textureSearchReplace.replaceTextureGUID;
                        newReplacePivot = textureSearchReplace.replacePivotOffset;
                        newReplaceUseDefaultPivot = textureSearchReplace.replaceUseDefaultPivot;
                    }

                    textureSearchReplaceComponent.SetSearchAtlas(newSearchAtlas);
                    textureSearchReplaceComponent.SetSearchTextureGUID(newSearchTextureGUID);

                    textureSearchReplaceComponent.SetReplaceAtlas(newReplaceAtlas);
                    textureSearchReplaceComponent.SetReplaceTextureGUID(newReplaceTextureGUID);
                    textureSearchReplaceComponent.SetReplacePivotOffset(newReplacePivot, newReplaceUseDefaultPivot);

                    _textureSearchReplaceComponents.Add(textureSearchReplaceComponent);
                }
            }

            for (int ts = 0; ts < _baseBehaviour.textureSearchReplaceCount; ts++)
            {
                _textureSearchReplaceComponents[ts].OnGUI();
            }
        }

        void OnEnable()
        {
            //Resources.OnEnable();

            _baseBehaviour = (TextureFunctionMonoBehaviour)target;

            if (_baseBehaviour != null)
            {
                if (_baseBehaviour.textureSelectList == null)
                {
                    _baseBehaviour.textureSelectList = new List<TextureSelect>();
                }

                if (_textureSelectComponents == null)
                {
                    _textureSelectComponents = new List<TextureSelectComponent>();
                }
                else
                {
                    _textureSelectComponents.Clear();
                }

                TextureSelectComponent textureSelectComponent;
                for (int i = 0; i < _baseBehaviour.textureSelectList.Count; i++)
                {
                    textureSelectComponent = new TextureSelectComponent(_textureSelectComponents.Count, true);
                    textureSelectComponent.SetAtlasChangedDelegate(_baseBehaviour.gameObject, SetSelectAtlas);
                    textureSelectComponent.SetTextureChangedDelegate(SetSelectTextureGUID);
                    textureSelectComponent.SetPivotOffsetChangedDelegate(SetSelectPivotOffset);

                    textureSelectComponent.SetAtlas(_baseBehaviour.textureSelectList[i].atlas);
                    textureSelectComponent.SetTextureGUID(_baseBehaviour.textureSelectList[i].textureGUID);
                    textureSelectComponent.SetPivotOffset(_baseBehaviour.textureSelectList[i].pivotOffset, _baseBehaviour.textureSelectList[i].useDefaultPivot);

                    _textureSelectComponents.Add(textureSelectComponent);
                }


                if (_baseBehaviour.textureSearchReplaceList == null)
                {
                    _baseBehaviour.textureSearchReplaceList = new List<TextureSearchReplace>();
                }

                if (_textureSearchReplaceComponents == null)
                {
                    _textureSearchReplaceComponents = new List<TextureSearchReplaceComponent>();
                }
                else
                {
                    _textureSearchReplaceComponents.Clear();
                }

                TextureSearchReplaceComponent textureSearchReplaceComponent;
                for (int i = 0; i < _baseBehaviour.textureSearchReplaceList.Count; i++)
                {
                    textureSearchReplaceComponent = new TextureSearchReplaceComponent(_textureSearchReplaceComponents.Count, _baseBehaviour.gameObject);
                    textureSearchReplaceComponent.SetSearchAtlasChangedDelegate(SetSearchAtlas);
                    textureSearchReplaceComponent.SetSearchTextureChangedDelegate(SetSearchTextureGUID);
                    textureSearchReplaceComponent.SetReplaceAtlasChangedDelegate(SetReplaceAtlas);
                    textureSearchReplaceComponent.SetReplaceTextureChangedDelegate(SetReplaceTextureGUID);
                    textureSearchReplaceComponent.SetReplacePivotOffsetChangedDelegate(SetReplacePivotOffset);

                    textureSearchReplaceComponent.SetSearchAtlas(_baseBehaviour.textureSearchReplaceList[i].searchAtlas);
                    textureSearchReplaceComponent.SetSearchTextureGUID(_baseBehaviour.textureSearchReplaceList[i].searchTextureGUID);
                    textureSearchReplaceComponent.SetReplaceAtlas(_baseBehaviour.textureSearchReplaceList[i].replaceAtlas);
                    textureSearchReplaceComponent.SetReplaceTextureGUID(_baseBehaviour.textureSearchReplaceList[i].replaceTextureGUID);
                    textureSearchReplaceComponent.SetReplacePivotOffset(_baseBehaviour.textureSearchReplaceList[i].replacePivotOffset, _baseBehaviour.textureSearchReplaceList[i].replaceUseDefaultPivot);

                    _textureSearchReplaceComponents.Add(textureSearchReplaceComponent);
                }
            }
        }

        void OnDisable()
        {
            _baseBehaviour = null;
        }

        public void SetSelectAtlas(TextureSelectComponent textureSelectComponent, TextureAtlas newAtlas)
        {
            _baseBehaviour.textureSelectList[textureSelectComponent.index].atlas = newAtlas;
        }

        public void SetSelectTextureGUID(TextureSelectComponent textureSelectComponent, string newTextureGUID)
        {
            _baseBehaviour.textureSelectList[textureSelectComponent.index].textureGUID = newTextureGUID;
        }

        public void SetSelectPivotOffset(TextureSelectComponent textureSelectComponent, Vector2 newPivotOffset, bool newUseDefaultPivot)
        {
            _baseBehaviour.textureSelectList[textureSelectComponent.index].pivotOffset = newPivotOffset;
            _baseBehaviour.textureSelectList[textureSelectComponent.index].useDefaultPivot = newUseDefaultPivot;
        }


        public void SetSearchAtlas(TextureSelectComponent textureSelectComponent, TextureAtlas newAtlas)
        {
            _baseBehaviour.textureSearchReplaceList[textureSelectComponent.index].searchAtlas = newAtlas;
        }

        public void SetSearchTextureGUID(TextureSelectComponent textureSelectComponent, string newTextureGUID)
        {
            _baseBehaviour.textureSearchReplaceList[textureSelectComponent.index].searchTextureGUID = newTextureGUID;
        }

        public void SetReplaceAtlas(TextureSelectComponent textureSelectComponent, TextureAtlas newAtlas)
        {
            _baseBehaviour.textureSearchReplaceList[textureSelectComponent.index].replaceAtlas = newAtlas;
        }

        public void SetReplaceTextureGUID(TextureSelectComponent textureSelectComponent, string newTextureGUID)
        {
            _baseBehaviour.textureSearchReplaceList[textureSelectComponent.index].replaceTextureGUID = newTextureGUID;
        }

        public void SetReplacePivotOffset(TextureSelectComponent textureSelectComponent, Vector2 newPivotOffset, bool newUseDefaultPivot)
        {
            if (_baseBehaviour != null)
            {
                _baseBehaviour.textureSearchReplaceList[textureSelectComponent.index].replacePivotOffset = newPivotOffset;
                _baseBehaviour.textureSearchReplaceList[textureSelectComponent.index].replaceUseDefaultPivot = newUseDefaultPivot;
            }
        }
    }

}
