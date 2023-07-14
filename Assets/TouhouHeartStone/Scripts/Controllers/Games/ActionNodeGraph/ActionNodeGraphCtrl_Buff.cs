using System;
using System.Linq;
using System.Threading.Tasks;
using Boo.Lang;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        #region 私有方法
        #region 事件回调
        private void onItemBuffChangedCallback(ActionNodeItem item, int index)
        {
            GeneratedBuffDefine buffDefine = getOrCreateBuffDefineByIndex(index);
            var node = getActionNode(item);
            string name = GetBuffDefineAction.constName;
            var beforeValue = node.getConst(name);
            updateBuffActionNode(item, buffDefine);
            _recordList.addOperation(new ChangeNodeConstOperation(this, node, name, beforeValue, node.getConst(GetBuffDefineAction.constName)));
            _recordList.addBreakpoint();
            OnEdited();
        }
        private async void onItemEditBuffCallback(ActionNodeItem item)
        {
            if (tryGetBuffDefine(getActionNode(item).getConst(GetBuffDefineAction.constName), out GeneratedBuffDefine buffDefine))
            {
                var clone = game.card.cloneBuffDefine(buffDefine);
                var skins = game.card.cloneBuffSkins(_curCardPoolId, buffDefine.id);
                await scene.editor.openBuffEditor(_curCardPoolId, clone, skins);
            }
        }
        #endregion
        private GeneratedBuffDefine getOrCreateBuffDefineByIndex(int index)
        {
            if (index == 0)
            {
                var buffDefine = game.card.createBuffDefine(_curCardPoolId);
                game.card.addBuffDefine(_curCardPoolId, buffDefine);
                return buffDefine;
            }
            else
            {
                var cardPoolRef = game.card.getLocalCardPoolReference(_curCardPoolId);
                if (cardPoolRef == null)
                {
                    return null;
                }
                index--;
                return game.card.getBuffDefines(cardPoolRef)
                    .OrderBy(p => p.Value.cardPoolId)
                    .ThenBy(p => p.Value.id)
                    .ElementAt(index).Value as GeneratedBuffDefine;
            }
        }
        private bool tryGetBuffDefine(object value, out GeneratedBuffDefine buffDefine)
        {
            long cardPoolId;
            int cardId;
            if (value is DefineReference defineRef)
            {
                cardPoolId = defineRef.cardPoolId;
                cardId = defineRef.defineId;
            }
            else if (value is CardReference cardRef)
            {
                cardPoolId = cardRef.cardPoolId;
                cardId = cardRef.cardId;
            }
            else if (value is int buffDefineId)
            {
                cardPoolId = _curCardPoolId;
                cardId = buffDefineId;
            }
            else
            {
                buffDefine = null;
                return false;
            }


            var cardPoolRef = game.card.getLocalCardPoolReference(cardPoolId, false);
            if (cardPoolRef != null)
            {
                return game.card.tryGetBuffDefine(cardPoolRef, cardId, out buffDefine);
            }


            buffDefine = null;
            return false;
        }
        private void updateBuffActionNode(ActionNodeItem item, GeneratedBuffDefine buffDefine)
        {
            var cardPoolRef = game.card.getLocalCardPoolReference(_curCardPoolId);
            if (cardPoolRef == null)
            {
                return;
            }
            var action = getActionNode(item) as ActionNode;
            if (action == null)
                return;
            action.setConst(GetBuffDefineAction.constName, new DefineReference(buffDefine.cardPoolId, buffDefine.id));
            BuffDefine[] buffDefines = game.card.getBuffDefines(cardPoolRef)
                .OrderBy(p => p.Value.cardPoolId)
                .ThenBy(p => p.Value.id)
                .Select(p => p.Value)
                .ToArray();

            var buffNames = new List<string>()
            {
                game.lang._(ACTION_NODE_NEW_BUFF)
            };
            buffNames.AddRange(game.card.getBuffDefineNames(buffDefines));
            var viewData = new ActionNodeBuffViewData()
            {
                buffNames = buffNames.ToArray(),
                buffIndex = Array.IndexOf(buffDefines, buffDefine) + 1
            };
            _ui.updateBuffActionNode(item, viewData);
        }
        #endregion
        #region 属性字段
        [TranslateMsg("动作节点/增益下拉选框/新建增益项")]
        public const string ACTION_NODE_NEW_BUFF = "新建增益";
        #endregion
    }
}
