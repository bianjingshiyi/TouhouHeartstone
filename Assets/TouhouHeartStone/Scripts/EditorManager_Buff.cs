using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using MukioI18n;
using TouhouCardEngine;
using UI;

namespace Game
{
    public partial class EditorManager : Manager
    {
        public bool tryGetBuffDefine(long curCardPoolId, object value, out GeneratedBuffDefine buffDefine)
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
                cardPoolId = curCardPoolId;
                cardId = buffDefineId;
            }
            else
            {
                buffDefine = null;
                return false;
            }

            var env = getEnvCardPoolRefs();
            var cardPoolRef = game.card.getCardPoolReferenceInEnv(cardPoolId, env);
            if (cardPoolRef != null)
            {
                return game.card.tryGetBuffDefine(cardPoolRef, cardId, out buffDefine);
            }

            buffDefine = null;
            return false;
        }
        /// <summary>
        /// 重新获取当前卡池内的增益定义。
        /// </summary>
        public void updateBuffDefines()
        {
            var env = getEnvCardPoolRefs();
            _currentBuffDefines = env.Select(poolRef => game.card.getBuffDefines(poolRef))
                .Where(d => d != null)
                .SelectMany(d => d.Values)
                .OrderBy(p => p.cardPoolId)
                .ThenBy(p => p.id)
                .ToArray();
        }
        /// <summary>
        /// 获取卡池内的增益定义。
        /// </summary>
        public BuffDefine[] getCurrentBuffDefines()
        {
            return _currentBuffDefines;
        }
        /// <summary>
        /// 获取当前卡池环境下的所有增益名称。
        /// </summary>
        public string[] getBuffNames()
        {
            var newBuffName = game.lang._(DROPDOWN_NEW_BUFF);
            return _currentBuffDefines.Select(b => game.card.getDefaultBuffSkin(b)?.name ?? b.ToString()).Prepend(newBuffName).ToArray();
        }
        /// <summary>
        /// 获取当前卡池环境下的所有增益引用。
        /// </summary>
        public BuffReference[] getBuffReferences()
        {
            return _currentBuffDefines.Select(b => new BuffReference(b.cardPoolId, b.id)).Prepend(newBuffOptionRef).ToArray();
        }
        /// <summary>
        /// 根据增益定义列表获取用于更新UI控件的结构。
        /// </summary>
        public ValueInputFieldProperties getBuffInputProperties()
        {
            var newBuffName = game.lang._(DROPDOWN_NEW_BUFF);
            var buffNames = getBuffNames();
            var buffValues = getBuffReferences();
            return ValueInputFieldProperties.buffReference(buffNames, buffValues);
        }
        /// <summary>
        /// 对单个增益引用进行检查。如果没有指定的增益定义或增益皮肤，获取第一个或创建一个，并通知需要更新增益。
        /// </summary>
        /// <param name="value">单个增益引用的值。</param>
        /// <param name="newValue">被改变后的值。没有改变则为null。</param>
        /// <param name="needUpdateBuffs">是否需要更新所有增益。</param>
        /// <return>值是否发生改变？</return>
        public bool validateSingleBuffReferenceValue(long curCardPoolId, object value, out BuffReference newValue, out bool needUpdateBuffs)
        {
            newValue = null;
            needUpdateBuffs = false;
            BuffDefine buffDefine = null;

            bool needCreate = false;
            if (value is BuffReference buffRef && buffRef == newBuffOptionRef)
            {
                // 新建增益选项。
                needCreate = true;
            }
            else
            {
                // 被选中的增益。
                if (tryGetBuffDefine(curCardPoolId, value, out var gen))
                {
                    // 找得到对应的增益。
                    buffDefine = gen;
                }
                else
                {
                    // 找不到对应的增益。
                    // 设置为卡池内的第一个增益。
                    buffDefine = _currentBuffDefines.FirstOrDefault(b => b != null);

                    if (buffDefine == null) //卡池内没有任何增益
                        needCreate = true;
                    else
                        newValue = new BuffReference(buffDefine.cardPoolId, buffDefine.id);
                }
            }
            if (needCreate)
            {
                var gen = game.card.createBuffDefine(curCardPoolId);
                game.card.addBuffDefine(curCardPoolId, gen);
                buffDefine = gen;
                newValue = new BuffReference(buffDefine.cardPoolId, buffDefine.id);
                needUpdateBuffs = true;
            }
            //获取增益对应的皮肤
            BuffSkin buffSkin = game.card.getBuffSkin(buffDefine);
            if (buffSkin == null)
            {
                //如果不存在皮肤，那就创建一个
                buffSkin = game.card.createBuffSkin(buffDefine.cardPoolId, buffDefine.id, game.langManager.getLanguage());
                game.card.addBuffSkin(curCardPoolId, buffSkin);
                needUpdateBuffs = true;
            }
            return newValue != null;
        }

        public object validatePropertyValue(object value, Action updateBuffDefineAction)
        {
            if (value == null)
                return value;
            var type = value.GetType().getNodeValueType();
            return validatePropertyValue(type, value, updateBuffDefineAction);
        }
        public object validatePropertyValue(NodeValueType targetValueType, object value, Action updateBuffDefineAction)
        {
            if (!targetValueType.isArray())
            {
                // 单个值。
                var actualValueType = ActionNodeValueType.none;
                if (value != null)
                {
                    actualValueType = value.GetType().getNodeValueType().type;
                }
                if (targetValueType.type == ActionNodeValueType.buffReference || actualValueType == ActionNodeValueType.buffReference)
                {
                    if (game.editor.validateSingleBuffReferenceValue(currentCardPoolId, value, out var newBuffRef, out var needUpdateBuffs))
                        value = newBuffRef;
                    if (needUpdateBuffs)
                    {
                        if (updateBuffDefineAction != null)
                            updateBuffDefineAction();
                        else
                            updateBuffDefines();
                    }
                }
            }
            else if (value is IList list)
            {
                // 数组。
                for (int i = 0; i < list.Count; i++)
                {
                    var listElement = list[i];
                    if (listElement == null)
                        continue;
                    // 这里的数组可能是object[]，也有可能是单一类型的数组，因此改为获取列表元素的类型。
                    var elementTargetType = listElement.GetType().getNodeValueType();
                    list[i] = validatePropertyValue(elementTargetType, listElement, updateBuffDefineAction);
                }
                value = list;
            }
            return value;
        }

        #region 属性字段
        [TranslateMsg("增益下拉选框/新建增益项")]
        public const string DROPDOWN_NEW_BUFF = "新建增益";
        private static readonly BuffReference newBuffOptionRef = new BuffReference(0, -1);
        private BuffDefine[] _currentBuffDefines;
        #endregion
    }
}
