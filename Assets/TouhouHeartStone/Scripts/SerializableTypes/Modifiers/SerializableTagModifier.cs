using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableTagModifier : SerializablePropModifier<string[]>
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableTagModifier() : base()
        {
        }
        public SerializableTagModifier(TagModifier modifier) : base(modifier)
        {
            modifyType = (int)modifier.modifyType;
        }
        public override PropModifier deserialize()
        {
            return new TagModifier((TagModifier.ModifyType)modifyType, getDefaultValue())
            {
                relatedPropName = relatedPropName
            };
        }
        [Key(3)]
        public int modifyType;
    }
}
