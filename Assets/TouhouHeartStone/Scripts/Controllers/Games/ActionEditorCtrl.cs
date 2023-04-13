using System;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using MukioI18n;
using TouhouCardEngine;
using UnityEngine;
using UI;
using System.Linq;
using UnityEngine.XR;

namespace Controllers
{
    class ActionEditorCtrl : CardPoolEditor
    {
        #region 公有方法
        /// <summary>
        /// 打开一个自定义动作进行编辑
        /// </summary>
        public async Task display(long cardPoolId, GeneratedActionDefine actionDefine)
        {
            gameObject.SetActive(true);
            _cardPoolId = cardPoolId;
            _actionDefine = actionDefine;
            var types = game.editor.getEditorVarTypeNames();
            var constTypes = game.editor.getEditorConstTypeNames();
            _ui.initPropertyItemTypes(types, constTypes);
            _ui.setName(actionDefine.defineName);

            var categoryStrings = Array.ConvertAll(categories, c => game.langManager.getString(c));
            _ui.initCategories(categoryStrings);
            var categoryIndex = actionDefine.category == null ? 0 : categories.indexOf(actionDefine.category);
            _ui.setCategory(categoryIndex);

            await _actionNodeGraphCtrl.display(new ActionNodeGraphCtrl.DisplayInfo(
                        cardPoolId,
                        actionDefine,
                        null,
                        game.card.getOrCreateActionDefineActionNodeGraphInfo(cardPoolId, actionDefine.id)));
            updateProperties();
        }
        public override Task display(CardPoolDisplayInfo info)
        {
            if (info == null)
                return Task.CompletedTask;
            if (info.displayObj is GeneratedActionDefine actionDefine)
                return display(info.cardPoolId, actionDefine);
            return Task.CompletedTask;
        }
        public void markActionUnsaved()
        {
            hasUnsavedChanges = true;
        }
        public void clearActionUnsaved()
        {
            hasUnsavedChanges = false;
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponentInChildren<ActionEditorPanel>();
            }
            _ui.onChangeName += onChangeName;
            _ui.onChangeCategory += onChangeCategory;

            _ui.onAddInput += onAddInput;
            _ui.onChangeInputType += onChangeInputType;
            _ui.onChangeInputName += onChangeInputName;
            _ui.onChangeInputIsArray += onChangeInputIsArray;
            _ui.onChangeInputIsParams += onChangeInputIsParams;
            _ui.onDeleteInput += onDeleteInput;

            _ui.onAddConst += onAddConst;
            _ui.onChangeConstType += onChangeConstType;
            _ui.onChangeConstName += onChangeConstName;
            _ui.onChangeConstIsArray += onChangeConstIsArray;
            _ui.onChangeConstIsParams += onChangeConstIsParams;
            _ui.onDeleteConst += onDeleteConst;

            _ui.onAddOutput += onAddOutput;
            _ui.onChangeOutputType += onChangeOutputType;
            _ui.onChangeOutputName += onChangeOutputName;
            _ui.onChangeOutputIsArray += onChangeOutputIsArray;
            _ui.onChangeOutputIsParams += onChangeOutputIsParams;
            _ui.onDeleteOutput += onDeleteOutput;

