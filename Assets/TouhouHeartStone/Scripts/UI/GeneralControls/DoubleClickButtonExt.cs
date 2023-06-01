using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class DoubleClickButtonExt : MonoBehaviour
    {
        private void Awake()
        {
            button.onClick.AddListener(onClickCallback);
        }
        private void onClickCallback()
        {
            float time = Time.unscaledTime;
            var diffTime = time - _clickTime;
            if (diffTime < _doubleClickThresold)
            {
                onDoubleClick?.Invoke();
                _clickTime = 0;
            }
            else
            {
                _clickTime = time;
            }
        }
        public Button button
        {
            get
            {
                if (!_button)
                {
                    _button = GetComponent<Button>();
                }
                return _button;
            }
        }
        public Button.ButtonClickedEvent onDoubleClick;
        private Button _button;
        [SerializeField]
        private float _doubleClickThresold = 0.3f;
        private float _clickTime;
    }

}