using System.Threading.Tasks;
using Controllers;
using MukioI18n;
using TouhouCardEngine;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// 用于编辑卡池的编辑器UI基类。
    /// </summary>
    abstract class CardPoolEditor : GameObjectCtrl<MainSceneCtrl>
    {
        public abstract Task display(CardPoolDisplayInfo info);
        public virtual void hide()
        {
            gameObject.SetActive(false);
        }
        #region 事件回调
        #endregion
        #region 效果
        protected abstract void setUIEffects(string[] names, int activeIndex);
        protected void updateEffectList(GeneratedEffect[] effects)
        {
            var currentEffect = _effectEditorCtrl.getCurrentEffect();
            int activeIndex = -1;
            string[] names = new string[effects.Length];
            for (int i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (string.IsNullOrEmpty(effect.name))
                {
                    effect.name = getEffectDefaultName(effect, i + 1);
                }
                names[i] = effect.name;
                if (currentEffect == effect)
                {
                    activeIndex = i;
                }
            }
            setUIEffects(names, activeIndex);
        }
        protected string getEffectDefaultName(GeneratedEffect effect, int num)
        {
            var names = game.card.getEffectNames();
            var index = game.card.getEffectTypeIndex(effect);
            return game.lang._(CARD_EFFECT_DEFAULT_NAME_TEMPLATE, names?[index] ?? CARD_EFFECT_DEFAULT_NAME, num);
        }
        #endregion
        [TranslateMsg("效果默认名称")]
        public const string CARD_EFFECT_DEFAULT_NAME = "效果";
        [TranslateMsg("效果默认名称的模板（{0}为效果名，{1}为效果序号）")]
        public const string CARD_EFFECT_DEFAULT_NAME_TEMPLATE = "{0}{1}";
        [SerializeField]
        protected EffectEditorCtrl _effectEditorCtrl;
    }
    public class CardPoolDisplayInfo
    {
        public long cardPoolId;
        public object displayObj;
    }
}
