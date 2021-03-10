using BJSYGameCore.UI;

namespace UI
{
    partial class ReplaySelectPanel : UIObject
    {
        protected override void Awake()
        {
            base.Awake();
            autoInit();
            _playButton.onClick.AddListener(onPlayerButtonClick);
        }
        void onPlayerButtonClick()
        {

        }
    }
}
