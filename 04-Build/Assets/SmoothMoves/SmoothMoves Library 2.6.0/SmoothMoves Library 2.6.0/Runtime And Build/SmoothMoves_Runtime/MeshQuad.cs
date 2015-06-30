using System.Collections.Generic;

namespace SmoothMoves
{
    [System.Serializable]
    public class MeshQuad
    {
        public BoneQuad boneQuad;
        public int materialIndex;
        public int depth;

        public MeshQuad(BoneQuad q, int mi, int d)
        {
            boneQuad = q;
            materialIndex = mi;
            depth = d;
        }
    }

    public class SortMeshQuadDepthDescending : IComparer<MeshQuad>
    {
        int IComparer<MeshQuad>.Compare(MeshQuad a, MeshQuad b)
        {
            if (a.depth < b.depth)
                return 1;
            if (a.depth > b.depth)
                return -1;
            else
                return 0;
        }
    }
}
