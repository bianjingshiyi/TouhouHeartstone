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
        /// 打开一个函数节点的动作节点图
        /// </summary>
        public void openFunctionNodeGraph(long cardPoolId, FunctionNode node, ActionNodeGraphInfo graphInfo)
        {
            display(new DisplayInfo(cardPoolId, null, node, null, graphInfo));
        }
        public void updateFunctionNodeEntryNode()
        {
            ValueDefine[] outputDefines = getFunctionNodeParameterDefines();
            (string name, Color color)[] outputData = new (string name, Color color)[outputDefines.Length + 1];
            outputData[0] = (string.Empty, game.editor.getColorByType(typeof(ActionNode)));
            for (int i = 0; i < outputDefines.Length; i++)
            {
                var define = outputDefines[i];
                outputData[i + 1] = (game.lang._(define.name), game.editor.getColorByType(define.type));
            }
            _entryNodeItem.setOutputs(outputData);

        }
        public void updateFunctionNodeReturnNode()
        {
            if (_returnNodeItem == null)
                return;
            ValueDefine[] returnDefines = getFunctionNodeReturnDefines();
            (string name, Color color)[] returnData = new (string name, Color color)[returnDefines.Length];
            for (int i = 0; i < returnDefines.Length; i++)
            {
                var define = returnDefines[i];
                returnData[i] = (game.lang._(define.name), game.editor.getColorByType(define.type));
            }
            _returnNodeItem.setInputs(returnData);

        }
        #endregion
        #region 私有方法
        private async Task openFunctionNodeGraph(FunctionNode node)
        {
            resetEditObjects();
            _functionNode = node;
            //显示UI
            _ui.clearGraph();
            //创建入口节点
            _entryNodeItem = _ui.createNode(0, 0, game.langManager.getString(StringTable.EDITOR_NODE_NAME_INPUT));
            //设置参数
            updateFunctionNodeEntryNode();
            //创建出口节点
            var returnDefines = getFunctionNodeReturnDefines();
            if (returnDefines != null && returnDefines.Length > 0)
            {
                _returnNodeItem = _ui.createNode(500, 0, game.langManager.getString(StringTable.EDITOR_NODE_NAME_OUTPUT));
                //设置返回值
                updateFunctionNodeReturnNode();
            }
            //连接后续动作节点
            ActionNode branch = _functionNode.action;
            if (branch != null)
            {
                ActionNodeItem inputActionNodeItem = await createGraph4ActionNode(branch);
                _ui.connectNode(_entryNodeItem, 0, inputActionNodeItem, 0, ActionNodeConnectionType.action);
            }
            //连接输入节点
            for (int i = 0; i < _functionNode.returns.Length; i++)
            {
                if (_functionNode.returns[i] == null || _functionNode.returns[i].valueRef == null)
                {
                    continue;
                }
                ActionValueRef valueRef = _functionNode.returns[i].valueRef;
                ActionNodeItem outputActionNodeItem;
                int outputIndex;
                if (valueRef.action != null)
                {
                    outputActionNodeItem = await createGraph4ActionNode(valueRef.action);
                    outputIndex = getBranchesCount(getActionDefine(valueRef.action.defineName)) + valueRef.index;
                }
                else if (valueRef.actionNodeId != 0)
                {
                    ActionNode actionNode = getActionNode(valueRef.actionNodeId);
                    outputActionNodeItem = await createGraph4ActionNode(actionNode);
                    outputIndex = getBranchesCount(getActionDefine(actionNode.defineName)) + valueRef.index;
                }
                else
                {
                    outputActionNodeItem = _entryNodeItem;
                    outputIndex = 1 + valueRef.index;
                }
                if (_returnNodeItem != null)
                {
                    _ui.connectNode(outputActionNodeItem, outputIndex, _returnNodeItem, i, ActionNodeConnectionType.value);
                }
            }
            await createFreeNodes();
            updateContentSize();
        }


        private bool validateConnection4FunctionNodeOutput2Return(int outputIndex, int inputIndex)
        {
            // 尝试连接：入口节点（动作）----出口节点
            if (outputIndex < 1)
            {
                //值输出只能与值输入相互连接
                ShowValueConnectActionError();
                return false;
            }
            Type outputType = getFunctionNodeParameterTypeAt(outputIndex - 1);
            Type inputType = getFunctionNodeReturnTypeAt(inputIndex);
            return validateConnectionType(outputType, inputType);
        }
        private bool validateConnection4FunctionNodeOutput(int outputIndex, ActionNode inputActionNode, int inputIndex, bool showErrorTip = true)
        {
            // 尝试连接：函数节点（值）----动作节点（前置动作）
            if (inputIndex < 1)
            {
                //值输出只能与值输入相互连接
                if (showErrorTip)
                    ShowValueConnectActionError();
                return false;
            }
            Type outputType = getFunctionNodeParameterTypeAt(outputIndex - 1);
            Type inputType = getActionDefine(inputActionNode.defineName).inputs[inputIndex - 1].type;
            return validateConnectionType(outputType, inputType, showErrorTip);
        }
        /// <summary>
        /// 将输入节点的输出值连接到输入上
        /// </summary>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNode"></param>
        /// <param name="inputIndex"></param>
        private void connectNode4FunctionNodeOutput(int outputIndex, ActionNode inputActionNode, int inputIndex)
        {
            //判断是否已经相连
            if (inputActionNode.inputs != null &&
                inputActionNode.inputs.Length > inputIndex &&
                inputActionNode.inputs[inputIndex] != null)
            {
                if (inputActionNode.inputs[inputIndex].index == outputIndex)
                {
                    //已经相连，什么也不做
                    return;
                }
                else
                {
                    //相连的是其他输入节点，断开连接
                    unconnectNode4AnyType(null, inputActionNode.inputs[inputIndex].index, inputActionNode, inputIndex);
                }
            }
            //建立新的连接
            inputActionNode.inputs[inputIndex] = new ActionValueRef(outputIndex);
            //更新ui
            _ui.connectNode(_entryNodeItem, 1 + outputIndex, getActionNodeItem(inputActionNode), 1 + inputIndex, ActionNodeConnectionType.value);
        }

        /// <summary>
        /// 连接：动作节点（值输出）----函数节点出口节点（值输入）
        /// </summary>
        /// <param name="outputActionNode">动作节点（值输出）</param>
        /// <param name="outputIndex">输出索引</param>
        /// <param name="inputIndex">输入索引</param>
        private void connectNode4FunctionNodeReturn(ActionNode outputActionNode, int outputIndex, int inputIndex)
        {
            //判断是否已经相连
            var returns = _functionNode.returns;
            ReturnValueRef returnValueRef = returns.FirstOrDefault(r => r != null && r.valueRef != null && r.returnIndex == inputIndex);
            if (returnValueRef != null)
            {
                if (returnValueRef.valueRef != null &&
                    returnValueRef.valueRef.action != null)
                {
                    if (returnValueRef.valueRef.action == outputActionNode && returnValueRef.valueRef.index == outputIndex)
                    {
                        //已经相连，什么也不做
                        return;
                    }
                    else
                    {
                        //如果输入已经和其他输出连接，则断开。
                        unconnectNode4AnyType(returnValueRef.valueRef.action, returnValueRef.valueRef.index, null, inputIndex);
                    }
                }
            }
            //建立新的连接
            if (returns == null || inputIndex >= returns.Length)
            {
                var newArray = new ReturnValueRef[inputIndex + 1];
                returns.CopyTo(newArray, 0);
                returns = newArray;
                _functionNode.returns = newArray;
            }
            //建立新的连接
            if (returns[inputIndex] != null)
            {
                returns[inputIndex].valueRef = new ActionValueRef(outputActionNode, outputIndex);
            }
            else
            {
                returns[inputIndex] = new ReturnValueRef(outputActionNode, outputIndex, inputIndex);
            }
            //从离散节点列表中去掉被连接的节点
            _graphInfo.freeActionNodeList.Remove(outputActionNode);
            //更新输出节点的输出变量状态
            updateRegVar(outputActionNode);
            //更新ui
            _ui.connectNode(getActionNodeItem(outputActionNode), outputActionNode.branches.Length + outputIndex, _returnNodeItem, 0, ActionNodeConnectionType.value);
        }
        /// <summary>
        /// 将入口节点的输出值连接到出口节点上
        /// </summary>
        /// <param name="outputIndex"></param>
        /// <param name="inputIndex"></param>
        private void connectNode4FunctionNodeOutput2Return(int outputIndex, int inputIndex)
        {
            //判断是否已经相连
            if (_functionNode.returns != null &&
                _functionNode.returns.Length > inputIndex &&
                _functionNode.returns[inputIndex] != null)
            {
                var valueRef = _functionNode.returns[inputIndex].valueRef;
                if (valueRef.index == outputIndex)
                {
                    //已经相连，什么也不做
                    return;
                }
                else
                {
                    //相连的是其他输入节点，断开连接
                    unconnectNode4AnyType(null, valueRef.index, null, inputIndex);
                }
            }
            //建立新的连接
            if (_functionNode.returns[inputIndex] != null)
            {
                _functionNode.returns[inputIndex].valueRef = new ActionValueRef(outputIndex);
            }
            else
            {
                _functionNode.returns[inputIndex] = new ReturnValueRef(outputIndex, inputIndex);
            }
            //更新ui
            _ui.connectNode(_entryNodeItem, 1 + outputIndex, _returnNodeItem, inputIndex, ActionNodeConnectionType.value);
        }
        private void unconnectNode4FunctionNodeOutput(int outputIndex, ActionNode inputActionNode, int inputIndex)
        {
            //判断是否已经相连
            if (inputActionNode.inputs != null &&
                inputActionNode.inputs.Length > inputIndex &&
                inputActionNode.inputs[inputIndex] != null)
            {
                if (inputActionNode.inputs[inputIndex].index == outputIndex)
                {
                    //相连，断开连接
                    inputActionNode.inputs[inputIndex] = null;
                    //更新ui
                    _ui.unconnectNode(_entryNodeItem, 1 + outputIndex, getActionNodeItem(inputActionNode), 1 + inputIndex);
                }
            }
        }
        private void unconnectNode4FunctionNodeReturn(ActionNode outputActionNode, int outputIndex, int inputIndex)
        {
            //判断是否已经相连
            if (inputIndex >= 0)
            {
                if (_functionNode.returns[inputIndex] != null &&
                    _functionNode.returns[inputIndex].valueRef.index == outputIndex)
                {
                    //相连，断开连接
                    _functionNode.returns[inputIndex] = null;
                    //更新ui
                    _ui.unconnectNode(getActionNodeItem(outputActionNode), outputActionNode.branches.Length + outputIndex, _returnNodeItem, inputIndex);
                }
            }
            //更新输出节点的输出变量状态
            updateRegVar(outputActionNode);
        }
        private void unconnectNode4FunctionNodeOutput2Return(int outputIndex, int inputIndex)
        {
            //判断是否已经相连
            if (inputIndex >= 0)
            {
                if (_functionNode.returns[inputIndex] != null &&
                    _functionNode.returns[inputIndex].valueRef.index == outputIndex)
                {
                    //相连，断开连接
                    _functionNode.returns[inputIndex] = null;
                    //更新ui
                    _ui.unconnectNode(_entryNodeItem, 1 + outputIndex, _returnNodeItem, inputIndex);
                }
            }
        }


        private FunctionDefine getFunctionNodeDefine()
        {
            if (_functionDefineDict.TryGetValue(_functionNode.functionName, out FunctionDefine value))
            {
                return value;
            }
            return null;
        }
        private ValueDefine[] getFunctionNodeParameterDefines()
        {
            return getFunctionNodeDefine().parameters;
        }
        private ValueDefine[] getFunctionNodeReturnDefines()
        {
            return getFunctionNodeDefine().returns;
        }
        private Type getFunctionNodeParameterTypeAt(int index)
        {
            var outputs = getFunctionNodeParameterDefines();
            if (outputs == null)
                return null;
            if (index < outputs.Length)
                return outputs[index].type;
            return outputs[index - outputs.Length].type;
        }
        private Type getFunctionNodeReturnTypeAt(int index)
        {
            var returns = getFunctionNodeReturnDefines();
            if (returns == null)
                return null;
            if (index < returns.Length)
                return returns[index].type;
            return returns[index - returns.Length].type;
        }
        #endregion
        #region 属性字段
        private FunctionNode _functionNode;
        #endregion
    }
}
