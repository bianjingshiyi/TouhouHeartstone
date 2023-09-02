using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class NodeAnimRefKeyPopup : MonoBehaviour
    {
        public void setKey(string key)
        {
            _inputField.text = key;
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _inputField.onEndEdit.AddListener(onEndEditCallback);
            _node.onAnimationRefKeyUpdated += onAnimationRefKeyUpdatedCallback;
        }
        #endregion
        #region 事件回调
        private void onEndEditCallback(string text)
        {
            _node.notifyAnimationRefKeyChange(text);
        }
        private void onAnimationRefKeyUpdatedCallback(string key)
        {
            _inputField.SetTextWithoutNotify(key);
        }
        #endregion
        #endregion
        [SerializeField]
        InputField _inputField;
        [SerializeField]
        ActionNodeItem _node;
    }
}
