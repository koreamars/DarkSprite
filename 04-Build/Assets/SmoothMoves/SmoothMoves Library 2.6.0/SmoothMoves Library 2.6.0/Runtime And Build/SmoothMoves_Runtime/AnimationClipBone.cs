using UnityEngine;
using System.Collections.Generic;

namespace SmoothMoves
{
    [System.Serializable]
    public class AnimationClipBone
    {
        private bool _animatesColor = false;
        private AnimationCurve _colorRCurve = null;
        private AnimationCurve _colorGCurve = null;
        private AnimationCurve _colorBCurve = null;
        private AnimationCurve _colorACurve = null;
        private AnimationCurve _colorBlendWeightCurve = null;

        public enum KEYFRAME_COPY_MODE
        {
            None,
            CopyFirst,
            CopyPrevious
        }

        public enum DEFAULT_SETTING
        {
            Blank,
            All,
            PositionRotation,
            Texture
        }

        public int boneDataIndex;
        public List<KeyframeSM> keyframes;
        public bool mixTransform;
        public bool visible;
        public AnimationCurveSerializable colorRCurveSerialized;
        public AnimationCurveSerializable colorGCurveSerialized;
        public AnimationCurveSerializable colorBCurveSerialized;
        public AnimationCurveSerializable colorACurveSerialized;
        public AnimationCurveSerializable colorBlendWeightCurveSerialized;

        public AnimationClipBone(int bdIndex)
        {
            boneDataIndex = bdIndex;
            visible = true;
    
            keyframes = new List<KeyframeSM>();

            KeyframeSM keyframe = new KeyframeSM(bdIndex, AnimationClipBone.DEFAULT_SETTING.All);
            keyframes.Add(keyframe);
        }

        public void Reset()
        {
            Clear();
            AddKeyframe(boneDataIndex, 0, KEYFRAME_COPY_MODE.None, AnimationClipBone.DEFAULT_SETTING.All);
        }

        public void Clear()
        {
            if (keyframes != null)
            {
                foreach (KeyframeSM keyframe in keyframes)
                {
                    keyframe.Clear();
                }
                keyframes.Clear();
            }
        }

        public void ClearColorKeys()
        {
            if (colorRCurveSerialized == null)
                colorRCurveSerialized = new AnimationCurveSerializable();
            else
                colorRCurveSerialized.Clear();

            if (colorGCurveSerialized == null)
                colorGCurveSerialized = new AnimationCurveSerializable();
            else
                colorGCurveSerialized.Clear();

            if (colorBCurveSerialized == null)
                colorBCurveSerialized = new AnimationCurveSerializable();
            else
                colorBCurveSerialized.Clear();

            if (colorACurveSerialized == null)
                colorACurveSerialized = new AnimationCurveSerializable();
            else
                colorACurveSerialized.Clear();

            if (colorBlendWeightCurveSerialized == null)
                colorBlendWeightCurveSerialized = new AnimationCurveSerializable();
            else
                colorBlendWeightCurveSerialized.Clear();
        }

        public void SetBoneDataIndex(int bdIndex)
        {
            boneDataIndex = bdIndex;

            foreach (KeyframeSM keyframe in keyframes)
            {
                keyframe.boneDataIndex = bdIndex;
            }
        }

        public bool GetKeyframeExists(int frame)
        {
            foreach (KeyframeSM keyframe in keyframes)
            {
                if (keyframe.frame == frame)
                    return true;
            }

            return false;
        }

        public KeyframeSM GetKeyframe(int frame)
        {
            foreach (KeyframeSM keyframe in keyframes)
            {
                if (keyframe.frame == frame)
                    return keyframe;
            }

            return null;
        }

