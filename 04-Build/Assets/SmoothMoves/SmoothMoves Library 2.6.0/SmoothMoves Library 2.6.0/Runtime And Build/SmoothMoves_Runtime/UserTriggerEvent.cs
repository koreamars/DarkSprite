using UnityEngine;

namespace SmoothMoves
{
    /// <summary>
    /// This class stores information when a user trigger is fired from a 
    /// keyframe in a SmoothMoves BoneAnimation. This can be useful to control
    /// external events in your project's code, such as playing a sound, stopping
    /// or starting another animation, etc.
    /// </summary>
    public class UserTriggerEvent
    {
        /// <summary>
        /// The bone animation related to this trigger
        /// </summary>
        public BoneAnimation boneAnimation;

        /// <summary>
        /// The name of the bone where the trigger was fired
        /// </summary>
        public string boneName;

        /// <summary>
        /// The index of the animation clip where the trigger was fired
        /// </summary>
        public int clipIndex;

        /// <summary>
        /// The name of the animation where the trigger was fired
        /// </summary>
        public string animationName;

        /// <summary>
        /// The frame the trigger was fired on
        /// </summary>
        public int frame;

        /// <summary>
        /// The time the trigger was fired
        /// </summary>
        public float time;

        /// <summary>
        /// The normalized time (0..1) that the trigger was fired
        /// </summary>
        public float normalizedTime;

        /// <summary>
        /// The index of the bone node where the trigger was fired
        /// </summary>
        public int boneNodeIndex;

        /// <summary>
        /// The transform of the bone where the trigger was fired
        /// </summary>
        public Transform boneTransform;

        /// <summary>
        /// The transform of the sprite where the trigger was fired
        /// </summary>
        public Transform spriteTransform;

        /// <summary>
        /// A string value of the trigger. This can be useful to allow multiple triggers to
        /// have the same effect by passing the same tag, or to distinguish between
        /// triggers and what they should do.
        /// </summary>
        public string tag;

        public UserTriggerEvent()
        {
            boneAnimation = null;
            boneName = "";
            clipIndex = -1;
            animationName = "";
            frame = -1;
            time = -1.0f;
            normalizedTime = -1.0f;
            boneNodeIndex = -1;
            boneTransform = null;
            spriteTransform = null;
            tag = "";
        }

        public UserTriggerEvent(UserTriggerEvent copyEvent)
        {
            CopyEvent(copyEvent);
        }

        public void CopyEvent(UserTriggerEvent copyEvent)
        {
            boneAnimation = copyEvent.boneAnimation;
            boneName = copyEvent.boneName;
            clipIndex = copyEvent.clipIndex;
            animationName = copyEvent.animationName;
            frame = copyEvent.frame;
            time = copyEvent.time;
            normalizedTime = copyEvent.normalizedTime;
            boneNodeIndex = copyEvent.boneNodeIndex;
            boneTransform = copyEvent.boneTransform;
            spriteTransform = copyEvent.spriteTransform;
            tag = copyEvent.tag;
        }
    }
}