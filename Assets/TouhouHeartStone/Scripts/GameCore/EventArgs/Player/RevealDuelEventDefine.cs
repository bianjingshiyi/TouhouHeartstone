using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class RevealDuelEventDefine : EventDefine, IDescribableEventDefine
    {
        #region 公有方法
        public RevealDuelEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
                new EventVariableInfo() { name = VAR_SELF_CARD, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_OPPO_CARD, type = typeof(Card) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
                new EventVariableInfo() { name = VAR_SELF_CARD, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_OPPO_CARD, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_RESULT, type = typeof(bool) },
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setVar(VAR_SELF_CARDS, arg);
            record.setVar(VAR_OPPO_CARDS, arg);
            record.setCardState(VAR_SELF_CARD, arg);
            record.setCardState(VAR_OPPO_CARD, arg);
            record.setVar(VAR_RESULT, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var duelPlayer = arg.getVar<Player>(EVENTVAR.PLAYER);
            var selfCards = arg.getVar<Card[]>(VAR_SELF_CARDS);
            var oppoCards = arg.getVar<Card[]>(VAR_OPPO_CARDS);
            var selfCard = selfCards.random(game);
            var oppoCard = oppoCards.random(game);

            arg.setVar(VAR_SELF_CARD, selfCard);
            arg.setVar(VAR_OPPO_CARD, oppoCard);
            await game.Reveal(selfCard, false);
            await game.Reveal(oppoCard, false);

            bool result = false;
            if (selfCard != null && oppoCard == null)
                result = true;
            else if (selfCard != null && selfCard.getCost(game) > oppoCard.getCost(game))
                result = true;

            arg.setVar(VAR_RESULT, result);

            if (selfCard != null)
                game.logger.log($"{duelPlayer}发动了对决，她揭示了：{selfCard}。她的对手揭示了：{oppoCard}。结果是{result}");
            else
                game.logger.log($"{duelPlayer}揭示了个寂寞，输掉了对决。");
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(arg), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, Card[] selfCards, Card[] enemyCards)
        {
            var define = game.getEventDefine<RevealDuelEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, player);
            arg.setVar(VAR_SELF_CARDS, selfCards);
            arg.setVar(VAR_OPPO_CARDS, enemyCards);
            return game.triggers.doEvent(arg);
        }
        #endregion

        #region 私有方法

        #region 接口实现
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg arg, IPlayer viewer)
        {
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            return player.getMaster();
        }
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg arg, IPlayer viewer)
        {
            var youCard = arg.getVar<Card>(VAR_SELF_CARD);
            var oppoCard = arg.getVar<Card>(VAR_OPPO_CARD);
            return new DescribableCard[] { youCard, oppoCard };
        }
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return toString(eventArg);
        }

        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return localizeTemplateString(eventArg);
        }
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return localizeStringArgs(eventArg);
        }
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(EVENTVAR.PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion
        private string localizeTemplateString(EventArg eventArg)
        {
            var youCard = eventArg.getVar<Card>(VAR_SELF_CARD);
            return youCard != null ? TEMPLATE : TEMPLATE_NULL;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var result = eventArg.getVar<bool>(VAR_RESULT);
            var youCard = eventArg.getVar<Card>(VAR_SELF_CARD);
            var oppoCard = eventArg.getVar<Card>(VAR_OPPO_CARD);
            return new object[] {
                    player.name,
                    youCard?.getFormatString(),
                    oppoCard?.getFormatString(),
                    result?"胜利":"失败"
                };
        }

        #endregion

        [TranslateMsg("{0}是玩家，{1}是自己的卡，{2}是对手的卡，{3}是结果")]
        const string TEMPLATE = "{0}发动了对决，揭示了:{1}。 对手揭示了：{2}。 结果是 {3}";
        [TranslateMsg("{0}是玩家")]
        const string TEMPLATE_NULL = "{0}发动了对决 揭示了个寂寞，输掉了对决。";
        [TranslateMsg("游戏事件日志项名称", Translations.CONTEXT_GAME_LOG_NAME)]
        private const string LOG_ITEM_NAME = "信仰对决";
        public const string VAR_SELF_CARDS = "己方可选卡牌";
        public const string VAR_OPPO_CARDS = "对方可选卡牌";
        public const string VAR_SELF_CARD = "己方卡牌";
        public const string VAR_OPPO_CARD = "对方卡牌";
        public const string VAR_RESULT = "是否胜利";
    }
}
