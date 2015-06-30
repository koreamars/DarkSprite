using UnityEngine;
using System.Collections.Generic;

namespace SmoothMoves
{
    [System.Serializable]
    public class AnimationBone
    {
        public int boneNodeIndex;
        public int boneDataIndex;
        public string boneName;

        public BoneQuad boneQuad;

        public GameObject gameObject;
        public Transform boneTransform;
        public Transform spriteTransform;

        public BoneAnimation boneAnimation;
        public AnimationBoneCollider boneCollider;

        public BoneColor boneColor;

        public bool active;
        public bool visible;
        public int materialIndex;
        public int depth;
		
		
        public AnimationBone(BoneAnimation ba, int bnIndex, int bdIndex, string name, Transform parentTransform, int clipCount, bool a, BoneColor bColor)
        {
            GameObject go;

            boneAnimation = ba;

            boneNodeIndex = bnIndex;
            boneDataIndex = bdIndex;
            boneName = name;

            gameObject = new GameObject(boneName);
            gameObject.layer = boneAnimation.gameObject.layer;
            boneTransform = gameObject.transform;
            boneTransform.parent = parentTransform;
            boneTransform.localPosition = Vector3.zero;
            boneTransform.localRotation = Quaternion.identity;
            boneTransform.localScale = Vector3.one;

            go = new GameObject(boneName + "_Sprite");
            go.layer = boneAnimation.gameObject.layer;
            spriteTransform = go.transform;
            spriteTransform.parent = boneTransform;
            spriteTransform.localPosition = Vector3.zero;
            spriteTransform.localRotation = Quaternion.identity;
            spriteTransform.localScale = Vector3.one;

            boneQuad = new BoneQuad(boneNodeIndex);

            active = a;
            visible = false;
            materialIndex = -1;
            depth = 0;
            boneColor = bColor;
        }

        public void Awake(BoneAnimation boneAnimation)
        {
            if (boneCollider != null)
                boneCollider.Initialize(boneAnimation, this);
        }

        public void SetLayer(int layer)
        {
            gameObject.layer = layer;
        }

        private void CreateBoneCollider()
        {
            if (boneCollider == null)
            {
                boneCollider = gameObject.AddComponent<AnimationBoneCollider>();
                boneCollider.Initialize(boneAnimation, this);
            }
        }

        public void CreateBoxCollider()
        {
            CreateBoneCollider();
            boneCollider.CreateBoxCollider();
        }

        public void CreateSphereCollider()
        {
            CreateBoneCollider();
            boneCollider.CreateSphereCollider();
        }

        public void TurnOnBoxCollider(Vector3 center, Vector3 boxSize, string tag)
        {
            if (boneCollider != null)
            {
                boneCollider.TurnOnBoxCollider(center, boxSize, tag);
            }
        }

        public void TurnOffBoxCollider()
        {
            if (boneCollider != null)
            {
                boneCollider.TurnOffBoxCollider();
            }
        }

        public void TurnOnSphereCollider(Vector3 center, float radius, string tag)
        {
            if (boneCollider != null)
            {
                boneCollider.TurnOnSphereCollider(center, radius, tag);
            }
        }

        public void TurnOffSphereCollider()
        {
            if (boneCollider != null)
            {
                boneCollider.TurnOffSphereCollider();
            }
        }

        public void SetTrigger(bool triggerOn)
        {
            if (boneCollider != null)
            {
                boneCollider.SetTrigger(triggerOn);
            }
        }
    }
}