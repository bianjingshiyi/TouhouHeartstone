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
        public static async Task branchAction(CardEngine game, Card card, Buff buff, EventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)] bool isTrue,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ACTION, isOut: true)] ControlInput thenAction,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELSE_ACTION, isOut: true)] ControlInput elseAction)
        {
            var port = isTrue ? thenAction : elseAction;
            if (port == null)
                return;
            flow.EnterScope();
            await game.runActions(flow, port);
            flow.ExitScope();
        }
        [ActionNodeMethod(StringTable.EDIOTR_NODE_NAME_FOR_ACTION, null)]
        public static async Task forAction(CardEngine game, Card card, Buff buff, EventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ACTION, isOut: true)] ControlInput action,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_REPEAT_TIME, isOut: true)] int repeatTime)
        {
            if (action == null)
                return;
            var repeatTimePort = node.getOutputPort<ValueOutput>(nameof(repeatTime));
            for (int i = 1; i <= count; i++)
            {
                flow.EnterScope();
                flow.setValue(repeatTimePort, i);
                await game.runActions(flow, action);
                flow.ExitScope();
            }
        }

        [TranslateMsg("动作图编辑器中动作节点名称之重复动作直到动作")]
        private const string EDIOTR_NODE_NAME_DO_UNTIL_ACTION = "重复动作直到";
        [TranslateMsg("动作图编辑器中动作节点参数名称之最多重复次数参数")]
        private const string EDIOTR_PARAM_NAME_MAX_REPEAT_TIME = "最多重复次数";
        [TranslateMsg("动作图编辑器中动作节点参数名称之结束条件参数")]
        private const string EDIOTR_PARAM_NAME_STOP_CONDITION = "结束条件";

        [ActionNodeMethod(EDIOTR_NODE_NAME_DO_UNTIL_ACTION, null)]
        public static async Task doUntilAction(CardEngine game, Card card, Buff buff, EventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(EDIOTR_PARAM_NAME_STOP_CONDITION, isOut: false)] ControlInput condition,
            [ActionNodeParam(EDIOTR_PARAM_NAME_MAX_REPEAT_TIME)] int maxRepeatTime,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ACTION, isOut: true)] ControlInput action,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_REPEAT_TIME, isOut: true)] int repeatTime)
        {
            if (action == null)
                return;
            if (maxRepeatTime <= 0)
                return;
            int times = 0;
            bool finished = false;

            var repeatTimePort = node.getOutputPort<ValueOutput>(nameof(repeatTime));
            var resultPort = condition?.node.getOutputPorts<ValueOutput>().FirstOrDefault(o => o.define.type == typeof(bool));
            do
            {
                times++;
                finished = false;

                flow.EnterScope();
                flow.setValue(repeatTimePort, times);
                await game.runActions(flow, action);
                if (resultPort != null && await game.getValue<bool>(flow, resultPort))
                {
                    finished = true;
                }
                flow.ExitScope();
            }
            while (times < maxRepeatTime && !finished);
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
            return player?.getProp(propName);
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
            player?.setProp(propName, value);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_PLAYER_OPPONENT, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_OPPONENT_PLAYER)]
        public static Player getPlayerOpponent(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player == null)
                return null;
            return game.getOpponent(player);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_MASTER, StringTable.EDITOR_NODE_CATEGORY_PLAYER, "GetMaster")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card getMaster([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player?.getMaster();
        }


        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_GET_CURRENT_PLAYER = "获取当前回合的玩家";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_CURRENT_PLAYER, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)]
        public static Player getCurrentPlayer(CardEngine game)
        {
            return game.getCurrentPlayer();
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
            if (player == null)
                return Array.Empty<Card>();
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
            if (player == null)
                return Array.Empty<Card>();
            return player.getGrave();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_PLAYER_DECK_CARDS, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getPlayerDeckCards([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player == null)
                return Array.Empty<Card>();
            return player.getDeck();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DRAW_TARGET_CARD, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        public static Task drawTargetCard(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card targetCard,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DRAW_EVENT)] out HearthStoneRule.DrawEventArg drawEvent,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BURN_EVENT)] out HearthStoneRule.BurnEventArg burnEvent)
        {
            if (targetCard == null)
            {
                drawEvent = null;
                burnEvent = null;
                return Task.CompletedTask;
            }
            return targetCard.owner.draw(game, targetCard, out drawEvent, out burnEvent);
        }
        ///从一定数量卡牌中选一张
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SELECT_ONE_FROM_MANY_CARDS, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Task<Card> selectOneFromManyCards(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count)
        {
            if (cards == null)
                return null;
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
            if (defines == null)
                return null;
            return game.askSelect(card.owner, defines, count);
        }
        /// <summary>
        /// 丢弃卡牌
        /// </summary>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_DISCARD, StringTable.EDITOR_NODE_CATEGORY_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DRAW_EVENT)]
        public static Task<THHCard.DiscardEventArg[]> discard(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HANDS)] Card[] hands)
        {
            if (hands == null)
                return null;
            return hands.discard(game);
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
        [Obsolete]
        public static async Task<Card[]> getCardsInDeckWithCondition(CardEngine game, Card card, IBuff buff, IEventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION, isConst: true)] ControlInput condition)
        {
            Pile deck = card.owner.getDeck();
            List<Card> cardList = new List<Card>();
            var resultPort = condition?.node.getOutputPorts<ValueOutput>().FirstOrDefault(o => o.define.type == typeof(bool));
            for (int i = 0; i < deck.count; i++)
            {

                bool fits = true;
                if (resultPort != null)
                {
                    flow.EnterScope();
                    fits = await game.getValue<bool>(flow, resultPort);
                    flow.ExitScope();
                }
                if (fits)
                {
                    cardList.Add(deck[i]);
                }
            }
            return cardList.ToArray();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_FILTER_CARDS_WITH_CONDITION, StringTable.EDITOR_NODE_CATEGORY_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        [Obsolete]
        public static async Task<Card[]> getCardsWithCondition(CardEngine game, Card card, Buff buff, EventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION)] ControlInput condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_FILTERING_CARD, isOut: true)] Card filteringCard)
        {
            if (cards == null)
                return Array.Empty<Card>();

            List<Card> cardList = new List<Card>();
            var resultPort = condition?.node.getOutputPorts<ValueOutput>().FirstOrDefault(o => o.define.type == typeof(bool));
            var filteringPort = node.getOutputPort<ValueOutput>(nameof(filteringCard));
            for (int i = 0; i < cards.Length; i++)
            {

                bool fits = true;
                if (resultPort != null)
                {
                    flow.EnterScope();
                    flow.setValue(filteringPort, cards[i]);
                    fits = await game.getValue<bool>(flow, resultPort);
                    flow.ExitScope();
                }
                if (fits)
                {
                    cardList.Add(cards[i]);
                }
            }
            return cardList.ToArray();
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_CARDS_COUNT, StringTable.EDITOR_NODE_CATEGORY_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)]
        [Obsolete]
        public static int getCardsCount([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            if (cards == null)
                return 0;
            return cards.Length;
        }

        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_CHECK_CARD_PILE = "卡牌所在牌堆判断";
        /// <summary>
        /// 卡牌所在牌堆判断
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        [ActionNodeMethod(EDITOR_NODE_NAME_CHECK_CARD_PILE, StringTable.EDITOR_NODE_CATEGORY_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool checkCardPile(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE, isConst: true)] string pileName)
        {
            return card?.pile?.name == pileName;
        }
        #endregion
        //  与增益相关
        #region 增益
        //[ActionNodeMethod(StringTable.EDITOR_NODE_NAME_GET_BUFF_DEFINE, StringTable.EDITOR_NODE_CATEGORY_BUFF, "GetBuffDefine")]
        //[return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF_DEFINE)]
        //public static BuffDefine getBuffDefine(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ID, isConst: true)] DefineReference defRef)
        //{
        //    return game.getBuffDefine(defRef.cardPoolId, defRef.defineId);
        //}
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
            if (cards == null || buffDefine == null)
                return Array.Empty<Buff>();

            List<Buff> buffList = new List<Buff>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
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
            if (card == null || buff == null)
                return Task.FromResult(false);

            return card.removeBuff(game, buff);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_HAS_BUFF, StringTable.EDITOR_NODE_CATEGORY_BUFF)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool hasBuff(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF_DEFINE)] BuffDefine buffDefine)
        {
            if (card == null || buffDefine == null)
                return false;
            return card.hasBuff(buffDefine);
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_SET_BUFF_PROP, StringTable.EDITOR_NODE_CATEGORY_BUFF)]
        public static async Task setBuffProp(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)] Buff[] buffs,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME, isConst: true)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            if (buffs == null)
                return;

            for (int i = 0; i < buffs.Length; i++)
            {
                if (buffs[i] == null)
                    continue;
                await buffs[i].setProp(game, propName, value);
            }
        }


        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_GET_BUFF_PROP = "获取增益属性";
        /// <summary>
        /// 获取增益属性
        /// </summary>
        /// <param name="game"></param>
        /// <param name="buff"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_BUFF_PROP, StringTable.EDITOR_NODE_CATEGORY_BUFF)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static object getBuffProp(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)] Buff buff,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME, isConst: true)] string propName)
        {
            if (buff == null)
                return default;
            return buff.getProp(game, propName);
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
            if (player == null)
                return null;
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
            if (cards == null)
                return Array.Empty<Card>();
            if (player == null)
                return cards;

            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
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
            if (cards == null)
                return Array.Empty<Card>();
            if (player == null)
                return cards;

            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
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
            if (cards == null)
                return Array.Empty<Card>();
            if (player == null)
                return cards;

            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
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
            if (cards == null)
                return Array.Empty<Card>();
            if (player == null)
                return cards;

            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
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
            if (cards == null)
                return Array.Empty<Card>();
            if (player == null)
                return cards;

            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
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
        [TranslateMsg("动作图编辑器中参数名称之装备事件")]
        public const string EDITOR_PARAM_NAME_EQUIP_EVENT = "装备事件";
        [ActionNodeMethod(EDITOR_NODE_NAME_CARD_MOVE_TO_ITEM, StringTable.EDITOR_CATEGORY__ACTION__NAME)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EQUIP_EVENT)]
        public static async Task<HearthStoneRule.EquipItemEventArg> moveToItem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card card)
        {
            if (card == null)
                return null;
            if (player == null)
                return null;
            return await player.equip(game, card.pile, card);
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
            if (cards == null)
                return Array.Empty<Card>();
            if (player == null)
                return cards;

            List<Card> MoveList = new List<Card>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
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
            if (elements == null)
                return Array.Empty<object>();
            return elements;
        }
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_ADD_ELEMENT_TO_ARRAY, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)]
        public static Array addElementToArray(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isParams: true)] object[] elements)
        {
            if (array == null)
                array = Array.Empty<object>();

            object[] newArray = new object[array.Length + elements.Length];
            Array.Copy(array, 0, newArray, 0, array.Length);

            if (elements == null)
                return newArray;
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
        public static async Task<object[]> where(CardEngine game, Card card, Buff buff, EventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION)] ControlInput condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null)
                return Array.Empty<object>();

            List<object> objList = new List<object>();
            List<object> negList = new List<object>();

            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));
            var resultPort = condition?.node.getOutputPorts<ValueOutput>().FirstOrDefault(o => o.define.type == typeof(bool));

            for (int i = 0; i < array.Length; i++)
            {
                object obj = array.GetValue(i);

                flow.setValue(elementPort, obj);


                bool result = true;
                if (resultPort != null)
                {
                    flow.EnterScope();
                    result = await game.getValue<bool>(flow, resultPort);
                    flow.ExitScope();
                }

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
            if (collection == null)
                return 0;
            return collection.Length;
        }


        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CONTAINS = "包含";
        [TranslateMsg("编辑器-包含节点-参数名称")]
        public const string EDITOR_PARAM_NAME_CONTAINS_ELEMENT = "是否包含元素";
        /// <summary>
        /// 包含
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [ActionNodeMethod(EDITOR_NODE_NAME_CONTAINS, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_CONTAINS_ELEMENT)]
        public static bool getCount([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] collection,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT)] object element)
        {
            if (collection == null)
                return false;
            return collection.Contains(element);
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
        public static Array orderBy(CardEngine game, Card card, Buff buff, EventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(EDITOR_NODE_NAME_DESCENDING, isConst: true)] bool descending,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(EDITOR_NODE_NAME_ORDER_KEY)] ControlInput condition,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null)
                return Array.Empty<object>();
            if (condition == null)
                return array;

            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));
            var resultPort = condition?.node.getOutputPorts<ValueOutput>().FirstOrDefault();

            object func(object obj)
            {
                flow.setValue(elementPort, obj);
                object value = null;
                if (resultPort != null)
                {
                    flow.EnterScope();
                    value = game.getValue<object>(flow, resultPort).Result;
                    flow.ExitScope();
                }
                return value;
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
            if (index >= 0 && index < array.Length)
                return array[index];
            return null;
        }
        [TranslateMsg("编辑器_动作节点_遍历")]
        public const string EDITOR_NODE_NAME_FOREACH = "遍历";
        /// <summary>
        /// 遍历集合
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_FOREACH, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        public static async Task foreachAction(CardEngine game, Card card, Buff buff, EventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ACTION)] ControlInput action,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isOut: true)] object element)
        {
            if (array == null)
                return;
            if (action == null)
                return;
            var elementPort = node.getOutputPort<ValueOutput>(nameof(element));
            foreach (object ele in array)
            {
                flow.EnterScope();
                flow.setValue(elementPort, ele);
                await game.runActions(flow, action);
                flow.ExitScope();
            }
        }
        public const string EDITOR_NODE_NAME_GET_RANDOM_ELEMENT = "获取集合中的随机元素";
        [ActionNodeMethod(EDITOR_NODE_NAME_GET_RANDOM_ELEMENT, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT)]
        public static object getRandomElement(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] collection)
        {
            if (collection == null || collection.Length <= 0)
                return null;
            if (collection.Length == 1)
                return collection[0];
            return collection[game.randomInt(0, collection.Length - 1)];
        }
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_REVERSE_COLLECTION = "反转集合";
        /// <summary>
        /// 反转集合
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_REVERSE_COLLECTION, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static object[] reverseCollection(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array)
        {
            if (array == null)
                return null;
            return array.Reverse().ToArray();
        }
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_DISTINCT_COLLECTION = "集合去重";
        /// <summary>
        /// 集合去重
        /// </summary>
        [ActionNodeMethod(EDITOR_NODE_NAME_DISTINCT_COLLECTION, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION)]
        [return :ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array distinct(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array)
        {
            if (array == null)
                return null;

            return array.Distinct().ToArray();
        }
        [TranslateMsg("编辑器-集合相减-参数名称")]
        public const string EDITOR_PARAM_NAME_EXCEPTED_COLLECTION = "减去的集合";
        /// <summary>
        /// 集合相减
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        [ActionNodeMethod(StringTable.EDITOR_NODE_NAME_EXCEPT, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, "Except")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array except(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] array, 
            [ActionNodeParam(EDITOR_PARAM_NAME_EXCEPTED_COLLECTION)] object[] excepted)
        {
            if (array == null)
                return null;
            if (excepted == null)
                return array;
            return array.Except(excepted).ToArray();
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
            if (eventArg == null)
                return;
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
            if (eventArg.getVar<Card[]>(EVENTVAR.TARGETS).Length > 0)
                eventArg.setVar(EVENTVAR.TARGETS, target);
        }
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_DAMAGE_CHANGE_TARGET = "伤害/更改受伤卡牌";
        [ActionNodeMethod(EDITOR_NODE_EVENT_DAMAGE_CHANGE_TARGET, EDITOR_NODE_EVENT)]
        public static void damageEvent_changeTarget(
            [ActionNodeParam(EDITOR_NODE_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card target)
        {
            if (eventArg is THHCard.DamageEventArg damageArg)
            {
                damageArg.target = target;
            }
        }
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_DAMAGE_CHANGE_SOURCE = "伤害/更改伤害源";
        [ActionNodeMethod(EDITOR_NODE_EVENT_DAMAGE_CHANGE_SOURCE, EDITOR_NODE_EVENT)]
        public static void damageEvent_changeSource(
            [ActionNodeParam(EDITOR_NODE_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card source)
        {
            if (eventArg is THHCard.DamageEventArg damageArg)
            {
                damageArg.source = source;
            }
        }
        [TranslateMsg("编辑器/事件-节点名称")]
        public const string EDITOR_NODE_EVENT_DAMAGE_CHANGE_VALUE = "伤害/更改伤害值";
        [ActionNodeMethod(EDITOR_NODE_EVENT_DAMAGE_CHANGE_VALUE, EDITOR_NODE_EVENT)]
        public static void damageEvent_changeValue(
            [ActionNodeParam(EDITOR_NODE_EVENT)] EventArg eventArg,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] int value)
        {
            if (eventArg is THHCard.DamageEventArg damageArg)
            {
                damageArg.damageValue = value;
            }
        }
        #endregion
    }
}
