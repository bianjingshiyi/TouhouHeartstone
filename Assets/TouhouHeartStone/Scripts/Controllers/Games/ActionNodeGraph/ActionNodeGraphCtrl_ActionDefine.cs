using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Game;
using MukioI18n;
using TouhouCardEngine;
using UI;
using UnityEngine.UIElements;

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
            // 连接向上移动。
            movePortConnections(deletedPort, -1);
        }
        /// <summary>
        /// 改变自定义动作的一个输入值的类型时，检查连接到该值的线的合法性。
        /// </summary>
        /// <param name="name"></param>
        public void onChangeActionDefineInputType(string name)
        {
            // 检查连接合法性。
            var entryNode = _generatedActionDefine.getEntryNode();
            var changedPort = entryNode?.getOutputPort(name);
            if (changedPort == null)
                return;
            disconnectPortInvalidConnections(changedPort);
            updateActionDefineEntryNode();
        }
        /// <summary>
        /// 改变自定义动作的一个常量值的类型时，检查连接到该值的线的合法性。
        /// </summary>
        /// <param name="name"></param>
        public void onChangeActionDefineConstType(string name)
        {
            // 检查连接合法性。
            var entryNode = _generatedActionDefine.getEntryNode();
            var changedPort = entryNode?.getOutputPort(name);
            if (changedPort == null)
                return;
            disconnectPortInvalidConnections(changedPort);
            updateActionDefineEntryNode();
        }
        /// <summary>
        /// 改变自定义动作的一个返回值的类型时，检查连接到该值的线的合法性。
        /// </summary>
        /// <param name="name"></param>
        public void onChangeActionDefineOutputType(string name)
        {
            // 检查连接合法性。
            var returnNode = _generatedActionDefine.getReturnNode();
            var changedPort = returnNode?.getInputPort(name);
            if (changedPort == null)
                return;
            disconnectPortInvalidConnections(changedPort);
            updateActionDefineEntryNode();
        }

        public async void onCreateActionDefine(string name, string category)
        {
            if (string.IsNullOrEmpty(name))
            {
                //空名
                _ui.showCreateActionDefineDialogError(game.langManager.getString(EDITOR_ERROR_ACTION_DEFINE_NAME_EMPTY));
                return;
            }
            else if (isActionDefineNameExist(name))
            {
                //重名
                _ui.showCreateActionDefineDialogError(game.langManager.getString(StringTable.EDITOR_ERROR_ACTION_DEFINE_NAME_ALREADY_EXIST));
                return;
            }

            //新建自定义动作
            GeneratedActionDefine generatedActionDefine = game.card.createActionDefine(_curCardPoolId, name);
            generatedActionDefine.setCategory(category);
            var clone = game.card.cloneActionDefine(_curCardPoolId, generatedActionDefine);
            _ui.displayCreateActionDefineDialog(false);
            await scene.editor.openActionEditor(_curCardPoolId, clone);
            OnEdited();
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

            var viewData = getActionDefineReturnNodeInputViewData(returnNode);

            var returnNodeItem = getActionNodeItem(returnNode);
            returnNodeItem.setInputs(viewData);
        }

        private void updateActionDefineEntryNode()
        {
            var entryNode = _generatedActionDefine.getEntryNode();
            entryNode.Define();

            var viewData = getActionDefineEntryNodeOutputViewData(entryNode);

            var entryNodeItem = getActionNodeItem(entryNode);
            entryNodeItem.setOutputs(viewData);
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
        private ActionDefine getActionDefine(string name)
        {
            return game.getActionDefine(name, _curCardPoolId);
        }
        private ActionDefine[] getAllActionDefines()
        {
            return game.getActionDefinesArray(_curCardPoolId);
        }
        private string getActionDefineCategory(ActionDefine actionDefine)
        {
            string name = string.Empty;
            if (!string.IsNullOrEmpty(actionDefine.category))
                name = actionDefine.category;
            if (_nameTextDict.ContainsKey(name))
                name = _nameTextDict[name];
            var category = game.langManager.getString(name);
            category = category.Replace('\\', '/').Trim('/');
            category = Regex.Replace(category, "/+", "/");
            return category;
        }
        private string[] getActionDefineCategories(string parent)
        {
            if (parent == null)
                parent = string.Empty;
            parent = parent.Replace('\\', '/').Trim('/');
            return getAllActionDefines().Where(d => !d.disableCreate).Select(actionDefine =>
            {
                if (actionDefine.disableCreate)
                    return null;
                var nodeCategory = getActionDefineCategory(actionDefine);
                // 去掉父类别。
                if (!string.IsNullOrEmpty(parent))
                {
                    string parentPath = parent + "/";
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
            }).Where(c => !string.IsNullOrEmpty(c)).Distinct().OrderBy(g => g).ToArray();
        }
        private ActionDefine[] getActionDefinesInCategory(string parent)
        {
            if (parent == null)
                parent = string.Empty;
            parent = parent.Replace('\\', '/').Trim('/');
            return getAllActionDefines().Where(actionDefine => 
            {
                if (actionDefine.disableCreate)
                    return false;
                var nodeCategory = getActionDefineCategory(actionDefine);
                return nodeCategory == parent;
            }).ToArray();
        }
        #endregion
        #endregion
        #region 属性字段
        [TranslateMsg("尝试添加空名称的自定义属性的错误提示")]
        public const string EDITOR_ERROR_ACTION_DEFINE_NAME_EMPTY = "请输入动作名称";
        private GeneratedActionDefine _generatedActionDefine;
        #endregion
    }
}
