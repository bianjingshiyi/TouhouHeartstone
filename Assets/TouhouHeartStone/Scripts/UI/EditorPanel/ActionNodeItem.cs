using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIHelper = BJSYGameCore.UI.UIHelper;

namespace UI
{
    public partial class ActionNodeItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region 公有方法
        public void switchToActiveEntryNode()
        {
            _triggerFieldsRoot.gameObject.SetActive(true);
            _effectPropertyFields.SetActive(true);
            _inputRoot.gameObject.SetActive(true);
            _outputRoot.gameObject.SetActive(true);
            SetDirty();
        }
        public void switchToActionNode()
        {
            _triggerFieldsRoot.gameObject.SetActive(false);
            _effectPropertyFields.SetActive(false);
            _inputRoot.gameObject.SetActive(true);
            _outputRoot.gameObject.SetActive(true);
            SetDirty();
        }
        public void switchToEventEntryNode()
        {
            _triggerFieldsRoot.gameObject.SetActive(false);
            _effectPropertyFields.SetActive(true);
            _inputRoot.gameObject.SetActive(true);
            _outputRoot.gameObject.SetActive(true);
            SetDirty();
        }
        public void setName(string name)
        {
            _nameText.text = name;
            SetDirty();
        }
        public void setInputs(InputPortInfo[] inputViewData)
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
                    var inputInfo = inputViewData[i];
                    var portInfo = inputInfo.portInfo;
                    var defaultValue = inputInfo.defaultValueInfo;
                    var showPort = inputInfo.showPort;
                    var showDefaultValue = inputInfo.showDefaultValue;

                    item.setName(portInfo.name);
                    item.setColor(portInfo.color);
                    item.isControl = portInfo.isControl;

