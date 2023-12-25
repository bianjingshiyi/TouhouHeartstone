using System;
using Controllers;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BattleTable.UI
{
    public class TableUI : MonoBehaviour
    {
        private void Awake()
        {
            _quitButton.onClick.AddListener(onQuitClickedCallback);
            _surrenderButton.onClick.AddListener(onSurrenderClickedCallback);
        }
        private void onQuitClickedCallback()
        {
            onQuit?.Invoke();
        }
        private void onSurrenderClickedCallback()
        {
            onSurrender?.Invoke();
        }
        public void showSurrenderButton(bool show)
        {
            _quitButton.gameObject.SetActive(!show);
            _surrenderButton.gameObject.SetActive(show);
        }
        #region 事件
        public event Action onQuit;
        public event Action onSurrender;
        #endregion
        #region 属性字段
        public GameObject CardDisplayerObj => _cardDisplayerObj;
        public CardDisplayer CardDisplayer => _cardDisplayer;
        public GameObject OpponentWaitingObj => _opponentWaitingObj;
        public PilePanel PilePanel => _pilePanel;
        public MulliganDialog InitReplaceDialog => _mulliganDialog;
        public TurnTipImage TurnTipImage => _turnTipImage;
        public GameResultDialog GameResultDialog => _gameResultDialog;
        public TipBox TipBox => _tipBox;
        public Text ErrorTipText => _errorText;
        public CardPopUp CardPopUp => _cardPopUp;
        public GameLogCtrl LogCtrl => _logCtrl;
        [SerializeField]
        private GameObject _cardDisplayerObj;
        [SerializeField]
        private CardDisplayer _cardDisplayer;
        [SerializeField]
        private Button _quitButton;
        [SerializeField]
        private Button _surrenderButton;
        [SerializeField]
        private GameObject _opponentWaitingObj;
        [SerializeField]
        private PilePanel _pilePanel;
        [SerializeField]
        private MulliganDialog _mulliganDialog;
        [SerializeField]
        private TurnTipImage _turnTipImage;
        [SerializeField]
        private GameResultDialog _gameResultDialog;
        [SerializeField]
        private TipBox _tipBox;
        [SerializeField]
        private Text _errorText;
        [SerializeField]
        private GameLogCtrl _logCtrl;
        [SerializeField]
        private CardPopUp _cardPopUp;
        #endregion
    }
}
