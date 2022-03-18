using MukioI18n;

namespace Game
{
    partial class StringTable
    {
        [TranslateMsg("编辑器中新建卡池的默认名称")]
        public const string EDITOR_NEW_CARDPOOL_DEFAULT_NAME = "新建卡池";
        [TranslateMsg("编辑器中新建皮肤的默认名称")]
        public const string EDITOR_NEW_SKIN_DEFAULT_NAME = "新建皮肤";
        [TranslateMsg("编辑器中效果的属性名称之生效区域")]
        public const string EDITOR_EFFECT_PROP_NAME_PILE_LIST = "生效区域";
        [TranslateMsg("编辑器中效果的属性名称之生效动作")]
        public const string EDITOR_EFFECT_PROP_NAME_ON_ENABLE_ACTION = "生效动作";
        [TranslateMsg("编辑器中效果的属性名称之失效动作")]
        public const string EDITOR_EFFECT_PROP_NAME_ON_DISABLE_ACTION = "失效动作";
        [TranslateMsg("编辑器中效果的属性名称之触发器列表")]
        public const string EDITOR_EFFECT_PROP_NAME_TRIGGER_LIST = "触发器列表";
        [TranslateMsg("编辑器中效果的属性名称之标签列表")]
        public const string EDITOR_EFFECT_PROP_NAME_TAG_LIST = "标签列表";
        [TranslateMsg("编辑器中效果的属性名称之作用区域")]
        public const string EDITOR_EFFECT_PROP_NAME_RANGE_LIST = "作用区域";
        [TranslateMsg("编辑器中效果的属性名称之作用条件")]
        public const string EDITOR_EFFECT_PROP_NAME_AFFECT_CONDITION = "作用条件";
        [TranslateMsg("编辑器中效果的属性名称之增益定义")]
        public const string EDITOR_EFFECT_PROP_NAME_GET_HALO_BUFF_DEFINE = "增益定义";
        [TranslateMsg("编辑器中效果的属性名称之生效触发器")]
        public const string EDITOR_EFFECT_PROP_NAME_ACTIVE_TRIGGER = "生效触发";
        [TranslateMsg("编辑器中效果类型名称之通用效果")]
        public const string EDITOR_EFFECT_NAME_NORMAL_EFFECT = "通用效果";
        [TranslateMsg("编辑器中效果类型名称之光环效果")]
        public const string EDITOR_EFFECT_NAME_HALO_EFFECT = "光环效果";
        [TranslateMsg("编辑器中效果类型名称之宣言效果")]
        public const string EDITOR_EFFECT_NAME_BATTLECRY_EFFECT = "宣言效果";
        [TranslateMsg("编辑器中动作图编辑器的背景菜单中常量分类的名字")]
        public const string EDITOR_ACTIONGRAPH_MENU_CATEGORY_CONST = "Const";
        [TranslateMsg("编辑器中动作图编辑器的背景菜单中的返回上一层选项")]
        public const string EDITOR_ACTIONGRAPH_MENU_RETURN = "返回上一层";
        [TranslateMsg("编辑器中生效动作节点的名字")]
        public const string EDITOR_NODE_NAME_ONENABLE = "生效";
        [TranslateMsg("编辑器中失效动作节点的名字")]
        public const string EDITOR_NODE_NAME_ONDISABLE = "失效";
        [TranslateMsg("编辑器中动作节点图里输出节点的名字")]
        public const string EDITOR_NODE_NAME_OUTPUT = "输出节点";
        [TranslateMsg("编辑器中动作节点图里触发器节点的名字")]
        public const string EDITOR_NODE_NAME_TRIGGER = "触发器";
        [TranslateMsg("编辑器中目标检查器节点中目标类型编辑框的标注名称")]
        public const string EDITOR_VALUE_NAME_TARGET_TYPE = "目标类型";
        [TranslateMsg("编辑器中目标检查器节点中错误提示编辑框的标注名称")]
        public const string EDITOR_VALUE_NAME_INVALID_MSG = "错误提示";
        [TranslateMsg("编辑器错误提示")]
        public const string EDITOR_ERROR_INVALID_TYPE_CONNECT = "相互连接的输入输出类型必须可以相互转化";
        [TranslateMsg("编辑器错误提示")]
        public const string EDITOR_ERROR_OUTPUT_CONNECT_INPUT = "值输出只能与值输入相互连接";
        [TranslateMsg("编辑器中编辑效果按钮显示文字，参数为效果名称")]
        public const string EDITOR_EDIT_EFFECT = "编辑{0}";
        [TranslateMsg("编辑器中询问玩家是否要删除卡池")]
        public const string EDITOR_DIALOG_CONFIRM_DELETE_CARDPOOL = "你是否要删除这个卡池？";
        [TranslateMsg("编辑器中提示玩家不能删除官方标准卡池")]
        public const string EDITOR_DIALOG_CANNOT_DELETE_STANDARD_CARDPOOL = "你不能删除标准卡池";
        [TranslateMsg("编辑器中提示玩家不能修改官方标准卡池")]
        public const string EDITOR_DIALOG_CANNOT_MODIFY_STANDARD_CARDPOOL = "你不能修改标准卡池";
        [TranslateMsg("编辑器中编辑动作的按钮提示")]
        public const string EDITOR_BUTTON_EDIT_ACTION = "编辑动作";
        [TranslateMsg("编辑器中编辑返回值的按钮提示")]
        public const string EDITOR_BUTTON_EDIT_RETURN_VALUE = "编辑返回值";
        [TranslateMsg("编辑器中编辑触发器的按钮提示")]
        public const string EDITOR_BUTTON_EDIT_TRIGGER = "编辑触发器";
        #region 动作
        #region 内置
        [TranslateMsg("动作图编辑器中动作节点名称之整数运算")]
        public const string EDITOR_NODE_NAME_INTEGER_OPERATION = "整数运算";
        [TranslateMsg("动作图编辑器中动作节点名称之逻辑运算")]
        public const string EDIOTR_NODE_NAME_LOGIC_OPERATION = "逻辑运算";
        [TranslateMsg("动作图编辑器中动作节点名称之比较")]
        public const string EDIOTR_NODE_NAME_COMPARE = "比较";
        [TranslateMsg("动作图编辑器中动作节点名称之固定整数")]
        public const string EDIOTR_NODE_NAME_INTEGER_CONST = "固定整数";
        [TranslateMsg("动作图编辑器中动作节点名称之固定字符串")]
        public const string EDIOTR_NODE_NAME_STRING_CONST = "固定字符串";
        [TranslateMsg("动作图编辑器中动作节点名称之固定真值")]
        public const string EDIOTR_NODE_NAME_BOOLEAN_CONST = "固定真值";
        #endregion
        #region 游戏
        [TranslateMsg("动作图编辑器中动作节点分类之游戏")]
        public const string EDITOR_NODE_CATEGORY_GAME = "游戏";
        [TranslateMsg("动作图编辑器中动作节点名称之这张卡牌")]
        public const string EDITOR_NODE_NAME_THIS_CARD = "这张卡牌";
        [TranslateMsg("动作图编辑器中动作节点名称之获取增益定义")]
        public const string EDITOR_NODE_NAME_GET_BUFF_DEFINE = "获取增益定义";
        [TranslateMsg("动作图编辑器中动作节点名称之Id编号")]
        public const string EDITOR_PARAM_NAME_ID = "编号";
        [TranslateMsg("动作图编辑器中动作节点名称之增益定义")]
        public const string EDITOR_PARAM_NAME_BUFF_DEFINE = "增益定义";
        #endregion
        #region 玩家
        [TranslateMsg("动作图编辑器中动作节点分类之玩家")]
        public const string EDITOR_NODE_CATEGORY_PLAYER = "玩家";
        [TranslateMsg("动作图编辑器中动作节点名称之获取牌堆")]
        public const string EDITOR_NODE_NAME_GET_PILE = "获取牌堆";
        [TranslateMsg("动作图编辑器中动作节点名称之获取法术伤害")]
        public const string EDITOR_NODE_NAME_GET_SPELL_DAMAGE = "获取法术伤害";
        [TranslateMsg("动作图编辑器中动作节点名称之获取目标")]
        public const string EDITOR_NODE_NAME_GET_TARGET = "获取目标";
        [TranslateMsg("动作图编辑器中动作节点名称之抽目标卡牌")]
        public const string EDITOR_NODE_NAME_DRAW_TARGET_CARD = "抽目标卡牌";
        [TranslateMsg("动作图编辑器中动作节点参数名称之玩家")]
        public const string EDITOR_PARAM_NAME_PLAYER = "玩家";
        [TranslateMsg("动作图编辑器中动作节点参数名称之对手玩家")]
        public const string EDITOR_PARAM_NAME_OPPONENT_PLAYER = "对手玩家";
        [TranslateMsg("动作图编辑器中动作节点参数名称之牌堆")]
        public const string EDITOR_PARAM_NAME_PILE = "牌堆";
        [TranslateMsg("动作图编辑器中动作节点参数名称之抽牌事件")]
        public const string EDITOR_PARAM_NAME_DRAW_EVENT = "抽牌事件";
        [TranslateMsg("动作图编辑器中动作节点参数名称之爆牌事件")]
        public const string EDITOR_PARAM_NAME_BURN_EVENT = "爆牌事件";
        [TranslateMsg("动作图编辑器中动作节点名称之从一定数量的卡牌中选择一张")]
        public const string EDITOR_NODE_NAME_SELECT_ONE_FROM_MANY_CARDS = "从一定数量的卡牌中选择一张";
        [TranslateMsg("动作图编辑器中动作节点参数名称之数量")]
        public const string EDITOR_PARAM_NAME_COUNT = "数量";
        [TranslateMsg("动作图编辑器中动作节点名称之获取牌库中满足条件的卡牌")]
        public const string EDITOR_NODE_NAME_GET_CARDS_IN_DECK_WITH_CONDITION = "获取牌库中满足条件的卡牌";
        [TranslateMsg("动作图编辑器中动作节点名称之获取牌堆中的牌")]
        public const string EDITOR_NODE_NAME_GET_CARDS_IN_PILE = "获取牌堆中的牌";
        [TranslateMsg("动作图编辑器中动作节点参数名称之条件")]
        public const string EDITOR_PARAM_NAME_CONDITION = "条件";
        [TranslateMsg("动作图编辑器中动作节点参数名称之触发条件")]
        public const string EDITOR_PARAM_NAME_TRIGGER_CONDITION = "触发条件";
        [TranslateMsg("动作图编辑器中动作节点参数名称之目标{0}条件")]
        public const string EDITOR_PARAM_NAME_TARGET_CONDITION = "目标{0}条件";
        [TranslateMsg("动作图编辑器中动作节点参数名称之获取玩家的手牌")]
        public const string EDITOR_NODE_NAME_GET_PLAYER_HAND_CARDS = "获取玩家的手牌";
        [TranslateMsg("动作图编辑器中动作节点参数名称之获取玩家的墓地中的卡牌")]
        public const string EDITOR_NODE_NAME_GET_PLAYER_GRAVE_CARDS = "获取玩家的墓地卡牌";
        [TranslateMsg("动作图编辑器中动作节点参数名称之设置玩家属性")]
        public const string EDITOR_NODE_NAME_SET_PLAYER_PROP = "设置玩家属性";
        [TranslateMsg("动作图编辑器中动作节点参数名称之获取玩家属性")]
        public const string EDITOR_NODE_NAME_GET_PLAYER_PROP = "获取玩家属性";
        [TranslateMsg("动作图编辑器中动作节点参数名称之获取玩家英雄")]
        public const string EDITOR_NODE_NAME_GET_MASTER = "获取玩家英雄";
        [TranslateMsg("动作图编辑器中动作节点参数名称之获取玩家对手")]
        public const string EDITOR_NODE_NAME_GET_PLAYER_OPPONENT = "获取玩家对手";
        [TranslateMsg("动作图编辑器中动作节点参数名称之手牌")]
        public const string EDITOR_PARAM_NAME_HANDS = "手牌";
        [TranslateMsg("动作图编辑器中动作节点参数名称之初始灵力值")]
        public const string EDITOR_PARAM_NAME_INIT_GEM = "初始灵力值";
        [TranslateMsg("动作图编辑器中动作节点参数名称之替换的卡牌")]
        public const string EDITOR_PARAM_NAME_REPLACED_CARDS = "替换的卡牌";
        [TranslateMsg("动作图编辑器中动作节点参数名称之选项")]
        public const string EDITOR_PARAM_NAME_OPTIONS = "选项";
        [TranslateMsg("动作图编辑器中动作节点参数名称之所选选项")]
        public const string EDITOR_PARAM_NAME_SELECTED_OPTION = "所选选项";
        [TranslateMsg("动作图编辑器中动作节点参数名称之所选卡牌")]
        public const string EDITOR_PARAM_NAME_SELECTED_CARD = "所选卡牌";
        #endregion
        #region 牌堆
        [TranslateMsg("动作图编辑器中动作节点分类之牌堆")]
        public const string EDITOR_NODE_CATEGORY_PILE = "牌堆";
        [TranslateMsg("动作图编辑器中动作节点名称之玩家牌堆是否发生改变")]
        public const string EDITOR_NODE_NAME_IS_PLAYER_PILE_CHANGED = "玩家牌堆是否发生改变";
        #endregion
        #region 卡牌
        [TranslateMsg("动作图编辑器中动作节点分类之卡牌")]
        public const string EDITOR_NODE_CATEGORY_CARD = "卡牌";
        [TranslateMsg("动作图编辑器中动作输入名称之卡牌")]
        public const string EDITOR_PARAM_NAME_CARD = "卡牌";
        [TranslateMsg("动作图编辑器中动作节点名称之获取卡牌拥有者")]
        public const string EDITOR_NODE_NAME_GET_OWNER = "获取卡牌拥有者";
        [TranslateMsg("动作图编辑器中动作输出名称之拥有者")]
        public const string EDITOR_NODE_PARAM_NAME_OWNER = "拥有者";
        [TranslateMsg("动作图编辑器中动作节点名称之添加整数修改器")]
        public const string EDITOR_NODE_NAME_ADD_INT_MODIFIER = "添加整数修改器";
        [TranslateMsg("动作图编辑器中动作输出名称之修改器")]
        public const string EDITOR_NODE_PARAM_NAME_MODIFIER = "修改器";
        [TranslateMsg("动作图编辑器中动作节点名称之移除修改器")]
        public const string EDITOR_NODE_NAME_REMOVE_MODIFIER = "移除修改器";
        [TranslateMsg("动作图编辑器中动作输出名称之移除修改器事件")]
        public const string EDITOR_PARAM_NAME_REMOVE_MODIFIER_EVENT = "移除修改器事件";
        [TranslateMsg("动作图编辑器中动作输出名称之属性更改事件")]
        public const string EDITOR_PARAM_NAME_PROP_CHANGE_EVENT = "属性更改事件";
        [TranslateMsg("动作图编辑器中动作输出名称之沉默事件")]
        public const string EDITOR_PARAM_NAME_SILENCE_EVENT = "沉默事件";
        [TranslateMsg("动作图编辑器中动作节点名称之获取属性")]
        public const string EDITOR_NODE_NAME_GET_PROPERTY = "获取属性";
        [TranslateMsg("动作图编辑器中动作输入名称之属性名称")]
        public const string EDITOR_PARAM_NAME_PROPERTY_NAME = "属性名称";
        [TranslateMsg("动作图编辑器中动作节点名称之设置属性")]
        public const string EDITOR_NODE_NAME_SET_PROPERTY = "设置属性";
        [TranslateMsg("动作图编辑器中动作输入名称之值")]
        public const string EDITOR_PARAM_NAME_VALUE = "值";
        [TranslateMsg("动作图编辑器中动作输入名称之是设置")]
        public const string EDITOR_PARAM_NAME_IS_VALUE = "是设置";
        [TranslateMsg("动作图编辑器中动作节点名称之获取唯一属性名")]
        public const string EDITOR_NODE_NAME_GET_UNIQUE_PROPERTY_NAME = "获取唯一属性名";
        [TranslateMsg("动作图编辑器中动作节点名称之造成伤害")]
        public const string EDITOR_NODE_NAME_DAMAGE = "造成伤害";
        [TranslateMsg("动作图编辑器中动作节点名称之添加增益")]
        public const string EDITOR_NODE_NAME_ADD_BUFF = "添加增益";
        [TranslateMsg("动作图编辑器中动作节点名称之是否为法术牌")]
        public const string EDITOR_NODE_NAME_IS_SPELL = "是否为法术牌";
        [TranslateMsg("动作图编辑器中动作节点名称之移除增益")]
        public const string EDITOR_NODE_NAME_REMOVE_BUFF = "移除增益";
        [TranslateMsg("动作图编辑器中动作节点名称之是否具有增益")]
        public const string EDITOR_NODE_NAME_HAS_BUFF = "是否具有增益";
        [TranslateMsg("动作图编辑器中动作参数名称之增益是否移除")]
        public const string EDITOR_PARAM_NAME_IS_BUFF_REMOVED = "增益是否移除";
        [TranslateMsg("动作图编辑器中动作节点名称之该增益")]
        public const string EDITOR_NODE_NAME_THIS_BUFF = "该增益";
        [TranslateMsg("动作图编辑器中动作参数名称之增益")]
        public const string EDITOR_PARAM_NAME_BUFF = "增益";
        [TranslateMsg("编辑器/卡牌/卡牌类型判断/节点名称")]
        public const string EDITOR_NODE__IS_DEFINE = "卡牌类型判断";
        [TranslateMsg("动作图编辑器中动作参数名称之目标卡牌")]
        public const string EDITOR_PARAM_NAME_TARGET_CARD = "目标卡牌";
        [TranslateMsg("动作图编辑器中动作节点名称之获取卡牌定义")]
        public const string EDITOR_NODE_NAME_GET_CARD_DEFINE = "获取卡牌定义";
        [TranslateMsg("动作图编辑器中动作节点名称之获取卡牌定义列表")]
        public const string EDITOR_NODE_NAME_GET_CARD_DEFINE_ARRAY = "获取卡牌定义列表";
        [TranslateMsg("动作图编辑器中动作参数名称之卡牌引用")]
        public const string EDITOR_PARAM_NAME_CARD_REFERENCE = "卡牌引用";
        [TranslateMsg("动作图编辑器中动作参数名称之卡牌定义")]
        public const string EDITOR_PARAM_NAME_CARD_DEFINE = "卡牌定义";
        [TranslateMsg("动作图编辑器中动作节点名称之获取随机卡牌定义")]
        public const string EDITOR_NODE_NAME_GET_RANDOM_CARD_DEFINE = "获取随机卡牌定义";
        [TranslateMsg("动作图编辑器中动作节点名称之创建衍生卡并置入手牌")]
        public const string EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_HAND = "创建衍生卡并置入手牌";
        [TranslateMsg("动作图编辑器中动作节点名称之对固定数量的随机目标造成固定法术伤害")]
        public const string EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_RANDOM_TARGET = "对固定数量的随机目标造成固定法术伤害";
        [TranslateMsg("动作图编辑器中动作节点参数名称之伤害事件")]
        public const string EDITOR_PARAM_NAME_DAMAGE_EVENT = "伤害事件";
        [TranslateMsg("动作图编辑器中动作节点参数名称之伤害")]
        public const string EDITOR_PARAM_NAME_DAMAGE = "伤害";
        [TranslateMsg("动作图编辑器中动作节点参数名称之基础伤害")]
        public const string EDITOR_PARAM_NAME_BASE_DAMAGE = "基础伤害";
        [TranslateMsg("动作图编辑器中动作节点名称之获取敌方随从")]
        public const string EDITOR_NODE_NAME_GET_ENEMY_SERVANTS = "获取敌方随从";
        [TranslateMsg("动作图编辑器中动作节点名称之获取友方角色")]
        public const string EDITOR_NODE_NAME_GET_FRIEND_CHARACTERS = "获取友方角色";
        [TranslateMsg("动作图编辑器中动作节点名称之获取随机卡牌")]
        public const string EDITOR_NODE_NAME_GET_RANDOM_CARD = "获取随机卡牌";
        [TranslateMsg("动作图编辑器中动作节点名称之创建衍生卡并洗入牌库")]
        public const string EDITOR_NODE_NAME_CREATE_TOKEN_CARD_SHUFFLE_TO_DECK = "创建衍生卡并洗入牌库";
        [TranslateMsg("动作图编辑器中动作节点名称之禁锢")]
        public const string EDITOR_NODE_NAME_FREEZE = "禁锢";
        [TranslateMsg("动作图编辑器中动作节点名称之沉默")]
        public const string EDITOR_NODE_NAME_SILENCE = "沉默";
        [TranslateMsg("动作图编辑器中动作节点名称之创建衍生卡并置入战场")]
        public const string EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_FIELD = "创建衍生卡并置入战场";
        [TranslateMsg("动作图编辑器中动作节点名称之对目标造成固定法术伤害")]
        public const string EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_TARGETS = "对目标造成固定法术伤害";
        [TranslateMsg("动作图编辑器中动作节点名称之消灭")]
        public const string EDITOR_NODE_NAME_DESTROY = "消灭";
        [TranslateMsg("动作图编辑器中动作节点参数名称之死亡事件")]
        public const string EDITOR_PARAM_NAME_DEATH_EVENT = "死亡事件";
        #endregion
        #region 增益
        [TranslateMsg("动作图编辑器中动作节点分类之增益")]
        public const string EDITOR_NODE_CATEGORY_BUFF = "增益";
        #endregion
        #region 修改器 
        [TranslateMsg("动作图编辑器中动作节点分类之修改器")]
        public const string EDITOR_NODE_CATEGORY_MODIFIER = "修改器";
        [TranslateMsg("动作图编辑器中动作节点名称之设置整数修改器值")]
        public const string EDITOR_NODE_NAME_SET_INT_PROP_MODIFIER_VALUE = "设置整数修改器值";
        #endregion
        #region 事件
        [TranslateMsg("动作图编辑器中动作节点分类之事件")]
        public const string EDITOR_NODE_CATEGORY_EVENT = "事件";
        [TranslateMsg("动作图编辑器中动作节点名称之获取变量")]
        public const string EDITOR_NODE_NAME_GET_VARIABLE = "获取变量";
        [TranslateMsg("动作图编辑器中动作节点名称之设置变量")]
        public const string EDITOR_NODE_NAME_SET_VARIABLE = "设置变量";
        #endregion
        #region 集合运算
        [TranslateMsg("动作图编辑器中动作节点分类之集合运算")]
        public const string EDITOR_NODE_CATEGORY_COLLECTION_OPERATION = "集合运算";
        [TranslateMsg("动作图编辑器中动作节点名称之集合相减")]
        public const string EDITOR_NODE_NAME_EXCEPT = "集合相减";
        [TranslateMsg("动作图编辑器中动作节点名称之获取元素数量")]
        public const string EDITOR_NODE_NAME_GET_COUNT = "获取元素数量";
        #endregion
        #endregion
        #region 事件类型
        [TranslateMsg("编辑器中的事件类型之回合开始时")]
        public const string EDITOR_EVENT_NAME_BEFORE_TURN_START = "回合开始时";
        [TranslateMsg("编辑器中的事件类型之回合开始后")]
        public const string EDITOR_EVENT_NAME_AFTER_TURN_START = "回合开始后";
        [TranslateMsg("编辑器中的事件类型之使用卡牌时")]
        public const string EDITOR_EVENT_NAME_BEFORE_USE = "使用卡牌时";
        [TranslateMsg("编辑器中的事件类型之使用卡牌后")]
        public const string EDITOR_EVENT_NAME_AFTER_USE = "使用卡牌后";
        [TranslateMsg("编辑器中的事件类型之发动事件")]
        public const string EDITOR_EVENT_NAME_ACTIVE = "发动事件";
        [TranslateMsg("编辑器中的事件类型之护甲改变时")]
        public const string EDITOR_EVENT_NAME_BEFORE_ARMOR_CHANGE = "护甲改变时";
        [TranslateMsg("编辑器中的事件类型之护甲改变后")]
        public const string EDITOR_EVENT_NAME_AFTER_ARMOR_CHANGE = "护甲改变后";
        [TranslateMsg("编辑器中的事件类型之回合结束时")]
        public const string EDITOR_EVENT_NAME_BEFORE_TURN_END = "回合结束时";
        [TranslateMsg("编辑器中的事件类型之回合结束后")]
        public const string EDITOR_EVENT_NAME_AFTER_TURN_END = "回合结束后";
        [TranslateMsg("编辑器中的事件类型之沉默时")]
        public const string EDITOR_EVENT_NAME_BEFORE_SILENCE = "沉默时";
        [TranslateMsg("编辑器中的事件类型之沉默后")]
        public const string EDITOR_EVENT_NAME_AFTER_SILENCE = "沉默后";
        [TranslateMsg("编辑器中的事件类型之攻击时")]
        public const string EDITOR_EVENT_NAME_BEFORE_ATTACK = "攻击时";
        [TranslateMsg("编辑器中的事件类型之攻击后")]
        public const string EDITOR_EVENT_NAME_AFTER_ATTACK = "攻击后";
        [TranslateMsg("编辑器中的事件类型之伤害时")]
        public const string EDITOR_EVENT_NAME_BEFORE_DAMAGE = "伤害时";
        [TranslateMsg("编辑器中的事件类型之伤害后")]
        public const string EDITOR_EVENT_NAME_AFTER_DAMAGE = "伤害后";
        [TranslateMsg("编辑器中的事件类型之攻击时")]
        public const string EDITOR_EVENT_NAME_BEFORE_HEAL = "治疗时";
        [TranslateMsg("编辑器中的事件类型之攻击后")]
        public const string EDITOR_EVENT_NAME_AFTER_HEAL = "治疗后";
        [TranslateMsg("编辑器中的事件类型之死亡时")]
        public const string EDITOR_EVENT_NAME_BEFORE_DEATH = "死亡时";
        [TranslateMsg("编辑器中的事件类型之死亡后")]
        public const string EDITOR_EVENT_NAME_AFTER_DEATH = "死亡后";
        [TranslateMsg("编辑器中的事件类型之奥秘触发时")]
        public const string EDITOR_EVENT_NAME_BEFORE_SECRETTRIGGER = "奥秘触发时";
        [TranslateMsg("编辑器中的事件类型之奥秘触发后")]
        public const string EDITOR_EVENT_NAME_AFTER_SECRETTRIGGER = "奥秘触发后";
        [TranslateMsg("编辑器中的事件类型之任务完成时")]
        public const string EDITOR_EVENT_NAME_BEFORE_MISSIONCOMPLETE = "任务完成时";
        [TranslateMsg("编辑器中的事件类型之任务完成后")]
        public const string EDITOR_EVENT_NAME_AFTER_MISSIONCOMPLETE = "任务完成后";
        [TranslateMsg("编辑器中的事件类型之宣言时")]
        public const string EDITOR_EVENT_NAME_BEFORE_COMEONSTAGE = "宣言时";
        [TranslateMsg("编辑器中的事件类型之宣言后")]
        public const string EDITOR_EVENT_NAME_AFTER_COMEONSTAGE = "宣言后";
        [TranslateMsg("编辑器中的事件类型之初始化时")]
        public const string EDITOR_EVENT_NAME_BEFORE_INIT = "初始化时";
        [TranslateMsg("编辑器中的事件类型之初始化后")]
        public const string EDITOR_EVENT_NAME_AFTER_INIT = "初始化后";
        [TranslateMsg("编辑器中的事件类型之对战开始时")]
        public const string EDITOR_EVENT_NAME_BEFORE_START = "对战开始时";
        [TranslateMsg("编辑器中的事件类型之对战开始后")]
        public const string EDITOR_EVENT_NAME_AFTER_START = "对战开始后";
        [TranslateMsg("编辑器中的事件类型之回合结束时")]
        public const string EDITOR_EVENT_NAME_BEFORE_TURNEND = "回合结束时";
        [TranslateMsg("编辑器中的事件类型之回合结束后")]
        public const string EDITOR_EVENT_NAME_AFTER_TURNEND = "回合结束后";
        [TranslateMsg("编辑器中的事件类型之游戏结束时")]
        public const string EDITOR_EVENT_NAME_BEFORE_GAMEEND = "游戏结束时";
        [TranslateMsg("编辑器中的事件类型之游戏结束后")]
        public const string EDITOR_EVENT_NAME_AFTER_GAMEEND = "游戏结束后";
        [TranslateMsg("编辑器中的事件类型之初始手牌替换时")]
        public const string EDITOR_EVENT_NAME_BEFORE_INITREPLACE = "初始手牌替换时";
        [TranslateMsg("编辑器中的事件类型之初始手牌替换后")]
        public const string EDITOR_EVENT_NAME_AFTER_INITREPLACE = "初始手牌替换后";
        [TranslateMsg("编辑器中的事件类型之设置最大灵力时")]
        public const string EDITOR_EVENT_NAME_BEFORE_SETMAXGEM = "设置最大灵力时";
        [TranslateMsg("编辑器中的事件类型之设置最大灵力后")]
        public const string EDITOR_EVENT_NAME_AFTER_SETMAXGEM = "设置最大灵力后";
        [TranslateMsg("编辑器中的事件类型之爆牌时")]
        public const string EDITOR_EVENT_NAME_BEFORE_BURN = "爆牌时";
        [TranslateMsg("编辑器中的事件类型之爆牌后")]
        public const string EDITOR_EVENT_NAME_AFTER_BURN = "爆牌后";
        [TranslateMsg("编辑器中的事件类型之选择一项时")]
        public const string EDITOR_EVENT_NAME_BEFORE_CHOOSEONE = "选择一项时";
        [TranslateMsg("编辑器中的事件类型之选择一项后")]
        public const string EDITOR_EVENT_NAME_AFTER_CHOOSEONE = "选择一项后";
        [TranslateMsg("编辑器中的事件类型之复生时")]
        public const string EDITOR_EVENT_NAME_BEFORE_REBORN = "复生时";
        [TranslateMsg("编辑器中的事件类型之复生后")]
        public const string EDITOR_EVENT_NAME_AFTER_REBORN = "复生后";
        [TranslateMsg("编辑器中的事件类型之入场时")]
        public const string EDITOR_EVENT_NAME_BEFORE_ENTERFIELD = "入场时";
        [TranslateMsg("编辑器中的事件类型之入场后")]
        public const string EDITOR_EVENT_NAME_AFTER_ENTERFIELD = "入场后";
        [TranslateMsg("编辑器中的事件类型之揭示时")]
        public const string EDITOR_EVENT_NAME_BEFORE_REVEAL = "揭示时";
        [TranslateMsg("编辑器中的事件类型之揭示后")]
        public const string EDITOR_EVENT_NAME_AFTER_REVEAL = "揭示后";
        [TranslateMsg("编辑器中的事件类型之抽卡时")]
        public const string EDITOR_EVENT_NAME_BEFORE_DRAW = "抽卡时";
        [TranslateMsg("编辑器中的事件类型之抽卡后")]
        public const string EDITOR_EVENT_NAME_AFTER_DRAW = "抽卡后";
        [TranslateMsg("编辑器中的事件类型之疲劳时")]
        public const string EDITOR_EVENT_NAME_BEFORE_FATIGUE = "疲劳时";
        [TranslateMsg("编辑器中的事件类型之疲劳后")]
        public const string EDITOR_EVENT_NAME_AFTER_FATIGUE = "疲劳后";
        [TranslateMsg("编辑器中的事件类型之检视时")]
        public const string EDITOR_EVENT_NAME_BEFORE_SEARCHPILE = "检视时";
        [TranslateMsg("编辑器中的事件类型之检视后")]
        public const string EDITOR_EVENT_NAME_AFTER_SEARCHPILE = "检视后";
        [TranslateMsg("编辑器中的事件类型之激活时")]
        public const string EDITOR_EVENT_NAME_BEFORE_ACTIVE = "激活时";
        [TranslateMsg("编辑器中的事件类型之激活后")]
        public const string EDITOR_EVENT_NAME_AFTER_ACTIVE = "激活后";
        [TranslateMsg("编辑器中的事件类型之弃牌时")]
        public const string EDITOR_EVENT_NAME_BEFORE_DISCARD = "弃牌时";
        [TranslateMsg("编辑器中的事件类型之弃牌后")]
        public const string EDITOR_EVENT_NAME_AFTER_DISCARD = "弃牌后";
        [TranslateMsg("编辑器中的事件类型之设置灵力时")]
        public const string EDITOR_EVENT_NAME_BEFORE_SETGEM = "设置灵力时";
        [TranslateMsg("编辑器中的事件类型之设置灵力后")]
        public const string EDITOR_EVENT_NAME_AFTER_SETGEM = "设置灵力后";
        [TranslateMsg("编辑器中的事件类型之召唤衍生物时")]
        public const string EDITOR_EVENT_NAME_BEFORE_CREATETOKEN = "召唤衍生物时";
        [TranslateMsg("编辑器中的事件类型之召唤衍生物后")]
        public const string EDITOR_EVENT_NAME_AFTER_CREATETOKEN = "召唤衍生物后";
        [TranslateMsg("编辑器中的事件类型之添加修改器时")]
        public const string EDITOR_EVENT_NAME_BEFORE_ADDMODI = "添加修改器时";
        [TranslateMsg("编辑器中的事件类型之添加修改器后")]
        public const string EDITOR_EVENT_NAME_AFTER_ADDMODI = "添加修改器后";
        [TranslateMsg("编辑器中的事件类型之变形时")]
        public const string EDITOR_EVENT_NAME_BEFORE_SETDEFINE = "变形时";
        [TranslateMsg("编辑器中的事件类型之变形后")]
        public const string EDITOR_EVENT_NAME_AFTER_SETDEFINE = "变形后";
        [TranslateMsg("编辑器中的事件类型之移除修改器时")]
        public const string EDITOR_EVENT_NAME_BEFORE_REMOVEMODI = "移除修改器时";
        [TranslateMsg("编辑器中的事件类型之移除修改器后")]
        public const string EDITOR_EVENT_NAME_AFTER_REMOVEMODI = "移除修改器后";
        [TranslateMsg("编辑器中的事件类型之属性修改时")]
        public const string EDITOR_EVENT_NAME_BEFORE_PROPCHANGE = "属性修改时";
        [TranslateMsg("编辑器中的事件类型之属性修改后")]
        public const string EDITOR_EVENT_NAME_AFTER_PROPCHANGE = "属性修改后";
        [TranslateMsg("编辑器中的事件类型之指令时")]
        public const string EDITOR_EVENT_NAME_BEFORE_COMMAND = "指令时";
        [TranslateMsg("编辑器中的事件类型之指令后")]
        public const string EDITOR_EVENT_NAME_AFTER_COMMAND = "指令后";
        [TranslateMsg("编辑器中的事件类型之测试时")]
        public const string EDITOR_EVENT_NAME_BEFORE_TEST = "测试时";
        [TranslateMsg("编辑器中的事件类型之测试后")]
        public const string EDITOR_EVENT_NAME_AFTER_TEST = "测试后";
        [TranslateMsg("编辑器中的事件类型之信仰对决时")]
        public const string EDITOR_EVENT_NAME_BEFORE_REVEALDUEL = "信仰对决时";
        [TranslateMsg("编辑器中的事件类型之信仰对决后")]
        public const string EDITOR_EVENT_NAME_AFTER_REVEALDUEL = "信仰对决后";
        [TranslateMsg("编辑器中的事件类型之发现时")]
        public const string EDITOR_EVENT_NAME_BEFORE_DISCOVER = "发现时";
        [TranslateMsg("编辑器中的事件类型之发现后")]
        public const string EDITOR_EVENT_NAME_AFTER_DISCOVER = "发现后";
        [TranslateMsg("编辑器中的事件类型之选择时")]
        public const string EDITOR_EVENT_NAME_BEFORE_SELECT = "选择时";
        [TranslateMsg("编辑器中的事件类型之选择后")]
        public const string EDITOR_EVENT_NAME_AFTER_SELECT = "选择后";
        [TranslateMsg("编辑器中的事件类型之元素融合时")]
        public const string EDITOR_EVENT_NAME_BEFORE_ELEMENT = "元素融合时";
        [TranslateMsg("编辑器中的事件类型之元素融合后")]
        public const string EDITOR_EVENT_NAME_AFTER_ELEMENT = "元素融合后";
        #endregion
    }
}

