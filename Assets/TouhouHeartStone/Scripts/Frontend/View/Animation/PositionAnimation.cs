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
                target.localPosition = Vector3.Lerp(Positions[0], Positions[1], val);
            }
            if (Rotations.Length > 1)
            {
                target.localRotation = Quaternion.Euler(Vector3.Lerp(standardizeRotation(Rotations[0], Rotations[1]), Rotations[1], val));
            }
            return (deltaTime > Duration);
        }


        float standardizeRotation(float from, float to)
        {
            int sig = from > to ? -1 : 1;
            while (Mathf.Abs(from - to) > 180)
            {
                from += sig * 360;
            }

            return from;
        }

        Vector3 standardizeRotation(Vector3 from, Vector3 to)
        {
            var st = from;
            st.x = standardizeRotation(from.x, to.x);
            st.y = standardizeRotation(from.y, to.y);
            st.z = standardizeRotation(from.z, to.z);

            return st;
        }
    }
}
