using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class RevealEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public RevealEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = VAR_CARD,type=typeof(Card)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = VAR_CARD,type=typeof(Card)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setCardState(VAR_CARD, arg);
            record.setVar(VAR_ANIMATE, arg);
        }
        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(VAR_CARD);
            game.logger.logTrace(card != null ? $"揭示了{card.getFormatString()}" : "什么也没揭示");
            return Task.CompletedTask;
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(arg), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, Card card, bool animate)
        {
            var define = game.getEventDefine<RevealEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, player);
            arg.setVar(VAR_CARD, card);
            arg.setVar(VAR_ANIMATE, animate);
            return game.triggers.doEvent(arg);
        }
        #region 私有方法

        #region 接口实现
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return eventArg.getVar<Card>(VAR_CARD);
        }

        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        ICard ICardEventDefine.getCard(IEventArg eventArg) => eventArg.getVar<Card>(VAR_CARD);
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString(eventArg);
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(EVENTVAR.PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion
        private string localizeTemplateString(EventArg eventArg)
        {
            var card = eventArg.getVar<Card>(VAR_CARD);
            return card == null ? TEMPLATE_INVALID : TEMPLATE_VALID;
        }

        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var card = eventArg.getVar<Card>(VAR_CARD);
            return new object[] { player.name, card?.getFormatString() };
        }
        #endregion

        [TranslateMsg("{0}是玩家，{1}是卡")]
        const string TEMPLATE_VALID = "{0} 揭示了 {1}";
        [TranslateMsg("{0}是玩家")]
        const string TEMPLATE_INVALID = "{0} 揭示了个寂寞";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "揭示";
        public const string VAR_ANIMATE = "播放动画";
        public static string VAR_CARD => EVENTVAR.CARD;
    }
}
