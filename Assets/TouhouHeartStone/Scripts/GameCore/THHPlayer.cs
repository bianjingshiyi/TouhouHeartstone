using System.Linq;
using System;
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
        /// <summary>
        /// 正在使用的法术卡都会被置入亚空间
        /// </summary>
        public Pile warp { get; }
        /// <summary>
        /// 头顶的五个玩意
        /// </summary>
        public Pile head { get; }
        public Card item
        {
            get { return this[PILE.ITEM].count > 0 ? this[PILE.ITEM][0] : null; }
        }
        public bool isPrepared { get; set; } = false;
        public int gem { get; private set; } = 0;
        public int maxGem { get; private set; } = 0;
        public int fatigue { get; private set; } = 0;
        public THHPlayer(THHGame game, int id, string name, MasterCardDefine master, IEnumerable<CardDefine> deck) : base(id, name)
        {
            this.master = game.createCard(master);
            addPile(new Pile(game, PILE.MASTER, 1));
            getPile(PILE.MASTER).add(game, this.master);
            skill = game.createCardById(master.skillID);
            addPile(new Pile(game, PILE.SKILL, 1));
            this[PILE.SKILL].add(game, skill);
            this.deck = new Pile(game, PILE.DECK);
            if (deck != null)
                this.deck.add(game, deck.Select(d => game.createCard(d)).ToArray());
            addPile(this.deck);
            init = new Pile(game, PILE.INIT, maxCount: 4);
            addPile(init);
            hand = new Pile(game, PILE.HAND, maxCount: 10);
            addPile(hand);
            field = new Pile(game, PILE.FIELD, maxCount: 7);
            addPile(field);
            grave = new Pile(game, PILE.GRAVE);
            addPile(grave);
            warp = new Pile(game, PILE.WARP);
            addPile(warp);
            addPile(new Pile(game, PILE.ITEM, 1));
            head = new Pile(game, PILE.HEAD, maxCount: 5);
            addPile(head);

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
        public class SetGemEventArg : EventArg, IDescribableEventArg
        {
            public THHPlayer player;
            public int value;
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return null;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return player.name + "的法力值变为" + value;
            }
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
        public class SetMaxGemEventArg : EventArg, IDescribableEventArg
        {
            public THHPlayer player;
            public int value;
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return null;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return player.name + "的最大法力值变为" + value;
            }
        }
        public Task draw(THHGame game, out DrawEventArg drawEvent, out BurnEventArg burnEvent, out FatigueEventArg fatigueEvent)
        {
            if (deck.count < 1)//无牌可抽，疲劳！
            {
                drawEvent = null;
                burnEvent = null;
                fatigueEvent = new FatigueEventArg() { player = this };
                return game.triggers.doEvent(fatigueEvent, onFatigue);
                async Task onFatigue(FatigueEventArg arg)
                {
                    THHPlayer player = arg.player;
                    player.fatigue++;
                    game.logger.log(player + "已经没有卡牌了，当前疲劳值：" + player.fatigue);
                    var damage = await player.master.damage(game, null, player.fatigue);
                    await game.updateDeath();
                }
            }
            else
            {
                fatigueEvent = null;
                return draw(game, deck.top, out drawEvent, out burnEvent);
            }
        }
        public Task draw(THHGame game, out DrawEventArg drawEvent)
        {
            return draw(game, out drawEvent, out _, out _);
        }
        public Task draw(THHGame game)
        {
            return draw(game, out _, out _, out _);
        }
        public Task draw(THHGame game, Card card, out DrawEventArg drawEvent, out BurnEventArg burnEvent)
        {
            if (!deck.Contains(card))
            {
                drawEvent = null;
                burnEvent = null;
                return Task.CompletedTask;
            }
            if (hand.count >= hand.maxCount)
            {
                drawEvent = null;
                burnEvent = new BurnEventArg() { player = this, card = card };
                return game.triggers.doEvent(burnEvent, arg =>
                {
                    card = arg.card;
                    arg.player.deck.moveTo(game, card, arg.player.grave, arg.player.grave.count);
                    game.logger.log(arg.player + "的手牌已经满了，" + card + "被送入墓地");
                    return Task.CompletedTask;
                });
            }
            else
            {
                drawEvent = new DrawEventArg() { player = this, card = card };
                burnEvent = null;
                return game.triggers.doEvent(drawEvent, async arg =>
                {
                    THHPlayer player = arg.player;
                    card = arg.card;
                    if (card.define is SpellCardDefine spell && card.define.hasKeyword(KEYWORD.AUTOCAST))
                    {
                        await player.deck.moveTo(game, card, player.warp);
                        game.logger.log("debug");
                        await card.activeEffect(game, player, new Card[0]);
                        await player.warp.moveTo(game, card, player.grave);
                        await game.updateDeath();
                        await draw(game);
                    }
                    else
                    {
                        await player.deck.moveTo(game, card, player.hand, player.hand.count);
                        game.logger.log(player + "抽" + card);
                    }
                });
            }
        }
        public Task draw(THHGame game, Card card)
        {
            return draw(game, card, out _, out _);
        }
        public Task draw(THHGame game, Card card, out DrawEventArg drawEvent)
        {
            return draw(game, card, out drawEvent, out _);
        }
        /// <summary>
        /// 抽一张符合条件的牌，如果没有符合条件的牌则抽牌不会发生，返回事件为空。
        /// </summary>
        /// <param name="game"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Task draw(THHGame game, Func<Card, bool> filter, out DrawEventArg drawEvent)
        {
            var cards = deck.Where(filter);
            if (cards.Count() > 0)
            {
                var card = cards.random(game);
                return draw(game, card, out drawEvent);
            }
            else
            {
                drawEvent = null;
                return Task.CompletedTask;
            }
        }
        public class DrawEventArg : EventArg, IDescribableEventArg
        {
            /// <summary>
            /// 抽卡的玩家
            /// </summary>
            public THHPlayer player;
            /// <summary>
            /// 抽到的卡
            /// </summary>
            public Card card;
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return viewer == player ? card : null;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                if (!isCanceled)
                {
                    if (viewer == player)
                        return player.name + "抽{card:" + card.define.id + "}";
                    else
                        return player.name + "抽了一张牌";
                }
                else
                {
                    return null;
                }
            }
        }
        public class BurnEventArg : EventArg, IDescribableEventArg
        {
            public THHPlayer player;
            public Card card;
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return player.name + "抽{card:" + card.define.id + "}，但是由于手牌已满它被送入墓地";
            }
        }
        public class FatigueEventArg : EventArg, IDescribableEventArg
        {
            public THHPlayer player;

            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return null;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return player.name + "已经无牌可抽，即将受到疲劳伤害";
            }
        }
        /// <summary>
        /// 尝试使用一张卡牌
        /// </summary>
        /// <param name="game">游戏环境</param>
        /// <param name="card">使用的卡牌</param>
        /// <param name="position">随从卡放置在场上的位置</param>
        /// <param name="targets">卡牌使用目标</param>
        /// <returns></returns>
        public async Task<bool> tryUse(THHGame game, Card card, int position, params Card[] targets)
        {
            if (!card.isUsable(game, this, out _))
                //卡牌不可用
                return false;
            if (targets.Any(t => t is Card targetCard && targetCard.isElusive(game) && (card.define is SkillCardDefine || card.define is SpellCardDefine)))
                //目标是魔免
                return false;
            if (card.isSkill())
            {
                //使用技能，设置技能使用状态为“已使用”
                await card.setUsed(game, true);
            }
            await setGem(game, gem - card.getCost(game));//消耗法力值
            await game.triggers.doEvent(new UseEventArg() { player = this, card = card, position = position, cost = card.getCost(game), targets = targets }, async arg =>
             {
                 THHPlayer player = arg.player;
                 card = arg.card;
                 targets = arg.targets;
                 game.logger.log(arg.player + "使用" + arg.card + (targets.Length > 0 ? "，目标：" + string.Join<Card>("，", targets) : null));
                 if (arg.card.define is ServantCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == DEFINE.SERVANT))
                 {
                     await tryEnterField(game, arg.player.hand, arg.card, arg.position);//随从卡，将卡置入战场
                     await card.ComeOnStage(game, player, targets);//触发战吼事件
                                                                   //await card.activeEffect(game, player, targets);//执行随从效果
                 }
                 else if (card.define is SkillCardDefine)
                 {
                     await card.activeEffect(game, player, targets);
                 }
                 else if (card.define is SpellCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == DEFINE.SPELL))
                 {
                     //法术卡,释放效果
                     await player.hand.moveTo(game, card, player.warp);
                     if (card.hasKeyword(game, KEYWORD.SECRET))
                         await tryEnterHead(game, warp, card);
                     else
                     {
                         await card.activeEffect(game, player, targets);
                         if (card.hasKeyword(game, KEYWORD._RETURNHEAD))
                         {
                             await player.warp.moveTo(game, card, player.hand);
                         }
                         await player.warp.moveTo(game, card, player.grave);//如果是普通法术，则送入墓地
                     }
                 }
                 else if (card.define is ItemCardDefine || (card.define is GeneratedCardDefine gDefine && gDefine.type == DEFINE.ITEM))
                 {
                     //物品卡，置入物品栏
                     await player.equip(game, card);
                     await card.activeEffect(game, player, targets);
                 }
             });
            await game.updateDeath();
            return true;
        }
        public async Task equip(THHGame game, Card item)
        {
            if (this[PILE.ITEM].isFull)
            {
                game.logger.log("由于" + this + "的道具栏已满，摧毁当前道具");
                await destroyItem(game);
            }
            await item.setAttack(game, item.getAttack(game));
            await item.setCurrentLife(game, item.getLife(game));
            await hand.moveTo(game, item, this[PILE.ITEM]);
            game.logger.log(this + "装备" + item);
        }
        public Task destroyItem(THHGame game)
        {
            if (this[PILE.ITEM].count > 0 && this[PILE.ITEM][0] is Card item)
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
        public async Task<Card> discover(THHGame game, IEnumerable<Card> cards, int count = 3, string title = null)
        {
            if (cards.Count() < 1)
                return null;
            if (count > 0)
                cards = cards.randomTake(game, count);
            game.logger.log(this + "要从" + string.Join("，", cards) + "中选择一张卡片");
            var task = game.answers.ask(id, new DiscoverRequest(cards.Select(c => c.id).ToArray(), title));
            await task;
            Card selected = cards.First(c => c.id == (task.Result as DiscoverResponse).cardId);
            game.logger.log(this + "选择" + selected);
            return selected;
        }
        public class UseEventArg : EventArg, IDescribableEventArg
        {
            public THHPlayer player;
            public Card card;
            public int position;
            public int cost;
            public Card[] targets;
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                if (card.hasKeyword(game, KEYWORD.SECRET))
                    return viewer == player ? card : null;
                return card;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return targets;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                if (card.hasKeyword(game, KEYWORD.SECRET))
                {
                    return viewer == player ? player.name + "使用{card:" + card.define.id + "}" : player.name + "使用了一张奥秘牌";
                }
                if (targets == null || targets.Length < 1)
                    return player.name + "使用{card:" + card.define.id + "}";
                else
                    return player.name + "使用{card:" + card.define.id + "}，目标：" + string.Join("，", targets.Select(c => "{card:" + c.define.id + "}"));
            }
        }
        public class ActiveEventArg : EventArg, IDescribableEventArg
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
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return targets.OfType<Card>().ToArray();
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                if (targets == null || targets.Length < 1)
                    return player.name + "发动" + card.getFormatString() + "的效果";
                else
                    return player.name + "发动" + card.getFormatString() + "的效果，目标：" + string.Join("，", targets.Select(t => t is Card c ? c.getFormatString() : t.ToString()));
            }
        }
        public Task<EnterFieldEventArg> tryEnterField(THHGame game, Pile from, Card card, int position)
        {
            if (field.count >= field.maxCount)//没位置了
                return Task.FromResult(default(EnterFieldEventArg));
            return game.triggers.doEvent(new EnterFieldEventArg() { player = this, from = from, card = card, position = position }, onEnterField);
            async Task onEnterField(EnterFieldEventArg arg)
            {
                THHPlayer player = arg.player;
                from = arg.from;
                card = arg.card;
                position = arg.position;
                if (from != null)
                    game.logger.log(arg.player + "将" + arg.card + "从" + arg.from + "置入战场，位于" + arg.position);
                else
                    game.logger.log(arg.player + "将" + arg.card + "置入战场，位于" + arg.position);
                if (card.define is ServantCardDefine servant)//设置随从状态
                {
                    await card.setDead(game, false);
                    await card.setCurrentLife(game, card.getLife(game));
                    await card.setReady(game, false);
                }
                if (from != null)
                    await from.moveTo(game, arg.card, arg.player.field, arg.position);//从某pile召唤随从
                else
                    await player.field.insert(game, card, position);//虚空生成随从
                arg.attack = card.getAttack(game);
                arg.life = card.getLife(game);
            }
        }
        public async Task<bool> tryEnterHead(THHGame game, Pile from, Card card)
        {
            if (head.isFull)
                return false;
            await from.moveTo(game, card, head);
            return true;
        }
        public class EnterFieldEventArg : EventArg, IDescribableEventArg
        {
            public THHPlayer player;
            public Pile from;
            public Card card;
            public int position;
            public int attack;
            public int life;
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return player.name + "将" + card.getFormatString() + "置入战场，位于" + position;
            }
        }
        public Task<CreateTokenEventArg> createToken(THHGame game, CardDefine define, int position = -1)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (define == null)
                throw new ArgumentNullException(nameof(define));
            if (field.count >= field.maxCount)
                return null;
            if (position < 0)
                position = field.count;
            CreateTokenEventArg eventArg = new CreateTokenEventArg() { player = this, define = define, position = position };
            return game.triggers.doEvent(eventArg, async arg =>
            {
                THHPlayer player = arg.player;
                define = arg.define;
                position = arg.position;
                if (field.count >= field.maxCount)
                    return;
                game.logger.log(player + "召唤" + define.GetType().Name + "位于" + position);
                arg.card = game.createCard(define);
                await tryEnterField(game, null, arg.card, position);
            });
        }
        public async Task<CreateTokenEventArg[]> createToken(THHGame game, int count, CardDefine define, int position = -1)
        {
            List<CreateTokenEventArg> list = new List<CreateTokenEventArg>();
            for (int i = 0; i < count; i++)
            {
                list.Add(await createToken(game, define, position));
            }
            return list.ToArray();
        }
        public class CreateTokenEventArg : EventArg, IDescribableEventArg
        {
            public THHPlayer player;
            public CardDefine define;
            public int position;
            public Card card;

            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return player.name + "召唤衍生物" + card.getFormatString();
            }
        }
        public async Task<RebornEventArg> reborn(THHGame game, Card card, Pile pile, int position = -1)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (pile.count >= pile.maxCount)
                return null;
            if (position < 0)
                position = pile.count;
            if (card == null)
                return null;
            RebornEventArg eventArg = new RebornEventArg() { player = this, card = card, pile = pile, position = position };
            await game.triggers.doEvent(eventArg, async arg =>
            {
                THHPlayer player = arg.player;
                position = arg.position;
                if (pile.count >= pile.maxCount)
                    return;
                game.logger.log(player + "复活" + arg.card.define.GetType().Name + "位于" + position);
                await arg.player.grave.moveTo(game, arg.card, arg.pile, position);
                foreach (IPassiveEffect effect in arg.card.define.effects.OfType<DeathRattle>())
                {
                    if (effect.piles.Contains(pile.name))
                    {
                        await effect.onDisable(game, card, null);
                        await effect.onEnable(game, card, null);
                    }
                }
            });
            return eventArg;
        }
        public class RebornEventArg : EventArg, IDescribableEventArg
        {
            public THHPlayer player;
            public int position;
            public Card card;
            public Pile pile;

            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return player.name + "复活" + card.getFormatString();
            }
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
        public Task shuffleCardToDeck(THHGame game, CardDefine define)
        {
            Card card = game.createCard(define);
            return shuffleCardToDeck(game, card);
        }
        public Task shuffleCardToDeck(THHGame game, Card card)
        {
            return deck.insert(game, card, game.randomInt(0, deck.count));
        }
        public Task addCardToHand<T>(THHGame game) where T : CardDefine
        {
            if (hand.isFull)
                return Task.CompletedTask;
            Card card = game.createCard<T>();
            return hand.add(game, card);
        }
        public async Task<bool> tryAddCardToHand(THHGame game, Card card)
        {
            if (hand.isFull)
                return false;
            game.logger.log("将" + card + "置入" + this + "的手牌");
            await hand.add(game, card);
            return true;
        }
        public Task addRandomCardToHand(THHGame game, IEnumerable<CardDefine> pool)
        {
            if (hand.isFull)//手牌满了
                return Task.CompletedTask;
            Card card = game.createCardByRandom(pool);
            return hand.add(game, card);
        }
        public Task randomDiscard(THHGame game, int count = 1, Func<Card, bool> filter = null)
        {
            var cards = filter == null ? hand : hand.Where(c => filter(c));
            if (cards.Count() < 1)
                return Task.CompletedTask;
            else if (cards.Count() <= count)
                return discard(game, cards);
            else
                return discard(game, cards.randomTake(game, count));
        }
        public Task discard(THHGame game, Card card)
        {
            return discard(game, new Card[] { card });
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
        public class DiscardEventArg : EventArg, IDescribableEventArg
        {
            public THHPlayer player;
            public Card[] cards;

            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return null;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return cards;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return player.name + "弃牌：" + string.Join("，", cards.Select(c => c.getFormatString()));
            }
        }
        public bool isFriend(Card card)
        {
            if (card.getOwner() == this)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 找到符合条件的卡片，无论它在哪里。
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <remarks>但是其实并不会真的哪里都去找啦</remarks>
        public Card[] findCardsWherever(THHGame game, Func<Card, bool> filter, PileFlag pile = PileFlag.self | PileFlag.field | PileFlag.hand | PileFlag.grave | PileFlag.deck)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));
            List<Card> cardList = new List<Card>();
            foreach (var p in pile.getPiles(game, this))
            {
                cardList.AddRange(p.Where(filter));
            }
            return cardList.ToArray();
        }


        /// <summary>
        /// 揭示Reveal
        /// 向双方展示牌库里的一张牌
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cards">被展示卡的来源</param>
        /// <param name="define">设定揭示的类别:0是所有类别；1是法术牌；2是随从牌；3是武器牌</param>
        /// <returns></returns>

        public async Task<Card> Reveal(THHGame game, Pile cards, int define = 0)
        {
            Card show = null;

            switch (define)
            {
                case 0:
                    if (cards.count > 0)
                    {
                        show = cards.random(game);
                        await game.triggers.doEvent(new RevealArg() { player = this, card = show }, async arg =>
                        {
                            game.logger.log("揭示了{card:" + show.define.id + "}");
                        });
                        return show;
                    }
                    else
                    {
                        await game.triggers.doEvent(new RevealArg() { player = this, card = null }, async arg =>
                        {
                            game.logger.log("揭示了null");
                        });
                        return show;
                    }
                case 1:
                    if (cards.Count(c => c.isSpell()) > 0)
                    {
                        show = cards.Where(c => c.isSpell()).random(game);
                        await game.triggers.doEvent(new RevealArg() { player = this, card = show }, async arg =>
                        {
                            game.logger.log("揭示了{card:" + show.define.id + "}");
                        });
                        return show;

                    }
                    else
                    {
                        await game.triggers.doEvent(new RevealArg() { player = this, card = null }, async arg =>
                        {
                            game.logger.log("揭示了null");
                        });
                        return show;
                    }
                case 2:
                    if (cards.Count(c => c.isServant()) > 0)
                    {
                        show = cards.Where(c => c.isServant()).random(game);
                        await game.triggers.doEvent(new RevealArg() { player = this, card = show }, async arg =>
                        {
                            game.logger.log("揭示了{card:" + show.define.id + "}");
                        });
                        return show;

                    }
                    else
                    {
                        await game.triggers.doEvent(new RevealArg() { player = this, card = null }, async arg =>
                        {
                            game.logger.log("揭示了null");
                        });
                        return show;
                    }
                case 3:
                    if (cards.Count(c => c.isItem()) > 0)
                    {
                        show = cards.Where(c => c.isItem()).random(game);
                        await game.triggers.doEvent(new RevealArg() { player = this, card = show }, async arg =>
                        {
                            game.logger.log("揭示了{card:" + show.define.id + "}");
                        });
                        return show;

                    }
                    else
                    {
                        await game.triggers.doEvent(new RevealArg() { player = this, card = null }, async arg =>
                        {
                            game.logger.log("揭示了null");
                        });
                        return show;
                    }
                default:
                    game.logger.log("揭示失败：类别不对");
                    return null;



            }
        }

        public class RevealArg : EventArg, IDescribableEventArg
        {
            public THHPlayer player;
            public Card card;  //被揭示的牌

            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            public ICard[] getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            public string toString(IGame game, IPlayer viewer)
            {
                return card == null ? "揭示了个寂寞" : player.name + "揭示了{card:" + card.define.id + "}";
            }


        }



        #region Command
        public Task cmdInitReplace(THHGame game, params Card[] cards)
        {
            InitReplaceResponse response = new InitReplaceResponse()
            {
                playerId = id,
                cardsId = cards.Select(c => c.id).ToArray()
            };
            game.logPlayerAction(id, response);
            return game.answers.answer(id, response);
        }
        public Task cmdUse(THHGame game, Card card, int position = 0, params Card[] targets)
        {
            var response = new UseResponse()
            {
                cardId = card.id,
                position = position,
                targetsId = targets.Select(c => c.id).ToArray()
            };
            game.logPlayerAction(id, response);
            return game.answers.answer(id, response);
        }
        public Task cmdTurnEnd(THHGame game)
        {
            var resp = new TurnEndResponse() { };
            game.logPlayerAction(id, resp);
            return game.answers.answer(id, resp);
        }
        public Task cmdAttack(THHGame game, Card card, Card target)
        {
            var resp = new AttackResponse()
            {
                cardId = card.id,
                targetId = target.id
            };
            game.logPlayerAction(id, resp);
            return game.answers.answer(id, resp);
        }
        public Task cmdSurrender(THHGame game)
        {
            game.logPlayerAction(id, new SurrenderResponse());
            return game.surrender(this);
        }
        public Task cmdDiscover(THHGame game, int cardId)
        {
            var resp = new DiscoverResponse()
            {
                cardId = cardId
            };
            game.logPlayerAction(id, resp);
            return game.answers.answer(id, resp);
        }
        #endregion
    }
}