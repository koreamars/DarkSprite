using UnityEngine;

namespace SmoothMoves
{
    [System.Serializable]
    public class AnimationClipBone_Lite
    {
        private AnimationCurve _colorRCurve;
        private AnimationCurve _colorGCurve;
        private AnimationCurve _colorBCurve;
        private AnimationCurve _colorACurve;
        private AnimationCurve _colorBlendWeightCurve;

        public int boneDataIndex;
        public bool mixTransform;
        public bool animatesColor;

        public AnimationCurveSerializable colorRCurveSerialized;
        public AnimationCurveSerializable colorGCurveSerialized;
        public AnimationCurveSerializable colorBCurveSerialized;
        public AnimationCurveSerializable colorACurveSerialized;
        public AnimationCurveSerializable colorBlendWeightCurveSerialized;

        public AnimationClipBone_Lite(AnimationClipBone copyAnimationClipBone)
        {
            boneDataIndex = copyAnimationClipBone.boneDataIndex;
            mixTransform = copyAnimationClipBone.mixTransform;
            animatesColor = false;

            colorRCurveSerialized = new AnimationCurveSerializable(copyAnimationClipBone.colorRCurveSerialized);
            colorGCurveSerialized = new AnimationCurveSerializable(copyAnimationClipBone.colorGCurveSerialized);
            colorBCurveSerialized = new AnimationCurveSerializable(copyAnimationClipBone.colorBCurveSerialized);
            colorACurveSerialized = new AnimationCurveSerializable(copyAnimationClipBone.colorACurveSerialized);
            colorBlendWeightCurveSerialized = new AnimationCurveSerializable(copyAnimationClipBone.colorBlendWeightCurveSerialized);
        }

        public void InitializeColorCurves()
        {
            _colorRCurve = colorRCurveSerialized.ToAnimationCurve();
            _colorGCurve = colorGCurveSerialized.ToAnimationCurve();
            _colorBCurve = colorBCurveSerialized.ToAnimationCurve();
            _colorACurve = colorACurveSerialized.ToAnimationCurve();
            _colorBlendWeightCurve = colorBlendWeightCurveSerialized.ToAnimationCurve();

            animatesColor = true;
            if (_colorBlendWeightCurve.keys.Length == 1)
            {
                if (_colorBlendWeightCurve.keys[0].value == 0)
                    animatesColor = false;
            }
        }

        public bool EvaluateAnimationColor(float time, out Color color, out float blendWeight)
        {
            color = Color.black;
            blendWeight = 0;

            if (animatesColor)
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
    }
}
