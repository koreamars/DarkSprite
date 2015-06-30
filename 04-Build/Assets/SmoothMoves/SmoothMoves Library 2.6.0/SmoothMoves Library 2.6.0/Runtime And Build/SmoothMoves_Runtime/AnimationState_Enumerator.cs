using System;
using System.Collections;

namespace SmoothMoves
{
    public class AnimationState_Enumerator : IEnumerator
    {
        public AnimationStateSM[] _animationStates;

        int position = -1;

        public AnimationState_Enumerator(AnimationStateSM[] array)
        {
            _animationStates = array;
        }

        public bool MoveNext()
        {
            position++;
            return (position < _animationStates.Length);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public AnimationStateSM Current
        {
            get
            {
                try
                {
                    return _animationStates[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}