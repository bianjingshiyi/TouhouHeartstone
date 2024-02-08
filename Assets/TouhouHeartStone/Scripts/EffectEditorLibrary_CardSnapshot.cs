using System;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    partial class EffectEditorLibrary
    {
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_GET_CARD_SNAPSHOT_CARD = "获取卡牌快照对应卡牌";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_GET_CARD_SNAPSHOT_CARD_DEFINE = "获取卡牌快照卡牌定义";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NAME_NODE_IS_CARD_SNAPSHOT_TYPE = "卡牌快照类型判断";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_CARD_SNAPSHOT_HAS_KEYWORDS = "卡牌快照关键词判断";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_PROPERTY = "获取卡牌快照属性";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_COST = "获取卡牌快照花费";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_ATTACK = "获取卡牌快照攻击力";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_MAX_LIFE = "获取卡牌快照最大生命值";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_CURRENT_LIFE = "获取卡牌快照当前生命值";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_OWNER = "获取卡牌快照拥有者";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_ARMOR = "获取卡牌快照护甲值";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_CARD_SNAPSHOT_HAS_BATTLECRY = "卡牌快照具有宣言";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_CARD_SNAPSHOT_HAS_DEATHRATTLE = "卡牌快照具有遗言";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_CARD_SNAPSHOT_IS_TRAP = "卡牌快照是否是陷阱";
        
        [TranslateMsg("编辑器/卡牌快照-节点名称")]
        public const string EDITOR_NODE_CARD_SNAPSHOT_IS_CHARGED = "卡牌快照是否已蓄力";
        [TranslateMsg("编辑器/卡牌快照-节点名称")]
        public const string EDITOR_NODE_CARD_SNAPSHOT_GET_CHARGED_LEVEL = "获取卡牌快照蓄力层数";
        /// <summary>
        /// 获取卡牌快照对应卡牌
        /// </summary>
        /// <param name="card">卡牌快照</param>
        /// <returns>bool,对应卡牌</returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardOfCardSnapshot, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_NAME_GET_CARD_SNAPSHOT_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card getCardOfCardSnapshot(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return null;
            return card.card;
        }
        /// <summary>
        /// 获取卡牌快照卡牌定义
        /// </summary>
        /// <param name="card">卡牌快照</param>
        /// <returns>卡牌定义</returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardDefineOfCardSnapshot, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_NAME_GET_CARD_SNAPSHOT_CARD_DEFINE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getCardDefineOfCardSnapshot(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return null;
            return card.define;
        }
        /// <summary>
        /// 卡牌快照类型判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.CheckCardSnapshotType_Obs_CardType, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NAME_NODE_IS_CARD_SNAPSHOT_TYPE)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool IsTypeOfCardSnapshotObsolete(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_TYPE, isConst: true)] CardDefineSelect select)
        {
            if (card == null)
                return false;
            switch (select)
            {
                case CardDefineSelect.Spell:
                    return card.isSpell();
                case CardDefineSelect.Servant:
                    return card.isServant();
                case CardDefineSelect.Master:
                    return card.isMaster();
                case CardDefineSelect.Skill:
                    return card.isSkill();
                default:
                    return card.isItem();
            }
        }
        /// <summary>
        /// 卡牌快照类型判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="type">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.CheckCardSnapshotType, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NAME_NODE_IS_CARD_SNAPSHOT_TYPE)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool IsTypeOfCardSnapshot(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_TYPE)] CardType type)
        {
            if (card == null || type == null)
                return false;
            return card.define.type == type.value;
        }
        /// <summary>
        /// 卡牌快照关键词判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.CardSnapshotHasKeywords, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_NAME_CARD_SNAPSHOT_HAS_KEYWORDS)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool CardSnapshotHasKeywords(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_KEYWORD)] string select)
        {
            if (card == null)
                return false;
            return card.hasKeyword(game, select);
        }
        /// <summary>
        /// 获取卡牌快照属性
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardSnapshotProp, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_PROPERTY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getCardSnapshotProp(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        {
            return card?.getProp(game, propName);
        }
        /// <summary>
        /// 获取卡牌快照花费
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardSnapshotCost, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_COST)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardSnapshotCost(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return 0;
            return card.getCost(game);
        }
        /// <summary>
        /// 获取卡牌快照攻击力
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardSnapshotAttack, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_ATTACK)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardSnapshotAttack(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return 0;
            return card.getAttack(game);
        }
        /// <summary>
        /// 获取卡牌快照最大生命值
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardSnapshotMaxLife, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_MAX_LIFE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardSnapshotMaxLife(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return 0;
            return card.getLife(game);
        }
        /// <summary>
        /// 获取卡牌快照当前生命值
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardSnapshotLife, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_CURRENT_LIFE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardSnapshotLife(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return 0;
            return card.getCurrentLife(game);
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardSnapshotOwner, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_OWNER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)]
        public static Player getCardSnapshotOwner([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card)
        {
            return card?.owner;
        }
        /// <summary>
        /// 获取卡牌快照护甲
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardSnapshotArmor, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_ARMOR)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardSnapshotArmor(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return 0;
            return card.getArmor(game);
        }

        /// <summary>
        /// 是否已蓄力
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.IsCardSnapshotCharged, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_CARD_SNAPSHOT_IS_CHARGED)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_IS_CHARGED)]
        public static bool isCardSnapshotCharged(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return false;
            return card.isCharged(game);
        }

        /// <summary>
        /// 获取蓄力层数
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardSnapshotChargedLevel, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_CARD_SNAPSHOT_GET_CHARGED_LEVEL)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_CHARGED_LEVEL)]
        public static int getCardSnapshotChargedLevel(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return 0;
            return card.getChargedLevel(game);
        }
        /// <summary>
        /// 卡牌快照具有宣言
        /// </summary>
        /// <param name="card">卡牌快照</param>
        /// <returns>卡牌定义</returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.DoesCardSnapshotHaveBattlecry, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_NAME_CARD_SNAPSHOT_HAS_BATTLECRY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool cardSnapshotHasBattlecry(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return false;
            return card.hasBattlecry(game);
        }
        /// <summary>
        /// 卡牌快照具有遗言
        /// </summary>
        /// <param name="card">卡牌快照</param>
        /// <returns>卡牌定义</returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.DoesCardSnapshotHaveDeathrattle, StringTable.EDITOR_NODE_CATEGORY_CARD_SNAPSHOT, EDITOR_NODE_NAME_CARD_SNAPSHOT_HAS_DEATHRATTLE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool cardSnapshotHasDeathrattle(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return false;
            return card.hasDeathRattle(game);
        }
        /// <summary>
        /// 卡牌快照是否是陷阱
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.CardSnapshotIsTrap, StringTable.EDITOR_NODE_CATEGORY_CARD, EDITOR_NODE_NAME_CARD_SNAPSHOT_IS_TRAP)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool cardSnapshotIsTrap(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_SNAPSHOT)] CardSnapshot cardSnapshot)
        {
            if (cardSnapshot == null)
                return false;
            return cardSnapshot.isTrap(game);
        }
    }
}




