using MukioI18n;

namespace Game
{
    public class StringTable
    {
        #region 通用
        [TranslateMsg("所有按钮上通用的确认")]
        public const string CONFIRM = "确认";
        [TranslateMsg("所有按钮通用的是")]
        public const string YES = "是";
        [TranslateMsg("所有按钮通用的否")]
        public const string NO = "否";
        [TranslateMsg("所有提示通用的错误")]
        public const string ERROR = "错误";
        [TranslateMsg("发现了新版本的游戏，询问玩家是否要打开一个外部链接进行下载")]
        public const string WANNA_DOWNLOAD_NEW_VERSION = "发现了更新版本的游戏，是否下载？";
        [TranslateMsg("发现了新版本的游戏，但是没有适合的下载链接")]
        public const string INVALID_DOWNLOAD_URL = "发现了更新版本的游戏，但无法获取下载链接。请访问官网手动更新游戏。";
        [TranslateMsg]
        public const string HINT_TITLE = "提示";
        [TranslateMsg("在发现界面提示玩家选择一张卡牌")]
        public const string HINT_SELECT_A_CARD = "选择一张卡牌";
        [TranslateMsg("所有按钮通用的删除")]
        public const string DELETE = "删除";
        #endregion
        #region 收藏
        [TranslateMsg("浏览卡牌界面中费用升序排序选项")]
        public const string CARDVIEW_SORT_COST = "费用升序";
        [TranslateMsg("浏览卡牌界面中费用降序排序选项")]
        public const string CARDVIEW_SORT_COST_DESCENDING = "费用降序";
        [TranslateMsg("浏览卡牌界面中稀有度升序排序选项")]
        public const string CARDVIEW_SORT_RARE = "稀有度升序";
        [TranslateMsg("浏览卡牌界面中稀有度降序排序选项")]
        public const string CARDVIEW_SORT_RARE_DESCENDING = "稀有度降序";
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
        #region 编辑器
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
        [TranslateMsg("编辑器中编辑效果按钮显示文字")]
        public const string EDITOR_EDIT_EFFECT = "编辑效果";
        [TranslateMsg("编辑器中询问玩家是否要删除卡池")]
        public const string EDITOR_DIALOG_CONFIRM_DELETE_CARDPOOL = "你是否要删除这个卡池？";
        #region 动作
        #region 游戏
        [TranslateMsg("动作图编辑器中动作节点分类之游戏")]
        public const string EDITOR_NODE_CATEGORY_GAME = "Game";
        [TranslateMsg("动作图编辑器中动作节点名称之这张卡牌")]
        public const string EDITOR_NODE_NAME_THIS_CARD = "ThisCard";
        #endregion
        #region 玩家
        [TranslateMsg("动作图编辑器中动作节点分类之玩家")]
        public const string EDITOR_NODE_CATEGORY_PLAYER = "Player";
        [TranslateMsg("动作图编辑器中动作节点名称之获取牌堆")]
        public const string EDITOR_NODE_NAME_GET_PILE = "GetPile";
        [TranslateMsg("动作图编辑器中动作节点名称之获取法术伤害")]
        public const string EDITOR_NODE_NAME_GET_SPELL_DAMAGE = "GetSpellDamage";
        [TranslateMsg("动作图编辑器中动作节点名称之获取目标")]
        public const string EDITOR_NODE_NAME_GET_TARGET = "GetTarget";
        #endregion
        #region 牌堆
        [TranslateMsg("动作图编辑器中动作节点分类之牌堆")]
        public const string EDITOR_NODE_CATEGORY_PILE = "Pile";
        [TranslateMsg("动作图编辑器中动作节点名称之玩家牌堆是否发生改变")]
        public const string EDITOR_NODE_NAME_IS_PLAYER_PILE_CHANGED = "IsPlayerPileChanged";
        #endregion
        #region 卡牌
        [TranslateMsg("动作图编辑器中动作节点分类之卡牌")]
        public const string EDITOR_NODE_CATEGORY_CARD = "Card";
        [TranslateMsg("动作图编辑器中动作节点名称之获取卡牌拥有者")]
        public const string EDITOR_NODE_NAME_GET_OWNER = "GetOwner";
        [TranslateMsg("动作图编辑器中动作节点名称之添加整数修改器")]
        public const string EDITOR_NODE_NAME_ADD_INT_MODIFIER = "AddIntModifier";
        [TranslateMsg("动作图编辑器中动作节点名称之移除修改器")]
        public const string EDITOR_NODE_NAME_REMOVE_MODIFIER = "RemoveModifier";
        [TranslateMsg("动作图编辑器中动作节点名称之获取属性")]
        public const string EDITOR_NODE_NAME_GET_PROPERTY = "GetProperty";
        [TranslateMsg("动作图编辑器中动作节点名称之设置属性")]
        public const string EDITOR_NODE_NAME_SET_PROPERTY = "SetProperty";
        [TranslateMsg("动作图编辑器中动作节点名称之获取唯一属性名")]
        public const string EDITOR_NODE_NAME_GET_UNIQUE_PROPERTY_NAME = "GetUniquePropertyName";
        [TranslateMsg("动作图编辑器中动作节点名称之造成伤害")]
        public const string EDITOR_NODE_NAME_DAMAGE = "Damage";
        #endregion
        #region 修改器
        [TranslateMsg("动作图编辑器中动作节点分类之修改器")]
        public const string EDITOR_NODE_CATEGORY_MODIFIER = "Modifier";
        [TranslateMsg("动作图编辑器中动作节点名称之设置整数修改器值")]
        public const string EDITOR_NODE_NAME_SET_INT_PROP_MODIFIER_VALUE = "SetIntPropModifierValue";
        #endregion
        #region 事件
        [TranslateMsg("动作图编辑器中动作节点分类之事件")]
        public const string EDITOR_NODE_CATEGORY_EVENT = "Event";
        [TranslateMsg("动作图编辑器中动作节点名称之获取变量")]
        public const string EDITOR_NODE_NAME_GET_VARIABLE = "GetVariable";
        [TranslateMsg("动作图编辑器中动作节点名称之设置变量")]
        public const string EDITOR_NODE_NAME_SET_VARIABLE = "SetVariable";
        #endregion
        #region 集合运算
        [TranslateMsg("动作图编辑器中动作节点分类之集合运算")]
        public const string EDITOR_NODE_CATEGORY_COLLECTION_OPERATION = "CollectionOperation";
        [TranslateMsg("动作图编辑器中动作节点名称之集合相减")]
        public const string EDITOR_NODE_NAME_EXCEPT = "Except";
        [TranslateMsg("动作图编辑器中动作节点名称之获取元素数量")]
        public const string EDITOR_NODE_NAME_GET_COUNT = "GetCount";
        #endregion
        #endregion
        #endregion
        #region 错误
        [TranslateMsg]
        public const string ERROR_CARD_WITH_ID_NOT_EXISTS = "不存在ID为{0}的卡。";
        [TranslateMsg]
        public const string ERROR_INVALID_IN_VISITOR_MODE = "游客无法使用此功能。";
        #endregion
    }
}
