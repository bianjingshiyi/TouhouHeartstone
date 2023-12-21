using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.BattleTable;
using UnityEngine;
using static Sentry.MeasurementUnit;

namespace Game.Animations
{
    /// <summary>
    /// 动画定义
    /// </summary>
    public class GeneratedAnimationDefine : AnimationDefine
    {
        public GeneratedAnimationDefine(long cardPoolId, int id, string name) : base(cardPoolId, id, name)
        {

        }
        public GeneratedAnimationDefine(int id, string name) : this(0, id, name)
        {

        }
        public override TableEffect createEffect(BattleTableManager table, Transform parent)
        {
            throw new NotImplementedException();
        }
    }
    [Serializable]
    public class SerializableAnimationDefine
    {
        #region 公有方法
        #region 构造方法
        public SerializableAnimationDefine(AnimationDefine animDef)
        {
            id = animDef.id;
            name = animDef.name;
            duration = animDef.duration;
            blockTime = animDef.blockTime;
            foreach (string propName in animDef.getPropNames())
            {
                propDict.Add(propName, animDef.getProp<object>(propName));
            }
        }
        #endregion
        public GeneratedAnimationDefine toAnimDefine()
        {
            var anim = new GeneratedAnimationDefine(id, name);
            anim.duration = duration;
            anim.blockTime = blockTime;
            foreach (var pair in propDict)
            {
                anim.setProp(pair.Key, pair.Value);
            }
            return anim;
        }
        #endregion
        #region 属性字段
        public int id;
        public string name;
        public float duration;
        public float blockTime;
        public Dictionary<string, object> propDict = new Dictionary<string, object>();
        #endregion
    }
}