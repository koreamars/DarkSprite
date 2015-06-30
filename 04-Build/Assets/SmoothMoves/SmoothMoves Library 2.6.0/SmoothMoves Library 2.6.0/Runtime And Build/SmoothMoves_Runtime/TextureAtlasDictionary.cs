using System;
using System.Collections.Generic;

namespace SmoothMoves
{
    [Serializable]
	public class TextureAtlasDictionary
	{
        public List<string> keys;
        public List<TextureAtlas> data;

        public TextureAtlasDictionary()
        {
            keys = new List<string>();
            data = new List<TextureAtlas>();
        }

        public void Add(string k, TextureAtlas d)
        {
            keys.Add(k);
            data.Add(d);
        }

        public void Clear()
        {
            keys.Clear();
            data.Clear();
        }

        public bool ContainsKey(string key)
        {
            foreach (string k in keys)
            {
                if (key == k)
                    return true;
            }

            return false;
        }

        public Dictionary<string, TextureAtlas> ToDictionary()
        {
            if (keys == null)
                return null;

            Dictionary<string, TextureAtlas> dict = new Dictionary<string, TextureAtlas>();

            for (int index = 0; index < keys.Count; index++)
            {
                dict.Add(keys[index], data[index]);
            }

            return dict;
        }

        public TextureAtlas[] ToArray()
        {
            TextureAtlas[] outputArray = new TextureAtlas[data.Count];

            for (int textureAtlasIndex=0; textureAtlasIndex < data.Count; textureAtlasIndex++)
            {
                outputArray[textureAtlasIndex] = data[textureAtlasIndex];
            }

            return outputArray;
        }
	}
}
