using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game
{
    partial class EffectEditorLibrary
    {
        [TranslateMsg("变量名称")]
        public const string EDITOR_VAR_TYPE_CARD_SNAPSHOT = "卡牌快照";
        [TranslateMsg("节点分类名称")]
        public const string EDITOR_NODE_CATEGORY_CARD_SNAPSHOT = "卡牌快照";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_IS_CARD_SNAPSHOT_TYPE = "卡牌快照类型判断";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_CARD_SNAPSHOT_HAS_KEYWORDS = "卡牌快照关键词判断";
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
        [TranslateMsg("编辑器/卡牌快照-节点名称")]
        public const string EDITOR_NODE_CARD_SNAPSHOT_IS_CHARGED = "卡牌快照是否已蓄力";
        [TranslateMsg("编辑器/卡牌快照-节点名称")]
        public const string EDITOR_NODE_CARD_SNAPSHOT_GET_CHARGED_LEVEL = "获取卡牌快照蓄力层数";
        /// <summary>
        /// 卡牌快照类型判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [ActionNodeMethod(EDITOR_NODE_IS_CARD_SNAPSHOT_TYPE, EDITOR_NODE_CATEGORY_CARD_SNAPSHOT)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool IsTypeOfCardSnapshot(
            [ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)] CardSnapshot card,
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_CARD_TYPE, isConst: true)] CardDefineSelect select)
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
        /// 卡牌快照关键词判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [ActionNodeMethod(EDITOR_NODE_CARD_SNAPSHOT_HAS_KEYWORDS, EDITOR_NODE_CATEGORY_CARD_SNAPSHOT)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool CardSnapshotHasKeywords(CardEngine game,
            [ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)] CardSnapshot card,
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_KEYWORDS_TYPE, isConst: true)] string select)
        {
            if (card == null)
                return false;
            return card.hasKeyword(game, select);
        }
        /// <summary>
        /// 获取卡牌快照属性
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_PROPERTY, EDITOR_NODE_CATEGORY_CARD_SNAPSHOT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getCardSnapshotProp(
            [ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)] CardSnapshot card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        {
            return card?.getProp(propName);
        }
        /// <summary>
        /// 获取卡牌快照花费
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_COST, EDITOR_NODE_CATEGORY_CARD_SNAPSHOT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCardSnapshotCost(CardEngine game,
            [ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return 0;
            return card.getCost(game);
        }
        /// <summary>
        /// 获取卡牌快照攻击力
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_ATTACK, EDITOR_NODE_CATEGORY_CARD_SNAPSHOT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getAttack(CardEngine game,
            [ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return 0;
            return card.getAttack(game);
        }
        /// <summary>
        /// 获取卡牌快照最大生命值
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_MAX_LIFE, EDITOR_NODE_CATEGORY_CARD_SNAPSHOT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getMaxLife(CardEngine game,
            [ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return 0;
            return card.getLife(game);
        }
        /// <summary>
        /// 获取卡牌快照当前生命值
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_CURRENT_LIFE, EDITOR_NODE_CATEGORY_CARD_SNAPSHOT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getLife(CardEngine game,
           [ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return 0;
            return card.getCurrentLife(game);
        }

        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_OWNER, EDITOR_NODE_CATEGORY_CARD_SNAPSHOT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)]
        public static Player getOwner([ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)] CardSnapshot card)
        {
            return card?.owner;
        }
        /// <summary>
        /// 获取卡牌快照护甲
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_SNAPSHOT_GET_ARMOR, EDITOR_NODE_CATEGORY_CARD_SNAPSHOT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getArmor(CardEngine game,
           [ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return 0;
            return card.getArmor(game);
        }

        /// <summary>
        /// 是否已蓄力
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_CARD_SNAPSHOT_IS_CHARGED, EDITOR_NODE_CATEGORY_CARD_SNAPSHOT)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_IS_CHARGED)]
        public static bool isCardSnapshotCharged(CardEngine game,
            [ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return false;
            return card.isCharged(game);
        }

        /// <summary>
        /// 获取蓄力层数
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_CARD_SNAPSHOT_GET_CHARGED_LEVEL, EDITOR_NODE_CATEGORY_CARD_SNAPSHOT)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_CHARGED_LEVEL)]
        public static int getCardSnapshotChargedLevel(CardEngine game,
            [ActionNodeParam(EDITOR_VAR_TYPE_CARD_SNAPSHOT)] CardSnapshot card)
        {
            if (card == null)
                return 0;
            return card.getChargedLevel(game);
        }
    }
}