        public KeyframeSM AddKeyframe(int bdIndex, int frame, KEYFRAME_COPY_MODE copyMode, DEFAULT_SETTING defaultSetting)
        {
            KeyframeSM keyframe = GetKeyframe(frame);

            if (keyframe == null)
            {
                keyframe = new KeyframeSM(bdIndex, defaultSetting);

                switch (copyMode)
                {
                    case KEYFRAME_COPY_MODE.CopyPrevious:
                        KeyframeSM copyKeyframe = GetPreviousKeyframe(frame, 1);
                        if (copyKeyframe != null)
                        {
                            keyframe.CopyKeyframe(copyKeyframe);
                        }
                        break;

                    case KEYFRAME_COPY_MODE.CopyFirst:
                        keyframe.CopyKeyframe(keyframes[0]);
                        break;
                }

                keyframe.frame = frame;

                for (int index = 0; index < keyframes.Count; index++)
                {
                    if (frame < keyframes[index].frame)
                    {
                        keyframes.Insert(index, keyframe);
                        return keyframe;
                    }
                }

                keyframes.Add(keyframe);
            }
            else
            {
                switch (copyMode)
                {
                    case KEYFRAME_COPY_MODE.CopyPrevious:
                        KeyframeSM copyKeyframe = GetPreviousKeyframe(frame - 1, 1);
                        if (copyKeyframe != null)
                        {
                            keyframe.CopyKeyframe(copyKeyframe);
                        }
                        break;

                    case KEYFRAME_COPY_MODE.CopyFirst:
                        keyframe.CopyKeyframe(keyframes[0]);
                        break;
                }

                keyframe.frame = frame;
            }

            return keyframe;
        }

        public void RemoveKeyframe(int frame)
        {
            if (frame > 0)
            {
                for (int keyframeIndex = 0; keyframeIndex < keyframes.Count; keyframeIndex++)
                {
                    if (keyframes[keyframeIndex].frame == frame)
                    {
                        keyframes.RemoveAt(keyframeIndex);
                        break;
                    }
                }
            }
        }

        public int GetMaxFrame()
        {
            int max = -1;
            foreach (KeyframeSM keyframe in keyframes)
            {
                if (keyframe.BeingUsed)
                {
                    if (keyframe.frame > max)
                    {
                        max = keyframe.frame;
                    }
                }
            }

            return max;
        }

        public KeyframeSM GetPreviousKeyframe(int frame, int direction)
        {
            // direction is play direction, not direction of previous

            if (direction == 1)
            {
                // forward
                for (int index = keyframes.Count - 1; index >= 0; index--)
                {
                    if (keyframes[index].frame <= frame)
                        return keyframes[index];
                }
            }
            else
            {
                // reverse (used in evaluating ping-pong wrapmode
                for (int index = 0; index < keyframes.Count; index++)
                {
                    if (keyframes[index].frame >= frame)
                        return keyframes[index];
                }

                // could not find a keyframe going in reverse, so we use the last keyframe going forward
                return GetPreviousKeyframe(frame, 1);
            }

            return null;
        }

