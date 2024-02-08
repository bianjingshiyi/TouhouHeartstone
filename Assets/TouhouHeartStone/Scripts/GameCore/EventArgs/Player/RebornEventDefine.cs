using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(EnterFieldEventDefine))]
    public class RebornEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public RebornEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = VAR_CARD,type=typeof(Card)},
                new EventVariableInfo(){name = VAR_POSITION,type=typeof(int)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = VAR_CARD,type=typeof(Card)},
                new EventVariableInfo(){name = VAR_POSITION,type=typeof(int)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(VAR_PLAYER, arg);
            record.setCardState(VAR_CARD, arg);
            record.setVar(VAR_POSITION, arg);
        }
        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(VAR_PLAYER);
            var card = arg.getVar<Card>(VAR_CARD);
            var position = arg.getVar<int>(VAR_POSITION);
            game.logger.logTrace($"{player}复活{card.define.GetType().Name}位于{position}");
            return player.tryEnterField(game, player.getGrave(), card, position);
        }

        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, Card card, int position)
        {
            var define = game.getEventDefine<RebornEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_PLAYER, player);
            arg.setVar(VAR_CARD, card);
            arg.setVar(VAR_POSITION, position);
            return game.triggers.doEvent(arg);
        }

        #region 私有方法

        #region 接口实现

        ICard ICardEventDefine.getCard(IEventArg arg)
        {
            return arg.getVar<Card>(VAR_CARD);
        }
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return eventArg.getVar<Card>(VAR_CARD);
        }
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString();
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(VAR_PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => Translations.EVENT_LOG_ITEM_NAME_REVIVE;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion

        private string localizeTemplateString()
        {
            return TEMPLATE;
        }

        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(VAR_PLAYER);
            var card = eventArg.getVar<Card>(VAR_CARD);
            return new object[]
            {
                player.name,
                card.getFormatString()
            };
        }

        #endregion

        [TranslateMsg("{0}是玩家，{1}是卡")]
        const string TEMPLATE = "{0} 复活了 {1}";
        public const string VAR_CARD = EVENTVAR.CARD;
        public const string VAR_PLAYER = EVENTVAR.PLAYER;
        public const string VAR_POSITION = EVENTVAR.POSITION;
    }
}
