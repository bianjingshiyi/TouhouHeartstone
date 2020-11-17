using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using TouhouCardEngine;
using NitoriNetwork.Common;

namespace TouhouHeartstone
{
    /// <summary>
    /// 这个炉石规则是测试用的。
    /// </summary>
    public class HeartStoneRule : Rule
    {
        public HeartStoneRule(params CardDefine[] defines) : base(defines)
        {
            //加载卡池
            //加载内置卡池
            //加载参数卡池
            //加载外置卡池
        }
        public override void onGameStart(CardEngine game, RoomPlayerInfo[] playersInfo)
        {
            throw new NotImplementedException();
        }
    }
}