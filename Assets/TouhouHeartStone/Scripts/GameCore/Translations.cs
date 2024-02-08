using MukioI18n;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    public static class Translations
    {
        [TranslateMsg("游戏事件日志项描述，{0}是玩家，{1}是选项")]
        public const string EVENT_LOG_DESC_TEMPLATE_SELECT = "{0} 选择了 {1}";
        [TranslateMsg("游戏事件日志项描述，{0}是玩家")]
        public const string EVENT_LOG_DESC_TEMPLATE_SELECT_NOT_SHOW = "{0} 进行了选择";

        #region 游戏日志名
        public const string CONTEXT_GAME_LOG_NAME = "游戏日志名";

        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_SELECT_CARD = "选择";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_SET_GEM = "灵力变化";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_ATTACK = "攻击";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_DEATH = "死亡";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_PLAY = "使用";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_FAITHDUEL = "信仰对决";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_REVIVE = "复活";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_FATIGUE = "疲劳";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_EQUIP = "装备";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_SUMMON = "召唤";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_DRAW = "抽牌";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_MILL = "爆牌";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_REVEAL = "揭示";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_DISPLAY = "展示";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_SEAL = "封印";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_SHACKLE = "禁锢";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_TRAP = "陷阱";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_MISSION = "委托";
        [TranslateMsg("游戏事件日志项名称", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_DISCARD = "弃牌";
        [TranslateMsg("游戏事件日志项名称-伤害事件（{0}为失去的生命值）", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_DAMAGE = "-{0}";
        [TranslateMsg("游戏事件日志项名称-护甲值更改事件（{0}为当前护甲值）", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_ARMOR = "=>{0}";
        [TranslateMsg("游戏事件日志项名称-治疗事件（{0}为获得的生命值）", CONTEXT_GAME_LOG_NAME)]
        public const string EVENT_LOG_ITEM_NAME_HEAL = "+{0}";
        #endregion

        #region 节点参数
        public const string CONTEXT_NODE_PARAM_NAME = "节点参数名称";

        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_ENABLE_ACTION = "生效动作";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_DISABLE_ACTION = "失效动作";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_TRIGGER_CONDITION = "触发条件";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_AFFECT_CONDITION = "生效条件";
        [TranslateMsg("编辑器/参数名称（{0}为目标的号码）", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_TARGET_CONDITION = "目标{0}条件";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_PLAYER = "玩家";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_CARD = "卡牌";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_BUFF_DEFINE = "增益定义";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_TARGET_CARD = "目标卡牌";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_TARGET_BUFF = "目标增益";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_TAG_LIST = "标签列表";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_PRIORITY = "优先级";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_ACTIVE_PILES = "生效区域";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_AFFECT_PILES = "作用区域";
        #endregion
    }
}
