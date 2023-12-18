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
                listItem.onValueChanged += onPropValueChangedCallback;
                listItem.onClickButton += onClickPropButtonCallback;
                listItem.onClickItemButton += onClickPropChildButtonCallback;
            }, 
            rect =>
            {
                var listItem = rect.GetComponent<PropertyItemField>();
                listItem.onValueChanged -= onPropValueChangedCallback;
                listItem.onClickButton -= onClickPropButtonCallback;
                listItem.onClickItemButton -= onClickPropChildButtonCallback;
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
        private void onClickPropButtonCallback(string name, ValueInputFieldButton button)
        {
            onClickPropButton?.Invoke(name, button);
        }
        private void onPropValueChangedCallback(string name, object value)
        {
            onPropValueChanged?.Invoke(name, value);
        }
        private void onClickPropChildButtonCallback(string name, int index, ValueInputFieldButton button)
        {
            onClickPropChildButton?.Invoke(name, index, button);
        }

        public event Action<string, object> onPropValueChanged;
        public event Action<string, ValueInputFieldButton> onClickPropButton;
        public event Action<string, int, ValueInputFieldButton> onClickPropChildButton;

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