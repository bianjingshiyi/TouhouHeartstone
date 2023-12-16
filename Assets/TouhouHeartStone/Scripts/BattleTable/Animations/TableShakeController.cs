using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.BattleTable
{
    public class TableShakeController : MonoBehaviour
    {
        public void addFactor(TableShakeFactor factor)
        {
            _factors.Add(factor);
        }
        public bool removeFactor(TableShakeFactor factor)
        {
            if (_factors.Remove(factor))
            {
                if (_factors.Count <= 0)
                {
                    _camera.transform.localPosition = Vector3.zero;
                }
                return true;
            }
            return false;
        }
        private void Update()
        {
            if (_factors.Count > 0)
            {
                var shakeValue = _factors.Max(f => f.shakeValue);
                if (shakeValue == 0)
                    return;
                var angle = Random.Range(0, 360);
                var length = Random.Range(0, shakeValue * maxShakeValue);
                var offset = Quaternion.Euler(0, 0, angle) * new Vector3(length, 0, 0);
                _camera.transform.localPosition = offset;
            }
        }
        [SerializeField]
        private Camera _camera;
        [SerializeField]
        private float maxShakeValue = 1;
        [SerializeField]
        private List<TableShakeFactor> _factors = new List<TableShakeFactor>();
    }
}
