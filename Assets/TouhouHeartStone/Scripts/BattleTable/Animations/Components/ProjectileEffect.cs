using UnityEngine;

namespace Game.Animations
{
    public class ProjectileEffect : TableEffect
    {
        public override void init(IVFXTable table, AnimationDefine define, VisualEffectInfo info)
        {
            base.init(table, define, info);

            _randomValue = Random.Range(0, 1f);
            if (_randomParticles != null && _randomParticles.Length > 0)
            {
                int index = Random.Range(0, _randomParticles.Length);
                for (int i = 0; i < _randomParticles.Length; i++)
                {
                    _randomParticles[i].SetActive(i == index);
                }
            }
        }
        protected override void updateAnim(float deltaTime)
        {
            Vector3 sourcePos = (sourceTransform ? sourceTransform.position : Vector3.zero) + Vector3.up * heightOffset;
            Vector3 targetPos = (destTransform ? destTransform.position : sourcePos) + Vector3.up * heightOffset;
            Vector3 distance = targetPos - sourcePos;
            Vector3 axis = distance.normalized;
            Vector3 offsetAxis = axis;
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

            float t = time;
            float distanceLength = (distance.magnitude - _flyStartDistance - _flyEndDistance);
            float fly = _flyCurve.Evaluate(t) * distanceLength + _flyStartDistance;
            float offset = _offsetCurve.Evaluate(t, _randomValue) * distanceLength + _flyStartDistance;

            Vector3 targetWorldPos = sourcePos + axis * fly + offsetAxis * offset;
            if (_onLaunchRoot != null)
            {
                _onLaunchRoot.position = sourcePos;
                if (t == 0)
                {
                    Vector3 nextPos = sourcePos +
                        axis * (_flyCurve.Evaluate(deltaTime) * distanceLength + _flyStartDistance) +
                        offsetAxis * (_offsetCurve.Evaluate(deltaTime, _randomValue) * distanceLength + _flyStartDistance);
                    _onLaunchRoot.transform.forward = (nextPos - sourcePos).normalized;
                }
            }
            if (_positionRoot)
            {
                _positionRoot.localPosition = _positionRoot.parent.InverseTransformPoint(targetWorldPos) + rootObj.transform.localPosition;
            }
            else
            {
                transform.position = targetWorldPos;
            }
            if (_onHitRoot != null)
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
        private ParticleSystem.MinMaxCurve _offsetCurve = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Constant(0, 1, 0));
        [SerializeField]
        private float _flyStartDistance = 0;
        [SerializeField]
        private float _flyEndDistance = 0;
        [SerializeField]
        private GameObject[] _randomParticles;
        [SerializeField]
        private Transform _positionRoot;
        [SerializeField]
        private Transform _rotationRoot;
        [SerializeField]
        private Transform _onLaunchRoot;
        [SerializeField]
        private Transform _onHitRoot;
        [SerializeField]
        private bool _faceToTarget = true;

        private float _randomValue;
    }
}