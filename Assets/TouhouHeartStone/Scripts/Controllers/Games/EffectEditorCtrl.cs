using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using BJSYGameCore;
using System.Linq;
using Game;
using UnityEngine;
using System;
using System.Collections.Generic;
using BJSYGameCore.UI;
using System.Threading.Tasks;
using MukioI18n;
using System.Reflection;

namespace Controllers
{
    partial class EffectEditorCtrl : GameObjectCtrl<MainSceneCtrl>, IUICtrl
    {
        #region 公有方法
        public Task display(long cardPoolId, CardDefine cardDefine, GeneratedEffect effect)
        {
            return display(new DisplayInfo(cardPoolId, cardDefine, effect, new ActionNodeGraphCtrl.DisplayInfo(
                        cardPoolId,
                        effect,
                        null,
                        game.card.getOrCreateCardEffectActionNodeGraphInfo(cardPoolId, cardDefine.id, cardDefine.getEffects().indexOf(effect)))
                ));
        }
        public Task display(long cardPoolId, GeneratedBuffDefine buffDefine, GeneratedEffect effect)
        {
            return display(new DisplayInfo(cardPoolId, buffDefine, effect, new ActionNodeGraphCtrl.DisplayInfo(
                        cardPoolId,
                        effect,
                        null,
                        game.card.getOrCreateBuffEffectActionNodeGraphInfo(cardPoolId, buffDefine.id, buffDefine.effectList.IndexOf(effect))
                )));
        }
        public Task display(long cardPoolId, GeneratedActionDefine actionDefine)
        {
            return display(
                new DisplayInfo(cardPoolId, actionDefine,
                    new ActionNodeGraphCtrl.DisplayInfo(
                        cardPoolId,
                        actionDefine,
                        null,
                        game.card.getOrCreateActionDefineActionNodeGraphInfo(cardPoolId, actionDefine.id))));
        }
        public async Task display(UIDisplayInfo displayInfo)
        {
            if (displayInfo is DisplayInfo info)
            {
                _cardPoolId = info.cardPoolId;
                _cardDefine = info.cardDefine;
                _buffDefine = info.buffDefine;
                _effect = info.effect;
                _actionDefine = info.actionDefine;
                _ui.gameObject.SetActive(true);
                if (_effect != null)
                {
                    //更新效果属性
                    //updateEffectPropList();
                }
                else if (_actionDefine != null)
                {
                    //更新自定义动作属性
                    updateActionDefinePropList();
                }
                if (info.actionNodeGraphDisplayInfo != null)
                    await _actionNodeGraphCtrl.display(info.actionNodeGraphDisplayInfo);
                else
                    await _actionNodeGraphCtrl.hide();
            }
        }
        public UIDisplayInfo getCurrentDisplayInfo()
        {
            if (_buffDefine != null)
            {
                return new DisplayInfo(_cardPoolId, _buffDefine, _effect,
                    _actionNodeGraphCtrl.getCurrentDisplayInfo() as ActionNodeGraphCtrl.DisplayInfo);
            }
            else
            {
                return new DisplayInfo(_cardPoolId, _cardDefine, _effect,
                    _actionNodeGraphCtrl.getCurrentDisplayInfo() as ActionNodeGraphCtrl.DisplayInfo);
            }
        }
        public Task hide()
        {
            _ui.gameObject.SetActive(false);
            return Task.CompletedTask;
        }
        public void hideActionGraph()
        {
            _actionNodeGraphCtrl.hide();
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponentInChildren<EffectEditorPanel>();
            }
            _selectableRanges = CardEditorCtrl.pileOptionNameDict.Keys.ToArray();
            string[] activeRanges = new string[_selectableRanges.Length];
            for (int i = 0; i < activeRanges.Length; i++)
            {
                activeRanges[i] = game.langManager.getString(CardEditorCtrl.pileOptionNameDict[_selectableRanges[i]]);
            }
            _ui.initActiveRanges(activeRanges, _selectableRanges);
            string[] effectTagNames = new string[_effectTags.Length];
            for (int i = 0; i < effectTagNames.Length; i++)
            {
                effectTagNames[i] = game.langManager.getString(_effectTagOptionNameDict[_effectTags[i]]);
            }
            _ui.initEffectTagNames(effectTagNames, _effectTags);
            //_ui.onPropValueChanged += onPropValueChangedCallback;
            //_ui.onClickPropButton += onClickPropButtonCallback;
            //_ui.onPropItemValueChanged += onPropItemValueChanged;
            //_ui.onClickPropItemButton += onClickPropItemButtonCallback;
            //_ui.onClickPropAddItemButton += onClickPropAddItemButtonCallback;
            //_ui.onClickPropItemRemoveButton += onClickPropItemRemoveButtonCallback;

