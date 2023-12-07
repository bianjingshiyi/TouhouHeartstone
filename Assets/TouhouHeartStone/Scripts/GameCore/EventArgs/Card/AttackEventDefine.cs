using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(DamageEventDefine))]
    public class AttackEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public AttackEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                new EventVariableInfo(){name = EVENTVAR.TARGET_CARD,type=typeof(Card)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                new EventVariableInfo(){name = EVENTVAR.TARGET_CARD,type=typeof(Card)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(EVENTVAR.CARD, arg);
            record.setCardState(EVENTVAR.TARGET_CARD, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            var target = arg.getVar<Card>(EVENTVAR.TARGET_CARD);
            if (!card.canAttackTest(game))
            {
                game.logger.logTrace($"{card}无法进行攻击");
                arg.isCanceled = true;
            }
            else if (card == target)
            {
                game.logger.log(card + "尝试攻击自己");
                arg.isCanceled = true;
            }
            else
            {
                game.logger.logTrace($"{card}攻击{target}");
                int targetAttack = target.getAttack(game);
                int cardAttack = card.getAttack(game);
                await card.damage(game, target, targetAttack);
                await target.damage(game, card, cardAttack);

                // 取消隐匿
                if (card.isStealth(game))
                {
                    await card.setStealth(game, false);
                }
                // 消耗耐久
                var owner = card.getOwner();
                if (owner != null && owner.getMaster() == card)
                {
                    var item = owner.getItem();
                    if (item != null)
                    {
                        await item.damageItem(game, 1);
                    }
                }
            }
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Card card, Card target)
        {
            var define = game.getEventDefine<AttackEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.CARD, card);
            arg.setVar(EVENTVAR.TARGET_CARD, target);
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
            return new DescribableCard[] { eventArg.getVar<Card>(EVENTVAR.TARGET_CARD) };
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
            var target = eventArg.getVar<Card>(EVENTVAR.TARGET_CARD);
            return new object[] {
                card.getFormatString(),
                target.getFormatString()
            };
        }

        #endregion
        [TranslateMsg("{0} {1}是卡")]
        private const string TEMPLATE = "{0} 攻击 {1}";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "攻击";
    }
}
