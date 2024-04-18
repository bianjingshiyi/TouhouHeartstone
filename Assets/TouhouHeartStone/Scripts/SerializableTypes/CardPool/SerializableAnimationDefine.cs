using Game.Animations;
using MessagePack;
using System;
using System.Collections.Generic;
using TouhouCardEngine;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableAnimationDefine
    {
        #region 公有方法
        #region 构造方法
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableAnimationDefine()
        {
        }
        public SerializableAnimationDefine(AnimationDefine animDef)
        {
            id = animDef.id;
            name = animDef.name;
            duration = animDef.duration;
            blockTime = animDef.blockTime;
            type = (int)animDef.type;
            foreach (string propName in animDef.getPropNames())
            {
                var prop = animDef.getProp<object>(propName);
                propDict.Add(propName, ObjectProxy.TryProxy(prop));
            }
        }
        #endregion
        public GeneratedAnimationDefine toAnimDefine()
        {
            var anim = new GeneratedAnimationDefine(id, name);
            anim.duration = duration;
            anim.blockTime = blockTime;
            anim.type = (AnimationType)type;
            foreach (var pair in propDict)
            {
                var prop = pair.Value;
                anim.setProp(pair.Key, ObjectProxy.TryConvertBack(prop));
            }
            return anim;
        }
        #endregion
        #region 属性字段
        [Key(0)]
        public int id;
        [Key(1)]
        public string name;
        [Key(2)]
        public float duration;
        [Key(3)]
        public float blockTime;
        [Key(4)]
        public int type;
        [Key(5)]
        public Dictionary<string, object> propDict = new Dictionary<string, object>();
        #endregion
    }
}
