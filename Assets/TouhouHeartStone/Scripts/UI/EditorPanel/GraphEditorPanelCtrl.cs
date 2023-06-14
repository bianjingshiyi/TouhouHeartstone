using System;
using System.Linq;
using System.Threading.Tasks;
using Controllers;
using Game;
using MukioI18n;
using TouhouCardEngine;
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
            updatePasteButton();
            updateDeleteButton();
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
                    bool editable = false;
                    // 如果是自定义动作
                    if (d.actionDefine is GeneratedActionDefine define)
                    {
                        var displaying = scene.editor.getCurrentDisplayInfo();
                        // 如果当前正在编辑的自定义动作不是这个，则可以进行编辑。
                        if (displaying == null || !(displaying.displayObj is GeneratedActionDefine displayingDefine && define.id == displayingDefine.id))
                        {
                            editable = true;
                        }
                    }
                    return new NodeViewData(name, spr, editable);
                }
                if (d.eventDefine != null)
                {
                    Sprite spr = null;
                    return new NodeViewData(name, spr, false);
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
            _deleteButton.onClick.AddListener(OnClickDeleteButton);
            _createActionDefineDialog.onConfirm += onCreateActionDefineCallback;

            _actionGraphCtrl.OnNodeSelected += OnNodeSelected;
            _actionGraphCtrl.OnNodeDeselected += OnNodeDeselected;

            var categoryNames = categories.Select(c => _actionGraphCtrl.tryTranslateWithDict(c)).ToArray();
            _nodesPanel.initCategories(categoryNames, categories);
            _nodesPanel.initTypes(game.editor.getEditorVarTypeNames());
            _nodesPanel.onItemBeginDrag += OnNodeItemBeginDrag;
            _nodesPanel.onItemDrag += OnNodeItemDrag;
            _nodesPanel.onItemEndDrag += OnNodeItemEndDrag;
            _nodesPanel.onItemEditClicked += OnNodeItemEditClicked;
            _nodesPanel.onItemDoubleClicked += OnNodeItemDoubleClicked;
            _nodesPanel.onCategoryFilterEndEdit += onCategoryFilterEndEdit;
            _nodesPanel.onOutputFilterValueChanged += onOutputFilterValueChanged;
            _nodesPanel.onSearchFilterEndEdit += onSearchFilterEndEdit;
            _nodesPanel.onCreateActionClicked += onCreateActionClicked;

            updatePasteButton();
        }
        private void Update()
        {
            if (_deleteButtonDirty)
            {
                updateDeleteButton();
            }
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
        private void OnClickDeleteButton()
        {
            _actionGraphCtrl.deleteSelection();
        }
        private void OnNodeSelected(Node node)
        {
            _deleteButtonDirty = true;
        }
        private void OnNodeDeselected(Node node)
        {
            _deleteButtonDirty = true;
        }
        public async void onCreateActionDefineCallback(string name, string category)
        {
            //新建自定义动作
            if (string.IsNullOrEmpty(name))
            {
                //空名
                showCreateActionDefineDialogError(game.langManager.getString(EDITOR_ERROR_ACTION_DEFINE_NAME_EMPTY));
                return;
            }
            else if (_actionGraphCtrl.isActionDefineNameExist(name))
            {
                //重名
                showCreateActionDefineDialogError(game.langManager.getString(StringTable.EDITOR_ERROR_ACTION_DEFINE_NAME_ALREADY_EXIST));
                return;
            }

            //新建自定义动作
            GeneratedActionDefine generatedActionDefine = game.card.createActionDefine(_cardPoolId, name);
            generatedActionDefine.setCategory(category);
            var clone = game.card.cloneActionDefine(_cardPoolId, generatedActionDefine);
            displayCreateActionDefineDialog(false);
            await scene.editor.openActionEditor(_cardPoolId, clone);
        }
        private Task<bool> OnNodeItemBeginDrag(int index, PointerEventData data)
        {
            //记录拖拽信息
            _dragNode = _filteredDefines[index];
            _dragStartPosition = data.position;
            _isDraggingHorizontal = false;
            _isDraggingVertical = false;
            return Task.FromResult(false);
        }
        private Task<bool> OnNodeItemDrag(int index, PointerEventData data)
        {
            if (_dragNode == null)
            {
                //判断是否在拖拽节点，避免事件错误发生
                return Task.FromResult(false);
            }
            if (!_isDraggingHorizontal && !_isDraggingVertical)
            {
                //还没有进入拖拽状态
                var delta = _dragStartPosition - data.position;
                if (delta.magnitude > 0)
                {
                    if (Math.Abs(delta.x) > Math.Abs(delta.y))
                    {
                        //横向拖拽
                        if (Math.Abs(delta.x) > _dragThreshold)
                        {
                            //横向拖拽并且大于阈值，设置节点显示
                            _isDraggingHorizontal = true;
                            showDragNode();
                            moveDragNode(data.position);
                        }
                    }
                    else
                    {
                        //纵向拖拽
                        _isDraggingVertical = true;
                        return Task.FromResult(false);
                    }
                }
                return Task.FromResult(false);
            }
            else if (_isDraggingHorizontal)
            {
                showDragNode();
                moveDragNode(data.position);
                return Task.FromResult(true);
            }
            else if (_isDraggingVertical)
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(false);
        }
        private async Task<bool> OnNodeItemEndDrag(int index, PointerEventData data)
        {
            if (_isDraggingVertical)
            {
                //纵向拖拽
                //重置状态字段
                _dragNode = null;
                _dragStartPosition = Vector2.zero;
                _isDraggingHorizontal = false;
                _isDraggingVertical = false;
                return false;
            }
            if (_dragNode == null)
            {
                //判断是否在拖拽节点，避免事件错误发生
                return false;
            }
            //执行操作
            if (isPositionOverActionGraph(data.position))
            {
                var pos = _actionGraphContent.InverseTransformPoint(data.position);
                var node = await _actionGraphCtrl.createNode(_dragNode, pos);
                if (node != null)
                    _actionGraphCtrl.moveNodeToFreePosition(node);
            }
            //重置状态字段
            _dragNode = null;
            _dragStartPosition = Vector2.zero;
            _isDraggingHorizontal = false;
            _isDraggingVertical = false;
            //隐藏拖拽节点
            hideDragNode();
            //不管做什么都必须return false以保证ScrollView的释放事件被触发
            //否则会导致ScrollView不能正确回弹
            return false;
        }
        private void OnNodeItemEditClicked(int index)
        {
            if (index < 0 || index >= _filteredDefines.Length)
                return;
            var define = _filteredDefines[index].actionDefine as GeneratedActionDefine;
            if (define != null)
            {
                GeneratedActionDefine generatedActionDefine = define;
                var clone = game.card.cloneActionDefine(_cardPoolId, generatedActionDefine);
                scene.editor.openActionEditor(_cardPoolId, clone);
            }
        }
        private async void OnNodeItemDoubleClicked(int index)
        {
            if (index < 0 || index >= _filteredDefines.Length)
                return;
            var define = _filteredDefines[index];
            var pos = _actionGraphCtrl.getViewportCenter();
            var node = await _actionGraphCtrl.createNode(define, pos);
            if (node != null)
                _actionGraphCtrl.moveNodeToFreePosition(node);
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
        private void onCreateActionClicked()
        {
            displayCreateActionDefineDialog(true);
        }
        #endregion
        #region 节点拖拽
        /// <summary>
        /// 判断坐标是否位于卡组卡牌列表区域中
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool isPositionOverActionGraph(Vector2 position)
        {
            return _actionGraphViewport.rect.Contains(_actionGraphViewport.InverseTransformPoint(position));
        }
        private void moveDragNode(Vector2 position)
        {
            _dragNodeItemObj.transform.position = position;
        }
        private void showDragNode()
        {
            if (_dragNode == null)
                return;
            _dragNodeItemObj.SetActive(true);
            var name = getNodeDefineName(_dragNode);
            Sprite icon = null;
            _dragNodeItem.setNode(name, icon, false);
        }
        private void hideDragNode()
        {
            _dragNodeItemObj.SetActive(false);
        }
        #endregion
        #region 创建自定义动作
        public void displayCreateActionDefineDialog(bool display)
        {
            if (display)
            {
                _createActionDefineDialog.display();
            }
            else
            {
                _createActionDefineDialog.hide();
            }
        }
        public void showCreateActionDefineDialogError(string message)
        {
            _createActionDefineDialog.showErrorTip(message);
        }
        #endregion
        private void updatePasteButton()
        {
            _pasteButton.interactable = !string.IsNullOrEmpty(scene.editor.nodeClipboard);
        }
        private void updateDeleteButton()
        {
            var selectedNodes = _actionGraphCtrl.getSelectedNodes();
            _deleteButton.interactable = selectedNodes != null && selectedNodes.Length > 0 && selectedNodes.Any(n => _actionGraphCtrl.CanNodeBeDeleted(n));
            _deleteButtonDirty = false;
        }
        private string getNodeDefineName(NodeDefine define)
        {
            if (define.actionDefine != null)
            {
                return _actionGraphCtrl.tryTranslateWithDict(define.actionDefine.defineName);
            }
            if (define.eventDefine != null)
            {
                return EventTypeInfoLoader.translateEventTypeNameForEditor(game.lang, define.eventDefine.eventName);
            }
            return game.lang._(StringTable.EDITOR_NODE_NAME_UNKNOW);
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
        private bool _deleteButtonDirty;
        [SerializeField]
        ActionNodeGraphCtrl _actionGraphCtrl;
        [SerializeField]
        RectTransform _actionGraphViewport;
        [SerializeField]
        RectTransform _actionGraphContent;
        [SerializeField]
        CreateActionDefineDialog _createActionDefineDialog;

        [Header("按钮")]
        [SerializeField]
        Button _singleSelectButton;
        [SerializeField]
        Button _multiSelectButton;
        [SerializeField]
        Button _copyButton;
        [SerializeField]
        Button _pasteButton;
        [SerializeField]
        Button _deleteButton;

        [Header("高亮")]
        [SerializeField]
        GameObject _singleSelectObj;
        [SerializeField]
        GameObject _multiSelectObj;

        [Header("节点")]
        [SerializeField]
        NodesPanel _nodesPanel;
        [SerializeField]
        GameObject _dragNodeItemObj;
        [SerializeField]
        NodesPanelItem _dragNodeItem;
        [SerializeField]
        float _dragThreshold = 50f;
        Vector2 _dragStartPosition;
        bool _isDraggingHorizontal = false;
        bool _isDraggingVertical = false;
        NodeDefine _dragNode;

        private string _categoryFilter;
        private bool[] _outputFilter;
        private string _searchFilter;
        private NodeDefine[] _nodeDefines;
        private NodeDefine[] _filteredDefines;

        [TranslateMsg("没有分类的动作的分类")]
        public const string ACTION_CATEGORY_NAME_NONE = "无";
        [TranslateMsg("尝试添加空名称的自定义属性的错误提示")]
        public const string EDITOR_ERROR_ACTION_DEFINE_NAME_EMPTY = "请输入动作名称";

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