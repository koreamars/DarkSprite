using UnityEngine;

namespace SmoothMoves
{
    [System.Serializable]
	public class KeyframeColorCurveParameter
	{
        public Color val;
        public float inTangentR;
        public float outTangentR;
        public int tangentModeR;
        public float inTangentG;
        public float outTangentG;
        public int tangentModeG;
        public float inTangentB;
        public float outTangentB;
        public int tangentModeB;
        public float inTangentA;
        public float outTangentA;
        public int tangentModeA;
        public bool use;
        public float blendWeight;
        public float inTangentBlendWeight;
        public float outTangentBlendWeight;
        public int tangentModeBlendWeight;

        public KeyframeColorCurveParameter(Color c)
        {
            use = true;
            val = c;
            inTangentR = 0;
            outTangentR = 0;
            tangentModeR = (int)KeyframeSM.TANGENT_MODE.Smooth;
            inTangentG = 0;
            outTangentG = 0;
            tangentModeG = (int)KeyframeSM.TANGENT_MODE.Smooth;
            inTangentB = 0;
            outTangentB = 0;
            tangentModeB = (int)KeyframeSM.TANGENT_MODE.Smooth;
            inTangentA = 0;
            outTangentA = 0;
            tangentModeA = (int)KeyframeSM.TANGENT_MODE.Smooth;

            blendWeight = 0;
            inTangentBlendWeight = 0;
            outTangentBlendWeight = 0;
            tangentModeBlendWeight = (int)KeyframeSM.TANGENT_MODE.Smooth;
        }

        public void Copy(KeyframeColorCurveParameter p, int frame)
        {
            use = p.use || frame == 0;
            val = p.val;
            inTangentR = p.inTangentR;
            outTangentR = p.outTangentR;
            tangentModeR = p.tangentModeR;
            inTangentG = p.inTangentG;
            outTangentG = p.outTangentG;
            tangentModeG = p.tangentModeG;
            inTangentB = p.inTangentB;
            outTangentB = p.outTangentB;
            tangentModeB = p.tangentModeB;
            inTangentA = p.inTangentA;
            outTangentA = p.outTangentA;
            tangentModeA = p.tangentModeA;

            blendWeight = p.blendWeight;
            inTangentBlendWeight = p.inTangentBlendWeight;
            outTangentBlendWeight = p.outTangentBlendWeight;
            tangentModeBlendWeight = p.tangentModeBlendWeight;
        }
	}
}
