using System.Linq;
using TouhouHeartstone;
using UI;
using System.Collections.Generic;
namespace Game
{
    /// <summary>
    /// 死亡动画，这是一个事件动画
    /// </summary>
    class DeathAnimation : EventAnimation<THHCard.DeathEventArg>
    {
        List<AnimAnim> _animList = null;
        /// <summary>
        /// 每一帧调用
        /// </summary>
        /// <param name="table">游戏桌面UI管理器</param>
        /// <param name="eventArg">死亡事件</param>
        /// <returns></returns>
        public override bool update(TableManager table, THHCard.DeathEventArg eventArg, float time)
        {
            var card = eventArg.card;
            if (_animList == null)
            {
                _animList = new List<AnimAnim>();
                if (table.tryGetServant(card, out var serv))//尝试获取死亡的随从的随从UI
                {
                    serv.onDeath.invokeBeforeAnim();
                    _animList.Add(new AnimAnim(serv.servantAnimator, serv.onDeath.animName));
                    //table.ui.SelfFieldList.removeItem(servant);//从自己的场上移除随从UI
                    //table.ui.EnemyFieldList.removeItem(servant);//从敌人的场上移除随从UI
                }
                else if (table.tryGetMaster(card, out var master))
                {
                    master.onDeath.invokeBeforeAnim();
                    _animList.Add(new AnimAnim(master.animator, master.onDeath.animName));
                }
                else if (table.tryGetItem(card, out var item))
                {
                    item.onDestroy.invokeBeforeAnim();
                    _animList.Add(new AnimAnim(item.animator, item.onDestroy.animName));
                }
            }
            bool isAllAnimDone = true;
            foreach (var anim in _animList)
            {
                if (!anim.update(table, time))
                    isAllAnimDone = false;
            }
            if (!isAllAnimDone)
                return false;
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
                item.hide();
            }
            table.setSkill(table.ui.SelfSkill, table.player.getSkill());
            return true;
        }

        public override bool blockAnim(UIAnimation nextAnim)
        {
            return false;
        }
    }
}
