using System;
using System.Collections.Generic;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    [Obsolete("请勿直接使用内置属性")]
    public abstract class THHCardDefine : CardDefine
    {
        protected THHCardDefine(string type) : base(0, type, null, null)
        {
        }

        /// <summary>
        /// 是否是衍生物？
        /// </summary>
        public virtual bool isToken { get; set; } = false;
        public virtual bool isActive { get; set; } = false;
        public virtual int cost { get; set; } = int.MaxValue;
        public virtual int attack { get; set; } = 0;
        public virtual int life { get; set; } = 0;
        public virtual int spellDamage { get; set; } = 0;
        public virtual int damageReduce { get; set; } = 0;
        public virtual string[] tags { get; set; } = new string[0];
        public virtual string[] keywords { get; set; } = new string[0];
        public override void merge(CardDefine newVersion)
        {
            if (newVersion.type != type)
                UberDebug.LogWarning(newVersion + "的类型与" + this + "不同，可能是一次非法的数据合并！");
            foreach (var propName in getPropNames())
            {
                setProp(propName, newVersion.getProp<object>(propName));
            }
        }
    }
}