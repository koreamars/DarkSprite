using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace SmoothMoves
{
    static public class EditorHelper
    {
        public const string VERSION = "2.6.0";

        static public bool AutoUpdate
        {
            get
            {
                return (PlayerPrefs.GetInt("SmoothMoves_ControlPanel_AutoUpdate", 1) == 1);
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_ControlPanel_AutoUpdate", (value ? 1 : 0));
            }
        }

        static public bool LogUpdates
        {
            get
            {
                return (PlayerPrefs.GetInt("SmoothMoves_ControlPanel_LogUpdate", 0) == 1);
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_ControlPanel_LogUpdate", (value ? 1 : 0));
            }
        }

        //static public bool BuildPrefabs
        //{
        //    get
        //    {
        //        return (PlayerPrefs.GetInt("SmoothMoves_ControlPanel_BuildPrefabs", 1) == 1);
        //    }
        //    set
        //    {
        //        PlayerPrefs.SetInt("SmoothMoves_ControlPanel_BuildPrefabs", (value ? 1 : 0));
        //    }
        //}

        static public bool ShowWelcomeScreen
        {
            get
            {
                return (PlayerPrefs.GetInt("SmoothMoves_WelcomeScreen_ShowWelcomeScreen", 1) == 1);
            }
            set
            {
                PlayerPrefs.SetInt("SmoothMoves_WelcomeScreen_ShowWelcomeScreen", (value ? 1 : 0));
            }
        }

        static public void CreateAsset<T>(string assetName) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + assetName + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        static public float KeepAngleInBounds(float angle)
        {
            float percent360F = (angle / 360.0f);
            int percent360I = Mathf.FloorToInt(angle / 360.0f);
            return ((percent360F - (float)percent360I) * 360.0f);
        }

        static public bool LeftMouseButton(Event evt)
        {
            return (evt.button == 0 && (!evt.alt) && (!evt.command));
        }

        static public bool RightMouseButton(Event evt)
        {
            return (evt.button == 1 || (evt.button == 0 && evt.alt));
        }

        static public bool MiddleMouseButton(Event evt)
        {
            return (evt.button == 2 || (evt.button == 0 && evt.command));
        }

        static public string GenerateIncrementedIndexedName(string name)
        {
            string[] s = name.Split(" "[0]);
            string firstPartOfName = "";
            if (s.Length > 1)
            {
                for (int i = 0; i < s.Length - 1; i++)
                {
                    firstPartOfName += s[i] + " ";
                }

                try
                {
                    name = firstPartOfName + (Convert.ToInt16(s[s.Length - 1]) + 1).ToString();
                }
                catch
                {
                    name = name + " 2";
                }
            }
            else
            {
                name = name + " 2";
            }

            return name;
        }

        static public float RoundFloat(float f, int decimalPlaces)
        {
            float mult = Mathf.Pow(10, decimalPlaces);
            return (Mathf.Round(f * mult) / mult);
        }

        static public Vector2 Vector3ToVector2(Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        static public bool TransformedRectContains(Rect r, Matrix4x4 mat, Vector2 pos)
        {
            return (r.Contains(mat.inverse.MultiplyPoint3x4(new Vector3(pos.x, pos.y, 0))));
        }

        static public List<string> GetAllScenesInProject()
        {
            DirectoryInfo di;
            FileInfo[] allFiles;
            int assetPathIndex;
            string sceneName;

            di = new DirectoryInfo(Application.dataPath);
            allFiles = di.GetFiles("*.unity", SearchOption.AllDirectories);

            List<string> scenesInProject = new List<string>();

            foreach (FileInfo file in allFiles)
            {
                assetPathIndex = file.FullName.IndexOf("Assets/");

                if (assetPathIndex == -1)
                {
                    assetPathIndex = file.FullName.IndexOf(@"Assets\");
                }

                if (assetPathIndex >= 0)
                {
                    sceneName = file.FullName.Substring(assetPathIndex, file.FullName.Length - assetPathIndex);
                    scenesInProject.Add(sceneName);
                }
            }

            return scenesInProject;
        }

        static public string GetSceneName(string scenePath)
        {
            string [] o = scenePath.Split("/"[0]);

            if (o.Length == 1)
            {
                o = scenePath.Split(@"\"[0]);
            }

            if (o.Length > 0)
            {
                return o[o.Length - 1].Replace(".unity", "");
            }
            else
            {
                return "";
            }
        }
    }
}
