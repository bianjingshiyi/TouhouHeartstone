using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TouhouCardEngine;
using UI;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 公有方法
        public void updateActionDefineEntryAndReturnNodes()
        {
            updateActionDefineEntryNode();
            updateActionDefineReturnNodes();
        }
        /// <summary>
        /// 添加自定义动作的一个输入值时，重新连接所有常量UI的连接。
        /// </summary>
        /// <param name="name"></param>
        public void onAddActionDefineInputPort()
        {
            // 常量连接向下移动。
            var entryNode = _generatedActionDefine.getEntryNode();
            var portDefine = _generatedActionDefine.inputDefines.LastOrDefault();
            if (portDefine == null)
                return;
            var port = entryNode.getOutputPort(portDefine.name);
            if (port == null)
                return;
            movePortConnections(port, 1);
        }
        /// <summary>
        /// 删除自定义动作的一个输入值时，断开所有连接。
        /// </summary>
        /// <param name="name"></param>
        public void onDeleteActionDefineInputPort(string name)
        {
            var entryNode = _generatedActionDefine.getEntryNode();
            var deletedPort = entryNode?.getOutputPort(name);
            if (deletedPort == null)
                return;
            disconnectPortAll(deletedPort);
            if (_connectingPort == deletedPort)
                resetConnectingPort();

            // 连接向上移动。
            movePortConnections(deletedPort, -1);
        }
        /// <summary>
        /// 删除自定义动作的一个输入常量时，断开所有连接。
        /// </summary>
        /// <param name="name"></param>
        public void onDeleteActionDefineConstPort(string name)
        {
            var entryNode = _generatedActionDefine.getEntryNode();
            var deletedPort = entryNode?.getOutputPort(name);
            if (deletedPort == null)
                return;
            disconnectPortAll(deletedPort);
            if (_connectingPort == deletedPort)
                resetConnectingPort();
            // 连接向上移动。
            movePortConnections(deletedPort, -1);
        }
        /// <summary>
        /// 删除自定义动作的一个输出值时，断开所有连接。
        /// </summary>
        /// <param name="name"></param>
        public void onDeleteActionDefineOutputPort(string name)
        {
            var returnNode = _generatedActionDefine.getReturnNode();
            var deletedPort = returnNode?.getInputPort(name);
            if (deletedPort == null)
                return;
            disconnectPortAll(deletedPort);
            if (_connectingPort == deletedPort)
                resetConnectingPort();
            // 连接向上移动。
            movePortConnections(deletedPort, -1);
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
            updateActionDefineEntryAndReturnNodes();
            updateContentSize();
        }
        private void updateActionDefineReturnNodes()
        {
            var returnNode = _generatedActionDefine.getReturnNode();
            returnNode.Define();

            var viewData = getActionDefineReturnNodeInputViewData(returnNode);

            var returnNodeItem = getActionNodeItem(returnNode);
            returnNodeItem.setInputs(viewData);
            updateNodePortModes(returnNode, _connectingPort);
        }

        private void updateActionDefineEntryNode()
        {
            var entryNode = _generatedActionDefine.getEntryNode();
            entryNode.Define();

            var viewData = getActionDefineEntryNodeOutputViewData(entryNode);

            var entryNodeItem = getActionNodeItem(entryNode);
            entryNodeItem.setOutputs(viewData);
            updateNodePortModes(entryNode, _connectingPort);
        }
        private PortItemInfo[] getActionDefineEntryNodeOutputViewData(GeneratedActionEntryNode entryNode)
        {
            var outputs = entryNode.outputPorts;

            PortItemInfo[] viewData = new PortItemInfo[outputs.Count()];
            for (int i = 0; i < outputs.Count(); i++)
            {
                var input = outputs.ElementAt(i);
                viewData[i] = new PortItemInfo(input.define?.getDisplayName(), game.editor.getColorByType(input.define?.type), input.GetPortType() == PortType.Control);
            }
            return viewData;
        }
        private PortItemInfo[] getActionDefineReturnNodeInputViewData(GeneratedActionReturnNode returnNode)
        {
            var inputs = returnNode.inputPorts;
            PortItemInfo[] viewData = new PortItemInfo[inputs.Count()];
            for (int i = 0; i < inputs.Count(); i++)
            {
                var input = inputs.ElementAt(i);
                viewData[i] = new PortItemInfo(input.define?.getDisplayName(), game.editor.getColorByType(input.define?.type), input.GetPortType() == PortType.Control);
            }
            return viewData;
        }
        private PortItemInfo[] getActionDefineInputViewData(GeneratedActionDefine actionDefine)
        {
            PortItemInfo[] inputs = new PortItemInfo[actionDefine.inputDefines.Count()];
            for (int i = 0; i < actionDefine.inputDefines.Count(); i++)
            {
                var inputDefine = actionDefine.inputDefines.ElementAtOrDefault(i);
                if (inputDefine == null)
                    continue;
                var inputName = tryTranslateWithDict(inputDefine.getDisplayName());
                inputs[i] = new PortItemInfo(inputName, game.editor.getColorByType(inputDefine.type), inputDefine.GetPortType() == PortType.Control);
            }
            return inputs;
        }
        private PortItemInfo[] getActionDefineOutputViewData(GeneratedActionDefine actionDefine)
        {
            PortItemInfo[] outputs = new PortItemInfo[actionDefine.outputDefines.Count()];
            for (int i = 0; i < actionDefine.outputDefines.Count(); i++)
            {
                var outputDefine = actionDefine.outputDefines.ElementAtOrDefault(i);
                if (outputDefine == null)
                    continue;
                var outputName = tryTranslateWithDict(outputDefine.getDisplayName());
                outputs[i] = new PortItemInfo(outputName, game.editor.getColorByType(outputDefine.type), outputDefine.GetPortType() == PortType.Control);
            }
            return outputs;
        }
        #region 动作定义
        private ActionDefine getActionDefine(ActionReference actRef)
        {
            return game.card.getActionDefine(actRef);
        }
        private ActionDefine[] getAllActionDefines()
        {
            return game.card.getAllActionDefines(_curCardPoolId);
        }
        private string getActionDefineCategory(ActionDefine actionDefine)
        {
            string name = string.Empty;
            if (!string.IsNullOrEmpty(actionDefine.category))
                name = actionDefine.category;
            if (_nameTextDict.ContainsKey(name))
                name = _nameTextDict[name];
            var category = name;
            category = category.Replace('\\', '/').Trim('/');
            category = Regex.Replace(category, "/+", "/");
            return category;
        }
        private string[] getActionDefineCategories(string parent)
        {
            if (parent == null)
                parent = string.Empty;
            parent = parent.Replace('\\', '/').Trim('/');
            var categories = getAllActionDefines().Where(d => CanActionDefineBeCreated(d)).Select(actionDefine =>
            {
                var nodeCategory = getActionDefineCategory(actionDefine);
                // 去掉父类别。
                if (!string.IsNullOrEmpty(parent))
                {
                    string parentPath = $"{parent}/";
                    int parentIndex = nodeCategory.IndexOf(parentPath);
                    if (parentIndex == 0)
                    {
                        nodeCategory = nodeCategory.Substring(parentPath.Length, nodeCategory.Length - parentPath.Length).TrimStart('/');
                    }
                    else
                    {
                        return null;
                    }
                }
                // 去掉子类别。
                int childIndex = nodeCategory.IndexOf('/');
                if (childIndex > 0)
                {
                    nodeCategory = nodeCategory.Substring(0, childIndex);
                }
                return nodeCategory;
            }).Where(c => !string.IsNullOrEmpty(c)).ToList();

            if (string.IsNullOrEmpty(parent))
            {
                categories.Add(game.lang._(EDITOR_CATEGORY_EVENT_ENTRY_NODE));
            }
            return categories.Distinct().OrderBy(g => g).ToArray();
        }
        private ActionDefine[] getActionDefinesInCategory(string parent)
        {
            if (parent == null)
                parent = string.Empty;
            parent = parent.Replace('\\', '/').Trim('/');
            return getAllActionDefines().Where(actionDefine =>
            {
                if (!CanActionDefineBeCreated(actionDefine))
                    return false;
                var nodeCategory = getActionDefineCategory(actionDefine);
                return nodeCategory == parent;
            }).ToArray();
        }
        #endregion
        #endregion
        #region 属性字段
        private GeneratedActionDefine _generatedActionDefine;
        #endregion
    }
}
