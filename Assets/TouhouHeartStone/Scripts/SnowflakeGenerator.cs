using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Game
{
    public class SnowflakeGenerator
    {
        #region 公共方法
        public SnowflakeGenerator(int playerKey)
        {
            this.playerKey = playerKey;
        }
        public long NewId()
        {
            long timestamp = (long)(DateTime.Now - startTime).TotalSeconds;
            if (timestamp < lastTimestamp)
            {
                throw new SnowflakeException("当前时间戳早于上一次生成时的时间戳。");
            }
            else if (timestamp == lastTimestamp)
            {
                throw new SnowflakeException("尝试在生成的1秒冷却中再次生成一个Id。");
            }
            lastTimestamp = timestamp;
            long playerKeyPart = (((long)playerKey) << playerKeyShift) & playerKeyMask;
            long timestampPart = timestamp & timestampMask;
            return playerKeyPart | timestampPart;
        }
        #endregion

        #region 属性字段
        //上次生成ID的时间戳。
        private long lastTimestamp;
        //玩家Key。
        private int playerKey;
        //开始时间戳。
        //private const long startTime = new DateTime(2022, 11, 11, 0, 0, 0, 0).Second;
        //时间戳位数。
        private const int timestampBits = 30;
        /// <summary>
        /// 时间戳占最后30位，以秒计算可以表示17年
        /// </summary>
        private const int timestampMask = 0x3FFFFFFF;
        //玩家Key位数。
        private const int playerKeyBits = 32;
        //玩家Key的位数偏移。
        private const int playerKeyShift = timestampBits;
        private const long playerKeyMask = (long)0xFFFFFFFF << playerKeyShift;
        private readonly DateTime startTime = new DateTime(2022, 11, 11, 0, 0, 0, 0);
        #endregion
    }
    public class SnowflakeException : Exception
    {
        public SnowflakeException(string message) : base(message)
        {

        }
    }
}
