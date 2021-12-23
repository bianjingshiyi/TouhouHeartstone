using BJSYGameCore.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    /// <summary>
    /// 效果编辑器UI脚本
    /// </summary>
    public class EffectEditorPanel : MonoBehaviour
    {
        #region 公有方法
        public void setActiveRanges(string[] activeRangeNames, string[] pileValues)
        {
            _activeRangeDropdown.ClearOptions();
            _activeRangeDropdown.AddOptions(new List<string>(activeRangeNames));
            _effectPropListTemplate.GetComponent<ValueInputField>().setupPileNames(activeRangeNames, pileValues);
        }
        public void setActiveRange(bool[] value)
        {
            _activeRangeDropdown.value = value;
        }
        public void updateTriggerList(string[] triggerNames)
        {
            if (triggerNames == null || triggerNames.Length < 1)
            {
                _triggerListRoot.gameObject.SetActive(false);
            }
            else
            {
                _triggerListRoot.gameObject.SetActive(true);
                UIHelper.updateList(_triggerListRoot, _triggerListItemTemplate, _triggerItemList, triggerNames.Length, (i, itemRect) =>
                {
                    EditTriggerListItem item = itemRect.GetComponent<EditTriggerListItem>();
                    item.updateView(triggerNames[i]);
                }, itemRect =>
                {
                    EditTriggerListItem item = itemRect.GetComponent<EditTriggerListItem>();
                    item.onClickEditButton += onClickTriggerListItemEditButton;
                    item.onClickRemoveButton += onClickTriggerListItemRemoveButton;
                }, itemRect =>
                {
                    EditTriggerListItem item = itemRect.GetComponent<EditTriggerListItem>();
                    item.onClickEditButton -= onClickTriggerListItemEditButton;
                    item.onClickRemoveButton -= onClickTriggerListItemRemoveButton;
                });
                LayoutRebuilder.ForceRebuildLayoutImmediate(_triggerListRoot);
            }
        }
        public void updatePropList(ActionNodeValueInfo[] valueInfos)
        {
            UIHelper.updateList(_effectPropListRoot, _effectPropListTemplate, _effectPropList, valueInfos.Length, (i, itemRect) =>
            {
                ValueInputField item = itemRect.GetComponent<ValueInputField>();
                item.setValueInfo(valueInfos[i].name, valueInfos[i]);
            }, itemRect =>
            {
                ValueInputField item = itemRect.GetComponent<ValueInputField>();
                item.onValueChanged += onPropValueChangedCallback;
                item.onClickButton += onClickPropButtonCallback;
                item.onItemValueChanged += onPropItemValueChangedCallback;
                item.onClickItemButton += onClickPropItemButtonCallback;
                item.onClickAddItemButton += onClickPropAddItemButtonCallback;
                item.onClickItemRemoveButton += onClickPropItemRemoveButtonCallback;
            }, itemRect =>
            {
                ValueInputField item = itemRect.GetComponent<ValueInputField>();
                item.onValueChanged -= onPropValueChangedCallback;
                item.onClickButton -= onClickPropButtonCallback;
                item.onItemValueChanged -= onPropItemValueChangedCallback;
                item.onClickItemButton -= onClickPropItemButtonCallback;
                item.onClickAddItemButton -= onClickPropAddItemButtonCallback;
                item.onClickItemRemoveButton -= onClickPropItemRemoveButtonCallback;
            });
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _activeRangeDropdown.onValueChanged.AddListener(onActiveRangeDropdownValueChangedCallback);
            _editEnableActionButton.onClick.AddListener(onClickEditEnableActionButtonCallback);
            _editDisableActionButton.onClick.AddListener(onClickEditDisableActionButtonCallback);
            _addTriggerButton.onClick.AddListener(onClickAddTriggerButtonCallback);
            if (_triggerListItemTemplate.transform.parent == _triggerListRoot)
            {
                _triggerListItemTemplate.gameObject.SetActive(false);
            }
            _returnButton.onClick.AddListener(onClickReturnButtonCallback);
        }
        #endregion
        #region 事件回调
        private void onActiveRangeDropdownValueChangedCallback(bool[] value)
        {
            onActiveRangeDropdownValueChanged?.Invoke(value);
        }
        private void onClickEditEnableActionButtonCallback()
        {
            onClickEditEnableActionButton?.Invoke();
        }
        private void onClickEditDisableActionButtonCallback()
        {
            onClickEditDisableActionButton?.Invoke();
        }
        private void onClickAddTriggerButtonCallback()
        {
            onClickAddTriggerButton?.Invoke();
        }
        private void onClickTriggerListItemEditButton(EditTriggerListItem item)
        {
            onClickEditTriggerButton?.Invoke(_triggerItemList.IndexOf(item.transform as RectTransform));
        }
        private void onClickTriggerListItemRemoveButton(EditTriggerListItem item)
        {
            onClickRemoveTriggerButton?.Invoke(_triggerItemList.IndexOf(item.transform as RectTransform));
        }
        private void onPropValueChangedCallback(ValueInputField item, object value)
        {
            onPropValueChanged?.Invoke(_effectPropList.IndexOf(item.transform as RectTransform), value);
        }
        private void onClickPropButtonCallback(ValueInputField item)
        {
            onClickPropButton?.Invoke(_effectPropList.IndexOf(item.transform as RectTransform));
        }
        private void onPropItemValueChangedCallback(ValueInputField item, int itemIndex, object value)
        {
            onPropItemValueChanged?.Invoke(_effectPropList.IndexOf(item.transform as RectTransform), itemIndex, value);
        }
        public void onClickPropItemButtonCallback(ValueInputField item, int index)
        {
            onClickPropItemButton?.Invoke(_effectPropList.IndexOf(item.transform as RectTransform), index);
        }
        public void onClickPropAddItemButtonCallback(ValueInputField item)
        {
            onClickPropAddItemButton?.Invoke(_effectPropList.IndexOf(item.transform as RectTransform));
        }
        public void onClickPropItemRemoveButtonCallback(ValueInputField item, int index)
        {
            onClickPropItemRemoveButton?.Invoke(_effectPropList.IndexOf(item.transform as RectTransform), index);
        }
        private void onClickReturnButtonCallback()
        {
            onClickReturnButton?.Invoke();
        }
        #endregion
        #endregion
        #region 事件
        /// <summary>
        /// 当点击编辑生效范围按钮
        /// </summary>
        public event Action<bool[]> onActiveRangeDropdownValueChanged;
        /// <summary>
        /// 当点击编辑激活动作按钮
        /// </summary>
        public event Action onClickEditEnableActionButton;
        /// <summary>
        /// 当点击禁用激活动作按钮
        /// </summary>
        public event Action onClickEditDisableActionButton;
        /// <summary>
        /// 当点击添加触发器按钮
        /// </summary>
        public event Action onClickAddTriggerButton;
        /// <summary>
        /// 当点击移除触发器按钮，参数为移除的触发器索引。
        /// </summary>
        public event Action<int> onClickRemoveTriggerButton;
        /// <summary>
        /// 当点击编辑触发器按钮，参数为编辑的触发器索引。
        /// </summary>
        public event Action<int> onClickEditTriggerButton;
        /// <summary>
        /// 当属性列表项值发生改变，参数为发生改变的项目索引和改变值
        /// </summary>
        public event Action<int, object> onPropValueChanged;
        /// <summary>
        /// 当属性列表项目列表中的项目的值发生改变，参数为项目索引和项目列表中项目的索引以及改变的值。
        /// </summary>
        public event Action<int, int, object> onPropItemValueChanged;
        /// <summary>
        /// 当点击属性列表项的按钮，参数为点击的项目索引
        /// </summary>
        public event Action<int> onClickPropButton;
        /// <summary>
        /// 当点击属性列表项目列表中的项目，参数为项目索引和项目列表中项目的索引
        /// </summary>
        public event Action<int, int> onClickPropItemButton;
        /// <summary>
        /// 当点击属性列表项目列表的添加项目按钮，参数为项目索引
        /// </summary>
        public event Action<int> onClickPropAddItemButton;
        /// <summary>
        /// 当点击属性列表项目列表中的项目的移除按钮，参数为项目索引和项目列表中项目的索引
        /// </summary>
        public event Action<int, int> onClickPropItemRemoveButton;
        /// <summary>
        /// 当点击返回按钮
        /// </summary>
        public event Action onClickReturnButton;
        #endregion
        #region 属性字段
        [SerializeField]
        MultiSelectDropdown _activeRangeDropdown;
        [SerializeField]
        Button _editEnableActionButton;
        [SerializeField]
        Button _editDisableActionButton;
        [SerializeField]
        Button _addTriggerButton;
        [SerializeField]
        RectTransform _triggerListRoot;
        [SerializeField]
        RectTransform _triggerListItemTemplate;
        [SerializeField]
        List<RectTransform> _triggerItemList;
        [SerializeField]
        RectTransform _effectPropListRoot;
        [SerializeField]
        RectTransform _effectPropListTemplate;
        [SerializeField]
        List<RectTransform> _effectPropList = new List<RectTransform>();
        [SerializeField]
        Button _returnButton;
        #endregion
    }
}