using System.Collections.Generic;
using UnityEngine;

namespace Game.BattleTable
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRendererController))]
    public class MeshRendererAnimator : MonoBehaviour
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
        public MeshRendererController controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<MeshRendererController>();
                }
                return _controller;
            }
        }
        private MeshRendererController _controller;
    }
}
