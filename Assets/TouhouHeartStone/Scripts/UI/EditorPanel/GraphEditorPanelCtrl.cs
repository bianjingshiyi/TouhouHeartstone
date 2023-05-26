using System.Linq;
using Controllers;
using Game;
using MukioI18n;
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
            _categoryFilter = null;
            _outputFilter = null;
            _searchFilter = null;
            _filteredDefines = null;
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
            _filteredDefines = _nodeDefines.Where(d => filterNodeDefine(d)).ToArray();

            var viewDatas = _filteredDefines.Select(d =>
            {
                string name = getNodeDefineName(d);
                if (d.actionDefine != null)
                {
                    Sprite spr = null;
                    return new NodeViewData(name, spr);
                }
                if (d.eventDefine != null)
                {
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

            var categoryNames = categories.Select(c => _actionGraphCtrl.tryTranslateWithDict(c)).ToArray();
            _nodesPanel.initCategories(categoryNames, categories);
            _nodesPanel.initTypes(game.editor.getEditorVarTypeNames());
            _nodesPanel.onItemBeginDrag += OnNodeItemBeginDrag;
            _nodesPanel.onItemDrag += OnNodeItemDrag;
            _nodesPanel.onItemEndDrag += OnNodeItemEndDrag;
            _nodesPanel.onCategoryFilterEndEdit += onCategoryFilterEndEdit;
            _nodesPanel.onOutputFilterValueChanged += onOutputFilterValueChanged;
            _nodesPanel.onSearchFilterEndEdit += onSearchFilterEndEdit;

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
        private void onCategoryFilterEndEdit(string value)
        {
            _categoryFilter = value;
            updateNodesList();
        }
        private void onOutputFilterValueChanged(bool[] value)
        {
            _outputFilter = value;
            updateNodesList();
        }
        private void onSearchFilterEndEdit(string value)
        {
            _searchFilter = value;
            updateNodesList();
        }
        #endregion
        private void updatePasteButton()
        {
            _pasteButton.interactable = !string.IsNullOrEmpty(scene.editor.nodeClipboard);
        }
        private string getNodeDefineName(NodeDefine define)
        {
            return _actionGraphCtrl.tryTranslateWithDict(define.getDefineEditorName());
        }
        private bool filterNodeDefine(NodeDefine define)
        {
            if (!string.IsNullOrEmpty(_categoryFilter))
            {
                if (_categoryFilter != ACTION_CATEGORY_NAME_NONE)
                {
                    string category = _actionGraphCtrl.tryTranslateWithDict(define.getCategory());
                    if (string.IsNullOrEmpty(category))
                        return false;
                    if (!(category + '/').Contains(_categoryFilter + '/'))
                    {
                        return false;
                    }
                }
            }
            if (_outputFilter != null && _outputFilter.Length > 0)
            {
                var outputs = define.getOutputTypes();
                if (outputs == null || outputs.Length <= 0)
                    return false;

                for (int i = 0; i < _outputFilter.Length; i++)
                {
                    if (!_outputFilter[i])
                        continue;
                    var outputType = game.editor.getEditorVarTypeAt(i);
                    if (!outputs.Any(d => d == outputType))
                    {
                        return false;
                    }
                }
            }
            if (!string.IsNullOrEmpty(_searchFilter))
            {
                string name = getNodeDefineName(define);
                if (string.IsNullOrEmpty(name))
                    return false;
                if (!name.Contains(_searchFilter))
                {
                    return false;
                }
            }
            return true;
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

        private string _categoryFilter;
        private bool[] _outputFilter;
        private string _searchFilter;
        private NodeDefine[] _nodeDefines;
        private NodeDefine[] _filteredDefines;

        [TranslateMsg("没有分类的动作的分类")]
        public const string ACTION_CATEGORY_NAME_NONE = "无";

        public static readonly string[] categories = new string[]
        {
            ACTION_CATEGORY_NAME_NONE,
            StringTable.EDITOR_NODE_CATEGORY_PLAYER,
            StringTable.EDITOR_NODE_CATEGORY_CARD,
            EffectEditorLibrary.EDITOR_NODE_CATEGORY_CARD_DEFINE,
            StringTable.EDITOR_NODE_CATEGORY_PILE,
            StringTable.EDITOR_CATEGORY__ACTION__NAME,
            StringTable.EDITOR_NODE_CATEGORY_BUFF,
            StringTable.EDITOR_NODE_CATEGORY_MODIFIER,
            StringTable.EDITOR_NODE_CATEGORY_EVENT,
            ActionNodeGraphCtrl.EDITOR_CATEGORY_EVENT_ENTRY_NODE,
            StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION,
            EffectEditorLibrary.EDIOTR_CATEGORY_NAME_ADVANCED,
        };
    }
}