using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    [BsonDiscriminator("KeywordsModifier")]
    public class SerializableKeywordsModifier : SerializablePropModifier<string[]>
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableKeywordsModifier() : base()
        {
        }
        public SerializableKeywordsModifier(KeywordsModifier modifier) : base(modifier)
        {
            modifyType = (int)modifier.modifyType;
        }
        public override PropModifier deserialize()
        {
            return new KeywordsModifier((KeywordsModifier.ModifyType)modifyType, getDefaultValue())
            {
                relatedPropName = relatedPropName
            };
        }
        [Key(3)]
        public int modifyType;
    }
}
