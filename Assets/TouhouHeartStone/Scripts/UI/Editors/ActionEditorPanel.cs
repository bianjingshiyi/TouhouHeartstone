using System;
using System.Collections.Generic;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ActionEditorPanel : EditorPanelBase
    {
        #region 公有方法

        #region 基础
        public void setName(string name)
        {
            _nameText.text = name;
        }
        public void setCategory(string category)
        {
            _categoryInputField.SetTextWithoutNotify(category);
        }
        public void setDefineType(string name)
        {
            _defineTypeText.text = name;
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
        #region 输入变量
        public void updateInputList(ActionPortDefineViewData[] viewDatas)
        {
            UIHelper.updateList(_inputItemRoot, _inputItemTemplate, _inputItemList, viewDatas.Length, (i, rect) =>
            {
                var input = rect.GetComponent<EffectEditorValueDefineItem>();
                var viewData = viewDatas[i];
                input.setType(viewData.typeName);
                input.setName(viewData.name);
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


        #region 输出变量
        public void updateOutputList(ActionPortDefineViewData[] viewDatas)
        {
            UIHelper.updateList(_outputItemRoot, _outputItemTemplate, _outputItemList, viewDatas.Length, (i, rect) =>
            {
                var output = rect.GetComponent<EffectEditorValueDefineItem>();
                var viewData = viewDatas[i];
                output.setType(viewData.typeName);
                output.setName(viewData.name);
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

        public override void reset()
        {
            displayPage(_actionPage, _actionPageIndicator);
        }
        #endregion

        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _categoryInputField.onValueChanged.AddListener(onChangeCategoryCallback);

            _createPortDefineDialog.onConfirm += onCreatePortDefineCallback;

            _addInputItemButton.onClick.AddListener(onAddInputCallback);
            _addOutputItemButton.onClick.AddListener(onAddOutputCallback);

            _actionPageButton.onClick.AddListener(onClickBasicPageButtonCallback);
        }

        #endregion

        #region 事件回调
        void onChangeCategoryCallback(string category)
        {
            onChangeCategory?.Invoke(category);
        }
        void onClickBasicPageButtonCallback()
        {
            displayPage(_actionPage, _actionPageIndicator);
        }
        private void onCreatePortDefineCallback(ActionPortDefineItem item)
        {
            onCreatePortDefine?.Invoke(item);
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

        protected override IEnumerable<GameObject> getPages()
        {
            foreach (var page in base.getPages())
            {
                yield return page;
            }
            yield return _actionPage;
        }
        protected override IEnumerable<GameObject> getPageIndicators()
        {
            foreach (var indicator in base.getPageIndicators())
            {
                yield return indicator;
            }
            yield return _actionPageIndicator;
        }
        #endregion

        #region 事件
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

        [Header("Basic")]
        [SerializeField]
        Text _nameText;
        [SerializeField]
        InputField _categoryInputField;
        [SerializeField]
        Text _defineTypeText;
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
        GameObject _actionPage;
        [SerializeField]
        GameObject _actionPageIndicator;
        #endregion
    }

    public struct ActionPortDefineViewData
    {
        public string typeName;
        public string name;
    }
}