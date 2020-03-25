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
        internal async Task initReplace(THHGame game, params Card[] cards)
        {
            await game.triggers.doEvent(new InitReplaceEventArg() { player = this, cards = cards }, arg =>
            {
                arg.replacedCards = arg.player.init.replaceByRandom(game, arg.cards, arg.player.deck);
                game.logger.log(arg.player + "替换卡牌：" + string.Join("，", arg.cards.Select(c => c.ToString())) + "=>"
                    + string.Join("，", arg.replacedCards.Select(c => c.ToString())));
                return Task.CompletedTask;
            });
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
            else if (card.define is SkillCardDefine skill)
            {
                if (card.isUsed())//已经用过了
                    return false;
                if (gem < card.getCost())//费用不够
                    return false;
            }
            else
            {
                return false;//不知道是什么卡
            }
            card.setUsed(true);
            await setGem(game, gem - card.getCost());
            await game.triggers.doEvent(new UseEventArg() { player = this, card = card, position = position, targets = targets }, async arg =>
            {
                game.logger.log(arg.player + "使用" + arg.card);
                if (arg.card.define is ServantCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == CardDefineType.servant))
                {
                    //随从卡，将卡置入战场
                    await tryMove(game, arg.player.hand, arg.card, arg.position);
                    IEffect effect = arg.card.define.getEffectOn<BattleCryEventArg>(game.triggers);
                    if (effect != null)
                    {
                        await game.triggers.doEvent(new BattleCryEventArg() { player = arg.player, card = arg.card, effect = effect, targets = arg.targets }, arg2 =>
                        {
                            return arg2.effect.execute(game, arg2.player, arg2.card, new object[0], arg2.targets);
                        });
                    }
                }
                else if (card.define is SkillCardDefine)
                {
                    IEffect effect = arg.card.define.getEffectOn<ActiveEventArg>(game.triggers);
                    await effect.execute(game, arg.player, arg.card, new object[0], arg.targets);
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
        public class ActiveEventArg : EventArg
        {
        }
        public class BattleCryEventArg : EventArg
        {
            public THHPlayer player;
            public Card card;
            public IEffect effect;
            public Card[] targets;
        }
        public async Task<bool> tryMove(THHGame game, Pile from, Card card, int position)
        {
            if (field.count >= field.maxCount)//没位置了
                return false;
            await game.triggers.doEvent(new MoveEventArg() { player = this, from = from, card = card, position = position }, arg =>
            {
                THHPlayer player = arg.player;
                from = arg.from;
                card = arg.card;
                position = arg.position;
                if (from != null)
                    game.logger.log(arg.player + "将" + arg.card + "从" + arg.from + "置入战场，位于" + arg.position);
                else
                    game.logger.log(arg.player + "将" + arg.card + "置入战场，位于" + arg.position);
                if (from != null)
                    from.moveTo(arg.card, arg.player.field, arg.position);
                else
                    player.field.insert(card, position);
                if (card.define is ServantCardDefine servant)
                {
                    card.setCurrentLife(servant.life);
                    card.setReady(false);
                }
                return Task.CompletedTask;
            });
            return true;
        }
        public class MoveEventArg : EventArg
        {
            public THHPlayer player;
            public Pile from;
            public Card card;
            public int position;
        }
        public async Task<bool> createToken(THHGame game, CardDefine define, int position)
        {
            if (field.count >= field.maxCount)
                return false;
            await game.triggers.doEvent(new CreateTokenEventArg() { player = this, define = define, position = position }, async arg =>
            {
                THHPlayer player = arg.player;
                define = arg.define;
                position = arg.position;
                if (field.count >= field.maxCount)
                    return;
                game.logger.log(player + "召唤" + define.GetType().Name + "位于" + position);
                arg.card = game.createCard(define);
                await tryMove(game, null, arg.card, position);
            });
            return true;
        }
        public class CreateTokenEventArg : EventArg
        {
            public THHPlayer player;
            public CardDefine define;
            public int position;
            public Card card;
        }
        #region Command
        public void cmdInitReplace(THHGame game, params Card[] cards)
        {
            game.answers.answer(id, new InitReplaceResponse()
            {
                cardsId = cards.Select(c => c.id).ToArray()
            });
        }
        public void cmdUse(THHGame game, Card card, int position, params Card[] targets)
        {
            game.answers.answer(id, new UseResponse()
            {
                cardId = card.id,
                position = position,
                targetsId = targets.Select(c => c.id).ToArray()
            });
        }
        public void cmdTurnEnd(THHGame game)
        {
            game.answers.answer(id, new TurnEndResponse()
            {
            });
        }
        public void cmdAttack(THHGame game, Card card, Card target)
        {
            game.answers.answer(id, new AttackResponse()
            {
                cardId = card.id,
                targetId = target.id
            });
        }
        #endregion
    }
}