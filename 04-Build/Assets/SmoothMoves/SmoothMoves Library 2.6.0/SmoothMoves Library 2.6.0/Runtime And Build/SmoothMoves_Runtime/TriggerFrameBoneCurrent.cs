using UnityEngine;

namespace SmoothMoves
{
    [System.Serializable]
    public class TriggerFrameBoneCurrent
    {
        public int boneNodeIndex;
        public string animationName;
        public int animationLayer;

        public TriggerFrameBoneCurrent(int bnIndex)
        {
            boneNodeIndex = bnIndex;
            animationName = "";
            animationLayer = -1;
        }
    }
}