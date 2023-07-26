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
            transform.position = from;
            _arrowHeadTransform.position = to;
            transform.up = (to - from).normalized;
            _arrowLineTransform.localScale = new Vector3(1, Vector3.Distance(from, to) / _arrowLineLength, 1);
        }

        [SerializeField]
        private Transform _arrowHeadTransform;
        [SerializeField]
        private Transform _arrowLineTransform;
        [SerializeField]
        private float _arrowLineLength;
    }
}
