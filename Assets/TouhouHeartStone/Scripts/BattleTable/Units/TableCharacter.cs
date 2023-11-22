using UI;
using UnityEngine;

namespace Game.BattleTable
{
    public abstract class TableCharacter : TableUnit
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

        public SimpleAnim onSetDefine;
        public SimpleAnim onTargeted;
        public SimpleAnim onAttackStart;
        public SimpleAnim onAttack;
        public SimpleAnim onAttackEnd;
        public SimpleAnim onAttacked;
        public SimpleAnim onDamage;
        public ParticleSystem damageEffectPrefab;
        public SimpleAnim onHeal;
        public ParticleSystem healEffectPrefab;
        public SimpleAnim onDeath;
        private bool _attackHit;

    }
}
