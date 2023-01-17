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
        public static Task branchAction(CardEngine game, Card card, Buff buff, EventArg eventArg, Scope scope,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)] bool isTrue,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ACTION, isOut: true)] ActionNode thenAction,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELSE_ACTION, isOut: true)] ActionNode elseAction)
        {
            Scope newScope = new Scope(scope);
            if (isTrue)
            {
                return thenAction != null ? game.doActionsAsync(card, buff, eventArg, thenAction, newScope) : Task.CompletedTask;
            }
            else
            {
                return elseAction != null ? game.doActionsAsync(card, buff, eventArg, elseAction, newScope) : Task.CompletedTask;
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
                scope.setOutParamValue(0, i);
                Scope newScope = new Scope(scope);
                await game.doActionsAsync(card, buff, eventArg, action, newScope);
            }
        }

        [TranslateMsg("动作图编辑器中动作节点名称之重复动作直到动作")]
        private const string EDIOTR_NODE_NAME_DO_UNTIL_ACTION = "重复动作直到";
        [TranslateMsg("动作图编辑器中动作节点参数名称之最多重复次数参数")]
        private const string EDIOTR_PARAM_NAME_MAX_REPEAT_TIME = "最多重复次数";
        [TranslateMsg("动作图编辑器中动作节点参数名称之结束条件参数")]
        private const string EDIOTR_PARAM_NAME_STOP_CONDITION = "结束条件";

        [ActionNodeMethod(EDIOTR_NODE_NAME_DO_UNTIL_ACTION, null)]
        public static async Task doUntilAction(CardEngine game, Card card, Buff buff, EventArg eventArg, Scope scope,
            [ActionNodeParam(EDIOTR_PARAM_NAME_STOP_CONDITION, isOut: false)] ActionNode condition,
            [ActionNodeParam(EDIOTR_PARAM_NAME_MAX_REPEAT_TIME)] int maxRepeatTime,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ACTION, isOut: true)] ActionNode action,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_REPEAT_TIME, isOut: true)] int repeatTime)
        {
            if (action == null)
                return;
            if (maxRepeatTime <= 0)
                return;
            int times = 0;
            do
            {
                times++;
                scope.setOutParamValue(0, times);
                Scope newScope = new Scope(scope);
                await game.doActionsAsync(card, buff, eventArg, action, newScope);
                if (condition != null)
                {
                    if (await game.getActionReturnValueAsync<bool>(card, buff, eventArg, condition, scope: newScope))
                    {
                        break;
                    }
                }
            }
            while (times < maxRepeatTime);
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
        /// <summary>
        /// 丢弃卡牌
        /// </summary>
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
                scope.setOutParamValue(0, cards[i]);
                Scope newScope = new Scope(scope);
                bool result = game.getActionReturnValueAsync<bool>(card, buff, eventArg, condition, scope: newScope).Result;
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
        //  与增益相关
        #region 增益
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_BUFF_DEFINE, StringTable.EDITOR_NODE_CATEGORY_BUFF, "GetBuffDefine")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF_DEFINE)]
        public static BuffDefine getBuffDefine(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ID, isConst: true)] DefineReference defRef)
        {
            return game.getBuffDefine(defRef.cardPoolId, defRef.defineId);
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
        /// 卡牌置入战场
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之卡牌置入战场")]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_FIELD = "卡牌置入战场";
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_MOVE_TO_FIELD, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> moveToField(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card[] cards)
        {
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                if (card.isServant())
                {
                    if (player.getField().isFull)
                    {
                        game.logger.log(player + "的战场已满，无法将" + string.Join(",", card) + "置入战场");
                    }
                    else
                    {
                        game.logger.log("将" + string.Join("，", card) + "置入" + player + "的战场");
                        //await card.pile.moveTo(game, card, player.getField());//直接移动卡牌似乎没有入场事件
                        if (card.pile?.name == PILE.GRAVE)//如果是墓地置入战场需要清除增益
                        {
                            await card.resetCard(game);
                        }
                        await player.tryEnterField(game, card.pile, card, player.getField().count);
                        MoveList.Add(card);
                    }
                }
                else
                {
                    game.logger.log("卡牌类型非法，无法将" + string.Join(",", card) + "置入战场");
                }
            }
            return MoveList.ToArray();
        }
        /// <summary>
        /// 卡牌移回手牌
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之卡牌移回手牌")]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_HAND = "卡牌移回手牌";
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_MOVE_TO_HAND, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> moveToHand(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card[] cards)
        {
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                int restCount = player.getHand().maxCount - player.getHand().count;
                if (restCount > 0)
                {
                    game.logger.log("将" + string.Join("，", card) + "置入" + player + "的手牌");
                    await card.resetCard(game);
                    await card.pile.moveTo(game, card, player.getHand());
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
        /// 卡牌从洗入牌库
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之卡牌洗入牌库")]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_DECK = "卡牌洗入牌库";
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_MOVE_TO_DECK, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> moveToDeck(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_NODE_NAME_IS_DECK_TOP, isConst: true)] bool top)
        {
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                int position = top ? player.getDeck().count : game.randomInt(0, player.getDeck().count);
                game.logger.log("将" + string.Join("，", card) + "洗入" + player + "的牌库");
                await card.resetCard(game);
                await card.pile.moveTo(game, card, player.getDeck(), position);
                MoveList.Add(card);
            }
            return MoveList.ToArray();
        }
        /// <summary>
        /// 卡牌从置入墓地
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之卡牌置入墓地")]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_GRAVE = "卡牌置入墓地";
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_MOVE_TO_GRAVE, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> moveToGrave(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card[] cards)
        {
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                game.logger.log("将" + string.Join("，", card) + "置入" + player + "的墓地");
                await card.resetCard(game);
                await card.pile.moveTo(game, card, player.getGrave());
                MoveList.Add(card);
            }
            return MoveList.ToArray();
        }
        /// <summary>
        /// 卡牌置入延迟区
        /// </summary>
        [TranslateMsg("动作图编辑器中动作节点名称之卡牌置入延迟区")]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_HEAD = "卡牌置入延迟区";
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_MOVE_TO_HEAD, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> moveToHead(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card[] cards)
        {
            Card[] movelist;
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                if (card.hasKeyword(game, KEYWORD.SECRET) || card.hasKeyword(game, KEYWORD.MISSION))
                {
                    if (player.getHead().isFull)
                    {
                        game.logger.log(player + "的延迟区已满，无法将" + string.Join(",", card) + "置入延迟区");
                    }
                    else
                    {
                        game.logger.log("将" + string.Join("，", card) + "置入" + player + "的延迟区");
                        await card.pile.moveTo(game, card, player.getHead());
                        MoveList.Add(card);
                    }
                }
                else
                {
                    game.logger.log("卡牌类型非法，无法将" + string.Join(",", card) + "置入延迟区");
                }
            }
            return MoveList.ToArray();
        }
        /// <summary>
        /// 卡牌装备到道具栏(有显示问题）
        /// </summary>
        [TranslateMsg]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_ITEM = "卡牌装备到道具栏";
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_MOVE_TO_ITEM, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card> moveToItem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card card)
        {
            if (card.isItem())
            {
                if (player[PILE.ITEM].isFull)
                {
                    game.logger.logTrace("由于" + player + "的道具栏已满，摧毁当前道具");
                    await player.destroyItem(game);
                }
                game.logger.log("将" + string.Join("，", card) + "装备到" + player + "的道具栏");
                await card.setCurrentLife(game, card.getLife(game));
                await card.pile.moveTo(game, card, player[PILE.ITEM]);
                return card;
            }
            else
            {
                game.logger.log("卡牌类型非法，无法将" + string.Join(",", card) + "置入道具栏");
                return null;
            }
        }
        /// <summary>
        /// 卡牌移动到暂存区(有显示问题）
        /// </summary>
        [TranslateMsg]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_WARP = "卡牌移动到暂存区";
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_MOVE_TO_WARP, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> moveToWARP(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card[] cards)
        {
            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                game.logger.log("将" + string.Join("，", card) + "置入" + player + "的暂存区");
                //await card.resetCard(game);
                await card.pile.moveTo(game, card, player.getWarp());
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
        /// <summary>
        /// 筛选
        /// </summary>
        [TranslateMsg("动作图编辑器中参数节点名称之匹配的集合")]
        public const string EDITOR_PARAM_NAME_ARRAY_MATCHED = "匹配的集合";
        [TranslateMsg("动作图编辑器中参数节点名称之集合")]
        public const string EDITOR_PARAM_NAME_ARRAY_MISMATCHED = "不匹配的集合";
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_WHERE, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_ARRAY_MATCHED)]
        public static Array where(CardEngine game, Card card, Buff buff, EventArg eventArg, Scope scope,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] Array array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION)] ActionNode condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            List<object> objList = new List<object>();
            List<object> negList = new List<object>();
            for (int i = 0; i < array.Length; i++)
            {
                object obj = array.GetValue(i);
                scope.setOutParamValue(0, obj);
                Scope newScope = new Scope(scope);
                bool result = game.getActionReturnValueAsync<bool>(card, buff, eventArg, condition, scope: newScope).Result;
                if (result)
                {
                    objList.Add(obj);
                }
                else
                {
                    negList.Add(obj);
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

        [TranslateMsg("编辑器_动作节点_排序")]
        public const string EDITOR_NODE_NAME_ORDER_BY = "排序";
        [TranslateMsg("编辑器_参数名称_降序")]
        public const string EDITOR_NODE_NAME_DESCENDING = "降序";
        [TranslateMsg("编辑器_参数名称_排序依据")]
        public const string EDITOR_NODE_NAME_ORDER_KEY = "排序依据";
        /// <summary>
        /// 根据排序依据排序。
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_ORDER_BY, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array orderBy(CardEngine game, Card card, Buff buff, EventArg eventArg, Scope scope,
            [ActionNodeParam(EDITOR_NODE_NAME_DESCENDING, isConst: true)] bool descending,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(EDITOR_NODE_NAME_ORDER_KEY)] ActionNode condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            object func(object obj)
            {
                scope.setOutParamValue(0, obj);
                Scope newScope = new Scope(scope);
                return game.getActionReturnValueAsync<object>(card, buff, eventArg, condition, scope: newScope).Result;
            }
            if (!descending)
            {
                return array.OrderBy(func).ToArray();
            }
            else
            {
                return array.OrderByDescending(func).ToArray();
            }
        }

        [TranslateMsg("编辑器_动作节点_获取第X个元素")]
        public const string EDITOR_NODE_NAME_GET_ELEMENT_BY_INDEX = "获取第X个元素";
        [TranslateMsg("编辑器_参数名称_元素位置")]
        public const string EDITOR_PARAM_NAME_INDEX = "元素位置";
        /// <summary>
        /// 获取第X个元素。
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_ELEMENT_BY_INDEX, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT)]
        public static object getElementByIndex(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(EDITOR_PARAM_NAME_INDEX)] int index)
        {
            return array[index];
        }
        [TranslateMsg("编辑器_动作节点_遍历")]
        public const string EDITOR_NODE_NAME_FOREACH = "遍历";
        /// <summary>
        /// 遍历集合
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_FOREACH, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        public static async Task foreachAction(CardEngine game, Card card, Buff buff, EventArg eventArg, Scope scope,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ACTION)] ActionNode action,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (action == null)
                return;
            foreach (object ele in array)
            {
                scope.setOutParamValue(0, ele);
                Scope newScope = new Scope(scope);
                await game.doActionsAsync(card, buff, eventArg, action, newScope);
            }
        }
        public const string EDITOR_NODE_NAME_GET_RANDOM_ELEMENT = "获取集合中的随机元素";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_RANDOM_ELEMENT, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT)]
        public static object getRandomElement(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] collection)
        {
            return collection[game.randomInt(0, collection.Length - 1)];
        }
        #endregion

        //  事件处理
        [TranslateMsg("编辑器/事件_集合名称")]
        public const string EDITOR_NODE_EVENT = "事件";
        #region 事件
        [TranslateMsg("编辑器/事件_当前事件_节点名称")]
        public const string EDITOR_NODE_EVENT_THIS_EVENT = "当前事件";
        [ActionNodeMethod(EDITOR_NODE_EVENT_THIS_EVENT, EDITOR_NODE_EVENT)]
        [return: ActionNodeParam(EDITOR_NODE_EVENT)]
        public static EventArg thisEvent(EventArg eventArg)
        {
            return eventArg;
        }
        [TranslateMsg("编辑器/事件_阻止事件_节点名称")]
        public const string EDITOR_NODE_EVENT_STOP_EVENT = "阻止事件";
        [ActionNodeMethod(EDITOR_NODE_EVENT_STOP_EVENT, EDITOR_NODE_EVENT)]
        public static void stopEvent([ActionNodeParam(EDITOR_NODE_EVENT)] EventArg eventArg)
        {
            eventArg.isCanceled = true;
        }

        [TranslateMsg("编辑器/事件_更改攻击目标_节点名称")]
        public const string EDITOR_NODE_EVENT_CHANGE_ATTACK_TARGET = "更改攻击目标";
        [ActionNodeMethod(EDITOR_NODE_EVENT_CHANGE_ATTACK_TARGET, EDITOR_NODE_EVENT)]
        public static void changeAttackTarget(
            [ActionNodeParam(EDITOR_NODE_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)] Card target)
        {
            if (eventArg is THHCard.AttackEventArg attackArg)
            {
                attackArg.target = target;
            }
        }
        [TranslateMsg("编辑器/事件_更改使用目标_节点名称")]
        public const string EDITOR_NODE_EVENT_CHANGE_USE_TARGET = "更改使用目标";
        [ActionNodeMethod(EDITOR_NODE_EVENT_CHANGE_USE_TARGET, EDITOR_NODE_EVENT)]
        public static void changeUseTargets(
            [ActionNodeParam(EDITOR_NODE_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] Card[] target)
        {
            if (eventArg.getVar<Card[]>(HearthStoneRule.UseEventArg.VAR_TARGETS).Length > 0)
                eventArg.setVar(HearthStoneRule.UseEventArg.VAR_TARGETS, target);
        }
        #endregion
    }
}
