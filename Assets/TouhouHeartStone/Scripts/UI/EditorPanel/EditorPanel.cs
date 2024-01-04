using System;
using System.Linq;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    class EditorPanel : MonoBehaviour
    {
        #region 公有方法
        public void setTitleText(string text)
        {
            _editorTitle.text = text;
        }
        public void setPasteButtonInteractable(bool value)
        {
            _pasteCardButton.interactable = value;
        }
        public void initEventNames(string[] names)
        {
            _createEventRuleDialog.setTypes(names);
        }
        public void displayTabPage(GameObject page)
        {
            setTabPageActive(_ruleTabPage, page);
            setTabPageActive(_cardPoolDepTabPage, page);
        }
        public void setCardPoolListActive(bool value)
        {
            _cardPoolListObj.SetActive(value);
        }
        public void setEditCardPoolPanelActive(bool value)
        {
            _cardPoolEditPanel.SetActive(value);
        }
        public void updateCurCardPoolInfo(string name, Sprite image)
        {
            _curCardPoolNameText.text = name;
            _curCardPoolImage.sprite = image;
            _cardPoolNameInputField.SetTextWithoutNotify(name);
        }
        /// <summary>
        /// 更新卡池列表
        /// </summary>
        /// <param name="deckViewData"></param>
        public void updateCardPoolList((string name, Sprite image, bool errorTip)[] deckViewData)
        {
            _cardPoolList.update(deckViewData.Select(i => (i.name, i.image, i.errorTip, false)).ToArray());
        }

        #region 游戏规则
        public void initRuleNames(string[] names)
        {
            _addRuleLogicDropdown.ClearOptions();
            _addRuleLogicDropdown.AddOptions(names.ToList());
        }
        public void openCreateEventRuleDialog()
        {
            _createEventRuleDialogObj.SetActive(true);
        }
        public void hideCreateEventRuleDialog()
        {
            _createEventRuleDialogObj.SetActive(false);
        }
        public void updateRules(string[] names)
        {
            _ruleLogicElementList.updateList(names.Length, (i, rect) =>
            {
                var name = names[i];
                var effectListItem = rect.GetComponent<EffectListItem>();
                effectListItem.update(name);
            },
            rect =>
            {
                var effectListItem = rect.GetComponent<EffectListItem>();
                effectListItem.onEditButtonClick += onClickRuleItemCallback;
                effectListItem.onRemoveButtonClick += onClickRemoveRuleItemCallback;
            },
            rect =>
            {
                var effectListItem = rect.GetComponent<EffectListItem>();
                effectListItem.onEditButtonClick -= onClickRuleItemCallback;
                effectListItem.onRemoveButtonClick -= onClickRemoveRuleItemCallback;
            });
        }
        #endregion

        #region 卡池依赖
        public void openSelectCardPoolDependencyDialog()
        {
            _addCardPoolDepDialogObj.SetActive(true);
        }
        public void hideSelectCardPoolDependencyDialog()
        {
            _addCardPoolDepDialogObj.SetActive(false);
        }
        public void updateSelectCardPoolDependencyDialog((string name, Sprite spr, bool selected)[] viewDatas)
        {
            _addCardPoolDepDialog.updateAddCardPools(viewDatas);
        }
        public void setAutoUpdateDepButtonInteractable(bool interactable)
        {
            _updateCardPoolDepsButton.interactable = interactable;
        }
        public void updateCardPoolDependencies(string[] names, bool[] outdated)
        {
            _cardPoolDepList.updateList(names.Length, (i, rect) =>
            {
                var name = names[i];
                var item = rect.GetComponent<CardPoolDependencyItem>();
                item.update(name, outdated[i]);
            },
            rect =>
            {
                var item = rect.GetComponent<CardPoolDependencyItem>();
                item.onRemoveButtonClick += onClickRemoveDependencyItemCallback;
            },
            rect =>
            {
                var item = rect.GetComponent<CardPoolDependencyItem>();
                item.onRemoveButtonClick -= onClickRemoveDependencyItemCallback;
            });
        }
        #endregion

        #endregion

        #region 私有方法

        #region 生命周期
        private void Awake()
        {
            _returnButton.onClick.AddListener(() => onClickReturnButton?.Invoke());
            _cardPoolList.onClickItemButton += index => onClickCardPoolListItem?.Invoke(index);
            _cardPoolList.onClickItemDeleteButton += index => onClickCardPoolListItemDeleteButton?.Invoke(index);

            _addCardPoolButton.onClick.AddListener(onClickAddCardPoolButtonCallback);
            _saveCardPoolButton.onClick.AddListener(onClickSaveCardPoolButtonCallback);
            _returnToCardPoolViewButton.onClick.AddListener(onClickReturnToCardPoolViewButtonCallback);
            _deleteCurCardPoolButton.onClick.AddListener(onClickDeleteCurCardPoolButtonCallback);

            _curCardPoolImageButton.onClick.AddListener(onClickEditCardPoolImageButtonCallback);
            _curCardPoolNameButton.onClick.AddListener(onClickEditCardPoolNameButtonCallback);
            _cardPoolNameInputField.onEndEdit.AddListener(onEndEditCardPoolNameInputFieldCallback);

            _newCardButton.onClick.AddListener(onClickNewCardButtonCallback);
            _pasteCardButton.onClick.AddListener(onClickPasteCardButtonCallback);
            _exportCardPoolButton.onClick.AddListener(onClickExportCardPoolButtonCallback);
            _importCardPoolButton.onClick.AddListener(onClickImportCardPoolButtonCallback);

            _addRuleLogicDropdown.onClickItem.AddListener(onClickAddRuleItemCallback);
            _createEventRuleDialog.onConfirm += onConfirmCreateEventRuleCallback;

            _ruleTabButton.onClick.AddListener(() => displayTabPage(_ruleTabPage));
            _cardPoolDepTabButton.onClick.AddListener(() => displayTabPage(_cardPoolDepTabPage));

            _addCardPoolDepButton.onClick.AddListener(() => onClickAddCardPoolDepButton?.Invoke());
            _addCardPoolDepDialog.onClickItemButton += index => onClickAddCardPoolDepItem?.Invoke(index);
            _addCardPoolDepDialog.onItemToggleValueChanged += (index, value) => onClickAddCardPoolDepItem?.Invoke(index);
            _cancelAddCardPoolDepButton.onClick.AddListener(() => hideSelectCardPoolDependencyDialog());
            _confirmAddCardPoolDepButton.onClick.AddListener(() => onClickConfirmAddCardPoolDepButton?.Invoke());
            _updateCardPoolDepsButton.onClick.AddListener(() => onClickUpdateCardPoolDepsButton?.Invoke());
        }
        #endregion

        #region 事件回调
        private void onClickAddCardPoolButtonCallback()
        {
            onClickAddCardPoolButton?.Invoke();
        }
        private void onClickSaveCardPoolButtonCallback()
        {
            onClickSaveCardPoolButton?.Invoke();
        }
        private void onClickReturnToCardPoolViewButtonCallback()
        {
            onClickReturnToCardPoolViewButton?.Invoke();
        }
        private void onClickDeleteCurCardPoolButtonCallback()
        {
            onClickDeleteCurCardPoolButton?.Invoke();
        }
        private void onClickEditCardPoolImageButtonCallback()
        {
            onClickEditCardPoolImageButton?.Invoke();
        }
        private void onClickEditCardPoolNameButtonCallback()
        {
            //编辑名称模式
            _cardPoolNameInputField.gameObject.SetActive(true);
            _cardPoolNameInputField.ActivateInputField();
            _curCardPoolNameButton.gameObject.SetActive(false);
            _curCardPoolNameText.gameObject.SetActive(false);
        }
        private void onEndEditCardPoolNameInputFieldCallback(string value)
        {
            _cardPoolNameInputField.gameObject.SetActive(false);
            _curCardPoolNameButton.gameObject.SetActive(true);
            _curCardPoolNameText.gameObject.SetActive(true);
            _curCardPoolNameText.text = value;
            onCardPoolNameInputFieldValueChanged?.Invoke(value);
        }
        private void onClickNewCardButtonCallback()
        {
            onClickNewCardButton?.Invoke();
        }
        private void onClickPasteCardButtonCallback()
        {
            onClickPasteCardButton?.Invoke();
        }
        private void onClickExportCardPoolButtonCallback()
        {
            onClickExportCardPoolButton?.Invoke();
        }
        private void onClickImportCardPoolButtonCallback()
        {
            onClickImportCardPoolButton?.Invoke();
        }

        #region 游戏规则
        private void onClickAddRuleItemCallback(int index)
        {
            onClickAddRuleItem?.Invoke(index);
        }
        private void onClickRuleItemCallback(EffectListItem ruleItem)
        {
            onClickRuleItem?.Invoke(_ruleLogicElementList.indexOf(ruleItem));
        }
        private void onClickRemoveRuleItemCallback(EffectListItem ruleItem)
        {
            onClickRemoveRuleItem?.Invoke(_ruleLogicElementList.indexOf(ruleItem));
        }
        private void onConfirmCreateEventRuleCallback(int index)
        {
            onConfrimCreateEventRule?.Invoke(index);
        }
        #endregion

        #region 卡池依赖
        private void onClickRemoveDependencyItemCallback(CardPoolDependencyItem item)
        {
            onClickRemoveCardPoolDepButton?.Invoke(_cardPoolDepList.indexOf(item));
        }
        #endregion

        #endregion

        private void setTabPageActive(GameObject page, GameObject opening)
        {
            page.SetActive(opening == page);
        }

        #endregion

        #region 事件

        #region 卡池列表
        /// <summary>
        /// 当点击卡池列表项，参数为索引
        /// </summary>
        public event Action<int> onClickCardPoolListItem;
        /// <summary>
        /// 当点击卡池列表项的删除按钮，参数为列表索引
        /// </summary>
        public event Action<int> onClickCardPoolListItemDeleteButton;
        #endregion

        public event Action onClickReturnButton;
        /// <summary>
        /// 当点击返回浏览卡池按钮
        /// </summary>
        public event Action onClickReturnToCardPoolViewButton;
        /// <summary>
        /// 当点击删除当前卡池按钮
        /// </summary>
        public event Action onClickDeleteCurCardPoolButton;
        /// <summary>
        /// 当点击修改卡池图片按钮。
        /// </summary>
        public event Action onClickEditCardPoolImageButton;
        public event Action<string> onCardPoolNameInputFieldValueChanged;
        /// <summary>
        /// 当点击右侧卡组列表添加按钮
        /// </summary>
        public event Action onClickAddCardPoolButton;
        /// <summary>
        /// 当点击保存卡池按钮
        /// </summary>
        public event Action onClickSaveCardPoolButton;


        public event Action onClickNewCardButton;
        public event Action onClickPasteCardButton;
        public event Action onClickExportCardPoolButton;
        public event Action onClickImportCardPoolButton;
        public event Action<int> onClickAddRuleItem;
        public event Action<int> onClickRuleItem;
        public event Action<int> onClickRemoveRuleItem;
        public event Action<int> onConfrimCreateEventRule;

        public event Action onClickAddCardPoolDepButton;
        public event Action<int> onClickRemoveCardPoolDepButton;
        public event Action<int> onClickAddCardPoolDepItem;
        public event Action onClickConfirmAddCardPoolDepButton;
        public event Action onClickUpdateCardPoolDepsButton;
        #endregion

        #region 属性字段


        [Header("卡池列表")]
        [SerializeField]
        GameObject _cardPoolListObj;
        [SerializeField]
        DeckViewPanel _cardPoolList;
        [SerializeField]
        Button _returnButton;
        [SerializeField]
        Button _addCardPoolButton;
        [SerializeField]
        Button _importCardPoolButton;


        [Header("编辑器")]
        [SerializeField]
        GameObject _cardPoolEditPanel;
        [SerializeField]
        InputField _cardPoolNameInputField;
        [SerializeField]
        Button _saveCardPoolButton;
        [SerializeField]
        Button _returnToCardPoolViewButton;
        [SerializeField]
        Button _deleteCurCardPoolButton;
        [SerializeField]
        Text _curCardPoolNameText;
        [SerializeField]
        Image _curCardPoolImage;
        [SerializeField]
        Button _curCardPoolImageButton;
        [SerializeField]
        Button _curCardPoolNameButton;
        [SerializeField]
        Text _editorTitle;
        [SerializeField]
        Button _newCardButton;
        [SerializeField]
        Button _pasteCardButton;
        [SerializeField]
        Button _exportCardPoolButton;

        [Header("选项卡")]
        [SerializeField]
        Button _ruleTabButton;
        [SerializeField]
        Button _cardPoolDepTabButton;
        [SerializeField]
        GameObject _ruleTabPage;
        [SerializeField]
        GameObject _cardPoolDepTabPage;

        [Header("规则编辑器")]
        [SerializeField]
        DropdownMenu _addRuleLogicDropdown;
        [SerializeField]
        ElementList _ruleLogicElementList;
        [SerializeField]
        GameObject _createEventRuleDialogObj;
        [SerializeField]
        CreateEventEffectDialog _createEventRuleDialog;

        [Header("卡池依赖")]
        [SerializeField]
        Button _addCardPoolDepButton;
        [SerializeField]
        ElementList _cardPoolDepList;
        [SerializeField]
        GameObject _addCardPoolDepDialogObj;
        [SerializeField]
        DeckViewPanel _addCardPoolDepDialog;
        [SerializeField]
        Button _cancelAddCardPoolDepButton;
        [SerializeField]
        Button _confirmAddCardPoolDepButton;
        [SerializeField]
        Button _updateCardPoolDepsButton;

        #endregion
    }
}