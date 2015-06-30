using UnityEngine;
using System.Collections.Generic;

namespace SmoothMoves
{
    public class BoneAnimationData : ScriptableObject
    {
        public BoneAnimationData()
        {
        }

        private class SortIntAscending : IComparer<int>
        {
            int IComparer<int>.Compare(int a, int b)
            {
                if (a > b)
                    return 1;
                if (a < b)
                    return -1;
                else
                    return 0;
            }
        }

        private class SortIntDescending : IComparer<int>
        {
            int IComparer<int>.Compare(int a, int b)
            {
                if (a > b)
                    return -1;
                if (a < b)
                    return 1;
                else
                    return 0;
            }
        }

        private BoneTree _boneTreeRoot = null;

        [HideInInspector]
        public string buildID;
        [HideInInspector]
        public bool needsRebuilt;
        [HideInInspector]
        public List<BoneData> boneDataList = new List<BoneData>();
        [HideInInspector]
        public List<DFSBoneNode> dfsBoneNodeList = new List<DFSBoneNode>();
        [HideInInspector]
        public List<AnimationClipSM> animationClips = new List<AnimationClipSM>();
        [HideInInspector]
        public List<string> boneTransformPaths = new List<string>();
        [HideInInspector]
        public List<string> spriteTransformPaths = new List<string>();
        [HideInInspector]
        public Color meshColor = Color.white;
        [HideInInspector]
        public float importScale = 1.0f;
        [HideInInspector]
        public int dataVersion;
        [HideInInspector]
        public TextureAtlasDictionary textureAtlasDictionary;
        [HideInInspector]
        public List<Material> materialSource = new List<Material>();
        [HideInInspector]
        public List<AnimationClip> animationClipSourceAssets = new List<AnimationClip>();
        [HideInInspector]
        public AnimationClipTriggerFrames[] animationClipTriggerFrames;
        [HideInInspector]
        public List<TriggerFrame> triggerFrames = new List<TriggerFrame>();

        [HideInInspector]
        public Vector3[] vertices;
        [HideInInspector]
        public Vector2[] uvs;
        [HideInInspector]
        public Color[] colors;
        [HideInInspector]
        public Vector3[] normals;
        [HideInInspector]
        public Matrix4x4[] bindPoses;

        public int BoneCount
        {
            get
            {
                if (boneDataList == null)
                    return 0;
                else
                    return boneDataList.Count;
            }
        }

        public int AnimationClipCount
        {
            get
            {
                if (animationClips == null)
                    return 0;
                else
                    return animationClips.Count;
            }
        }

        void OnDisable()
        {
        }

        public void Initialize()
        {
            bool createNew = false;
            if (boneDataList == null)
                createNew = true;
            else if (boneDataList.Count == 0)
                createNew = true;

            if (animationClips == null)
            {
                animationClips = new List<AnimationClipSM>();
            }

            if (createNew)
            {
                _boneTreeRoot = new BoneTree();

                CreateRootBone();
            }
        }

        #region Bones

        public int GetBoneNodeIndexFromName(string boneName)
        {
            return GetBoneNodeIndex(GetBoneDataIndexFromName(boneName, false));
        }

        public int GetBoneDataIndexFromName(string boneName, bool ignoreCasing)
        {
            for (int boneDataIndex = 0; boneDataIndex < boneDataList.Count; boneDataIndex++)
            {
                if (ignoreCasing)
                {
                    if (boneDataList[boneDataIndex].boneName.ToLower() == boneName.ToLower())
                    {
                        return boneDataIndex;
                    }
                }
                else
                {
                    if (boneDataList[boneDataIndex].boneName == boneName)
                    {
                        return boneDataIndex;
                    }
                }
            }

            return -1;
        }

        public bool BoneNameExists(string boneName)
        {
            return (GetBoneDataIndexFromName(boneName, true) != -1);
        }

        private void CreateRootBone()
        {
            boneDataList.Add(new BoneData("Root"));
            dfsBoneNodeList.Add(new DFSBoneNode(-1, 0, 0));

            foreach (AnimationClipSM clip in animationClips)
            {
                clip.AddBone(boneDataList.Count - 1);
            }
        }

