using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class ActionNodeItemInput : ActionNodeItemKnob
    {
        #region 公有方法
        public void initDefaultValueProperties(ValueInputFieldProperties properties)
        {
            _defaultValueInputField.initProperties(properties);
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
            _defaultValueInputField.onClickItemButton += onDefaultValueItemButtonClickCallback;
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
        private void onDefaultValueButtonClickCallback(ValueInputField item, ValueInputFieldButton type)
        {
            onClickDefaultValueButton?.Invoke(this, type);
        }
        private void onDefaultValueItemButtonClickCallback(ValueInputField item, int index, ValueInputFieldButton type)
        {
            onClickDefaultValueItemButton?.Invoke(this, index, type);
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
        public event Action<ActionNodeItemInput, ValueInputFieldButton> onClickDefaultValueButton;
        public event Action<ActionNodeItemInput, int, ValueInputFieldButton> onClickDefaultValueItemButton;

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