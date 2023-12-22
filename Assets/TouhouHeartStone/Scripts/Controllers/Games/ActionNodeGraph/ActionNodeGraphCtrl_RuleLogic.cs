using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 私有方法
        private async Task openGeneratedRuleLogic(GeneratedRuleLogic generatedRuleLogic)
        {
            resetEditObjects();
            _generatedRuleLogic = generatedRuleLogic;
            graph = generatedRuleLogic.graph;
            //显示UI
            _ui.clearGraph();
            //创建动作节点
            await ApplyGraph(_generatedRuleLogic.graph);
        }

        #region 动作定义
        private GeneratedRuleLogic getRuleLogic(ActionReference actRef)
        {
            return game.card.getLocalRuleLogic(actRef);
        }
        private GeneratedRuleLogic[] getAllRuleLogics()
        {
            return game.card.getAllLocalRuleLogics(_curCardPoolId);
        }
        #endregion

        #endregion
        #region 属性字段
        private GeneratedRuleLogic _generatedRuleLogic;
        #endregion
    }
}
