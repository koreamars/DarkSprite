using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SmoothMoves
{
    public class AnimationWindowBone
    {
        public int boneNodeIndex;
        public string boneName;
        public bool visible;
        public int depth;
        public Matrix4x4 positionMatrix;
        public Matrix4x4 drawMatrix;
        public Rect drawRect;
        public Texture2D texture;
        public Color color;
        public bool selected;
        public Vector3 globalPosition;
        public float globalRotation;
        public float parentGlobalRotation;
        public ColliderSM collider;
        public Vector2 globalColliderCenter;
        public Vector2 parentGlobalScale;
        public Vector2 globalScale;

        public bool gizmoMoveX;
        public bool gizmoMoveY;
        public bool gizmoRotation;
        public bool gizmoScaleX;
        public bool gizmoScaleY;
        public bool gizmoDepth;
    }

    public class SortAnimationWindowBoneDepthDescending : IComparer<AnimationWindowBone>
    {
        int IComparer<AnimationWindowBone>.Compare(AnimationWindowBone a, AnimationWindowBone b)
        {
            if (a.depth < b.depth)
                return 1;
            if (a.depth > b.depth)
                return -1;
            else
                return 0;
        }
    }

    static public class AnimationWindow
    {
        private const int GRID_DEPTH = 1000;
        private const int BASE_DEPTH = 100;
        private const float SCROLL_WHEEL_SENSITIVITY = 0.03f;
        private const float SCALE_GIZMO_FACTOR = 0.01f;
        private const float GIZMO_BOX_OFFSET = 10.0f;
        private const float ROTATION_RING_OUTER_OFFSET = 10.0f;
        private const float ROTATION_RING_INNER_OFFSET = 10.0f;
        private const float ROTATION_SENSITIVITY = 0.3f;
        private const float GIZMO_LABEL_RADIUS = 140.0f;
        private const int SPHERE_COLLIDER_POINTS = 40;
        private const float MINIMUM_LOCAL_SCALE = 0.00001f;

        static private ACTION _action;
        static private Vector2 _dragStartPosition;
        static private Vector2 _dragStartOrigin;
        static private Vector2 _objectStartPosition;
        static private Vector2 _objectStartScale;
        static private float _objectClickDistance;
        static private float _rotationRingOuterMagSqr;
        static private float _rotationRingInnerMagSqr;
        static private bool _willBeDirty;
        static private float _zoom;
        static private Vector2 _animationOriginOffset;
        static private bool _drawGizmos;
        static private bool _needsRecentered;

        static private Rect _gizmoRotationRingRect;
        static private Rect _gizmoMoveArrowRect;
        static private Rect _gizmoScaleBarRect;
        static private Rect _gizmoMoveFreeBoxRect;
        static private float _selectedRotation;
        static private Vector2 _selectedPivot;
        static private float _parentRotation;
        static private Rect _gizmoLocalScaleButtonRect;
        static private Rect _gizmoImageScaleButtonRect;

        static private Rect _areaRect;
        static private Rect _leftMaskRect;
        static private Rect _bottomMaskRect;
        static private Rect _topMaskRect;
        static private Vector2 _offset;
        static private Rect _moveFreeRect;
		static private Rect _boneLabelRect;
		static private Rect _frameLabelRect;
		static private Rect _depthUpRect;
		static private Rect _depthLabelRect;
		static private Rect _depthDownRect;
        static private Rect _scaleXLabelRect;
        static private Rect _scaleYLabelRect;
        static private Rect _positionLabelRect;
        static private Rect _rotationLabelRect;
        static private Rect _keyframeLabelRect;
        static private bool _repositionAfterResize;
		
        static private CollisionArc _moveYArc;
        static private CollisionArc _moveXArc;
        static private CollisionArc _scaleYArc;
        static private CollisionArc _scaleXArc;
        static private CollisionArc _scaleUniformArc;

        static public bool _gizmoMoveX;
        static public bool _gizmoMoveY;
        static public bool _gizmoRotation;
        static public bool _gizmoScaleX;
        static public bool _gizmoScaleY;
        static public bool _gizmoDepth;

        static private List<AnimationWindowBone> _animationWindowBones = new List<AnimationWindowBone>();
        static private AnimationWindowBone _selectedAnimationWindowBone;

        public enum ACTION
        {
            None,
            DragScreen,
            MoveX,
            MoveY,
            MoveFree,
            Rotate,
            ImageScaleX,
            ImageScaleY,
            ImageScaleUniform,
            LocalScaleX,
            LocalScaleY,
            LocalScaleUniform,
			MoveDepthUp,
			MoveDepthDown
        }

        static private BoneAnimationDataEditorWindow editor { get { return BoneAnimationDataEditorWindow.Instance; } }

        static public bool RepositionAfterResize
        {
            set
            {
                _repositionAfterResize = value;
            }
        }
        static public Rect AreaRect 
        { 
            get 
            { 
                return _areaRect; 
            } 
            set 
            {
                Rect oldAreaRect = _areaRect;

                _areaRect = value;
                _offset = GUIHelper.UpperLeftCorner(_areaRect);

                if (_repositionAfterResize)
                {
                    Vector2 offset = new Vector2(oldAreaRect.width / 2.0f, oldAreaRect.height / 2.0f) - _animationOriginOffset;
                    _animationOriginOffset = new Vector2(_areaRect.width / 2.0f, _areaRect.height / 2.0f) - offset;

                    _repositionAfterResize = false;
                }
            } 
        }
        static public float Zoom { get { return _zoom; } set { _zoom = value; } }
        static public ACTION Action { get { return _action; } }

        static public void OnEnable()
        {
            _zoom = 1.0f;
        }

        static public void SetRects()
        {
            _gizmoRotationRingRect = new Rect(0, 0, 128.0f, 128.0f);
            _gizmoMoveArrowRect = new Rect(0, 0, 10.0f, 100.0f);
            _gizmoScaleBarRect = new Rect(0, 0, 10.0f, 100.0f);
            _gizmoMoveFreeBoxRect = new Rect(0, 0, 41.0f, 41.0f);

            _moveFreeRect = _gizmoMoveFreeBoxRect;
			
			_depthUpRect = new Rect(0, 0, 22, 11);
			_depthDownRect = new Rect(0, 0, 22, 11);
			_depthLabelRect = new Rect(0, 0, 60.0f, 23.0f);
			
			_boneLabelRect = new Rect(0, 0, 100.0f, 23.0f);
			_frameLabelRect = new Rect(0, 0, 100.0f, 23.0f);

            _scaleXLabelRect = new Rect(0, 0, 70.0f, 23.0f);
            _scaleYLabelRect = new Rect(0, 0, 70.0f, 23.0f);
            _positionLabelRect = new Rect(0, 0, 150.0f, 23.0f);
            _rotationLabelRect = new Rect(0, 0, 100.0f, 23.0f);

            _keyframeLabelRect = new Rect(0, 0, 115.0f, 40.0f);

            _moveYArc = new CollisionArc(80.0f, 100.0f, 150.0f);
            _moveXArc = new CollisionArc(-10.0f, 10.0f, 150.0f);
            _scaleYArc = new CollisionArc(260.0f, 280.0f, 150.0f);
            _scaleXArc = new CollisionArc(170.0f, 190.0f, 150.0f);
            _scaleUniformArc = new CollisionArc(215.0f, 235.0f, 150.0f);

            _gizmoLocalScaleButtonRect = new Rect(0, 0, 73.0f, 16.0f);
            _gizmoImageScaleButtonRect = new Rect(0, 0, 73.0f, 16.0f);

            _rotationRingOuterMagSqr = Mathf.Pow((_gizmoRotationRingRect.width / 2.0f) + ROTATION_RING_OUTER_OFFSET, 2);
            _rotationRingInnerMagSqr = Mathf.Pow((_gizmoRotationRingRect.width / 2.0f) - ROTATION_RING_INNER_OFFSET, 2);

            if (editor != null)
            {
                _leftMaskRect = new Rect(0, 0, _areaRect.xMin, editor.position.height);
                _bottomMaskRect = new Rect(0, _areaRect.yMax, editor.position.width, editor.position.height - _areaRect.yMax);
                _topMaskRect = new Rect(0, 0, editor.position.width, _areaRect.yMin);
            }
        }

        static public void OnGUI()
        {
            if (ClipBrowserWindow.SelectedAnimationClipIndex != -1)
            {
                BoneTree boneTreeRoot;

                if (_needsRecentered)
                {
                    CenterOrigin();
                    _needsRecentered = false;
                }

                GUIStyle gridLineStyle;

                if (SettingsWindow.AnimationContrastDark)
                {
                    GUIHelper.DrawBox(_areaRect, Style.windowRectBackgroundStyle, true);
                    gridLineStyle = Style.gridLineStyle;
                }
                else
                {
                    GUIHelper.DrawBox(_areaRect, Style.lightStyle, true);
                    gridLineStyle = Style.lightGridLineStyle;
                }

                if ((_zoom > 0.3f || SettingsWindow.AnimationGridSize > 20.0f) && SettingsWindow.AnimationDrawGrid)
                {
                    // horizontal lines, centered on the offset
                    for (float y = _animationOriginOffset.y + (SettingsWindow.AnimationGridSize * _zoom);
                         y <= _areaRect.height;
                        y += (SettingsWindow.AnimationGridSize * _zoom)
                        )
                    {
                        GUIHelper.DrawHorizontalLine(new Vector2(0, y) + _offset, _areaRect.width, 1.0f, gridLineStyle);
                    }
                    for (float y = _animationOriginOffset.y - (SettingsWindow.AnimationGridSize * _zoom);
                         y >= 0;
                        y -= (SettingsWindow.AnimationGridSize * _zoom)
                        )
                    {
                        GUIHelper.DrawHorizontalLine(new Vector2(0, y) + _offset, _areaRect.width, 1.0f, gridLineStyle);
                    }

                    // vertical lines, centered on the offset
                    for (float x = _animationOriginOffset.x + (SettingsWindow.AnimationGridSize * _zoom);
                         x <= _areaRect.width;
                        x += (SettingsWindow.AnimationGridSize * _zoom))
                    {
                        GUIHelper.DrawVerticalLine(new Vector2(x, 0) + _offset, _areaRect.height, 1.0f, gridLineStyle);
                    }
                    for (float x = _animationOriginOffset.x - (SettingsWindow.AnimationGridSize * _zoom);
                         x >= 0;
                        x -= (SettingsWindow.AnimationGridSize * _zoom))
                    {
                        GUIHelper.DrawVerticalLine(new Vector2(x, 0) + _offset, _areaRect.height, 1.0f, gridLineStyle);
                    }
                }

                // axis
                GUIHelper.DrawHorizontalLine(new Vector2(0, _animationOriginOffset.y) + _offset, _areaRect.width, SettingsWindow.AnimationAxisThickness, Style.xAxisStyle);
                GUIHelper.DrawVerticalLine(new Vector2(_animationOriginOffset.x, 0) + _offset, _areaRect.height, SettingsWindow.AnimationAxisThickness, Style.yAxisStyle);

                boneTreeRoot = editor.boneAnimationData.GenerateBoneTree();

                if (!AnimationHelper.BoneCurvesExist)
                    AnimationHelper.GenerateAnimationCurves(ClipBrowserWindow.CurrentClip);

                _animationWindowBones.Clear();
                _selectedAnimationWindowBone = null;

                // initialize the gui matrix to the window offset and zoom scale
                Matrix4x4 backupMatrix = GUI.matrix;
                GUI.matrix = Matrix4x4.TRS(_animationOriginOffset + _offset, Quaternion.identity, new Vector3(_zoom, _zoom, 1.0f));
                // calculate the bone matrices.
                // we store a success here in case something went wrong with the calcs, 
                // most likely a frame was outside of the bounds due to dragging the editor window around.
                bool calculateBonesSuccess = CalculateAnimationWindowBones(boneTreeRoot, 0, Vector2.one);
                GUI.matrix = backupMatrix;

                if (calculateBonesSuccess)
                {
                    DrawAnimationWindowBones();

                    if (!TimelineWindow.IsPlaying)
                        DrawGizmos();
                }

                // since the BeginGroup clips textures BEFORE they are rotated, the images are
                // often only half visible. As a compromise, I turn off the clipping, offset everything,
                // and just cover the overdrawn stuff with mask boxes here.
                GUIHelper.DrawBox(_leftMaskRect, Style.maskStyle, false);
                GUIHelper.DrawBox(_bottomMaskRect, Style.maskStyle, false);
                GUIHelper.DrawBox(_topMaskRect, Style.maskStyle, false);
            }
        }

        static private bool CalculateAnimationWindowBones(BoneTree boneTree, float parentRotation, Vector3 parentGlobalScale)
        {
            Matrix4x4 backupMatrix;
            BoneCurves boneCurves;
            int clipIndex;
            int boneNodeIndex;
            BoneData boneData;
            float time;
            AnimationClipSM clip;
            bool process;
            AnimationClipBone clipBone;
            bool animate;
            KeyframeSM keyframe;
            int bdIndex;
            Vector3 localPosition;
            float localRotation;
            Vector3 localScale;
            KeyframeSM.KEYFRAME_TYPE keyframeType;
            ColliderSM collider;
            TextureAtlas atlas;
            string textureGUID;
            Vector2 pivotOffset;
            int depth;
            Quaternion q;
            Vector2 imageScale;
            Color color;
            int textureIndex;
            bool hasImage;
            Texture2D texture;
            Vector2 textureSize;
            Rect drawRect;
            Quaternion localQuaternion;
            Vector3 pivotPosition;
            TextureDictionaryEntry tde;
            Color animationColor;
            AnimationWindowBone animationWindowBone;
            Vector3 globalPosition;
            bool showGizmos;
            ColliderSM.COLLIDER_TYPE colliderType;
            Vector2 colliderSize;
            Vector2 colliderPivotOffset;
            Matrix4x4 backupPositionRotationScaleMatrix;
            Vector2 globalScale;

            backupMatrix = GUI.matrix;


            clip = ClipBrowserWindow.CurrentClip;

            process = true;
            if (clip == null)
            {
                process = false;
            }
            else
            {
                if (clip.mix && boneTree.parent == null)
                {
                    process = false;
                }
            }

            localRotation = 0;
            globalScale = parentGlobalScale;

            if (process)
            {
                clipIndex = ClipBrowserWindow.SelectedAnimationClipIndex;
                boneNodeIndex = editor.boneAnimationData.GetBoneNodeIndex(boneTree.boneDataIndex);
                boneData = editor.boneAnimationData.boneDataList[boneTree.boneDataIndex];

                clipBone = editor.boneAnimationData.GetAnimationClipBoneFromBoneDataIndex(clipIndex, boneTree.boneDataIndex);
                animate = (!clip.mix || (clip.mix && clipBone.mixTransform));

                if (animate)
                {
                    keyframe = editor.boneAnimationData.GetPreviousKeyframeFromBoneDataIndex(clipIndex, TimelineWindow.CurrentFrame, boneTree.boneDataIndex, (int)TimelineWindow.PlayDirection);
                    time = TimelineWindow.FrameTime;
                }
                else
                {
                    keyframe = editor.boneAnimationData.GetKeyframe(clipIndex, boneNodeIndex, 0, out bdIndex);
                    time = 0;
                }

                // The keyframe could not be found so we exit out.
                // this is most likely caused by dragging the editor window around while
                // previewing the animation at a high FPS.
                if (keyframe == null)
                {
                    return false;
                }

                boneCurves = AnimationHelper.GetBoneDataCurves(boneTree.boneDataIndex);
                showGizmos = (TimelineWindow.CurrentFrame == keyframe.frame);

                if (boneCurves != null)
                {
                    keyframeType = boneCurves.EvaluateKeyframeTypeCurve(time, TimelineWindow.PlayDirection);
                    collider = boneCurves.EvaluateColliderCurve(time, TimelineWindow.PlayDirection);
                    if (keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                    {
                        atlas = boneCurves.EvaluateAtlasCurve(time, TimelineWindow.PlayDirection);
                        textureGUID = boneCurves.EvaluateTextureGUIDCurve(time, TimelineWindow.PlayDirection);
                        pivotOffset = boneCurves.EvaluatePivotOffsetCurve(time, TimelineWindow.PlayDirection);
                        depth = boneCurves.EvaluateDepthCurve(time, TimelineWindow.PlayDirection);
                    }
                    else
                    {
                        atlas = null;
                        textureGUID = "";
                        pivotOffset = Vector2.zero;
                        depth = 0;
                    }

                    // determine if this atlas actually contains the texture
                    if (atlas == null)
                    {
                        textureIndex = -1;
                        textureGUID = "";
                    }
                    else
                    {
                        textureIndex = atlas.GetTextureIndex(textureGUID);
                        if (textureIndex == -1)
                        {
                            textureGUID = "";
                        }
                    }

                    // make sure the texture manager has the texture
                    if (TextureManager.TextureDictionary.ContainsKey(textureGUID))
                    {
                        hasImage = true;
                    }
                    else
                    {
                        hasImage = false;
                    }

                    // determine the local position, rotation, and scale
                    localPosition = new Vector3(boneCurves.localPositionXCurve.Evaluate(time), -boneCurves.localPositionYCurve.Evaluate(time), 0);
                    q = new Quaternion(boneCurves.localRotationXCurve.Evaluate(time),
                                        boneCurves.localRotationYCurve.Evaluate(time),
                                        boneCurves.localRotationZCurve.Evaluate(time),
                                        boneCurves.localRotationWCurve.Evaluate(time));
                    localRotation = q.eulerAngles.z;
                    localQuaternion = Quaternion.Euler(0, 0, -localRotation);
                    localScale = new Vector3(boneCurves.localScaleXCurve.Evaluate(time), boneCurves.localScaleYCurve.Evaluate(time), 1.0f);

                    if (collider != null)
                    {
                        colliderType = collider.type;
                        switch (collider.type)
                        {
                            case ColliderSM.COLLIDER_TYPE.None:
                                colliderSize = Vector2.zero;
                                break;

                            case ColliderSM.COLLIDER_TYPE.Box:
                                colliderSize = new Vector2(
                                                            collider.boxSize.x,
                                                            collider.boxSize.y
                                                            );
                                break;

                            case ColliderSM.COLLIDER_TYPE.Sphere:
                                colliderSize = new Vector2(
                                                            collider.sphereRadius * 2.0f * _zoom,
                                                            collider.sphereRadius * 2.0f * _zoom
                                                            );
                                break;

                            default:
                                colliderSize = Vector2.zero;
                                break;
                        }
                    }
                    else
                    {
                        colliderType = ColliderSM.COLLIDER_TYPE.None;
                        colliderSize = Vector2.zero;
                    }

                    if (colliderType != ColliderSM.COLLIDER_TYPE.None)
                    {
                        colliderPivotOffset = new Vector2(collider.center.x, -collider.center.y); // + (new Vector2(-colliderSize.x, -colliderSize.y) * 0.5f);
                    }
                    else
                    {
                        colliderPivotOffset = Vector2.zero;
                    }

                    if (localScale.x == 0)
                        localScale.x = MINIMUM_LOCAL_SCALE;
                    if (localScale.y == 0)
                        localScale.y = MINIMUM_LOCAL_SCALE;
                    if (localScale.z == 0)
                        localScale.x = MINIMUM_LOCAL_SCALE;

                    // set the local position, rotation, and scale
                    GUI.matrix *= Matrix4x4.TRS(localPosition, localQuaternion, localScale);
                    // calculate the global position with the current state of the gui matrix. This
                    // is used for positioning gizmos later since we don't want all the other distortions
                    // of the matrix when drawing the gizmos
                    globalPosition = GUI.matrix.MultiplyPoint3x4(Vector3.zero);

                    // create animation window bone
                    animationWindowBone = new AnimationWindowBone();
                    _animationWindowBones.Add(animationWindowBone);

                    // set the bone properties
                    animationWindowBone.boneNodeIndex = boneNodeIndex;
                    animationWindowBone.boneName = boneData.boneName;
                    animationWindowBone.positionMatrix = GUI.matrix;
                    animationWindowBone.visible = clipBone.visible;
                    animationWindowBone.depth = depth;
                    animationWindowBone.globalPosition = globalPosition;
                    animationWindowBone.selected = TimelineWindow.KeyframeSelected(keyframe);
                    animationWindowBone.globalRotation = parentRotation + localRotation;
                    animationWindowBone.parentGlobalRotation = parentRotation;
                    animationWindowBone.collider = collider;
                    animationWindowBone.parentGlobalScale = parentGlobalScale;
                    animationWindowBone.gizmoMoveX = keyframe.localPosition3.useX && showGizmos;
                    animationWindowBone.gizmoMoveY = keyframe.localPosition3.useY && showGizmos;
                    animationWindowBone.gizmoRotation = keyframe.localRotation.use && showGizmos;
                    animationWindowBone.gizmoScaleX = ((SettingsWindow.AnimationScaleGizmoImage && keyframe.imageScale.useX) || (!SettingsWindow.AnimationScaleGizmoImage && keyframe.localScale3.useX)) && showGizmos;
                    animationWindowBone.gizmoScaleY = ((SettingsWindow.AnimationScaleGizmoImage && keyframe.imageScale.useY) || (!SettingsWindow.AnimationScaleGizmoImage && keyframe.localScale3.useY)) && showGizmos;
                    animationWindowBone.gizmoDepth = keyframe.useDepth && showGizmos && hasImage;

                    if (hasImage && clipBone.visible)
                    {
                        // this bone has an image and is visible

                        imageScale = new Vector2(boneCurves.imageScaleXCurve.Evaluate(time), boneCurves.imageScaleYCurve.Evaluate(time));
                        color = Color.Lerp(editor.boneAnimationData.meshColor, boneData.boneColor.color, boneData.boneColor.blendingWeight);
                        if (animate && boneCurves.colorRCurve.keys.Length > 0)
                        {
                            animationColor = new Color(boneCurves.colorRCurve.Evaluate(time),
                                                                boneCurves.colorGCurve.Evaluate(time),
                                                                boneCurves.colorBCurve.Evaluate(time),
                                                                boneCurves.colorACurve.Evaluate(time));

                            color = Color.Lerp(color, animationColor, boneCurves.colorBlendWeightCurve.Evaluate(time));
                        }

                        tde = TextureManager.TextureDictionary[textureGUID];

                        texture = tde.texture;

                        textureSize = new Vector2(tde.size.x * imageScale.x,
                                                    tde.size.y * imageScale.y);


                        pivotPosition = new Vector3(-(textureSize.x * (0.5f + pivotOffset.x)), -(textureSize.y * (0.5f - pivotOffset.y)), 0);

                        drawRect = new Rect(0, 0, textureSize.x, textureSize.y);

                        // backup the gui matrix before offsetting to the pivot
                        backupPositionRotationScaleMatrix = GUI.matrix;

                        // offset to pivot
                        GUI.matrix *= Matrix4x4.TRS(pivotPosition, Quaternion.identity, Vector3.one);

                        animationWindowBone.drawMatrix = GUI.matrix;
                        animationWindowBone.texture = texture;
                        animationWindowBone.drawRect = drawRect;
                        animationWindowBone.color = color;

                        // if this bone has children or a collider, we need to back up to the local position, rotation and scale.
                        // otherwise the matrix will just be reset anyway
                        if (boneTree.children.Count > 0 || colliderType != ColliderSM.COLLIDER_TYPE.None)
                        {
                            GUI.matrix = backupPositionRotationScaleMatrix;
                        }
                    }
                    else
                    {
                        // This is only a transform bone, no image
                        animationWindowBone.drawMatrix = GUI.matrix;
                        animationWindowBone.texture = null;
                        animationWindowBone.drawRect = new Rect();
                        animationWindowBone.color = Color.white;
                    }

                    // if this bone has a collider, we need to calculate the global center
                    // position of the collider
                    if (colliderType != ColliderSM.COLLIDER_TYPE.None)
                    {
                        Matrix4x4 colliderBackupMatrix = GUI.matrix;
                        GUI.matrix *= Matrix4x4.TRS(colliderPivotOffset, Quaternion.identity, Vector3.one);
                        animationWindowBone.globalColliderCenter = GUI.matrix.MultiplyPoint3x4(Vector3.zero);
                        GUI.matrix = colliderBackupMatrix;

                    }

                    // get the width scale by transforming a right vector inside the current matrix
                    globalScale.x = animationWindowBone.drawMatrix.MultiplyVector(Vector3.right).magnitude; ;
                    // get the height scale by transforming an up vector inside the current matrix
                    globalScale.y = animationWindowBone.drawMatrix.MultiplyVector(Vector3.up).magnitude;

                    animationWindowBone.globalScale = globalScale;

                    if (animationWindowBone.selected)
                        _selectedAnimationWindowBone = animationWindowBone;
                }
            }

            // draw each of the children with the current matrix as a basis
            foreach (BoneTree childTree in boneTree.children)
            {
                if (!CalculateAnimationWindowBones(childTree, parentRotation + localRotation, globalScale))
                    return false;
            }

            // set the matrix back to what it was before processing this bone
            // (moving back up the hierarchy)
            GUI.matrix = backupMatrix;

            return true;
        }

        static private void DrawAnimationWindowBones()
        {
            _drawGizmos = false;

            // sort the animation window bones in descending depth order
            _animationWindowBones.Sort(new SortAnimationWindowBoneDepthDescending());

            // reset the GUI matrix
            GUI.matrix = Matrix4x4.identity;

            // draw each bone
            foreach (AnimationWindowBone animationWindowBone in _animationWindowBones)
            {
                // set the bone matrix to the calculated pivot matrix
                GUI.matrix = animationWindowBone.drawMatrix;

                if (animationWindowBone.selected)
                {
                    _drawGizmos = true;

                    _selectedPivot = animationWindowBone.globalPosition;
                    _selectedRotation = -animationWindowBone.globalRotation;
                    _parentRotation = -animationWindowBone.parentGlobalRotation;

                    _gizmoRotationRingRect.x = _selectedPivot.x - (_gizmoRotationRingRect.width / 2.0f);
                    _gizmoRotationRingRect.y = _selectedPivot.y - (_gizmoRotationRingRect.height / 2.0f);

                    _gizmoMoveFreeBoxRect.x = _selectedPivot.x - (_gizmoMoveFreeBoxRect.width / 2.0f);
                    _gizmoMoveFreeBoxRect.y = _selectedPivot.y - (_gizmoMoveFreeBoxRect.height / 2.0f);

                    _gizmoMoveArrowRect.x = _selectedPivot.x - (_gizmoMoveArrowRect.width / 2.0f);
                    _gizmoMoveArrowRect.y = _selectedPivot.y - _gizmoMoveArrowRect.height - (_gizmoMoveFreeBoxRect.height / 2.0f) - GIZMO_BOX_OFFSET;

                    _gizmoScaleBarRect.x = _selectedPivot.x - (_gizmoScaleBarRect.width / 2.0f);
                    _gizmoScaleBarRect.y = _selectedPivot.y - _gizmoScaleBarRect.height - (_gizmoMoveFreeBoxRect.height / 2.0f) - GIZMO_BOX_OFFSET;

                    _gizmoLocalScaleButtonRect.x = _selectedPivot.x - (_gizmoLocalScaleButtonRect.width + 105.0f);
                    _gizmoLocalScaleButtonRect.y = _selectedPivot.y + (_gizmoLocalScaleButtonRect.height + 85.0f);

                    _gizmoImageScaleButtonRect.x = _selectedPivot.x - (_gizmoLocalScaleButtonRect.width + 105.0f);
                    _gizmoImageScaleButtonRect.y = _selectedPivot.y + ((_gizmoLocalScaleButtonRect.height * 2.0f) + 90.0f);

                    _moveFreeRect.x = _selectedPivot.x - (_moveFreeRect.width / 2.0f);
                    _moveFreeRect.y = _selectedPivot.y - (_moveFreeRect.height / 2.0f);

                    _depthUpRect.x = _selectedPivot.x - (_depthUpRect.width * 0.5f);
                    _depthUpRect.y = _selectedPivot.y - (190.0f);

                    _depthLabelRect.x = _selectedPivot.x - (_depthLabelRect.width * 0.5f);
                    _depthLabelRect.y = _selectedPivot.y - (182.0f);

                    _depthDownRect.x = _selectedPivot.x - (_depthDownRect.width * 0.5f);
                    _depthDownRect.y = _selectedPivot.y - (160.0f);

                    _keyframeLabelRect.x = _selectedPivot.x - 165.0f;
                    _keyframeLabelRect.y = _selectedPivot.y - 190.0f;

                    _boneLabelRect.x = _selectedPivot.x - 160.0f;
                    _boneLabelRect.y = _selectedPivot.y - 190.0f;

                    _frameLabelRect.x = _selectedPivot.x - 160.0f;
                    _frameLabelRect.y = _selectedPivot.y - 175.0f;

                    SetGizmoLabelPosition(ref _scaleXLabelRect, _selectedPivot, 180.0f, _parentRotation);
                    SetGizmoLabelPosition(ref _scaleYLabelRect, _selectedPivot, 270.0f, _parentRotation);
                    SetGizmoLabelPosition(ref _positionLabelRect, _selectedPivot, 45.0f, _parentRotation);
                    SetGizmoLabelPosition(ref _rotationLabelRect, _selectedPivot, 315.0f, _parentRotation);

                    _moveYArc.Update(_selectedPivot, _parentRotation);
                    _moveXArc.Update(_selectedPivot, _parentRotation);
                    _scaleYArc.Update(_selectedPivot, _parentRotation);
                    _scaleXArc.Update(_selectedPivot, _parentRotation);
                    _scaleUniformArc.Update(_selectedPivot, _parentRotation);

                    _gizmoMoveX = animationWindowBone.gizmoMoveX;
                    _gizmoMoveY = animationWindowBone.gizmoMoveY;
                    _gizmoRotation = animationWindowBone.gizmoRotation;
                    _gizmoScaleX = animationWindowBone.gizmoScaleX;
                    _gizmoScaleY = animationWindowBone.gizmoScaleY;
                    _gizmoDepth = animationWindowBone.gizmoDepth;

                    Style.PushColor(animationWindowBone.color);

                    if (SettingsWindow.AnimationShowSelectedBoneBounds)
                    {
                        GUIHelper.DrawBox(animationWindowBone.drawRect, Style.boundsStyle, true);
                    }
                }
                else
                {
                    if (TimelineWindow.KeyframesSelected)
                        Style.PushColor(animationWindowBone.color, 1.0f - SettingsWindow.AnimationNonSelectionDarkenFactor);
                    else
                        Style.PushColor(animationWindowBone.color);
                }

                if (animationWindowBone.texture != null)
                    GUI.DrawTexture(animationWindowBone.drawRect, animationWindowBone.texture);

                Style.PopColor();
            }

            // reset the GUI matrix
            GUI.matrix = Matrix4x4.identity;

            // draw each bone collider
            foreach (AnimationWindowBone animationWindowBone in _animationWindowBones)
            {
                if (animationWindowBone.visible)
                {
                    DrawCollider(animationWindowBone);
                }
            }

            // reset the GUI matrix
            GUI.matrix = Matrix4x4.identity;
        }

        static private void SetGizmoLabelPosition(ref Rect rect, Vector2 offset, float angle, float parentAngle)
        {
            float totalAngle = EditorHelper.KeepAngleInBounds(angle - parentAngle);
            float xRadius = GIZMO_LABEL_RADIUS;

            if (totalAngle > 90.0f && totalAngle < 270.0f)
            {
                xRadius += rect.width;
            }

            rect.x = offset.x + (Mathf.Cos(-totalAngle * Mathf.Deg2Rad) * xRadius);
            rect.y = offset.y + (Mathf.Sin(-totalAngle * Mathf.Deg2Rad) * GIZMO_LABEL_RADIUS) - (rect.height * 0.5f);
        }

        static private void DrawGizmos()
        {
            if (_drawGizmos)
            {
                if (TimelineWindow.OneKeyframeSelected)
                {
                    if (Resources.gizmoRotationRingTexture != null)
                    {
                        if (TimelineWindow.AllSelectedKeyframesInSameFrame)
                        {
                            if (SettingsWindow.AnimationContrastDark)
                                Style.PushColor(Style.boundsStyle);
                            else
                                Style.PushColor(Style.boundsDarkStyle);

                            Matrix4x4 backupMatrix;
                            Matrix4x4 backupMatrix2;

                            if (TimelineWindow.FirstSelectedKeyframe.frame == TimelineWindow.CurrentFrame)
                            {
                                if (SettingsWindow.AnimationDrawGizmoLabels)
                                {
                                    DrawGizmo(_keyframeLabelRect, Resources.gizmoMoveFreeBoxTexture, false);

                                    GUI.Label(_boneLabelRect,
                                              editor.boneAnimationData.boneDataList[TimelineWindow.FirstSelectedKeyframe.boneDataIndex].boneName,
                                              Style.animationLabelStyle);
                                    GUI.Label(_frameLabelRect,
                                              "Frame: " + TimelineWindow.FirstSelectedKeyframe.frame.ToString(),
                                              Style.animationLabelStyle);
                                }

                                if (GUI.Button(_gizmoLocalScaleButtonRect, (SettingsWindow.AnimationScaleGizmoImage ? Resources.localScaleButtonOff : Resources.localScaleButtonOn), Style.noBorderButtonStyle))
                                {
                                    SettingsWindow.AnimationScaleGizmoImage = false;
                                }
                                if (GUI.Button(_gizmoImageScaleButtonRect, (SettingsWindow.AnimationScaleGizmoImage ? Resources.imageScaleButtonOn : Resources.imageScaleButtonOff), Style.noBorderButtonStyle))
                                {
                                    SettingsWindow.AnimationScaleGizmoImage = true;
                                }
                            }

                            if (LastKeyframe.keyframeType == KeyframeSM.KEYFRAME_TYPE.Image)
                            {
                                if (_gizmoDepth)
                                {
                                    DrawGizmo(_depthUpRect, Resources.gizmoDepthUpTexture, _action == ACTION.MoveDepthUp);
                                    if (SettingsWindow.AnimationDrawGizmoLabels)
                                    {
                                        GUI.Label(_depthLabelRect, "Depth: " + TimelineWindow.FirstSelectedKeyframe.depth.ToString(), Style.animationLabelStyle);
                                    }
                                    DrawGizmo(_depthDownRect, Resources.gizmoDepthDownTexture, _action == ACTION.MoveDepthDown);
                                }

                                if (SettingsWindow.AnimationDrawGizmoLabels)
                                {
                                    if (SettingsWindow.AnimationScaleGizmoImage)
                                    {
                                        if (_gizmoScaleX)
                                            GUI.Label(_scaleXLabelRect, "X: " + TimelineWindow.FirstSelectedKeyframe.imageScale.val.x.ToString(), Style.animationLabelStyle);
                                        if (_gizmoScaleY)
                                            GUI.Label(_scaleYLabelRect, "Y: " + TimelineWindow.FirstSelectedKeyframe.imageScale.val.y.ToString(), Style.animationLabelStyle);
                                    }
                                    else
                                    {
                                        if (_gizmoScaleX)
                                            GUI.Label(_scaleXLabelRect, "X: " + TimelineWindow.FirstSelectedKeyframe.localScale3.val.x.ToString(), Style.animationLabelStyle);
                                        if (_gizmoScaleY)
                                            GUI.Label(_scaleYLabelRect, "Y: " + TimelineWindow.FirstSelectedKeyframe.localScale3.val.y.ToString(), Style.animationLabelStyle);
                                    }
                                }
                            }

                            if (SettingsWindow.AnimationDrawGizmoLabels)
                            {
                                if (_gizmoMoveX || _gizmoMoveY)
                                    GUI.Label(_positionLabelRect, "Pos: " + TimelineWindow.FirstSelectedKeyframe.localPosition3.val.ToString(), Style.animationLabelStyle);
                                if (_gizmoRotation)
                                    GUI.Label(_rotationLabelRect, "Rot: " + TimelineWindow.FirstSelectedKeyframe.localRotation.val.ToString(), Style.animationLabelStyle);
                            }

                            backupMatrix = GUI.matrix;

                            GUIUtility.RotateAroundPivot(_parentRotation, _selectedPivot);

                            if (_gizmoRotation)
                            {
                                backupMatrix2 = GUI.matrix;

                                GUIUtility.RotateAroundPivot(_selectedRotation - _parentRotation, _selectedPivot);
                                DrawGizmo(_gizmoRotationRingRect, Resources.gizmoRotationRingTexture, _action == ACTION.Rotate);

                                GUI.matrix = backupMatrix2;
                            }

                            if (_gizmoMoveX && _gizmoMoveY)
                                DrawGizmo(_gizmoMoveFreeBoxRect, Resources.gizmoMoveFreeBoxTexture, _action == ACTION.MoveFree);

                            if (_gizmoMoveY)
                                DrawGizmo(_gizmoMoveArrowRect, Resources.gizmoMoveArrowTexture, _action == ACTION.MoveY);

                            backupMatrix2 = GUI.matrix;

                            GUIUtility.RotateAroundPivot(90.0f, _selectedPivot);
                            if (_gizmoMoveX)
                                DrawGizmo(_gizmoMoveArrowRect, Resources.gizmoMoveArrowTexture, _action == ACTION.MoveX);

                            GUIUtility.RotateAroundPivot(90.0f, _selectedPivot);
                            if (_gizmoScaleY)
                                DrawGizmo(_gizmoScaleBarRect, Resources.gizmoScaleBarTexture, (_action == ACTION.ImageScaleY || _action == ACTION.LocalScaleY));

                            GUIUtility.RotateAroundPivot(45.0f, _selectedPivot);
                            if (_gizmoScaleX && _gizmoScaleY)
                                DrawGizmo(_gizmoScaleBarRect, Resources.gizmoScaleBarTexture, (_action == ACTION.ImageScaleUniform || _action == ACTION.LocalScaleUniform));

                            GUIUtility.RotateAroundPivot(45.0f, _selectedPivot);
                            if (_gizmoScaleX)
                                DrawGizmo(_gizmoScaleBarRect, Resources.gizmoScaleBarTexture, (_action == ACTION.ImageScaleX || _action == ACTION.LocalScaleX));

                            GUI.matrix = backupMatrix2;

                            GUI.matrix = backupMatrix;

                            Style.PopColor();
                        }
                    }
                }
            }
        }

        static public void DrawGizmo(Rect gizmoRect, Texture2D texture, bool selected)
        {
            if (selected)
                Style.PushColor(Style.targetStyle);

            GUI.DrawTexture(gizmoRect, texture);

            if (selected)
                Style.PopColor();
        }

        static private void DrawCollider(AnimationWindowBone animationWindowBone)
        {
            if (animationWindowBone.collider != null)
            {
                if (animationWindowBone.collider.type != ColliderSM.COLLIDER_TYPE.None)
                {
                    switch (animationWindowBone.collider.type)
                    {
                        case ColliderSM.COLLIDER_TYPE.Box:
                            Rect drawRect = new Rect(0,
                                                0,
                                                animationWindowBone.collider.boxSize.x * animationWindowBone.globalScale.x,
                                                animationWindowBone.collider.boxSize.y * animationWindowBone.globalScale.y
                                                );
                            Vector3 pivot = new Vector3(animationWindowBone.globalColliderCenter.x, animationWindowBone.globalColliderCenter.y)
                                            - (new Vector3(drawRect.width, drawRect.height) * 0.5f);

                            GUI.matrix = Matrix4x4.TRS(pivot, Quaternion.identity, Vector3.one);
                            GUIUtility.RotateAroundPivot(-animationWindowBone.globalRotation, animationWindowBone.globalColliderCenter);
                            GUIHelper.DrawBox(drawRect, Style.colliderStyle);

                            // reset the matrix
                            GUI.matrix = Matrix4x4.identity;
                            break;

                        case ColliderSM.COLLIDER_TYPE.Sphere:
                            // no matrix calculations needed for the sphere collider since everything is in global coordinates

                            GUIHelper.DrawCircle(animationWindowBone.globalColliderCenter,
                                                    animationWindowBone.collider.sphereRadius * Mathf.Max(animationWindowBone.globalScale.x, animationWindowBone.globalScale.y),
                                                    SPHERE_COLLIDER_POINTS,
                                                    Style.colliderStyle);
                            break;
                    }
                }
            }
        }

        static public void GetInput(Event evt)
        {
            Vector2 areaMousePos;

            if (evt.type == EventType.MouseDown && _areaRect.Contains(evt.mousePosition))
            {
                editor.KeyboardFocus = BoneAnimationDataEditorWindow.KEYBOARD_FOCUS.Animation;
            }

            if (_areaRect.Contains(evt.mousePosition))
            {
                areaMousePos = evt.mousePosition - GUIHelper.UpperLeftCorner(_areaRect) + _offset;

                switch (evt.type)
                {
                    case EventType.MouseDown:
                        _dragStartPosition = areaMousePos;
                        _dragStartOrigin = _animationOriginOffset;

                        if (ClipBrowserWindow.SelectedAnimationClipIndex != -1 && !TimelineWindow.IsPlaying && !editor.ModalPopup)
                        {
                            if (EditorHelper.RightMouseButton(evt))
                            {
                                if (_selectedAnimationWindowBone != null)
                                {
                                    GenericMenu frameContextMenu = new GenericMenu();
                                    frameContextMenu.AddItem(new GUIContent("Reset Transform" + (TimelineWindow.OneKeyframeSelected ? "" : "s")), false, AnimationContextMenuCallback_ResetTransform, null);
                                    if (_selectedAnimationWindowBone.visible)
                                    {
                                        frameContextMenu.AddItem(new GUIContent("Hide Bone" + (TimelineWindow.OneKeyframeSelected ? "" : "s")), false, AnimationContextMenuCallback_HideBone, null);
                                    }
                                    else
                                    {
                                        frameContextMenu.AddItem(new GUIContent("Show Bone" + (TimelineWindow.OneKeyframeSelected ? "" : "s")), false, AnimationContextMenuCallback_ShowBone, null);
                                    }
                                    frameContextMenu.ShowAsContext();
                                }
                                evt.Use();
                            }

                            if (EditorHelper.LeftMouseButton(evt) && TimelineWindow.OneKeyframeSelected)
                            {
                                _objectStartPosition = new Vector2(TimelineWindow.FirstSelectedKeyframe.localPosition3.val.x, TimelineWindow.FirstSelectedKeyframe.localPosition3.val.y);
                                _objectStartScale = (SettingsWindow.AnimationScaleGizmoImage ? TimelineWindow.FirstSelectedKeyframe.imageScale.val : EditorHelper.Vector3ToVector2(TimelineWindow.FirstSelectedKeyframe.localScale3.val));
                                _objectClickDistance = (_dragStartPosition - _selectedPivot).magnitude;

                                if (_moveFreeRect.Contains(areaMousePos) && _gizmoMoveX && _gizmoMoveY)
                                {
                                    _action = ACTION.MoveFree;
                                    _willBeDirty = true;
                                    evt.Use();
                                }
                                else if (_moveYArc.CheckCollision(areaMousePos) && _gizmoMoveY)
                                {
                                    _action = ACTION.MoveY;
                                    _willBeDirty = true;
                                    evt.Use();
                                }
                                else if (_moveXArc.CheckCollision(areaMousePos) && _gizmoMoveX)
                                {
                                    _action = ACTION.MoveX;
                                    _willBeDirty = true;
                                    evt.Use();
                                }
                                else if (_scaleYArc.CheckCollision(areaMousePos) && _gizmoScaleY)
                                {
                                    _action = (SettingsWindow.AnimationScaleGizmoImage ? ACTION.ImageScaleY : ACTION.LocalScaleY);
                                    _willBeDirty = true;
                                    evt.Use();
                                }
                                else if (_scaleXArc.CheckCollision(areaMousePos) && _gizmoScaleX)
                                {
                                    _action = (SettingsWindow.AnimationScaleGizmoImage ? ACTION.ImageScaleX : ACTION.LocalScaleX);
                                    _willBeDirty = true;
                                    evt.Use();
                                }
                                else if (_scaleUniformArc.CheckCollision(areaMousePos) && _gizmoScaleX && _gizmoScaleY)
                                {
                                    _action = (SettingsWindow.AnimationScaleGizmoImage ? ACTION.ImageScaleUniform : ACTION.LocalScaleUniform);
                                    _willBeDirty = true;
                                    evt.Use();
                                }
                                else if (_depthUpRect.Contains(areaMousePos) && _gizmoDepth)
                                {
                                    WillBeDirty();

                                    _action = ACTION.MoveDepthUp;
                                    TimelineWindow.FirstSelectedKeyframe.Depth--;
                                    LastKeyframe.depth = TimelineWindow.FirstSelectedKeyframe.depth;
                                    //_willBeDirty = true;
                                    evt.Use();
                                }
                                else if (_depthDownRect.Contains(areaMousePos) && _gizmoDepth)
                                {
                                    WillBeDirty();

                                    _action = ACTION.MoveDepthDown;
                                    TimelineWindow.FirstSelectedKeyframe.Depth++;
                                    LastKeyframe.depth = TimelineWindow.FirstSelectedKeyframe.depth;
                                    //_willBeDirty = true;
                                    evt.Use();
                                }
                                else
                                {
                                    float distance = (areaMousePos - _selectedPivot).sqrMagnitude;
                                    if (distance >= _rotationRingInnerMagSqr && distance <= _rotationRingOuterMagSqr)
                                    {
                                        _action = ACTION.Rotate;
                                        _willBeDirty = true;
                                        evt.Use();
                                    }
                                    else
                                    {
                                        _action = ACTION.None;
                                    }
                                }

                                if (_action == AnimationWindow.ACTION.None)
                                {
                                    CheckClickOnEmptyKeyframe(areaMousePos, evt);
                                    evt.Use();
                                }
                            }
                            else if (EditorHelper.LeftMouseButton(evt))
                            {
                                CheckClickOnEmptyKeyframe(areaMousePos, evt);
                                evt.Use();
                            }
                            else if (EditorHelper.MiddleMouseButton(evt))
                            {
                                _action = ACTION.DragScreen;
                                evt.Use();
                            }
                            else
                            {
                                _action = ACTION.None;
                            }
                        }
                        else
                        {
                            if (EditorHelper.MiddleMouseButton(evt))
                            {
                                _action = ACTION.DragScreen;
                                evt.Use();
                            }
                        }
                        break;

                    case EventType.MouseDrag:
                        Matrix4x4 matRotate = Matrix4x4.identity;
                        Vector2 dragDelta = new Vector3((areaMousePos.x - _dragStartPosition.x), (_dragStartPosition.y - areaMousePos.y), 0);
                        Vector3 rotatedDelta = Vector3.zero;

                        switch (_action)
                        {
                            case ACTION.DragScreen:
                                _animationOriginOffset = _dragStartOrigin + (areaMousePos - _dragStartPosition);
                                editor.SetNeedsRepainted();
                                evt.Use();
                                break;
                        }

                        if (_selectedAnimationWindowBone != null)
                        {
                            KeyframeSM keyframe = TimelineWindow.FirstSelectedKeyframe;
                            BoneCurves boneCurves = AnimationHelper.GetBoneDataCurves(keyframe.boneDataIndex);
                            float frame = keyframe.frame;
                            int keyIndex = -1;
                            int clipIndex = ClipBrowserWindow.SelectedAnimationClipIndex;
                            float currentDistance;
                            float uniformScale;

                            switch (_action)
                            {
                                case ACTION.MoveY:
                                case ACTION.MoveX:
                                case ACTION.MoveFree:
                                case ACTION.ImageScaleY:
                                case ACTION.ImageScaleX:
                                case ACTION.LocalScaleY:
                                case ACTION.LocalScaleX:
                                    matRotate = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, _parentRotation), Vector3.one);
                                    rotatedDelta = matRotate.MultiplyPoint3x4(dragDelta);
                                    rotatedDelta.x /= _selectedAnimationWindowBone.parentGlobalScale.x;
                                    rotatedDelta.y /= _selectedAnimationWindowBone.parentGlobalScale.y;
                                    break;
                            }

                            switch (_action)
                            {
                                case ACTION.MoveY:
                                    if (_willBeDirty)
                                        WillBeDirty();

                                    keyframe.localPosition3.val.y = rotatedDelta.y + _objectStartPosition.y;
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.localPositionYCurve, frame);
                                    ChangeKey(keyIndex, keyframe.localPosition3.val.y, boneCurves.localPositionYCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.LocalPositionY);

                                    evt.Use();
                                    break;

                                case ACTION.MoveX:
                                    if (_willBeDirty)
                                        WillBeDirty();

                                    keyframe.localPosition3.val.x = rotatedDelta.x + _objectStartPosition.x;
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.localPositionXCurve, frame);
                                    ChangeKey(keyIndex, keyframe.localPosition3.val.x, boneCurves.localPositionXCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.LocalPositionX);
                                    evt.Use();

                                    //_willBeDirty = true;
                                    break;

                                case ACTION.MoveFree:
                                    if (_willBeDirty)
                                        WillBeDirty();

                                    keyframe.localPosition3.val.x = rotatedDelta.x + _objectStartPosition.x;
                                    keyframe.localPosition3.val.y = rotatedDelta.y + _objectStartPosition.y;
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.localPositionXCurve, frame);
                                    ChangeKey(keyIndex, keyframe.localPosition3.val.x, boneCurves.localPositionXCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.LocalPositionX);
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.localPositionYCurve, frame);
                                    ChangeKey(keyIndex, keyframe.localPosition3.val.y, boneCurves.localPositionYCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.LocalPositionY);
                                    evt.Use();
                                    break;

                                case ACTION.ImageScaleY:
                                    if (_willBeDirty)
                                        WillBeDirty();

                                    keyframe.imageScale.val.y = (-rotatedDelta.y * SCALE_GIZMO_FACTOR) + _objectStartScale.y;
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.imageScaleYCurve, frame);
                                    ChangeKey(keyIndex, keyframe.imageScale.val.y, boneCurves.imageScaleYCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.ImageScaleY);
                                    evt.Use();
                                    break;

                                case ACTION.ImageScaleX:
                                    if (_willBeDirty)
                                        WillBeDirty();

                                    keyframe.imageScale.val.x = (-rotatedDelta.x * SCALE_GIZMO_FACTOR) + _objectStartScale.x;
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.imageScaleXCurve, frame);
                                    ChangeKey(keyIndex, keyframe.imageScale.val.x, boneCurves.imageScaleXCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.ImageScaleX);
                                    evt.Use();
                                    break;

                                case ACTION.ImageScaleUniform:
                                    if (_willBeDirty)
                                        WillBeDirty();

                                    currentDistance = (areaMousePos - _selectedPivot).magnitude;
                                    uniformScale = (currentDistance - _objectClickDistance) * SCALE_GIZMO_FACTOR;
                                    keyframe.imageScale.val = _objectStartScale + new Vector2(uniformScale, uniformScale);
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.imageScaleXCurve, frame);
                                    ChangeKey(keyIndex, keyframe.imageScale.val.x, boneCurves.imageScaleXCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.ImageScaleX);
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.imageScaleYCurve, frame);
                                    ChangeKey(keyIndex, keyframe.imageScale.val.y, boneCurves.imageScaleYCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.ImageScaleY);
                                    evt.Use();
                                    break;

                                case ACTION.LocalScaleY:
                                    if (_willBeDirty)
                                        WillBeDirty();

                                    keyframe.localScale3.val.y = (-rotatedDelta.y * SCALE_GIZMO_FACTOR) + _objectStartScale.y;
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.localScaleYCurve, frame);
                                    ChangeKey(keyIndex, keyframe.localScale3.val.y, boneCurves.localScaleYCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.LocalScaleY);
                                    evt.Use();
                                    break;

                                case ACTION.LocalScaleX:
                                    if (_willBeDirty)
                                        WillBeDirty();

                                    keyframe.localScale3.val.x = (-rotatedDelta.x * SCALE_GIZMO_FACTOR) + _objectStartScale.x;
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.localScaleXCurve, frame);
                                    ChangeKey(keyIndex, keyframe.localScale3.val.x, boneCurves.localScaleXCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.LocalScaleX);
                                    evt.Use();
                                    break;

                                case ACTION.LocalScaleUniform:
                                    if (_willBeDirty)
                                        WillBeDirty();

                                    currentDistance = (areaMousePos - _selectedPivot).magnitude;
                                    uniformScale = (currentDistance - _objectClickDistance) * SCALE_GIZMO_FACTOR;
                                    keyframe.localScale3.val = _objectStartScale + new Vector2(uniformScale, uniformScale);
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.localScaleXCurve, frame);
                                    ChangeKey(keyIndex, keyframe.localScale3.val.x, boneCurves.localScaleXCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.LocalScaleX);
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.localScaleYCurve, frame);
                                    ChangeKey(keyIndex, keyframe.localScale3.val.y, boneCurves.localScaleYCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.LocalScaleY);
                                    evt.Use();
                                    break;

                                case ACTION.Rotate:
                                    if (_willBeDirty)
                                        WillBeDirty();

                                    float angle = keyframe.localRotation.val;
                                    angle += (evt.delta.y * ROTATION_SENSITIVITY) * Mathf.Sign(_selectedPivot.x - areaMousePos.x);
                                    angle += (evt.delta.x * ROTATION_SENSITIVITY) * -Mathf.Sign(_selectedPivot.y - areaMousePos.y);
                                    keyframe.localRotation.val = angle;
                                    keyIndex = AnimationHelper.GetKeyIndex(boneCurves.localRotationCurve, frame);
                                    ChangeKey(keyIndex, keyframe.localRotation.val, boneCurves.localRotationCurve, clipIndex, keyframe, KeyframeSM.CURVE_PROPERTY.LocalRotation);
                                    AnimationHelper.BakeQuaternionCurves(ref boneCurves.localRotationCurve,
                                                                                        ref boneCurves.localRotationXCurve,
                                                                                        ref boneCurves.localRotationYCurve,
                                                                                        ref boneCurves.localRotationZCurve,
                                                                                        ref boneCurves.localRotationWCurve);
                                    evt.Use();
                                    break;
                            }
                        }
                        break;

                    case EventType.MouseUp:
                        _action = ACTION.None;

                        evt.Use();
                        break;

                    case EventType.ScrollWheel:
                        float previousZoom = _zoom;
                        _zoom = Mathf.Clamp((_zoom - (evt.delta.y * SCROLL_WHEEL_SENSITIVITY)), AnimationControlsWindow.ZOOM_MIN, AnimationControlsWindow.ZOOM_MAX);
                        // change the animation origin position so that the zoom centers around the mouse
                        _animationOriginOffset = (areaMousePos - _offset) + (((_animationOriginOffset + _offset) - areaMousePos) * (_zoom / previousZoom));
                        editor.SetNeedsRepainted();
                        evt.Use();
                        break;
                }

                //if (_willBeDirty)
                //{
                //    Debug.Log("AnimationWindow: will be dirty");

                //    _willBeDirty = false;
                //    editor.SetWillBeDirty();
                //}

            }
        }

        static private void WillBeDirty()
        {
            //Debug.Log("AnimationWindow: will be dirty");

            editor.SetWillBeDirty();

            _willBeDirty = false;
        }

        static private void CheckClickOnEmptyKeyframe(Vector2 areaMousePos, Event evt)
        {
            AnimationWindowBone animationWindowBone;

            // traverse the bone list in depth ascending order so that bones on top will be clicked before those on bottom
            for (int i = _animationWindowBones.Count - 1; i >= 0; i--)
            {
                animationWindowBone = _animationWindowBones[i];

                // ignore bones that aren't visible
                if (animationWindowBone.visible)
                {
                    if (EditorHelper.TransformedRectContains(animationWindowBone.drawRect, animationWindowBone.drawMatrix, areaMousePos))
                    {
                        TimelineWindow.SelectFrame(animationWindowBone.boneNodeIndex, TimelineWindow.CurrentFrame);

                        if (!TimelineWindow.KeyframesSelected)
                        {
                            GenericMenu frameContextMenu = new GenericMenu();
                            frameContextMenu.AddItem(new GUIContent("Add Pos - Rot Keyframe for '" + animationWindowBone.boneName + "' at frame " + TimelineWindow.CurrentFrame.ToString() + "?"), false, AnimationContextMenuCallback_AddPositionRotationKeyframe, null);
                            frameContextMenu.AddItem(new GUIContent("Cancel"), false, AnimationContextMenuCallback_AddKeyframeCancel, null);
                            frameContextMenu.ShowAsContext();
                        }

                        break;
                    }
                }
            }
        }

        static private void ChangeKey(int keyIndex, float value, AnimationCurve curve, int clipIndex, KeyframeSM keyframe, KeyframeSM.CURVE_PROPERTY curveProperty)
        {
            Keyframe key = curve.keys[keyIndex];
            key.value = value;
            curve.MoveKey(keyIndex, key);

            int boneDataIndex = keyframe.boneDataIndex;

            AnimationHelper.AdjustLinearTangents(curve, editor.boneAnimationData.GetAnimationClipBoneFromBoneDataIndex(clipIndex, boneDataIndex), curveProperty);

            //AnimationHelper.ResetRefreshAnimationCurveEditorBoneDataIndexList();
            //AnimationHelper.AddBoneDataIndexToRefreshList(boneDataIndex);
            AnimationHelper.refreshAnimationEditorWindowPostCycle = true;
        }

        static public void AnimationContextMenuCallback_ResetTransform(System.Object obj)
        {
            TimelineWindow.ResetKeyframeTransforms();
        }

        static public void AnimationContextMenuCallback_HideBone(System.Object obj)
        {
            TimelineWindow.HideBones(true);
        }

        static public void AnimationContextMenuCallback_ShowBone(System.Object obj)
        {
            TimelineWindow.HideBones(false);
        }
		
		static public void AnimationContextMenuCallback_AddPositionRotationKeyframe(System.Object obj)
		{
			if (TimelineWindow.SelectedFrames.Count == 1)
			{
				BoneFrame boneFrame = TimelineWindow.SelectedFrames[0];
				
				KeyframeSM keyframe = TimelineWindow.AddKeyframe(ClipBrowserWindow.SelectedAnimationClipIndex, 
				                                       boneFrame.boneNodeIndex,
			        	                               boneFrame.frame,
				        	                           AnimationClipBone.KEYFRAME_COPY_MODE.None,
                                                       AnimationClipBone.DEFAULT_SETTING.PositionRotation,
				            	                       true);

                BoneCurves boneCurves = AnimationHelper.GetBoneDataCurves(keyframe.boneDataIndex);
                if (boneCurves != null)
                {
                    keyframe.localPosition3.val.x = boneCurves.localPositionXCurve.Evaluate(keyframe.frame);
                    keyframe.localPosition3.val.y = boneCurves.localPositionYCurve.Evaluate(keyframe.frame);
                    keyframe.localPosition3.val.z = boneCurves.localPositionZCurve.Evaluate(keyframe.frame);
                    keyframe.localRotation.val = boneCurves.localRotationCurve.Evaluate(keyframe.frame);
                }
			}
		}
		
		static public void AnimationContextMenuCallback_AddKeyframeCancel(System.Object obj)
		{
		}

        static public void CenterOrigin()
        {
            if (_areaRect.height <= 100)
            {
                _needsRecentered = true;
                return;
            }

            _animationOriginOffset = new Vector2((_areaRect.width * 0.5f), _areaRect.yMax - 80.0f);
        }

        static public void LostFocus()
        {
        }
    }
}
