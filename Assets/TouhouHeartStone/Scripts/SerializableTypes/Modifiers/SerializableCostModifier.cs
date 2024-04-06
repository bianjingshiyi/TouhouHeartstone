using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    [BsonDiscriminator("CostModifier")]
    public class SerializableCostModifier : SerializableIntPropModifier
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableCostModifier() : base()
        {
        }
        public SerializableCostModifier(CostModifier modifier) : base(modifier)
        {
        }
        public override PropModifier deserialize()
        {
            return new CostModifier(getDefaultValue(), isSet)
            {
                relatedPropName = relatedPropName
            };
        }
        [IgnoreMember]
        [Obsolete]
        [BsonIgnoreIfNull]
        public CheckConditionDelegate onCheckCondition;
    }
}
