using UnityEngine;
using BJSYGameCore;
using UnityEngine.EventSystems;

namespace UI
{
    partial class LogListItem : IPointerEnterHandler, IPointerExitHandler
    {


        public Sprite selfsprite
        {
            get { return _selfsprite; } 
        }
        [Header("Config")]
        [SerializeField]
        private Sprite _selfsprite;
        [SerializeField]
        private Sprite _opposprite;

        public Sprite opposprite
        {
            get { return _opposprite; }
        }

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
        Sprite _defaultIcon;
        public Sprite defaultIcon
        {
            get { return _defaultIcon; }
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
        [SerializeField]
        Sprite _armorIcon;
        public Sprite armorIcon
        {
            get { return _armorIcon; }
        }
        [SerializeField]
        Sprite _healIcon;
        public Sprite healIcon
        {
            get { return _healIcon; }
        }
        [SerializeField]
        Sprite _silenceIcon;
        public Sprite silenceIcon
        {
            get { return _silenceIcon; }
        }
        [SerializeField]
        Sprite _secretIcon;
        public Sprite secretIcon
        {
            get { return _secretIcon; }
        }
        [SerializeField]
        Sprite _deathIcon;
        public Sprite deathIcon
        {
            get { return _deathIcon; }
        }
        [Header("Status")]
        [SerializeField]
        bool _isHovered = false;
        public bool isHovered
        {
            get { return _isHovered; }
        }

        [SerializeField]
        RectTransform content;

        public ActionEvent<LogListItem> onPointerEnter { get; } = new ActionEvent<LogListItem>();
        public ActionEvent<LogListItem> onPointerExit { get; } = new ActionEvent<LogListItem>();

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter.invoke(this);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            onPointerExit.invoke(this);
        }
        public void SetLevel(int lvl)
        {
            var min = content.offsetMin;
            min.x = lvl * 20;
            content.offsetMin = min;
        }
    }
}
