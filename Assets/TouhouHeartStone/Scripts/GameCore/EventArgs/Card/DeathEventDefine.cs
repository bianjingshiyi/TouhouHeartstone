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
                new EventVariableInfo(){name = VAR_CARD,type=typeof(Card)},
                new EventVariableInfo(){name = VAR_PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = VAR_POSITION,type=typeof(int)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_CARD,type=typeof(Card)},
                new EventVariableInfo(){name = VAR_PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = VAR_POSITION,type=typeof(int)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(VAR_CARD, arg);
            record.setVar(VAR_PLAYER, arg);
            record.setVar(VAR_POSITION, arg);
            record.setVar(VAR_DEATHRATTLE_BUFFS, arg);
            record.setVar(VAR_DEATHRATTLE_EFFECTS, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            Card card = arg.getVar<Card>(VAR_CARD);

            var deathrattles = card.getDeathRattles(game);
            Buff[] deathrattleBuffs = new Buff[deathrattles.Length];
            Effect[] deathrattleEffects = new Effect[deathrattles.Length];
            for (int i = 0; i < deathrattles.Length; i++)
            {
                var deathrattle = deathrattles[i];
                deathrattleBuffs[i] = deathrattle.buff;
                deathrattleEffects[i] = deathrattle.effect;
            }
            arg.setVar(VAR_DEATHRATTLE_BUFFS, deathrattleBuffs);
            arg.setVar(VAR_DEATHRATTLE_EFFECTS, deathrattleEffects);

            var pile = card.pile;
            if (pile?.name == PILE.FIELD)//随从阵亡
            {
                await pile.moveTo(game, card, card.getOwner().getGrave());
                game.logger.logTrace($"{card}阵亡");
            }
            else if (pile?.name == PILE.ITEM)//装备摧毁
            {
                await pile.moveTo(game, card, card.getOwner().getGrave());
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
            arg.setVar(VAR_CARD, card);
            arg.setVar(VAR_PLAYER, card.getOwner());
            arg.setVar(VAR_POSITION, card.pile.indexOf(card));
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
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Card>(VAR_CARD).getOwner();
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => Translations.EVENT_LOG_ITEM_NAME_DEATH;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;

        #endregion

        private string localizeTemplateString()
        {
            return TEMPLATE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var card = eventArg.getVar<Card>(VAR_CARD);
            return new object[] { card.getFormatString() };
        }

        #endregion
        [TranslateMsg("{0}是卡牌列表")]
        const string TEMPLATE = "{0} 死亡";
        public const string VAR_CARD = EVENTVAR.CARD;
        public const string VAR_PLAYER = EVENTVAR.PLAYER;
        public const string VAR_POSITION = EVENTVAR.POSITION;
        public const string VAR_DEATHRATTLE_BUFFS = EVENTVAR.LASTWORD_BUFFS;
        public const string VAR_DEATHRATTLE_EFFECTS = EVENTVAR.LASTWORD_EFFECTS;
    }
}
