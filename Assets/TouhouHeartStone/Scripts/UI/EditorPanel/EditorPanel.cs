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
        public void initRuleNames(string[] names)
        {
            _addRuleLogicDropdown.ClearOptions();
            _addRuleLogicDropdown.AddOptions(names.ToList());
        }
        public void initEventNames(string[] names)
        {
            _createEventRuleDialog.setTypes(names);
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
            },
            rect =>
            {
                var effectListItem = rect.GetComponent<EffectListItem>();
                effectListItem.onEditButtonClick -= onClickRuleItemCallback;
            });
        }
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
        private void onClickAddRuleItemCallback(int index)
        {
            onClickAddRuleItem?.Invoke(index);
        }
        private void onClickRuleItemCallback(EffectListItem ruleItem)
        {
            onClickRuleItem?.Invoke(_ruleLogicElementList.indexOf(ruleItem));
        }
        private void onConfirmCreateEventRuleCallback(int index)
        {
            onConfrimCreateEventRule?.Invoke(index);
        }

        #endregion

        #endregion

        #region 事件
        public event Action onClickNewCardButton;
        public event Action onClickPasteCardButton;
        public event Action onClickExportCardPoolButton;
        public event Action onClickImportCardPoolButton;
        public event Action<int> onClickAddRuleItem;
        public event Action<int> onClickRuleItem;
        public event Action<int> onConfrimCreateEventRule;
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

        [Header("规则编辑器")]
        [SerializeField]
        DropdownMenu _addRuleLogicDropdown;
        [SerializeField]
        ElementList _ruleLogicElementList;
        [SerializeField]
        GameObject _createEventRuleDialogObj;
        [SerializeField]
        CreateEventEffectDialog _createEventRuleDialog;

        #endregion
    }
}