using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine.Histories;
using TouhouHeartstone;
using UnityEngine;
using Card = TouhouCardEngine.Card;

namespace Game.BattleTable
{
    using Arg = DamageEventDefine;
    class DamageAnimation : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            yield return pauseWhileNotCompleted();
            yield return stopWhenCanceled();

            var targetId = eventArg.getVar<Card>(EVENTVAR.CARD);
            var damagedValue = eventArg.getVar<int>(Arg.VAR_DAMAGED_VALUE);

            if (table.tryGetServant(targetId, out var servant))
            {
                //显示伤害数字
                servant.showDamageText(damagedValue);
                if (AnimHelper.hasAnim(servant.animator, servant.onDamage.animName, out int layer))
                    servant.animator.Play(servant.onDamage.animName, layer);
                //创建打击特效
                _damageEffect = Object.Instantiate(servant.damageEffectPrefab, servant.transform.position, Quaternion.identity, table.table.EffectsParent);
            }
            else if (table.tryGetMaster(targetId, out Master master))
            {
                master.showDamageText(damagedValue);
                if (AnimHelper.hasAnim(master.animator, master.onDamage.animName, out int layer))
                    master.animator.Play(master.onDamage.animName, layer);
                //创建打击特效
                _damageEffect = Object.Instantiate(master.damageEffectPrefab, master.transform.position, Quaternion.identity, table.table.EffectsParent);
            }
            else if (table.tryGetItem(targetId, out var item))
            {
                if (AnimHelper.hasAnim(item.animator, item.onLifeDown.animName, out int layer))
                    item.animator.Play(item.onLifeDown.animName, layer);
                //道具只会损失耐久度
                item.onLifeDown.invokeBeforeAnim();
            }

            yield return new WaitForSeconds(1.5f);

            if (table.tryGetItem(targetId, out var animItem))
            {
                animItem.onLifeDown.invokeAfterAnim();
            }
            if (_damageEffect != null)
                Object.Destroy(_damageEffect.gameObject);
        }
        public override IEnumerable<TableAnimation> getGroupedAnimations(IEnumerable<TableAnimation> following)
        {
            return following.OfType<DamageAnimation>().Where(a => a.eventArg.parent == eventArg.parent);
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (nextGroup.Any(a => a is RequestAnimation))
                return false;
            if (nextGroup.Any(a => a is TurnEndAnimation))
                return false;
            return true;
        }
        ParticleSystem _damageEffect;
    }
}
