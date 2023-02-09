using BJSYGameCore.UI;
using System;
using System.Collections.Generic;
using System.IO;
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
        /// <summary>
        /// 切换到效果编辑模式
        /// </summary>
        public void switchToEffect()
        {
            _effectRoot.SetActive(true);
            _actionDefineRoot.SetActive(false);
        }
        /// <summary>
        /// 切换到自定义动作编辑模式
        /// </summary>
        public void switchToActionDefine()
        {
            _effectRoot.SetActive(false);
            _actionDefineRoot.SetActive(true);
        }
        /// <summary>
        /// 设置名字输入框的字符串
        /// </summary>
        /// <param name="name"></param>
        public void setActionDefineName(string name)
        {
            _nameInputField.SetTextWithoutNotify(name);
        }
        /// <summary>
        /// 设置卡牌定义的分类
        /// </summary>
        /// <param name="index"></param>
        public void setActionDefineCategory(int index)
        {
            _categoryDropdown.SetValueWithoutNotify(index);
        }
        public void initActionDefineViewData(string[] categories, string[] variableTypes, string[] constTypes)
        {
            _categoryDropdown.ClearOptions();
            _categoryDropdown.AddOptions(new List<string>(categories));
            _inputTemplate.GetComponent<EffectEditorValueDefineItem>().initTypeDropdown(variableTypes);
            _constTemplate.GetComponent<EffectEditorValueDefineItem>().initTypeDropdown(constTypes);
            _outputListTemplate.GetComponent<EffectEditorValueDefineItem>().initTypeDropdown(variableTypes);
        }
        public void updateInputList((int typeIndex, string name, bool isArray, bool isParams)[] viewData)
        {
            UIHelper.updateList(_inputListRoot, _inputTemplate, _inputList, viewData.Length, (i, itemRect) =>
            {
                EffectEditorValueDefineItem item = itemRect.GetComponent<EffectEditorValueDefineItem>();
                item.setType(viewData[i].typeIndex);
                item.setName(viewData[i].name);
                item.setIsArray(viewData[i].isArray);
                item.setIsParams(viewData[i].isParams);
            }, itemRect =>
            {
                EffectEditorValueDefineItem item = itemRect.GetComponent<EffectEditorValueDefineItem>();
                item.onTypeDropdownValueChanged += onInputListItemTypeDropdownValueChangedCallback;
                item.onNameInputFieldValueChanged += onInputListItemNameInputFieldValueChangedCallback;
                item.onIsArrayToggleValueChanged += onInputListItemIsArrayToggleValueChangedCallback;
                item.onIsParamsToggleValueChanged += onInputListItemIsParamsToggleValueChangedCallback;
                item.onDeleteButtonClicked += onInputListItemDeleteButtonClickedCallback;
            }, itemRect =>
            {
                EffectEditorValueDefineItem item = itemRect.GetComponent<EffectEditorValueDefineItem>();
                item.onTypeDropdownValueChanged -= onInputListItemTypeDropdownValueChangedCallback;
                item.onNameInputFieldValueChanged -= onInputListItemNameInputFieldValueChangedCallback;
                item.onIsArrayToggleValueChanged -= onInputListItemIsArrayToggleValueChangedCallback;
                item.onIsParamsToggleValueChanged -= onInputListItemIsParamsToggleValueChangedCallback;
                item.onDeleteButtonClicked -= onInputListItemDeleteButtonClickedCallback;
            });
        }
        public void updateConstList((int typeIndex, string name)[] viewData)
        {
            UIHelper.updateList(_constListRoot, _constTemplate, _constList, viewData.Length, (i, itemRect) =>
            {
                EffectEditorValueDefineItem item = itemRect.GetComponent<EffectEditorValueDefineItem>();
                item.setType(viewData[i].typeIndex);
                item.setName(viewData[i].name);
            }, itemRect =>
            {
                EffectEditorValueDefineItem item = itemRect.GetComponent<EffectEditorValueDefineItem>();
                item.onTypeDropdownValueChanged += onConstListItemTypeDropdownValueChangedCallback;
                item.onNameInputFieldValueChanged += onConstListItemNameInputFieldValueChangedCallback;
                item.onDeleteButtonClicked += onConstListItemDeleteButtonClickedCallback;
            },
            itemRect =>
            {
                EffectEditorValueDefineItem item = itemRect.GetComponent<EffectEditorValueDefineItem>();
                item.onTypeDropdownValueChanged -= onConstListItemTypeDropdownValueChangedCallback;
                item.onNameInputFieldValueChanged -= onConstListItemNameInputFieldValueChangedCallback;
                item.onDeleteButtonClicked -= onConstListItemDeleteButtonClickedCallback;
            });
        }
        public void updateOutputList((int typeIndex, string name, bool isArray)[] viewData)
        {
            UIHelper.updateList(_outputListRoot, _outputListTemplate, _outputList, viewData.Length, (i, itemRect) =>
            {
                EffectEditorValueDefineItem item = itemRect.GetComponent<EffectEditorValueDefineItem>();
                item.setType(viewData[i].typeIndex);
                item.setName(viewData[i].name);
                item.setIsArray(viewData[i].isArray);
            }, itemRect =>
            {
                EffectEditorValueDefineItem item = itemRect.GetComponent<EffectEditorValueDefineItem>();
                item.onTypeDropdownValueChanged += onOutputListItemTypeDropdownValueChangedCallback;
                item.onNameInputFieldValueChanged += onOutputListItemNameInputFieldValueChangedCallback;
                item.onIsArrayToggleValueChanged += onOutputListItemIsArrayToggleValueChangedCallback;
                item.onDeleteButtonClicked += onOutputListItemDeleteButtonClickedCallback;
            }, itemRect =>
            {
                EffectEditorValueDefineItem item = itemRect.GetComponent<EffectEditorValueDefineItem>();
                item.onTypeDropdownValueChanged -= onOutputListItemTypeDropdownValueChangedCallback;
                item.onNameInputFieldValueChanged -= onOutputListItemNameInputFieldValueChangedCallback;
                item.onIsArrayToggleValueChanged -= onOutputListItemIsArrayToggleValueChangedCallback;
                item.onDeleteButtonClicked -= onOutputListItemDeleteButtonClickedCallback;
            });
        }
        public void initActiveRanges(string[] activeRangeNames, string[] pileValues)
        {
            _effectPropListTemplate.GetComponent<ValueInputField>().initPileNames(activeRangeNames, pileValues);
        }
        public void initEffectTagNames(string[] effectTagNames, string[] effectTagValues)
        {
            _effectPropListTemplate.GetComponent<ValueInputField>().initEffectTagNames(effectTagNames, effectTagValues);
        }
        public void initKeywords(string[] keywordNames, string[] keywordValues)
        {
            _effectPropListTemplate.GetComponent<ValueInputField>().initKeywords(keywordNames, keywordValues);
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
        public ValueInputField getPropItemAt(int index)
        {
            return _effectPropList[index].GetComponent<ValueInputField>();
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _nameInputField.onEndEdit.AddListener(onEndEditActionDefineNameInputFieldCallback);
            _categoryDropdown.onValueChanged.AddListener(onActionDefineCategoryDropdownValueChangedCallback);
            if (_inputTemplate.transform.parent == _inputListRoot)
            {
                _inputTemplate.gameObject.SetActive(false);
            }
            _addInputButton.onClick.AddListener(onAddInputButtonClickCallback);
            if (_constTemplate.transform.parent == _constListRoot)
            {
                _constTemplate.gameObject.SetActive(false);
            }
            _addConstButton.onClick.AddListener(onAddConstButtonClickCallback);
            if (_outputListTemplate.transform.parent == _outputListRoot)
            {
                _outputListTemplate.gameObject.SetActive(false);
            }
            _addOutputButton.onClick.AddListener(onAddOutputButtonClickCallback);
            if (_effectPropListTemplate.transform.parent == _effectPropListRoot)
            {
                _effectPropListTemplate.gameObject.SetActive(false);
            }
            _deleteActionDefineButton.onClick.AddListener(onDeleteActionDefineButtonClickedCallback);
            _returnButton.onClick.AddListener(onClickReturnButtonCallback);
        }
        #endregion
        #region 事件回调
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
        private void onClickPropItemButtonCallback(ValueInputField item, int index)
        {
            onClickPropItemButton?.Invoke(_effectPropList.IndexOf(item.transform as RectTransform), index);
        }
        private void onClickPropAddItemButtonCallback(ValueInputField item)
        {
            onClickPropAddItemButton?.Invoke(_effectPropList.IndexOf(item.transform as RectTransform));
        }
        private void onClickPropItemRemoveButtonCallback(ValueInputField item, int index)
        {
            onClickPropItemRemoveButton?.Invoke(_effectPropList.IndexOf(item.transform as RectTransform), index);
        }
        private void onEndEditActionDefineNameInputFieldCallback(string value)
        {
            onActionDefineNameInputFieldValueChanged?.Invoke(value);
        }
        private void onActionDefineCategoryDropdownValueChangedCallback(int index)
        {
            onActionDefineCategoryDropdownValueChanged?.Invoke(index);
        }
        private void onInputListItemTypeDropdownValueChangedCallback(EffectEditorValueDefineItem item, int typeIndex)
        {
            onInputListItemTypeChanged?.Invoke(_inputList.IndexOf(item.transform as RectTransform), typeIndex);
        }
        private void onInputListItemNameInputFieldValueChangedCallback(EffectEditorValueDefineItem item, string name)
        {
            onInputListItemNameChanged?.Invoke(_inputList.IndexOf(item.transform as RectTransform), name);
        }
        private void onInputListItemIsArrayToggleValueChangedCallback(EffectEditorValueDefineItem item, bool value)
        {
            onInputListItemIsArrayValueChanged?.Invoke(_inputList.IndexOf(item.transform as RectTransform), value);
        }
        private void onInputListItemIsParamsToggleValueChangedCallback(EffectEditorValueDefineItem item, bool value)
        {
            onInputListItemIsParamsValueChanged?.Invoke(_inputList.IndexOf(item.transform as RectTransform), value);
        }
        private void onInputListItemDeleteButtonClickedCallback(EffectEditorValueDefineItem item)
        {
            onInputListItemDeleteButtonClicked?.Invoke(_inputList.IndexOf(item.transform as RectTransform));
        }
        private void onAddInputButtonClickCallback()
        {
            onClickAddInputButton?.Invoke();
        }
        private void onConstListItemTypeDropdownValueChangedCallback(EffectEditorValueDefineItem item, int typeIndex)
        {
            onConstListItemTypeChanged?.Invoke(_constList.IndexOf(item.transform as RectTransform), typeIndex);
        }
        private void onConstListItemNameInputFieldValueChangedCallback(EffectEditorValueDefineItem item, string name)
        {
            onConstListItemNameChanged?.Invoke(_constList.IndexOf(item.transform as RectTransform), name);
        }
        private void onConstListItemDeleteButtonClickedCallback(EffectEditorValueDefineItem item)
        {
            onConstListItemDeleteButtonClicked?.Invoke(_constList.IndexOf(item.transform as RectTransform));
        }
        private void onAddConstButtonClickCallback()
        {
            onClickAddConstButton?.Invoke();
        }
        private void onOutputListItemTypeDropdownValueChangedCallback(EffectEditorValueDefineItem item, int typeIndex)
        {
            onOutputListItemTypeChanged?.Invoke(_outputList.IndexOf(item.transform as RectTransform), typeIndex);
        }
        private void onOutputListItemNameInputFieldValueChangedCallback(EffectEditorValueDefineItem item, string name)
        {
            onOutputListItemNameChanged?.Invoke(_outputList.IndexOf(item.transform as RectTransform), name);
        }
        private void onOutputListItemIsArrayToggleValueChangedCallback(EffectEditorValueDefineItem item, bool value)
        {
            onOutputListItemIsArrayValueChanged?.Invoke(_outputList.IndexOf(item.transform as RectTransform), value);
        }
        private void onOutputListItemDeleteButtonClickedCallback(EffectEditorValueDefineItem item)
        {
            onOutputListItemDeleteButtonClicked?.Invoke(_outputList.IndexOf(item.transform as RectTransform));
        }
        private void onAddOutputButtonClickCallback()
        {
            onClickAddOutputButton?.Invoke();
        }
        private void onDeleteActionDefineButtonClickedCallback()
        {
            onDeleteActionDefineButtonClicked?.Invoke();
        }
        private void onClickReturnButtonCallback()
        {
            onClickReturnButton?.Invoke();
        }
        #endregion
        #endregion
        #region 事件
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
        /// 当更改名字输入栏的值，参数为更改的字符串
        /// </summary>
        public event Action<string> onActionDefineNameInputFieldValueChanged;
        public event Action<int> onActionDefineCategoryDropdownValueChanged;
        public event Action<int, int> onInputListItemTypeChanged;
        public event Action<int, string> onInputListItemNameChanged;
        public event Action<int, bool> onInputListItemIsArrayValueChanged;
        public event Action<int, bool> onInputListItemIsParamsValueChanged;
        public event Action<int> onInputListItemDeleteButtonClicked;
        public event Action onClickAddInputButton;
        public event Action<int, int> onConstListItemTypeChanged;
        public event Action<int, string> onConstListItemNameChanged;
        public event Action<int, bool> onConstListItemIsArrayValueChanged;
        public event Action<int, bool> onConstListItemIsParamsValueChanged;
        public event Action<int> onConstListItemDeleteButtonClicked;
        public event Action onClickAddConstButton;
        public event Action<int, int> onOutputListItemTypeChanged;
        public event Action<int, string> onOutputListItemNameChanged;
        public event Action<int, bool> onOutputListItemIsArrayValueChanged;
        public event Action<int, bool> onOutputListItemIsParamsValueChanged;
        public event Action<int> onOutputListItemDeleteButtonClicked;
        public event Action onClickAddOutputButton;
        public event Action onDeleteActionDefineButtonClicked;
        /// <summary>
        /// 当点击返回按钮
        /// </summary>
        public event Action onClickReturnButton;
        #endregion
        #region 属性字段
        [SerializeField]
        GameObject _effectRoot;
        [SerializeField]
        RectTransform _effectPropListRoot;
        [SerializeField]
        RectTransform _effectPropListTemplate;
        [SerializeField]
        List<RectTransform> _effectPropList = new List<RectTransform>();
        [SerializeField]
        GameObject _actionDefineRoot;
        [SerializeField]
        InputField _nameInputField;
        [SerializeField]
        Dropdown _categoryDropdown;
        [SerializeField]
        RectTransform _inputListRoot;
        [SerializeField]
        RectTransform _inputTemplate;
        [SerializeField]
        List<RectTransform> _inputList = new List<RectTransform>();
        [SerializeField]
        Button _addInputButton;
        [SerializeField]
        RectTransform _constListRoot;
        [SerializeField]
        RectTransform _constTemplate;
        [SerializeField]
        List<RectTransform> _constList = new List<RectTransform>();
        [SerializeField]
        Button _addConstButton;
        [SerializeField]
        RectTransform _outputListRoot;
        [SerializeField]
        RectTransform _outputListTemplate;
        [SerializeField]
        List<RectTransform> _outputList = new List<RectTransform>();
        [SerializeField]
        Button _addOutputButton;
        [SerializeField]
        Button _deleteActionDefineButton;
        [SerializeField]
        Button _returnButton;
        #endregion
        }
}