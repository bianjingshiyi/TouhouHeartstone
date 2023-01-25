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
        public GeneratedEffect createEffectByIndex(int index)
        {
            if (index == 3)
                return new GeneratedDeathRattle();
            else if (index == 2)
                return new GeneratedBattlecry();
            else if (index == 1)
                return new GeneratedAura();
            else
                return new GeneratedEffect();
        }
        public int getEffectTypeIndex(GeneratedEffect effect)
        {
            if (effect is GeneratedDeathRattle)
                return 3;
            else if (effect is GeneratedBattlecry)
                return 2;
            else if (effect is GeneratedAura || effect is GeneratedHalo)
                return 1;
            else
                return 0;
        }
        #endregion
        #region 属性字段
        readonly string[] _effectTypes = new string[]
        {
            StringTable.EDITOR_EFFECT_NAME_NORMAL_EFFECT,
            StringTable.EDITOR_EFFECT_NAME_HALO_EFFECT,
            StringTable.EDITOR_EFFECT_NAME_BATTLECRY_EFFECT,
            StringTable.EDITOR_EFFECT_NAME_DEATH_RATTLE_EFFECT
        };
        #endregion
    }
}
