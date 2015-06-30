using UnityEngine;
using System.Collections.Generic;

namespace SmoothMoves
{
    [System.Serializable]
    public class AnimationClipTriggerFrames
    {
        public int clipIndex;
        public List<TriggerFrame> triggerFrames;

        public AnimationClipTriggerFrames(AnimationClipTriggerFrames copyTriggerFrames)
        {
            clipIndex = copyTriggerFrames.clipIndex;
            triggerFrames = new List<TriggerFrame>();
            TriggerFrame newTriggerFrame;
            foreach (TriggerFrame copyTriggerFrame in copyTriggerFrames.triggerFrames)
            {
                newTriggerFrame = new TriggerFrame(copyTriggerFrame);
                triggerFrames.Add(newTriggerFrame);
            }
        }

        public AnimationClipTriggerFrames(int cIndex)
        {
            clipIndex = cIndex;
            triggerFrames = new List<TriggerFrame>();
        }

        public TriggerFrame AddTriggerFrame(int frame)
        {
            TriggerFrame foundtf = null;

            if (triggerFrames != null)
            {
                foreach (TriggerFrame tf in triggerFrames)
                {
                    if (tf.frame == frame)
                        foundtf = tf;
                }
            }

            if (foundtf != null)
                return foundtf;

            TriggerFrame triggerFrame = new TriggerFrame(clipIndex, frame);
            triggerFrames.Add(triggerFrame);
            triggerFrames.Sort(new SortTriggerFramesAscending());

            return triggerFrame;
        }

        public TriggerFrame GetTriggerFrame(int frame)
        {
            if (triggerFrames != null)
            {
                foreach (TriggerFrame tf in triggerFrames)
                {
                    if (tf.frame == frame)
                        return tf;
                }
            }

            return null;
        }
    }


    public class SortTriggerFramesAscending : IComparer<TriggerFrame>
    {
        int IComparer<TriggerFrame>.Compare(TriggerFrame a, TriggerFrame b)
        {
            if (a.frame > b.frame)
                return 1;
            if (a.frame < b.frame)
                return -1;
            else
                return 0;
        }
    }
}