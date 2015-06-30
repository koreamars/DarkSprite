using UnityEngine;
using System.Collections.Generic;

namespace SmoothMoves
{
    [System.Serializable]
    public class TriggerFrameBone
    {
        public enum TRIGGER_EVENT_TYPE
        {
            // This doesn't change the mesh, just notifies the user of an event
            UserTrigger,

            // These three require the mesh to be regenerated
            HideTexture,
            ChangeMaterial,
            ChangeDepth,

            // These only require elements of the mesh to be updated
            ChangeTexture,
            ChangePivot,

            // These are outside of the mesh
            ChangeCollider
        }

        public string userTriggerTag;

        public int boneNodeIndex;
        public List<TRIGGER_EVENT_TYPE> triggerEventTypes;

        public Vector2 pivotOffset;

        public Vector3 upperLeft;
        public Vector3 bottomLeft;
        public Vector3 bottomRight;
        public Vector3 upperRight;

        public Rect uv;

        public Vector3 localPosition;
        public float localRotation;
        public Vector2 localScale;

        public TextureAtlas atlas;
        public string textureGUID;
        public int materialIndex;
        public int depth;
        public ColliderSM collider;

        public TextureAtlas originalAtlas;
        public string originalTextureGUID;
        public int originalMaterialIndex;
        public Rect originalUV;
        public Vector2 originalPivotOffset;

        public TriggerFrameBone(TriggerFrameBone copyTriggerBone)
        {
            userTriggerTag = copyTriggerBone.userTriggerTag;
            boneNodeIndex = copyTriggerBone.boneNodeIndex;
            triggerEventTypes = new List<TRIGGER_EVENT_TYPE>();
            foreach (TRIGGER_EVENT_TYPE copyTriggerEventType in copyTriggerBone.triggerEventTypes)
            {
                triggerEventTypes.Add(copyTriggerEventType);
            }
            pivotOffset = copyTriggerBone.pivotOffset;
            upperLeft = copyTriggerBone.upperLeft;
            bottomLeft = copyTriggerBone.bottomLeft;
            bottomRight = copyTriggerBone.bottomRight;
            upperRight = copyTriggerBone.upperRight;

            uv = copyTriggerBone.uv;

            localPosition = copyTriggerBone.localPosition;
            localRotation = copyTriggerBone.localRotation;
            localScale = copyTriggerBone.localScale;

            atlas = copyTriggerBone.atlas;
            textureGUID = copyTriggerBone.textureGUID;
            materialIndex = copyTriggerBone.materialIndex;
            depth = copyTriggerBone.depth;
            collider = new ColliderSM(copyTriggerBone.collider);

            originalAtlas = copyTriggerBone.originalAtlas;
            originalTextureGUID = copyTriggerBone.originalTextureGUID;
            originalMaterialIndex = copyTriggerBone.originalMaterialIndex;
            originalUV = copyTriggerBone.originalUV;
            originalPivotOffset = copyTriggerBone.originalPivotOffset;
        }

        public TriggerFrameBone(int bnIndex)
        {
            boneNodeIndex = bnIndex;
            triggerEventTypes = new List<TRIGGER_EVENT_TYPE>();
        }

        public void AddUserTriggerEvent(string tag)
        {
            AddTriggerEventType(TRIGGER_EVENT_TYPE.UserTrigger);
            userTriggerTag = tag;
        }

        public void AddHideTextureEvent()
        {
            ClearTriggerEventTypes();
            AddTriggerEventType(TRIGGER_EVENT_TYPE.HideTexture);
        }

        public void AddChangeMaterialEvent(int matIndex)
        {
            AddTriggerEventType(TRIGGER_EVENT_TYPE.ChangeMaterial);
            materialIndex = matIndex;
        }

        public void AddChangeDepthEvent(int dep)
        {
            AddTriggerEventType(TRIGGER_EVENT_TYPE.ChangeDepth);
            depth = dep;
        }

        public void AddChangeTextureEvent(TextureAtlas atlas, string textureGUID, Vector2 pivotOffset, int matIndex)
        {
            AddTriggerEventType(TRIGGER_EVENT_TYPE.ChangeTexture);
            CalculateVertices(atlas, textureGUID, pivotOffset);
            materialIndex = matIndex;
        }

        public void AddChangePivotEvent(TextureAtlas atlas, string textureGUID, Vector2 pivotOffset)
        {
            AddTriggerEventType(TRIGGER_EVENT_TYPE.ChangePivot);
            CalculateVertices(atlas, textureGUID, pivotOffset);
        }

        public void AddChangeColliderEvent(ColliderSM coll)
        {
            AddTriggerEventType(TRIGGER_EVENT_TYPE.ChangeCollider);
            collider = coll;
        }

        private void AddTriggerEventType(TRIGGER_EVENT_TYPE triggerEventType)
        {
            if (!triggerEventTypes.Contains(triggerEventType))
            {
                triggerEventTypes.Add(triggerEventType);
            }
        }

        public void CalculateVertices(TextureAtlas atlas, string textureGUID, Vector2 pivot)
        {
            this.atlas = atlas;
            this.textureGUID = textureGUID;

            if (atlas == null)
                return;

            pivotOffset = pivot;

            int textureIndex = atlas.GetTextureIndex(textureGUID);
            Vector2 textureSize;
            Vector2 textureHalfSize;

            if (textureIndex != -1)
            {
                textureSize = atlas.textureSizes[textureIndex];
                textureHalfSize = textureSize * 0.5f;

                upperLeft = new Vector3((-pivotOffset.x * textureSize.x) - textureHalfSize.x,
                                                       (-pivotOffset.y * textureSize.y) + textureHalfSize.y,
                                                       0);

                bottomLeft = new Vector3((-pivotOffset.x * textureSize.x) - textureHalfSize.x,
                                                        (-pivotOffset.y * textureSize.y) - textureHalfSize.y,
                                                        0);

                bottomRight = new Vector3((-pivotOffset.x * textureSize.x) + textureHalfSize.x,
                                                         (-pivotOffset.y * textureSize.y) - textureHalfSize.y,
                                                         0);

                upperRight = new Vector3((-pivotOffset.x * textureSize.x) + textureHalfSize.x,
                                                        (-pivotOffset.y * textureSize.y) + textureHalfSize.y,
                                                        0);

                uv = atlas.uvs[textureIndex];
            }
        }

        public void CaptureOriginalSettings()
        {
            originalAtlas = atlas;
            originalTextureGUID = textureGUID;
            originalMaterialIndex = materialIndex;
            originalUV = uv;
            originalPivotOffset = pivotOffset;
        }

        public void RestoreOriginalSettings()
        {
            atlas = originalAtlas;
            textureGUID = originalTextureGUID;
            materialIndex = originalMaterialIndex;
            uv = originalUV;
            pivotOffset = originalPivotOffset;

            CalculateVertices(atlas, textureGUID, pivotOffset);
        }

        private void ClearTriggerEventTypes()
        {
            triggerEventTypes.Clear();
        }
    }
}
