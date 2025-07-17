using System.Collections.Generic;
using UnityEngine;

namespace Game.BattleTable
{
    [ExecuteAlways]
    [RequireComponent(typeof(RendererController))]
    public class RendererAnimator : MonoBehaviour
    {
        private void Update()
        {
            if (_gradient != null)
                controller.color = _gradient.Evaluate(_time);
        }
        public Gradient gradient 
        { 
            get => _gradient; 
            set => _gradient = value; 
        }
        [SerializeField]
        Gradient _gradient = new Gradient();
        [SerializeField]
        [Range(0, 1)]
        float _time;
        public RendererController controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<RendererController>();
                }
                return _controller;
            }
        }
        private RendererController _controller;
    }
}
