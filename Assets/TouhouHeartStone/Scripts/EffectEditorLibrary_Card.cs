using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using MukioI18n;

namespace Game
{
    partial class EffectEditorLibrary
    {
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ALL_CARD_DEFINE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine[] getAllCardDefines(CardEngine game)
        {
            return game.getDefines();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_CARD_DEFINE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getCardDefine(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_REFERENCE, true)] CardReference cardRef)
        {
            return game.getDefine(cardRef.cardId);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_CARD_DEFINE_ARRAY, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine[] getCardDefineArray(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_REFERENCE, true)] CardReference[] cardRefs)
        {
            if (cardRefs == null || cardRefs.Length < 1)
                return new CardDefine[0];
            CardDefine[] cardDefines = new CardDefine[cardRefs.Length];
            for (int i = 0; i < cardRefs.Length; i++)
            {
                cardDefines[i] = game.getDefine(cardRefs[i].cardId);
            }
            return cardDefines;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CARD_DEFINE_HAS_TAG, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool hasTag(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TAG)] string tag)
        {
            return cardDefine.hasTag(tag);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_RANDOM_CARD_DEFINE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static CardDefine getRandomCardDefine(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine[] cardDefines)
        {
            if (cardDefines == null || cardDefines.Length < 1)
                return null;
            if (cardDefines.Length == 1)
                return cardDefines[0];
            return cardDefines[game.randomInt(0, cardDefines.Length - 1)];
        }
        /// <summary>
        /// 卡牌类型判断节点
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <param name="select">选项</param>
        /// <returns>bool,是否属于该类型</returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE__IS_DEFINE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_TYPE_NAME_BOOL)]
        public static bool IsCardDefine(
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_CARD)] Card card, 
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_CARD_TYPE, isConst: true)] CardDefineSelect select)
        {
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
        /// 获取随机卡牌
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_RANDOM_CARD, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card getRandomCard(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            return cards.random(game);
        }
        /// <summary>
        /// 创建衍生物并置入战场
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_FIELD, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardToField(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            Player player = card.getOwner();
            var createTokenEvent = await player.createToken(game, cardDefine);
            return createTokenEvent?.card;
        }
        /// <summary>
        /// 创建衍生卡并置入手牌
        /// </summary>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_TO_HAND, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardToHand(CardEngine game, Card card, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            Player player = card.getOwner();
            if (player.getHand().isFull)
                return null;
            Card createdCard = game.createCard(cardDefine);
            await player.getHand().add(game, createdCard);
            return createdCard;
        }
        /// <summary>
        /// 创建衍生卡并洗入牌库
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CREATE_TOKEN_CARD_SHUFFLE_TO_DECK, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> createTokenCardShuffleToDeck(CardEngine game, Card card, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            Player player = card.owner;
            Card createdCard = game.createCard(cardDefine);

            await player.getDeck().insert(game, createdCard, game.randomInt(0, player.getDeck().count));
            return createdCard;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_THIS_CARD, StringTable.EDITOR_NODE_CATEGORY_CARD, "ThisCard")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card thisCard(Card card)
        {
            return card;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_PROPERTY, StringTable.EDITOR_NODE_CATEGORY_CARD, "GetProperty")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getProp(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        {
            return card.getProp(game, propName);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SET_PROPERTY, StringTable.EDITOR_NODE_CATEGORY_CARD, "SetProperty")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROP_CHANGE_EVENT)]
        public static Task<IPropChangeEventArg> setProp(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            return card.setProp(game, propName, value);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_COST, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getCost(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.getCost(game);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ATTACK, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getAttack(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.getAttack(game);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_MAX_LIFE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getMaxLife(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.getLife(game);
        }

        [TranslateMsg("动作图编辑器中动作节点名称之获取当前生命值")]
        public const string EDITOR_NODE_NAME_GET_CURRENT_LIFE = "获取当前生命值";
     [ActionNodeMethod(EDITOR_NODE_NAME_GET_CURRENT_LIFE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getLife(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.getCurrentLife(game);
        }
        /// <summary>
        /// 获取一个独一无二的属性名。当你在一段可以重复生效的效果中为卡牌设置额外属性的时候，应该使用这个方法。
        /// 注意，这个方法是为了避免属性名之间的冲突，在不会发生冲突的情况下，输入相同的参数仍然会让这个方法返回相同的属性名。
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="buff"></param>
        /// <param name="eventArg"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_UNIQUE_PROPERTY_NAME, StringTable.EDITOR_NODE_CATEGORY_CARD, "GetUniquePropertyName")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)]
        public static string getUniquePropName(IBuff buff, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        {
            if (buff != null)
                propName = buff.instanceID + propName;
            return propName;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_OWNER, StringTable.EDITOR_NODE_CATEGORY_CARD, "GetOwner")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)]
        public static Player getOwner([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.owner;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_SPELL_DAMAGE, StringTable.EDITOR_NODE_CATEGORY_PLAYER, "GetSpellDamage")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)]
        public static int getSpellDamage(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BASE_DAMAGE)] int baseDamage)
        {
            return player.getSpellDamage(game, baseDamage);
        }
        /// <summary>
        /// 这是不是一张法术牌
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_IS_SPELL, StringTable.EDITOR_NODE_CATEGORY_CARD, "IsSpell")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool isSpell([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.define.type == DEFINE.SPELL;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_IS_TOKEN, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool isToken([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine cardDefine)
        {
            return cardDefine.isToken();
        }
        /// <summary>
        /// 对目标造成固定的法术伤害
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="damage"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_TARGETS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg> dealSpecificSpellDamageToTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            return targets.damage(game, card, card.owner.getSpellDamage(game, damage));
        }
        /// <summary>
        /// 对随机敌人造成造成固定的法术伤害
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="damage"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_RANDOM_TARGETS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg[]> dealSpecificSpellDamageToRandomTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            return targets.damageByRandom(game, card, getSpellDamage(game, card.owner, damage));
        }
        /// <summary>
        /// 对固定数量的随机敌人造成固定伤害
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="count"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_DAMAGE_TO_SPECIFIC_NUMBER_OF_RANDOM_TARGET, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg> dealSpecificDamageToSpecificNumberOfRandomTarget(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            return targets.damageByRandomCount(game, card, damage, count);
        }
        /// <summary>
        /// 对固定数量的随机敌人造成固定法术伤害
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="count"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DEAL_SPECIFIC_SPELL_DAMAGE_TO_SPECIFIC_NUMBER_OF_RANDOM_TARGET, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE_EVENT)]
        public static Task<THHCard.DamageEventArg> dealSpecificSpellDamageToSpecificNumberOfRandomTarget(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE, isConst: true)] int damage,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            return targets.damageByRandomCount(game, card, card.owner.getSpellDamage(game, damage), count);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_HEAL_TARGETS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HEAL_EVENT)]
        public static Task<THHCard.HealEventArg> healTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            return targets.heal(game, card, value);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_HEAL_SPECIFIC_AMOUNT_TO_TARGETS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HEAL_EVENT)]
        public static Task<THHCard.HealEventArg> healSpecificAmountToTargets(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE, isConst: true)] int value)
        {
            return targets.heal(game, card, value);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ALL_SERVANTS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getAllServants(CardEngine game)
        {
            List<Card> cardList = new List<Card>();
            foreach (var player in game.players)
            {
                cardList.AddRange(player.getField());
            }
            return cardList.ToArray();
        }
        /// <summary>
        /// 获取敌方随从
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ENEMY_SERVANTS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getEnemyServants(CardEngine game, Card card)
        {
            return game.getOpponent(card.owner).getField();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_FRIEND_CHARACTERS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getFriendCharacters(Card card)
        {
            Card master = card.owner.getMaster();
            Card[] servants = card.owner.getField();
            Card[] characters = new Card[servants.Length + 1];
            characters[0] = master;
            Array.Copy(servants, 0, characters, 1, servants.Length);
            return characters;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ENEMY_CHARACTERS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getEnemyCharacters(CardEngine game, Card card)
        {
            Card master = game.getOpponent(card.owner).getMaster();
            Card[] servants = game.getOpponent(card.owner).getField();
            Card[] characters = new Card[servants.Length + 1];
            characters[0] = master;
            Array.Copy(servants, 0, characters, 1, servants.Length);
            return characters;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_ALL_CHARACTERS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getAllCharacters(CardEngine game)
        {
            List<Card> cardList = new List<Card>();
            foreach (var player in game.players)
            {
                cardList.Add(player.getMaster());
                cardList.AddRange(player.getField());
            }
            return cardList.ToArray();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_FREEZE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROP_CHANGE_EVENT)]
        public static Task<IPropChangeEventArg> freeze(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.setFreeze(game, true);
        }
        /// <summary>
        /// 沉默目标卡牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cards"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SILENCE, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_SILENCE_EVENT)]
        public static Task<THHCard.SilenceEventArg> silence(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            return cards.silence(game);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DESTROY, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DEATH_EVENT)]
        public static Task<THHCard.DeathEventArg> destroy(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            return cards.die(game);
        }

        /// <summary>
        /// 从墓地取一张卡牌至手牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="cardDefine"></param>
        /// <returns></returns>

        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_MOVE_CARD_FROM_GRAVE_TO_HAND, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        public static async Task moveCardFromGraveToHand(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            if (!player.getHand().isFull)
            {
                await player.getGrave().moveTo(game, cards, player.getHand());
            }
            else if ((player.getHand().maxCount - player.getHand().count) < cards.Length)
            {
                await player.getGrave().moveTo(game, cards.Take((player.getHand().maxCount - player.getHand().count)), player.getHand());
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 获取所有友方随从
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [TranslateMsg("动作图编辑器中动作节点名称之获取友方随从")]
        public const string EDITOR_NODE_NAME_GET_FRIENDLY_SERVANTS = "获取友方随从";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_FRIENDLY_SERVANTS, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getFriendlyServants(CardEngine game, Card card)
        {
            {
                Card[] servants = card.owner.getField();
                return servants;
            }
        }

        /// <summary>
        /// 增加护甲
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之增加固定数量护甲")]
        public const string EDITOR_NODE_ADD_AMOUNT_OF_ARMOR = "增加固定数量护甲";
        [ActionNodeMethod(EDITOR_NODE_ADD_AMOUNT_OF_ARMOR, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        public static async Task addArmorToTarget(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            await player.getMaster().gainArmor(game, count);
        }
        /// <summary>
        /// 增加固定数量的护甲
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之增加护甲")]
        public const string EDITOR_NODE_ADD_ARMOR = "增加护甲";
        [ActionNodeMethod(EDITOR_NODE_ADD_ARMOR, StringTable.EDITOR_NODE_CATEGORY_CARD)]
        public static async Task addAmountOfArmorToTarget(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
             [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            await player.getMaster().gainArmor(game, count);
        }

    }
    public enum CardDefineSelect
    {
        Spell,
        Servant,
        Item,
        Master,
        Skill
    }
}



 

