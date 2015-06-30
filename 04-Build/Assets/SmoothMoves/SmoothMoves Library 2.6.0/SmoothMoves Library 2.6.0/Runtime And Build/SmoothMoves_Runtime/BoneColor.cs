using UnityEngine;

namespace SmoothMoves
{
    [System.Serializable]
	public class BoneColor
	{
        public float blendingWeight;
        public Color color;

        public BoneColor(float blend, Color c)
        {
            blendingWeight = blend;
            color = c;
        }

        public BoneColor(BoneColor otherBoneColor)
        {
            blendingWeight = otherBoneColor.blendingWeight;
            color = otherBoneColor.color;
        }
	}
}
