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

        #endregion
    }
}