using MukioI18n;

namespace Game
{
    public partial class StringTable
    {
        #region 收藏
        [TranslateMsg("浏览卡牌界面中按费用排序选项")]
        public const string CARDVIEW_SORTTYPE_COST = "费用";
        [TranslateMsg("浏览卡牌界面中按名称排序选项")]
        public const string CARDVIEW_SORTTYPE_NAME = "名称";
        [TranslateMsg("浏览卡牌界面中的中立卡牌筛选选项")]
        public const string CARDVIEW_CHARACTER_NEUTRAL = "中立";
        [TranslateMsg("构筑卡牌界面中新建卡组的默认名称")]
        public const string BUILD_NEW_DECK_DEFAULT_NAME = "新建卡组";
        [TranslateMsg("当达到一副卡组可以携带的卡牌数量上限时的提示")]
        public const string BUILD_TIP_DECK_MAX_SIZE = "你不能在一副卡组中携带更多卡牌";
        [TranslateMsg("当达到一张卡牌携带数量上限的时候的提示")]
        public const string BUILD_TIP_CARD_MAX_COUNT = "你不能携带更多同一张卡牌";
        [TranslateMsg("当卡组添加角色无法携带的卡牌的时候的提示")]
        public const string BUILD_TIP_CARD_CARRY_LIMIT = "你不能将这张卡牌添加到卡组中，因为卡组使用的角色无法携带它";
        [TranslateMsg]
        public const string BUILD_CONFIRM_UPLOAD = "卡组数据上传功能将会覆盖您在云端的数据，是否上传？";
        [TranslateMsg]
        public const string BUILD_CONFIRM_DOWNLOAD = "卡组数据下载功能将会从云端同步你的卡组数据并与本地数据合并，可能会覆盖本地的某些卡组，是否下载？";
        [TranslateMsg]
        public const string BUILD_DECK_UPLOAD_COMPLETE = "卡组上传完毕";
        [TranslateMsg]
        public const string BUILD_DECK_DOWNLOAD_COMPLETE = "卡组下载完毕";
        [TranslateMsg]
        public const string BUILD_ERROR_DECKCODE_FORMATE_ERROR = "卡组代码格式错误";
        [TranslateMsg]
        public const string BUILD_ERROR_IMPORT_FAILED = "导入失败\n{0}";
        [TranslateMsg]
        public const string BUILD_CONFIRM_EXIT_WITHOUT_SAVE = "卡组尚未保存，退出后现有变更将丢失。\n是否退出？";
        [TranslateMsg("未知卡牌的名字")]
        public const string BUILD_UNKNOW_CARD_NAME = "未知卡牌";
        #endregion
        #region 游戏
        [TranslateMsg("游戏中战场概念的名称")]
        public const string GAME_FIELD = "战场";
        [TranslateMsg("游戏中手牌概念的名称")]
        public const string GAME_HAND = "手牌";
        [TranslateMsg("游戏中牌库概念的名称")]
        public const string GAME_DECK = "牌库";
        [TranslateMsg("游戏中墓地概念的名称")]
        public const string GAME_GRAVE = "墓地";
        [TranslateMsg("游戏中玩家角色概念的名称")]
        public const string GAME_MASTER = "英雄";
        [TranslateMsg("游戏中技能概念的名称")]
        public const string GAME_SKILL = "技能";
        [TranslateMsg("游戏中物品概念的名称")]
        public const string GAME_ITEM = "物品";
        [TranslateMsg("游戏中延迟区概念的名称")]
        public const string GAME_DELAY = "延迟";
        [TranslateMsg("游戏中卡牌可选的目标类型之角色")]
        public const string GAME_TARGET_TYPE_CHARACTER = "角色";
        [TranslateMsg("游戏中卡牌可选的目标类型之随从")]
        public const string GAME_TARGET_TYPE_SERVANT = "随从";
        [TranslateMsg("游戏中卡牌的费用属性的名称")]
        public const string GAME_CARD_PROP_NAME_COST = "花费";
        [TranslateMsg("游戏中卡牌的攻击力属性的名称")]
        public const string GAME_CARD_PROP_NAME_ATTACK = "攻击力";
        [TranslateMsg("游戏中卡牌的生命值属性的名称")]
        public const string GAME_CARD_PROP_NAME_LIFE = "生命值";
        [TranslateMsg("游戏中卡牌的属性修改类型之增加")]
        public const string GAME_CARD_PROP_MODIFY_ADD = "增加";
        [TranslateMsg("游戏中卡牌的属性修改类型之减少")]
        public const string GAME_CARD_PROP_MODIFY_SUB = "减少";
        [TranslateMsg("游戏中卡牌的属性修改类型之乘以")]
        public const string GAME_CARD_PROP_MODIFY_MUL = "乘以";
        [TranslateMsg("游戏中卡牌的属性修改类型之除以")]
        public const string GAME_CARD_PROP_MODIFY_DIV = "除以";
        [TranslateMsg("游戏中卡牌的属性修改类型之变为")]
        public const string GAME_CARD_PROP_MODIFY_SETTO = "变为";
        [TranslateMsg("游戏中的关键词守卫")]
        public const string GAME_KEYWORD_TAUNT = "守卫";
        [TranslateMsg("游戏中的关键词冲锋")]
        public const string GAME_KEYWORD_CHARGE = "先攻";
        [TranslateMsg("游戏中的关键词速攻")]
        public const string GAME_KEYWORD_RUSH = "速攻";
        [TranslateMsg("游戏中的关键词护盾")]
        public const string GAME_KEYWORD_SHIELD = "护盾";
        [TranslateMsg("游戏中的关键词隐形")]
        public const string GAME_KEYWORD_STEALTH = "隐形";
        [TranslateMsg("游戏中的关键词永久隐形")]
        public const string GAME_KEYWORD_PERMANENT_STEALTH = "永久隐形";
        [TranslateMsg("游戏中的关键词吸血")]
        public const string GAME_KEYWORD_DRAIN = "吸血";
        [TranslateMsg("游戏中的关键词致命")]
        public const string GAME_KEYWORD_POISONOUS = "致命";
        [TranslateMsg("游戏中的关键词魔免")]
        public const string GAME_KEYWORD_ELUSIVE = "魔免";
        [TranslateMsg("游戏中的关键词禁锢")]
        public const string GAME_KEYWORD_FREEZE = "禁锢";
        [TranslateMsg("游戏中的关键词唯一")]
        public const string GAME_KEYWORD_UNIQUE = "唯一";
        [TranslateMsg("游戏中的关键词自动施放")]
        public const string GAME_KEYWORD_AUTOCAST = "自动施放";
        [TranslateMsg("游戏中的关键词无法攻击")]
        public const string GAME_KEYWORD_CANTATTACK = "无法攻击";
        [TranslateMsg("游戏中的关键词双击")]
        public const string GAME_KEYWORD_WINDFURY = "双击";
        [TranslateMsg("游戏中的关键词无敌")]
        public const string GAME_KEYWORD_IMMUNITY = "无敌";
        [TranslateMsg("游戏中的关键词陷阱")]
        public const string GAME_KEYWORD_SECRET = "陷阱";
        [TranslateMsg("游戏中的关键词任务")]
        public const string GAME_KEYWORD_MISSION = "任务";
        #endregion
        #region 错误
        [TranslateMsg]
        public const string ERROR_CARD_WITH_ID_NOT_EXISTS = "不存在ID为{0}的卡。";
        [TranslateMsg]
        public const string ERROR_INVALID_IN_VISITOR_MODE = "游客无法使用此功能。";
        #endregion
    }
}
