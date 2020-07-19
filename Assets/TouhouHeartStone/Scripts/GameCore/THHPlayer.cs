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
        /// <summary>
        /// 正在使用的法术卡都会被置入亚空间
        /// </summary>
        public Pile warp { get; }
        public Card item
        {
            get { return this[PileName.ITEM].count > 0 ? this[PileName.ITEM][0] : null; }
        }
        public bool isPrepared { get; set; } = false;
        public int gem { get; private set; } = 0;
        public int maxGem { get; private set; } = 0;
        public int fatigue { get; private set; } = 0;
        public THHPlayer(THHGame game, int id, string name, MasterCardDefine master, IEnumerable<CardDefine> deck) : base(id, name)
        {
            this.master = game.createCard(master);
            addPile(new Pile(game, PileName.MASTER, 1));
            getPile(PileName.MASTER).add(game, this.master);
            skill = game.createCardById(master.skillID);
            addPile(new Pile(game, PileName.SKILL, 1));
            this[PileName.SKILL].add(game, skill);
            this.deck = new Pile(game, PileName.DECK);
            if (deck != null)
                this.deck.add(game, deck.Select(d => game.createCard(d)).ToArray());
            addPile(this.deck);
            init = new Pile(game, PileName.INIT, maxCount: 4);
            addPile(init);
            hand = new Pile(game, PileName.HAND, maxCount: 10);
            addPile(hand);
            field = new Pile(game, PileName.FIELD, maxCount: 7);
            addPile(field);
            grave = new Pile(game, PileName.GRAVE);
            addPile(grave);
            warp = new Pile(game, PileName.WARP);
            addPile(warp);
            addPile(new Pile(game, PileName.ITEM));
        }
        internal async Task initReplace(THHGame game, params Card[] cards)
        {
            await game.triggers.doEvent(new InitReplaceEventArg() { player = this, cards = cards }, onInitReplace);
            async Task onInitReplace(InitReplaceEventArg arg)
            {
                arg.replacedCards = await arg.player.init.replaceByRandom(game, arg.cards, arg.player.deck);
                game.logger.log(arg.player + "替换卡牌：" + string.Join("，", arg.cards.Select(c => c.ToString())) + "=>"
                    + string.Join("，", arg.replacedCards.Select(c => c.ToString())));
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
                return game.triggers.doEvent(new FatigueEventArg() { player = this }, onFatigue);
                async Task onFatigue(FatigueEventArg arg)
                {
                    arg.player.fatigue++;
                    game.logger.log(arg.player + "已经没有卡牌了，当前疲劳值：" + arg.player.fatigue);
                    await arg.player.master.damage(game, null, arg.player.fatigue);
                    await game.updateDeath();
                }
            }
            else
                return draw(game, deck.top);
        }
        public Task draw(THHGame game, Card card)
        {
            if (!deck.Contains(card))
                return Task.CompletedTask;
            if (hand.count >= hand.maxCount)
            {
                return game.triggers.doEvent(new BurnEventArg() { player = this, card = card }, arg =>
                {
                    card = arg.card;
                    arg.player.deck.moveTo(game, card, arg.player.grave, arg.player.grave.count);
                    game.logger.log(arg.player + "的手牌已经满了，" + card + "被送入墓地");
                    return Task.CompletedTask;
                });
            }
            else
            {
                return game.triggers.doEvent(new DrawEventArg() { player = this, card = card }, arg =>
                {
                    card = arg.card;
                    arg.player.deck.moveTo(game, card, arg.player.hand, arg.player.hand.count);
                    game.logger.log(arg.player + "抽" + card);
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
                //卡牌不可用
                return false;
            if (targets.Any(t => t is Card targetCard && targetCard.isElusive(game) && (card.define is SkillCardDefine || card.define is SpellCardDefine)))
                //目标是魔免
                return false;
            if (card.define is SkillCardDefine)
                //使用技能
                card.setUsed(true);
            await setGem(game, gem - card.getCost(game));
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
                    ITriggerEffect effect = arg.card.define.getEffectOn<ActiveEventArg>(game.triggers);
                    if (effect != null)
                    {
                        ActiveEventArg active = new ActiveEventArg(player, card, targets);
                        await game.triggers.doEvent(active, activeLogic);
                        async Task activeLogic(ActiveEventArg eventArg)
                        {
                            await effect.execute(game, card, new object[] { eventArg }, targets);
                        }
                    }
                    IActiveEffect activeEffect = arg.card.define.getActiveEffect();
                    if (activeEffect != null)
                    {
                        await activeEffect.execute(game, card, new object[] { new ActiveEventArg(player, card, targets) }, targets);
                    }
                }
                else if (card.define is SkillCardDefine)
                {
                    ITriggerEffect effect = arg.card.define.getEffectOn<ActiveEventArg>(game.triggers);
                    if (effect != null)
                    {
                        await effect.execute(game, arg.card, new object[] { new ActiveEventArg(player, card, targets) }, arg.targets);
                    }
                    IActiveEffect activeEffect = arg.card.define.getActiveEffect();
                    if (activeEffect != null)
                    {
                        await activeEffect.execute(game, card, new object[] { new ActiveEventArg(player, card, targets) }, targets);
                    }
                }
                else if (card.define is SpellCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == CardDefineType.SPELL))
                {
                    //法术卡，释放效果然后丢进墓地
                    await player.hand.moveTo(game, card, player.warp);
                    ITriggerEffect triggerEffect = arg.card.define.getEffectOn<ActiveEventArg>(game.triggers);
                    if (triggerEffect != null)
                    {
                        await triggerEffect.execute(game, card, new object[] { new ActiveEventArg(player, card, targets) }, targets);
                    }
                    IActiveEffect activeEffect = arg.card.define.getActiveEffect();
                    if (activeEffect != null)
                    {
                        await activeEffect.execute(game, card, new object[] { new ActiveEventArg(player, card, targets) }, targets);
                    }
                    await player.warp.moveTo(game, card, player.grave);
                }
                else if (card.define is ItemCardDefine || (card.define is GeneratedCardDefine gDefine && gDefine.type == CardDefineType.ITEM))
                {

                    //物品卡，置入物品栏
                    await player.equip(game, card);
                    ITriggerEffect triggerEffect = arg.card.define.getEffectOn<ActiveEventArg>(game.triggers);
                    if (triggerEffect != null)
                    {
                        await triggerEffect.execute(game, card, new object[] { new ActiveEventArg(player, card, targets) }, targets);
                    }
                    IActiveEffect activeEffect = arg.card.define.getActiveEffect();
                    if (activeEffect != null)
                    {
                        await activeEffect.execute(game, card, new object[] { new ActiveEventArg(player, card, targets) }, targets);
                    }
                }
            });
            await game.updateDeath();
            return true;
        }
        public Task equip(THHGame game, Card item)
        {
            if (this[PileName.ITEM].isFull)
                destroyItem(game);
            item.setCurrentLife(item.getLife(game));
            return hand.moveTo(game, item, this[PileName.ITEM]);
        }
        public Task destroyItem(THHGame game)
        {
            if (this[PileName.ITEM].count > 0 && this[PileName.ITEM][0] is Card item)
                return item.die(game);
            return Task.CompletedTask;
        }
        /// <summary>
        /// 从给出的卡牌中发现一张牌，默认是从中挑三张
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cards"></param>
        /// <param name="count">如果小于等于0，则全都可以挑</param>
        /// <returns></returns>
        public async Task<Card> discover(THHGame game, IEnumerable<Card> cards, int count = 3)
        {
            if (count > 0)
                cards = cards.randomTake(game, count);
            var task = game.answers.ask(id, new DiscoverRequest(cards.Select(c => c.id).ToArray()));
            await task;
            return cards.First(c => c.id == (task.Result as DiscoverResponse).cardId);
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
                    card.setDead(false);
                    card.setCurrentLife(servant.life);
                    card.setReady(false);
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
        public int getSpellDamage(THHGame game, int baseDamage)
        {
            int result = baseDamage;
            foreach (var card in field)
            {
                result += card.getSpellDamage(game);
            }
            return result;
        }
        public Task addRandomCardToHand(THHGame game, IEnumerable<CardDefine> pool)
        {
            if (hand.isFull)//手牌满了
                return Task.CompletedTask;
            Card card = game.createCardByRandom(pool);
            return hand.add(game, card);
        }
        public Task randomDiscard(THHGame game, int count = 1)
        {
            if (hand.count < 1)
                return Task.CompletedTask;
            else if (hand.count <= count)
                return discard(game, hand);
            else
                return discard(game, hand.randomTake(game, count));
        }
        public Task discard(THHGame game, IEnumerable<Card> cards)
        {
            return game.triggers.doEvent(new DiscardEventArg() { player = this, cards = cards.ToArray() }, arg =>
            {
                THHPlayer player = arg.player;
                cards = arg.cards;
                return player.hand.moveTo(game, cards, player.grave);
            });
        }
        public class DiscardEventArg : EventArg
        {
            public THHPlayer player;
            public Card[] cards;
        }
        #region Command
        public Task cmdInitReplace(THHGame game, params Card[] cards)
        {
            return game.answers.answer(id, new InitReplaceResponse()
            {
                cardsId = cards.Select(c => c.id).ToArray()
            });
        }
        public Task cmdUse(THHGame game, Card card, int position = 0, params Card[] targets)
        {
            return game.answers.answer(id, new UseResponse()
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
        public Task cmdSurrender(THHGame game)
        {
            return game.surrender(this);
        }
        public Task cmdDiscover(THHGame game, int cardId)
        {
            return game.answers.answer(id, new DiscoverResponse()
            {
                cardId = cardId
            });
        }
        #endregion
    }
}