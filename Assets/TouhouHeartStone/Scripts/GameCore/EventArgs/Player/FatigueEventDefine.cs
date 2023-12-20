using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(DamageEventDefine))]
    public class FatigueEventDefine : EventDefine, IDescribableEventDefine
    {
        public FatigueEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_PLAYER,type=typeof(Player) },
                new EventVariableInfo() { name = VAR_DAMAGE,type=typeof(int) }
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_PLAYER,type=typeof(Player) },
                new EventVariableInfo() { name = VAR_DAMAGE,type=typeof(int) }
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(VAR_PLAYER, arg);
            record.setVar(VAR_DAMAGE, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(VAR_PLAYER);
            var damage = arg.getVar<int>(VAR_DAMAGE);
            game.logger.logTrace($"{player}已经没有卡牌了，当前疲劳值：{damage}");
            player.setFatigue(game, player.getFatigue() + 1);
            await player.getMaster().damage(game, null, damage);
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static EventArg newEvent(CardEngine game, Player player, int damage)
        {
            var define = game.getEventDefine<FatigueEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_PLAYER, player);
            arg.setVar(VAR_DAMAGE, damage);
            return arg;
        }
        public static EventArg newEvent(CardEngine game, Player player)
        {
            return newEvent(game, player, player.getFatigue() + 1);
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, int damage)
        {
            var arg = newEvent(game, player, damage);
            return game.triggers.doEvent(arg);
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player)
        {
            var arg = newEvent(game, player);
            return game.triggers.doEvent(arg);
        }
        #region 私有方法

        #region 接口实现
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            var player = eventArg.getVar<Player>(VAR_PLAYER);
            return player.getMaster();
        }

        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }

        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString();
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(VAR_PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion
        private string localizeTemplateString()
        {
            return TEMPLATE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(VAR_PLAYER);
            return new object[] { player.name };
        }

        #endregion

        [TranslateMsg("{0}是玩家")]
        const string TEMPLATE = "{0}已经无牌可抽，即将受到疲劳伤害";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "疲劳";
        public static string VAR_PLAYER => EVENTVAR.PLAYER;
        public const string VAR_DAMAGE = "伤害";
    }
}
