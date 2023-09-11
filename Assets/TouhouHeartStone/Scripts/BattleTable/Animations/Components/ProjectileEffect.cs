using UnityEngine;

namespace Game.Animations
{
    public class ProjectileEffect : TableEffect
    {
        protected override void updateAnim(float deltaTime)
        {
            var sourcePos = sourceTransform ? sourceTransform.position : Vector3.zero;
            var targetPos = destTransform ? destTransform.position : sourcePos;
            var distance = targetPos - sourcePos;
            var axis = distance.normalized;
            var offsetAxis = axis;
            if (isTestMode())
            {
                offsetAxis = Quaternion.Euler(0, 90, 0) * offsetAxis;
            }
            else
            {
                offsetAxis = table.WorldToTablePosition(axis);
                offsetAxis = Quaternion.Euler(0, 90, 0) * offsetAxis;
                offsetAxis = table.TableToWorldPosition(offsetAxis);
            }

            var dur = duration > 0 ? duration : 1;
            var t = time / dur;
            var fly = _flyCurve.Evaluate(t) * distance.magnitude;
            var offset = _offsetCurve.Evaluate(t) * distance.magnitude;
            transform.position = sourcePos + axis * fly + offsetAxis * offset;
            transform.forward = distance.normalized;
        }
        [SerializeField]
        private AnimationCurve _flyCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField]
        private AnimationCurve _offsetCurve = AnimationCurve.Constant(0, 1, 0);
    }
}
