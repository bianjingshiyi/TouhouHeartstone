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
        public async Task display(UIDisplayInfo displayInfo)
        {
            if (displayInfo is DisplayInfo info)
            {
                _cardPoolId = info.cardPoolId;
                _cardDefine = info.cardDefine;
                _buffDefine = info.buffDefine;
                _effect = info.effect;
                _ui.gameObject.SetActive(true);
                updatePropList();
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
            _ui.onActiveRangeDropdownValueChanged += onActiveRangeDropdownValueChanged;
            _ui.onClickEditEnableActionButton += onClickEditEnableActionButton;
            _ui.onClickEditDisableActionButton += onClickEditDisableActionButton;
            _ui.onClickAddTriggerButton += onClickAddTriggerButton;
            _ui.onClickRemoveTriggerButton += onClickRemoveTriggerButton;
            _ui.onClickEditTriggerButton += onClickEditTriggerButton;
            _ui.onPropValueChanged += onPropValueChangedCallback;
            _ui.onClickPropButton += onClickPropButtonCallback;
            _ui.onPropItemValueChanged += onPropItemValueChanged;
            _ui.onClickPropItemButton += onClickPropItemButtonCallback;
            _ui.onClickPropAddItemButton += onClickPropAddItemButtonCallback;
            _ui.onClickPropItemRemoveButton += onClickPropItemRemoveButtonCallback;
            _ui.onClickReturnButton += onClickReturnButton;
        }
        #endregion
        #region 事件回调
        private void onActiveRangeDropdownValueChanged(bool[] values)
        {
            for (int i = 0; i < _selectableRanges.Length; i++)
            {
                if (values[i] && !_effect.pileList.Contains(_selectableRanges[i]))
                    _effect.pileList.Add(_selectableRanges[i]);
                else
                    _effect.pileList.Remove(_selectableRanges[i]);
            }
        }
        private void onClickEditEnableActionButton()
        {
            ActionNodeGraphInfo graphInfo = _buffDefine != null ?
                game.card.getOrCreateBuffEffectActionNodeGraphInfo(_cardPoolId, _buffDefine.id, _buffDefine.effectList.IndexOf(_effect), nameof(GeneratedEffect.onEnableAction), 0) :
                game.card.getOrCreateCardEffectActionNodeGraphInfo(_cardPoolId, _cardDefine.id, _cardDefine.getEffectIndex(_effect), nameof(GeneratedEffect.onEnableAction), 0);
            scene.actionNodeGraphCtrl.openOnEnableActionGraph(_cardPoolId, _effect, graphInfo);
        }
        private void onClickEditDisableActionButton()
        {
            ActionNodeGraphInfo graphInfo = _buffDefine != null ?
                game.card.getOrCreateBuffEffectActionNodeGraphInfo(_cardPoolId, _buffDefine.id, _buffDefine.effectList.IndexOf(_effect), nameof(GeneratedEffect.onDisableAction), 0) :
                game.card.getOrCreateCardEffectActionNodeGraphInfo(_cardPoolId, _cardDefine.id, _cardDefine.getEffectIndex(_effect), nameof(GeneratedEffect.onDisableAction), 0);
            scene.actionNodeGraphCtrl.openOnDisableActionGraph(_cardPoolId, _effect, graphInfo);
        }
        private void onClickAddTriggerButton()
        {
            _effect.triggerList.Add(new TriggerGraph());
            updateTriggerList();
        }
        private void onClickRemoveTriggerButton(int index)
        {
            _effect.triggerList.RemoveAt(index);
        }
        private void onClickEditTriggerButton(int index)
        {
            ActionNodeGraphInfo graphInfo = _buffDefine != null ?
                game.card.getOrCreateBuffEffectActionNodeGraphInfo(_cardPoolId, _buffDefine.id, _buffDefine.effectList.IndexOf(_effect), null, index) :
                game.card.getOrCreateCardEffectActionNodeGraphInfo(_cardPoolId, _cardDefine.id, _cardDefine.getEffectIndex(_effect), null, index);
            scene.actionNodeGraphCtrl.openTriggerGraph(_cardPoolId, _effect.triggerList[index], graphInfo);
        }
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
                scene.actionNodeGraphCtrl.openTriggerGraph(_cardPoolId, trigger, graphInfo);
            }
        }
        private void onPropItemValueChanged(int index, int itemIndex, object value)
        {
            EffectPropertyInfo[] propInfos = _effect.getPropInfos();
        }
        public void onClickPropItemButtonCallback(int index, int itemIndex)
        {
            EffectPropertyInfo[] propInfos = _effect.getPropInfos();
            if (propInfos[index].type == typeof(TriggerCollection))
            {
                TriggerCollection triggers = _effect.getProp<TriggerCollection>(propInfos[index].name);
                ActionNodeGraphInfo graphInfo = _buffDefine != null ?
                    game.card.getOrCreateBuffEffectActionNodeGraphInfo(_cardPoolId, _buffDefine.id, _buffDefine.effectList.IndexOf(_effect), propInfos[index].name, itemIndex) :
                    game.card.getOrCreateCardEffectActionNodeGraphInfo(_cardPoolId, _cardDefine.id, _cardDefine.getEffectIndex(_effect), propInfos[index].name, itemIndex);
                scene.actionNodeGraphCtrl.openTriggerGraph(_cardPoolId, triggers[itemIndex], graphInfo);
            }
        }
        public void onClickPropAddItemButtonCallback(int index)
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
            updatePropList();
        }
        public void onClickPropItemRemoveButtonCallback(int index, int itemIndex)
        {
            EffectPropertyInfo[] propInfos = _effect.getPropInfos();
            if (propInfos[index].type == typeof(TriggerCollection))
            {
                TriggerCollection triggers = _effect.getProp<TriggerCollection>(propInfos[index].name);
                triggers.RemoveAt(itemIndex);
            }
            updatePropList();
        }
        private async void onClickReturnButton()
        {
            await game.ui.returnPrevUI(() =>
            {
                _ui.gameObject.SetActive(false);
            });
        }
        #endregion
        private void updateTriggerList()
        {
            string[] triggerNames = new string[_effect.triggerList.Count];
            for (int i = 0; i < triggerNames.Length; i++)
            {
                triggerNames[i] = "触发器" + (i + 1);
            }
            _ui.updateTriggerList(triggerNames);
        }
        /// <summary>
        /// 更新属性信息UI
        /// </summary>
        private void updatePropList()
        {
            EffectPropertyInfo[] propInfos = _effect.getPropInfos();
            ActionNodeValueInfo[] valueInfos = new ActionNodeValueInfo[propInfos.Length];
            for (int i = 0; i < valueInfos.Length; i++)
            {
                valueInfos[i] = new ActionNodeValueInfo()
                {
                    name = propInfos[i].name,
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
        string[] _selectableRanges;
        string[] _effectTags = new string[]
        {
            KEYWORD.BATTLECRY,
            KEYWORD.DEATHRATTLE
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
            public int cardPoolId;
            public CardDefine cardDefine;
            public GeneratedBuffDefine buffDefine;
            public GeneratedEffect effect;
            public ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo;
        }
        #endregion
    }
}