            _ui.initActionDefineViewData(
                Array.ConvertAll(_categories, c => game.langManager.getString(c)),
                game.editor.getEditorVarTypeNames(),
                game.editor.getEditorConstTypeNames());
            _ui.onActionDefineNameInputFieldValueChanged += onActionDefineNameInputFieldValueChanged;
            _ui.onActionDefineCategoryDropdownValueChanged += onActionDefineCategoryDropdownValueChanged;

            _ui.onClickAddInputButton += onAddInputButtonClicked;
            _ui.onInputListItemDeleteButtonClicked += onInputListItemDeleteButtonClicked;
            _ui.onInputListItemTypeChanged += onInputListItemTypeChanged;
            _ui.onInputListItemNameChanged += onInputListItemNameChanged;
            _ui.onInputListItemIsArrayValueChanged += onInputListItemIsArrayValueChanged;
            _ui.onInputListItemIsParamsValueChanged += onInputListItemIsParamsValueChanged;

            _ui.onClickAddConstButton += onAddConstButtonClicked;
            _ui.onConstListItemDeleteButtonClicked += onConstListItemDeleteButtonClicked;
            _ui.onConstListItemTypeChanged += onConstListItemTypeChanged;
            _ui.onConstListItemNameChanged += onConstListItemNameChanged;
            _ui.onConstListItemIsArrayValueChanged += onConstListItemIsArrayValueChanged;
            _ui.onConstListItemIsParamsValueChanged += onConstListItemIsParamsValueChanged;

            _ui.onClickAddOutputButton += onAddOutputButtonClicked;
            _ui.onOutputListItemDeleteButtonClicked += onOutputListItemDeleteButtonClicked;
            _ui.onOutputListItemTypeChanged += onOutputListItemTypeChanged;
            _ui.onOutputListItemNameChanged += onOutputListItemNameChanged;
            _ui.onOutputListItemIsArrayValueChanged += onOutputListItemIsArrayValueChanged;
            _ui.onOutputListItemIsParamsValueChanged += onOutputListItemIsParamsValueChanged;

            _ui.onDeleteActionDefineButtonClicked += onDeleteActionDefineButtonClickedCallback;

