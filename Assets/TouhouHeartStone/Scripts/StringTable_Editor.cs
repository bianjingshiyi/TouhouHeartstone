using MukioI18n;

namespace Game
{
    partial class StringTable
    {
        [TranslateMsg("编辑器中效果类型名称之通用效果")]
        public const string EDITOR_EFFECT_NAME_NORMAL_EFFECT = "通用效果";
        [TranslateMsg("编辑器中效果类型名称之光环效果")]
        public const string EDITOR_EFFECT_NAME_HALO_EFFECT = "光环效果";
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
        [TranslateMsg("动作图编辑器中动作节点参数名称之条件")]
        public const string EDITOR_PARAM_NAME_CONDITION = "条件";
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
        public const string EDITOR_NODE_PARAM_NAME_REMOVE_MODIFIER_EVENT = "移除修改器事件";
        [TranslateMsg("动作图编辑器中动作节点名称之获取属性")]
        public const string EDITOR_NODE_NAME_GET_PROPERTY = "获取属性";
        [TranslateMsg("动作图编辑器中动作输入名称之属性名称")]
        public const string EDITOR_PARAM_NAME_PROPERTY_NAME = "属性名称";
        [TranslateMsg("动作图编辑器中动作输出名称之值")]
        public const string EDITOR_NODE_PARAM_NAME_VALUE = "值";
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
    }
}

