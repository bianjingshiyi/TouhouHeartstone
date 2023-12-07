using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(MoveCardEventDefine))]
    public class DeathEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public DeathEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(EVENTVAR.CARD, arg);
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setVar(EVENTVAR.POSITION, arg);
            record.setVar(VAR_DEATHRATTLE_BUFFS, arg);
            record.setVar(VAR_DEATHRATTLE_EFFECTS, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            Card card = arg.getVar<Card>(EVENTVAR.CARD);

            var deathrattles = card.getDeathRattles(game);
            Buff[] deathrattleBuffs = new Buff[deathrattles.Length];
            IEffect[] deathrattleEffects = new IEffect[deathrattles.Length];
            for (int i = 0; i < deathrattles.Length; i++)
            {
                var deathrattle = deathrattles[i];
                deathrattleBuffs[i] = deathrattle.buff;
                deathrattleEffects[i] = deathrattle.effect;
            }
            arg.setVar(VAR_DEATHRATTLE_BUFFS, deathrattleBuffs);
            arg.setVar(VAR_DEATHRATTLE_EFFECTS, deathrattleEffects);

            if (card.pile.name == PILE.FIELD)//随从阵亡
            {
                await card.pile.moveTo(game, card, card.getOwner().getGrave());
                game.logger.logTrace($"{card}阵亡");
            }
            else if (card.pile.name == PILE.ITEM)//装备摧毁
            {
                await card.pile.moveTo(game, card, card.getOwner().getGrave());
                game.logger.logTrace($"{card}摧毁");
            }
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Card card)
        {
            var define = game.getEventDefine<DeathEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.CARD, card);
            arg.setVar(EVENTVAR.PLAYER, card.getOwner());
            arg.setVar(EVENTVAR.POSITION, card.pile.indexOf(card));
            return game.triggers.doEvent(arg);
        }
        #region 私有方法

        #region 接口实现
        ICard ICardEventDefine.getCard(IEventArg arg)
        {
            return arg.getVar<Card>(EVENTVAR.CARD);
        }
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return eventArg.getVar<Card>(EVENTVAR.CARD);
        }

        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }

        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString();
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Card>(EVENTVAR.CARD).getOwner();
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;

        #endregion

        private string localizeTemplateString()
        {
            return TEMPLATE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            return new object[] { card.getFormatString() };
        }

        #endregion
        [TranslateMsg("{0}是卡牌列表")]
        const string TEMPLATE = "{0} 死亡";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "死亡";
        public const string VAR_DEATHRATTLE_BUFFS = "遗言增益列表";
        public const string VAR_DEATHRATTLE_EFFECTS = "遗言效果列表";
    }
}
