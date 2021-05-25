using TouhouCardEngine;
using System;
using System.Reflection;
namespace TouhouHeartstone
{
    [Obsolete("请勿直接使用内置属性")]
    public abstract class SkillCardDefine : THHCardDefine
    {
        protected SkillCardDefine() : base(DEFINE.SKILL)
        {
        }
    }
}