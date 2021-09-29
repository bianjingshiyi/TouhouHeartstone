using System;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UI
{
    public class ActionNodeItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public void switchToTriggerNode()
        {
            _triggerFieldsRoot.gameObject.SetActive(true);
            _inputRoot.gameObject.SetActive(false);
            _valueRoot.gameObject.SetActive(false);
            _outputRoot.gameObject.SetActive(true);
        }
        public void switchToActionNode()
        {
            _triggerFieldsRoot.gameObject.SetActive(false);
            _inputRoot.gameObject.SetActive(true);
            _valueRoot.gameObject.SetActive(true);
            _outputRoot.gameObject.SetActive(true);
        }
        public void setName(string name)
        {
            _nameText.text = name;
        }
        public void setInputs(string[] inputNames)
        {
            if (inputNames == null || inputNames.Length < 1)
            {
                _inputRoot.gameObject.SetActive(false);
            }
            else
            {
                _inputRoot.gameObject.SetActive(true);
                UIHelper.updateList(_inputRoot, _inputTemplate, _inputList, inputNames.Length, (i, itemRect) =>
                {
                    ActionNodeItemInput item = itemRect.GetComponent<ActionNodeItemInput>();
                    item.setName(inputNames[i]);
                }, itemRect =>
                {
                    ActionNodeItemInput item = itemRect.GetComponent<ActionNodeItemInput>();
                    item.onBeginDragNode += onBeginDragInputNodeCallback;
                    item.onDragNode += onDragInputNodeCallback;
                    item.onEndDragNode += onEndDragInputNodeCallback;
                }, itemRect =>
                {
                    ActionNodeItemInput item = itemRect.GetComponent<ActionNodeItemInput>();
                    item.onBeginDragNode -= onBeginDragInputNodeCallback;
                    item.onDragNode -= onDragInputNodeCallback;
                    item.onEndDragNode -= onEndDragInputNodeCallback;
                });
            }
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
                    ActionNodeItemConstValue item = itemRect.GetComponent<ActionNodeItemConstValue>();
                    item.setValueInfo(valueInfos[i]);
                }, itemRect =>
                {
                    ActionNodeItemConstValue item = itemRect.GetComponent<ActionNodeItemConstValue>();
                    item.onValueChanged += onConstValueChangedCallback;
                }, itemRect =>
                {
                    ActionNodeItemConstValue item = itemRect.GetComponent<ActionNodeItemConstValue>();
                    item.onValueChanged -= onConstValueChangedCallback;
                });
            }
        }
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
                }, itemRect =>
                {
                    ActionNodeItemOutput item = itemRect.GetComponent<ActionNodeItemOutput>();
                    item.onBeginDragNode -= onBeginDragOutputNodeCallback;
                    item.onDragNode -= onDragOutputNodeCallback;
                    item.onEndDragNode -= onEndDragOutputNodeCallback;
                });
            }
        }
        public void setTrigger(string[] targetTypes, string eventName, (int typeIndex, string name, string errorTip)[] targetCheckers)
        {
            _eventNameInputField.SetTextWithoutNotify(eventName);
            updateTargetCheckerList(targetTypes, targetCheckers);
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
            _eventNameInputField.onEndEdit.AddListener(onEndEditEventNameInputField);
            _editConditionButton.onClick.AddListener(onClickEditConditionButtonCallback);
            _addTargetCheckerButton.onClick.AddListener(onClickAddTargetCheckerButtonCallback);
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
        private void onConstValueChangedCallback(ActionNodeItemConstValue item, object value)
        {
            onConstValueChanged?.Invoke(this, _valueList.IndexOf(item.transform as RectTransform), value);
        }
        private void onEndEditEventNameInputField(string eventName)
        {
            onEventNameChanged?.Invoke(this, eventName);
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
        public event Action<ActionNodeItem, ActionNodeItemOutput, BaseEventData> onBeginDragOutputNode;
        public event Action<ActionNodeItem, ActionNodeItemOutput, BaseEventData> onDragOutputNode;
        public event Action<ActionNodeItem, ActionNodeItemOutput, BaseEventData> onEndDragOutputNode;
        public event Action<ActionNodeItem, int, object> onConstValueChanged;
        public event Action<ActionNodeItem, string> onEventNameChanged;
        public event Action<ActionNodeItem> onClickEditConditionButton;
        public event Action<ActionNodeItem> onClickAddTargetCheckerButton;
        public event Action<ActionNodeItem, int, int> onTargetCheckerTypeValueChanged;
        public event Action<ActionNodeItem, int> onClickEditTargetCheckerButton;
        public event Action<ActionNodeItem, int> onClickRemoveTargetCheckerButton;
        public event Action<ActionNodeItem, int, string> onTargetCheckerErrorTipValueChanged;
        #endregion
        public float posX
        {
            get { throw new NotImplementedException(); }
        }
        public float posY
        {
            get { throw new NotImplementedException(); }
        }
        [SerializeField]
        Text _nameText;
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
        InputField _eventNameInputField;
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
    }
}