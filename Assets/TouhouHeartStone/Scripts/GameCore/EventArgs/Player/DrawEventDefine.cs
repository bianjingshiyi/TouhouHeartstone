using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(MoveCardEventDefine))]
    public class DrawEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public DrawEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_PLAYER, type=typeof(Player) },
                new EventVariableInfo() { name = VAR_CARD, type=typeof(Card) }
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_PLAYER, type=typeof(Player) },
                new EventVariableInfo() { name = VAR_CARD, type=typeof(Card) }
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(VAR_PLAYER, arg);
            record.setCardState(VAR_CARD, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(VAR_PLAYER);
            var card = arg.getVar<Card>(VAR_CARD);
            if (card.isSpell() && card.hasKeyword(game, KEYWORD.AUTOCAST))
            {
                await player.autocast(game, card);
            }
            else
            {
                var from = player.getDeck();
                var to = player.getHand();
                await from.moveTo(game, card, player.getHand(), to.count);
                game.logger.logTrace($"{player}抽{card}");
            }
        }

        public override string toString(EventArg arg)
        {
            var player = arg.getVar<Player>(VAR_PLAYER);
            var card = arg.getVar<Card>(VAR_CARD);
            return string.Format(TEMPLATE_SELF, player, card);
        }
        public static EventArg newEvent(CardEngine game, Player player, Card card)
        {
            var define = game.getEventDefine<DrawEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_PLAYER, player);
            arg.setVar(VAR_CARD, card);
            return arg;
        }

        #region 私有方法

        #region 接口实现
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            var player = eventArg.getVar<Player>(VAR_PLAYER);
            var card = eventArg.getVar<Card>(VAR_CARD);
            return viewer == player ? card : player.getMaster();
        }
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        ICard ICardEventDefine.getCard(IEventArg arg)
        {
            var card = arg.getVar<Card>(VAR_CARD);
            return card;
        }

        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer)
        {
            IDescribableEventDefine arg = this;
            return string.Format(arg.localizeTemplateString(game, eventArg, viewer), arg.localizeStringArgs(game, eventArg, viewer));
        }

        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer)
        {
            if (!eventArg.isCanceled)
            {
                var player = eventArg.getVar<Player>(VAR_PLAYER);
                return viewer == player ? TEMPLATE_SELF : TEMPLATE_OTHER;
            }
            else
            {
                return string.Empty;
            }
        }
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer)
        {
            var player = eventArg.getVar<Player>(VAR_PLAYER);
            var card = eventArg.getVar<Card>(VAR_CARD);
            return new object[] {
                    player.name,
                    card.getFormatString()
                };
        }
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(VAR_PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => Translations.EVENT_LOG_ITEM_NAME_DRAW;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion

        #endregion

        [MukioI18n.TranslateMsg("{0}玩家，{1}卡")]
        const string TEMPLATE_SELF = "{0}抽{1}";
        [MukioI18n.TranslateMsg("{0}玩家")]
        const string TEMPLATE_OTHER = "抽了一张牌";
        public const string VAR_CARD = EVENTVAR.CARD;
        public const string VAR_PLAYER = EVENTVAR.PLAYER;
    }
}
