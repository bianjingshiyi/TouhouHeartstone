using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.BattleTable
{
    public class TableLightController : MonoBehaviour
    {
        public void addFactor(TableLightFactor factor)
        {
            _factors.Add(factor);
        }
        public bool removeFactor(TableLightFactor factor)
        {
            return _factors.Remove(factor);
        }
        private void Update()
        {
            var minIntensity = _factors.Count > 0 ? _factors.Min(f => f.targetIntensity) * maxIntensity : maxIntensity;
            _light.intensity = minIntensity;
        }
        [SerializeField]
        private Light _light;
        [SerializeField]
        private float maxIntensity = 1;
        [SerializeField]
        private List<TableLightFactor> _factors = new List<TableLightFactor>();
    }
}
