using BJSYGameCore.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class EffectList : MonoBehaviour
    {
        #region 公有方法
        public void setAddEffectMenu(IEnumerable<string> effectNames)
        {
            _addEffectMenu.ClearOptions();
            _addEffectMenu.AddOptions(effectNames.ToList());
        }
        public void updateList(string[] effectNames, int activeIndex = -1)
        {
            UIHelper.updateList(_itemRoot, _itemTemplate, _itemList, effectNames.Length, (i, itemRect) =>
            {
                EffectListItem item = itemRect.GetComponent<EffectListItem>();
                item.update(effectNames[i]);
                item.setActive(activeIndex == i);
            }, itemRect =>
            {
                EffectListItem item = itemRect.GetComponent<EffectListItem>();
                item.onEditButtonClick += onClickItemEditButtonCallback;
                item.onRemoveButtonClick += onClickItemRemoveButtonCallback;
                item.onInputFieldEndEdit += onItemInputFieldEndEditCallback;
            }, itemRect =>
            {
                EffectListItem item = itemRect.GetComponent<EffectListItem>();
                item.onEditButtonClick -= onClickItemEditButtonCallback;
                item.onRemoveButtonClick -= onClickItemRemoveButtonCallback;
                item.onInputFieldEndEdit -= onItemInputFieldEndEditCallback;
            });
        }

        public void showItemInputField(int index, string name)
        {
            var rect = _itemList?[index];
            if (rect)
            {
                EffectListItem item = rect.GetComponent<EffectListItem>();
                item.showInputField(name);
            }
        }
        public void hideItemInputField(int index)
        {
            var rect = _itemList?[index];
            if (rect)
            {
                EffectListItem item = rect.GetComponent<EffectListItem>();
                item.hideInputField();
            }
        }
        #endregion
        #region 私有方法
        protected void Awake()
        {
            _addEffectMenu.onClickItem.AddListener(onClickAddEffectItemCallback);
            if (_itemTemplate.transform.parent == _itemRoot)
            {
                _itemTemplate.gameObject.SetActive(false);
            }
        }
        #region 回调
        private void onClickAddEffectItemCallback(int index)
        {
            onClickAddEffect?.Invoke(index);
        }
        private void onClickItemEditButtonCallback(EffectListItem item)
        {
            onClickItemEditButton?.Invoke(_itemList.IndexOf(item.transform as RectTransform));
        }
        private void onClickItemRemoveButtonCallback(EffectListItem item)
        {
            onClickItemRemoveButton?.Invoke(_itemList.IndexOf(item.transform as RectTransform));
        }
        private void onItemInputFieldEndEditCallback(EffectListItem item, string value)
        {
            onItemInputFieldEndEdit?.Invoke(_itemList.IndexOf(item.transform as RectTransform), value);
        }
        #endregion
        #endregion
        #region 事件
        public event Action<int> onClickItemEditButton;
        public event Action<int> onClickItemRemoveButton;
        public event Action<int> onClickAddEffect;
        public event Action<int, string> onItemInputFieldEndEdit;
        #endregion
        #region 属性字段
        [Header("添加效果")]
        [SerializeField]
        DropdownMenu _addEffectMenu;

        [Header("效果列表")]
        [SerializeField]
        RectTransform _itemRoot;
        [SerializeField]
        RectTransform _itemTemplate;
        [SerializeField]
        List<RectTransform> _itemList = new List<RectTransform>();
        #endregion
    }
}