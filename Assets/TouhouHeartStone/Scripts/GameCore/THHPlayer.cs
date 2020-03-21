using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone
{
    public class THHPlayer : Player
    {
        public Card master { get; }
        public Card skill { get; }
        public Pile deck { get; }
        public Pile init { get; }
        public Pile hand { get; }
        public Pile field { get; }
        public Pile grave { get; }
        public bool isPrepared { get; set; } = false;
        public int gem { get; private set; } = 0;
        public int maxGem { get; private set; } = 0;
        public int fatigue { get; private set; } = 0;
        public THHPlayer(THHGame game, int id, string name, MasterCardDefine master, IEnumerable<CardDefine> deck) : base(id, name)
        {
            this.master = game.createCard(master);
            pileList.Add(new Pile("Master", new Card[] { this.master }, 1));
            skill = game.createCardById(master.skillID);
            pileList.Add(new Pile("Skill", new Card[] { skill }, 1));
            this.deck = new Pile("Deck", deck.Select(d => game.createCard(d)).ToArray());
            pileList.Add(this.deck);
            init = new Pile("Init", maxCount: 4);
            pileList.Add(init);
            hand = new Pile("Hand", maxCount: 10);
            pileList.Add(hand);
            field = new Pile("Field", maxCount: 7);
            pileList.Add(field);
            grave = new Pile("Grave");
            pileList.Add(grave);
        }
        public async Task initReplace(THHGame game, params Card[] cards)
        {
            if (isPrepared)
                throw new AlreadyPreparedException(this);
            await game.triggers.doEvent(new InitReplaceEventArg() { player = this, cards = cards }, arg =>
            {
                arg.replacedCards = arg.player.init.replaceByRandom(game, arg.cards, arg.player.deck);
                //玩家准备完毕
                arg.player.isPrepared = true;
                game.logger.log(arg.player + "替换卡牌：" + string.Join("，", arg.cards.Select(c => c.ToString())) + "=>"
                    + string.Join("，", arg.replacedCards.Select(c => c.ToString())));
                return Task.CompletedTask;
            });
            //判断是否所有玩家都准备完毕
            if (game.players.All(p => { return p.isPrepared; }))
            {
                //对战开始
                await game.start();
            }
        }
        public class InitReplaceEventArg : EventArg
        {
            public THHPlayer player;
            public Card[] cards;
            public Card[] replacedCards;
        }
        public Task setGem(THHGame game, int value)
        {
            return game.triggers.doEvent(new SetGemEventArg() { player = this, value = value }, arg =>
            {
                arg.player.gem = arg.value;
                if (arg.player.gem < 0)
                    arg.player.gem = 0;
                if (arg.player.gem > arg.player.maxGem)
                    arg.player.gem = arg.player.maxGem;
                game.logger.log(arg.player + "的法力水晶变为" + arg.player.gem);
                return Task.CompletedTask;
            });
        }
        public class SetGemEventArg : EventArg
        {
            public THHPlayer player;
            public int value;
        }
        public Task setMaxGem(THHGame game, int value)
        {
            return game.triggers.doEvent(new SetMaxGemEventArg() { player = this, value = value }, arg =>
            {
                arg.player.maxGem = arg.value;
                if (arg.player.maxGem < 0)
                    arg.player.maxGem = 0;
                if (arg.player.maxGem > 10)
                    arg.player.maxGem = 10;
                if (arg.player.gem > arg.player.maxGem)
                    arg.player.gem = 0;
                game.logger.log(arg.player + "的法力水晶上限变为" + arg.player.maxGem);
                return Task.CompletedTask;
            });
        }
        public class SetMaxGemEventArg : EventArg
        {
            public THHPlayer player;
            public int value;
        }
        public Task draw(THHGame game)
        {
            if (deck.count < 1)//无牌可抽，疲劳！
            {
                return game.triggers.doEvent(new FatigueEventArg() { player = this }, arg =>
                {
                    arg.player.fatigue++;
                    game.logger.log(arg.player + "已经没有卡牌了，当前疲劳值：" + arg.player.fatigue);
                    return arg.player.master.damage(game, arg.player.fatigue);
                });
            }
            else if (hand.count >= hand.maxCount)
            {
                return game.triggers.doEvent(new BurnEventArg() { player = this }, arg =>
                {
                    arg.card = arg.player.deck.top;
                    arg.player.deck.moveTo(arg.card, arg.player.grave, arg.player.grave.count);
                    game.logger.log(arg.player + "的手牌已经满了，" + arg.card + "被送入墓地");
                    return Task.CompletedTask;
                });
            }
            else
            {
                return game.triggers.doEvent(new DrawEventArg() { player = this }, arg =>
                {
                    arg.card = arg.player.deck.top;
                    arg.player.deck.moveTo(arg.card, arg.player.hand, arg.player.hand.count);
                    game.logger.log(arg.player + "抽" + arg.card);
                    return Task.CompletedTask;
                });
            }
        }
        public class DrawEventArg : EventArg
        {
            public THHPlayer player;
            public Card card;
        }
        public class BurnEventArg : EventArg
        {
            public THHPlayer player;
            public Card card;
        }
        public class FatigueEventArg : EventArg
        {
            public THHPlayer player;
        }
        public async Task<bool> tryUse(THHGame game, Card card, int position, params Card[] targets)
        {
            if (game.currentPlayer != this)//不是你的回合
                return false;
            if (card.define is ServantCardDefine servant)
            {
                if (gem < card.getCost())//费用不够
                    return false;
            }
            else if (card.define is SpellCardDefine spell)
            {
                if (gem < card.getCost())
                    return false;
            }
            else
            {
                return false;//不知道是什么卡
            }
            await setGem(game, gem - card.getCost());
            await game.triggers.doEvent(new UseEventArg() { player = this, card = card, position = position, targets = targets }, async arg =>
            {
                game.logger.log(arg.player + "使用" + arg.card);
                if (card.define is ServantCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == CardDefineType.servant))
                {
                    //随从卡，将卡置入战场
                    await trySummon(game, arg.player.hand, arg.card, arg.position);
                    Effect effect = arg.card.define.getEffectOn<BattleCryEventArg>();
                    if (effect != null)
                    {
                        await game.triggers.doEvent(new BattleCryEventArg() { player = arg.player, card = arg.card, effect = effect, targets = arg.targets }, arg2 =>
                        {
                            return arg2.effect.execute(game, arg2.player, arg2.card, new object[0], arg2.targets);
                        });
                    }
                }
                else if (card.define is SpellCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == CardDefineType.spell))
                {
                    //法术卡，释放效果然后丢进墓地
                    //player["Hand"].moveTo(card, player["Warp"], player["Warp"].count);
                    //if (card.define.effects != null && card.define.effects.Length > 0)
                    //{
                    //    Effect effect = card.define.effects.FirstOrDefault(e => { return card.pile.name == e.pile && e.trigger == "onUse"; });
                    //    if (effect != null)
                    //        effect.execute(engine, player, card, targetCards);
                    //}
                    //player["Warp"].moveTo(card, player["Grave"], player["Grave"].count);
                }
            });
            return true;
        }
        public class UseEventArg : EventArg
        {
            public THHPlayer player;
            public Card card;
            public int position;
            public Card[] targets;
        }
        public class BattleCryEventArg : EventArg
        {
            public THHPlayer player;
            public Card card;
            public Effect effect;
            public Card[] targets;
        }
        public async Task<bool> trySummon(THHGame game, Pile from, Card card, int position)
        {
            if (field.count >= field.maxCount)//没位置了
                return false;
            await game.triggers.doEvent(new SummonEventArg() { player = this, from = from, card = card, position = position }, arg =>
            {
                game.logger.log(arg.player + "从" + arg.from + "召唤" + arg.card + "，位于" + arg.position);
                arg.from.moveTo(arg.card, arg.player.field, arg.position);
                if (card.define is ServantCardDefine servant)
                {
                    card.setCurrentLife(servant.life);
                    card.setReady(false);
                }
                return Task.CompletedTask;
            });
            return true;
        }
        public class SummonEventArg : EventArg
        {
            public THHPlayer player;
            public Pile from;
            public Card card;
            public int position;
        }
        #region Command
        public void cmdInitReplace(IAnswerManager manager, params Card[] cards)
        {
            manager.unaskedAnswer(id, new InitReplaceResponse()
            {
                cardsId = cards.Select(c => c.id).ToArray()
            });
        }
        public void cmdUse(IAnswerManager manager, Card card, int position, params Card[] targets)
        {
            manager.unaskedAnswer(id, new UseResponse()
            {
                cardId = card.id,
                position = position,
                targetsId = targets.Select(c => c.id).ToArray()
            });
        }
        public void cmdTurnEnd(IAnswerManager manager)
        {
            manager.unaskedAnswer(id, new TurnEndResponse()
            {
            });
        }
        public void cmdAttack(IAnswerManager manager, Card card, Card target)
        {
            manager.unaskedAnswer(id, new AttackResponse()
            {
                cardId = card.id,
                targetId = target.id
            });
        }
        #endregion
    }
}