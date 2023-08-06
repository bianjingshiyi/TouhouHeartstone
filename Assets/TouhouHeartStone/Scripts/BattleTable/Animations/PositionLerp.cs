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
                Vector3 targetPosition = Vector3.MoveTowards(targetTransform.position, _startPosition, _stopDistance);
                transform.position = Vector3.Lerp(_startPosition, targetPosition, _time);
            }
        }
        public Transform targetTransform => _targetTransform;
        [Range(0, 1)]
        [SerializeField]
        public float _time;
        [SerializeField]
        Transform _targetTransform;
        [SerializeField]
        float _stopDistance;
        [SerializeField]
        Vector3 _startPosition;
        public Vector3 startPosition
        {
            get => _startPosition;
            set => _startPosition = value;
        }
    }
}