        public int AddBone(int parentBoneNodeIndex, string boneName)
        {
            GenerateBoneTree();

            BoneTree parentBoneTree = _boneTreeRoot.GetBoneTreeWithDataIndex(dfsBoneNodeList[parentBoneNodeIndex].boneDataIndex);
            if (parentBoneTree != null)
            {
                BoneData boneData = new BoneData(boneName);
                boneDataList.Add(boneData);

                parentBoneTree.AddNewChild(BoneCount - 1);
            }

            GenerateDFSList();

            foreach (AnimationClipSM clip in animationClips)
            {
                clip.AddBone(boneDataList.Count - 1);
            }

            for (int boneNodeIndex = 0; boneNodeIndex < dfsBoneNodeList.Count; boneNodeIndex++)
            {
                if (dfsBoneNodeList[boneNodeIndex].boneDataIndex == (boneDataList.Count - 1))
                    return boneNodeIndex;
            }

            return -1;
        }

        public void RemoveBone(int boneNodeIndex)
        {
            BoneTree boneTree;
            List<DFSBoneNode> boneNodesToRemove = new List<DFSBoneNode>();
            List<int> boneDataIndicesToRemove = new List<int>();
            int dfsIndex = 0;
            int bdrIndex;

            GenerateBoneTree();

            // get the subtree that will be removed
            boneTree = _boneTreeRoot.GetBoneTreeWithDataIndex(dfsBoneNodeList[boneNodeIndex].boneDataIndex);
            boneTree.AddDFSChildren(ref boneNodesToRemove, boneNodeIndex, ref dfsIndex);

            // gather the bone data indices that will be removed
            foreach (DFSBoneNode boneNode in boneNodesToRemove)
            {
                boneDataIndicesToRemove.Add(boneNode.boneDataIndex);
            }

            // kill the subtree
            boneTree.Orphan();
            boneTree.Clear();

            // regenerate the dfs list
            GenerateDFSList();

            // sort the bone data indices so that we can adjust the dfs list's data indices correctly
            boneDataIndicesToRemove.Sort(new SortIntAscending());

            // adjust the dfs list indices based on the bone data that was removed
            foreach (DFSBoneNode boneNode in dfsBoneNodeList)
            {
                for (bdrIndex = boneDataIndicesToRemove.Count - 1; bdrIndex >= 0; bdrIndex--)
                {
                    if (boneNode.boneDataIndex > boneDataIndicesToRemove[bdrIndex])
                    {
                        boneNode.boneDataIndex -= (bdrIndex + 1);
                        break;
                    }
                }
            }

            // remove the bone data from the list starting with the last element and working backward
            boneDataIndicesToRemove.Sort(new SortIntDescending());

            for (bdrIndex = 0; bdrIndex < boneDataIndicesToRemove.Count; bdrIndex++)
            {
                boneDataList[boneDataIndicesToRemove[bdrIndex]].Clear();
                boneDataList.RemoveAt(boneDataIndicesToRemove[bdrIndex]);

                // remove bones and keyframes from animation clips
                foreach (AnimationClipSM clip in animationClips)
                {
                    clip.RemoveBone(boneDataIndicesToRemove[bdrIndex]);
                }
            }
        }

        public void MoveBoneToParent(int boneNodeIndex, int parentBoneNodeIndex)
        {
            GenerateBoneTree();

            BoneTree boneTree = GetBoneTreeWithDFSIndex(boneNodeIndex);
            BoneTree parentBoneTree = GetBoneTreeWithDFSIndex(parentBoneNodeIndex);

            if (boneTree == null || parentBoneTree == null)
                return;

            parentBoneTree.AddExistingChild(boneTree);

            GenerateDFSList();
        }

        public bool GetIsBoneDescendant(int boneNodeIndex, int descendentBoneNodeIndex)
        {
            GenerateBoneTree();

            BoneTree boneTree = GetBoneTreeWithDFSIndex(boneNodeIndex);
            BoneTree descendentBoneTree = GetBoneTreeWithDFSIndex(descendentBoneNodeIndex);

            if (boneTree == null || descendentBoneTree == null)
                return false;

            return boneTree.HasDescendant(descendentBoneTree);
        }

