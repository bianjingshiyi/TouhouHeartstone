using System;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
namespace TouhouHeartstone
{
    public class CostModifier : IntPropModifier
    {
        public CostModifier(int value) : base(DEFINE.COST, value)
        {
        }
        public CostModifier(int value, bool isSet) : base(DEFINE.COST, value, isSet)
        {
        }
        public override SerializablePropModifier serialize()
        {
            return new SerializableCostModifier(this);
        }
    }
    [Serializable]
    [BsonDiscriminator("CostModifier")]
    public class SerializableCostModifier : SerializableIntPropModifier
    {
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
        [Obsolete]
        [BsonIgnoreIfNull]
        public CheckConditionDelegate onCheckCondition;
    }
}