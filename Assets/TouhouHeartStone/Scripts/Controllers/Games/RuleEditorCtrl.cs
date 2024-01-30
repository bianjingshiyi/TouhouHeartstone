using System;
using System.Linq;
using System.Threading.Tasks;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Controllers
{
    class RuleEditorCtrl : CardPoolEditor
    {
        #region 公有方法
        /// <summary>
        /// 打开一个自定义规则进行编辑
        /// </summary>
        public async Task display(long cardPoolId, GeneratedRuleLogic ruleLogic)
        {
            gameObject.SetActive(true);
            _ui.reset();
            _ui.setName(ruleLogic.name);
            _curCardPoolId = cardPoolId;
            _ruleLogic = ruleLogic;

            await _actionNodeGraphCtrl.display(new ActionNodeGraphCtrl.DisplayInfo(cardPoolId, ruleLogic));
            _graphEditor.display(_curCardPoolId);
        }
        public override async Task display(CardPoolDisplayPage info)
        {
            if (info == null)
                return;
            displayInfo = info;
            if (info?.displayObj?.ruleLogic is GeneratedRuleLogic ruleLogic)
            {
                await display(info.cardPoolId, ruleLogic);
                if (info.unsaved)
                    markUnsaved();
                else
                    clearUnsaved();
            }
        }
        public override void markUnsaved()
        {
            base.markUnsaved();
            _ui.setTitleText(scene.editor.getTitleText(hasUnsavedChanges, game.lang._(RULE_EDITOR_TITLE)));
        }
        public override void clearUnsaved()
        {
            base.clearUnsaved();
            _ui.setTitleText(scene.editor.getTitleText(hasUnsavedChanges, game.lang._(RULE_EDITOR_TITLE)));
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            if (_ui == null)
            {
                _ui = GetComponentInChildren<RuleEditorPanel>();
            }
            _ui.onClickDeleteButton += onClickDeleteRuleButtonCallback;
            _ui.onClickSaveButton += onClickSaveButton;
            _ui.onClickCloseButton += onClickCloseButton;
            _ui.onNameChanged += onNameChangedCallback;

            _actionNodeGraphCtrl.OnGraphEdited += onGraphEdited;
        }
        #endregion
        #region 事件回调
        private void onGraphEdited()
        {
            markUnsaved();
        }
        private void onClickDeleteRuleButtonCallback()
        {
            //询问玩家
            game.ui.dialog.ShowQuestion(
               game.langManager.getString(StringTable.WARNING),
               game.langManager.getString(EDITOR_WARNING_DELETE_RULE),
               async () =>
               {
                   //删除动作定义
                   game.card.removeRuleLogic(_curCardPoolId, _ruleLogic.id);
                   scene.editor.markCardPoolUnsaved(_curCardPoolId);
                   //返回上个界面
                   await exit(true);
               });
        }
        private void onClickSaveButton()
        {
            saveLogic();
        }
        private async void onClickCloseButton()
        {
            if (hasUnsavedChanges)
            {
                string title = game.lang._(StringTable.WARNING);

                string desc = game.lang._(RULE_WARNING_UNSAVED_CHANGES);
                string[] optNames = new string[]
                {
                    game.lang._(RULE_WARNING_OPTION_SAVE),
                    game.lang._(RULE_WARNING_OPTION_DONT_SAVE),
                    game.lang._(RULE_WARNING_OPTION_CANCEL)
                };
                game.ui.dialog.ShowOptions(title, desc, optNames, async (index) =>
                {
                    if (index == 0)
                    {
                        saveLogic();
                        await exit();
                    }
                    else if (index == 1)
                    {
                        scene.editor.removeDisplayPageOfObject(_ruleLogic);
                        await exit();
                    }
                });
            }
            else
            {
                await exit();
            }
        }
        private void onNameChangedCallback(string name)
        {
            markUnsaved();
            _ruleLogic.name = name;
            _ui.setName(name);
        }
        #endregion
        protected override void setUIEffects(string[] names, int activeIndex)
        {
        }
        /// <summary>
        /// 返回上一个界面
        /// </summary>
        private async Task exit(bool forceSave = false)
        {
            clearUnsaved();
            await scene.editor.closeEditor(forceSave);
        }
        private void saveLogic()
        {
            var poolRef = getCurrentCardPoolRef();
            if (game.card.getRuleLogic(poolRef, _ruleLogic.id) is GeneratedRuleLogic gen)
            {
                game.card.removeRuleLogic(gen);
            }
            game.card.addRuleLogic(_curCardPoolId, _ruleLogic);

            scene.editor.markCardPoolUnsaved(_curCardPoolId);
            clearUnsaved();
        }
        #endregion


        RuleEditorPanel _ui;
        [SerializeField]
        ActionNodeGraphCtrl _actionNodeGraphCtrl;


        [TranslateMsg("动作编辑器/标题")]
        public const string RULE_EDITOR_TITLE = "规则编辑器";

        [TranslateMsg("编辑器中当玩家删除一个自定义规则的时候发出的警告")]
        public const string EDITOR_WARNING_DELETE_RULE = "是否要删除该自定义规则？";
        [TranslateMsg("尝试退出自定义规则的编辑页面，但没有保存情况下的警告")]
        public const string RULE_WARNING_UNSAVED_CHANGES = "是否将更改保存到该自定义规则中？";
        [TranslateMsg("尝试退出自定义规则的编辑页面，但没有保存情况下的按钮名称")]
        public const string RULE_WARNING_OPTION_SAVE = "保存";
        [TranslateMsg("尝试退出自定义规则的编辑页面，但没有保存情况下的按钮名称")]
        public const string RULE_WARNING_OPTION_DONT_SAVE = "不保存";
        [TranslateMsg("尝试退出自定义规则的编辑页面，但没有保存情况下的按钮名称")]
        public const string RULE_WARNING_OPTION_CANCEL = "取消";

        /// <summary>
        /// 正在编辑的动作定义。
        /// </summary>
        GeneratedRuleLogic _ruleLogic;
    }
}