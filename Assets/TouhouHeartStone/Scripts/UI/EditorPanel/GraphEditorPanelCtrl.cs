using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Controllers;
using Game;
using Game.Animations;
using HSVPicker;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    internal class GraphEditorPanelCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public void display()
        {
            var categoryNames = categories.Select(c => _actionGraphCtrl.translateCategoryName(c)).ToArray();
            _nodesPanel.initCategories(categoryNames, categories);
            _nodesPanel.initTypes(game.editor.getEditorPortTypeNames());
            _nodesPanel.setCategoryFilter(categoryFilter);
            _nodesPanel.setOutputFilter(outputFilter);
            _nodesPanel.setSearchFilter(searchFilter);
            _createActionDefineDialog.setDefineTypeOptions(defineTypeNames.Select(n => game.lang._(n)).ToArray());
            updatePasteButton();
            updateDeleteButton();
            updateUndoRedoButton();
            updateNodeDefines();
            updateNodesList();

            string[] positions = Array.ConvertAll(visualEffectPositionNames, p => game.lang._p(CONTEXT_VISUAL_EFFECT, p));
            initNodeAnimationPositions(positions);

            hideErrorTip();
            hideCardSelectPanel();
        }
        public void updateNodeDefines()
        {
            var env = getCardPoolEnv();
            var actionDefines = game.card.getAllActionDefines(env);
            var nodeDefines = actionDefines.Select(a => new NodeDefine(a));
            var eventNodeDefines = eventTriggerTimes.Select(e => new NodeDefine(game.card.getEventDefineInEnv(e.defineRef, env), e.type));
            nodeDefines = nodeDefines.Union(eventNodeDefines);

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
                if (d.isEventEntry())
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

        #region 特效对话框
        public void initNodeAnimationDialog(string[] animations)
        {
            _editVisualEffectDialog.initEffects(animations);
        }
        public void initNodeAnimationPositions(string[] positions)
        {
            _editVisualEffectDialog.initPositionOptions(positions);
        }
        public void initNodeAnimationDialogEvents(string[] eventNames)
        {
            _editVisualEffectDialog.initEvents(eventNames);
        }
        public void updateAnimationDialog(VisualEffectDialogViewData viewData)
        {
            _editVisualEffectDialog.update(viewData);
        }
        public void updateAnimationDialogColors(ColorModifyViewData[] viewDatas)
        {
            _editVisualEffectDialog.updateColors(viewDatas);
        }
        public void displayAnimationDialog(VFXSelectSource source)
        {
            _curVFXSelectSource = source;
            initNodeAnimationDialog(_curVFXSelectSource.getAnimationNames());
            _editVisualEffectDialogObj.SetActive(true);
            _editVisualEffectDialog.resetPreview();
        }
        #endregion

        #region 特效调色板
        public void displayColorPicker(Color color)
        {
            _colorPickerPanelObj.SetActive(true);
            _colorPicker.CurrentColor = color;
        }
        #endregion

        #region 特效预览
        public bool isPreviewPlaying()
        {
            return _editVisualEffectDialog.previewTable.isPlaying();
        }
        public void playAnimationPreview(AnimationDefine define, VisualEffectInfo info)
        {
            _editVisualEffectDialog.previewTable.play(define, info);
            _editVisualEffectDialog.updatePreviewPaused();
        }
        public void modifyAnimationPreviewEffectColor(ColorTarget target, Color color)
        {
            _editVisualEffectDialog.previewTable.modifyColor(target, color);
        }
        public void pauseAnimationPreview()
        {
            _editVisualEffectDialog.previewTable.pause();
            _editVisualEffectDialog.updatePreviewPaused();
        }
        public void resumeAnimationPreview()
        {
            _editVisualEffectDialog.previewTable.resume();
            _editVisualEffectDialog.updatePreviewPaused();
        }
        public void switchAnimationPreviewPaused()
        {
            if (_editVisualEffectDialog.previewTable.isPaused())
                resumeAnimationPreview();
            else
                pauseAnimationPreview();
        }
        public void stopAnimationPreview()
        {
            _editVisualEffectDialog.previewTable.stop();
            _editVisualEffectDialog.updatePreviewPaused();
        }
        public Transform getPreviewTableTransform(int position)
        {
            return _editVisualEffectDialog.previewTable.getPreviewTableTransform(position);
        }
        #endregion

        public async Task displayCardSelectPanel(long[] cardPoolFiltersId = null, DefineReference[] characterRefs = null, Predicate<CardDefine> cardFilter = null)
        {
            var cardPoolsRange = getCardPoolEnv()
                .Where(poolRef => game.card.isLocalCardPoolRefExists(poolRef))
                .Select(r => r.CardPoolID)
                .ToArray();
            await _cardSelectPanel.display(cardPoolsRange, cardPoolFiltersId, characterRefs, cardFilter);
            _cardSelectPanelObj.SetActive(true);
        }

        #endregion

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

            _actionGraphCtrl.onOpenNodeInputDefaultValueCardDefine += onOpenActionNodeConstCardDefineCallback;
            _actionGraphCtrl.onOpenEffectPropertyCardDefine += onOpenEffectPropertyCardDefineCallback;

            _nodesPanel.onItemBeginDrag += OnNodeItemBeginDrag;
            _nodesPanel.onItemDrag += OnNodeItemDrag;
            _nodesPanel.onItemEndDrag += OnNodeItemEndDrag;
            _nodesPanel.onItemEditClicked += OnNodeItemEditClicked;
            _nodesPanel.onItemDoubleClicked += OnNodeItemDoubleClicked;
            _nodesPanel.onCategoryFilterEndEdit += onCategoryFilterEndEdit;
            _nodesPanel.onOutputFilterValueChanged += onOutputFilterValueChanged;
            _nodesPanel.onSearchFilterEndEdit += onSearchFilterEndEdit;
            _nodesPanel.onCreateActionClicked += onCreateActionClicked;

            _editVisualEffectDialog.onPropValueChanged += onAnimationPropChangedCallback;
            _editVisualEffectDialog.onConfirm += onAnimationConfirmButtonClickedCallback;
            _editVisualEffectDialog.onColorModifyItemClicked += onAnimationColorModifyItemClickedCallback;
            _editVisualEffectDialogResetColorButton.onClick.AddListener(onAnimationResetColorClickedCallback);

            _editVisualEffectDialog.onShowPreviewButtonClicked += onAnimationShowPreviewButtonClickedCallback;
            _editVisualEffectDialog.onPreviewPlayButtonClicked += onAnimationPreviewPlayButtonClickedCallback;
            _editVisualEffectDialog.onPreviewPauseButtonClicked += onAnimationPreviewPauseButtonClickedCallback;
            _editVisualEffectDialog.onPreviewStopButtonClicked += onAnimationPreviewStopButtonClickedCallback;

            _colorPicker.onValueChanged.AddListener(onColorPickerValueChangedCallback);
            _colorPicker.onHSVChanged.AddListener((h, s, v) => onColorPickerValueChangedCallback(Color.HSVToRGB(h, s, v)));
            _colorPickerConfirmButton.onClick.AddListener(onColorPickerConfirmButtonClickedCallback);

            _createEffectPropDialog.onConfirm += onConfirmCreateEffectPropertyCallback;

            _cancelSelectCardButton?.onClick.AddListener(onClickCancelSelectButtonCallback);

            _cardSelectPanel.enableCount(false);
            _cardSelectPanel.onClickCard += onClickCardSelectPanelCardCallback;
            _cardSelectPanel.onCardHoverEnter += (item, def) => scene.addLocalCardPopup(item.card, def);
            _cardSelectPanel.onCardHoverExit += (item, def) => scene.removeCardPopup(item.card);

            var customPropTypes = EditorManager.getCustomPropTypes();
            var customPropNames = game.editor.getCustomPropNames();
            setEffectPropertyTypes(customPropTypes, customPropNames);

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
            _actionGraphCtrl.addRecordBreakpoint();
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
            GeneratedActionDefine generatedActionDefine = game.card.createActionDefine(curCardPoolId, name, type);
            game.card.addActionDefine(curCardPoolId, generatedActionDefine);
            scene.editor.updateAllEvents();

            generatedActionDefine.setCategory(category);
            displayCreateActionDefineDialog(false);
            await scene.editor.openActionEditor(generatedActionDefine);
        }
        private Task<bool> OnNodeItemBeginDrag(int index, PointerEventData data)
        {
            //记录拖拽信息
            _dragNode = _filteredDefines[index];
            _dragStartPosition = data.pointerCurrentRaycast.worldPosition;
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
            Vector2 worldPos = data.pointerCurrentRaycast.worldPosition;
            if (!_isDraggingHorizontal && !_isDraggingVertical)
            {
                //还没有进入拖拽状态
                var delta = _dragStartPosition - worldPos;
                if (delta.magnitude > 0)
                {
                    if (Math.Abs(delta.x) > Math.Abs(delta.y))
                    {
                        //横向拖拽
                        if (Math.Abs(delta.x) > _dragThreshold)
                        {
                            //横向拖拽并且大于阈值，设置节点显示
                            _isDraggingHorizontal = true;
                            dragNode(data.pointerCurrentRaycast.worldPosition);
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
                dragNode(worldPos);
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
            Vector2 worldPos = data.pointerCurrentRaycast.worldPosition;
            if (isPositionOverActionGraph(worldPos) && !_actionGraphCtrl.isReadonly)
            {
                var pos = _actionGraphContent.InverseTransformPoint(worldPos);
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
        private void onAnimationPropChangedCallback(VisualEffectDialogInfo info)
        {
            _curVFXSelectSource.changeProp(info);
            VisualEffectDialogViewData eventViewData = _curVFXSelectSource.getAnimationViewData();
            updateAnimationDialog(eventViewData);
            ColorModifyViewData[] colorViewData = _curVFXSelectSource.getColorViewDatas();
            updateAnimationDialogColors(colorViewData);
        }
        private void onAnimationConfirmButtonClickedCallback()
        {
            _editVisualEffectDialogObj.SetActive(false);
            _curVFXSelectSource = null;
            _curEditingAnimationColorIndex = -1;
        }
        private void onAnimationColorModifyItemClickedCallback(int index)
        {
            _curEditingAnimationColorIndex = index;
            var color = _curVFXSelectSource.getAnimationColor(index);
            displayColorPicker(color);
        }
        private void onAnimationResetColorClickedCallback()
        {
            _curVFXSelectSource.resetColors();
            var colorViewData = _curVFXSelectSource.getColorViewDatas();
            updateAnimationDialogColors(colorViewData);
        }
        private void onColorPickerValueChangedCallback(Color color)
        {
            _curVFXSelectSource.setAnimationColor(_curEditingAnimationColorIndex, color);
            var colorViewData = _curVFXSelectSource.getColorViewDatas();
            updateAnimationDialogColors(colorViewData);

            if (isPreviewPlaying())
                stopAnimationPreview();
        }
        private void onColorPickerConfirmButtonClickedCallback()
        {
            _colorPickerPanelObj.SetActive(false);
            _curEditingAnimationColorIndex = -1;
        }
        private void onAnimationShowPreviewButtonClickedCallback()
        {
            var value = !_editVisualEffectDialog.isPreviewVisible();
            _editVisualEffectDialog.setPreviewVisible(value);
            var text = game.lang._p(CONTEXT_VISUAL_EFFECT, value ? VISUAL_EFFECT_HIDE_PREVIEW : VISUAL_EFFECT_SHOW_PREVIEW);
            _editVisualEffectDialog.setShowPreviewButtonText(text);
        }
        private void onAnimationPreviewPlayButtonClickedCallback()
        {
            var previewData = _curVFXSelectSource.getPreviewData();
            var animDef = previewData.animDefine;
            if (animDef == null)
                return;

            var animRef = animDef.getDefineRef();
            var srcTrans = getPreviewTableTransform(previewData.sourcePosition);
            var destTrans = getPreviewTableTransform(previewData.targetPosition);
            var info = new VisualEffectInfo()
            {
                srcTransform = srcTrans,
                destTransform = destTrans,
            };
            playAnimationPreview(animDef, info);

            if (previewData.colorModifies == null)
                return;
            var targetGroups = animDef.getColorTargetGroups();
            foreach (var modify in previewData.colorModifies)
            {
                if (modify.animationRef != animRef)
                    continue;
                if (modify.targetIndex < 0 || modify.targetIndex >= targetGroups.Length)
                    continue;
                var targetGroup = targetGroups[modify.targetIndex];
                foreach (var target in targetGroup.targets)
                {
                    modifyAnimationPreviewEffectColor(target, modify.getColor());
                }
            }
        }
        private void onAnimationPreviewPauseButtonClickedCallback()
        {
            switchAnimationPreviewPaused();
        }
        private void onAnimationPreviewStopButtonClickedCallback()
        {
            stopAnimationPreview();
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
                string errorMessage = game.lang._(StringTable.EDITOR_ERROR_NAME_EMPTY);
                showEffectPropertyDialogError(errorMessage);
                return;
            }
            if (_actionGraphCtrl.isPropertyNameTaken(item.propertyName))
            {
                string errorMessage = game.lang._(StringTable.EDITOR_ERROR_NAME_TAKEN);
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
        private async void onOpenActionNodeConstCardDefineCallback(Node node, string name, int paramIndex, int[] itemIndexes)
        {
            var defaultValue = node.getInputDefaultValue(name, paramIndex);
            if (defaultValue.tryGetListElementByIndexes(itemIndexes, out DefineReference defRef))
            {
                await scene.editor.openReferedCard(defRef, setPropAction);
                void setPropAction(CardDefine cardDefine)
                {
                    var newDefRef = new DefineReference(cardDefine.cardPoolId, cardDefine.id);
                    if (defaultValue.replaceListElementByIndexes(itemIndexes, newDefRef, out var newValue))
                    {
                        _actionGraphCtrl.setNodeDefaultValue(node, name, paramIndex, newValue);
                    }
                }
            }
        }
        private async void onOpenEffectPropertyCardDefineCallback(GeneratedEffect effect, string name, int[] itemIndexes)
        {
            var prop = effect.getProp(name);
            if (prop.tryGetListElementByIndexes(itemIndexes, out DefineReference defRef))
            {
                await scene.editor.openReferedCard(defRef, setPropAction);
                void setPropAction(CardDefine cardDefine)
                {
                    var newDefRef = new DefineReference(cardDefine.cardPoolId, cardDefine.id);
                    if (prop.replaceListElementByIndexes(itemIndexes, newDefRef, out var newProp))
                    {
                        _actionGraphCtrl.setEffectProp(name, newDefRef);
                        _actionGraphCtrl.updatePropertyItem(name);
                    }
                }
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
        private void setEffectPropertyTypes(ActionNodeValueType[] types, string[] names)
        {
            _createEffectPropDialog.setTypes(types, names);
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
            _hintMessageText.show(msg);
        }
        private void hideErrorTip()
        {
            _hintMessageText.hide();
        }
        #endregion
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
            return _actionGraphCtrl.getNodeDefineName(define);
        }
        private bool filterNodeDefine(NodeDefine define)
        {
            if (define.isObsolete())
                return false;
            if (!string.IsNullOrEmpty(categoryFilter))
            {
                if (categoryFilter != ACTION_CATEGORY_NAME_NONE)
                {
                    string category = define.getCategory();
                    if (string.IsNullOrEmpty(category))
                        return false;
                    if (!$"{category}/".Contains($"{categoryFilter}/"))
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
                    var outputType = GraphTypes.getPortTypeAt(i);
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
                if (!name.ToUpper().Contains(searchFilter.ToUpper()))
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
                        _actionGraphCtrl.addRecordBreakpoint();
                    }
                }
                if (Input.GetKeyDown(KeyCode.D) && !_actionGraphCtrl.isReadonly)
                {
                    // Ctrl + D：复制并粘贴
                    var clipboard = _actionGraphCtrl.copySelection();
                    scene.editor.nodeClipboard = clipboard;
                    updatePasteButton();
                    if (!string.IsNullOrEmpty(clipboard))
                    {
                        await _actionGraphCtrl.pasteClipboard(clipboard);
                        _actionGraphCtrl.addRecordBreakpoint();
                    }
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

        #region 卡池环境
        private ICardPoolReference[] getCardPoolEnv() => game.editor.getEnvCardPoolRefs();
        #endregion


        #endregion
        #region 事件
        public event Action<Node> onOpenNodeAnimationDialog;
        #endregion

        private bool _deleteButtonDirty;
        private VFXSelectSource _curVFXSelectSource;
        private int _curEditingAnimationColorIndex;
        [SerializeField]
        ActionNodeGraphCtrl _actionGraphCtrl;
        [SerializeField]
        RectTransform _actionGraphViewport;
        [SerializeField]
        RectTransform _actionGraphContent;
        [SerializeField]
        CreateActionDefineDialog _createActionDefineDialog;
        [SerializeField]
        GameObject _editVisualEffectDialogObj;
        [SerializeField]
        VisualEffectEditorDialog _editVisualEffectDialog;
        [SerializeField]
        Button _editVisualEffectDialogResetColorButton;
        [SerializeField]
        GameObject _colorPickerPanelObj;
        [SerializeField]
        ColorPicker _colorPicker;
        [SerializeField]
        Button _colorPickerConfirmButton;

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
        float _dragThreshold = 0.5f;

        [Header("全屏UI引用")]
        [SerializeField]
        private Button _cancelSelectCardButton;
        [SerializeField]
        private GameObject _cardSelectPanelObj;
        [SerializeField]
        private CardPoolViewPanelCtrl _cardSelectPanel;
        [SerializeField]
        private HintMessage _hintMessageText;
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
        public static string ACTION_CATEGORY_NAME_NONE => StringTable.NONE;
        public const string CONTEXT_VISUAL_EFFECT = "画面特效";

        [TranslateMsg("尝试添加空名称的自定义属性的错误提示")]
        public const string EDITOR_ERROR_ACTION_DEFINE_NAME_EMPTY = "请输入动作名称";


        [TranslateMsg("特效动画的始末位置选项", CONTEXT_VISUAL_EFFECT)]
        public const string VISUAL_EFFECT_POSITION_SELF_MASTER = "己方英雄";
        [TranslateMsg("特效动画的始末位置选项", CONTEXT_VISUAL_EFFECT)]
        public const string VISUAL_EFFECT_POSITION_ENEMY_MASTER = "敌方英雄";
        [TranslateMsg("特效动画的始末位置选项", CONTEXT_VISUAL_EFFECT)]
        public const string VISUAL_EFFECT_POSITION_THIS_CARD = "这张卡牌";
        [TranslateMsg("特效动画的始末位置选项", CONTEXT_VISUAL_EFFECT)]
        public const string VISUAL_EFFECT_POSITION_TARGET_CARD = "目标卡牌";
        [TranslateMsg("特效动画的始末位置选项", CONTEXT_VISUAL_EFFECT)]
        public const string VISUAL_EFFECT_POSITION_TEMP_VAR = "临时变量";
        [TranslateMsg("特效动画的始末位置选项", CONTEXT_VISUAL_EFFECT)]
        public const string VISUAL_EFFECT_POSITION_EVENT_VAR = "事件变量";

        [TranslateMsg("特效动画的预览特效按钮", CONTEXT_VISUAL_EFFECT)]
        public const string VISUAL_EFFECT_HIDE_PREVIEW = "隐藏预览";
        [TranslateMsg("特效动画的预览特效按钮", CONTEXT_VISUAL_EFFECT)]
        public const string VISUAL_EFFECT_SHOW_PREVIEW = "显示预览";

        public static readonly string[] categories = new string[]
        {
            StringTable.EDITOR_NODE_CATEGORY_NONE,
            StringTable.EDITOR_NODE_CATEGORY_PLAYER, // 玩家
            StringTable.EDITOR_NODE_CATEGORY_CARD, // 卡牌
            StringTable.EDITOR_NODE_CATEGORY_CARD_DEFINE, // 卡牌定义
            StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, // 卡牌快照
            StringTable.EDITOR_NODE_CATEGORY_PILE, // 牌堆
            StringTable.EDITOR_NODE_CATEGORY_BUFF, //增益
            StringTable.EDITOR_NODE_CATEGORY_EFFECT, // 效果
            StringTable.EDITOR_NODE_CATEGORY_EVENT, // 事件
            StringTable.EDITOR_NODE_CATEGORY_EVENT_ENTRY_NODE, // 事件入口
            StringTable.EDITOR_NODE_CATEGORY_ACTION, // 行动
            StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, // 集合运算
            StringTable.EDITOR_NODE_CATEGORY_MAP, // 映射
            StringTable.EDITOR_NODE_CATEGORY_OTHER, // 其他
        };
        public static readonly NodeDefineType[] defineTypes = new NodeDefineType[]
        {
            NodeDefineType.Action,
            NodeDefineType.Function,
            NodeDefineType.Event
        };
        public static readonly string[] defineTypeNames = new string[]
        {
            StringTable.NODE_TYPE_ACTION,
            StringTable.NODE_TYPE_FUNCTION,
            StringTable.NODE_TYPE_EVENT,
        };

        public static readonly string[] visualEffectPositionNames = new string[]
        {
            VISUAL_EFFECT_POSITION_SELF_MASTER,
            VISUAL_EFFECT_POSITION_ENEMY_MASTER,
            VISUAL_EFFECT_POSITION_THIS_CARD,
            VISUAL_EFFECT_POSITION_TARGET_CARD,
            VISUAL_EFFECT_POSITION_TEMP_VAR,
            VISUAL_EFFECT_POSITION_EVENT_VAR
        };
        private long curCardPoolId => game.editor.currentCardPoolId;
        private EventTriggerTime[] eventTriggerTimes => scene.editor.eventTriggerTimes;
    }
    public static class VisualEffectPosition
    {
        public const int SELF_MASTER = 0;
        public const int ENEMY_MASTER = 1;
        public const int THIS_CARD = 2;
        public const int TARGET_CARD = 3;
        public const int TEMP_VAR = 4;
        public const int EVENT_VAR = 5;
    }
    public struct VFXPreviewData
    {
        public AnimationDefine animDefine;
        public ColorModifyData[] colorModifies;
        public int sourcePosition;
        public int targetPosition;
    }
}