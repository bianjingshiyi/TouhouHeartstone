using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    [BsonIgnoreExtraElements]
    public class SerializableTargetChecker
    {
        #region 公有方法
        #region 构造方法
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableTargetChecker()
        {
        }
        public SerializableTargetChecker(TargetChecker targetChecker)
        {
            if (targetChecker == null)
                throw new ArgumentNullException(nameof(targetChecker));
            targetType = targetChecker.targetType;
            errorTip = targetChecker.errorTip;
        }
        #endregion
        public TargetChecker toTargetChecker()
        {
            return new TargetChecker(targetType, errorTip);
        }
        #endregion
        #region 属性字段
        [Key(0)]
        public string targetType;
        [IgnoreMember]
        [Obsolete]
        public SerializableActionValueRef condition;
        [Key(1)]
        public string errorTip;
        #endregion
    }
}
