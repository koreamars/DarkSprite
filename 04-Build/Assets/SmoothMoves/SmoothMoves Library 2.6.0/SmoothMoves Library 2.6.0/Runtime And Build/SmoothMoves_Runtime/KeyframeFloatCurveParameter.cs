using UnityEngine;

namespace SmoothMoves
{
    [System.Serializable]
    public class KeyframeFloatCurveParameter
    {
        public float val;
        public float inTangent;
        public float outTangent;
        public int tangentMode;
        public bool use;

        public KeyframeFloatCurveParameter()
        {
            val = 0;
            inTangent = 0;
            outTangent = 0;
            tangentMode = (int)KeyframeSM.TANGENT_MODE.Smooth;
            use = true;
        }

        public void Copy(KeyframeFloatCurveParameter p, int frame)
        {
            val = p.val;
            inTangent = p.inTangent;
            outTangent = p.outTangent;
            tangentMode = p.tangentMode;
            use = p.use || frame == 0;
        }
    }
}
