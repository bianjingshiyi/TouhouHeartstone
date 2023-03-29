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
using TouhouCardEngine.Interfaces;

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
            display(new DisplayInfo(cardPoolId, actionDefine, null, graphInfo));
        }
        public void updateActionDefineEntryAndReturnNodes()
        {
            updateActionDefineEntryNode();
            updateActionDefineReturnNodes();
        }
        /// <summary>
        /// 为自定义动作添加一个输入值。
        /// </summary>
        /// <param name="portDefine"></param>
        public void addActionDefineInput(PortDefine portDefine)
        {
            _generatedActionDefine.addInput(portDefine);
            updateActionDefineEntryNode();
        }
        /// <summary>
        /// 删除自定义动作的一个输入值。
        /// </summary>
        /// <param name="portDefine"></param>
        public void deleteActionDefineInput(PortDefine portDefine)
        {
            _generatedActionDefine.removeInput(portDefine);
            updateActionDefineEntryNode();
        }
        /// <summary>
        /// 删除自定义动作的一个常量值。
        /// </summary>
        /// <param name="portDefine"></param>
        public void deleteActionDefineConst(PortDefine portDefine)
        {
            _generatedActionDefine.removeConst(portDefine);
            updateActionDefineEntryNode();
        }
        /// <summary>
        /// 删除自定义动作的一个返回值。
        /// </summary>
        /// <param name="inputIndex"></param>
        public void deleteActionDefineOutput(PortDefine portDefine)
        {
            _generatedActionDefine.removeOutput(portDefine);
            updateActionDefineReturnNodes();
        }
        /// <summary>
        /// 改变自定义动作的一个输入值的类型，并检查连接到该值的线的合法性。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        public void changeActionDefineInputType(string name, Type type)
        {
            var input = _generatedActionDefine.getInputDefine(name);
            if (input == null)
                return;

            input.type = type;

            // 检查连接合法性。
            // 断开不合法的，连接到返回节点上的连接
            var entryNode = _generatedActionDefine.getEntryNode();
            var changedPort = entryNode.getOutputPort(name);
            var returnNode = _generatedActionDefine.getReturnNode();
            foreach (var returnInput in returnNode.inputPorts)
            {
                if (returnInput == null)
                    continue;

                var connection = graph.getConnection(changedPort, returnInput);
                if (connection != null)
                {
                    if (!isValidTypeForConnection(type, returnInput.define.type))
                    {
                        disconnectPorts(changedPort, returnInput);
                    }
                }
            }

            // 断开不合法的，连接到其他节点上的连接
            foreach (var node in _actionNodeList)
            {
                if (node == null)
                    continue;
                foreach (var nodeInput in node.inputPorts)
                {
                    if (nodeInput == null)
                        continue;
                    if (!graph.isConnected(changedPort, nodeInput))
                        continue;

                    if (!isValidTypeForConnection(type, nodeInput.define.type))
                    {
                        disconnectPorts(changedPort, nodeInput);
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
        public void changeActionDefineConstType(string name, Type type)
        {
            var constDefine = _generatedActionDefine.getConstDefine(name);
            if (constDefine == null)
                return;
            constDefine.type = type;
            // 检查连接合法性。
            // 断开不合法的，连接到返回节点上的连接
            var entryNode = _generatedActionDefine.getEntryNode();
            var changedPort = entryNode.getOutputPort(name);
            var returnNode = _generatedActionDefine.getReturnNode();
            foreach (var returnInput in returnNode.inputPorts)
            {
                if (returnInput == null)
                    continue;

                var connection = graph.getConnection(changedPort, returnInput);
                if (connection != null)
                {
                    if (!isValidTypeForConnection(type, returnInput.define.type))
                    {
                        disconnectPorts(changedPort, returnInput);
                    }
                }
            }

            // 断开不合法的，连接到其他节点上的连接
            foreach (var node in _actionNodeList)
            {
                if (node == null)
                    continue;
                foreach (var nodeInput in node.inputPorts)
                {
                    if (nodeInput == null)
                        continue;
                    if (!graph.isConnected(changedPort, nodeInput))
                        continue;

                    if (!isValidTypeForConnection(type, nodeInput.define.type))
                    {
                        disconnectPorts(changedPort, nodeInput);
                    }
                }
            }
            updateActionDefineEntryNode();
        }
        /// <summary>
        /// 改变自定义动作的一个返回值的类型，并检查连接到该值的线的合法性。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        public void changeActionDefineOutputType(string name, Type type)
        {
            var outputDefine = _generatedActionDefine.getOutputDefine(name);
            if (outputDefine == null)
                return;
            outputDefine.type = type;
            // 检查连接合法性。
            // 断开不合法的，连接到返回节点上的连接
            var entryNode = _generatedActionDefine.getEntryNode();
            var returnNode = _generatedActionDefine.getReturnNode();
            var changedPort = returnNode.getInputPort(name);
            foreach (var entryOutput in entryNode.outputPorts)
            {
                if (entryOutput == null)
                    continue;

                var connection = graph.getConnection(entryOutput, changedPort);
                if (connection != null)
                {
                    if (!isValidTypeForConnection(entryOutput.define.type, type))
                    {
                        disconnectPorts(entryOutput, changedPort);
                    }
                }
            }

            // 断开不合法的，连接到其他节点上的连接
            foreach (var node in _actionNodeList)
            {
                if (node == null)
                    continue;
                foreach (var nodeOutput in node.outputPorts)
                {
                    if (nodeOutput == null)
                        continue;
                    if (!graph.isConnected(nodeOutput, changedPort))
                        continue;

                    if (!isValidTypeForConnection(type, nodeOutput.define.type))
                    {
                        disconnectPorts(nodeOutput, changedPort);
                    }
                }
            }
            updateActionDefineEntryNode();
        }
        #endregion
        #region 私有方法
        private async Task openGeneratedActionDefine(GeneratedActionDefine generatedActionDefine)
        {
            resetEditObjects();
            _generatedActionDefine = generatedActionDefine;
            //显示UI
            _ui.clearGraph();
            //创建动作节点
            await ApplyGraph(_generatedActionDefine.graph);
            //设置参数
            updateActionDefineEntryNode();
            //设置返回
            updateActionDefineReturnNodes();
            updateContentSize();
        }
        private void updateActionDefineReturnNodes()
        {
            var returnNode = _generatedActionDefine.getReturnNode();
            returnNode.Define();

            var inputs = returnNode.inputPorts;
            (string name, Color color)[] viewData = new (string name, Color color)[inputs.Count()];
            for (int i = 0; i < inputs.Count(); i++)
            {
                var output = inputs.ElementAt(i);
                viewData[i] = (output.define?.name, game.editor.getColorByType(output.define?.type));
            }
            var returnNodeItem = getActionNodeItem(returnNode);
            returnNodeItem.setInputs(viewData);
        }

        private void updateActionDefineEntryNode()
        {
            var entryNode = _generatedActionDefine.getEntryNode();
            entryNode.Define();

            var outputs = entryNode.outputPorts;
            var consts = _generatedActionDefine.constDefines;

            (string name, Color color)[] viewData = new (string name, Color color)[outputs.Count() + consts.Count()];
            for (int i = 0; i < outputs.Count(); i++)
            {
                var input = outputs.ElementAt(i);
                viewData[i] = (input.define?.getDisplayName(), game.editor.getColorByType(input.define?.type));
            }
            for (int i = 0; i < consts.Count(); i++)
            {
                var constDef = consts.ElementAt(i);
                viewData[i + consts.Count()] = (constDef.name, game.editor.getColorByType(constDef.type));
            }
            var entryNodeItem = getActionNodeItem(entryNode);
            entryNodeItem.setOutputs(viewData);
        }
        private (string name, Color color)[] getActionDefineInputViewData(GeneratedActionDefine actionDefine)
        {
            (string name, Color color)[] inputs = new (string name, Color color)[actionDefine.inputDefines.Count()];
            for (int i = 0; i < actionDefine.inputDefines.Count(); i++)
            {
                var inputDefine = actionDefine.inputDefines.ElementAtOrDefault(i);
                if (inputDefine == null)
                    continue;
                var inputName = tryTranslateWithDict(inputDefine.getDisplayName());
                inputs[i] = (inputName, game.editor.getColorByType(inputDefine.type));
            }
            return inputs;
        }
        private (string name, Color color)[] getActionDefineOutputViewData(GeneratedActionDefine actionDefine)
        {
            (string name, Color color)[] outputs = new (string name, Color color)[actionDefine.outputDefines.Count()];
            for (int i = 0; i < actionDefine.outputDefines.Count(); i++)
            {
                var outputDefine = actionDefine.outputDefines.ElementAtOrDefault(i);
                if (outputDefine == null)
                    continue;
                var outputName = tryTranslateWithDict(outputDefine.getDisplayName());
                outputs[i] = (outputName, game.editor.getColorByType(outputDefine.type));
            }
            return outputs;
        }
        #region 节点连接
        /// <summary>
        /// 将输入节点的输出值连接到输入上
        /// </summary>
        /// <param name="outputIndex"></param>
        /// <param name="inputActionNode"></param>
        /// <param name="inputIndex"></param>
        private void connectNode4ActionDefineArg(string outputName, ActionNode inputActionNode, string inputName)
        {
            var outputPort = _generatedActionDefine.getEntryNode().getOutputPort<ValueOutput>(outputName);
            var inputPort = inputActionNode.getInputPort<ValueInput>(inputName);

            connectValuePorts(outputPort, inputPort);
        }
        private void connectNode4ActionDefineArgToReturn(string outputName, string inputName)
        {
            var outputPort = _generatedActionDefine.getEntryNode().getOutputPort<ValueOutput>(outputName);
            var inputPort = _generatedActionDefine.getReturnNode().getInputPort<ValueInput>(inputName);

            connectValuePorts(outputPort, inputPort);
        }
        private void unconnectNode4ActionDefineArg(string outputName, ActionNode inputActionNode, string inputName)
        {
            var outputPort = _generatedActionDefine.getEntryNode().getOutputPort<ValueOutput>(outputName);
            var inputPort = inputActionNode.getInputPort<ValueInput>(inputName);

            disconnectPorts(outputPort, inputPort);
        }
        private void unconnectNode4ActionDefineArgToReturn(string outputName, string inputName)
        {
            var outputPort = _generatedActionDefine.getEntryNode().getOutputPort<ValueOutput>(outputName);
            var inputPort = _generatedActionDefine.getReturnNode().getInputPort<ValueInput>(inputName);

            disconnectPorts(outputPort, inputPort);
        }
        #endregion
        #region 动作定义
        private ActionDefine getActionDefine(string name)
        {
            return game.getActionDefine(name, _curCardPoolId);
        }
        private ActionDefine[] getAllActionDefines()
        {
            return game.getActionDefinesArray(_curCardPoolId);
        }
        private IGrouping<string, ActionDefine>[] getActionDefineGroups()
        {
            return getAllActionDefines().Where(d => !d.disableCreate).GroupBy(actionDefine =>
            {
                string name;
                if (string.IsNullOrEmpty(actionDefine.category))
                    name = actionDefine.defineName;
                else
                    name = actionDefine.category;
                if (_nameTextDict.ContainsKey(name))
                    name = _nameTextDict[name];
                return game.langManager.getString(name);
            }, a => a).OrderBy(g => g.Count() > 1).ThenBy(g => g.Key).ToArray();
        }
        #endregion
        #endregion
        #region 属性字段
        private GeneratedActionDefine _generatedActionDefine;
        #endregion
    }
}
