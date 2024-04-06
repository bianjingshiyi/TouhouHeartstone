using MessagePack;
using System;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableBuffExistLimitDefine
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableBuffExistLimitDefine()
        {
        }
        public SerializableBuffExistLimitDefine(BuffExistLimitDefine define)
        {
            triggerTime = define.triggerTime;
            count = define.count;
        }
        public BuffExistLimitDefine toDefine()
        {
            var triggerTime = this.triggerTime == null ? EventTriggerTime.fromName(0, eventName) : this.triggerTime;
            return new BuffExistLimitDefine()
            {
                triggerTime = triggerTime,
                count = count
            };
        }
        [IgnoreMember]
        [Obsolete]
        public string eventName;
        [Key(0)]
        public EventTriggerTime triggerTime;
        [Key(1)]
        public int count;
    }
}
