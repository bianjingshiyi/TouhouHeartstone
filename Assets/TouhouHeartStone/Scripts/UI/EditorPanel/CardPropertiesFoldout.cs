using System;
using System.Collections.Generic;
using System.Linq;
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
        public void updateItems(PropertyItemInfo[] items)
        {
            UIHelper.updateList(_propItemsRoot, _propItemTemplate, _itemList, items.Length, (index, rect) =>
            {
                PropertyItemField listItem = rect.GetComponent<PropertyItemField>();
                listItem.updateItem(items[index]);
            }, 
            rect =>
            {
                var listItem = rect.GetComponent<PropertyItemField>();
                listItem.onValueChanged += onPropValueChangedCallback;
                listItem.onClickButton += onClickPropButtonCallback;
            }, 
            rect =>
            {
                var listItem = rect.GetComponent<PropertyItemField>();
                listItem.onValueChanged -= onPropValueChangedCallback;
                listItem.onClickButton -= onClickPropButtonCallback;
            });
        }
        public PropertyItemField[] getPropertyItemFields()
        {
            return _itemList.Select(i => i.GetComponent<PropertyItemField>()).ToArray();
        }
        public PropertyItemField getPropertyItemField(string name)
        {
            foreach (var rect in _itemList)
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

        private void Awake()
        {
            _headerButton.onClick.AddListener(onClickHeaderButtonCallback);
        }
        private void onClickHeaderButtonCallback()
        {
            setFolded(!isExpanded());
        }
        private void onClickPropButtonCallback(string name, int[] itemIndexes, ValueInputFieldButton button)
        {
            onClickPropButton?.Invoke(name, itemIndexes, button);
        }
        private void onPropValueChangedCallback(string name, object value)
        {
            onPropValueChanged?.Invoke(name, value);
        }

        public event Action<string, object> onPropValueChanged;
        public event Action<string, int[], ValueInputFieldButton> onClickPropButton;

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