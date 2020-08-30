using UnityEngine;
namespace Game
{
    [ExecuteInEditMode]
    public class PositionLerp : MonoBehaviour
    {
        [Range(0, 1)]
        [SerializeField]
        float _time;
        [SerializeField]
        RectTransform _targetTransform;
        public RectTransform targetTransofrm
        {
            get { return _targetTransform; }
        }
        [SerializeField]
        float _stopDistance;
        Vector3 _startPosition;
        protected void OnEnable()
        {
            _startPosition = transform.position;
        }
        protected void Update()
        {
            //不能将位置的更新放在LateUpdate中，LateUpdate中TargetTransform的位置与Update中的不同，可能与动画和Layout有关，此时处于没有计算Layout的状态。
            //也不能在Update中获取位置，然后在LateUpdate中更新位置，更新的位置仍然是错的。
            if (targetTransofrm != null)
            {
                Vector3 targetPosition = Vector3.MoveTowards(targetTransofrm.position, _startPosition, _stopDistance);
                transform.position = Vector3.Lerp(_startPosition, targetPosition, _time);
            }
        }
        protected void OnDisable()
        {
            //tryLerp();//好吧，看来不能在这里Lerp，因为Lerp的参数可能已经被重置了
        }
        private void tryLerp()
        {

        }
        public void setTarget(RectTransform target, float stopDistance = 0)
        {
            _targetTransform = target;
            _stopDistance = stopDistance;
        }
        public void resetPosition()
        {
            transform.localPosition = Vector3.zero;
        }
        protected void Reset()
        {
            enabled = false;
        }
    }
}
