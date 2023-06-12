using System.Collections.Generic;
using UnityEngine;
using Game;
using BJSYGameCore;
using System.IO;
using System;
using UnityEngine.UI;

namespace UI
{
    class EditorPanel : CardViewPanel
    {
        public void setTitleText(string text)
        {
            _editorTitle.text = text;
        }
        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _newCardButton.onClick.AddListener(onClickNewCardButtonCallback);
            _exportCardPoolButton.onClick.AddListener(onClickExportCardPoolButtonCallback);
            _importCardPoolButton.onClick.AddListener(onClickImportCardPoolButtonCallback);
        }
        #endregion
        #region 事件回调
        private void onClickNewCardButtonCallback()
        {
            onClickNewCardButton?.Invoke();
        }
        private void onClickExportCardPoolButtonCallback()
        {
            onClickExportCardPoolButton?.Invoke();
        }
        private void onClickImportCardPoolButtonCallback()
        {
            onClickImportCardPoolButton?.Invoke();
        }
        #endregion
        #endregion
        #region 事件
        public event Action onClickNewCardButton;
        public event Action onClickExportCardPoolButton;
        public event Action onClickImportCardPoolButton;
        #endregion
        #region 属性字段
        [SerializeField]
        Text _editorTitle;
        [SerializeField]
        Button _newCardButton;
        [SerializeField]
        Button _exportCardPoolButton;
        [SerializeField]
        Button _importCardPoolButton;
        #endregion
    }
}