using Game;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HintMessage : MonoBehaviour
    {
        public void show(string text)
        {
            _text.text = text;
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            if (_sePlayer)
                _sePlayer.Play();
        }
        public void hide()
        {
            _text.text = string.Empty;
            gameObject.SetActive(false);
        }
        [SerializeField]
        private Text _text;
        [SerializeField]
        private SEPlayer _sePlayer;
    }
}
