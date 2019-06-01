using UnityEngine;

namespace TouhouHeartstone.Frontend.View.Animation
{
    public class PositionAnimation : MonoBehaviour
    {
        float? startTime;
        public float Duration = 0;
        public float MaxDuration = 1;
        float minMovingSpeed = 1000;
        float minRotateSpeed = 180;

        public Vector3[] Positions { get; private set; } = new Vector3[0];
        public Vector3[] Rotations { get; private set; } = new Vector3[0];
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        GenericAction callback;

        float[] timeDurations;

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

            timeDurations = new float[position.Length];

            Duration = 0;
            for (int i = 0; i < Positions.Length - 1; i++)
            {
                var dist = Vector3.Distance(Positions[i], Positions[i + 1]);
                var rot = Vector3.Distance(standardizeRotation(Rotations[i], Rotations[i + 1]), Rotations[i + 1]);

                var posDuration = dist / minMovingSpeed;
                var rotDuration = rot / minRotateSpeed;

                Duration += Mathf.Min(MaxDuration, Mathf.Max(posDuration, rotDuration));
                timeDurations[i + 1] = Duration;
            }
        }

        private void Update()
        {
            if (startTime != null)
            {
                float deltaTime = Time.time - startTime.Value;

                float val = 1;
                int index = 0;

                for (int i = 1; i < timeDurations.Length; i++)
                {
                    index = i;
                    if (timeDurations[i - 1] <= deltaTime && deltaTime <= timeDurations[i])
                    {
                        var dura = timeDurations[i] - timeDurations[i - 1];
                        if (dura != 0)
                        {
                            var t = Mathf.Clamp01((deltaTime - timeDurations[i - 1]) / dura);
                            val = Curve.Evaluate(t);
                        }
                        break;
                    }
                }

                if (Positions.Length > index)
                {
                    transform.localPosition = Vector3.Lerp(Positions[index - 1], Positions[index], val);
                }
                if (Rotations.Length > index)
                {
                    transform.localRotation = Quaternion.Euler(Vector3.Lerp(standardizeRotation(Rotations[index - 1], Rotations[index]), Rotations[index], val));
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
