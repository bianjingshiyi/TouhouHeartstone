using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class PositionAnimation : MonoBehaviour
    {
        float? startTime;
        public float Duration = 1;
        public float MaxDuration = 1;
        float minMovingSpeed = 1000;
        float minRotateSpeed = 180;

        public Vector3[] Positions { get; private set; } = new Vector3[0];
        public Vector3[] Rotations { get; private set; } = new Vector3[0];
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        GenericAction callback;

        public void Play(Vector3[] position, Vector3[] rotation, GenericAction callback)
        {
            if (startTime != null)
            {
                startTime = null;
                this.callback?.Invoke(this, null);
            }

            this.startTime = Time.time;
            this.callback = callback;

            this.Positions = position;
            this.Rotations = rotation;

            var dist = Vector3.Distance(Positions[0], Positions[1]);
            var posDuration = dist / minMovingSpeed;

            var rot = Vector3.Distance(standardizeRotation(Rotations[0], Rotations[1]), Rotations[1]);
            var rotDuration = rot / minRotateSpeed;

            Duration = Mathf.Min(MaxDuration, Mathf.Max(posDuration, rotDuration));
        }

        private void Update()
        {
            if (startTime != null)
            {
                float deltaTime = Time.time - startTime.Value;

                float val = 1;

                if (Duration != 0)
                {
                    var t = Mathf.Clamp01(deltaTime / Duration);
                    val = Curve.Evaluate(t);
                }

                if (Positions.Length > 1)
                {
                    transform.localPosition = Vector3.Lerp(Positions[0], Positions[1], val);
                }
                if (Rotations.Length > 1)
                {
                    transform.localRotation = Quaternion.Euler(Vector3.Lerp(standardizeRotation(Rotations[0], Rotations[1]), Rotations[1], val));
                }

                if (deltaTime > Duration)
                {
                    startTime = null;
                    callback?.Invoke(this, null);
                }
            }
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
