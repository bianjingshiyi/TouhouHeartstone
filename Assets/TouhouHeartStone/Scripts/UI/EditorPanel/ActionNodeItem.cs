using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Game;
using MukioI18n;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UIHelper = BJSYGameCore.UI.UIHelper;

namespace UI
{
    public partial class ActionNodeItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region 公有方法
        public void switchToActiveEntryNode()
        {
            _buffFieldsRoot.gameObject.SetActive(false);
            _triggerFieldsRoot.gameObject.SetActive(true);
            _effectPropertyFields.SetActive(true);
            _inputRoot.gameObject.SetActive(true);
            _valueRoot.gameObject.SetActive(false);
            _outputRoot.gameObject.SetActive(true);
            SetDirty();
        }
        public void switchToActionNode()
        {
            _buffFieldsRoot.gameObject.SetActive(false);
            _triggerFieldsRoot.gameObject.SetActive(false);
            _effectPropertyFields.SetActive(false);
            _inputRoot.gameObject.SetActive(true);
            _valueRoot.gameObject.SetActive(true);
            _outputRoot.gameObject.SetActive(true);
            SetDirty();
        }
        public void switchToEventEntryNode()
        {
            _buffFieldsRoot.gameObject.SetActive(false);
            _triggerFieldsRoot.gameObject.SetActive(false);
            _effectPropertyFields.SetActive(true);
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
        public void setInputs(PortItemInfo[] inputViewData)
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
                    item.isControl = inputViewData[i].isControl;
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
                }, rebuild: true);
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
                    item.text = valueInfos[i].displayName;
                });
                UIHelper.updateList(_valueRoot, _valueTemplate, _valueList, valueInfos.Length, (i, itemRect) =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.setValueInfo(valueInfos[i]);
                    item.setReadonly(_isReadonly);
                }, itemRect =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.onValueChanged += onConstValueChangedCallback;
                    item.onClickButton += onConstValueButtonClickCallback;
                    item.onClickCardButton += onConstValueCardButtonClickCallback;
                    item.onClickCardEditButton += onConstValueCardEditButtonClickCallback;
                    item.onClickItemButton += onConstValueItemButtonClickCallback;
                    item.onClickItemCardButton += onConstValueItemCardButtonClickCallback;
                    item.onClickItemCardEditButton += onConstValueItemCardEditButtonClickCallback;
                }, itemRect =>
                {
                    ValueInputField item = itemRect.GetComponent<ValueInputField>();
                    item.onValueChanged -= onConstValueChangedCallback;
                    item.onClickButton -= onConstValueButtonClickCallback;
                    item.onClickCardButton -= onConstValueCardButtonClickCallback;
                    item.onClickCardEditButton -= onConstValueCardEditButtonClickCallback;
                    item.onClickItemButton -= onConstValueItemButtonClickCallback;
                    item.onClickItemCardButton -= onConstValueItemCardButtonClickCallback;
                    item.onClickItemCardEditButton -= onConstValueItemCardEditButtonClickCallback;
                }, rebuild: true);
            }
            SetDirty();
        }
        public ValueInputField getConstInputField(int index)
        {
            return _valueList[index].GetComponent<ValueInputField>();
        }
        /// <summary>
        /// 初始化事件名称
        /// </summary>
        /// <param name="eventNames"></param>
        public void initEventNames(string[] eventNames, string[] eventValues)
        {
            ValueInputField item = _valueTemplate.GetComponent<ValueInputField>();
            item.initEventNames(eventNames, eventValues);
        }
        /// <summary>
        /// 初始化事件类型名称
        /// </summary>
        /// <param name="eventNames"></param>
        public void initPileNames(string[] pileNames, string[] pileValues)
        {
            ValueInputField item = _valueTemplate.GetComponent<ValueInputField>();
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
            ValueInputField item = _valueTemplate.GetComponent<ValueInputField>();
            item.initEffectTagNames(effectTagNames, effectTagValues);
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
            ValueInputField item = _valueTemplate.GetComponent<ValueInputField>();
            item.initCardProperties(cardProperties);
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
            _buffDropdown.interactable = canEdit;
            _editBuffButton.interactable = canEdit;
            _visualEffectButton.interactable = canEdit;
            _visualEffectButton.interactable = canEdit;

            foreach (var targetCheckerRect in _targetCheckerList)
            {
                var targetChecker = targetCheckerRect.GetComponent<ActionNodeItemTriggerTargetChecker>();
                targetChecker.setReadonly(_isReadonly);
            }
            foreach (var valueFieldRect in _valueList)
            {
                var valueField = valueFieldRect.GetComponent<ValueInputField>();
                valueField.setReadonly(_isReadonly);
            }
        }
        #region 自定义属性
        public void updateEffectProperties(PropertyItemInfo[] properties, CardManager cardManager)
        {
            UIHelper.updateList(_effectPropertyRoot, _effectPropertyItemTemplate, _effectPropertyItems, properties.Length, async (i, itemRect) =>
            {
                PropertyItemField item = itemRect.GetComponent<PropertyItemField>();
                await item.updateItem(properties[i], cardManager);
            }, itemRect =>
            {
                PropertyItemField item = itemRect.GetComponent<PropertyItemField>();
                item.onValueChanged += onEffectPropertyValueChangedCallback;
                item.onClickButton += onClickEffectPropertyButtonCallback;
                item.onClickCardButton += onClickEffectPropertyCardButtonCallback;
                item.onClickCardEditButton += onClickEffectPropertyCardEditButtonCallback;
                item.onClickDelete += onDeleteEffectPropertyCallback;

                item.onClickAddItemButton += onClickEffectPropertyAddItemButtonCallback;
                item.onItemValueChanged += onEffectPropertyItemValueChangedCallback;
                item.onClickItemButton += onClickEffectPropertyItemButtonCallback;
                item.onClickItemCardButton += onClickEffectPropertyItemCardButtonCallback;
                item.onClickItemCardEditButton += onClickEffectPropertyItemCardEditButtonCallback;
                item.onClickRemoveItemButton += onClickEffectPropertyRemoveItemButtonCallback;
            }, itemRect =>
            {
                PropertyItemField item = itemRect.GetComponent<PropertyItemField>();
                item.onValueChanged -= onEffectPropertyValueChangedCallback;
                item.onClickButton -= onClickEffectPropertyButtonCallback;
                item.onClickCardButton -= onClickEffectPropertyCardButtonCallback;
                item.onClickCardEditButton -= onClickEffectPropertyCardEditButtonCallback;
                item.onClickDelete -= onDeleteEffectPropertyCallback;

                item.onClickAddItemButton -= onClickEffectPropertyAddItemButtonCallback;
                item.onItemValueChanged -= onEffectPropertyItemValueChangedCallback;
                item.onClickItemButton -= onClickEffectPropertyItemButtonCallback;
                item.onClickItemCardButton -= onClickEffectPropertyItemCardButtonCallback;
                item.onClickItemCardEditButton -= onClickEffectPropertyItemCardEditButtonCallback;
                item.onClickRemoveItemButton -= onClickEffectPropertyRemoveItemButtonCallback;
            }, rebuild: true);
            SetDirty();
        }
        public async void updateEffectPropertyItem(string name, PropertyItemInfo item, CardManager cardManager)
        {
            PropertyItemField listItem = null;
            foreach (var rect in _effectPropertyItems)
            {
                if (rect == null)
                    continue;
                var i = rect.GetComponent<PropertyItemField>();
                if (i == null)
                    continue;
                if (i.propertyName == name)
                {
                    listItem = i;
                    break;
                }
            }
            if (listItem == null)
                return;
            await listItem.updateItem(item, cardManager);
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
            if (_effectPropertyItemTemplate.transform.parent == _effectPropertyRoot)
            {
                _effectPropertyItemTemplate.gameObject.SetActive(false);
            }
            _addTargetCheckerButton.onClick.AddListener(onClickAddTargetCheckerButtonCallback);
            _warningObj.onClick.AddListener(onClickObsoleteBtnCallback);
            _helpBtn.onClick.AddListener(onClickHelpBtn);
            _visualEffectButton.onClick.AddListener(onClickVisualEffectButtonCallback);
            _addEffectPropertyItemButton.onClick.AddListener(onClickAddEffectPropertyButtonCallback);
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
        private void onConstValueCardButtonClickCallback(ValueInputField item)
        {
            onClickConstValueCardButton?.Invoke(this, _valueList.IndexOf(item.transform as RectTransform));
        }
        private void onConstValueCardEditButtonClickCallback(ValueInputField item)
        {
            onClickConstValueCardEditButton?.Invoke(this, _valueList.IndexOf(item.transform as RectTransform));
        }
        private void onConstValueItemButtonClickCallback(ValueInputField item, int index)
        {
            onClickConstValueItemButton?.Invoke(this, _valueList.IndexOf(item.transform as RectTransform), index);
        }
        private void onConstValueItemCardButtonClickCallback(ValueInputField item, int index)
        {
            onClickConstValueItemCardButton?.Invoke(this, _valueList.IndexOf(item.transform as RectTransform), index);
        }
        private void onConstValueItemCardEditButtonClickCallback(ValueInputField item, int index)
        {
            onClickConstValueItemCardEditButton?.Invoke(this, _valueList.IndexOf(item.transform as RectTransform), index);
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

        #region 自定义属性
        private void onClickAddEffectPropertyButtonCallback()
        {
            onClickAddEffectPropertyButton?.Invoke();
        }
        private void onClickEffectPropertyItemButtonCallback(string name, int index)
        {
            onClickEffectPropertyItemButton?.Invoke(name, index);
        }
        private void onClickEffectPropertyItemCardButtonCallback(string name, int index)
        {
            onClickEffectPropertyItemCardButton?.Invoke(name, index);
        }
        private void onClickEffectPropertyItemCardEditButtonCallback(string name, int index)
        {
            onClickEffectPropertyItemCardEditButton?.Invoke(name, index);
        }
        private void onEffectPropertyItemValueChangedCallback(string name, int index, object value)
        {
            onEffectPropertyItemValueChanged?.Invoke(name, index, value);
        }
        private void onClickEffectPropertyAddItemButtonCallback(string name)
        {
            onClickEffectPropertyAddItemButton?.Invoke(name);
        }
        private void onClickEffectPropertyRemoveItemButtonCallback(string name, int index)
        {
            onClickEffectPropertyRemoveItemButton?.Invoke(name, index);
        }
        private void onClickEffectPropertyButtonCallback(string name)
        {
            onClickEffectPropertyButton?.Invoke(name);
        }
        private void onClickEffectPropertyCardButtonCallback(string name)
        {
            onClickEffectPropertyCardButton?.Invoke(name);
        }
        private void onClickEffectPropertyCardEditButtonCallback(string name)
        {
            onClickEffectPropertyCardEditButton?.Invoke(name);
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
        public event Action<ActionNodeItem, int, object> onConstValueChanged;
        public event Action<ActionNodeItem, int> onClickConstValueButton;
        public event Action<ActionNodeItem, int> onClickConstValueCardButton;
        public event Action<ActionNodeItem, int> onClickConstValueCardEditButton;
        public event Action<ActionNodeItem, int, int> onClickConstValueItemButton;
        public event Action<ActionNodeItem, int, int> onClickConstValueItemCardButton;
        public event Action<ActionNodeItem, int, int> onClickConstValueItemCardEditButton;
        public event Action<ActionNodeItem> onClickEditConditionButton;
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
        public event Action<string, int> onClickEffectPropertyItemButton;
        public event Action<string, int> onClickEffectPropertyItemCardButton;
        public event Action<string, int> onClickEffectPropertyItemCardEditButton;
        public event Action<string, int, object> onEffectPropertyItemValueChanged;
        public event Action<string> onClickEffectPropertyButton;
        public event Action<string> onClickEffectPropertyCardButton;
        public event Action<string> onClickEffectPropertyCardEditButton;
        public event Action<string, object> onEffectPropertyValueChanged;
        public event Action<string> onDeleteEffectProperty;
        public event Action<string> onClickEffectPropertyAddItemButton;
        public event Action<string, int> onClickEffectPropertyRemoveItemButton;
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

        [Header("值")]
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