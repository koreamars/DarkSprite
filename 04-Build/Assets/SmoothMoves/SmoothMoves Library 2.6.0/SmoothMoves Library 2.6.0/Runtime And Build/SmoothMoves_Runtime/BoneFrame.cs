using UnityEngine;

namespace SmoothMoves
{
    public class BoneFrame
    {
        public int boneDataIndex;
        public int boneNodeIndex;
        public int frame;
        public int originalFrame;

        public BoneFrame(int bdIndex, int bnIndex, int fr)
        {
            Set(bdIndex, bnIndex, fr);
            originalFrame = frame;
        }

        public void Set(int bdIndex, int bnIndex, int fr)
        {
            boneDataIndex = bdIndex;
            boneNodeIndex = bnIndex;
            frame = fr;
        }

        public void Copy(BoneFrame otherBoneFrame)
        {
            boneDataIndex = otherBoneFrame.boneDataIndex;
            boneNodeIndex = otherBoneFrame.boneNodeIndex;
            frame = otherBoneFrame.frame;
            originalFrame = otherBoneFrame.originalFrame;
        }
    }
}
