using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PropertyItemField : MonoBehaviour
    {
        private void Awake()
        {
            _valueInputField.onValueChanged += (item, value) => onValueChanged?.Invoke(_name, value);
            _valueInputField.onClickButton += (item, type) => onClickButton?.Invoke(_name, type);
            _valueInputField.onClickItemButton += (item, index, type) => onClickItemButton?.Invoke(_name, index, type);
            _deleteButton.onClick.AddListener(() => onClickDelete?.Invoke(_name));
        }
        public void updateItem(PropertyItemInfo info)
        {
            _name = info.propertyName;
            _nameText.text = info.getDisplayName();

            var type = info.type;
            var value = info.value;
            var valueInfo = new ActionNodeValueInfo(string.Empty, type, value);
            _valueInputField.setValueInfo(string.Empty, valueInfo);
        }
        public void setPiles(string[] pileNames, string[] pileValues)
        {
            _valueInputField.initPileNames(pileNames, pileValues);
        }
        public void setPiles(IEnumerable<(string name, string value)> piles)
        {
            setPiles(piles.Select(p => p.name).ToArray(), piles.Select(p => p.value).ToArray());
        }
        public void setKeywords(string[] keywordNames, string[] keywordValues)
        {
            _valueInputField.initKeywords(keywordNames, keywordValues);
        }
        public void setKeywords(IEnumerable<(string name, string value)> keywords)
        {
            setKeywords(keywords.Select(p => p.name).ToArray(), keywords.Select(p => p.value).ToArray());
        }

        public event Action<string> onClickDelete;

        public event Action<string, object> onValueChanged;
        public event Action<string, ValueInputFieldButton> onClickButton;
        public event Action<string, int, ValueInputFieldButton> onClickItemButton;

        public string propertyName => _name;
        public ValueInputField valueInputField => _valueInputField;
        private string _name;
        [SerializeField]
        private Text _nameText;
        [SerializeField]
        private ValueInputField _valueInputField;
        [SerializeField]
        private Button _deleteButton;
    }
    public class PropertyItemInfo
    {
        public string propertyName;
        public string displayName;
        public NodeValueType type;
        public object value;

        public PropertyItemInfo(string name, NodeValueType type)
        {
            propertyName = name;
            this.type = type;
        }
        public PropertyItemInfo(string name, string displayName, NodeValueType type)
        {
            propertyName = name;
            this.displayName = displayName;
            this.type = type;
        }
        public string getDisplayName()
        {
            if (string.IsNullOrEmpty(displayName))
            {
                return propertyName;
            }
            return displayName;
        }
    }
}
