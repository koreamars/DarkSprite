using UnityEngine;

namespace SmoothMoves
{
    /// <summary>
    /// This structure holds information stored when a collider trigger event occurs
    /// on a collider located on a SmoothMoves BoneAnimation bone
    /// </summary>
    public class ColliderTriggerEvent
    {
        /// <summary>
        /// The type of event that was triggered
        /// </summary>
        public enum TRIGGER_TYPE
        {
            /// <summary>
            /// Nothing happened
            /// </summary>
            None,

            /// <summary>
            /// A trigger was entered
            /// </summary>
            Enter,

            /// <summary>
            /// A trigger was stayed within
            /// </summary>
            Stay,

            /// <summary>
            /// A trigger was exited
            /// </summary>
            Exit
        }

        /// <summary>
        /// The type of trigger event
        /// </summary>
        public TRIGGER_TYPE triggerType;

        /// <summary>
        /// The bone animation associated with the trigger
        /// </summary>
        public BoneAnimation boneAnimation;

        /// <summary>
        /// The bone node index of the bone where the collider resides
        /// </summary>
        public int boneNodeIndex;

        /// <summary>
        /// The name of the bone where the collider resides
        /// </summary>
        public string boneName;

        /// <summary>
        /// The reference to the other collider that caused this trigger
        /// </summary>
        public Collider otherCollider;

        /// <summary>
        /// A point in space on the other collider that is closest 
        /// to the bone's pivot point for this collider
        /// </summary>
        public Vector3 otherColliderClosestPointToBone;

        /// <summary>
        /// A string tag
        /// </summary>
        public string tag;

        public ColliderTriggerEvent()
        {
            triggerType = TRIGGER_TYPE.None;
            boneAnimation = null;
            boneNodeIndex = -1;
            boneName = "";
            otherCollider = null;
            otherColliderClosestPointToBone = Vector3.zero;
            tag = "";
        }

        public ColliderTriggerEvent(ColliderTriggerEvent copyEvent)
        {
            CopyEvent(copyEvent);
        }

        public void CopyEvent(ColliderTriggerEvent copyEvent)
        {
            triggerType = copyEvent.triggerType;
            boneAnimation = copyEvent.boneAnimation;
            boneNodeIndex = copyEvent.boneNodeIndex;
            boneName = copyEvent.boneName;
            otherCollider = copyEvent.otherCollider;
            otherColliderClosestPointToBone = copyEvent.otherColliderClosestPointToBone;
            tag = copyEvent.tag;
        }
    }
}