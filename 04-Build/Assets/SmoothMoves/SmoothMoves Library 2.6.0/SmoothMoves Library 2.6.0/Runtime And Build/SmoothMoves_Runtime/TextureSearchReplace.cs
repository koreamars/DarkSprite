using UnityEngine;
using System;

namespace SmoothMoves
{
    [Serializable]
    public class TextureSearchReplace
    {
        public TextureAtlas searchAtlas;
        public string searchTextureGUID;

        public TextureAtlas replaceAtlas;
        public string replaceTextureGUID;
        public Vector2 replacePivotOffset;
        public bool replaceUseDefaultPivot;
    }
}
