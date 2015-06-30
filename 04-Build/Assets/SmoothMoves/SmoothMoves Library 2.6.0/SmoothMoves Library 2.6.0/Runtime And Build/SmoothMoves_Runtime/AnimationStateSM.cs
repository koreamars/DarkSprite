using UnityEngine;

namespace SmoothMoves
{
    /// <summary>
    /// This class wraps the AnimationStates from the Animation object in BoneAnimation.
    /// It's vital that this state is used in place of the Animation object's AnimationStates since
    /// SmoothMoves modifies some values like speed and time to better reflect the workflow that 
    /// animators are used to. For example, the speed is actually set to the Frames per Second of the
    /// animation in order to handle animation curves appropriately. This class returns 1.0f if the 
    /// speed equals the original fps. There are some additions to the AnimationState you are used to, 
    /// like the fps variable which returns how fast the animation is going relative to the frames
    /// you created in the editor.
    /// 
    /// For more reference, refer to the animation state documentation in Unity since most of these
    /// variables and functions are just wrappers around Unity's AnimationState.
    /// </summary>
    ///
    [System.Serializable]
    public class AnimationStateSM
    {
        public AnimationStateSM()
        {
        }

        // References to the original AnimationState and AnimationClipSM
        public AnimationState _realAnimationState;
        public float _fps;

        /// <summary>
        /// Gets or sets whether this animation is enabled. Can be used to blend animations
        /// on the same layer.
        /// </summary>
        public bool enabled
        {
            get
            {
                return _realAnimationState.enabled;
            }
            set
            {
                _realAnimationState.enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the weight of the animation state. This can be set in the editor as well.
        /// </summary>
        public float weight
        {
            get
            {
                return _realAnimationState.weight;
            }
            set
            {
                _realAnimationState.weight = value;
            }
        }

        /// <summary>
        /// Gets or sets the wrap mode of the animation state. This can be set in the editor as well.
        /// </summary>
        public WrapMode wrapMode
        {
            get
            {
                return _realAnimationState.wrapMode;
            }
            set
            {
                _realAnimationState.wrapMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the time of the animation. This value will return what you are traditionally
        /// used to with normal animated meshes by converting the internal time (based on fps) to 
        /// real time.
        /// </summary>
        public float time
        {
            get
            {
                return _realAnimationState.time / _fps;
            }
            set
            {
                _realAnimationState.time = value * _fps;
            }
        }


        /// <summary>
        /// Gets or sets the frame of the animation. This value will return what you set in the animation
        /// editor.
        /// </summary>
        public float frame
        {
            get
            {
                return _realAnimationState.time;
            }
            set
            {
                _realAnimationState.time = value;
            }
        }

        /// <summary>
        /// Gets or sets the frame of the animation. This value will return what you set in the animation
        /// editor. This member converts the frame to the last integer frame passed.
        /// </summary>
        public int frameInt
        {
            get
            {
                return Mathf.FloorToInt(_realAnimationState.time);
            }
            set
            {
                _realAnimationState.time = (float)value;
            }
        }

        /// <summary>
        /// Gets or sets the normalized time of the animation state. The normalized time ranges from 0 to 1
        /// </summary>
        public float normalizedTime
        {
            get
            {
                return _realAnimationState.normalizedTime;
            }
            set
            {
                _realAnimationState.normalizedTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the speed of the animation. This value will return what you are traditionally
        /// used to with normal animated meshes by converting the internal speed (based on fps) to 
        /// real speed.
        /// </summary>
        public float speed
        {
            get
            {
                return _realAnimationState.speed / _fps;
            }
            set
            {
                _realAnimationState.speed = value * _fps;
            }
        }

        /// <summary>
        /// This is a bonus setting that lets you get or set the frames per second based on the 
        /// animation created in the editor.
        /// </summary>
        public float fps
        {
            get
            {
                return _realAnimationState.speed;
            }
            set
            {
                _realAnimationState.speed = value;
            }
        }

        /// <summary>
        /// Gets or sets the normalized speed of the animation state. 
        /// </summary>
        public float normalizedSpeed
        {
            get
            {
                return _realAnimationState.normalizedSpeed;
            }
            set
            {
                _realAnimationState.normalizedSpeed = value;
            }
        }

        /// <summary>
        /// Gets the length of the animation state. This value will return what you are traditionally
        /// used to with normal animated meshes by converting the internal length (based on fps) to 
        /// real length. 
        /// </summary>
        public float length
        {
            get
            {
                return _realAnimationState.length / _fps;
            }
        }

        /// <summary>
        /// Gets the number of frames of the animation state.
        /// </summary>
        public int totalFrames
        {
            get
            {
                return Mathf.FloorToInt(_realAnimationState.length);
            }
        }

        /// <summary>
        /// Gets or sets the layer this animation plays on. Higher layers receive blend weights first
        /// </summary>
        public int layer
        {
            get
            {
                return _realAnimationState.layer;
            }
            set
            {
                _realAnimationState.layer = value;
            }
        }

        /// <summary>
        /// Gets the clip for this animation state.
        /// </summary>
        public AnimationClip clip
        {
            get
            {
                return _realAnimationState.clip;
            }
        }

        /// <summary>
        /// Gets or sets the name of this animation state
        /// </summary>
        public string name
        {
            get
            {
                return _realAnimationState.name;
            }
            set
            {
                _realAnimationState.name = value;
            }
        }

        /// <summary>
        /// Gets or sets the blend mode of the animation state.
        /// </summary>
        public AnimationBlendMode blendMode
        {
            get
            {
                return _realAnimationState.blendMode;
            }
            set
            {
                _realAnimationState.blendMode = value;
            }
        }

        /// <summary>
        /// Creates a new animation state. This is a function used internally by BoneAnimation and should
        /// not need to be called through user scripts.
        /// </summary>
        /// <param name="realAnimationState">The actual animation state this class is wrapping</param>
        /// <param name="smAnimationClip">The AnimationClipSM that this class is utilizing for adjustments</param>
        public AnimationStateSM(ref AnimationState realAnimationState, float fps)
        {
            _realAnimationState = realAnimationState;
            _fps = fps;
        }

        /// <summary>
        /// Wrapper for AnimationState.AddMixingTransform. This can be done in the editor, so it probably
        /// won't be used often in script.
        /// </summary>
        /// <param name="mix">The transform to add to the mixing animation</param>
        public void AddMixingTransform(Transform mix)
        {
            _realAnimationState.AddMixingTransform(mix);
        }

        /// <summary>
        /// Wrapper for AnimationState.AddMixingTransform. This can be done in the editor, so it probably
        /// won't be used often in scripts
        /// </summary>
        /// <param name="mix">The transform to add to the mixing animation</param>
        /// <param name="recursive">Recursively add child transforms to the mixing animation</param>
        public void AddMixingTransform(Transform mix, bool recursive)
        {
            _realAnimationState.AddMixingTransform(mix, recursive);
        }

        /// <summary>
        /// Wrapper for the AnimationState.RemoveMixingTransform
        /// </summary>
        /// <param name="mix">Transform to remove from animation mixing</param>
        public void RemoveMixingTransform(Transform mix)
        {
            _realAnimationState.RemoveMixingTransform(mix);
        }
    }
}