        public void ClearBones()
        {
            if (_boneTreeRoot == null)
                _boneTreeRoot = new BoneTree();
            else
                _boneTreeRoot.Clear();

            dfsBoneNodeList.Clear();
            boneDataList.Clear();

            foreach (AnimationClipSM clip in animationClips)
            {
                clip.Clear();
            }

            CreateRootBone();
        }

        private void GenerateDFSList()
        {
            dfsBoneNodeList.Clear();

            int dfsIndex = 0;

            _boneTreeRoot.AddDFSChildren(ref dfsBoneNodeList, -1, ref dfsIndex);
        }

        public BoneTree GenerateBoneTree()
        {
            if (_boneTreeRoot == null)
                _boneTreeRoot = new BoneTree();
            else
                _boneTreeRoot.Clear();

            foreach (DFSBoneNode node in dfsBoneNodeList)
            {
                if (node.parentBoneIndex == -1)
                {
                    _boneTreeRoot.boneDataIndex = 0;
                    _boneTreeRoot.depth = 0;
                }
                else
                {
                    BoneTree parentBoneTree = GetBoneTreeWithDFSIndex(node.parentBoneIndex);
                    if (parentBoneTree != null)
                    {
                        parentBoneTree.AddNewChild(node.boneDataIndex);
                    }
                }
            }

            return _boneTreeRoot;
        }

        public BoneData GetBoneData(int boneNodeIndex)
        {
            int boneDataIndex = GetBoneDataIndex(boneNodeIndex);
            if (boneDataIndex != -1)
                return boneDataList[boneDataIndex];
            else
                return null;
        }

        private BoneTree GetBoneTreeWithDFSIndex(int boneNodeIndex)
        {
            return _boneTreeRoot.GetBoneTreeWithDataIndex(GetBoneDataIndex(boneNodeIndex));
        }

        private bool GetIsValidBoneNode(int boneNodeIndex)
        {
            return (boneNodeIndex > -1 && boneNodeIndex < dfsBoneNodeList.Count);
        }

        private bool GetIsValidBoneData(int boneDataIndex)
        {
            return (boneDataIndex > -1 && boneDataIndex < boneDataList.Count);
        }

        public int GetBoneDataIndex(int boneNodeIndex)
        {
            if (GetIsValidBoneNode(boneNodeIndex))
                return dfsBoneNodeList[boneNodeIndex].boneDataIndex;
            else
                return -1;
        }

        public int GetBoneNodeIndex(int boneDataIndex)
        {
            if (GetIsValidBoneData(boneDataIndex))
            {
                for (int boneNodeIndex = 0; boneNodeIndex < dfsBoneNodeList.Count; boneNodeIndex++)
                {
                    if (dfsBoneNodeList[boneNodeIndex].boneDataIndex == boneDataIndex)
                        return boneNodeIndex;
                }
            }

            return -1;
        }

        public void GenerateBoneTransformPaths()
        {
            GenerateBoneTree();

            if (boneTransformPaths == null)
                boneTransformPaths = new List<string>();
            else
                boneTransformPaths.Clear();

            if (spriteTransformPaths == null)
                spriteTransformPaths = new List<string>();
            else
                spriteTransformPaths.Clear();

            string boneTransformPath;
            string spriteTransformPath;

            for (int boneNodeIndex = 0; boneNodeIndex < dfsBoneNodeList.Count; boneNodeIndex++)
            {
                CalculateBoneTransformPath(boneNodeIndex, out boneTransformPath, out spriteTransformPath);

                boneTransformPaths.Add(boneTransformPath);
                spriteTransformPaths.Add(spriteTransformPath);
            }
        }

