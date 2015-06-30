using UnityEngine;
using System.Collections.Generic;

namespace SmoothMoves
{
    [System.Serializable]
    public class BoneData
    {
        public bool active;
        public string boneName;
        public BoneColor boneColor;

        public string ShortenedBoneName
        {
            get
            {
                if (boneName.Length > 17)
                {
                    return " " + boneName.Substring(0, 14) + "...";
                }
                else
                {
                    return " " + boneName;
                }
            }
        }

        public BoneData()
        {
            Initialize("");
        }

        public BoneData(string boneName)
        {
            Initialize(boneName);
        }

        private void Initialize(string bn)
        {
            active = true;
            boneName = bn;
            boneColor = new BoneColor(0.0f, Color.white);
        }

        public void Clear()
        {
        }
    }
}
