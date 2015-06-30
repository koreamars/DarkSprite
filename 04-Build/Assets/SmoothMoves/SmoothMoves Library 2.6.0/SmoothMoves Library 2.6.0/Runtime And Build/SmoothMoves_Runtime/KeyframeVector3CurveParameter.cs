using UnityEngine;

namespace SmoothMoves
{
    [System.Serializable]
    public class KeyframeVector3CurveParameter
    {
        public Vector3 val;
        public float inTangentX;
        public float outTangentX;
        public int tangentModeX;
        public float inTangentY;
        public float outTangentY;
        public int tangentModeY;
        public float inTangentZ;
        public float outTangentZ;
        public int tangentModeZ;
        public bool useX;
        public bool useY;
        public bool useZ;

        public KeyframeVector3CurveParameter(Vector3 v)
        {
            val = v;
            inTangentX = 0;
            outTangentX = 0;
            tangentModeX = (int)KeyframeSM.TANGENT_MODE.Smooth;
            inTangentY = 0;
            outTangentY = 0;
            tangentModeY = (int)KeyframeSM.TANGENT_MODE.Smooth;
            inTangentZ = 0;
            outTangentZ = 0;
            tangentModeZ = (int)KeyframeSM.TANGENT_MODE.Smooth;
            useX = true;
            useY = true;
            useZ = true;
        }

        public void Copy(KeyframeVector3CurveParameter p, int frame)
        {
            val = p.val;
            inTangentX = p.inTangentX;
            outTangentX = p.outTangentX;
            tangentModeX = p.tangentModeX;
            inTangentY = p.inTangentY;
            outTangentY = p.outTangentY;
            tangentModeY = p.tangentModeY;
            inTangentZ = p.inTangentZ;
            outTangentZ = p.outTangentZ;
            tangentModeZ = p.tangentModeZ;
            useX = p.useX || frame == 0;
            useY = p.useY || frame == 0;
            useZ = p.useZ || frame == 0;
        }
    }
}