        private void CalculateBoneTransformPath(int boneNodeIndex, out string boneTransformPath, out string spriteTransformPath)
        {
            BoneTree boneTree = GetBoneTreeWithDFSIndex(boneNodeIndex);
            string transformPath = "";
            string childBone = "";

            while (boneTree != null)
            {
                if (childBone == "")
                    childBone = boneDataList[boneTree.boneDataIndex].boneName;

                transformPath = boneDataList[boneTree.boneDataIndex].boneName + (transformPath != "" ? "/" + transformPath : "");
                boneTree = boneTree.parent;
            }

            boneTransformPath = transformPath;

            if (childBone != "")
                spriteTransformPath = transformPath + "/" + childBone + "_Sprite";
            else
                spriteTransformPath = "";
        }

        public void MakeBonesChildrenOfBone(int boneNodeIndex)
        {
            GenerateBoneTree();

            if (boneNodeIndex > 0 && _boneTreeRoot.children.Count > 0)
            {
                BoneTree newParentBoneTree = GetBoneTreeWithDFSIndex(boneNodeIndex);
                BoneTree childBone;
                int index = 0;

                index = 0;
                while (index < _boneTreeRoot.children.Count)
                {
                    childBone = _boneTreeRoot.children[index];

                    if (!childBone.HasDescendant(newParentBoneTree))
                    {
                        newParentBoneTree.AddExistingChild(childBone);
                    }
                    else
                    {
                        index++;
                    }
                }
            }

            GenerateDFSList();
        }

        #endregion

        #region Timeline

        public KeyframeSM AddKeyframe(int clipIndex, int boneNodeIndex, int frame, AnimationClipBone.KEYFRAME_COPY_MODE copyMode, AnimationClipBone.DEFAULT_SETTING defaultSetting)
        {
            if (GetIsValidClip(clipIndex))
                return animationClips[clipIndex].AddKeyframe(GetBoneDataIndex(boneNodeIndex), frame, copyMode, defaultSetting);
            else
                return null;
        }

        public void RemoveKeyframe(int clipIndex, int boneNodeIndex, int frame)
        {
            if (GetIsValidClip(clipIndex))
                animationClips[clipIndex].RemoveKeyframe(GetBoneDataIndex(boneNodeIndex), frame);
        }

        public void AddSelectedKeyframes(int clipIndex, ref List<BoneFrame> boneFrames, AnimationClipBone.KEYFRAME_COPY_MODE copyMode, AnimationClipBone.DEFAULT_SETTING defaultSetting)
        {
            if (GetIsValidClip(clipIndex))
            {
                List<BoneFrame> boneDataFrames = TranslateSelectedFramesToBoneData(ref boneFrames);
                animationClips[clipIndex].AddSelectedKeyframes(ref boneDataFrames, copyMode, defaultSetting);
            }
        }

        public void RemoveSelectedKeyframes(int clipIndex, ref List<BoneFrame> boneFrames)
        {
            if (GetIsValidClip(clipIndex))
            {
                List<BoneFrame> boneDataFrames = TranslateSelectedFramesToBoneData(ref boneFrames);
                animationClips[clipIndex].RemoveSelectedKeyframes(ref boneDataFrames);
            }
        }

        private List<BoneFrame> TranslateSelectedFramesToBoneData(ref List<BoneFrame> boneFrames)
        {
            List<BoneFrame> newBoneFrames = new List<BoneFrame>();

            foreach (BoneFrame boneFrame in boneFrames)
            {
                newBoneFrames.Add(new BoneFrame(dfsBoneNodeList[boneFrame.boneNodeIndex].boneDataIndex, boneFrame.boneNodeIndex, boneFrame.frame));
            }

            return newBoneFrames;
        }

        #endregion

        #region Animation Clips

        public int GetClipIndexFromName(string clipName, bool ignoreCasing)
        {
            for (int clipIndex = 0; clipIndex < animationClips.Count; clipIndex++)
            {
                if (ignoreCasing)
                {
                    if (animationClips[clipIndex].animationName.ToLower() == clipName.ToLower())
                    {
                        return clipIndex;
                    }
                }
                else
                {
                    if (animationClips[clipIndex].animationName == clipName)
                    {
                        return clipIndex;
                    }
                }
            }

            return -1;
        }

        public int GetClipIndex(AnimationClipSM clip)
        {
            if (clip == null)
                return -1;

            int clipIndex = 0;
            foreach (AnimationClipSM otherClip in animationClips)
            {
                if (otherClip == clip)
                    return clipIndex;

                clipIndex++;
            }

            return -1;
        }

