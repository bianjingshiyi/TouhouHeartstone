using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 用于控制卡池编辑器中，编辑器面板的伸缩开关。
    /// </summary>
    public class PoolEditorRegionCtrl : MonoBehaviour
    {
        private void Awake()
        {
            _closeButton.onClick.AddListener(onCloseButtonClicked);
        }
        public void onCloseButtonClicked()
        {
            closed = !closed;
            _animator.SetBool("Closed", closed);
        }
        [SerializeField]
        private bool closed = false;
        [SerializeField]
        private Button _closeButton;
        [SerializeField]
        private Animator _animator;
    }

}