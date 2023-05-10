using Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    internal class GraphEditorPanelCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        public void setOperationMode(GraphOperationMode mode)
        {
            _actionGraphCtrl.SetOperationMode(mode);
            _singleSelectObj.SetActive(mode == GraphOperationMode.Select);
            _multiSelectObj.SetActive(mode == GraphOperationMode.MultiSelect);
        }
        void Awake()
        {
            _singleSelectButton.onClick.AddListener(OnClickSingleSelectButton);
            _multiSelectButton.onClick.AddListener(OnClickMultiSelectButton);
            _copyButton.onClick.AddListener(OnClickCopyButton);
            _pasteButton.onClick.AddListener(OnClickPasteButton);

            updatePasteButton();
        }
        private void updatePasteButton()
        {
            _pasteButton.interactable = !string.IsNullOrEmpty(scene.editor.nodeClipboard);
        }

        private void OnClickSingleSelectButton()
        {
            setOperationMode(GraphOperationMode.Select);
        }
        private void OnClickMultiSelectButton()
        {
            setOperationMode(GraphOperationMode.MultiSelect);
        }
        private void OnClickCopyButton()
        {
            var clipboard = _actionGraphCtrl.copySelection();
            scene.editor.nodeClipboard = clipboard;
            updatePasteButton();
        }
        private async void OnClickPasteButton()
        {
            var clipboard = scene.editor.nodeClipboard;
            if (string.IsNullOrEmpty(clipboard))
                return;
            await _actionGraphCtrl.pasteClipboard(clipboard);
            updatePasteButton();
        }
        [SerializeField]
        ActionNodeGraphCtrl _actionGraphCtrl;

        [Header("按钮")]
        [SerializeField]
        Button _singleSelectButton;
        [SerializeField]
        Button _multiSelectButton;
        [SerializeField]
        Button _copyButton;
        [SerializeField]
        Button _pasteButton;

        [Header("高亮")]
        [SerializeField]
        GameObject _singleSelectObj;
        [SerializeField]
        GameObject _multiSelectObj;
    }
}