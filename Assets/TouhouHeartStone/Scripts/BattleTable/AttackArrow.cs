using UnityEngine;

namespace Game.BattleTable
{
    public class AttackArrow : MonoBehaviour
    {
        public void setActive(bool active)
        {
            gameObject.SetActive(active);
        }
        public void connect(BattleTableManager table, Vector3 from, Vector3 to)
        {
            var maxY = Mathf.Max(from.y, to.y);
            from.y = maxY;
            to.y = maxY;
            _arrowLineTransform.position = table.TableToWorldPosition(from);
            _arrowHeadTransform.position = table.TableToWorldPosition(to);

            var distance = to - from;
            
            var quat = Quaternion.FromToRotation(Vector3.forward, distance);
            var euler = quat.eulerAngles;
            euler.x *= -1;
            euler.z = 0;
            _arrowLineTransform.localEulerAngles = euler;
            _arrowHeadTransform.localEulerAngles = euler;

            var lineLength = Mathf.Max(0, distance.magnitude - _arrowHeadHeight);
            var scale = lineLength / _arrowLineLength;
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
