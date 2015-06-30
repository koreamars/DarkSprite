using UnityEngine;

namespace SmoothMoves
{
    [System.Serializable]
    public class KeyframeVector2CurveParameter
    {
        public Vector2 val;
        public float inTangentX;
        public float outTangentX;
        public int tangentModeX;
        public float inTangentY;
        public float outTangentY;
        public int tangentModeY;
        public bool useX;
        public bool useY;

        public KeyframeVector2CurveParameter(Vector2 v)
        {
            val = v;
            inTangentX = 0;
            outTangentX = 0;
            tangentModeX = (int)KeyframeSM.TANGENT_MODE.Smooth; ;
            inTangentY = 0;
            outTangentY = 0;
            tangentModeY = (int)KeyframeSM.TANGENT_MODE.Smooth; ;
            useX = true;
            useY = true;
        }

        public void Copy(KeyframeVector2CurveParameter p, int frame)
        {
            val = p.val;
            inTangentX = p.inTangentX;
            outTangentX = p.outTangentX;
            tangentModeX = p.tangentModeX;
            inTangentY = p.inTangentY;
            outTangentY = p.outTangentY;
            tangentModeY = p.tangentModeY;
            useX = p.useX || frame == 0;
            useY = p.useY || frame == 0;
        }
    }
}
