using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.BattleTable
{
    public class TableLightFactor : MonoBehaviour
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
        float _targetIntensity = 1;

        public float targetIntensity
        {
            get => _targetIntensity;
            set => _targetIntensity = value;
        }
        [SerializeField]
        private List<TableLightController> _controllers = new List<TableLightController>();
    }
}
