using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CommentDialog : MonoBehaviour
    {
        public void reset()
        {
            _inputField.text = string.Empty;
            _positiveToggle.isOn = true;
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => onClose?.Invoke());
            _submitButton.onClick.AddListener(() => onSubmit?.Invoke(_inputField.text, _positiveToggle.isOn));
        }

        public event Action onClose;
        public event Action<string, bool> onSubmit;

        [SerializeField]
        private TMP_InputField _inputField;
        [SerializeField]
        private Toggle _positiveToggle;
        [SerializeField]
        private Toggle _negativeToggle;
        [SerializeField]
        private Button _submitButton;
        [SerializeField]
        private Button _closeButton;
    }
}
