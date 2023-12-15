using System;
using Game;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class ActionNodeItemInput : ActionNodeItemKnob
    {
        #region 公有方法
        public void initEventReferences(string[] eventNames, EventReference[] eventValues)
        {
            _defaultValueInputField.initEventReferences(eventNames, eventValues);
        }
        public void initPileNames(string[] pileNames, string[] pileValues)
        {
            _defaultValueInputField.initPileNames(pileNames, pileValues);
        }
        public void initEffectTagNames(string[] effectTagNames, string[] effectTagValues)
        {
            _defaultValueInputField.initEffectTagNames(effectTagNames, effectTagValues);
        }
        public void initEventVarNames(string[] eventVarNameNames, string[] eventVarNameValues)
        {
            _defaultValueInputField.initEventVarNames(eventVarNameNames, eventVarNameValues);
        }
        public void initCardProperties(CardPropertyNameItem[] cardProperties)
        {
            _defaultValueInputField.initCardProperties(cardProperties);
        }
        public void setReadonly(bool isReadonly)
        {
            _defaultValueInputField.setReadonly(isReadonly);
        }
        public void setDefaultValue(ActionNodeValueInfo valueInfo)
        {
            _defaultValueInputField.setValueInfo(valueInfo);
        }
        public void setDefaultValueActive(bool value)
        {
            _defaultValueInputFieldObj.SetActive(value);
        }
        #endregion

        #region 私有方法

        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _defaultValueInputField.onValueChanged += onDefaultValueChangedCallback;
            _defaultValueInputField.onClickButton += onDefaultValueButtonClickCallback;
            _defaultValueInputField.onClickCardButton += onDefaultValueCardButtonClickCallback;
            _defaultValueInputField.onClickCardEditButton += onDefaultValueCardEditButtonClickCallback;
            _defaultValueInputField.onClickItemButton += onDefaultValueItemButtonClickCallback;
            _defaultValueInputField.onClickItemCardButton += onDefaultValueItemCardButtonClickCallback;
            _defaultValueInputField.onClickItemCardEditButton += onDefaultValueItemCardEditButtonClickCallback;
        }
        #endregion

        #region 事件回调
        protected override void onBeginDragNodeCallback(BaseEventData eventData)
        {
            onBeginDragNode?.Invoke(this, eventData);
        }
        protected override void onDragNodeCallback(BaseEventData eventData)
        {
            onDragNode?.Invoke(this, eventData);
        }
        protected override void onEndDragNodeCallback(BaseEventData eventData)
        {
            onEndDragNode?.Invoke(this, eventData);
        }
        protected override void onClickNodeCallback(BaseEventData eventData)
        {
            onClickNode?.Invoke(this, eventData);
        }
        #region 默认值
        private void onDefaultValueChangedCallback(ValueInputField item, object value)
        {
            onDefaultValueChanged?.Invoke(this, value);
        }
        private void onDefaultValueButtonClickCallback(ValueInputField item)
        {
            onClickDefaultValueButton?.Invoke(this);
        }
        private void onDefaultValueCardButtonClickCallback(ValueInputField item)
        {
            onClickDefaultValueCardButton?.Invoke(this);
        }
        private void onDefaultValueCardEditButtonClickCallback(ValueInputField item)
        {
            onClickDefaultValueCardEditButton?.Invoke(this);
        }
        private void onDefaultValueItemButtonClickCallback(ValueInputField item, int index)
        {
            onClickDefaultValueItemButton?.Invoke(this, index);
        }
        private void onDefaultValueItemCardButtonClickCallback(ValueInputField item, int index)
        {
            onClickDefaultValueItemCardButton?.Invoke(this, index);
        }
        private void onDefaultValueItemCardEditButtonClickCallback(ValueInputField item, int index)
        {
            onClickDefaultValueItemCardEditButton?.Invoke(this, index);
        }
        #endregion

        #endregion

        #endregion

        #region 事件

        public event Action<ActionNodeItemInput, BaseEventData> onBeginDragNode;
        public event Action<ActionNodeItemInput, BaseEventData> onDragNode;
        public event Action<ActionNodeItemInput, BaseEventData> onEndDragNode;
        public event Action<ActionNodeItemInput, BaseEventData> onClickNode;

        #region 默认值

        public event Action<ActionNodeItemInput, object> onDefaultValueChanged;
        public event Action<ActionNodeItemInput> onClickDefaultValueButton;
        public event Action<ActionNodeItemInput> onClickDefaultValueCardButton;
        public event Action<ActionNodeItemInput> onClickDefaultValueCardEditButton;
        public event Action<ActionNodeItemInput, int> onClickDefaultValueItemButton;
        public event Action<ActionNodeItemInput, int> onClickDefaultValueItemCardButton;
        public event Action<ActionNodeItemInput, int> onClickDefaultValueItemCardEditButton;

        #endregion

        #endregion

        #region 属性字段
        public ValueInputField defaultValueInputField => _defaultValueInputField;
        [Header("默认值")]
        [SerializeField]
        private GameObject _defaultValueInputFieldObj;
        [SerializeField]
        private ValueInputField _defaultValueInputField;
        #endregion
    }
}