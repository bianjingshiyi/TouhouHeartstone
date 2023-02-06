using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

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
            _inputField.onValueChanged.AddListener(onValueChanged);
        }
        private void Update()
        {
            _lastText = _inputField.text;
        }
        private void onValueChanged(string text)
        {
            text = text.Replace("\r", "").Replace("\t", "");
            // 如果以换行符结尾，并且除去换行符就是上一帧的文字，认定为按下回车键。
            if (text.EndsWith("\n") && text.Substring(0, text.Length - 1) == _lastText)
            {
                _inputField.SetTextWithoutNotify(_lastText);
                onSubmit.Invoke(text);
                if (!_keepTouchScreenKeyboard)
                    _inputField.touchScreenKeyboard.active = false;
            }
            // 否则，视为粘贴了一大段文字，把换行符变成空格。
            else
            {
                _inputField.SetTextWithoutNotify(text.Replace("\n", " "));
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