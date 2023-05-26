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
        public void setCategory(string category)
        {
            _categoryInputField.SetTextWithoutNotify(category);
        }
        public void setActionPortTypes((Type type, string name)[] types)
        {
            _createPortDefineDialog.setTypes(types);
        }
        public void showCreatePortDefineDialogErrorTip(string errorTip)
        {
            _createPortDefineDialog.showErrorTip(errorTip);
        }
        public void displayCreatePortDefineDialog(ActionPortDefineType portType, string title, bool canBeParams = false)
        {
            _createPortDefineDialog.display(portType, title, canBeParams);
        }
        public void hideCreatePortDefineDialog()
        {
            _createPortDefineDialog.hide();
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
                input.setShowParams(viewData.showParams);
                input.setIsParams(viewData.isParams);
            },
            rect =>
            {
                var input = rect.GetComponent<EffectEditorValueDefineItem>();
                input.onDeleteButtonClicked += onDeleteInputCallback;
            },
            rect =>
            {
                var input = rect.GetComponent<EffectEditorValueDefineItem>();
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
                cst.setShowParams(viewData.showParams);
                cst.setIsParams(viewData.isParams);
            },
            rect =>
            {
                var cst = rect.GetComponent<EffectEditorValueDefineItem>();
                cst.onDeleteButtonClicked += onDeleteConstCallback;
            },
            rect =>
            {
                var cst = rect.GetComponent<EffectEditorValueDefineItem>();
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
                output.setShowParams(viewData.showParams);
                output.setIsParams(viewData.isParams);
            },
            rect =>
            {
                var output = rect.GetComponent<EffectEditorValueDefineItem>();
                output.onDeleteButtonClicked += onDeleteOutputCallback;
            },
            rect =>
            {
                var output = rect.GetComponent<EffectEditorValueDefineItem>();
                output.onDeleteButtonClicked -= onDeleteOutputCallback;
            });
        }
        #endregion

        #endregion

        public void reset()
        {
            displayPage(_actionPage, _actionPageIndicator);
        }
        #endregion

        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _nameInputField.onEndEdit.AddListener(onChangeNameCallback);
            _categoryInputField.onValueChanged.AddListener(onChangeCategoryCallback);

            _createPortDefineDialog.onConfirm += onCreatePortDefineCallback;

            _addInputItemButton.onClick.AddListener(onAddInputCallback);
            _addConstItemButton.onClick.AddListener(onAddConstCallback);
            _addOutputItemButton.onClick.AddListener(onAddOutputCallback);

            _deleteCardButton.onClick.AddListener(onClickDeleteActionButtonCallback);
            _saveCardButton.onClick.AddListener(onClickSaveButtonCallback);
            _closeCardButton.onClick.AddListener(onClickCloseButtonCallback);

            _actionPageButton.onClick.AddListener(onClickBasicPageButtonCallback);
            _propertiesPageButton.onClick.AddListener(onClickPropertiesPageButtonCallback);
            _nodesPageButton.onClick.AddListener(onClickNodesPageButtonCallback);
        }

        #endregion

        #region 事件回调

        void onChangeNameCallback(string name)
        {
            onChangeName?.Invoke(name);
        }
        void onChangeCategoryCallback(string category)
        {
            onChangeCategory?.Invoke(category);
        }
        void onClickBasicPageButtonCallback()
        {
            displayPage(_actionPage, _actionPageIndicator);
        }
        void onClickPropertiesPageButtonCallback()
        {
            displayPage(_propertiesPage, _propertiesPageIndicator);
        }
        void onClickNodesPageButtonCallback()
        {
            displayPage(_nodesPage, _nodesPageIndicator);
        }
        private void onCreatePortDefineCallback(ActionPortDefineItem item)
        {
            onCreatePortDefine?.Invoke(item);
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
        void onDeleteOutputCallback(EffectEditorValueDefineItem item)
        {
            var itemIndex = _outputItemList.IndexOf(item.transform as RectTransform);
            onDeleteOutput?.Invoke(itemIndex);
        }
        #endregion


        #endregion

        private void displayPage(GameObject pageObj, GameObject indicator)
        {
            _actionPage.SetActive(false);
            _propertiesPage.SetActive(false);
            _nodesPage.SetActive(false);

            _actionPageIndicator.SetActive(false);
            _propertiesPageIndicator.SetActive(false);
            _nodesPageIndicator.SetActive(false);

            pageObj.SetActive(true);
            indicator.SetActive(true);
        }
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
        public event Action<string> onChangeCategory;
        /// <summary>
        /// 当创建新的端口定义。
        /// 参数：端口定义项。
        /// </summary>
        public event Action<ActionPortDefineItem> onCreatePortDefine;
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
        InputField _categoryInputField;
        [SerializeField]
        CreateActionPortDefineDialog _createPortDefineDialog;

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
        Button _actionPageButton;
        [SerializeField]
        Button _propertiesPageButton;
        [SerializeField]
        Button _nodesPageButton;
        [SerializeField]
        GameObject _actionPage;
        [SerializeField]
        GameObject _propertiesPage;
        [SerializeField]
        GameObject _nodesPage;
        [SerializeField]
        GameObject _actionPageIndicator;
        [SerializeField]
        GameObject _propertiesPageIndicator;
        [SerializeField]
        GameObject _nodesPageIndicator;
        #endregion
    }

    public struct ActionPortDefineViewData
    {
        public int typeIndex;
        public string name;
        public bool isArray;
        public bool showParams;
        public bool isParams;
    }
}