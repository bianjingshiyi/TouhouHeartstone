using UnityEngine;
namespace UI
{
    partial class LogListItem
    {
        [SerializeField]
        Color _selfColor = new Color(.2f, .2f, .8f);
        public Color selfColor
        {
            get { return _selfColor; }
        }
        [SerializeField]
        Color _oppoColor = new Color(.8f, .2f, .2f);
        public Color oppoColor
        {
            get { return _oppoColor; }
        }
        [SerializeField]
        Sprite _useIcon;
        public Sprite useIcon
        {
            get { return _useIcon; }
        }
        [SerializeField]
        Sprite _attackIcon;
        public Sprite attackIcon
        {
            get { return _attackIcon; }
        }
        [SerializeField]
        Sprite _damageIcon;
        public Sprite damageIcon
        {
            get { return _damageIcon; }
        }
    }
}
