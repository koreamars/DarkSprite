using UnityEngine;
using System.Collections.Generic;

namespace SmoothMoves
{
    [System.Serializable]
    public class TriggerFrame
    {
        public int clipIndex;
        public int frame;

        public List<TriggerFrameBone> triggerFrameBones;

        public TriggerFrame(TriggerFrame copyTriggerFrame)
        {
            clipIndex = copyTriggerFrame.clipIndex;
            frame = copyTriggerFrame.frame;

            triggerFrameBones = new List<TriggerFrameBone>();
            TriggerFrameBone newTriggerFrameBone;
            foreach (TriggerFrameBone copyTriggerFrameBone in copyTriggerFrame.triggerFrameBones)
            {
                newTriggerFrameBone = new TriggerFrameBone(copyTriggerFrameBone);
                triggerFrameBones.Add(newTriggerFrameBone);
            }
        }

        public TriggerFrame(int cIndex, int fr)
        {
            clipIndex = cIndex;
            frame = fr;
            triggerFrameBones = new List<TriggerFrameBone>();
        }

        public void AddTriggerFrameBoneUserTrigger(int boneNodeIndex, string tag)
        {
            TriggerFrameBone triggerFrameBone = AddTriggerFrameBone(boneNodeIndex);
            triggerFrameBone.AddUserTriggerEvent(tag);
        }

        public void AddTriggerFrameBoneHideTexture(int boneNodeIndex)
        {
            TriggerFrameBone triggerFrameBone = AddTriggerFrameBone(boneNodeIndex);
            triggerFrameBone.AddHideTextureEvent();
        }

        public void AddTriggerFrameBoneMaterialChangeOriginal(int boneNodeIndex, int materialIndex)
        {
            TriggerFrameBone triggerFrameBone = AddTriggerFrameBone(boneNodeIndex);
            triggerFrameBone.AddChangeMaterialEvent(materialIndex);
            triggerFrameBone.CaptureOriginalSettings();
        }

        public void AddTriggerFrameBoneMaterialChangeNew(int boneNodeIndex, int materialIndex)
        {
            TriggerFrameBone triggerFrameBone = AddTriggerFrameBone(boneNodeIndex);
            triggerFrameBone.AddChangeMaterialEvent(materialIndex);
        }

        public void AddTriggerFrameBoneDepthChange(int boneNodeIndex, int depth)
        {
            TriggerFrameBone triggerFrameBone = AddTriggerFrameBone(boneNodeIndex);
            triggerFrameBone.AddChangeDepthEvent(depth);
        }

        public void AddTriggerFrameBoneTextureChangeOriginal(int boneNodeIndex, TextureAtlas atlas, string textureGUID, Vector2 pivotOffset, int materialIndex)
        {
            TriggerFrameBone triggerFrameBone = AddTriggerFrameBone(boneNodeIndex);
            triggerFrameBone.AddChangeTextureEvent(atlas, textureGUID, pivotOffset, materialIndex);
            triggerFrameBone.CaptureOriginalSettings();
        }

        public void AddTriggerFrameBoneTextureChangeNew(int boneNodeIndex, TextureAtlas atlas, string textureGUID, Vector2 pivotOffset, int materialIndex)
        {
            TriggerFrameBone triggerFrameBone = AddTriggerFrameBone(boneNodeIndex);
            triggerFrameBone.AddChangeTextureEvent(atlas, textureGUID, pivotOffset, materialIndex);
        }

        public void AddTriggerFrameBonePivotChangeOriginal(int boneNodeIndex, TextureAtlas atlas, string textureGUID, Vector2 pivotOffset)
        {
            TriggerFrameBone triggerFrameBone = AddTriggerFrameBone(boneNodeIndex);
            triggerFrameBone.AddChangePivotEvent(atlas, textureGUID, pivotOffset);
            triggerFrameBone.CaptureOriginalSettings();
        }

        public void AddTriggerFrameBonePivotChangeNew(int boneNodeIndex, TextureAtlas atlas, string textureGUID, Vector2 pivotOffset)
        {
            TriggerFrameBone triggerFrameBone = AddTriggerFrameBone(boneNodeIndex);
            triggerFrameBone.AddChangePivotEvent(atlas, textureGUID, pivotOffset);
        }
        
        public void AddTriggerFrameBoneColliderChange(int boneNodeIndex, ColliderSM collider)
        {
            TriggerFrameBone triggerFrameBone = AddTriggerFrameBone(boneNodeIndex);
            triggerFrameBone.AddChangeColliderEvent(collider);
        }

        private TriggerFrameBone AddTriggerFrameBone(int boneNodeIndex)
        {
            TriggerFrameBone foundtfb = null;

            if (triggerFrameBones != null)
            {
                foreach (TriggerFrameBone tfb in triggerFrameBones)
                {
                    if (tfb.boneNodeIndex == boneNodeIndex)
                        foundtfb = tfb;
                }
            }

            if (foundtfb != null)
                return foundtfb;

            TriggerFrameBone triggerFrameBone = new TriggerFrameBone(boneNodeIndex);
            triggerFrameBones.Add(triggerFrameBone);
            triggerFrameBones.Sort(new SortTriggerFrameBonesAscending());

            return triggerFrameBone;
        }

        public TriggerFrameBone GetTriggerFrameBone(int boneNodeIndex)
        {
            if (triggerFrameBones != null)
            {
                foreach (TriggerFrameBone tfb in triggerFrameBones)
                {
                    if (tfb.boneNodeIndex == boneNodeIndex)
                        return tfb;
                }
            }

            return null;
        }

        public void RestoreTriggerFrameBone(int boneNodeIndex)
        {
            TriggerFrameBone tfb = GetTriggerFrameBone(boneNodeIndex);
            if (tfb != null)
            {
                tfb.RestoreOriginalSettings();
            }
        }
    }

    public class SortTriggerFrameBonesAscending : IComparer<TriggerFrameBone>
    {
        int IComparer<TriggerFrameBone>.Compare(TriggerFrameBone a, TriggerFrameBone b)
        {
            if (a.boneNodeIndex > b.boneNodeIndex)
                return 1;
            if (a.boneNodeIndex < b.boneNodeIndex)
                return -1;
            else
                return 0;
        }
    }
}