            _ui.onClickReturnButton += onClickReturnButton;
        }
        #endregion
        #region 事件回调
        private void onActionDefineNameInputFieldValueChanged(string value)
        {
            MarkCardUnsaved();
            if (!_actionNodeGraphCtrl.isActionDefineNameExist(value))
            {
                _actionDefine.defineName = value;
            }
            else
            {
                //重名
                game.ui.dialog.ShowConfirm(
                    game.langManager.getString(StringTable.ERROR),
                    game.langManager.getString(StringTable.EDITOR_ERROR_ACTION_DEFINE_NAME_ALREADY_EXIST));
                _actionDefine.defineName = game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_NAME_DEFAULT);
                _ui.setActionDefineName(_actionDefine.defineName);
            }
        }
        private void onActionDefineCategoryDropdownValueChanged(int index)
        {
            MarkCardUnsaved();
            _actionDefine.setCategory(_categories[index]);
        }
        private void onAddInputButtonClicked()
        {
            MarkCardUnsaved();
            int id = 0;
            while (_actionDefine.inputDefines.Any(d => d.name == $"input{id}"))
            {
                id++;
            }
            var valueDefine = PortDefine.Value(
                typeof(int),
                $"input{id}",
                game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_VAR_NAME_DEFAULT) + (id + 1));
            _actionNodeGraphCtrl.addActionDefineInput(valueDefine);
            updateActionDefinePropList();
        }
        private void onInputListItemDeleteButtonClicked(int index)
        {
            MarkCardUnsaved();
            var define = _actionDefine.inputDefines.ElementAt(index);
            _actionNodeGraphCtrl.deleteActionDefineInput(define);
            updateActionDefinePropList();
        }
        private void onInputListItemTypeChanged(int index, int typeIndex)
        {
            MarkCardUnsaved();
            var targetType = game.editor.getEditorVarTypeAt(typeIndex);
            var define = _actionDefine.inputDefines.ElementAt(index);
            var curType = define.type;
            if (curType.IsArray)
                targetType  = targetType.MakeArrayType();
            _actionNodeGraphCtrl.changeActionDefineInputType(define.name, targetType);
        }
        private void onInputListItemNameChanged(int index, string name)
        {
            MarkCardUnsaved();
            _actionDefine.inputDefines.ElementAt(index).name = name;
            updateActionDefinePropList();
            _actionNodeGraphCtrl.updateActionDefineEntryAndReturnNodes();
        }
        private void onInputListItemIsArrayValueChanged(int index, bool value)
        {
            MarkCardUnsaved();
            var define = _actionDefine.inputDefines.ElementAt(index);
            var targetType = define.type;
            if (value && !targetType.IsArray)
                targetType = targetType.MakeArrayType();
            else if (!value && targetType.IsArray)
                targetType = targetType.GetElementType();
            _actionNodeGraphCtrl.changeActionDefineInputType(define.name, targetType);
        }
        private void onInputListItemIsParamsValueChanged(int index, bool value)
        {
            MarkCardUnsaved();
            _actionDefine.isParams = value;
            _actionNodeGraphCtrl.updateActionDefineEntryAndReturnNodes();
        }
        private void onAddConstButtonClicked()
        {
            MarkCardUnsaved();
            int id = 0;
            while (_actionDefine.constDefines.Any(d => d.name == $"const{id}"))
            {
                id++;
            }
            _actionDefine.addConst(PortDefine.Const(
                typeof(int),
                $"const{id}",
                game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_CONST_NAME_DEFAULT) + (id + 1)));
            updateActionDefinePropList();
            _actionNodeGraphCtrl.updateActionDefineEntryAndReturnNodes();
        }
        private void onConstListItemDeleteButtonClicked(int index)
        {
            MarkCardUnsaved();
            var define = _actionDefine.constDefines.ElementAt(index);
            _actionNodeGraphCtrl.deleteActionDefineConst(define);
            updateActionDefinePropList();
        }
        private void onConstListItemTypeChanged(int index, int typeIndex)
        {
            MarkCardUnsaved();
            var targetType = game.editor.getEditorConstTypeAt(typeIndex);
            var define = _actionDefine.constDefines.ElementAt(index);
            var curType = define.type;
            if (curType.IsArray)
                targetType = targetType.MakeArrayType();
            _actionNodeGraphCtrl.changeActionDefineConstType(define.name, targetType);
        }
        private void onConstListItemNameChanged(int index, string name)
        {
            MarkCardUnsaved();
            var define = _actionDefine.constDefines.ElementAt(index);
            define.displayName = name;
            updateActionDefinePropList();
            _actionNodeGraphCtrl.updateActionDefineEntryAndReturnNodes();
        }
        private void onConstListItemIsArrayValueChanged(int index, bool value)
        {
            MarkCardUnsaved();
            var define = _actionDefine.constDefines.ElementAt(index);
            var targetType = define.type;
            if (value && !targetType.IsArray)
                targetType = targetType.MakeArrayType();
            else if (!value && targetType.IsArray)
                targetType = targetType.GetElementType();
            _actionNodeGraphCtrl.changeActionDefineConstType(define.name, targetType);
        }
        private void onConstListItemIsParamsValueChanged(int index, bool value)
        {
        }
        private void onAddOutputButtonClicked()
        {
            MarkCardUnsaved();
            int id = 0;
            while (_actionDefine.outputDefines.Any(d => d.name == $"output{id}"))
            {
                id++;
            }
            _actionDefine.addOutput(PortDefine.Value(
                typeof(int),
                $"output{id}",
                game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_VAR_NAME_DEFAULT) + (id + 1)));
            updateActionDefinePropList();
            _actionNodeGraphCtrl.updateActionDefineEntryAndReturnNodes();
        }
        private void onOutputListItemDeleteButtonClicked(int index)
        {
            MarkCardUnsaved();
            var define = _actionDefine.outputDefines.ElementAt(index);
            _actionNodeGraphCtrl.deleteActionDefineOutput(define);
            updateActionDefinePropList();
        }
        private void onOutputListItemTypeChanged(int index, int typeIndex)
        {
            MarkCardUnsaved();
            var targetType = game.editor.getEditorVarTypeAt(typeIndex);
            var define = _actionDefine.outputDefines.ElementAt(index);
            var curType = define.type;
            if (curType.IsArray)
                targetType = targetType.MakeArrayType();
            _actionNodeGraphCtrl.changeActionDefineOutputType(define.name, targetType);
        }
        private void onOutputListItemNameChanged(int index, string name)
        {
            MarkCardUnsaved();
            var define = _actionDefine.outputDefines.ElementAt(index);
            define.displayName = name;
            _actionNodeGraphCtrl.updateActionDefineEntryAndReturnNodes();
        }
        private void onOutputListItemIsArrayValueChanged(int index, bool value)
        {
            MarkCardUnsaved();
            var define = _actionDefine.outputDefines.ElementAt(index);
            var targetType = define.type;
            if (value && !targetType.IsArray)
                targetType = targetType.MakeArrayType();
            else if (!value && targetType.IsArray)
                targetType = targetType.GetElementType();
            _actionNodeGraphCtrl.changeActionDefineOutputType(define.name, targetType);
        }
        private void onOutputListItemIsParamsValueChanged(int index, bool value)
        {
        }
        private void onDeleteActionDefineButtonClickedCallback()
        {
            MarkCardUnsaved();
            //询问玩家
            game.ui.dialog.ShowQuestion(
               game.langManager.getString(StringTable.WARNING),
               game.langManager.getString(StringTable.EDITOR_WARNING_DELETE_ACTION_DFINE),
               async () =>
               {
                   //删除动作定义
                   game.card.removeActionDefine(_cardPoolId, _actionDefine);
                   //退出当前界面
                   await game.ui.returnPrevUI(() =>
                   {
                       _ui.gameObject.SetActive(false);
                   });
               });
        }
        private async void onClickReturnButton()
        {
            _actionNodeGraphCtrl.quitGraph();
            await game.ui.returnPrevUI(() =>
            {
                _ui.gameObject.SetActive(false);
            });
        }
        #endregion
        private void MarkCardUnsaved()
        {
            scene.cardEditor.MarkCardUnsaved();
        }

        private ActionNodeValueType getValueTypeByType(Type type)
        {
            if (type == typeof(int))
                return ActionNodeValueType.integer;
            else if (type == typeof(float))
                return ActionNodeValueType.real;
            else if (type == typeof(bool))
                return ActionNodeValueType.boolean;
            else if (type == typeof(string))
                return ActionNodeValueType.text;
            else if (type == typeof(PileNameCollection))
                return ActionNodeValueType.pileNameCollection;
            else if (type == typeof(EffectTagCollection))
                return ActionNodeValueType.effectTagCollection;
            else if (type == typeof(ActionNode))
                return ActionNodeValueType.actionNode;
            else if (type == typeof(FunctionNode))
                return ActionNodeValueType.functionNode;
            else if (type == typeof(ActionValueRef))
                return ActionNodeValueType.actionValueRef;
            else if (type == typeof(TriggerGraph))
                return ActionNodeValueType.trigger;
            else if (type == typeof(TriggerCollection))
                return ActionNodeValueType.triggerCollection;
            else if (type == typeof(DefineReference))
                return ActionNodeValueType.cardReference;
            else if (type == typeof(DefineReference[]))
                return ActionNodeValueType.cardReferenceCollection;
            else if (type == typeof(KeywordCollection))
                return ActionNodeValueType.keywords;
            else
                throw new NotImplementedException();
        }
        private void updateActionDefinePropList()
        {
            //显示自定义动作属性
            _ui.switchToActionDefine();
            //更新名称
            _ui.setActionDefineName(_actionDefine.defineName);
            //更新类型
            _ui.setActionDefineCategory(Array.IndexOf(_categories, _actionDefine.category));
            //更新输入
            var inputs = _actionDefine.inputDefines;
            (int typeIndex, string name, bool isArray)[] inputViewData =
                new (int typeIndex, string name, bool isArray)[inputs.Count()];
            for (int i = 0; i < inputs.Count(); i++)
            {
                PortDefine valueDefine = inputs.ElementAt(i);
                inputViewData[i] = (game.editor.getEditorVarTypeNameIndex(valueDefine.type.Name), valueDefine.name, valueDefine.type.IsArray);
            }
            _ui.updateInputList(inputViewData);
            //更新常量
            var consts = _actionDefine.constDefines;
            (int typeIndex, string name)[] constViewData = new (int typeIndex, string name)[consts.Count()];
            for (int i = 0; i < consts.Count(); i++)
            {
                PortDefine valueDefine = consts.ElementAt(i);
                constViewData[i] = (game.editor.getEditorConstTypeNameIndex(valueDefine.type.Name), valueDefine.name);
            }
            _ui.updateConstList(constViewData);
            //更新输出
            var outputs = _actionDefine.constDefines;
            (int typeIndex, string name, bool isArray)[] outputViewData = new (int typeIndex, string name, bool isArray)[outputs.Count()];
            for (int i = 0; i < outputs.Count(); i++)
            {
                PortDefine valueDefine = outputs.ElementAt(i);
                outputViewData[i] = (game.editor.getEditorVarTypeNameIndex(valueDefine.type.Name), valueDefine.name, valueDefine.type.IsArray);
            }
            _ui.updateOutputList(outputViewData);
        }
        #endregion
        #region 属性字段
        [TranslateMsg("编辑器-效果-属性名称-更新动作（光环效果）")]
        private const string EDITOR_EFFECT_PROP_NAME_UPDATE_ACTION = "更新动作";
        
        [SerializeField]
        EffectEditorPanel _ui;
        [SerializeField]
        ActionNodeGraphCtrl _actionNodeGraphCtrl;
        [Obsolete("使用game作为替代")]
        public IAppManager app { get; }
        long _cardPoolId;
        CardDefine _cardDefine;
        GeneratedBuffDefine _buffDefine;
        GeneratedEffect _effect;
        GeneratedActionDefine _actionDefine;
        string[] _selectableRanges;
        static readonly string[] _effectTags = new string[]
        {
            KEYWORD.BATTLECRY,
            KEYWORD.DEATHRATTLE
        };
        static readonly Dictionary<string, string> _effectTagOptionNameDict = new Dictionary<string, string>()
        {
            { KEYWORD.BATTLECRY, StringTable.GAME_KEYWORD_BATTLECRY },
            { KEYWORD.DEATHRATTLE, StringTable.GAME_KEYWORD_DEATHRATTLE }
        };
        static readonly string[] _categories = new string[]
        {
            StringTable.EDITOR_NODE_CATEGORY_GAME,
            StringTable.EDITOR_NODE_CATEGORY_PLAYER,
            StringTable.EDITOR_NODE_CATEGORY_PILE,
            StringTable.EDITOR_NODE_CATEGORY_CARD,
            StringTable.EDITOR_NODE_CATEGORY_BUFF,
            StringTable.EDITOR_NODE_CATEGORY_MODIFIER,
            StringTable.EDITOR_NODE_CATEGORY_EVENT,
            StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION
        };
        #endregion
        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(long cardPoolId, CardDefine cardDefine, GeneratedEffect effect, ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo)
            {
                this.cardPoolId = cardPoolId;
                this.cardDefine = cardDefine;
                this.effect = effect;
                this.actionNodeGraphDisplayInfo = actionNodeGraphDisplayInfo;
            }
            public DisplayInfo(long cardPoolId, GeneratedBuffDefine buffDefine, GeneratedEffect effect, ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo)
            {
                this.cardPoolId = cardPoolId;
                this.buffDefine = buffDefine;
                this.effect = effect;
                this.actionNodeGraphDisplayInfo = actionNodeGraphDisplayInfo;
            }
            public DisplayInfo(long cardPoolId, GeneratedActionDefine actionDefine, ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo)
            {
                this.cardPoolId = cardPoolId;
                this.actionDefine = actionDefine;
                this.actionNodeGraphDisplayInfo = actionNodeGraphDisplayInfo;
            }
            public long cardPoolId;
            public CardDefine cardDefine;
            public GeneratedBuffDefine buffDefine;
            public GeneratedEffect effect;
            public GeneratedActionDefine actionDefine;
            public ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo;
        }
        #endregion
    }
}
