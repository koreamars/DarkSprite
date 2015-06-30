using UnityEngine;

namespace SmoothMoves
{
    /// <summary>
    /// This structure holds information stored when a physics collision occurs
    /// with a collider on a SmoothMoves BoneAnimation bone
    /// </summary>
    public class CollisionEvent
    {
        /// <summary>
        /// The type of collision that occurred
        /// </summary>
        public enum COLLISION_TYPE
        {
            /// <summary>
            /// No collision
            /// </summary>
            None,

            /// <summary>
            /// The collider was entered
            /// </summary>
            Enter,

            /// <summary>
            /// The collider was stayed within
            /// </summary>
            Stay,

            /// <summary>
            /// The collider was exited
            /// </summary>
            Exit
        }

        /// <summary>
        /// The type of the collision event
        /// </summary>
        public COLLISION_TYPE collisionType;

        /// <summary>
        /// The bone animation associated with this collision
        /// </summary>
        public BoneAnimation boneAnimation;

        /// <summary>
        /// The bone node index of the bone where the collider is attached
        /// </summary>
        public int boneNodeIndex;

        /// <summary>
        /// The name of the bone where the collider is attached
        /// </summary>
        public string boneName;

        /// <summary>
        /// The collision information stored by Unity's physics engine
        /// </summary>
        public Collision collision;

        /// <summary>
        /// A string tag
        /// </summary>
        public string tag;

        public CollisionEvent()
        {
            collisionType = COLLISION_TYPE.None;
            boneAnimation = null;
            boneNodeIndex = -1;
            boneName = "";
            collision = null;
            tag = "";
        }

        public CollisionEvent(CollisionEvent copyEvent)
        {
            CopyEvent(copyEvent);
        }

        public void CopyEvent(CollisionEvent copyEvent)
        {
            collisionType = copyEvent.collisionType;
            boneAnimation = copyEvent.boneAnimation;
            boneNodeIndex = copyEvent.boneNodeIndex;
            boneName = copyEvent.boneName;
            collision = copyEvent.collision;
            tag = copyEvent.tag;
        }
    }
}