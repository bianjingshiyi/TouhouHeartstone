using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(SetCardPropEventDefine))]
    public class HealEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public HealEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                new EventVariableInfo(){name = VAR_HEAL_SOURCE,type=typeof(Card)},
                new EventVariableInfo(){name = EVENTVAR.VALUE,type=typeof(int)},
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                new EventVariableInfo(){name = VAR_HEAL_SOURCE,type=typeof(Card)},
                new EventVariableInfo(){name = EVENTVAR.VALUE,type=typeof(int)},
                new EventVariableInfo(){name = VAR_HEALED_VALUE,type=typeof(int)},
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(EVENTVAR.CARD, arg);
            record.setCardState(VAR_HEAL_SOURCE, arg);
            record.setVar(EVENTVAR.VALUE, arg);
            record.setVar(VAR_HEALED_VALUE, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            var value = arg.getVar<int>(EVENTVAR.VALUE);
            if (card == null)
                return;
            int healedValue = Math.Min(value, card.getLife(game) - card.getCurrentLife(game));
            if (healedValue > 0)
            {
                await card.setCurrentLife(game, card.getCurrentLife(game) + healedValue);
                game.logger.logTrace($"{card}恢复{healedValue}点生命值，生命值=>{card.getCurrentLife(game)}");
            }
            else
            {
                healedValue = 0;
                game.logger.logTrace($"{card}没有恢复生命值，生命值=>{card.getCurrentLife(game)}");
            }
            arg.setVar(VAR_HEALED_VALUE, healedValue);
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Card card, Card source, int value)
        {
            var define = game.getEventDefine<HealEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.CARD, card);
            arg.setVar(VAR_HEAL_SOURCE, source);
            arg.setVar(EVENTVAR.VALUE, value);
            return game.triggers.doEvent(arg);
        }
        #region 私有方法

        #region 接口实现
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return eventArg.getVar<Card>(EVENTVAR.CARD);
        }

        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        ICard ICardEventDefine.getCard(IEventArg eventArg)
        {
            return eventArg.getVar<Card>(EVENTVAR.CARD);
        }
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString();
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Card>(EVENTVAR.CARD).getOwner();
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => new object[] { eventArg.getVar<int>(VAR_HEALED_VALUE) };
        #endregion
        private string localizeTemplateString()
        {
            return TEMPLATE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            var healedValue = eventArg.getVar<int>(VAR_HEALED_VALUE);
            return new object[] { card.getFormatString(), healedValue };
        }

        #endregion
        [TranslateMsg("{0}是卡，{1}是治疗量")]
        const string TEMPLATE = "治疗 {0} {1} 点生命值";
        public const string VAR_HEAL_SOURCE = "治疗来源";
        public const string VAR_HEALED_VALUE = "实际治疗值";
        [TranslateMsg("游戏事件日志项名称-治疗事件（{0}为获得的生命值）")]
        private const string LOG_ITEM_NAME = "+{0}";
    }
}
