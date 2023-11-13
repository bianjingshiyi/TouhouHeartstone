using System;
using System.Collections.Generic;
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
            _nodesPanel.setCategoryFilter(categoryFilter);
            _nodesPanel.setOutputFilter(outputFilter);
            _nodesPanel.setSearchFilter(searchFilter);
            _createActionDefineDialog.setDefineTypeOptions(defineTypeNames.Select(n => game.lang._(n)).ToArray());
            _eventTriggerTimes = game.card.getAllEventDefines(_cardPoolId).getEventTriggerTimes();
            updatePasteButton();
            updateDeleteButton();
            updateUndoRedoButton();
            updateNodeDefines();
            updateNodesList();

            var animations = game.anims.getAllAnimationDefines().Select(d => game.lang._(d.name)).Prepend(game.lang._(VISUAL_EFFECT_NAME_NONE)).ToArray();
            var positions = visualEffectPositions.Select(p => game.lang._(p)).ToArray();
            initNodeAnimationDialog(animations, positions);

            hideErrorTip();
            hideCardSelectPanel();
        }
        public void updateNodeDefines()
        {
            var actionDefines = game.card.getAllActionDefines(_cardPoolId); ;
            var nodeDefines = actionDefines.Select(a => new NodeDefine(a));
            nodeDefines = nodeDefines.Union(_eventTriggerTimes.Select(e => new NodeDefine(e)));

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
                        var displaying = scene.editor.getCurrentDisplayPage();
                        var displayingDefine = displaying?.displayObj?.actionDefine;
                        // 如果当前正在编辑的自定义动作不是这个，则可以进行编辑。
                        if (displayingDefine == null || !(define.cardPoolId == displayingDefine.cardPoolId && define.defineId == displayingDefine.defineId))
                        {
                            editable = true;
                        }
                    }
                    return new NodeViewData(name, spr, editable);
                }
                if (d.eventTriggerTime != null)
                {
                    Sprite spr = null;
                    return new NodeViewData(name, spr, false);
                }
                return default;
            }).ToArray();
            _nodesPanel.updateList(viewDatas);
        }
        public void updateUndoRedoButton()
        {
            _undoButton.interactable = _actionGraphCtrl.canUndo();
            _redoButton.interactable = _actionGraphCtrl.canRedo();
        }
        public void initNodeAnimationDialog(string[] animations, string[] positions)
        {
            _editVisualEffectDialog.initEffects(animations);
            _editVisualEffectDialog.initSourcePositionOptions(positions);
            _editVisualEffectDialog.initTargetPositionOptions(positions);
        }
        public void setNodeAnimationDialog(int animIndex, VisualEffectDialogInfo info)
        {
            _editVisualEffectDialog.setAnimation(animIndex + 1);
            _editVisualEffectDialog.setVisualEffectInfo(info);
        }
        public void displayNodeAnimationDialog(Node node)
        {
            _editVisualEffectDialogObj.SetActive(true);
            _curEditingVisualEffectNode = node;
        }
        #region 私有方法
        #region 生命周期
        void Awake()
        {
            _undoButton.onClick.AddListener(OnClickUndoButton);
            _redoButton.onClick.AddListener(OnClickRedoButton);
            _copyButton.onClick.AddListener(OnClickCopyButton);
            _pasteButton.onClick.AddListener(OnClickPasteButton);
            _deleteButton.onClick.AddListener(OnClickDeleteButton);
            _createActionDefineDialog.onConfirm += onCreateActionDefineCallback;

            _actionGraphCtrl.OnNodeSelected += OnNodeSelected;
            _actionGraphCtrl.OnNodeDeselected += OnNodeDeselected;
            _actionGraphCtrl.OnGraphEdited += OnGraphEditedCallback;
            _actionGraphCtrl.OnNodeVisualEffectButtonClicked += OnNodeVisualEffectButtonClicked;
            _actionGraphCtrl.onStartSelectCard += onStartSelectCardForGraphCallback;
            _actionGraphCtrl.onShowErrorTip += onShowErrorTipCallback;
            _actionGraphCtrl.onAddEffectPropertyButtonClicked += onAddEffectPropertyButtonClickedCallback;

            _actionGraphCtrl.onOpenActionNodeConstCardDefine += onOpenActionNodeConstCardDefineCallback;
            _actionGraphCtrl.onOpenActionNodeConstItemCardDefine += onOpenActionNodeConstItemCardDefineCallback;
            _actionGraphCtrl.onOpenEffectPropertyCardDefine += onOpenEffectPropertyCardDefineCallback;
            _actionGraphCtrl.onOpenEffectPropertyItemCardDefine += onOpenEffectPropertyItemCardDefineCallback;

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

            _editVisualEffectDialog.onEffectDropdownValueChanged += onAnimationValueChangedCallback;
            _editVisualEffectDialog.onPropValueChanged += onAnimationPropChangedCallback;
            _editVisualEffectDialog.onConfirm += onAnimationConfirmButtonClickedCallback;

            _createEffectPropDialog.onConfirm += onConfirmCreateEffectPropertyCallback;

            _cancelSelectCardButton?.onClick.AddListener(onClickCancelSelectButtonCallback);

            _cardSelectPanel.enableCount(false);
            _cardSelectPanel.onClickCard += onClickCardSelectPanelCardCallback;

            var customPropTypes = CardEditorCtrl.customPropTypes.Select(a => (a.type, game.lang._(a.name))).ToArray();
            setEffectPropertyTypes(customPropTypes);

            hideCardSelectPanel();

        }
        private void Update()
        {
            if (_deleteButtonDirty)
            {
                updateDeleteButton();
            }

#if UNITY_STANDALONE
            shortcutsUpdate();
#endif
        }
        #endregion
        #region 事件回调
        private void OnClickUndoButton()
        {
            _actionGraphCtrl.undo();
            updateUndoRedoButton();
        }
        private void OnClickRedoButton()
        {
            _actionGraphCtrl.redo();
            updateUndoRedoButton();
        }
        private void OnClickCopyButton()
        {
            var clipboard = _actionGraphCtrl.copySelection();
            scene.editor.nodeClipboard = clipboard;
            updatePasteButton();
        }
        private async void OnClickPasteButton()
        {
            if (_actionGraphCtrl.isReadonly)
                return;
            var clipboard = scene.editor.nodeClipboard;
            if (string.IsNullOrEmpty(clipboard))
                return;
            await _actionGraphCtrl.pasteClipboard(clipboard);
            updatePasteButton();
        }
        private void OnClickDeleteButton()
        {
            if (_actionGraphCtrl.isReadonly)
                return;
            _actionGraphCtrl.deleteSelection();
            _actionGraphCtrl.addRecordBreakpoint();
        }
        private void OnNodeSelected(Node node)
        {
            _deleteButtonDirty = true;
        }
        private void OnNodeDeselected(Node node)
        {
            _deleteButtonDirty = true;
        }
        private void OnGraphEditedCallback()
        {
            updateUndoRedoButton();
        }
        private void OnNodeVisualEffectButtonClicked(Node node)
        {
            if (_actionGraphCtrl.isReadonly)
                return;
            onOpenNodeAnimationDialog?.Invoke(node);
        }
        public async void onCreateActionDefineCallback(string name, string category, int typeIndex)
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
            var type = defineTypes[typeIndex];
            GeneratedActionDefine generatedActionDefine = game.card.createActionDefine(_cardPoolId, name, type);
            generatedActionDefine.setCategory(category);
            displayCreateActionDefineDialog(false);
            await scene.editor.openActionEditor(generatedActionDefine);
        }
        private Task<bool> OnNodeItemBeginDrag(int index, PointerEventData data)
        {
            //记录拖拽信息
            _dragNode = _filteredDefines[index];
            _dragStartPosition = data.position;
            _isDraggingHorizontal = false;
            _isDraggingVertical = false;
            _actionGraphCtrl.hidePreviewNode();
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
                            dragNode(data.position);
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
                dragNode(data.position);
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
            if (isPositionOverActionGraph(data.position) && !_actionGraphCtrl.isReadonly)
            {
                var pos = _actionGraphContent.InverseTransformPoint(data.position);
                pos = _actionGraphCtrl.getNodeSnapPreviewPosition(pos);
                var node = await _actionGraphCtrl.createNode(_dragNode, pos);
                if (node != null && _actionGraphCtrl.isNodeSnapped())
                {
                    _actionGraphCtrl.connectSnapConnections(node);
                    _actionGraphCtrl.moveNodeToFreePosition(node);
                }
                _actionGraphCtrl.addRecordBreakpoint();
            }
            _actionGraphCtrl.hidePreviewNode();
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
            if (_filteredDefines[index].actionDefine is GeneratedActionDefine define)
            {
                scene.editor.openActionEditor(define);
            }
        }
        private async void OnNodeItemDoubleClicked(int index)
        {
            if (_actionGraphCtrl.isReadonly)
                return;
            if (index < 0 || index >= _filteredDefines.Length)
                return;
            var define = _filteredDefines[index];
            var pos = _actionGraphCtrl.getViewportCenter();
            var node = await _actionGraphCtrl.createNode(define, pos);
            if (node != null)
                _actionGraphCtrl.moveNodeToFreePosition(node);
            _actionGraphCtrl.addRecordBreakpoint();
        }
        private void onCategoryFilterEndEdit(string value)
        {
            categoryFilter = value;
            updateNodesList();
        }
        private void onOutputFilterValueChanged(bool[] value)
        {
            outputFilter = value;
            updateNodesList();
        }
        private void onSearchFilterEndEdit(string value)
        {
            searchFilter = value;
            updateNodesList();
        }
        private void onCreateActionClicked()
        {
            displayCreateActionDefineDialog(true);
        }
        #region 特效
        private void onAnimationValueChangedCallback(int index)
        {
            onAnimationValueChanged?.Invoke(_curEditingVisualEffectNode, index - 1);
        }
        private void onAnimationPropChangedCallback(VisualEffectDialogInfo info)
        {
            onAnimationPropChanged?.Invoke(_curEditingVisualEffectNode, info);
        }
        private void onAnimationConfirmButtonClickedCallback()
        {
            _editVisualEffectDialogObj.SetActive(false);
            _curEditingVisualEffectNode = null;
        }
        #endregion

        #region 效果属性
        private void onAddEffectPropertyButtonClickedCallback()
        {
            if (_actionGraphCtrl.isReadonly)
                return;
            displayEffectPropertyDialog(true);
        }
        private void onConfirmCreateEffectPropertyCallback(PropertyItemInfo item)
        {
            if (_actionGraphCtrl.isReadonly)
                return;
            if (string.IsNullOrEmpty(item.propertyName))
            {
                string errorMessage = game.lang._(NODE_EFFECT_PROP_NAME_EMPTY);
                showEffectPropertyDialogError(errorMessage);
                return;
            }
            if (_actionGraphCtrl.isPropertyNameTaken(item.propertyName))
            {
                string errorMessage = game.lang._(NODE_EFFECT_PROP_NAME_TAKEN);
                showEffectPropertyDialogError(errorMessage);
                return;
            }
            _actionGraphCtrl.addEffectProperty(item);
            displayEffectPropertyDialog(false);
        }
        #endregion

        #region 卡牌选择
        private Task onStartSelectCardForGraphCallback(long cardPoolId)
        {
            return displayCardSelectPanel(cardPoolFiltersId: new long[] { cardPoolId }, cardFilter: c => true);
        }
        private void onClickCancelSelectButtonCallback()
        {
            _actionGraphCtrl.resetCardSelectInfo();
            //隐藏UI
            hideCardSelectPanel();
        }
        private void onClickCardSelectPanelCardCallback(CardDefine define)
        {
            _actionGraphCtrl.setSelectedCardFromPanel(define);
            //隐藏UI
            hideCardSelectPanel();
        }
        #endregion

        #region 卡牌跳转
        private async void onOpenActionNodeConstCardDefineCallback(ActionNode action, string name)
        {
            var value = action.getConst(name) as DefineReference;
            await scene.editor.openReferedCard(value, setPropAction);
            void setPropAction(CardDefine cardDefine)
            {
                _actionGraphCtrl.setNodeConst(action, name, new DefineReference(cardDefine.cardPoolId, cardDefine.id));
            }
        }
        private async void onOpenActionNodeConstItemCardDefineCallback(ActionNode action, string name, int index)
        {
            IList<DefineReference> list = null;
            if (action.getConst(name) is IList<DefineReference> defList)
            {
                list = defList;
            }
            await scene.editor.openReferedCard(list[index], setPropAction);
            void setPropAction(CardDefine cardDefine)
            {
                list[index] = new DefineReference(cardDefine.cardPoolId, cardDefine.id);
                _actionGraphCtrl.setNodeConst(action, name, list);
            };
        }
        private async void onOpenEffectPropertyCardDefineCallback(GeneratedEffect effect, string name)
        {
            var prop = effect.getProp<DefineReference>(name);
            await scene.editor.openReferedCard(prop, setPropAction);

            void setPropAction(CardDefine cardDefine)
            {
                _actionGraphCtrl.setEffectProp(name, new DefineReference(cardDefine.cardPoolId, cardDefine.id));
                _actionGraphCtrl.updatePropertyItem(name);
            }
        }
        private async void onOpenEffectPropertyItemCardDefineCallback(GeneratedEffect effect, string name, int index)
        {
            var prop = effect.getProp<IList<DefineReference>>(name);
            if (prop == null || index < 0 || index >= prop.Count)
                return;
            var define = prop[index];
            await scene.editor.openReferedCard(define, setPropAction);

            void setPropAction(CardDefine cardDefine)
            {
                prop[index] = new DefineReference(cardDefine.cardPoolId, cardDefine.id);
                _actionGraphCtrl.setEffectProp(name, prop);
                _actionGraphCtrl.updatePropertyItem(name);
            }
        }
        #endregion

        #region 错误信息
        private void onShowErrorTipCallback(string msg)
        {
            showErrorTip(msg);
        }
        #endregion

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

        #region 效果属性
        private void setEffectPropertyTypes((ActionNodeValueType type, string name)[] types)
        {
            _createEffectPropDialog.setTypes(types);
        }
        private void showEffectPropertyDialogError(string message)
        {
            _createEffectPropDialog.showErrorTip(message);
        }
        private void displayEffectPropertyDialog(bool display)
        {
            if (display)
            {
                _createEffectPropDialog.display();
            }
            else
            {
                _createEffectPropDialog.hide();
            }
        }
        #endregion

        #region 错误信息
        private void showErrorTip(string msg)
        {
            _hintMessageText.text = msg;
            _hintMessageText.gameObject.SetActive(false);
            _hintMessageText.gameObject.SetActive(true);
            var sePlayer = _hintMessageText.gameObject.GetComponent<SEPlayer>();
            if (sePlayer)
                sePlayer.Play();
        }
        private void hideErrorTip()
        {
            _hintMessageText.text = string.Empty;
            _hintMessageText.gameObject.SetActive(false);
        }
        #endregion
        private async Task displayCardSelectPanel(long[] cardPoolsRange = null, long[] cardPoolFiltersId = null, DefineReference[] characterRefs = null, Predicate<CardDefine> cardFilter = null)
        {
            await _cardSelectPanel.display(cardPoolsRange, cardPoolFiltersId, characterRefs, cardFilter);
            _cardSelectPanelObj.SetActive(true);
        }
        private void hideCardSelectPanel()
        {
            _cardSelectPanelObj.SetActive(false);
        }

        private void updatePasteButton()
        {
            _pasteButton.interactable = !string.IsNullOrEmpty(scene.editor.nodeClipboard) && !_actionGraphCtrl.isReadonly;
        }
        private void updateDeleteButton()
        {
            var selectedNodes = _actionGraphCtrl.getSelectedNodes();
            _deleteButton.interactable = selectedNodes != null && selectedNodes.Length > 0 && selectedNodes.Any(n => ActionNodeGraphCtrl.CanNodeBeDeleted(n)) && !_actionGraphCtrl.isReadonly;
            _deleteButtonDirty = false;
        }
        private string getNodeDefineName(NodeDefine define)
        {
            if (define.actionDefine != null)
            {
                return _actionGraphCtrl.tryTranslateWithDict(define.actionDefine.editorName);
            }
            if (define.eventTriggerTime != null)
            {
                return game.eventManager.translateEventTriggerTimeNameForEditor(define.eventTriggerTime);
            }
            return game.lang._(StringTable.EDITOR_NODE_NAME_UNKNOW);
        }
        private bool filterNodeDefine(NodeDefine define)
        {
            if (define.isObsolete())
                return false;
            if (!string.IsNullOrEmpty(categoryFilter))
            {
                if (categoryFilter != ACTION_CATEGORY_NAME_NONE)
                {
                    string category = _actionGraphCtrl.tryTranslateWithDict(define.getCategory());
                    if (string.IsNullOrEmpty(category))
                        return false;
                    if (!($"{category}/").Contains($"{categoryFilter}/"))
                    {
                        return false;
                    }
                }
            }
            if (outputFilter != null && outputFilter.Length > 0)
            {
                var outputs = define.getOutputTypes();
                if (outputs == null || outputs.Length <= 0)
                    return false;

                for (int i = 0; i < outputFilter.Length; i++)
                {
                    if (!outputFilter[i])
                        continue;
                    var outputType = game.editor.getEditorVarTypeAt(i);
                    if (!outputs.Any(d => d == outputType))
                    {
                        return false;
                    }
                }
            }
            if (!string.IsNullOrEmpty(searchFilter))
            {
                string name = getNodeDefineName(define);
                if (string.IsNullOrEmpty(name))
                    return false;
                if (!name.Contains(searchFilter))
                {
                    return false;
                }
            }
            return true;
        }
        private void dragNode(Vector2 position)
        {
            if (isPositionOverActionGraph(position))
            {
                hideDragNode();
                var pos = _actionGraphContent.InverseTransformPoint(position);
                _actionGraphCtrl.dragNodeItemToGraph(_dragNode, pos);
            }
            else
            {
                showDragNode();
                moveDragNode(position);
                _actionGraphCtrl.hidePreviewNode();
            }
        }

        private async void shortcutsUpdate()
        {
            if (!_actionGraphCtrl.isFocused())
                return;
            // PC快捷键
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    // Ctrl + C：复制
                    var clipboard = _actionGraphCtrl.copySelection();
                    scene.editor.nodeClipboard = clipboard;
                    updatePasteButton();
                }
                if (Input.GetKeyDown(KeyCode.V) && !_actionGraphCtrl.isReadonly)
                {
                    // Ctrl + V：粘贴
                    var clipboard = scene.editor.nodeClipboard;
                    if (!string.IsNullOrEmpty(clipboard))
                    {
                        await _actionGraphCtrl.pasteClipboard(clipboard);
                        updatePasteButton();
                    }
                }
                if (Input.GetKeyDown(KeyCode.D) && !_actionGraphCtrl.isReadonly)
                {
                    // Ctrl + D：复制并粘贴
                    var clipboard = _actionGraphCtrl.copySelection();
                    scene.editor.nodeClipboard = clipboard;
                    if (!string.IsNullOrEmpty(clipboard))
                    {
                        await _actionGraphCtrl.pasteClipboard(clipboard);
                    }
                    updatePasteButton();
                }
                if (Input.GetKeyDown(KeyCode.X) && !_actionGraphCtrl.isReadonly)
                {
                    // Ctrl + X：复制并删除
                    var clipboard = _actionGraphCtrl.copySelection();
                    scene.editor.nodeClipboard = clipboard;
                    updatePasteButton();
                    _actionGraphCtrl.deleteSelection();
                    _actionGraphCtrl.addRecordBreakpoint();
                }
                if (Input.GetKeyDown(KeyCode.Z) && !_actionGraphCtrl.isReadonly)
                {
                    // Ctrl + Z：撤销
                    if (_actionGraphCtrl.canUndo())
                    {
                        _actionGraphCtrl.undo();
                        updateUndoRedoButton();
                    }
                }
                if (Input.GetKeyDown(KeyCode.Y) && !_actionGraphCtrl.isReadonly)
                {
                    // Ctrl + Y：重做
                    if (_actionGraphCtrl.canRedo())
                    {
                        _actionGraphCtrl.redo();
                        updateUndoRedoButton();
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Delete) && !_actionGraphCtrl.isReadonly)
            {
                // Delete：删除
                _actionGraphCtrl.deleteSelection();
                _actionGraphCtrl.addRecordBreakpoint();
            }
        }
        #endregion
        #region 事件
        public event Action<Node> onOpenNodeAnimationDialog;
        public event Action<Node, int> onAnimationValueChanged;
        public event Action<Node, VisualEffectDialogInfo> onAnimationPropChanged;
        #endregion

        private long _cardPoolId;
        private bool _deleteButtonDirty;
        private Node _curEditingVisualEffectNode;
        [SerializeField]
        ActionNodeGraphCtrl _actionGraphCtrl;
        [SerializeField]
        RectTransform _actionGraphViewport;
        [SerializeField]
        RectTransform _actionGraphContent;
        [SerializeField]
        CreateActionDefineDialog _createActionDefineDialog;
        [SerializeField]
        VisualEffectEditorDialog _editVisualEffectDialog;
        [SerializeField]
        GameObject _editVisualEffectDialogObj;

        [Header("按钮")]
        [SerializeField]
        Button _undoButton;
        [SerializeField]
        Button _redoButton;
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

        [Header("全屏UI引用")]
        [SerializeField]
        private Button _cancelSelectCardButton;
        [SerializeField]
        private GameObject _cardSelectPanelObj;
        [SerializeField]
        private CardPoolViewPanelCtrl _cardSelectPanel;
        [SerializeField]
        private Text _hintMessageText;
        [SerializeField]
        private CreatePropertyItemDialog _createEffectPropDialog;


        Vector2 _dragStartPosition;
        bool _isDraggingHorizontal = false;
        bool _isDraggingVertical = false;
        NodeDefine _dragNode;

        private string categoryFilter
        {
            get => game.editor.categoryFilter;
            set => game.editor.categoryFilter = value;
        }
        private bool[] outputFilter
        {
            get => game.editor.outputFilter;
            set => game.editor.outputFilter = value;
        }
        private string searchFilter
        {
            get => game.editor.searchFilter;
            set => game.editor.searchFilter = value;
        }
        private NodeDefine[] _nodeDefines;
        private NodeDefine[] _filteredDefines;

        [TranslateMsg("尝试添加重复名称的自定义属性的错误提示")]
        public const string NODE_EFFECT_PROP_NAME_TAKEN = "该属性名已被使用！";
        [TranslateMsg("尝试添加空名称的自定义属性的错误提示")]
        public const string NODE_EFFECT_PROP_NAME_EMPTY = "请输入属性名！";

        [TranslateMsg("没有分类的动作的分类")]
        public const string ACTION_CATEGORY_NAME_NONE = "无";
        [TranslateMsg("尝试添加空名称的自定义属性的错误提示")]
        public const string EDITOR_ERROR_ACTION_DEFINE_NAME_EMPTY = "请输入动作名称";


        [TranslateMsg("特效动画的无特效引用选项")]
        public const string VISUAL_EFFECT_NAME_NONE = "无";
        [TranslateMsg("特效动画的始末位置选项")]
        public const string VISUAL_EFFECT_POSITION_SELF_MASTER = "己方英雄";
        [TranslateMsg("特效动画的始末位置选项")]
        public const string VISUAL_EFFECT_POSITION_ENEMY_MASTER = "敌方英雄";
        [TranslateMsg("特效动画的始末位置选项")]
        public const string VISUAL_EFFECT_POSITION_THIS_CARD = "这张卡牌";
        [TranslateMsg("特效动画的始末位置选项")]
        public const string VISUAL_EFFECT_POSITION_TARGET_CARD = "目标卡牌";

        public static readonly string[] categories = new string[]
        {
            ACTION_CATEGORY_NAME_NONE,
            StringTable.EDITOR_NODE_CATEGORY_PLAYER, // 玩家
            StringTable.EDITOR_NODE_CATEGORY_CARD, // 卡牌
            EffectEditorLibrary.EDITOR_NODE_CATEGORY_CARD_DEFINE, // 卡牌定义
            EffectEditorLibrary.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, // 卡牌快照
            StringTable.EDITOR_NODE_CATEGORY_PILE, // 牌堆
            StringTable.EDITOR_NODE_CATEGORY_BUFF, //增益
            EffectEditorLibrary.EDITOR_CATEGORY_NAME_EFFECTS, // 效果
            StringTable.EDITOR_NODE_CATEGORY_EVENT, // 事件
            EffectEditorLibrary.EDITOR_TYPE_NAME_EVENT_RECORD, // 事件记录
            ActionNodeGraphCtrl.EDITOR_CATEGORY_EVENT_ENTRY_NODE, // 事件入口
            StringTable.EDITOR_CATEGORY__ACTION__NAME, // 行动
            StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, // 集合运算
            EffectEditorLibrary.EDITOR_CATEGORY_NAME_OTHER, // 其他
        };
        public static readonly NodeDefineType[] defineTypes = new NodeDefineType[]
        {
            NodeDefineType.Action,
            NodeDefineType.Event
        };
        public static readonly string[] defineTypeNames = new string[]
        {
            ActionEditorCtrl.ACTION_DEFINE_TYPE_ACTION,
            ActionEditorCtrl.ACTION_DEFINE_TYPE_EVENT,
        };

        public static readonly string[] visualEffectPositions = new string[]
        {
            VISUAL_EFFECT_POSITION_SELF_MASTER,
            VISUAL_EFFECT_POSITION_ENEMY_MASTER,
            VISUAL_EFFECT_POSITION_THIS_CARD,
            VISUAL_EFFECT_POSITION_TARGET_CARD
        };
        private EventTriggerTime[] _eventTriggerTimes;
    }
    public static class VisualEffectPosition
    {
        public const int SELF_MASTER = 0;
        public const int ENEMY_MASTER = 1;
        public const int THIS_CARD = 2;
        public const int TARGET_CARD = 3;
    }
}