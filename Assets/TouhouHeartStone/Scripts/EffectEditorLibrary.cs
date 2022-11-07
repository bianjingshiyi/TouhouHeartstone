using MukioI18n;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;


namespace Game
{

    public static partial class EffectEditorLibrary
    {
        [ActionNodeMethod(StringTable.EDIOTR_NODE_NAME_BRANCH_ACTION, null)]
        public static Task branchAction(CardEngine game, Card card, Buff buff, EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)] bool isTrue,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ACTION, isOut: true)] ActionNode thenAction,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELSE_ACTION, isOut: true)] ActionNode elseAction)
        {
            if (isTrue)
            {
                return thenAction != null ? game.doActionsAsync(card, buff, eventArg, thenAction) : Task.CompletedTask;
            }
            else
            {
                return elseAction != null ? game.doActionsAsync(card, buff, eventArg, elseAction) : Task.CompletedTask;
            }
        }
        [ActionNodeMethod(StringTable.EDIOTR_NODE_NAME_FOR_ACTION, null)]
        public static async Task forAction(CardEngine game, Card card, Buff buff, EventArg eventArg, Scope scope,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ACTION, isOut: true)] ActionNode action,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_REPEAT_TIME, isOut: true)] int repeatTime)
        {
            if (action == null)
                return;
            for (int i = 1; i <= count; i++)
            {
                scope.setOutParamValue(1, i);
                await game.doActionsAsync(card, buff, eventArg, action);
            }
        }
        //  把这个“玩家”逐步移到下面几个去
        #region 玩家
        /// <summary>
        /// 获取玩家属性
        /// </summary>
        /// <param name="player"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_PLAYER_PROP, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getPlayerProp(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME, isConst: true)] string propName)
        {
            return player.getProp(propName);
        }
        /// <summary>
        /// 设置玩家属性
        /// </summary>
        /// <param name="player"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SET_PLAYER_PROP, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static void setPlayerProp(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME, isConst: true)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            player.setProp(propName, value);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_PLAYER_OPPONENT, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_OPPONENT_PLAYER)]
        public static Player getPlayerOpponent(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return game.getOpponent(player);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_MASTER, StringTable.EDITOR_NODE_CATEGORY_PLAYER, "GetMaster")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card getMaster([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player.getMaster();
        }
        /// <summary>
        /// 获取玩家手牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_PLAYER_HAND_CARDS, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getHandCards([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player.getHand();
        }
        /// <summary>
        /// 获取玩家墓地中的卡牌
        /// </summary>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_PLAYER_GRAVE_CARDS, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getPlayerGraveCards([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player.getGrave();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_PLAYER_DECK_CARDS, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getPlayerDeckCards([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player.getDeck();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DRAW_TARGET_CARD, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static Task drawTargetCard(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card targetCard,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DRAW_EVENT)] out HearthStoneRule.DrawEventArg drawEvent,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BURN_EVENT)] out HearthStoneRule.BurnEventArg burnEvent)
        {
            return card.owner.draw(game, targetCard, out drawEvent, out burnEvent);
        }
        ///从一定数量卡牌中选一张
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SELECT_ONE_FROM_MANY_CARDS, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Task<Card> selectOneFromManyCards(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count)
        {
            return game.askSelect(card.owner, cards, count);
        }
        ///从一定数量卡牌定义中选一张
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SELECT_ONE_FROM_MANY_DEFINES, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)]
        public static Task<CardDefine> selectOneFromManyDefines(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD_DEFINE)] CardDefine[] defines,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count/*,
            [ActionNodeParam(StringTable.EDITOR_TYPE_NAME_STRING, isConst: true)] string title = "选择一项"*/)
        {
            return game.askSelect(card.owner, defines, count);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DISCARD, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DRAW_EVENT)]
        public static Task<HearthStoneRule.DiscardEventArg> discard(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HANDS)] Card[] hands)
        {
            return card.owner.discard(game, hands);
        }
        #endregion

        //  有关全局的操作，比如说获取发现池，获取卡牌
        #region 全局

        #endregion

        //  有关日志的一些操作，比如说获取出牌记录，获取事件发生次数 
        #region 日志

        #endregion

        //  有关牌堆的操作，比如说洗牌，获取特定牌等等
        #region 牌堆
        /// <summary>
        /// 获取牌堆中的卡牌
        /// </summary>
        /// <param name="pile"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_CARDS_IN_PILE, StringTable.EDITOR_NODE_CATEGORY_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getCardsInPile([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE)] Pile pile)
        {
            return pile;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_CARDS_IN_DECK_WITH_CONDITION, StringTable.EDITOR_NODE_CATEGORY_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> getCardsInDeckWithCondition(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION, isConst: true)] ActionValueRef condition)
        {
            Pile deck = card.owner.getDeck();
            List<Card> cardList = new List<Card>();
            for (int i = 0; i < deck.count; i++)
            {
                bool result = await game.getActionReturnValueAsync<bool>(deck[i], null, null, condition.action, condition.index);
                if (result)
                {
                    cardList.Add(deck[i]);
                }
            }
            return cardList.ToArray();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_FILTER_CARDS_WITH_CONDITION, StringTable.EDITOR_NODE_CATEGORY_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getCardsWithCondition(CardEngine game, Card card, Buff buff, EventArg eventArg, Scope scope,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION)] ActionNode condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_FILTERING_CARD, isOut: true)] Card filteringCard)
        {
            List<Card> cardList = new List<Card>();
            for (int i = 0; i < cards.Length; i++)
            {
                scope.localVarDict[scope.getLocalVarName(0)] = cards[i];
                bool result = game.getActionReturnValueAsync<bool>(card, buff, eventArg, condition, scope: new Scope(scope)).Result;
                if (result)
                {
                    cardList.Add(cards[i]);
                }
            }
            return cardList.ToArray();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_CARDS_COUNT, StringTable.EDITOR_NODE_CATEGORY_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)]
        public static int getCardsCount([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            return cards.Length;
        }
        #endregion

        #region 增益
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_BUFF_DEFINE, StringTable.EDITOR_NODE_CATEGORY_BUFF, "GetBuffDefine")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF_DEFINE)]
        public static BuffDefine getBuffDefine(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ID, isConst: true)] int id)
        {
            return game.getBuffDefine(id);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_THIS_BUFF, StringTable.EDITOR_NODE_CATEGORY_BUFF, "ThisBuff")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)]
        public static Buff thisBuff(Buff buff)
        {
            return buff;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_ADD_BUFF, StringTable.EDITOR_NODE_CATEGORY_BUFF, "AddBuff")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)]
        public static async Task<Buff[]> addBuff(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF_DEFINE)] BuffDefine buffDefine)
        {
            List<Buff> buffList = new List<Buff>();
            foreach (var card in cards)
            {
                Buff buff = new GeneratedBuff(buffDefine);
                buffList.Add(buff);
                await card.addBuff(game, buff);
            }
            return buffList.ToArray();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_REMOVE_BUFF, StringTable.EDITOR_NODE_CATEGORY_BUFF, "RemoveBuff")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_IS_BUFF_REMOVED)]
        public static Task<bool> removeBuff(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)] Buff buff)
        {
            return card.removeBuff(game, buff);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_HAS_BUFF, StringTable.EDITOR_NODE_CATEGORY_BUFF)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool hasBuff(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF_DEFINE)] BuffDefine buffDefine)
        {
            return card.hasBuff(buffDefine);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SET_BUFF_PROP, StringTable.EDITOR_NODE_CATEGORY_BUFF)]
        public static async Task setBuffProp(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)] Buff[] buffs,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME, isConst: true)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            for (int i = 0; i < buffs.Length; i++)
            {
                await buffs[i].setProp(game, propName, value);
            }
        }
        #endregion
        //  玩家的行动，比如抽一张牌，获得一张牌
        #region 行动
        /// <summary>
        /// 简单抽牌
        /// 抽牌行动的简单实现，抽一张牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="player"></param>
        /// <param name="drawEvent"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE__SIMPLE_DRAW_CARD__NAME, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_NODE__SIMPLE_DRAW_CARD__DRAW_EVENT)]
        public static Card simpleDrawCard(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_NODE__SIMPLE_DRAW_CARD__PLAYER)] Player player)
        {
            player.draw(game, out var drawEvent);
            return drawEvent?.card;
        }
        /// <summary>
        /// 战场卡牌移回手牌
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_FIELD_TO_HAND, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> fieldToHand(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card[] targets)
        {
            await targets.fieldToHand(game);
            return targets;
        }
        /// <summary>
        /// 牌库仆从置入战场
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_DECK_SEVANTS_TO_FIELD, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> deckServantsToField(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            Card[] movelist;
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                var moveEvent = await player.tryEnterField(game, card.getOwner().getDeck(), card, 1);
                MoveList.Add(moveEvent.card);
            }
            movelist = MoveList.ToArray();
            return movelist;
        }
        /// <summary>
        /// 手牌仆从置入战场
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_HAND_SEVANTS_TO_FIELD, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> handServantsToField(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            Card[] movelist;
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                var moveEvent = await player.tryEnterField(game, card.getOwner().getHand(), card, 1);
                MoveList.Add(moveEvent.card);
            }
            movelist = MoveList.ToArray();
            return movelist;
        }
        /// <summary>
        /// 墓地仆从置入战场
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_GRAVE_SEVANTS_TO_FIELD, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> graveServantsToField(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            Card[] movelist;
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                var moveEvent = await player.tryEnterField(game, card.getOwner().getGrave(), card, 1);
                MoveList.Add(moveEvent.card);
            }
            movelist = MoveList.ToArray();
            return movelist;
        }
        /// <summary>
        /// 墓地卡牌置入手牌
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CARD_GRAVE_TO_HAND, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> graveToHand(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                int restCount = player.getHand().maxCount - player.getHand().count;
                if (restCount > 0)
                {
                    game.logger.log("将" + string.Join("，", card) + "置入" + player + "的手牌");
                    await card.getOwner().getGrave().moveTo(game, card, player.getHand());
                    MoveList.Add(card);
                }
                else
                {
                    game.logger.log(player + "的手牌已满，无法将" + string.Join(",", card) + "置入手牌");
                }
            }
            return MoveList.ToArray();
        }
        /// <summary>
        /// 墓地卡牌洗入牌库
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CARD_GRAVE_TO_DECK, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> graveToDeck(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_NODE_NAME_IS_DECK_TOP, isConst: true)] bool top)
        {
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                int position = top ? player.getDeck().count : game.randomInt(0, player.getDeck().count);
                game.logger.log("将" + string.Join("，", card) + "洗入" + player + "的牌库");
                await card.getOwner().getGrave().moveTo(game, card, player.getDeck(), position);
                MoveList.Add(card);
            }
            return MoveList.ToArray();
        }
        /// <summary>
        /// 手牌洗入牌库
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CARD_HAND_TO_DECK, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> handToDeck(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_NODE_NAME_IS_DECK_TOP, isConst: true)] bool top)
        {
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                int position = top ? player.getDeck().count : game.randomInt(0, player.getDeck().count);
                game.logger.log("将" + string.Join("，", card) + "洗入" + player + "的牌库");
                await card.getOwner().getHand().moveTo(game, card, player.getDeck(), position);
                MoveList.Add(card);
            }
            return MoveList.ToArray();
        }
        /// <summary>
        /// 战场仆从洗入牌库
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CARD_FIELD_TO_DECK, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> fieldToDeck(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_NODE_NAME_IS_DECK_TOP, isConst: true)] bool top)
        {
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                int position = top ? player.getField().count : game.randomInt(0, player.getDeck().count);
                game.logger.log("将" + string.Join("，", card) + "洗入" + player + "的牌库");
                await card.getOwner().getHand().moveTo(game, card, player.getDeck(), position);
                MoveList.Add(card);
            }
            return MoveList.ToArray();
        }
        #endregion
        #region 集合运算
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_CREATE_ARRAY, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)]
        public static Array createArray([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isParams: true)] object[] elements)
        {
            return elements;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_ADD_ELEMENT_TO_ARRAY, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)]
        public static Array addElementToArray(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] Array array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isParams: true)] object[] elements)
        {
            object[] newArray = new object[array.Length + elements.Length];
            Array.Copy(array, 0, newArray, 0, array.Length);
            Array.Copy(elements, 0, newArray, array.Length, elements.Length);
            return newArray;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_WHERE, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)]
        public static Array where(CardEngine game, Card card, Buff buff, EventArg eventArg, Scope scope,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] Array array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION)] ActionNode condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            List<object> objList = new List<object>();
            for (int i = 0; i < array.Length; i++)
            {
                object obj = array.GetValue(i);
                scope.setOutParamValue(0, obj);
                Scope childScope = new Scope() { parentScope = scope };
                bool result = game.getActionReturnValueAsync<bool>(card, buff, eventArg, condition, scope: childScope).Result;
                if (result)
                {
                    objList.Add(obj);
                }
            }
            return objList.ToArray();
        }
        /// <summary>
        /// 计算集合中元素数量
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_COUNT, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)]
        public static int getCount([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] collection)
        {
            return collection.Length;
        }
        #endregion

        //  事件处理
        [TranslateMsg("编辑器/事件_集合名称")]
        public const string EDITOR_NODE_EVENT = "事件";

        #region 事件
        [TranslateMsg("编辑器/事件_阻止事件_节点名称")]
        public const string EDITOR_NODE_EVENT_STOP_EVENT = "阻止事件";
        [ActionNodeMethod(EDITOR_NODE_EVENT_STOP_EVENT, EDITOR_NODE_EVENT)]
        public static void stopEvent([ActionNodeParam(EDITOR_NODE_EVENT, isParams: true)] EventArg eventArg)
        {
            eventArg.isCanceled = true;
        }
        #endregion
    }
}
