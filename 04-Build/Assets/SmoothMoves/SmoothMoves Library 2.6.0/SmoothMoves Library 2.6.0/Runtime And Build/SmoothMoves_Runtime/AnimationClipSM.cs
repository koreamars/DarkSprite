using UnityEngine;
using System.Collections.Generic;

namespace SmoothMoves
{
    [System.Serializable]
    public class AnimationClipSM
    {
        public AnimationClipSM()
        {
        }

        public static int MAX_FRAMES = 1000;
        public static int MAX_LAYERS = 32;
        public static float DEFAULT_FPS = 5.0f;

        public enum DUPLICATE_MODE
        {
            EntireClip,
            FirstFrames,
            LastFrames
        }

        public string animationName;
        public List<AnimationClipBone> bones;
        public float fps;
        public int maxFrame = -1;
        public WrapMode wrapMode;
        public bool mix;
        public AnimationBlendMode blendMode;
        public int layer;
        public float blendWeight;
        public bool animationNeedsTwoKeyframes;

        public AnimationClipSM(string name)
        {
            animationName = name;
            bones = new List<AnimationClipBone>();
            fps = DEFAULT_FPS;
            maxFrame = -1;
            wrapMode = WrapMode.Once;
            mix = false;
            blendMode = AnimationBlendMode.Blend;
            layer = 0;
            blendWeight = 1.0f;
        }

        public void Clear()
        {
            foreach (AnimationClipBone bone in bones)
            {
                bone.Clear();
            }
            bones.Clear();
            maxFrame = -1;
        }

        public AnimationClipBone AddBone(int bdIndex)
        {
            AnimationClipBone bone = new AnimationClipBone(bdIndex);

            bones.Add(bone);
            SetMaxKeyframe();

            return bone;
        }

        public bool GetKeyframeExists(int boneDataIndex, int frame)
        {
            if (GetIsValidBoneData(boneDataIndex))
                return bones[boneDataIndex].GetKeyframeExists(frame);
            else
                return false;
        }

        public KeyframeSM AddKeyframe(int boneDataIndex, int frame, AnimationClipBone.KEYFRAME_COPY_MODE copyMode, AnimationClipBone.DEFAULT_SETTING defaultSetting)
        {
            if (GetIsValidBoneData(boneDataIndex))
            {
                KeyframeSM keyframe = bones[boneDataIndex].AddKeyframe(boneDataIndex, frame, copyMode, defaultSetting);
                SetMaxKeyframe();

                return keyframe;
            }

            return null;
        }

        public void RemoveKeyframe(int boneDataIndex, int frame)
        {
            if (GetIsValidBoneData(boneDataIndex))
            {
                bones[boneDataIndex].RemoveKeyframe(frame);
                SetMaxKeyframe();
            }
        }

        public void AddSelectedKeyframes(ref List<BoneFrame> boneDataFrames, AnimationClipBone.KEYFRAME_COPY_MODE copyMode, AnimationClipBone.DEFAULT_SETTING defaultSetting)
        {
            if (boneDataFrames.Count == 0)
                return;

            foreach (BoneFrame boneFrame in boneDataFrames)
            {
                if (GetIsValidBoneData(boneFrame.boneDataIndex))
                {
                    bones[boneFrame.boneDataIndex].AddKeyframe(boneFrame.boneDataIndex, boneFrame.frame, copyMode, defaultSetting);
                }
            }

            SetMaxKeyframe();
        }

        public void RemoveSelectedKeyframes(ref List<BoneFrame> boneDataFrames)
        {
            if (boneDataFrames.Count == 0)
                return;

            foreach (BoneFrame boneFrame in boneDataFrames)
            {
                if (GetIsValidBoneData(boneFrame.boneDataIndex))
                    bones[boneFrame.boneDataIndex].RemoveKeyframe(boneFrame.frame);
            }

            SetMaxKeyframe();
        }

        public KeyframeSM GetKeyframe(int frame, int boneDataIndex)
        {
            KeyframeSM keyframe = null;

            if (GetIsValidBoneData(boneDataIndex))
            {
                keyframe = bones[boneDataIndex].GetKeyframe(frame);
            }
            else
            {
                boneDataIndex = -1;
            }

            return keyframe;
        }

        public KeyframeSM GetPreviousKeyframe(int frame, int boneDataIndex, int direction)
        {
            if (GetIsValidBoneData(boneDataIndex))
                return bones[boneDataIndex].GetPreviousKeyframe(frame, direction);
            else
                return null;
        }

        public AnimationClipBone GetAnimationClipBone(int boneDataindex)
        {
            if (GetIsValidBoneData(boneDataindex))
                return bones[boneDataindex];
            else
                return null;
        }

        public void RemoveBone(int boneDataIndex)
        {
            bones[boneDataIndex].Clear();
            bones.RemoveAt(boneDataIndex);

            RecalculateBoneDataIndices();

            SetMaxKeyframe();
        }

