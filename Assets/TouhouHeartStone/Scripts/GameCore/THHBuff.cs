using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System;
using MukioI18n;

namespace TouhouHeartstone
{
    public static class THHBuff
    {
        #region 公有方法

        #region 基础扩展
        public static Task<EventArg> setProp(this Buff card, IGame game, string name, object value)
        {
            return BuffPropertyChangeEventDefine.doEvent(game as CardEngine, card, name, value);
        }
        #endregion

        public static bool isHiddenToVisible(this Buff buff)
        {
            return buff.getProp<bool>(BUFF.HIDDEN_TO_VISIBLE);
        }

        public static Effect[] getActiveEffects(this Buff buff)
        {
            return buff.getEffects().getActiveEffects();
        }
        public static Effect[] getFullTargetActiveEffects(this Buff buff, CardEngine game, Card source)
        {
            return buff.getActiveEffects().Where(e => e.hasFullTargets(game, source, buff)).ToArray();
        }
        public static bool hasFullTargetActiveEffect(this Buff buff, CardEngine game, Card source)
        {
            return buff.getActiveEffects().Any(e => e.hasFullTargets(game, source, buff));
        }
        public static int getTargetCheckerCount(this Buff buff)
        {
            var activeEffects = buff.getActiveEffects();
            if (activeEffects.Length <= 0)
                return 0;
            return activeEffects.Max(e => e.getTargetCheckerCount());
        }
        #endregion
    }
}