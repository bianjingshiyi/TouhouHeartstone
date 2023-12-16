using System.Collections.Generic;
using UnityEngine;

namespace Game.BattleTable
{
    public class TableShakeFactor : MonoBehaviour
    {
        private void OnTransformParentChanged()
        {
            removeControllerFactor();
            addControllerFactor();
        }
        private void OnEnable()
        {
            addControllerFactor();
        }
        private void OnDisable()
        {
            removeControllerFactor();
        }
        private void removeControllerFactor()
        {
            foreach (var controller in _controllers)
            {
                controller.removeFactor(this);
            }
            _controllers.Clear();
        }
        private void addControllerFactor()
        {
            _controllers.Clear();
            transform.GetComponentsInParent(false, _controllers);
            foreach (var controller in _controllers)
            {
                controller.addFactor(this);
            }
        }
        [SerializeField]
        [Range(0, 1)]
        float _shakeValue = 0;

        public float shakeValue
        {
            get => _shakeValue;
            set => _shakeValue = value;
        }
        [SerializeField]
        private List<TableShakeController> _controllers = new List<TableShakeController>();
    }
}