            _ui.onClickDeleteActionButton += onClickDeleteActionButton;
            _ui.onClickSaveButton += onClickSaveButton;
            _ui.onClickCloseButton += onClickCloseButton;
        }
        #endregion
        #region 事件回调
        /// <summary>
        /// 当用户更改动作名
        /// </summary>
        /// <param name="name"></param>
        void onChangeName(string name)
        {
            _actionDefine.defineName = name;
            _ui.setName(name);
            markActionUnsaved();
        }
        /// <summary>
        /// 当用户更改动作分类
        /// </summary>
        /// <param name="index"></param>
        void onChangeCategory(int index)
        {
            _actionDefine.setCategory(categories[index]);
            _ui.setCategory(index);
            markActionUnsaved();
        }
        private async void onClickDeleteActionButton()
        {
            scene.editor.hasUnsavedChanges = true;
            //删除当前动作
            game.card.removeActionDefine(_cardPoolId, _actionDefine);
            //返回上个界面
            await scene.editor.closeEditorPanel();
            clearActionUnsaved();
        }
        private void onClickSaveButton()
        {
            saveAction();
        }
        private void onClickCloseButton()
        {
            if (hasUnsavedChanges)
            {
                string title = game.lang._(StringTable.WARNING);

                string desc = game.lang._(ACTION_WARNING_UNSAVED_CHANGES);
                string[] optNames = new string[2]
                {
                    game.lang._(ACTION_WARNING_OPTION_SAVE),
                    game.lang._(ACTION_WARNING_OPTION_CANCEL)
                };
                game.ui.dialog.ShowOptions(title, desc, optNames, (index) =>
                {
                    if (index == 0)
                    {
                        saveAction();
                        exit();
                    }
                });
            }
            else
            {
                exit();
            }
        }
        #region 输入变量
        void onAddInput()
        {
            string defaultName = game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_VAR_NAME_DEFAULT);
            int index = _actionDefine.inputDefines.Count() + 1;
            string name = game.lang._(EDITOR_ACTION_DEFINE_PORT_NAME_TEMPLATE, defaultName, index);
            var valueDefine = PortDefine.Value(typeof(int), name);
            _actionDefine.addInput(valueDefine);
            updateInputs();
            markActionUnsaved();
        }
        void onChangeInputType(int inputIndex, int categoryIndex)
        {
            var targetType = game.editor.getEditorVarTypeAt(categoryIndex);
            var curType = _actionDefine.inputDefines.ElementAt(inputIndex).type;
            if (curType.IsArray)
                targetType = targetType.MakeArrayType();
            _actionDefine.inputDefines.ElementAt(inputIndex).type = targetType;
            updateInputs();
            markActionUnsaved();
        }
        void onChangeInputName(int inputIndex, string name)
        {
            _actionDefine.inputDefines.ElementAt(inputIndex).name = name;
            updateInputs();
            markActionUnsaved();
        }
        void onChangeInputIsArray(int inputIndex, bool isArray)
        {
            var targetType = _actionDefine.inputDefines.ElementAt(inputIndex).type;
            if (isArray && !targetType.IsArray)
                targetType = targetType.MakeArrayType();
            else if (!isArray && targetType.IsArray)
                targetType = targetType.GetElementType();
            _actionDefine.inputDefines.ElementAt(inputIndex).type = targetType;
            updateInputs();
            markActionUnsaved();
        }
        void onChangeInputIsParams(int inputIndex, bool isParams)
        {
            _actionDefine.isParams = isParams;
            updateInputs();
            markActionUnsaved();
        }
        void onDeleteInput(int inputIndex)
        {
            var portDefine = _actionDefine.inputDefines.ElementAt(inputIndex);
            _actionDefine.removeInput(portDefine);
            updateInputs();
            markActionUnsaved();
        }
        #endregion

        #region 输入常量
        void onAddConst()
        {
            string defaultName = game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_CONST_NAME_DEFAULT);
            int index = _actionDefine.constDefines.Count() + 1;
            string name = game.lang._(EDITOR_ACTION_DEFINE_PORT_NAME_TEMPLATE, defaultName, index);
            var valueDefine = PortDefine.Const(typeof(int), name);
            _actionDefine.addConst(valueDefine);
            updateConsts();
            markActionUnsaved();
        }
        void onChangeConstType(int constIndex, int categoryIndex)
        {
            var targetType = game.editor.getEditorVarTypeAt(categoryIndex);
            var curType = _actionDefine.constDefines.ElementAt(constIndex).type;
            if (curType.IsArray)
                targetType = targetType.MakeArrayType();
            _actionDefine.constDefines.ElementAt(constIndex).type = targetType;
            updateConsts();
            markActionUnsaved();
        }
        void onChangeConstName(int constIndex, string name)
        {
            _actionDefine.constDefines.ElementAt(constIndex).name = name;
            updateConsts();
            markActionUnsaved();
        }
        void onChangeConstIsArray(int constIndex, bool isArray)
        {
            var targetType = _actionDefine.constDefines.ElementAt(constIndex).type;
            if (isArray && !targetType.IsArray)
                targetType = targetType.MakeArrayType();
            else if (!isArray && targetType.IsArray)
                targetType = targetType.GetElementType();
            _actionDefine.constDefines.ElementAt(constIndex).type = targetType;
            updateConsts();
            markActionUnsaved();
        }
        void onChangeConstIsParams(int constIndex, bool isParams)
        {
            _actionDefine.isParams = isParams;
            updateConsts();
            markActionUnsaved();
        }
        void onDeleteConst(int constIndex)
        {
            var constDefine = _actionDefine.constDefines.ElementAt(constIndex);
            _actionDefine.removeConst(constDefine);
            updateConsts();
            markActionUnsaved();
        }
        #endregion

        #region 输出变量
        void onAddOutput()
        {
            string defaultName = game.langManager.getString(EDITOR_ACTION_DEFINE_OUTPUT_NAME_DEFAULT);
            int index = _actionDefine.outputDefines.Count() + 1;
            string name = game.lang._(EDITOR_ACTION_DEFINE_PORT_NAME_TEMPLATE, defaultName, index);
            var valueDefine = PortDefine.Value(typeof(int),name);
            _actionDefine.addOutput(valueDefine);
            updateOutputs();
            markActionUnsaved();
        }
        void onChangeOutputType(int outputIndex, int categoryIndex)
        {
            var targetType = game.editor.getEditorVarTypeAt(categoryIndex);
            var curType = _actionDefine.outputDefines.ElementAt(outputIndex).type;
            if (curType.IsArray)
                targetType = targetType.MakeArrayType();
            _actionDefine.outputDefines.ElementAt(outputIndex).type = targetType;
            updateOutputs();
            markActionUnsaved();
        }
        void onChangeOutputName(int outputIndex, string name)
        {
            _actionDefine.outputDefines.ElementAt(outputIndex).name = name;
            updateOutputs();
            markActionUnsaved();
        }
        void onChangeOutputIsArray(int outputIndex, bool isArray)
        {
            var targetType = _actionDefine.outputDefines.ElementAt(outputIndex).type;
            if (isArray && !targetType.IsArray)
                targetType = targetType.MakeArrayType();
            else if (!isArray && targetType.IsArray)
                targetType = targetType.GetElementType();
            _actionDefine.outputDefines.ElementAt(outputIndex).type = targetType;
            updateOutputs();
            markActionUnsaved();
        }
        void onChangeOutputIsParams(int outputIndex, bool isParams)
        {
            _actionDefine.isParams = isParams;
            updateOutputs();
            markActionUnsaved();
        }
        void onDeleteOutput(int outputIndex)
        {
            var portDefine = _actionDefine.outputDefines.ElementAt(outputIndex);
            _actionDefine.removeOutput(portDefine);
            updateOutputs();
            markActionUnsaved();
        }
        #endregion

        #endregion
        protected override void setUIEffects(string[] names, int activeIndex)
        {
        }
        /// <summary>
        /// 返回上一个界面
        /// </summary>
        private async void exit()
        {
            await scene.editor.closeEditorPanel();
            clearActionUnsaved();
        }
        private void saveAction()
        {
            scene.editor.hasUnsavedChanges = true;
            clearActionUnsaved();
        }

        private void updateProperties()
        {
            updateInputs();
            updateConsts();
            updateOutputs();
        }
        private void updateInputs()
        {
            var inputs = _actionDefine.inputDefines;
            ActionPortDefineViewData[] viewDatas = inputs.Select((input, i) =>
            {
                int typeIndex = game.editor.getEditorVarTypeNameIndex(input.type.Name);
                bool isArray = input.type.IsArray;
                return new ActionPortDefineViewData()
                {
                    name = input.name,
                    isParams = _actionDefine.isParams,
                    isArray = isArray,
                    typeIndex = typeIndex
                };
            }).ToArray();

            _ui.updateInputList(viewDatas);
            _actionNodeGraphCtrl.updateActionDefineEntryAndReturnNodes();
        }
        private void updateConsts()
        {
            var consts = _actionDefine.constDefines;
            ActionPortDefineViewData[] viewDatas = consts.Select((cst, i) =>
            {
                int typeIndex = game.editor.getEditorVarTypeNameIndex(cst.type.Name);
                bool isArray = cst.type.IsArray;
                return new ActionPortDefineViewData()
                {
                    name = cst.name,
                    isParams = _actionDefine.isParams,
                    isArray = isArray,
                    typeIndex = typeIndex
                };
            }).ToArray();

            _ui.updateConstList(viewDatas);
            _actionNodeGraphCtrl.updateActionDefineEntryAndReturnNodes();
        }
        private void updateOutputs()
        {
            var outputs = _actionDefine.outputDefines;
            ActionPortDefineViewData[] viewDatas = outputs.Select((output, i) =>
            {
                int typeIndex = game.editor.getEditorVarTypeNameIndex(output.type.Name);
                bool isArray = output.type.IsArray;
                return new ActionPortDefineViewData()
                {
                    name = output.name,
                    isParams = _actionDefine.isParams,
                    isArray = isArray,
                    typeIndex = typeIndex
                };
            }).ToArray();

            _ui.updateOutputList(viewDatas);
            _actionNodeGraphCtrl.updateActionDefineEntryAndReturnNodes();
        }

        #endregion


        ActionEditorPanel _ui;
        [SerializeField]
        ActionNodeGraphCtrl _actionNodeGraphCtrl;

        [TranslateMsg("自定义动作的端口名称模板（{0}：端口类型（输入/常量/输出），{1}：序号）")]
        public const string EDITOR_ACTION_DEFINE_PORT_NAME_TEMPLATE = "{0}{1}";
        [TranslateMsg("自定义动作的默认输出值名称前缀")]
        public const string EDITOR_ACTION_DEFINE_OUTPUT_NAME_DEFAULT = "输出值";
        [TranslateMsg("尝试退出一张卡牌的编辑页面，但没有保存情况下的警告")]
        public const string ACTION_WARNING_UNSAVED_CHANGES = "是否将更改保存到该自定义动作中？";
        [TranslateMsg("尝试退出一张卡牌的编辑页面，但没有保存情况下的按钮名称")]
        public const string ACTION_WARNING_OPTION_SAVE = "保存";
        [TranslateMsg("尝试退出一张卡牌的编辑页面，但没有保存情况下的按钮名称")]
        public const string ACTION_WARNING_OPTION_CANCEL = "取消";
        [TranslateMsg("尝试添加重复名称的自定义属性的错误提示")]
        public const string ACTION_ERROR_NAME_TAKEN = "该动作名已被使用！";
        [TranslateMsg("尝试添加空名称的自定义属性的错误提示")]
        public const string ACTION_ERROR_NAME_EMPTY = "请输入动作名！";
        [TranslateMsg("没有分类的动作的分类")]
        public const string ACTION_CATEGORY_NAME_NONE = "无";
        public bool hasUnsavedChanges { get; private set; }

        long _cardPoolId;
        /// <summary>
        /// 正在编辑的动作定义。
        /// </summary>
        GeneratedActionDefine _actionDefine;


        public readonly string[] categories = new string[]
        {
            ACTION_CATEGORY_NAME_NONE,
            StringTable.EDITOR_NODE_CATEGORY_GAME,
            StringTable.EDITOR_NODE_CATEGORY_PLAYER,
            StringTable.EDITOR_NODE_CATEGORY_PILE,
            StringTable.EDITOR_NODE_CATEGORY_CARD,
            StringTable.EDITOR_NODE_CATEGORY_BUFF,
            StringTable.EDITOR_NODE_CATEGORY_MODIFIER,
            StringTable.EDITOR_NODE_CATEGORY_EVENT,
            StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION
        };
    }
}
