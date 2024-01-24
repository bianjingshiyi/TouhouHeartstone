using UnityEngine;
using Game;
using UnityEngine.UI;

namespace UI
{
    partial class Main
    {
        #region 公有方法
        public void setBackgroundVisible(bool visible)
        {
            BackgroundImage.gameObject.SetActive(visible);
        }
        #endregion

        #region 私有方法
        partial void onAwake()
        {
            display(Loading);
        }
        #endregion

        #region 属性字段
        public DebugPage DebugPage => _debug;
        public Image BackgroundImage => _backgroundImage;
        public GameManager game
        {
            get { return _game; }
            set { _game = value; }
        }
        [SerializeField]
        GameManager _game;
        [SerializeField]
        DebugPage _debug;
        [SerializeField]
        Image _backgroundImage;
        #endregion
    }
}
