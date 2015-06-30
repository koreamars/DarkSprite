using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SmoothMoves
{
    [CustomEditor(typeof(BoneAnimation))]
    public class BoneAnimationInspector : Editor
    {
        [MenuItem("Tools/SmoothMoves/Tools/Control Panel")]
        static public void ShowControlPanel()
        {
            ControlPanel.ShowEditor();
        }

        private BoneAnimation _boneAnimation;

        public int _selectedBoneAnimationDataIndex = -1;
        public List<BoneAnimationData> _boneAnimationDataList;
        public List<string> _boneAnimationDataNameList;
        public List<string> _boneAnimationDataGUIDList;

        public override void OnInspectorGUI()
        {
            if (_boneAnimation != null)
            {
                //base.OnInspectorGUI();

                //if (_boneAnimationDataGUIDList != null)
                //{
                //    GUILayout.BeginHorizontal();
                //    GUILayout.Label("Data:");
                //    int newBoneAnimationDataIndex = EditorGUILayout.Popup(_selectedBoneAnimationDataIndex, _boneAnimationDataNameList.ToArray());
                //    if (newBoneAnimationDataIndex != _selectedBoneAnimationDataIndex)
                //    {
                //        _boneAnimation.animationDataGUID = _boneAnimationDataGUIDList[newBoneAnimationDataIndex];
                //        _selectedBoneAnimationDataIndex = newBoneAnimationDataIndex;

                //        EditorUtility.SetDirty(_boneAnimation);
                //    }
                //    GUILayout.EndHorizontal();
                //}

                BoneAnimationData newBoneAnimationData = (BoneAnimationData)EditorGUILayout.ObjectField("Data:", _boneAnimation.animationData, typeof(BoneAnimationData), false);
                if (newBoneAnimationData != _boneAnimation.animationData)
                {
                    _boneAnimation.animationData = newBoneAnimationData;
                    _boneAnimation.animationDataGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_boneAnimation.animationData));
                    EditorUtility.SetDirty(_boneAnimation);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(10.0f);
                bool newUpdateColors = GUILayout.Toggle(_boneAnimation.updateColors, "Update Colors");
                if (newUpdateColors != _boneAnimation.updateColors)
                {
                    _boneAnimation.updateColors = newUpdateColors;
                }
                GUILayout.EndHorizontal();

                if (EditorApplication.isPlaying
                        ||
                        EditorApplication.isPaused)
                {
                    Color newMeshColor = EditorGUILayout.ColorField("Mesh Color", _boneAnimation.mMeshColor);
                    if (newMeshColor != _boneAnimation.mMeshColor)
                    {
                        _boneAnimation.SetMeshColor(newMeshColor);
                    }
                }

                GUILayout.Space(10.0f);

                if (GUILayout.Button("Open Control Panel", GUILayout.Height(40.0f)))
                {
                    ShowControlPanel();
                }

                GUILayout.Space(5.0f);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Force Build This\n Bone Animation", GUILayout.Height(60.0f)))
                {
                    BuildHelper.UpdateSingleBoneAnimationAndData(_boneAnimation);
                }

                if (_boneAnimation != null)
                {
                    GUILayout.BeginVertical();

                    bool newUpdatePrefabs = GUILayout.Toggle(_boneAnimation.updatePrefabs, "Search Other Prefabs");
                    if (newUpdatePrefabs != _boneAnimation.updatePrefabs)
                    {
                        _boneAnimation.updatePrefabs = newUpdatePrefabs;
                        EditorUtility.SetDirty(_boneAnimation);
                    }

                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();
            }
        }

        //private void GetBoneAnimationDataList()
        //{
        //    DirectoryInfo di;
        //    FileInfo[] allFiles;
        //    int assetPathIndex;
        //    string assetPath;
        //    BoneAnimationData ad;

        //    if (_boneAnimationDataNameList == null)
        //    {
        //        _boneAnimationDataNameList = new List<string>();
        //    }
        //    else
        //    {
        //        _boneAnimationDataNameList.Clear();
        //    }

        //    if (_boneAnimationDataList == null)
        //    {
        //        _boneAnimationDataList = new List<BoneAnimationData>();
        //    }
        //    else
        //    {
        //        _boneAnimationDataList.Clear();
        //    }

        //    if (_boneAnimationDataGUIDList == null)
        //    {
        //        _boneAnimationDataGUIDList = new List<string>();
        //    }
        //    else
        //    {
        //        _boneAnimationDataGUIDList.Clear();
        //    }

        //    di = new DirectoryInfo(Application.dataPath);
        //    allFiles = di.GetFiles("*.asset", SearchOption.AllDirectories);
        //    foreach (FileInfo file in allFiles)
        //    {
        //        assetPathIndex = file.FullName.IndexOf("Assets/");

        //        if (assetPathIndex == -1)
        //        {
        //            assetPathIndex = file.FullName.IndexOf(@"Assets\");
        //        }

        //        if (assetPathIndex >= 0)
        //        {
        //            assetPath = file.FullName.Substring(assetPathIndex, file.FullName.Length - assetPathIndex);
        //            ad = (BoneAnimationData)AssetDatabase.LoadAssetAtPath(assetPath, typeof(BoneAnimationData));

        //            if (ad != null)
        //            {
        //                _boneAnimationDataList.Add(ad);
        //                _boneAnimationDataNameList.Add(ad.name);
        //                _boneAnimationDataGUIDList.Add(AssetDatabase.AssetPathToGUID(assetPath));
        //            }
        //        }

        //    }
        //}

        public void OnEnable()
        {
            _boneAnimation = (BoneAnimation)target;

            if (_boneAnimation != null)
            {
                if (_boneAnimation.animationData == null)
                {
                    if (!string.IsNullOrEmpty(_boneAnimation.animationDataGUID))
                    {
                        _boneAnimation.animationData = (BoneAnimationData)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_boneAnimation.animationDataGUID), typeof(BoneAnimationData));
                        EditorUtility.SetDirty(_boneAnimation);
                    }
                }

                //GetBoneAnimationDataList();

                //if (_boneAnimation.animationData != null)
                //{
                //    _selectedBoneAnimationDataIndex = _boneAnimationDataList.IndexOf(_boneAnimation.animationData);
                //    _boneAnimation.animationDataGUID = _boneAnimationDataGUIDList[_selectedBoneAnimationDataIndex];
                //    _boneAnimation.animationData = null;
                //}
                //else
                //{
                //    _selectedBoneAnimationDataIndex = _boneAnimationDataGUIDList.IndexOf(_boneAnimation.animationDataGUID);
                //    if (_selectedBoneAnimationDataIndex == -1)
                //        _selectedBoneAnimationDataIndex = 0;

                //    _boneAnimation.animationDataGUID = _boneAnimationDataGUIDList[_selectedBoneAnimationDataIndex];
                //    EditorUtility.SetDirty(_boneAnimation);
                //}
            }
        }

        public void OnDisable()
        {
            _boneAnimation = null;
        }
    }
}
