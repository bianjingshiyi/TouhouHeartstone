using System;
using System.Collections.Generic;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ActionEditorPanel : MonoBehaviour
    {
        #region 公有方法
        #region 基础
        public void setName(string name)
        {
            _nameInputField.SetTextWithoutNotify(name);
        }
        public void setCategory(int index)
        {
            _categoryDropdown.SetValueWithoutNotify(index);
        }
        public void initCategories(string[] categories)
        {
            _categoryDropdown.ClearOptions();
            _categoryDropdown.AddOptions(new List<string>(categories));
        }
        #endregion

        #region 属性

        public void initPropertyItemTypes(string[] types, string[] constTypes)
        {
            var input = _inputItemTemplate.GetComponent<EffectEditorValueDefineItem>();
            input.initTypeDropdown(types);

            var constItem = _constItemTemplate.GetComponent<EffectEditorValueDefineItem>();
            constItem.initTypeDropdown(constTypes);

            var output = _outputItemTemplate.GetComponent<EffectEditorValueDefineItem>();
            output.initTypeDropdown(types);
        }
        #region 输入变量
        public void updateInputList(ActionPortDefineViewData[] viewDatas)
        {
            UIHelper.updateList(_inputItemRoot, _inputItemTemplate, _inputItemList, viewDatas.Length, (i, rect) =>
            {
                var input = rect.GetComponent<EffectEditorValueDefineItem>();
                var viewData = viewDatas[i];
                input.setType(viewData.typeIndex);
                input.setName(viewData.name);
                input.setIsArray(viewData.isArray);
                input.setIsParams(viewData.isParams);
            },
            rect =>
            {
                var input = rect.GetComponent<EffectEditorValueDefineItem>();
                input.onTypeDropdownValueChanged += onChangeInputTypeCallback;
                input.onNameInputFieldValueChanged += onChangeInputNameCallback;
                input.onIsArrayToggleValueChanged += onChangeInputIsArrayCallback;
                input.onIsParamsToggleValueChanged += onChangeInputIsParamsCallback;
                input.onDeleteButtonClicked += onDeleteInputCallback;
            },
            rect =>
            {
                var input = rect.GetComponent<EffectEditorValueDefineItem>();
                input.onTypeDropdownValueChanged -= onChangeInputTypeCallback;
                input.onNameInputFieldValueChanged -= onChangeInputNameCallback;
                input.onIsArrayToggleValueChanged -= onChangeInputIsArrayCallback;
                input.onIsParamsToggleValueChanged -= onChangeInputIsParamsCallback;
                input.onDeleteButtonClicked -= onDeleteInputCallback;
            });
        }
        #endregion

        #region 输入常量
        public void updateConstList(ActionPortDefineViewData[] viewDatas)
        {
            UIHelper.updateList(_constItemRoot, _constItemTemplate, _constItemList, viewDatas.Length, (i, rect) =>
            {
                var cst = rect.GetComponent<EffectEditorValueDefineItem>();
                var viewData = viewDatas[i];
                cst.setType(viewData.typeIndex);
                cst.setName(viewData.name);
                cst.setIsArray(viewData.isArray);
                cst.setIsParams(viewData.isParams);
            },
            rect =>
            {
                var cst = rect.GetComponent<EffectEditorValueDefineItem>();
                cst.onTypeDropdownValueChanged += onChangeConstTypeCallback;
                cst.onNameInputFieldValueChanged += onChangeConstNameCallback;
                cst.onIsArrayToggleValueChanged += onChangeConstIsArrayCallback;
                cst.onIsParamsToggleValueChanged += onChangeConstIsParamsCallback;
                cst.onDeleteButtonClicked += onDeleteConstCallback;
            },
            rect =>
            {
                var cst = rect.GetComponent<EffectEditorValueDefineItem>();
                cst.onTypeDropdownValueChanged -= onChangeConstTypeCallback;
                cst.onNameInputFieldValueChanged -= onChangeConstNameCallback;
                cst.onIsArrayToggleValueChanged -= onChangeConstIsArrayCallback;
                cst.onIsParamsToggleValueChanged -= onChangeConstIsParamsCallback;
                cst.onDeleteButtonClicked -= onDeleteConstCallback;
            });
        }
        #endregion


        #region 输出变量
        public void updateOutputList(ActionPortDefineViewData[] viewDatas)
        {
            UIHelper.updateList(_outputItemRoot, _outputItemTemplate, _outputItemList, viewDatas.Length, (i, rect) =>
            {
                var output = rect.GetComponent<EffectEditorValueDefineItem>();
                var viewData = viewDatas[i];
                output.setType(viewData.typeIndex);
                output.setName(viewData.name);
                output.setIsArray(viewData.isArray);
                output.setIsParams(viewData.isParams);
            },
            rect =>
            {
                var output = rect.GetComponent<EffectEditorValueDefineItem>();
                output.onTypeDropdownValueChanged += onChangeOutputTypeCallback;
                output.onNameInputFieldValueChanged += onChangeOutputNameCallback;
                output.onIsArrayToggleValueChanged += onChangeOutputIsArrayCallback;
                output.onIsParamsToggleValueChanged += onChangeOutputIsParamsCallback;
                output.onDeleteButtonClicked += onDeleteOutputCallback;
            },
            rect =>
            {
                var output = rect.GetComponent<EffectEditorValueDefineItem>();
                output.onTypeDropdownValueChanged -= onChangeOutputTypeCallback;
                output.onNameInputFieldValueChanged -= onChangeOutputNameCallback;
                output.onIsArrayToggleValueChanged -= onChangeOutputIsArrayCallback;
                output.onIsParamsToggleValueChanged -= onChangeOutputIsParamsCallback;
                output.onDeleteButtonClicked -= onDeleteOutputCallback;
            });
        }
        #endregion

        #endregion

        public void closeAllEdit()
        {
            _nameInputField.DeactivateInputField();;
        }
        #endregion

        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _nameInputField.onEndEdit.AddListener(onChangeNameCallback);
            _categoryDropdown.onValueChanged.AddListener(onChangeCategoryCallback);

            _addInputItemButton.onClick.AddListener(onAddInputCallback);
            _addConstItemButton.onClick.AddListener(onAddConstCallback);
            _addOutputItemButton.onClick.AddListener(onAddOutputCallback);

            _deleteCardButton.onClick.AddListener(onClickDeleteActionButtonCallback);
            _saveCardButton.onClick.AddListener(onClickSaveButtonCallback);
            _closeCardButton.onClick.AddListener(onClickCloseButtonCallback);

            _basicPageButton.onClick.AddListener(onClickBasicPageButtonCallback);
            _propertiesPageButton.onClick.AddListener(onClickPropertiesPageButtonCallback);
        }
        #endregion

        #region 事件回调

        void onChangeNameCallback(string name)
        {
            onChangeName?.Invoke(name);
        }
        void onChangeCategoryCallback(int categoryIndex)
        {
            onChangeCategory?.Invoke(categoryIndex);
        }
        void onClickBasicPageButtonCallback()
        {
            _basicPage.SetActive(true);
            _propertiesPage.SetActive(false);
        }
        void onClickPropertiesPageButtonCallback()
        {
            _basicPage.SetActive(false);
            _propertiesPage.SetActive(true);
        }
        void onClickDeleteActionButtonCallback()
        {
            onClickDeleteActionButton?.Invoke();
        }
        void onClickSaveButtonCallback()
        {
            onClickSaveButton?.Invoke();
        }
        void onClickCloseButtonCallback()
        {
            onClickCloseButton?.Invoke();
        }

        #region 输入变量
        void onAddInputCallback()
        {
            onAddInput?.Invoke();
        }
        void onChangeInputTypeCallback(EffectEditorValueDefineItem item, int typeIndex)
        {
            var itemIndex = _inputItemList.IndexOf(item.transform as RectTransform);
            onChangeInputType?.Invoke(itemIndex, typeIndex);
        }
        void onChangeInputNameCallback(EffectEditorValueDefineItem item, string name)
        {
            var itemIndex = _inputItemList.IndexOf(item.transform as RectTransform);
            onChangeInputName?.Invoke(itemIndex, name);
        }
        void onChangeInputIsArrayCallback(EffectEditorValueDefineItem item, bool isArray)
        {
            var itemIndex = _inputItemList.IndexOf(item.transform as RectTransform);
            onChangeInputIsArray?.Invoke(itemIndex, isArray);
        }
        void onChangeInputIsParamsCallback(EffectEditorValueDefineItem item, bool isParams)
        {
            var itemIndex = _inputItemList.IndexOf(item.transform as RectTransform);
            onChangeInputIsParams?.Invoke(itemIndex, isParams);
        }
        void onDeleteInputCallback(EffectEditorValueDefineItem item)
        {
            var itemIndex = _inputItemList.IndexOf(item.transform as RectTransform);
            onDeleteInput?.Invoke(itemIndex);
        }
        #endregion

        #region 输入常量
        void onAddConstCallback()
        {
            onAddConst?.Invoke();
        }
        void onChangeConstTypeCallback(EffectEditorValueDefineItem item, int typeIndex)
        {
            var itemIndex = _constItemList.IndexOf(item.transform as RectTransform);
            onChangeConstType?.Invoke(itemIndex, typeIndex);
        }
        void onChangeConstNameCallback(EffectEditorValueDefineItem item, string name)
        {
            var itemIndex = _constItemList.IndexOf(item.transform as RectTransform);
            onChangeConstName?.Invoke(itemIndex, name);
        }
        void onChangeConstIsArrayCallback(EffectEditorValueDefineItem item, bool isArray)
        {
            var itemIndex = _constItemList.IndexOf(item.transform as RectTransform);
            onChangeConstIsArray?.Invoke(itemIndex, isArray);
        }
        void onChangeConstIsParamsCallback(EffectEditorValueDefineItem item, bool isParams)
        {
            var itemIndex = _constItemList.IndexOf(item.transform as RectTransform);
            onChangeConstIsParams?.Invoke(itemIndex, isParams);
        }
        void onDeleteConstCallback(EffectEditorValueDefineItem item)
        {
            var itemIndex = _constItemList.IndexOf(item.transform as RectTransform);
            onDeleteConst?.Invoke(itemIndex);
        }
        #endregion

        #region 输出变量
        void onAddOutputCallback()
        {
            onAddOutput?.Invoke();
        }
        void onChangeOutputTypeCallback(EffectEditorValueDefineItem item, int typeIndex)
        {
            var itemIndex = _outputItemList.IndexOf(item.transform as RectTransform);
            onChangeOutputType?.Invoke(itemIndex, typeIndex);
        }
        void onChangeOutputNameCallback(EffectEditorValueDefineItem item, string name)
        {
            var itemIndex = _outputItemList.IndexOf(item.transform as RectTransform);
            onChangeOutputName?.Invoke(itemIndex, name);
        }
        void onChangeOutputIsArrayCallback(EffectEditorValueDefineItem item, bool isArray)
        {
            var itemIndex = _outputItemList.IndexOf(item.transform as RectTransform);
            onChangeOutputIsArray?.Invoke(itemIndex, isArray);
        }
        void onChangeOutputIsParamsCallback(EffectEditorValueDefineItem item, bool isParams)
        {
            var itemIndex = _outputItemList.IndexOf(item.transform as RectTransform);
            onChangeOutputIsParams?.Invoke(itemIndex, isParams);
        }
        void onDeleteOutputCallback(EffectEditorValueDefineItem item)
        {
            var itemIndex = _outputItemList.IndexOf(item.transform as RectTransform);
            onDeleteOutput?.Invoke(itemIndex);
        }
        #endregion


        #endregion
        #endregion

        #region 事件

        /// <summary>
        /// 当动作名称被更改。
        /// 参数：更改后的名称。
        /// </summary>
        public event Action<string> onChangeName;
        /// <summary>
        /// 当动作分类被更改。
        /// 参数：更改后的分类索引。
        /// </summary>
        public event Action<int> onChangeCategory;
        /// <summary>
        /// 当点击删除动作按钮
        /// </summary>
        public event Action onClickDeleteActionButton;
        /// <summary>
        /// 当点击保存按钮
        /// </summary>
        public event Action onClickSaveButton;
        /// <summary>
        /// 当点击关闭按钮
        /// </summary>
        public event Action onClickCloseButton;

        #region 输入变量
        /// <summary>
        /// 当添加输入变量
        /// </summary>
        public event Action onAddInput;
        /// <summary>
        /// 当输入变量的类型被更改
        /// 参数：输入变量索引、类型索引。
        /// </summary>
        public event Action<int, int> onChangeInputType;
        /// <summary>
        /// 当输入变量的名称被更改
        /// 参数：输入变量索引、名称。
        /// </summary>
        public event Action<int, string> onChangeInputName;
        /// <summary>
        /// 当输入变量的是否是数组属性被改变
        /// 参数：输入变量索引、是否是数组。
        /// </summary>
        public event Action<int, bool> onChangeInputIsArray;
        /// <summary>
        /// 当输入变量的是否是变长变量属性被改变
        /// 参数：输入变量索引、是否是变长变量。
        /// </summary>
        public event Action<int, bool> onChangeInputIsParams;
        /// <summary>
        /// 当输入变量被删除
        /// 参数：输入变量索引。
        /// </summary>
        public event Action<int> onDeleteInput;
        #endregion

        #region 输入常量
        /// <summary>
        /// 当添加输入常量
        /// </summary>
        public event Action onAddConst;
        /// <summary>
        /// 当输入常量的类型被更改
        /// 参数：输入常量索引、类型索引。
        /// </summary>
        public event Action<int, int> onChangeConstType;
        /// <summary>
        /// 当输入常量的名称被更改
        /// 参数：输入常量索引、名称。
        /// </summary>
        public event Action<int, string> onChangeConstName;
        /// <summary>
        /// 当输入常量的是否是数组属性被改变
        /// 参数：输入常量索引、是否是数组。
        /// </summary>
        public event Action<int, bool> onChangeConstIsArray;
        /// <summary>
        /// 当输入常量的是否是变长变量属性被改变
        /// 参数：输入常量索引、是否是变长变量。
        /// </summary>
        public event Action<int, bool> onChangeConstIsParams;
        /// <summary>
        /// 当输入常量被删除
        /// 参数：输入常量索引。
        /// </summary>
        public event Action<int> onDeleteConst;
        #endregion

        #region 输出变量
        /// <summary>
        /// 当添加输出变量
        /// </summary>
        public event Action onAddOutput;
        /// <summary>
        /// 当输出变量的类型被更改
        /// 参数：输出变量索引、类型索引。
        /// </summary>
        public event Action<int, int> onChangeOutputType;
        /// <summary>
        /// 当输出变量的名称被更改
        /// 参数：输出变量索引、名称。
        /// </summary>
        public event Action<int, string> onChangeOutputName;
        /// <summary>
        /// 当输出变量的是否是数组属性被改变
        /// 参数：输出变量索引、是否是数组。
        /// </summary>
        public event Action<int, bool> onChangeOutputIsArray;
        /// <summary>
        /// 当输出变量的是否是变长变量属性被改变
        /// 参数：输出变量索引、是否是变长变量。
        /// </summary>
        public event Action<int, bool> onChangeOutputIsParams;
        /// <summary>
        /// 当输出变量被删除
        /// 参数：输出变量索引。
        /// </summary>
        public event Action<int> onDeleteOutput;
        #endregion

        #endregion

        #region 属性字段

        [Header("Generic")]
        [SerializeField]
        Button _deleteCardButton;
        [SerializeField]
        Button _saveCardButton;
        [SerializeField]
        Button _closeCardButton;

        [Header("Basic")]
        [SerializeField]
        InputField _nameInputField;
        [SerializeField]
        Dropdown _categoryDropdown;

        [Header("Properties")]
        [Header("Inputs")]
        [SerializeField]
        RectTransform _inputItemRoot;
        [SerializeField]
        RectTransform _inputItemTemplate;
        [SerializeField]
        List<RectTransform> _inputItemList;
        [SerializeField]
        Button _addInputItemButton;


        [Header("Consts")]
        [SerializeField]
        RectTransform _constItemRoot;
        [SerializeField]
        RectTransform _constItemTemplate;
        [SerializeField]
        List<RectTransform> _constItemList;
        [SerializeField]
        Button _addConstItemButton;


        [Header("Outputs")]
        [SerializeField]
        RectTransform _outputItemRoot;
        [SerializeField]
        RectTransform _outputItemTemplate;
        [SerializeField]
        List<RectTransform> _outputItemList;
        [SerializeField]
        Button _addOutputItemButton;

        [Header("Pages")]
        [SerializeField]
        Button _basicPageButton;
        [SerializeField]
        Button _propertiesPageButton;
        [SerializeField]
        GameObject _basicPage;
        [SerializeField]
        GameObject _propertiesPage;
        #endregion
    }

    public struct ActionPortDefineViewData
    {
        public int typeIndex;
        public string name;
        public bool isArray;
        public bool isParams;
    }
}