        public bool AnimationClipNameExists(string clipName)
        {
            return (GetClipIndexFromName(clipName, true) != -1);
        }

        public void AddAnimationClip(string animationName)
        {
            AnimationClipSM clip = new AnimationClipSM(animationName);
            animationClips.Add(clip);

            for (int boneDataIndex = 0; boneDataIndex < boneDataList.Count; boneDataIndex++)
            {
                clip.AddBone(boneDataIndex);
            }
        }

        public void DuplicateAnimationClip(int clipIndex, AnimationClipSM.DUPLICATE_MODE duplicateMode, string clipName)
        {
            AnimationClipSM clip = new AnimationClipSM(clipName);

            if (GetIsValidClip(clipIndex))
                clip.DuplicateClip(animationClips[clipIndex], duplicateMode);

            animationClips.Add(clip);
        }

        public void RemoveAnimationClip(int clipIndex)
        {
            if (GetIsValidClip(clipIndex))
            {
                animationClips[clipIndex].Clear();
                animationClips.RemoveAt(clipIndex);
            }
        }

        public void ClearAnimations()
        {
            animationClips.Clear();
        }

        public bool GetKeyframeIsSet(int clipIndex, int boneNodeIndex, int frame)
        {
            if (GetIsValidClip(clipIndex))
                return animationClips[clipIndex].GetKeyframeExists(GetBoneDataIndex(boneNodeIndex), frame);
            else
                return false;
        }

        public KeyframeSM GetKeyframe(int clipIndex, int boneNodeIndex, int frame, out int boneDataIndex)
        {
            boneDataIndex = GetBoneDataIndex(boneNodeIndex);

            if (GetIsValidClip(clipIndex))
                return animationClips[clipIndex].GetKeyframe(frame, boneDataIndex);
            else
                return null;
        }

        public KeyframeSM GetKeyframe(int clipIndex, int boneDataIndex, int frame)
        {
            if (GetIsValidClip(clipIndex))
                return animationClips[clipIndex].GetKeyframe(frame, boneDataIndex);
            else
                return null;
        }

        public KeyframeSM GetPreviousKeyframeFromBoneDataIndex(int clipIndex, int frame, int boneDataIndex, int direction)
        {
            if (GetIsValidClip(clipIndex))
                return animationClips[clipIndex].GetPreviousKeyframe(frame, boneDataIndex, direction);
            else
                return null;
        }

        public AnimationClipBone GetAnimationClipBoneFromBoneDataIndex(int clipIndex, int boneDataIndex)
        {
            if (GetIsValidClip(clipIndex))
                return animationClips[clipIndex].GetAnimationClipBone(boneDataIndex);
            else
                return null;
        }

        public void ResetKeyframes(int clipIndex)
        {
            if (GetIsValidClip(clipIndex))
                animationClips[clipIndex].ResetKeyframes();
        }

        private bool GetIsValidClip(int clipIndex)
        {
            return (clipIndex != -1 && clipIndex < animationClips.Count);
        }

        public void RemoveBlankKeyframes(int clipIndex)
        {
            if (GetIsValidClip(clipIndex))
            {
                animationClips[clipIndex].RemoveBlankKeyframes();
            }
        }

        #endregion

        public void RecalculateTextureIndices()
        {
            TextureAtlas atlas;
            string textureGUID;

            foreach (AnimationClipSM clip in animationClips)
            {
                foreach (AnimationClipBone bone in clip.bones)
                {
                    foreach (KeyframeSM keyframe in bone.keyframes)
                    {
                        atlas = bone.GetPreviousAtlas(keyframe.frame);
                        textureGUID = bone.GetPreviousTextureGUID(keyframe.frame);

                        if (atlas == null)
                        {
                            keyframe.textureIndex = -1;
                        }
                        else
                        {
                            keyframe.textureIndex = atlas.GetTextureIndex(textureGUID);
                        }
                    }
                }
            }
        }

        public void GenerateBuildID()
        {
            System.DateTime d = System.DateTime.Now;
            buildID = d.ToString("yyyyMMddHHmmssffff");

            needsRebuilt = true;
        }

