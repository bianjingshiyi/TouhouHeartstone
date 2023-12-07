using System;
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
                new EventVariableInfo() {name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo() {name = VAR_OPTIONS,type=typeof(Card[])}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() {name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo() {name = VAR_OPTIONS,type=typeof(Card[])},
                new EventVariableInfo() {name = EVENTVAR.CHOSEN_CARD,type=typeof(Card)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setCardStates(VAR_OPTIONS, arg);
            record.setCardState(EVENTVAR.CHOSEN_CARD, arg);
            record.setVar(VAR_CARD_SEEN, arg);
            record.setVar(VAR_COUNT, arg);
            record.setVar(VAR_TITLE, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var options = arg.getVar<Card[]>(VAR_OPTIONS);
            var count = arg.getVar<int>(VAR_COUNT);
            var title = arg.getVar<string>(VAR_TITLE);

            var cards = options.randomTake(game, count);
            game.logger.logTrace($"{player}从{string.Join("，", cards)}中选择一张");
            var selected = await game.askSelect(player, cards, count, title);
            arg.setVar(EVENTVAR.CHOSEN_CARD, selected);
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(arg), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, Card[] options, int count = 3, string title = null, bool isShow = false)
        {
            var define = game.getEventDefine<SelectCardEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, player);
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
            var chosen = eventArg.getVar<Card>(EVENTVAR.CHOSEN_CARD);
            return chosen;
        }
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString(eventArg);
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(EVENTVAR.PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion
        private string localizeTemplateString(EventArg eventArg)
        {
            var seen = eventArg.getVar<bool>(VAR_CARD_SEEN);
            if (seen)
                return TEMPLATE;
            else
                return TEMPLATE_NotShow;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var chosen = eventArg.getVar<Card>(EVENTVAR.CHOSEN_CARD);
            return new object[]
            {
                player.name,
                chosen?.getFormatString()
            };
        }
        #endregion

        [MukioI18n.TranslateMsg("{0}是玩家，{1}是选项。")]
        const string TEMPLATE = "{0} 选择了 {1}";
        [MukioI18n.TranslateMsg("{0}是玩家")]
        const string TEMPLATE_NotShow = "{0} 进行了选择";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "选择";

        public const string VAR_OPTIONS = "选项";
        public const string VAR_CARD_SEEN = "明牌";
        public const string VAR_COUNT = "选项数量";
        public const string VAR_TITLE = "标题";
    }
}
