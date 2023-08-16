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
        #region 无分类
        [TranslateMsg("动作图编辑器中动作节点名称之重复动作直到动作")]
        private const string EDIOTR_NODE_NAME_DO_UNTIL_ACTION = "重复动作直到";
        [TranslateMsg("动作图编辑器中动作节点参数名称之最多重复次数参数")]
        private const string EDIOTR_PARAM_NAME_MAX_REPEAT_TIME = "最多重复次数";
        [TranslateMsg("动作图编辑器中动作节点参数名称之结束条件参数")]
        private const string EDIOTR_PARAM_NAME_STOP_CONDITION = "结束条件";


        [ObsoletedKeyMethod(BuiltinActionDefineID.BranchAction, null, StringTable.EDIOTR_NODE_NAME_BRANCH_ACTION)]
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


        [ObsoletedKeyMethod(BuiltinActionDefineID.ForAction, null, StringTable.EDIOTR_NODE_NAME_FOR_ACTION)]
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


        [ObsoletedKeyMethod(BuiltinActionDefineID.DoUntilAction, null, EDIOTR_NODE_NAME_DO_UNTIL_ACTION)]
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
            var resultPort = condition?.node.getOutputPorts<ValueOutput>().FirstOrDefault();
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
        #endregion

        #region 常量
        [TranslateMsg("动作图编辑器中动作节点名称之X到Y之间的随机数")]
        public const string EDITOR_NODE_NAME_GET_RANDOM_INT = "X到Y之间的随机整数";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_GET_INT_X = "整数X";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_GET_INT_Y = "整数Y";
        /// <summary>
        /// X到Y之间随机整数
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.RandomInt, StringTable.EDITOR_ACTIONGRAPH_MENU_CATEGORY_CONST, EDITOR_NODE_NAME_GET_RANDOM_INT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int randomInt(CardEngine game,
           [ActionNodeParam(EDITOR_PARAM_NAME_GET_INT_X, isConst: false)] int countX,
           [ActionNodeParam(EDITOR_PARAM_NAME_GET_INT_Y, isConst: false)] int countY)
        {
            return game.randomInt(countX, countY);
        }
        #endregion

        #region 玩家
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_GET_CURRENT_PLAYER = "获取当前回合的玩家";
        [TranslateMsg("动作图编辑器中动作节点名称之增加固定数量护甲")]
        public const string EDITOR_NODE_ADD_AMOUNT_OF_ARMOR = "增加固定数量护甲";
        [TranslateMsg("动作图编辑器中动作节点名称之增加护甲")]
        public const string EDITOR_NODE_ADD_ARMOR = "增加护甲";
        [TranslateMsg("动作图编辑器中动作节点名称之失去护甲")]
        public const string EDITOR_NODE_LOSE_ARMOR = "失去护甲";
        [TranslateMsg("动作图编辑器中动作节点名称之获取玩家延迟区")]
        public const string EDITOR_NODE_NAME_GET_HEAD = "获取玩家延迟区";
        [TranslateMsg("动作图编辑器中动作节点名称之获取玩家技能")]
        public const string EDITOR_NODE_NAME_GET_SKILL = "获取玩家技能";
        [TranslateMsg("动作图编辑器中动作节点名称之复原技能")]
        public const string EDITOR_NODE_RESET_SKILL = "复原技能";
        [TranslateMsg("动作图编辑器中动作节点名称之获取玩家道具")]
        public const string EDITOR_NODE_NAME_GET_ITEM = "获取玩家道具";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_PLAYER_HAS_ITEM = "玩家是否装备道具";
        [TranslateMsg("动作图编辑器中动作节点名称之摧毁道具")]
        public const string EDITOR_NODE_DESTROY_ITEM = "摧毁道具";
        [TranslateMsg("动作图编辑器中动作节点名称之获取玩家暂存区")]
        public const string EDITOR_NODE_NAME_GET_WARP = "获取玩家暂存区";
        [TranslateMsg("动作图编辑器中动作节点名称之获取当前灵力值")]
        public const string EDITOR_NODE_NAME_GET_GEM = "获取当前灵力值";
        [TranslateMsg("动作图编辑器中动作节点名称之获取灵力上限")]
        public const string EDITOR_NODE_NAME_GET_MAX_GEM = "获取灵力上限";
        [TranslateMsg("动作图编辑器中动作节点名称之设置当前灵力值")]
        public const string EDITOR_NODE_ADD_AMOUNT_OF_GEM = "设置当前灵力值";
        [TranslateMsg("动作图编辑器中动作节点名称之增加当前灵力值")]
        public const string EDITOR_NODE_ADD_GEM = "增加当前灵力值";
        [TranslateMsg("动作图编辑器中动作节点名称之设置灵力上限")]
        public const string EDITOR_NODE_SET_MAX_GEM = "设置灵力上限";
        [TranslateMsg("动作图编辑器中动作节点名称之增加灵力上限")]
        public const string EDITOR_NODE_ADD_MAX_GEM = "增加灵力上限";
        [TranslateMsg("编辑器/卡牌-节点名称")]
        public const string EDITOR_NODE_FAITHDUEL = "信仰对决";
        [TranslateMsg("编辑器/卡牌-信仰对决节点参数")]
        public const string EDITOR_NODE_FAITHDUEL_WIN = "是否胜利";
        [TranslateMsg("编辑器/卡牌-信仰对决节点参数")]
        public const string EDITOR_NODE_FAITHDUEL_FILTER = "筛选条件";
        [TranslateMsg("编辑器/卡牌-信仰对决节点参数")]
        public const string EDITOR_NODE_FAITHDUEL_FILTER_CARD = "筛选卡牌";
        [TranslateMsg("编辑器/卡牌-信仰对决节点参数")]
        public const string EDITOR_NODE_FAITHDUEL_SELF_CARD = "己方卡牌";
        [TranslateMsg("编辑器/卡牌-信仰对决节点参数")]
        public const string EDITOR_NODE_FAITHDUEL_OPPONENT_CARD = "对方卡牌";
        [TranslateMsg("节点参数 TargetName")]
        const string EDITOR_PARAM_TARGET_NAME = "目标名";


        /// <summary>
        /// 获取玩家属性
        /// </summary>
        /// <param name="player"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerProp, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_GET_PLAYER_PROP)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetPlayerProp, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_SET_PLAYER_PROP)]
        public static void setPlayerProp(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME, isConst: true)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            player?.setProp(propName, value);
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerOpponent, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_GET_PLAYER_OPPONENT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_OPPONENT_PLAYER)]
        public static Player getPlayerOpponent(CardEngine game, [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player == null)
                return null;
            return game.getOpponent(player);
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerMaster, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_GET_MASTER, "GetMaster")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card getMaster([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player?.getMaster();
        }


        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCurrentPlayer, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_NAME_GET_CURRENT_PLAYER)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerHands, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_GET_PLAYER_HAND_CARDS)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerGraveCards, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_GET_PLAYER_GRAVE_CARDS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getPlayerGraveCards([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player == null)
                return Array.Empty<Card>();
            return player.getGrave();
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerDeckCards, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_GET_PLAYER_DECK_CARDS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getPlayerDeckCards([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player == null)
                return Array.Empty<Card>();
            return player.getDeck();
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.DrawTargetCard, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_DRAW_TARGET_CARD)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.SelectOneFromCards, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_SELECT_ONE_FROM_MANY_CARDS)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.SelectOneFromCardDefines, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_SELECT_ONE_FROM_MANY_DEFINES)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.Discard, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_DISCARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DRAW_EVENT)]
        public static Task<THHCard.DiscardEventArg[]> discard(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_HANDS)] Card[] hands)
        {
            if (hands == null)
                return null;
            return hands.discard(game);
        }


        /// <summary>
        /// 增加固定数量的护甲
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddArmor, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_ADD_AMOUNT_OF_ARMOR)]
        public static async Task addArmorToTarget(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: true)] int count,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player != null)
                await player.getMaster().gainArmor(game, count);
        }
        /// <summary>
        /// 增加护甲
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddAmountOfArmor, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_ADD_ARMOR)]
        public static async Task addAmountOfArmorToTarget(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
             [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            if (player != null)
                await player.getMaster().gainArmor(game, count);
        }
        /// <summary>
        /// 失去护甲
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.LoseArmor, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_LOSE_ARMOR)]
        public static async Task loseArmor(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            if (player != null)
                await player.getMaster().loseArmor(game, count);
        }
        /// <summary>
        /// 获取延迟区
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerHeadCards, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_NAME_GET_HEAD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getHead(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player == null)
                return Array.Empty<Card>();
            return player.getHead();
        }
        /// <summary>
        /// 获取技能
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerSkill, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_NAME_GET_SKILL)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card getSkill(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player?.getSkill();
        }
        /// <summary>
        /// 复原技能
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.ResetSkill, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_RESET_SKILL)]
        public static async Task resetSkill(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if ((player != null) && player.getSkill().isUsed(game))
            {
                await player.getSkill().setUsed(game, false);
            }
        }
        /// <summary>
        /// 获取道具
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerItem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_NAME_GET_ITEM)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card getItem(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player?.getItem();
        }

        /// <summary>
        /// 玩家是否装备道具
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.DoesPlayerHaveItem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_NAME_PLAYER_HAS_ITEM)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool playerHasItem(
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            return player?.getItem() != null;
        }

        /// <summary>
        /// 摧毁道具
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.DestroyItem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_DESTROY_ITEM)]
        public static async Task destroyItem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player != null)
                await player.destroyItem(game);
        }
        /// <summary>
        /// 获取暂存区
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerWarpCards, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_NAME_GET_WARP)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getWarp(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player == null)
                return Array.Empty<Card>();
            return player.getWarp();
        }
        /// <summary>
        /// 获取当前灵力值
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerGem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_NAME_GET_GEM)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getGem(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player == null)
                return 0;
            return player.getGem();
        }
        /// <summary>
        /// 获取灵力上限
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerMaxGem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_NAME_GET_MAX_GEM)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int getMaxGem(CardEngine game,
           [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player == null)
                return 0;
            return player.getMaxGem();
        }
        /// <summary>
        /// 设置当前灵力值
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetPlayerGem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_ADD_AMOUNT_OF_GEM)]
        public static async Task setGem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            if (player != null)
                await player.setGem(game, count);
        }
        /// <summary>
        /// 增加当前灵力值
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddPlayerGem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_ADD_GEM)]
        public static async Task addGem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            if (player != null)
                await player.setGem(game, player.getGem() + count);
        }
        /// <summary>
        /// 设置灵力上限
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetPlayerMaxGem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_SET_MAX_GEM)]
        public static async Task setMaxGem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            if (player != null)
                await player.setMaxGem(game, count);
        }
        /// <summary>
        /// 增加灵力上限
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddPlayerMaxGem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_ADD_MAX_GEM)]
        public static async Task addMaxGem(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT, isConst: false)] int count)
        {
            if (player != null)
                await player.setMaxGem(game, player.getMaxGem() + count);
        }


        /// <summary>
        /// 信仰对决
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.FaithDuel, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_FAITHDUEL)]
        [return: ActionNodeParam(EDITOR_NODE_FAITHDUEL_WIN)]
        public static bool faithduel(CardEngine game, Card card, Buff buff, EventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(EDITOR_NODE_FAITHDUEL_FILTER)] ControlInput filter,
            [ActionNodeParam(EDITOR_NODE_FAITHDUEL_FILTER_CARD, isOut: true)] Card element,
            [ActionNodeParam(EDITOR_NODE_FAITHDUEL_SELF_CARD, isOut: true)] Card selfCard,
            [ActionNodeParam(EDITOR_NODE_FAITHDUEL_OPPONENT_CARD, isOut: true)] Card opponentCard)
        {
            if (player == null)
                return false;

            bool filterFunc(Card c)
            {
                bool valid = true;
                if (filter != null)
                {
                    var elementPort = node.getOutputPort<ValueOutput>(nameof(element));
                    var resultPort = filter?.node.getOutputPorts<ValueOutput>().FirstOrDefault();
                    flow.EnterScope();
                    flow.setValue(elementPort, c);
                    valid = game.getValue<bool>(flow, resultPort).Result;
                    flow.ExitScope();
                }
                return valid;
            }
            var arg = player.revealDuel(game, filterFunc).Result;
            flow.setValue(node.getOutputPort<ValueOutput>(nameof(selfCard)), arg.youCard);
            flow.setValue(node.getOutputPort<ValueOutput>(nameof(opponentCard)), arg.oppoCard);
            return arg.result;
        }

        /// <summary>
        /// 获取牌堆
        /// </summary>
        /// <param name="player"></param>
        /// <param name="pileName"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPile, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_GET_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE)]
        public static Pile getPile(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player, 
            [ActionNodeParam(EDITOR_PARAM_PILE_NAME)] string pileName)
        {
            return player.getPile(pileName);
        }


        [ObsoletedKeyMethod(BuiltinActionDefineID.GetTarget, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_GET_TARGET)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)]
        [Obsolete]
        public static Card getTarget(HearthStoneRule.ActiveEventArg eventArg, 
            [ActionNodeParam(EDITOR_PARAM_TARGET_NAME, true)] string targetName)
        {
            if (targetName == "Target")
                return (Card)eventArg.targets[0];
            throw new NotImplementedException();
        }
        #endregion

        //  有关牌堆的操作，比如说洗牌，获取特定牌等等
        #region 牌堆
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_CHECK_CARD_PILE = "卡牌所在牌堆判断";
        [TranslateMsg("编辑器-参数名称 PileName")]
        private const string EDITOR_PARAM_PILE_NAME = "牌堆名";
        [TranslateMsg("编辑器-参数名称 IsChanged")]
        private const string EDITOR_PARAM_NAME_IS_CHANGED = "是否已改变";
        /// <summary>
        /// 获取牌堆中的卡牌
        /// </summary>
        /// <param name="pile"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardsInPile, StringTable.EDITOR_NODE_CATEGORY_PILE, StringTable.EDITOR_NODE_NAME_GET_CARDS_IN_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Card[] getCardsInPile([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE)] Pile pile)
        {
            return pile;
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.FilterCardsInDeck, StringTable.EDITOR_NODE_CATEGORY_PILE, StringTable.EDITOR_NODE_NAME_GET_CARDS_IN_DECK_WITH_CONDITION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        [Obsolete]
        public static async Task<Card[]> getCardsInDeckWithCondition(CardEngine game, Card card, IBuff buff, IEventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION, isConst: true)] ControlInput condition)
        {
            Pile deck = card.owner.getDeck();
            List<Card> cardList = new List<Card>();
            var resultPort = condition?.node.getOutputPorts<ValueOutput>().FirstOrDefault();
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.FilterCards, StringTable.EDITOR_NODE_CATEGORY_PILE, StringTable.EDITOR_NODE_NAME_FILTER_CARDS_WITH_CONDITION)]
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
            var resultPort = condition?.node.getOutputPorts<ValueOutput>().FirstOrDefault();
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCardsCount, StringTable.EDITOR_NODE_CATEGORY_PILE, StringTable.EDITOR_NODE_NAME_GET_CARDS_COUNT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)]
        [Obsolete]
        public static int getCardsCount([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            if (cards == null)
                return 0;
            return cards.Length;
        }

        /// <summary>
        /// 卡牌所在牌堆判断
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.CheckCardPile, StringTable.EDITOR_NODE_CATEGORY_PILE, EDITOR_NODE_NAME_CHECK_CARD_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool checkCardPile(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE, isConst: true)] string pileName)
        {
            return card?.pile?.name == pileName;
        }


        [ObsoletedKeyMethod(BuiltinActionDefineID.IsPlayerPileChanged, StringTable.EDITOR_NODE_CATEGORY_PILE, StringTable.EDITOR_NODE_NAME_IS_PLAYER_PILE_CHANGED)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_IS_CHANGED)]
        public static bool isPlayerPileChanged(Pile.MoveCardEventArg moveCardEvent, 
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player, 
            [ActionNodeParam(EDITOR_PARAM_PILE_NAME)] string pileName)
        {
            return (moveCardEvent.from.owner == player && moveCardEvent.from.name == pileName) ||
                (moveCardEvent.to.owner == player && moveCardEvent.to.name == pileName);
        }
        #endregion
        //  与增益相关
        #region 增益
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_GET_BUFF_PROP = "获取增益属性";

        [ObsoletedKeyMethod(BuiltinActionDefineID.ThisBuff, StringTable.EDITOR_NODE_CATEGORY_BUFF, StringTable.EDITOR_NODE_NAME_THIS_BUFF)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)]
        public static Buff thisBuff(Buff buff)
        {
            return buff;
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddBuff, StringTable.EDITOR_NODE_CATEGORY_BUFF, StringTable.EDITOR_NODE_NAME_ADD_BUFF)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.RemoveBuff, StringTable.EDITOR_NODE_CATEGORY_BUFF, StringTable.EDITOR_NODE_NAME_REMOVE_BUFF)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_IS_BUFF_REMOVED)]
        public static Task<bool> removeBuff(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)] Buff buff)
        {
            if (card == null || buff == null)
                return Task.FromResult(false);

            return card.removeBuff(game, buff);
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.HasBuff, StringTable.EDITOR_NODE_CATEGORY_BUFF, StringTable.EDITOR_NODE_NAME_HAS_BUFF)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool hasBuff(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF_DEFINE)] BuffDefine buffDefine)
        {
            if (card == null || buffDefine == null)
                return false;
            return card.hasBuff(buffDefine);
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetBuffProp, StringTable.EDITOR_NODE_CATEGORY_BUFF, StringTable.EDITOR_NODE_NAME_SET_BUFF_PROP)]
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


        /// <summary>
        /// 获取增益属性
        /// </summary>
        /// <param name="game"></param>
        /// <param name="buff"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetBuffProp, StringTable.EDITOR_NODE_CATEGORY_BUFF, EDITOR_NODE_NAME_GET_BUFF_PROP)]
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
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_DECK = "卡牌洗入牌库";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_HAND = "卡牌移回手牌";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_FIELD = "卡牌置入战场";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_ITEM = "卡牌装备到道具栏";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_HEAD = "卡牌置入延迟区";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_GRAVE = "卡牌置入墓地";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CARD_MOVE_TO_WARP = "卡牌移动到暂存区";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_EQUIP_EVENT = "装备事件";


        /// <summary>
        /// 简单抽牌
        /// 抽牌行动的简单实现，抽一张牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="player"></param>
        /// <param name="drawEvent"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.SimpleDraw, StringTable.EDITOR_CATEGORY__ACTION__NAME, StringTable.EDITOR_NODE__SIMPLE_DRAW_CARD__NAME)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToField, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_FIELD)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToHand, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_HAND)]
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
                await card.resetCard(game);
                await card.returnToHand(game, player);
                MoveList.Add(card);
            }
            return MoveList.ToArray();
        }
        /// <summary>
        /// 卡牌从洗入牌库
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToDeck, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_DECK)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToGrave, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_GRAVE)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToHead, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_HEAD)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToItem, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_ITEM)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToWarp, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_WARP)]
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
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_CONTAINS = "包含";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_ORDER_BY = "排序";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_GET_ELEMENT_BY_INDEX = "获取第X个元素";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_GET_RANDOM_ELEMENT = "获取集合中的随机元素";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_REVERSE_COLLECTION = "反转集合";
        [TranslateMsg("编辑器-节点名称")]
        public const string EDITOR_NODE_NAME_DISTINCT_COLLECTION = "集合去重";
        [TranslateMsg("编辑器_动作节点_遍历")]
        public const string EDITOR_NODE_NAME_FOREACH = "遍历";

        [TranslateMsg("编辑器_参数名称_降序")]
        public const string EDITOR_PARAM_NAME_DESCENDING = "降序";
        [TranslateMsg("编辑器_参数名称_排序依据")]
        public const string EDITOR_PARAM_NAME_ORDER_KEY = "排序依据";
        [TranslateMsg("编辑器-包含节点-参数名称")]
        public const string EDITOR_PARAM_NAME_CONTAINS_ELEMENT = "是否包含元素";
        [TranslateMsg("动作图编辑器中参数节点名称之匹配的集合")]
        public const string EDITOR_PARAM_NAME_ARRAY_MATCHED = "匹配的集合";
        [TranslateMsg("动作图编辑器中参数节点名称之集合")]
        public const string EDITOR_PARAM_NAME_ARRAY_MISMATCHED = "不匹配的集合";
        [TranslateMsg("编辑器_参数名称_元素位置")]
        public const string EDITOR_PARAM_NAME_INDEX = "元素位置";
        [TranslateMsg("编辑器-集合相减-参数名称")]
        public const string EDITOR_PARAM_NAME_EXCEPTED_COLLECTION = "减去的集合";


        [ObsoletedKeyMethod(BuiltinActionDefineID.CreateArray, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, StringTable.EDITOR_NODE_NAME_CREATE_ARRAY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)]
        public static Array createArray([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT, isParams: true)] object[] elements)
        {
            if (elements == null)
                return Array.Empty<object>();
            return elements;
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddElementToArray, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, StringTable.EDITOR_NODE_NAME_ADD_ELEMENT_TO_ARRAY)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.Where, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, StringTable.EDITOR_NODE_NAME_WHERE)]
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
            var resultPort = condition?.node.getOutputPorts<ValueOutput>().FirstOrDefault();

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
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetCount, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, StringTable.EDITOR_NODE_NAME_GET_COUNT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)]
        public static int getCount([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] collection)
        {
            if (collection == null)
                return 0;
            return collection.Length;
        }

        /// <summary>
        /// 包含
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.Contains, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_CONTAINS)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_CONTAINS_ELEMENT)]
        public static bool contains([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)] object[] collection,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT)] object element)
        {
            if (collection == null)
                return false;
            return collection.Contains(element);
        }

        /// <summary>
        /// 根据排序依据排序。
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.OrderBy, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_ORDER_BY)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array orderBy(CardEngine game, Card card, Buff buff, EventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(EDITOR_PARAM_NAME_DESCENDING, isConst: true)] bool descending,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(EDITOR_PARAM_NAME_ORDER_KEY)] ControlInput condition,
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

        /// <summary>
        /// 获取第X个元素。
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetElementByIndex, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_GET_ELEMENT_BY_INDEX)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ELEMENT)]
        public static object getElementByIndex(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array,
            [ActionNodeParam(EDITOR_PARAM_NAME_INDEX)] int index)
        {
            if (index >= 0 && index < array.Length)
                return array[index];
            return null;
        }
        /// <summary>
        /// 遍历集合
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.ForeachAction, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_FOREACH)]
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
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetRandomElement, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_GET_RANDOM_ELEMENT)]
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
        /// <summary>
        /// 反转集合
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.ReverseCollection, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_REVERSE_COLLECTION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static object[] reverseCollection(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array)
        {
            if (array == null)
                return null;
            return array.Reverse().ToArray();
        }
        /// <summary>
        /// 集合去重
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.Distinct, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, EDITOR_NODE_NAME_DISTINCT_COLLECTION)]
        [return :ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COLLECTION)]
        public static Array distinct(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ARRAY)] object[] array)
        {
            if (array == null)
                return null;

            return array.Distinct().ToArray();
        }
        /// <summary>
        /// 集合相减
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.Except, StringTable.EDITOR_NODE_CATEGORY_COLLECTION_OPERATION, StringTable.EDITOR_NODE_NAME_EXCEPT, "Except")]
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

    }



    /// <summary>
    /// 用于将过时的MethodActionDefine转换为以内置名为主键的动作定义。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ObsoletedKeyMethodAttribute : ActionNodeMethodAttribute
    {
        public ObsoletedKeyMethodAttribute(BuiltinActionDefineID id, string category, string editorName, params string[] obsoleteNames) : 
            this((int)id, category, editorName, obsoleteNames.Append(editorName).ToArray())
        {
        }
        public ObsoletedKeyMethodAttribute(int id, string category, string editorName, params string[] obsoleteNames) : base(id, category, editorName, obsoleteNames.Append(editorName).ToArray())
        {
        }
    }
}
