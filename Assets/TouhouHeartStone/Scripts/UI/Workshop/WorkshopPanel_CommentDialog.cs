using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public partial class WorkshopPanel
    {
        #region 公有方法

        #region 选择上传卡池面板
        public void showCommentDialog()
        {
            _commentDialogObj.SetActive(true);
            _commentDialog.reset();
        }
        public void hideCommentDialog()
        {
            _commentDialogObj.SetActive(false);
        }
        #endregion

        #endregion

        #region 私有方法
        private void Awake_CommentDialog()
        {
            _commentDialog.onClose += () => onCommentDialogClose?.Invoke();
            _commentDialog.onSubmit += (comment, pos) => onSubmitComment?.Invoke(comment, pos);
        }
        #endregion

        #region 事件
        public event Action onCommentDialogClose;
        public event Action<string, bool> onSubmitComment;
        #endregion

        #region 属性字段
        [Header("Comment Dialog")]
        [SerializeField]
        private GameObject _commentDialogObj;
        [SerializeField]
        private CommentDialog _commentDialog;
        #endregion

    }
}