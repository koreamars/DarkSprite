using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

namespace SmoothMoves
{
	static public class BuildHelper
    {
        private const string UPDATE_PROGRESS_TITLE = "SmoothMoves Animation Update";
        private const string RESOURCES_EXTENSION = "_Resources";

        static private float BAKE_NODE_OFFSET = 0.001f;

        static public bool RunningUpdate = false;

        static private float Depth0ZOffset
        {
            get
            {
                return PlayerPrefs.GetFloat("SmoothMoves_Editor_Depth0ZOffset", 0);
            }
        }

        static private float DepthMaxZOffset
        {
            get
            {
                return PlayerPrefs.GetFloat("SmoothMoves_Editor_DepthMaxZOffset", 0.05f);
            }
        }

        static public string GetSceneName(string scenePath)
        {
            string[] o = scenePath.Split("/"[0]);

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

        static public bool UpdateSingleBoneAnimationAndData(BoneAnimation boneAnimation)
        {
            try
            {
                BoneAnimationData boneAnimationData = GetBoneAnimationDataFromBoneAnimation(boneAnimation);

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, "Updating animation data", boneAnimationData, "", 0, 1.0f, 0);

                if (LogUpdates)
                    Debug.Log("BoneAnimation Data: updating [" + boneAnimationData.name + "]");

                int animationDataUpdateCount;
                int prefabUpdateCount;
                int totalAnimationClipCount;
                int sceneBoneAnimationUpdateCount;

                if (!UpdateBoneAnimationData(boneAnimationData, 0, 0, out totalAnimationClipCount))
                {
                    HideProgress();
                    return false;
                }

                if (!UpdateBoneAnimation(boneAnimation))
                {
                    HideProgress();
                    return false;
                }

                if (boneAnimation.updatePrefabs)
                {
                    if (!UpdateAnimationDataAndPrefabs(boneAnimationData, true, out animationDataUpdateCount, out prefabUpdateCount, out totalAnimationClipCount))
                    {
                        HideProgress();
                        return false;
                    }
                }

                if (!UpdateSceneBoneAnimations(boneAnimationData, true, out sceneBoneAnimationUpdateCount))
                {
                    HideProgress();
                    return false;
                }

                DisplayProgress(UPDATE_PROGRESS_TITLE, true, "Updating scene BoneAnimation", boneAnimation, "", 0, 1.0f, 0);

                if (LogUpdates)
                    Debug.Log("Scene [" + GetSceneName(EditorApplication.currentScene) + "] BoneAnimation Scene Object: updating [" + boneAnimation.name + "]");

                EditorApplication.SaveAssets();
                AssetDatabase.SaveAssets();

                HideProgress();
            }
            catch (Exception ex)
            {
                Debug.LogError("UpdateSingleBoneAnimationAndData Error: " + ex.Message);

                HideProgress();
                return false;
            }

            return true;
        }

        static public bool UpdateBoneAnimationsAndDataInAllScenes()
        {
            List<string> sceneNames = GetAllScenesInProject();
            string currentScene = EditorApplication.currentScene;
            int animationDataUpdateCount;
            int totalAnimationClipCount;
            int prefabUpdateCount;
            int sceneBoneAnimationUpdateCount;
            int totalSceneBoneAnimationUpdateCount = 0;

            if (sceneNames.Count == 0)
                return true;

            if (!EditorApplication.SaveCurrentSceneIfUserWantsTo())
                return true;

            RunningUpdate = true;

            if (!UpdateAnimationDataAndPrefabs(null, true, out animationDataUpdateCount, out prefabUpdateCount, out totalAnimationClipCount))
                return false;

            for (int sceneIndex = 0; sceneIndex < sceneNames.Count; sceneIndex++)
            {
                EditorApplication.OpenScene(sceneNames[sceneIndex]);

                if (!UpdateSceneBoneAnimations(null, true, out sceneBoneAnimationUpdateCount))
                    return false;
                totalSceneBoneAnimationUpdateCount += sceneBoneAnimationUpdateCount;

                EditorApplication.SaveScene();
            }

            if (currentScene != sceneNames[sceneNames.Count - 1])
                EditorApplication.OpenScene(currentScene);

            EditorApplication.SaveAssets();
            EditorApplication.SaveScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("SmoothMoves Force-Build Complete",
                                        "Successfully completed the force-build.\n\n" +
                                        "Stats:\n" +
                                        "___________________________________________\n" +
                                        "\n" +
                                        "BoneAnimationData assets updated: " + animationDataUpdateCount.ToString() + "\n" +
                                        "Total animation clips created: " + totalAnimationClipCount.ToString() + "\n" +
                                        "\n" +
                                        "BoneAnimation prefabs updated: " + prefabUpdateCount.ToString() + "\n" +
                                        "\n" +
                                        "Scenes Processed: " + sceneNames.Count.ToString() + "\n" +
                                        "Total BoneAnimation scene objects updated: " + totalSceneBoneAnimationUpdateCount.ToString() + "\n",
                                        "OK");

            RunningUpdate = false;

            return true;
        }

        static public bool UpdateBoneAnimationsAndDataInCurrentScene(bool forceUpdate)
        {
            RunningUpdate = true;

            int animationDataUpdateCount;
            int totalAnimationClipCount;
            int prefabUpdateCount;
            int sceneBoneAnimationUpdateCount;

            if (!UpdateAnimationDataAndPrefabs(null, forceUpdate, out animationDataUpdateCount, out prefabUpdateCount, out totalAnimationClipCount))
                return false;
            if (!UpdateSceneBoneAnimations(null, forceUpdate, out sceneBoneAnimationUpdateCount))
                return false;

            EditorApplication.SaveAssets();
            EditorApplication.SaveScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            RunningUpdate = false;

            return true;
        }

        static private bool UpdateAnimationDataAndPrefabs(BoneAnimationData updateOnlyBoneAnimationData, bool forceUpdate, out int animationDataUpdateCount, out int prefabUpdateCount, out int totalAnimationClipCount)
        {
            animationDataUpdateCount = 0;
            prefabUpdateCount = 0;
            totalAnimationClipCount = 0;

            try
            {
                DirectoryInfo di;
                FileInfo[] allFiles;
                int assetPathIndex;
                string assetPath;
                BoneAnimationData ad;
                int progressValue = 0;
                float progress;
                float progressChunkSize;
                //List<BoneAnimationData> animationDatasToUpdate = new List<BoneAnimationData>();
                float animationDataProgressStart = 0;
                float animationDataProgressWidth = 0.60f;
                float prefabProgressStart = animationDataProgressWidth;
                float prefabProgressWidth = 0.15f;
                int animationClipCount;
                DirectoryInfo resourcesDirectoryInfo;
                List<string> boneAnimationDataPathList = new List<string>();
                BoneAnimationData boneAnimationData = null;

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, "Gathering animation data", null, "", 0, 1.0f, 0);

                // gather all the BoneAnimationData asset files in the project

                di = new DirectoryInfo(Application.dataPath);
                allFiles = di.GetFiles("*.asset", SearchOption.AllDirectories);
                foreach (FileInfo file in allFiles)
                {
                    assetPathIndex = file.FullName.IndexOf("Assets/");

                    if (assetPathIndex == -1)
                    {
                        assetPathIndex = file.FullName.IndexOf(@"Assets\");
                    }

                    if (assetPathIndex >= 0)
                    {
                        assetPath = file.FullName.Substring(assetPathIndex, file.FullName.Length - assetPathIndex);
                        ad = (BoneAnimationData)AssetDatabase.LoadAssetAtPath(assetPath, typeof(BoneAnimationData));

                        if ((ad != null) && (ad == updateOnlyBoneAnimationData || updateOnlyBoneAnimationData == null))
                        {
                            // the asset file contained a BoneAnimationData
                            // and the BoneAnimationData matched the data we are trying to update 
                            // (or we are updating all animation data)

                            resourcesDirectoryInfo = new DirectoryInfo(Path.Combine(new FileInfo(AssetDatabase.GetAssetPath(ad)).Directory.FullName, ad.name + RESOURCES_EXTENSION));

                            if (ad.needsRebuilt || forceUpdate || !resourcesDirectoryInfo.Exists)
                            {
                                // if this animation data needs rebuilt
                                // or we are force updating
                                // or the resources folder is missing,
                                // add this data to the list to process

                                boneAnimationDataPathList.Add(assetPath);
                            }

                            ad = null;

                            EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences();
                        }
                    }

                }

                animationDataUpdateCount = boneAnimationDataPathList.Count;

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, "Updating animation data", null, "", 0, 1.0f, animationDataProgressWidth);
                progressValue = 0;

                // calculate the progress chunk size to pass to the UpdateBoneAnimationData function

                if (boneAnimationDataPathList.Count > 0)
                {
                    progressChunkSize = animationDataProgressWidth / boneAnimationDataPathList.Count;
                }
                else
                {
                    progressChunkSize = 0;
                }

                // process the animation data list

                for (int adPathIndex = 0; adPathIndex < boneAnimationDataPathList.Count; adPathIndex++)
                {
                    boneAnimationData = (BoneAnimationData)AssetDatabase.LoadAssetAtPath(boneAnimationDataPathList[adPathIndex], typeof(BoneAnimationData));

                    progress = animationDataProgressStart + ((float)progressValue / (float)boneAnimationDataPathList.Count) * animationDataProgressWidth;
                    DisplayProgress(UPDATE_PROGRESS_TITLE, false, "Updating animation data", boneAnimationData, "", 0, 1.0f, progress);

                    if (LogUpdates)
                        Debug.Log("BoneAnimation Data: updating [" + boneAnimationData.name + "]");

                    if (!UpdateBoneAnimationData(boneAnimationData, progress, progressChunkSize, out animationClipCount))
                    {
                        HideProgress();
                        return false;
                    }
                    totalAnimationClipCount += animationClipCount;

                    boneAnimationData = null;
                    EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences();

                    progressValue++;
                }


                // update bone animation prefabs

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, "Updating BoneAnimations", null, "", 0, 1.0f, prefabProgressStart);

                List<string> boneAnimationPathList = new List<string>();

                // gather the BoneAnimation prefabs from the project
                // note that we can't use FindObjectsOfTypeAll since that only accesses
                // assets that have been loaded into RAM. Instead we'll 
                // use the file system.

                di = new DirectoryInfo(Application.dataPath);
                allFiles = di.GetFiles("*.prefab", SearchOption.AllDirectories);
                BoneAnimation boneAnimation;
                for (int fileIndex = 0; fileIndex < allFiles.Length; fileIndex++)
                {
                    DisplayProgress(UPDATE_PROGRESS_TITLE, false, "Updating BoneAnimations - Checking asset [" + fileIndex.ToString() + " of " + allFiles.Length.ToString() + "]", null, "", 0, 1.0f, prefabProgressStart);

                    assetPathIndex = allFiles[fileIndex].FullName.IndexOf("Assets/");

                    if (assetPathIndex == -1)
                    {
                        assetPathIndex = allFiles[fileIndex].FullName.IndexOf(@"Assets\");
                    }

                    if (assetPathIndex >= 0)
                    {
                        assetPath = allFiles[fileIndex].FullName.Substring(assetPathIndex, allFiles[fileIndex].FullName.Length - assetPathIndex);
                        boneAnimation = (BoneAnimation)AssetDatabase.LoadAssetAtPath(assetPath, typeof(BoneAnimation));

                        if (boneAnimation != null)
                        {
                            // there was a boneanimation script on this object

                            boneAnimationData = GetBoneAnimationDataFromBoneAnimation(boneAnimation);

                            if (boneAnimationData != null && (updateOnlyBoneAnimationData == null || boneAnimationData == updateOnlyBoneAnimationData))
                            {
                                // the animation data is not null
                                // and the animation data matches what we are looking for
                                // (or we are updating all prefabs)

                                if (PrefabUtility.GetPrefabType(boneAnimation) == PrefabType.Prefab)
                                {
                                    // the asset is a prefab

                                    if (
                                        (boneAnimationData.buildID != boneAnimation.buildID)
                                        ||
                                        forceUpdate
                                        )
                                    {
                                        // the BoneAnimation needs updating
                                        // or we are force updating everything
                                        // then add to the list to process

                                        boneAnimationPathList.Add(assetPath);
                                    }
                                }
                            }
                        }

                        boneAnimation = null;
                        boneAnimationData = null;

                        EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences();
                    }

                }

                prefabUpdateCount = boneAnimationPathList.Count;

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, "Updating prefabs", null, "", 0, 1.0f, prefabProgressStart);
                progressValue = 0;

