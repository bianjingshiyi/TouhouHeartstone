using BJSYGameCore;
using Game;
using UnityEngine.UI;
using UnityEngine;
using BJSYGameCore.UI;
using Controllers;

namespace UI
{
    public class DebugPage : UIObject
    {
        new void Awake()
        {
            backBtn.onClick.AddListener(onBack);
            _clearTutorialDataButton.onClick.AddListener(onClickClearTutorialDataButtonCallback);
        }

        protected override void onDisplay()
        {
            base.onDisplay();

            serverAddrField.text = net.logic.ServerClient.baseUri;
            skipCardNumCheckField.isOn = debug.SkipCardNumberCheck;
        }

        async void onBack()
        {
            if (serverAddrField.text != net.logic.ServerClient.baseUri)
            {
                await net.SetServerAddr(serverAddrField.text);
            }
            debug.SkipCardNumberCheck = skipCardNumCheckField.isOn;

            hide();
            scene.menu.display();
        }
        private void onClickClearTutorialDataButtonCallback()
        {
            main.tutorial.clearTutorialFinishedData();
            scene.guide.hide();
        }

        MainManager main => this.findInstance<MainManager>();
        MainSceneCtrl scene => this.findInstance<MainSceneCtrl>();
        DebugManager debug => this.findInstance<DebugManager>();
        NetworkingManager net => this.findInstance<NetworkingManager>();

        [SerializeField]
        InputField serverAddrField;
        [SerializeField]
        Toggle skipCardNumCheckField;
        [SerializeField]
        Button _clearTutorialDataButton;
        [SerializeField]
        Button backBtn;
    }
}
