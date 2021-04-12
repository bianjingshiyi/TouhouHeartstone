using TouhouCardEngine;

namespace TouhouHeartstone
{
    public abstract class THHCardDefine : CardDefine
    {
        /// <summary>
        /// 是否是衍生物？
        /// </summary>
        public virtual bool isToken { get; set; } = false;
    }
}