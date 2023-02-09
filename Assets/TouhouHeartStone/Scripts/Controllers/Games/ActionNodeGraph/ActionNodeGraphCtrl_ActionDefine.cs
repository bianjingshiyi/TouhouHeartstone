using TouhouCardEngine;
using System.Threading.Tasks;
using UnityEngine;
using UI;
using Game;
using System;
using System.Collections.Generic;
using System.Reflection;
using TouhouHeartstone;
using System.Linq;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 公有方法
        /// <summary>
        /// 打开一个自定义动作的动作节点图
        /// </summary>
        /// <param name="trigger"></param>
        public void openGeneratedActionDefine(int cardPoolId, GeneratedActionDefine actionDefine, ActionNodeGraphInfo graphInfo)
        {
            display(new DisplayInfo(cardPoolId, null, actionDefine, null, graphInfo));
        }
        public void updateActionDefineEntryAndReturnNodes()
        {
            updateActionDefineEntryNode();
            updateActionDefineReturnNodes();
        }
        /// <summary>
        /// 为自定义动作添加一个输入值，并重新连接所有常量。
        /// 因为新增输入值会把常量往下顶，所以要单独写一个。
        /// </summary>
        /// <param name="valueDef"></param>
        public void addActionDefineInput(ValueDefine valueDef)
        {
            List<ReturnValueRef> reconnectReturnValueRefs = new List<ReturnValueRef>();
            List<(ActionNode inputNode, int outputIndex, int inputIndex)> reconnectActionNodes = new List<(ActionNode inputNode, int outputIndex, int inputIndex)>();
            // 断开入口节点的常量输出到出口节点的输出连接。
            foreach (var returnRef in _generatedActionDefine.returnValueRefList.FindAll(r => r != null && r.valueRef != null && r.valueRef.action == null && r.valueRef.argIndex >= _generatedActionDefine.inputList.Count))
            {
                var valueRef = returnRef.valueRef;
                reconnectReturnValueRefs.Add(returnRef);
                unconnectNode4ActionDefineArgToReturn(valueRef.argIndex, returnRef.returnIndex);
            }

            //与其他节点断开连接
            for (int i = 0; i < _actionNodeList.Count; i++)
            {
                var node = _actionNodeList[i];
                if (node == null)
                    continue;
                //与输出值断开连接
                if (node.inputs != null)
                {
                    for (int inputIndex = 0; inputIndex < node.inputs.Length; inputIndex++)
                    {
                        var input = node.inputs[inputIndex];
                        if (input == null)
                            continue;
                        if (input.action == null && input.argIndex >= _generatedActionDefine.inputList.Count)
                        {
                            reconnectActionNodes.Add((node, input.argIndex, inputIndex));
                            unconnectNode4ActionDefineArg(input.argIndex, node, inputIndex);
                        }
                    }
                }
            }
            _generatedActionDefine.inputList.Add(valueDef);
            updateActionDefineEntryNode();

            // 重新连接入口节点输出到出口节点的输出连接。
            foreach (var returnRef in reconnectReturnValueRefs)
            {
                connectNode4ActionDefineArgToReturn(returnRef.valueRef.argIndex + 1, returnRef.returnIndex);
            }

            // 重新连接其他节点
            foreach (var (node, outIndex, inputIndex) in reconnectActionNodes)
            {
                connectNode4ActionDefineArg(outIndex + 1, node, inputIndex);
            }
        }
        /// <summary>
        /// 删除自定义动作的一个输入值，并重新连接所有下方的点。
        /// </summary>
        /// <param name="outputIndex"></param>
        public void deleteActionDefineInput(int outputIndex)
        {
            List<ReturnValueRef> reconnectReturnValueRefs = new List<ReturnValueRef>();
            List<(ActionNode inputNode, int outputIndex, int inputIndex)> reconnectActionNodes = new List<(ActionNode inputNode, int outputIndex, int inputIndex)> ();
            // 断开入口节点输出到出口节点的输出连接。
            foreach (var returnRef in _generatedActionDefine.returnValueRefList.ToArray())
            {
                if (returnRef == null || returnRef.valueRef == null)
                    continue;
                var valueRef = returnRef.valueRef;
                if (valueRef.action == null)
                {
                    if (valueRef.argIndex > outputIndex)
                        reconnectReturnValueRefs.Add(returnRef);
                    if (valueRef.argIndex >= outputIndex)
                        unconnectNode4ActionDefineArgToReturn(valueRef.argIndex, returnRef.returnIndex);
                }
            }

            //与其他节点断开连接
            for (int i = 0; i < _actionNodeList.Count; i++)
            {
                var node = _actionNodeList[i];
                if (node == null)
                    continue;
                //与输出值断开连接
                if (node.inputs != null)
                {
                    for (int inputIndex = 0; inputIndex < node.inputs.Length; inputIndex++)
                    {
                        var input = node.inputs[inputIndex];
                        if (input == null)
                            continue;
                        if (input.action == null && input.actionNodeId == 0)
                        {
                            if (input.argIndex > outputIndex)
                                reconnectActionNodes.Add((node, input.argIndex, inputIndex));
                            if (input.argIndex >= outputIndex)
                                unconnectNode4ActionDefineArg(input.argIndex, node, inputIndex);
                        }
                    }
                }
            }
            if (outputIndex < _generatedActionDefine.inputList.Count)
                _generatedActionDefine.inputList.RemoveAt(outputIndex);
            else
                _generatedActionDefine.constList.RemoveAt(outputIndex - _generatedActionDefine.inputList.Count);
            updateActionDefineEntryNode();

            // 重新连接入口节点输出到出口节点的输出连接。
            foreach (var returnRef in reconnectReturnValueRefs)
            {
                connectNode4ActionDefineArgToReturn(returnRef.valueRef.argIndex - 1, returnRef.returnIndex);
            }

            // 重新连接其他节点
            foreach (var (node, outIndex, inputIndex) in reconnectActionNodes)
            {
                connectNode4ActionDefineArg(outIndex - 1, node, inputIndex);
            }
        }
        /// <summary>
        /// 删除自定义动作的一个常量值，并重新连接所有下方的点。
        /// </summary>
        /// <param name="constIndex"></param>
        public void deleteActionDefineConst(int constIndex)
        {
            int outputIndex = _generatedActionDefine.inputList.Count + constIndex;
            deleteActionDefineInput(outputIndex);
        }
        /// <summary>
        /// 删除自定义动作的一个返回值，并重新连接所有下方的点。
        /// </summary>
        /// <param name="inputIndex"></param>
        public void deleteActionDefineOutput(int inputIndex)
        {
            List<ReturnValueRef> reconnectReturnValueRefs = new List<ReturnValueRef>();
            // 断开输出到出口节点的连接。
            var returns = _generatedActionDefine.returnValueRefList;
            if (returns != null)
            {
                foreach (var returnRef in returns.FindAll(r => r.returnIndex >= inputIndex))
                {
                    if (returnRef == null || returnRef.valueRef == null)
                        continue;
                    if (returnRef.returnIndex > inputIndex)
                        reconnectReturnValueRefs.Add(returnRef);

                    var valueRef = returnRef.valueRef;
                    ActionNode actionNode = valueRef.action ?? getActionNode(valueRef.actionNodeId);
                    if (actionNode != null)
                    {
                        unconnectNode4ReturnValueRef(actionNode, valueRef.index, returnRef.returnIndex);
                    }
                    else
                    {
                        unconnectNode4ActionDefineArgToReturn(valueRef.argIndex, returnRef.returnIndex);
                    }
                }
            }
            _generatedActionDefine.outputList.RemoveAt(inputIndex);
            updateActionDefineReturnNodes();


            // 重新连接输出到断开的点下方的点的连接。
            foreach (var returnRef in reconnectReturnValueRefs)
            {
                var valueRef = returnRef.valueRef;
                if (valueRef.action != null)
                {
                    connectNode4ReturnValueRef(valueRef.action, valueRef.index, returnRef.returnIndex - 1);
                }
                else if (valueRef.actionNodeId != 0)
                {
                    ActionNode actionNode = getActionNode(valueRef.actionNodeId);
                    connectNode4ReturnValueRef(actionNode, valueRef.index, returnRef.returnIndex - 1);
                }
                else
                {
                    connectNode4ActionDefineArgToReturn(valueRef.argIndex, returnRef.returnIndex - 1);
                }
            }
        }
        /// <summary>
        /// 改变自定义动作的一个输入值的类型，并检查连接到该值的线的合法性。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        public void changeActionDefineInputType(int index, Type type)
        {
            var inputList = _generatedActionDefine.inputList;
            if (index < inputList.Count)
                _generatedActionDefine.inputList[index].type = type;
            else
                _generatedActionDefine.constList[index - inputList.Count].type = type;
            // 检查连接合法性。
            // 断开不合法的，连接到返回节点上的连接
            var returns = _generatedActionDefine.returnValueRefList.FindAll(r => r != null && r.valueRef != null && r.valueRef.action == null && r.valueRef.argIndex == index);
            foreach (var returnRef in returns)
            {
                if (returnRef == null || returnRef.valueRef == null)
                    continue;
                bool valid = false;

                // 自定义动作的outputList，在其内部其实是输入值。
                var inputs = _generatedActionDefine.outputList;
                if (inputs != null && returnRef.returnIndex >= 0 && returnRef.returnIndex < inputs.Count)
                    valid = isValidTypeForConnection(type, inputs[returnRef.returnIndex].type);

                if (!valid)
                {
                    unconnectNode4ActionDefineArgToReturn(index, returnRef.returnIndex);
                }
            }

            // 断开不合法的，连接到其他节点上的连接
            for (int i = 0; i < _actionNodeList.Count; i++)
            {
                var node = _actionNodeList[i];
                if (node == null || node.inputs == null)
                    continue;
                for (int inputIndex = 0; inputIndex < node.inputs.Length; inputIndex++)
                {
                    var input = node.inputs[inputIndex];
                    if (input != null && input.action == null && input.actionNodeId == 0 && input.argIndex == index)
                    {
                        bool valid = false;

                        var define = getActionDefine(node.defineName);
                        if (define == null)
                            continue;
                        var inputs = define.inputs;
                        if (inputs != null && inputIndex >= 0 && inputIndex < inputs.Length)
                            valid = isValidTypeForConnection(type, inputs[inputIndex].type);

                        if (!valid)
                        {
                            unconnectNode4ActionDefineArg(index, node, inputIndex);
                        }
                    }
                }
            }
            updateActionDefineEntryNode();
        }
        /// <summary>
        /// 改变自定义动作的一个常量值的类型，并检查连接到该值的线的合法性。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        public void changeActionDefineConstType(int index, Type type)
        {
            int outputIndex = _generatedActionDefine.inputList.Count + index;
            changeActionDefineInputType(outputIndex, type);
        }
        /// <summary>
        /// 改变自定义动作的一个返回值的类型，并检查连接到该值的线的合法性。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        public void changeActionDefineOutputType(int index, Type type)
        {
            _generatedActionDefine.outputList[index].type = type;
            // 断开输出到出口节点的连接。
            var returns = _generatedActionDefine.returnValueRefList;
            if (returns != null)
            {
                var returnRef = returns.Find(r => r != null && r.returnIndex == index);
                if (returnRef != null && returnRef.valueRef != null)
                {
                    var valueRef = returnRef.valueRef;
                    ActionNode actionNode = valueRef.action ?? getActionNode(valueRef.actionNodeId);
                    if (actionNode != null)
                    {
                        var valid = false;
                        var define = getActionDefine(actionNode.defineName);
                        if (define != null)
                            valid = isValidTypeForConnection(define.outputs[valueRef.index].type, type);
                        if (!valid)
                            unconnectNode4ReturnValueRef(actionNode, valueRef.index, returnRef.returnIndex);
                    }
                    else
                    {
                        var valid = false;
                        var inputs = _generatedActionDefine.inputs;
                        if (valueRef.argIndex >= 0 && valueRef.argIndex < inputs.Length)
                            valid = isValidTypeForConnection(inputs[valueRef.argIndex].type, type);
                        if (!valid)
                            unconnectNode4ActionDefineArgToReturn(valueRef.argIndex, returnRef.returnIndex);
                    }
                }

            }
            updateActionDefineReturnNodes();
        }
        #endregion
        #region 私有方法
        #region 生命周期
        private void onAwake_ActionDefine()
        {
            //内置动作定义
            _actionDefineDict = ActionDefine.loadDefinesFromAssemblies(new Assembly[]
            {
                typeof(CardEngine).Assembly,
                typeof(HearthStoneRule).Assembly,
                typeof(EffectEditorLibrary).Assembly
            });
        }
        #endregion
        private async Task openGeneratedActionDefine(GeneratedActionDefine generatedActionDefine)
        {
            resetEditObjects();
            _generatedActionDefine = generatedActionDefine;
            //显示UI
            _ui.clearGraph();
            //创建入口节点
            _entryNodeItem = _ui.createNode(0, 0, game.langManager.getString(StringTable.EDITOR_NODE_NAME_INPUT));
            //设置参数
            updateActionDefineEntryNode();
            //创建出口节点
            _returnNodeItem = _ui.createNode(500, 0, game.langManager.getString(StringTable.EDITOR_NODE_NAME_OUTPUT));
            //设置返回
            updateActionDefineReturnNodes();
            //创建动作节点
            if (_generatedActionDefine.action != null)
            {
                ActionNodeItem actionNodeItem = await createGraph4ActionNode(_generatedActionDefine.action);
                _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0, ActionNodeConnectionType.action);
            }
            //连接返回节点
            for (int i = 0; i < _generatedActionDefine.returnValueRefList.Count; i++)
            {
                ReturnValueRef returnValueRef = _generatedActionDefine.returnValueRefList[i];
                ActionNodeItem outputActionNodeItem;
                int outputIndex;

                ActionNode actionNode = returnValueRef.valueRef.action ?? getActionNode(returnValueRef.valueRef.actionNodeId);
                if (actionNode != null)
                {
                    if (!tryGetActionNodeItem(actionNode, out outputActionNodeItem))
                    {
                        outputActionNodeItem = await createGraph4ActionNode(actionNode);
                    }
                    outputIndex = getBranchesCount(getActionDefine(actionNode.defineName)) + returnValueRef.valueRef.index;
                }
                else
                {
                    outputActionNodeItem = _entryNodeItem;
                    outputIndex = 1 + returnValueRef.valueRef.argIndex;
                }
                _ui.connectNode(outputActionNodeItem, outputIndex, _returnNodeItem, returnValueRef.returnIndex, ActionNodeConnectionType.value);
            }
            await createFreeNodes();
            updateContentSize();
        }
        private void updateActionDefineReturnNodes()
        {
            (string name, Color color)[] viewData = new (string name, Color color)[_generatedActionDefine.outputList.Count];
            for (int i = 0; i < _generatedActionDefine.outputList.Count; i++)
            {
                viewData[i] = (_generatedActionDefine.outputList[i].name,
                    game.editor.getColorByType(_generatedActionDefine.outputList[i].type));
            }
            _returnNodeItem.setInputs(viewData);
        }

        private void updateActionDefineEntryNode()
        {
            (string name, Color color)[] viewData = new (string name, Color color)[_generatedActionDefine.inputList.Count + _generatedActionDefine.constList.Count + 1];
            viewData[0] = (string.Empty, game.editor.getColorByType(typeof(ActionNode)));
            for (int i = 0; i < _generatedActionDefine.inputList.Count; i++)
            {
                viewData[i + 1] = (_generatedActionDefine.inputList[i].name,
                    game.editor.getColorByType(_generatedActionDefine.inputList[i].type));
            }
            for (int i = 0; i < _generatedActionDefine.constList.Count; i++)
            {
                viewData[i + _generatedActionDefine.inputList.Count + 1] = (_generatedActionDefine.constList[i].name,
                    game.editor.getColorByType(_generatedActionDefine.constList[i].type));
            }
            _entryNodeItem.setOutputs(viewData);
        }

        private bool validateConnection4ActionDefineEntryOutput(int outputIndex, ActionNode inputActionNode, int inputIndex, bool showErrorTip = true)
        {
            // 尝试连接：自定义动作入口节点（值输出）----出口节点（前置动作）
            if (inputIndex < 1)
            {
                //值输出只能与值输入相互连接
                if (showErrorTip)
                    ShowValueConnectActionError();
                return false;
            }
            try
            {
                Type outputType = getActionDefineInputTypeAt(outputIndex - 1);
                Type inputType = getActionDefine(inputActionNode.defineName).getValueInputAt(inputIndex - 1).type;
                return validateConnectionType(outputType, inputType, showErrorTip);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"验证自定义动作入口节点和动作节点“{inputActionNode.defineName}”的连接合法性时出现异常：{e}");
                return false;
            }
        }
        private bool validateConnection4ActionDefineArgToReturn(int outputIndex, int inputIndex, bool showErrorTip = true)
        {
            // 尝试连接：入口节点（动作）----出口节点
            if (outputIndex < 1)
            {
                //值输出只能与值输入相互连接
                if (showErrorTip)
                    ShowValueConnectActionError();
                return false;
            }
            try
            {
                Type outputType = getActionDefineInputTypeAt(outputIndex - 1);
                Type inputType = _generatedActionDefine.outputList[inputIndex].type;
                return validateConnectionType(outputType, inputType, showErrorTip);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"验证自定义动作入口节点和自定义动作出口节点的连接合法性时出现异常：{e}");
                return false;
            }
        }
        #region 节点连接
        /// <summary>
        /// 将输入节点的输出值连接到输入上
        /// </summary>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNode"></param>
        /// <param name="inputIndex"></param>
        private void connectNode4ActionDefineArg(int outputIndex, ActionNode inputActionNode, int inputIndex)
        {
            //判断是否已经相连
            if (inputActionNode.inputs != null &&
                inputActionNode.inputs.Length > inputIndex &&
                inputActionNode.inputs[inputIndex] != null)
            {
                var input = inputActionNode.inputs[inputIndex];
                if (input.argIndex == outputIndex)
                {
                    //已经相连，什么也不做
                    return;
                }
                else
                {
                    //相连的是其他输入节点，断开连接
                    unconnectNode4AnyType(input.action ?? getActionNode(input.actionNodeId), input.index, inputActionNode, inputIndex);
                }
            }
            // 新建或扩容动作节点的输入列表。
            if (inputActionNode.inputs == null || inputActionNode.inputs.Length <= inputIndex)
            {
                var newArray = new ActionValueRef[inputIndex + 1];
                if (inputActionNode.inputs != null)
                    inputActionNode.inputs.CopyTo(newArray, 0);
                inputActionNode.inputs = newArray;
            }
            //建立新的连接
            inputActionNode.inputs[inputIndex] = new ActionValueRef(outputIndex);
            //更新ui
            _ui.connectNode(_entryNodeItem, 1 + outputIndex, getActionNodeItem(inputActionNode), 1 + inputIndex, ActionNodeConnectionType.value);
        }
        private void connectNode4ActionDefineArgToReturn(int outputIndex, int inputIndex)
        {
            //判断是否已经相连
            int index = _generatedActionDefine.returnValueRefList.FindIndex(r =>
                r != null &&
                r.returnIndex == inputIndex);
            if (index >= 0)
            {
                var returnRef = _generatedActionDefine.returnValueRefList[index];
                var input = returnRef.valueRef;
                if (input != null && input.action == null && input.actionNodeId == 0 && input.argIndex == outputIndex)
                {
                    //已经相连，什么也不做
                    return;
                }
                else
                {
                    //相连的是其他输入节点，断开连接
                    unconnectNode4AnyType(input.action ?? getActionNode(input.actionNodeId), input.index, null, inputIndex);
                }
            }
            //建立新的连接
            _generatedActionDefine.returnValueRefList.Add(new ReturnValueRef(outputIndex, inputIndex));
            //更新ui
            _ui.connectNode(_entryNodeItem, 1 + outputIndex, _returnNodeItem, inputIndex, ActionNodeConnectionType.value);
        }
        private void unconnectNode4ActionDefineArgToReturn(int outputIndex, int inputIndex)
        {
            //判断是否已经相连
            int index = _generatedActionDefine.returnValueRefList.FindIndex(r =>
                r != null &&
                r.returnIndex == inputIndex);
            if (index >= 0)
            {
                var returnRef = _generatedActionDefine.returnValueRefList[index];
                if (returnRef.valueRef != null && returnRef.valueRef.action == null && returnRef.valueRef.actionNodeId == 0 && returnRef.valueRef.argIndex == outputIndex)
                {
                    //相连，断开连接
                    _generatedActionDefine.returnValueRefList.RemoveAt(index);
                    //更新ui
                    _ui.unconnectNode(_entryNodeItem, 1 + outputIndex, _returnNodeItem, inputIndex);
                }
            }
        }
        private void unconnectNode4ActionDefineArg(int outputIndex, ActionNode inputActionNode, int inputIndex)
        {
            //判断是否已经相连
            if (inputActionNode.inputs != null &&
                inputActionNode.inputs.Length > inputIndex &&
                inputActionNode.inputs[inputIndex] != null)
            {
                if (inputActionNode.inputs[inputIndex].argIndex == outputIndex)
                {
                    //相连，断开连接
                    inputActionNode.inputs[inputIndex] = null;
                    //更新ui
                    _ui.unconnectNode(_entryNodeItem, 1 + outputIndex, getActionNodeItem(inputActionNode), 1 + inputIndex);
                }
            }
        }
        #endregion
        #region 动作定义
        private Type getActionDefineInputTypeAt(int index)
        {
            if (index < _generatedActionDefine.inputList.Count)
                return _generatedActionDefine.inputList[index].type;
            return _generatedActionDefine.constList[index - _generatedActionDefine.inputList.Count].type;
        }
        private ActionDefine getActionDefine(string name)
        {
            //内置
            if (_actionDefineDict.TryGetValue(name, out ActionDefine actionDefine))
            {
                return actionDefine;
            }
            //自定义
            if (game.card.getActionDefine(_curCardPoolId, name, out GeneratedActionDefine generatedActionDefine))
            {
                return generatedActionDefine;
            }
            return null;
        }
        private ActionDefine[] getAllActionDefines()
        {
            //内置
            List<ActionDefine> actionDefineList = new List<ActionDefine>();
            foreach (var pair in _actionDefineDict)
            {
                actionDefineList.Add(pair.Value);
            }
            //自定义
            actionDefineList.AddRange(game.card.getActionDefines(_curCardPoolId));
            return actionDefineList.ToArray();
        }
        private IGrouping<string, ActionDefine>[] getActionDefineGroups()
        {
            return getAllActionDefines().GroupBy(actionDefine =>
            {
                string name;
                if (actionDefine is MethodActionDefine methodActionDefine)
                {
                    if (string.IsNullOrEmpty(methodActionDefine.category))
                        name = methodActionDefine.defineName;
                    else
                        name = methodActionDefine.category;
                }
                else if (actionDefine is GeneratedActionDefine generatedActionDefine)
                {
                    if (string.IsNullOrEmpty(generatedActionDefine.category))
                        name = generatedActionDefine.defineName;
                    else
                        name = generatedActionDefine.category;
                }
                else if (actionDefine.defineName.EndsWith("Const"))
                {
                    // 以Const结尾的类型是常量
                    name = StringTable.EDITOR_ACTIONGRAPH_MENU_CATEGORY_CONST;
                }
                else
                {
                    name = actionDefine.defineName;
                }
                if (_nameTextDict.ContainsKey(name))
                    name = _nameTextDict[name];
                return game.langManager.getString(name);
            }, a => a).OrderBy(g => g.Count() > 1).ThenBy(g => g.Key).ToArray();
        }
        #endregion
        #endregion
        #region 属性字段
        Dictionary<string, ActionDefine> _actionDefineDict;
        private GeneratedActionDefine _generatedActionDefine;
        #endregion
    }
}
