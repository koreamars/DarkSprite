using UnityEngine;

namespace SmoothMoves
{
    [System.Serializable]
	public class BoneColorAnimation
	{
        private Color _startingColor;
        private float _startingBlendWeight;
        private Color _endingColor;
        private float _endingBlendWeight;
        private float _duration;
        private int _iterations;
        private bool _resetColorOnFinish;
        private Color _colorResetTo;
        private float _blendWeightResetTo;

        private float _timeLeft;

        public Color boneColor;
        public float blendWeight;

        public bool Flashing { get { return _timeLeft > 0; } }
        public bool ResetColorOnFinish { get { return _resetColorOnFinish; } }
        public Color ColorResetTo { get { return _colorResetTo; } }
        public float BlendWeightResetTo { get { return _blendWeightResetTo; } }

        public BoneColorAnimation()
        {
            _timeLeft = 0;
            _resetColorOnFinish = true;
        }

        public void Reset(Color startingColor, float startingBlendWeight, Color endingColor, float endingBlendWeight, float duration, int iterations, bool resetColorOnFinish, Color colorResetTo, float blendWeightResetTo)
        {
            _startingColor = startingColor;
            _startingBlendWeight = startingBlendWeight;
            _endingColor = endingColor;
            _endingBlendWeight = endingBlendWeight;
            _duration = duration;
            _iterations = iterations;
            _resetColorOnFinish = resetColorOnFinish;
            _colorResetTo = colorResetTo;
            _blendWeightResetTo = blendWeightResetTo;

            if (_iterations == 0)
                Stop();
            else
                _timeLeft = duration;
        }

        public void Stop()
        {
            _timeLeft = 0;
            _iterations = 0;

            Calculate();
        }

        public bool FrameUpdate(Color meshColor, float deltaTime)
        {
            _timeLeft -= deltaTime;
            if (_timeLeft <= 0)
            {
                if (_iterations < 0)
                {
                    _timeLeft = _duration;
                }
                else
                {
                    _iterations--;

                    if (_iterations <= 0)
                        Stop();
                    else
                        _timeLeft = _duration;
                }
            }

            Calculate();

            return (_timeLeft > 0);
        }

        private void Calculate()
        {
            float progress = ((_duration - _timeLeft) / _duration);

            boneColor = Color.Lerp(_startingColor, _endingColor, progress);
            blendWeight = Mathf.Lerp(_startingBlendWeight, _endingBlendWeight, progress);
        }
	}
}
