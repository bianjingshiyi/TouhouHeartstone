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
            if (_editor == null)
            {
                _editor = TransformHelper.findComponentInScene<EditorCtrl>(gameObject.scene);
            }
            if (_cardEditor == null)
            {
                _cardEditor = TransformHelper.findComponentInScene<CardEditorCtrl>(gameObject.scene);
            }
            if (_effectEditor == null)
            {
                _effectEditor = TransformHelper.findComponentInScene<EffectEditorCtrl>(gameObject.scene);
            }
            if (_actionNodeGraphCtrl == null)
            {
                _actionNodeGraphCtrl = TransformHelper.findComponentInScene<ActionNodeGraphCtrl>(gameObject.scene);
            }
            _menu.gameObject.SetActive(true);
            _room.gameObject.SetActive(false);
            _editor.gameObject.SetActive(false);
            _cardEditor.gameObject.SetActive(false);
            _effectEditor.gameObject.SetActive(false);
            _actionNodeGraphCtrl.gameObject.SetActive(false);
        }
        public MainMenuCtrl menu
        {
            get { return _menu; }
        }
        public RoomCtrl room
        {
            get { return _room; }
        }
        public EditorCtrl editor
        {
            get { return _editor; }
        }
        public CardEditorCtrl cardEditor
        {
            get { return _cardEditor; }
        }
        public EffectEditorCtrl effectEditor
        {
            get { return _effectEditor; }
        }
        public ActionNodeGraphCtrl actionNodeGraphCtrl
        {
            get { return _actionNodeGraphCtrl; }
        }
        [SerializeField]
        MainMenuCtrl _menu;
        [SerializeField]
        RoomCtrl _room;
        [SerializeField]
        EditorCtrl _editor;
        [SerializeField]
        CardEditorCtrl _cardEditor;
        [SerializeField]
        EffectEditorCtrl _effectEditor;
        [SerializeField]
        ActionNodeGraphCtrl _actionNodeGraphCtrl;
    }
}
