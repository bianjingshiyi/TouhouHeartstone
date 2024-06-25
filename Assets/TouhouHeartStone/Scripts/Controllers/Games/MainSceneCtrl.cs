using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using TouhouCardEngine.Shared;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    partial class MainSceneCtrl : SceneCtrl
    {
        public async Task displayCardPoolSingleSelectPanel(CardPoolSelectSource source)
        {
            cardPoolSelectSource = source;
            _singleSelectCardPoolDialogTitle.text = source.title;
            _singleSelectCardPoolDialogObj.SetActive(true);
            await updateSingleSelectCardPoolDialog();
        }
        public async Task displayCardPoolMultiSelectPanel(CardPoolSelectSource source)
        {
            cardPoolSelectSource = source;
            _multiSelectCardPoolDialogTitle.text = source.title;
            _multiSelectCardPoolDialogObj.SetActive(true);
            await updateMultiSelectCardPoolDialog();
        }
        public void hideCardPoolSingleSelectPanel()
        {
            cardPoolSelectSource = null;
            _singleSelectCardPoolDialogObj.SetActive(false);
        }
        public void hideCardPoolMultiSelectPanel()
        {
            cardPoolSelectSource = null;
            _multiSelectCardPoolDialogObj.SetActive(false);
        }
        private void Awake()
        {
            _multiSelectCardPoolDialog.onClickItemButton += onClickMultiSelectCardPoolItemCallback;
            _multiSelectCardPoolDialog.onItemToggleValueChanged += (index, value) => onClickMultiSelectCardPoolItemCallback(index);
            _multiSelectCardPoolConfirmButton.onClick.AddListener(onClickMultiSelectCardPoolConfirmButtonCallback);
            _multiSelectCardPoolCancelButton.onClick.AddListener(() => hideCardPoolSingleSelectPanel());

            _singleSelectCardPoolDialog.onClickItemButton += onClickSingleSelectCardPoolItemCallback;
            _singleSelectCardPoolCancelButton.onClick.AddListener(() => hideCardPoolMultiSelectPanel());
        }
        protected override void Start()
        {
            if (_menu == null)
            {
                _menu = TransformHelper.findComponentInScene<MainMenuCtrl>(gameObject.scene);
            }
            if (_room == null)
            {
                _room = TransformHelper.findComponentInScene<RoomCtrl>(gameObject.scene);
            }
            if (_build == null)
            {
                _build = TransformHelper.findComponentInScene<BuildCtrl>(gameObject.scene);
            }
            if (_pveRoom == null)
            {
                _pveRoom = TransformHelper.findComponentInScene<PVERoomCtrl>(gameObject.scene);
            }
            if (_editor == null)
            {
                _editor = TransformHelper.findComponentInScene<EditorCtrl>(gameObject.scene);
            }
            if (_cardEditor == null)
            {
                _cardEditor = TransformHelper.findComponentInScene<CardEditorCtrl>(gameObject.scene);
            }
            if (_buffEditor == null)
            {
                _buffEditor = TransformHelper.findComponentInScene<BuffEditorCtrl>(gameObject.scene);
            }
            if (_actionEditor == null)
            {
                _actionEditor = TransformHelper.findComponentInScene<ActionEditorCtrl>(gameObject.scene);
            }
            if (_ruleEditor == null)
            {
                _ruleEditor = TransformHelper.findComponentInScene<RuleEditorCtrl>(gameObject.scene);
            }
            if (_chatCtrl == null)
            {
                _chatCtrl = TransformHelper.findComponentInScene<ChatCtrl>(gameObject.scene);
            }
            if (_lanCtrl == null)
            {
                _lanCtrl = TransformHelper.findComponentInScene<LANPanelCtrl>(gameObject.scene);
            }
            if (_replayCtrl == null)
            {
                _replayCtrl = TransformHelper.findComponentInScene<ReplaySelectPanelCtrl>(gameObject.scene);
            }
            if (_guideCtrl == null)
            {
                _guideCtrl = TransformHelper.findComponentInScene<GuideTipCtrl>(gameObject.scene);
            }
            if (_workshop == null)
            {
                _workshop = TransformHelper.findComponentInScene<WorkshopCtrl>(gameObject.scene);
            }
            if (_waitingBlocker == null)
            {
                _waitingBlocker = TransformHelper.findComponentInScene<WaitingBlocker>(gameObject.scene);
            }
            if (_cardPopup == null)
            {
                _cardPopup = TransformHelper.findComponentInScene<CardPopUpUI>(gameObject.scene);
            }
            _menu.gameObject.SetActive(true);
            _room.gameObject.SetActive(false);
            _build.gameObject.SetActive(false);
            _pveRoom.gameObject.SetActive(false);
            _editor.gameObject.SetActive(false);
            _cardEditor.gameObject.SetActive(false);
            _buffEditor.gameObject.SetActive(false);
            _actionEditor.gameObject.SetActive(false);
            _ruleEditor.gameObject.SetActive(false);
            _chatCtrl.gameObject.SetActive(false);
            _lanCtrl.gameObject.SetActive(false);
            _replayCtrl.gameObject.SetActive(false);
            _guideCtrl.gameObject.SetActive(false);
            _workshop.gameObject.SetActive(false);
            _waitingBlocker.gameObject.SetActive(false);
            _cardPopup.gameObject.SetActive(false);
        }
        private async void Update()
        {
            await updateCardPopup();
        }

        #region 卡池单选面板
        private void onClickSingleSelectCardPoolItemCallback(int index)
        {
            var selecting = getAllSelectingCardPools();
            var depRef = selecting[index];
            cardPoolSelectSource.select(depRef);
            if (cardPoolSelectSource.Confirm())
            {
                hideCardPoolSingleSelectPanel();
            }
        }
        private Task updateSingleSelectCardPoolDialog()
        {
            return updateSingleSelectCardPoolDialog(getAllSelectingCardPools(), getSelectedCardPools());
        }
        private async Task updateSingleSelectCardPoolDialog(IEnumerable<ICardPoolReference> cardPoolRefs, IEnumerable<ICardPoolReference> selected = null)
        {
            var viewDatas = new DeckListItemViewData[cardPoolRefs.Count()];
            for (int i = 0; i < viewDatas.Length; i++)
            {
                var dep = cardPoolRefs.ElementAt(i);
                var info = game.card.getCardPoolInfo(dep);
                var cardPoolName = game.card.getCardPoolName(info);
                viewDatas[i] = new DeckListItemViewData()
                {
                    name = cardPoolName,
                    image = await game.card.loadCardPoolImage(info),
                    selected = selected?.Contains(dep) ?? false,
                    mode = DeckListItem.Mode.Default
                };
            }
            _singleSelectCardPoolDialog.update(viewDatas);
        }
        #endregion

        #region 卡池多选面板
        private async void onClickMultiSelectCardPoolItemCallback(int index)
        {
            var selecting = getAllSelectingCardPools();
            var selected = getSelectedCardPools();
            var depRef = selecting[index];
            if (selected.Contains(depRef))
            {
                cardPoolSelectSource.deselect(depRef);
            }
            else
            {
                cardPoolSelectSource.select(depRef);
            }
            await updateMultiSelectCardPoolDialog();
        }
        private void onClickMultiSelectCardPoolConfirmButtonCallback()
        {
            if (cardPoolSelectSource.Confirm())
            {
                hideCardPoolMultiSelectPanel();
            }
        }
        private Task updateMultiSelectCardPoolDialog()
        {
            return updateMultiSelectCardPoolDialog(getAllSelectingCardPools(), getSelectedCardPools());
        }
        private async Task updateMultiSelectCardPoolDialog(IEnumerable<ICardPoolReference> cardPoolRefs, IEnumerable<ICardPoolReference> selected = null)
        {
            var viewDatas = new DeckListItemViewData[cardPoolRefs.Count()];
            for (int i = 0; i < viewDatas.Length; i++)
            {
                var dep = cardPoolRefs.ElementAt(i);
                var info = game.card.getCardPoolInfo(dep);
                var cardPoolName = game.card.getCardPoolName(info);
                viewDatas[i] = new DeckListItemViewData()
                {
                    name = cardPoolName,
                    image = await game.card.loadCardPoolImage(info),
                    selected = selected?.Contains(dep) ?? false,
                    mode = DeckListItem.Mode.CanSelect
                };
            }
            _multiSelectCardPoolDialog.update(viewDatas);
        }
        #endregion
        private ICardPoolReference[] getAllSelectingCardPools()
        {
            return cardPoolSelectSource.getAllCardPools();
        }
        private ICardPoolReference[] getSelectedCardPools()
        {
            return cardPoolSelectSource.getSelectedCardPools();
        }
        public MainMenuCtrl menu => _menu;
        public RoomCtrl room => _room;
        public BuildCtrl build => _build;
        public PVERoomCtrl pveRoom => _pveRoom;
        public EditorCtrl editor => _editor;
        public CardEditorCtrl cardEditor => _cardEditor;
        public BuffEditorCtrl buffEditor => _buffEditor;
        public ActionEditorCtrl actionEditor => _actionEditor;
        public RuleEditorCtrl ruleEditor => _ruleEditor;
        public ChatCtrl chatCtrl => _chatCtrl;
        public LANPanelCtrl lan => _lanCtrl;
        public ReplaySelectPanelCtrl replay => _replayCtrl;
        public GuideTipCtrl guide => _guideCtrl;
        public WorkshopCtrl workshop => _workshop;
        public WaitingBlocker waitingBlocker => _waitingBlocker;
        [SerializeField]
        MainMenuCtrl _menu;
        [SerializeField]
        RoomCtrl _room;
        [SerializeField]
        BuildCtrl _build;
        [SerializeField]
        PVERoomCtrl _pveRoom;
        [SerializeField]
        EditorCtrl _editor;
        [SerializeField]
        CardEditorCtrl _cardEditor;
        [SerializeField]
        ActionEditorCtrl _actionEditor;
        [SerializeField]
        RuleEditorCtrl _ruleEditor;
        [SerializeField]
        BuffEditorCtrl _buffEditor;
        [SerializeField]
        ChatCtrl _chatCtrl;
        [SerializeField]
        LANPanelCtrl _lanCtrl;
        [SerializeField]
        ReplaySelectPanelCtrl _replayCtrl;
        [SerializeField]
        GuideTipCtrl _guideCtrl;
        [SerializeField]
        WorkshopCtrl _workshop;
        [SerializeField]
        WaitingBlocker _waitingBlocker;


        [Header("卡池单选")]
        [SerializeField]
        GameObject _singleSelectCardPoolDialogObj;
        [SerializeField]
        Text _singleSelectCardPoolDialogTitle;
        [SerializeField]
        DeckViewPanel _singleSelectCardPoolDialog;
        [SerializeField]
        Button _singleSelectCardPoolCancelButton;

        [Header("卡池多选")]
        [SerializeField]
        GameObject _multiSelectCardPoolDialogObj;
        [SerializeField]
        Text _multiSelectCardPoolDialogTitle;
        [SerializeField]
        DeckViewPanel _multiSelectCardPoolDialog;
        [SerializeField]
        Button _multiSelectCardPoolConfirmButton;
        [SerializeField]
        Button _multiSelectCardPoolCancelButton;

        private CardPoolSelectSource cardPoolSelectSource;
    }
}
