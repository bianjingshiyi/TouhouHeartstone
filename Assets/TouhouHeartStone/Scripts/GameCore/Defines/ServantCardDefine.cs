using System;
using TouhouCardEngine;
namespace TouhouHeartstone
{
    [Obsolete("请勿直接使用内置属性")]
    public abstract class ServantCardDefine : THHCardDefine
    {
        protected ServantCardDefine() : base(DEFINE.SERVANT)
        {
        }
    }
}