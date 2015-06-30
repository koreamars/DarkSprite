namespace SmoothMoves
{
    [System.Serializable]
    public class BoneQuad
    {
        public int boneNodeIndex;
        public int[] vertexIndices;

        public BoneQuad(int bnIndex)
        {
            boneNodeIndex = bnIndex;

            vertexIndices = new int[6];

            vertexIndices[0] = (boneNodeIndex * 4) + 0;
            vertexIndices[1] = (boneNodeIndex * 4) + 3;
            vertexIndices[2] = (boneNodeIndex * 4) + 1;

            vertexIndices[3] = (boneNodeIndex * 4) + 3;
            vertexIndices[4] = (boneNodeIndex * 4) + 2;
            vertexIndices[5] = (boneNodeIndex * 4) + 1;
        }
    }
}