        public void ResetKeyframes()
        {
            foreach (AnimationClipBone boneKeyframes in bones)
            {
                boneKeyframes.Reset();
            }

            maxFrame = 0;
        }

        private bool GetIsValidBoneData(int boneDataIndex)
        {
            return (boneDataIndex > -1 && boneDataIndex < bones.Count);
        }

        public void SetMaxKeyframe()
        {
            int boneMaxFrame;

            maxFrame = -1;

            foreach (AnimationClipBone bone in bones)
            {
                boneMaxFrame = bone.GetMaxFrame();
                if (boneMaxFrame > maxFrame)
                    maxFrame = boneMaxFrame;
            }
        }

        public void SetFPS(float framesPerSecond)
        {
            if (framesPerSecond < 0)
                fps = 0;
            else
                fps = framesPerSecond;
        }

        private void RecalculateBoneDataIndices()
        {
            for (int bdIndex = 0; bdIndex < bones.Count; bdIndex++)
            {
                bones[bdIndex].SetBoneDataIndex(bdIndex);
            }
        }

        public void DuplicateClip(AnimationClipSM clip, DUPLICATE_MODE duplicateMode)
        {
            AnimationClipBone newBone;
            KeyframeSM newKeyframe;

            Clear();

            fps = clip.fps;
            wrapMode = clip.wrapMode;
            mix = clip.mix;
            blendMode = clip.blendMode;
            layer = clip.layer;
            blendWeight = clip.blendWeight;

            foreach (AnimationClipBone bone in clip.bones)
            {
                newBone = AddBone(bone.boneDataIndex);
                newBone.mixTransform = bone.mixTransform;

                switch (duplicateMode)
                {
                    case DUPLICATE_MODE.EntireClip:
                        foreach (KeyframeSM keyframe in bone.keyframes)
                        {
                            newKeyframe = newBone.AddKeyframe(keyframe.boneDataIndex, keyframe.frame, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.All);
                            newKeyframe.CopyKeyframe(keyframe);
                        }
                        SetMaxKeyframe();
                        break;

                    case DUPLICATE_MODE.FirstFrames:
                        if (bone.keyframes.Count > 0)
                        {
                            newKeyframe = newBone.AddKeyframe(bone.keyframes[0].boneDataIndex, bone.keyframes[0].frame, AnimationClipBone.KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.All);
                            newKeyframe.CopyKeyframe(bone.keyframes[0]);
                        }
                        SetMaxKeyframe();
                        break;

                    case DUPLICATE_MODE.LastFrames:
                        if (bone.keyframes.Count > 0)
                        {
                            newKeyframe = newBone.GetKeyframe(0);

                            int lastFrame = bone.keyframes[bone.keyframes.Count - 1].frame;
                            float valX;
                            float inTangentX;
                            float outTangentX;
                            float valY;
                            float inTangentY;
                            float outTangentY;
                            float valZ;
                            float inTangentZ;
                            float outTangentZ;

                            newKeyframe.keyframeType = bone.GetPreviousKeyframeType(lastFrame);
                            newKeyframe.atlas = bone.GetPreviousAtlas(lastFrame);
                            newKeyframe.textureGUID = bone.GetPreviousTextureGUID(lastFrame);
                            newKeyframe.pivotOffset = bone.GetPreviousPivotOffset(lastFrame);
                            newKeyframe.useDefaultPivot = bone.GetPreviousUseDefaultPivot(lastFrame);
                            newKeyframe.depth = bone.GetPreviousDepth(lastFrame);
                            newKeyframe.collider.CopyCollider(bone.GetPreviousCollider(lastFrame));

                            bone.GetPreviousLocalPositionX(lastFrame, out valX, out inTangentX, out outTangentX);
                            bone.GetPreviousLocalPositionY(lastFrame, out valY, out inTangentY, out outTangentY);
                            bone.GetPreviousLocalPositionZ(lastFrame, out valZ, out inTangentZ, out outTangentZ);
                            newKeyframe.localPosition3.val = new Vector3(valX, valY, valZ);
                            newKeyframe.localPosition3.inTangentX = inTangentX;
                            newKeyframe.localPosition3.outTangentX = outTangentX;
                            newKeyframe.localPosition3.inTangentY = inTangentY;
                            newKeyframe.localPosition3.outTangentY = outTangentY;
                            newKeyframe.localPosition3.inTangentZ = inTangentZ;
                            newKeyframe.localPosition3.outTangentZ = outTangentZ;

                            bone.GetPreviousLocalRotation(lastFrame, out valX, out inTangentX, out outTangentX);
                            newKeyframe.localRotation.val = valX;
                            newKeyframe.localRotation.inTangent = inTangentX;
                            newKeyframe.localRotation.outTangent = outTangentX;

                            bone.GetPreviousLocalScaleX(lastFrame, out valX, out inTangentX, out outTangentX);
                            bone.GetPreviousLocalScaleY(lastFrame, out valY, out inTangentY, out outTangentY);
                            bone.GetPreviousLocalScaleZ(lastFrame, out valZ, out inTangentZ, out outTangentZ);
                            newKeyframe.localScale3.val = new Vector3(valX, valY, valZ);
                            newKeyframe.localScale3.inTangentX = inTangentX;
                            newKeyframe.localScale3.outTangentX = outTangentX;
                            newKeyframe.localScale3.inTangentY = inTangentY;
                            newKeyframe.localScale3.outTangentY = outTangentY;
                            newKeyframe.localScale3.inTangentZ = inTangentZ;
                            newKeyframe.localScale3.outTangentZ = outTangentZ;

                            bone.GetPreviousImageScaleX(lastFrame, out valX, out inTangentX, out outTangentX);
                            bone.GetPreviousImageScaleY(lastFrame, out valY, out inTangentY, out outTangentY);
                            newKeyframe.imageScale.val = new Vector2(valX, valY);
                            newKeyframe.imageScale.inTangentX = inTangentX;
                            newKeyframe.imageScale.outTangentX = outTangentX;
                            newKeyframe.imageScale.inTangentY = inTangentY;
                            newKeyframe.imageScale.outTangentY = outTangentY;

                            Color color;
                            float bw;
                            float inTangentR;
                            float outTangentR;
                            float inTangentG;
                            float outTangentG;
                            float inTangentB;
                            float outTangentB;
                            float inTangentA;
                            float outTangentA;
                            float inTangentBlendWeight;
                            float outTangentBlendWeight;

                            bone.GetPreviousColor(lastFrame, out color, out bw,
                                                    out inTangentR, out outTangentR,
                                                    out inTangentG, out outTangentG,
                                                    out inTangentB, out outTangentB,
                                                    out inTangentA, out outTangentA,
                                                    out inTangentBlendWeight, out outTangentBlendWeight);
                            newKeyframe.color.val = color;
                            newKeyframe.color.blendWeight = bw;
                            newKeyframe.color.inTangentR = inTangentR;
                            newKeyframe.color.outTangentR = outTangentR;
                            newKeyframe.color.inTangentG = inTangentG;
                            newKeyframe.color.outTangentG = outTangentG;
                            newKeyframe.color.inTangentB = inTangentB;
                            newKeyframe.color.outTangentB = outTangentB;
                            newKeyframe.color.inTangentA = inTangentA;
                            newKeyframe.color.outTangentA = outTangentA;
                            newKeyframe.color.inTangentBlendWeight = inTangentBlendWeight;
                            newKeyframe.color.outTangentBlendWeight = outTangentBlendWeight;

                            newKeyframe.frame = 0;
                        }
                        SetMaxKeyframe();
                        break;
                }
            }
        }

