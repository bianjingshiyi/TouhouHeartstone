using BJSYGameCore;
using Game;
using UnityEngine;

namespace Controllers
{
    class GameObjectCtrl<T> : MonoBehaviour where T : SceneCtrl
    {
        protected void Start()
        {
            if (_scene == null)
            {
                _scene = TransformHelper.findComponentInScene<T>(gameObject.scene);
            }
        }
        protected void Reset()
        {
            _scene = TransformHelper.findComponentInScene<T>(gameObject.scene);
        }
        public MainManager game
        {
            get { return scene.game; }
        }
        public T scene
        {
            get
            {
                if (_scene == null)
                {
                    _scene = TransformHelper.findComponentInScene<T>(gameObject.scene);
                }
                return _scene;
            }
        }
        [SerializeField]
        T _scene;
    }
}
