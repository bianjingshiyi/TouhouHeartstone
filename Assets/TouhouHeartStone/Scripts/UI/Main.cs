using UnityEngine;
using Game;
using UnityEngine.UI;

namespace UI
{
    partial class Main
    {
        public void setBackgroundVisible(bool visible)
        {
            BackgroundImage.gameObject.SetActive(visible);
        }
        partial void onAwake()
        {
            display(Loading);
        }

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
    }
}
