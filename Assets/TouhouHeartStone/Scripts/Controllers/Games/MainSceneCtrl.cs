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
            if (_effectEditor == null)
            {
                _effectEditor = TransformHelper.findComponentInScene<EffectEditorCtrl>(gameObject.scene);
            }
            if (_actionNodeGraphCtrl == null)
            {
                _actionNodeGraphCtrl = TransformHelper.findComponentInScene<ActionNodeGraphCtrl>(gameObject.scene);
            }
            if (_chatCtrl == null)
            {
                _chatCtrl = TransformHelper.findComponentInScene<ChatCtrl>(gameObject.scene);
            }
            _menu.gameObject.SetActive(true);
            _room.gameObject.SetActive(false);
            _build.gameObject.SetActive(false);
            _pveRoom.gameObject.SetActive(false);
            _editor.gameObject.SetActive(false);
            _cardEditor.gameObject.SetActive(false);
            _buffEditor.gameObject.SetActive(false);
            _effectEditor.gameObject.SetActive(false);
            _actionNodeGraphCtrl.gameObject.SetActive(false);
            _chatCtrl.gameObject.SetActive(false);
        }
        public MainMenuCtrl menu => _menu;
        public RoomCtrl room => _room;
        public BuildCtrl build => _build;
        public PVERoomCtrl pveRoom => _pveRoom;
        public EditorCtrl editor => _editor;
        public CardEditorCtrl cardEditor => _cardEditor;
        public BuffEditorCtrl buffEditor => _buffEditor;
        public ChatCtrl chatCtrl => _chatCtrl;
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
        BuffEditorCtrl _buffEditor;
        [SerializeField]
        EffectEditorCtrl _effectEditor;
        [SerializeField]
        ActionNodeGraphCtrl _actionNodeGraphCtrl;
        [SerializeField]
        ChatCtrl _chatCtrl;
    }
}
