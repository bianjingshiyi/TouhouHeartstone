using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;

namespace Game
{

    public static partial class EffectEditorLibrary
    {
        public const string SCOPE_VARIABLE_LOOP_STATE = "BUILTIN_VARIABLE_LOOP_STATE";
        public const int SCOPE_LOOP_STATE_LOOPING = 1;
        public const int SCOPE_LOOP_STATE_BROKEN = 2;
        public const int SCOPE_STATE_CONTINUED = 3;
        private static void setScopeLoopState(FlowScope scope, int state)
        {
            scope.setScopeVariable(SCOPE_VARIABLE_LOOP_STATE, state);
        }
        private static int getScopeLoopState(FlowScope scope)
        {
            if (scope.tryGetScopeVariable(SCOPE_VARIABLE_LOOP_STATE, out object state) && state is int intValue)
            {
                return intValue;
            }
            return 0;
        }
        private static void setScopeLooping(FlowScope scope)
        {
            setScopeLoopState(scope, SCOPE_LOOP_STATE_LOOPING);
        }
        private static bool isScopeLooping(FlowScope scope)
        {
            return getScopeLoopState(scope) == SCOPE_LOOP_STATE_LOOPING;
        }
        private static void setOutParam(Flow flow, ActionNode node, string name, object value)
        {
            flow.setValue(node.getOutputPort<ValueOutput>(name), value);
        }

        #region 其他
        [TranslateMsg("动作图编辑器中动作节点名称之X到Y之间的随机数")]
        public const string EDITOR_NODE_NAME_GET_RANDOM_INT = "X到Y之间的随机整数";
        [TranslateMsg("动作图编辑器中动作节点名称之重复动作直到动作")]
        private const string EDIOTR_NODE_NAME_DO_UNTIL_ACTION = "重复动作直到";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_NAME_IS_NOT_EXISTS = "是否不存在";

        [TranslateMsg("动作图编辑器中动作节点参数名称之最多重复次数参数")]
        private const string EDIOTR_PARAM_NAME_MAX_REPEAT_TIME = "最多重复次数";
        [TranslateMsg("动作图编辑器中动作节点参数名称之结束条件参数")]
        private const string EDIOTR_PARAM_NAME_STOP_CONDITION = "结束条件";
        [TranslateMsg("节点参数-是否不存在/输出值")]
        public const string EDITOR_PARAM_NAME_NOT_EXISTS = "不存在";

        [TranslateMsg("编辑器-节点分类名")]
        public const string EDITOR_CATEGORY_NAME_OTHER = "其他";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_GET_INT_X = "整数X";
        [TranslateMsg("编辑器-参数名称")]
        public const string EDITOR_PARAM_NAME_GET_INT_Y = "整数Y";

