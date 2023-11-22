using UnityEngine;

namespace Game.BattleTable
{
    public abstract class TableUnit : MonoBehaviour
    {
        /// <summary>
        /// 给Animator用的公开接口，用于给攻击动画标注攻击时机。
        /// </summary>
        public void setAttackHit()
        {
            _attackHit = true;
        }
        /// <summary>
        /// 取消用于攻击动画标注攻击时机。
        /// </summary>
        public void resetAttackHit()
        {
            _attackHit = false;
        }
        /// <summary>
        /// 是否在动画中使用攻击击中了目标？
        /// </summary>
        /// <param name="value"></param>
        public bool isAttackHit()
        {
            return _attackHit;
        }
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

        private bool _attackHit;

    }
}
