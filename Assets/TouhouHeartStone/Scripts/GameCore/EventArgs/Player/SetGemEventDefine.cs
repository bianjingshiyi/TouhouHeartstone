using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class SetGemEventDefine : EventDefine, IDescribableEventDefine
    {
        public SetGemEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() {name = VAR_PLAYER, type = typeof(Player)},
                new EventVariableInfo() {name = VAR_BEFORE_VALUE, type = typeof(int)},
                new EventVariableInfo() {name = VAR_VALUE, type = typeof(int)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() {name = VAR_PLAYER, type = typeof(Player)},
                new EventVariableInfo() {name = VAR_BEFORE_VALUE, type = typeof(int)},
                new EventVariableInfo() {name = VAR_VALUE, type = typeof(int)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(VAR_PLAYER, arg);
            record.setVar(VAR_VALUE, arg);
        }
        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            var value = arg.getVar<int>(VAR_VALUE);
            var player = arg.getVar<Player>(VAR_PLAYER);

            value = Math.Clamp(value, 0, 10);
            arg.setVar(VAR_VALUE, value);

            player.setProp(game, PLAYER.GEM, value);
            game.logger.logTrace($"{player}的灵力值变为{value}");
            return Task.CompletedTask;
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, int value)
        {
            var define = game.getEventDefine<SetGemEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_PLAYER, player);
            arg.setVar(VAR_BEFORE_VALUE, player.getGem());
            arg.setVar(VAR_VALUE, value);
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
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(VAR_PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => Translations.EVENT_LOG_ITEM_NAME_SET_GEM;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion
        private string localizeTemplateString() => TEMPLATE;
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(VAR_PLAYER);
            var value = eventArg.getVar<int>(VAR_VALUE);
            return new object[] {
                    player.name,
                    value
                };
        }

        #endregion
        [TranslateMsg("{0}是玩家名称，{1}是灵力值")]
        const string TEMPLATE = "{0}的灵力值变为{1}";
        public const string VAR_PLAYER = EVENTVAR.PLAYER;
        public const string VAR_BEFORE_VALUE = EVENTVAR.BEFORE_VALUE;
        public const string VAR_VALUE = EVENTVAR.VALUE;
    }
}
