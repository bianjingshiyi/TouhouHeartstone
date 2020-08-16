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
        Sprite _gemIcon;
        public Sprite gemIcon
        {
            get { return _gemIcon; }
        }
        [SerializeField]
        Sprite _maxGemIcon;
        public Sprite maxGemIcon
        {
            get { return _maxGemIcon; }
        }
        [SerializeField]
        Sprite _drawIcon;
        public Sprite drawIcon
        {
            get { return _drawIcon; }
        }
        [SerializeField]
        Sprite _burnIcon;
        public Sprite burnIcon
        {
            get { return _burnIcon; }
        }
        [SerializeField]
        Sprite _fatigueIcon;
        public Sprite fatigueIcon
        {
            get { return _fatigueIcon; }
        }
        [SerializeField]
        Sprite _discardIcon;
        public Sprite discardIcon
        {
            get { return _discardIcon; }
        }
        [SerializeField]
        Sprite _useIcon;
        public Sprite useIcon
        {
            get { return _useIcon; }
        }
        [SerializeField]
        Sprite _rebornIcon;
        public Sprite rebornIcon
        {
            get { return _rebornIcon; }
        }
        [SerializeField]
        Sprite _tokenIcon;
        public Sprite tokenIcon
        {
            get { return _tokenIcon; }
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
