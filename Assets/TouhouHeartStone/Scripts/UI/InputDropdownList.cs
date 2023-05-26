using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InputDropdownList : MonoBehaviour
    {
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _inputField.onEndEdit.AddListener(onEndEditCallback);
        }
        #endregion
        #region 事件回调
        private void onEndEditCallback(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                _dropdown.value = text;
            }
        }
        #endregion
        #endregion
        [SerializeField]
        InputField _inputField;
        [SerializeField]
        InputDropdown _dropdown;
    }
}