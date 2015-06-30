using UnityEngine;
using System.Collections.Generic;

namespace SmoothMoves
{
    [System.Serializable]
    public class AnimationClipSM_Lite
    {
        public string animationName;
        public float fps;
        public WrapMode wrapMode;
        public bool mix;
        public AnimationBlendMode blendMode;
        public int layer;
        public float blendWeight;
        public List<AnimationClipBone_Lite> bones = new List<AnimationClipBone_Lite>();

        public AnimationClipSM_Lite(AnimationClipSM copyAnimationClipSM)
        {
            animationName = copyAnimationClipSM.animationName;
            fps = copyAnimationClipSM.fps;
            wrapMode = copyAnimationClipSM.wrapMode;
            mix = copyAnimationClipSM.mix;
            blendMode = copyAnimationClipSM.blendMode;
            layer = copyAnimationClipSM.layer;
            blendWeight = copyAnimationClipSM.blendWeight;

            bones = new List<AnimationClipBone_Lite>();
            foreach (AnimationClipBone animationClipBone in copyAnimationClipSM.bones)
            {
                bones.Add(new AnimationClipBone_Lite(animationClipBone));
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
    }
}
