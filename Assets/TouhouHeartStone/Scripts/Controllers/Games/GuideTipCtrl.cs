using UI;
using UnityEngine;

namespace Controllers
{
    class GuideTipCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        public void show(GuideTipViewData viewData)
        {
            if (!viewData.isValid())
            {
                hide();
                return;
            }
            gameObject.SetActive(true);
            time = 0;
            _ui.show(viewData);
        }
        public void hide()
        {
            gameObject.SetActive(false);
            _ui.hide();
        }
        private void Update()
        {
            time += Time.deltaTime;
            time %= interval;
            _ui.update(time / interval);
        }
        [SerializeField]
        private GuideTip _ui;
        [SerializeField]
        private float interval = 1f;

        private float time;
    }
}
