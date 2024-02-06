using System;
using System.Linq;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(SetCardPropEventDefine))]
    public class SilenceEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public SilenceEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(EVENTVAR.CARD, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            if (card == null)
                return;
            await card.setProp(game, CARD.SILENCE, true);
            await card.removeAllBuffs(game);
            foreach (var effect in card.define.getEffects().Where(e => e is IPassiveEffect))
            {
                await effect.disable(game, card, null);
            }
            await card.setKeywords(game, new string[0]);
            await card.setSpellDamage(game, 0);
            await card.setDamageReduce(game, 0);
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Card card)
        {
            var define = game.getEventDefine<SilenceEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.CARD, card);
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
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion

        private string localizeTemplateString()
        {
            return TEMPLATE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            return new object[] { string.Join("，", card.getFormatString()) };
        }

        #endregion

        [TranslateMsg("{0}是卡牌列表")]
        const string TEMPLATE = "{0} 被封印";
        [TranslateMsg("游戏事件日志项名称", Translations.CONTEXT_GAME_LOG_NAME)]
        private const string LOG_ITEM_NAME = "封印";
    }
}
