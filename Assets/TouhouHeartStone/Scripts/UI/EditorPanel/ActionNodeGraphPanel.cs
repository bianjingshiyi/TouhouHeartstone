using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
using TouhouCardEngine;
using TouhouHeartstone;
using System.Reflection;
using TouhouHeartstone.Builtin;
using System;
using System.IO.Compression;
using System.IO;
using MukioI18n;
using NitoriNetwork.Common;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using BJSYGameCore.UI;
using UnityEngine.UI;
using BJSYGameCore;
using Timer = BJSYGameCore.Timer;
namespace UI
{
    public partial class ActionNodeGraphPanel : MonoBehaviour
    {
        #region 公有方法
        /// <summary>
        /// 在指定位置上创建一个具有指定名称，输入，常量值和输出的节点。
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="outputNames"></param>
        /// <returns></returns>
        public ActionNodeItem createNode(float posX, float posY, string nodeName, string[] inputNames, ActionNodeValueInfo[] constsInfo, string[] outputNames)
        {
            //当对应名称数组长度为0的时候，直接隐藏列表根节点。
            ActionNodeItem actionNodeItem = createNode(posX, posY);
            actionNodeItem.onConstValueChanged += onActionNodeItemConstValueChanged;
            actionNodeItem.onClickConstValueButton += onClickActionNodeItemConstValueButton;
            // actionNodeItem.onEventNameChanged += onTriggerNodeEventNameChanged;
            actionNodeItem.onEventTypeDropdownValueChanged += onTriggerNodeEventIndexChanged;
            actionNodeItem.onClickEditConditionButton += onClickTriggerNodeEditConditionButton;
            actionNodeItem.onClickAddTargetCheckerButton += onClickTriggerNodeAddTargetCheckerButton;
            actionNodeItem.onClickEditTargetCheckerButton += onClickTriggerNodeEditTargetCheckerButton;
            actionNodeItem.onClickRemoveTargetCheckerButton += onClickTriggerNodeRemoveTargetCheckerButton;
            actionNodeItem.onTargetCheckerTypeValueChanged += onTargetCheckerTypeValueChangedCallback;
            actionNodeItem.onTargetCheckerErrorTipValueChanged += onTargetCheckerErrorTipValueChangedCallback;
            actionNodeItem.switchToActionNode();
            actionNodeItem.setName(nodeName);
            actionNodeItem.setInputs(inputNames);
            actionNodeItem.setConsts(constsInfo);
            actionNodeItem.setOutputs(outputNames);
            return actionNodeItem;
        }
        /// <summary>
        /// 在指定位置上创建一个触发器节点。
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="eventName"></param>
        /// <param name="targetCheckerCount"></param>
        /// <returns></returns>
        public ActionNodeItem createTriggerNode(float posX, float posY, string[] targetCheckerTypes, int eventIndex, (int typeIndex, string name, string errorTip)[] targetCheckers)
        {
            ActionNodeItem actionNodeItem = Instantiate(_actionNodeItemTemplate, _actionNodesRoot);
            _actionNodeItemList.Add(actionNodeItem);
            actionNodeItem.gameObject.SetActive(true);
            RectTransform itemRect = actionNodeItem.transform as RectTransform;
            itemRect.anchoredPosition = new Vector2(posX, posY);
            actionNodeItem.switchToTriggerNode();
            actionNodeItem.setTrigger(targetCheckerTypes, eventIndex, targetCheckers);
            string[] outputNames = new string[]
            {
                string.Empty
            };
            //onSelectedEventIndex
            actionNodeItem.setOutputs(outputNames);

            actionNodeItem.onBeginDragInputNode += onBeginDragInputNode;
            actionNodeItem.onDragInputNode += onDragInputNode;
            actionNodeItem.onEndDragInputNode += onEndDragInputNode;
            actionNodeItem.onBeginDragOutputNode += onBeginDragOutputNode;
            actionNodeItem.onDragOutputNode += onDragOutputNode;
            actionNodeItem.onEndDragOutputNode += onEndDragOutputNode;
            actionNodeItem.onPointerDown += onPointerDownActionNodeItem;
            actionNodeItem.onPointerUp += onPointerUpActionNodeItem;
            actionNodeItem.onBeginDrag += onBeginDragActionNodeItem;
            actionNodeItem.onDrag += onDragActionNodeItem;
            actionNodeItem.onEndDrag += onEndDragActionNodeItem;
            actionNodeItem.onConstValueChanged += onActionNodeItemConstValueChanged;
            actionNodeItem.onClickConstValueButton += onClickActionNodeItemConstValueButton;
            // actionNodeItem.onEventNameChanged += onTriggerNodeEventNameChanged;
            actionNodeItem.onEventTypeDropdownValueChanged += onTriggerNodeEventIndexChanged;
            actionNodeItem.onClickEditConditionButton += onClickTriggerNodeEditConditionButton;
            actionNodeItem.onClickAddTargetCheckerButton += onClickTriggerNodeAddTargetCheckerButton;
            actionNodeItem.onClickEditTargetCheckerButton += onClickTriggerNodeEditTargetCheckerButton;
            actionNodeItem.onClickRemoveTargetCheckerButton += onClickTriggerNodeRemoveTargetCheckerButton;
            actionNodeItem.onTargetCheckerTypeValueChanged += onTargetCheckerTypeValueChangedCallback;
            actionNodeItem.onTargetCheckerErrorTipValueChanged += onTargetCheckerErrorTipValueChangedCallback;
            return actionNodeItem;
        }
        /// <summary>
        /// 在指定位置上创建一个触发器节点
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="eventTypeIndex"></param>
        /// <param name="targetCheckers"></param>
        /// <returns></returns>
        public ActionNodeItem createTriggerNode(float posX, float posY, int eventTypeIndex, (int typeIndex, string name, string errorTip)[] targetCheckers)
        {
            throw new NotImplementedException();
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
            Vector2 from = node.getOutputWorldPosition(outputIndex);
            from = _actionNodeLineRoot.InverseTransformPoint(from);
            Vector2 to = targetNode.getInputWorldPosition(inputIndex);
            to = _actionNodeLineRoot.InverseTransformPoint(to);
            ActionNodeGraphLine line = createLine(from, to);
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
            Destroy(_connectionList[index].line.gameObject);
            _connectionList.RemoveAt(index);
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
                    Destroy(_connectionList[i].line.gameObject);
                    _connectionList.RemoveAt(i);
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
            //设置节点位置
            item.transform.localPosition = graphPosition + _dragItemPositionOffset;
            //设置背景大小
            //刷新连线位置
            for (int i = 0; i < _connectionList.Count; i++)
            {
                if (_connectionList[i].outputNode == _dragItem || _connectionList[i].inputNode == _dragItem)
                {
                    Vector2 from = _connectionList[i].outputNode.getOutputWorldPosition(_connectionList[i].outputIndex);
                    from = _actionNodesRoot.InverseTransformPoint(from);
                    Vector2 to = _connectionList[i].inputNode.getInputWorldPosition(_connectionList[i].inputIndex);
                    to = _actionNodesRoot.InverseTransformPoint(to);
                    _connectionList[i].line.setPosition(from, to);
                }
            }
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
                    Destroy(_connectionList[i].line.gameObject);
                    _connectionList.RemoveAt(i);
                    i--;
                    continue;
                }
            }
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
        }
        /// <summary>
        /// 在指定坐标展开菜单，左上角对齐。
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="options"></param>
        public void openMenu(float posX, float posY, string[] options)
        {
            _menuRoot.gameObject.SetActive(true);
            _menuRoot.anchoredPosition = new Vector2(posX, posY);
            updateMenu(options);
        }
        public void updateMenu(string[] options)
        {
            UIHelper.updateList(_Content, _menuItemTemplate, _menuItemList, options.Length, (i, itemRect) =>
            {
                ActionNodeGraphMenuOption item = itemRect.GetComponent<ActionNodeGraphMenuOption>();
                item.setTitle(options[i]);
            }, itemRect =>
            {
                ActionNodeGraphMenuOption item = itemRect.GetComponent<ActionNodeGraphMenuOption>();
                item.onClick += onClickMenuOption;
            }, itemRect =>
            {
                ActionNodeGraphMenuOption item = itemRect.GetComponent<ActionNodeGraphMenuOption>();
                item.onClick -= onClickMenuOption;
            });
        }
        /// <summary>
        /// 关闭菜单。
        /// </summary>
        public void closeMenu()
        {
            _menuRoot.gameObject.SetActive(false);
        }
        /// <summary>
        /// 向玩家显示一条错误提示
        /// </summary>
        /// <param name="msg"></param>
        public void showErrorTip(string msg)
        {
            //TODO:使用通用提示框实现
            Debug.LogError(msg);
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
        public void init(string[] languages, string[] eventTypes)
        {
            _actionNodeItemTemplate.initBuffLanguages(languages);
            _actionNodeItemTemplate.initEventTypes(eventTypes);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            EventTrigger trigger = _backgroundImage.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener(onPointerDownBackgroundImage);
            trigger.triggers.Add(entry);
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener(onPointerUpBackgroundImage);
            trigger.triggers.Add(entry);
            if (_actionNodeItemTemplate.transform.parent == _actionNodesRoot)
            {
                _actionNodeItemTemplate.gameObject.SetActive(false);
            }
            if (_actionNodeLineTemplate.transform.parent == _actionNodeLineRoot)
            {
                _actionNodeLineTemplate.gameObject.SetActive(false);
            }
            if (_menuItemTemplate.transform.parent == _menuRoot)
            {
                _menuItemTemplate.gameObject.SetActive(false);
            }
            _menuRoot.gameObject.SetActive(false);
            _returnButton.onClick.AddListener(onClickReturnButtonCallback);
        }
        protected void Update()
        {
            //更新线段位置
            foreach (ActionNodeConnectionData connection in _connectionList)
            {
                Vector2 from = connection.outputNode.getOutputWorldPosition(connection.outputIndex);
                from = _actionNodesRoot.InverseTransformPoint(from);
                Vector2 to = connection.inputNode.getInputWorldPosition(connection.inputIndex);
                to = _actionNodesRoot.InverseTransformPoint(to);
                connection.line.setPosition(from, to);
            }
        }
        #endregion
        #region 事件回调
        private void onPointerDownBackgroundImage(BaseEventData eventData)
        {
            _longPressTimer.start();
        }
        private void onPointerUpBackgroundImage(BaseEventData eventData)
        {
            Vector2 screenPosition = eventData.currentInputModule.input.mousePosition;
            Vector2 localPoistion = _backgroundImage.transform.InverseTransformPoint(screenPosition);
            if (_longPressTimer.isExpired())
            {
                onLongPressBackground?.Invoke(localPoistion.x, localPoistion.y);
            }
            else
            {
                if ((eventData as PointerEventData).button == PointerEventData.InputButton.Right)
                {
                    onRightClickBackground?.Invoke(localPoistion.x, localPoistion.y);
                }
                else
                {
                    onLeftClickBackground?.Invoke(localPoistion.x, localPoistion.y);
                }
            }
        }
        /// <summary>
        /// 开始拖拽输入接口，创建一根跟着鼠标移动的连线
        /// </summary>
        /// <param name="item"></param>
        /// <param name="input"></param>
        /// <param name="eventData"></param>
        private void onBeginDragInputNode(ActionNodeItem item, ActionNodeItemInput input, BaseEventData eventData)
        {
            int index = _connectionList.FindIndex(c => c.inputNode == item && c.inputIndex == item.indexOfInput(input));
            if (index < 0)
            {
                Vector2 from = input.getNodeWorldPosition();
                from = _actionNodeLineRoot.transform.InverseTransformPoint(from);
                Vector2 to = eventData.currentInputModule.input.mousePosition;
                to = _actionNodeLineRoot.transform.InverseTransformPoint(to);
                _dragLine = createLine(from, to);
            }
            else
            {
                _dragLine = _connectionList[index].line;
            }
        }
        /// <summary>
        /// 拖拽输入接口，使连线一段的位置跟着鼠标移动
        /// </summary>
        /// <param name="item"></param>
        /// <param name="input"></param>
        /// <param name="eventData"></param>
        private void onDragInputNode(ActionNodeItem item, ActionNodeItemInput input, BaseEventData eventData)
        {
            Vector2 from = input.getNodeWorldPosition();
            from = _actionNodeLineRoot.transform.InverseTransformPoint(from);
            Vector2 to = eventData.currentInputModule.input.mousePosition;
            to = _actionNodeLineRoot.transform.InverseTransformPoint(to);
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
            int index = _connectionList.FindIndex(c => c.inputNode == item && c.inputIndex == item.indexOfInput(input));
            if (index < 0)
            {
                Destroy(_dragLine.gameObject);
                _dragLine = null;
            }
            else
            {
                _dragLine = null;
            }
            //使用射线判断是否在其他的输入点上
            List<RaycastResult> raycastList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData as PointerEventData, raycastList);
            for (int i = 0; i < raycastList.Count; i++)
            {
                ActionNodeItemOutput output = raycastList[i].gameObject.GetComponentInParent<ActionNodeItemOutput>();
                if (output == null)
                    continue;
                ActionNodeItem outputItem = output.GetComponentInParent<ActionNodeItem>();
                onConnectNodes?.Invoke(outputItem, outputItem.indexOfOutput(output), item, item.indexOfInput(input));
                return;
            }
            //不在任何点上，如果目前的连线已经与节点链接，则断开它们的连接
            if (index < 0)
                return;
            onUnconnectNodes?.Invoke(_connectionList[index].outputNode, _connectionList[index].outputIndex,
                _connectionList[index].inputNode, _connectionList[index].inputIndex);
        }
        /// <summary>
        /// 开始拖拽输入接口，创建一根跟着鼠标移动的连线
        /// </summary>
        /// <param name="item"></param>
        /// <param name="output"></param>
        /// <param name="eventData"></param>
        private void onBeginDragOutputNode(ActionNodeItem item, ActionNodeItemOutput output, BaseEventData eventData)
        {
            int index = _connectionList.FindIndex(c => c.outputNode == item && c.outputIndex == item.indexOfOutput(output));
            if (index < 0)
            {
                Vector2 from = output.getNodeWorldPosition();
                from = _actionNodeLineRoot.transform.InverseTransformPoint(from);
                Vector2 to = eventData.currentInputModule.input.mousePosition;
                to = _actionNodeLineRoot.transform.InverseTransformPoint(to);
                _dragLine = createLine(from, to);
            }
            else
            {
                _dragLine = _connectionList[index].line;
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
            Vector2 from = output.getNodeWorldPosition();
            from = _actionNodeLineRoot.transform.InverseTransformPoint(from);
            Vector2 to = eventData.currentInputModule.input.mousePosition;
            to = _actionNodeLineRoot.transform.InverseTransformPoint(to);
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
            int index = _connectionList.FindIndex(c => c.outputNode == item && c.outputIndex == item.indexOfOutput(output));
            if (index < 0)
            {
                Destroy(_dragLine.gameObject);
                _dragLine = null;
            }
            else
            {
                _dragLine = null;
            }
            //使用射线判断是否在其他的输入点上
            List<RaycastResult> raycastList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData as PointerEventData, raycastList);
            for (int i = 0; i < raycastList.Count; i++)
            {
                ActionNodeItemInput input = raycastList[i].gameObject.GetComponentInParent<ActionNodeItemInput>();
                if (input == null)
                    continue;
                ActionNodeItem inputItem = input.GetComponentInParent<ActionNodeItem>();
                onConnectNodes?.Invoke(item, item.indexOfOutput(output), inputItem, inputItem.indexOfInput(input));
                return;
            }
            //不在任何点上，如果目前的连线已经与节点链接，则断开它们的连接
            if (index < 0)
                return;
            onUnconnectNodes?.Invoke(_connectionList[index].outputNode, _connectionList[index].outputIndex,
                _connectionList[index].inputNode, _connectionList[index].inputIndex);
        }
        private void onPointerDownActionNodeItem(ActionNodeItem item, PointerEventData eventData)
        {
            _pointerOverActionNodeItem = item;
            _longPressTimer.start();
        }
        private void onPointerUpActionNodeItem(ActionNodeItem item, PointerEventData eventData)
        {
            if (_pointerOverActionNodeItem == item)
            {
                if (_longPressTimer.isExpired())
                {
                    onLongPressActionNode?.Invoke(_pointerOverActionNodeItem, eventData.position);
                    _pointerOverActionNodeItem = null;
                    _longPressTimer.reset();
                }
                else
                {
                    onClickActionNode?.Invoke(item);
                }
            }
        }
        private void onBeginDragActionNodeItem(ActionNodeItem item, PointerEventData eventData)
        {
            if (_pointerOverActionNodeItem == item)
            {
                _pointerOverActionNodeItem = null;
                _longPressTimer.reset();
            }
            _dragItem = item;
            _dragItemPositionOffset = item.transform.localPosition - _actionNodesRoot.InverseTransformPoint(eventData.position);
        }
        private void onDragActionNodeItem(ActionNodeItem item, PointerEventData eventData)
        {
            if (item != _dragItem)
                return;
            Vector2 graphPosition = _actionNodesRoot.InverseTransformPoint(eventData.position);
            onDragActionNode?.Invoke(item, graphPosition);
        }
        private void onEndDragActionNodeItem(ActionNodeItem item, PointerEventData eventData)
        {
            _dragItem = null;
            _dragItemPositionOffset = Vector3.zero;
        }
        private void onActionNodeItemConstValueChanged(ActionNodeItem item, int constValueIndex, object value)
        {
            onActionNodeValueChange?.Invoke(item, constValueIndex, value);
        }
        private void onClickActionNodeItemConstValueButton(ActionNodeItem item, int constValueIndex)
        {
            onClickActionNodeConstValueEditButton?.Invoke(item, constValueIndex);
        }
        private void onTriggerNodeEventNameChanged(ActionNodeItem item, string eventName)
        {
            onChangeTriggerEventName?.Invoke(eventName);
        }

        private void onTriggerNodeEventIndexChanged(ActionNodeItem item, int eventIndex)
        {
            onTriggerEventTypeChanged?.Invoke(eventIndex);
        }
        private void onClickTriggerNodeEditConditionButton(ActionNodeItem item)
        {
            onClickTriggerEditConditionButton?.Invoke();
        }
        private void onClickTriggerNodeAddTargetCheckerButton(ActionNodeItem item)
        {
            onClickAddTargetCheckerButton?.Invoke();
        }
        private void onTargetCheckerTypeValueChangedCallback(ActionNodeItem item, int targetCheckerIndex, int value)
        {
            onTargetCheckerTypeValueChanged?.Invoke(targetCheckerIndex, value);
        }
        private void onClickTriggerNodeEditTargetCheckerButton(ActionNodeItem item, int targetCheckerIndex)
        {
            onClickEditTargetCheckerButton?.Invoke(targetCheckerIndex);
        }
        private void onClickTriggerNodeRemoveTargetCheckerButton(ActionNodeItem item, int targetCheckerIndex)
        {
            onClickRemoveTargetCheckerButton?.Invoke(targetCheckerIndex);
        }
        private void onTargetCheckerErrorTipValueChangedCallback(ActionNodeItem item, int targetCheckerIndex, string value)
        {
            onTargetCheckerErrorTipValueChanged?.Invoke(targetCheckerIndex, value);
        }
        private void onClickMenuOption(ActionNodeGraphMenuOption item)
        {
            onClickMenuItem?.Invoke(_menuItemList.IndexOf(item.transform as RectTransform));
        }
        private void onClickReturnButtonCallback()
        {
            onClickReturnButton?.Invoke();
        }
        #endregion
        /// <summary>
        /// 在指定位置创建连线
        /// </summary>
        /// <param name="from">相对于连线根节点的局部坐标</param>
        /// <param name="to">相对于连线根节点的局部坐标</param>
        /// <returns></returns>
        private ActionNodeGraphLine createLine(Vector2 from, Vector2 to)
        {
            ActionNodeGraphLine line = Instantiate(_actionNodeLineTemplate, _actionNodeLineRoot);
            line.gameObject.SetActive(true);
            line.setPosition(from, to);
            return line;
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
            actionNodeItem.onBeginDragOutputNode += onBeginDragOutputNode;
            actionNodeItem.onDragOutputNode += onDragOutputNode;
            actionNodeItem.onEndDragOutputNode += onEndDragOutputNode;
            actionNodeItem.onPointerDown += onPointerDownActionNodeItem;
            actionNodeItem.onPointerUp += onPointerUpActionNodeItem;
            actionNodeItem.onBeginDrag += onBeginDragActionNodeItem;
            actionNodeItem.onDrag += onDragActionNodeItem;
            actionNodeItem.onEndDrag += onEndDragActionNodeItem;
            return actionNodeItem;
        }
        #endregion
        #region 事件
        /// <summary>
        /// 当左击背景，参数为点击的x坐标与y坐标。
        /// </summary>
        public event Action<float, float> onLeftClickBackground;
        /// <summary>
        /// 当右击背景，参数为点击的x坐标与y坐标。
        /// </summary>
        public event Action<float, float> onRightClickBackground;
        /// <summary>
        /// 当长按背景，参数为长按点的x坐标与y坐标
        /// </summary>
        public event Action<float, float> onLongPressBackground;
        /// <summary>
        /// 当点击菜单项，参数为菜单项索引
        /// </summary>
        public event Action<int> onClickMenuItem;
        /// <summary>
        /// 当连接节点，参数为输出节点和索引以及输入节点和索引
        /// </summary>
        public event Action<ActionNodeItem, int, ActionNodeItem, int> onConnectNodes;
        /// <summary>
        /// 当断开节点之间的链接，参数为输出节点和索引以及输入节点和索引
        /// </summary>
        public event Action<ActionNodeItem, int, ActionNodeItem, int> onUnconnectNodes;
        /// <summary>
        /// 当点击节点，参数为点击的节点
        /// </summary>
        public event Action<ActionNodeItem> onClickActionNode;
        /// <summary>
        /// 当拖拽节点，参数为拖拽的节点和图坐标位置
        /// </summary>
        public event Action<ActionNodeItem, Vector2> onDragActionNode;
        /// <summary>
        /// 当长按节点，参数为长按的节点和长按的屏幕坐标
        /// </summary>
        public event Action<ActionNodeItem, Vector2> onLongPressActionNode;
        /// <summary>
        /// 当节点的常量值输入改变，参数为被改变的节点，常量值索引和改变的值
        /// </summary>
        public event Action<ActionNodeItem, int, object> onActionNodeValueChange;
        /// <summary>
        /// 当节点的常量的编辑按钮被点击，参数为被点击的节点和常量索引
        /// </summary>
        public event Action<ActionNodeItem, int> onClickActionNodeConstValueEditButton;
        /// <summary>
        /// 当触发器节点的事件名称改变，参数为更改后的事件名称
        /// </summary>
        public event Action<string> onChangeTriggerEventName;
        /// <summary>
        /// 当触发器节点的事件名称改变，参数为更改后的事件名称的索引ID
        /// </summary>
        public event Action<int> onTriggerEventTypeChanged;
        /// <summary>
        /// 当点击触发器节点的编辑条件按钮
        /// </summary>
        public event Action onClickTriggerEditConditionButton;
        /// <summary>
        /// 当点击触发器节点的添加目标按钮
        /// </summary>
        public event Action onClickAddTargetCheckerButton;
        /// <summary>
        /// 当触发器节点的某个目标类型发生改变的时候，参数为发生改变的目标索引和目标值的索引
        /// </summary>
        public event Action<int, int> onTargetCheckerTypeValueChanged;
        /// <summary>
        /// 当点击触发器节点的编辑目标按钮，参数为点击的按钮的索引
        /// </summary>
        public event Action<int> onClickEditTargetCheckerButton;
        /// <summary>
        /// 当点击触发器节点的删除目标按钮，参数为点击的按钮的索引
        /// </summary>
        public event Action<int> onClickRemoveTargetCheckerButton;
        /// <summary>
        /// 当触发器节点的某个目标错误提示发生改变，参数为发生改变的目标索引和错误提示的字符串
        /// </summary>
        public event Action<int, string> onTargetCheckerErrorTipValueChanged;
        /// <summary>
        /// 当点击返回之前编辑的动作图按钮
        /// </summary>
        public event Action onClickReturnButton;
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
        [SerializeField]
        float _autoPosSpacingWidth = 600;
        [SerializeField]
        float _autoPosSpacingHeight = 300;
        [SerializeField]
        Button _returnButton;
        [SerializeField]
        Image _backgroundImage;
        [SerializeField]
        ActionNodeItem _actionNodeItemTemplate;
        [SerializeField]
        RectTransform _actionNodesRoot;
        [SerializeField]
        List<ActionNodeItem> _actionNodeItemList = new List<ActionNodeItem>();
        [SerializeField]
        ActionNodeGraphLine _actionNodeLineTemplate;
        [SerializeField]
        RectTransform _actionNodeLineRoot;
        [SerializeField]
        List<ActionNodeConnectionData> _connectionList = new List<ActionNodeConnectionData>();
        [SerializeField]
        RectTransform _menuRoot;
        [SerializeField]
        RectTransform _Content;
        [SerializeField]
        RectTransform _menuItemTemplate;
        [SerializeField]
        List<RectTransform> _menuItemList = new List<RectTransform>();
        [SerializeField]
        ActionNodeItem _pointerOverActionNodeItem;
        [SerializeField]
        Timer _longPressTimer = new Timer() { duration = .5f };
        [SerializeField]
        ActionNodeItem _dragItem;
        [SerializeField]
        Vector2 _dragItemPositionOffset;
        [SerializeField]
        ActionNodeGraphLine _dragLine;
        #endregion
    }
    public struct ActionNodeValueInfo
    {
        public string name;
        public ActionNodeValueType type;
        public object value;
        public Type enumType;
    }
    [Serializable]
    struct ActionNodeConnectionData
    {
        public ActionNodeItem outputNode;
        public int outputIndex;
        public ActionNodeItem inputNode;
        public int inputIndex;
        public ActionNodeGraphLine line;
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
        tagCollection,
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
    }
    public enum ActionNodeConnectionType
    {
        action,
        value
    }
}