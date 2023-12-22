using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public abstract class EditorPanelBase : MonoBehaviour
    {
        #region 公有方法
        public void setTitleText(string title)
        {
            _editorTitle.text = title;
        }
        public virtual void reset()
        {
            displayPage(_nodesPage, _nodesPageIndicator);
        }
        #endregion

        #region 私有方法

        #region 生命周期
        protected virtual void Awake()
        {
            _deleteButton.onClick.AddListener(onClickDeleteButtonCallback);
            _saveButton.onClick.AddListener(onClickSaveButtonCallback);
            _closeButton.onClick.AddListener(onClickCloseButtonCallback);

            _nodesPageButton.onClick.AddListener(onClickNodesPageButtonCallback);
        }

        #endregion

        #region 事件回调
        void onClickNodesPageButtonCallback()
        {
            displayPage(_nodesPage, _nodesPageIndicator);
        }
        void onClickDeleteButtonCallback()
        {
            onClickDeleteButton?.Invoke();
        }
        void onClickSaveButtonCallback()
        {
            onClickSaveButton?.Invoke();
        }
        void onClickCloseButtonCallback()
        {
            onClickCloseButton?.Invoke();
        }

        #endregion

        protected virtual IEnumerable<GameObject> getPages()
        {
            yield return _nodesPage;
        }
        protected virtual IEnumerable<GameObject> getPageIndicators()
        {
            yield return _nodesPageIndicator;
        }

        protected void displayPage(GameObject pageObj, GameObject indicator)
        {
            foreach (var page in getPages())
            {
                page.SetActive(pageObj == page);
            }
            foreach (var pageIndicator in getPageIndicators())
            {
                pageIndicator.SetActive(indicator == pageIndicator);
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 当点击删除动作按钮
        /// </summary>
        public event Action onClickDeleteButton;
        /// <summary>
        /// 当点击保存按钮
        /// </summary>
        public event Action onClickSaveButton;
        /// <summary>
        /// 当点击关闭按钮
        /// </summary>
        public event Action onClickCloseButton;

        #endregion

        #region 属性字段

        [SerializeField]
        Text _editorTitle;
        [Header("General")]
        [SerializeField]
        Button _deleteButton;
        [SerializeField]
        Button _saveButton;
        [SerializeField]
        Button _closeButton;

        [Header("Pages")]
        [SerializeField]
        Button _nodesPageButton;
        [SerializeField]
        GameObject _nodesPage;
        [SerializeField]
        GameObject _nodesPageIndicator;
        #endregion
    }
}