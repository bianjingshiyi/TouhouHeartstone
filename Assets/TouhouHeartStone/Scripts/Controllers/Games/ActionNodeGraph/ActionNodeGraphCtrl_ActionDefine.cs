using TouhouCardEngine;
using System.Threading.Tasks;
using UnityEngine;
using UI;
using Game;
using System;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 公有方法
        /// <summary>
        /// 打开一个自定义动作的动作节点图
        /// </summary>
        /// <param name="trigger"></param>
        public void openGeneratedActionDefine(int cardPoolId, SerializableActionDefine actionDefine, ActionNodeGraphInfo graphInfo)
        {
            display(new DisplayInfo(cardPoolId, null, actionDefine, null, graphInfo));
        }
        #endregion
        #region 私有方法
        private async Task openGeneratedActionDefine(SerializableActionDefine generatedActionDefine)
        {
            _actionNodeList.Clear();
            _actionNodeItemList.Clear();
            _entryType = null;
            _effect = null;
            _valueRef = null;
            _targetChecker = null;
            _trigger = null;
            _generatedActionDefine = generatedActionDefine;
            //显示UI
            _ui.clearGraph();
            //创建入口节点
            _entryNodeItem = _ui.createNode(0, 0, game.langManager.getString(StringTable.EDITOR_NODE_NAME_INPUT));
            //设置参数
            (string name, Color color)[] viewData = new (string name, Color color)[_generatedActionDefine.inputList.Count + _generatedActionDefine.constList.Count + 1];
            viewData[0] = (string.Empty, game.editor.getColorByTypeName(nameof(ActionNode)));
            for (int i = 0; i < _generatedActionDefine.inputList.Count; i++)
            {
                viewData[i + 1] = (_generatedActionDefine.inputList[i].name,
                    game.editor.getColorByTypeName(_generatedActionDefine.inputList[i].typeName));
            }
            for (int i = 0; i < _generatedActionDefine.constList.Count; i++)
            {
                viewData[i + _generatedActionDefine.inputList.Count + 1] = (_generatedActionDefine.constList[i].name,
                    game.editor.getColorByTypeName(_generatedActionDefine.constList[i].typeName));
            }
            _entryNodeItem.setOutputs(viewData);
            //创建出口节点
            _returnNodeItem = _ui.createNode(500, 0, game.langManager.getString(StringTable.EDITOR_NODE_NAME_OUTPUT));
            //设置返回
            viewData = new (string name, Color color)[_generatedActionDefine.outputList.Count];
            for (int i = 0; i < _generatedActionDefine.outputList.Count; i++)
            {
                viewData[i] = (_generatedActionDefine.outputList[i].name,
                    game.editor.getColorByTypeName(_generatedActionDefine.outputList[i].typeName));
            }
            _returnNodeItem.setInputs(viewData);
            //创建动作节点
            if (_generatedActionDefine.action != null)
            {
                ActionNodeItem actionNodeItem = await createGraph4ActionNode(_generatedActionDefine.action);
                _ui.connectNode(_entryNodeItem, 0, actionNodeItem, 0, ActionNodeConnectionType.action);
            }
            //创建离散节点
            for (int i = 0; i < _graphInfo.freeActionNodeList.Count; i++)
            {
                if (_graphInfo.freeActionNodeList[i] == null)
                    continue;
                if (getActionNode(_graphInfo.freeActionNodeList[i].id) != null)
                {
                    //防止因为离散节点表中错误的记录了已经存在的节点而重复进行创建
                    _graphInfo.freeActionNodeList.RemoveAt(i);
                    i--;
                    continue;
                }
                await createGraph4ActionNode(_graphInfo.freeActionNodeList[i]);
            }
            updateContentSize();
        }
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
                if (inputActionNode.inputs[inputIndex].argIndex == outputIndex)
                {
                    //已经相连，什么也不做
                    return;
                }
                else
                {
                    //相连的是其他输入节点，断开连接
                    unconnectNode4AnyType(outputIndex, inputActionNode, inputIndex);
                }
            }
            //建立新的连接
            inputActionNode.inputs[inputIndex] = new ActionValueRef(outputIndex);
            //更新ui
            _ui.connectNode(_entryNodeItem, 1 + outputIndex, getActionNodeItem(inputActionNode), inputActionNode.branches.Length + inputIndex, ActionNodeConnectionType.value);
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
                    _ui.unconnectNode(_entryNodeItem, 1 + outputIndex, getActionNodeItem(inputActionNode), inputActionNode.branches.Length + inputIndex);
                }
            }
        }
        private Type getActionDefineInputTypeAt(int index)
        {
            if (index < _generatedActionDefine.inputList.Count)
            {
                return game.editor.getTypeByName(_generatedActionDefine.inputList[index].typeName);
            }
            return game.editor.getTypeByName(_generatedActionDefine.constList[index - _generatedActionDefine.inputList.Count].typeName);
        }
        #endregion
        #region 属性字段
        private SerializableActionDefine _generatedActionDefine;
        #endregion
    }
}
