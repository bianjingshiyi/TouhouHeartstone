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
                if (returnValueRef.valueRef.action != null)
                {
                    outputActionNodeItem = getActionNodeItem(returnValueRef.valueRef.action);
                    outputIndex = getBranchesCount(getActionDefine(returnValueRef.valueRef.action.defineName)) + returnValueRef.valueRef.index;
                }
                else if (returnValueRef.valueRef.actionNodeId != 0)
                {
                    ActionNode actionNode = getActionNode(returnValueRef.valueRef.actionNodeId);
                    outputActionNodeItem = getActionNodeItem(actionNode);
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
        private bool validateConnection4ActionDefineEntryOutput(int outputIndex, ActionNode inputActionNode, int inputIndex)
        {
            // 尝试连接：自定义动作入口节点（值输出）----出口节点（前置动作）
            if (inputIndex < inputActionNode.branches.Length)
            {
                //值输出只能与值输入相互连接
                ShowValueConnectActionError();
                return false;
            }
            Type outputType = getActionDefineInputTypeAt(outputIndex - 1);
            Type inputType = getActionDefine(inputActionNode.defineName).inputs[inputIndex - inputActionNode.branches.Length].type;
            return validateConnectionType(outputType, inputType);
        }
        private bool validateConnection4ActionDefineReturn(int outputIndex, int inputIndex)
        {
            // 尝试连接：入口节点（动作）----出口节点
            if (outputIndex < 1)
            {
                //值输出只能与值输入相互连接
                ShowValueConnectActionError();
                return false;
            }
            Type outputType = getActionDefineInputTypeAt(outputIndex - 1);
            Type inputType = _generatedActionDefine.outputList[inputIndex].type;
            return validateConnectionType(outputType, inputType);
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
                if (inputActionNode.inputs[inputIndex].argIndex == outputIndex)
                {
                    //已经相连，什么也不做
                    return;
                }
                else
                {
                    //相连的是其他输入节点，断开连接
                    unconnectNode4AnyType(null, inputActionNode.inputs[inputIndex].argIndex, inputActionNode, inputIndex);
                }
            }
            //建立新的连接
            inputActionNode.inputs[inputIndex] = new ActionValueRef(outputIndex);
            //更新ui
            _ui.connectNode(_entryNodeItem, 1 + outputIndex, getActionNodeItem(inputActionNode), inputActionNode.branches.Length + inputIndex, ActionNodeConnectionType.value);
        }
        private void connectNode4ActionDefineArgToReturn(int outputIndex, int inputIndex)
        {
            //判断是否已经相连
            int index = _generatedActionDefine.returnValueRefList.FindIndex(r =>
                r != null &&
                r.returnIndex == inputIndex);
            if (index >= 0)
            {
                if (_generatedActionDefine.returnValueRefList[index].valueRef != null &&
                    _generatedActionDefine.returnValueRefList[index].valueRef.argIndex == outputIndex)
                {
                    //已经相连，什么也不做
                    return;
                }
                else
                {
                    //相连的是其他输入节点，断开连接
                    unconnectNode4AnyType(null, _generatedActionDefine.returnValueRefList[index].valueRef.argIndex, null, inputIndex);
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
                if (_generatedActionDefine.returnValueRefList[index].valueRef != null &&
                    _generatedActionDefine.returnValueRefList[index].valueRef.argIndex == outputIndex)
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
                    _ui.unconnectNode(_entryNodeItem, 1 + outputIndex, getActionNodeItem(inputActionNode), inputActionNode.branches.Length + inputIndex);
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
