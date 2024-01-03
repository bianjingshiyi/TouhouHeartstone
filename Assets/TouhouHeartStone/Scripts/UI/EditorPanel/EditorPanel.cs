using System;
using System.Linq;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    class EditorPanel : CardViewPanel
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
        public void openSelectCardPoolDependencyDialog((string name, Sprite spr, bool selected)[] viewDatas)
        {
            _addCardPoolDepDialogObj.SetActive(true);
            _addCardPoolDepDialog.updateAddCardPools(viewDatas);
        }
        public void hideSelectCardPoolDependencyDialog()
        {
            _addCardPoolDepDialogObj.SetActive(false);
        }
        public void updateCardPoolDependencies(string[] names)
        {
            _cardPoolDepList.updateList(names.Length, (i, rect) =>
            {
                var name = names[i];
                var item = rect.GetComponent<CardPoolDependencyItem>();
                item.update(name);
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
        protected override void Awake()
        {
            base.Awake();
            _newCardButton.onClick.AddListener(onClickNewCardButtonCallback);
            _pasteCardButton.onClick.AddListener(onClickPasteCardButtonCallback);
            _exportCardPoolButton.onClick.AddListener(onClickExportCardPoolButtonCallback);
            _importCardPoolButton.onClick.AddListener(onClickImportCardPoolButtonCallback);

            _addRuleLogicDropdown.onClickItem.AddListener(onClickAddRuleItemCallback);
            _createEventRuleDialog.onConfirm += onConfirmCreateEventRuleCallback;

            _ruleTabButton.onClick.AddListener(() => displayTabPage(_ruleTabPage));
            _cardPoolDepTabButton.onClick.AddListener(() => displayTabPage(_cardPoolDepTabPage));

            _addCardPoolDepButton.onClick.AddListener(() => onClickAddCardPoolDepButton?.Invoke());
        }
        #endregion

        #region 事件回调
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
        #endregion

        #region 属性字段

        [Header("编辑器")]
        [SerializeField]
        Text _editorTitle;
        [SerializeField]
        Button _newCardButton;
        [SerializeField]
        Button _pasteCardButton;
        [SerializeField]
        Button _exportCardPoolButton;
        [SerializeField]
        Button _importCardPoolButton;

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

        #endregion
    }
}