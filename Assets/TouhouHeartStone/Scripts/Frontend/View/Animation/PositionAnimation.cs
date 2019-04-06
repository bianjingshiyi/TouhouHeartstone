using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class PositionAnimation
    {
        float startTime;
        public float Duration = 1;

        public Vector3[] Positions = new Vector3[0];
        public Vector3[] Rotations = new Vector3[0];
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        Transform target;

        public PositionAnimation(float startTime, Transform target)
        {
            this.startTime = startTime;
            this.target = target;
        }

        public bool Update(float time)
        {
            var deltaTime = time - startTime;
            var val = Curve.Evaluate(deltaTime / Duration);

            if (Positions.Length > 1)
            {
                target.position = Vector3.Lerp(Positions[0], Positions[1], val);
            }
            if (Rotations.Length > 1)
            {
                target.rotation = Quaternion.Euler(Vector3.Lerp(Rotations[0], Rotations[1], val));
            }
            return (deltaTime > Duration);
        }
    }
}
