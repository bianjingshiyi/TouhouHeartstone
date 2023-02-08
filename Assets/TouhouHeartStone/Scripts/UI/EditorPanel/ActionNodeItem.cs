using System;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using Controllers;
using UnityEngine.EventSystems;
using MukioI18n;
using Game;
using UIHelper = BJSYGameCore.UI.UIHelper;

namespace UI
{
    public partial class ActionNodeItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region 公有方法
        public void switchToTriggerNode()
        {
            _buffFieldsRoot.gameObject.SetActive(false);
            _triggerFieldsRoot.gameObject.SetActive(true);
            _inputRoot.gameObject.SetActive(false);
            _valueRoot.gameObject.SetActive(false);
            _outputRoot.gameObject.SetActive(true);
            SetDirty();
        }
        public void switchToActionNode()
        {
            _buffFieldsRoot.gameObject.SetActive(false);
            _triggerFieldsRoot.gameObject.SetActive(false);
            _inputRoot.gameObject.SetActive(true);
            _valueRoot.gameObject.SetActive(true);
            _outputRoot.gameObject.SetActive(true);
            SetDirty();
        }
        public void setName(string name)
        {
            _nameText.text = name;
            SetDirty();
        }
        public void displayEventType(bool value)
        {
            _eventTypeDropdown.transform.parent.gameObject.SetActive(value);
            SetDirty();
        }
        public void setInputs((string name, Color color)[] inputViewData)
        {
            if (inputViewData == null || inputViewData.Length < 1)
            {
                _inputRoot.gameObject.SetActive(false);
            }
            else
            {
                _inputRoot.gameObject.SetActive(true);
                UIHelper.updateList(_inputRoot, _inputTemplate, _inputList, inputViewData.Length, (i, itemRect) =>
                {
                    ActionNodeItemInput item = itemRect.GetComponent<ActionNodeItemInput>();
                    item.setName(inputViewData[i].name);
                    item.setColor(inputViewData[i].color);
                }, itemRect =>
                {
                    ActionNodeItemInput item = itemRect.GetComponent<ActionNodeItemInput>();
                    item.onBeginDragNode += onBeginDragInputNodeCallback;
                    item.onDragNode += onDragInputNodeCallback;
                    item.onEndDragNode += onEndDragInputNodeCallback;
                    item.onClickNode += onClickInputNodeCallback;
                }, itemRect =>
                {
                    ActionNodeItemInput item = itemRect.GetComponent<ActionNodeItemInput>();
                    item.onBeginDragNode -= onBeginDragInputNodeCallback;
                    item.onDragNode -= onDragInputNodeCallback;
                    item.onEndDragNode -= onEndDragInputNodeCallback;
                    item.onClickNode -= onClickInputNodeCallback;
                });
            }
            SetDirty();
        }
        public void setConsts(ActionNodeValueInfo[] valueInfos)
        {
            if (valueInfos == null || valueInfos.Length < 1)
            {
                _valueNameRoot.gameObject.SetActive(false);
                _valueRoot.gameObject.SetActive(false);
            }
            else
            {
                _valueNameRoot.gameObject.SetActive(true);
                _valueRoot.gameObject.SetActive(true);
                UIHelper.updateList(_valueNameRoot, _valueNameTempltate, _valueNameList, valueInfos.Length, (i, itemRect) =>
                {
                    Text item = itemRect.GetComponentInChildren<Text>();
                    item.text = valueInfos[i].name;
                });
                UIHelper.updateList(_valueRoot, _valueTemplate, _valueList, valueInfos.Length, (i, itemRect) =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.setValueInfo(valueInfos[i]);
                }, itemRect =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.onValueChanged += onConstValueChangedCallback;
                    item.onClickButton += onConstValueButtonClickCallback;
                    item.onClickItemButton += onConstValueItemButtonClickCallback;
                }, itemRect =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.onValueChanged -= onConstValueChangedCallback;
                    item.onClickButton -= onConstValueButtonClickCallback;
                    item.onClickItemButton -= onConstValueItemButtonClickCallback;
                });
            }
            SetDirty();
        }
        public ValueInputField getConstInputField(int index)
        {
            return _valueList[index].GetComponent<ValueInputField>();
        }
        /// <summary>
        /// 初始化事件类型名称
        /// </summary>
        /// <param name="eventNames"></param>
        public void initEventTypes(string[] eventNames)
        {
            _eventTypeDropdown.ClearOptions();
            _eventTypeDropdown.AddOptions(new List<string>(eventNames));
        }


        /// <summary>
        /// 获取类型索引，用于输出颜色、
        /// 这里可以添加各类字符串类型来控制最终的IO输出的颜色。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetTypeIndex(string type)
        {
            var index = 0;
            switch (type)
            {
                case "Owner":
                case "Player":
                case "玩家":
                    index = 1;
                    break;
                case "Value":
                case "Card":
                case "卡牌":
                    index = 2;
                    break;
                case "目标卡牌":
                    index = 3;
                    break;
                case "Buff":
                    index = 4;
                    break;
                case "":
                    index = 5;
                    break; ;
            }

            return index;
        }
        /// <summary>
        /// 新的设置输出点颜色的函数
        /// </summary>
        /// <param name="outputInfos"></param>
        /// <param name="listColor">传入的颜色列表</param>
        public void setOutputs((string name, Color color)[] viewData)
        {
            if (viewData == null || viewData.Length < 1)
            {
                _outputRoot.gameObject.SetActive(false);
            }
            else
            {
                _outputRoot.gameObject.SetActive(true);
                UIHelper.updateList(_outputRoot, _outputTemplate, _outputList, viewData.Length, (i, itemRect) =>
                {
                    ActionNodeItemOutput item = itemRect.GetComponent<ActionNodeItemOutput>();
                    item.setName(viewData[i].name);
                    item.setColor(viewData[i].color);
                }, itemRect =>
                {
                    ActionNodeItemOutput item = itemRect.GetComponent<ActionNodeItemOutput>();
                    item.onBeginDragNode += onBeginDragOutputNodeCallback;
                    item.onDragNode += onDragOutputNodeCallback;
                    item.onEndDragNode += onEndDragOutputNodeCallback;
                    item.onClickNode += onClickOutputNodeCallback;
                }, itemRect =>
                {
                    ActionNodeItemOutput item = itemRect.GetComponent<ActionNodeItemOutput>();
                    item.onBeginDragNode -= onBeginDragOutputNodeCallback;
                    item.onDragNode -= onDragOutputNodeCallback;
                    item.onEndDragNode -= onEndDragOutputNodeCallback;
                    item.onClickNode -= onClickOutputNodeCallback;
                });
            }
            SetDirty();
        }

        /// <summary>
        /// 保留旧的函数方法，用于buff之类的创建
        /// </summary>
        /// <param name="outputNames"></param>
        public void setOutputs(string[] outputNames)
        {
            if (outputNames == null || outputNames.Length < 1)
            {
                _outputRoot.gameObject.SetActive(false);
            }
            else
            {
                _outputRoot.gameObject.SetActive(true);
                UIHelper.updateList(_outputRoot, _outputTemplate, _outputList, outputNames.Length, (i, itemRect) =>
                {
                    ActionNodeItemOutput item = itemRect.GetComponent<ActionNodeItemOutput>();
                    item.setName(outputNames[i]);
                }, itemRect =>
                {
                    ActionNodeItemOutput item = itemRect.GetComponent<ActionNodeItemOutput>();
                    item.onBeginDragNode += onBeginDragOutputNodeCallback;
                    item.onDragNode += onDragOutputNodeCallback;
                    item.onEndDragNode += onEndDragOutputNodeCallback;
                    item.onClickNode += onClickOutputNodeCallback;
                }, itemRect =>
                {
                    ActionNodeItemOutput item = itemRect.GetComponent<ActionNodeItemOutput>();
                    item.onBeginDragNode -= onBeginDragOutputNodeCallback;
                    item.onDragNode -= onDragOutputNodeCallback;
                    item.onEndDragNode -= onEndDragOutputNodeCallback;
                    item.onClickNode -= onClickOutputNodeCallback;
                });
            }
            SetDirty();
        }
        public void setTrigger(string[] targetTypes, int eventIndex, (int typeIndex, string name, string errorTip)[] targetCheckers)
        {
            _eventTypeDropdown.SetValueWithoutNotify(eventIndex);
            updateTargetCheckerList(targetTypes, targetCheckers);
        }

        public void setObsoleteMsg(string message)
        {
            obsoleteMessage = message;
            if (message == null)
            {
                // normal
                _nameBackground.color = _normalTitleColor;
                _warningObj.gameObject.SetActive(false);
            }
            else
            {
                // obsolete
                _nameBackground.color = _warningTitleColor;
                _warningObj.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 设置帮助按钮对应的Key
        /// </summary>
        /// <param name="key"></param>
        public void setDocumentKey(string key)
        {
            documentKey = key;
            _helpBtn.gameObject.SetActive(key != null);
        }

        public void updateTargetCheckerList(string[] targetTypes, (int typeIndex, string name, string errorTip)[] targetCheckers)
        {
            UIHelper.updateList(_targetCheckerRoot, _targetCheckerTemplate, _targetCheckerList, targetCheckers.Length, (i, itemRect) =>
            {
                ActionNodeItemTriggerTargetChecker item = itemRect.GetComponent<ActionNodeItemTriggerTargetChecker>();
                item.setTargetTypes(targetTypes);
                item.setTargetType(targetCheckers[i].typeIndex);
                item.setName(targetCheckers[i].name);
                item.setErrorTip(targetCheckers[i].errorTip);
            }, itemRect =>
            {
                ActionNodeItemTriggerTargetChecker item = itemRect.GetComponent<ActionNodeItemTriggerTargetChecker>();
                item.onTargetDropdownValueChanged += onTargetCheckerTypeValueChangedCallback;
                item.onClickEditButton += onClickEditTargetCheckerButtonCallback;
                item.onClickRemoveButton += onClickRemoveTargetCheckerButtonCallback;
                item.onErrorTipInputFieldValueChanged += onTargetCheckerErrorTipValueChangedCallback;
            }, itemRect =>
            {
                ActionNodeItemTriggerTargetChecker item = itemRect.GetComponent<ActionNodeItemTriggerTargetChecker>();
                item.onTargetDropdownValueChanged -= onTargetCheckerTypeValueChangedCallback;
                item.onClickEditButton -= onClickEditTargetCheckerButtonCallback;
                item.onClickRemoveButton -= onClickRemoveTargetCheckerButtonCallback;
                item.onErrorTipInputFieldValueChanged -= onTargetCheckerErrorTipValueChangedCallback;
            });
            SetDirty();
        }
        public int indexOfInput(ActionNodeItemInput input)
        {
            return _inputList.IndexOf(input.transform as RectTransform);
        }
        public int indexOfOutput(ActionNodeItemOutput output)
        {
            return _outputList.IndexOf(output.transform as RectTransform);
        }
        public Vector2 getInputWorldPosition(int index)
        {
            return _inputList[index].GetComponent<ActionNodeItemInput>().getNodeWorldPosition();
        }
        public Vector2 getOutputWorldPosition(int index)
        {
            return _outputList[index].GetComponent<ActionNodeItemOutput>().getNodeWorldPosition();
        }
        public ActionNodeItemInput getInput(int index)
        {
            return _inputList[index].GetComponent<ActionNodeItemInput>();
        }
        public ActionNodeItemOutput getOutput(int index)
        {
            return _outputList[index].GetComponent<ActionNodeItemOutput>();
        }
        /// <summary>
        /// 将该节点项设置为不洁净。会在本帧结束时调用onUpdateTransform回调。
        /// </summary>
        public void SetDirty()
        {
            _dirty = true;
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_inputTemplate.transform.parent == _inputRoot)
            {
                _inputTemplate.gameObject.SetActive(false);
            }
            if (_valueNameTempltate.transform.parent == _valueNameRoot)
            {
                _valueNameTempltate.gameObject.SetActive(false);
            }
            if (_valueTemplate.transform.parent == _valueRoot)
            {
                _valueTemplate.gameObject.SetActive(false);
            }
            if (_outputTemplate.transform.parent == _outputRoot)
            {
                _outputTemplate.gameObject.SetActive(false);
            }
            if (_targetCheckerTemplate.transform.parent == _targetCheckerRoot)
            {
                _targetCheckerTemplate.gameObject.SetActive(false);
            }
            // _eventNameInputField.onEndEdit.AddListener(onEndEditEventNameInputField);
            _eventTypeDropdown.onValueChanged.AddListener(onChangeSelectedEventIndex);
            _editConditionButton.onClick.AddListener(onClickEditConditionButtonCallback);
            _addTargetCheckerButton.onClick.AddListener(onClickAddTargetCheckerButtonCallback);
            _addPropModifierButton.onClick.AddListener(onClickAddPropModifierButtonCallback);
            _effectList.onClickAddEffect += onClickPropertyModifierAddEffectMenuCallback;
            _effectList.onClickItemRemoveButton += onClickPropertyModifierEffectRemoveButtonCallback;
            _effectList.onClickItemEditButton += onClickPropertyModifierEffectEditButtonCallback;
            _warningObj.onClick.AddListener(onClickObsoleteBtn);
            _helpBtn.onClick.AddListener(onClickHelpBtn);
            //增益
            onAwake_Buff();
        }

        private void LateUpdate()
        {
            if (_dirty)
            {
                _dirty = false;
                onUpdateTransform?.Invoke(this);
            }
        }
        #endregion
        #region 事件回调
        private void onBeginDragInputNodeCallback(ActionNodeItemInput item, BaseEventData eventData)
        {
            onBeginDragInputNode?.Invoke(this, item, eventData);
        }
        private void onDragInputNodeCallback(ActionNodeItemInput item, BaseEventData eventData)
        {
            onDragInputNode?.Invoke(this, item, eventData);
        }
        private void onEndDragInputNodeCallback(ActionNodeItemInput item, BaseEventData eventData)
        {
            onEndDragInputNode?.Invoke(this, item, eventData);
        }
        private void onClickInputNodeCallback(ActionNodeItemInput item, BaseEventData eventData)
        {
            onClickInputNode?.Invoke(this, item, eventData);
        }
        private void onBeginDragOutputNodeCallback(ActionNodeItemOutput item, BaseEventData eventData)
        {
            onBeginDragOutputNode?.Invoke(this, item, eventData);
        }
        private void onDragOutputNodeCallback(ActionNodeItemOutput item, BaseEventData eventData)
        {
            onDragOutputNode?.Invoke(this, item, eventData);
        }
        private void onEndDragOutputNodeCallback(ActionNodeItemOutput item, BaseEventData eventData)
        {
            onEndDragOutputNode?.Invoke(this, item, eventData);
        }
        private void onClickOutputNodeCallback(ActionNodeItemOutput item, BaseEventData eventData)
        {
            onClickOutputNode?.Invoke(this, item, eventData);
        }
        private void onConstValueChangedCallback(ValueInputField item, object value)
        {
            onConstValueChanged?.Invoke(this, _valueList.IndexOf(item.transform as RectTransform), value);
        }
        private void onConstValueButtonClickCallback(ValueInputField item)
        {
            onClickConstValueButton?.Invoke(this, _valueList.IndexOf(item.transform as RectTransform));
        }
        private void onConstValueItemButtonClickCallback(ValueInputField item, int index)
        {
            onClickConstValueItemButton?.Invoke(this, _valueList.IndexOf(item.transform as RectTransform), index);
        }
        private void onChangeSelectedEventIndex(int eventIndex)
        {
            onEventTypeDropdownValueChanged?.Invoke(this, eventIndex);
        }
        private void onClickEditConditionButtonCallback()
        {
            onClickEditConditionButton?.Invoke(this);
        }
        private void onClickAddTargetCheckerButtonCallback()
        {
            onClickAddTargetCheckerButton?.Invoke(this);
        }
        private void onTargetCheckerTypeValueChangedCallback(ActionNodeItemTriggerTargetChecker item, int value)
        {
            onTargetCheckerTypeValueChanged?.Invoke(this, _targetCheckerList.IndexOf(item.transform as RectTransform), value);
        }
        private void onClickEditTargetCheckerButtonCallback(ActionNodeItemTriggerTargetChecker item)
        {
            onClickEditTargetCheckerButton?.Invoke(this, _targetCheckerList.IndexOf(item.transform as RectTransform));
        }
        private void onClickRemoveTargetCheckerButtonCallback(ActionNodeItemTriggerTargetChecker item)
        {
            onClickRemoveTargetCheckerButton?.Invoke(this, _targetCheckerList.IndexOf(item.transform as RectTransform));
        }
        private void onTargetCheckerErrorTipValueChangedCallback(ActionNodeItemTriggerTargetChecker item, string value)
        {
            onTargetCheckerErrorTipValueChanged?.Invoke(this, _targetCheckerList.IndexOf(item.transform as RectTransform), value);
        }

        [TranslateMsg]
        const string TITLE_NODE_OBSOLETE = "过时的节点";

        [TranslateMsg("参数0为节点自带的消息")]
        const string MESSAGE_NODE_OBSOLETE = "此节点已被弃用，很可能在之后的版本中删除。\n{0}";

        private void onClickObsoleteBtn()
        {
            var ctrl = gameObject.GetComponentInParent<EffectEditorCtrl>();
            ctrl.game.ui.dialog?.ShowConfirm(ctrl.game.lang._(TITLE_NODE_OBSOLETE), ctrl.game.lang._(MESSAGE_NODE_OBSOLETE, obsoleteMessage));
        }

        private void onClickHelpBtn()
        {
            onClickHelpButton?.Invoke(documentKey);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            eventData.Use();
            onPointerDown?.Invoke(this, eventData);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            eventData.Use();
            onPointerUp?.Invoke(this, eventData);
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            eventData.Use();
            onBeginDrag?.Invoke(this, eventData);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            eventData.Use();
            onDrag?.Invoke(this, eventData);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            eventData.Use();
            onEndDrag?.Invoke(this, eventData);
        }
        #endregion
        #endregion
        #region 事件
        public event Action<ActionNodeItem, PointerEventData> onPointerDown;
        public event Action<ActionNodeItem, PointerEventData> onPointerUp;
        public event Action<ActionNodeItem, PointerEventData> onBeginDrag;
        public event Action<ActionNodeItem, PointerEventData> onDrag;
        public event Action<ActionNodeItem, PointerEventData> onEndDrag;
        public event Action<ActionNodeItem, ActionNodeItemInput, BaseEventData> onBeginDragInputNode;
        public event Action<ActionNodeItem, ActionNodeItemInput, BaseEventData> onDragInputNode;
        public event Action<ActionNodeItem, ActionNodeItemInput, BaseEventData> onEndDragInputNode;
        public event Action<ActionNodeItem, ActionNodeItemInput, BaseEventData> onClickInputNode;
        public event Action<ActionNodeItem, ActionNodeItemOutput, BaseEventData> onBeginDragOutputNode;
        public event Action<ActionNodeItem, ActionNodeItemOutput, BaseEventData> onDragOutputNode;
        public event Action<ActionNodeItem, ActionNodeItemOutput, BaseEventData> onEndDragOutputNode;
        public event Action<ActionNodeItem, ActionNodeItemOutput, BaseEventData> onClickOutputNode;
        public event Action<ActionNodeItem, int, object> onConstValueChanged;
        public event Action<ActionNodeItem, int> onClickConstValueButton;
        public event Action<ActionNodeItem, int, int> onClickConstValueItemButton;
        public event Action<ActionNodeItem, string> onEventNameChanged;
        public event Action<ActionNodeItem, int> onEventTypeDropdownValueChanged;
        public event Action<ActionNodeItem> onClickEditConditionButton;
        public event Action<ActionNodeItem> onClickAddTargetCheckerButton;
        public event Action<ActionNodeItem, int, int> onTargetCheckerTypeValueChanged;
        public event Action<ActionNodeItem, int> onClickEditTargetCheckerButton;
        public event Action<ActionNodeItem, int> onClickRemoveTargetCheckerButton;
        public event Action<ActionNodeItem, int, string> onTargetCheckerErrorTipValueChanged;
        public event Action<ActionNodeItem> onUpdateTransform;
        public event Action<string> onClickHelpButton;
        #endregion
        #region 属性字段
        public float posX
        {
            get { return (transform as RectTransform).anchoredPosition.x; }
        }
        public float posY
        {
            get { return (transform as RectTransform).anchoredPosition.y; }
        }

        string obsoleteMessage = "";
        string documentKey = "";
        private bool _dirty;

        [SerializeField]
        Text _nameText;
        [SerializeField]
        Image _nameBackground;
        [SerializeField]
        Button _warningObj;
        [SerializeField]
        Button _helpBtn;
        [SerializeField]
        RectTransform _inputRoot;
        [SerializeField]
        RectTransform _inputTemplate;
        [SerializeField]
        List<RectTransform> _inputList = new List<RectTransform>();
        [SerializeField]
        RectTransform _valueNameRoot;
        [SerializeField]
        RectTransform _valueNameTempltate;
        [SerializeField]
        List<RectTransform> _valueNameList = new List<RectTransform>();
        [SerializeField]
        RectTransform _valueRoot;
        [SerializeField]
        RectTransform _valueTemplate;
        [SerializeField]
        List<RectTransform> _valueList = new List<RectTransform>();
        [SerializeField]
        RectTransform _outputRoot;
        [SerializeField]
        RectTransform _outputTemplate;
        [SerializeField]
        List<RectTransform> _outputList = new List<RectTransform>();
        [SerializeField]
        RectTransform _triggerFieldsRoot;
        [SerializeField]
        Dropdown _eventTypeDropdown;
        [SerializeField]
        Button _editConditionButton;
        [SerializeField]
        RectTransform _targetCheckerRoot;
        [SerializeField]
        RectTransform _targetCheckerTemplate;
        [SerializeField]
        List<RectTransform> _targetCheckerList = new List<RectTransform>();
        [SerializeField]
        Button _addTargetCheckerButton;

        [SerializeField]
        Color _normalTitleColor;
        [SerializeField]
        Color _warningTitleColor;
        #endregion
    }
}