using System;
using System.Collections.Generic;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    /// <summary>
    /// 卡池浏览界面
    /// </summary>
    public class CardPoolViewPanel : MonoBehaviour
    {
        #region 公有方法
        public void enableCardPoolFilter(bool value)
        {
            _cardPoolDropdown.gameObject.SetActive(value);
        }
        public void setCardPoolOptions(string[] options)
        {
            _cardPoolDropdown.ClearOptions();
            _cardPoolDropdown.AddOptions(new List<string>(options));
        }
        public void setCardPoolValues(bool[] value)
        {
            _cardPoolDropdown.SetValueWithoutNotify(value);
        }
        public void setSortTypes(string[] sortTypes)
        {
            _sortDropdown.ClearOptions();
            _sortDropdown.AddOptions(new List<string>(sortTypes));
        }
        public void setSortType(int index)
        {
            _sortDropdown.SetValueWithoutNotify(index);
        }
        public void setCharacterFilters(string[] characterFilters)
        {
            _characterFilterDropdown.ClearOptions();
            _characterFilterDropdown.AddOptions(new List<string>(characterFilters));
        }
        public void setCharacterFilter(bool[] value)
        {
            _characterFilterDropdown.SetValueWithoutNotify(value);
        }
        public void setCostFilters(string[] costFilters)
        {
            _costFilterDropdown.ClearOptions();
            _costFilterDropdown.AddOptions(new List<string>(costFilters));
        }
        public void setCostFilter(bool[] value)
        {
            _costFilterDropdown.SetValueWithoutNotify(value);
        }
        public void setTypeFilters(string[] typeFilters)
        {
            _typeFilterDropdown.ClearOptions();
            _typeFilterDropdown.AddOptions(new List<string>(typeFilters));
        }
        public void setTypeFilter(bool[] value)
        {
            _typeFilterDropdown.SetValueWithoutNotify(value);
        }
        public void setSearchInputField(string value)
        {
            _searchInputField.SetTextWithoutNotify(value);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _cardPoolDropdown.onValueChanged.AddListener(onCardPoolsValueChangedCallback);
            _characterFilterDropdown.onValueChanged.AddListener(onCharacterFilterValueChangedCallback);
            _costFilterDropdown.onValueChanged.AddListener(onCostFilterValueChangedCallback);
            _typeFilterDropdown.onValueChanged.AddListener(onTypeFilterValueChangedCallback);
            _sortDropdown.onValueChanged.AddListener(onSortTypeValueChangedCallback);
            _searchInputField.onEndEdit.AddListener(onSearchInputFieldEndEditCallback);
        }
        #endregion
        #region 事件回调
        private void onCardPoolsValueChangedCallback(bool[] value)
        {
            onCardPoolsValueChanged?.Invoke(value);
        }
        /// <summary>
        /// 修改了排序
        /// </summary>
        private void onSortTypeValueChangedCallback(int value)
        {
            onSortTypeValueChanged?.Invoke(value);
        }
        private void onCharacterFilterValueChangedCallback(bool[] value)
        {
            onCharacterFilterValueChanged?.Invoke(value);
        }
        private void onCostFilterValueChangedCallback(bool[] value)
        {
            onCostFilterValueChanged?.Invoke(value);
        }
        private void onTypeFilterValueChangedCallback(bool[] value)
        {
            onTypeFilterValueChanged?.Invoke(value);
        }
        private void onSearchInputFieldEndEditCallback(string value)
        {
            onSearchInputFieldValueChanged?.Invoke(value);
        }
        #endregion
        #endregion
        #region 事件
        public event Action<bool[]> onCardPoolsValueChanged;
        public event Action<int> onSortTypeValueChanged;
        public event Action<bool[]> onCharacterFilterValueChanged;
        public event Action<bool[]> onCostFilterValueChanged;
        public event Action<bool[]> onTypeFilterValueChanged;
        public event Action<string> onSearchInputFieldValueChanged;
        #endregion
        #region 属性字段
        [SerializeField]
        MultiSelectDropdown _cardPoolDropdown;
        [SerializeField]
        Dropdown _sortDropdown;
        [SerializeField]
        MultiSelectDropdown _characterFilterDropdown;
        [SerializeField]
        MultiSelectDropdown _costFilterDropdown;
        [SerializeField]
        MultiSelectDropdown _typeFilterDropdown;
        [SerializeField]
        InputField _searchInputField;
        #endregion
    }
}