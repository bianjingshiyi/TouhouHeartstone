using UnityEngine;

namespace Game.BattleTable
{
    public abstract class TableUnit : MonoBehaviour
    {
        public float getHeight()
        {
            return _height;
        }
        public void destroy()
        {
            Destroy(gameObject);
        }
        public PositionLerp positionLerp => _positionLerp;
        public Animator animator => _animator;
        [SerializeField]
        protected PositionLerp _positionLerp;
        [SerializeField]
        protected Animator _animator;
        [SerializeField]
        protected float _height;
    }
}
