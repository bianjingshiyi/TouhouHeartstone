using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private async void onClickNodeDefaultValueBuffEditButton(ActionNodeItem item, int index)
        {
            var node = getActionNode(item);
            var inputPort = node.getInputPortAt(index);
            if (inputPort is not ValueInput valueInput)
                return;
            var buffRefObj = node.getInputDefaultValue(valueInput.name, valueInput.paramIndex);
            if (tryGetBuffDefine(buffRefObj, out GeneratedBuffDefine buffDefine))
            {
                await scene.editor.openBuffEditor(buffDefine);
            }
        }
        private async void onClickNodeDefaultValueItemBuffEditButton(ActionNodeItem item, int portIndex, int index)
        {
            var node = getActionNode(item);
            var inputPort = node.getInputPortAt(portIndex);
            if (inputPort is not ValueInput valueInput)
                return;
            var buffRefObj = node.getInputDefaultValue(valueInput.name, valueInput.paramIndex);
            if (buffRefObj is IList list)
            {
                var buffRef = list[index];
                if (tryGetBuffDefine(buffRef, out GeneratedBuffDefine buffDefine))
                {
                    await scene.editor.openBuffEditor(buffDefine);
                }
            }
        }
        #endregion

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


            var cardPoolRef = getCardPoolReference(cardPoolId);
            if (cardPoolRef != null)
            {
                return game.card.tryGetBuffDefine(cardPoolRef, cardId, out buffDefine);
            }


            buffDefine = null;
            return false;
        }
        /// <summary>
        /// 更新所有增益引用，并重新设置节点图内的增益定义的选项。
        /// </summary>
        private void updateBuffReferences()
        {
            BuffDefine[] buffDefines = getCardPoolEnv().Select(poolRef => game.card.getBuffDefines(poolRef))
                .Where(d => d != null)
                .SelectMany(d => d.Values)
                .OrderBy(p => p.cardPoolId)
                .ThenBy(p => p.id)
                .ToArray();
            _buffReferences = buffDefines.Select(b => new BuffReference(b.cardPoolId, b.id)).ToArray();

            var newBuffName = game.lang._(ACTION_NODE_NEW_BUFF);
            var buffNames = buffDefines.Select(b => game.card.getDefaultBuffSkin(b)?.name ?? b.ToString()).Prepend(newBuffName).ToArray();
            var buffValues = _buffReferences.Prepend(new BuffReference(0, -1)).ToArray();
            var fieldProps = ValueInputFieldProperties.buffReference(buffNames, buffValues);
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
            newValue = null;
            needUpdateBuffs = false;
            GeneratedBuffDefine buffDefine = null;
            // 新建增益选项。
            if (value is BuffReference buffRef && buffRef.cardPoolId == 0 && buffRef.defineId == -1)
            {
                buffDefine = game.card.createBuffDefine(_curCardPoolId);
                game.card.addBuffDefine(_curCardPoolId, buffDefine);
                needUpdateBuffs = true;

                newValue = new BuffReference(buffDefine.cardPoolId, buffDefine.id);
            }
            else if (value == null || !tryGetBuffDefine(value, out buffDefine))
            {
                // 节点没有增益引用，或者找不到对应的增益

                // 设置为卡池内的第一个增益。
                buffDefine = _buffReferences.Select(r =>
                {
                    var poolRef = getCardPoolReference(r.cardPoolId);
                    if (game.card.tryGetBuffDefine(poolRef, r.defineId, out var buffDefine))
                    {
                        return buffDefine;
                    }
                    return null;
                }).FirstOrDefault(b => b != null);

                if (buffDefine == null) //卡池内没有任何增益
                {
                    // 新建一个。
                    buffDefine = game.card.createBuffDefine(_curCardPoolId);
                    game.card.addBuffDefine(_curCardPoolId, buffDefine);
                    needUpdateBuffs = true;
                }
                newValue = new BuffReference(buffDefine.cardPoolId, buffDefine.id);
            }
            //获取增益对应的皮肤
            BuffSkin buffSkin = game.card.getBuffSkin(buffDefine);
            if (buffSkin == null)
            {
                //如果不存在皮肤，那就创建一个
                buffSkin = game.card.createBuffSkin(buffDefine.cardPoolId, buffDefine.id, game.langManager.getLanguage());
                game.card.addBuffSkin(_curCardPoolId, buffSkin);
                needUpdateBuffs = true;
            }
            return newValue != null;
        }
        #endregion

        #region 属性字段
        [TranslateMsg("动作节点/增益下拉选框/新建增益项")]
        public const string ACTION_NODE_NEW_BUFF = "新建增益";
        BuffReference[] _buffReferences;
        #endregion
    }
}
