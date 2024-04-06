using MessagePack;
using System;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableInputDefaultValue
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableInputDefaultValue()
        {
        }
        public SerializableInputDefaultValue(InputDefaultValue value)
        {
            name = value.name;
            paramIndex = value.paramIndex;
            this.value = value.value;
        }
        public InputDefaultValue deserialize()
        {
            return new InputDefaultValue(name, paramIndex, value);
        }
        [Key(0)]
        public string name;
        [Key(1)]
        public int paramIndex;
        [Key(2)]
        public object value;
    }
}