                    item.setPortActive(showPort);
                    item.setDefaultValueActive(showDefaultValue);
                    if (showDefaultValue)
                    {
                        item.setDefaultValue(defaultValue);
                    }
                }, itemRect =>
                {
                    ActionNodeItemInput item = itemRect.GetComponent<ActionNodeItemInput>();
                    item.onBeginDragNode += onBeginDragInputNodeCallback;
                    item.onDragNode += onDragInputNodeCallback;
                    item.onEndDragNode += onEndDragInputNodeCallback;
                    item.onClickNode += onClickInputNodeCallback;
                    item.onDefaultValueChanged += onDefaultValueChangedCallback;
                    item.onClickDefaultValueButton += onDefaultValueButtonClickCallback;
                    item.onClickDefaultValueItemButton += onDefaultValueItemButtonClickCallback;
                }, itemRect =>
                {
                    ActionNodeItemInput item = itemRect.GetComponent<ActionNodeItemInput>();
                    item.onBeginDragNode -= onBeginDragInputNodeCallback;
                    item.onDragNode -= onDragInputNodeCallback;
                    item.onEndDragNode -= onEndDragInputNodeCallback;
                    item.onClickNode -= onClickInputNodeCallback;
                    item.onDefaultValueChanged -= onDefaultValueChangedCallback;
                    item.onClickDefaultValueButton -= onDefaultValueButtonClickCallback;
                    item.onClickDefaultValueItemButton -= onDefaultValueItemButtonClickCallback;
                }, rebuild: true);
            }
            SetDirty();
        }
        public ValueInputField getInputDefaultValueInputField(int index)
        {
            return _inputList[index].GetComponent<ActionNodeItemInput>()?.defaultValueInputField;
        }
        /// <summary>
        /// 初始化事件名称
        /// </summary>
        /// <param name="eventNames"></param>
        public void initEventReferences(string[] eventNames, EventReference[] eventValues)
        {
            ActionNodeItemInput item = _inputTemplate.GetComponent<ActionNodeItemInput>();
            item.initEventReferences(eventNames, eventValues);
        }
        /// <summary>
        /// 初始化事件类型名称
        /// </summary>
        /// <param name="eventNames"></param>
        public void initPileNames(string[] pileNames, string[] pileValues)
        {
            ActionNodeItemInput item = _inputTemplate.GetComponent<ActionNodeItemInput>();
            item.initPileNames(pileNames, pileValues);

            PropertyItemField propItem = _effectPropertyItemTemplate.GetComponent<PropertyItemField>();
            propItem.setPiles(pileNames, pileValues);
        }
        /// <summary>
        /// 初始化效果标签
        /// </summary>
        /// <param name="eventNames"></param>
        public void initEffectTags(string[] effectTagNames, string[] effectTagValues)
        {
            ActionNodeItemInput item = _inputTemplate.GetComponent<ActionNodeItemInput>();
            item.initEffectTagNames(effectTagNames, effectTagValues);
        }
        /// <summary>
        /// 设置事件变量名称列表
        /// </summary>
        /// <param name="eventNames"></param>
        public void setEventVarNames(string[] eventVarNameNames, string[] eventVarNameValues)
        {
            ActionNodeItemInput item = _inputTemplate.GetComponent<ActionNodeItemInput>();
            item.initEventVarNames(eventVarNameNames, eventVarNameValues);
            foreach (var valueRect in _inputList)
            {
                var valueItem = valueRect.GetComponent<ActionNodeItemInput>();
                valueItem.initEventVarNames(eventVarNameNames, eventVarNameValues);
            }
        }
        public void initKeywords(string[] keywordNames, string[] keywordValues)
        {
            PropertyItemField propItem = _effectPropertyItemTemplate.GetComponent<PropertyItemField>();
            propItem.setKeywords(keywordNames, keywordValues);
        }
        /// <summary>
        /// 初始化卡牌属性名。
        /// </summary>
        /// <param name="cardProperties"></param>
        public void initCardProperties(CardPropertyNameItem[] cardProperties)
        {
            ActionNodeItemInput item = _inputTemplate.GetComponent<ActionNodeItemInput>();
            item.initCardProperties(cardProperties);
        }
        /// <summary>
        /// 初始化增益名称
        /// </summary>
        /// <param name="buffNames"></param>
        public void initBuffReferences(string[] buffNames, BuffReference[] buffValues)
        {
            ActionNodeItemInput item = _inputTemplate.GetComponent<ActionNodeItemInput>();
            item.initBuffReferences(buffNames, buffValues);
            foreach (var valueRect in _inputList)
            {
                var valueItem = valueRect.GetComponent<ActionNodeItemInput>();
                valueItem.initBuffReferences(buffNames, buffValues);
            }
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
        public void setOutputs(PortItemInfo[] viewData)
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
                    item.isControl = viewData[i].isControl;
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
                }, rebuild: true);
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
                }, rebuild: true);
            }
            SetDirty();
        }
        public void setActiveEntry(string[] targetTypes, (int typeIndex, string name, string errorTip)[] targetCheckers)
        {
            updateTargetCheckerList(targetTypes, targetCheckers);
        }

        public void setObsoleteMsg(string message)
        {
            obsoleteMessage = message;
            if (message == null)
            {
                // normal
                _nameBackground.color = _normalTitleColor;
                _selectedOutline.color = _normalTitleColor;
                _warningObj.gameObject.SetActive(false);
            }
            else
            {
                // obsolete
                _nameBackground.color = _warningTitleColor;
                _selectedOutline.color = _warningTitleColor;
                _warningObj.gameObject.SetActive(true);
            }
            SetDirty();
        }

        /// <summary>
        /// 设置帮助按钮对应的Key
        /// </summary>
        /// <param name="key"></param>
        public void setDocumentKey(int key)
        {
            documentKey = key;
            _helpBtn.gameObject.SetActive(key >= 0);
            SetDirty();
        }
        /// <summary>
        /// 显示特效按钮
        /// </summary>
        /// <param name="key"></param>
        public void showVisualEffectButton(bool value)
        {
            _visualEffectButtonObj.SetActive(value);
            SetDirty();
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
                item.setReadonly(_isReadonly);
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
            }, rebuild: true);
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
            if (index < 0 || index >= _inputList.Count)
                return null;
            return _inputList[index].GetComponent<ActionNodeItemInput>();
        }
        public ActionNodeItemOutput getOutput(int index)
        {
            if (index < 0 || index >= _outputList.Count)
                return null;
            return _outputList[index].GetComponent<ActionNodeItemOutput>();
        }
        public ActionNodeItemInput[] getInputs()
        {
            return _inputList.Select(r => r.GetComponent<ActionNodeItemInput>()).ToArray();
        }
        public ActionNodeItemOutput[] getOutputs()
        {
            return _outputList.Select(r => r.GetComponent<ActionNodeItemOutput>()).ToArray();
        }
        public void setReadonly(bool isReadonly)
        {
            _isReadonly = isReadonly;
            bool canEdit = !isReadonly;
            _addEffectPropertyItemButton.interactable = canEdit;
            _addTargetCheckerButton.interactable = canEdit;
            _visualEffectButton.interactable = canEdit;
            _visualEffectButton.interactable = canEdit;

            foreach (var targetCheckerRect in _targetCheckerList)
            {
                var targetChecker = targetCheckerRect.GetComponent<ActionNodeItemTriggerTargetChecker>();
                targetChecker.setReadonly(_isReadonly);
            }
            foreach (var inputRect in _inputList)
            {
                var valueField = inputRect.GetComponent<ActionNodeItemInput>();
                valueField.setReadonly(_isReadonly);
            }
        }
        #region 自定义属性
        public void updateEffectProperties(PropertyItemInfo[] properties)
        {
            UIHelper.updateList(_effectPropertyRoot, _effectPropertyItemTemplate, _effectPropertyItems, properties.Length, (i, itemRect) =>
            {
                PropertyItemField item = itemRect.GetComponent<PropertyItemField>();
                item.updateItem(properties[i]);
            }, itemRect =>
            {
                PropertyItemField item = itemRect.GetComponent<PropertyItemField>();
                item.onValueChanged += onEffectPropertyValueChangedCallback;
                item.onClickButton += onClickEffectPropertyButtonCallback;
                item.onClickDelete += onDeleteEffectPropertyCallback;

                item.onClickItemButton += onClickEffectPropertyItemButtonCallback;
            }, itemRect =>
            {
                PropertyItemField item = itemRect.GetComponent<PropertyItemField>();
                item.onValueChanged -= onEffectPropertyValueChangedCallback;
                item.onClickButton -= onClickEffectPropertyButtonCallback;
                item.onClickDelete -= onDeleteEffectPropertyCallback;

                item.onClickItemButton -= onClickEffectPropertyItemButtonCallback;
            }, rebuild: true);
            SetDirty();
        }
        public PropertyItemField[] getEffectPropertyItemFields()
        {
            return _effectPropertyItems.Select(i => i.GetComponent<PropertyItemField>()).ToArray();
        }
        public PropertyItemField getEffectPropertyItemField(string name)
        {
            foreach (var rect in _effectPropertyItems)
            {
                if (rect == null)
                    continue;
                var field = rect.GetComponent<PropertyItemField>();
                if (field == null)
                    continue;
                if (field.propertyName == name)
                {
                    return field;
                }
            }
            return null;
        }
        #endregion
        /// <summary>
        /// 将该节点项设置为不洁净。会在本帧结束时调用onUpdateTransform回调。
        /// </summary>
        public void SetDirty()
        {
            _dirty = true;
        }
        public Rect getNodeItemRect()
        {
            var itemRectTrans = transform as RectTransform;
            var itemRect = itemRectTrans.rect;
            var itemPos = new Vector2(posX, posY);
            itemRect.center = itemPos;
            return itemRect;
        }
        #region 选择
        public void select()
        {
            _selectedIndicator.SetActive(true);
        }
        public void deselect()
        {
            _selectedIndicator.SetActive(false);
        }
        #endregion
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_inputTemplate.transform.parent == _inputRoot)
            {
                _inputTemplate.gameObject.SetActive(false);
            }
            if (_outputTemplate.transform.parent == _outputRoot)
            {
                _outputTemplate.gameObject.SetActive(false);
            }
            if (_targetCheckerTemplate.transform.parent == _targetCheckerRoot)
            {
                _targetCheckerTemplate.gameObject.SetActive(false);
            }
            if (_effectPropertyItemTemplate.transform.parent == _effectPropertyRoot)
            {
                _effectPropertyItemTemplate.gameObject.SetActive(false);
            }
            _addTargetCheckerButton.onClick.AddListener(onClickAddTargetCheckerButtonCallback);
            _warningObj.onClick.AddListener(onClickObsoleteBtnCallback);
            _helpBtn.onClick.AddListener(onClickHelpBtn);
            _visualEffectButton.onClick.AddListener(onClickVisualEffectButtonCallback);
            _addEffectPropertyItemButton.onClick.AddListener(onClickAddEffectPropertyButtonCallback);
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
        #region 输入默认值
        private void onDefaultValueChangedCallback(ActionNodeItemInput item, object value)
        {
            onDefaultValueChanged?.Invoke(this, _inputList.IndexOf(item.transform as RectTransform), value);
        }
        private void onDefaultValueButtonClickCallback(ActionNodeItemInput item, ValueInputFieldButton type)
        {
            onClickDefaultValueButton?.Invoke(this, _inputList.IndexOf(item.transform as RectTransform), type);
        }
        private void onDefaultValueItemButtonClickCallback(ActionNodeItemInput item, int index, ValueInputFieldButton type)
        {
            onClickDefaultValueItemButton?.Invoke(this, _inputList.IndexOf(item.transform as RectTransform), index, type);
        }
        #endregion
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

        #region 自定义属性
        private void onClickAddEffectPropertyButtonCallback()
        {
            onClickAddEffectPropertyButton?.Invoke();
        }
        private void onClickEffectPropertyItemButtonCallback(string name, int index, ValueInputFieldButton buttonType)
        {
            onClickEffectPropertyItemButton?.Invoke(name, index, buttonType);
        }
        private void onClickEffectPropertyButtonCallback(string name, ValueInputFieldButton buttonType)
        {
            onClickEffectPropertyButton?.Invoke(name, buttonType);
        }
        private void onEffectPropertyValueChangedCallback(string name, object value)
        {
            onEffectPropertyValueChanged?.Invoke(name, value);
        }
        private void onDeleteEffectPropertyCallback(string name)
        {
            onDeleteEffectProperty?.Invoke(name);
        }
        #endregion

        private void onClickObsoleteBtnCallback()
        {
            onClickObsoleteButton?.Invoke(obsoleteMessage);
        }

        private void onClickHelpBtn()
        {
            onClickHelpButton?.Invoke(documentKey);
        }
        private void onClickVisualEffectButtonCallback()
        {
            onClickVisualEffectButton?.Invoke(this);
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
        public event Action<ActionNodeItem, int, object> onDefaultValueChanged;
        public event Action<ActionNodeItem, int, ValueInputFieldButton> onClickDefaultValueButton;
        public event Action<ActionNodeItem, int, int, ValueInputFieldButton> onClickDefaultValueItemButton;
        public event Action<ActionNodeItem> onClickAddTargetCheckerButton;
        public event Action<ActionNodeItem, int, int> onTargetCheckerTypeValueChanged;
        public event Action<ActionNodeItem, int> onClickEditTargetCheckerButton;
        public event Action<ActionNodeItem, int> onClickRemoveTargetCheckerButton;
        public event Action<ActionNodeItem, int, string> onTargetCheckerErrorTipValueChanged;
        public event Action<ActionNodeItem> onUpdateTransform;
        public event Action<int> onClickHelpButton;
        public event Action<string> onClickObsoleteButton;
        public event Action<ActionNodeItem> onClickVisualEffectButton;

        #region 效果属性
        /// <summary>
        /// 当确认创建自定义属性。
        /// </summary>
        public event Action onClickAddEffectPropertyButton;
        public event Action<string> onDeleteEffectProperty;

        public event Action<string, object> onEffectPropertyValueChanged;
        public event Action<string, ValueInputFieldButton> onClickEffectPropertyButton;
        public event Action<string, int, ValueInputFieldButton> onClickEffectPropertyItemButton;
        #endregion
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
        int documentKey = -1;
        private bool _dirty;
        private bool _isReadonly;

        [Header("基本")]
        [SerializeField]
        Text _nameText;
        [SerializeField]
        Image _nameBackground;
        [SerializeField]
        Button _warningObj;
        [SerializeField]
        Button _helpBtn;
        [SerializeField]
        GameObject _visualEffectButtonObj;
        [SerializeField]
        Button _visualEffectButton;
        [SerializeField]
        GameObject _selectedIndicator;
        [SerializeField]
        Image _selectedOutline;

        [Header("输入")]
        [SerializeField]
        RectTransform _inputRoot;
        [SerializeField]
        RectTransform _inputTemplate;
        [SerializeField]
        List<RectTransform> _inputList = new List<RectTransform>();

        [Header("输出")]
        [SerializeField]
        RectTransform _outputRoot;
        [SerializeField]
        RectTransform _outputTemplate;
        [SerializeField]
        List<RectTransform> _outputList = new List<RectTransform>();

        [Header("触发器")]
        [SerializeField]
        RectTransform _triggerFieldsRoot;

        [Header("目标检查器")]
        [SerializeField]
        RectTransform _targetCheckerRoot;
        [SerializeField]
        RectTransform _targetCheckerTemplate;
        [SerializeField]
        List<RectTransform> _targetCheckerList = new List<RectTransform>();
        [SerializeField]
        Button _addTargetCheckerButton;

        [Header("效果属性")]
        [SerializeField]
        GameObject _effectPropertyFields;
        [SerializeField]
        RectTransform _effectPropertyRoot;
        [SerializeField]
        RectTransform _effectPropertyItemTemplate;
        [SerializeField]
        List<RectTransform> _effectPropertyItems = new List<RectTransform>();
        [SerializeField]
        Button _addEffectPropertyItemButton;

        [Header("颜色")]
        [SerializeField]
        Color _normalTitleColor;
        [SerializeField]
        Color _warningTitleColor;
        #endregion
    }
}