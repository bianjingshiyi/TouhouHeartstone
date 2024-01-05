using System;
using System.Linq;
using System.Threading.Tasks;
using Game;
using MukioI18n;
using TouhouCardEngine;
using UI;
using UnityEngine;

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
            _ui.reset();
            _curCardPoolId = cardPoolId;
            _actionDefine = actionDefine;

            _ui.setName(actionDefine.editorName);

            _ui.setCategory(actionDefine.category);
            _ui.setDefineType(getDefineTypeName(actionDefine.type));

            await _actionNodeGraphCtrl.display(new ActionNodeGraphCtrl.DisplayInfo(cardPoolId, actionDefine));
            updateProperties();
            _graphEditor.display(_curCardPoolId);
        }
        public override async Task display(CardPoolDisplayPage info)
        {
            if (info == null)
                return;
            displayInfo = info;
            if (info?.displayObj?.actionDefine is GeneratedActionDefine actionDefine)
            {
                await display(info.cardPoolId, actionDefine);
                if (info.unsaved)
                    markUnsaved();
                else
                    clearUnsaved();
            }
        }
        public override void markUnsaved()
        {
            base.markUnsaved();
            _ui.setTitleText(scene.editor.getTitleText(hasUnsavedChanges, game.lang._(ACTION_EDITOR_TITLE)));
        }
        public override void clearUnsaved()
        {
            base.clearUnsaved();
            _ui.setTitleText(scene.editor.getTitleText(hasUnsavedChanges, game.lang._(ACTION_EDITOR_TITLE)));
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
            _ui.onChangeCategory += onChangeCategory;
            _ui.onCreatePortDefine += onCreatePortDefine;

            _ui.onAddInput += onAddInput;
            _ui.onDeleteInput += onDeleteInput;

            _ui.onAddOutput += onAddOutput;
            _ui.onDeleteOutput += onDeleteOutput;

            _ui.onClickDeleteButton += onClickDeleteActionButton;
            _ui.onClickSaveButton += onClickSaveButton;
            _ui.onClickCloseButton += onClickCloseButton;

            _actionNodeGraphCtrl.OnGraphEdited += onGraphEdited;
        }
        #endregion
        #region 事件回调
        private void onGraphEdited()
        {
            markUnsaved();
        }
        /// <summary>
        /// 当用户更改动作分类
        /// </summary>
        /// <param name="index"></param>
        private void onChangeCategory(string category)
        {
            _actionDefine.setCategory(category);
            _ui.setCategory(category);
            markUnsaved();
        }
        private void onCreatePortDefine(ActionPortDefineItem item)
        {
            string name = item.propertyName;
            string displayName = name;
            if (string.IsNullOrEmpty(name))
            {
                string errorTip = game.lang._(ACTION_ERROR_NEW_PORT_NAME_EMPTY);
                _ui.showCreatePortDefineDialogErrorTip(errorTip);
                return;
            }
            // 检查重名端口。
            bool duplicateName = false;
            switch (item.portType)
            {
                case ActionPortDefineType.Input:
                    duplicateName = _actionDefine.getInputDefine(name) != null;
                    break;
                case ActionPortDefineType.Output:
                    duplicateName = _actionDefine.getOutputDefine(name) != null;
                    break;
            }
            if (duplicateName)
            {
                string errorTip = game.lang._(ACTION_ERROR_NEW_PORT_NAME_TAKEN);
                _ui.showCreatePortDefineDialogErrorTip(errorTip);
                return;
            }

            Type type = item.type;
            if (item.isArray)
            {
                type = type.MakeArrayType();
            }
            var valueDefine = PortDefine.Value(type, name, displayName, item.isParams);
            switch (item.portType)
            {
                case ActionPortDefineType.Input:
                    _actionDefine.addInput(valueDefine);
                    var paramDefine = _actionDefine.inputDefines.FirstOrDefault(d => d.isParams);
                    if (paramDefine != null)
                    {
                        _actionDefine.removeInput(paramDefine);
                        _actionDefine.addInput(paramDefine);
                    }
                    updateInputs();
                    _actionNodeGraphCtrl.onAddActionDefineInputPort();
                    break;
                case ActionPortDefineType.Output:
                    _actionDefine.addOutput(valueDefine);
                    updateOutputs();
                    break;
            }
            _ui.hideCreatePortDefineDialog();
            markUnsaved();
        }
        private void onClickDeleteActionButton()
        {
            //询问玩家
            game.ui.dialog.ShowQuestion(
               game.langManager.getString(StringTable.WARNING),
               game.langManager.getString(StringTable.EDITOR_WARNING_DELETE_ACTION_DFINE),
               async () =>
               {
                   //删除动作定义
                   game.card.removeActionDefine(_curCardPoolId, _actionDefine.defineId);
                   scene.editor.updateAllEvents();
                   scene.editor.markCardPoolUnsaved(_curCardPoolId);
                   //返回上个界面
                   await exit(true);
               });
        }
        private void onClickSaveButton()
        {
            saveAction();
        }
        private async void onClickCloseButton()
        {
            if (hasUnsavedChanges)
            {
                string title = game.lang._(StringTable.WARNING);

                string desc = game.lang._(ACTION_WARNING_UNSAVED_CHANGES);
                string[] optNames = new string[]
                {
                    game.lang._(ACTION_WARNING_OPTION_SAVE),
                    game.lang._(ACTION_WARNING_OPTION_DONT_SAVE),
                    game.lang._(ACTION_WARNING_OPTION_CANCEL)
                };
                game.ui.dialog.ShowOptions(title, desc, optNames, async (index) =>
                {
                    if (index == 0)
                    {
                        saveAction();
                        await exit();
                    }
                    else if (index == 1)
                    {
                        scene.editor.removeDisplayPageOfObject(_actionDefine);
                        await exit();
                    }
                });
            }
            else
            {
                await exit();
            }
        }
        private (Type type, string name)[] getEditorVarTypes()
        {
            var typeNames = game.editor.getEditorVarTypeNames();
            (Type type, string name)[] types = new (Type type, string name)[typeNames.Length];
            for (int i = 0; i < types.Length; i++)
            {
                var name = typeNames[i];
                types[i] = (game.editor.getEditorVarTypeAt(i), name);
            }
            return types;
        }
        #region 输入变量
        private void onAddInput()
        {
            string title = game.lang._(ACTION_NEW_INPUT_DIALOG_TITLE);
            _ui.setActionPortTypes(getEditorVarTypes());
            _ui.displayCreatePortDefineDialog(ActionPortDefineType.Input, title, !_actionDefine.inputDefines.Any(d => d.isParams));
        }
        private void onDeleteInput(int inputIndex)
        {
            inputIndex++; //去掉了UI的动作端口，索引+1。
            var portDefine = _actionDefine.inputDefines.ElementAt(inputIndex);
            _actionNodeGraphCtrl.onDeleteActionDefineInputPort(portDefine.name);
            _actionDefine.removeInput(portDefine);
            updateInputs();
            markUnsaved();
        }
        #endregion

        #region 输出变量
        private void onAddOutput()
        {
            string title = game.lang._(ACTION_NEW_OUTPUT_DIALOG_TITLE);
            _ui.setActionPortTypes(getEditorVarTypes());
            _ui.displayCreatePortDefineDialog(ActionPortDefineType.Output, title);
        }
        private void onDeleteOutput(int outputIndex)
        {
            outputIndex++; //去掉了UI的输出端口，索引+1。
            var portDefine = _actionDefine.outputDefines.ElementAt(outputIndex);
            _actionNodeGraphCtrl.onDeleteActionDefineOutputPort(portDefine.name);
            _actionDefine.removeOutput(portDefine);
            updateOutputs();
            markUnsaved();
        }
        #endregion

        #endregion
        protected override void setUIEffects(string[] names, int activeIndex)
        {
        }
        /// <summary>
        /// 返回上一个界面
        /// </summary>
        private async Task exit(bool forceSave = false)
        {
            clearUnsaved();
            await scene.editor.closeEditor(forceSave);
        }
        private void saveAction()
        {
            var cardPoolRef = getCurrentCardPoolRef();
            if (game.card.getActionDefine(cardPoolRef, _actionDefine.defineId) is GeneratedActionDefine gen)
            {
                game.card.removeActionDefine(gen);
            }
            game.card.addActionDefine(_curCardPoolId, _actionDefine);
            scene.editor.updateAllEvents();

            scene.editor.markCardPoolUnsaved(_curCardPoolId);
            clearUnsaved();
        }

        private void updateProperties()
        {
            updateInputs();
            updateOutputs();
        }
        private void updateInputs()
        {
            var inputs = _actionDefine.inputDefines.Where(i => i.name != ActionNode.enterControlName);
            ActionPortDefineViewData[] viewDatas = inputs.Select((input, i) =>
            {
                i++; //UI隐藏了动作端口，索引+1。
                Type type = input.type;
                bool isParams = input.isParams;
                bool isArray = isTypeArray(type);
                if (isArray)
                {
                    type = type.GetElementType();
                }
                int typeIndex = game.editor.getEditorVarTypeNameIndex(type.Name);
                var typeName = game.editor.getEditorVarTypeNames()[typeIndex];
                var arrayStr = isArray ? game.lang._(ACTION_VARIABLE_IS_ARRAY) : string.Empty;
                var paramsStr = isParams ? game.lang._(ACTION_VARIABLE_IS_PARAMS) : string.Empty;
                var typeNameStr = game.lang._(ACTION_VARIABLE_TYPE_NAME_TEMPLATE, typeName, arrayStr, paramsStr);
                return new ActionPortDefineViewData()
                {
                    name = input.getDisplayName(),
                    typeName = typeNameStr
                };
            }).ToArray();

            _ui.updateInputList(viewDatas);
            _actionNodeGraphCtrl.updateActionDefineEntryAndReturnNodes();
        }
        private void updateOutputs()
        {
            var outputs = _actionDefine.outputDefines.Where(i => i.name != ActionNode.exitControlName);
            ActionPortDefineViewData[] viewDatas = outputs.Select((output, i) =>
            {
                Type type = output.type;
                bool isArray = isTypeArray(type);
                if (isArray)
                {
                    type = type.GetElementType();
                }
                int typeIndex = game.editor.getEditorVarTypeNameIndex(type.Name);
                var typeName = game.editor.getEditorVarTypeNames()[typeIndex];
                var arrayStr = isArray ? game.lang._(ACTION_VARIABLE_IS_ARRAY) : string.Empty;
                var typeNameStr = game.lang._(ACTION_VARIABLE_TYPE_NAME_TEMPLATE, typeName, arrayStr, string.Empty);
                return new ActionPortDefineViewData()
                {
                    name = output.getDisplayName(),
                    typeName = typeNameStr
                };
            }).ToArray();

            _ui.updateOutputList(viewDatas);
            _actionNodeGraphCtrl.updateActionDefineEntryAndReturnNodes();
        }
        private bool isTypeArray(Type type)
        {
            return type.IsArray && type != typeof(Array);
        }
        private Type getTargetType(Type originType, bool isArray)
        {
            var targetType = originType;
            bool currentIsArray = isTypeArray(targetType);
            if (isArray && !currentIsArray)
                targetType = targetType.MakeArrayType();
            else if (!isArray && currentIsArray)
                targetType = targetType.GetElementType();
            return targetType;
        }
        private bool isLastInput(int index)
        {
            return index == _actionDefine.inputDefines.Count() - 1;
        }
        private string getDefineTypeName(NodeDefineType type)
        {
            var defineTypeName = ACTION_DEFINE_TYPE_ACTION;
            switch (type)
            {
                case NodeDefineType.Function:
                    defineTypeName = ACTION_DEFINE_TYPE_FUNCTION;
                    break;
                case NodeDefineType.Event:
                    defineTypeName = ACTION_DEFINE_TYPE_EVENT;
                    break;
            }
            return game.lang._(defineTypeName);
        }
        #endregion


        ActionEditorPanel _ui;
        [SerializeField]
        ActionNodeGraphCtrl _actionNodeGraphCtrl;


        [TranslateMsg("动作编辑器/标题")]
        public const string ACTION_EDITOR_TITLE = "动作编辑器";
        [TranslateMsg("动作编辑器/添加变量对话框/标题")]
        public const string ACTION_NEW_INPUT_DIALOG_TITLE = "添加输入变量";
        [TranslateMsg("动作编辑器/添加变量对话框/标题")]
        public const string ACTION_NEW_CONST_DIALOG_TITLE = "添加输入常量";
        [TranslateMsg("动作编辑器/添加变量对话框/标题")]
        public const string ACTION_NEW_OUTPUT_DIALOG_TITLE = "添加输出变量";
        [TranslateMsg("动作编辑器/添加变量对话框/错误提示")]
        public const string ACTION_ERROR_NEW_PORT_NAME_TAKEN = "该名称已被使用！";
        [TranslateMsg("动作编辑器/添加变量对话框/错误提示")]
        public const string ACTION_ERROR_NEW_PORT_NAME_EMPTY = "请输入名称！";
        [TranslateMsg("动作编辑器/变量/数组变量的后缀")]
        public const string ACTION_VARIABLE_IS_ARRAY = "[数组]";
        [TranslateMsg("动作编辑器/变量/输入变量的可变长度后缀")]
        public const string ACTION_VARIABLE_IS_PARAMS = "[可变长度]";
        [TranslateMsg("动作编辑器/变量/变量的类型名称模板（{0}：类型名称；{1}：如果是数组，则为数组后缀，否则为空；{2}：如果可变长度，则为可变长度后缀，否则为空）")]
        public const string ACTION_VARIABLE_TYPE_NAME_TEMPLATE = "{0}{1}{2}";


        [TranslateMsg("自定义动作的端口名称模板（{0}：端口类型（输入/常量/输出），{1}：序号）")]
        public const string EDITOR_ACTION_DEFINE_PORT_NAME_TEMPLATE = "{0}{1}";
        [TranslateMsg("自定义动作的默认输出值名称前缀")]
        public const string EDITOR_ACTION_DEFINE_OUTPUT_NAME_DEFAULT = "输出值";
        [TranslateMsg("尝试退出自定义动作的编辑页面，但没有保存情况下的警告")]
        public const string ACTION_WARNING_UNSAVED_CHANGES = "是否将更改保存到该自定义动作中？";
        [TranslateMsg("尝试退出自定义动作的编辑页面，但没有保存情况下的按钮名称")]
        public const string ACTION_WARNING_OPTION_SAVE = "保存";
        [TranslateMsg("尝试退出自定义动作的编辑页面，但没有保存情况下的按钮名称")]
        public const string ACTION_WARNING_OPTION_DONT_SAVE = "不保存";
        [TranslateMsg("尝试退出自定义动作的编辑页面，但没有保存情况下的按钮名称")]
        public const string ACTION_WARNING_OPTION_CANCEL = "取消";
        [TranslateMsg("没有分类的动作的分类")]
        public const string ACTION_CATEGORY_NAME_NONE = "无";
        [TranslateMsg("节点动作类型/动作")]
        public const string ACTION_DEFINE_TYPE_ACTION = "动作";
        [TranslateMsg("节点动作类型/函数")]
        public const string ACTION_DEFINE_TYPE_FUNCTION = "函数";
        [TranslateMsg("节点动作类型/事件")]
        public const string ACTION_DEFINE_TYPE_EVENT = "事件";
        /// <summary>
        /// 正在编辑的动作定义。
        /// </summary>
        GeneratedActionDefine _actionDefine;
    }
}