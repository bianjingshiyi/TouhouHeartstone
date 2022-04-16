using System;
using System.Linq;
using System.Collections.Generic;

using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class GeneratedCardDefine : CardDefine
    {
        public GeneratedCardDefine(int id, string type, Dictionary<string, object> props, GeneratedEffect[] effects) : base(id, type, props, effects)
        {
        }
        public override void merge(CardDefine newVersion)
        {
            if (newVersion.type != type)
                UberDebug.LogWarning(newVersion + "的类型与" + this + "不同，可能是一次非法的数据合并！");
            foreach (var propName in getPropNames())
            {
                setProp(propName, newVersion.getProp<object>(propName));
            }
        }
        public override string ToString()
        {
            return "Generated<" + id + ">";
        }
    }
}