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
            set { _targetTransform = value; }
        }
        Vector3 _startPosition;
        protected void OnEnable()
        {
            _startPosition = GetComponent<RectTransform>().position;
        }
        protected void Update()
        {
            if (targetTransofrm != null)
            {
                RectTransform transform = GetComponent<RectTransform>();
                transform.position = Vector3.Lerp(_startPosition, targetTransofrm.position, _time);
            }
        }
        protected void Reset()
        {
            enabled = false;
        }
    }
}
