using UnityEngine;
namespace Game.BattleTable
{
    [ExecuteInEditMode]
    public class PositionLerp : MonoBehaviour
    {
        public void setStart()
        {
            setStart(transform.position);
        }
        public void setStart(Vector3 position)
        {
            if (_isLocalMode)
            {
                position = globalToLocalPoint(position);
            }
            _startPosition = position;
        }
        public void setTarget(Transform target, float stopDistance = 0)
        {
            _targetTransform = target;
            _stopDistance = stopDistance;
        }
        public void setPosition(Vector3 pos)
        {
            transform.position = pos;
        }
        public void resetPosition()
        {
            transform.localPosition = Vector3.zero;
        }
        protected void Reset()
        {
            enabled = false;
        }
        protected void OnEnable()
        {
            if (Application.isEditor) 
            {
                return;
            }
            setStart();
        }
        protected void Update()
        {
            //不能将位置的更新放在LateUpdate中，LateUpdate中TargetTransform的位置与Update中的不同，可能与动画和Layout有关，此时处于没有计算Layout的状态。
            //也不能在Update中获取位置，然后在LateUpdate中更新位置，更新的位置仍然是错的。
            if (targetTransform != null)
            {
                Vector3 targetPosition = targetTransform.position;
                Vector3 startPosition = _startPosition;
                if (_isLocalMode)
                {
                    Vector3 globalStartPosition = localToGlobalPoint(startPosition);
                    Vector3 closePosition = Vector3.MoveTowards(targetPosition, globalStartPosition, _stopDistance);
                    closePosition = globalToLocalPoint(closePosition);
                    transform.localPosition = Vector3.Lerp(startPosition, closePosition, _time);
                }
                else
                {
                    Vector3 closePosition = Vector3.MoveTowards(targetPosition, startPosition, _stopDistance);
                    transform.position = Vector3.Lerp(startPosition, closePosition, _time);
                }
            }
        }
        private Vector3 globalToLocalPoint(Vector3 global)
        {
            if (!transform.parent)
            {
                return global;
            }
            return transform.parent.InverseTransformPoint(global);
        }
        private Vector3 localToGlobalPoint(Vector3 global)
        {
            if (!transform.parent)
            {
                return global;
            }
            return transform.parent.TransformPoint(global);
        }
        public Transform targetTransform => _targetTransform;
        [Range(0, 1)]
        [SerializeField]
        public float _time;
        [SerializeField]
        public bool _isLocalMode;
        [SerializeField]
        Transform _targetTransform;
        [SerializeField]
        float _stopDistance;
        [SerializeField]
        Vector3 _startPosition;
        public bool isLocalMode
        {
            get => _isLocalMode;
            set => _isLocalMode = value;
        }
        public Vector3 startPosition
        {
            get => _startPosition;
        }
    }
}
