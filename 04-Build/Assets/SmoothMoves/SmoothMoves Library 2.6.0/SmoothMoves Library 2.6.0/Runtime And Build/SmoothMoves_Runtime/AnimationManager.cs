using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmoothMoves
{
    /// <summary>
    /// This class' sole purpose is to cut down on the amount of reflection Unity has to perform each frame.
    /// Without this singleton managing the animations, each animation would have to call their LateUpdate()
    /// function, which would be much slower than a single call to LateUpdate in this class.
    /// </summary>
	public class AnimationManager : MonoBehaviour
	{
        static private AnimationManager _instance;

        private List<BoneAnimation> _boneAnimations;

        static public bool Exists
        {
            get
            {
                return (_instance != null);
            }
        }

        /// <summary>
        /// This class is a singleton, so referencing the Instance will create a new
        /// gameobject if it does not exist, or pull the existing manager component
        /// </summary>
        static public AnimationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go;
                    
                    // first look for a leftover object in case Unity failed to clean it up
                    go = GameObject.Find("_SmoothMoves_Manager");
                    if (go == null)
                    {
                        // no object found, so we make a new one
                        go = new GameObject("_SmoothMoves_Manager");
                        go.transform.localPosition = Vector3.zero;
                        go.transform.localRotation = Quaternion.identity;
                        go.transform.localScale = Vector3.one;

                        _instance = go.AddComponent<AnimationManager>();
                    }
                    else
                    {
                        // the object was found, now check to see if the animation manager component
                        // still exists
                        _instance = go.GetComponent<AnimationManager>();
                        if (_instance == null)
                        {
                            // no manager exists, so we create a new one
                            _instance = go.AddComponent<AnimationManager>();
                        }
                    }

                    _instance.Initialize();
                }

                return _instance;
            }
        }

        public int TotalBoneAnimations
        {
            get
            {
                if (_boneAnimations == null)
                    return 0;
                else
                    return _boneAnimations.Count;
            }
        }

        public void Initialize()
        {
            if (_boneAnimations == null)
            {
                _boneAnimations = new List<BoneAnimation>();
            }
            else
            {
                _boneAnimations.Clear();
            }
        }

        /// <summary>
        /// This function is called by the BoneAnimation script in the 
        /// OnEnable function
        /// </summary>
        /// <param name="boneAnimation"></param>
        public void AddBoneAnimation(BoneAnimation boneAnimation)
        {
            if (!_boneAnimations.Contains(boneAnimation))
            {
                _boneAnimations.Add(boneAnimation);
            }
        }

        /// <summary>
        /// This function is called by the BoneAnimation script in the
        /// OnDisable function
        /// </summary>
        /// <param name="boneAnimation"></param>
        public void RemoveBoneAnimation(BoneAnimation boneAnimation)
        {
            if (_boneAnimations.Contains(boneAnimation))
            {
                _boneAnimations.Remove(boneAnimation);
            }
        }

        /// <summary>
        /// Using a single Update call cuts down on the amount of reflection Unity has to do.
        /// This will see a performance boost, especially on mobile devices.
        /// </summary>
        void LateUpdate()
        {
            //foreach (BoneAnimation boneAnimation in _copiedBoneAnimations)
            foreach (BoneAnimation boneAnimation in _boneAnimations)
            {
                boneAnimation.LateFrameUpdate();
            }
        }
	}
}
