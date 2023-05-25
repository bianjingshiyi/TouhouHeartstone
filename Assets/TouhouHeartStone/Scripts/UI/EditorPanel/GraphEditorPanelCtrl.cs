using System.Linq;
using Controllers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    internal class GraphEditorPanelCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        public void display(long cardPoolId)
        {
            _cardPoolId = cardPoolId;
            setOperationMode(scene.editor.graphOperationMode);
            updateNodeDefines();
            updateNodesList();
        }
        public void setOperationMode(GraphOperationMode mode)
        {
            scene.editor.graphOperationMode = mode;
            _actionGraphCtrl.SetOperationMode(mode);
            _singleSelectObj.SetActive(mode == GraphOperationMode.Select);
            _multiSelectObj.SetActive(mode == GraphOperationMode.MultiSelect);
        }
        public void updateNodeDefines()
        {
            var actionDefines = game.getActionDefinesArray(_cardPoolId);
            var eventDefines = EventTypeInfoLoader.getOrLoadEventDefines();
            var nodeDefines = actionDefines.Select(a => new NodeDefine(a));
            nodeDefines = nodeDefines.Union(eventDefines.Select(e => new NodeDefine(e)));

            _nodeDefines = nodeDefines.ToArray();
        }
        public void updateNodesList()
        {
            var viewDatas = _nodeDefines.Select(d =>
            {
                if (d.actionDefine != null)
                {
                    string name = _actionGraphCtrl.tryTranslateWithDict(d.actionDefine.defineName);
                    Sprite spr = null;
                    return new NodeViewData(name, spr);
                }
                if (d.eventDefine != null)
                {
                    string name = game.lang._(d.eventDefine.editorName);
                    Sprite spr = null;
                    return new NodeViewData(name, spr);
                }
                return default;
            }).ToArray();
            _nodesPanel.updateList(viewDatas);
        }
        #region 私有方法
        #region 生命周期
        void Awake()
        {
            _singleSelectButton.onClick.AddListener(OnClickSingleSelectButton);
            _multiSelectButton.onClick.AddListener(OnClickMultiSelectButton);
            _copyButton.onClick.AddListener(OnClickCopyButton);
            _pasteButton.onClick.AddListener(OnClickPasteButton);

            _nodesPanel.onItemBeginDrag += OnNodeItemBeginDrag;
            _nodesPanel.onItemDrag += OnNodeItemBeginDrag;
            _nodesPanel.onItemEndDrag += OnNodeItemBeginDrag;

            updatePasteButton();
        }
        #endregion
        #region 事件回调
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
        private bool OnNodeItemBeginDrag(int index, PointerEventData data)
        {
            return false;
        }
        private bool OnNodeItemDrag(int index, PointerEventData data)
        {
            return false;
        }
        private bool OnNodeItemEndDrag(int index, PointerEventData data)
        {
            return false;
        }
        #endregion
        private void updatePasteButton()
        {
            _pasteButton.interactable = !string.IsNullOrEmpty(scene.editor.nodeClipboard);
        }
        #endregion

        private long _cardPoolId;
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

        [Header("节点")]
        [SerializeField]
        NodesPanel _nodesPanel;
        private NodeDefine[] _nodeDefines;
    }
}