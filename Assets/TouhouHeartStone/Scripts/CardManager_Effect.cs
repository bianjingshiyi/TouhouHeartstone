using Controllers;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        /// <summary>
        /// 获取所有效果类型的名称
        /// </summary>
        /// <returns></returns>
        public string[] getEffectNames()
        {
            return _effectTypes;
        }
        /// <summary>
        /// 获取所有给定效果类型的名称
        /// </summary>
        /// <param name="effects"></param>
        /// <returns></returns>
        public string[] getEffectNames(GeneratedEffect[] effects)
        {
            string[] effectNames = new string[effects.Length];
            for (int i = 0; i < effectNames.Length; i++)
            {
                string effectName = _effectTypes[getEffectTypeIndex(effects[i])];
                effectNames[i] = string.Format(game.langManager.getString(StringTable.EDITOR_EDIT_EFFECT), effectName);
            }
            return effectNames;
        }
        public GeneratedEffect createEffectByIndex(long cardPoolId, int index)
        {
            ActionGraph graph = new ActionGraph();
            GeneratedEffect effect;
            if (index == 3)
                effect = new GeneratedAura(graph);
            if (index == 2)
                effect = new GeneratedEventEffect(graph);
            else if (index == 1)
                effect = new GeneratedPassiveEffect(graph);
            else
                effect = new GeneratedActiveEffect(graph);
            effect.Init();

            var nodeDefiner = new THHNodeDefiner(name => game.getActionDefine(name, cardPoolId), EventTypeInfoLoader.FindEventTypeInfo);
            graph.DefineNodes(nodeDefiner);
            return effect;
        }
        public int getEffectTypeIndex(GeneratedEffect effect)
        {
            if (effect is GeneratedAura)
                return 3;
            if (effect is GeneratedEventEffect)
                return 2;
            else if (effect is GeneratedPassiveEffect)
                return 1;
            else
                return 0;
        }
        #endregion
        #region 属性字段
        readonly string[] _effectTypes = new string[]
        {
            StringTable.EDITOR_EFFECT_NAME_ACTIVE_EFFECT,
            StringTable.EDITOR_EFFECT_NAME_PASSIVE_EFFECT,
            StringTable.EDITOR_EFFECT_NAME_EVENT_EFFECT,
            StringTable.EDITOR_EFFECT_NAME_AURA_EFFECT,
        };
        #endregion
    }
}
