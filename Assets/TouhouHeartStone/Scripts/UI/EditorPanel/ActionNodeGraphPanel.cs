using System;
using System.Collections.Generic;
using System.Linq;
using BJSYGameCore;
using FreespaceFinder;
using Game;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public partial class ActionNodeGraphPanel : MonoBehaviour
    {
        #region 公有方法
        /// <summary>
        /// 在指定位置上创建一个具有指定名称，输入，常量值和输出的节点。
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="outputViewData"></param>
        /// <returns></returns>
        public ActionNodeItem createNode(float posX, float posY, string nodeName,
            PortItemInfo[] inputViewData,
            ActionNodeValueInfo[] constsInfo,
            PortItemInfo[] outputViewData,
            string obsoleteMsg = null,
            int nodeKey = -1)
        {
            //当对应名称数组长度为0的时候，直接隐藏列表根节点。
            ActionNodeItem actionNodeItem = createNode(posX, posY);
            actionNodeItem.onConstValueChanged += onActionNodeItemConstValueChanged;
            actionNodeItem.onClickConstValueButton += onClickActionNodeItemConstValueButton;
            actionNodeItem.onClickConstValueItemButton += onClickActionNodeItemConstValueItemButton;
            actionNodeItem.onClickAddTargetCheckerButton += onClickTriggerNodeAddTargetCheckerButton;
            actionNodeItem.onClickRemoveTargetCheckerButton += onClickTriggerNodeRemoveTargetCheckerButton;
            actionNodeItem.onTargetCheckerTypeValueChanged += onTargetCheckerTypeValueChangedCallback;
            actionNodeItem.onTargetCheckerErrorTipValueChanged += onTargetCheckerErrorTipValueChangedCallback;
            actionNodeItem.onClickHelpButton += onActionNodeClickHelpButton;

            actionNodeItem.onClickAddEffectPropertyButton += onClickAddEffectPropertyButtonCallback;
            actionNodeItem.onClickEffectPropertyItemButton += onClickEffectPropertyItemButtonCallback;
            actionNodeItem.onEffectPropertyItemValueChanged += onEffectPropertyItemValueChangedCallback;
            actionNodeItem.onClickEffectPropertyButton += onClickEffectPropertyButtonCallback;
            actionNodeItem.onEffectPropertyValueChanged += onEffectPropertyValueChangedCallback;
            actionNodeItem.onDeleteEffectProperty += onDeleteEffectPropertyCallback;
            actionNodeItem.onClickEffectPropertyAddItemButton += onClickEffectPropertyAddItemButtonCallback;
            actionNodeItem.onClickEffectPropertyRemoveItemButton += onClickEffectPropertyRemoveItemButtonCallback;

            actionNodeItem.switchToActionNode();
            actionNodeItem.setName(nodeName);
            actionNodeItem.setInputs(inputViewData);
            actionNodeItem.setConsts(constsInfo);
            actionNodeItem.setOutputs(outputViewData);
            actionNodeItem.setObsoleteMsg(obsoleteMsg);
            actionNodeItem.setDocumentKey(nodeKey);
            return actionNodeItem;
        }
        /// <summary>
        /// 在指定位置上创建一个主动效果入口节点。
        /// </summary>
        public ActionNodeItem createActiveEntryNode(float posX, float posY, string nodeName,
            PortItemInfo[] inputViewData,
            string[] targetCheckerTypes, (int typeIndex, string name, string errorTip)[] targetCheckers,
            PortItemInfo[] outputViewData)
        {
            ActionNodeItem actionNodeItem = Instantiate(_actionNodeItemTemplate, _actionNodesRoot);
            _actionNodeItemList.Add(actionNodeItem);
            actionNodeItem.gameObject.SetActive(true);
            RectTransform itemRect = actionNodeItem.transform as RectTransform;
            itemRect.anchoredPosition = new Vector2(posX, posY);
            actionNodeItem.setName(nodeName);
            actionNodeItem.switchToActiveEntryNode();
            actionNodeItem.setActiveEntry(targetCheckerTypes, targetCheckers);
            actionNodeItem.setInputs(inputViewData);
            actionNodeItem.setOutputs(outputViewData);

            actionNodeItem.onBeginDragInputNode += onBeginDragInputNode;
            actionNodeItem.onDragInputNode += onDragInputNode;
            actionNodeItem.onEndDragInputNode += onEndDragInputNode;
            actionNodeItem.onClickInputNode += onClickInputNode;
            actionNodeItem.onBeginDragOutputNode += onBeginDragOutputNode;
            actionNodeItem.onDragOutputNode += onDragOutputNode;
            actionNodeItem.onEndDragOutputNode += onEndDragOutputNode;
            actionNodeItem.onClickOutputNode += onClickOutputNode;
            actionNodeItem.onPointerDown += onPointerDownActionNodeItem;
            actionNodeItem.onPointerUp += onPointerUpActionNodeItem;
            actionNodeItem.onBeginDrag += onBeginDragActionNodeItem;
            actionNodeItem.onDrag += onDragActionNodeItem;
            actionNodeItem.onEndDrag += onEndDragActionNodeItem;
            actionNodeItem.onConstValueChanged += onActionNodeItemConstValueChanged;
            actionNodeItem.onClickConstValueButton += onClickActionNodeItemConstValueButton;
            actionNodeItem.onClickConstValueItemButton += onClickActionNodeItemConstValueItemButton;
            actionNodeItem.onClickAddTargetCheckerButton += onClickTriggerNodeAddTargetCheckerButton;
            actionNodeItem.onClickRemoveTargetCheckerButton += onClickTriggerNodeRemoveTargetCheckerButton;
            actionNodeItem.onTargetCheckerTypeValueChanged += onTargetCheckerTypeValueChangedCallback;
            actionNodeItem.onTargetCheckerErrorTipValueChanged += onTargetCheckerErrorTipValueChangedCallback;
            actionNodeItem.onUpdateTransform += onActionNodeItemUpdateTransform;

            actionNodeItem.onClickAddEffectPropertyButton += onClickAddEffectPropertyButtonCallback;
            actionNodeItem.onClickEffectPropertyItemButton += onClickEffectPropertyItemButtonCallback;
            actionNodeItem.onEffectPropertyItemValueChanged += onEffectPropertyItemValueChangedCallback;
            actionNodeItem.onClickEffectPropertyButton += onClickEffectPropertyButtonCallback;
            actionNodeItem.onEffectPropertyValueChanged += onEffectPropertyValueChangedCallback;
            actionNodeItem.onDeleteEffectProperty += onDeleteEffectPropertyCallback;
            actionNodeItem.onClickEffectPropertyAddItemButton += onClickEffectPropertyAddItemButtonCallback;
            actionNodeItem.onClickEffectPropertyRemoveItemButton += onClickEffectPropertyRemoveItemButtonCallback;

            clearFreePlaceCheckRegion();
            return actionNodeItem;
        }
        /// <summary>
        /// 在指定位置上创建一个具有指定名称的节点。
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public ActionNodeItem createNode(float posX, float posY, string nodeName)
        {
            //当对应名称数组长度为0的时候，直接隐藏列表根节点。
            ActionNodeItem actionNodeItem = createNode(posX, posY);
            actionNodeItem.onConstValueChanged += onActionNodeItemConstValueChanged;
            actionNodeItem.onClickConstValueButton += onClickActionNodeItemConstValueButton;
            actionNodeItem.onClickConstValueItemButton += onClickActionNodeItemConstValueItemButton;
            actionNodeItem.onClickAddTargetCheckerButton += onClickTriggerNodeAddTargetCheckerButton;
            actionNodeItem.onClickRemoveTargetCheckerButton += onClickTriggerNodeRemoveTargetCheckerButton;
            actionNodeItem.onTargetCheckerTypeValueChanged += onTargetCheckerTypeValueChangedCallback;
            actionNodeItem.onTargetCheckerErrorTipValueChanged += onTargetCheckerErrorTipValueChangedCallback;
            actionNodeItem.switchToActionNode();
            actionNodeItem.setName(nodeName);
            return actionNodeItem;
        }

        /// <summary>
        /// 将节点的指定输出连接到目标节点的输入上
        /// </summary>
        /// <param name="node"></param>
        /// <param name="outputIndex"></param>
        /// <param name="targetNode"></param>
        /// <param name="inputIndex"></param>
        public void connectNodeWithOutColor(ActionNodeItem node, int outputIndex, ActionNodeItem targetNode, int inputIndex, ActionNodeConnectionType connectionType)
        {
            if (node == targetNode)
                throw new ArgumentException("一个节点不能连接自己");
            if (_connectionList.Exists(c => c.outputNode == node && c.outputIndex == outputIndex && c.inputNode == targetNode && c.inputIndex == inputIndex))
            {
                //已经存在连接
                return;
            }

            var from = node.getOutput(outputIndex);
            var to = targetNode.getOutput(inputIndex);
            //to = _actionNodeLineRoot.InverseTransformPoint(to);
            //ActionNodeGraphLine line = createLine(from, to);


            ActionNodeGraphLine line = createLine(node.gameObject, targetNode.gameObject);

            _connectionList.Add(new ActionNodeConnectionData()
            {
                outputNode = node,
                outputIndex = outputIndex,
                inputNode = targetNode,
                inputIndex = inputIndex,
                line = line
            });
        }

        /// <summary>
        /// 将节点的指定输出连接到目标节点的输入上
        /// </summary>
        /// <param name="node"></param>
        /// <param name="outputIndex"></param>
        /// <param name="targetNode"></param>
        /// <param name="inputIndex"></param>
        public void connectNode(ActionNodeItem node, int outputIndex, ActionNodeItem targetNode, int inputIndex, ActionNodeConnectionType connectionType)
        {
            if (node == targetNode)
                throw new ArgumentException("一个节点不能连接自己");
            if (_connectionList.Exists(c => c.outputNode == node && c.outputIndex == outputIndex && c.inputNode == targetNode && c.inputIndex == inputIndex))
            {
                //已经存在连接
                return;
            }

            var output = node.getOutput(outputIndex);
            var input = targetNode.getInput(inputIndex);
            if (!output || !input)
            {
                Debug.LogWarning("不存在" + node + ":" + outputIndex + "与" + targetNode + ":" + inputIndex + "之间的连接");
                return;
            }
            ActionNodeGraphLine line = createLine(output.gameObject, input.gameObject, output.GetColor(), input.GetColor());

            _connectionList.Add(new ActionNodeConnectionData()
            {
                outputNode = node,
                outputIndex = outputIndex,
                inputNode = targetNode,
                inputIndex = inputIndex,
                line = line
            });
        }
        /// <summary>
        /// 断开指定节点输出和目标节点输入的连接
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ouputIndex"></param>
        /// <param name="targetNode"></param>
        /// <param name="inputIndex"></param>
        public void unconnectNode(ActionNodeItem node, int ouputIndex, ActionNodeItem targetNode, int inputIndex)
        {
            int index = _connectionList.FindIndex(c =>
                c.outputNode == node &&
                c.outputIndex == ouputIndex &&
                c.inputNode == targetNode &&
                c.inputIndex == inputIndex);
            if (index < 0)
            {
                Debug.LogWarning("不存在" + node + ":" + ouputIndex + "与" + targetNode + ":" + inputIndex + "之间的连接");
                return;
            }
            removeConnection(index);
        }
        /// <summary>
        /// 断开指定节点的所有输入连接
        /// </summary>
        /// <param name="node"></param>
        public void unconnectNodeAllInput(ActionNodeItem node)
        {
            for (int i = 0; i < _connectionList.Count; i++)
            {
                if (_connectionList[i].inputNode == node)
                {
                    removeConnection(i);
                    i--;
                }
            }
        }
        /// <summary>
        /// 将动作节点移动到图上的某个位置
        /// </summary>
        /// <param name="item"></param>
        /// <param name="graphPosition">动作图坐标系坐标</param>
        public void moveActionNodeTo(ActionNodeItem item, Vector2 graphPosition)
        {
            // 设置节点位置
            (item.transform as RectTransform).anchoredPosition = graphPosition; //+ _dragItemPositionOffset;
            item.SetDirty();
        }
        /// <summary>
        /// 从图上移除一个指定节点
        /// </summary>
        /// <param name="node"></param>
        public void removeNode(ActionNodeItem node)
        {
            //销毁节点UI
            _actionNodeItemList.Remove(node);
            Destroy(node.gameObject);
            //设置背景大小
            //销毁所有连线UI
            for (int i = 0; i < _connectionList.Count; i++)
            {
                if (_connectionList[i].inputNode == node || _connectionList[i].outputNode == node)
                {
                    removeConnection(i);
                    i--;
                    continue;
                }
            }
            clearFreePlaceCheckRegion();
        }
        /// <summary>
        /// 清空图上的所有节点和连线
        /// </summary>
        public void clearGraph()
        {
            for (int i = 0; i < _actionNodeItemList.Count; i++)
            {
                if (_actionNodeItemList[i] == null)
                    continue;
                Destroy(_actionNodeItemList[i].gameObject);
            }
            _actionNodeItemList.Clear();
            for (int i = 0; i < _connectionList.Count; i++)
            {
                if (_connectionList[i].line == null)
                    continue;
                Destroy(_connectionList[i].line.gameObject);
            }
            _connectionList.Clear();

            _dragHiddenConnection = null;
            clearFreePlaceCheckRegion();
        }
        public void setReturnButtonActive(bool value)
        {
            _returnButton.gameObject.SetActive(value);
        }
        public Vector2 getActionNodePosition(ActionNodeItem item)
        {
            Vector3 position = item.transform.position;
            position = _actionNodesRoot.InverseTransformPoint(position);
            return position;
        }
        public Vector2 worldPositionToGraphPosition(Vector2 worldPosition)
        {
            return _actionNodesRoot.InverseTransformPoint(worldPosition);
        }

        public void displayCardSelectPanel(bool value)
        {
            _cardSelectPanel.SetActive(value);
        }
        public void initPiles((string name, string value)[] piles)
        {
            initPiles(piles.Select(i => i.name).ToArray(), piles.Select(i => i.value).ToArray());
        }
        public void initPiles(string[] pileNames, string[] pileValues)
        {
            _actionNodeItemTemplate.initPileNames(pileNames, pileValues);
        }
        public void initCardProperties(CardPropertyNameItem[] cardProperties)
        {
            _actionNodeItemTemplate.initCardProperties(cardProperties);
        }
        public void initKeywords(string[] keywordNames, string[] keywordValues)
        {
            _actionNodeItemTemplate.initKeywords(keywordNames, keywordValues);
        }
        public void initEffectTags(string[] effectTagNames, string[] effectTagValues)
        {
            _actionNodeItemTemplate.initEffectTags(effectTagNames, effectTagValues);
        }
        public void initEventNames(string[] eventNames, string[] eventValues)
        {
            _actionNodeItemTemplate.initEventNames(eventNames, eventValues);
        }
        public void resetView()
        {
            _hintMessageText.gameObject.SetActive(false);
            _currentSelectedKnob = default;
            _content.localPosition = Vector3.zero;
            _content.localScale = Vector3.one;
            _selectFrameTrans.gameObject.SetActive(false);
            clearFreePlaceCheckRegion();
        }
        public void updateNodeLines(ActionNodeItem item)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(item.transform as RectTransform);
            var connections = _connectionList.FindAll(c => c.outputNode == item || c.inputNode == item);
            foreach (var connection in connections)
            {
                connection.line.UpdatePoints();
            }
        }
        public Vector2 getNodesRootPosition()
        {
            return _actionNodesRoot.anchoredPosition;
        }
        public Vector2 getViewCenterPosition()
        {
            return -new Vector2(_content.anchoredPosition.x / _content.localScale.y, _content.anchoredPosition.y / _content.localScale.y);
        }

        public void showErrorTip(string msg)
        {
            _hintMessageText.text = msg;
            _hintMessageText.gameObject.SetActive(false);
            _hintMessageText.gameObject.SetActive(true);
            _hintMessageText.gameObject.GetComponent<SEPlayer>()?.Play();
        }

        #region 操作模式/节点选择
        public IEnumerable<ActionNodeItem> getSelectedNodes()
        {
            return _selectedNodes;
        }
        public void clearSelection()
        {
            foreach (ActionNodeItem item in _selectedNodes)
            {
                if (!item)
                    continue;
                item.deselect();
                onNodeDeselected?.Invoke(item);
            }
            _selectedNodes.Clear();
        }
        public void selectNode(ActionNodeItem item)
        {
            _selectedNodes.Add(item);
            item.select();
            onNodeSelected?.Invoke(item);
        }
        public void deselectNode(ActionNodeItem item)
        {
            _selectedNodes.Remove(item);
            item.deselect();
            onNodeDeselected?.Invoke(item);
        }
        public bool isNodeSelected(ActionNodeItem item)
        {
            return _selectedNodes.Contains(item);
        }
        /// <summary>
        /// 取消所有选择，并选择一个节点。。
        /// </summary>
        /// <param name="item">要选择的节点。</param>
        public void clearAndSelectNode(ActionNodeItem item)
        {
            clearSelection();
            selectNode(item);
        }
        #endregion


        #region 效果属性
        public void setEffectPropertyTypes((ActionNodeValueType type, string name)[] types)
        {
            _createEffectPropDialog.setTypes(types);
        }
        public void showEffectPropertyDialogError(string message)
        {
            _createEffectPropDialog.showErrorTip(message);
        }
        public void displayEffectPropertyDialog(bool display)
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
        public bool isNodePositionFree(Vector2 pos, Vector2 size)
        {
            return isNodePositionFree(new Rect(pos, size));
        }
        public bool isNodePositionFree(Rect itemRect)
        {
            var region = getGraphFreePlaceCheckRegion();
            var offset = getGraphFreePlaceCheckRegionOffset();
            var checkEmptyRect = itemRect;
            checkEmptyRect.position += offset;
            return region.ContainsFreeRect(checkEmptyRect);
        }
        public Vector2 findFreeNodeCreatePosition(ActionNodeItem item)
        {
            return findFreeNodeCreatePosition(item.getNodeItemRect());
        }
        public Vector2 findFreeNodeCreatePosition(Rect itemRect)
        {
            return findFreeNodeCreatePosition(itemRect, itemRect.center);
        }
        public Vector2 findFreeNodeCreatePosition(Rect itemRect, Vector2 distancePivot)
        {
            var itemPos = itemRect.center;
            var region = getGraphFreePlaceCheckRegion();
            var offset = getGraphFreePlaceCheckRegionOffset();
            var checkEmptyRect = itemRect;
            checkEmptyRect.position += offset;
            // 如果本来就有空位，直接返回位置。
            if (region.ContainsFreeRect(checkEmptyRect))
            {
                return itemPos;
            }
            // 是否有足够的空间容纳该节点。
            if (region.canCutByNearest(itemRect.width, itemRect.height, itemRect.center + offset, distancePivot + offset, out Rect resultRect))
            {
                // 转换成content的位置。
                Rect contentRect = resultRect;
                contentRect.position -= offset;

                // 寻找空间中最近的位置。
                var range = contentRect;
                // 范围缩小节点的大小，以评估节点的中心点。
                range.size -= itemRect.size;
                range.center = contentRect.center;
                itemPos = findNearestPosInRect(itemRect.center, range);
            }
            return itemPos;
        }
        /// <summary>
        /// 将空余位置检测域设置为不洁净，使其在下次检测空余位置时重新生成一个域。
        /// </summary>
        public void clearFreePlaceCheckRegion()
        {
            _graphRegion = null;
            _graphRegionSize = Vector2.zero;
        }
        public void updateGraphFreePlaceCheckRegion(params ActionNodeItem[] exceptedNodes)
        {
            updateGraphFreePlaceCheckRegion(new Vector2(600, 400), exceptedNodes);
        }
        /// <summary>
        /// 更新用于检测空余位置的域对象。
        /// </summary>
        public void updateGraphFreePlaceCheckRegion(Vector2 rectExpansion, params ActionNodeItem[] exceptedNodes)
        {
            Vector2 contentScale = _content.localScale;
            Vector2 reverseScale = new Vector2(1 / contentScale.x, 1 / contentScale.y);

            RectTransform rectTrans = _viewPort;
            var viewRect = rectTrans.rect;
            viewRect.size = Vector2.Scale(viewRect.size + rectExpansion, reverseScale);
            viewRect.center = Vector2.Scale(-_content.localPosition, reverseScale);


            Vector2 size = viewRect.size;
            _graphRegion = new Region(size.x, size.y);
            _graphRegionSize = size;
            Vector2 offset = 0.5f * size - viewRect.center;

            // 将其他节点所占的位置裁切出去。
            foreach (var nodeItem in _actionNodeItemList)
            {
                if (exceptedNodes.Contains(nodeItem))
                    continue;
                var nodeRect = nodeItem.getNodeItemRect();
                // 只裁切在视图中的节点。
                if (viewRect.Overlaps(nodeRect))
                {
                    nodeRect.position += offset;
                    _graphRegion.Cut(nodeRect);
                }
            }

        }
        /// <summary>
        /// 将操作焦点设置为节点图。
        /// </summary>
        public void setFocused()
        {
            _focused = true;
        }
        /// <summary>
        /// 操作焦点是否为节点图。
        /// </summary>
        public bool isFocused()
        {
            return EventSystem.current.currentSelectedGameObject == gameObject;
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _backgroundEventTrigger.onPointerDown += onPointerDownBackgroundImage;
            _backgroundEventTrigger.onPointerUp += onPointerUpBackgroundImage;
            if (_actionNodeItemTemplate.transform.parent == _actionNodesRoot)
            {
                _actionNodeItemTemplate.gameObject.SetActive(false);
            }
            /*  if (_actionNodeLineTemplate.transform.parent == _actionNodeLineRoot)
              {
                  _actionNodeLineTemplate.gameObject.SetActive(false);
              }*/
            if (_ActionNodeGraphLineTemplate.transform.parent == _actionNodeLineRoot)
            {
                _ActionNodeGraphLineTemplate.gameObject.SetActive(false);
            }

            _returnButton.onClick.AddListener(onClickReturnButtonCallback);
            _cancelSelectButton.onClick.AddListener(onClickCancelSelectButtonCallback);

            displayCardSelectPanel(false);

            // 效果属性。
            _createEffectPropDialog.onConfirm += onConfirmCreateEffectPropertyCallback;
            #region 缩放函数所需要的初始化参数及eventTrigger相关By断剑君
            _backgroundOriginScale = _content.localScale;//读取目前的缩放比例 

            ///UnityAction <BaseEventData> zoomingAction =  new UnityAction <BaseEventData> (EditorZooming);
            EventTrigger.Entry scrollZooming = new EventTrigger.Entry();
            scrollZooming.eventID = EventTriggerType.Scroll;
            scrollZooming.callback.AddListener(onBackgroundScroll);
            EventTrigger addTriggerIntoBackgrond = _backgroundImage.gameObject.AddComponent<EventTrigger>();
            addTriggerIntoBackgrond.triggers.Add(scrollZooming);

            EventTrigger.Entry BackgroundonDrag = new EventTrigger.Entry();
            BackgroundonDrag.eventID = EventTriggerType.Drag;
            BackgroundonDrag.callback.AddListener(onBackgroundDrag);
            EventTrigger addOnDragIntoBackground = _backgroundImage.gameObject.AddComponent<EventTrigger>();
            addOnDragIntoBackground.triggers.Add(BackgroundonDrag);

            EventTrigger.Entry BackgroundBeginDrag = new EventTrigger.Entry();
            BackgroundBeginDrag.eventID = EventTriggerType.BeginDrag;
            BackgroundBeginDrag.callback.AddListener(onBackgroundBeginDrag);
            EventTrigger addBeginDragIntoBackground = _backgroundImage.gameObject.AddComponent<EventTrigger>();
            addBeginDragIntoBackground.triggers.Add(BackgroundBeginDrag);

            EventTrigger.Entry BackgroundEndDrag = new EventTrigger.Entry();
            BackgroundEndDrag.eventID = EventTriggerType.EndDrag;
            BackgroundEndDrag.callback.AddListener(OnBackgroundEndDrag);
            EventTrigger addEndDragIntoBackground = _backgroundImage.gameObject.AddComponent<EventTrigger>();
            addEndDragIntoBackground.triggers.Add(BackgroundEndDrag);

            EventTrigger.Entry ScreenOnTouch = new EventTrigger.Entry();
            ScreenOnTouch.eventID = EventTriggerType.PointerDown;
            ScreenOnTouch.callback.AddListener(OnFingertouch);
            EventTrigger addScreenTouchIntoBackground =
                _backgroundImage.gameObject.AddComponent<EventTrigger>();
            addScreenTouchIntoBackground.triggers.Add(ScreenOnTouch);

            EventTrigger.Entry ScreenDuringTouch = new EventTrigger.Entry();
            ScreenDuringTouch.eventID = EventTriggerType.Drag;
            ScreenDuringTouch.callback.AddListener(onFingerDragging);
            EventTrigger addScreenDuringTouchIntoBackground =
                _backgroundImage.gameObject.AddComponent<EventTrigger>();
            addScreenDuringTouchIntoBackground.triggers.Add(ScreenDuringTouch);

            EventTrigger.Entry ScreenEndTouch = new EventTrigger.Entry();
            ScreenEndTouch.eventID = EventTriggerType.PointerUp;
            ScreenEndTouch.callback.AddListener(onFigerEndTouch);
            EventTrigger addEndTouchIntoBackground =
                _backgroundImage.gameObject.AddComponent<EventTrigger>();
            addScreenTouchIntoBackground.triggers.Add(ScreenEndTouch);

            #endregion
        }
        private void Update()
        {
            if (_focused)
            {
                _focused = false;
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
        #endregion
        #region 事件回调
        private void onPointerDownBackgroundImage(EventTriggerWithoutUseEvent eventTrigger, BaseEventData eventData)
        {
            _longPressTimer.start();
            setFocused();
        }
        private void onPointerUpBackgroundImage(EventTriggerWithoutUseEvent eventTrigger, BaseEventData eventData)
        {
            if (_longPressTimer.isExpired())
            {
                _longPressTimer.reset();
            }
            else
            {
                if (!_isDragging)
                {
                    clearSelection();
                }
            }
            setFocused();
        }
        private void onBackgroundScroll(BaseEventData eventData)
        {
            var delta = Vector3.one * (Input.GetAxis("Mouse ScrollWheel") * zoomSpeed);//读取滚轮的变化           
            var desiredScale = _content.localScale + delta;//进行缩放
            desiredScale = ClampDesiredScale(desiredScale);//对缩放目标进行一个数据的clamp以保证成倍缩放
            _content.localScale = desiredScale;
            clearFreePlaceCheckRegion();
            setFocused();
        }

        private void OnFingertouch(BaseEventData eventData)
        {
            if (Input.touchCount > 1)
            {
                var _currentTouch1 = Input.GetTouch(0);
                var _currentTouch2 = Input.GetTouch(1);
                if (_currentTouch1.phase == TouchPhase.Began || _currentTouch2.phase == TouchPhase.Began)
                {
                    _perviousTouch1 = _currentTouch1;
                    _perviousTouch2 = _currentTouch2;
                }
            }
        }
        /// <summary>
        /// 此方法下的debuglog暂且以注释形式保留，为后续优化做准备
        /// </summary>
        /// <param name="eventData"></param>
        private void onFingerDragging(BaseEventData eventData)
        {
            if (Input.touchCount > 1)
            {
                _isTouchZooming = true;
                var _currentTouch1 = Input.GetTouch(0);
                var _currentTouch2 = Input.GetTouch(1);
                Vector2 _perviousPosition1 = _perviousTouch1.position;
                Vector2 _perviousPosition2 = _perviousTouch2.position;
                Vector2 _currentPosition1 = _currentTouch1.position;
                Vector2 _currentPosition2 = _currentTouch2.position;

                float leng1 = Vector2.Distance(_perviousPosition1, _perviousPosition2);
                float leng2 = Vector2.Distance(_currentPosition1, _currentPosition2);
                var scaleFactor = leng2 / leng1;
                Vector3 prevCenter = (_perviousPosition1 + _perviousPosition2) / 2;
                Vector3 currCenter = (_currentPosition1 + _currentPosition2) / 2;
                Vector3 _localScale = _content.localScale;
                Vector3 desiredScale = _localScale * scaleFactor;

                if (scaleFactor != 1 && desiredScale.x < _maxScale && desiredScale.y < _maxScale && desiredScale.x > _minScale && desiredScale.y > _minScale)
                {
                    var localPos = _content.position;
                    var targetPos = (1 - scaleFactor) * prevCenter + scaleFactor * localPos;
                    //Debug.Log($"c1: {prevCenter}, c2: {currCenter}, factor: {scaleFactor}, delta: {targetPos - localPos}, prev: {localPos}");

                    _content.localScale = desiredScale;
                    _content.position = targetPos;
                }

                _content.position += currCenter - prevCenter;
                clearFreePlaceCheckRegion();

                _perviousTouch1 = _currentTouch1;
                _perviousTouch2 = _currentTouch2;
            }
        }
        public void onFigerEndTouch(BaseEventData eventData)
        {
            if (Input.touchCount <= 1)
            {
                _isTouchZooming = false;
            }
        }
        public void onBackgroundDrag(BaseEventData eventData)
        {
            setFocused();
            _isDragging = true;
            if (shouldDragSelectionFrame(eventData))
            {
                updateSelectFrame();
                return;
            }
            _longPressTimer.reset();
            if (!_isTouchZooming)
            {
                _content.position = (Vector2)Input.mousePosition - _backgroundDragStartPosition;
                clearFreePlaceCheckRegion();
            }
        }
        public void onBackgroundBeginDrag(BaseEventData eventData)
        {
            setFocused();
            if (shouldDragSelectionFrame(eventData))
            {
                _selectFrameDragStartPosition = (Vector2)Input.mousePosition - (Vector2)_content.position;
                return;
            }
            _longPressTimer.reset();
            _backgroundDragStartPosition = (Vector2)Input.mousePosition - (Vector2)_content.position;
        }
        public void OnBackgroundEndDrag(BaseEventData eventData)
        {
            _isDragging = false;
            _selectFrameTrans.gameObject.SetActive(false);
        }
        /// <summary>
        /// 开始拖拽输入接口，创建一根跟着鼠标移动的连线
        /// </summary>
        /// <param name="item"></param>
        /// <param name="input"></param>
        /// <param name="eventData"></param>
        private void onBeginDragInputNode(ActionNodeItem item, ActionNodeItemInput input, BaseEventData eventData)
        {
            if (_isTouchZooming) return;

            setFocused();
            int inputIndex = item.indexOfInput(input);
            int index = _connectionList.FindIndex(c => c.inputNode == item && c.inputIndex == inputIndex);
            if (index >= 0 && !input.isControl)
            {
                _dragHiddenConnection = _connectionList[index].line.gameObject;
                _dragHiddenConnection.SetActive(false);
            }

            GameObject from = input.getNode();
            GameObject to = new GameObject();
            to.name = "Slot";
            GameObject tod = new GameObject();
            tod.name = "Slot_d";
            tod.transform.SetParent(to.transform);
            to.transform.SetParent(_actionNodeLineRoot);
            to.transform.position = eventData.currentInputModule.input.mousePosition;
            _dragLine = createLine(from, to, input.GetColor(), input.GetColor());


            onStartConnection?.Invoke(item, inputIndex, false);
        }
        /// <summary>
        /// 拖拽输入接口，使连线一段的位置跟着鼠标移动
        /// </summary>
        /// <param name="item"></param>
        /// <param name="input"></param>
        /// <param name="eventData"></param>
        private void onDragInputNode(ActionNodeItem item, ActionNodeItemInput input, BaseEventData eventData)
        {
            if (_isTouchZooming) return;

            setFocused();
            GameObject from = input.getNode();
            GameObject to = new GameObject();
            to.name = "Slot";
            GameObject tod = new GameObject();
            tod.name = "Slot_d";
            tod.transform.SetParent(to.transform);
            to.transform.SetParent(_dragLine.transform);
            to.transform.position = eventData.currentInputModule.input.mousePosition;
            _dragLine.setPosition(from, to);

        }
        /// <summary>
        /// 停止拖拽输入接口，销毁拖拽连线，如果在另外一个输出接口上则触发事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="input"></param>
        /// <param name="eventData"></param>
        private void onEndDragInputNode(ActionNodeItem item, ActionNodeItemInput input, BaseEventData eventData)
        {
            var inputIndex = item.indexOfInput(input);
            if (_dragHiddenConnection)
            {
                _dragHiddenConnection.SetActive(true);
                _dragHiddenConnection = null;
            }
            Destroy(_dragLine.gameObject);
            _dragLine = null;
            //使用射线判断是否在其他的输入点上
            List<RaycastResult> raycastList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData as PointerEventData, raycastList);
            for (int i = 0; i < raycastList.Count; i++)
            {
                ActionNodeItemOutput output = raycastList[i].gameObject.GetComponentInParent<ActionNodeItemOutput>();
                if (output == null)
                    continue;
                ActionNodeItem outputItem = output.GetComponentInParent<ActionNodeItem>();
                var outputIndex = outputItem.indexOfOutput(output);
                if (_connectionList.Exists(c => c.inputNode == item && c.inputIndex == inputIndex && c.outputNode == outputItem && c.outputIndex == outputIndex))
                {
                    // 已连接，断开连接。
                    onUnconnectNodes?.Invoke(outputItem, outputIndex, item, inputIndex);
                    onEndConnection?.Invoke(item, inputIndex, false);
                    return;
                }
                else
                {
                    // 未连接，连接。
                    onConnectNodes?.Invoke(outputItem, outputIndex, item, inputIndex);
                    onEndConnection?.Invoke(item, inputIndex, false);
                    return;
                }
            }
            //不在任何点上：
            //如果（该输入端点是值端点）或者（只有一个连接），尝试断开连接。
            int connectCount = _connectionList.Count(c => c.inputNode == item && c.inputIndex == inputIndex);
            if (!input.isControl || connectCount == 1)
            {
                //如果目前的连线已经与节点链接，则断开它们的连接。
                int index = _connectionList.FindIndex(c => c.inputNode == item && c.inputIndex == inputIndex);
                if (index >= 0)
                {
                    onUnconnectNodes?.Invoke(_connectionList[index].outputNode, _connectionList[index].outputIndex, item, inputIndex);
                }
            }
            onEndConnection?.Invoke(item, inputIndex, false);
        }
        /// <summary>
        /// 点击输入接口，如果是原本的接口则取消选择，如果在另外一个接口上则触发事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="input"></param>
        /// <param name="eventData"></param>
        private void onClickInputNode(ActionNodeItem item, ActionNodeItemInput input, BaseEventData eventData)
        {
            setFocused();
            int inputIndex = item.indexOfInput(input);
            if (_currentSelectedKnob.inputNode == item && _currentSelectedKnob.inputIndex == inputIndex)
            {
                // 点击的输入点是本身。
                // 取消选择。
                input?.setSelected(false);
                _currentSelectedKnob = default;
                onEndConnection?.Invoke(item, inputIndex, false);
                return;
            }
            else if (_currentSelectedKnob.outputNode != null && _currentSelectedKnob.outputIndex >= 0)
            {
                // 选择的输出点存在。
                var output = _currentSelectedKnob.outputNode;
                var outputIndex = _currentSelectedKnob.outputIndex;
                bool connected = _connectionList.Any(c => c.inputNode == item && c.inputIndex == inputIndex && c.outputNode == output && c.outputIndex == outputIndex);

                if (connected)
                {
                    // 已经连接，断开连接。
                    onUnconnectNodes?.Invoke(output, outputIndex, item, inputIndex);
                }
                else
                {
                    // 连接输入点与输出点。
                    onConnectNodes?.Invoke(output, outputIndex, item, inputIndex);
                }
                _currentSelectedKnob.outputKnob?.setSelected(false);
                _currentSelectedKnob = default;
                onEndConnection?.Invoke(output, outputIndex, true);
                return;
            }
            else
            {
                // 点击的输入点不是本身，并且当前选择的输出点不存在。
                // 选择新的输入点。
                _currentSelectedKnob.inputKnob?.setSelected(false);
                _currentSelectedKnob.inputNode = item;
                _currentSelectedKnob.inputIndex = inputIndex;
                _currentSelectedKnob.inputKnob = input;
                _currentSelectedKnob.inputKnob?.setSelected(true);
                onStartConnection?.Invoke(item, inputIndex, false);
            }
        }
        /// <summary>
        /// 开始拖拽输入接口，创建一根跟着鼠标移动的连线
        /// </summary>
        /// <param name="item"></param>
        /// <param name="output"></param>
        /// <param name="eventData"></param>
        private void onBeginDragOutputNode(ActionNodeItem item, ActionNodeItemOutput output, BaseEventData eventData)
        {
            if (!_isTouchZooming)
            {
                setFocused();
                int outputIndex = item.indexOfOutput(output);
                int index = _connectionList.FindIndex(c => c.outputNode == item && c.outputIndex == outputIndex);
                if (index >= 0 && output.isControl)
                {
                    _dragHiddenConnection = _connectionList[index].line.gameObject;
                    _dragHiddenConnection.SetActive(false);
                }

                GameObject from = output.getNode();
                GameObject to = new GameObject();
                to.name = "Slot";
                GameObject tod = new GameObject();
                tod.name = "Slot_d";
                tod.transform.SetParent(to.transform);
                to.transform.SetParent(_actionNodeLineRoot);
                to.transform.position = eventData.currentInputModule.input.mousePosition;
                _dragLine = createLine(from, to, output.GetColor(), output.GetColor());

                onStartConnection?.Invoke(item, outputIndex, true);
            }
        }
        /// <summary>
        /// 拖拽输入接口，使连线一段的位置跟着鼠标移动
        /// </summary>
        /// <param name="item"></param>
        /// <param name="output"></param>
        /// <param name="eventData"></param>
        private void onDragOutputNode(ActionNodeItem item, ActionNodeItemOutput output, BaseEventData eventData)
        {
            if (_isTouchZooming) return;

            setFocused();
            GameObject from = output.getNode();
            GameObject to = new GameObject();
            to.name = "Slot";
            GameObject tod = new GameObject();
            tod.name = "Slot_d";
            tod.transform.SetParent(to.transform);
            to.transform.SetParent(_dragLine.transform);
            to.transform.position = eventData.currentInputModule.input.mousePosition;
            _dragLine.setPosition(from, to);
        }
        /// <summary>
        /// 停止拖拽输入接口，销毁拖拽连线，如果在另外一个输出接口上则触发事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="output"></param>
        /// <param name="eventData"></param>
        private void onEndDragOutputNode(ActionNodeItem item, ActionNodeItemOutput output, BaseEventData eventData)
        {
            int outputIndex = item.indexOfOutput(output);
            if (_dragHiddenConnection)
            {
                _dragHiddenConnection.SetActive(true);
                _dragHiddenConnection = null;
            }
            Destroy(_dragLine.gameObject);
            _dragLine = null;
            //使用射线判断是否在其他的输入点上
            List<RaycastResult> raycastList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData as PointerEventData, raycastList);
            for (int i = 0; i < raycastList.Count; i++)
            {
                ActionNodeItemInput input = raycastList[i].gameObject.GetComponentInParent<ActionNodeItemInput>();
                if (input == null)
                    continue;
                ActionNodeItem inputItem = input.GetComponentInParent<ActionNodeItem>();
                var inputIndex = inputItem.indexOfInput(input);
                if (_connectionList.Exists(c => c.inputNode == inputItem && c.inputIndex == inputIndex && c.outputNode == item && c.outputIndex == outputIndex))
                {
                    // 已连接，断开连接。
                    onUnconnectNodes?.Invoke(item, outputIndex, inputItem, inputIndex);
                    onEndConnection?.Invoke(item, outputIndex, true);
                    return;
                }
                else
                {
                    // 未连接，连接。
                    onConnectNodes?.Invoke(item, outputIndex, inputItem, inputIndex);
                    onEndConnection?.Invoke(item, outputIndex, true);
                    return;
                }
            }
            // 不在任何点上：
            // 如果（该输出端点是控制端点）或者（只有一个连接），尝试断开连接。
            int connectedCount = _connectionList.Count(c => c.outputNode == item && c.outputIndex == outputIndex);
            if (output.isControl || connectedCount == 1)
            {
                //如果目前的连线已经与节点链接，则断开它们的连接。
                int index = _connectionList.FindIndex(c => c.outputNode == item && c.outputIndex == outputIndex);
                if (index >= 0)
                {
                    onUnconnectNodes?.Invoke(item, outputIndex, _connectionList[index].inputNode, _connectionList[index].inputIndex);
                }
            }
            onEndConnection?.Invoke(item, outputIndex, true);

        }
        /// <summary>
        /// 点击输出接口，如果是原本的接口则取消选择，如果在另外一个接口上则触发事件
        /// </summary>
        /// <param name="item"></param>
        /// <param name="input"></param>
        /// <param name="eventData"></param>
        private void onClickOutputNode(ActionNodeItem item, ActionNodeItemOutput output, BaseEventData eventData)
        {
            setFocused();
            int outputIndex = item.indexOfOutput(output);
            if (_currentSelectedKnob.outputNode == item && _currentSelectedKnob.outputIndex == outputIndex)
            {
                // 点击的输出点是本身。
                // 取消选择。
                output?.setSelected(false);
                _currentSelectedKnob = default;
                onEndConnection?.Invoke(item, outputIndex, true);
                return;
            }
            else if (_currentSelectedKnob.inputNode != null && _currentSelectedKnob.inputIndex >= 0)
            {
                // 选择的输入点存在。
                var input = _currentSelectedKnob.inputNode;
                var inputIndex = _currentSelectedKnob.inputIndex;
                bool connected = _connectionList.Any(c => c.inputNode == input && c.inputIndex == inputIndex && c.outputNode == item && c.outputIndex == item.indexOfOutput(output));

                if (connected)
                {
                    // 已经连接，断开连接。
                    onUnconnectNodes?.Invoke(item, outputIndex, input, inputIndex);
                }
                else
                {
                    // 连接输入点与输出点。
                    onConnectNodes?.Invoke(item, outputIndex, input, inputIndex);
                }
                _currentSelectedKnob.inputKnob?.setSelected(false);
                _currentSelectedKnob = default;
                onEndConnection?.Invoke(input, inputIndex, false);
                return;
            }
            else
            {
                // 没有选择输入点，并且输出点也不是本身。
                // 选择新的输出点。
                _currentSelectedKnob.outputKnob?.setSelected(false);
                _currentSelectedKnob.outputNode = item;
                _currentSelectedKnob.outputIndex = outputIndex;
                _currentSelectedKnob.outputKnob = output;
                _currentSelectedKnob.outputKnob?.setSelected(true);
                onStartConnection?.Invoke(item, outputIndex, true);
            }
        }
        private void onPointerDownActionNodeItem(ActionNodeItem item, PointerEventData eventData)
        {
            if (_isTouchZooming) return;
            setFocused();
            _pointerOverActionNodeItem = item;
        }
        private void onPointerUpActionNodeItem(ActionNodeItem item, PointerEventData eventData)
        {
            if (_pointerOverActionNodeItem == item)
            {
                setFocused();
                clearAndSelectNode(item);
            }
        }
        private void onBeginDragActionNodeItem(ActionNodeItem item, PointerEventData eventData)
        {
            if (_isTouchZooming) return;

            setFocused();
            if (_pointerOverActionNodeItem == item)
            {
                _pointerOverActionNodeItem = null;
            }

            _dragItem = item;
            _dragItemPositionOffset = (item.transform as RectTransform).anchoredPosition - (Vector2)_actionNodesRoot.InverseTransformPoint(eventData.position);
            onBeginDragActionNode?.Invoke(item);
        }
        private void onDragActionNodeItem(ActionNodeItem item, PointerEventData eventData)
        {
            if (item != _dragItem)
                return;

            setFocused();
            Vector2 graphPosition = (Vector2)_actionNodesRoot.InverseTransformPoint(eventData.position) + _dragItemPositionOffset;
            if (!isNodeSelected(item))
            {
                clearAndSelectNode(item);
            }
            onMoveActionNode?.Invoke(item, graphPosition);
        }
        private void onEndDragActionNodeItem(ActionNodeItem item, PointerEventData eventData)
        {
            _isDragging = false;
            _selectFrameTrans.gameObject.SetActive(false);
            _dragItem = null;
            _dragItemPositionOffset = Vector3.zero;
            onEndDragActionNode?.Invoke(item);
        }
        private void onActionNodeItemConstValueChanged(ActionNodeItem item, int constValueIndex, object value)
        {
            setFocused();
            onActionNodeValueChange?.Invoke(item, constValueIndex, value);
        }
        private void onClickActionNodeItemConstValueButton(ActionNodeItem item, int constValueIndex)
        {
            setFocused();
            onClickActionNodeConstValueEditButton?.Invoke(item, constValueIndex);
        }
        private void onClickActionNodeItemConstValueItemButton(ActionNodeItem item, int constValueIndex, int itemIndex)
        {
            setFocused();
            onClickActionNodeConstValueItemEditButton?.Invoke(item, constValueIndex, itemIndex);
        }
        private void onClickTriggerNodeAddTargetCheckerButton(ActionNodeItem item)
        {
            setFocused();
            onClickAddTargetCheckerButton?.Invoke(item);
        }
        private void onTargetCheckerTypeValueChangedCallback(ActionNodeItem item, int targetCheckerIndex, int value)
        {
            setFocused();
            onTargetCheckerTypeValueChanged?.Invoke(item, targetCheckerIndex, value);
        }
        private void onClickTriggerNodeRemoveTargetCheckerButton(ActionNodeItem item, int targetCheckerIndex)
        {
            setFocused();
            onClickRemoveTargetCheckerButton?.Invoke(item, targetCheckerIndex);
        }
        private void onTargetCheckerErrorTipValueChangedCallback(ActionNodeItem item, int targetCheckerIndex, string value)
        {
            setFocused();
            onTargetCheckerErrorTipValueChanged?.Invoke(item, targetCheckerIndex, value);
        }
        private void onClickReturnButtonCallback()
        {
            onClickReturnButton?.Invoke();
        }
        private void onClickCancelSelectButtonCallback()
        {
            setFocused();
            onClickCancelSelectButton?.Invoke();
        }
        private void onActionNodeItemUpdateTransform(ActionNodeItem item)
        {
            updateNodeLines(item);
            clearFreePlaceCheckRegion();
        }

        private void onActionNodeClickHelpButton(int key)
        {
            setFocused();
            if (_nodeDocument != null)
                _nodeDocument.Show(key);
        }
        private void onActionNodeVisualEffectEditButtonClick(ActionNodeItem item)
        {
            setFocused();
            onNodeVisualEffectEditButtonClick?.Invoke(item);
        }

        #region 效果属性
        private void onClickAddEffectPropertyButtonCallback()
        {
            setFocused();
            onClickAddEffectPropertyButton?.Invoke();
        }
        private void onConfirmCreateEffectPropertyCallback(EffectPropertyItem item)
        {
            setFocused();
            onConfirmCreateEffectProperty?.Invoke(item);
        }
        private void onClickEffectPropertyItemButtonCallback(string name, int index)
        {
            setFocused();
            onClickEffectPropertyItemButton?.Invoke(name, index);
        }
        private void onEffectPropertyItemValueChangedCallback(string name, int index, object value)
        {
            setFocused();
            onEffectPropertyItemValueChanged?.Invoke(name, index, value);
        }
        private void onClickEffectPropertyAddItemButtonCallback(string name)
        {
            setFocused();
            onClickEffectPropertyAddItemButton?.Invoke(name);
        }
        private void onClickEffectPropertyRemoveItemButtonCallback(string name, int index)
        {
            setFocused();
            onClickEffectPropertyRemoveItemButton?.Invoke(name, index);
        }
        private void onClickEffectPropertyButtonCallback(string name)
        {
            setFocused();
            onClickEffectPropertyButton?.Invoke(name);
        }
        private void onEffectPropertyValueChangedCallback(string name, object value)
        {
            setFocused();
            onEffectPropertyValueChanged?.Invoke(name, value);
        }
        private void onDeleteEffectPropertyCallback(string name)
        {
            setFocused();
            onDeleteEffectProperty?.Invoke(name);
        }
        #endregion

        #endregion
        /// <summary>
        /// 在指定位置创建连线
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private ActionNodeGraphLine createLine(GameObject from, GameObject to)
        {
            ActionNodeGraphLine line = Instantiate(_ActionNodeGraphLineTemplate, _actionNodeLineRoot);
            line.gameObject.SetActive(true);
            line.setPosition(from, to);
            return line;
        }
        /// <summary>
        /// 在指定位置创建连线
        /// </summary>
        /// <param name="from">相对于连线根节点的局部坐标</param>
        /// <param name="to">相对于连线根节点的局部坐标</param>
        /// <returns></returns>
        private ActionNodeGraphLine createLine(GameObject from, GameObject to, Color color, Color targetColor)
        {
            ActionNodeGraphLine line = Instantiate(_ActionNodeGraphLineTemplate, _actionNodeLineRoot);
            line.gameObject.SetActive(true);
            line.setPosition(from, to);
            line.SetColor(color, targetColor);
            return line;
        }
        private void removeConnection(int index)
        {
            Destroy(_connectionList[index].line.gameObject);
            _connectionList.RemoveAt(index);
        }
        /// <summary>
        /// 创建一个不确定类型的节点，必须在之后对其进行初始化。
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        private ActionNodeItem createNode(float posX, float posY)
        {
            ActionNodeItem actionNodeItem = Instantiate(_actionNodeItemTemplate, _actionNodesRoot);
            _actionNodeItemList.Add(actionNodeItem);
            actionNodeItem.gameObject.SetActive(true);
            RectTransform itemRect = actionNodeItem.transform as RectTransform;
            //设置当前节点的位置
            itemRect.anchoredPosition = new Vector2(posX, posY);
            //调整当前背景大小
            actionNodeItem.onBeginDragInputNode += onBeginDragInputNode;
            actionNodeItem.onDragInputNode += onDragInputNode;
            actionNodeItem.onEndDragInputNode += onEndDragInputNode;
            actionNodeItem.onClickInputNode += onClickInputNode;
            actionNodeItem.onBeginDragOutputNode += onBeginDragOutputNode;
            actionNodeItem.onDragOutputNode += onDragOutputNode;
            actionNodeItem.onEndDragOutputNode += onEndDragOutputNode;
            actionNodeItem.onClickOutputNode += onClickOutputNode;
            actionNodeItem.onPointerDown += onPointerDownActionNodeItem;
            actionNodeItem.onPointerUp += onPointerUpActionNodeItem;
            actionNodeItem.onBeginDrag += onBeginDragActionNodeItem;
            actionNodeItem.onDrag += onDragActionNodeItem;
            actionNodeItem.onEndDrag += onEndDragActionNodeItem;
            actionNodeItem.onUpdateTransform += onActionNodeItemUpdateTransform;
            clearFreePlaceCheckRegion();
            return actionNodeItem;
        }
        private Vector3 ClampDesiredScale(Vector3 desiredScale)//clamp函数
        {
            desiredScale = Vector3.Max(_backgroundOriginScale / maxZoom, desiredScale);//最大倍率和最小倍率
            desiredScale = Vector3.Min(_backgroundOriginScale * maxZoom, desiredScale);
            return desiredScale;
        }
        private Vector2 findNearestPosInRect(Vector2 target, Rect range)
        {
            // 大小完全超出范围
            float x = Mathf.Clamp(target.x, range.min.x, range.max.x);
            float y = Mathf.Clamp(target.y, range.min.y, range.max.y);
            return new Vector2(x, y);
        }
        private bool shouldDragSelectionFrame(BaseEventData eventData)
        {
            bool dragSelection = false;
#if UNITY_STANDALONE
            dragSelection = eventData is PointerEventData pointerData && pointerData.button == PointerEventData.InputButton.Left;
#elif UNITY_ANDROID
            dragSelection = _longPressTimer.isExpired();
#endif
            return dragSelection;
        }

        private Region getGraphFreePlaceCheckRegion()
        {
            if (_graphRegion == null)
            {
                updateGraphFreePlaceCheckRegion(new Vector2(600, 400));
            }
            return _graphRegion;
        }
        private Vector2 getGraphFreePlaceCheckRegionOffset()
        {
            if (_graphRegion == null)
            {
                updateGraphFreePlaceCheckRegion(new Vector2(600, 400));
            }
            Vector2 contentScale = _content.localScale;
            Vector2 reverseScale = new Vector2(1 / contentScale.x, 1 / contentScale.y);

            Vector2 viewSize = _graphRegionSize;
            Vector2 viewCenter = Vector2.Scale(-_content.localPosition, reverseScale);
            return 0.5f * viewSize - viewCenter;
        }
        #region 节点选择
        private void updateSelectFrame()
        {
            _selectFrameTrans.gameObject.SetActive(true);
            var startPos = _selectFrameDragStartPosition + (Vector2)_content.position;
            var endPos = (Vector2)Input.mousePosition;

            var minPos = Vector2.Min(startPos, endPos);
            var maxPos = Vector2.Max(startPos, endPos);

            var minContentPos = _content.InverseTransformPoint(minPos);
            var maxContentPos = _content.InverseTransformPoint(maxPos);
            var rect = new Rect(minContentPos, maxContentPos - minContentPos);

            var minFramePos = _selectFrameTrans.parent.InverseTransformPoint(minPos);
            var maxFramePos = _selectFrameTrans.parent.InverseTransformPoint(maxPos);
            _selectFrameTrans.localPosition = minFramePos;
            _selectFrameTrans.sizeDelta = maxFramePos - minFramePos;

            clearSelection();
            foreach (var item in _actionNodeItemList)
            {
                var itemRectTrans = item.transform as RectTransform;
                if (!itemRectTrans)
                {
                    continue;
                }
                var size = itemRectTrans.sizeDelta;
                var pos = itemRectTrans.anchoredPosition - Vector2.Scale(size, itemRectTrans.pivot);
                var itemRect = new Rect(pos, size);
                if (itemRect.Overlaps(rect))
                {
                    selectNode(item);
                }
            }
        }
        #endregion
        #endregion
        #region 事件
        /// <summary>
        /// 当连接节点，参数为输出节点和索引以及输入节点和索引
        /// </summary>
        public event Action<ActionNodeItem, int, ActionNodeItem, int> onConnectNodes;
        /// <summary>
        /// 当断开节点之间的链接，参数为输出节点和索引以及输入节点和索引
        /// </summary>
        public event Action<ActionNodeItem, int, ActionNodeItem, int> onUnconnectNodes;
        /// <summary>
        /// 当开始拖拽移动节点，参数为拖拽的节点
        /// </summary>
        public event Action<ActionNodeItem> onBeginDragActionNode;
        /// <summary>
        /// 当移动节点，参数为拖拽的节点和图坐标位置
        /// </summary>
        public event Action<ActionNodeItem, Vector2> onMoveActionNode;
        /// <summary>
        /// 当结束拖拽移动节点，参数为拖拽的节点
        /// </summary>
        public event Action<ActionNodeItem> onEndDragActionNode;
        /// <summary>
        /// 当开始拖拽端口连线
        /// 参数：节点UI，端口索引，是否是输出值
        /// </summary>
        public event Action<ActionNodeItem, int, bool> onStartConnection;
        /// <summary>
        /// 当结束拖拽端口连线
        /// 参数：节点UI，端口索引，是否是输出值
        /// </summary>
        public event Action<ActionNodeItem, int, bool> onEndConnection;
        /// <summary>
        /// 当节点的常量值输入改变，参数为被改变的节点，常量值索引和改变的值
        /// </summary>
        public event Action<ActionNodeItem, int, object> onActionNodeValueChange;
        /// <summary>
        /// 当节点的常量的编辑按钮被点击，参数为被点击的节点和常量索引
        /// </summary>
        public event Action<ActionNodeItem, int> onClickActionNodeConstValueEditButton;
        /// <summary>
        /// 当点击节点的常量的子项的编辑按钮，参数为点击的节点，常量索引和常量子项的索引
        /// </summary>
        public event Action<ActionNodeItem, int, int> onClickActionNodeConstValueItemEditButton;
        /// <summary>
        /// 当触发器节点的事件名称改变，参数为更改后的事件名称的索引ID
        /// </summary>
        public event Action<ActionNodeItem, int> onTriggerEventTypeChanged;
        /// <summary>
        /// 当点击触发器节点的添加目标按钮
        /// </summary>
        public event Action<ActionNodeItem> onClickAddTargetCheckerButton;
        /// <summary>
        /// 当触发器节点的某个目标类型发生改变的时候，参数为发生改变的目标索引和目标值的索引
        /// </summary>
        public event Action<ActionNodeItem, int, int> onTargetCheckerTypeValueChanged;
        /// <summary>
        /// 当点击触发器节点的删除目标按钮，参数为点击的按钮的索引
        /// </summary>
        public event Action<ActionNodeItem, int> onClickRemoveTargetCheckerButton;
        /// <summary>
        /// 当触发器节点的某个目标错误提示发生改变，参数为发生改变的目标索引和错误提示的字符串
        /// </summary>
        public event Action<ActionNodeItem, int, string> onTargetCheckerErrorTipValueChanged;
        /// <summary>
        /// 当点击返回之前编辑的动作图按钮
        /// </summary>
        public event Action onClickReturnButton;
        /// <summary>
        /// 当点击取消选择卡牌按钮
        /// </summary>
        public event Action onClickCancelSelectButton;
        /// <summary>
        /// 当选择节点
        /// </summary>
        public event Action<ActionNodeItem> onNodeSelected;
        /// <summary>
        /// 当取消选择节点
        /// </summary>
        public event Action<ActionNodeItem> onNodeDeselected;
        public event Action<ActionNodeItem> onNodeVisualEffectEditButtonClick;


        #region 效果属性
        /// <summary>
        /// 当确认创建自定义属性。
        /// </summary>
        public event Action onClickAddEffectPropertyButton;
        public event Action<EffectPropertyItem> onConfirmCreateEffectProperty;
        public event Action<string, int> onClickEffectPropertyItemButton;
        public event Action<string, int, object> onEffectPropertyItemValueChanged;
        public event Action<string> onClickEffectPropertyButton;
        public event Action<string, object> onEffectPropertyValueChanged;
        public event Action<string> onDeleteEffectProperty;
        public event Action<string> onClickEffectPropertyAddItemButton;
        public event Action<string, int> onClickEffectPropertyRemoveItemButton;
        #endregion
        #endregion
        #region 属性字段
        /// <summary>
        /// 自动生成位置的宽度间隔
        /// </summary>
        public float autoPosSpacingWidth => _autoPosSpacingWidth;
        /// <summary>
        /// 自动生成位置的高度间隔
        /// </summary>
        public float autoPosSpacingHeight => _autoPosSpacingHeight;
        public bool IsDragging => _isDragging;

        #region 多选/操作模式
        HashSet<ActionNodeItem> _selectedNodes = new HashSet<ActionNodeItem>();
        [SerializeField]
        private Vector2 _selectFrameDragStartPosition;
        #endregion

        [Header("模板属性和UI控件")]
        [SerializeField]
        float _autoPosSpacingWidth = 600;
        [SerializeField]
        float _autoPosSpacingHeight = 300;
        [SerializeField]
        Button _returnButton;
        [SerializeField]
        Image _backgroundImage;
        [SerializeField]
        EventTriggerWithoutUseEvent _backgroundEventTrigger;
        [SerializeField]
        RectTransform _content;
        [SerializeField]
        RectTransform _selectFrameTrans;
        [SerializeField]
        RectTransform _viewPort;
        [SerializeField]
        CreateEffectPropertyDialog _createEffectPropDialog;

        [Header("缩放")]
        [SerializeField]
        private float zoomSpeed = 0.1f;
        [SerializeField]
        private float maxZoom = 10f;
        [SerializeField]
        float _maxScale = 1.7f;
        [SerializeField]
        float _minScale = 0.3f;

        [Header("外部引用")]
        [SerializeField]
        CreateActionDefineDialog _createActionDefineDialog;
        [SerializeField]
        Text _hintMessageText;
        [SerializeField]
        GameObject _cardSelectPanel;
        [SerializeField]
        Button _cancelSelectButton;

        [Header("节点")]
        [SerializeField]
        ActionNodeItem _actionNodeItemTemplate;
        [SerializeField]
        RectTransform _actionNodesRoot;
        [SerializeField]
        List<ActionNodeItem> _actionNodeItemList = new List<ActionNodeItem>();
        [SerializeField]
        NodeDocument _nodeDocument;

        [Header("连接")]
        [SerializeField]
        ActionNodeGraphLine _ActionNodeGraphLineTemplate;
        [SerializeField]
        RectTransform _actionNodeLineRoot;
        [SerializeField]
        List<ActionNodeConnectionData> _connectionList = new List<ActionNodeConnectionData>();



        [Header("杂项字段")]
        [SerializeField]
        ActionNodeItem _pointerOverActionNodeItem;
        [SerializeField]
        ActionNodeItem _dragItem;
        [SerializeField]
        private Vector2 _dragItemPositionOffset;
        [SerializeField]
        ActionNodeGraphLine _dragLine;
        // ActionNodeGraphLine _dragLine;
        [SerializeField]
        private Vector3 _backgroundOriginScale;
        [SerializeField]
        private Vector2 _backgroundDragStartPosition;
        private bool _isDragging;
        private BJSYGameCore.Timer _longPressTimer = new BJSYGameCore.Timer() { duration = .5f };
        private Touch _perviousTouch1;
        private Touch _perviousTouch2;
        private bool _isTouchZooming;
        private ActionNodeSelectedKnob _currentSelectedKnob;
        /// <summary>
        /// 因为拖拽而被隐藏的连接。
        /// </summary>
        private GameObject _dragHiddenConnection;
        /// <summary>
        /// 用于检测空闲节点区域的对象。
        /// </summary>
        private Region _graphRegion;
        /// <summary>
        /// 用于检测空闲节点区域的对象的大小。
        /// </summary>
        private Vector2 _graphRegionSize;
        /// <summary>
        /// 被设置为焦点，会在下一次Update中将EventSystem的选择物体设置为自身。
        /// </summary>
        private bool _focused;
        #endregion
    }
    public struct ActionNodeValueInfo
    {
        public ActionNodeValueInfo(string name, ActionNodeValueType type, object value) : this(name, name, type, value, null, null)
        {
        }
        public ActionNodeValueInfo(string displayName, string name, ActionNodeValueType type, object value) : this(displayName, name, type, value, null, null)
        {
        }
        public ActionNodeValueInfo(string name, ActionNodeValueType type, object value, Type enumType)
        {
            displayName = name;
            this.name = name;
            this.type = type;
            this.value = value;
            optionalValues = Enum.GetValues(enumType).Cast<object>().ToArray();
            optionalNames = new string[optionalValues.Length];
            for (int i = 0; i < optionalNames.Length; i++)
            {
                optionalNames[i] = Enum.GetName(enumType, optionalValues[i]);
            }
            isArray = false;
        }
        public ActionNodeValueInfo(string name, ActionNodeValueType type, object value, string[] optionalNames, object[] optionalValues)
        {
            displayName = name;
            this.name = name;
            this.type = type;
            this.value = value;
            this.optionalNames = optionalNames;
            this.optionalValues = optionalValues;
            isArray = false;
        }
        public ActionNodeValueInfo(string displayName, string name, ActionNodeValueType type, object value, string[] optionalNames, object[] optionalValues)
        {
            this.displayName = displayName;
            this.name = name;
            this.type = type;
            this.value = value;
            this.optionalNames = optionalNames;
            this.optionalValues = optionalValues;
            isArray = false;
        }
        public string displayName;
        public string name;
        public ActionNodeValueType type;
        public object value;
        public string[] optionalNames;
        public object[] optionalValues;
        public bool isArray;
    }
    [Serializable]
    struct ActionNodeConnectionData
    {
        public ActionNodeItem outputNode;
        public int outputIndex;
        public ActionNodeItem inputNode;
        public int inputIndex;
        public ActionNodeGraphLine line;
        //public ActionNodeGraphLine line;        
    }
    struct ActionNodeSelectedKnob
    {
        public ActionNodeItem outputNode;
        public ActionNodeItemOutput outputKnob;
        public int outputIndex;
        public ActionNodeItem inputNode;
        public ActionNodeItemInput inputKnob;
        public int inputIndex;
    }
    public enum ActionNodeValueType
    {
        /// <summary>
        /// 整数
        /// </summary>
        integer,
        /// <summary>
        /// 实数
        /// </summary>
        real,
        /// <summary>
        /// 真值
        /// </summary>
        boolean,
        /// <summary>
        /// 字符串
        /// </summary>
        text,
        /// <summary>
        /// 枚举值
        /// </summary>
        enumValue,
        /// <summary>
        /// 牌堆名称集
        /// </summary>
        pileNameCollection,
        /// <summary>
        /// 标签集合
        /// </summary>
        effectTagCollection,
        /// <summary>
        /// 动作节点
        /// </summary>
        actionNode,
        /// <summary>
        /// 动作值引用
        /// </summary>
        actionValueRef,
        /// <summary>
        /// 触发器
        /// </summary>
        trigger,
        /// <summary>
        /// 触发器集合
        /// </summary>
        triggerCollection,
        /// <summary>
        /// 卡牌引用
        /// </summary>
        cardReference,
        /// <summary>
        /// 卡牌引用集合
        /// </summary>
        cardReferenceCollection,
        /// <summary>
        /// 关键词
        /// </summary>
        keywords,
        /// <summary>
        /// 单个效果标签
        /// </summary>
        effectTag,
        /// <summary>
        /// 事件引用
        /// </summary>
        eventReference,
        /// <summary>
        /// 卡牌属性（可设置）
        /// </summary>
        cardPropertyNameSet,
        /// <summary>
        /// 卡牌属性（可读取）
        /// </summary>
        cardPropertyNameGet
    }
    public enum ActionNodeConnectionType
    {
        action,
        value
    }

}