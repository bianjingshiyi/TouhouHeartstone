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
            _startPosition = GetComponent<RectTransform>().position;
        }
        protected void Update()
        {
            tryLerp();
        }
        protected void OnDisable()
        {
            tryLerp();
        }
        private void tryLerp()
        {
            if (targetTransofrm != null)
            {
                RectTransform transform = GetComponent<RectTransform>();
                Vector3 targetPosition = Vector3.MoveTowards(targetTransofrm.position, _startPosition, _stopDistance);
                transform.position = Vector3.Lerp(_startPosition, targetPosition, _time);
            }
        }
        public void setTarget(RectTransform target, float stopDistance = 0)
        {
            _targetTransform = target;
            _stopDistance = stopDistance;
        }
        protected void Reset()
        {
            enabled = false;
        }
    }
}
