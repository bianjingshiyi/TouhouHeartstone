using System;
using UnityEngine;

namespace Game.BattleTable
{
    public class StartDialog : MonoBehaviour
    {
        public void setActive(bool active)
        {
            gameObject.SetActive(active);
        }
        public void setHomebrewMode()
        {
            _homebrewOpponentCards.gameObject.SetActive(true);
            _homebrewMulliganDialog.setActive(true);
            _customMulliganDialog.setActive(false);
            _activeMulliganDialog = _homebrewMulliganDialog;
        }
        public void setCustomMode()
        {
            _homebrewOpponentCards.gameObject.SetActive(false);
            _homebrewMulliganDialog.setActive(false);
            _customMulliganDialog.setActive(true);
            _activeMulliganDialog = _customMulliganDialog;
        }
        public void clearChoices()
        {
            _homebrewOpponentCards.clear();
            _homebrewMulliganDialog.clearChoices();
            _customMulliganDialog.clearChoices();
        }
        public CardUIList getOpponentCustomCards()
        {
            return _homebrewOpponentCards;
        }
        public MulliganDialog getMulliganDialog()
        {
            return _activeMulliganDialog;
        }
        private void Awake()
        {
            _homebrewMulliganDialog.onItemPointerEnter += onMulliganItemPointerEnterCallback;
            _homebrewMulliganDialog.onItemPointerExit += onMulliganItemPointerExitCallback;
            _homebrewMulliganDialog.onItemClicked += onMulliganItemClickedCallback;
            _homebrewMulliganDialog.onAccept += onMulliganAcceptCallback;

            _customMulliganDialog.onItemPointerEnter += onMulliganItemPointerEnterCallback;
            _customMulliganDialog.onItemPointerExit += onMulliganItemPointerExitCallback;
            _customMulliganDialog.onItemClicked += onMulliganItemClickedCallback;
            _customMulliganDialog.onAccept += onMulliganAcceptCallback;
        }
        private void onMulliganItemPointerEnterCallback(int index)
        {
            onMulliganItemPointerEnter?.Invoke(index);
        }
        private void onMulliganItemPointerExitCallback(int index)
        {
            onMulliganItemPointerExit?.Invoke(index);
        }
        private void onMulliganItemClickedCallback(int index)
        {
            onMulliganItemClicked?.Invoke(index);
        }
        private void onMulliganAcceptCallback()
        {
            onMulliganAccept?.Invoke();
        }

        public event Action<int> onMulliganItemPointerEnter;
        public event Action<int> onMulliganItemPointerExit;
        public event Action<int> onMulliganItemClicked;
        public event Action onMulliganAccept;
        public bool isShowing => gameObject.activeSelf;
        [SerializeField]
        private CardUIList _homebrewOpponentCards;
        [SerializeField]
        private MulliganDialog _homebrewMulliganDialog;
        [SerializeField]
        private MulliganDialog _customMulliganDialog;
        private MulliganDialog _activeMulliganDialog;
    }
}
