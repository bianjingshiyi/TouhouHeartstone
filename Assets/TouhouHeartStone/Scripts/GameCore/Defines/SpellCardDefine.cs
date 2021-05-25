using TouhouCardEngine;
using System;
namespace TouhouHeartstone
{
    [Obsolete("请勿直接使用内置属性")]
    public abstract class SpellCardDefine : THHCardDefine
    {
        protected SpellCardDefine() : base(DEFINE.SPELL)
        {
        }
    }
}