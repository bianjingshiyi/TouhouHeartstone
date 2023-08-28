using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;

namespace Game.BattleTable
{
    using Arg = THHCard.DamageEventArg;
    class DamageAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            yield return pauseWhileNotCompleted();
            yield return stopWhenCanceled();

            var target = arg.target;
            var damagedValue = arg.damagedValue;

            if (table.tryGetServant(target, out var servant))
            {
                //显示伤害数字
                servant.showDamageText(damagedValue);
                if (AnimHelper.hasAnim(servant.animator, servant.onDamage.animName, out int layer))
                    servant.animator.Play(servant.onDamage.animName, layer);
                //创建打击特效
                _damageEffect = UnityEngine.Object.Instantiate(servant.damageEffectPrefab, servant.transform.position, Quaternion.identity);
            }
            else if (table.tryGetMaster(target, out Master master))
            {
                master.showDamageText(damagedValue);
                if (AnimHelper.hasAnim(master.animator, master.onDamage.animName, out int layer))
                    master.animator.Play(master.onDamage.animName, layer);
                //创建打击特效
                _damageEffect = UnityEngine.Object.Instantiate(master.damageEffectPrefab, master.transform.position, Quaternion.identity);
            }
            else if (table.tryGetItem(target, out var item))
            {
                if (AnimHelper.hasAnim(item.animator, item.onLifeDown.animName, out int layer))
                    item.animator.Play(item.onLifeDown.animName, layer);
                //道具只会损失耐久度
                item.onLifeDown.invokeBeforeAnim();
            }

            yield return new WaitForSeconds(1.5f);

            if (table.tryGetItem(target, out var animItem))
            {
                animItem.onLifeDown.invokeAfterAnim();
            }
            if (_damageEffect != null)
                UnityEngine.Object.Destroy(_damageEffect.gameObject);
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is RequestAnimation)
                return true;
            if (nextAnim is TurnEndAnimation)
                return true;
            return false;
        }
        ParticleSystem _damageEffect;
    }
}
