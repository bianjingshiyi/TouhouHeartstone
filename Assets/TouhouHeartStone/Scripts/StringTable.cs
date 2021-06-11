using MukioI18n;

namespace Game
{
    class StringTable
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
        #region 编辑器
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
        #endregion
    }
}
