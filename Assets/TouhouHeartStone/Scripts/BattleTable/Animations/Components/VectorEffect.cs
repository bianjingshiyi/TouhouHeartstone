using System;
using UnityEngine;

namespace Game.Animations
{
    public class VectorEffect : TableEffect
    {
        protected override void updateAnim(float deltaTime)
        {
            var sourcePos = sourceTransform ? sourceTransform.position : Vector3.zero;
            var targetPos = destTransform ? destTransform.position : sourcePos;
            var distance = targetPos - sourcePos;
            transform.position = sourcePos;
            transform.forward = distance.normalized;
            if (_scaleByDistance)
            {
                var distanceLength = distance.magnitude;
                var scaleX = Mathf.Lerp(1, distanceLength * _scaleMultiplier.x, _scaleFactors.x);
                var scaleY = Mathf.Lerp(1, distanceLength * _scaleMultiplier.y, _scaleFactors.y);
                var scaleZ = Mathf.Lerp(1, distanceLength * _scaleMultiplier.z, _scaleFactors.z);
                var scale = new Vector3(scaleX, scaleY, scaleZ);
                if (_scaleRoot)
                {
                    _scaleRoot.localScale = scale;
                }
                else
                {
                    transform.localScale = scale;
                }
            }
        }
        [Header("Scaling")]
        [SerializeField]
        private bool _scaleByDistance = false;
        [SerializeField]
        private Vector3 _scaleFactors = Vector3.zero;
        [SerializeField]
        private Vector3 _scaleMultiplier = Vector3.one;
        [SerializeField]
        private Transform _scaleRoot;
    }
}
