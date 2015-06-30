using UnityEngine;

namespace SmoothMoves
{
    public class CollisionArc
    {
        private float _startDeg;
        private float _endDeg;
        private float _rotationDeg;
        private Vector2 _center;
        private float _maxRadiusSquared;

        public float RotationDeg { get { return _rotationDeg; } }

        public CollisionArc(float startDeg, float endDeg, float maxRadius)
        {
            _center = Vector2.zero;
            _maxRadiusSquared = maxRadius * maxRadius;
            SetAngles(startDeg, endDeg);
        }

        public void Update(Vector2 center, float rotationDeg)
        {
            _rotationDeg = EditorHelper.KeepAngleInBounds(rotationDeg);

            _center = center;
        }

        public void SetAngles(float startDeg, float endDeg)
        {
            _startDeg = startDeg;
            _endDeg = endDeg;
        }

        public bool CheckCollision(Vector2 point)
        {
            float pointAngle = (Mathf.Atan2(_center.y - point.y, point.x - _center.x) * Mathf.Rad2Deg) + _rotationDeg;
            bool withinRadius = (_center - point).sqrMagnitude <= _maxRadiusSquared;
            bool withinArc = false;

            pointAngle = EditorHelper.KeepAngleInBounds(pointAngle);

            // this assumes _endDeg will always be postive and less than 360

            if (_startDeg >= 0)
            {
                if (pointAngle >= _startDeg && pointAngle <= _endDeg)
                {
                    withinArc = true;
                }
            }
            else
            {
                if (pointAngle > 180.0f)
                {
                    if (pointAngle >= (360.0f + _startDeg) && pointAngle < (360.0f + _endDeg))
                    {
                        withinArc = true;
                    }
                }
                else
                {
                    if (pointAngle <= _endDeg)
                    {
                        withinArc = true;
                    }
                }
            }


            return (withinArc && withinRadius);
        }
    }
}
