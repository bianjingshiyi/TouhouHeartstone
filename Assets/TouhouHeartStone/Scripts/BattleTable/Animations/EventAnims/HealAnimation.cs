using System.Collections;
using System.Linq;
using TouhouCardEngine.Histories;
using TouhouHeartstone;
using UnityEngine;
using Card = TouhouCardEngine.Card;

namespace Game.BattleTable
{
    using Arg = HealEventDefine;
    class HealAnimation : EventAnimation<Arg>
    {
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var cardId = eventArg.getVar<Card>(EVENTVAR.CARD).id;
            var healedValue = eventArg.getVar<int>(Arg.VAR_HEALED_VALUE);

            ParticleSystem healEffect = null;
            if (table.tryGetServant(cardId, out Servant servant))
            {
                //显示治疗数字
                servant.showHealText(healedValue);
                if (AnimHelper.hasAnim(servant.animator, servant.onHeal.animName, out int layer))
                    servant.animator.Play(servant.onHeal.animName, layer);
                //播放粒子特效
                healEffect = Object.Instantiate(servant.healEffectPrefab, servant.transform.position, Quaternion.identity, table.table.EffectsParent);
            }
            else if (table.tryGetMaster(cardId, out Master master))
            {
                //显示治疗数字
                master.showHealText(healedValue);
                if (AnimHelper.hasAnim(master.animator, master.onHeal.animName, out int layer))
                    master.animator.Play(master.onHeal.animName, layer);
                //播放粒子特效
                healEffect = Object.Instantiate(master.healEffectPrefab, master.transform.position, Quaternion.identity, table.table.EffectsParent);
            }
            yield return new WaitForSeconds(1.5f);
            if (healEffect)
            {
                Object.Destroy(healEffect.gameObject);
            }
        }

        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            if (!nextGroup.Any(a => a is RequestAnimation || a is TurnEndAnimation))
                return true;
            return base.continueNextGroup(nextGroup);
        }
    }
}
