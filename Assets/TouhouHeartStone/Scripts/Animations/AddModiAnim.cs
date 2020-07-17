using UI;
using UnityEngine;
using TouhouHeartstone;
namespace Game
{
    class AddModiAnim : EventAnimation<TouhouCardEngine.Card.AddModiEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, TouhouCardEngine.Card.AddModiEventArg eventArg)
        {
            SimpleAnim simpleAnim = null;
            Animator animator = null;
            if (table.tryGetServant(eventArg.card, out var servant))
            {
                animator = servant.animator;
                if (eventArg.modifier is AttackModifier atkMod)
                {
                    if (atkMod.value > 0)
                        simpleAnim = servant.onAttackUp;
                    else
                        simpleAnim = servant.onAttackDown;
                }
                else if (eventArg.modifier is LifeModifier lifMod)
                {
                    if (lifMod.value > 0)
                        simpleAnim = servant.onLifeUp;
                    else
                        simpleAnim = servant.onLifeDown;
                }
            }
            simpleAnim.beforeAnim.Invoke();
            if (!string.IsNullOrEmpty(simpleAnim.animName))
            {
                if (_anim == null)
                    _anim = new AnimAnim(animator, simpleAnim.animName);
                if (!_anim.update(table))
                    return false;
            }
            simpleAnim.afterAnim.Invoke();
            return true;
        }
    }
}
