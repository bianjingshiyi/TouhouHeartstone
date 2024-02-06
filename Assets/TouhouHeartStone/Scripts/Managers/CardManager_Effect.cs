using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法

        #region 创建效果
        public GeneratedEffect createEffectByIndex(int index)
        {
            ActionGraph graph = new ActionGraph();
            GeneratedEffect effect;
            if (index == AURA_EFFECT_IDX)
                effect = new GeneratedAura(graph);
            else if (index == EVENT_EFFECT_IDX)
                effect = new GeneratedEventEffect(graph);
            else if (index == PASSIVE_EFFECT_IDX)
                effect = new GeneratedPassiveEffect(graph);
            else
                effect = new GeneratedActiveEffect(graph);
            effect.Init();

            var nodeDefiner = getLocalNodeDefiner();
            graph.DefineNodes(nodeDefiner);
            return effect;
        }
        #endregion

        #region 获取效果
        /// <summary>
        /// 获取所有效果类型的名称
        /// </summary>
        /// <returns></returns>
        public string[] getEffectNames()
        {
            return _effectTypeNames.Select(n => game.lang._p(StringTable.CONTEXT_EFFECT_NAME, n)).ToArray();
        }
        public int getEffectTypeIndex(GeneratedEffect effect)
        {
            if (effect is GeneratedAura)
                return AURA_EFFECT_IDX;
            else if (effect is GeneratedEventEffect)
                return EVENT_EFFECT_IDX;
            else if (effect is GeneratedPassiveEffect)
                return PASSIVE_EFFECT_IDX;
            else
                return ACTIVE_EFFECT_IDX;
        }
        #endregion

        #endregion

        #region 属性字段
        public const int ACTIVE_EFFECT_IDX = 0;
        public const int PASSIVE_EFFECT_IDX = 1;
        public const int EVENT_EFFECT_IDX = 2;
        public const int AURA_EFFECT_IDX = 3;
        readonly string[] _effectTypeNames = new string[]
        {
            StringTable.EDITOR_EFFECT_NAME_ACTIVE_EFFECT,
            StringTable.EDITOR_EFFECT_NAME_PASSIVE_EFFECT,
            StringTable.EDITOR_EFFECT_NAME_EVENT_EFFECT,
            StringTable.EDITOR_EFFECT_NAME_AURA_EFFECT,
        };
        #endregion
    }
}
