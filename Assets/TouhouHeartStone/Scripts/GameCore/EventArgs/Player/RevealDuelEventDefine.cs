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
                new EventVariableInfo() { name = VAR_PLAYER, type = typeof(Player) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_PLAYER, type = typeof(Player) },
                new EventVariableInfo() { name = VAR_SELF_CARD, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_OPPO_CARD, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_RESULT, type = typeof(bool) },
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(VAR_PLAYER, arg);
            record.setVar(VAR_SELF_CARDS, arg);
            record.setVar(VAR_OPPO_CARDS, arg);
            record.setCardState(VAR_SELF_CARD, arg);
            record.setCardState(VAR_OPPO_CARD, arg);
            record.setVar(VAR_RESULT, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var duelPlayer = arg.getVar<Player>(VAR_PLAYER);
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
            arg.setVar(VAR_PLAYER, player);
            arg.setVar(VAR_SELF_CARDS, selfCards);
            arg.setVar(VAR_OPPO_CARDS, enemyCards);
            return game.triggers.doEvent(arg);
        }
        #endregion

        #region 私有方法

        #region 接口实现
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg arg, IPlayer viewer)
        {
            var player = arg.getVar<Player>(VAR_PLAYER);
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
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(VAR_PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => Translations.EVENT_LOG_ITEM_NAME_FAITHDUEL;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion
        private string localizeTemplateString(EventArg eventArg)
        {
            var youCard = eventArg.getVar<Card>(VAR_SELF_CARD);
            var result = eventArg.getVar<bool>(VAR_RESULT);
            if (youCard == null)
                return TEMPLATE_NULL;
            if (result)
                return TEMPLATE_WIN;
            else
                return TEMPLATE_LOSE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(VAR_PLAYER);
            var youCard = eventArg.getVar<Card>(VAR_SELF_CARD);
            var oppoCard = eventArg.getVar<Card>(VAR_OPPO_CARD);
            return new object[] {
                    player.name,
                    youCard?.getFormatString(),
                    oppoCard?.getFormatString()
                };
        }

        #endregion

        [TranslateMsg("{0}是玩家，{1}是自己的卡，{2}是对手的卡")]
        const string TEMPLATE_WIN = "{0}发动了对决，揭示了:{1}。 对手揭示了：{2}。结果是胜利！";
        [TranslateMsg("{0}是玩家，{1}是自己的卡，{2}是对手的卡")]
        const string TEMPLATE_LOSE = "{0}发动了对决，揭示了:{1}。 对手揭示了：{2}。结果是失败。";
        [TranslateMsg("{0}是玩家")]
        const string TEMPLATE_NULL = "{0}发动了对决 揭示了个寂寞，输掉了对决。";
        public const string VAR_PLAYER = EVENTVAR.PLAYER;
        public const string VAR_SELF_CARD = EVENTVAR.SELF_CARD;
        public const string VAR_OPPO_CARD = EVENTVAR.OPPO_CARD;
        public const string VAR_RESULT = EVENTVAR.WIN;
        public const string VAR_SELF_CARDS = EVENTVAR.SELF_VALID_CARDS;
        public const string VAR_OPPO_CARDS = EVENTVAR.OPPO_VALID_CARDS;
    }
}
