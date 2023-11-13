namespace PVE
{
    public static class GEMTAG
    {
        #region 效果类型标签
        /// <summary>
        /// 表示这个插件只能为主动效果添加内容，不能被插入其他效果插槽中。
        /// </summary>
        public const string ACTIVE = "active";
        /// <summary>
        /// 表示这个插件只能为被动效果添加内容，不能被插入其他效果插槽中。
        /// </summary>
        public const string PASSIVE = "passive";
        /// <summary>
        /// 表示这个插件只能为光环效果添加内容，不能被插入其他效果插槽中。
        /// </summary>
        public const string AURA = "aura";
        #endregion

        #region 节点类型标签
        /// <summary>
        /// 表示这个插件会为效果添加条件。
        /// </summary>
        public const string CONDITION = "condition";
        /// <summary>
        /// 表示这个插件会为效果添加动作。
        /// </summary>
        public const string ACTION = "action";
        #endregion

        #region 节点类型标签
        /// <summary>
        /// 表示这个插件只能为宣言效果添加内容。
        /// </summary>
        public const string PLAYED = "condition";
        /// <summary>
        /// 表示这个插件只能为遗言效果添加内容。
        /// </summary>
        public const string LASTWORD = "action";
        #endregion

        #region 节点类型标签
        /// <summary>
        /// 表示这个插件只能为有目标的效果添加内容。
        /// </summary>
        public const string TARGET = "target";
        /// <summary>
        /// 表示这个插件只能为以随从为目标的效果添加内容。
        /// </summary>
        public const string TARGET_SERVANT = "target_servant";
        #endregion


        #region 卡牌类型标签
        /// <summary>
        /// 表示这个插件只能为随从的效果添加内容。
        /// </summary>
        public const string SERVANT = "servant";
        /// <summary>
        /// 表示这个插件只能为法术的效果添加内容。
        /// </summary>
        public const string SPELL = "spell";
        #endregion
    }
}
