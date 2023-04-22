using System;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using MukioI18n;
using TouhouCardEngine;
using UnityEngine;
using UI;
using System.Linq;

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
            _ui.onDeleteConst += onDeleteConst;

            _ui.onAddOutput += onAddOutput;
            _ui.onChangeOutputType += onChangeOutputType;
            _ui.onChangeOutputName += onChangeOutputName;
            _ui.onChangeOutputIsArray += onChangeOutputIsArray;
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
        private void onChangeName(string value)
        {
            if (!_actionNodeGraphCtrl.isActionDefineNameExist(value))
            {
                _actionDefine.defineName = value;
                _ui.setName(value);
                markActionUnsaved();
            }
            else
            {
                //重名
                game.ui.dialog.ShowConfirm(
                    game.langManager.getString(StringTable.ERROR),
                    game.langManager.getString(StringTable.EDITOR_ERROR_ACTION_DEFINE_NAME_ALREADY_EXIST));
                _actionDefine.defineName = game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_NAME_DEFAULT);
                _ui.setName(_actionDefine.defineName);
            }
        }
        /// <summary>
        /// 当用户更改动作分类
        /// </summary>
        /// <param name="index"></param>
        private void onChangeCategory(int index)
        {
            _actionDefine.setCategory(index == 0 ? null : categories[index]);
            _ui.setCategory(index);
            markActionUnsaved();
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
                   game.card.removeActionDefine(_cardPoolId, _actionDefine);
                   //返回上个界面
                   await scene.editor.closeEditorPanel();
                   scene.editor.hasUnsavedChanges = true;
                   clearActionUnsaved();
               });
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
        private void onAddInput()
        {
            string defaultName = game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_VAR_NAME_DEFAULT);
            int id = 0;
            string name = $"input{id}";
            while (_actionDefine.inputDefines.Any(d => d.name == name))
            {
                id++;
                name = $"input{id}";
            }
            string displayName = game.lang._(EDITOR_ACTION_DEFINE_PORT_NAME_TEMPLATE, defaultName, id + 1);
            var valueDefine = PortDefine.Value(typeof(int), name, displayName);
            _actionDefine.addInput(valueDefine);
            updateInputs();
            _actionNodeGraphCtrl.onAddActionDefineInputPort();
            markActionUnsaved();
        }
        private void onChangeInputType(int inputIndex, int typeIndex)
        {
            inputIndex++; //去掉了UI的动作端口，索引+1。
            var newType = game.editor.getEditorVarTypeAt(typeIndex);
            var port = _actionDefine.inputDefines.ElementAt(inputIndex);
            var curType = port.type;
            bool isArray = isTypeArray(curType);
            // 变长参数的类型是数组。
            isArray |= isLastInput(inputIndex) && _actionDefine.isParams;
            port.type = getTargetType(newType, isArray);
            updateInputs();
            _actionNodeGraphCtrl.onChangeActionDefineInputType(port.name);
            markActionUnsaved();
        }
        private void onChangeInputName(int inputIndex, string name)
        {
            inputIndex++; //去掉了UI的动作端口，索引+1。
            _actionDefine.inputDefines.ElementAt(inputIndex).displayName = name;
            updateInputs();
            markActionUnsaved();
        }
        private void onChangeInputIsArray(int inputIndex, bool isArray)
        {
            inputIndex++; //去掉了UI的动作端口，索引+1。
            var port = _actionDefine.inputDefines.ElementAt(inputIndex);
            // 变长参数的类型是数组。
            isArray |= isLastInput(inputIndex) && _actionDefine.isParams;
            port.type = getTargetType(port.type, isArray);
            updateInputs();
            _actionNodeGraphCtrl.onChangeActionDefineInputType(port.name);
            markActionUnsaved();
        }
        private void onChangeInputIsParams(int inputIndex, bool isParams)
        {
            _actionDefine.isParams = isParams;

            int lastIndex = _actionDefine.inputDefines.Count() - 1;
            var port = _actionDefine.inputDefines.ElementAt(lastIndex);
            var curType = port.type;
            bool isArray = isTypeArray(curType) || isParams;
            port.type = getTargetType(port.type, isArray);

            updateInputs();
            _actionNodeGraphCtrl.onChangeActionDefineInputType(port.name);
            markActionUnsaved();
        }
        private void onDeleteInput(int inputIndex)
        {
            inputIndex++; //去掉了UI的动作端口，索引+1。
            var portDefine = _actionDefine.inputDefines.ElementAt(inputIndex);
            _actionNodeGraphCtrl.onDeleteActionDefineInputPort(portDefine.name);
            _actionDefine.removeInput(portDefine);
            updateInputs();
            markActionUnsaved();
        }
        #endregion

        #region 输入常量
        private void onAddConst()
        {
            string defaultName = game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_CONST_NAME_DEFAULT);
            int id = 0;
            string name = $"const{id}";
            while (_actionDefine.constDefines.Any(d => d.name == name))
            {
                id++;
                name = $"const{id}";
            }
            string displayName = game.lang._(EDITOR_ACTION_DEFINE_PORT_NAME_TEMPLATE, defaultName, id + 1);
            var valueDefine = PortDefine.Const(typeof(int), name, displayName);
            _actionDefine.addConst(valueDefine);
            updateConsts();
            markActionUnsaved();
        }
        private void onChangeConstType(int constIndex, int typeIndex)
        {
            var newType = game.editor.getEditorConstTypeAt(typeIndex);
            var port = _actionDefine.constDefines.ElementAt(constIndex);
            var curType = port.type;
            bool isArray = isTypeArray(curType);
            port.type = getTargetType(newType, isArray);

            updateConsts();
            _actionNodeGraphCtrl.onChangeActionDefineConstType(port.name);
            markActionUnsaved();
        }
        private void onChangeConstName(int constIndex, string name)
        {
            _actionDefine.constDefines.ElementAt(constIndex).displayName = name;
            updateConsts();
            markActionUnsaved();
        }
        private void onChangeConstIsArray(int constIndex, bool isArray)
        {
            var port = _actionDefine.constDefines.ElementAt(constIndex);
            port.type = getTargetType(port.type, isArray);
            updateConsts();
            _actionNodeGraphCtrl.onChangeActionDefineConstType(port.name);
            markActionUnsaved();
        }
        private void onDeleteConst(int constIndex)
        {
            var constDefine = _actionDefine.constDefines.ElementAt(constIndex);
            _actionNodeGraphCtrl.onDeleteActionDefineConstPort(constDefine.name);
            _actionDefine.removeConst(constDefine);
            updateConsts();
            markActionUnsaved();
        }
        #endregion

        #region 输出变量
        private void onAddOutput()
        {
            string defaultName = game.langManager.getString(EDITOR_ACTION_DEFINE_OUTPUT_NAME_DEFAULT);
            int id = 0;
            string name = $"output{id}";
            while (_actionDefine.outputDefines.Any(d => d.name == name))
            {
                id++;
                name = $"output{id}";
            }
            string displayName = game.lang._(EDITOR_ACTION_DEFINE_PORT_NAME_TEMPLATE, defaultName, id + 1);
            var valueDefine = PortDefine.Value(typeof(int), name, displayName);
            _actionDefine.addOutput(valueDefine);
            updateOutputs();
            markActionUnsaved();
        }
        private void onChangeOutputType(int outputIndex, int typeIndex)
        {
            var newType = game.editor.getEditorVarTypeAt(typeIndex);
            var port = _actionDefine.outputDefines.ElementAt(outputIndex);
            var curType = port.type;
            bool isArray = isTypeArray(curType);
            port.type = getTargetType(newType, isArray);

            updateOutputs();
            _actionNodeGraphCtrl.onChangeActionDefineOutputType(port.name);
            markActionUnsaved();
        }
        private void onChangeOutputName(int outputIndex, string name)
        {
            _actionDefine.outputDefines.ElementAt(outputIndex).displayName = name;
            updateOutputs();
            markActionUnsaved();
        }
        private void onChangeOutputIsArray(int outputIndex, bool isArray)
        {
            var port = _actionDefine.outputDefines.ElementAt(outputIndex);
            port.type = getTargetType(port.type, isArray);
            updateOutputs();
            _actionNodeGraphCtrl.onChangeActionDefineOutputType(port.name);
            markActionUnsaved();
        }
        private void onDeleteOutput(int outputIndex)
        {
            var portDefine = _actionDefine.outputDefines.ElementAt(outputIndex);
            _actionNodeGraphCtrl.onDeleteActionDefineOutputPort(portDefine.name);
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
            var inputs = _actionDefine.inputDefines.Where(i => i.name != ActionNode.enterControlName);
            ActionPortDefineViewData[] viewDatas = inputs.Select((input, i) =>
            {
                i++; //UI隐藏了动作端口，索引+1。
                Type type = input.type;
                bool isArray = isTypeArray(type);
                if (isArray)
                {
                    type = type.GetElementType();
                }
                int typeIndex = game.editor.getEditorVarTypeNameIndex(type.Name);
                return new ActionPortDefineViewData()
                {
                    name = input.getDisplayName(),
                    isParams = _actionDefine.isParams,
                    showParams = isLastInput(i),
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
                Type type = cst.type;
                bool isArray = isTypeArray(type);
                if (isArray)
                {
                    type = type.GetElementType();
                }
                int typeIndex = game.editor.getEditorConstTypeNameIndex(type.Name);
                return new ActionPortDefineViewData()
                {
                    name = cst.getDisplayName(),
                    showParams = false,
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
                Type type = output.type;
                bool isArray = isTypeArray(type);
                if (isArray)
                {
                    type = type.GetElementType();
                }
                int typeIndex = game.editor.getEditorVarTypeNameIndex(type.Name);
                return new ActionPortDefineViewData()
                {
                    name = output.getDisplayName(),
                    showParams = false,
                    isArray = isArray,
                    typeIndex = typeIndex
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
        #endregion


        ActionEditorPanel _ui;
        [SerializeField]
        ActionNodeGraphCtrl _actionNodeGraphCtrl;

        [TranslateMsg("自定义动作的端口名称模板（{0}：端口类型（输入/常量/输出），{1}：序号）")]
        public const string EDITOR_ACTION_DEFINE_PORT_NAME_TEMPLATE = "{0}{1}";
        [TranslateMsg("自定义动作的默认输出值名称前缀")]
        public const string EDITOR_ACTION_DEFINE_OUTPUT_NAME_DEFAULT = "输出值";
        [TranslateMsg("尝试退出自定义动作的编辑页面，但没有保存情况下的警告")]
        public const string ACTION_WARNING_UNSAVED_CHANGES = "是否将更改保存到该自定义动作中？";
        [TranslateMsg("尝试退出自定义动作的编辑页面，但没有保存情况下的按钮名称")]
        public const string ACTION_WARNING_OPTION_SAVE = "保存";
        [TranslateMsg("尝试退出自定义动作的编辑页面，但没有保存情况下的按钮名称")]
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
