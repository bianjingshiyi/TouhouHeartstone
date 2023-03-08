using System;
using UnityEngine.UI;
using UnityEngine;

namespace UI
{
    public partial class RoomListItem
    {
        #region 公有方法
        public void ShowLockedIcon(bool value)
        {
            _lockedIcon.SetActive(value);
        }
        #endregion
        #region 私有字段
        [Header("密码")]
        [SerializeField]
        private GameObject _lockedIcon;
        #endregion
    }
}
