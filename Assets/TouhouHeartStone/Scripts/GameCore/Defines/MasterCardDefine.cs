using System;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    [Obsolete("请勿直接使用内置属性")]
    public abstract class MasterCardDefine : THHCardDefine
    {
        protected MasterCardDefine() : base(DEFINE.MASTER)
        {
        }
        public abstract int skillID { get; set; }
    }
}