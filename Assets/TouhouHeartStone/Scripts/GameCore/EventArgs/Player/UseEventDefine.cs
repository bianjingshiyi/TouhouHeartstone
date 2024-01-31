using System;
using System.Linq;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class UseEventDefine : EventDefine, IDescribableEventDefine
    {
        public UseEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.PLAYER, type=typeof(Player)},
                new EventVariableInfo() { name = EVENTVAR.CARD, type=typeof(Card)},
                new EventVariableInfo() { name = EVENTVAR.POSITION, type=typeof(int)},
                new EventVariableInfo() { name = EVENTVAR.COST, type=typeof(int)},
                new EventVariableInfo() { name = VAR_TARGETS, type = typeof(Card[]) }
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo() { name = EVENTVAR.CARD,type=typeof(Card)},
                new EventVariableInfo() { name = EVENTVAR.POSITION,type=typeof(int)},
                new EventVariableInfo() { name = EVENTVAR.COST,type=typeof(int)},
                new EventVariableInfo() { name = VAR_TARGETS, type = typeof(Card[]) }
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setCardState(EVENTVAR.CARD, arg);
            record.setVar(EVENTVAR.POSITION, arg);
            record.setCardStates(VAR_TARGETS, arg);
            record.setVar(EVENTVAR.COST, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var card = arg.getVar<Card>(EVENTVAR.CARD); ;
            var targets = arg.getVar<Card[]>(VAR_TARGETS);
            var position = arg.getVar<int>(EVENTVAR.POSITION);
            var targetLog = targets.Length > 0 ? $"，目标：{string.Join<Card>("，", targets)}" : null;
            game.logger.logTrace($"{player}使用{card}{targetLog}");
            if (card.isServant())
            {
                await player.tryEnterField(game, player.getHand(), card, position);//随从卡，将卡置入战场
                await card.triggerActiveEffect(game, targets);//执行随从效果
            }
            else if (card.isSkill())
            {
                await card.triggerActiveEffect(game, targets);
            }
            else if (card.isSpell())
            {
                //法术卡,释放效果
                await player.getHand().moveTo(game, card, player.getWarp());
                if (card.isTrap(game) || card.hasKeyword(game, KEYWORD.MISSION))
                {
                    await player.tryEnterHead(game, player.getWarp(), card);
                }
                else
                {
                    await card.triggerActiveEffect(game, targets);
                    await player.getWarp().moveTo(game, card, player.getGrave());//送入墓地
                }
            }
            else if (card.isItem())
            {
                //物品卡，置入物品栏
                await player.equip(game, player.getHand(), card);
                await card.triggerActiveEffect(game, targets);
            }
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(arg), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, Card card, int position, Card[] targets, int cost)
        {
            var define = game.getEventDefine<UseEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, player);
            arg.setVar(EVENTVAR.CARD, card);
            arg.setVar(EVENTVAR.POSITION, position);
            arg.setVar(VAR_TARGETS, targets);
            arg.setVar(EVENTVAR.COST, cost);
            return game.triggers.doEvent(arg);
        }
        #region 私有方法

        #region 接口实现
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            if (card.isTrap(game))
                return viewer == player ? card : player.getMaster();
            return card;
        }

        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            var targets = eventArg.getVar<Card[]>(VAR_TARGETS);
            return targets.Select(t => (DescribableCard)t).ToArray();
        }

        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer)
        {
            IDescribableEventDefine arg = this;
            return string.Format(arg.localizeTemplateString(game, eventArg, viewer), arg.localizeStringArgs(game, eventArg, viewer));
        }
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            if (card.isTrap(game))
            {
                return viewer == player ? TEMPLATE_NO_TARGET : TEMPLATE_SECRET_OTHER;
            }
            return localizeTemplateString(eventArg);
        }

        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(EVENTVAR.PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion

        private string localizeTemplateString(EventArg eventArg)
        {
            var targets = eventArg.getVar<Card[]>(VAR_TARGETS);
            if (targets == null || targets.Length < 1)
                return TEMPLATE_NO_TARGET;
            else
                return TEMPLATE_HAS_TARGET;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            var targets = eventArg.getVar<Card[]>(VAR_TARGETS);
            return new object[]
            {
                player.name,
                card?.getFormatString(),
                string.Join("，", targets?.Select(c => c.getFormatString()))
            };
        }

        #endregion

        [TranslateMsg("{0}是玩家，{1}是卡。")]
        const string TEMPLATE_NO_TARGET = "{0} 使用 {1}";
        [TranslateMsg("{0}是玩家，使用的是陷阱")]
        const string TEMPLATE_SECRET_OTHER = "{0} 使用了一张陷阱牌";
        [TranslateMsg("{0}是玩家，{1}是卡，{2}是目标")]
        const string TEMPLATE_HAS_TARGET = "{0} 对 {2} 使用了 {1}";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "使用";

        public static string VAR_TARGETS => EVENTVAR.TARGET_CARD;
    }
}
