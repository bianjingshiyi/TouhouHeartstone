using System.Collections;
using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;

namespace Game.BattleTable
{
    using Arg = THHCard.HealEventArg;
    class HealAnimation : EventAnimation<Arg>
    {
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            if (record == null || record.isCanceled)
                yield break;
            var card = arg.card;
            var healedValue = arg.healedValue;

            if (table.tryGetServant(card, out Servant servant))
            {
                //显示治疗数字
                servant.showHealText(healedValue);
                if (AnimHelper.hasAnim(servant.animator, servant.onHeal.animName, out int layer))
                    servant.animator.Play(servant.onHeal.animName, layer);
                //播放粒子特效
                _healEffect = Object.Instantiate(servant.healEffectPrefab, servant.transform.position, Quaternion.identity);
            }
            else if (table.tryGetMaster(card, out Master master))
            {
                //显示治疗数字
                master.showHealText(healedValue);
                if (AnimHelper.hasAnim(master.animator, master.onHeal.animName, out int layer))
                    master.animator.Play(master.onHeal.animName, layer);
                //播放粒子特效
                _healEffect = Object.Instantiate(master.healEffectPrefab, master.transform.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(1.5f);
            Object.Destroy(_healEffect.gameObject);
        }

        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is RequestAnimation)
                return true;
            if (nextAnim is TurnEndAnimation)
                return true;
            return false;
        }
        private ParticleSystem _healEffect;
    }
}
