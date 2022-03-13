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
        #region 私有方法
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _newCardButton.onClick.AddListener(onClickNewCardButtonCallback);
        }
        #endregion
        #region 事件回调
        private void onClickNewCardButtonCallback()
        {
            onClickNewCardButton?.Invoke();
        }
        #endregion
        #endregion
        #region 事件
        public event Action onClickNewCardButton;
        #endregion
        #region 属性字段
        [SerializeField]
        Button _newCardButton;
        #endregion
    }
}