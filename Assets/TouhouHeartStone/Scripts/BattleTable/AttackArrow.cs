using UI;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace Game.BattleTable
{
    public class AttackArrow : MonoBehaviour
    {
        public void setActive(bool active)
        {
            gameObject.SetActive(active);
        }
        public void connect(Vector3 from, Vector3 to)
        {
            _arrowLineTransform.position = from;
            _arrowHeadTransform.position = to;

            var localPos = _arrowLineTransform.localPosition;
            localPos.y = 0;
            _arrowLineTransform.localPosition = localPos;

            localPos = _arrowHeadTransform.localPosition;
            localPos.y = 0;
            _arrowHeadTransform.localPosition = localPos;

            var angle = Vector3.SignedAngle(to - from, Vector3.up, Vector3.forward);
            var euler = Vector3.up * angle;
            _arrowLineTransform.localEulerAngles = euler;
            _arrowHeadTransform.localEulerAngles = euler;

            var scale = Mathf.Max(0, Vector3.Distance(from, to) - _arrowHeadHeight) / _arrowLineLength;
            _arrowLineTransform.localScale = new Vector3(1, 1, scale);
        }

        [SerializeField]
        private Transform _arrowHeadTransform;
        [SerializeField]
        private Transform _arrowLineTransform;
        [SerializeField]
        private float _arrowHeadHeight;
        [SerializeField]
        private float _arrowLineLength;
    }
}
