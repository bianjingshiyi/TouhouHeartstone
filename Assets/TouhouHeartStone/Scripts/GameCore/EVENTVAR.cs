using MukioI18n;

namespace TouhouHeartstone
{
    public class EVENTVAR
    {
        public const string CONTEXT_EVENT_VAR = "事件变量名";

        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string CARD = "卡牌";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string PLAYER = "玩家";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string BUFF = "增益";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string BUFF_DEFINE = "定义";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string EFFECT = "效果";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string EVENT = "事件";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string EVENT_VAR_NAME = "事件变量名";

        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string CARD_DEFINE = "卡牌定义";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string POSITION = "位置";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string CHOSEN_CARD = "所选卡牌";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string VALUE = "值";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string BEFORE_VALUE = "原数值";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string TARGET_CARD = "目标卡牌";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string TARGETS = "目标";
        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string COST = "花费";

        // 伤害事件
        [TranslateMsg("伤害事件/变量名", CONTEXT_EVENT_VAR)]
        public const string DAMAGE_SOURCE = "伤害来源";
        [TranslateMsg("伤害事件/变量名", CONTEXT_EVENT_VAR)]
        public const string DAMAGE_VALUE = "伤害值";
        [TranslateMsg("伤害事件/变量名", CONTEXT_EVENT_VAR)]
        public const string DAMAGED_VALUE = "实际伤害值";
        [TranslateMsg("伤害事件/变量名", CONTEXT_EVENT_VAR)]
        public const string DAMAGED_CARDS = "击中的卡牌";

        // 治疗事件
        [TranslateMsg("治疗事件/变量名", CONTEXT_EVENT_VAR)]
        public const string HEAL_SOURCE = "治疗来源";
        [TranslateMsg("治疗事件/变量名", CONTEXT_EVENT_VAR)]
        public const string HEALED_VALUE = "实际治疗值";

        // 吸血事件
        [TranslateMsg("吸血事件/变量名", CONTEXT_EVENT_VAR)]
        public const string DRAIN_CARD = "吸血卡牌";

        [TranslateMsg("事件变量名", CONTEXT_EVENT_VAR)]
        public const string SUCCESS = "是否成功";

        // 信仰对决事件
        [TranslateMsg("信仰对决事件/变量名", CONTEXT_EVENT_VAR)]
        public const string SELF_VALID_CARDS = "己方可选卡牌";
        [TranslateMsg("信仰对决事件/变量名", CONTEXT_EVENT_VAR)]
        public const string OPPO_VALID_CARDS = "对方可选卡牌";
        [TranslateMsg("信仰对决事件/变量名", CONTEXT_EVENT_VAR)]
        public const string SELF_CARD = "己方卡牌";
        [TranslateMsg("信仰对决事件/变量名", CONTEXT_EVENT_VAR)]
        public const string OPPO_CARD = "对方卡牌";
        [TranslateMsg("信仰对决事件/变量名", CONTEXT_EVENT_VAR)]
        public const string WIN = "是否胜利";

        // 初始换牌事件
        [TranslateMsg("初始换牌事件/变量名", CONTEXT_EVENT_VAR)]
        public const string REPLACED_CARDS = "替换的卡牌";

        // 移动事件
        [TranslateMsg("移动事件/变量名", CONTEXT_EVENT_VAR)]
        public const string TO_DECK_TOP = "置入牌库顶";
        [TranslateMsg("移动事件/变量名", CONTEXT_EVENT_VAR)]
        public const string MOVED_CARDS = "移动的卡牌";
        [TranslateMsg("移动事件/变量名", CONTEXT_EVENT_VAR)]
        public const string FROM_PILE = "源牌堆";
        [TranslateMsg("移动事件/变量名", CONTEXT_EVENT_VAR)]
        public const string TO_PILE = "目标牌堆";
        [TranslateMsg("移动事件/变量名", CONTEXT_EVENT_VAR)]
        public const string RESET_PROPS = "重置属性";

        // 变形事件
        [TranslateMsg("变形事件/变量名", CONTEXT_EVENT_VAR)]
        public const string TARGET_CARD_DEFINE = "目标卡牌定义";

        // 死亡事件
        [TranslateMsg("死亡事件/变量名", CONTEXT_EVENT_VAR)]
        public const string LASTWORD_BUFFS = "遗言增益列表";
        [TranslateMsg("死亡事件/变量名", CONTEXT_EVENT_VAR)]
        public const string LASTWORD_EFFECTS = "遗言效果列表";

        // 游戏结束事件
        [TranslateMsg("游戏结束事件/变量名", CONTEXT_EVENT_VAR)]
        public const string WINNERS = "获胜玩家";

        // 展示卡牌定义事件
        [TranslateMsg("展示卡牌定义事件/变量名", CONTEXT_EVENT_VAR)]
        public const string INVISIBLE_TO_SELF = "自己不可见";
        [TranslateMsg("展示卡牌定义事件/变量名", CONTEXT_EVENT_VAR)]
        public const string INVISIBLE_TO_OPPO = "对手不可见";

        // 揭示卡牌事件
        [TranslateMsg("揭示卡牌事件/变量名", CONTEXT_EVENT_VAR)]
        public const string PLAY_ANIMATION = "播放动画";

        // 开始游戏事件
        [TranslateMsg("开始游戏事件/变量名", CONTEXT_EVENT_VAR)]
        public const string START_HAND_OFFENSIVE = "先手起始手牌";
        [TranslateMsg("开始游戏事件/变量名", CONTEXT_EVENT_VAR)]
        public const string START_HAND_DEFENSIVE = "后手起始手牌";

        // 更新游戏状态事件
        [TranslateMsg("更新游戏状态事件/变量名", CONTEXT_EVENT_VAR)]
        public const string CLEARED_CARDS = "清理的卡牌";

        // 选择卡牌事件
        [TranslateMsg("选择卡牌事件/变量名", CONTEXT_EVENT_VAR)]
        public const string OPTIONS = "选项";
        [TranslateMsg("选择卡牌事件/变量名", CONTEXT_EVENT_VAR)]
        public const string CARD_SEEN = "明牌";
        [TranslateMsg("选择卡牌事件/变量名", CONTEXT_EVENT_VAR)]
        public const string OPTION_COUNT = "选项数量";
        [TranslateMsg("选择卡牌事件/变量名", CONTEXT_EVENT_VAR)]
        public const string TITLE = "标题";
    }
}