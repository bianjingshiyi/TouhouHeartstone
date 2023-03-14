using BJSYGameCore.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EffectList : MonoBehaviour
    {
        #region 公有方法
        public void setAddEffectMenu(IEnumerable<string> effectNames)
        {
            _effectListNames = effectNames.ToArray();
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
            _addEffectButton.onClick.AddListener(onClickAddEffectButtonCallback);
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
        private void onClickAddEffectButtonCallback()
        {
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
        private string[] _effectListNames;

        [SerializeField]
        Button _addEffectButton;
        [SerializeField]
        RectTransform _addEffectLayout;
        [SerializeField]
        RectTransform _itemRoot;
        [SerializeField]
        RectTransform _itemTemplate;
        [SerializeField]
        List<RectTransform> _itemList = new List<RectTransform>();
        #endregion
    }
}