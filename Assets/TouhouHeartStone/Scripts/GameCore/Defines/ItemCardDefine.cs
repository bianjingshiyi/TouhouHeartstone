using TouhouCardEngine;
using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Obsolete("请勿直接使用内置属性")]
    public abstract class ItemCardDefine : THHCardDefine
    {
        public ItemCardDefine() : base(DEFINE.ITEM)
        {
        }
    }
}