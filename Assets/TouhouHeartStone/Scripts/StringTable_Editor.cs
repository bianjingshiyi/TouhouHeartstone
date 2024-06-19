using MukioI18n;
using TouhouHeartstone;

namespace Game
{
    partial class StringTable
    {
        #region UI
        [TranslateMsg("编辑器中新建卡池的默认名称")]
        public const string EDITOR_NEW_CARDPOOL_DEFAULT_NAME = "新建卡池";
        [TranslateMsg("编辑器中新建皮肤的默认名称")]
        public const string EDITOR_NEW_SKIN_DEFAULT_NAME = "新建皮肤";
        [TranslateMsg("编辑器的动作流程图中的增益节点的属性修整器关联属性默认名称")]
        public const string EDITOR_ACTIONGRAPH_BUFF_PROP_NAME_DEFAULT = "属性名称";
        [TranslateMsg("卡池名模板，{0}为卡池名，{1}为卡池版本")]
        public const string EDITOR_CARDPOOL_NAME_VERSION_TEMPLATE = "{0}（版本{1}）";
        [TranslateMsg("卡池名模板，{0}为卡池名，{1}为卡池版本")]
        public const string EDITOR_CARDPOOL_NAME_VERSION_TEMPLATE_DOT = "·{0}（版本{1}）";
        [TranslateMsg("丢失的卡池名称")]
        public const string EDITOR_UNKNOWN_CARD_POOL = "未知卡池";
        #endregion

        #region 对话框
        [TranslateMsg("尝试退出编辑页面，但没有保存情况下的按钮名称")]
        public const string SAVE = "保存";
        [TranslateMsg("尝试退出编辑页面，但没有保存情况下的按钮名称")]
        public const string DONT_SAVE = "不保存";
        [TranslateMsg("编辑器中当玩家删除一个节点的时候发出的警告")]
        public const string EDITOR_WARNING_DELETE_ACTION_DFINE = "删除一个动作定义会导致所有使用它的节点无效，确定要这么做吗？";
        [TranslateMsg("编辑器中当玩家离开编辑界面但是还有没保存的更改的时候的警告")]
        public const string EDITOR_WARNING_UNSAVED_CHANGES = "你有未保存的改动，是否先保存卡池再离开编辑界面？";
        [TranslateMsg("编辑器中询问玩家是否要删除卡池")]
        public const string EDITOR_DIALOG_CONFIRM_DELETE_CARDPOOL = "你是否要删除这个卡池？";
        [TranslateMsg("编辑器中提示玩家不能删除官方标准卡池")]
        public const string EDITOR_DIALOG_CANNOT_DELETE_STANDARD_CARDPOOL = "你不能删除标准卡池";
        [TranslateMsg("编辑器中提示玩家不能修改官方标准卡池")]
        public const string EDITOR_DIALOG_CANNOT_MODIFY_STANDARD_CARDPOOL = "你不能修改标准卡池";
        #endregion

        #region 错误提示
        [TranslateMsg("编辑器错误提示")]
        public const string EDITOR_ERROR_INVALID_TYPE_CONNECT = "相互连接的输入输出类型必须可以相互转化";
        [TranslateMsg("编辑器错误提示")]
        public const string EDITOR_ERROR_OUTPUT_CONNECT_INPUT = "值输出只能与值输入相互连接";
        [TranslateMsg("编辑器错误提示")]
        public const string EDITOR_ERROR_ACTION_DEFINE_NAME_ALREADY_EXIST = "已存在具有相同名称的动作定义";
        [TranslateMsg("尝试添加重复名称的错误提示")]
        public const string EDITOR_ERROR_NAME_TAKEN = "该名称已被使用！";
        [TranslateMsg("尝试添加空名称的错误提示")]
        public const string EDITOR_ERROR_NAME_EMPTY = "请输入名称！";
        #endregion

        #region 节点类型
        public const string CONTEXT_NODE_TYPE = "节点类型";

        [TranslateMsg("节点类型", CONTEXT_NODE_TYPE)]
        public const string NODE_TYPE_ACTION = "动作";
        [TranslateMsg("节点类型", CONTEXT_NODE_TYPE)]
        public const string NODE_TYPE_FUNCTION = "函数";
        [TranslateMsg("节点类型", CONTEXT_NODE_TYPE)]
        public const string NODE_TYPE_EVENT = "事件";
        #endregion

