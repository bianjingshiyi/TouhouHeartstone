using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(InputField))]
    public class InputFieldSubmit : MonoBehaviour
    {
        private void Awake()
        { 
            _inputField = GetComponent<InputField>();
        }
        private void OnEnable()
        {
            _inputField.onValueChanged.AddListener(onValueChanged);
            _inputField.lineType = InputField.LineType.MultiLineNewline;
        }
        private void OnDisable()
        {
            _inputField.onValueChanged.RemoveListener(onValueChanged);
        }
        private void Update()
        {
            _lastText = _inputField.text;
        }
        private void onValueChanged(string text)
        {
            text = text.Replace("\r", "").Replace("\t", "");
            string trimedText = text.TrimEnd('\n');
            // 如果以换行符结尾，并且除去换行符就是上一帧的文字，认定为按下回车键。
            if (text.EndsWith("\n") && trimedText == _lastText)
            {
                _inputField.SetTextWithoutNotify(trimedText);
                SendSubmit(trimedText);
            }
            // 否则，视为粘贴了一大段文字，把换行符变成空格。
            else
            {
                _inputField.SetTextWithoutNotify(text.Replace("\n", " "));
            }
        }
        private void SendSubmit(string text)
        {
            // 空字符则不响应。
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            onSubmit.Invoke(text);
            if (_inputField.touchScreenKeyboard != null && !_keepTouchScreenKeyboard)
            {
                _inputField.touchScreenKeyboard.active = false;
            }
        }
        public InputField.SubmitEvent onSubmit;
        private InputField _inputField;
        private string _lastText;
        /// <summary>
        /// 提交之后是否保留触摸屏键盘。
        /// </summary>
        [SerializeField]
        private bool _keepTouchScreenKeyboard = false;
    }

}