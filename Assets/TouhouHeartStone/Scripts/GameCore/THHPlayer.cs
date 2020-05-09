using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone.Builtin;
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
            addPile(new Pile(game, "Master", new Card[] { this.master }, 1));
            skill = game.createCardById(master.skillID);
            addPile(new Pile(game, "Skill", new Card[] { skill }, 1));
            this.deck = new Pile(game, "Deck", deck.Select(d => game.createCard(d)).ToArray());
            addPile(this.deck);
            init = new Pile(game, "Init", maxCount: 4);
            addPile(init);
            hand = new Pile(game, "Hand", maxCount: 10);
            addPile(hand);
            field = new Pile(game, "Field", maxCount: 7);
            addPile(field);
            grave = new Pile(game, "Grave");
            addPile(grave);
        }
        internal async Task initReplace(THHGame game, params Card[] cards)
        {
            await game.triggers.doEvent(new InitReplaceEventArg() { player = this, cards = cards }, onInitReplace);
            Task onInitReplace(InitReplaceEventArg arg)
            {
                arg.replacedCards = arg.player.init.replaceByRandom(game, arg.cards, arg.player.deck);
                game.logger.log(arg.player + "替换卡牌：" + string.Join("，", arg.cards.Select(c => c.ToString())) + "=>"
                    + string.Join("，", arg.replacedCards.Select(c => c.ToString())));
                return Task.CompletedTask;
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
                if (arg.player.gem > 10)
                    arg.player.gem = 10;
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
                    arg.player.deck.moveTo(game, arg.card, arg.player.grave, arg.player.grave.count);
                    game.logger.log(arg.player + "的手牌已经满了，" + arg.card + "被送入墓地");
                    return Task.CompletedTask;
                });
            }
            else
            {
                return game.triggers.doEvent(new DrawEventArg() { player = this }, arg =>
                {
                    arg.card = arg.player.deck.top;
                    arg.player.deck.moveTo(game, arg.card, arg.player.hand, arg.player.hand.count);
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
            if (!card.isUsable(game, this, out _))
                return false;
            card.setUsed(true);
            await setGem(game, gem - card.getCost());
            await game.triggers.doEvent(new UseEventArg() { player = this, card = card, position = position, targets = targets }, async arg =>
            {
                THHPlayer player = arg.player;
                card = arg.card;
                targets = arg.targets;
                game.logger.log(arg.player + "使用" + arg.card + (targets.Length > 0 ? "，目标：" + string.Join<Card>("，", targets) : null));
                if (arg.card.define is ServantCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == CardDefineType.SERVANT))
                {
                    //随从卡，将卡置入战场
                    await tryPutIntoField(game, arg.player.hand, arg.card, arg.position);
                    IEffect effect = arg.card.define.getEffectOn<ActiveEventArg>(game.triggers);
                    if (effect != null)
                    {
                        ActiveEventArg active = new ActiveEventArg(player, card, targets);
                        await game.triggers.doEvent(active, activeLogic);
                        async Task activeLogic(ActiveEventArg eventArg)
                        {
                            await effect.execute(game, player, card, new object[] { eventArg }, targets);
                        }
                    }
                    //IEffect effect = arg.card.define.getEffectOn<BattleCryEventArg>(game.triggers);
                    //if (effect != null)
                    //{
                    //    await game.triggers.doEvent(new BattleCryEventArg() { player = arg.player, card = arg.card, effect = effect, targets = arg.targets }, arg2 =>
                    //    {
                    //        return arg2.effect.execute(game, arg2.player, arg2.card, new object[] { arg2 }, arg2.targets);
                    //    });
                    //}
                }
                else if (card.define is SkillCardDefine)
                {
                    IEffect effect = arg.card.define.getEffectOn<ActiveEventArg>(game.triggers);
                    await effect.execute(game, arg.player, arg.card, new object[] { new ActiveEventArg(player, card, targets) }, arg.targets);
                }
                else if (card.define is SpellCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == CardDefineType.SPELL))
                {
                    //法术卡，释放效果然后丢进墓地
                    player.hand.remove(game, card);
                    IEffect effect = arg.card.define.getEffectOn<ActiveEventArg>(game.triggers);
                    await effect.execute(game, player, card, new object[] { new ActiveEventArg(player, card, targets) }, targets);
                    player.grave.add(game, card);
                }
            });
            await game.updateDeath();
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
            public THHPlayer player;
            public Card card;
            public object[] targets;
            public ActiveEventArg(THHPlayer player, Card card, object[] targets)
            {
                this.player = player;
                this.card = card;
                this.targets = targets;
            }
        }
        public async Task<bool> tryPutIntoField(THHGame game, Pile from, Card card, int position)
        {
            if (field.count >= field.maxCount)//没位置了
                return false;
            await game.triggers.doEvent(new MoveEventArg() { player = this, from = from, card = card, position = position }, logic);
            Task logic(MoveEventArg arg)
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
                    from.moveTo(game, arg.card, arg.player.field, arg.position);
                else
                    player.field.insert(game, card, position);
                if (card.define is ServantCardDefine servant)
                {
                    card.setCurrentLife(servant.life);
                    card.setReady(card.isCharge());
                }
                return Task.CompletedTask;
            }
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
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (define == null)
                throw new ArgumentNullException(nameof(define));
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
                await tryPutIntoField(game, null, arg.card, position);
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
        public int getSpellDamage(int baseDamage)
        {
            int result = baseDamage;
            foreach (var card in field)
            {
                result += card.getSpellDamage();
            }
            return result;
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