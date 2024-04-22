using MukioI18n;

namespace Game
{
    public partial class StringTable
    {
        #region 主界面
        [TranslateMsg("发现了新版本的游戏，询问玩家是否要打开一个外部链接进行下载")]
        public const string WANNA_DOWNLOAD_NEW_VERSION = "发现了更新版本的游戏，是否下载？";
        [TranslateMsg("发现了新版本的游戏，但是没有适合的下载链接")]
        public const string INVALID_DOWNLOAD_URL = "发现了更新版本的游戏，但无法获取下载链接。请访问官网手动更新游戏。";
        #endregion

        #region 房间
        [TranslateMsg("人机的玩家名")]
        public const string LOCAL_PLAYER = "本地玩家";
        #endregion

        #region 收藏
        [TranslateMsg("浏览卡牌界面中按费用排序选项")]
        public const string CARDVIEW_SORTTYPE_COST = "灵力";
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
        public const string BUILD_ERROR_DECKCODE_FORMATE_ERROR = "卡组代码格式错误或与此版本不兼容。\n请检查组卡代码是否复制完整，或卡组是否使用了新版本游戏导出。";
        [TranslateMsg]
        public const string BUILD_ERROR_IMPORT_FAILED = "导入失败\n{0}";
        [TranslateMsg]
        public const string BUILD_CONFIRM_EXIT_WITHOUT_SAVE = "卡组尚未保存，退出后现有变更将丢失。\n是否退出？";
        [TranslateMsg("未知卡牌的名字")]
        public const string BUILD_UNKNOWN_CARD_NAME = "未知卡牌";
        #endregion

        #region 牌堆
        public const string CONTEXT_PILE_NAME = "牌堆名";

        [TranslateMsg("牌堆名称", CONTEXT_PILE_NAME)]
        public const string PILE_NAME_INIT = "初始手牌";
        [TranslateMsg("牌堆名称", CONTEXT_PILE_NAME)]
        public const string PILE_NAME_WARP = "暂存区";
        [TranslateMsg("牌堆名称", CONTEXT_PILE_NAME)]
        public const string PILE_NAME_FIELD = "战场";
        [TranslateMsg("牌堆名称", CONTEXT_PILE_NAME)]
        public const string PILE_NAME_HAND = "手牌";
        [TranslateMsg("牌堆名称", CONTEXT_PILE_NAME)]
        public const string PILE_NAME_DECK = "牌库";
        [TranslateMsg("牌堆名称", CONTEXT_PILE_NAME)]
        public const string PILE_NAME_GRAVE = "墓地";
        [TranslateMsg("牌堆名称", CONTEXT_PILE_NAME)]
        public const string PILE_NAME_MASTER = "英雄";
        [TranslateMsg("牌堆名称", CONTEXT_PILE_NAME)]
        public const string PILE_NAME_SKILL = "技能";
        [TranslateMsg("牌堆名称", CONTEXT_PILE_NAME)]
        public const string PILE_NAME_ITEM = "道具";
        [TranslateMsg("牌堆名称", CONTEXT_PILE_NAME)]
        public const string PILE_NAME_DELAY = "延迟";
        [TranslateMsg("牌堆名称", CONTEXT_PILE_NAME)]
        public const string PILE_NAME_ANY = "任意";
        #endregion

        #region 游戏
        [TranslateMsg("游戏中卡牌可选的目标类型之角色")]
        public const string GAME_TARGET_TYPE_CHARACTER = "角色";
        [TranslateMsg("游戏中卡牌可选的目标类型之仆从")]
        public const string GAME_TARGET_TYPE_SERVANT = "仆从";
        #endregion

        #region 修改器运算
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
        [TranslateMsg("游戏中卡牌的属性修改类型之增加属性")]
        public const string GAME_CARD_PROP_MODIFY_ADD_PROP = "增加属性";
        [TranslateMsg("游戏中卡牌的属性修改类型之减少属性")]
        public const string GAME_CARD_PROP_MODIFY_SUB_PROP = "减少属性";
        [TranslateMsg("游戏中卡牌的属性修改类型之变为属性")]
        public const string GAME_CARD_PROP_MODIFY_SETTO_PROP = "变为属性";
        #endregion

        #region 卡牌类型
        public const string CONTEXT_CARD_TYPE_NAME = "卡牌类型名";

        [TranslateMsg("卡牌类型名", CONTEXT_CARD_TYPE_NAME)]
        public const string CARD_TYPE_SERVANT = "仆从";
        [TranslateMsg("卡牌类型名", CONTEXT_CARD_TYPE_NAME)]
        public const string CARD_TYPE_SPELL = "法术";
        [TranslateMsg("卡牌类型名", CONTEXT_CARD_TYPE_NAME)]
        public const string CARD_TYPE_ITEM = "道具";
        [TranslateMsg("卡牌类型名", CONTEXT_CARD_TYPE_NAME)]
        public const string CARD_TYPE_MASTER = "英雄";
        [TranslateMsg("卡牌类型名", CONTEXT_CARD_TYPE_NAME)]
        public const string CARD_TYPE_SKILL = "技能";
        #endregion

