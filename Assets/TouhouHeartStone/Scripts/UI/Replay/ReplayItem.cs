using System;
using BJSYGameCore.UI;
using Game;
using TMPro;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    class ReplayItem : UIObject
    {
        #region 公有方法
        public void updateView(ReplayItemViewData viewData)
        {
            _playerAImage.sprite = viewData.playerAMasterSprite;
            _playerBImage.sprite = viewData.playerBMasterSprite;
            _nameText.text = viewData.name;
        }
        public void setSelected(bool selected)
        {
            _selectedFrameObj.SetActive(selected);
        }
        #endregion

        #region 私有方法
        protected override void Awake()
        {
            _itemButton.onClick.AddListener(() => onClick?.Invoke(this));
            _deleteButton.onClick.AddListener(() => onDeleteClick?.Invoke(this));
        }
        #endregion

        #region 事件
        public event Action<ReplayItem> onClick;
        public event Action<ReplayItem> onDeleteClick;
        #endregion

        #region 属性字段
        [SerializeField]
        private Image _playerAImage;
        [SerializeField]
        private Image _playerBImage;
        [SerializeField]
        private Button _itemButton;
        [SerializeField]
        private Button _deleteButton;
        [SerializeField]
        private TextMeshProUGUI _nameText;
        [SerializeField]
        private GameObject _selectedFrameObj;
        #endregion
    }
    public class ReplayItemViewData
    {
        public Sprite playerAMasterSprite;
        public Sprite playerBMasterSprite;
        public string name;
    }
}
