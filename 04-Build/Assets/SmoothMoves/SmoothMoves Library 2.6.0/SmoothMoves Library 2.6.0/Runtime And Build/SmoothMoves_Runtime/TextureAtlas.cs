using UnityEngine;
using System.Collections.Generic;

namespace SmoothMoves
{
    public class TextureAtlas : ScriptableObject
    {
        [HideInInspector]
        public Material material;
        [HideInInspector]
        public List<Rect> uvs = new List<Rect>();
        [HideInInspector]
        public List<string> textureGUIDs = new List<string>();
        [HideInInspector]
        public List<Vector2> textureSizes = new List<Vector2>();
        [HideInInspector]
        public List<Vector2> defaultPivotOffsets = new List<Vector2>();
        [HideInInspector]
        public List<string> textureNames = new List<string>();
        [HideInInspector]
        public List<string> texturePaths = new List<string>();
        [HideInInspector]
        public string lastBuildID;
        [HideInInspector]
        public int needsRebuilt;
        [HideInInspector]
        public int dataVersion;
        [HideInInspector]
        public int maxAtlasSize;
        [HideInInspector]
        public int padding;
        [HideInInspector]
        public bool forceSquare;

        public int GetTextureIndex(string guid)
        {
            if (textureGUIDs != null)
                return textureGUIDs.IndexOf(guid);
            else
                return -1;
        }

        public string GetTextureGUIDFromName(string name)
        {
            if (textureNames != null && textureGUIDs != null)
            {
                int index = textureNames.IndexOf(name);

                if (index >= 0 && index < textureGUIDs.Count)
                    return textureGUIDs[index];
                else
                    return "";
            }
            else
                return "";
        }

        public string GetNameFromTextureGUID(string guid)
        {
            if (textureGUIDs != null && textureNames != null)
            {
                int index = textureGUIDs.IndexOf(guid);

                if (index >= 0 && index < textureNames.Count)
                    return textureNames[index];
                else
                    return "";
            }
            else
                return "";
        }

        public Vector2 GetTextureSize(int textureIndex)
        {
            if (textureIndex > -1 && textureIndex < textureSizes.Count)
            {
                return textureSizes[textureIndex];
            }

            return Vector2.zero;
        }

        public bool UpdateDataVersion()
        {
            // Versions
            // 0: Initial Release
            // 2: Added default pivot offset
            // 3: Added texture names

            int startingVersion = dataVersion;

            if (dataVersion < 2)
            {
                dataVersion = 2;
            }

            if (dataVersion == 2)
            {
                dataVersion = 3;
            }

            if (dataVersion == 3)
            {
                maxAtlasSize = PlayerPrefs.GetInt("SMAtlas.maxAtlasSize", 1024);
                padding = PlayerPrefs.GetInt("SMAtlas.atlasPadding", 1);
                forceSquare = (PlayerPrefs.GetInt("SMAtlas.forceSquare", 0) == 1 ? true : false);

                dataVersion = 4;
            }

            return (startingVersion != dataVersion);
        }

        public Vector2 LookupDefaultPivotOffset(int textureIndex)
        {
            if (textureIndex > -1 && textureIndex < defaultPivotOffsets.Count)
            {
                return defaultPivotOffsets[textureIndex];
            }

            return Vector2.zero;
        }

        public Vector2 LookupDefaultPivotOffset(string textureGUID)
        {
            return LookupDefaultPivotOffset(GetTextureIndex(textureGUID));
        }
    }
}