        public bool UpdateDataVersion()
        {
            // Versions
            // 0: Initial Release
            // 2: Added localPosition3 for Z positioning
            // 3: Added bone colors
            // 4: Added bone animation colors, import scale
            // 5: Added localScale3
            // 6: Added visibility to the animationclipbone

            int startingVersion = dataVersion;

            if (dataVersion == 0)
            {
                foreach (AnimationClipSM clip in animationClips)
                {
                    clip.ConvertKeyframesToLocalPosition3();
                }

                dataVersion = 2;
            }

            if (dataVersion == 2)
            {
                foreach (BoneData boneData in boneDataList)
                {
                    boneData.boneColor.blendingWeight = 0;
                    boneData.boneColor.color = Color.white;
                }

                dataVersion = 3;
            }

            if (dataVersion == 3)
            {
                foreach (AnimationClipSM clip in animationClips)
                {
                    clip.CreateColor();
                }

                if (importScale == 0)
                    importScale = 1.0f;

                dataVersion = 4;
            }

            if (dataVersion == 4)
            {
                foreach (AnimationClipSM clip in animationClips)
                {
                    clip.AddLocalScale3();
                }

                dataVersion = 5;
            }

            if (dataVersion == 5)
            {
                foreach (AnimationClipSM clip in animationClips)
                {
                    clip.InitializeBoneVisibility();
                }

                dataVersion = 6;
            }

            if (dataVersion == 6)
            {
                foreach (BoneData boneData in boneDataList)
                {
                    boneData.active = true;
                }

                dataVersion = 7;
            }

            if (dataVersion == 7)
            {
                foreach (AnimationClipSM clip in animationClips)
                {
                    clip.SetDefaultColliderTag();
                }

                dataVersion = 8;
            }

            // gather atlases and trim negative keyframes
            List<TextureAtlas> atlases = new List<TextureAtlas>();
            for (int clipIndex = 0; clipIndex < animationClips.Count; clipIndex++)
            {
                foreach (AnimationClipBone bone in animationClips[clipIndex].bones)
                {
                    bone.TrimNegativeKeyframes();

                    foreach (KeyframeSM kf in bone.keyframes)
                    {
                        if (kf.atlas != null && (kf.useAtlas || kf.frame == 0))
                        {
                            if (!atlases.Contains(kf.atlas))
                            {
                                atlases.Add(kf.atlas);
                            }
                        }
                    }
                }
            }
            // update the data versions of the atlases for this animation
            foreach (TextureAtlas atlas in atlases)
            {
                atlas.UpdateDataVersion();
            }

            return (startingVersion != dataVersion);
        }

        public void InsertFramesForBoneDataIndex(int clipIndex, int boneDataIndex, int atFrame, int frameCount)
        {
            if (!GetIsValidClip(clipIndex))
                return;

            animationClips[clipIndex].InsertFrames(boneDataIndex, atFrame, frameCount);
        }

        public void InsertFrames(int clipIndex, int atFrame, int frameCount)
        {
            if (!GetIsValidClip(clipIndex))
                return;

            animationClips[clipIndex].InsertFrames(atFrame, frameCount);
        }

        public void DeleteFramesForBoneDataIndex(int clipIndex, int boneDataIndex, int atFrame, int frameCount)
        {
            if (!GetIsValidClip(clipIndex))
                return;

            animationClips[clipIndex].DeleteFrames(boneDataIndex, atFrame, frameCount);
        }

        public void DeleteFrames(int clipIndex, int atFrame, int frameCount)
        {
            if (!GetIsValidClip(clipIndex))
                return;

            animationClips[clipIndex].DeleteFrames(atFrame, frameCount);
        }

        public bool ContainsAtlas(TextureAtlas atlas)
        {
            foreach (AnimationClipSM clip in animationClips)
            {
                foreach (AnimationClipBone bone in clip.bones)
                {
                    foreach (KeyframeSM keyframe in bone.keyframes)
                    {
                        if (atlas == bone.GetPreviousAtlas(keyframe.frame))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
