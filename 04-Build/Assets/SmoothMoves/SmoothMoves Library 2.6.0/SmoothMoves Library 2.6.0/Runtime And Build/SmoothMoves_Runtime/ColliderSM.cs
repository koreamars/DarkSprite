using UnityEngine;

namespace SmoothMoves
{
    [System.Serializable]
    public class ColliderSM
    {
        public enum COLLIDER_TYPE
        {
            None = 0,
            Sphere = 1,
            Box = 2
        }

        public COLLIDER_TYPE type;
        public Vector3 center;
        public Vector3 boxSize;
        public float sphereRadius;
        public bool useAnimationLayer;
        public int layer;
        public bool isTrigger;
        public string tag;

        public ColliderSM(ColliderSM copyCollider)
        {
            Reset();

            if (copyCollider != null)
                CopyCollider(copyCollider);
        }

        public ColliderSM()
        {
            Reset();
        }

        public void Reset()
        {
            type = COLLIDER_TYPE.None;
            center = Vector3.zero;
            boxSize = Vector3.zero;
            sphereRadius = 0;
            useAnimationLayer = true;
            layer = 0;
            isTrigger = true;
            tag = "";
        }

        public void CopyCollider(ColliderSM copyCollider)
        {
            type = copyCollider.type;
            center = copyCollider.center;
            boxSize = copyCollider.boxSize;
            sphereRadius = copyCollider.sphereRadius;
            useAnimationLayer = copyCollider.useAnimationLayer;
            layer = copyCollider.layer;
            isTrigger = copyCollider.isTrigger;
            tag = copyCollider.tag;
        }


        //public bool EqualsCollider(ColliderSM otherCollider)
        //{
        //    return (
        //            type == otherCollider.type
        //            &&
        //            center == otherCollider.center
        //            &&
        //            useAnimationLayer == otherCollider.useAnimationLayer
        //            &&
        //            layer == otherCollider.layer
        //            &&
        //            isTrigger == otherCollider.isTrigger
        //            &&
        //            (
        //             (type == ColliderSM.COLLIDER_TYPE.Box && boxSize == otherCollider.boxSize)
        //                ||
        //             (type == ColliderSM.COLLIDER_TYPE.Sphere && sphereRadius == otherCollider.sphereRadius)
        //            )
        //            );
        //}
    }
}