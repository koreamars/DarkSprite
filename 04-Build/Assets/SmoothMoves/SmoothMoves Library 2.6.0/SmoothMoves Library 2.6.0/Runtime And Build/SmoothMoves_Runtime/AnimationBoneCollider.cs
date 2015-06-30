using UnityEngine;

namespace SmoothMoves
{
    public class AnimationBoneCollider : MonoBehaviour
    {
        private BoneAnimation _boneAnimation;
        private AnimationBone _animationBone;

        private Rigidbody _rigidBody;
        private BoxCollider _boxCollider;
        private SphereCollider _sphereCollider;

        public string BoneColliderTag { get; private set; }

        public void Initialize(BoneAnimation boneAnimation, AnimationBone animationBone)
        {
            _boneAnimation = boneAnimation;
            _animationBone = animationBone;

            _rigidBody = gameObject.GetComponent<Rigidbody>();
            _boxCollider = gameObject.GetComponent<BoxCollider>();
            _sphereCollider = gameObject.GetComponent<SphereCollider>();
        }

        public void CreateBoxCollider()
        {
            _rigidBody = gameObject.GetComponent<Rigidbody>();
            if (_rigidBody == null)
            {
                _rigidBody = gameObject.AddComponent<Rigidbody>();
                _rigidBody.isKinematic = true;
                _rigidBody.useGravity = false;
            }

            _boxCollider = gameObject.GetComponent<BoxCollider>();
            if (_boxCollider == null)
            {
                _boxCollider = gameObject.AddComponent<BoxCollider>();
                _boxCollider.isTrigger = true;
                _boxCollider.enabled = false;
            }
        }

        public void CreateSphereCollider()
        {
            _rigidBody = gameObject.GetComponent<Rigidbody>();
            if (_rigidBody == null)
            {
                _rigidBody = gameObject.AddComponent<Rigidbody>();
                _rigidBody.isKinematic = true;
                _rigidBody.useGravity = false;
            }

            _sphereCollider = gameObject.GetComponent<SphereCollider>();
            if (_sphereCollider == null)
            {
                _sphereCollider = gameObject.AddComponent<SphereCollider>();
                _sphereCollider.isTrigger = true;
                _sphereCollider.enabled = false;
            }
        }

        public void TurnOnBoxCollider(Vector3 center, Vector3 boxSize, string tag)
        {
            BoneColliderTag = tag;

            if (_boxCollider != null)
            {
                _boxCollider.enabled = true;
                _boxCollider.center = center;
                _boxCollider.size = boxSize;
            }
        }

        public void TurnOffBoxCollider()
        {
            if (_boxCollider != null)
            {
                _boxCollider.enabled = false;
                _boxCollider.center = Vector3.zero;
                _boxCollider.size = Vector3.zero;
            }
        }

        public void TurnOnSphereCollider(Vector3 center, float radius, string tag)
        {
            BoneColliderTag = tag;

            if (_sphereCollider != null)
            {
                _sphereCollider.enabled = true;
                _sphereCollider.center = center;
                _sphereCollider.radius = radius;
            }
        }

        public void TurnOffSphereCollider()
        {
            if (_sphereCollider != null)
            {
                _sphereCollider.enabled = false;
                _sphereCollider.center = Vector3.zero;
                _sphereCollider.radius = 0;
            }
        }

        public void SetTrigger(bool triggerOn)
        {
            if (_boxCollider != null)
            {
                _boxCollider.isTrigger = triggerOn;
            }

            if (_sphereCollider != null)
            {
                _sphereCollider.isTrigger = triggerOn;
            }
        }

        void OnTriggerEnter(Collider otherCollider)
        {
            _boneAnimation.ColliderTrigger(ColliderTriggerEvent.TRIGGER_TYPE.Enter, _animationBone, otherCollider, BoneColliderTag);
        }

        void OnTriggerStay(Collider otherCollider)
        {
            _boneAnimation.ColliderTrigger(ColliderTriggerEvent.TRIGGER_TYPE.Stay, _animationBone, otherCollider, BoneColliderTag);
        }

        void OnTriggerExit(Collider otherCollider)
        {
            _boneAnimation.ColliderTrigger(ColliderTriggerEvent.TRIGGER_TYPE.Exit, _animationBone, otherCollider, BoneColliderTag);
        }

        void OnCollisionEnter(Collision collision)
        {
            _boneAnimation.Collision(CollisionEvent.COLLISION_TYPE.Enter, _animationBone, collision, BoneColliderTag);
        }

        void OnCollisionStay(Collision collision)
        {
            _boneAnimation.Collision(CollisionEvent.COLLISION_TYPE.Stay, _animationBone, collision, BoneColliderTag);
        }

        void OnCollisionExit(Collision collision)
        {
            _boneAnimation.Collision(CollisionEvent.COLLISION_TYPE.Exit, _animationBone, collision, BoneColliderTag);
        }
    }
}