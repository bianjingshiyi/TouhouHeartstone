using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// 用于给InputField加上Shift+回车结束编辑的功能。
    /// </summary>
    [RequireComponent(typeof(InputField))]
    [DefaultExecutionOrder(-2000)] //把执行顺序前调，免得让这次回车加一个换行符进去
    public class ShiftConfirmInputField : MonoBehaviour
    {
        private void Update()
        {
            if (!_inputField.isFocused)
                return;
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetButtonDown("Submit"))
            {
                _inputField.DeactivateInputField();
            }
        }
        [SerializeField]
        private InputField _inputField;
    }
}