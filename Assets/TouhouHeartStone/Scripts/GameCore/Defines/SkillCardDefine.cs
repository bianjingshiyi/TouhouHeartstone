using TouhouCardEngine;
using System;
using System.Reflection;
namespace TouhouHeartstone
{
    [Obsolete("请勿直接使用内置属性")]
    public abstract class SkillCardDefine : THHCardDefine
    {
        public override string type { get; set; } = DEFINE.SKILL;
        public override string isUsable(CardEngine engine, TouhouCardEngine.Player player, Card card)
        {
            return null;
        }
    }
}