using IGensoukyo;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class NodeDocument : MonoBehaviour
    {
        #region 公有方法
        public void Show(string str)
        {
            _html.Html = str;

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        #endregion

        #region 私有方法

        #region 生命周期
        private void Awake()
        {
            _closeBtn.onClick.AddListener(Hide);
        }
        #endregion

        #endregion

        #region 属性字段
        [SerializeField]
        HTMLRenderer _html;
        [SerializeField]
        Button _closeBtn;
        #endregion
    }
}

