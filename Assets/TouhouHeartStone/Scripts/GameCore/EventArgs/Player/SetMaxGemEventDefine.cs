using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class SetMaxGemEventDefine : EventDefine, IDescribableEventDefine
    {
        public SetMaxGemEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() {name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo() {name = EVENTVAR.VALUE,type=typeof(int)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() {name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo() {name = EVENTVAR.VALUE,type=typeof(int)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setVar(EVENTVAR.VALUE, arg);
        }
        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            var value = arg.getVar<int>(EVENTVAR.VALUE);
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);

            value = Math.Clamp(value, 0, 10);
            player.setProp(game, PLAYER.MAXGEM, value);
            game.logger.logTrace(toString(arg as EventArg));
            return Task.CompletedTask;
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, int value)
        {
            var define = game.getEventDefine<SetMaxGemEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, player);
            arg.setVar(EVENTVAR.VALUE, value);
            return game.triggers.doEvent(arg);
        }
        #region 私有方法

        #region 接口实现
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString();
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(EVENTVAR.PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion
        private string localizeTemplateString() => TEMPLATE;
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var value = eventArg.getVar<int>(EVENTVAR.VALUE);
            return new object[] {
                player.name,
                value
            };
        }

        #endregion
        [MukioI18n.TranslateMsg("{0}是玩家名称，{1}是灵力值")]
        const string TEMPLATE = "{0}的最大灵力值变为{1}";
        [TranslateMsg("游戏事件日志项名称")]
        const string LOG_ITEM_NAME = "灵力变化";
    }
}
