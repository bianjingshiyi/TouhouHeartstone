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

namespace Controllers
{
    partial class EffectEditorCtrl : GameObjectCtrl<MainSceneCtrl>, IUICtrl
    {
        #region 公有方法
        public Task display(int cardPoolId, CardDefine cardDefine, GeneratedEffect effect)
        {
            return game.ui.displayUIWithReturn(this, new DisplayInfo(cardPoolId, cardDefine, effect, null));
        }
        public Task display(int cardPoolId, GeneratedBuffDefine buffDefine, GeneratedEffect effect)
        {
            return game.ui.displayUIWithReturn(this, new DisplayInfo(cardPoolId, buffDefine, effect, null));
        }
        public Task display(int cardPoolId, GeneratedActionDefine actionDefine)
        {
            return game.ui.displayUIWithReturn(this, new DisplayInfo(cardPoolId, actionDefine, null));
        }
        public async Task display(UIDisplayInfo displayInfo)
        {
            if (displayInfo is DisplayInfo info)
            {
                _cardPoolId = info.cardPoolId;
                _cardDefine = info.cardDefine;
                _buffDefine = info.buffDefine;
                _effect = info.effect;
                _generatedActionDefine = info.generatedActionDefine;
                _ui.gameObject.SetActive(true);
                if (_generatedActionDefine == null)
                {
                    //更新效果属性
                    updateEffectPropList();
                }
                else
                {
                    //更新自定义动作属性
                    updateActionDefinePropList();
                }
                if (info.actionNodeGraphDisplayInfo != null)
                    await scene.actionNodeGraphCtrl.display(info.actionNodeGraphDisplayInfo);
                else
                    await scene.actionNodeGraphCtrl.hide();
            }
        }
        public UIDisplayInfo getCurrentDisplayInfo()
        {
            if (_buffDefine != null)
            {
                return new DisplayInfo(_cardPoolId, _buffDefine, _effect,
                    scene.actionNodeGraphCtrl.getCurrentDisplayInfo() as ActionNodeGraphCtrl.DisplayInfo);
            }
            else
            {
                return new DisplayInfo(_cardPoolId, _cardDefine, _effect,
                    scene.actionNodeGraphCtrl.getCurrentDisplayInfo() as ActionNodeGraphCtrl.DisplayInfo);
            }
        }
        public Task hide()
        {
            _ui.gameObject.SetActive(false);
            return Task.CompletedTask;
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
            _selectableRanges = new string[]
            {
                PILE.FIELD,
                PILE.HAND,
                PILE.DECK,
                PILE.GRAVE,
                PILE.MASTER,
                PILE.SKILL,
                PILE.ITEM,
                PILE.HEAD
            };
            string[] activeRanges = new string[_selectableRanges.Length];
            for (int i = 0; i < activeRanges.Length; i++)
            {
                activeRanges[i] = game.langManager.getString(_pileOptionNameDict[_selectableRanges[i]]);
            }
            _ui.initActiveRanges(activeRanges, _selectableRanges);
            string[] effectTagNames = new string[_effectTags.Length];
            for (int i = 0; i < effectTagNames.Length; i++)
            {
                effectTagNames[i] = game.langManager.getString(_effectTagOptionNameDict[_effectTags[i]]);
            }
            _ui.initEffectTagNames(effectTagNames, _effectTags);
            _ui.onPropValueChanged += onPropValueChangedCallback;
            _ui.onClickPropButton += onClickPropButtonCallback;
            _ui.onPropItemValueChanged += onPropItemValueChanged;
            _ui.onClickPropItemButton += onClickPropItemButtonCallback;
            _ui.onClickPropAddItemButton += onClickPropAddItemButtonCallback;
            _ui.onClickPropItemRemoveButton += onClickPropItemRemoveButtonCallback;

            _ui.initValueDefineTypes(getEditorVarTypeNames(), getEditorConstTypeNames());
            _ui.onActionDefineNameInputFieldValueChanged += onActionDefineNameInputFieldValueChanged;
            
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
            
            _ui.onActionDefineNameInputFieldValueChanged += onActionDefineNameInputFieldValueChanged;
            _ui.onClickAddInputButton += onAddInputButtonClicked;
            _ui.onInputListItemDeleteButtonClicked += onInputListItemDeleteButtonClicked;
            _ui.onInputListItemTypeChanged += onInputListItemTypeChanged;
            _ui.onInputListItemNameChanged += onInputListItemNameChanged;
            _ui.onInputListItemIsArrayValueChanged += onInputListItemIsArrayValueChanged;
            _ui.onInputListItemIsParamsValueChanged += onInputListItemIsParamsValueChanged;

            _ui.onClickReturnButton += onClickReturnButton;
        }
        #endregion
        #region 事件回调
        private void onPropValueChangedCallback(int index, object value)
        {
            EffectPropertyInfo[] propInfos = _effect.getPropInfos();
            _effect.setProp(propInfos[index].name, value);
        }
        private void onClickPropButtonCallback(int index)
        {
            EffectPropertyInfo[] propInfos = _effect.getPropInfos();
            if (propInfos[index].type == typeof(ActionNode))
            {
                ActionNodeGraphInfo graphInfo = _buffDefine != null ?
                    game.card.getOrCreateBuffEffectActionNodeGraphInfo(_cardPoolId, _buffDefine.id, _buffDefine.effectList.IndexOf(_effect), propInfos[index].name, 0) :
                    game.card.getOrCreateCardEffectActionNodeGraphInfo(_cardPoolId, _cardDefine.id, _cardDefine.getEffectIndex(_effect), propInfos[index].name, 0);
                scene.actionNodeGraphCtrl.openActionGraph(_cardPoolId, _effect, propInfos[index].name, graphInfo);
            }
            else if (propInfos[index].type == typeof(ActionValueRef))
            {
                ActionValueRef valueRef = _effect.getProp<ActionValueRef>(propInfos[index].name);
                if (valueRef == null)
                {
                    valueRef = new ActionValueRef();
                    _effect.setProp(propInfos[index].name, valueRef);
                }
                ActionNodeGraphInfo graphInfo = _buffDefine != null ?
                    game.card.getOrCreateBuffEffectActionNodeGraphInfo(_cardPoolId, _buffDefine.id, _buffDefine.effectList.IndexOf(_effect), propInfos[index].name, 0) :
                    game.card.getOrCreateCardEffectActionNodeGraphInfo(_cardPoolId, _cardDefine.id, _cardDefine.getEffectIndex(_effect), propInfos[index].name, 0);
                scene.actionNodeGraphCtrl.openActionValueRefGraph(_cardPoolId, valueRef, graphInfo);
            }
            else if (propInfos[index].type == typeof(TriggerGraph))
            {
                TriggerGraph trigger = _effect.getProp<TriggerGraph>(propInfos[index].name);
                if (trigger == null)
                {
                    trigger = new TriggerGraph();
                    _effect.setProp(propInfos[index].name, trigger);
                }
                ActionNodeGraphInfo graphInfo = _buffDefine != null ?
                    game.card.getOrCreateBuffEffectActionNodeGraphInfo(_cardPoolId, _buffDefine.id, _buffDefine.effectList.IndexOf(_effect), propInfos[index].name, 0) :
                    game.card.getOrCreateCardEffectActionNodeGraphInfo(_cardPoolId, _cardDefine.id, _cardDefine.getEffectIndex(_effect), propInfos[index].name, 0);
                scene.actionNodeGraphCtrl.openTriggerGraph(_cardPoolId, propInfos[index].name, trigger, graphInfo);
            }
        }
        private void onPropItemValueChanged(int index, int itemIndex, object value)
        {
            EffectPropertyInfo[] propInfos = _effect.getPropInfos();
        }
        private void onClickPropItemButtonCallback(int index, int itemIndex)
        {
            EffectPropertyInfo[] propInfos = _effect.getPropInfos();
            if (propInfos[index].type == typeof(TriggerCollection))
            {
                TriggerCollection triggers = _effect.getProp<TriggerCollection>(propInfos[index].name);
                ActionNodeGraphInfo graphInfo = _buffDefine != null ?
                    game.card.getOrCreateBuffEffectActionNodeGraphInfo(_cardPoolId, _buffDefine.id, _buffDefine.effectList.IndexOf(_effect), propInfos[index].name, itemIndex) :
                    game.card.getOrCreateCardEffectActionNodeGraphInfo(_cardPoolId, _cardDefine.id, _cardDefine.getEffectIndex(_effect), propInfos[index].name, itemIndex);
                scene.actionNodeGraphCtrl.openTriggerGraph(_cardPoolId, null, triggers[itemIndex], graphInfo);
            }
        }
        private void onClickPropAddItemButtonCallback(int index)
        {
            EffectPropertyInfo[] propInfos = _effect.getPropInfos();
            if (propInfos[index].type == typeof(TriggerCollection))
            {
                TriggerCollection triggers = _effect.getProp<TriggerCollection>(propInfos[index].name);
                if (triggers == null)
                {
                    triggers = new TriggerCollection();
                    _effect.setProp(propInfos[index].name, triggers);
                }
                triggers.Add(new TriggerGraph());
            }
            updateEffectPropList();
        }
        private void onClickPropItemRemoveButtonCallback(int index, int itemIndex)
        {
            EffectPropertyInfo[] propInfos = _effect.getPropInfos();
            if (propInfos[index].type == typeof(TriggerCollection))
            {
                TriggerCollection triggers = _effect.getProp<TriggerCollection>(propInfos[index].name);
                triggers.RemoveAt(itemIndex);
            }
            updateEffectPropList();
        }
        private void onActionDefineNameInputFieldValueChanged(string value)
        {
            _generatedActionDefine.defineName = value;
        }
        private void onAddInputButtonClicked()
        {
            _generatedActionDefine.inputList.Add(new ValueDefine(
                typeof(int),
                game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_VAR_NAME_DEFAULT),
                false,
                false));
            updateActionDefinePropList();
        }
        private void onInputListItemDeleteButtonClicked(int index)
        {
            _generatedActionDefine.inputList.RemoveAt(index);
            updateActionDefinePropList();
        }
        private void onInputListItemTypeChanged(int index, int typeIndex)
        {
            bool isArray = _generatedActionDefine.inputList[index].type.IsArray;
            _generatedActionDefine.inputList[index].type = isArray ? getEditorVarTypeAt(typeIndex).MakeArrayType() : getEditorVarTypeAt(typeIndex);
            updateActionDefinePropList();
        }
        private void onInputListItemNameChanged(int index, string name)
        {
            _generatedActionDefine.inputList[index].name = name;
            updateActionDefinePropList();
        }
        private void onInputListItemIsArrayValueChanged(int index, bool value)
        {
            Type type = _generatedActionDefine.inputList[index].type;
            _generatedActionDefine.inputList[index].type = value ? type.MakeArrayType() : type.GetElementType();
            updateActionDefinePropList();
        }
        private void onInputListItemIsParamsValueChanged(int index, bool value)
        {
            _generatedActionDefine.inputList[index].isParams = value;
            updateActionDefinePropList();
        }
        private void onAddConstButtonClicked()
        {
            _generatedActionDefine.constList.Add(new ValueDefine(
                typeof(int),
                game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_VAR_NAME_DEFAULT),
                false,
                false));
            updateActionDefinePropList();
        }
        private void onConstListItemDeleteButtonClicked(int index)
        {
            _generatedActionDefine.constList.RemoveAt(index);
            updateActionDefinePropList();
        }
        private void onConstListItemTypeChanged(int index, int typeIndex)
        {
            bool isArray = _generatedActionDefine.constList[index].type.IsArray;
            _generatedActionDefine.constList[index].type = isArray ? getEditorConstTypeAt(typeIndex).MakeArrayType() : getEditorConstTypeAt(typeIndex);
            updateActionDefinePropList();
        }
        private void onConstListItemNameChanged(int index, string name)
        {
            _generatedActionDefine.constList[index].name = name;
            updateActionDefinePropList();
        }
        private void onConstListItemIsArrayValueChanged(int index, bool value)
        {
            Type type = _generatedActionDefine.constList[index].type;
            _generatedActionDefine.constList[index].type = value ? type.MakeArrayType() : type.GetElementType();
            updateActionDefinePropList();
        }
        private void onConstListItemIsParamsValueChanged(int index, bool value)
        {
            _generatedActionDefine.constList[index].isParams = value;
            updateActionDefinePropList();
        }
        private void onAddOutputButtonClicked()
        {
            _generatedActionDefine.outputList.Add(new ValueDefine(
                typeof(int),
                game.langManager.getString(StringTable.EDITOR_ACTION_DEFINE_VAR_NAME_DEFAULT),
                false,
                false));
            updateActionDefinePropList();
        }
        private void onOutputListItemDeleteButtonClicked(int index)
        {
            _generatedActionDefine.outputList.RemoveAt(index);
            updateActionDefinePropList();
        }
        private void onOutputListItemTypeChanged(int index, int typeIndex)
        {
            bool isArray = _generatedActionDefine.outputList[index].type.IsArray;
            _generatedActionDefine.outputList[index].type = isArray ? getEditorVarTypeAt(typeIndex).MakeArrayType() : getEditorVarTypeAt(typeIndex);
            updateActionDefinePropList();
        }
        private void onOutputListItemNameChanged(int index, string name)
        {
            _generatedActionDefine.outputList[index].name = name;
            updateActionDefinePropList();
        }
        private void onOutputListItemIsArrayValueChanged(int index, bool value)
        {
            Type type = _generatedActionDefine.outputList[index].type;
            _generatedActionDefine.outputList[index].type = value ? type.MakeArrayType() : type.GetElementType();
            updateActionDefinePropList();
        }
        private void onOutputListItemIsParamsValueChanged(int index, bool value)
        {
            _generatedActionDefine.outputList[index].isParams = value;
            updateActionDefinePropList();
        }
        private async void onClickReturnButton()
        {
            await game.ui.returnPrevUI(() =>
            {
                _ui.gameObject.SetActive(false);
            });
        }
        #endregion
        /// <summary>
        /// 更新属性信息UI
        /// </summary>
        private void updateEffectPropList()
        {
            EffectPropertyInfo[] propInfos = _effect.getPropInfos();
            ActionNodeValueInfo[] valueInfos = new ActionNodeValueInfo[propInfos.Length];
            for (int i = 0; i < valueInfos.Length; i++)
            {
                valueInfos[i] = new ActionNodeValueInfo()
                {
                    //翻译属性名称
                    name = _propNameDict.ContainsKey(propInfos[i].name) ? game.langManager.getString(_propNameDict[propInfos[i].name]) : propInfos[i].name,
                    type = getValueTypeByType(propInfos[i].type),
                    value = _effect.getProp(propInfos[i].name)
                };
            }
            _ui.updatePropList(valueInfos);
            for (int i = 0; i < valueInfos.Length; i++)
            {
                ValueInputField item = _ui.getPropItemAt(i);
                if (valueInfos[i].type == ActionNodeValueType.actionNode)
                {
                    item.setButtonText(game.langManager.getString(StringTable.EDITOR_BUTTON_EDIT_ACTION));
                }
                else if (valueInfos[i].type == ActionNodeValueType.actionValueRef)
                {
                    item.setButtonText(game.langManager.getString(StringTable.EDITOR_BUTTON_EDIT_RETURN_VALUE));
                }
                else if (valueInfos[i].type == ActionNodeValueType.trigger)
                {
                    item.setButtonText(game.langManager.getString(StringTable.EDITOR_BUTTON_EDIT_TRIGGER));
                }
                else if (valueInfos[i].type == ActionNodeValueType.triggerCollection)
                {
                    if (valueInfos[i].value is TriggerCollection triggerCol)
                    {
                        for (int j = 0; j < triggerCol.Count; j++)
                        {
                            item.getItemAt(j).setButtonText(game.langManager.getString(StringTable.EDITOR_BUTTON_EDIT_TRIGGER));
                        }
                    }
                }
            }
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
            else if (type == typeof(ActionValueRef))
                return ActionNodeValueType.actionValueRef;
            else if (type == typeof(TriggerGraph))
                return ActionNodeValueType.trigger;
            else if (type == typeof(TriggerCollection))
                return ActionNodeValueType.triggerCollection;
            else if (type == typeof(CardReference))
                return ActionNodeValueType.cardReference;
            else if (type == typeof(CardReference[]))
                return ActionNodeValueType.cardReferenceCollection;
            else
                throw new NotImplementedException();
        }
        private void updateActionDefinePropList()
        {
            //显示自定义动作属性
            _ui.switchToActionDefine();
            //更新名称
            _ui.setActionDefineName(_generatedActionDefine.defineName);
            //更新输入
            (int typeIndex, string name, bool isArray, bool isParams)[] inputViewData =
                new (int typeIndex, string name, bool isArray, bool isParams)[_generatedActionDefine.inputs.Length];
            for (int i = 0; i < _generatedActionDefine.inputs.Length; i++)
            {
                ValueDefine valueDefine = _generatedActionDefine.inputs[i];
                inputViewData[i] = (getEditorVarTypeIndex(valueDefine.type), valueDefine.name, valueDefine.type.IsArray, valueDefine.isParams);
            }
            _ui.updateInputList(inputViewData);
            //更新常量
            (int typeIndex, string name)[] constViewData = new (int typeIndex, string name)[_generatedActionDefine.consts.Length];
            for (int i = 0; i < _generatedActionDefine.consts.Length; i++)
            {
                ValueDefine valueDefine = _generatedActionDefine.consts[i];
                constViewData[i] = (getEditorConstTypeIndex(valueDefine.type), valueDefine.name);
            }
            _ui.updateConstList(constViewData);
            //更新输出
            (int typeIndex, string name, bool isArray)[] outputViewData = new (int typeIndex, string name, bool isArray)[_generatedActionDefine.outputs.Length];
            for (int i = 0; i < _generatedActionDefine.outputs.Length; i++)
            {
                ValueDefine valueDefine = _generatedActionDefine.outputs[i];
                outputViewData[i] = (getEditorVarTypeIndex(valueDefine.type), valueDefine.name, valueDefine.type.IsArray);
            }
            _ui.updateOutputList(outputViewData);
        }
        private int getEditorVarTypeIndex(Type type)
        {
            int index = -1;
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].isVarType)
                    index++;
                if (_editorTypeInfos[i].type == type)
                    return index;
            }
            return index;
        }
        private int getEditorConstTypeIndex(Type type)
        {
            int index = -1;
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].isConstType)
                    index++;
                if (_editorTypeInfos[i].type == type)
                    return index;
            }
            return index;
        }
        private Type getEditorVarTypeAt(int index)
        {
            int count = 0;
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].isVarType)
                {
                    if (count == index)
                        return _editorTypeInfos[i].type;
                    else
                        count++;
                }
            }
            throw new IndexOutOfRangeException("无法找到索引" + index + "对应变量类型");
        }
        private Type getEditorConstTypeAt(int index)
        {
            int count = 0;
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].isConstType)
                {
                    if (count == index)
                        return _editorTypeInfos[i].type;
                    else
                        count++;
                }
            }
            throw new IndexOutOfRangeException("无法找到索引" + index + "对应常量类型");
        }
        private string[] getEditorVarTypeNames()
        {
            List<string> nameList = new List<string>();
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].isVarType)
                {
                    nameList.Add(game.langManager.getString(_editorTypeInfos[i].typeName));
                }
            }
            return nameList.ToArray();
        }
        private string[] getEditorConstTypeNames()
        {
            List<string> nameList = new List<string>();
            for (int i = 0; i < _editorTypeInfos.Length; i++)
            {
                if (_editorTypeInfos[i].isConstType)
                {
                    nameList.Add(game.langManager.getString(_editorTypeInfos[i].typeName));
                }
            }
            return nameList.ToArray();
        }
        #endregion
        #region 属性字段
        [SerializeField]
        EffectEditorPanel _ui;
        [Obsolete("使用game作为替代")]
        public IAppManager app { get; }
        int _cardPoolId;
        CardDefine _cardDefine;
        GeneratedBuffDefine _buffDefine;
        GeneratedEffect _effect;
        GeneratedActionDefine _generatedActionDefine;
        string[] _selectableRanges;
        readonly string[] _effectTags = new string[]
        {
            KEYWORD.BATTLECRY,
            KEYWORD.DEATHRATTLE
        };
        readonly Dictionary<string, string> _propNameDict = new Dictionary<string, string>()
        {
            { nameof(GeneratedEffect.pileList), StringTable.EDITOR_EFFECT_PROP_NAME_PILE_LIST },
            { nameof(GeneratedEffect.onEnableAction), StringTable.EDITOR_EFFECT_PROP_NAME_ON_ENABLE_ACTION },
            { nameof(GeneratedEffect.onDisableAction), StringTable.EDITOR_EFFECT_PROP_NAME_ON_DISABLE_ACTION },
            { nameof(GeneratedEffect.triggerList), StringTable.EDITOR_EFFECT_PROP_NAME_TRIGGER_LIST },
            { nameof(GeneratedEffect.tagList), StringTable.EDITOR_EFFECT_PROP_NAME_TAG_LIST },
            { nameof(GeneratedHalo.rangeList), StringTable.EDITOR_EFFECT_PROP_NAME_RANGE_LIST },
            { nameof(GeneratedHalo.affectCondition), StringTable.EDITOR_EFFECT_PROP_NAME_AFFECT_CONDITION },
            { nameof(GeneratedHalo.getHaloBuffDefine), StringTable.EDITOR_EFFECT_PROP_NAME_GET_HALO_BUFF_DEFINE },
            { nameof(GeneratedBattlecry.activeTrigger), StringTable.EDITOR_EFFECT_PROP_NAME_ACTIVE_TRIGGER }
        };
        readonly Dictionary<string, string> _pileOptionNameDict = new Dictionary<string, string>()
        {
            { PILE.FIELD, StringTable.GAME_FIELD }  ,
            { PILE.HAND, StringTable.GAME_HAND },
            { PILE.DECK, StringTable.GAME_DECK },
            { PILE.GRAVE, StringTable.GAME_GRAVE },
            { PILE.MASTER, StringTable.GAME_MASTER } ,
            { PILE.SKILL, StringTable.GAME_SKILL },
            { PILE.ITEM, StringTable.GAME_ITEM },
            { PILE.HEAD, StringTable.GAME_DELAY }
        };
        readonly Dictionary<string, string> _effectTagOptionNameDict = new Dictionary<string, string>()
        {
            { KEYWORD.BATTLECRY, StringTable.GAME_KEYWORD_BATTLECRY },
            { KEYWORD.DEATHRATTLE, StringTable.GAME_KEYWORD_DEATHRATTLE }
        };
        readonly EditorTypeInfo[] _editorTypeInfos = new EditorTypeInfo[]
        {
            #region 基础类型
            new EditorTypeInfo() { type = typeof(object), typeName = StringTable.EDITOR_TYPE_NAME_OBJECT, isVarType = true },
            new EditorTypeInfo() { type = typeof(int), typeName = StringTable.EDITOR_TYPE_NAME_INT, isVarType = true, isConstType = true },
            new EditorTypeInfo() { type = typeof(float), typeName = StringTable.EDITOR_TYPE_NAME_FLOAT, isVarType = true, isConstType = true },
            new EditorTypeInfo() { type = typeof(bool), typeName = StringTable.EDITOR_TYPE_NAME_BOOL, isVarType = true, isConstType = true },
            new EditorTypeInfo() { type = typeof(char), typeName = StringTable.EDITOR_TYPE_NAME_CHAR, isVarType = true, isConstType = true },
            new EditorTypeInfo() { type = typeof(string), typeName = StringTable.EDITOR_TYPE_NAME_STRING, isVarType = true, isConstType = true },
            #endregion
            #region 引擎类型
            new EditorTypeInfo() { type = typeof(Rule), typeName = StringTable.EDITOR_TYPE_NAME_RULE, isVarType = true },
            new EditorTypeInfo() { type = typeof(CardEngine), typeName = StringTable.EDITOR_TYPE_NAME_CARD_ENGINE, isVarType = true },
            new EditorTypeInfo() { type = typeof(Player), typeName = StringTable.EDITOR_TYPE_NAME_PLAYER, isVarType = true },
            new EditorTypeInfo() { type = typeof(Pile), typeName = StringTable.EDITOR_TYPE_NAME_PILE, isVarType = true },
            new EditorTypeInfo() { type = typeof(PileNameCollection), typeName = StringTable.EDITOR_TYPE_NAME_PILE_NAME_COLLECTION, isConstType = true },
            new EditorTypeInfo() { type = typeof(CardDefine), typeName = StringTable.EDITOR_TYPE_NAME_CARD_DEFINE, isVarType = true },
            new EditorTypeInfo() { type = typeof(CardReference), typeName = StringTable.EDITOR_TYPE_NAME_CARD_REFERENCE, isConstType = true },
            new EditorTypeInfo() { type = typeof(TouhouCardEngine.Card), typeName = StringTable.EDITOR_TYPE_NAME_CARD, isVarType = true },
            new EditorTypeInfo() { type = typeof(BuffDefine), typeName = StringTable.EDITOR_TYPE_NAME_BUFF_DEFINE, isVarType = true },
            new EditorTypeInfo() { type = typeof(Buff), typeName = StringTable.EDITOR_TYPE_NAME_BUFF, isVarType = true },
            new EditorTypeInfo() { type = typeof(Effect), typeName = StringTable.EDITOR_TYPE_NAME_EFFECT, isVarType = true },
            new EditorTypeInfo() { type = typeof(EffectTagCollection), typeName = StringTable.EDITOR_TYPE_NAME_EFFECT_TAG_COLLECTION, isConstType = true },
            new EditorTypeInfo() { type = typeof(EventArg), typeName = StringTable.EDITOR_TYPE_NAME_EVENT_ARG, isVarType = true },
            new EditorTypeInfo() { type = typeof(PropModifier), typeName = StringTable.EDITOR_TYPE_NAME_PROP_MODIFIER, isVarType = true },
            new EditorTypeInfo() { type = typeof(TriggerGraph), typeName = StringTable.EDITOR_TYPE_NAME_TRIGGER_GRAPH, isVarType = true },
            //typeof(TriggerCollection),
            new EditorTypeInfo() { type = typeof(TargetChecker), typeName = StringTable.EDITOR_TYPE_NAME_TARGET_CHECKER, isVarType = true },
            new EditorTypeInfo() { type = typeof(ActionValueRef), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_VALUE_REF, isVarType = true, isConstType = true },
            new EditorTypeInfo() { type = typeof(ActionDefine), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_DEFINE, isVarType = true },
            new EditorTypeInfo() { type = typeof(ActionNode), typeName = StringTable.EDITOR_TYPE_NAME_ACTION_NODE, isVarType = true, isConstType = true },
            #endregion
        };
        #endregion
        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(int cardPoolId, CardDefine cardDefine, GeneratedEffect effect, ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo)
            {
                this.cardPoolId = cardPoolId;
                this.cardDefine = cardDefine;
                this.effect = effect;
                this.actionNodeGraphDisplayInfo = actionNodeGraphDisplayInfo;
            }
            public DisplayInfo(int cardPoolId, GeneratedBuffDefine buffDefine, GeneratedEffect effect, ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo)
            {
                this.cardPoolId = cardPoolId;
                this.buffDefine = buffDefine;
                this.effect = effect;
                this.actionNodeGraphDisplayInfo = actionNodeGraphDisplayInfo;
            }
            public DisplayInfo(int cardPoolId, GeneratedActionDefine generatedActionDefine, ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo)
            {
                this.cardPoolId = cardPoolId;
                this.generatedActionDefine = generatedActionDefine;
                this.actionNodeGraphDisplayInfo = actionNodeGraphDisplayInfo;
            }
            public int cardPoolId;
            public CardDefine cardDefine;
            public GeneratedBuffDefine buffDefine;
            public GeneratedEffect effect;
            public GeneratedActionDefine generatedActionDefine;
            public ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo;
        }
        class EditorTypeInfo
        {
            public Type type;
            public string typeName;
            public bool isVarType;
            public bool isConstType;
        }
        #endregion
    }
}
