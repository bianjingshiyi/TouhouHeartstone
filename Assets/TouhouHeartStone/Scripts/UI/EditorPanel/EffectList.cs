using BJSYGameCore.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace UI
{
    public class EffectList : MonoBehaviour
    {
        #region 公有方法
        public void setAddEffectMenu(IEnumerable<string> effectNames)
        {
            _addEffectMenu.ClearOptions();
            _addEffectMenu.AddOptions(new List<string>(effectNames));
        }
        public void updateList(string[] effectNames)
        {
            UIHelper.updateList(_itemRoot, _itemTemplate, _itemList, effectNames.Length, (i, itemRect) =>
            {
                EffectListItem item = itemRect.GetComponent<EffectListItem>();
                item.update(effectNames[i]);
            }, itemRect =>
            {
                EffectListItem item = itemRect.GetComponent<EffectListItem>();
                item.onEditButtonClick += onClickItemEditButtonCallback;
                item.onRemoveButtonClick += onClickItemRemoveButtonCallback;
            }, itemRect =>
            {
                EffectListItem item = itemRect.GetComponent<EffectListItem>();
                item.onEditButtonClick -= onClickItemEditButtonCallback;
                item.onRemoveButtonClick -= onClickItemRemoveButtonCallback;
            });
        }
        #endregion
        #region 私有方法
        protected void Awake()
        {
            _addEffectMenu.onClickItem.AddListener(onClickAddEffectMenuCallback);
            if (_itemTemplate.transform.parent == _itemRoot)
            {
                _itemTemplate.gameObject.SetActive(false);
            }
        }
        private void onClickItemEditButtonCallback(EffectListItem item)
        {
            onClickItemEditButton?.Invoke(_itemList.IndexOf(item.transform as RectTransform));
        }
        private void onClickItemRemoveButtonCallback(EffectListItem item)
        {
            onClickItemRemoveButton?.Invoke(_itemList.IndexOf(item.transform as RectTransform));
        }
        private void onClickAddEffectMenuCallback(int index)
        {
            onClickAddEffect?.Invoke(index);
        }
        #endregion
        #region 事件
        public event Action<int> onClickItemEditButton;
        public event Action<int> onClickItemRemoveButton;
        public event Action<int> onClickAddEffect;
        #endregion
        #region 属性字段
        [SerializeField]
        DropdownMenu _addEffectMenu;
        [SerializeField]
        RectTransform _itemRoot;
        [SerializeField]
        RectTransform _itemTemplate;
        [SerializeField]
        List<RectTransform> _itemList = new List<RectTransform>();
        #endregion
    }
}