using UnityEngine;
using UnityEngine.UI;

namespace Game.BattleTable
{
    public class TurnTipImage : MonoBehaviour
    {
        public void setActive(bool active)
        {
            gameObject.SetActive(active);
        }
        public void setFriendly(bool friendly)
        {
            _image.sprite = friendly ? _yourTurn : _oppoTurn;
        }
        public void play()
        {
            setActive(true);
            _animator.Play("Display");
        }
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private Image _image;
        [SerializeField]
        private Sprite _yourTurn;
        [SerializeField]
        private Sprite _oppoTurn;
    }
}
