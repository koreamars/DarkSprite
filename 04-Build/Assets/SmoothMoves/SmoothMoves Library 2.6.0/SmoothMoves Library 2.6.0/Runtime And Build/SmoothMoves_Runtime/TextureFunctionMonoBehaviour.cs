using UnityEngine;
using System.Collections.Generic;

namespace SmoothMoves
{
    public class TextureFunctionMonoBehaviour : MonoBehaviour
    {
        [HideInInspector]
        public int textureSelectCount;
        [HideInInspector]
        public List<TextureSelect> textureSelectList;

        [HideInInspector]
        public int textureSearchReplaceCount;
        [HideInInspector]
        public List<TextureSearchReplace> textureSearchReplaceList;
    }
}