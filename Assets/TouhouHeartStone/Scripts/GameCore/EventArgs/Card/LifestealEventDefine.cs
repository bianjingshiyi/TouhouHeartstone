using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(HealEventDefine))]
    public class LifestealEventDefine : EventDefine, ICardEventDefine
    {
        public LifestealEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_HEAL_SOURCE,type=typeof(Card)},
                new EventVariableInfo(){name = VAR_HEAL_VALUE,type=typeof(int)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_HEAL_SOURCE,type=typeof(Card)},
                new EventVariableInfo(){name = VAR_HEAL_VALUE,type=typeof(int)},
                new EventVariableInfo(){name = VAR_HEALED_VALUE,type=typeof(int)}
            };
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var source = arg.getVar<Card>(VAR_HEAL_SOURCE);
            var healValue = arg.getVar<int>(VAR_HEAL_VALUE);
            await source.getOwner().getMaster().heal(game, source, healValue);
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Card source, int value)
        {
            var define = game.getEventDefine<LifestealEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_HEAL_SOURCE, source);
            arg.setVar(VAR_HEAL_VALUE, value);
            return game.triggers.doEvent(arg);
        }
        #region 私有方法

        #region 接口实现
        ICard ICardEventDefine.getCard(IEventArg eventArg)
        {
            return eventArg.getVar<Card>(VAR_HEAL_SOURCE);
        }
        #endregion

        private string localizeTemplateString()
        {
            return TEMPLATE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var source = eventArg.getVar<Card>(VAR_HEAL_SOURCE);
            var damagedValue = eventArg.getVar<int>(VAR_HEALED_VALUE);
            return new object[] {
                    source.getFormatString(),
                    damagedValue
                };
        }
        #endregion

        [TranslateMsg("吸血事件描述，{0}是吸血卡牌，{1}是治疗值")]
        public const string TEMPLATE = "{0} 触发吸血，为英雄恢复 {1} 点生命值";


        public const string VAR_HEAL_SOURCE = EVENTVAR.DRAIN_CARD;
        public const string VAR_HEAL_VALUE = EVENTVAR.VALUE;
        public const string VAR_HEALED_VALUE = EVENTVAR.HEALED_VALUE;
    }
}
