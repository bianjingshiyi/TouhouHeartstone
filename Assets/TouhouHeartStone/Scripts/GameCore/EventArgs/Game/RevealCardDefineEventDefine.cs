using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using UnityEngine;

namespace TouhouHeartstone
{
    public class RevealCardDefineEventDefine : EventDefine, IDescribableEventDefine
    {
        public RevealCardDefineEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_CARD_DEFINE,type=typeof(CardDefine)},
                new EventVariableInfo(){name = VAR_PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = VAR_INVISIBLE_FOR_SELF,type=typeof(bool)},
                new EventVariableInfo(){name = VAR_INVISIBLE_FOR_OPPO,type=typeof(bool)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_CARD_DEFINE,type=typeof(CardDefine)},
                new EventVariableInfo(){name = VAR_PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = VAR_INVISIBLE_FOR_SELF,type=typeof(bool)},
                new EventVariableInfo(){name = VAR_INVISIBLE_FOR_OPPO,type=typeof(bool)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(VAR_CARD_DEFINE, arg);
            record.setVar(VAR_PLAYER, arg);
            record.setVar(VAR_INVISIBLE_FOR_SELF, arg);
            record.setVar(VAR_INVISIBLE_FOR_OPPO, arg);
        }
        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            var cardDefine = arg.getVar<CardDefine>(VAR_CARD_DEFINE);
            game.logger.logTrace(cardDefine != null ? $"揭示了{cardDefine.getFormatString()}" : "什么也没揭示");
            return Task.CompletedTask;
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, CardDefine cardDefine, Player player, bool invisibleForSelf = false, bool invisibleForOppo = false)
        {
            var define = game.getEventDefine<RevealCardDefineEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_CARD_DEFINE, cardDefine);
            arg.setVar(VAR_PLAYER, player);
            arg.setVar(VAR_INVISIBLE_FOR_SELF, invisibleForSelf);
            arg.setVar(VAR_INVISIBLE_FOR_OPPO, invisibleForOppo);
            return game.triggers.doEvent(arg);
        }
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg arg, IPlayer viewer) => arg.getVar<CardDefine>(VAR_CARD_DEFINE);
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg arg, IPlayer viewer) => null;
        string IDescribableEventDefine.toString(IGame game, EventArg arg, IPlayer viewer) => toString(arg);
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg arg, IPlayer viewer) => localizeStringArgs(arg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg arg, IPlayer viewer) => localizeTemplateString();
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(VAR_PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => Translations.EVENT_LOG_ITEM_NAME_DISPLAY;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        private object[] localizeStringArgs(EventArg arg)
        {
            var player = arg.getVar<Player>(VAR_PLAYER);
            var cardDefine = arg.getVar<CardDefine>(VAR_CARD_DEFINE);
            return new object[]
            {
                player.name,
                cardDefine?.getFormatString()
            };
        }
        private string localizeTemplateString()
        {
            return TEMPLATE_VALID;
        }

        [TranslateMsg("{0}是玩家，{1}是卡")]
        const string TEMPLATE_VALID = "{0} 展示了 {1}";
        public const string VAR_CARD_DEFINE = EVENTVAR.CARD_DEFINE;
        public const string VAR_PLAYER = EVENTVAR.PLAYER;
        public const string VAR_INVISIBLE_FOR_SELF = EVENTVAR.INVISIBLE_TO_SELF;
        public const string VAR_INVISIBLE_FOR_OPPO = EVENTVAR.INVISIBLE_TO_OPPO;
    }
}
