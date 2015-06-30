using UnityEngine;
using System;

namespace SmoothMoves
{
    [Serializable]
    public class TextureSelect
    {
        public TextureAtlas atlas;
        public string textureGUID;
        public Vector2 pivotOffset;
        public bool useDefaultPivot;
    }
}