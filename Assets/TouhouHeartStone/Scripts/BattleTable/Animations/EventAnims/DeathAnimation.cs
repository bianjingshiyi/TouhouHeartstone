using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine.Histories;
using TouhouHeartstone;
using UnityEngine;
using Card = TouhouCardEngine.Card;

namespace Game.BattleTable
{
    using Arg = DeathEventDefine;
    /// <summary>
    /// 死亡动画，这是一个事件动画
    /// </summary>
    class DeathAnimation : EventAnimation<Arg>
    {
        /// <summary>
        /// 每一帧调用
        /// </summary>
        /// <param name="table">游戏桌面UI管理器</param>
        /// <param name="record">死亡事件</param>
        /// <returns></returns>
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var cardId = eventArg.getVar<Card>(EVENTVAR.CARD).id;
            AnimAnim anim = null;
            if (table.tryGetServant(cardId, out var serv))//尝试获取死亡的随从的随从UI
            {
                serv.onDeath.invokeBeforeAnim();
                anim = new AnimAnim(serv.animator, serv.onDeath.animName);
            }
            else if (table.tryGetMaster(cardId, out var master))
            {
                master.onDeath.invokeBeforeAnim();
                anim = new AnimAnim(master.animator, master.onDeath.animName);
            }
            else if (table.tryGetItem(cardId, out var item))
            {
                item.onDestroy.invokeBeforeAnim();
                anim = new AnimAnim(item.animator, item.onDestroy.animName);
            }
            while (anim != null && !anim.update(table, _deltaTime))
                yield return null;
            Vector3? position = null;
            if (table.tryGetServant(cardId, out var servant))//尝试获取死亡的随从的随从UI
            {
                servant.onDeath.invokeAfterAnim();
                position = servant.transform.position;
                table.table.SelfField.removeServant(servant);//从自己的场上移除随从UI
                table.table.EnemyField.removeServant(servant);//从敌人的场上移除随从UI
            }
            else if (table.tryGetMaster(cardId, out var master))
            {
                master.onDeath.invokeAfterAnim();
            }
            else if (table.tryGetItem(cardId, out var item))
            {
                item.onDestroy.invokeAfterAnim();
                position = item.transform.position;
                table.removeItem(cardId);
            }
            if (position.HasValue)
            {
                table.createDeadCardAnchor(cardId, position.Value);
            }
        }

        public override IEnumerable<TableAnimation> getGroupedAnimations(IEnumerable<TableAnimation> following)
        {
            return following.OfType<DeathAnimation>().Where(a => a.eventArg.parent == eventArg.parent);
        }
    }
}
