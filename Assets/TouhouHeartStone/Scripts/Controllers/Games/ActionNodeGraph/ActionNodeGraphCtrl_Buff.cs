using System.Linq;
using System.Threading.Tasks;
using Game;
using MukioI18n;
using TouhouCardEngine;
using UI;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 私有方法

        #region 事件回调
        private async void onClickNodeDefaultValueBuffEditButton(ActionNodeItem item, int index, int[] itemIndexes)
        {
            var node = getActionNode(item);
            var inputPort = node.getInputPortAt(index);
            if (inputPort is not ValueInput valueInput)
                return;
            var buffRefObj = node.getInputDefaultValue(valueInput.name, valueInput.paramIndex);
            if (buffRefObj.tryGetListElementByIndexes(itemIndexes, out object result))
            {
                if (tryGetBuffDefine(result, out GeneratedBuffDefine buffDefine))
                {
                    await scene.editor.openBuffEditor(buffDefine);
                }
            }
        }
        #endregion

        private bool tryGetBuffDefine(object value, out GeneratedBuffDefine buffDefine)
        {
            return game.editor.tryGetBuffDefine(_curCardPoolId, value, out buffDefine);
        }
        /// <summary>
        /// 更新所有增益引用，并重新设置节点图内的增益定义的选项。
        /// </summary>
        private void updateBuffReferences()
        {
            game.editor.updateBuffDefines();
            var fieldProps = game.editor.getBuffInputProperties();
            _ui.initValueInputFieldProperties(fieldProps);
        }
        /// <summary>
        /// 对单个增益引用进行检查。如果没有指定的增益定义或增益皮肤，获取第一个或创建一个，并通知需要更新增益。
        /// </summary>
        /// <param name="value">单个增益引用的值。</param>
        /// <param name="newValue">被改变后的值。没有改变则为null。</param>
        /// <param name="needUpdateBuffs">是否需要更新所有增益。</param>
        /// <return>值是否发生改变？</return>
        private bool validateSingleBuffReferenceValue(object value, out BuffReference newValue, out bool needUpdateBuffs)
        {
            return game.editor.validateSingleBuffReferenceValue(_curCardPoolId, value, out newValue, out needUpdateBuffs);
        }
        #endregion
    }
}
