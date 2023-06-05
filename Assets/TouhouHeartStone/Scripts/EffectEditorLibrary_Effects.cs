using System;
using System.Collections.Generic;
using System.Linq;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace Game
{

    public static partial class EffectEditorLibrary
    {
        [TranslateMsg("编辑器-节点分类名称")]
        public const string EDITOR_CATEGORY_NAME_EFFECTS = "效果";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_EFFECT = "效果";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_EFFECT_TAG = "效果标签";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_EFFECT_HAS_TAG = "效果具有标签";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_GET_EFFECT_PROPERTY = "获取效果属性";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_GET_ALL_CARD_EFFECTS = "获取卡牌的所有效果";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_GET_ALL_BUFF_EFFECTS = "获取增益的所有效果";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_GET_ALL_CARD_DEFINE_EFFECTS = "获取卡牌定义的所有效果";
        /// <summary>
        /// 效果具有标签
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_EFFECT_HAS_TAG, EDITOR_CATEGORY_NAME_EFFECTS)]
        public static bool effectHasTag(
            [ActionNodeParam(EDITOR_PARAM_NAME_EFFECT)] IEffect effect,
            [ActionNodeParam(EDITOR_PARAM_NAME_EFFECT_TAG, isConst: true)] EffectTag tag)
        {
            if (tag == null)
                return false;
            if (effect is GeneratedEffect gen)
            {
                return gen.hasTag(tag);
            }
            return false;
        }
        /// <summary>
        /// 获取效果属性
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_EFFECT_PROPERTY, EDITOR_CATEGORY_NAME_EFFECTS)]
        public static object getEffectProp(
            [ActionNodeParam(EDITOR_PARAM_NAME_EFFECT)] IEffect effect,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME, isConst: true)] string propName)
        {
            if (effect is GeneratedEffect gen)
            {
                return gen.getProp(propName);
            }
            return null;
        }
        /// <summary>
        /// 获取卡牌的所有效果
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_ALL_CARD_EFFECTS, EDITOR_CATEGORY_NAME_EFFECTS)]
        public static IEffect[] getAllCardEffects(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            if (card == null)
                return Array.Empty<IEffect>();
            List<IEffect> effects = new List<IEffect>();
            if (!card.isSilenced(game))
            {
                effects.AddRange(card.define.getEffects());
            }
            foreach (var buff in card.getBuffs())
            {
                effects.AddRange(buff.getEffects(game));
            }
            return effects.ToArray();
        }
        /// <summary>
        /// 获取增益的所有效果
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_ALL_BUFF_EFFECTS, EDITOR_CATEGORY_NAME_EFFECTS)]
        public static IEffect[] getAllBuffEffects(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)] Buff buff)
        {
            if (buff == null)
                return Array.Empty<IEffect>();
            return buff.getEffects(game).ToArray();
        }
        /// <summary>
        /// 获取卡牌定义的所有效果
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_ALL_CARD_DEFINE_EFFECTS, EDITOR_CATEGORY_NAME_EFFECTS)]
        public static IEffect[] getAllCardDefineEffects(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            if (cardDefine == null)
                return Array.Empty<IEffect>();
            return cardDefine.getEffects().ToArray();
        }
    }
    [Serializable]
    public class EffectTag
    {
        public string effectTag;
        public EffectTag(string tag)
        {
            effectTag = tag;
        }

        public override string ToString()
        {
            return effectTag;
        }
        public override bool Equals(object obj)
        {
            if (obj is string str)
            {
                return effectTag == str;
            }
            else if (obj is EffectTag other)
            {
                return effectTag == other.effectTag;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return effectTag.GetHashCode();
        }

        public static bool operator ==(EffectTag lhs, EffectTag rhs)
        {
            if (lhs is null)
                return rhs is null;
            return lhs.Equals(rhs);
        }
        public static bool operator !=(EffectTag lhs, EffectTag rhs)
        {
            if (lhs is null)
                return !(rhs is null);
            return !lhs.Equals(rhs);
        }
        public static implicit operator string(EffectTag tag)
        {
            if (tag is null)
                return null;
            return tag.effectTag;
        }
    }
}
