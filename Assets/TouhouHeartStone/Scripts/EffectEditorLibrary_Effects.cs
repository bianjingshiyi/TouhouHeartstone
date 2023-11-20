using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
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
        public const string EDITOR_PARAM_NAME_EFFECT_COLLECTION = "效果集合";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_EFFECT_TAG = "效果标签";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_THIS_EFFECT = "该效果";
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
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_GET_EFFECT_TARGETS = "获取效果的合法使用目标"; 
        /// <summary>
        /// 该效果
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.ThisEffect, EDITOR_CATEGORY_NAME_EFFECTS, EDITOR_NODE_NAME_THIS_EFFECT)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EFFECT)]
        public static IEffect thisEffect(IEffect effect)
        {
            return effect;
        }
        /// <summary>
        /// 效果具有标签
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.EffectHasTag, EDITOR_CATEGORY_NAME_EFFECTS, EDITOR_NODE_NAME_EFFECT_HAS_TAG)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetEffectProp, EDITOR_CATEGORY_NAME_EFFECTS, EDITOR_NODE_NAME_GET_EFFECT_PROPERTY)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetAllCardEffects, EDITOR_CATEGORY_NAME_EFFECTS, EDITOR_NODE_NAME_GET_ALL_CARD_EFFECTS)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EFFECT_COLLECTION)]
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
                effects.AddRange(buff.getEffects());
            }
            return effects.ToArray();
        }
        /// <summary>
        /// 获取增益的所有效果
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetAllBuffEffects, EDITOR_CATEGORY_NAME_EFFECTS, EDITOR_NODE_NAME_GET_ALL_BUFF_EFFECTS)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EFFECT_COLLECTION)]
        public static IEffect[] getAllBuffEffects(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)] Buff buff)
        {
            if (buff == null)
                return Array.Empty<IEffect>();
            return buff.getEffects().ToArray();
        }
        /// <summary>
        /// 获取卡牌定义的所有效果
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetAllCardDefineEffects, EDITOR_CATEGORY_NAME_EFFECTS, EDITOR_NODE_NAME_GET_ALL_CARD_DEFINE_EFFECTS)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EFFECT_COLLECTION)]
        public static IEffect[] getAllCardDefineEffects(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            if (cardDefine == null)
                return Array.Empty<IEffect>();
            return cardDefine.getEffects().ToArray();
        }
        /// <summary>
        /// 获取效果合法目标集合
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.GetValidEffectTargets, EDITOR_CATEGORY_NAME_EFFECTS, EDITOR_NODE_GET_EFFECT_TARGETS)]
        [return: ActionNodeParam(EDITOR_NODE_GET_CARD_TARGETS_TARGETS)]
        public static Card[] getAllLegalEffectTargets(CardEngine game,
            [ActionNodeParam(EDITOR_PARAM_NAME_EFFECT)] IEffect effect,
            [ActionNodeParam(EDITOR_PARAM_NAME_SOURCE_CARD)] Card source)
        {
            if (effect == null || source == null)
                return Array.Empty<Card>();
            return effect.getAvaliableTargets(game, source);
        }
    }
}
