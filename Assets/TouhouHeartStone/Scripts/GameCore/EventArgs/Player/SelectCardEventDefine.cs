using System;
using System.Linq;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 选择卡牌事件
    /// </summary>
    public class SelectCardEventDefine : EventDefine, IDescribableEventDefine
    {
        public SelectCardEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() {name = VAR_PLAYER,type=typeof(Player)},
                new EventVariableInfo() {name = VAR_OPTIONS,type=typeof(Card[])}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() {name = VAR_PLAYER,type=typeof(Player)},
                new EventVariableInfo() {name = VAR_OPTIONS,type=typeof(Card[])},
                new EventVariableInfo() {name = VAR_CHOSEN_CARD,type=typeof(Card)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(VAR_PLAYER, arg);
            record.setCardStates(VAR_OPTIONS, arg);
            record.setCardState(VAR_CHOSEN_CARD, arg);
            record.setVar(VAR_CARD_SEEN, arg);
            record.setVar(VAR_COUNT, arg);
            record.setVar(VAR_TITLE, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(VAR_PLAYER);
            var options = arg.getVar<Card[]>(VAR_OPTIONS);
            var count = arg.getVar<int>(VAR_COUNT);
            var title = arg.getVar<string>(VAR_TITLE);

            var cards = options.Where(c => c != null);
            if (cards.Count() > 0)
            {
                if (count > 0)
                {
                    cards = options.randomTake(game, count);
                }
                game.logger.logTrace($"{player}从{string.Join("，", cards)}中选择一张");
                var selected = await game.askSelect(player, cards, count, title);
                arg.setVar(VAR_CHOSEN_CARD, selected);
            }
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(arg), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, Card[] options, int count = 3, string title = null, bool isShow = false)
        {
            var define = game.getEventDefine<SelectCardEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_PLAYER, player);
            arg.setVar(VAR_OPTIONS, options);
            arg.setVar(VAR_COUNT, count);
            arg.setVar(VAR_TITLE, title);
            arg.setVar(VAR_CARD_SEEN, isShow);
            return game.triggers.doEvent(arg);
        }
        #region 私有方法

        #region 接口实现
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            var chosen = eventArg.getVar<Card>(VAR_CHOSEN_CARD);
            return chosen;
        }
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString(eventArg);
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(VAR_PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => Translations.EVENT_LOG_ITEM_NAME_SELECT_CARD;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion
        private string localizeTemplateString(EventArg eventArg)
        {
            var seen = eventArg.getVar<bool>(VAR_CARD_SEEN);
            if (seen)
                return Translations.EVENT_LOG_DESC_TEMPLATE_SELECT;
            else
                return Translations.EVENT_LOG_DESC_TEMPLATE_SELECT_NOT_SHOW;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(VAR_PLAYER);
            var chosen = eventArg.getVar<Card>(VAR_CHOSEN_CARD);
            return new object[]
            {
                player.name,
                chosen?.getFormatString()
            };
        }
        #endregion

        public const string VAR_PLAYER = EVENTVAR.PLAYER;
        public const string VAR_CHOSEN_CARD = EVENTVAR.CHOSEN_CARD;
        public const string VAR_OPTIONS = EVENTVAR.OPTIONS;
        public const string VAR_CARD_SEEN = EVENTVAR.CARD_SEEN;
        public const string VAR_COUNT = EVENTVAR.OPTION_COUNT;
        public const string VAR_TITLE = EVENTVAR.TITLE;
    }
}
