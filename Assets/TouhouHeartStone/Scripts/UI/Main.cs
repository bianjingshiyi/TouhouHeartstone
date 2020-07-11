using UnityEngine;
using Game;
namespace UI
{
    partial class Main
    {
        [SerializeField]
        GameManager _game;
        public GameManager game
        {
            get { return _game; }
            set { _game = value; }
        }
        partial void onAwake()
        {
            display(Loading);
        }
    }
}
