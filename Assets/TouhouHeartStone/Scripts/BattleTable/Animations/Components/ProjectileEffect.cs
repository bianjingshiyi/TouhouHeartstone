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

            var t = time;
            var distanceLength = (distance.magnitude - _flyStartDistance - _flyEndDistance);
            var fly = _flyCurve.Evaluate(t) * distanceLength + _flyStartDistance;
            var offset = _offsetCurve.Evaluate(t) * distanceLength + _flyStartDistance;

            var targetWorldPos = sourcePos + axis * fly + offsetAxis * offset;
            if (_positionRoot)
            {
                _positionRoot.localPosition = _positionRoot.parent.InverseTransformPoint(targetWorldPos) + rootObj.transform.localPosition;
            }
            else
            {
                transform.position = targetWorldPos;
            }
            if (_onHitRoot)
            {
                _onHitRoot.position = targetPos;
            }

            if (_faceToTarget)
            {
                if (_rotationRoot)
                {
                    _rotationRoot.localRotation = Quaternion.LookRotation(distance);
                }
                else
                {
                    transform.forward = distance.normalized;
                }
            }
        }

        [Header("Projectile")]
        [SerializeField]
        private AnimationCurve _flyCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField]
        private AnimationCurve _offsetCurve = AnimationCurve.Constant(0, 1, 0);
        [SerializeField]
        private float _flyStartDistance = 0;
        [SerializeField]
        private float _flyEndDistance = 0;
        [SerializeField]
        private Transform _positionRoot;
        [SerializeField]
        private Transform _rotationRoot;
        [SerializeField]
        private Transform _onHitRoot;
        [SerializeField]
        private bool _faceToTarget = true;
    }
}