                // update the prefabs

                for (int pathIndex = 0; pathIndex < boneAnimationPathList.Count; pathIndex++)
                {
                    boneAnimation = (BoneAnimation)AssetDatabase.LoadAssetAtPath(boneAnimationPathList[pathIndex], typeof(BoneAnimation));

                    progress = prefabProgressStart + (((float)progressValue / (float)boneAnimationPathList.Count) * prefabProgressWidth);
                    DisplayProgress(UPDATE_PROGRESS_TITLE, false, "Updating prefab", boneAnimation, "", 0, 1.0f, progress);

                    if (LogUpdates)
                        Debug.Log("BoneAnimation Prefab: updating [" + boneAnimation.name + "]");

                    if (!UpdateBoneAnimation(boneAnimation))
                        return false;

                    boneAnimation = null;
                    EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences();

                    progressValue++;
                }

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, "Saving Changes", null, "", 0, 1.0f, 0.98f);
            }
            catch (Exception ex)
            {
                Debug.LogError("UpdateAnimationDataAndPrefabs Error: " + ex.Message);

                HideProgress();
                return false;
            }

            HideProgress();
            return true;
        }

        static public bool UpdateSceneBoneAnimationMeshes()
        {
            List<BoneAnimation> boneAnimationList = new List<BoneAnimation>();
            BoneAnimation boneAnimation;
            BoneAnimationData boneAnimationData;
            UnityEngine.Object[] objectList = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(BoneAnimation));
            foreach (UnityEngine.Object obj in objectList)
            {
                boneAnimation = (BoneAnimation)obj;
                boneAnimationData = GetBoneAnimationDataFromBoneAnimation(boneAnimation);

                if (boneAnimationData != null && boneAnimation.mRenderer != null)
                {
                    CreateMeshFromAnimationData(boneAnimation);
                }
            }

            return true;
        }

        static private bool UpdateSceneBoneAnimations(BoneAnimationData updateOnlyBoneAnimationData, bool forceUpdate, out int sceneBoneAnimationUpdateCount)
        {
            sceneBoneAnimationUpdateCount = 0;

            try
            {
                int progressValue;
                float progress;
                bool parentDifferent;

                float sceneProgressStart = 0.75f;
                float sceneProgressWidth = 0.20f;

                // gather the BoneAnimations from the scene

                List<BoneAnimation> boneAnimationList = new List<BoneAnimation>();
                BoneAnimation boneAnimation;
                BoneAnimationData boneAnimationData;
                BoneAnimationData parentBoneAnimationData;
                UnityEngine.Object[] objectList = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(BoneAnimation));
                foreach (UnityEngine.Object obj in objectList)
                {
                    boneAnimation = (BoneAnimation)obj;
                    boneAnimationData = GetBoneAnimationDataFromBoneAnimation(boneAnimation);

                    if (boneAnimationData != null && (updateOnlyBoneAnimationData == null || boneAnimationData == updateOnlyBoneAnimationData))
                    {
                        // the animation data is not null
                        // and the animation data matches what we are looking for
                        // (or we are updating everything)

                        if (PrefabUtility.GetPrefabType(boneAnimation) != PrefabType.Prefab)
                        {
                            // the object is not a prefab

                            // default the parent prefab's animation data being the same
                            // as this BoneAnimation's data
                            parentDifferent = false;

                            // check to see if the prefab data has a different animation data
                            // (if applicable)
                            switch (PrefabUtility.GetPrefabType(boneAnimation))
                            {
                                case PrefabType.PrefabInstance:
                                case PrefabType.DisconnectedPrefabInstance:

                                    // if the object is a prefab instance or a disconnected prefab instance
                                    // we will check to see if the parent prefab has a different animation data 
                                    // set.

                                    GameObject parentGameObject = (GameObject)PrefabUtility.GetPrefabParent(boneAnimation.gameObject);
                                    if (parentGameObject != null)
                                    {
                                        // found a parent prefab

                                        BoneAnimation parentBoneAnimation = parentGameObject.GetComponent<BoneAnimation>();
                                        if (parentBoneAnimation != null)
                                        {
                                            // found the BoneAnimation of the parent prefab

                                            parentBoneAnimationData = GetBoneAnimationDataFromBoneAnimation(parentBoneAnimation);

                                            if (parentBoneAnimationData != boneAnimationData)
                                            {
                                                // if the parent prefab's animation data is different,
                                                // then we flag this

                                                parentDifferent = true;
                                            }
                                        }
                                    }
                                    break;
                            }

                            if (
                                (boneAnimationData.buildID != boneAnimation.buildID)
                                ||
                                parentDifferent
                                ||
                                forceUpdate
                                )
                            {
                                // if the BoneAnimation build doesn't match the animation data build
                                // or the parent prefab's animation data is different than the BoneAnimation's data
                                // or we are force updating,
                                // then add to the list to process

                                boneAnimationList.Add(boneAnimation);
                            }
                        }
                    }
                }

                sceneBoneAnimationUpdateCount = boneAnimationList.Count;

                DisplayProgress(UPDATE_PROGRESS_TITLE, true, "Updating scene BoneAnimations", null, "", 0, 1.0f, sceneProgressStart);
                progressValue = 0;

                // update scene BoneAnimations

                foreach (BoneAnimation ba in boneAnimationList)
                {
                    progress = sceneProgressStart + (((float)progressValue / (float)boneAnimationList.Count) * sceneProgressWidth);
                    DisplayProgress(UPDATE_PROGRESS_TITLE, true, "Updating scene BoneAnimation", ba, "", 0, 1.0f, progress);

                    if (LogUpdates)
                        Debug.Log("Scene [" + GetSceneName(EditorApplication.currentScene) + "] BoneAnimation Scene Object: updating [" + ba.name + "]");

                    if (!UpdateBoneAnimation(ba))
                        return false;

                    progressValue++;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("UpdateSceneBoneAnimations Error: " + ex.Message);

                HideProgress();
                return false;
            }

            HideProgress();
            return true;
        }

        static private bool UpdateBoneAnimationData(BoneAnimationData boneAnimationData, float progressStart, float progressChunkSize, out int animationClipCount)
        {
            animationClipCount = 0;

            try
            {
                // Do some quick data checks before building the mesh

                if (boneAnimationData.animationClips.Count == 0)
                {
                    Debug.LogWarning("No animation clips were found for this animation data asset");
                    return false;
                }

                if (boneAnimationData.boneDataList.Count == 0)
                {
                    Debug.LogWarning("No bones were found for this animation data asset");
                    return false;
                }

                float progress;
                string progressDescription = "Updating animation data";

                string resourcesPath = null;
                string animationAssetPath;

                // create the directory for the animation clips
                string newFolderName = boneAnimationData.name + RESOURCES_EXTENSION;
                Directory.CreateDirectory(Path.Combine(new FileInfo(AssetDatabase.GetAssetPath(boneAnimationData)).Directory.FullName, newFolderName));
                resourcesPath = AssetDatabase.GetAssetPath(boneAnimationData).Replace(boneAnimationData.name + ".asset", "");
                resourcesPath = Path.Combine(resourcesPath, newFolderName);

                // Update the animation data to the latest version
                boneAnimationData.UpdateDataVersion();

                Dictionary<int, AnimationClipTriggerFrames> animationClipTriggerFrames;

                // Reset data
                animationClipTriggerFrames = new Dictionary<int, AnimationClipTriggerFrames>();

                // build texture atlas dictionary
                if (boneAnimationData.textureAtlasDictionary == null)
                {
                    boneAnimationData.textureAtlasDictionary = new TextureAtlasDictionary();
                }
                else
                {
                    boneAnimationData.textureAtlasDictionary.Clear();
                }

                AnimationClipBone animationClipBone;
                KeyframeSM keyframe;
                KeyframeSM.KEYFRAME_TYPE lastKeyframeType;
                TextureAtlas lastAtlas;
                string lastTextureGUID;
                Vector2 lastPivotOffset;
                int lastDepth;
                ColliderSM lastCollider;
                KeyframeSM.KEYFRAME_TYPE currentKeyframeType;
                TextureAtlas currentAtlas;
                string currentTextureGUID;
                bool atlasChanged;
                bool textureGUIChanged;
                Vector2 currentPivotOffset;
                bool currentUseDefaultPivot;
                int currentDepth;
                ColliderSM currentCollider;
                AnimationClip animationClip;
                string bonePath;
                string spriteBonePath;
                AnimationClipSM clip;
                int bnIndex;
                TriggerFrame triggerFrame;
                int matIndex;
                int frame;

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, progressDescription, boneAnimationData, "Building material dictionary", progressStart, progressChunkSize, 0.1f);

                // Build a dictionary of materials
                boneAnimationData.materialSource = new List<Material>();
                for (int clipIndex = 0; clipIndex < boneAnimationData.animationClips.Count; clipIndex++)
                {
                    foreach (AnimationClipBone bone in boneAnimationData.animationClips[clipIndex].bones)
                    {
                        foreach (KeyframeSM kf in bone.keyframes)
                        {
                            if (kf.useKeyframeType && kf.keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                            {
                                if (kf.atlas != null && kf.textureGUID != "")
                                {
                                    if (!boneAnimationData.materialSource.Contains(kf.atlas.material))
                                    {
                                        boneAnimationData.materialSource.Add(kf.atlas.material);
                                    }

                                    if (!boneAnimationData.textureAtlasDictionary.ContainsKey(kf.atlas.name))
                                    {
                                        boneAnimationData.textureAtlasDictionary.Add(kf.atlas.name, kf.atlas);
                                    }
                                }
                            }
                        }
                    }
                }

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, progressDescription, boneAnimationData, "Building matrices and normals", progressStart, progressChunkSize, 0.15f);

                boneAnimationData.bindPoses = new Matrix4x4[boneAnimationData.dfsBoneNodeList.Count * 2];
                for (int boneNodeIndex = 0; boneNodeIndex < (boneAnimationData.dfsBoneNodeList.Count * 2); boneNodeIndex++)
                {
                    boneAnimationData.bindPoses[boneNodeIndex] = Matrix4x4.identity;
                }

                boneAnimationData.normals = new Vector3[boneAnimationData.dfsBoneNodeList.Count * 4];
                for (int boneNodeIndex = 0; boneNodeIndex < boneAnimationData.dfsBoneNodeList.Count; boneNodeIndex++)
                {
                    SetNormalData(ref boneAnimationData.normals, (boneNodeIndex * 4), Vector3.back);
                }

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, progressDescription, boneAnimationData, "Building triggers", progressStart, progressChunkSize, 0.2f);

                // build triggers
                for (int clipIndex = 0; clipIndex < boneAnimationData.animationClips.Count; clipIndex++)
                {
                    clip = boneAnimationData.animationClips[clipIndex];

                    for (int boneDataIndex = 0; boneDataIndex < clip.bones.Count; boneDataIndex++)
                    {
                        bnIndex = boneAnimationData.GetBoneNodeIndex(boneDataIndex);
                        animationClipBone = clip.bones[boneDataIndex];

                        if (!clip.mix || (clip.mix && animationClipBone.mixTransform))
                        {
                            lastKeyframeType = KeyframeSM.KEYFRAME_TYPE.TransformOnly;
                            lastAtlas = null;
                            lastTextureGUID = "";
                            lastPivotOffset = Vector2.zero;
                            lastDepth = 0;
                            lastCollider = null;

                            currentKeyframeType = KeyframeSM.KEYFRAME_TYPE.TransformOnly;
                            currentAtlas = null;
                            currentTextureGUID = "";
                            currentPivotOffset = Vector2.zero;
                            currentUseDefaultPivot = false;
                            currentDepth = 0;
                            currentCollider = null;

                            for (int keyframeIndex = 0; keyframeIndex < animationClipBone.keyframes.Count; keyframeIndex++)
                            {
                                keyframe = animationClipBone.keyframes[keyframeIndex];
                                matIndex = GetMaterialIndex(ref boneAnimationData.materialSource, keyframe.atlas);

                                frame = keyframe.frame;

                                atlasChanged = false;
                                textureGUIChanged = false;

                                if (keyframeIndex == 0)
                                {
                                    triggerFrame = AddTriggerFrame(ref animationClipTriggerFrames, clipIndex, frame);

                                    currentKeyframeType = keyframe.keyframeType;
                                    currentAtlas = keyframe.atlas;
                                    currentTextureGUID = keyframe.textureGUID;
                                    currentPivotOffset = keyframe.pivotOffset;
                                    currentUseDefaultPivot = keyframe.useDefaultPivot;
                                    currentDepth = keyframe.depth;
                                    currentCollider = keyframe.collider;

                                    if (currentKeyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                                    {
                                        if (keyframe.useDefaultPivot)
                                        {
                                            if (currentAtlas != null)
                                                currentPivotOffset = currentAtlas.LookupDefaultPivotOffset(keyframe.textureGUID);
                                            else
                                                currentPivotOffset = lastPivotOffset;
                                        }
                                        else
                                        {
                                            currentPivotOffset = keyframe.pivotOffset;
                                        }

                                        triggerFrame.AddTriggerFrameBoneMaterialChangeOriginal(bnIndex, matIndex);
                                        triggerFrame.AddTriggerFrameBoneTextureChangeOriginal(bnIndex, currentAtlas, currentTextureGUID, currentPivotOffset, matIndex);
                                        triggerFrame.AddTriggerFrameBoneDepthChange(bnIndex, currentDepth);
                                    }
                                    else
                                    {
                                        triggerFrame.AddTriggerFrameBoneHideTexture(bnIndex);
                                    }

                                    triggerFrame.AddTriggerFrameBoneColliderChange(bnIndex, (keyframe.useCollider ? keyframe.collider : null));
                                }
                                else
                                {
                                    if (keyframe.useKeyframeType)
                                        currentKeyframeType = keyframe.keyframeType;
                                    else
                                        currentKeyframeType = lastKeyframeType;

                                    if (keyframe.useAtlas)
                                    {
                                        if (currentAtlas != keyframe.atlas)
                                            atlasChanged = true;

                                        currentAtlas = keyframe.atlas;
                                    }
                                    else
                                    {
                                        currentAtlas = lastAtlas;
                                    }

                                    if (keyframe.useTextureGUID)
                                    {
                                        if (currentTextureGUID != keyframe.textureGUID)
                                            textureGUIChanged = true;

                                        currentTextureGUID = keyframe.textureGUID;
                                    }
                                    else
                                    {
                                        currentTextureGUID = lastTextureGUID;
                                    }

                                    if (keyframe.usePivotOffset)
                                    {
                                        if (keyframe.useDefaultPivot)
                                        {
                                            if (currentAtlas != null)
                                                currentPivotOffset = currentAtlas.LookupDefaultPivotOffset(currentTextureGUID);
                                            else
                                                currentPivotOffset = lastPivotOffset;
                                        }
                                        else
                                        {
                                            currentPivotOffset = keyframe.pivotOffset;
                                        }

                                        currentUseDefaultPivot = keyframe.useDefaultPivot;
                                    }
                                    else if (atlasChanged || textureGUIChanged)
                                    {
                                        if (currentUseDefaultPivot)
                                        {
                                            if (currentAtlas != null)
                                            {
                                                currentPivotOffset = currentAtlas.LookupDefaultPivotOffset(currentTextureGUID); ;
                                            }
                                            else
                                            {
                                                currentPivotOffset = lastPivotOffset;
                                            }
                                        }
                                        else
                                        {
                                            currentPivotOffset = lastPivotOffset;
                                        }
                                    }

                                    if (keyframe.useDepth)
                                        currentDepth = keyframe.depth;
                                    else
                                        currentDepth = lastDepth;

                                    if (keyframe.useCollider)
                                        currentCollider = keyframe.collider;
                                    else
                                        currentCollider = lastCollider;

                                    switch (currentKeyframeType)
                                    {
                                        case KeyframeSM.KEYFRAME_TYPE.Image:

                                            if (lastKeyframeType == KeyframeSM.KEYFRAME_TYPE.TransformOnly)
                                            {
                                                triggerFrame = AddTriggerFrame(ref animationClipTriggerFrames, clipIndex, frame);
                                                triggerFrame.AddTriggerFrameBoneMaterialChangeOriginal(bnIndex, matIndex);
                                                triggerFrame.AddTriggerFrameBoneTextureChangeOriginal(bnIndex, currentAtlas, currentTextureGUID, currentPivotOffset, matIndex);
                                                triggerFrame.AddTriggerFrameBoneDepthChange(bnIndex, currentDepth);
                                            }
                                            else
                                            {
                                                //if (currentAtlas != lastAtlas)
                                                if (keyframe.useAtlas)
                                                {
                                                    triggerFrame = AddTriggerFrame(ref animationClipTriggerFrames, clipIndex, frame);
                                                    triggerFrame.AddTriggerFrameBoneMaterialChangeOriginal(bnIndex, matIndex);
                                                    triggerFrame.AddTriggerFrameBoneTextureChangeOriginal(bnIndex, currentAtlas, currentTextureGUID, currentPivotOffset, matIndex);
                                                }
                                                //else if (currentTextureGUID != lastTextureGUID)
                                                else if (keyframe.useTextureGUID)
                                                {
                                                    triggerFrame = AddTriggerFrame(ref animationClipTriggerFrames, clipIndex, frame);
                                                    triggerFrame.AddTriggerFrameBoneTextureChangeOriginal(bnIndex, currentAtlas, currentTextureGUID, currentPivotOffset, matIndex);
                                                }
                                                //else if (currentPivotOffset != lastPivotOffset)
                                                else if (keyframe.usePivotOffset)
                                                {
                                                    triggerFrame = AddTriggerFrame(ref animationClipTriggerFrames, clipIndex, frame);
                                                    triggerFrame.AddTriggerFrameBonePivotChangeOriginal(bnIndex, currentAtlas, currentTextureGUID, currentPivotOffset);
                                                }

                                                //if (currentDepth != lastDepth)
                                                if (keyframe.useDepth)
                                                {
                                                    triggerFrame = AddTriggerFrame(ref animationClipTriggerFrames, clipIndex, frame);
                                                    triggerFrame.AddTriggerFrameBoneDepthChange(bnIndex, currentDepth);
                                                }
                                            }
                                            break;

                                        case KeyframeSM.KEYFRAME_TYPE.TransformOnly:
                                            if (lastKeyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                                            {
                                                triggerFrame = AddTriggerFrame(ref animationClipTriggerFrames, clipIndex, frame);
                                                triggerFrame.AddTriggerFrameBoneHideTexture(bnIndex);
                                            }
                                            break;
                                    }

                                    if (keyframe.useCollider)
                                    {
                                        triggerFrame = AddTriggerFrame(ref animationClipTriggerFrames, clipIndex, frame);
                                        triggerFrame.AddTriggerFrameBoneColliderChange(bnIndex, currentCollider);
                                    }
                                }

                                if (keyframe.useKeyframeType)
                                    lastKeyframeType = currentKeyframeType;

                                if (keyframe.useAtlas)
                                    lastAtlas = currentAtlas;

                                if (keyframe.useTextureGUID)
                                    lastTextureGUID = currentTextureGUID;

                                if (keyframe.usePivotOffset)
                                    lastPivotOffset = currentPivotOffset;

                                if (keyframe.useDepth)
                                    lastDepth = currentDepth;

                                if (keyframe.useCollider)
                                {
                                    lastCollider = currentCollider;
                                }

                                if (keyframe.userTriggerCallback)
                                {
                                    triggerFrame = AddTriggerFrame(ref animationClipTriggerFrames, clipIndex, frame);
                                    triggerFrame.AddTriggerFrameBoneUserTrigger(bnIndex, keyframe.userTriggerTag);
                                }
                            }
                        }
                    }
                }

                AnimationCurve localPositionXCurve;
                AnimationCurve localPositionYCurve;
                AnimationCurve localPositionZCurve;
                AnimationCurve localRotationCurve;
                AnimationCurve localScaleXCurve;
                AnimationCurve localScaleYCurve;
                AnimationCurve localScaleZCurve;
                AnimationCurve localRotationXCurve;
                AnimationCurve localRotationYCurve;
                AnimationCurve localRotationZCurve;
                AnimationCurve localRotationWCurve;
                AnimationCurve imageScaleXCurve;
                AnimationCurve imageScaleYCurve;
                int imageScaleZMaxFrame;
                string animationName;
                KeyframeSM.KEYFRAME_TYPE keyframeType;
                Vector3 localPosition3;
                Vector3 localScale3;
                Vector2 imageScale2;
                AnimationEvent animationEvent;
                List<AnimationEvent> animationEvents = new List<AnimationEvent>();

                boneAnimationData.GenerateBoneTransformPaths();
                boneAnimationData.animationClipSourceAssets = new List<AnimationClip>();

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, progressDescription, boneAnimationData, "Building animation clips", progressStart, progressChunkSize, 0.3f);
                float clipProgressStart = progressStart + (progressChunkSize * 0.3f);
                float clipProgressChunkSize = (0.7f - 0.3f) * progressChunkSize;

                int triggerFrameIndex = 0;
                boneAnimationData.triggerFrames = new List<TriggerFrame>();

                for (int clipIndex = 0; clipIndex < boneAnimationData.animationClips.Count; clipIndex++)
                {
                    animationName = boneAnimationData.animationClips[clipIndex].animationName;

                    progress = (float)clipIndex / (float)boneAnimationData.animationClips.Count;
                    DisplayProgress(UPDATE_PROGRESS_TITLE, false, progressDescription, boneAnimationData, "clip [" + animationName + "]", clipProgressStart, clipProgressChunkSize, progress);

                    clip = boneAnimationData.animationClips[clipIndex];

                    animationClip = new AnimationClip();
                    animationClip.name = animationName;
                    animationClip.wrapMode = clip.wrapMode;
                    animationClip.legacy = true;

                    for (int boneNodeIndex = 0; boneNodeIndex < boneAnimationData.boneDataList.Count; boneNodeIndex++)
                    {
                        animationClipBone = clip.bones[boneAnimationData.GetBoneDataIndex(boneNodeIndex)];

                        bonePath = boneAnimationData.boneTransformPaths[boneNodeIndex];
                        spriteBonePath = boneAnimationData.spriteTransformPaths[boneNodeIndex];

                        // Create animation curves

                        localPositionXCurve = new AnimationCurve();
                        localPositionYCurve = new AnimationCurve();
                        localPositionZCurve = new AnimationCurve();
                        localRotationCurve = new AnimationCurve();
                        localScaleXCurve = new AnimationCurve();
                        localScaleYCurve = new AnimationCurve();
                        localScaleZCurve = new AnimationCurve();
                        localRotationXCurve = new AnimationCurve();
                        localRotationYCurve = new AnimationCurve();
                        localRotationZCurve = new AnimationCurve();
                        localRotationWCurve = new AnimationCurve();
                        imageScaleXCurve = new AnimationCurve();
                        imageScaleYCurve = new AnimationCurve();

                        animationClipBone.ClearColorKeys();

                        keyframeType = KeyframeSM.KEYFRAME_TYPE.TransformOnly;

                        imageScaleZMaxFrame = -1;

                        for (int keyframeIndex = 0; keyframeIndex < animationClipBone.keyframes.Count; keyframeIndex++)
                        {
                            keyframe = animationClipBone.keyframes[keyframeIndex];
                            frame = keyframe.frame;

                            if (keyframeIndex == 0 || keyframe.useKeyframeType)
                                keyframeType = keyframe.keyframeType;

                            localPosition3 = keyframe.localPosition3.val * boneAnimationData.importScale;

                            if (keyframe.localPosition3.useX)
                                localPositionXCurve.AddKey(new Keyframe(frame, localPosition3.x, keyframe.localPosition3.inTangentX * boneAnimationData.importScale, keyframe.localPosition3.outTangentX * boneAnimationData.importScale));
                            if (keyframe.localPosition3.useY)
                                localPositionYCurve.AddKey(new Keyframe(frame, localPosition3.y, keyframe.localPosition3.inTangentY * boneAnimationData.importScale, keyframe.localPosition3.outTangentY * boneAnimationData.importScale));
                            if (keyframe.useDepth)
                            {
                                int localDepth = GetLocalDepth(boneAnimationData, boneNodeIndex, clip, keyframe);
                                float depthIncrement = (DepthMaxZOffset - Depth0ZOffset) / (float)KeyframeSM.MAX_DEPTH;
                                localPositionZCurve.AddKey(new Keyframe(frame, (float)localDepth * depthIncrement, Mathf.Infinity, Mathf.Infinity));
                            }

                            //if (keyframe.localPosition3.useZ)
                            //    localPositionZCurve.AddKey(new Keyframe(frame, localPosition3.z, keyframe.localPosition3.inTangentZ * boneAnimationData.importScale, keyframe.localPosition3.outTangentZ * boneAnimationData.importScale));

                            if (keyframe.localRotation.use)
                            {
                                localRotationCurve.AddKey(new Keyframe(frame, keyframe.localRotation.val, keyframe.localRotation.inTangent, keyframe.localRotation.outTangent));
                            }

                            localScale3 = keyframe.localScale3.val;

                            if (keyframe.localScale3.useX)
                                localScaleXCurve.AddKey(new Keyframe(frame, localScale3.x, keyframe.localScale3.inTangentX, keyframe.localScale3.outTangentX));
                            if (keyframe.localScale3.useY)
                                localScaleYCurve.AddKey(new Keyframe(frame, localScale3.y, keyframe.localScale3.inTangentY, keyframe.localScale3.outTangentY));
                            if (keyframe.localScale3.useZ)
                                localScaleZCurve.AddKey(new Keyframe(frame, localScale3.z, keyframe.localScale3.inTangentZ, keyframe.localScale3.outTangentZ));

                            imageScale2 = keyframe.imageScale.val * boneAnimationData.importScale;

                            if (keyframe.imageScale.useX)
                                imageScaleXCurve.AddKey(new Keyframe(frame, imageScale2.x, keyframe.imageScale.inTangentX * boneAnimationData.importScale, keyframe.imageScale.outTangentX * boneAnimationData.importScale));
                            if (keyframe.imageScale.useY)
                                imageScaleYCurve.AddKey(new Keyframe(frame, imageScale2.y, keyframe.imageScale.inTangentY * boneAnimationData.importScale, keyframe.imageScale.outTangentY * boneAnimationData.importScale));

                            if (keyframe.imageScale.useX || keyframe.imageScale.useY)
                            {
                                if (frame > imageScaleZMaxFrame)
                                    imageScaleZMaxFrame = frame;
                            }

                            if (keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                            {
                                if (keyframe.color.use)
                                {
                                    animationClipBone.colorRCurveSerialized.AddKeyframe(frame, keyframe.color.val.r, keyframe.color.inTangentR, keyframe.color.outTangentR);
                                    animationClipBone.colorGCurveSerialized.AddKeyframe(frame, keyframe.color.val.g, keyframe.color.inTangentG, keyframe.color.outTangentG);
                                    animationClipBone.colorBCurveSerialized.AddKeyframe(frame, keyframe.color.val.b, keyframe.color.inTangentB, keyframe.color.outTangentB);
                                    animationClipBone.colorACurveSerialized.AddKeyframe(frame, keyframe.color.val.a, keyframe.color.inTangentA, keyframe.color.outTangentA);
                                    animationClipBone.colorBlendWeightCurveSerialized.AddKeyframe(frame, keyframe.color.blendWeight, keyframe.color.inTangentBlendWeight, keyframe.color.outTangentBlendWeight);
                                }
                            }
                        }

                        if (boneNodeIndex == 0)
                        {
                            localPositionXCurve.AddKey(new Keyframe(clip.maxFrame, animationClipBone.keyframes[0].localPosition3.val.x, 0, 0));
                            localPositionYCurve.AddKey(new Keyframe(clip.maxFrame, animationClipBone.keyframes[0].localPosition3.val.y, 0, 0));
                            localPositionZCurve.AddKey(new Keyframe(clip.maxFrame, animationClipBone.keyframes[0].localPosition3.val.z, 0, 0));

                            localRotationCurve.AddKey(new Keyframe(clip.maxFrame, animationClipBone.keyframes[0].localRotation.val, 0, 0));

                            localScaleXCurve.AddKey(new Keyframe(clip.maxFrame, animationClipBone.keyframes[0].localScale3.val.x, 0, 0));
                            localScaleYCurve.AddKey(new Keyframe(clip.maxFrame, animationClipBone.keyframes[0].localScale3.val.y, 0, 0));
                            localScaleZCurve.AddKey(new Keyframe(clip.maxFrame, animationClipBone.keyframes[0].localScale3.val.z, 0, 0));
                        }

                        if (localPositionXCurve.keys.Length > 1)
                            animationClip.SetCurve(bonePath, typeof(Transform), "localPosition.x", localPositionXCurve);
                        if (localPositionYCurve.keys.Length > 1)
                            animationClip.SetCurve(bonePath, typeof(Transform), "localPosition.y", localPositionYCurve);
                        if (localPositionZCurve.keys.Length > 1)
                            animationClip.SetCurve(bonePath, typeof(Transform), "localPosition.z", localPositionZCurve);

                        localRotationXCurve = null;
                        localRotationYCurve = null;
                        localRotationZCurve = null;
                        localRotationWCurve = null;
                        BakeQuaternionCurves(ref localRotationCurve,
                                                ref localRotationXCurve,
                                                ref localRotationYCurve,
                                                ref localRotationZCurve,
                                                ref localRotationWCurve);


                        if (localRotationXCurve.keys.Length > 1)
                            animationClip.SetCurve(bonePath, typeof(Transform), "localRotation.x", localRotationXCurve);
                        if (localRotationYCurve.keys.Length > 1)
                            animationClip.SetCurve(bonePath, typeof(Transform), "localRotation.y", localRotationYCurve);
                        if (localRotationZCurve.keys.Length > 1)
                            animationClip.SetCurve(bonePath, typeof(Transform), "localRotation.z", localRotationZCurve);
                        if (localRotationWCurve.keys.Length > 1)
                            animationClip.SetCurve(bonePath, typeof(Transform), "localRotation.w", localRotationWCurve);

                        if (localScaleXCurve.keys.Length > 1)
                        {
                            animationClip.SetCurve(bonePath, typeof(Transform), "localScale.x", localScaleXCurve);
                        }
                        else if (localScaleXCurve.keys.Length == 1 && (localScaleYCurve.keys.Length > 1 || localScaleZCurve.keys.Length > 1))
                        {
                            localScaleXCurve.MoveKey(0, new Keyframe(localScaleXCurve.keys[0].time, localScaleXCurve.keys[0].value, 0, 0));
                            localScaleXCurve.AddKey(new Keyframe(clip.maxFrame, animationClipBone.keyframes[0].localScale3.val.x, 0, 0));
                            animationClip.SetCurve(bonePath, typeof(Transform), "localScale.x", localScaleXCurve);
                        }

                        if (localScaleYCurve.keys.Length > 1)
                        {
                            animationClip.SetCurve(bonePath, typeof(Transform), "localScale.y", localScaleYCurve);
                        }
                        else if (localScaleYCurve.keys.Length == 1 && (localScaleXCurve.keys.Length > 1 || localScaleZCurve.keys.Length > 1))
                        {
                            localScaleYCurve.MoveKey(0, new Keyframe(localScaleYCurve.keys[0].time, localScaleYCurve.keys[0].value, 0, 0));
                            localScaleYCurve.AddKey(new Keyframe(clip.maxFrame, animationClipBone.keyframes[0].localScale3.val.y, 0, 0));
                            animationClip.SetCurve(bonePath, typeof(Transform), "localScale.y", localScaleYCurve);
                        }

                        if (localScaleZCurve.keys.Length > 1)
                        {
                            animationClip.SetCurve(bonePath, typeof(Transform), "localScale.z", localScaleZCurve);
                        }
                        else if (localScaleZCurve.keys.Length == 1 && (localScaleXCurve.keys.Length > 1 || localScaleYCurve.keys.Length > 1))
                        {
                            localScaleZCurve.MoveKey(0, new Keyframe(localScaleZCurve.keys[0].time, localScaleZCurve.keys[0].value, 0, 0));
                            localScaleZCurve.AddKey(new Keyframe(clip.maxFrame, animationClipBone.keyframes[0].localScale3.val.z, 0, 0));
                            animationClip.SetCurve(bonePath, typeof(Transform), "localScale.z", localScaleZCurve);
                        }

                        if (imageScaleXCurve.keys.Length > 1)
                        {
                            animationClip.SetCurve(spriteBonePath, typeof(Transform), "localScale.x", imageScaleXCurve);
                        }
                        else if (imageScaleXCurve.keys.Length == 1 && (imageScaleYCurve.keys.Length > 1))
                        {
                            imageScaleXCurve.MoveKey(0, new Keyframe(imageScaleXCurve.keys[0].time, imageScaleXCurve.keys[0].value, 0, 0));
                            imageScaleXCurve.AddKey(new Keyframe(clip.maxFrame, animationClipBone.keyframes[0].imageScale.val.x, 0, 0));
                            animationClip.SetCurve(spriteBonePath, typeof(Transform), "localScale.x", imageScaleXCurve);
                        }

                        if (imageScaleYCurve.keys.Length > 1)
                        {
                            animationClip.SetCurve(spriteBonePath, typeof(Transform), "localScale.y", imageScaleYCurve);
                        }
                        else if (imageScaleYCurve.keys.Length == 1 && (imageScaleXCurve.keys.Length > 1))
                        {
                            imageScaleYCurve.MoveKey(0, new Keyframe(imageScaleYCurve.keys[0].time, imageScaleYCurve.keys[0].value, 0, 0));
                            imageScaleYCurve.AddKey(new Keyframe(clip.maxFrame, animationClipBone.keyframes[0].imageScale.val.y, 0, 0));
                            animationClip.SetCurve(spriteBonePath, typeof(Transform), "localScale.y", imageScaleYCurve);
                        }

                        if (imageScaleXCurve.keys.Length > 1 || imageScaleYCurve.keys.Length > 1)
                        {
                            AnimationCurve imageScaleZCurve = new AnimationCurve();
                            imageScaleZCurve.AddKey(new Keyframe(0, 1.0f, 0, 0));
                            imageScaleZCurve.AddKey(new Keyframe(imageScaleZMaxFrame, 1.0f, 0, 0));
                            animationClip.SetCurve(spriteBonePath, typeof(Transform), "localScale.z", imageScaleZCurve);
                        }
                    }

                    animationEvents.Clear();
                    foreach (TriggerFrame clipTriggerFrame in animationClipTriggerFrames[clipIndex].triggerFrames)
                    {
                        animationEvent = new AnimationEvent();
                        animationEvent.functionName = "AnimationEvent_Triggered";
                        animationEvent.time = clipTriggerFrame.frame;
                        animationEvent.intParameter = triggerFrameIndex;
                        animationEvents.Add(animationEvent);

                        boneAnimationData.triggerFrames.Add(clipTriggerFrame);
                        triggerFrameIndex++;
                    }

                    AnimationUtility.SetAnimationEvents(animationClip, animationEvents.ToArray());

                    animationAssetPath = Path.Combine(resourcesPath, animationName + ".anim");
                    AssetDatabase.CreateAsset(animationClip, animationAssetPath);
                    boneAnimationData.animationClipSourceAssets.Add((AnimationClip)AssetDatabase.LoadAssetAtPath(animationAssetPath, typeof(AnimationClip)));
                }

                List<AnimationClipTriggerFrames> tempAnimationClipTriggerFrameList = new List<AnimationClipTriggerFrames>();
                foreach (KeyValuePair<int, AnimationClipTriggerFrames> kvp in animationClipTriggerFrames)
                {
                    tempAnimationClipTriggerFrameList.Add(kvp.Value);
                }
                boneAnimationData.animationClipTriggerFrames = tempAnimationClipTriggerFrameList.ToArray();

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, progressDescription, boneAnimationData, "Building mesh", progressStart, progressChunkSize, 0.7f);

                boneAnimationData.vertices = new Vector3[boneAnimationData.boneDataList.Count * 4];
                boneAnimationData.uvs = new Vector2[boneAnimationData.boneDataList.Count * 4];
                boneAnimationData.colors = new Color[boneAnimationData.boneDataList.Count * 4];

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, progressDescription, boneAnimationData, "Setting initial state", progressStart, progressChunkSize, 0.8f);

                // Set mesh based on initial trigger in first clip
                TriggerFrameBone triggerFrameBone;
                if (boneAnimationData.animationClipTriggerFrames.Length > 0)
                {
                    triggerFrame = boneAnimationData.animationClipTriggerFrames[0].GetTriggerFrame(0);
                    if (triggerFrame != null)
                    {
                        for (bnIndex = 0; bnIndex < boneAnimationData.boneDataList.Count; bnIndex++)
                        {
                            triggerFrameBone = triggerFrame.GetTriggerFrameBone(bnIndex);
                            if (triggerFrameBone != null)
                            {
                                SetVertexData(ref boneAnimationData.vertices,
                                                (bnIndex * 4),
                                                triggerFrameBone.upperLeft,
                                                triggerFrameBone.bottomLeft,
                                                triggerFrameBone.bottomRight,
                                                triggerFrameBone.upperRight);

                                SetBoneUV(ref boneAnimationData.uvs, (bnIndex * 4), triggerFrameBone.uv);
                            }
                        }
                    }
                }

                // Set the initial colors
                if (boneAnimationData.animationClips.Count > 0)
                {
                    for (int boneDataIndex = 0; boneDataIndex < boneAnimationData.animationClips[0].bones.Count; boneDataIndex++)
                    {
                        bnIndex = boneAnimationData.GetBoneNodeIndex(boneDataIndex);

                        SetBoneColor(ref boneAnimationData.colors, (bnIndex * 4), Color.Lerp(boneAnimationData.meshColor, boneAnimationData.boneDataList[boneDataIndex].boneColor.color, boneAnimationData.boneDataList[boneDataIndex].boneColor.blendingWeight));
                    }
                }

                boneAnimationData.needsRebuilt = false;

                EditorUtility.SetDirty(boneAnimationData);

                animationClipCount = boneAnimationData.animationClips.Count;

                DisplayProgress(UPDATE_PROGRESS_TITLE, false, progressDescription, boneAnimationData, "Done", progressStart, progressChunkSize, 0.9f);

            }
            catch (Exception ex)
            {
                Debug.LogError("UpdateBoneAnimationData Error: " + ex.Message);

                HideProgress();
                return false;
            }

            return true;
        }

        static private int GetLocalDepth(BoneAnimationData data, int boneNodeIndex, AnimationClipSM clip, KeyframeSM keyframe)
        {
            int localDepth = -1;
            int parentIndex = -1;
            AnimationClipBone animationClipBone;

            localDepth = keyframe.depth;
            parentIndex = data.dfsBoneNodeList[boneNodeIndex].parentBoneIndex;

            if (parentIndex != -1)
            {
                animationClipBone = clip.bones[data.GetBoneDataIndex(parentIndex)];
                localDepth -= animationClipBone.GetPreviousDepth(keyframe.frame);
            }

            return localDepth;
        }

        static private bool UpdateBoneAnimation(BoneAnimation boneAnimation)
        {
            try
            {
                BoneAnimationData boneAnimationData = GetBoneAnimationDataFromBoneAnimation(boneAnimation);

                if (boneAnimationData == null)
                {
                    Debug.LogWarning("No animation data set for [" + boneAnimation.name + "], skipping update of object.");
                    return true;
                }

                PrefabType prefabType;
                UnityEngine.Object originalPrefab = null;
                int bdIndex;
                int bnIndex;

                boneAnimation.mLocalTransform = boneAnimation.transform;

                // gather a list of children that are not a part of the skeleton.
                // we do this before any prefab attaching so that the children are not
                // lost. Children are temporarily moved to the root of the scene,
                // then later reattached when the skeleton is recreated.
                // any bone name changes will result in the children being
                // attached to the boneanimation (at the same level as the root).

                // gather a list of objects that need to be re-attached after the bone restructuring
                Dictionary<string, Transform> childPaths = new Dictionary<string, Transform>();
                Dictionary<string, List<Transform>> boneChildren = new Dictionary<string, List<Transform>>();
                List<Transform> highLevelChildren = new List<Transform>();
                foreach (Transform ct in boneAnimation.mLocalTransform)
                {
                    if (ct.name.ToLower().Trim() == "root")
                    {
                        BuildPathsForRoot(ct, "Root", ref childPaths);
                        break;
                    }
                    else
                    {
                        // add this non-root transform to the high level children
                        // and move it outside of the boneanimation until the skeleton
                        // is recreated
                        highLevelChildren.Add(ct);
                        ct.parent = null;
                    }
                }

                if (boneAnimation.mBoneTransformPaths != null && boneAnimation.mSpriteTransformPaths != null)
                {
                    if ((boneAnimation.mBoneTransformPaths.Length == 0 || boneAnimation.mSpriteTransformPaths.Length == 0) && childPaths.Count > 0)
                    {
                        // this is an upgrade because the bone animation has no current transform paths stored,
                        // we will ignore capturing stray children
                    }
                    else
                    {
                        bool isBoneOrSprite;
                        List<string> childKeysToRemove = new List<string>();

                        // first, gather the list of child keys that don't need to be processed.
                        // these are transforms that are children to objects that are not bones.
                        foreach (KeyValuePair<string, Transform> kvp in childPaths)
                        {
                            isBoneOrSprite = false;

                            foreach (string boneTransformPath in boneAnimation.mBoneTransformPaths)
                            {
                                if (boneTransformPath.ToLower() == kvp.Key)
                                {
                                    isBoneOrSprite = true;
                                    break;
                                }
                            }
                            if (!isBoneOrSprite)
                            {
                                foreach (string spriteTransformPath in boneAnimation.mSpriteTransformPaths)
                                {
                                    if (spriteTransformPath.ToLower() == kvp.Key)
                                    {
                                        isBoneOrSprite = true;
                                        break;
                                    }
                                }
                            }

                            if (!isBoneOrSprite)
                            {
                                GatherChildKeysToRemoveFromParent(ref childPaths, kvp.Value, ref childKeysToRemove);
                            }
                        }

                        // next, remove the unnecessary children from the paths list.
                        // Essentially, we want only the high level parents of the objects
                        // that need to be moved out of the bone animation.
                        foreach (string childKey in childKeysToRemove)
                        {
                            if (childPaths.ContainsKey(childKey))
                            {
                                childPaths.Remove(childKey);
                            }
                        }

                        // next, gather the high level parents so that we can move
                        // them out of the bone animation
                        foreach (KeyValuePair<string, Transform> kvp in childPaths)
                        {
                            isBoneOrSprite = false;

                            foreach (string boneTransformPath in boneAnimation.mBoneTransformPaths)
                            {
                                if (boneTransformPath.ToLower() == kvp.Key)
                                {
                                    isBoneOrSprite = true;
                                    break;
                                }
                            }
                            if (!isBoneOrSprite)
                            {
                                foreach (string spriteTransformPath in boneAnimation.mSpriteTransformPaths)
                                {
                                    if (spriteTransformPath.ToLower() == kvp.Key)
                                    {
                                        isBoneOrSprite = true;
                                        break;
                                    }
                                }
                            }

                            if (!isBoneOrSprite)
                            {
                                if (boneChildren.ContainsKey(kvp.Value.parent.name.ToLower()))
                                {
                                    boneChildren[kvp.Value.parent.name.ToLower()].Add(kvp.Value);
                                }
                                else
                                {
                                    List<Transform> list = new List<Transform>();
                                    list.Add(kvp.Value);
                                    boneChildren.Add(kvp.Value.parent.name.ToLower(), list);
                                }
                            }
                        }

                        // finally, move the child objects outside of the skeleton
                        // so they won't be destroyed
                        foreach (KeyValuePair<string, List<Transform>> kvp in boneChildren)
                        {
                            foreach (Transform child in kvp.Value)
                            {
                                child.parent = null;
                            }
                        }
                    } // end upgrade
                }

                // determine the prefab type
                prefabType = PrefabUtility.GetPrefabType(boneAnimation);
                switch (prefabType)
                {
                    case PrefabType.Prefab:
                        originalPrefab = boneAnimation.gameObject;

                        // to update the prefab, we first have to create an instance of it in the scene.
                        // we'll delete the instance at the end of this function after we have applied
                        // the changes back to the prefab. Unity won't allow us to update a prefab's
                        // transform hierarchy directly.

                        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(originalPrefab);
                        boneAnimation = go.GetComponent<BoneAnimation>();
                        break;

                    case PrefabType.DisconnectedPrefabInstance:

                        // if this game object is a disconnected prefab instance
                        // then we need to reconnect it to the prefab in case there 
                        // have been changes made to the prefab. After reconnecting,
                        // we'll revert the game object back to its prefab state. this 
                        // state will be overwritten and disconnected again, but it
                        // will at least update according to the prefab data.

                        PrefabUtility.ReconnectToLastPrefab(boneAnimation.gameObject);
                        PrefabUtility.RevertPrefabInstance(boneAnimation.gameObject);
                        break;
                }



                // create the components

                boneAnimation.mAnimation = boneAnimation.gameObject.GetComponent<Animation>();
                if (boneAnimation.mAnimation == null)
                {
                    // new component

                    boneAnimation.mAnimation = boneAnimation.gameObject.AddComponent<Animation>();
                }
                else
                {
                    // component exists, so we'll gather the settings that the user may have changed,
                    // then destroy the component, recreate it, and reapply the settings

                    bool playAutomatically = boneAnimation.mAnimation.playAutomatically;
                    bool animatePhysics = boneAnimation.mAnimation.animatePhysics;
                    AnimationCullingType cullingType = boneAnimation.mAnimation.cullingType;

                    Editor.DestroyImmediate(boneAnimation.mAnimation);
                    boneAnimation.mAnimation = boneAnimation.gameObject.AddComponent<Animation>();

                    boneAnimation.mAnimation.playAutomatically = playAutomatically;
                    boneAnimation.mAnimation.animatePhysics = animatePhysics;
                    boneAnimation.mAnimation.cullingType = cullingType;
                }

                boneAnimation.mRenderer = boneAnimation.gameObject.GetComponent<SkinnedMeshRenderer>();
                if (boneAnimation.mRenderer == null)
                {
                    // new component

                    boneAnimation.mRenderer = boneAnimation.gameObject.AddComponent<SkinnedMeshRenderer>();
                    boneAnimation.mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    boneAnimation.mRenderer.receiveShadows = false;
                    boneAnimation.mRenderer.quality = SkinQuality.Bone1;
                }
                else
                {
                    // component exists, so we'll gather the settings that the user may have changed,
                    // then destroy the component, recreate it, and reapply the settings.
                    // this is necessary to recalculate the renderer bounds for some reason.

                    UnityEngine.Rendering.ShadowCastingMode shadowCastingMode = boneAnimation.mRenderer.shadowCastingMode;
                    bool receiveShadows = boneAnimation.mRenderer.receiveShadows;
                    bool useLightProbes = boneAnimation.mRenderer.useLightProbes;
                    Transform probeAnchor = boneAnimation.mRenderer.probeAnchor;
                    bool updateWhenOffscreen = boneAnimation.mRenderer.updateWhenOffscreen;

                    Editor.DestroyImmediate(boneAnimation.mRenderer);
                    boneAnimation.mRenderer = boneAnimation.gameObject.AddComponent<SkinnedMeshRenderer>();
                    boneAnimation.mRenderer.quality = SkinQuality.Bone1;

                    boneAnimation.mRenderer.shadowCastingMode = shadowCastingMode;
                    boneAnimation.mRenderer.receiveShadows = receiveShadows;
                    boneAnimation.mRenderer.useLightProbes = useLightProbes;
                    boneAnimation.mRenderer.probeAnchor = probeAnchor;
                    boneAnimation.mRenderer.updateWhenOffscreen = updateWhenOffscreen;
                }




                if (boneAnimationData == null)
                {
                    Debug.LogWarning("No animation data set for [" + boneAnimation.name + "], skipping update of object.");
                    return true;
                }

                boneAnimation.mFinalImportScale = boneAnimationData.importScale;

                // delete the skeleton at the root onward
                foreach (Transform childTransform in boneAnimation.mLocalTransform)
                {
                    if (childTransform.name.ToLower().Trim() == "root")
                    {
                        Editor.DestroyImmediate(childTransform.gameObject, true);
                        break;
                    }
                }

                // Create bones
                List<AnimationBone> boneSource = new List<AnimationBone>();
                BoneData boneData;
                AnimationBone animationBone;

                for (bnIndex = 0; bnIndex < boneAnimationData.dfsBoneNodeList.Count; bnIndex++)
                {
                    boneData = boneAnimationData.boneDataList[boneAnimationData.dfsBoneNodeList[bnIndex].boneDataIndex];

                    animationBone = new AnimationBone(
                                            boneAnimation,
                                            bnIndex,
                                            boneAnimationData.dfsBoneNodeList[bnIndex].boneDataIndex,
                                            boneData.boneName,
                                            (bnIndex == 0 ? boneAnimation.mLocalTransform : boneSource[boneAnimationData.dfsBoneNodeList[bnIndex].parentBoneIndex].boneTransform),
                                            boneAnimationData.animationClips.Count,
                                            boneData.active,
                                            boneData.boneColor
                                            );
                    boneSource.Add(animationBone);

                    // add children back to the bone
                    if (boneChildren.ContainsKey(animationBone.boneTransform.name.ToLower()))
                    {
                        foreach (Transform child in boneChildren[animationBone.boneTransform.name.ToLower()])
                        {
                            child.parent = animationBone.boneTransform;
                        }
                    }

                    // add children back to the sprite
                    if (boneChildren.ContainsKey(animationBone.spriteTransform.name.ToLower()))
                    {
                        foreach (Transform child in boneChildren[animationBone.spriteTransform.name.ToLower()])
                        {
                            child.parent = animationBone.spriteTransform;
                        }
                    }
                }

                // attach orphaned children back to the bone animation to keep the scene neat
                foreach (KeyValuePair<string, List<Transform>> kvp in boneChildren)
                {
                    foreach (Transform child in kvp.Value)
                    {
                        if (child.parent == null)
                        {
                            child.parent = boneAnimation.mLocalTransform;
                        }
                    }
                }

                // reattach high level children back to the bone animation
                foreach (Transform highLevelChild in highLevelChildren)
                {
                    highLevelChild.parent = boneAnimation.mLocalTransform;
                }

                boneAnimation.mBoneSource = boneSource.ToArray();

                boneAnimation.mBoneTransforms = new Transform[boneSource.Count * 2];
                for (bnIndex = 0; bnIndex < boneSource.Count; bnIndex++)
                {
                    boneAnimation.mBoneTransforms[(bnIndex * 2) + 0] = boneSource[bnIndex].boneTransform;
                    boneAnimation.mBoneTransforms[(bnIndex * 2) + 1] = boneSource[bnIndex].spriteTransform;
                }

                boneAnimation.mVertices = null;
                boneAnimation.mUVs = null;
                boneAnimation.mColors = null;






                //BoneAnimationData boneAnimationData = boneAnimation.animationData;

                boneAnimation.mMaterialSource = boneAnimationData.materialSource.ToArray();
                boneAnimation.mMaterials = boneAnimationData.materialSource.ToArray();



                //// Copy the trigger frames
                //AnimationClipTriggerFrames newActf;
                //int actfIndex = 0;
                //boneAnimation.mAnimationClipTriggerFrames = new AnimationClipTriggerFrames[boneAnimationData.animationClipTriggerFrames.Length];
                //foreach (AnimationClipTriggerFrames copyActf in boneAnimationData.animationClipTriggerFrames)
                //{
                //    newActf = new AnimationClipTriggerFrames(copyActf);
                //    boneAnimation.mAnimationClipTriggerFrames[actfIndex] = newActf;
                //    actfIndex++;
                //}

                boneAnimation.triggerFrames = new TriggerFrame[boneAnimationData.triggerFrames.Count];
                int triggerFrameIndex;
                for (triggerFrameIndex=0; triggerFrameIndex < boneAnimationData.triggerFrames.Count; triggerFrameIndex++)
                {
                    boneAnimation.triggerFrames[triggerFrameIndex] = new TriggerFrame(boneAnimationData.triggerFrames[triggerFrameIndex]);
                }

                foreach (TriggerFrame tf in boneAnimation.triggerFrames)
                {
                    foreach (TriggerFrameBone tfb in tf.triggerFrameBones)
                    {
                        if (tfb.triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.ChangeCollider))
                        {
                            if (tfb.collider != null)
                            {
                                animationBone = boneAnimation.mBoneSource[tfb.boneNodeIndex];

                                switch (tfb.collider.type)
                                {
                                    case ColliderSM.COLLIDER_TYPE.Box:
                                        animationBone.CreateBoxCollider();
                                        break;

                                    case ColliderSM.COLLIDER_TYPE.Sphere:
                                        animationBone.CreateSphereCollider();
                                        break;
                                }
                            }
                        }
                    }
                }

                //AnimationBone animationBone;
                //foreach (AnimationClipTriggerFrames actf in boneAnimation.mAnimationClipTriggerFrames)
                //{
                //    foreach (TriggerFrame tf in actf.triggerFrames)
                //    {
                //        foreach (TriggerFrameBone tfb in tf.triggerFrameBones)
                //        {
                //            if (tfb.triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.ChangeCollider))
                //            {
                //                if (tfb.collider != null)
                //                {
                //                    animationBone = boneAnimation.mBoneSource[tfb.boneNodeIndex];

                //                    switch (tfb.collider.type)
                //                    {
                //                        case ColliderSM.COLLIDER_TYPE.Box:
                //                            animationBone.CreateBoxCollider();
                //                            break;

                //                        case ColliderSM.COLLIDER_TYPE.Sphere:
                //                            animationBone.CreateSphereCollider();
                //                            break;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                // Set mesh based on initial trigger in first clip
                TriggerFrame triggerFrame;
                TriggerFrameBone triggerFrameBone;
                //if (boneAnimation.mAnimationClipTriggerFrames.Length > 0)
                //{
                //    triggerFrame = boneAnimation.mAnimationClipTriggerFrames[0].GetTriggerFrame(0);
                if (boneAnimation.triggerFrames.Length > 0)
                {
                    triggerFrame = boneAnimation.triggerFrames[0];
                    if (triggerFrame != null)
                    {
                        for (bnIndex = 0; bnIndex < boneAnimation.mBoneSource.Length; bnIndex++)
                        {
                            triggerFrameBone = triggerFrame.GetTriggerFrameBone(bnIndex);
                            if (triggerFrameBone != null)
                            {
                                if (triggerFrameBone.triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.HideTexture))
                                {
                                    boneAnimation.mBoneSource[bnIndex].visible = false;
                                }
                                else
                                {
                                    boneAnimation.mBoneSource[bnIndex].visible = true;
                                    boneAnimation.mBoneSource[bnIndex].materialIndex = triggerFrameBone.materialIndex;
                                    boneAnimation.mBoneSource[bnIndex].depth = triggerFrameBone.depth;
                                }

                                if (triggerFrameBone.collider == null)
                                {
                                    boneAnimation.mBoneSource[bnIndex].TurnOffBoxCollider();
                                    boneAnimation.mBoneSource[bnIndex].TurnOffSphereCollider();
                                    boneAnimation.mBoneSource[bnIndex].SetLayer(boneAnimation.gameObject.layer);
                                }
                                else
                                {
                                    switch (triggerFrameBone.collider.type)
                                    {
                                        case ColliderSM.COLLIDER_TYPE.None:
                                            boneAnimation.mBoneSource[bnIndex].TurnOffBoxCollider();
                                            boneAnimation.mBoneSource[bnIndex].TurnOffSphereCollider();
                                            boneAnimation.mBoneSource[bnIndex].SetLayer(boneAnimation.gameObject.layer);
                                            break;

                                        case ColliderSM.COLLIDER_TYPE.Box:
                                            boneAnimation.mBoneSource[bnIndex].TurnOnBoxCollider(triggerFrameBone.collider.center * boneAnimation.mFinalImportScale, triggerFrameBone.collider.boxSize * boneAnimation.mFinalImportScale, triggerFrameBone.collider.tag);
                                            boneAnimation.mBoneSource[bnIndex].TurnOffSphereCollider();
                                            boneAnimation.mBoneSource[bnIndex].SetTrigger(triggerFrameBone.collider.isTrigger);
                                            boneAnimation.mBoneSource[bnIndex].SetLayer(triggerFrameBone.collider.useAnimationLayer ? boneAnimation.gameObject.layer : triggerFrameBone.collider.layer);
                                            break;

                                        case ColliderSM.COLLIDER_TYPE.Sphere:
                                            boneAnimation.mBoneSource[bnIndex].TurnOffBoxCollider();
                                            boneAnimation.mBoneSource[bnIndex].TurnOnSphereCollider(triggerFrameBone.collider.center * boneAnimation.mFinalImportScale, triggerFrameBone.collider.sphereRadius * boneAnimation.mFinalImportScale, triggerFrameBone.collider.tag);
                                            boneAnimation.mBoneSource[bnIndex].SetTrigger(triggerFrameBone.collider.isTrigger);
                                            boneAnimation.mBoneSource[bnIndex].SetLayer(triggerFrameBone.collider.useAnimationLayer ? boneAnimation.gameObject.layer : triggerFrameBone.collider.layer);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                // Set the initial colors
                boneAnimation.mMeshColor = boneAnimationData.meshColor;

                // Move bones to starting positions of first clip
                AnimationClipBone animationClipBone;
                KeyframeSM keyframe;
                Vector2 imageScale2;
                for (bnIndex = 0; bnIndex < boneAnimation.mBoneSource.Length; bnIndex++)
                {
                    animationBone = boneAnimation.mBoneSource[bnIndex];
                    animationClipBone = boneAnimationData.animationClips[0].bones[boneAnimationData.GetBoneDataIndex(bnIndex)];

                    if (animationClipBone.keyframes.Count > 0)
                    {
                        keyframe = animationClipBone.keyframes[0];

                        animationBone.boneTransform.localPosition = keyframe.localPosition3.val * boneAnimation.mFinalImportScale;
                        animationBone.boneTransform.localRotation = Quaternion.Euler(0, 0, keyframe.localRotation.val);
                        animationBone.boneTransform.localScale = keyframe.localScale3.val; // *_finalImportScale;

                        imageScale2 = keyframe.imageScale.val * boneAnimation.mFinalImportScale;

                        animationBone.spriteTransform.localScale = new Vector3(
                                                                          imageScale2.x,
                                                                          imageScale2.y,
                                                                          1.0f
                                                                          );
                    }
                }

                // add the animation clips
                foreach (AnimationClip animationClip in boneAnimationData.animationClipSourceAssets)
                {
                    boneAnimation.mAnimation.AddClip(animationClip, animationClip.name);
                }
                if (boneAnimationData.animationClipSourceAssets.Count > 0)
                    boneAnimation.mAnimation.clip = boneAnimationData.animationClipSourceAssets[0];

                boneAnimation.mDefaultAnimationName = "";
                if (boneAnimationData.animationClips.Count > 0)
                {
                    boneAnimation.mDefaultAnimationName = boneAnimationData.animationClips[0].animationName;
                }

                boneAnimation.mAnimationClips = new AnimationClipSM_Lite[boneAnimationData.animationClips.Count];
                int clipIndex;
                for (clipIndex=0; clipIndex < boneAnimationData.animationClips.Count; clipIndex++)
                {
                    boneAnimation.mAnimationClips[clipIndex] = new AnimationClipSM_Lite(boneAnimationData.animationClips[clipIndex]);
                }



                boneAnimation.mLastBoneColor = new BoneColor[boneAnimation.mBoneSource.Length];
                boneAnimation.mLastFinalColor = new Color[boneAnimation.mBoneSource.Length];
                boneAnimation.mBoneColorAnimations = new BoneColorAnimation[boneAnimation.mBoneSource.Length];
                boneAnimation.mCurrentTriggerFrameBones = new TriggerFrameBoneCurrent[boneAnimation.mBoneSource.Length];

                for (bnIndex = 0; bnIndex < boneAnimation.mBoneSource.Length; bnIndex++)
                {
                    bdIndex = boneAnimation.mBoneSource[bnIndex].boneDataIndex;

                    boneAnimation.mLastBoneColor[bdIndex] = new BoneColor(boneAnimationData.boneDataList[bdIndex].boneColor.blendingWeight, boneAnimationData.boneDataList[bdIndex].boneColor.color);
                    boneAnimation.mLastFinalColor[bdIndex] = Color.Lerp(boneAnimation.mMeshColor, boneAnimationData.boneDataList[bdIndex].boneColor.color, boneAnimationData.boneDataList[bdIndex].boneColor.blendingWeight);
                    boneAnimation.mBoneColorAnimations[bdIndex] = new BoneColorAnimation();
                    boneAnimation.mCurrentTriggerFrameBones[bnIndex] = new TriggerFrameBoneCurrent(bnIndex);
                }

                CreateMeshFromAnimationData(boneAnimation);

                boneAnimation.mRenderer.bones = boneAnimation.mBoneTransforms;

                // copy bone and sprite transform paths
                boneAnimation.mBoneTransformPaths = new string[boneAnimationData.boneTransformPaths.Count];
                boneAnimation.mSpriteTransformPaths = new string[boneAnimationData.spriteTransformPaths.Count];
                for (bnIndex = 0; bnIndex < boneAnimationData.boneTransformPaths.Count; bnIndex++)
                {
                    boneAnimation.mBoneTransformPaths[bnIndex] = boneAnimationData.boneTransformPaths[bnIndex];
                }
                for (bnIndex = 0; bnIndex < boneAnimationData.spriteTransformPaths.Count; bnIndex++)
                {
                    boneAnimation.mSpriteTransformPaths[bnIndex] = boneAnimationData.spriteTransformPaths[bnIndex];
                }

                boneAnimation.textureAtlases = boneAnimationData.textureAtlasDictionary.ToArray();

                boneAnimation.buildID = boneAnimationData.buildID;

                switch (prefabType)
                {
                    case PrefabType.Prefab:

                        // since this was a prefab, we need to update the original prefab
                        // then delete the temporary scene object we created.

                        PrefabUtility.ReplacePrefab(boneAnimation.gameObject, originalPrefab); //, ReplacePrefabOptions.ConnectToPrefab);
                        Editor.DestroyImmediate(boneAnimation.gameObject);

                        EditorUtility.SetDirty(originalPrefab);
                        break;

                    default:
                        EditorUtility.SetDirty(boneAnimation.gameObject);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("UpdateBoneAnimation Error: " + ex.Message);

                return false;
            }

            return true;
        }

        static public bool CreateMeshFromAnimationData(BoneAnimation boneAnimation)
        {
            BoneAnimationData boneAnimationData = GetBoneAnimationDataFromBoneAnimation(boneAnimation);

            //if (animationData == null)
            //{
            //    Debug.LogWarning("No animation data set for BoneAnimation [" + name + "]");
            //    return false;
            //}

            if (boneAnimation.mAnimation == null)
            {
                Debug.LogWarning("No animation component for BoneAnimation [" + boneAnimation.name + "]. Please build animations or set auto-update on.");
                return false;
            }

            // only create the mesh if there is data missing.
            // most likely this will be due to the object being
            // created from a prefab.
            if (
                    boneAnimation.mMesh == null ||
                    boneAnimation.mRenderer.sharedMesh == null ||
                    boneAnimation.mVertices == null ||
                    boneAnimation.mUVs == null ||
                    boneAnimation.mColors == null
                )
            {
                // can't serialize the bone weights, so we generate them here
                BoneWeight[] boneWeights = new BoneWeight[boneAnimation.mBoneSource.Length * 4];
                for (int boneNodeIndex = 0; boneNodeIndex < boneAnimation.mBoneSource.Length; boneNodeIndex++)
                {
                    // sprite bone weight (skip the base bone since its transform will be propogated automatically)
                    boneWeights[(boneNodeIndex * 4) + 0].boneIndex0 = (boneNodeIndex * 2) + 1;
                    boneWeights[(boneNodeIndex * 4) + 0].weight0 = 1.0f;

                    boneWeights[(boneNodeIndex * 4) + 1].boneIndex0 = (boneNodeIndex * 2) + 1;
                    boneWeights[(boneNodeIndex * 4) + 1].weight0 = 1.0f;

                    boneWeights[(boneNodeIndex * 4) + 2].boneIndex0 = (boneNodeIndex * 2) + 1;
                    boneWeights[(boneNodeIndex * 4) + 2].weight0 = 1.0f;

                    boneWeights[(boneNodeIndex * 4) + 3].boneIndex0 = (boneNodeIndex * 2) + 1;
                    boneWeights[(boneNodeIndex * 4) + 3].weight0 = 1.0f;
                }

                boneAnimation.mVertices = new Vector3[boneAnimation.mBoneSource.Length * 4];
                boneAnimation.mUVs = new Vector2[boneAnimation.mBoneSource.Length * 4];
                boneAnimation.mColors = new Color[boneAnimation.mBoneSource.Length * 4];

                boneAnimationData.vertices.CopyTo(boneAnimation.mVertices, 0);
                boneAnimationData.uvs.CopyTo(boneAnimation.mUVs, 0);
                boneAnimationData.colors.CopyTo(boneAnimation.mColors, 0);

                boneAnimation.mMesh = new Mesh();
                boneAnimation.mRenderer.sharedMesh = boneAnimation.mMesh;

                boneAnimation.mMesh.subMeshCount = 1;
                boneAnimation.mMesh.vertices = boneAnimation.mVertices;
                boneAnimation.mMesh.uv = boneAnimation.mUVs;
                boneAnimation.mMesh.normals = boneAnimationData.normals;
                boneAnimation.mMesh.colors = boneAnimation.mColors;
                boneAnimation.mMesh.bindposes = boneAnimationData.bindPoses;
                boneAnimation.mMesh.boneWeights = boneWeights;

                boneAnimation.GenerateMesh();

                boneAnimation.mMesh.RecalculateBounds();
                boneAnimation.mMesh.Optimize();
            }

            return true;
        }

        static private void GatherChildKeysToRemoveFromParent(ref Dictionary<string, Transform> childPaths, Transform parent, ref List<string> keysToRemove)
        {
            foreach (KeyValuePair<string, Transform> kvp in childPaths)
            {
                if (kvp.Value.parent == parent)
                {
                    GatherChildKeysToRemoveFromParent(ref childPaths, kvp.Value, ref keysToRemove);

                    if (!keysToRemove.Contains(kvp.Key))
                        keysToRemove.Add(kvp.Key);
                }
            }
        }

        static private void SetVertexData(ref Vector3 [] vertices,
                                            int boneOffset,
                                            Vector3 upperLeft,
                                            Vector3 bottomLeft,
                                            Vector3 bottomRight,
                                            Vector3 upperRight)
        {
            vertices[boneOffset + 0] = upperLeft;
            vertices[boneOffset + 1] = bottomLeft;
            vertices[boneOffset + 2] = bottomRight;
            vertices[boneOffset + 3] = upperRight;
        }

        static private void SetBoneUV(ref Vector2[] uvs, int boneOffset, Rect uv)
        {
            uvs[boneOffset + 0] = new Vector2(uv.x, uv.yMax);
            uvs[boneOffset + 1] = new Vector2(uv.x, uv.y);
            uvs[boneOffset + 2] = new Vector2(uv.xMax, uv.y);
            uvs[boneOffset + 3] = new Vector2(uv.xMax, uv.yMax);
        }

        static private void SetNormalData(ref Vector3 [] normals,
                                            int boneOffset,
                                            Vector3 normal)
        {
            normals[boneOffset + 0] = normal;
            normals[boneOffset + 1] = normal;
            normals[boneOffset + 2] = normal;
            normals[boneOffset + 3] = normal;
        }

        static private void SetBoneColor(ref Color[] colors, int boneOffset, Color c)
        {
            colors[boneOffset + 0] = c;
            colors[boneOffset + 1] = c;
            colors[boneOffset + 2] = c;
            colors[boneOffset + 3] = c;
        }

        static private int GetMaterialIndex(ref List<Material> materialSource, TextureAtlas atlas)
        {
            if (atlas == null)
            {
                return -1;
            }
            else if (!materialSource.Contains(atlas.material))
            {
                materialSource.Add(atlas.material);
                return (materialSource.Count - 1);
            }
            else
            {
                return materialSource.IndexOf(atlas.material);
            }
        }

        static private TriggerFrame AddTriggerFrame(ref Dictionary<int, AnimationClipTriggerFrames> animationClipTriggerFrames, int clipIndex, int frame)
        {
            AnimationClipTriggerFrames actf;

            if (animationClipTriggerFrames.ContainsKey(clipIndex))
            {
                actf = animationClipTriggerFrames[clipIndex];
            }
            else
            {
                actf = new AnimationClipTriggerFrames(clipIndex);
                animationClipTriggerFrames.Add(clipIndex, actf);
            }

            return actf.AddTriggerFrame(frame);
        }

        static private void BuildPathsForRoot(Transform rootTransform, string rootPath, ref Dictionary<string, Transform> paths)
        {
            string childPath = "";

            foreach (Transform childTransform in rootTransform)
            {
                childPath = (rootPath + "/" + childTransform.name).ToLower();
                paths.Add(childPath, childTransform);
                BuildPathsForRoot(childTransform, childPath, ref paths);
            }
        }

        static private void DisplayProgress(string title, bool showScene, string description, UnityEngine.Object obj, string subDescription, float progressStart, float progressChunkSize, float subProgress)
        {
            float progress = progressStart + (subProgress * progressChunkSize);
            EditorUtility.DisplayProgressBar(title + (showScene ? " [" + GetSceneName(EditorApplication.currentScene) + "]" : ""), description + (obj != null ? " [" + obj.name + "]" : "") + (subDescription != "" ? ": " + subDescription : ""), progress);
        }

        static private void HideProgress()
        {
            EditorUtility.ClearProgressBar();
        }

        static private string GetAssetNameFromPath(string path, bool showExtension)
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

        static private BoneAnimationData GetBoneAnimationDataFromBoneAnimation(BoneAnimation boneAnimation)
        {
            if (boneAnimation == null)
            {
                return null;
            }
            if (boneAnimation.animationData != null)
            {
                boneAnimation.animationDataGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(boneAnimation.animationData));
                boneAnimation.animationData = null;
                return GetBoneAnimationDataFromGUID(boneAnimation.animationDataGUID);
            }
            else if (string.IsNullOrEmpty(boneAnimation.animationDataGUID))
            {
                return null;
            }
            else
            {
                return GetBoneAnimationDataFromGUID(boneAnimation.animationDataGUID);
            }
        }

        static private BoneAnimationData GetBoneAnimationDataFromGUID(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return null;
            else
                return (BoneAnimationData)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(BoneAnimationData));
        }




        // the higher this number, the closer the quaternion curve will match the
        // euler curve. higher values will also cause more processing, however.
        public const int QUATERNION_BAKE_TIME_INTERVAL_COUNT_PER_KEY = 7;

        private class KeyTime
        {
            public float time;
            public float value;

            public KeyTime(float t, float v)
            {
                time = t;
                value = v;
            }
        }

        static public void BakeQuaternionCurves(ref AnimationCurve eulerZCurve,
                                                ref AnimationCurve quaternionXCurve,
                                                ref AnimationCurve quaternionYCurve,
                                                ref AnimationCurve quaternionZCurve,
                                                ref AnimationCurve quaternionWCurve)
        {
            quaternionXCurve = null;
            quaternionYCurve = null;
            quaternionZCurve = null;
            quaternionWCurve = null;

            if (eulerZCurve.length == 0)
                return;

            if (eulerZCurve.keys.Length == 0)
                return;

            List<Keyframe> quaternionXKeys = new List<Keyframe>();
            List<Keyframe> quaternionYKeys = new List<Keyframe>();
            List<Keyframe> quaternionZKeys = new List<Keyframe>();
            List<Keyframe> quaternionWKeys = new List<Keyframe>();


            List<KeyTime> zKeyTimes = new List<KeyTime>();
            List<KeyTime> wKeyTimes = new List<KeyTime>();

            int tIndex;
            Quaternion q;
            float time;
            float intervalCount;
            float maxTime;

            if (eulerZCurve.keys.Length == 1)
            {
                q = Quaternion.Euler(0, 0, eulerZCurve.keys[0].value);

                quaternionXKeys.Add(new Keyframe(eulerZCurve.keys[0].time, 0));
                quaternionYKeys.Add(new Keyframe(eulerZCurve.keys[0].time, 0));
                quaternionZKeys.Add(new Keyframe(eulerZCurve.keys[0].time, q.z, 0, 0));
                quaternionWKeys.Add(new Keyframe(eulerZCurve.keys[0].time, q.w, 0, 0));

                quaternionXCurve = new AnimationCurve(quaternionXKeys.ToArray());
                quaternionYCurve = new AnimationCurve(quaternionYKeys.ToArray());
                quaternionZCurve = new AnimationCurve(quaternionZKeys.ToArray());
                quaternionWCurve = new AnimationCurve(quaternionWKeys.ToArray());

                return;
            }

            maxTime = eulerZCurve.keys[eulerZCurve.keys.Length - 1].time;
            intervalCount = QUATERNION_BAKE_TIME_INTERVAL_COUNT_PER_KEY * (eulerZCurve.keys.Length - 1);

            if (intervalCount > 0)
            {
                // first grab the times and values, storing the Z and W components of the Quaternion to be used for tangent processing
                for (tIndex = 0; tIndex <= intervalCount; tIndex++)
                {
                    time = ((float)tIndex / (float)intervalCount) * maxTime;

                    q = Quaternion.Euler(0, 0, eulerZCurve.Evaluate(time));

                    zKeyTimes.Add(new KeyTime(time, q.z));
                    wKeyTimes.Add(new KeyTime(time, q.w));
                }

                // add key times just before and after the frame times to get more accuracy for steep tangents (like constants).
                // only add points for keys that aren't the beginning or end of the curve by setting the indices for the start and
                // end one key inside.
                Keyframe fKey;
                for (int zKeyIndex = 0; zKeyIndex < eulerZCurve.keys.Length; zKeyIndex++)
                {
                    fKey = eulerZCurve.keys[zKeyIndex];

                    // only add point before if the key time is not zero
                    if (fKey.time > 0)
                    {
                        time = fKey.time - BAKE_NODE_OFFSET;
                        q = Quaternion.Euler(0, 0, eulerZCurve.Evaluate(time));
                        zKeyTimes.Add(new KeyTime(time, q.z));
                        wKeyTimes.Add(new KeyTime(time, q.w));
                    }

                    // only add point after if the key time is not the end of the curve
                    if (fKey.time < maxTime)
                    {
                        time = fKey.time + BAKE_NODE_OFFSET;
                        q = Quaternion.Euler(0, 0, eulerZCurve.Evaluate(time));
                        zKeyTimes.Add(new KeyTime(time, q.z));
                        wKeyTimes.Add(new KeyTime(time, q.w));
                    }
                }
            }

            // sort the new keys so that the extra keys added around the 
            // frames get put into the proper order with the interval keys
            zKeyTimes.Sort(new SortKeyTimesAcending());
            wKeyTimes.Sort(new SortKeyTimesAcending());

            float tanZIn;
            float tanZOut;
            float tanWIn;
            float tanWOut;
            KeyTime keyZ;
            KeyTime nextKeyZ;
            KeyTime prevKeyZ;
            KeyTime keyW;
            KeyTime nextKeyW;
            KeyTime prevKeyW;

            // second, calculate the tangents of the Z and W curves
            for (tIndex = 0; tIndex < zKeyTimes.Count; tIndex++)
            {
                keyZ = zKeyTimes[tIndex];
                keyW = wKeyTimes[tIndex];

                if (tIndex > 0)
                {
                    prevKeyZ = zKeyTimes[tIndex - 1];
                    tanZIn = (keyZ.value - prevKeyZ.value) / (keyZ.time - prevKeyZ.time);

                    prevKeyW = wKeyTimes[tIndex - 1];
                    tanWIn = (keyW.value - prevKeyW.value) / (keyW.time - prevKeyW.time);
                }
                else
                {
                    tanZIn = 0;
                    tanWIn = 0;
                }

                if (tIndex <= intervalCount)
                {
                    nextKeyZ = zKeyTimes[tIndex + 1];
                    tanZOut = (nextKeyZ.value - keyZ.value) / (nextKeyZ.time - keyZ.time);

                    nextKeyW = wKeyTimes[tIndex + 1];
                    tanWOut = (nextKeyW.value - keyW.value) / (nextKeyW.time - keyW.time);
                }
                else
                {
                    tanZOut = 0;
                    tanWOut = 0;
                }

                quaternionXKeys.Add(new Keyframe(keyZ.time, 0));
                quaternionYKeys.Add(new Keyframe(keyZ.time, 0));
                quaternionZKeys.Add(new Keyframe(keyZ.time, keyZ.value, tanZIn, tanZOut));
                quaternionWKeys.Add(new Keyframe(keyZ.time, keyW.value, tanWIn, tanWOut));
            }

            quaternionXCurve = new AnimationCurve(quaternionXKeys.ToArray());
            quaternionYCurve = new AnimationCurve(quaternionYKeys.ToArray());
            quaternionZCurve = new AnimationCurve(quaternionZKeys.ToArray());
            quaternionWCurve = new AnimationCurve(quaternionWKeys.ToArray());
        }

        public class SortKeyTimesAcending : IComparer<KeyTime>
        {
            int IComparer<KeyTime>.Compare(KeyTime a, KeyTime b)
            {
                if (a.time > b.time)
                    return 1;
                if (a.time < b.time)
                    return -1;
                else
                    return 0;
            }
        }
    }
}
