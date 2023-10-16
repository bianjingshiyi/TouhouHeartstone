using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game;
using UnityEngine;
using UnityEngine.UI;
using UIHelper = BJSYGameCore.UI.UIHelper;

namespace UI
{
    public class CardPropertiesFoldout : MonoBehaviour
    {
        public void setFolded(bool expand)
        {
            _contentsObj.SetActive(expand);
            _arrow.localEulerAngles = expand ? Vector3.back * 90 : Vector3.zero;
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
        public bool isExpanded()
        {
            return _contentsObj.activeSelf;
        }
        public void updateItems(PropertyItemInfo[] items, CardManager cardManager)
        {
            UIHelper.updateList(_propItemsRoot, _propItemTemplate, _itemList, items.Length, async (index, rect) =>
            {
                PropertyItemField listItem = rect.GetComponent<PropertyItemField>();
                await listItem.updateItem(items[index], cardManager);
            }, 
            rect =>
            {
                var listItem = rect.GetComponent<PropertyItemField>();
                listItem.onClickButton += onClickPropButtonCallback;
                listItem.onClickCardButton += onClickPropCardButtonCallback;
                listItem.onClickCardEditButton += onClickPropCardEditButtonCallback;
                listItem.onValueChanged += onPropValueChangedCallback;

                listItem.onClickAddItemButton += onClickPropAddChildButtonCallback;
                listItem.onClickRemoveItemButton += onClickPropRemoveChildButtonCallback;
                listItem.onClickItemButton += onClickPropChildButtonCallback;
                listItem.onClickItemCardButton += onClickPropChildCardButtonCallback;
                listItem.onClickItemCardEditButton += onClickPropChildCardEditButtonCallback;
                listItem.onItemValueChanged += onPropChildValueChangedCallback;
            }, 
            rect =>
            {
                var listItem = rect.GetComponent<PropertyItemField>();
                listItem.onClickButton -= onClickPropButtonCallback;
                listItem.onClickCardButton -= onClickPropCardButtonCallback;
                listItem.onClickCardEditButton -= onClickPropCardEditButtonCallback;
                listItem.onValueChanged -= onPropValueChangedCallback;

                listItem.onClickAddItemButton -= onClickPropAddChildButtonCallback;
                listItem.onClickRemoveItemButton -= onClickPropRemoveChildButtonCallback;
                listItem.onClickItemButton -= onClickPropChildButtonCallback;
                listItem.onClickItemCardButton -= onClickPropChildCardButtonCallback;
                listItem.onClickItemCardEditButton -= onClickPropChildCardEditButtonCallback;
                listItem.onItemValueChanged -= onPropChildValueChangedCallback;
            });
        }
        public async Task updateItem(string name, PropertyItemInfo item, CardManager cardManager)
        {
            PropertyItemField listItem = null;
            foreach (var rect in _itemList)
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

        private void Awake()
        {
            _headerButton.onClick.AddListener(onClickHeaderButtonCallback);
        }
        private void onClickHeaderButtonCallback()
        {
            setFolded(!isExpanded());
        }
        private void onClickPropButtonCallback(string name)
        {
            onClickPropButton?.Invoke(name);
        }
        private void onClickPropCardButtonCallback(string name)
        {
            onClickPropCardButton?.Invoke(name);
        }
        private void onClickPropCardEditButtonCallback(string name)
        {
            onClickPropCardEditButton?.Invoke(name);
        }
        private void onPropValueChangedCallback(string name, object value)
        {
            onPropValueChanged?.Invoke(name, value);
        }
        private void onClickPropAddChildButtonCallback(string name)
        {
            onClickPropAddChildButton?.Invoke(name);
        }
        private void onClickPropRemoveChildButtonCallback(string name, int index)
        {
            onClickPropRemoveChildButton?.Invoke(name, index);
        }
        private void onClickPropChildButtonCallback(string name, int index)
        {
            onClickPropChildButton?.Invoke(name, index);
        }
        private void onClickPropChildCardButtonCallback(string name, int index)
        {
            onClickPropChildCardButton?.Invoke(name, index);
        }
        private void onClickPropChildCardEditButtonCallback(string name, int index)
        {
            onClickPropChildCardEditButton?.Invoke(name, index);
        }
        private void onPropChildValueChangedCallback(string name, int index, object value)
        {
            onPropChildValueChanged?.Invoke(name, index, value);
        }

        public event Action<string> onClickPropButton;
        public event Action<string> onClickPropCardButton;
        public event Action<string> onClickPropCardEditButton;
        public event Action<string, object> onPropValueChanged;
        public event Action<string> onClickPropAddChildButton;
        public event Action<string, int> onClickPropRemoveChildButton;
        public event Action<string, int> onClickPropChildButton;
        public event Action<string, int> onClickPropChildCardButton;
        public event Action<string, int> onClickPropChildCardEditButton;
        public event Action<string, int, object> onPropChildValueChanged;

        [SerializeField]
        private Button _headerButton;
        [SerializeField]
        private RectTransform _arrow;
        [SerializeField]
        private GameObject _contentsObj;
        [SerializeField]
        private RectTransform _propItemsRoot;
        [SerializeField]
        private RectTransform _propItemTemplate;
        [SerializeField]
        private List<RectTransform> _itemList;
    }
}