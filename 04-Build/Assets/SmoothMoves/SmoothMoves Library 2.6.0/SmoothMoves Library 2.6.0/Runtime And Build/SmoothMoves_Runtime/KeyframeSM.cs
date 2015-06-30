using UnityEngine;

namespace SmoothMoves
{
    [System.Serializable]
    public class KeyframeSM
    {
        public const int MAX_DEPTH = 128;

        public enum KEYFRAME_TYPE
        {
            TransformOnly = 0,
            Image = 1
        }

        public enum CURVE_PROPERTY
        {
            None,
            LocalPositionX,
            LocalPositionY,
            LocalPositionZ,
            LocalRotation,
            LocalScaleX,
            LocalScaleY,
            LocalScaleZ,
            ImageScaleX,
            ImageScaleY,
            ColorR,
            ColorG,
            ColorB,
            ColorA,
            ColorBlend
        }

        public enum LEFT_RIGHT_TANGENT_MODE
        {
            None,
            Free,
            Linear,
            Constant
        }

        public enum TANGENT_MODE
        {
            Smooth = 0,
            LeftFreeRightFree = 1,
            LeftLinearRightFree = 5,
            LeftConstantRightFree = 7,
            Auto = 10,
            LeftFreeRightLinear = 17,
            LeftLinearRightLinear = 21,
            LeftConstantRightLinear = 23,
            LeftFreeRightConstant = 25,
            LeftLinearRightConstant = 29,
            LeftConstantRightConstant = 31
        }

        public int boneDataIndex;

        public int frame;

        public bool userTriggerCallback;
        public string userTriggerTag;

        public bool useKeyframeType;
        public KEYFRAME_TYPE keyframeType;

        public KeyframeVector2CurveParameter localPosition;
        public KeyframeVector3CurveParameter localPosition3;
        public KeyframeFloatCurveParameter localRotation;
        public KeyframeVector3CurveParameter localScale3;
        public KeyframeVector2CurveParameter imageScale;
        public KeyframeColorCurveParameter color;

        public bool useAtlas;
        public TextureAtlas atlas;

        public bool useTextureGUID;
        public string textureGUID;

        public bool usePivotOffset;
        public Vector2 pivotOffset;
        public bool useDefaultPivot;

        public bool useDepth;
        public int depth;

        public bool useCollider;
        public ColliderSM collider;

        public int textureIndex;

        public bool BeingUsed
        {
            get
            {
                return (userTriggerCallback ||
                        useKeyframeType ||
                        useAtlas ||
                        useTextureGUID ||
                        usePivotOffset ||
                        useDepth ||
                        useCollider ||
                        localPosition3.useX ||
                        localPosition3.useY ||
                        localPosition3.useZ ||
                        localRotation.use ||
                        localScale3.useX ||
                        localScale3.useY ||
                        localScale3.useZ ||
                        imageScale.useX ||
                        imageScale.useY ||
                        color.use);
            }
        }

        public int Depth
        {
            get
            {
                return depth;
            }
            set
            {
                depth = Mathf.Clamp(value, 0, MAX_DEPTH);
            }
        }

        public KeyframeSM(int bdIndex, AnimationClipBone.DEFAULT_SETTING defaultSetting)
        {
            frame = 0;

            userTriggerCallback = false;
            userTriggerTag = "";

            boneDataIndex = bdIndex;
            keyframeType = KEYFRAME_TYPE.TransformOnly;

            collider = new ColliderSM();

            Reset(defaultSetting);
        }

        public void Reset(AnimationClipBone.DEFAULT_SETTING defaultSetting)
        {
            useKeyframeType = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All);

