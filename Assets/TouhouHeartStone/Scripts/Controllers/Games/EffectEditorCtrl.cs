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
        public Task displayEffect(int cardPoolId, CardDefine cardDefine, GeneratedEffect effect, ActionNodeGraphInfo graphInfo)
        {
            return game.ui.displayUIWithReturn(this, new DisplayInfo(cardPoolId, cardDefine, effect, graphInfo, null));
        }
        public async Task display(UIDisplayInfo displayInfo)
        {
            if (displayInfo is DisplayInfo info)
            {
                _cardPoolId = info.cardPoolId;
                _cardDefine = info.cardDefine;
                _effect = info.effect;
                graphInfo = info.graphInfo;
                _ui.gameObject.SetActive(true);
                updatePropList();
                //bool[] activeRangeValue = new bool[_selectableRanges.Length];
                //for (int i = 0; i < activeRangeValue.Length; i++)
                //{
                //    activeRangeValue[i] = _effect.pileList.Contains(_selectableRanges[i]);
                //}
                //_ui.setActiveRange(activeRangeValue);
                //updateTriggerList();
                if (info.actionNodeGraphDisplayInfo != null)
                    await scene.actionNodeGraphCtrl.display(info.actionNodeGraphDisplayInfo);
                else
                    await scene.actionNodeGraphCtrl.hide();
            }
        }
        public UIDisplayInfo getCurrentDisplayInfo()
        {
            return new DisplayInfo(_cardPoolId, _cardDefine, _effect, graphInfo,
                scene.actionNodeGraphCtrl.getCurrentDisplayInfo() as ActionNodeGraphCtrl.DisplayInfo);
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
            _ui.setActiveRanges(activeRanges, _selectableRanges);
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
            scene.actionNodeGraphCtrl.openOnEnableActionGraph(_cardPoolId, _cardDefine, _effect);
        }
        private void onClickEditDisableActionButton()
        {
            scene.actionNodeGraphCtrl.openOnDisableActionGraph(_cardPoolId, _cardDefine, _effect);
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
            scene.actionNodeGraphCtrl.openTriggerGraph(_cardPoolId, _cardDefine, _effect.triggerList[index]);
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
                scene.actionNodeGraphCtrl.openActionGraph(_cardPoolId, _cardDefine, _effect, propInfos[index].name);
            }
            else if (propInfos[index].type == typeof(ActionValueRef))
            {
                ActionValueRef valueRef = _effect.getProp<ActionValueRef>(propInfos[index].name);
                if (valueRef == null)
                {
                    valueRef = new ActionValueRef();
                    _effect.setProp(propInfos[index].name, valueRef);
                }
                scene.actionNodeGraphCtrl.openActionValueRefGraph(_cardPoolId, _cardDefine, valueRef);
            }
            else if (propInfos[index].type == typeof(TriggerGraph))
            {
                TriggerGraph trigger = _effect.getProp<TriggerGraph>(propInfos[index].name);
                if (trigger == null)
                {
                    trigger = new TriggerGraph();
                    _effect.setProp(propInfos[index].name, trigger);
                }
                scene.actionNodeGraphCtrl.openTriggerGraph(_cardPoolId, _cardDefine, trigger);
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
                scene.actionNodeGraphCtrl.openTriggerGraph(_cardPoolId, _cardDefine, triggers[itemIndex]);
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
            else if (type == typeof(TagCollection))
                return ActionNodeValueType.tagCollection;
            else if (type == typeof(ActionNode))
                return ActionNodeValueType.actionNode;
            else if (type == typeof(ActionValueRef))
                return ActionNodeValueType.actionValueRef;
            else if (type == typeof(TriggerGraph))
                return ActionNodeValueType.trigger;
            else if (type == typeof(TriggerCollection))
                return ActionNodeValueType.triggerCollection;
            else
                throw new NotImplementedException();
        }
        #endregion
        #region 属性字段
        public ActionNodeGraphInfo graphInfo { get; private set; }
        [SerializeField]
        EffectEditorPanel _ui;
        [Obsolete("使用game作为替代")]
        public IAppManager app { get; }
        int _cardPoolId;
        CardDefine _cardDefine;
        GeneratedEffect _effect;
        string[] _selectableRanges;
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
        #endregion
        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(int cardPoolId, CardDefine cardDefine, GeneratedEffect effect, ActionNodeGraphInfo graphInfo, ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo)
            {
                this.cardPoolId = cardPoolId;
                this.cardDefine = cardDefine;
                this.effect = effect;
                this.graphInfo = graphInfo;
                this.actionNodeGraphDisplayInfo = actionNodeGraphDisplayInfo;
            }
            public int cardPoolId;
            public CardDefine cardDefine;
            public GeneratedEffect effect;
            public ActionNodeGraphInfo graphInfo;
            public ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo;
        }
        #endregion
    }
}