        public void RemoveBlankKeyframes()
        {
            foreach (AnimationClipBone bone in bones)
            {
                bone.RemoveBlankKeyframes();
            }
            SetMaxKeyframe();
        }

        public void ConvertKeyframesToLocalPosition3()
        {
            foreach (AnimationClipBone bone in bones)
            {
                bone.ConvertKeyframesToLocalPosition3();
            }
        }

        public void CreateColor()
        {
            foreach (AnimationClipBone bone in bones)
            {
                bone.CreateColor();
            }
        }

        public bool EvaluateAnimationBoneColor(int boneDataIndex, float time, out Color color, out float blendWeight)
        {
            color = Color.black;
            blendWeight = 0;

            if (!mix || (mix && bones[boneDataIndex].mixTransform))
            {
                return bones[boneDataIndex].EvaluateAnimationColor(time, out color, out blendWeight);
            }

            return false;
        }

        public void AddLocalScale3()
        {
            foreach (AnimationClipBone bone in bones)
            {
                bone.AddLocalScale3();
            }
        }

        public void InitializeBoneVisibility()
        {
            foreach (AnimationClipBone bone in bones)
            {
                bone.InitializeVisibility();
            }
        }

        public void InsertFrames(int boneDataIndex, int atFrame, int frameCount)
        {
            if (!GetIsValidBoneData(boneDataIndex))
                return;

            bones[boneDataIndex].InsertFrames(atFrame, frameCount);

            SetMaxKeyframe();
        }

        public void InsertFrames(int atFrame, int frameCount)
        {
            foreach (AnimationClipBone bone in bones)
            {
                bone.InsertFrames(atFrame, frameCount);
            }

            SetMaxKeyframe();
        }

        public void DeleteFrames(int boneDataIndex, int atFrame, int frameCount)
        {
            if (!GetIsValidBoneData(boneDataIndex))
                return;

            bones[boneDataIndex].DeleteFrames(atFrame, frameCount);

            SetMaxKeyframe();
        }

        public void DeleteFrames(int atFrame, int frameCount)
        {
            foreach (AnimationClipBone bone in bones)
            {
                bone.DeleteFrames(atFrame, frameCount);
            }

            SetMaxKeyframe();
        }

        public void SetDefaultColliderTag()
        {
            foreach (AnimationClipBone bone in bones)
            {
                bone.SetDefaultColliderTag();
            }
        }
    }
}
