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
        public void setActiveRanges(string[] activeRanges)
        {
            _activeRangeDropdown.ClearOptions();
            _activeRangeDropdown.AddOptions(new List<string>(activeRanges));
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
        private void onClickReturnButtonCallback()
        {
            onClickReturnButton?.Invoke();
        }
        #endregion
        #endregion
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
        /// 当点击返回按钮
        /// </summary>
        public event Action onClickReturnButton;
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
        Button _returnButton;
    }
}