using UnityEngine;

namespace SmoothMoves
{
    [System.Serializable]
    public class DFSBoneNode
    {
        public int parentBoneIndex;
        public int boneDataIndex;
        public int depth;

        public DFSBoneNode()
        {
            Set(-1, -1, 0);
        }

        public DFSBoneNode(int pbIndex, int bdIndex, int d)
        {
            Set(pbIndex, bdIndex, d);
        }

        public DFSBoneNode(DFSBoneNode copyBoneNode)
        {
            Copy(copyBoneNode);
        }

        public void Copy(DFSBoneNode copyBoneNode)
        {
            Set(copyBoneNode.parentBoneIndex, copyBoneNode.boneDataIndex, copyBoneNode.depth);
        }

        public void Set(int pbIndex, int bdIndex, int d)
        {
            parentBoneIndex = pbIndex;
            boneDataIndex = bdIndex;
            depth = d;
        }

        public void Set(int pbIndex, int d)
        {
            parentBoneIndex = pbIndex;
            depth = d;
        }

        public void Clear()
        {
        }
    }
}