using System.Collections.Generic;

namespace SmoothMoves
{
    [System.Serializable]
    public class SubMesh
    {
        public int materialIndex;
        public List<BoneQuad> boneQuads;

        public SubMesh(int mi)
        {
            boneQuads = new List<BoneQuad>();
            materialIndex = mi;
        }

        public void Reset(int mi)
        {
            materialIndex = mi;
            boneQuads.Clear();
        }

        public void AddBoneQuad(BoneQuad bq)
        {
            boneQuads.Add(bq);
        }
    }
}