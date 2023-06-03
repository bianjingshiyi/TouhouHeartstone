using TouhouHeartstone;
using UI;
using TouhouCardEngine;
using System.Collections;

namespace Game
{
    using Arg = THHCard.DeathEventArg;
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
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            var card = arg.card;
            AnimAnim anim = null;
            if (table.tryGetServant(card, out var serv))//尝试获取死亡的随从的随从UI
            {
                serv.onDeath.invokeBeforeAnim();
                anim = new AnimAnim(serv.servantAnimator, serv.onDeath.animName);
            }
            else if (table.tryGetMaster(card, out var master))
            {
                master.onDeath.invokeBeforeAnim();
                anim = new AnimAnim(master.animator, master.onDeath.animName);
            }
            else if (table.tryGetItem(card, out var item))
            {
                item.onDestroy.invokeBeforeAnim();
                anim = new AnimAnim(item.animator, item.onDestroy.animName);
            }
            while (anim != null && !anim.update(table, _time))
                yield return null;
            if (table.tryGetServant(card, out var servant))//尝试获取死亡的随从的随从UI
            {
                servant.onDeath.invokeAfterAnim();
                table.ui.SelfFieldList.removeItem(servant,true);//从自己的场上移除随从UI
                table.ui.EnemyFieldList.removeItem(servant,true);//从敌人的场上移除随从UI
            }
            else if (table.tryGetMaster(card, out var master))
            {
                master.onDeath.invokeAfterAnim();
            }
            else if (card.isItem())
            {
                Item item = card.getOwner() == table.player ? table.ui.SelfItem : table.ui.EnemyItem;
                item.onDestroy.invokeAfterAnim();
                table.removeItem(card);
            }
        }

        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is MoveCardAnim moveAnim)
            {
                var to = moveAnim.tEventArg.to;
                if (to?.name == PILE.FIELD)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
