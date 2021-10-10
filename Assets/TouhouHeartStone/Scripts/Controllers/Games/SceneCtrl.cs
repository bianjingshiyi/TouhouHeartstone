using BJSYGameCore;
using Game;
using UnityEngine;

namespace Controllers
{
    class SceneCtrl : MonoBehaviour
    {
        protected virtual void Start()
        {
            if (_game == null)
            {
                _game = TransformHelper.findComponentInAllScenes<MainManager>();
            }
        }
        protected void Reset()
        {
            _game = TransformHelper.findComponentInAllScenes<MainManager>();
        }
        public MainManager game
        {
            get { return _game; }
        }
        [SerializeField]
        MainManager _game;
    }
}
