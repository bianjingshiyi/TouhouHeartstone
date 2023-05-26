using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MultiDropdownList : MonoBehaviour
    {
        #region 私有方法
        #region 生命周期
        private void OnEnable()
        {
            _inputField.onEndEdit.AddListener(onEndEditCallback);
            _dropdown.onShowDropdownList();
        }
        private void OnDisable()
        {
            _inputField.onEndEdit.AddListener(onEndEditCallback);
            _dropdown.onHideDropdownList();
        }
        #endregion
        #region 事件回调
        private void onEndEditCallback(string text)
        {
            if (!_dropdown.values.Contains(text) && !string.IsNullOrEmpty(text))
            {
                _dropdown.values = _dropdown.values.Append(text).ToArray();
            }
        }
        #endregion
        #endregion
        [SerializeField]
        InputField _inputField;
        [SerializeField]
        MultiInputDropdown _dropdown;
    }
}