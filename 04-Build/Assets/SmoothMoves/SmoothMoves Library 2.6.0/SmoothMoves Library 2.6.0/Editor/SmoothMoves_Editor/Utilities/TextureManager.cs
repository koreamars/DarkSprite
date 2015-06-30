using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace SmoothMoves
{	
    public class TextureDictionaryEntry
    {
        public Texture2D texture;
        public Vector2 size;

        public TextureDictionaryEntry(Texture2D tex, Vector2 siz)
        {
            texture = tex;
            size = siz;
        }
    }

    public class AtlasDictionaryEntry
    {
        public string lastBuildID;
        public List<string> textureGUIDs;

        public AtlasDictionaryEntry(string buildID, List<string> guids)
        {
            lastBuildID = buildID;
            textureGUIDs = guids;
        }
    }

	static public class TextureManager
	{
        static private bool _generatedDictionaries;
        static private Dictionary<string, TextureDictionaryEntry> _textureDictionary;
        static private Dictionary<TextureAtlas, AtlasDictionaryEntry> _atlasTextureDictionary;

        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }
        static public Dictionary<string, TextureDictionaryEntry> TextureDictionary { get { return _textureDictionary; } }
        static public Dictionary<TextureAtlas, AtlasDictionaryEntry> AtlasTextureDictionary { get { return _atlasTextureDictionary; } }
        static public bool GeneratedDictionaries { get { return _generatedDictionaries; } set { _generatedDictionaries = value; } }

        static public void OnEnable()
        {
            _generatedDictionaries = false;

            if (_textureDictionary == null)
                _textureDictionary = new Dictionary<string, TextureDictionaryEntry>();
            else
                _textureDictionary.Clear();

            if (_atlasTextureDictionary == null)
                _atlasTextureDictionary = new Dictionary<TextureAtlas, AtlasDictionaryEntry>();
            else
                _atlasTextureDictionary.Clear();
        }

        static public void GenerateTextureDictionary(bool forceGeneration)
        {
            if (_generatedDictionaries == true && !forceGeneration)
                return;

            if (editor == null)
                return;

            if (editor.boneAnimationData == null)
                return;

            if (editor.boneAnimationData.animationClips == null)
                return;

            if (_textureDictionary == null)
                _textureDictionary = new Dictionary<string, TextureDictionaryEntry>();
            else
                _textureDictionary.Clear();

            if (_atlasTextureDictionary == null)
                _atlasTextureDictionary = new Dictionary<TextureAtlas, AtlasDictionaryEntry>();
            else
                _atlasTextureDictionary.Clear();

            for (int clipIndex = 0; clipIndex < editor.boneAnimationData.animationClips.Count; clipIndex++)
            {
                foreach (AnimationClipBone bone in editor.boneAnimationData.animationClips[clipIndex].bones)
                {
                    foreach (KeyframeSM kf in bone.keyframes)
                    {
                        if (kf.atlas != null)
                        {
							if (kf.atlas.needsRebuilt == 0)
           					{
    	                        if (!_atlasTextureDictionary.ContainsKey(kf.atlas))
        	                    {
	                                AddAtlasToDictionary(kf.atlas);
								}
                            }
                        }
                    }
                }
            }

            _generatedDictionaries = true;
        }

		static private void AddAtlasToDictionary(TextureAtlas atlas)
		{
            Texture2D texture;
			int textureIndex;
			string textureGUID;
            List<string> textureGUIDs = new List<string>();
            SortedDictionary<string, int> sortedGUIDs = new SortedDictionary<string, int>();

            for (textureIndex = 0; textureIndex < atlas.textureGUIDs.Count; textureIndex++)
            {
                sortedGUIDs.Add(GetAssetNameFromPath(AssetDatabase.GUIDToAssetPath(atlas.textureGUIDs[textureIndex]), true), textureIndex);
            }
            
            foreach (KeyValuePair<string, int> kvp in sortedGUIDs)
            {
                textureIndex = kvp.Value;

                textureGUID = atlas.textureGUIDs[textureIndex];
				textureGUIDs.Add(textureGUID);

                if (!_textureDictionary.ContainsKey(textureGUID))
                {
					texture = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(textureGUID), typeof(Texture2D));
                    _textureDictionary.Add(textureGUID, new TextureDictionaryEntry(texture, atlas.textureSizes[textureIndex]));
                }
            }
			
			_atlasTextureDictionary.Add(atlas, new AtlasDictionaryEntry(atlas.lastBuildID, textureGUIDs));
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
		
        static public int GetTextureIndex(TextureAtlas atlas, string guid)
        {
            if (atlas == null)
                return -1;

            if (_atlasTextureDictionary.ContainsKey(atlas))
            {
                return _atlasTextureDictionary[atlas].textureGUIDs.IndexOf(guid);
            }
            else
            {
                return -1;
            }
        }
	}
}
