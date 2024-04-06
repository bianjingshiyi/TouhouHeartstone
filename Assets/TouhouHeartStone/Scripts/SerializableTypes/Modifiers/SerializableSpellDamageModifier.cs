using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    [BsonDiscriminator("SpellDamageModifier")]
    public class SerializableSpellDamageModifier : SerializableIntPropModifier
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableSpellDamageModifier() : base()
        {
        }
        public SerializableSpellDamageModifier(SpellDamageModifier modifier) : base(modifier)
        {
        }
        public override PropModifier deserialize()
        {
            return new SpellDamageModifier(getDefaultValue(), isSet)
            {
                relatedPropName = relatedPropName
            };
        }
    }
}
