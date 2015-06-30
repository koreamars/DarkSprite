using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmoothMoves
{
    [Serializable]
    public class KeyframeSerializable
    {
        public float time;
        public float value;
        public float inTangent;
        public float outTangent;

        public KeyframeSerializable()
        {
            time = 0;
            value = 0;
            inTangent = 0;
            outTangent = 0;
        }
    }
    
    [Serializable]
    public class AnimationCurveSerializable
    {
        public List<KeyframeSerializable> keyframes;

        public AnimationCurveSerializable()
        {
            keyframes = new List<KeyframeSerializable>();
        }

        public AnimationCurveSerializable(AnimationCurveSerializable copyAnimationCurveSerializable)
        {
            keyframes = new List<KeyframeSerializable>();
            foreach (KeyframeSerializable copyKeyframe in copyAnimationCurveSerializable.keyframes)
            {
                AddKeyframe(copyKeyframe.time, copyKeyframe.value, copyKeyframe.inTangent, copyKeyframe.outTangent);
            }
        }

        public void AddKeyframe(float time, float value, float inTangent, float outTangent)
        {
            KeyframeSerializable key = new KeyframeSerializable();
            key.time = time;
            key.value = value;
            key.inTangent = inTangent;
            key.outTangent = outTangent;

            keyframes.Add(key);
        }

        public AnimationCurve ToAnimationCurve()
        {
            AnimationCurve curve = new AnimationCurve();
            Keyframe keyframe;

            foreach (KeyframeSerializable key in keyframes)
            {
                keyframe = new Keyframe(key.time, key.value, key.inTangent, key.outTangent);
                curve.AddKey(keyframe);
            }

            return curve;
        }

        public void Clear()
        {
            if (keyframes == null)
                keyframes = new List<KeyframeSerializable>();
            else
                keyframes.Clear();
        }
    }
}
