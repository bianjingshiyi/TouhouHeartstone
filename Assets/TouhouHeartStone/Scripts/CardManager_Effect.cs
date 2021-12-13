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
                string effectName;
                if (effects[i] is GeneratedHalo)
                    effectName = _effectTypes[1];
                else
                    effectName = _effectTypes[0];
                effectNames[i] = string.Format(game.langManager.getString(StringTable.EDITOR_EDIT_EFFECT), effectName);
            }
            return effectNames;
        }
        public GeneratedEffect createEffectByIndex(int index)
        {
            if (index == 1)
                return new GeneratedHalo();
            else
                return new GeneratedEffect();
        }
        public int getEffectTypeIndex(GeneratedEffect effect)
        {
            if (effect is GeneratedHalo)
                return 1;
            else
                return 0;
        }
        #endregion
        #region 属性字段
        readonly string[] _effectTypes = new string[]
        {
            StringTable.EDITOR_EFFECT_NAME_NORMAL_EFFECT,
            StringTable.EDITOR_EFFECT_NAME_HALO_EFFECT
        };
        #endregion
    }
}
