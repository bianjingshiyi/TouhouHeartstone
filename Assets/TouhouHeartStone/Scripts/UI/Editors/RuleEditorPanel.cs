using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RuleEditorPanel : EditorPanelBase
    {
        #region 公有方法

        public void setName(string name)
        {
            _nameInputField.SetTextWithoutNotify(name);
        }

        #endregion

        #region 私有方法

        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _cardPageButton.onClick.AddListener(onClickRulePageButtonCallback);
            _nameInputField.onEndEdit.AddListener(onNameChangedCallback);
        }
        #endregion

        #region 事件回调
        private void onClickRulePageButtonCallback()
        {
            displayPage(_cardPage, _cardPageIndicator);
        }
        private void onNameChangedCallback(string name)
        {
            onNameChanged?.Invoke(name);
        }
        #endregion

        public override void reset()
        {
            displayPage(_cardPage, _cardPageIndicator);
        }

        protected override IEnumerable<GameObject> getPages()
        {
            return base.getPages().Append(_cardPage);
        }
        protected override IEnumerable<GameObject> getPageIndicators()
        {
            return base.getPageIndicators().Append(_cardPageIndicator);
        }

        #endregion

        #region 事件
        public event Action<string> onNameChanged;
        #endregion

        [Header("Rule Page")]
        [SerializeField]
        InputField _nameInputField;

        [Header("Pages")]
        [SerializeField]
        Button _cardPageButton;
        [SerializeField]
        GameObject _cardPage;
        [SerializeField]
        GameObject _cardPageIndicator;
    }
}