using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CreateEventEffectDialog : MonoBehaviour
    {
        public void setTypes(string[] types)
        {
            _typeDropdown.ClearOptions();
            _typeDropdown.AddOptions(types.ToList());
        }
        public void setEventValue(int index)
        {
            _typeDropdown.SetValueWithoutNotify(index);
        }
        public void display()
        {
            gameObject.SetActive(true);
        }
        public void hide()
        {
            gameObject.SetActive(false);
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _confirmButton.onClick.AddListener(onConfirmClicked);
            _cancelButton.onClick.AddListener(onCancelClicked);
        }
        #endregion
        #region 事件回调
        private void onConfirmClicked()
        {
            onConfirm?.Invoke(_typeDropdown.value);
        }
        private void onCancelClicked()
        {
            hide();
        }
        #endregion

        #endregion

        #region 事件
        public event Action<int> onConfirm;
        #endregion
        [SerializeField]
        private Dropdown _typeDropdown;
        [SerializeField]
        private Button _confirmButton;
        [SerializeField]
        private Button _cancelButton;
    }
}