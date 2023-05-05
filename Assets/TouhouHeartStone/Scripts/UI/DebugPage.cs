using BJSYGameCore;
using Game;
using UnityEngine.UI;
using UnityEngine;
using BJSYGameCore.UI;

namespace UI
{
    public class DebugPage : UIObject
    {
        DebugManager debug => this.findInstance<DebugManager>();
        NetworkingManager net => this.findInstance<NetworkingManager>();

        [SerializeField]
        InputField serverAddrField;

        [SerializeField]
        Toggle skipCardNumCheckField;

        [SerializeField]
        Button backBtn;


        new void Awake()
        {
            backBtn.onClick.AddListener(onBack);
        }

        protected override void onDisplay()
        {
            base.onDisplay();

            serverAddrField.text = net.logic.ServerClient.baseUri;
            skipCardNumCheckField.isOn = debug.SkipCardNumberCheck;
        }

        void onBack()
        {
            if (serverAddrField.text != net.logic.ServerClient.baseUri)
            {
                net.SetServerAddr(serverAddrField.text);
            }
            debug.SkipCardNumberCheck = skipCardNumCheckField.isOn;

            var parent = GetComponentInParent<Main>();
            parent.display(parent.MainMenu);
        }
    }
}