        #region 增益
        [TranslateMsg("增益名称")]
        public const string BUFF_NAME_UNKNOWN = "未知增益";
        [TranslateMsg("未知增益的描述")]
        public const string BUFF_DESC_UNKNOWN = "不知道这是干什么的……";
        #endregion

        #region 关键词名称
        public const string CONTEXT_KEYWORD_NAME = "关键词名称";

        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_TAUNT = "守卫";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_CHARGE = "先攻";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_RUSH = "速攻";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_SHIELD = "护盾";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_STEALTH = "隐匿";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_PERMANENT_STEALTH = "永久隐匿";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_DRAIN = "吸血";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_POISONOUS = "致命";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_ELUSIVE = "魔免";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_FREEZE = "被禁锢";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_PERMANENT_FROZEN = "永久禁锢";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_CAN_BE_CHARGED = "可蓄力";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_UNIQUE = "唯一";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_AUTOCAST = "自动施放";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_CANTATTACK = "无法攻击";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_WINDFURY = "双击";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_IMMUNITY = "无敌";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_SECRET = "陷阱";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_MISSION = "委托";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_BATTLECRY = "宣言";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_DEATHRATTLE = "遗言";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_UNKNOWN = "未知关键词";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_CHARGED = "蓄力";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_SHACKLE = "禁锢";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_SEAL = "封印";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_FAITHDUEL = "信仰对决";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_UNPLAYABLE = "无法使用";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_ELEMENTAL_SPELL = "元素法术";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_DOWSE = "寻觅";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_ILLUSIVE = "幻象";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_VANISH = "消失";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_SPELL_DAMAGE = "法术伤害";
        [TranslateMsg("关键词名称", CONTEXT_KEYWORD_NAME)]
        public const string KEYWORD_NAME_DAMAGE_REDUCTION = "减伤";
        #endregion

        #region 种族
        [TranslateMsg("仆从种族")]
        public const string CARD_TAG_FAIRY = "妖精";
        [TranslateMsg("仆从种族")]
        public const string CARD_TAG_MECH = "机械";
        [TranslateMsg("仆从种族")]
        public const string CARD_TAG_DEMON = "恶魔";
        [TranslateMsg("仆从种族")]
        public const string CARD_TAG_ELEMENTAL = "元素";
        [TranslateMsg("仆从种族")]
        public const string CARD_TAG_BEAST = "妖兽";
        [TranslateMsg("仆从种族")]
        public const string CARD_TAG_GOD = "神明";
        [TranslateMsg("仆从种族")]
        public const string CARD_TAG_GHOST = "幽灵";
        #endregion

        #region 标签
        [TranslateMsg("游戏中的法术标签火")]
        public const string GAME_TAG_FIRE = "火";
        [TranslateMsg("游戏中的法术标签金")]
        public const string GAME_TAG_METAL = "金";
        [TranslateMsg("游戏中的法术标签木")]
        public const string GAME_TAG_WOOD = "木";
        [TranslateMsg("游戏中的法术标签水")]
        public const string GAME_TAG_WATER = "水";
        [TranslateMsg("游戏中的法术标签土")]
        public const string GAME_TAG_EARTH = "土";
        [TranslateMsg("游戏中的法术标签日")]
        public const string GAME_TAG_SUN = "日";
        [TranslateMsg("游戏中的法术标签月")]
        public const string GAME_TAG_MOON = "月";
        #endregion

        #region 卡牌属性
        public const string CONTEXT_CARD_PROPERTY = "卡牌属性";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_ISDEAD = "濒死";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_CURRENT_LIFE = "当前生命值";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_ARMOR = "护甲值";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_SILENCED = "被封印";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_ISREADY = "已就绪";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_ATTACK_TIMES = "攻击次数";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_COST = "灵力值消耗";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_ATTACK = "攻击力";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_LIFE = "生命值";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_SPELL_DAMAGE = "法术伤害";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_DAMAGE_REDUCE = "减伤";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_KEYWORDS = "关键词";
        [TranslateMsg("卡牌属性名", CONTEXT_CARD_PROPERTY)]
        public const string CARD_PROP_NAME_CUSTOM_PROPERTY = "自定义属性";
        #endregion

        #region 错误
        [TranslateMsg]
        public const string ERROR_CARD_WITH_ID_NOT_EXISTS = "不存在ID为{0}的卡。";
        [TranslateMsg]
        public const string ERROR_INVALID_IN_VISITOR_MODE = "游客无法使用此功能。";
        #endregion
    }
}
