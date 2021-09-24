using BJSYGameCore;
using Game;
using UI;
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
    class MainMenuCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public void display()
        {
            gameObject.SetActive(true);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponentInChildren<MainMenu>();
            }
            _ui.onEditorButtonClick += onEditorButtonClick;
        }
        #endregion
        #region 事件回调
        private void onEditorButtonClick()
        {
            _ui.gameObject.SetActive(false);
            scene.editor.display();
        }
        #endregion
        #endregion
        [SerializeField]
        MainMenu _ui;
    }
}