            localPosition3 = new KeyframeVector3CurveParameter(Vector3.zero);
            localPosition3.useX = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All || defaultSetting == AnimationClipBone.DEFAULT_SETTING.PositionRotation);
            localPosition3.useY = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All || defaultSetting == AnimationClipBone.DEFAULT_SETTING.PositionRotation);
            localPosition3.useZ = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All);

            localRotation = new KeyframeFloatCurveParameter();
            localRotation.use = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All || defaultSetting == AnimationClipBone.DEFAULT_SETTING.PositionRotation);

            localScale3 = new KeyframeVector3CurveParameter(Vector3.one);
            localScale3.useX = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All);
            localScale3.useY = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All);
            localScale3.useZ = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All);

            imageScale = new KeyframeVector2CurveParameter(Vector2.one);
            imageScale.useX = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All);
            imageScale.useY = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All);

            color = new KeyframeColorCurveParameter(Color.white);
            color.use = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All);

            useAtlas = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All);
            atlas = null;

            useTextureGUID = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All || defaultSetting == AnimationClipBone.DEFAULT_SETTING.Texture);
            textureGUID = "";

            usePivotOffset = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All);
            pivotOffset = Vector2.zero;
            useDefaultPivot = true;

            useDepth = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All);
            depth = 0;

            useCollider = (defaultSetting == AnimationClipBone.DEFAULT_SETTING.All);
            collider.Reset();

            textureIndex = -1;
        }

        public void Clear()
        {

        }

        public void CopyKeyframe(KeyframeSM copyKeyframe)
        {
            //if (copyKeyframe != null)
            //{
                boneDataIndex = copyKeyframe.boneDataIndex;

                frame = copyKeyframe.frame;

                userTriggerCallback = copyKeyframe.userTriggerCallback;
                userTriggerTag = copyKeyframe.userTriggerTag;

                useKeyframeType = copyKeyframe.useKeyframeType || (frame == 0);
                keyframeType = copyKeyframe.keyframeType;

                localPosition3.Copy(copyKeyframe.localPosition3, frame);
                localRotation.Copy(copyKeyframe.localRotation, frame);
                localScale3.Copy(copyKeyframe.localScale3, frame);
                imageScale.Copy(copyKeyframe.imageScale, frame);
                color.Copy(copyKeyframe.color, frame);

                useAtlas = copyKeyframe.useAtlas || (frame == 0);
                atlas = copyKeyframe.atlas;

                useTextureGUID = copyKeyframe.useTextureGUID || (frame == 0);
                textureGUID = copyKeyframe.textureGUID;

                usePivotOffset = copyKeyframe.usePivotOffset || (frame == 0);
                pivotOffset = copyKeyframe.pivotOffset;
                useDefaultPivot = copyKeyframe.useDefaultPivot;

                useDepth = copyKeyframe.useDepth || (frame == 0);
                depth = copyKeyframe.depth;

                useCollider = copyKeyframe.useCollider || (frame == 0);
                collider.CopyCollider(copyKeyframe.collider);

                textureIndex = copyKeyframe.textureIndex;
            //}
        }

        //public void CopyKeyframePositionRotationScale(KeyframeSM copyKeyframe)
        //{
        //    localPosition3.Copy(copyKeyframe.localPosition3, frame);
        //    localRotation.Copy(copyKeyframe.localRotation, frame);
        //    localScale3.Copy(copyKeyframe.localScale3, frame);
        //    imageScale.Copy(copyKeyframe.imageScale, frame);
        //}

        public void ConvertToLocalPosition3()
        {
            localPosition3.useX = localPosition.useX || frame == 0;
            localPosition3.val.x = localPosition.val.x;
            localPosition3.inTangentX = localPosition.inTangentX;
            localPosition3.outTangentX = localPosition.outTangentX;
            localPosition3.tangentModeX = localPosition.tangentModeX;

            localPosition3.useY = localPosition.useY || frame == 0;
            localPosition3.val.y = localPosition.val.y;
            localPosition3.inTangentY = localPosition.inTangentY;
            localPosition3.outTangentY = localPosition.outTangentY;
            localPosition3.tangentModeY = localPosition.tangentModeY;

            localPosition3.useZ = (frame == 0);
            localPosition3.val.z = 0;
            localPosition3.inTangentZ = 0;
            localPosition3.outTangentZ = 0;
            localPosition3.tangentModeZ = (int)TANGENT_MODE.Smooth;
        }

        public void CreateColor()
        {
            color.use = (frame == 0);
            color.val = Color.white;
            color.inTangentR = 0;
            color.outTangentR = 0;
            color.tangentModeR = (int)TANGENT_MODE.Smooth;
            color.inTangentG = 0;
            color.outTangentG = 0;
            color.tangentModeG = (int)TANGENT_MODE.Smooth;
            color.inTangentB = 0;
            color.outTangentB = 0;
            color.tangentModeB = (int)TANGENT_MODE.Smooth;
            color.inTangentA = 0;
            color.outTangentA = 0;
            color.tangentModeA = (int)TANGENT_MODE.Smooth;

            color.blendWeight = 0;
            color.inTangentBlendWeight = 0;
            color.outTangentBlendWeight = 0;
            color.tangentModeBlendWeight = (int)TANGENT_MODE.Smooth;
        }

        public void AddLocalScale3()
        {
            localScale3.useX = (frame == 0);
            localScale3.useY = (frame == 0);
            localScale3.useZ = (frame == 0);

            localScale3.val = Vector3.one;

            localScale3.inTangentX = 0;
            localScale3.outTangentX = 0;
            localScale3.tangentModeX = (int)TANGENT_MODE.Smooth;

            localScale3.inTangentY = 0;
            localScale3.outTangentY = 0;
            localScale3.tangentModeY = (int)TANGENT_MODE.Smooth;

            localScale3.inTangentZ = 0;
            localScale3.outTangentZ = 0;
            localScale3.tangentModeZ = (int)TANGENT_MODE.Smooth;
        }

        public void SetDefaultColliderTag()
        {
            collider.tag = "";
        }
    }
}