        #region 节点分类
        public const string CONTEXT_NODE_CATEGORY = "节点分类";

        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_NONE = "无";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_GAME = "游戏";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_PLAYER = "玩家";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_PILE = "牌堆";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_CARD = "卡牌";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_BUFF = "增益";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_EVENT = "事件";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_EVENT_RECORD = "事件记录";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_COLLECTION_OPERATION = "集合运算";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_ACTION = "行动";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_EFFECT = "效果";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_CARD_SNAPSHOT = "卡牌快照";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_CARD_DEFINE = "卡牌定义";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_EVENT_ENTRY_NODE = "事件入口";
        [TranslateMsg("编辑器/节点分类", CONTEXT_NODE_CATEGORY)]
        public const string EDITOR_NODE_CATEGORY_OTHER = "其他";
        #endregion

        #region 节点参数
        public const string CONTEXT_NODE_PARAM_NAME = Translations.CONTEXT_NODE_PARAM_NAME;

        public const string EDITOR_PARAM_NAME_PLAYER = Translations.EDITOR_PARAM_NAME_PLAYER;
        public const string EDITOR_PARAM_NAME_CARD = Translations.EDITOR_PARAM_NAME_CARD;
        public const string EDITOR_PARAM_NAME_TARGET_CARD = Translations.EDITOR_PARAM_NAME_TARGET_CARD;
        public const string EDITOR_PARAM_NAME_BUFF_DEFINE = Translations.EDITOR_PARAM_NAME_BUFF_DEFINE;
        public const string EDITOR_PARAM_NAME_TAG_LIST = Translations.EDITOR_PARAM_NAME_TAG_LIST;

        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_BUFF = "增益";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_IS_BUFF_REMOVED = "增益是否移除";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_CARD_SNAPSHOT = "卡牌快照";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_DRAWN_CARD = "抽到的卡";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_OPPONENT_PLAYER = "对手玩家";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_MOVED_CARD = "移动的卡牌";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_PILE = "牌堆";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_COLLECTION = "集合";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_COUNT = "数量";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_REPEAT_TIME = "重复次数";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_CONDITION = "条件";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_FILTERING_CARD = "被过滤的卡牌";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_HANDS = "手牌";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_SELECTED_CARD = "所选卡牌";

        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_EVENT = "事件";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_EVENT_RECORD = "事件记录";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_PROP_CHANGE_EVENT = "属性更改事件";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_SILENCE_EVENT = "封印事件";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_DRAW_EVENT = "抽牌事件";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_BURN_EVENT = "爆牌事件";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_DAMAGE_EVENT = "伤害事件";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_HEAL_EVENT = "治疗事件";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_DEATH_EVENT = "死亡事件";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_EVENT_TYPE = "事件类型";

        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_DAMAGE = "伤害";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_BASE_DAMAGE = "基础伤害";

        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_PROPERTY_NAME = "属性名称";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_VALUE = "值";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_OPERATOR = "运算符";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_RESULT = "结果";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_A = "A";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_B = "B";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_BOOLEAN = "真值";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_ACTION = "动作";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_ELSE_ACTION = "否则动作";
        [TranslateMsg("编辑器-参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_TARGETS = "目标卡牌集合";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_CARD_REFERENCE = "卡牌引用";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_CARD_DEFINE = "卡牌定义";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_TAG = "标签";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_ELEMENT = "元素";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_IS_DECK_TOP = "是否置于牌库顶";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_KEYWORD = "关键词";
        [TranslateMsg("编辑器/参数名称", CONTEXT_NODE_PARAM_NAME)]
        public const string EDITOR_PARAM_NAME_CARD_TYPE = "卡牌类型";

        #endregion

        #region 节点文档
        public const string CONTEXT_NODE_DOC_SUMMARY = "节点文档说明";
        public const string CONTEXT_NODE_DOC_EXAMPLE = "节点文档示例";
        public const string CONTEXT_NODE_DOC_PORT_SUMMARY = "节点端口说明";
        #endregion

        #region 节点名
        [TranslateMsg("编辑器中的节点图里输入节点的名字")]
        public const string EDITOR_NODE_NAME_INPUT = "输入节点";
        [TranslateMsg("编辑器中的节点图里输出节点的名字")]
        public const string EDITOR_NODE_NAME_OUTPUT = "输出节点";
        [TranslateMsg("编辑器中未知的节点名称")]
        public const string EDITOR_NODE_NAME_UNKNOWN = "未知节点";

        #region 内置
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_INTEGER_OPERATION = "整数运算";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_LOGIC_OPERATION = "逻辑运算";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_COMPARE = "比较";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_BRANCH_ACTION = "分支动作";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_FOR_ACTION = "重复动作";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_INTEGER_CONST = "固定整数";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_STRING_CONST = "固定字符串";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_BOOLEAN_CONST = "固定真值";
        #endregion

        #region 游戏
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_THIS_CARD = "这张卡牌";
        #endregion

        #region 玩家
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_PILE = "获取牌堆";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_SPELL_DAMAGE = "获取法术伤害";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_TARGET = "获取目标";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_DRAW_TARGET_CARD = "抽目标卡牌";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_SELECT_ONE_FROM_MANY_CARDS = "从一定数量的卡牌中选择一张";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_SELECT_ONE_FROM_MANY_DEFINES = "从一定数量的卡牌定义中选择一张";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_PLAYER_HAND_CARDS = "获取玩家的手牌";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_PLAYER_GRAVE_CARDS = "获取玩家的墓地卡牌";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_PLAYER_DECK_CARDS = "获取玩家牌库中的卡牌";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_SET_PLAYER_PROP = "设置玩家属性";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_PLAYER_PROP = "获取玩家属性";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_MASTER = "获取玩家英雄";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_PLAYER_OPPONENT = "获取玩家对手";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_DISCARD = "丢弃卡牌";
        #endregion

        #region 牌堆
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_FILTER_CARDS_WITH_CONDITION = "过滤满足条件的卡牌";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_CARDS_IN_DECK_WITH_CONDITION = "获取牌库中满足条件的卡牌";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_CARDS_IN_PILE = "获取牌堆中的牌";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_CARDS_COUNT = "获取卡牌数量";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_IS_PLAYER_PILE_CHANGED = "玩家牌堆是否发生改变";
        #endregion

        #region 卡牌
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_OWNER = "获取卡牌拥有者";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_PROPERTY = "获取属性";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_SET_PROPERTY = "设置属性";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_COST = "获取花费";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_ATTACK = "获取攻击力";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_MAX_LIFE = "获取最大生命值";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_UNIQUE_PROPERTY_NAME = "获取唯一属性名";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_ADD_BUFF = "添加增益";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_IS_SPELL = "是否为法术牌";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_IS_TOKEN = "是否为衍生牌";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_REMOVE_BUFF = "移除增益";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_HAS_BUFF = "是否具有增益";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_SET_BUFF_PROP = "设置增益属性";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_THIS_BUFF = "该增益";
        [TranslateMsg("编辑器/卡牌/卡牌类型判断/节点名称")]
        public const string EDITOR_NODE_NAME_CHECK_CARD_TYPE = "卡牌类型判断";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_ALL_CARD_DEFINE = "获取所有卡牌定义";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_CARD_DEFINE = "获取卡牌定义";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_CARD_DEFINE_ARRAY = "获取卡牌定义列表";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_CARD_DEFINE_HAS_TAG = "卡牌定义是否具有标签";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_RANDOM_CARD_DEFINE = "获取随机卡牌定义";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_HAND = "创建衍生卡并置入手牌";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_DEAL_SPECIFIC_DAMAGE_TO_SPECIFIC_NUMBER_OF_RANDOM_TARGET = "对固定数量的随机目标造成固定伤害";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_RANDOM_TARGET = "对固定数量的随机目标造成固定法术伤害";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_HEAL_TARGETS = "治疗目标卡牌";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_HEAL_SPECIFIC_AMOUNT_TO_TARGETS = "对目标卡牌造成固定治疗";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_ALL_SERVANTS = "获取所有仆从";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_ENEMY_SERVANTS = "获取敌方仆从";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_FRIEND_CHARACTERS = "获取友方角色";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_ENEMY_CHARACTERS = "获取敌方角色";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_ALL_CHARACTERS = "获取所有角色";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_RANDOM_CARD = "获取随机卡牌";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_CREATE_TOKEN_CARD_SHUFFLE_TO_DECK = "创建衍生卡并洗入牌库";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_FREEZE = "禁锢";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_SILENCE = "沉默";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_FIELD = "创建衍生卡并置入战场";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_TARGETS = "对目标造成固定法术伤害";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_RANDOM_TARGETS = "造成固定法术伤害并分配给随机目标";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_DESTROY = "消灭";
        #endregion

        #region 事件
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_VARIABLE = "获取变量";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_SET_VARIABLE = "设置变量";
        #endregion

        #region 集合运算
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_CREATE_ARRAY = "创建集合";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_ADD_ELEMENT_TO_ARRAY = "向集合添加元素";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_EXCEPT = "集合相减";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_GET_COUNT = "获取元素数量";
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_WHERE = "筛选";
        #endregion

        #region 行动
        [TranslateMsg("编辑器中的节点名称")]
        public const string EDITOR_NODE_NAME_SIMPLE_DRAW_CARD = "简单抽牌";
        #endregion

        #endregion

        #region 效果
        public const string CONTEXT_EFFECT_NAME = "效果名";
        public const string CONTEXT_EFFECT_TAG = "效果标签";

        [TranslateMsg("效果默认名称", CONTEXT_EFFECT_NAME)]
        public const string EDITOR_EFFECT_NAME_DEFAULT = "效果";
        [TranslateMsg("编辑器中效果类型名称", CONTEXT_EFFECT_NAME)]
        public const string EDITOR_EFFECT_NAME_ACTIVE_EFFECT = "主动效果";
        [TranslateMsg("编辑器中效果类型名称", CONTEXT_EFFECT_NAME)]
        public const string EDITOR_EFFECT_NAME_PASSIVE_EFFECT = "被动效果";
        [TranslateMsg("编辑器中效果类型名称", CONTEXT_EFFECT_NAME)]
        public const string EDITOR_EFFECT_NAME_EVENT_EFFECT = "事件效果";
        [TranslateMsg("编辑器中效果类型名称", CONTEXT_EFFECT_NAME)]
        public const string EDITOR_EFFECT_NAME_AURA_EFFECT = "光环效果";

        [TranslateMsg("编辑器中效果的属性名称")]
        public const string EDITOR_EFFECT_PROP_NAME_PILE_LIST = "生效区域";
        [TranslateMsg("编辑器中效果的属性名称")]
        public const string EDITOR_EFFECT_PROP_NAME_ON_ENABLE_ACTION = "生效动作";
        [TranslateMsg("编辑器中效果的属性名称")]
        public const string EDITOR_EFFECT_PROP_NAME_ON_DISABLE_ACTION = "失效动作";
        [TranslateMsg("编辑器中效果的属性名称")]
        public const string EDITOR_EFFECT_PROP_NAME_RANGE_LIST = "作用区域";
        [TranslateMsg("编辑器中效果的属性名称")]
        public const string EDITOR_EFFECT_PROP_NAME_AFFECT_CONDITION = "作用条件";

        [TranslateMsg("编辑器/效果标签", CONTEXT_EFFECT_TAG)]
        public const string EDITOR_EFFECT_TAG_NAME_PLAYED = "宣言";
        [TranslateMsg("编辑器/效果标签", CONTEXT_EFFECT_TAG)]
        public const string EDITOR_EFFECT_TAG_NAME_LASTWORDS = "遗言";
        [TranslateMsg("编辑器/效果标签", CONTEXT_EFFECT_TAG)]
        public const string EDITOR_EFFECT_TAG_NAME_TRAP = "陷阱";
        #endregion

        #region 事件类型
        public const string CONTEXT_EVENT_NAME = "事件名称";

        [TranslateMsg("事件名称模板（{0}为事件名）", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_BEFORE_TEMPLATE = "{0}时";
        [TranslateMsg("事件名称模板（{0}为事件名）", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_AFTER_TEMPLATE = "{0}后";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_TURN_START = "回合开始";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_USE = "使用卡牌";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_ARMOR_CHANGE = "护甲改变";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_TURN_END = "回合结束";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_SILENCE = "封印";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_ATTACK = "攻击";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_DAMAGE = "伤害";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_HEAL = "治疗";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_DEATH = "死亡/摧毀";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_SECRETTRIGGER = "陷阱触发";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MISSIONCOMPLETE = "任务完成";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_COMEONSTAGE = "宣言";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_INIT = "初始化";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_START = "对战开始";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_GAMEEND = "游戏结束";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_INITREPLACE = "初始手牌替换";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_SETMAXGEM = "设置最大灵力";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_BURN = "爆牌";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_REBORN = "复活";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_ENTERFIELD = "入场";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_REVEAL = "揭示";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_DRAW = "抽卡";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_FATIGUE = "疲劳";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_DISCARD = "弃牌";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_SETGEM = "设置灵力";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_SETDEFINE = "变形";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_REVEALDUEL = "信仰对决";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_ELEMENT = "元素融合";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_EQUIP = "装备道具";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_UNKNOWN = "未知事件";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_ANY = "任意事件";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_ADD_BUFF = "添加增益";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_REMOVE_BUFF = "移除增益";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_SHACKLE = "禁锢";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MOVE_CARD = "卡牌移动";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_CARD_PROP_CHANGE = "卡牌属性变动";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_CHANGE_CARD_VISIBILITY = "卡牌可见性变动";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_CHANGE_CARD_PROP_VISIBILITY = "卡牌属性可见性变动";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_CHANGE_BUFF_PROP_VISIBILITY = "增益属性可见性变动";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_REVEAL_CARD_DEFINE = "展示卡牌定义";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_SET_CARD_PROP = "修改卡牌属性";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_BE_CHARGED = "蓄力";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_SET_EVENT_VAR = "设置事件变量";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_AUTOCAST = "自动施放";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MASS_ADD_BUFF = "群体添加增益";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MASS_DAMAGE = "群体伤害";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MASS_DISCARD = "群体弃牌";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MASS_ENTER_FIELD = "群体入场";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MASS_HEAL = "群体治疗";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MASS_MOVE_CARD = "群体移动卡牌";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MASS_MOVE_TO_HEAD = "群体移动至延迟区";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MASS_RETURN_TO_HAND = "群体移回手牌";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MASS_SEAL = "群体封印";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MASS_SET_CARD_DEFINE = "群体变形";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MASS_SHUFFLE_TO_DECK = "群体洗入牌库";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_MASS_SPLIT_DAMAGE = "分配伤害";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_SELECT_CARD = "选择卡牌";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_SELECT_CARD_DEFINE = "选择卡牌定义";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_LIFESTEAL = "吸血";
        [TranslateMsg("事件名称", CONTEXT_EVENT_NAME)]
        public const string EDITOR_EVENT_NAME_EFFECT_TRIGGER = "效果发动";
        #endregion

        #region 数据类型
        public const string CONTEXT_EDITOR_TYPE = "编辑器数据类型";

        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_UNKNOWN = "未知类型";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_OBJECT = "任意类型";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_INT = "整数";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_FLOAT = "实数";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_BOOL = "真值";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_CHAR = "字符";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_STRING = "文本";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_ARRAY = "集合";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_RULE = "规则";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_CARD_ENGINE = "游戏";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_PLAYER = "玩家";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_PILE = "牌堆";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_PILE_NAME_COLLECTION = "牌堆名集合";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_PILE_NAME_RANGE = "牌堆名范围";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_CARD_DEFINE = "卡牌定义";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_CARD_REFERENCE = "卡牌定义引用";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_CARD = "卡牌";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_CARD_SNAPSHOT = "卡牌快照";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_CARD_TYPE = "卡牌类型";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_KEYWORD = "关键词";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_CARD_TAG = "卡牌标签";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_KEYWORD_COLLECTION = "关键词集合";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_CARD_TAG_COLLECTION = "卡牌标签集合";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_BUFF_DEFINE = "增益定义";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_BUFF_REFERENCE = "增益定义引用";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_BUFF = "增益";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_EFFECT = "效果";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_EFFECT_TAG_COLLECTION = "效果标签集合";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_EVENT_ARG = "事件";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_EVENT_RECORD = "事件记录"; 
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_PROP_MODIFIER = "属性修改器";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_TRIGGER_GRAPH = "触发器";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_TARGET_CHECKER = "目标检测器";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_ACTION_VALUE_REF = "动作值";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_ACTION_DEFINE = "动作定义";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_ACTION_NODE = "动作";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_CARD_PROP_GETTER_NAME = "卡牌属性名（获取）";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_CARD_PROP_SETTER_NAME = "卡牌属性名（设置）";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_CARD_DEFINE_PROP_GETTER_NAME = "卡牌定义属性名（获取）";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_EFFECT_TAG = "效果标签";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_EVENT_VAR_NAME = "事件变量名";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_PILE_NAME = "牌堆名";
        [TranslateMsg("编辑器/数据类型名", CONTEXT_EDITOR_TYPE)]
        public const string EDITOR_TYPE_NAME_EVENT_TYPE = "事件类型"; 
        #endregion
    }
}

