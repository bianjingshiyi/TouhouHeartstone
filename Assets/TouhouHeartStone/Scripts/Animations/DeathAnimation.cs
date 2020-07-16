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
        public override bool update(TableManager table, THHCard.DeathEventArg eventArg)
        {
            if (_animList == null)
            {
                _animList = new List<AnimAnim>();
                foreach (var p in eventArg.infoDic)//列举所有死亡的信息
                {
                    if (table.tryGetServant(p.Key, out var servant))//尝试获取死亡的随从的随从UI
                    {
                        _animList.Add(new AnimAnim(servant.animator, "Servant_Death"));
                        //table.ui.SelfFieldList.removeItem(servant);//从自己的场上移除随从UI
                        //table.ui.EnemyFieldList.removeItem(servant);//从敌人的场上移除随从UI
                    }
                }
            }
            bool isAllAnimDone = true;
            foreach (var anim in _animList)
            {
                if (!anim.update(table))
                    isAllAnimDone = false;
            }
            if (!isAllAnimDone)
                return false;
            foreach (var p in eventArg.infoDic)//列举所有死亡的信息
            {
                if (table.tryGetServant(p.Key, out var servant))//尝试获取死亡的随从的随从UI
                {
                    table.ui.SelfFieldList.removeItem(servant);//从自己的场上移除随从UI
                    table.ui.EnemyFieldList.removeItem(servant);//从敌人的场上移除随从UI
                }
            }
            return true;
        }
    }
}
