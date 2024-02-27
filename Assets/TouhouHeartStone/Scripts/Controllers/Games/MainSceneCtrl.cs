using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using TouhouCardEngine;
using UI;
using UnityEngine;

namespace Controllers
{
    partial class MainSceneCtrl : SceneCtrl
    {
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
    }
}
