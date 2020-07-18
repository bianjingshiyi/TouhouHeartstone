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
        [SerializeField]
        Vector3 _startPosition;
        protected void OnEnable()
        {
            _startPosition = GetComponent<RectTransform>().position;
        }
        protected void Update()
        {
            RectTransform transform = GetComponent<RectTransform>();
            transform.position = Vector3.Lerp(_startPosition, _targetTransform.position, _time);
        }
        protected void Reset()
        {
            enabled = false;
        }
    }
}
