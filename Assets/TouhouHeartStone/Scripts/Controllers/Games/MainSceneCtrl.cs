using BJSYGameCore;
using UnityEngine;

namespace Controllers
{
    class MainSceneCtrl : SceneCtrl
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
            if (_chatCtrl == null)
            {
                _chatCtrl = TransformHelper.findComponentInScene<ChatCtrl>(gameObject.scene);
            }
            if (_replayCtrl == null)
            {
                _replayCtrl = TransformHelper.findComponentInScene<ReplaySelectPanelCtrl>(gameObject.scene);
            }
            _menu.gameObject.SetActive(true);
            _room.gameObject.SetActive(false);
            _build.gameObject.SetActive(false);
            _pveRoom.gameObject.SetActive(false);
            _editor.gameObject.SetActive(false);
            _cardEditor.gameObject.SetActive(false);
            _buffEditor.gameObject.SetActive(false);
            _actionEditor.gameObject.SetActive(false);
            _chatCtrl.gameObject.SetActive(false);
            _replayCtrl.gameObject.SetActive(false);
        }
        public MainMenuCtrl menu => _menu;
        public RoomCtrl room => _room;
        public BuildCtrl build => _build;
        public PVERoomCtrl pveRoom => _pveRoom;
        public EditorCtrl editor => _editor;
        public CardEditorCtrl cardEditor => _cardEditor;
        public BuffEditorCtrl buffEditor => _buffEditor;
        public ActionEditorCtrl actionEditor => _actionEditor;
        public ChatCtrl chatCtrl => _chatCtrl;
        public ReplaySelectPanelCtrl replay => _replayCtrl;
        [SerializeField]
        MainMenuCtrl _menu;
        [SerializeField]
        RoomCtrl _room;
        [SerializeField]
        BuildCtrl _build;
        [SerializeField]
        PVERoomCtrl _pveRoom; // 待手动关联UI
        [SerializeField]
        EditorCtrl _editor;
        [SerializeField]
        CardEditorCtrl _cardEditor;
        [SerializeField]
        ActionEditorCtrl _actionEditor;
        [SerializeField]
        BuffEditorCtrl _buffEditor;
        [SerializeField]
        ChatCtrl _chatCtrl;
        [SerializeField]
        ReplaySelectPanelCtrl _replayCtrl;
    }
}