        public KeyframeSM.KEYFRAME_TYPE GetPreviousKeyframeType(int frame)
        {
            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].useKeyframeType)
                    return keyframes[index].keyframeType;
            }

            return KeyframeSM.KEYFRAME_TYPE.TransformOnly;
        }

        public TextureAtlas GetPreviousAtlas(int frame)
        {
            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].useAtlas)
                    return keyframes[index].atlas;
            }

            return null;
        }

        public string GetPreviousTextureGUID(int frame)
        {
            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].useTextureGUID)
                {
                    return keyframes[index].textureGUID;
                }
            }

            return "";
        }

        public Vector2 GetPreviousPivotOffset(int frame)
        {
            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].usePivotOffset)
                {
                    return keyframes[index].pivotOffset;
                }
            }

            return Vector2.zero;
        }

        public bool GetPreviousUseDefaultPivot(int frame)
        {
            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].usePivotOffset)
                {
                    return keyframes[index].useDefaultPivot;
                }
            }

            return true;
        }

        public int GetPreviousDepth(int frame)
        {
            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].useDepth)
                {
                    return keyframes[index].depth;
                }
            }

            return 0;
        }

        public ColliderSM GetPreviousCollider(int frame)
        {
            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].useCollider)
                {
                    return keyframes[index].collider;
                }
            }

            return null;
        }

        public void GetPreviousLocalPositionX(int frame, out float val, out float inTangent, out float outTangent)
        {
            val = 0;
            inTangent = 0;
            outTangent = 0;

            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].localPosition3.useX)
                {
                    val = keyframes[index].localPosition3.val.x;
                    inTangent = keyframes[index].localPosition3.inTangentX;
                    outTangent = keyframes[index].localPosition3.outTangentX;
                    break;
                }
            }
        }

        public void GetPreviousLocalPositionY(int frame, out float val, out float inTangent, out float outTangent)
        {
            val = 0;
            inTangent = 0;
            outTangent = 0;

            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].localPosition3.useY)
                {
                    val = keyframes[index].localPosition3.val.y;
                    inTangent = keyframes[index].localPosition3.inTangentY;
                    outTangent = keyframes[index].localPosition3.outTangentY;
                    break;
                }
            }
        }

        public void GetPreviousLocalPositionZ(int frame, out float val, out float inTangent, out float outTangent)
        {
            val = 0;
            inTangent = 0;
            outTangent = 0;

            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].localPosition3.useZ)
                {
                    val = keyframes[index].localPosition3.val.z;
                    inTangent = keyframes[index].localPosition3.inTangentZ;
                    outTangent = keyframes[index].localPosition3.outTangentZ;
                    break;
                }
            }
        }

        public void GetPreviousLocalRotation(int frame, out float val, out float inTangent, out float outTangent)
        {
            val = 0;
            inTangent = 0;
            outTangent = 0;

            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].localRotation.use)
                {
                    val = keyframes[index].localRotation.val;
                    inTangent = keyframes[index].localRotation.inTangent;
                    outTangent = keyframes[index].localRotation.outTangent;
                    break;
                }
            }
        }

        public void GetPreviousLocalScaleX(int frame, out float val, out float inTangent, out float outTangent)
        {
            val = 0;
            inTangent = 0;
            outTangent = 0;

            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].localScale3.useX)
                {
                    val = keyframes[index].localScale3.val.x;
                    inTangent = keyframes[index].localScale3.inTangentX;
                    outTangent = keyframes[index].localScale3.outTangentX;
                    break;
                }
            }
        }

        public void GetPreviousLocalScaleY(int frame, out float val, out float inTangent, out float outTangent)
        {
            val = 0;
            inTangent = 0;
            outTangent = 0;

            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].localScale3.useY)
                {
                    val = keyframes[index].localScale3.val.y;
                    inTangent = keyframes[index].localScale3.inTangentY;
                    outTangent = keyframes[index].localScale3.outTangentY;
                    break;
                }
            }
        }

        public void GetPreviousLocalScaleZ(int frame, out float val, out float inTangent, out float outTangent)
        {
            val = 0;
            inTangent = 0;
            outTangent = 0;

            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].localScale3.useZ)
                {
                    val = keyframes[index].localScale3.val.z;
                    inTangent = keyframes[index].localScale3.inTangentZ;
                    outTangent = keyframes[index].localScale3.outTangentZ;
                    break;
                }
            }
        }

        public void GetPreviousImageScaleX(int frame, out float val, out float inTangent, out float outTangent)
        {
            val = 0;
            inTangent = 0;
            outTangent = 0;

            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].imageScale.useX)
                {
                    val = keyframes[index].imageScale.val.x;
                    inTangent = keyframes[index].imageScale.inTangentX;
                    outTangent = keyframes[index].imageScale.outTangentX;
                    break;
                }
            }
        }

        public void GetPreviousImageScaleY(int frame, out float val, out float inTangent, out float outTangent)
        {
            val = 0;
            inTangent = 0;
            outTangent = 0;

            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].imageScale.useY)
                {
                    val = keyframes[index].imageScale.val.y;
                    inTangent = keyframes[index].imageScale.inTangentY;
                    outTangent = keyframes[index].imageScale.outTangentY;
                    break;
                }
            }
        }

        public void GetPreviousColor(int frame, out Color color, out float blendWeight, 
                                        out float inTangentR, out float outTangentR,
                                        out float inTangentG, out float outTangentG,
                                        out float inTangentB, out float outTangentB,
                                        out float inTangentA, out float outTangentA,
                                        out float inTangentBlendWeight, out float outTangentBlendWeight
                                    )
        {
            color = Color.white;
            blendWeight = 0;
            inTangentR = 0;
            outTangentR = 0;
            inTangentG = 0;
            outTangentG = 0;
            inTangentB = 0;
            outTangentB = 0;
            inTangentA = 0;
            outTangentA = 0;
            inTangentBlendWeight = 0;
            outTangentBlendWeight = 0;

            for (int index = keyframes.Count - 1; index >= 0; index--)
            {
                if (keyframes[index].frame <= frame && keyframes[index].color.use)
                {
                    color = keyframes[index].color.val;
                    blendWeight = keyframes[index].color.blendWeight;
                    inTangentR = keyframes[index].color.inTangentR;
                    outTangentR = keyframes[index].color.outTangentR;
                    inTangentG = keyframes[index].color.inTangentG;
                    outTangentG = keyframes[index].color.outTangentG;
                    inTangentB = keyframes[index].color.inTangentB;
                    outTangentB = keyframes[index].color.outTangentB;
                    inTangentA = keyframes[index].color.inTangentA;
                    outTangentA = keyframes[index].color.outTangentA;
                    inTangentBlendWeight = keyframes[index].color.inTangentBlendWeight;
                    outTangentBlendWeight = keyframes[index].color.outTangentBlendWeight;
                    break;
                }
            }
        }

        public void RemoveBlankKeyframes()
        {
            int kIndex = 0;

            while (kIndex < keyframes.Count)
            {
                if (keyframes[kIndex].BeingUsed)
                {
                    kIndex++;
                }
                else
                {
                    keyframes.RemoveAt(kIndex);
                }
            }
        }

        public void ConvertKeyframesToLocalPosition3()
        {
            foreach (KeyframeSM keyframe in keyframes)
            {
                keyframe.ConvertToLocalPosition3();
            }
        }

        public void CreateColor()
        {
            foreach (KeyframeSM keyframe in keyframes)
            {
                keyframe.CreateColor();
            }
        }

        //public void InitializeColorCurves(Keyframe [] colorRKeys,
        //                                    Keyframe [] colorGKeys,
        //                                    Keyframe [] colorBKeys,
        //                                    Keyframe [] colorAKeys,
        //                                    Keyframe [] colorBlendWeightKeys)
        //{
        //    _colorRCurve = new AnimationCurve(colorRKeys);
        //    _colorGCurve = new AnimationCurve(colorGKeys);
        //    _colorBCurve = new AnimationCurve(colorBKeys);
        //    _colorACurve = new AnimationCurve(colorAKeys);
        //    _colorBlendWeightCurve = new AnimationCurve(colorBlendWeightKeys);

        //    _animatesColor = true;
        //    if (colorBlendWeightKeys.Length == 1)
        //    {
        //        if (colorBlendWeightKeys[0].value == 0)
        //            _animatesColor = false;
        //    }
        //}

        //public void InitializeColorCurves()
        //{
        //    _colorRCurve = colorRCurveSerialized.ToAnimationCurve();
        //    _colorGCurve = colorGCurveSerialized.ToAnimationCurve();
        //    _colorBCurve = colorBCurveSerialized.ToAnimationCurve();
        //    _colorACurve = colorACurveSerialized.ToAnimationCurve();
        //    _colorBlendWeightCurve = colorBlendWeightCurveSerialized.ToAnimationCurve();

        //    _animatesColor = true;
        //    if (_colorBlendWeightCurve.keys.Length == 1)
        //    {
        //        if (_colorBlendWeightCurve.keys[0].value == 0)
        //            _animatesColor = false;
        //    }
        //}
        public bool EvaluateAnimationColor(float time, out Color color, out float blendWeight)
        {
            color = Color.black;
            blendWeight = 0;

            if (_animatesColor)
            {
                color.r = Mathf.Clamp01(_colorRCurve.Evaluate(time));
                color.g = Mathf.Clamp01(_colorGCurve.Evaluate(time));
                color.b = Mathf.Clamp01(_colorBCurve.Evaluate(time));
                color.a = Mathf.Clamp01(_colorACurve.Evaluate(time));

                blendWeight = Mathf.Clamp01(_colorBlendWeightCurve.Evaluate(time));

                return true;
            }

            return false;
        }

        public void AddLocalScale3()
        {
            foreach (KeyframeSM keyframe in keyframes)
            {
                keyframe.AddLocalScale3();
            }
        }

        public void SetDefaultColliderTag()
        {
            foreach (KeyframeSM keyframe in keyframes)
            {
                keyframe.SetDefaultColliderTag();
            }
        }

        public void InitializeVisibility()
        {
            visible = true;
        }

        public void InsertFrames(int atFrame, int frameCount)
        {
            foreach (KeyframeSM keyframe in keyframes)
            {
                if (keyframe.frame >= atFrame)
                {
                    keyframe.frame += frameCount;
                }
            }
        }

        public void DeleteFrames(int atFrame, int frameCount)
        {
            foreach (KeyframeSM keyframe in keyframes)
            {
                if (keyframe.frame >= (atFrame + frameCount))
                {
                    keyframe.frame -= frameCount;
                }
            }
        }

        public void TrimNegativeKeyframes()
        {
            int keyFrameIndex = 0;
            while (keyFrameIndex < keyframes.Count)
            {
                if (keyframes[keyFrameIndex].frame < 0)
                    keyframes.RemoveAt(keyFrameIndex);
                else
                    keyFrameIndex++;
            }
        }
    }
}
