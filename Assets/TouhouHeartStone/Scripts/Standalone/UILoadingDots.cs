using UnityEngine;
using UnityEngine.UI;

namespace IGensoukyo.Utilities
{
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class UILoadingDots : MonoBehaviour
    {
        [SerializeField]
        int durationTick = 10;

        int counter = 0;
        int dots = 0;


        private Text _textUI;
        public Text textUI
        {
            get {
                _textUI = _textUI ?? GetComponent<Text>();
                return _textUI;
            }
        }

        private void Update()
        {
            counter++;
            if (counter == durationTick)
            {
                dots++;
                if (dots > 6) dots = 0;
                textUI.text = "";
                for (int i = 0; i < dots; i++)
                {
                    textUI.text += ".";
                }
                counter = 0;
            }
        }

    }
}