        [TranslateMsg("编辑器-节点名称")]
        private const string EDIOTR_NODE_NAME_SET_TEMP_VARIABLE = "设置临时变量";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDIOTR_NODE_NAME_GET_TEMP_VARIABLE = "获取临时变量";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_BREAK = "停止重复动作";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_CONTINUE = "跳过重复动作";
        /// <summary>
        /// 设置临时变量
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetTempVar, EDITOR_CATEGORY_NAME_OTHER, EDIOTR_NODE_NAME_SET_TEMP_VARIABLE)]
        public static void setTempVariable(Flow flow,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME)] string variableName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            var key = variableName;
            flow.setFlowVariable(key, value);
        }
        /// <summary>
        /// 获取临时变量
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetTempVar, EDITOR_CATEGORY_NAME_OTHER, EDIOTR_NODE_NAME_GET_TEMP_VARIABLE)]
        public static object getTempVariable(Flow flow,
            [ActionNodeParam(EDITOR_PARAM_NAME_VARIABLE_NAME)] string variableName)
        {
            var key = variableName;
            return flow.getFlowVariable(key);
        }

        [ObsoletedKeyMethod(BuiltinActionDefineID.BranchAction, EDITOR_CATEGORY_NAME_OTHER, StringTable.EDIOTR_NODE_NAME_BRANCH_ACTION)]
        public static async Task branchAction(CardEngine game, Flow flow,
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


        [ObsoletedKeyMethod(BuiltinActionDefineID.ForAction, EDITOR_CATEGORY_NAME_OTHER, StringTable.EDIOTR_NODE_NAME_FOR_ACTION)]
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
                var scope = flow.EnterScope();

                setScopeLooping(scope);
                flow.setValue(repeatTimePort, i);
                await game.runActions(flow, action);
                int loopState = getScopeLoopState(scope);

                flow.ExitScope();

                if (loopState == SCOPE_LOOP_STATE_BROKEN)
                    break;
            }
        }

        [Obsolete]
        [THHActionMethod(BuiltinActionDefineID.DoUntilAction_Obsolete, EDITOR_CATEGORY_NAME_OTHER, "重复动作直到（过时）", EDIOTR_NODE_NAME_DO_UNTIL_ACTION)]
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

        [THHActionMethod(BuiltinActionDefineID.DoUntilAction, null, EDIOTR_NODE_NAME_DO_UNTIL_ACTION)]
        public static async Task doUntilAction(CardEngine game, Card card, Buff buff, EventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(EDIOTR_PARAM_NAME_STOP_CONDITION, isOut: false)] NodeValueRef<bool> condition,
            [ActionNodeParam(EDIOTR_PARAM_NAME_MAX_REPEAT_TIME)] int maxRepeatTime,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_ACTION, isOut: true)] ControlInput action,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_REPEAT_TIME, isOut: true)] int repeatTime)
        {
            if (action == null)
                return;
            if (maxRepeatTime <= 0)
                return;
            int times = 0;
            bool finished;

            var repeatTimePort = node.getOutputPort<ValueOutput>(nameof(repeatTime));
            do
            {
                times++;
                finished = false;

                var scope = flow.EnterScope();
                setScopeLooping(scope);

                flow.setValue(repeatTimePort, times);
                await game.runActions(flow, action);

                int flowState = getScopeLoopState(scope);
                flow.ExitScope();

                flow.EnterScope();
                if (condition.outputPort != null && await condition.getValue(flow))
                {
                    finished = true;
                }
                flow.ExitScope();

                if (flowState == SCOPE_LOOP_STATE_BROKEN)
                    break;
            }
            while (times < maxRepeatTime && !finished);
        }
        /// <summary>
        /// X到Y之间随机整数
        /// </summary>
        [ObsoletedKeyMethod(BuiltinActionDefineID.RandomInt, EDITOR_CATEGORY_NAME_OTHER, EDITOR_NODE_NAME_GET_RANDOM_INT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static int randomInt(CardEngine game,
           [ActionNodeParam(EDITOR_PARAM_NAME_GET_INT_X)] int countX,
           [ActionNodeParam(EDITOR_PARAM_NAME_GET_INT_Y)] int countY)
        {
            return game.randomInt(countX, countY);
        }
        /// <summary>
        /// 是否不存在
        /// </summary>
        /// <param name="pile"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.IsNotExists, EDITOR_CATEGORY_NAME_OTHER, EDITOR_NODE_NAME_IS_NOT_EXISTS)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_NOT_EXISTS)]
        public static bool isNotExists(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            return value == null;
        }
        /// <summary>
        /// 跳出循环
        /// </summary>
        /// <param name="pile"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Break, EDITOR_CATEGORY_NAME_OTHER, EDITOR_NODE_NAME_BREAK)]
        public static void breakLoop(Flow flow)
        {
            var scope = flow.currentScope;
            while (scope != null)
            {
                if (getScopeLoopState(scope) > 0) // 是循环作用域
                    break;
                scope = scope.parentScope;
            }
            if (scope == null || !isScopeLooping(scope)) // 作用域不存在，或者循环已经被中断
                return;
            setScopeLoopState(scope, SCOPE_LOOP_STATE_BROKEN);
            scope.terminate();
        }
        /// <summary>
        /// 跳过本次循环
        /// </summary>
        /// <param name="pile"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.Continue, EDITOR_CATEGORY_NAME_OTHER, EDITOR_NODE_NAME_CONTINUE)]
        public static void continueLoop(Flow flow)
        {
            var scope = flow.currentScope;
            while (scope != null)
            {
                if (getScopeLoopState(scope) > 0) // 是循环作用域
                    break;
                scope = scope.parentScope;
            }
            if (scope == null || !isScopeLooping(scope)) // 作用域不存在，或者循环已经被中断
                return;
            setScopeLoopState(scope, SCOPE_STATE_CONTINUED);
            scope.terminate();
        }
        #endregion

        #region 游戏
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_MAKE_PLAYERS_WIN = "使玩家获胜";
        [TranslateMsg("节点名称")]
        private const string EDITOR_NODE_MAKE_PLAYERS_LOSE = "使玩家失败";
        [TranslateMsg("节点名称")]
        private const string EDITOR_NODE_MAKE_PLAYERS_DRAW = "使玩家平局";
        [TranslateMsg("节点名称")]
        private const string EDITOR_NODE_CREATE_PILE = "创建牌堆";
        [TranslateMsg("节点名称")]
        private const string EDITOR_NODE_REMOVE_PILE = "删除牌堆";
        [TranslateMsg("节点名称")]
        private const string EDITOR_NODE_MOVE_CARD_FROM_PILE_TO_PILE = "从牌堆将卡牌移动到目标牌堆";

        [TranslateMsg("节点参数-创建牌堆/容量")]
        private const string EDITOR_PARAM_CAPACITY = "容量";
        [TranslateMsg("节点参数-删除牌堆/成功删除")]
        private const string EDITOR_PARAM_NAME_PILE_REMOVED = "成功删除";
        [TranslateMsg("节点参数-删除牌堆/从牌堆将卡牌移动到目标牌堆")]
        private const string EDITOR_PARAM_NAME_TO_PILE = "目标牌堆";
        [TranslateMsg("节点参数-删除牌堆/从牌堆将卡牌移动到目标牌堆")]
        private const string EDITOR_PARAM_NAME_FROM_PILE = "源牌堆";
        [TranslateMsg("节点参数-删除牌堆/成功移动")]
        private const string EDITOR_PARAM_NAME_CARD_MOVED = "成功移动";

        [TranslateMsg("节点参数")]
        private const string EDITOR_PARAM_NAME_GAME_END_EVENT = "游戏结束事件";
        [TranslateMsg("节点参数")]
        private const string EDITOR_PARAM_NAME_MOVE_CARD_EVENT = "卡牌移动事件";
        /// <summary>
        /// 使玩家获胜
        /// </summary>
        [EventNode(typeof(GameEndEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.MakePlayersWin, StringTable.EDITOR_NODE_CATEGORY_GAME, EDITOR_NODE_MAKE_PLAYERS_WIN)]
        public static async Task makePlayersWin(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player[] players,
            [ActionNodeParam(EDITOR_PARAM_NAME_GAME_END_EVENT, isOut: true)][NodeParamEventType(typeof(GameEndEventDefine))] EventArg outEvent)
        {
            outEvent = await game.gameEnd(players ?? Array.Empty<Player>());
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }

        /// <summary>
        /// 使玩家失败
        /// </summary>
        [EventNode(typeof(GameEndEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.MakePlayersLose, StringTable.EDITOR_NODE_CATEGORY_GAME, EDITOR_NODE_MAKE_PLAYERS_LOSE)]
        public static async Task makePlayersLose(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player[] losers,
            [ActionNodeParam(EDITOR_PARAM_NAME_GAME_END_EVENT, isOut: true)][NodeParamEventType(typeof(GameEndEventDefine))] EventArg outEvent)
        {
            var winners = losers == null ? game.getSortedPlayers() : game.getSortedPlayers().Except(losers).ToArray();
            outEvent = await game.gameEnd(winners);
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }
        /// <summary>
        /// 创建牌堆
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.CreatePile, StringTable.EDITOR_NODE_CATEGORY_GAME, EDITOR_NODE_CREATE_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE)]
        public static Pile createPile(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(EDITOR_PARAM_PILE_NAME)] string pileName,
            [ActionNodeParam(EDITOR_PARAM_CAPACITY)] int capacity)
        {
            if (player == null || pileName == null)
                return null;
            var existedPile = player.getPile(pileName);
            if (existedPile != null)
            {
                return existedPile;
            }
            var pile = new Pile(pileName, capacity);
            player.addPile(game, pile);
            return pile;
        }
        /// <summary>
        /// 删除牌堆
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.RemovePile, StringTable.EDITOR_NODE_CATEGORY_GAME, EDITOR_NODE_REMOVE_PILE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_PILE_REMOVED)]
        public static bool removePile(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE)] Pile pile)
        {
            if (player == null || pile == null)
                return false;
            return player.removePile(pile);
        }
        /// <summary>
        /// 从牌堆将卡牌移动到目标牌堆
        /// </summary>
        [EventNode(typeof(MoveCardEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.MoveCardFromPileToPile, StringTable.EDITOR_NODE_CATEGORY_GAME, EDITOR_NODE_MOVE_CARD_FROM_PILE_TO_PILE)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_CARD_MOVED)]
        public static async Task<bool> moveCardFromPileToPile(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_PARAM_NAME_FROM_PILE)] Pile fromPile,
            [ActionNodeParam(EDITOR_PARAM_NAME_TO_PILE)] Pile toPile,
            [ActionNodeParam(EDITOR_PARAM_NAME_MOVE_CARD_EVENT, isOut: true)][NodeParamEventType(typeof(MoveCardEventDefine))] EventArg outEvent)
        {
            if (card == null)
                return false;
            var arg = await card.moveCardFromPileToPile(game, fromPile, toPile);
            setOutParam(flow, node, nameof(outEvent), arg);
            return arg != null && arg.getVar<bool>(MoveCardEventDefine.VAR_SUCCESS);
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
        [TranslateMsg("动作图编辑器中动作节点名称之获取卡牌法术伤害")]
        public const string EDITOR_NODE_NAME_GET_CARD_SPELL_DAMAGE = "获取卡牌法术伤害";
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
        [TranslateMsg("编辑器/卡牌-节点名称")]
        public const string EDITOR_NODE_NAME_GET_PILE_OBSOLETE = "获取牌堆（过时）";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_GET_PLAYER_TURN_COUNT = "获取玩家的当前回合数";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_IS_OFFENSIVE_PLAYER = "玩家是否是先手";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_GET_ALL_PLAYERS = "获取所有玩家";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_GET_FATIGUE = "获取疲劳层数";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_SET_FATIGUE = "设置疲劳层数";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_ADD_FATIGUE = "增加疲劳层数";
        [TranslateMsg("节点名称")]
        public const string EDITOR_NODE_TAKE_FATIGUE_DAMAGE = "受到疲劳伤害";


        [TranslateMsg("节点参数 TargetName")]
        const string EDITOR_PARAM_TARGET_NAME = "目标名";
        [TranslateMsg("节点参数-获取法术伤害/输入")]
        const string EDITOR_PARAM_NAME_SPELL_CARD = "法术牌";
        [TranslateMsg("节点参数")]
        const string EDITOR_PARAM_NAME_SET_GEM_EVENT = "设置灵力值事件";
        [TranslateMsg("节点参数")]
        const string EDITOR_PARAM_NAME_SET_MAX_GEM_EVENT = "设置最大灵力值事件";
        [TranslateMsg("节点参数")]
        const string EDITOR_PARAM_NAME_FAITH_DUEL_EVENT = "信仰对决事件";
        [TranslateMsg("节点参数")]
        const string EDITOR_PARAM_NAME_FATIGUE_EVENT = "疲劳事件";
        [TranslateMsg("节点参数")]
        const string EDITOR_PARAM_FATIGUE_COUNT = "疲劳层数"; 


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
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
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
        public static void setPlayerProp(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value)
        {
            player?.setProp(game, propName, value);
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

        [EventNode(typeof(DrawEventDefine), typeof(BurnEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.DrawTargetCard, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_DRAW_TARGET_CARD)]
        public static Task drawTargetCard(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card targetCard,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DRAW_EVENT)][NodeParamEventType(typeof(DrawEventDefine))] out EventArg outEventDraw,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BURN_EVENT)][NodeParamEventType(typeof(BurnEventDefine))] out EventArg outEventBurn)
        {
            if (targetCard == null)
            {
                outEventDraw = null;
                outEventBurn = null;
                return Task.CompletedTask;
            }
            return targetCard.owner.draw(game, targetCard, out outEventDraw, out outEventBurn);
        }

        ///从一定数量卡牌中选一张
        [ObsoletedKeyMethod(BuiltinActionDefineID.SelectOneFromCards, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_SELECT_ONE_FROM_MANY_CARDS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static Task<Card> selectOneFromManyCards(CardEngine game, Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count)
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
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count)
        {
            if (defines == null)
                return null;
            return game.askSelect(card.owner, defines, count);
        }
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPlayerSpellDamage, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_GET_SPELL_DAMAGE, "GetSpellDamage")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DAMAGE)]
        public static int getSpellDamage(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BASE_DAMAGE)] int baseDamage,
            [ActionNodeParam(EDITOR_PARAM_NAME_SPELL_CARD)] Card spellCard)
        {
            if (player == null)
                return 0;
            return player.getSpellDamage(game, baseDamage, spellCard);
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
        [EventNode(typeof(SetCardPropEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.ResetSkill, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_RESET_SKILL)]
        public static async Task resetSkill(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(EDITOR_PARAM_NAME_CARD_PROP_CHANGE_EVENT, isOut: true)][NodeParamEventType(typeof(SetCardPropEventDefine))] EventArg outEvent)
        {
            if ((player != null) && player.getSkill().isUsed(game))
            {
                outEvent = await player.getSkill().setUsed(game, false);
                setOutParam(flow, node, nameof(outEvent), outEvent);
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
        [EventNode(typeof(SetCardPropEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.DestroyItem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_DESTROY_ITEM)]
        public static async Task destroyItem(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(EDITOR_PARAM_NAME_CARD_PROP_CHANGE_EVENT, isOut: true)][NodeParamEventType(typeof(SetCardPropEventDefine))] EventArg outEvent)
        {
            if (player == null || player.getItem() == null)
                return;
            outEvent = await player.getItem().kill(game);
            setOutParam(flow, node, nameof(outEvent), outEvent);
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
        [EventNode(typeof(SetGemEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetPlayerGem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_ADD_AMOUNT_OF_GEM)]
        public static async Task setGem(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(EDITOR_PARAM_NAME_SET_GEM_EVENT, isOut: true)][NodeParamEventType(typeof(SetGemEventDefine))] EventArg outEvent)
        {
            if (player == null)
                return;
            outEvent = await player.setGem(game, count);
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }

        /// <summary>
        /// 增加当前灵力值
        /// </summary>
        [EventNode(typeof(SetGemEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddPlayerGem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_ADD_GEM)]
        public static async Task addGem(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(EDITOR_PARAM_NAME_SET_GEM_EVENT, isOut: true)][NodeParamEventType(typeof(SetGemEventDefine))] EventArg outEvent)
        {
            if (player == null)
                return;
            outEvent = await player.setGem(game, player.getGem() + count);
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }

        /// <summary>
        /// 设置灵力上限
        /// </summary>
        [EventNode(typeof(SetMaxGemEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetPlayerMaxGem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_SET_MAX_GEM)]
        public static async Task setMaxGem(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(EDITOR_PARAM_NAME_SET_MAX_GEM_EVENT, isOut: true)][NodeParamEventType(typeof(SetMaxGemEventDefine))] EventArg outEvent)
        {
            if (player == null)
                return;
            outEvent = await player.setMaxGem(game, count);
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }

        /// <summary>
        /// 增加灵力上限
        /// </summary>
        [EventNode(typeof(SetMaxGemEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddPlayerMaxGem, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_ADD_MAX_GEM)]
        public static async Task addMaxGem(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int count,
            [ActionNodeParam(EDITOR_PARAM_NAME_SET_MAX_GEM_EVENT, isOut: true)][NodeParamEventType(typeof(SetMaxGemEventDefine))] EventArg outEvent)
        {
            if (player == null)
                return;
            outEvent = await player.setMaxGem(game, player.getMaxGem() + count);
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }


        /// <summary>
        /// 信仰对决（旧）
        /// </summary>
        [Obsolete]
        [THHActionMethod(BuiltinActionDefineID.FaithDuel_Obsolete, StringTable.EDITOR_NODE_CATEGORY_PLAYER, "信仰对决（过时）", EDITOR_NODE_FAITHDUEL)]
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
            flow.setValue(node.getOutputPort<ValueOutput>(nameof(selfCard)), arg.getVar<Card>(RevealDuelEventDefine.VAR_SELF_CARD));
            flow.setValue(node.getOutputPort<ValueOutput>(nameof(opponentCard)), arg.getVar<Card>(RevealDuelEventDefine.VAR_OPPO_CARD));
            return arg.getVar<bool>(RevealDuelEventDefine.VAR_RESULT);
        }

        /// <summary>
        /// 信仰对决
        /// </summary>
        [EventNode(typeof(RevealDuelEventDefine))]
        [THHActionMethod(BuiltinActionDefineID.FaithDuel, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_FAITHDUEL)]
        [return: ActionNodeParam(EDITOR_NODE_FAITHDUEL_WIN)]
        public static async Task<bool> faithduel(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(EDITOR_NODE_FAITHDUEL_FILTER)] NodeValueRef<bool> filter,
            [ActionNodeParam(EDITOR_NODE_FAITHDUEL_FILTER_CARD, isOut: true)] Card element,
            [ActionNodeParam(EDITOR_NODE_FAITHDUEL_SELF_CARD, isOut: true)] Card selfCard,
            [ActionNodeParam(EDITOR_NODE_FAITHDUEL_OPPONENT_CARD, isOut: true)] Card opponentCard,
            [ActionNodeParam(EDITOR_PARAM_NAME_FAITH_DUEL_EVENT, isOut: true)][NodeParamEventType(typeof(RevealDuelEventDefine))] EventArg outEvent)
        {
            if (player == null)
                return false;

            bool filterFunc(Card c)
            {
                bool valid = true;
                if (filter.outputPort != null)
                {
                    var elementPort = node.getOutputPort<ValueOutput>(nameof(element));
                    flow.EnterScope();
                    flow.setValue(elementPort, c);
                    valid = filter.getValue(flow).Result;
                    flow.ExitScope();
                }
                return valid;
            }
            outEvent = await player.revealDuel(game, filterFunc);

            setOutParam(flow, node, nameof(selfCard), outEvent.getVar<Card>(RevealDuelEventDefine.VAR_SELF_CARD));
            setOutParam(flow, node, nameof(opponentCard), outEvent.getVar<Card>(RevealDuelEventDefine.VAR_OPPO_CARD));
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return outEvent.getVar<bool>(RevealDuelEventDefine.VAR_RESULT);
        }
        /// <summary>
        /// 获取所有玩家
        /// </summary>
        [THHActionMethod(BuiltinActionDefineID.GetAllPlayers, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_GET_ALL_PLAYERS)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)]
        public static Player[] getAllPlayers(CardEngine game)
        {
            return game.getSortedPlayers();
        }

        /// <summary>
        /// 获取牌堆（过时）
        /// </summary>
        /// <param name="player"></param>
        /// <param name="pileName"></param>
        /// <returns></returns>
        [ObsoletedKeyMethod(BuiltinActionDefineID.GetPile_Obsolete, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_NAME_GET_PILE_OBSOLETE, "GetPile")]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE)]
        [Obsolete]
        public static Pile getPileObsolete(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player, 
            [ActionNodeParam(EDITOR_PARAM_PILE_NAME)] string pileName)
        {
            return player.getPile(pileName);
        }
        /// <summary>
        /// 获取牌堆
        /// </summary>
        /// <param name="player"></param>
        /// <param name="pileName"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.GetPile, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_GET_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE)]
        public static Pile getPile(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(EDITOR_PARAM_PILE_NAME)] PileName pileName)
        {
            return player.getPile(pileName);
        }


        [ObsoletedKeyMethod(ObsoletedActionDefineID.GetTarget, StringTable.EDITOR_NODE_CATEGORY_PLAYER, StringTable.EDITOR_NODE_NAME_GET_TARGET)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_TARGET_CARD)]
        [Obsolete]
        public static Card getTarget(EventArg eventArg, 
            [ActionNodeParam(EDITOR_PARAM_TARGET_NAME)] string targetName)
        {
            if (targetName == "Target")
                return eventArg.getVar<Card[]>(EVENTVAR.TARGET_CARD)[0];
            throw new NotImplementedException();
        }

        [THHActionMethod(BuiltinActionDefineID.GetPlayerTurnCount, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_GET_PLAYER_TURN_COUNT)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_TURN_COUNT)]
        public static int getPlayerTurnCount(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player == null)
                return 0;
            return player.getTurnCount();
        }

        [THHActionMethod(BuiltinActionDefineID.IsOffensivePlayer, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_IS_OFFENSIVE_PLAYER)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BOOLEAN)]
        public static bool isOffensivePlayer(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (game == null || player == null)
                return false;
            return game.getSortedPlayers().FirstOrDefault() == player;
        }


        [THHActionMethod(BuiltinActionDefineID.GetFatigue, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_GET_FATIGUE)]
        [return: ActionNodeParam(EDITOR_PARAM_FATIGUE_COUNT)]
        public static int getFatigue(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player)
        {
            if (player == null)
                return 0;
            return player.getFatigue();
        }

        [THHActionMethod(BuiltinActionDefineID.SetFatigue, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_SET_FATIGUE)]
        public static void setFatigue(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int value)
        {
            if (game == null || player == null)
                return;
            player.setFatigue(game, value);
        }
        [THHActionMethod(BuiltinActionDefineID.AddFatigue, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_ADD_FATIGUE)]
        public static void addFatigue(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)] int value)
        {
            if (game == null || player == null)
                return;
            player.setFatigue(game, player.getFatigue() + value);
        }
        [THHActionMethod(BuiltinActionDefineID.TakeFatigueDamage, StringTable.EDITOR_NODE_CATEGORY_PLAYER, EDITOR_NODE_TAKE_FATIGUE_DAMAGE)]
        public static async Task takeFatigueDamage(Flow flow, ActionNode node, CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player,
            [ActionNodeParam(EDITOR_PARAM_NAME_FATIGUE_EVENT, isOut: true)][NodeParamEventType(typeof(FatigueEventDefine))] EventArg outEvent)
        {
            if (game == null || player == null)
                return;
            outEvent = await player.fatigue(game);
            setOutParam(flow, node, nameof(outEvent), outEvent);
        }

        #endregion

        //  有关牌堆的操作，比如说洗牌，获取特定牌等等
        #region 牌堆
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_CHECK_CARD_PILE_OBSOLETE = "卡牌所在牌堆判断（过时）";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_CHECK_CARD_PILE = "卡牌所在牌堆判断";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_GET_PILE_NAME = "获取牌堆名";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_GET_OWNER_OF_PILE = "获取牌堆所属玩家";
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
        [ObsoletedKeyMethod(ObsoletedActionDefineID.GetCardsInDeckWithCondition, StringTable.EDITOR_NODE_CATEGORY_PILE, StringTable.EDITOR_NODE_NAME_GET_CARDS_IN_DECK_WITH_CONDITION)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        [Obsolete]
        public static async Task<Card[]> getCardsInDeckWithCondition(CardEngine game, Card card, IBuff buff, IEventArg eventArg, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CONDITION)] ControlInput condition)
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
        [ObsoletedKeyMethod(ObsoletedActionDefineID.GetCardsWithCondition, StringTable.EDITOR_NODE_CATEGORY_PILE, StringTable.EDITOR_NODE_NAME_FILTER_CARDS_WITH_CONDITION)]
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
        [ObsoletedKeyMethod(ObsoletedActionDefineID.GetCardsCount, StringTable.EDITOR_NODE_CATEGORY_PILE, StringTable.EDITOR_NODE_NAME_GET_CARDS_COUNT)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_COUNT)]
        [Obsolete]
        public static int getCardsCount([ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards)
        {
            if (cards == null)
                return 0;
            return cards.Length;
        }

        /// <summary>
        /// 卡牌所在牌堆判断（过时）
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        [Obsolete]
        [ObsoletedKeyMethod(BuiltinActionDefineID.CheckCardPile_Obsolete, StringTable.EDITOR_NODE_CATEGORY_PILE, EDITOR_NODE_NAME_CHECK_CARD_PILE_OBSOLETE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool checkCardPileObsolete(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE)] string pileName)
        {
            return card?.pile?.name == pileName;
        }

        /// <summary>
        /// 卡牌所在牌堆判断
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.CheckCardPile, StringTable.EDITOR_NODE_CATEGORY_PILE, EDITOR_NODE_NAME_CHECK_CARD_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)]
        public static bool checkCardPile(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(EDITOR_PARAM_PILE_NAME)] PileName pileName)
        {
            return card?.pile?.name == pileName;
        }

        [ObsoletedKeyMethod(ObsoletedActionDefineID.IsPlayerPileChanged, StringTable.EDITOR_NODE_CATEGORY_PILE, StringTable.EDITOR_NODE_NAME_IS_PLAYER_PILE_CHANGED)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_IS_CHANGED)]
        [Obsolete]
        public static bool isPlayerPileChanged(EventArg moveCardEvent, 
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)] Player player, 
            [ActionNodeParam(EDITOR_PARAM_PILE_NAME)] string pileName)
        {
            var from = moveCardEvent.getVar<Pile>(MoveCardEventDefine.VAR_FROM_PILE);
            var to = moveCardEvent.getVar<Pile>(MoveCardEventDefine.VAR_TO_PILE);
            return (from.owner == player && from.name == pileName) ||
                (to.owner == player && to.name == pileName);
        }
        /// <summary>
        /// 获取牌堆名
        /// </summary>
        /// <param name="pile"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.GetPileName, StringTable.EDITOR_NODE_CATEGORY_PILE, EDITOR_NODE_NAME_GET_PILE_NAME)]
        [return: ActionNodeParam(EDITOR_PARAM_PILE_NAME)]
        public static string getPileName(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE)] Pile pile)
        {
            if (pile == null)
                return null;
            return pile.name;
        }
        /// <summary>
        /// 获取牌堆所属玩家
        /// </summary>
        /// <param name="pile"></param>
        /// <returns></returns>
        [THHActionMethod(BuiltinActionDefineID.GetOwnerOfPile, StringTable.EDITOR_NODE_CATEGORY_PILE, EDITOR_NODE_NAME_GET_OWNER_OF_PILE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PLAYER)]
        public static Player getOwnerOfPile(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PILE)] Pile pile)
        {
            if (pile == null)
                return null;
            return pile.owner;
        }
        #endregion
        //  与增益相关
        #region 增益
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_GET_BUFF_PROP = "获取增益属性";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_GET_DEFINE_OF_BUFF = "获取单个增益的定义";
        [TranslateMsg("编辑器-节点名称")]
        private const string EDITOR_NODE_NAME_GET_ALL_BUFFS_ON_CARD = "获取卡牌上的所有增益";
        [TranslateMsg("节点参数")]
        private const string EDITOR_PARAM_NAME_REMOVE_BUFF_EVENT = "移除增益事件";
        [TranslateMsg("节点参数")]
        private const string EDITOR_PARAM_NAME_ENTER_FIELD_EVENT = "入场事件";
        [TranslateMsg("节点参数")]
        private const string EDITOR_PARAM_NAME_MASS_ADD_BUFF_EVENT = "群体添加增益事件";
        [TranslateMsg("节点参数")]
        private const string EDITOR_PARAM_NAME_MASS_ENTER_FIELD_EVENT = "群体入场事件";
        [TranslateMsg("节点参数")]
        private const string EDITOR_PARAM_NAME_MASS_RETURN_HAND_EVENT = "群体进入手牌事件";
        [TranslateMsg("节点参数")]
        private const string EDITOR_PARAM_NAME_MASS_SHUFFLE_TO_DECK_EVENT = "群体洗入牌库事件";
        [TranslateMsg("节点参数")]
        private const string EDITOR_PARAM_NAME_MASS_MOVE_TO_HEAD_EVENT = "群体置入延迟区事件";
        [TranslateMsg("节点参数")]
        private const string EDITOR_PARAM_NAME_MASS_MOVE_CARD_EVENT = "群体移动卡牌事件";

        [ObsoletedKeyMethod(BuiltinActionDefineID.ThisBuff, StringTable.EDITOR_NODE_CATEGORY_BUFF, StringTable.EDITOR_NODE_NAME_THIS_BUFF)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)]
        public static Buff thisBuff(Buff buff)
        {
            return buff;
        }

        [EventNode(typeof(MassAddBuffEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.AddBuff, StringTable.EDITOR_NODE_CATEGORY_BUFF, StringTable.EDITOR_NODE_NAME_ADD_BUFF)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)]
        public static async Task<Buff[]> addBuff(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF_DEFINE)] BuffDefine buffDefine,
            [ActionNodeParam(EDITOR_PARAM_NAME_MASS_ADD_BUFF_EVENT, isOut: true)][NodeParamEventType(typeof(MassAddBuffEventDefine))] EventArg outEvent)
        {
            if (cards == null || buffDefine == null)
                return Array.Empty<Buff>();
            outEvent = await cards.addBuffs(game, buffDefine);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return outEvent.getVar<Buff[]>(MassAddBuffEventDefine.VAR_BUFF);
        }

        [EventNode(typeof(RemoveBuffEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.RemoveBuff, StringTable.EDITOR_NODE_CATEGORY_BUFF, StringTable.EDITOR_NODE_NAME_REMOVE_BUFF)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_IS_BUFF_REMOVED)]
        public static async Task<bool> removeBuff(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)] Buff buff,
            [ActionNodeParam(EDITOR_PARAM_NAME_REMOVE_BUFF_EVENT, isOut: true)][NodeParamEventType(typeof(RemoveBuffEventDefine))] EventArg outEvent)
        {
            if (card == null || buff == null)
                return false;

            outEvent = await card.removeBuff(game, buff);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return outEvent?.getVar<bool>(RemoveBuffEventDefine.VAR_REMOVED) ?? false;
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

        [EventNode(typeof(BuffPropertyChangeEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.SetBuffProp, StringTable.EDITOR_NODE_CATEGORY_BUFF, StringTable.EDITOR_NODE_NAME_SET_BUFF_PROP)]
        public static async Task setBuffProp(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)] Buff[] buffs,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_VALUE)] object value,
            [ActionNodeParam(EDITOR_PARAM_NAME_BUFF_PROP_CHANGED_EVENT, isOut: true)][NodeParamEventType(typeof(BuffPropertyChangeEventDefine))] EventArg[] outEvent)
        {
            if (buffs == null)
                return;

            var list = new List<EventArg>();
            for (int i = 0; i < buffs.Length; i++)
            {
                if (buffs[i] == null)
                    continue;
                list.Add(await buffs[i].setProp(game, propName, value));
            }
            outEvent = list.ToArray();
            setOutParam(flow, node, nameof(outEvent), outEvent);
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
        public static object getBuffProp(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)] Buff buff,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_PROPERTY_NAME)] string propName)
        {
            if (buff == null)
                return default;
            return buff.getProp(propName);
        }

        [THHActionMethod(BuiltinActionDefineID.GetBuffDefineOfBuff, StringTable.EDITOR_NODE_CATEGORY_BUFF, EDITOR_NODE_NAME_GET_DEFINE_OF_BUFF)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF_DEFINE)]
        public static BuffDefine getDefineOfBuff(CardEngine game,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)] Buff buff)
        {
            if (buff is GeneratedBuff genBuff)
                return game.getBuffDefine(genBuff.defineRef);
            return null;
        }
        [THHActionMethod(BuiltinActionDefineID.GetAllBuffsOnCard, StringTable.EDITOR_NODE_CATEGORY_BUFF, EDITOR_NODE_NAME_GET_ALL_BUFFS_ON_CARD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BUFF)]
        public static Buff[] getAllBuffsOnCard(
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)] Card card)
        {
            return card.getBuffs();
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
        /// <param name="outEventDraw"></param>
        /// <returns></returns>
        [EventNode(typeof(DrawEventDefine), typeof(BurnEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.SimpleDraw, StringTable.EDITOR_CATEGORY__ACTION__NAME, StringTable.EDITOR_NODE__SIMPLE_DRAW_CARD__NAME)]
        [return: ActionNodeParam(StringTable.EDITOR_NODE__SIMPLE_DRAW_CARD__DRAW_EVENT)]
        public static Card simpleDrawCard(CardEngine game, 
            [ActionNodeParam(StringTable.EDITOR_NODE__SIMPLE_DRAW_CARD__PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_DRAW_EVENT)][NodeParamEventType(typeof(DrawEventDefine))] out EventArg outEventDraw,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_BURN_EVENT)][NodeParamEventType(typeof(BurnEventDefine))] out EventArg outEventBurn)
        {
            outEventDraw = null;
            outEventBurn = null;
            if (player == null)
                return null;
            player.draw(game, out outEventDraw, out outEventBurn);
            return outEventDraw?.getVar<Card>(DrawEventDefine.VAR_CARD);
        }

        /// <summary>
        /// 卡牌置入战场
        /// </summary>
        [EventNode(typeof(MassEnterFieldEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToField, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_FIELD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> moveToField(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card[] cards,
            [ActionNodeParam(EDITOR_PARAM_NAME_MASS_ENTER_FIELD_EVENT, isOut: true)][NodeParamEventType(typeof(MassEnterFieldEventDefine))] EventArg outEvent)
        {
            if (cards == null)
                return Array.Empty<Card>();
            if (player == null)
                return cards;
            outEvent = await cards.moveToField(game, player);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return outEvent?.getVar<Card[]>(MassEnterFieldEventDefine.VAR_MOVED_CARDS) ?? Array.Empty<Card>();
        }

        /// <summary>
        /// 卡牌移回手牌
        /// </summary>
        [EventNode(typeof(MassReturnHandEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToHand, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_HAND)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> moveToHand(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card[] cards,
            [ActionNodeParam(EDITOR_PARAM_NAME_MASS_RETURN_HAND_EVENT, isOut: true)][NodeParamEventType(typeof(MassReturnHandEventDefine))] EventArg outEvent)
        {
            if (cards == null)
                return Array.Empty<Card>();
            if (player == null)
                return cards;
            outEvent = await cards.returnToHand(game, player);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return outEvent?.getVar<Card[]>(MassEnterFieldEventDefine.VAR_MOVED_CARDS) ?? Array.Empty<Card>();
        }

        /// <summary>
        /// 卡牌从洗入牌库
        /// </summary>
        [EventNode(typeof(MassShuffleToDeckEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToDeck, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_DECK)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> moveToDeck(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card[] cards,
            [ActionNodeParam(StringTable.EDITOR_NODE_NAME_IS_DECK_TOP)] bool top,
            [ActionNodeParam(EDITOR_PARAM_NAME_MASS_SHUFFLE_TO_DECK_EVENT, isOut: true)][NodeParamEventType(typeof(MassShuffleToDeckEventDefine))] EventArg outEvent)
        {
            if (cards == null)
                return Array.Empty<Card>();
            if (player == null)
                return cards;
            outEvent = await cards.moveToDeck(game, player, top);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return outEvent?.getVar<Card[]>(MassEnterFieldEventDefine.VAR_MOVED_CARDS) ?? Array.Empty<Card>();
        }

        /// <summary>
        /// 卡牌从置入墓地
        /// </summary>
        [EventNode(typeof(MassMoveCardEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToGrave, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_GRAVE)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> moveToGrave(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card[] cards,
            [ActionNodeParam(EDITOR_PARAM_NAME_MASS_MOVE_CARD_EVENT, isOut: true)][NodeParamEventType(typeof(MassMoveCardEventDefine))] EventArg outEvent)
        {
            if (cards == null)
                return Array.Empty<Card>();
            if (player == null)
                return cards;

            outEvent = await cards.moveToGrave(game, player);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return outEvent?.getVar<Card[]>(MassEnterFieldEventDefine.VAR_MOVED_CARDS) ?? Array.Empty<Card>();
        }

        /// <summary>
        /// 卡牌置入延迟区
        /// </summary>
        [EventNode(typeof(MassMoveToHeadEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToHead, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_HEAD)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> moveToHead(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card[] cards,
            [ActionNodeParam(EDITOR_PARAM_NAME_MASS_MOVE_TO_HEAD_EVENT, isOut: true)][NodeParamEventType(typeof(MassMoveToHeadEventDefine))] EventArg outEvent)
        {
            if (cards == null)
                return Array.Empty<Card>();
            if (player == null)
                return cards;

            outEvent = await cards.moveToHead(game, player);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return outEvent?.getVar<Card[]>(MassEnterFieldEventDefine.VAR_MOVED_CARDS) ?? Array.Empty<Card>();
        }

        /// <summary>
        /// 卡牌装备到道具栏(有显示问题）
        /// </summary>
        [EventNode(typeof(EquipItemEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToItem, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_ITEM)]
        [return: ActionNodeParam(EDITOR_PARAM_NAME_EQUIP_EVENT)]
        [return: NodeParamEventType(typeof(EquipItemEventDefine))]
        public static async Task<EventArg> moveToItem(CardEngine game,
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
        [EventNode(typeof(MassMoveCardEventDefine))]
        [ObsoletedKeyMethod(BuiltinActionDefineID.MoveToWarp, StringTable.EDITOR_CATEGORY__ACTION__NAME, EDITOR_NODE_NAME_CARD_MOVE_TO_WARP)]
        [return: ActionNodeParam(StringTable.EDITOR_PARAM_NAME_CARD)]
        public static async Task<Card[]> moveToWARP(CardEngine game, Flow flow, ActionNode node,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_EFFECT_PLAYER)] Player player,
            [ActionNodeParam(StringTable.EDITOR_PARAM_NAME_MOVED_CARD)] Card[] cards,
            [ActionNodeParam(EDITOR_PARAM_NAME_MASS_MOVE_CARD_EVENT, isOut: true)][NodeParamEventType(typeof(MassMoveCardEventDefine))] EventArg outEvent)
        {
            if (cards == null)
                return Array.Empty<Card>();
            if (player == null)
                return cards;
            outEvent = await cards.moveToWarp(game, player);
            setOutParam(flow, node, nameof(outEvent), outEvent);
            return outEvent?.getVar<Card[]>(MassEnterFieldEventDefine.VAR_MOVED_CARDS) ?? Array.Empty<Card>();
        }
        #endregion
    }

    /// <summary>
    /// 自动从内置ID获取节点类型的动作定义属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class THHActionMethodAttribute : ActionNodeMethodAttribute
    {
        public THHActionMethodAttribute(int id, string category, string editorName, params string[] obsoleteNames) : base(id, category, editorName, id.getNodeDefineType(), obsoleteNames)
        {
        }
    }
    /// <summary>
    /// 用于将过时的MethodActionDefine转换为以内置名为主键的动作定义。
    /// 主要用于向ObsoletedNames添加当前的编辑器名称，以实现卡池转换。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ObsoletedKeyMethodAttribute : THHActionMethodAttribute
    {
        public ObsoletedKeyMethodAttribute(int id, string category, string editorName, params string[] obsoleteNames) : base(id, category, editorName, obsoleteNames.Append(editorName).ToArray())
        {
        }
    }
    /// <summary>
    /// 用于确定一个EventArg类型的节点参数是什么事件。
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class NodeParamEventTypeAttribute : NodeParamMetaAttribute
    {
        public NodeParamEventTypeAttribute(Type eventType) : base()
        {
            this.eventType = eventType;
        }
        public override IPortMeta toPortMeta()
        {
            return new EventTypePortMeta(this);
        }
        public Type eventType { get; }
    }
    public class EventTypePortMeta : IPortMeta
    {
        public EventTypePortMeta(NodeParamEventTypeAttribute attr)
        {
            eventType = attr.eventType;
        }
        public Type eventType { get; }
    }
}
