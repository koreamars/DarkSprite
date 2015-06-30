using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SmoothMoves
{
    // Delegates to pass back information to user scripts
    public delegate void UserTriggerDelegate(UserTriggerEvent triggerEvent);
    public delegate void ColliderTriggerDelegate(ColliderTriggerEvent colliderEvent);
    public delegate void CollisionDelegate(CollisionEvent collisionEvent);

    /// <summary>
    /// Handles runtime control of a SmoothMoves mesh object. The Animation object's functions are wrapped here for easy and safe access.
    /// </summary>
    public class BoneAnimation : MonoBehaviour
    {
        public BoneAnimation()
        {
        }

        // Internal variables used by the bone animation. Some of these are public so that
        // editor scripts can access them, but they should not be modified by user scripts.
        #region Internal Variables

        private const int MAX_SUBMESHES = 100;

        // internal events

        private UserTriggerEvent _lastUserTriggerEvent;
        private ColliderTriggerEvent _lastColliderTriggerEvent;
        private CollisionEvent _lastCollisionEvent;

        // internal reference lookups

        private int[] _boneDataIndexToBoneNodeIndexArray;
        private Material[] _originalMaterialArray;
        private TriggerFrameBone[] _lastTriggerFrameBoneArray;

        // internal members to reduce runtime allocations

        private Vector2 _uvPoint;
        private bool [] _refireList;
        private Rect _lastVisitedUVDefault = new Rect(0, 0, 0, 0);
        private int _animationClipIndex_internal;
        private int _animationClipIndex;
        private int _boneIndex_internal;
        private int _materialIndex_internal;
        private int _textureAtlasIndex_internal;
        private List<MeshQuad> _meshQuads;
        private MeshQuad _meshQuad;
        private int _meshQuadIndex;
        private int _meshQuadCount;
        private int _generateBoneIndex;
        private SortMeshQuadDepthDescending _sortMeshQuadDepthDescending;
        private List<SubMesh> _subMeshes = new List<SubMesh>();
        private SubMesh _subMesh;
        private int _subMeshIndex;
        private int _subMeshCount;
        private int _lastMaterialIndex;
        private int _triangleIndex;

        private Color _colorBlendMeshBoneColor;
        private float _colorBlendBoneAnimationColorBlendWeight = 0;
        private bool _colorBlendFoundBoneAnimationColor = false;
        private string _colorBlendAnimationName;
        private Color _colorBlendBoneAnimationColor = Color.black;
        private Color _colorBlendFinalColor;
        private int _colorBlendClipIndex;
        private int _colorBlendHighestLayer = -1;
        private int _colorBlendHighestClipIndex = -1;
        private float _colorBlendHighestTime = -1;
        private AnimationState _colorBlendAnimationState;
        private int _colorBlendStateIndex;

        // internal lists to keep track of events.
        // note that these are dynamic lists that may rarely change through the lifetime of the animation.

        private List<UserTriggerDelegate> _userTriggerDelegates = new List<UserTriggerDelegate>();
        private List<ColliderTriggerDelegate> _colliderTriggerDelegates = new List<ColliderTriggerDelegate>();
        private List<CollisionDelegate> _collisionDelegates = new List<CollisionDelegate>();

        // these are public to serialize the data,
        // but should not be referenced from outside scripts
        // other than AnimationHelper

        [HideInInspector]
        public SkinnedMeshRenderer mRenderer;
        [HideInInspector]
        public Animation mAnimation;
        [HideInInspector]
        public Mesh mMesh;

        [HideInInspector]
        public Vector3[] mVertices;
        [HideInInspector]
        public Vector2[] mUVs;
        [HideInInspector]
        public Color[] mColors;

        [HideInInspector]
        public BoneAnimationData animationData; // legacy, should be null going forward
        [HideInInspector]
        public string animationDataGUID;
        [HideInInspector]
        public Material[] mMaterialSource;
        [HideInInspector]
        public AnimationBone[] mBoneSource;
        [HideInInspector]
        public Transform[] mBoneTransforms;
        [HideInInspector]
        public Material[] mMaterials;
        [HideInInspector]
        public string buildID;
        [HideInInspector]
        public float mFinalImportScale;
        [HideInInspector]
        public string mDefaultAnimationName;
        [HideInInspector]
        public BoneColor[] mLastBoneColor;
        [HideInInspector]
        public Color[] mLastFinalColor;
        [HideInInspector]
        public BoneColorAnimation[] mBoneColorAnimations;
        [HideInInspector]
        public TriggerFrameBoneCurrent[] mCurrentTriggerFrameBones;
        [HideInInspector]
        public string[] mBoneTransformPaths;
        [HideInInspector]
        public string[] mSpriteTransformPaths;
        [HideInInspector]
        public TriggerFrame[] triggerFrames;
        [HideInInspector]
        public TextureAtlas[] textureAtlases;
        [HideInInspector]
        public AnimationClipSM_Lite[] mAnimationClips;
        [HideInInspector]
        public AnimationStateSM[] mAnimationStates;
        [HideInInspector]
        public Transform mLocalTransform;
        [HideInInspector]
        public Color mMeshColor;
        [HideInInspector]
        public bool updateColors = false;
        [HideInInspector]
        public bool updatePrefabs = true;

        #endregion

        // These event holders are transient and will only show the last event that occurred
        #region Temporary Storage for Events

        // The last user trigger that occurred. This will be overwritten as new user trigger events come in.
        public UserTriggerEvent LastUserTriggerEvent { get { return _lastUserTriggerEvent; } }

        // The last collider event that occurred. This will be overwritten as new collider events come in.
        public ColliderTriggerEvent LastColliderTriggerEvent { get { return _lastColliderTriggerEvent; } }

        // The last collision event that occurred. This will be overwritten as new collision events come in.
        public CollisionEvent LastCollisionEvent { get { return _lastCollisionEvent; } }

        #endregion

        // These wrappers mimic the functionality of the Animation object while protecting the
        // data and making sure all calls are safe.
        #region Wrappers for Animation object

        /// <summary>
        /// Item property to easily access an AnimationStateSM, referenced by animation name. Similar to the item property of the Animation object
        /// </summary>
        /// <param name="animationName"></param>
        public AnimationStateSM this[string animationName] 
        { 
            get 
            {
                _animationClipIndex = GetAnimationClipIndex(animationName);
                if (_animationClipIndex != -1)
                {
                    return mAnimationStates[_animationClipIndex]; 
                } 
                else 
                { 
                    Debug.LogError("The animation named [" + animationName + "] does not exist"); 
                    return null; 
                } 
            } 
        }

        /// <summary>
        /// Item property to easily access an AnimationStateSM, referenced by clip index. Similar to the item property of the Animation object
        /// </summary>
        /// <param name="clipIndex"></param>
        public AnimationStateSM this[int clipIndex] { get { if (ClipIndexOK(clipIndex)) { return mAnimationStates[clipIndex]; } else { return null; } } }

        /// <summary>
        /// Enumerator for the SmoothMoves_AnimationStates, so you can use foreach on this class. It will return an enumerator of the AnimationStateSM type
        /// </summary>
        public AnimationState_Enumerator GetEnumerator() { return new AnimationState_Enumerator(mAnimationStates); }

        /// <summary>
        /// Default clip of the Animation object
        /// </summary>
        public AnimationClip clip { get { return mAnimation.clip; } set { mAnimation.clip = value; } }

        /// <summary>
        /// Should the default animation clip automatically start playing on startup?
        /// </summary>
        public bool playAutomatically { get { return mAnimation.playAutomatically; } set { mAnimation.playAutomatically = value; } }

        /// <summary>
        /// How should time beyond the playback range of the clip be treated?
        /// </summary>
        public WrapMode wrapMode { get { return mAnimation.wrapMode; } set { mAnimation.wrapMode = value; } }

        /// <summary>
        /// Are we playing any animations?
        /// </summary>
        public bool isPlaying { get { return mAnimation.isPlaying; } }

        /// <summary>
        /// When turned on, animations will be executed in the physics loop. This is only useful in conjunction with kinematic rigidbodies
        /// </summary>
        public bool animatePhysics { get { return mAnimation.animatePhysics; } set { mAnimation.animatePhysics = value; } }

        /// <summary>
        /// Controls culling of the Animation component
        /// </summary>
        public AnimationCullingType cullingType { get { return mAnimation.cullingType; } set { mAnimation.cullingType = value; } }

        /// <summary>
        /// AABB of the animation in local space
        /// </summary>
        public Bounds localBounds { get { return mAnimation.localBounds; } set { mAnimation.localBounds = value; } }

        #endregion

        // Internal processes are used by editor scripts and should not be accessed by user scripts.
        #region Internal Processes

        private int GetBoneDataIndexFromBoneName(string boneName)
        {
            for (_boneIndex_internal = 0; _boneIndex_internal < mBoneSource.Length; _boneIndex_internal++)
            {
                if (mBoneSource[_boneIndex_internal].boneName == boneName)
                    return mBoneSource[_boneIndex_internal].boneDataIndex;
            }

            return -1;
        }

        private int GetBoneNodeIndexFromBoneName(string boneName)
        {
            for (_boneIndex_internal = 0; _boneIndex_internal < mBoneSource.Length; _boneIndex_internal++)
            {
                if (mBoneSource[_boneIndex_internal].boneName == boneName)
                    return _boneIndex_internal;
            }

            return -1;
        }

        private int GetOriginalMaterialIndex(Material material)
        {
            for (_materialIndex_internal = 0; _materialIndex_internal < _originalMaterialArray.Length; _materialIndex_internal++)
            {
                if (_originalMaterialArray[_materialIndex_internal] == material)
                    return _materialIndex_internal;
            }

            return -1;
        }

        private TextureAtlas GetTextureAtlasFromName(string textureAtlasName)
        {
            for (_textureAtlasIndex_internal = 0; _textureAtlasIndex_internal < textureAtlases.Length; _textureAtlasIndex_internal++)
            {
                if (textureAtlases[_textureAtlasIndex_internal].name == textureAtlasName)
                    return textureAtlases[_textureAtlasIndex_internal];
            }

            return null;
        }

        void Awake()
        {
            int boneNodeIndex;
            int clipIndex;

            mLocalTransform = transform;

            // set up our bone related dictionaries
            _boneDataIndexToBoneNodeIndexArray = new int[mBoneSource.Length];
            _refireList = new bool[mBoneSource.Length];
            _lastTriggerFrameBoneArray = new TriggerFrameBone[mBoneSource.Length];
            _meshQuads = new List<MeshQuad>();
            _subMeshes = new List<SubMesh>();
            for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                mBoneSource[boneNodeIndex].Awake(this);

                _boneDataIndexToBoneNodeIndexArray[mBoneSource[boneNodeIndex].boneDataIndex] = boneNodeIndex;
                _refireList[boneNodeIndex] = false;
                _lastTriggerFrameBoneArray[boneNodeIndex] = null;
                _meshQuads.Add(new MeshQuad(null, -1, -1));
                _subMeshes.Add(new SubMesh(-1));
            }

            _sortMeshQuadDepthDescending = new SortMeshQuadDepthDescending();

            _originalMaterialArray = new Material[mMaterialSource.Length];
            for (int matIndex = 0; matIndex < mMaterialSource.Length; matIndex++)
            {
                _originalMaterialArray[matIndex] = mMaterialSource[matIndex];
            }

            // create user trigger event and callback list
            _lastUserTriggerEvent = new UserTriggerEvent();
            _lastColliderTriggerEvent = new ColliderTriggerEvent();
            _lastCollisionEvent = new CollisionEvent();

            mAnimationStates = new AnimationStateSM[mAnimationClips.Length];

            string animationName;
            AnimationClipSM_Lite clip;
            AnimationState animState;
            AnimationStateSM smAnimationState;
            int clipBoneIndex;
            for (clipIndex = 0; clipIndex < mAnimationClips.Length; clipIndex++)
            {
                animationName = mAnimationClips[clipIndex].animationName;
                clip = mAnimationClips[clipIndex];
                animState = mAnimation[animationName];

                if (animState != null)
                {
                    animState.speed = clip.fps;
                    animState.blendMode = clip.blendMode;
                    animState.layer = clip.layer;
                    animState.weight = clip.blendWeight;

                    for (clipBoneIndex = 0; clipBoneIndex < clip.bones.Count; clipBoneIndex++)
                    {
                        clip.bones[clipBoneIndex].InitializeColorCurves();

                        // add in mixing if necessary
                        if (clip.mix)
                        {
                            if (clip.bones[clipBoneIndex].mixTransform)
                            {
                                boneNodeIndex = _boneDataIndexToBoneNodeIndexArray[clip.bones[clipBoneIndex].boneDataIndex];
                                animState.AddMixingTransform(mBoneSource[boneNodeIndex].boneTransform, false);
                                animState.AddMixingTransform(mBoneSource[boneNodeIndex].spriteTransform, false);
                            }
                        }
                    }

                    smAnimationState = (AnimationStateSM)(new AnimationStateSM(ref animState, clip.fps));
                    mAnimationStates[clipIndex] = smAnimationState;
                }
                else
                {
                    Debug.LogError("Could not find an animation state for the animation [" + animationName + "]");
                }
            }

            CreateMeshFromStructures();
        }

        private bool CreateMeshFromStructures()
        {
            // only create the mesh if there is data missing.
            // most likely this will be due to the object being
            // created from a prefab.
            if (
                    mMesh == null ||
                    mRenderer.sharedMesh == null ||
                    mVertices == null ||
                    mUVs == null ||
                    mColors == null
                )
            {
                Vector3[] normals = new Vector3[mBoneSource.Length * 4];
                Matrix4x4[] bindPoses = new Matrix4x4[mBoneSource.Length * 2];
                BoneWeight[] boneWeights = new BoneWeight[mBoneSource.Length * 4];
                int buildBoneNodeIndex;

                for (buildBoneNodeIndex = 0; buildBoneNodeIndex < mBoneSource.Length; buildBoneNodeIndex++)
                {
                    // sprite bone weight (skip the base bone since its transform will be propogated automatically)
                    boneWeights[(buildBoneNodeIndex * 4) + 0].boneIndex0 = (buildBoneNodeIndex * 2) + 1;
                    boneWeights[(buildBoneNodeIndex * 4) + 0].weight0 = 1.0f;

                    boneWeights[(buildBoneNodeIndex * 4) + 1].boneIndex0 = (buildBoneNodeIndex * 2) + 1;
                    boneWeights[(buildBoneNodeIndex * 4) + 1].weight0 = 1.0f;

                    boneWeights[(buildBoneNodeIndex * 4) + 2].boneIndex0 = (buildBoneNodeIndex * 2) + 1;
                    boneWeights[(buildBoneNodeIndex * 4) + 2].weight0 = 1.0f;

                    boneWeights[(buildBoneNodeIndex * 4) + 3].boneIndex0 = (buildBoneNodeIndex * 2) + 1;
                    boneWeights[(buildBoneNodeIndex * 4) + 3].weight0 = 1.0f;

                    SetNormalData(ref normals, (buildBoneNodeIndex * 4), Vector3.back);
                }

                for (buildBoneNodeIndex = 0; buildBoneNodeIndex < (mBoneSource.Length * 2); buildBoneNodeIndex++)
                {
                    bindPoses[buildBoneNodeIndex] = Matrix4x4.identity;
                }

                mMesh = new Mesh();
                mRenderer.sharedMesh = mMesh;

                mMesh.subMeshCount = 1;
                mMesh.vertices = mVertices;
                mMesh.uv = mUVs;
                mMesh.normals = normals;
                mMesh.colors = mColors;
                mMesh.bindposes = bindPoses;
                mMesh.boneWeights = boneWeights;

                GenerateMesh();

                mMesh.RecalculateBounds();
                mMesh.Optimize();
            }

            return true;
        }

        void Start()
        {
            if (mAnimation == null)
                return;

            if (mAnimation.playAutomatically)
            {
                Play(0);
            }
        }

        private void AnimationEvent_Triggered(AnimationEvent animationEvent)
        {
            AnimationEventTrigger(triggerFrames[animationEvent.intParameter]);
        }

        private void AnimationEventTrigger(TriggerFrame triggerFrame)
        {
            AnimationState animationState;
            int animationLayer;
            string animationName;
            TriggerFrameBoneCurrent currentTriggerFrameBone;
            bool useTrigger;

            if (triggerFrame == null)
                return;

            animationName = GetAnimationClipName(triggerFrame.clipIndex);
            animationState = mAnimation[animationName];
            animationLayer = animationState.layer;

            bool regenerateMesh = false;
            int triggerFrameBoneIndex;
            int userTriggerDelegateIndex;

            for (triggerFrameBoneIndex=0; triggerFrameBoneIndex < triggerFrame.triggerFrameBones.Count; triggerFrameBoneIndex++)
            {
                currentTriggerFrameBone = mCurrentTriggerFrameBones[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex];
                useTrigger = false;

                // check if the trigger just fired is on a higher layer than (or the same layer as) the last trigger for this bone
                if (animationLayer >= currentTriggerFrameBone.animationLayer)
                {
                    // this trigger has a higher layer, so we'll use the new trigger
                    currentTriggerFrameBone.animationName = animationName;
                    currentTriggerFrameBone.animationLayer = animationLayer;
                    useTrigger = true;
                }
                else
                {
                    // last trigger was on a higher layer, let's make sure the animation is still playing
                    if (!mAnimation.IsPlaying(currentTriggerFrameBone.animationName))
                    {
                        // the last animation has stopped, so we can use the new trigger
                        currentTriggerFrameBone.animationName = animationName;
                        currentTriggerFrameBone.animationLayer = animationLayer;
                        useTrigger = true;
                    }
                }

                if (useTrigger)
                {
                    if (triggerFrame.triggerFrameBones[triggerFrameBoneIndex].triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.UserTrigger))
                    {
                        // This frame had a user trigger, so we will notify the delegates registered through user scripts

                        _lastUserTriggerEvent.boneAnimation = this;
                        _lastUserTriggerEvent.boneName = mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].boneName;
                        _lastUserTriggerEvent.clipIndex = triggerFrame.clipIndex;
                        _lastUserTriggerEvent.animationName = animationName;
                        _lastUserTriggerEvent.frame = triggerFrame.frame;
                        _lastUserTriggerEvent.time = (float)triggerFrame.frame / (animationState.speed == 0 ? 1.0f : animationState.speed);
                        _lastUserTriggerEvent.normalizedTime = (float)triggerFrame.frame / animationState.length;
                        _lastUserTriggerEvent.boneNodeIndex = triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex;
                        _lastUserTriggerEvent.boneTransform = mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].boneTransform;
                        _lastUserTriggerEvent.spriteTransform = mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].spriteTransform;
                        _lastUserTriggerEvent.tag = triggerFrame.triggerFrameBones[triggerFrameBoneIndex].userTriggerTag;

                        for (userTriggerDelegateIndex=0; userTriggerDelegateIndex < _userTriggerDelegates.Count; userTriggerDelegateIndex++)
                        {
                            _userTriggerDelegates[userTriggerDelegateIndex](_lastUserTriggerEvent);
                        }
                    }

                    if (FireTextureTrigger(triggerFrame.triggerFrameBones[triggerFrameBoneIndex]))
                        regenerateMesh = true;

                    if (triggerFrame.triggerFrameBones[triggerFrameBoneIndex].triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.ChangeDepth))
                    {
                        // Depth has changed on a bone

                        mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].depth = triggerFrame.triggerFrameBones[triggerFrameBoneIndex].depth;
                        regenerateMesh = true;
                    }

                    if (triggerFrame.triggerFrameBones[triggerFrameBoneIndex].triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.ChangeCollider))
                    {
                        // Something changed about the collider for this bone

                        if (triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider == null)
                        {
                            // Collider is going away

                            mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].TurnOffBoxCollider();
                            mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].TurnOffSphereCollider();
                            mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].SetLayer(gameObject.layer);
                        }
                        else
                        {
                            switch (triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.type)
                            {
                                case ColliderSM.COLLIDER_TYPE.None:
                                    // Collider is going away
                                    mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].TurnOffBoxCollider();
                                    mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].TurnOffSphereCollider();
                                    mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].SetLayer(gameObject.layer);
                                    break;

                                case ColliderSM.COLLIDER_TYPE.Box:
                                    // Box collider is turning on
                                    mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].TurnOnBoxCollider(triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.center * mFinalImportScale, triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.boxSize * mFinalImportScale, triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.tag);
                                    mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].TurnOffSphereCollider();
                                    mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].SetTrigger(triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.isTrigger);
                                    mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].SetLayer(triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.useAnimationLayer ? gameObject.layer : triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.layer);
                                    break;

                                case ColliderSM.COLLIDER_TYPE.Sphere:
                                    // Sphere collider is turning on
                                    mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].TurnOffBoxCollider();
                                    mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].TurnOnSphereCollider(triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.center * mFinalImportScale, triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.sphereRadius * mFinalImportScale, triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.tag);
                                    mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].SetTrigger(triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.isTrigger);
                                    mBoneSource[triggerFrame.triggerFrameBones[triggerFrameBoneIndex].boneNodeIndex].SetLayer(triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.useAnimationLayer ? gameObject.layer : triggerFrame.triggerFrameBones[triggerFrameBoneIndex].collider.layer);
                                    break;
                            }
                        }
                    }
                }
            }

            if (regenerateMesh)
            {
                // Some trigger or a combination of triggers requires that this mesh be regenerated
                GenerateMesh();
            }
        }

        private bool FireTextureTrigger(TriggerFrameBone bone)
        {
            bool regenerateMesh = false;
            bool wasTextureEvent = false;

            if (bone.triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.HideTexture))
            {
                if (mBoneSource[bone.boneNodeIndex].visible)
                {
                    mBoneSource[bone.boneNodeIndex].visible = false;
                    regenerateMesh = true;
                }
            }
            else
            {
                if (!mBoneSource[bone.boneNodeIndex].visible)
                {
                    mBoneSource[bone.boneNodeIndex].visible = true;
                    regenerateMesh = true;
                }

                if (bone.triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.ChangeMaterial))
                {
                    // Material has changed on a bone

                    mBoneSource[bone.boneNodeIndex].materialIndex = bone.materialIndex;
                    regenerateMesh = true;
                    wasTextureEvent = true;
                }
                if (bone.triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.ChangeTexture))
                {
                    // Texture has changed on a bone

                    SetVertexData((bone.boneNodeIndex * 4),
                                  bone.upperLeft,
                                  bone.bottomLeft,
                                  bone.bottomRight,
                                  bone.upperRight);
                    mMesh.vertices = mVertices;

                    SetBoneUV((bone.boneNodeIndex * 4), bone.uv);
                    mMesh.uv = mUVs;

                    wasTextureEvent = true;
                }
                if (bone.triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.ChangePivot))
                {
                    // Texture pivot point has changed on a bone

                    SetVertexData((bone.boneNodeIndex * 4),
                                  bone.upperLeft,
                                  bone.bottomLeft,
                                  bone.bottomRight,
                                  bone.upperRight);
                    mMesh.vertices = mVertices;

                    wasTextureEvent = true;
                }
            }

            if (wasTextureEvent)
            {
                _lastTriggerFrameBoneArray[bone.boneNodeIndex] = bone;
            }

            return regenerateMesh;
        }

        private bool FireLastTextureAnimationEvent(string boneName)
        {
            int boneNodeIndex = GetBoneNodeIndexFromBoneName(boneName);
            
            if (boneNodeIndex == -1)
            {
                return false;
            }

            bool regenerateMesh = false;
            TriggerFrameBone lastTriggerFrameBone = _lastTriggerFrameBoneArray[boneNodeIndex];

            if (lastTriggerFrameBone != null)
            {
                regenerateMesh = FireTextureTrigger(lastTriggerFrameBone);
            }

            return regenerateMesh;
        }

        private void SetBoneUV(int boneOffset, Rect uv)
        {
            _uvPoint.x = uv.x;
            _uvPoint.y = uv.yMax;
            mUVs[boneOffset + 0] = _uvPoint;

            _uvPoint.x = uv.x;
            _uvPoint.y = uv.y;
            mUVs[boneOffset + 1] = _uvPoint;

            _uvPoint.x = uv.xMax;
            _uvPoint.y = uv.y;
            mUVs[boneOffset + 2] = _uvPoint;

            _uvPoint.x = uv.xMax;
            _uvPoint.y = uv.yMax;
            mUVs[boneOffset + 3] = _uvPoint;
        }

        private void SetBoneColor(int boneOffset, Color c)
        {
            mColors[boneOffset + 0] = c;
            mColors[boneOffset + 1] = c;
            mColors[boneOffset + 2] = c;
            mColors[boneOffset + 3] = c;
        }

        static private void SetNormalData(ref Vector3[] normals,
                                            int boneOffset,
                                            Vector3 normal)
        {
            normals[boneOffset + 0] = normal;
            normals[boneOffset + 1] = normal;
            normals[boneOffset + 2] = normal;
            normals[boneOffset + 3] = normal;
        }

        private void SetVertexData(int boneOffset,
                                   Vector3 upperLeft,
                                   Vector3 bottomLeft,
                                   Vector3 bottomRight,
                                   Vector3 upperRight)
        {
            mVertices[boneOffset + 0] = upperLeft;
            mVertices[boneOffset + 1] = bottomLeft;
            mVertices[boneOffset + 2] = bottomRight;
            mVertices[boneOffset + 3] = upperRight;
        }

        /// <summary>
        /// Regenerates the mesh, usually called after a relevant trigger changes the mesh
        /// </summary>
        public void GenerateMesh()
        {
            // check to see if the necessary structures exist,
            // if not, then build them

            if (_meshQuads == null)
                _meshQuads = new List<MeshQuad>();

            if (_subMeshes == null)
                _subMeshes = new List<SubMesh>();

            if (_meshQuads.Count == 0 || _subMeshes.Count == 0)
            {
                bool createMeshQuad = (_meshQuads.Count == 0);
                bool createSubMesh = (_subMeshes.Count == 0);

                for (_generateBoneIndex = 0; _generateBoneIndex < mBoneSource.Length; _generateBoneIndex++)
                {
                    if (createMeshQuad)
                        _meshQuads.Add(new MeshQuad(null, -1, -1));

                    if (createSubMesh)
                        _subMeshes.Add(new SubMesh(-1));
                }
            }

            // gather a list of bone quads that will be used in the final mesh.
            // this list also has information about the material used for the bone quad
            // and the depth of the bone

            _meshQuadCount = 0;
            for (_generateBoneIndex = 0; _generateBoneIndex < mBoneSource.Length; _generateBoneIndex++)
            {
                if (mBoneSource[_generateBoneIndex].visible && mBoneSource[_generateBoneIndex].active)
                {
                    // this bone is visible and active,
                    // so we add it to the list to be processed

                    _meshQuad = _meshQuads[_generateBoneIndex];
                    _meshQuad.boneQuad = mBoneSource[_generateBoneIndex].boneQuad;
                    _meshQuad.materialIndex = mBoneSource[_generateBoneIndex].materialIndex;
                    _meshQuad.depth = mBoneSource[_generateBoneIndex].depth;
                    _meshQuadCount++;
                }
                else
                {
                    // this bone is not visible or active, so we set the depth to -1.
                    // this effectively knocks it out of our final list when we sort
                    // by depth descending below.

                    _meshQuads[_generateBoneIndex].depth = -1;
                }
            }

            // sort the depth of the mesh quad list by depth descending
            // (deepest bones will be rendered first)

            if (_sortMeshQuadDepthDescending == null)
                _sortMeshQuadDepthDescending = new SortMeshQuadDepthDescending();

            _meshQuads.Sort(_sortMeshQuadDepthDescending);

            // gather a list of sub meshes for the final mesh.
            // a submesh is a grouping of bone quads that share a material.
            // since we have sorted by depth, there may be many material
            // switches, even among the same materials to get the final
            // mesh to look right. This is why you will sometimes see
            // duplicate materials.

            _lastMaterialIndex = -1;
            _subMeshCount = 0;
            for (_meshQuadIndex = 0; _meshQuadIndex < _meshQuadCount; _meshQuadIndex++)
            {
                _meshQuad = _meshQuads[_meshQuadIndex];

                if (_meshQuad.materialIndex != _lastMaterialIndex && _meshQuad.materialIndex != -1)
                {
                    // the material has changed, so we create a new submesh

                    _subMesh = _subMeshes[_subMeshCount];
                    _subMesh.Reset(_meshQuad.materialIndex);
                    _subMeshCount++;
                }

                // add the bone quad to the current submesh

                if (_subMesh != null)
                    _subMesh.AddBoneQuad(_meshQuad.boneQuad);

                // keep track of the last material index to see if the next
                // bone quad is a change in material

                _lastMaterialIndex = _meshQuad.materialIndex;
            }

            // set the list of materials and the final sub mesh count of the renderer

            mMaterials = new Material[_subMeshCount];
            mMesh.subMeshCount = _subMeshCount;

            // loop through the submeshes, gathering the vertices to pass to the 
            // triangle list for each submesh. The triangle list tells the mesh the
            // ordering of the vertices.

            int[] triangles;
            for (_subMeshIndex = 0; _subMeshIndex < _subMeshCount; _subMeshIndex++)
            {
                _subMesh = _subMeshes[_subMeshIndex];

                // allocate triangle list to the number of bone quads times 6 vertices (3 vertices per triangle).
                // unfortunately we have to do a heap memory allocation here to get the list of triangles since 
                // we need to call SetTriangles and pass a fresh array each time.

                _triangleIndex = 0;
                triangles = new int[_subMesh.boneQuads.Count * 6];

                // set the material for this submesh

                mMaterials[_subMeshIndex] = mMaterialSource[_subMesh.materialIndex];

                // loop through the bone quads for the submeshes to get the triangle list data

                for (int boneQuadIndex = 0; boneQuadIndex < _subMesh.boneQuads.Count; boneQuadIndex++)
                {
                    for (int vertexIndex = 0; vertexIndex < 6; vertexIndex++)
                    {
                        triangles[_triangleIndex] = _subMesh.boneQuads[boneQuadIndex].vertexIndices[vertexIndex];
                        _triangleIndex++;
                    }
                }

                // finally, pass the triangle list and submesh index to the mesh 

                mMesh.SetTriangles(triangles, _subMeshIndex);
            }

            // set the renderer's list of materials

            mRenderer.materials = mMaterials;
        }

        public void ColliderTrigger(ColliderTriggerEvent.TRIGGER_TYPE triggerType, AnimationBone animationBone, Collider otherCollider, string tag)
        {
            _lastColliderTriggerEvent.triggerType = triggerType;
            _lastColliderTriggerEvent.boneAnimation = this;
            _lastColliderTriggerEvent.boneNodeIndex = animationBone.boneNodeIndex;
            _lastColliderTriggerEvent.boneName = mBoneSource[animationBone.boneNodeIndex].boneName;
            _lastColliderTriggerEvent.otherCollider = otherCollider;
            _lastColliderTriggerEvent.otherColliderClosestPointToBone = otherCollider.ClosestPointOnBounds(animationBone.boneTransform.position);
            _lastColliderTriggerEvent.tag = tag;

            int colliderTriggerDelegateIndex;
            for (colliderTriggerDelegateIndex=0; colliderTriggerDelegateIndex < _colliderTriggerDelegates.Count; colliderTriggerDelegateIndex++)
            {
                _colliderTriggerDelegates[colliderTriggerDelegateIndex](_lastColliderTriggerEvent);
            }
        }

        public void Collision(CollisionEvent.COLLISION_TYPE collisionType, AnimationBone animationBone, Collision collision, string tag)
        {
            _lastCollisionEvent.collisionType = collisionType;
            _lastCollisionEvent.boneAnimation = this;
            _lastCollisionEvent.boneNodeIndex = animationBone.boneNodeIndex;
            _lastCollisionEvent.boneName = mBoneSource[animationBone.boneNodeIndex].boneName;
            _lastCollisionEvent.collision = collision;
            _lastCollisionEvent.tag = tag;

            int collisionDelegateIndex;
            for (collisionDelegateIndex=0; collisionDelegateIndex < _collisionDelegates.Count; collisionDelegateIndex++)
            {
                _collisionDelegates[collisionDelegateIndex](_lastCollisionEvent);
            }
        }

        private bool ClipIndexOK(int clipIndex)
        {
            if (clipIndex > -1 && clipIndex < mAnimationClips.Length)
            {
                return true;
            }
            else
            {
                Debug.LogError("Clip Index [" + clipIndex + "] is outside the range [0.." + mAnimationClips.Length.ToString() + "]");
                return false;
            }
        }

        private bool ReplaceAnimationBoneTexture(int clipIndex,
                                            string boneName,
                                            TextureAtlas searchAtlas,
                                            string searchTextureGUID,
                                            TextureAtlas replaceAtlas,
                                            string replaceTextureGUID,
                                            Vector2 replacePivotOffset,
                                            bool replaceUseDefaultPivot)
        {
            int boneNodeIndex = GetBoneNodeIndexFromBoneName(boneName);

            if (boneNodeIndex != -1)
            {
                TriggerFrameBone triggerFrameBone;
                Material searchAtlasMaterial = searchAtlas.material;
                Material replaceAtlasMaterial = replaceAtlas.material;
                bool refire = false;

                if (clipIndex != -1)
                {
                    if (!ClipIndexOK(clipIndex))
                    {
                        return false;
                    }
                }

                int searchMaterialIndex = GetOriginalMaterialIndex(searchAtlasMaterial);
                int replaceMaterialIndex = GetOriginalMaterialIndex(replaceAtlasMaterial);
                if (searchMaterialIndex == -1)
                {
                    Debug.LogError("The material for atlas [" + searchAtlas.name + "] could not be found in this mesh");
                    return false;
                }
                if (replaceMaterialIndex == -1)
                {
                    List<Material> newMaterialList = new List<Material>();
                    for (int matIndex = 0; matIndex < mMaterialSource.Length; matIndex++)
                    {
                        newMaterialList.Add(mMaterialSource[matIndex]);
                    }
                    newMaterialList.Add(replaceAtlasMaterial);
                    mMaterialSource = newMaterialList.ToArray();
                    replaceMaterialIndex = mMaterialSource.Length - 1;
                }

                int searchTextureIndex = searchAtlas.GetTextureIndex(searchTextureGUID);
                int replaceTextureIndex = replaceAtlas.GetTextureIndex(replaceTextureGUID);

                if (searchTextureIndex == -1 || searchTextureIndex >= searchAtlas.uvs.Count)
                {
                    Debug.LogError("Texture [" + searchTextureGUID + "] could not be found in atlas [" + searchAtlas.name + "]");
                    return false;
                }
                if (replaceTextureIndex == -1 || replaceTextureIndex >= replaceAtlas.uvs.Count)
                {
                    Debug.LogError("Texture [" + replaceTextureGUID + "] could not be found in atlas [" + replaceAtlas.name + "]");
                    return false;
                }

                Rect searchUV = searchAtlas.uvs[searchTextureIndex];

                Vector2 newPivotOffset;
                if (replaceUseDefaultPivot)
                {
                    if (replaceAtlas != null)
                    {
                        newPivotOffset = replaceAtlas.LookupDefaultPivotOffset(replaceTextureGUID);
                    }
                    else
                    {
                        newPivotOffset = Vector2.zero;
                    }
                }
                else
                {
                    newPivotOffset = replacePivotOffset;
                }

                Rect lastVisitedUV = _lastVisitedUVDefault;
                int lastVisitedMaterialIndex = -1;
                Vector2 lastVisitedPivotOffset = Vector2.zero;
                bool frameHasMaterialChange = false;
                bool frameHasTextureChange = false;
                bool frameHasPivotChange = false;
                int triggerFrameIndex;
                for (triggerFrameIndex=0; triggerFrameIndex < triggerFrames.Length; triggerFrameIndex++)
                {
                    if (triggerFrames[triggerFrameIndex].clipIndex == clipIndex || clipIndex == -1)
                    {
                        triggerFrameBone = triggerFrames[triggerFrameIndex].GetTriggerFrameBone(boneNodeIndex);

                        if (triggerFrameBone != null)
                        {
                            frameHasMaterialChange = triggerFrameBone.triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.ChangeMaterial);
                            frameHasTextureChange = triggerFrameBone.triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.ChangeTexture);
                            frameHasPivotChange = triggerFrameBone.triggerEventTypes.Contains(TriggerFrameBone.TRIGGER_EVENT_TYPE.ChangePivot);

                            if (frameHasMaterialChange)
                            {
                                lastVisitedMaterialIndex = triggerFrameBone.originalMaterialIndex;
                            }

                            if (frameHasTextureChange)
                            {
                                lastVisitedUV = triggerFrameBone.originalUV;
                            }

                            if (frameHasPivotChange)
                            {
                                lastVisitedPivotOffset = triggerFrameBone.originalPivotOffset;
                            }

                            if (
                                (lastVisitedMaterialIndex == searchMaterialIndex)
                                &&
                                (lastVisitedUV == searchUV)
                                )
                            {
                                if (searchMaterialIndex != replaceMaterialIndex && frameHasMaterialChange)
                                {
                                    triggerFrames[triggerFrameIndex].AddTriggerFrameBoneMaterialChangeNew(boneNodeIndex, replaceMaterialIndex);
                                    triggerFrames[triggerFrameIndex].AddTriggerFrameBoneTextureChangeNew(boneNodeIndex, replaceAtlas, replaceTextureGUID, newPivotOffset, replaceMaterialIndex);
                                    triggerFrames[triggerFrameIndex].AddTriggerFrameBonePivotChangeNew(boneNodeIndex, replaceAtlas, replaceTextureGUID, newPivotOffset);
                                }
                                else if (searchTextureGUID != replaceTextureGUID && frameHasTextureChange)
                                {
                                    triggerFrames[triggerFrameIndex].AddTriggerFrameBoneTextureChangeNew(boneNodeIndex, replaceAtlas, replaceTextureGUID, newPivotOffset, replaceMaterialIndex);
                                    triggerFrames[triggerFrameIndex].AddTriggerFrameBonePivotChangeNew(boneNodeIndex, replaceAtlas, replaceTextureGUID, newPivotOffset);
                                }
                                else if (newPivotOffset != lastVisitedPivotOffset && frameHasPivotChange)
                                {
                                    triggerFrames[triggerFrameIndex].AddTriggerFrameBonePivotChangeNew(boneNodeIndex, replaceAtlas, replaceTextureGUID, newPivotOffset);
                                }

                                refire = true;
                            }
                        }
                    }
                }

                return refire;
            }
            else
            {
                Debug.LogError("Cannot find bone [" + boneName + "] in the animation data");
                return false;
            }
        }

        private bool RestoreAnimationBoneTexture_Internal(int clipIndex,
                                                            string boneName)
        {
            int boneNodeIndex = GetBoneNodeIndexFromBoneName(boneName);

            if (boneNodeIndex != -1)
            {
                TriggerFrameBone triggerFrameBone;
                bool refire = false;
                int triggerFrameIndex;

                if (!ClipIndexOK(clipIndex))
                {
                    return false;
                }

                for (triggerFrameIndex=0; triggerFrameIndex < triggerFrames.Length; triggerFrameIndex++)
                {
                    if (triggerFrames[triggerFrameIndex].clipIndex == clipIndex)
                    {
                        if (triggerFrames[triggerFrameIndex] != null)
                        {
                            triggerFrameBone = triggerFrames[triggerFrameIndex].GetTriggerFrameBone(boneNodeIndex);

                            if (triggerFrameBone != null)
                            {
                                triggerFrames[triggerFrameIndex].RestoreTriggerFrameBone(boneNodeIndex);

                                refire = true;
                            }
                        }
                    }
                }

                return refire;
            }
            else
            {
                Debug.LogError("Cannot find bone [" + boneName + "] in the animation data");
                return false;
            }
        }

        private bool GetSwapTextureNamesFromGUIDs(TextureAtlas searchAtlas, 
                                                    string searchTextureName, 
                                                    TextureAtlas replaceAtlas,
                                                    string replaceTextureName, 
                                                    out string searchTextureGUID, 
                                                    out string replaceTextureGUID)
        {
            searchTextureGUID = searchAtlas.GetTextureGUIDFromName(searchTextureName);
            replaceTextureGUID = replaceAtlas.GetTextureGUIDFromName(replaceTextureName);

            if (searchTextureGUID == "")
            {
                Debug.LogError("Could not find texture [" + searchTextureName + "] in atlas [" + searchAtlas.name + "]");
                return false;
            }

            if (replaceTextureGUID == "")
            {
                Debug.LogError("Could not find texture [" + replaceTextureName + "] in atlas [" + replaceAtlas.name + "]");
                return false;
            }

            return true;
        }
                                            

        /// <summary>
        /// Instead of calling LateUpdate(), we call LateFrameUpdate() from the animation manager singleton.
        /// This cuts down on the amount of reflection Unity has to perform each frame.
        /// </summary>
        public void LateFrameUpdate()
        {
            if (updateColors)
            {
                UpdateColors();
            }
        }

        private void UpdateColors()
        {
            GetHighestLayerPlayingAnimationClip(out _colorBlendHighestClipIndex, out _colorBlendHighestTime);

            for (int bdIndex = 0; bdIndex < mBoneColorAnimations.Length; bdIndex++)
            {
                if (mBoneColorAnimations[bdIndex].Flashing)
                {
                    if (mBoneColorAnimations[bdIndex].FrameUpdate(mMeshColor, Time.deltaTime))
                    {
                        // bone is still flashing
                        SetBoneColor(_colorBlendHighestClipIndex, _colorBlendHighestTime, bdIndex, mBoneColorAnimations[bdIndex].boneColor, mBoneColorAnimations[bdIndex].blendWeight);
                    }
                    else
                    {
                        // bone has stopped flashing
                        if (mBoneColorAnimations[bdIndex].ResetColorOnFinish)
                        {
                            // reset the color to what it was before the flashing
                            SetBoneColor(_colorBlendHighestClipIndex, _colorBlendHighestTime, bdIndex, mBoneColorAnimations[bdIndex].ColorResetTo, mBoneColorAnimations[bdIndex].BlendWeightResetTo);
                        }
                        else
                        {
                            // just leave the color at the last value of the flash
                            SetBoneColor(_colorBlendHighestClipIndex, _colorBlendHighestTime, bdIndex, mBoneColorAnimations[bdIndex].boneColor, mBoneColorAnimations[bdIndex].blendWeight);
                        }
                    }
                }
                else
                {
                    // bone color is static, but we need to be sure that there is no animation color
                    SetBoneColor(_colorBlendHighestClipIndex, _colorBlendHighestTime, bdIndex, mLastBoneColor[bdIndex].color, mLastBoneColor[bdIndex].blendingWeight);
                }
            }
        }

        private void GetHighestLayerPlayingAnimationClip(out int highestClipIndex, out float highestTime)
        {
            _colorBlendHighestLayer = -1;
            highestClipIndex = -1;
            highestTime = -1;

            for (_colorBlendStateIndex = 0; _colorBlendStateIndex < mAnimationStates.Length; _colorBlendStateIndex++)
            {
                _colorBlendAnimationState = mAnimationStates[_colorBlendStateIndex]._realAnimationState;

                if (_colorBlendAnimationState.enabled)
                {
                    _colorBlendAnimationName = _colorBlendAnimationState.name.Replace(" - Queued Clone", "");

                    _colorBlendClipIndex = GetAnimationClipIndex(_colorBlendAnimationName);

                    if (_colorBlendClipIndex != -1)
                    {
                        if (_colorBlendAnimationState.layer > _colorBlendHighestLayer)
                        {
                            _colorBlendHighestLayer = _colorBlendAnimationState.layer;
                            highestClipIndex = _colorBlendClipIndex;

                            switch (_colorBlendAnimationState.wrapMode)
                            {
                                case WrapMode.ClampForever:

                                    highestTime = Mathf.Clamp(_colorBlendAnimationState.time, 0, _colorBlendAnimationState.length);

                                    break;

                                case WrapMode.PingPong:

                                    highestTime = _colorBlendAnimationState.time % _colorBlendAnimationState.length;
                                    if ((Mathf.FloorToInt(_colorBlendAnimationState.normalizedTime) % 2) == 1)
                                    {
                                        highestTime = _colorBlendAnimationState.length - highestTime;
                                    }

                                    break;

                                default:

                                    highestTime = _colorBlendAnimationState.time % _colorBlendAnimationState.length;

                                    break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Could not find animation [" + _colorBlendAnimationName + "] to set bone color");
                        break;
                    }
                }
            }
        }

        private void SetBoneColor(int highestAnimationClipIndex, float highestTime, int boneDataIndex, Color color, float blendingWeight)
        {
            int boneNodeIndex = _boneDataIndexToBoneNodeIndexArray[boneDataIndex];

            if (boneNodeIndex == -1)
                return;

            // clamp the blending weight
            blendingWeight = Mathf.Clamp01(blendingWeight);

            // cache the last bone color
            mLastBoneColor[boneDataIndex].color = color;
            mLastBoneColor[boneDataIndex].blendingWeight = blendingWeight;

            // blend the mesh color with the bone color
            _colorBlendMeshBoneColor = Color.Lerp(mMeshColor, color, blendingWeight);

            _colorBlendFoundBoneAnimationColor = false;
            if (highestAnimationClipIndex != -1)
            {
                _colorBlendFoundBoneAnimationColor = mAnimationClips[highestAnimationClipIndex].EvaluateAnimationBoneColor(boneDataIndex, highestTime, out _colorBlendBoneAnimationColor, out _colorBlendBoneAnimationColorBlendWeight);
            }

            if (_colorBlendFoundBoneAnimationColor)
            {
                // if we found a bone animation color, then blend the blended mesh/bone color with the bone animation color
                _colorBlendFinalColor = Color.Lerp(_colorBlendMeshBoneColor, _colorBlendBoneAnimationColor, _colorBlendBoneAnimationColorBlendWeight);
            }
            else
            {
                // no animation color, so just use the blended mesh/bone color
                _colorBlendFinalColor = _colorBlendMeshBoneColor;
            }

            // update the mesh colors if the final color is different than last frame.
            // this one check cuts the whole frameupdate in half under a stress test!
            if (_colorBlendFinalColor != mLastFinalColor[boneDataIndex])
            {
                SetBoneColor((boneNodeIndex * 4), _colorBlendFinalColor);
                mMesh.colors = mColors;

                mLastFinalColor[boneDataIndex] = _colorBlendFinalColor;
            }
        }

        void OnEnable()
        {
            // if the animation is enabled, either by being created
            // or by being set active, then we add it to the animation
            // manager's queue
            AnimationManager.Instance.AddBoneAnimation(this);
        }

        void OnDisable()
        {
            // Make sure the animation manager hasn't been destroyed to 
            // avoid creating a new instance when the game is shutting down
            if (AnimationManager.Exists)
            {
                // if the animation is disabled, either by being set inactive
                // or being destroyed, then we remove it from the animation
                // manager's queue
                AnimationManager.Instance.RemoveBoneAnimation(this);
            }
        }

        #endregion

        // These functions can be safely called by user scripts.
        // Many of the functions have two versions: one referenced by animation name and 
        // another referenced by the clip index.
        #region Public Functions

        /// <summary>
        /// Returns the bone transform, given the name of the bone
        /// </summary>
        /// <param name="boneName">Name of the bone to return</param>
        /// <returns></returns>
        public Transform GetBoneTransform(string boneName)
        {
            int boneNodeIndex = GetBoneNodeIndexFromBoneName(boneName);

            if (boneNodeIndex == -1)
                return null;

            return mBoneSource[boneNodeIndex].boneTransform;
        }

        /// <summary>
        /// Returns the sprite transform, given the name of the bone. Sprites sit on top of their base bone
        /// to allow scaling without affecting their children.
        /// </summary>
        /// <param name="boneName">Name of the bone which the sprite is attached to</param>
        /// <returns></returns>
        public Transform GetSpriteTransform(string boneName)
        {
            int boneNodeIndex = GetBoneNodeIndexFromBoneName(boneName);

            if (boneNodeIndex == -1)
                return null;

            return mBoneSource[boneNodeIndex].spriteTransform;
        }

        /// <summary>
        /// Request notification of when a user trigger is fired by the mesh to a user script
        /// </summary>
        /// <param name="triggerDelegate">Function in the user script that will handle the user trigger event</param>
        public void RegisterUserTriggerDelegate(UserTriggerDelegate triggerDelegate)
        {
            if (!_userTriggerDelegates.Contains(triggerDelegate))
            {
                _userTriggerDelegates.Add(triggerDelegate);
            }
        }

        /// <summary>
        /// Removes the registration of a user trigger
        /// </summary>
        /// <param name="triggerDelegate">Function in the user script that will handle the user trigger event</param>
        public void UnregisterUserTriggerDelegate(UserTriggerDelegate triggerDelegate)
        {
            if (_userTriggerDelegates.Contains(triggerDelegate))
            {
                _userTriggerDelegates.Remove(triggerDelegate);
            }
        }

        /// <summary>
        /// Request notification of when a collider hits a trigger to a user script
        /// </summary>
        /// <param name="colliderDelegate">Function in the user script that will handle the collider event</param>
        public void RegisterColliderTriggerDelegate(ColliderTriggerDelegate colliderDelegate)
        {
            if (!_colliderTriggerDelegates.Contains(colliderDelegate))
            {
                _colliderTriggerDelegates.Add(colliderDelegate);
            }
        }

        /// <summary>
        /// Removes the registration of a collider trigger
        /// </summary>
        /// <param name="colliderDelegate">Function in the user script that will handle the collider event</param>
        public void UnregisterColliderTriggerDelegate(ColliderTriggerDelegate colliderDelegate)
        {
            if (_colliderTriggerDelegates.Contains(colliderDelegate))
            {
                _colliderTriggerDelegates.Remove(colliderDelegate);
            }
        }

        /// <summary>
        /// Request notification of when a collider causes a collision to a user script
        /// </summary>
        /// <param name="colliderDelegate">Function in the user script that will handle the collision event</param>
        public void RegisterCollisionDelegate(CollisionDelegate collisionDelegate)
        {
            if (!_collisionDelegates.Contains(collisionDelegate))
            {
                _collisionDelegates.Add(collisionDelegate);
            }
        }

        /// <summary>
        /// Removes the registration of a collision delegate
        /// </summary>
        /// <param name="colliderDelegate">Function in the user script that will handle the collision event</param>
        public void UnregisterCollisionDelegate(CollisionDelegate collisionDelegate)
        {
            if (_collisionDelegates.Contains(collisionDelegate))
            {
                _collisionDelegates.Remove(collisionDelegate);
            }
        }

        /// <summary>
        /// Sets the mesh's color
        /// </summary>
        /// <param name="c">color of the mesh</param>
        public void SetMeshColor(Color c)
        {
            mMeshColor = c;
        }

        /// <summary>
        /// Sets a bone's color, overriding the mesh color
        /// </summary>
        /// <param name="boneName">name of the bone</param>
        /// <param name="color">color of the bone</param>
        public void SetBoneColor(string boneName, Color color, float blendingWeight)
        {
            int boneDataIndex = GetBoneDataIndexFromBoneName(boneName);

            if (boneDataIndex == -1)
                return;

            GetHighestLayerPlayingAnimationClip(out _colorBlendHighestClipIndex, out _colorBlendHighestTime);

            SetBoneColor(_colorBlendHighestClipIndex, _colorBlendHighestTime, boneDataIndex, color, blendingWeight);
        }

        /// <summary>
        /// Flashes a bone from a starting color and weight to an ending color and weight
        /// over a period of time with a number of durations
        /// </summary>
        /// <param name="boneName">Name of the bone to flash</param>
        /// <param name="startingColor">The color to start the flash at</param>
        /// <param name="startingBlendWeight">The weight of the blending for the color to start at</param>
        /// <param name="endingColor">The color for the flash to end on</param>
        /// <param name="endingBlendWeight">The weight of the blending for the color to end on</param>
        /// <param name="duration">The duration of each flash</param>
        /// <param name="iterations">The number of flashes</param>
        /// <param name="resetColorOnFinish">Whether or not to reset the bone color back to what it was before it started flashing</param>
        public void FlashBoneColor(string boneName,
                                    Color startingColor,
                                    float startingBlendWeight,
                                    Color endingColor,
                                    float endingBlendWeight,
                                    float duration,
                                    int iterations,
                                    bool resetColorOnFinish)
        {
            int boneDataIndex = GetBoneDataIndexFromBoneName(boneName);

            if (boneDataIndex == -1)
                return;

            // start the flash animation
            int boneNodeIndex = _boneDataIndexToBoneNodeIndexArray[boneDataIndex];

            // Get the last bone color to use in case reset color is true
            BoneColor lastBoneColor = mBoneSource[boneNodeIndex].boneColor;

            // if this bone is already flashing, then we don't want to overwrite the original color before flashing, so we'll get what has been stored previously
            if (mBoneColorAnimations[boneDataIndex].Flashing)
            {
                lastBoneColor.color = mBoneColorAnimations[boneDataIndex].ColorResetTo;
                lastBoneColor.blendingWeight = mBoneColorAnimations[boneDataIndex].BlendWeightResetTo;
            }

            mBoneColorAnimations[boneDataIndex].Reset(startingColor, startingBlendWeight, endingColor, endingBlendWeight, duration, iterations, resetColorOnFinish, lastBoneColor.color, lastBoneColor.blendingWeight);

            // set the initial color of the flash if is has time left
            if (mBoneColorAnimations[boneDataIndex].Flashing)
            {
                SetBoneColor(boneName, startingColor, startingBlendWeight);
            }
        }

        /// <summary>
        /// Halts the flashing of a bone
        /// </summary>
        /// <param name="boneName">The name of the bone to stop flashing</param>
        public void StopFlashingBoneColor(string boneName)
        {
            int boneDataIndex = GetBoneDataIndexFromBoneName(boneName);

            if (boneDataIndex == -1)
                return;

            // Stop the flashing
            mBoneColorAnimations[boneDataIndex].Stop();

            if (mBoneColorAnimations[boneDataIndex].ResetColorOnFinish)
            {
                // reset the bone color back to what it was before it started flashing
                SetBoneColor(boneName, mBoneColorAnimations[boneDataIndex].ColorResetTo, mBoneColorAnimations[boneDataIndex].BlendWeightResetTo);
            }
            else
            {
                // just set the bone color to the final color of the flash
                SetBoneColor(boneName, mBoneColorAnimations[boneDataIndex].boneColor, mBoneColorAnimations[boneDataIndex].blendWeight);
            }
        }

        /// <summary>
        /// Stops flashing of bone colors on all bones
        /// </summary>
        public void StopAllFlashingBoneColors()
        {
            int boneNodeIndex = 0;
            for (int boneDataIndex = 0; boneDataIndex < mBoneColorAnimations.Length; boneDataIndex++)
            {
                boneNodeIndex = _boneDataIndexToBoneNodeIndexArray[boneDataIndex];

                // only stop if the bone color is flashing
                if (mBoneColorAnimations[boneDataIndex].Flashing)
                {
                    mBoneColorAnimations[boneDataIndex].Stop();
                    if (mBoneColorAnimations[boneDataIndex].ResetColorOnFinish)
                    {
                        // reset the bone color back to what it was before it started flashing
                        SetBoneColor(mBoneSource[boneNodeIndex].boneName, mBoneColorAnimations[boneDataIndex].ColorResetTo, mBoneColorAnimations[boneDataIndex].BlendWeightResetTo);
                    }
                    else
                    {
                        // just set the bone color to the final color of the flash
                        SetBoneColor(mBoneSource[boneNodeIndex].boneName, mBoneColorAnimations[boneDataIndex].boneColor, mBoneColorAnimations[boneDataIndex].blendWeight);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the bone color back to what it was in the animation editor
        /// </summary>
        /// <param name="boneName">The name of the bone to set the color on</param>
        public void RestoreOriginalBoneColor(string boneName)
        {
            int boneDataIndex = GetBoneDataIndexFromBoneName(boneName);

            if (boneDataIndex == -1)
                return;

            // stop any bone color animations
            mBoneColorAnimations[boneDataIndex].Stop();

            // set the original animation bone color
            int boneNodeIndex = _boneDataIndexToBoneNodeIndexArray[boneDataIndex];
            BoneColor boneColor = mBoneSource[boneNodeIndex].boneColor;
            SetBoneColor(boneName, boneColor.color, boneColor.blendingWeight);
        }

        /// <summary>
        /// Sets all the bones in the mesh back to the colors that were originally assigned in the editor
        /// </summary>
        public void RestoreAllOriginalBoneColors()
        {
            int boneNodeIndex = 0;
            for (int boneDataIndex = 0; boneDataIndex < mBoneColorAnimations.Length; boneDataIndex++)
            {
                boneNodeIndex = _boneDataIndexToBoneNodeIndexArray[boneDataIndex];

                // set the original animation bone color
                mBoneColorAnimations[boneDataIndex].Stop();
                SetBoneColor(mBoneSource[boneNodeIndex].boneName, mBoneSource[boneNodeIndex].boneColor.color, mBoneSource[boneNodeIndex].boneColor.blendingWeight);

                boneDataIndex++;
            }
        }

        /// <summary>
        /// Stop playing animation
        /// </summary>
        public void Stop()
        {
            mAnimation.Stop();
        }

        /// <summary>
        /// Stop playing a specific animation
        /// </summary>
        /// <param name="animationName">Name of the animation to stop</param>
        public void Stop(string animationName)
        {
            mAnimation.Stop(animationName);
        }

        /// <summary>
        /// Stop playing a specific animation
        /// </summary>
        /// <param name="clipIndex">Clip index of the animation to stop</param>
        public void Stop(int clipIndex)
        {
            if (ClipIndexOK(clipIndex))
            {
                mAnimation.Stop(GetAnimationClipName(clipIndex));
            }
        }

        /// <summary>
        /// Rewind the animation
        /// </summary>
        public void Rewind()
        {
            mAnimation.Rewind();
        }

        /// <summary>
        /// Rewind a specific animation
        /// </summary>
        /// <param name="animationName">Name of the animation to rewind</param>
        public void Rewind(string animationName)
        {
            mAnimation.Rewind(animationName);
        }

        /// <summary>
        /// Rewind a specific animation
        /// </summary>
        /// <param name="clipIndex">Clip index of the animation to rewind</param>
        public void Rewind(int clipIndex)
        {
            if (ClipIndexOK(clipIndex))
            {
                mAnimation.Rewind(GetAnimationClipName(clipIndex));
            }
        }

        /// <summary>
        /// Sample the animation
        /// </summary>
        public void Sample()
        {
            mAnimation.Sample();
        }

        /// <summary>
        /// Returns true if the specific animation is playing
        /// </summary>
        /// <param name="animationName">Name of the animation to test</param>
        /// <returns></returns>
        public bool IsPlaying(string animationName)
        {
            return mAnimation.IsPlaying(animationName);
        }

        /// <summary>
        /// Returns true if the specific animation is playing
        /// </summary>
        /// <param name="clipIndex">Clip index of the animation to test</param>
        /// <returns></returns>
        public bool IsPlaying(int clipIndex)
        {
            if (ClipIndexOK(clipIndex))
            {
                return mAnimation.IsPlaying(GetAnimationClipName(clipIndex));
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the index of the animation clip, given the name. If no animation
        /// exists, -1 is returned.
        /// </summary>
        /// <param name="animationName">The name of the animation clip</param>
        /// <returns></returns>
        public int GetAnimationClipIndex(string animationName)
        {
            for (_animationClipIndex_internal = 0; _animationClipIndex_internal < mAnimationClips.Length; _animationClipIndex_internal++)
            {
                if (mAnimationClips[_animationClipIndex_internal].animationName == animationName)
                    return _animationClipIndex_internal;
            }

            return -1;
        }

        /// <summary>
        /// Gets the animation clip name, given the index. If no animation exists,
        /// a blank string ("") is returned.
        /// </summary>
        /// <param name="index">The index of the animation clip</param>
        /// <returns></returns>
        public string GetAnimationClipName(int index)
        {
            if (index >= 0 && index < mAnimationClips.Length)
            {
                return mAnimationClips[index].animationName;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Returns whether the animation clips exists, given the name
        /// </summary>
        /// <param name="animationName">The name of the animation clip</param>
        /// <returns></returns>
        public bool AnimationClipExists(string animationName)
        {
            return (GetAnimationClipIndex(animationName) != -1);
        }

        /// <summary>
        /// Returns whether the animation clip exists, given the index
        /// </summary>
        /// <param name="animationClipIndex">The index of the animation clip</param>
        /// <returns></returns>
        public bool AnimationClipExists(int animationClipIndex)
        {
            return (animationClipIndex > -1 && animationClipIndex < mAnimationClips.Length);
        }

        /// <summary>
        /// Play the default animation
        /// </summary>
        /// <returns></returns>
        public bool Play()
        {
            bool success = mAnimation.Play();
            return success;
        }

        /// <summary>
        /// Play the default animation with a specific playmode
        /// </summary>
        /// <param name="mode">The playmode to use while playing</param>
        /// <returns></returns>
        public bool Play(PlayMode mode)
        {
            bool success = mAnimation.Play(mode);
            return success;
        }

        /// <summary>
        /// Play a specific animation
        /// </summary>
        /// <param name="animationName">Name of the aniation to play</param>
        /// <returns></returns>
        public bool Play(string animationName)
        {
            bool success = mAnimation.Play(animationName);
            return success;
        }

        /// <summary>
        /// Play a specific animation with a play mode
        /// </summary>
        /// <param name="animationName">Name of the animation to play</param>
        /// <param name="mode">The playmode to use while playing</param>
        /// <returns></returns>
        public bool Play(string animationName, PlayMode mode)
        {
            bool success = mAnimation.Play(animationName, mode);
            return success;
        }

        /// <summary>
        /// Plays a specific animation
        /// </summary>
        /// <param name="clipIndex">Clip index of the animation to play</param>
        /// <returns></returns>
        public bool Play(int clipIndex)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                bool success = mAnimation.Play(animationName);
                return success;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Plays a specific animation with a play mode
        /// </summary>
        /// <param name="clipIndex">Clip index of the animation to play</param>
        /// <param name="mode">The playmode to use while playing</param>
        /// <returns></returns>
        public bool Play(int clipIndex, PlayMode mode)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                bool success = mAnimation.Play(GetAnimationClipName(clipIndex));
                return success;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Crossfades an animation with what is currently playing
        /// </summary>
        /// <param name="animationName">Name of the animation to crossfade</param>
        public void CrossFade(string animationName)
        {
            mAnimation.CrossFade(animationName);
        }

        /// <summary>
        /// Crossfades an animation with what is currently playing
        /// </summary>
        /// <param name="animationName">Name of animation to crossfade</param>
        /// <param name="fadeLength">Length of time to fade in</param>
        public void CrossFade(string animationName, float fadeLength)
        {
            mAnimation.CrossFade(animationName, fadeLength);
        }

        /// <summary>
        /// Crossfades an animation with what is currently playing
        /// </summary>
        /// <param name="animationName">Name of the animation to crossfade</param>
        /// <param name="fadeLength">Length of time to fade int</param>
        /// <param name="mode">The playmode to use while playing</param>
        public void CrossFade(string animationName, float fadeLength, PlayMode mode)
        {
            mAnimation.CrossFade(animationName, fadeLength, mode);
        }

        /// <summary>
        /// Crossfades an animation with what is currently playing
        /// </summary>
        /// <param name="animationName">Clip index of the animation to crossfade</param>
        public void CrossFade(int clipIndex)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                mAnimation.CrossFade(animationName);
            }
        }

        /// <summary>
        /// Crossfades an animation with what is currently playing
        /// </summary>
        /// <param name="animationName">Clip index of animation to crossfade</param>
        /// <param name="fadeLength">Length of time to fade in</param>
        public void CrossFade(int clipIndex, float fadeLength)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                mAnimation.CrossFade(animationName, fadeLength);
            }
        }

        /// <summary>
        /// Crossfades an animation with what is currently playing
        /// </summary>
        /// <param name="animationName">Clip index of the animation to crossfade</param>
        /// <param name="fadeLength">Length of time to fade int</param>
        /// <param name="mode">The playmode to use while playing</param>
        public void CrossFade(int clipIndex, float fadeLength, PlayMode mode)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                mAnimation.CrossFade(animationName, fadeLength, mode);
            }
        }

        /// <summary>
        /// Blends an animation
        /// </summary>
        /// <param name="animationName">Name of animation to blend</param>
        public void Blend(string animationName)
        {
            mAnimation.Blend(animationName);
        }

        /// <summary>
        /// Blends an animation towards target/weight
        /// </summary>
        /// <param name="animationName">Name of animation to blend</param>
        /// <param name="targetWeight">Weight to blend toward</param>
        public void Blend(string animationName, float targetWeight)
        {
            mAnimation.Blend(animationName, targetWeight);
        }


        /// <summary>
        /// Blends an animation towards target/weight over fadelength
        /// </summary>
        /// <param name="animationName">Name of the animation to blend</param>
        /// <param name="targetWeight">Weight to blend toward</param>
        /// <param name="fadeLength">Time required to blend</param>
        public void Blend(string animationName, float targetWeight, float fadeLength)
        {
            mAnimation.Blend(animationName, targetWeight, fadeLength);
        }

        /// <summary>
        /// Blends an animation
        /// </summary>
        /// <param name="animationName">Clip index of animation to blend</param>
        public void Blend(int clipIndex)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                mAnimation.Blend(animationName);
            }
        }

        /// <summary>
        /// Blends an animation towards target/weight
        /// </summary>
        /// <param name="animationName">Clip index of animation to blend</param>
        /// <param name="targetWeight">Weight to blend toward</param>
        public void Blend(int clipIndex, float targetWeight)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                mAnimation.Blend(animationName, targetWeight);
            }
        }

        /// <summary>
        /// Blends an animation towards target/weight over fadelength
        /// </summary>
        /// <param name="animationName">Clip index of the animation to blend</param>
        /// <param name="targetWeight">Weight to blend toward</param>
        /// <param name="fadeLength">Time required to blend</param>
        public void Blend(int clipIndex, float targetWeight, float fadeLength)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                mAnimation.Blend(animationName, targetWeight, fadeLength);
            }
        }

        /// <summary>
        /// Crossfades animation after current animations finish
        /// </summary>
        /// <param name="animationName">Name of animation to crossfadequeue</param>
        /// <returns></returns>
        public AnimationState CrossFadeQueued(string animationName)
        {
            AnimationState state = mAnimation.CrossFadeQueued(animationName);
            if (state != null)
            {
                state.speed = mAnimation[animationName].speed;
            }
            return state;
        }

        /// <summary>
        /// Crossfades animation after current animations finish over fadelength time
        /// </summary>
        /// <param name="animationName">Name of animation to crossfadequeue</param>
        /// <param name="fadeLength">Length of time to fade</param>
        /// <returns></returns>
        public AnimationState CrossFadeQueued(string animationName, float fadeLength)
        {
            AnimationState state = mAnimation.CrossFadeQueued(animationName, fadeLength);
            if (state != null)
            {
                state.speed = mAnimation[animationName].speed;
            }
            return state;
        }

        /// <summary>
        /// Crossfades animation in queue mode over fadelength time
        /// </summary>
        /// <param name="animationName">Name of animation to crossfadequeue</param>
        /// <param name="fadeLength">Length of time to fade</param>
        /// <param name="queueMode">Play now or later</param>
        /// <returns></returns>
        public AnimationState CrossFadeQueued(string animationName, float fadeLength, QueueMode queueMode)
        {
            AnimationState state = mAnimation.CrossFadeQueued(animationName, fadeLength, queueMode);
            if (state != null)
            {
                state.speed = mAnimation[animationName].speed;
            }
            return state;
        }

        /// <summary>
        /// Crossfades animation in queue mode over time with a play mode
        /// </summary>
        /// <param name="animationName">Name of animation to crossfadequeue</param>
        /// <param name="fadeLength">Length of time to fade</param>
        /// <param name="queueMode">Play now or later</param>
        /// <param name="mode">The playmode to use while playing</param>
        /// <returns></returns>
        public AnimationState CrossFadeQueued(string animationName, float fadeLength, QueueMode queueMode, PlayMode mode)
        {
            AnimationState state = mAnimation.CrossFadeQueued(animationName, fadeLength, queueMode, mode);
            if (state != null)
            {
                state.speed = mAnimation[animationName].speed;
            }
            return state;
        }

        /// <summary>
        /// Crossfades animation after current animations finish
        /// </summary>
        /// <param name="animationName">Clip index of animation to crossfadequeue</param>
        /// <returns></returns>
        public AnimationState CrossFadeQueued(int clipIndex)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                AnimationState state = mAnimation.CrossFadeQueued(animationName);
                if (state != null)
                {
                    state.speed = mAnimation[animationName].speed;
                }
                return state;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Crossfades animation after current animations finish over fadelength time
        /// </summary>
        /// <param name="animationName">Clip index of animation to crossfadequeue</param>
        /// <param name="fadeLength">Length of time to fade</param>
        /// <returns></returns>
        public AnimationState CrossFadeQueued(int clipIndex, float fadeLength)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                AnimationState state = mAnimation.CrossFadeQueued(animationName, fadeLength);
                if (state != null)
                {
                    state.speed = mAnimation[animationName].speed;
                }
                return state;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Crossfades animation in queue mode over fadelength time
        /// </summary>
        /// <param name="animationName">Clip index of animation to crossfadequeue</param>
        /// <param name="fadeLength">Length of time to fade</param>
        /// <param name="queueMode">Play now or later</param>
        /// <returns></returns>
        public AnimationState CrossFadeQueued(int clipIndex, float fadeLength, QueueMode queueMode)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                AnimationState state = mAnimation.CrossFadeQueued(animationName, fadeLength, queueMode);
                if (state != null)
                {
                    state.speed = mAnimation[animationName].speed;
                }
                return state;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Crossfades animation in queue mode over time with a play mode
        /// </summary>
        /// <param name="animationName">Clip index of animation to crossfadequeue</param>
        /// <param name="fadeLength">Length of time to fade</param>
        /// <param name="queueMode">Play now or later</param>
        /// <param name="mode">The playmode to use while playing</param>
        /// <returns></returns>
        public AnimationState CrossFadeQueued(int clipIndex, float fadeLength, QueueMode queueMode, PlayMode mode)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                AnimationState state = mAnimation.CrossFadeQueued(animationName, fadeLength, queueMode, mode);
                if (state != null)
                {
                    state.speed = mAnimation[animationName].speed;
                }
                return state;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Plays an animation after queue is finished
        /// </summary>
        /// <param name="animationName">Name of animation to play</param>
        /// <returns></returns>
        public AnimationState PlayQueued(string animationName)
        {
            AnimationState state = mAnimation.PlayQueued(animationName);
            if (state != null)
            {
                state.speed = mAnimation[animationName].speed;
            }
            return state;
        }

        /// <summary>
        /// Plays an animation following the queue mode
        /// </summary>
        /// <param name="animationName">Name of animation to play</param>
        /// <param name="queueMode">Play now or later</param>
        /// <returns></returns>
        public AnimationState PlayQueued(string animationName, QueueMode queueMode)
        {
            AnimationState state = mAnimation.PlayQueued(animationName, queueMode);
            if (state != null)
            {
                state.speed = mAnimation[animationName].speed;
            }
            return state;
        }

        /// <summary>
        /// Plays an animation following a queue mode with a play mode
        /// </summary>
        /// <param name="animationName">Name of animation to play</param>
        /// <param name="queueMode">Play now or later</param>
        /// <param name="mode">The play mode to use while playing</param>
        /// <returns></returns>
        public AnimationState PlayQueued(string animationName, QueueMode queueMode, PlayMode mode)
        {
            AnimationState state = mAnimation.PlayQueued(animationName, queueMode, mode);
            if (state != null)
            {
                state.speed = mAnimation[animationName].speed;
            }
            return state;
        }

        /// <summary>
        /// Plays an animation after queue is finished
        /// </summary>
        /// <param name="animationName">Clip index of animation to play</param>
        /// <returns></returns>
        public AnimationState PlayQueued(int clipIndex)
        {
            string animationName = GetAnimationClipName(clipIndex);
            AnimationState state = mAnimation.PlayQueued(animationName);
            if (state != null)
            {
                state.speed = mAnimation[animationName].speed;
            }
            return state;
        }

        /// <summary>
        /// Plays an animation following the queue mode
        /// </summary>
        /// <param name="animationName">Clip index of animation to play</param>
        /// <param name="queueMode">Play now or later</param>
        /// <returns></returns>
        public AnimationState PlayQueued(int clipIndex, QueueMode queueMode)
        {
            string animationName = GetAnimationClipName(clipIndex);
            AnimationState state = mAnimation.PlayQueued(animationName, queueMode);
            if (state != null)
            {
                state.speed = mAnimation[animationName].speed;
            }
            return state;
        }

        /// <summary>
        /// Plays an animation following the queue mode
        /// </summary>
        /// <param name="animationName">Clip index of animation to play</param>
        /// <param name="queueMode">Play now or later</param>
        /// <returns></returns>
        public AnimationState PlayQueued(int clipIndex, QueueMode queueMode, PlayMode mode)
        {
            if (ClipIndexOK(clipIndex))
            {
                string animationName = GetAnimationClipName(clipIndex);
                AnimationState state = mAnimation.PlayQueued(animationName, queueMode, mode);
                if (state != null)
                {
                    state.speed = mAnimation[animationName].speed;
                }
                return state;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the clip count of the Animation object
        /// </summary>
        /// <returns></returns>
        public int GetClipCount()
        {
            return mAnimation.GetClipCount();
        }

        /// <summary>
        /// Synchronizes playback speed of all animations in the layer
        /// </summary>
        /// <param name="layer">Layer to synchronize</param>
        public void SyncLayer(int layer)
        {
            mAnimation.SyncLayer(layer);
        }

        /// <summary>
        /// Adds a clip to the animation. This probabably won't be used since the animations
        /// are added through the editor
        /// </summary>
        /// <param name="clip">Animation clip to add</param>
        /// <param name="newName">Name of the clip</param>
        /// <param name="index">Index of the clip</param>
        public void AddClip(AnimationClip clip, string newName)
        {
            mAnimation.AddClip(clip, newName);
        }

        /// <summary>
        /// Adds a clip to the animation. This probabably won't be used since the animations
        /// are added through the editor
        /// </summary>
        /// <param name="clip">Animation clip to add</param>
        /// <param name="newName">Name of the clip</param>
        /// <param name="firstFrame">First frame of the clip</param>
        /// <param name="lastFrame">Last frame of the clip</param>
        /// <param name="addLoopFrame">Whether or not the clip should have an extra frame for looping</param>
        public void AddClip(AnimationClip clip, string newName, int firstFrame, int lastFrame, bool addLoopFrame)
        {
            mAnimation.AddClip(clip, newName, firstFrame, lastFrame, addLoopFrame);
        }

        /// <summary>
        /// Removes an animation clip from the Animation object
        /// </summary>
        /// <param name="clip">Animation clip to remove</param>
        public void RemoveClip(AnimationClip clip)
        {
            mAnimation.RemoveClip(clip);
        }

        /// <summary>
        /// Swaps one material for another in a mesh. Note that the textures need to be in
        /// the same position with the same size in both atlases, or your mesh will not look
        /// right. This may mean that you have to allot extra space for textures in one atlas, 
        /// even though that space is only used in the swapped atlas.
        /// </summary>
        /// <param name="existingMaterial"></param>
        /// <param name="newMaterial"></param>
        public void SwapMaterial(Material originalMaterial, Material newMaterial)
        {
            int originalMaterialIndex = GetOriginalMaterialIndex(originalMaterial);

            if (originalMaterialIndex == -1)
            {
                Debug.LogError("Could not find [" + originalMaterial + "] in the original list of materials");
                return;
            }

            mMaterialSource[originalMaterialIndex] = newMaterial;

            GenerateMesh();
        }

        /// <summary>
        /// Restores swapped materials to the original state, yet leaves newly added materials intact
        /// </summary>
        public void RestoreOriginalMaterials()
        {
            int originalMaterialIndex;
            for (originalMaterialIndex = 0; originalMaterialIndex < _originalMaterialArray.Length; originalMaterialIndex++)
            {
                mMaterialSource[originalMaterialIndex] = _originalMaterialArray[originalMaterialIndex];
            }

            GenerateMesh();
        }

        /// <summary>
        /// Hides a bone in the animation until it is manually shown again by calling HideBone with false 
        /// </summary>
        /// <param name="boneName">
        /// Name of the bone to hide <see cref="System.String"/>
        /// </param>
        /// <param name="hide">
        /// Whether or not to hide the bone (false will show the bone again)
        /// </param>
        public void HideBone(string boneName, bool hide)
        {
            int boneNodeIndex = GetBoneNodeIndexFromBoneName(boneName);

            if (boneNodeIndex != -1)
            {
                AnimationBone bone = mBoneSource[boneNodeIndex];
                bone.active = !hide;

                GenerateMesh();
            }
            else
            {
                Debug.LogError("Cannot find bone [" + boneName + "] in the animation data");
                return;
            }
        }

        /// <summary>
        /// Returns whether a specified bone has been hidden manually or not with HideBone
        /// </summary>
        /// <param name="boneName">The name of the bone to check</param>
        /// <returns></returns>
        public bool IsBoneHidden(string boneName)
        {
            int boneNodeIndex = GetBoneNodeIndexFromBoneName(boneName);

            if (boneNodeIndex != -1)
            {
                AnimationBone bone = mBoneSource[boneNodeIndex];
                return !bone.active;
            }
            else
            {
                Debug.LogError("Cannot find bone [" + boneName + "] in the animation data");
                return false;
            }
        }

        /// <summary>
        /// Attached a transform to a bone with local position, rotation, and scale settings 
        /// </summary>
        /// <param name="transform">
        /// Transform to attach to the bone
        /// </param>
        /// <param name="boneName">
        /// Name of the bone to attach the transform to
        /// </param>
        /// <param name="localPosition">
        /// Local position of the transform under the bone
        /// </param>
        /// <param name="localRotation">
        /// Local rotation of the transform under the bone
        /// </param>
        /// <param name="localScale">
        /// Local scale of the transform under the bone
        /// </param>
        public void AttachTransform(Transform transform, string boneName, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            if (transform == null)
            {
                Debug.LogError("No transform to attach to [" + boneName + "]");
            }

            Transform boneTransform = GetBoneTransform(boneName);
            if (boneTransform != null)
            {
                transform.parent = boneTransform;
                transform.localPosition = localPosition;
                transform.localRotation = localRotation;
                transform.localScale = localScale;
            }
            else
            {
                Debug.LogError("Bone name [" + boneName + "] not found");
                return;
            }
        }

        /// <summary>
        /// Attaches a transform to a bone, setting the child transform to zero offset, zero rotation, 
        /// and scale of one relative to the bone 
        /// </summary>
        /// <param name="transform">
        /// Transform to attach to the bone
        /// </param>
        /// <param name="boneName">
        /// Name of the bone to attach the transform to
        /// </param>
        public void AttachTransform(Transform transform, string boneName)
        {
            AttachTransform(transform, boneName, Vector3.zero, Quaternion.identity, Vector3.one);
        }

        /// <summary>
        /// Restores the textures set in the animation editor for this bone in all animation clips
        /// </summary>
        /// <param name="boneName">The name of the bone to restore the texture</param>
        public void RestoreBoneTexture(string boneName)
        {
            bool refire = false;

            for (int clipIndex = 0; clipIndex < mAnimationClips.Length; clipIndex++)
            {
                if (RestoreAnimationBoneTexture_Internal(clipIndex,
                                                           boneName
                                                           ))
                {
                    refire = true;
                }
            }

            if (refire)
            {
                if (FireLastTextureAnimationEvent(boneName))
                {
                    GenerateMesh();
                }
            }
        }

        /// <summary>
        /// Restores the textures set in the animation editor for this bone and animation clip
        /// </summary>
        /// <param name="clipIndex">The index of the animation to restore</param>
        /// <param name="boneName">The name of the bone to restore</param>
        public void RestoreAnimationBoneTexture(int clipIndex,
                                            string boneName)
        {
            if (RestoreAnimationBoneTexture_Internal(clipIndex,
                                        boneName))
            {
                if (IsPlaying(clipIndex))
                {
                    if (FireLastTextureAnimationEvent(boneName))
                    {
                        GenerateMesh();
                    }
                }
            }
        }

        /// <summary>
        /// Restores the textures set in the animation editor for this bone and animation clip
        /// </summary>
        /// <param name="animationName">The name of the animation to restore</param>
        /// <param name="boneName">The name of the bone to restore</param>
        public void RestoreAnimationBoneTexture(string animationName,
                                            string boneName)
        {
            int clipIndex = GetAnimationClipIndex(animationName);

            if (clipIndex == -1)
            {
                Debug.LogError("Animation [" + animationName + "] not found");
                return;
            }

            if (RestoreAnimationBoneTexture_Internal(clipIndex, boneName))
            {
                if (IsPlaying(animationName))
                {
                    if (FireLastTextureAnimationEvent(boneName))
                    {
                        GenerateMesh();
                    }
                }
            }
        }

        /// <summary>
        /// Restores the textures set in the animation editor for this animation clip
        /// </summary>
        /// <param name="animationName">The name of the animation to restore</param>
        public void RestoreAnimationTexture(int clipIndex)
        {
            int boneNodeIndex;

            for (boneNodeIndex=0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                if (RestoreAnimationBoneTexture_Internal(clipIndex, mBoneSource[boneNodeIndex].boneName))
                {
                    _refireList[boneNodeIndex] = true;
                }
                else
                {
                    _refireList[boneNodeIndex] = false;
                }
            }

            if (IsPlaying(clipIndex))
            {
                bool regenerateMesh = false;
                for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
                {
                    if (_refireList[boneNodeIndex])
                    {
                        if (FireLastTextureAnimationEvent(mBoneSource[boneNodeIndex].boneName))
                        {
                            regenerateMesh = true;
                        }
                    }
                }

                if (regenerateMesh)
                    GenerateMesh();
            }
        }

        /// <summary>
        /// Restores the textures set in the animation editor for this animation clip
        /// </summary>
        /// <param name="animationName">The name of the animation to restore</param>
        public void RestoreAnimationTexture(string animationName)
        {
            int clipIndex = GetAnimationClipIndex(animationName);

            if (clipIndex == -1)
            {
                Debug.LogError("Animation [" + animationName + "] not found");
                return;
            }

            int boneNodeIndex;

            for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                if (RestoreAnimationBoneTexture_Internal(clipIndex, mBoneSource[boneNodeIndex].boneName))
                {
                    _refireList[boneNodeIndex] = true;
                }
                else
                {
                    _refireList[boneNodeIndex] = false;
                }
            }

            if (IsPlaying(animationName))
            {
                bool regenerateMesh = false;
                for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
                {
                    if (_refireList[boneNodeIndex])
                    {
                        if (FireLastTextureAnimationEvent(mBoneSource[boneNodeIndex].boneName))
                        {
                            regenerateMesh = true;
                        }
                    }
                }

                if (regenerateMesh)
                    GenerateMesh();
            }
        }

        /// <summary>
        /// Restores the textures set in the animation editor for all animation clips and all bones
        /// </summary>
        public void RestoreTextures()
        {
            int boneNodeIndex;

            for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                _refireList[boneNodeIndex] = false;
            }

            for (int clipIndex = 0; clipIndex < mAnimationClips.Length; clipIndex++)
            {
                for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
                {
                    if (RestoreAnimationBoneTexture_Internal(clipIndex, mBoneSource[boneNodeIndex].boneName))
                    {
                        _refireList[boneNodeIndex] = true;
                    }
                }
            }

            bool regenerateMesh = false;
            for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                if (_refireList[boneNodeIndex])
                {
                    if (FireLastTextureAnimationEvent(mBoneSource[boneNodeIndex].boneName))
                    {
                        regenerateMesh = true;
                    }
                }
            }

            if (regenerateMesh)
                GenerateMesh();
        }

        /// <summary>
        /// Swaps a texture for an animation and bone
        /// </summary>
        /// <param name="animationName">Name of the animation to search</param>
        /// <param name="boneName">Name of the bone to search</param>
        /// <param name="searchAtlasName">Name of atlas to search for</param>
        /// <param name="searchTextureName">Texture name to search for</param>
        /// <param name="replaceAtlasName">Name of atlas to replace</param>
        /// <param name="replaceTextureName">Texture name to replace</param>
        /// <param name="replacePivotOffset">Pivot offset of the new texture</param>
        public void SwapAnimationBoneTexture(string animationName,
                                            string boneName,
                                            string searchAtlasName,
                                            string searchTextureName,
                                            string replaceAtlasName,
                                            string replaceTextureName,
                                            Vector2 replacePivotOffset)
        {
            int clipIndex = GetAnimationClipIndex(animationName);

            if (clipIndex == -1)
            {
                Debug.LogError("Animation [" + animationName + "] not found");
                return;
            }

            TextureAtlas searchAtlas = GetTextureAtlasFromName(searchAtlasName);
            TextureAtlas replaceAtlas = GetTextureAtlasFromName(replaceAtlasName);

            if (searchAtlas == null)
            {
                Debug.LogError("Atlas [" + searchAtlasName + "] not found");
                return;
            }
            if (replaceAtlas == null)
            {
                Debug.LogError("Atlas [" + replaceAtlasName + "] not found");
                return;
            }

            string searchTextureGUID = "";
            string replaceTextureGUID = "";

            GetSwapTextureNamesFromGUIDs(searchAtlas, searchTextureName, replaceAtlas, replaceTextureName, out searchTextureGUID, out replaceTextureGUID);

            if (ReplaceAnimationBoneTexture(clipIndex,
                                                boneName,
                                                searchAtlas,
                                                searchTextureGUID,
                                                replaceAtlas,
                                                replaceTextureGUID,
                                                replacePivotOffset,
                                                false))
            {
                if (IsPlaying(animationName))
                {
                    if (FireLastTextureAnimationEvent(boneName))
                    {
                        GenerateMesh();
                    }
                }
            }
        }

        /// <summary>
        /// Swaps a texture for an animation and bone
        /// </summary>
        /// <param name="animationName">Name of animation to search</param>
        /// <param name="boneName">Name of bone to search</param>
        /// <param name="searchAtlasName">Name of atlas to search for</param>
        /// <param name="searchTextureName">Texture name to search for</param>
        /// <param name="replaceAtlasName">Name of atlas to replace</param>
        /// <param name="replaceTextureName">Texture name to replace</param>
        public void SwapAnimationBoneTexture(string animationName,
                                            string boneName,
                                            string searchAtlasName,
                                            string searchTextureName,
                                            string replaceAtlasName,
                                            string replaceTextureName)
        {
            int clipIndex = GetAnimationClipIndex(animationName);

            if (clipIndex == -1)
            {
                Debug.LogError("Animation [" + animationName + "] not found");
                return;
            }

            TextureAtlas searchAtlas = GetTextureAtlasFromName(searchAtlasName);
            TextureAtlas replaceAtlas = GetTextureAtlasFromName(replaceAtlasName);

            if (searchAtlas == null)
            {
                Debug.LogError("Atlas [" + searchAtlasName + "] not found");
                return;
            }
            if (replaceAtlas == null)
            {
                Debug.LogError("Atlas [" + replaceAtlasName + "] not found");
                return;
            }

            string searchTextureGUID = "";
            string replaceTextureGUID = "";

            GetSwapTextureNamesFromGUIDs(searchAtlas, searchTextureName, replaceAtlas, replaceTextureName, out searchTextureGUID, out replaceTextureGUID);

            if (ReplaceAnimationBoneTexture(clipIndex,
                                                boneName,
                                                searchAtlas,
                                                searchTextureGUID,
                                                replaceAtlas,
                                                replaceTextureGUID,
                                                Vector2.zero,
                                                true))
            {
                if (IsPlaying(animationName))
                {
                    if (FireLastTextureAnimationEvent(boneName))
                    {
                        GenerateMesh();
                    }
                }
            }
        }

        /// <summary>
        /// Swaps a texture for an animation and bone
        /// </summary>
        /// <param name="clipIndex">Index of the animation to search</param>
        /// <param name="boneName">Name of the bone to search</param>
        /// <param name="searchAtlasName">Name of atlas to search for</param>
        /// <param name="searchTextureName">Texture name to search for</param>
        /// <param name="replaceAtlasName">Name of atlas to replace</param>
        /// <param name="replaceTextureName">Texture name to replace</param>
        /// <param name="replacePivotOffset">Pivot offset of the new texture</param>
        public void SwapAnimationBoneTexture(int clipIndex,
                                            string boneName,
                                            string searchAtlasName,
                                            string searchTextureName,
                                            string replaceAtlasName,
                                            string replaceTextureName,
                                            Vector2 replacePivotOffset)
        {
            TextureAtlas searchAtlas = GetTextureAtlasFromName(searchAtlasName);
            TextureAtlas replaceAtlas = GetTextureAtlasFromName(replaceAtlasName);

            if (searchAtlas == null)
            {
                Debug.LogError("Atlas [" + searchAtlasName + "] not found");
                return;
            }
            if (replaceAtlas == null)
            {
                Debug.LogError("Atlas [" + replaceAtlasName + "] not found");
                return;
            }

            string searchTextureGUID = "";
            string replaceTextureGUID = "";

            GetSwapTextureNamesFromGUIDs(searchAtlas, searchTextureName, replaceAtlas, replaceTextureName, out searchTextureGUID, out replaceTextureGUID);

            if (ReplaceAnimationBoneTexture(clipIndex,
                                        boneName,
                                        searchAtlas,
                                        searchTextureGUID,
                                        replaceAtlas,
                                        replaceTextureGUID,
                                        replacePivotOffset,
                                        false))
            {
                if (IsPlaying(clipIndex))
                {
                    if (FireLastTextureAnimationEvent(boneName))
                    {
                        GenerateMesh();
                    }
                }
            }
        }

        /// <summary>
        /// Swaps a texture on an animation and bone
        /// </summary>
        /// <param name="clipIndex">Index of the animation to search</param>
        /// <param name="boneName">Name of the bone to search</param>
        /// <param name="searchAtlasName">Name of atlas to search for</param>
        /// <param name="searchTextureName">Texture name to search for</param>
        /// <param name="replaceAtlasName">Name of atlas to replace</param>
        /// <param name="replaceTextureName">Texture name to replace</param>
        public void SwapAnimationBoneTexture(int clipIndex,
                                            string boneName,
                                            string searchAtlasName,
                                            string searchTextureName,
                                            string replaceAtlasName,
                                            string replaceTextureName)
        {
            TextureAtlas searchAtlas = GetTextureAtlasFromName(searchAtlasName);
            TextureAtlas replaceAtlas = GetTextureAtlasFromName(replaceAtlasName);

            if (searchAtlas == null)
            {
                Debug.LogError("Atlas [" + searchAtlasName + "] not found");
                return;
            }
            if (replaceAtlas == null)
            {
                Debug.LogError("Atlas [" + replaceAtlasName + "] not found");
                return;
            }

            string searchTextureGUID = "";
            string replaceTextureGUID = "";

            GetSwapTextureNamesFromGUIDs(searchAtlas, searchTextureName, replaceAtlas, replaceTextureName, out searchTextureGUID, out replaceTextureGUID);

            if (ReplaceAnimationBoneTexture(clipIndex,
                                        boneName,
                                        searchAtlas,
                                        searchTextureGUID,
                                        replaceAtlas,
                                        replaceTextureGUID,
                                        Vector2.zero,
                                        true))
            {
                if (IsPlaying(clipIndex))
                {
                    if (FireLastTextureAnimationEvent(boneName))
                    {
                        GenerateMesh();
                    }
                }
            }
        }

        /// <summary>
        /// Swaps a texture for an animation and bone
        /// </summary>
        /// <param name="animationName">Name of the animation to search</param>
        /// <param name="searchAtlasName">Name of atlas to search for</param>
        /// <param name="searchTextureName">Texture name to search for</param>
        /// <param name="replaceAtlasName">Name of atlas to replace</param>
        /// <param name="replaceTextureName">Texture name to replace</param>
        /// <param name="replacePivotOffset">Pivot offset of the new texture</param>
        public void SwapAnimationTexture(string animationName,
                                            string searchAtlasName,
                                            string searchTextureName,
                                            string replaceAtlasName,
                                            string replaceTextureName,
                                            Vector2 replacePivotOffset)
        {
            int clipIndex = GetAnimationClipIndex(animationName);

            if (clipIndex == -1)
            {
                Debug.LogError("Animation [" + animationName + "] not found");
                return;
            }

            TextureAtlas searchAtlas = GetTextureAtlasFromName(searchAtlasName);
            TextureAtlas replaceAtlas = GetTextureAtlasFromName(replaceAtlasName);

            if (searchAtlas == null)
            {
                Debug.LogError("Atlas [" + searchAtlasName + "] not found");
                return;
            }
            if (replaceAtlas == null)
            {
                Debug.LogError("Atlas [" + replaceAtlasName + "] not found");
                return;
            }

            string searchTextureGUID = "";
            string replaceTextureGUID = "";
            
            GetSwapTextureNamesFromGUIDs(searchAtlas, searchTextureName, replaceAtlas, replaceTextureName, out searchTextureGUID, out replaceTextureGUID);

            int boneNodeIndex;
            for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                if (ReplaceAnimationBoneTexture(clipIndex,
                                                    mBoneSource[boneNodeIndex].boneName,
                                                    searchAtlas,
                                                    searchTextureGUID,
                                                    replaceAtlas,
                                                    replaceTextureGUID,
                                                    replacePivotOffset,
                                                    false))
                {
                    _refireList[boneNodeIndex] = true;
                }
                else
                {
                    _refireList[boneNodeIndex] = false;
                }
            }

            if (IsPlaying(animationName))
            {
                bool regenerateMesh = false;
                for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
                {
                    if (_refireList[boneNodeIndex])
                    {
                        if (FireLastTextureAnimationEvent(mBoneSource[boneNodeIndex].boneName))
                        {
                            regenerateMesh = true;
                        }
                    }
                }

                if (regenerateMesh)
                    GenerateMesh();
            }
        }

        /// <summary>
        /// Swaps a texture for an animation and bone
        /// </summary>
        /// <param name="animationName">Name of animation to search</param>
        /// <param name="searchAtlasName">Name of atlas to search for</param>
        /// <param name="searchTextureName">Texture name to search for</param>
        /// <param name="replaceAtlasName">Name of atlas to replace</param>
        /// <param name="replaceTextureName">Texture name to replace</param>
        public void SwapAnimationTexture(string animationName,
                                            string searchAtlasName,
                                            string searchTextureName,
                                            string replaceAtlasName,
                                            string replaceTextureName)
        {
            int clipIndex = GetAnimationClipIndex(animationName);

            if (clipIndex == -1)
            {
                Debug.LogError("Animation [" + animationName + "] not found");
                return;
            }

            TextureAtlas searchAtlas = GetTextureAtlasFromName(searchAtlasName);
            TextureAtlas replaceAtlas = GetTextureAtlasFromName(replaceAtlasName);

            if (searchAtlas == null)
            {
                Debug.LogError("Atlas [" + searchAtlasName + "] not found");
                return;
            }
            if (replaceAtlas == null)
            {
                Debug.LogError("Atlas [" + replaceAtlasName + "] not found");
                return;
            }

            string searchTextureGUID = "";
            string replaceTextureGUID = "";

            GetSwapTextureNamesFromGUIDs(searchAtlas, searchTextureName, replaceAtlas, replaceTextureName, out searchTextureGUID, out replaceTextureGUID);

            int boneNodeIndex;
            for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                if (ReplaceAnimationBoneTexture(clipIndex,
                                                    mBoneSource[boneNodeIndex].boneName,
                                                    searchAtlas,
                                                    searchTextureGUID,
                                                    replaceAtlas,
                                                    replaceTextureGUID,
                                                    Vector2.zero,
                                                    true))
                {
                    _refireList[boneNodeIndex] = true;
                }
                else
                {
                    _refireList[boneNodeIndex] = false;
                }
            }

            if (IsPlaying(animationName))
            {
                bool regenerateMesh = false;
                for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
                {
                    if (_refireList[boneNodeIndex])
                    {
                        if (FireLastTextureAnimationEvent(mBoneSource[boneNodeIndex].boneName))
                        {
                            regenerateMesh = true;
                        }
                    }
                }

                if (regenerateMesh)
                    GenerateMesh();
            }
        }

        /// <summary>
        /// Swaps a texture for an animation and bone
        /// </summary>
        /// <param name="clipIndex">Index of the animation to search</param>
        /// <param name="searchAtlasName">Name of atlas to search for</param>
        /// <param name="searchTextureName">Texture name to search for</param>
        /// <param name="replaceAtlasName">Name of atlas to replace</param>
        /// <param name="replaceTextureName">Texture name to replace</param>
        /// <param name="replacePivotOffset">Pivot offset of the new texture</param>
        public void SwapAnimationTexture(int clipIndex,
                                            string searchAtlasName,
                                            string searchTextureName,
                                            string replaceAtlasName,
                                            string replaceTextureName,
                                            Vector2 replacePivotOffset)
        {
            TextureAtlas searchAtlas = GetTextureAtlasFromName(searchAtlasName);
            TextureAtlas replaceAtlas = GetTextureAtlasFromName(replaceAtlasName);

            if (searchAtlas == null)
            {
                Debug.LogError("Atlas [" + searchAtlasName + "] not found");
                return;
            }
            if (replaceAtlas == null)
            {
                Debug.LogError("Atlas [" + replaceAtlasName + "] not found");
                return;
            }

            string searchTextureGUID = "";
            string replaceTextureGUID = "";

            GetSwapTextureNamesFromGUIDs(searchAtlas, searchTextureName, replaceAtlas, replaceTextureName, out searchTextureGUID, out replaceTextureGUID);

            int boneNodeIndex;
            for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                if (ReplaceAnimationBoneTexture(clipIndex,
                                        mBoneSource[boneNodeIndex].boneName,
                                        searchAtlas,
                                        searchTextureGUID,
                                        replaceAtlas,
                                        replaceTextureGUID,
                                        replacePivotOffset,
                                        false))
                {
                    _refireList[boneNodeIndex] = true;
                }
                else
                {
                    _refireList[boneNodeIndex] = false;
                }
            }

            if (IsPlaying(clipIndex))
            {
                bool regenerateMesh = false;
                for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
                {
                    if (_refireList[boneNodeIndex])
                    {
                        if (FireLastTextureAnimationEvent(mBoneSource[boneNodeIndex].boneName))
                        {
                            regenerateMesh = true;
                        }
                    }
                }

                if (regenerateMesh)
                    GenerateMesh();
            }
        }

        /// <summary>
        /// Swaps a texture on an animation and bone
        /// </summary>
        /// <param name="clipIndex">Index of the animation to search</param>
        /// <param name="searchAtlasName">Name of atlas to search for</param>
        /// <param name="searchTextureName">Texture name to search for</param>
        /// <param name="replaceAtlasName">Name of atlas to replace</param>
        /// <param name="replaceTextureName">Texture name to replace</param>
        public void SwapAnimationTexture(int clipIndex,
                                            string searchAtlasName,
                                            string searchTextureName,
                                            string replaceAtlasName,
                                            string replaceTextureName)
        {
            TextureAtlas searchAtlas = GetTextureAtlasFromName(searchAtlasName);
            TextureAtlas replaceAtlas = GetTextureAtlasFromName(replaceAtlasName);

            if (searchAtlas == null)
            {
                Debug.LogError("Atlas [" + searchAtlasName + "] not found");
                return;
            }
            if (replaceAtlas == null)
            {
                Debug.LogError("Atlas [" + replaceAtlasName + "] not found");
                return;
            }

            string searchTextureGUID = "";
            string replaceTextureGUID = "";

            GetSwapTextureNamesFromGUIDs(searchAtlas, searchTextureName, replaceAtlas, replaceTextureName, out searchTextureGUID, out replaceTextureGUID);

            int boneNodeIndex;
            for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                if (ReplaceAnimationBoneTexture(clipIndex,
                                        mBoneSource[boneNodeIndex].boneName,
                                        searchAtlas,
                                        searchTextureGUID,
                                        replaceAtlas,
                                        replaceTextureGUID,
                                        Vector2.zero,
                                        true))
                {
                    _refireList[boneNodeIndex] = true;
                }
                else
                {
                    _refireList[boneNodeIndex] = false;
                }
            }

            if (IsPlaying(clipIndex))
            {
                bool regenerateMesh = false;
                for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
                {
                    if (_refireList[boneNodeIndex])
                    {
                        if (FireLastTextureAnimationEvent(mBoneSource[boneNodeIndex].boneName))
                        {
                            regenerateMesh = true;
                        }
                    }
                }

                if (regenerateMesh)
                    GenerateMesh();
            }
        }

        /// <summary>
        /// Swaps an texture for a bone across all animation clips
        /// </summary>
        /// <param name="boneName">Name of bone to search</param>
        /// <param name="searchAtlasName">Name of atlas to search for</param>
        /// <param name="searchTextureName">Texture name to search for</param>
        /// <param name="replaceAtlasName">Name of atlas to replace</param>
        /// <param name="replaceTextureName">Texture name to replace</param>
        /// <param name="replacePivotOffset">Pivot offset of the new texture</param>
        public void SwapBoneTexture(string boneName,
                                    string searchAtlasName,
                                    string searchTextureName,
                                    string replaceAtlasName,
                                    string replaceTextureName,
                                    Vector2 replacePivotOffset)
        {
            TextureAtlas searchAtlas = GetTextureAtlasFromName(searchAtlasName);
            TextureAtlas replaceAtlas = GetTextureAtlasFromName(replaceAtlasName);

            if (searchAtlas == null)
            {
                Debug.LogError("Atlas [" + searchAtlasName + "] not found");
                return;
            }
            if (replaceAtlas == null)
            {
                Debug.LogError("Atlas [" + replaceAtlasName + "] not found");
                return;
            }

            bool refire = false;
            string searchTextureGUID = "";
            string replaceTextureGUID = "";

            GetSwapTextureNamesFromGUIDs(searchAtlas, searchTextureName, replaceAtlas, replaceTextureName, out searchTextureGUID, out replaceTextureGUID);
            
            if (ReplaceAnimationBoneTexture(-1,
                                                boneName,
                                                searchAtlas,
                                                searchTextureGUID,
                                                replaceAtlas,
                                                replaceTextureGUID,
                                                replacePivotOffset,
                                                false))
            {
                refire = true;
            }

            if (refire)
            {
                if (FireLastTextureAnimationEvent(boneName))
                {
                    GenerateMesh();
                }
            }
        }

        /// <summary>
        /// Swaps a texture for a bone across all animation clips
        /// </summary>
        /// <param name="boneName">Name of bone to search for</param>
        /// <param name="searchAtlasName">Name of atlas to search for</param>
        /// <param name="searchTextureName">Texture name to search for</param>
        /// <param name="replaceAtlasName">Name of atlas to replace</param>
        /// <param name="replaceTextureName">Texture name to replace</param>
        public void SwapBoneTexture(string boneName,
                                    string searchAtlasName,
                                    string searchTextureName,
                                    string replaceAtlasName,
                                    string replaceTextureName)
        {
            TextureAtlas searchAtlas = GetTextureAtlasFromName(searchAtlasName);
            TextureAtlas replaceAtlas = GetTextureAtlasFromName(replaceAtlasName);

            if (searchAtlas == null)
            {
                Debug.LogError("Atlas [" + searchAtlasName + "] not found");
                return;
            }
            if (replaceAtlas == null)
            {
                Debug.LogError("Atlas [" + replaceAtlasName + "] not found");
                return;
            }

            bool refire = false;
            string searchTextureGUID = "";
            string replaceTextureGUID = "";

            GetSwapTextureNamesFromGUIDs(searchAtlas, searchTextureName, replaceAtlas, replaceTextureName, out searchTextureGUID, out replaceTextureGUID);

            if (ReplaceAnimationBoneTexture(-1,
                                                boneName,
                                                searchAtlas,
                                                searchTextureGUID,
                                                replaceAtlas,
                                                replaceTextureGUID,
                                                Vector2.zero,
                                                true))
            {
                refire = true;
            }

            if (refire)
            {
                if (FireLastTextureAnimationEvent(boneName))
                {
                    GenerateMesh();
                }
            }
        }

        /// <summary>
        /// Swaps an texture for all bones across all animation clips
        /// </summary>
        /// <param name="searchAtlasName">Name of atlas to search for</param>
        /// <param name="searchTextureName">Texture name to search for</param>
        /// <param name="replaceAtlasName">Name of atlas to replace</param>
        /// <param name="replaceTextureName">Texture name to replace</param>
        /// <param name="replacePivotOffset">Pivot offset of the new texture</param>
        public void SwapTexture(string searchAtlasName,
                                    string searchTextureName,
                                    string replaceAtlasName,
                                    string replaceTextureName,
                                    Vector2 replacePivotOffset)
        {
            TextureAtlas searchAtlas = GetTextureAtlasFromName(searchAtlasName);
            TextureAtlas replaceAtlas = GetTextureAtlasFromName(replaceAtlasName);

            if (searchAtlas == null)
            {
                Debug.LogError("Atlas [" + searchAtlasName + "] not found");
                return;
            }
            if (replaceAtlas == null)
            {
                Debug.LogError("Atlas [" + replaceAtlasName + "] not found");
                return;
            }

            string searchTextureGUID = "";
            string replaceTextureGUID = "";

            GetSwapTextureNamesFromGUIDs(searchAtlas, searchTextureName, replaceAtlas, replaceTextureName, out searchTextureGUID, out replaceTextureGUID);

            int boneNodeIndex;
            for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                if (ReplaceAnimationBoneTexture(-1,
                                                    mBoneSource[boneNodeIndex].boneName,
                                                    searchAtlas,
                                                    searchTextureGUID,
                                                    replaceAtlas,
                                                    replaceTextureGUID,
                                                    replacePivotOffset,
                                                    false))
                {
                    _refireList[boneNodeIndex] = true;
                }
                else
                {
                    _refireList[boneNodeIndex] = false;
                }
            }

            bool regenerateMesh = false;
            for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                if (_refireList[boneNodeIndex])
                {
                    if (FireLastTextureAnimationEvent(mBoneSource[boneNodeIndex].boneName))
                    {
                        regenerateMesh = true;
                    }
                }
            }

            if (regenerateMesh)
                GenerateMesh();
        }

        /// <summary>
        /// Swaps an texture for all bones across all animation clips
        /// </summary>
        /// <param name="searchAtlasName">Name of atlas to search for</param>
        /// <param name="searchTextureName">Texture name to search for</param>
        /// <param name="replaceAtlasName">Name of atlas to replace</param>
        /// <param name="replaceTextureName">Texture name to replace</param>
        /// <param name="replacePivotOffset">Pivot offset of the new texture</param>
        public void SwapTexture(string searchAtlasName,
                                    string searchTextureName,
                                    string replaceAtlasName,
                                    string replaceTextureName)
        {
            TextureAtlas searchAtlas = GetTextureAtlasFromName(searchAtlasName);
            TextureAtlas replaceAtlas = GetTextureAtlasFromName(replaceAtlasName);

            if (searchAtlas == null)
            {
                Debug.LogError("Atlas [" + searchAtlasName + "] not found");
                return;
            }
            if (replaceAtlas == null)
            {
                Debug.LogError("Atlas [" + replaceAtlasName + "] not found");
                return;
            }

            string searchTextureGUID = "";
            string replaceTextureGUID = "";

            GetSwapTextureNamesFromGUIDs(searchAtlas, searchTextureName, replaceAtlas, replaceTextureName, out searchTextureGUID, out replaceTextureGUID);

            int boneNodeIndex;
            for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                if (ReplaceAnimationBoneTexture(-1,
                                                    mBoneSource[boneNodeIndex].boneName,
                                                    searchAtlas,
                                                    searchTextureGUID,
                                                    replaceAtlas,
                                                    replaceTextureGUID,
                                                    Vector2.zero,
                                                    true))
                {
                    _refireList[boneNodeIndex] = true;
                }
                else
                {
                    _refireList[boneNodeIndex] = false;
                }
            }

            bool regenerateMesh = false;
            for (boneNodeIndex = 0; boneNodeIndex < mBoneSource.Length; boneNodeIndex++)
            {
                if (_refireList[boneNodeIndex])
                {
                    if (FireLastTextureAnimationEvent(mBoneSource[boneNodeIndex].boneName))
                    {
                        regenerateMesh = true;
                    }
                }
            }

            if (regenerateMesh)
                GenerateMesh();
        }

        /// <summary>
        /// Tells Unity to fire an animation event for the given animation clip and frame. If no animation
        /// event exists, then nothing will happen. This function is useful if you are playing an animation
        /// that runs at zero fps. Unity will not fire animation events for this type of clip, so you will
        /// need to call this function after setting your animation time.
        /// 
        /// This function should be used sparingly as it could potentially take a while to find and
        /// process the appropriate trigger, depending on the complexity of the animation.
        /// </summary>
        /// <param name="animationName">Name of the animation clip to fire</param>
        /// <param name="frame">The frame of the animation to fire</param>
        public void ForceAnimationEvent(string animationName, int frame)
        {
            int clipIndex = GetAnimationClipIndex(animationName);

            if (clipIndex != -1)
            {
                ForceAnimationEvent(clipIndex, frame);
            }
            else
            {
                Debug.LogError("ForceAnimationEvent: Could not find an animation clip with name [" + animationName + "]");
            }
        }

        /// <summary>
        /// Tells Unity to fire an animation event for the given animation clip and frame. If no animation
        /// event exists, then nothing will happen. This function is useful if you are playing an animation
        /// that runs at zero fps. Unity will not fire animation events for this type of clip, so you will
        /// need to call this function after setting your animation time.
        /// 
        /// This function should be used sparingly as it could potentially take a while to find and
        /// process the appropriate trigger, depending on the complexity of the animation.
        /// </summary>
        /// <param name="animationName">clip index of the animation clip to fire</param>
        /// <param name="frame">The frame of the animation to fire</param>
        public void ForceAnimationEvent(int clipIndex, int frame)
        {
            if (ClipIndexOK(clipIndex))
            {
                int triggerFrameIndex;
                for (triggerFrameIndex=0; triggerFrameIndex < triggerFrames.Length; triggerFrameIndex++)
                {
                    if (triggerFrames[triggerFrameIndex].clipIndex == clipIndex)
                    {
                        if (triggerFrames[triggerFrameIndex].frame == frame)
                        {
                            AnimationEventTrigger(triggerFrames[triggerFrameIndex]);
                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("ForceAnimationEvent: Could not find an animation clip at index [" + clipIndex.ToString() + "]");
            }
        }

        #endregion

        #region Legacy Public Functions

        /// <summary>
        /// Replaces a texture for an animation and bone
        /// </summary>
        /// <param name="animationName">Name of the animation to search</param>
        /// <param name="boneName">Name of the bone to search</param>
        /// <param name="searchAtlas">Atlas to search for</param>
        /// <param name="searchTextureGUID">Texture GUID to search for</param>
        /// <param name="replaceAtlas">Atlas to replace</param>
        /// <param name="replaceTextureGUID">Texture GUID to replace</param>
        /// <param name="replacePivotOffset">Pivot offset of the new texture</param>
        public void ReplaceAnimationBoneTexture(string animationName,
                                            string boneName,
                                            TextureAtlas searchAtlas,
                                            string searchTextureGUID,
                                            TextureAtlas replaceAtlas,
                                            string replaceTextureGUID,
                                            Vector2 replacePivotOffset)
        {
            int clipIndex = GetAnimationClipIndex(animationName);

            if (clipIndex == -1)
            {
                Debug.LogError("Animation [" + animationName + "] not found");
                return;
            }

            if (ReplaceAnimationBoneTexture(clipIndex,
                                                boneName,
                                                searchAtlas,
                                                searchTextureGUID,
                                                replaceAtlas,
                                                replaceTextureGUID,
                                                replacePivotOffset,
                                                false))
            {
                if (FireLastTextureAnimationEvent(boneName))
                {
                    GenerateMesh();
                }
            }
        }

        /// <summary>
        /// Replaces a texture for an animation and bone
        /// </summary>
        /// <param name="animationName">Name of animation to search</param>
        /// <param name="boneName">Name of bone to search</param>
        /// <param name="searchAtlas">Atlas to search for</param>
        /// <param name="searchTextureGUID">Texture GUID to search for</param>
        /// <param name="replaceAtlas">Atlas to replace</param>
        /// <param name="replaceTextureGUID">Texture GUID to replace</param>
        public void ReplaceAnimationBoneTexture(string animationName,
                                            string boneName,
                                            TextureAtlas searchAtlas,
                                            string searchTextureGUID,
                                            TextureAtlas replaceAtlas,
                                            string replaceTextureGUID)
        {
            int clipIndex = GetAnimationClipIndex(animationName);

            if (clipIndex == -1)
            {
                Debug.LogError("Animation [" + animationName + "] not found");
                return;
            }

            if (ReplaceAnimationBoneTexture(clipIndex,
                                                boneName,
                                                searchAtlas,
                                                searchTextureGUID,
                                                replaceAtlas,
                                                replaceTextureGUID,
                                                Vector2.zero,
                                                true))
            {
                if (FireLastTextureAnimationEvent(boneName))
                {
                    GenerateMesh();
                }
            }
        }

        /// <summary>
        /// Replaces a texture for an animation and bone
        /// </summary>
        /// <param name="animationName">Name of the animation to search</param>
        /// <param name="boneName">Name of bone to search</param>
        /// <param name="textureSearchReplace">A reference to a search and replace class that stores texture information</param>
        public void ReplaceAnimationBoneTexture(string animationName,
                                            string boneName,
                                            TextureSearchReplace textureSearchReplace)
        {
            int clipIndex = GetAnimationClipIndex(animationName);

            if (clipIndex == -1)
            {
                Debug.LogError("Animation [" + animationName + "] not found");
                return;
            }

            if (ReplaceAnimationBoneTexture(clipIndex,
                                                boneName,
                                                textureSearchReplace.searchAtlas,
                                                textureSearchReplace.searchTextureGUID,
                                                textureSearchReplace.replaceAtlas,
                                                textureSearchReplace.replaceTextureGUID,
                                                textureSearchReplace.replacePivotOffset,
                                                textureSearchReplace.replaceUseDefaultPivot))
            {
                if (FireLastTextureAnimationEvent(boneName))
                {
                    GenerateMesh();
                }
            }
        }

        /// <summary>
        /// Replaces a texture for an animation and bone
        /// </summary>
        /// <param name="clipIndex">Index of the animation to search</param>
        /// <param name="boneName">Name of the bone to search</param>
        /// <param name="searchAtlas">Atlas to search for</param>
        /// <param name="searchTextureGUID">Texture GUID to search for</param>
        /// <param name="replaceAtlas">Atlas to replace</param>
        /// <param name="replaceTextureGUID">Texture GUID to replace</param>
        /// <param name="replacePivotOffset">Pivot offset of the new texture</param>
        public void ReplaceAnimationBoneTexture(int clipIndex,
                                            string boneName,
                                            TextureAtlas searchAtlas,
                                            string searchTextureGUID,
                                            TextureAtlas replaceAtlas,
                                            string replaceTextureGUID,
                                            Vector2 replacePivotOffset)
        {
            if (ReplaceAnimationBoneTexture(clipIndex,
                                        boneName,
                                        searchAtlas,
                                        searchTextureGUID,
                                        replaceAtlas,
                                        replaceTextureGUID,
                                        replacePivotOffset,
                                        false))
            {
                if (FireLastTextureAnimationEvent(boneName))
                {
                    GenerateMesh();
                }
            }
        }

        /// <summary>
        /// Replaces a texture on an animation and bone
        /// </summary>
        /// <param name="clipIndex">Index of the animation to search</param>
        /// <param name="boneName">Name of the bone to search</param>
        /// <param name="searchAtlas">Atlas to search for</param>
        /// <param name="searchTextureGUID">Texture GUID to search for</param>
        /// <param name="replaceAtlas">Atlas to replace</param>
        /// <param name="replaceTextureGUID">Texture GUID to replace</param>
        public void ReplaceAnimationBoneTexture(int clipIndex,
                                            string boneName,
                                            TextureAtlas searchAtlas,
                                            string searchTextureGUID,
                                            TextureAtlas replaceAtlas,
                                            string replaceTextureGUID)
        {
            if (ReplaceAnimationBoneTexture(clipIndex,
                                        boneName,
                                        searchAtlas,
                                        searchTextureGUID,
                                        replaceAtlas,
                                        replaceTextureGUID,
                                        Vector2.zero,
                                        true))
            {
                if (FireLastTextureAnimationEvent(boneName))
                {
                    GenerateMesh();
                }
            }
        }

        /// <summary>
        /// Replaces a texture on an animation and bone
        /// </summary>
        /// <param name="clipIndex">Index of the animation to search</param>
        /// <param name="boneName">Name of the bone to search</param>
        /// <param name="textureSearchReplace">A reference to a search and replace class that stores texture information</param>
        public void ReplaceAnimationBoneTexture(int clipIndex,
                                            string boneName,
                                            TextureSearchReplace textureSearchReplace)
        {
            if (ReplaceAnimationBoneTexture(clipIndex,
                                        boneName,
                                        textureSearchReplace.searchAtlas,
                                        textureSearchReplace.searchTextureGUID,
                                        textureSearchReplace.replaceAtlas,
                                        textureSearchReplace.replaceTextureGUID,
                                        textureSearchReplace.replacePivotOffset,
                                        textureSearchReplace.replaceUseDefaultPivot))
            {
                if (FireLastTextureAnimationEvent(boneName))
                {
                    GenerateMesh();
                }
            }
        }

        /// <summary>
        /// Replaces an texture for a bone across all animation clips
        /// </summary>
        /// <param name="boneName">Name of bone to search</param>
        /// <param name="searchAtlas">Atlas to search for</param>
        /// <param name="searchTextureGUID">Texture GUID to search for</param>
        /// <param name="replaceAtlas">Atlas to replace</param>
        /// <param name="replaceTextureGUID">Texture GUID to replace</param>
        /// <param name="replacePivotOffset">Pivot offset of the new texture</param>
        public void ReplaceBoneTexture(string boneName,
                                   TextureAtlas searchAtlas,
                                    string searchTextureGUID,
                                    TextureAtlas replaceAtlas,
                                    string replaceTextureGUID,
                                    Vector2 replacePivotOffset)
        {
            bool refire = false;

            if (ReplaceAnimationBoneTexture(-1,
                                            boneName,
                                            searchAtlas,
                                            searchTextureGUID,
                                            replaceAtlas,
                                            replaceTextureGUID,
                                            replacePivotOffset,
                                            false))
            {
                refire = true;
            }

            if (refire)
            {
                if (FireLastTextureAnimationEvent(boneName))
                {
                    GenerateMesh();
                }
            }
        }

        /// <summary>
        /// Replaces a texture for a bone across all animation clips
        /// </summary>
        /// <param name="boneName">Name of bone to search for</param>
        /// <param name="searchAtlas">Atlas to search for</param>
        /// <param name="searchTextureGUID">Texture GUID to search for</param>
        /// <param name="replaceAtlas">Atlas to replace</param>
        /// <param name="replaceTextureGUID">Texture GUID to replace</param>
        public void ReplaceBoneTexture(string boneName,
                                   TextureAtlas searchAtlas,
                                    string searchTextureGUID,
                                    TextureAtlas replaceAtlas,
                                    string replaceTextureGUID)
        {
            bool refire = false;


            if (ReplaceAnimationBoneTexture(-1,
                                                boneName,
                                                searchAtlas,
                                                searchTextureGUID,
                                                replaceAtlas,
                                                replaceTextureGUID,
                                                Vector2.zero,
                                                true))
            {
                refire = true;
            }

            if (refire)
            {
                if (FireLastTextureAnimationEvent(boneName))
                {
                    GenerateMesh();
                }
            }
        }

        /// <summary>
        /// Replaces a texture on a bone across all animation clips
        /// </summary>
        /// <param name="boneName">Name of bone to search</param>
        /// <param name="textureSearchReplace">A reference to a search and replace class that stores texture information</param>
        public void ReplaceBoneTexture(string boneName,
                                   TextureSearchReplace textureSearchReplace)
        {
            bool refire = false;

            if (ReplaceAnimationBoneTexture(-1,
                                                boneName,
                                                textureSearchReplace.searchAtlas,
                                                textureSearchReplace.searchTextureGUID,
                                                textureSearchReplace.replaceAtlas,
                                                textureSearchReplace.replaceTextureGUID,
                                                textureSearchReplace.replacePivotOffset,
                                                textureSearchReplace.replaceUseDefaultPivot))
            {
                refire = true;
            }

            if (refire)
            {
                if (FireLastTextureAnimationEvent(boneName))
                {
                    GenerateMesh();
                }
            }
        }

        #endregion
    }
}
