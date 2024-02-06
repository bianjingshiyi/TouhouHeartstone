using MukioI18n;

namespace TouhouHeartstone
{
    public static class Translations
    {
        public const string CONTEXT_GAME_LOG_NAME = "游戏日志名";

        [TranslateMsg("游戏事件日志项描述，{0}是玩家，{1}是选项")]
        public const string EVENT_LOG_DESC_TEMPLATE_SELECT = "{0} 选择了 {1}";
        [TranslateMsg("游戏事件日志项描述，{0}是玩家")]
        public const string EVENT_LOG_DESC_TEMPLATE_SELECT_NOT_SHOW = "{0} 进行了选择";
        [TranslateMsg("游戏事件日志项名称")]
        public const string EVENT_LOG_ITEM_NAME_SELECT_CARD = "选择";
        [TranslateMsg("游戏事件日志项名称")]
        public const string EVENT_LOG_ITEM_NAME_SET_GEM = "灵力变化";
    }
}
