using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public static class THHPlayer
    {
        public static Card getMaster(this Player player) => player.getPile(PILE.MASTER)[0];
        public static Card getSkill(this Player player) => player.getPile(PILE.SKILL)[0];
        public static Pile getDeck(this Player player) => player.getPile(PILE.DECK);
        public static Pile getInit(this Player player) => player.getPile(PILE.INIT);
        public static Pile getHand(this Player player) => player.getPile(PILE.HAND);
        public static Pile getField(this Player player) => player.getPile(PILE.FIELD);
        public static Pile getGrave(this Player player) => player.getPile(PILE.GRAVE);
        /// <summary>
        /// 正在使用的法术卡都会被置入亚空间
        /// </summary>
        public static Pile getWarp(this Player player) => player.getPile(PILE.WARP);

        /// <summary>
        /// 头顶的五个玩意
        /// </summary>
        public static Pile getHead(this Player player) => player.getPile(PILE.HEAD);

        public static Card getItem(this Player player)
        {
            return player[PILE.ITEM].Any() ? player[PILE.ITEM][0] : null;
        }

        public static void setIsPrepared(this Player player,bool value) => player.setProp(PLAYER.ISPREPARED, value);
        public static bool getIsPrepared(this Player player) => player.getProp<bool>(PLAYER.ISPREPARED);
        public static int getGem(this Player player) => player.getProp<int>(PLAYER.GEM);
        public static int getMaxGem(this Player player) => player.getProp<int>(PLAYER.MAXGEM);

        public static void setFatigue(this Player player,int value) => player.setProp(PLAYER.FATIGUE, value);
        public static int getFatigue(this Player player) => player.getProp<int>(PLAYER.FATIGUE);

        internal static async Task initReplace(this Player player,CardEngine game, params Card[] cards)
        {
            await game.triggers.doEvent(new HearthStoneRule.InitReplaceEventArg() { player = player, cards = cards }, onInitReplace);
            async Task onInitReplace(HearthStoneRule.InitReplaceEventArg arg)
            {
                arg.replacedCards = await arg.player.getInit().replaceByRandom(game, arg.cards, arg.player.getDeck());
                game.logger.log(arg.player + "替换卡牌：" + string.Join("，", arg.cards.Select(c => c.ToString())) + "=>"
                    + string.Join("，", arg.replacedCards.Select(c => c.ToString())));
            }
        }

        public static Task setGem(this Player player,CardEngine game, int value)
        {
            return game.triggers.doEvent(new HearthStoneRule.SetGemEventArg() { player = player, value = value }, arg =>
            {
                arg.player.setProp(PLAYER.GEM, arg.value);
                if (arg.player.getGem() < 0)
                    arg.player.setProp(PLAYER.GEM, 0);
                if (arg.player.getGem() > 10)
                    arg.player.setProp(PLAYER.GEM, 10);
                game.logger.log(arg.player + "的法力水晶变为" + arg.player.getGem());
                return Task.CompletedTask;
            });
        }

        public static Task setMaxGem(this Player player,CardEngine game, int value)
        {
            return game.triggers.doEvent(new HearthStoneRule.SetMaxGemEventArg() { player = player, value = value }, arg =>
            {
                arg.player.setProp(PLAYER.MAXGEM, arg.value);
                if (arg.player.getMaxGem() < 0)
                    arg.player.setProp(PLAYER.MAXGEM, 0);
                if (arg.player.getMaxGem() > 10)
                    arg.player.setProp(PLAYER.MAXGEM, 10);
                if (arg.player.getGem() > arg.player.getMaxGem())
                    arg.player.setProp(PLAYER.MAXGEM, 0);
                game.logger.log(arg.player + "的法力水晶上限变为" + arg.player.getMaxGem());
                return Task.CompletedTask;
            });
        }

        public static Task draw(this Player player,CardEngine game, out HearthStoneRule.DrawEventArg drawEvent, out HearthStoneRule.BurnEventArg burnEvent, out HearthStoneRule.FatigueEventArg fatigueEvent)
        {
            if (player.getDeck().count < 1)//无牌可抽，疲劳！
            {
                drawEvent = null;
                burnEvent = null;
                fatigueEvent = new HearthStoneRule.FatigueEventArg() { player = player };
                return game.triggers.doEvent(fatigueEvent, onFatigue);
                async Task onFatigue(HearthStoneRule.FatigueEventArg arg)
                {
                    Player p = arg.player;
                    p.setFatigue(p.getFatigue() + 1);
                    game.logger.log(p + "已经没有卡牌了，当前疲劳值：" + p.getFatigue());
                    var damage = await p.getMaster().damage(game, null, p.getFatigue());
                    await game.updateDeath();
                }
            }
            else
            {
                fatigueEvent = null;
                return player.draw(game, player.getDeck().top, out drawEvent, out burnEvent);
            }
        }
        public static Task draw(this Player player,CardEngine game, out HearthStoneRule.DrawEventArg drawEvent)
        {
            return player.draw(game, out drawEvent, out _, out _);
        }
        public static Task draw(this Player player,CardEngine game)
        {
            return player.draw(game, out _, out _, out _);
        }
        public static Task draw(this Player player,CardEngine game, Card card, out HearthStoneRule.DrawEventArg drawEvent, out HearthStoneRule.BurnEventArg burnEvent)
        {
            if (!player.getDeck().Contains(card))
            {
                drawEvent = null;
                burnEvent = null;
                return Task.CompletedTask;
            }
            if (player.getHand().count >= player.getHand().maxCount)
            {
                drawEvent = null;
                burnEvent = new HearthStoneRule.BurnEventArg() { player = player, card = card };
                return game.triggers.doEvent(burnEvent, arg =>
                {
                    card = arg.card;
                    arg.player.getDeck().moveTo(game, card, arg.player.getGrave(), arg.player.getGrave().count);
                    game.logger.log(arg.player + "的手牌已经满了，" + card + "被送入墓地");
                    return Task.CompletedTask;
                });
            }
            else
            {
                drawEvent = new HearthStoneRule.DrawEventArg() { player = player, card = card };
                burnEvent = null;
                return game.triggers.doEvent(drawEvent, async arg =>
                {
                    Player p = arg.player;
                    card = arg.card;
                    if (card.define is SpellCardDefine spell && card.define.hasKeyword(KEYWORD.AUTOCAST))
                    {
                        await p.getDeck().moveTo(game, card, p.getWarp());
                        game.logger.log("debug");
                        await card.activeEffect(game, p, new Card[0]);
                        await p.getWarp().moveTo(game, card, p.getGrave());
                        await game.updateDeath();
                        await player.draw(game);
                    }
                    else
                    {
                        await p.getDeck().moveTo(game, card, p.getHand(), p.getHand().count);
                        game.logger.log(p + "抽" + card);
                    }
                });
            }
        }
        public static Task draw(this Player player,CardEngine game, Card card)
        {
            return player.draw(game, card, out _, out _);
        }
        public static Task draw(this Player player,CardEngine game, Card card, out HearthStoneRule.DrawEventArg drawEvent)
        {
            return player.draw(game, card, out drawEvent, out _);
        }
        /// <summary>
        /// 抽一张符合条件的牌，如果没有符合条件的牌则抽牌不会发生，返回事件为空。
        /// </summary>
        /// <param name="game"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Task draw(this Player player,CardEngine game, Func<Card, bool> filter, out HearthStoneRule.DrawEventArg drawEvent)
        {
            var cards = player.getDeck().Where(filter);
            if (cards.Count() > 0)
            {
                var card = cards.random(game);
                return player.draw(game, card, out drawEvent);
            }
            else
            {
                drawEvent = null;
                return Task.CompletedTask;
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
        public static async Task<bool> tryUse(this Player player,CardEngine game, Card card, int position, params Card[] targets)
        {
            if (!card.isUsable(game, player, out _))
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
            await player.setGem(game, player.getGem() - card.getCost(game));//消耗法力值
            await game.triggers.doEvent(new HearthStoneRule.UseEventArg() { player = player, card = card, position = position, cost = card.getCost(game), targets = targets }, async arg =>
             {
                 Player p = arg.player;
                 card = arg.card;
                 targets = arg.targets;
                 game.logger.log(p + "使用" + arg.card + (targets.Length > 0 ? "，目标：" + string.Join<Card>("，", targets) : null));
                 if (arg.card.define is ServantCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == DEFINE.SERVANT))
                 {
                     await player.tryEnterField(game, p.getHand(), arg.card, arg.position);//随从卡，将卡置入战场
                     await card.ComeOnStage(game, p, targets);//触发战吼事件
                                                                   //await card.activeEffect(game, p, targets);//执行随从效果
                 }
                 else if (card.define is SkillCardDefine)
                 {
                     await card.activeEffect(game, p, targets);
                 }
                 else if (card.define is SpellCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == DEFINE.SPELL))
                 {
                     //法术卡,释放效果
                     await p.getHand().moveTo(game, card, p.getWarp());
                     if (card.hasKeyword(game, KEYWORD.SECRET) || card.hasKeyword(game,KEYWORD.MISSION))
                         await player.tryEnterHead(game, player.getWarp(), card);
                     else
                     {
                         await card.activeEffect(game, p, targets);
                         await card.resetCard(game);
                         if (card.hasKeyword(game, KEYWORD._RETURNHEAD))
                         {
                             await p.getWarp().moveTo(game, card, p.getHand());
                         }
                         await p.getWarp().moveTo(game, card, p.getGrave());//如果是普通法术，则送入墓地
                     }
                 }
                 else if (card.define is ItemCardDefine || (card.define is GeneratedCardDefine gDefine && gDefine.type == DEFINE.ITEM))
                 {
                     //物品卡，置入物品栏
                     await p.equip(game, card);
                     await card.activeEffect(game, p, targets);
                 }
             });
            await game.updateDeath();
            return true;
        }
        public static async Task equip(this Player player,CardEngine game, Card item)
        {
            if (player[PILE.ITEM].isFull)
            {
                game.logger.log("由于" + player + "的道具栏已满，摧毁当前道具");
                await player.destroyItem(game);
            }
            await item.setAttack(game, item.getAttack(game));
            await item.setCurrentLife(game, item.getLife(game));
            await player.getHand().moveTo(game, item, player[PILE.ITEM]);
            game.logger.log(player + "装备" + item);
        }
        public static Task destroyItem(this Player player,CardEngine game)
        {
            if (player[PILE.ITEM].count > 0 && player[PILE.ITEM][0] is Card item)
                return item.die(game);
            return Task.CompletedTask;
        }
        /// <summary>
        /// 向玩家发出一个选择请求，玩家从提供的卡牌中选择一张，返回玩家选择的结果
        /// </summary>
        /// <param name="game"></param>
        /// <param name="cards">卡牌的ID，可以是游戏内的卡牌ID，也可以是卡牌定义的ID</param>
        /// <param name="count">如果小于等于0，则全都可以挑</param>
        /// <returns></returns>
        public static async Task<int> askSelect(this Player player,CardEngine game, int[] cards, int count = 3, string title = "选择一项")
        {
            if (cards.Count() < 1)
                return -1;
            if (count > 0)
                cards = cards.randomTake(game, count).ToArray();
            var eventArg = await game.triggers.doEvent(new SelectEventArg()
            {
                player = player, options = cards
            }, async arg =>
            {
                player = arg.player;
                cards = arg.options;
                game.logger.log(player + "要从卡牌或卡牌定义ID" + string.Join("，", cards) + "中选择一个");
                var res = await game.answers.ask(player.id, new DiscoverRequest(cards, title), float.MaxValue);
                int selectedID = cards.First(id => id == (res as DiscoverResponse).cardId);
                game.logger.log(player + "选择" + selectedID);
                arg.selected = selectedID;
            });
            return eventArg.selected;
        }
        public static async Task<Card> askSelect(this Player player,CardEngine game, IEnumerable<Card> cards, int count = 3, string title = null)
        {
            var selectedId = await player.askSelect(game, cards.Select(c => c.id).ToArray(), count, title);
            Card selectedCard = cards.First(c => c.id == selectedId);
            return selectedCard;
        }
        public static async Task<CardDefine> askSelect(this Player player,CardEngine game, IEnumerable<CardDefine> cards, int count = 3, string title = null)
        {
            var selectedId = await player.askSelect(game, cards.Select(c => c.id).ToArray(), count, title);
            CardDefine selectedCard = cards.First(c => c.id == selectedId);
            return selectedCard;
        }
        public class SelectEventArg : EventArg
        {
            /// <summary>
            /// 进行选择的玩家
            /// </summary>
            public Player player;
            /// <summary>
            /// 玩家的选择
            /// </summary>
            public int selected;
            /// <summary>
            /// 提供的选择
            /// </summary>
            public int[] options;
        }

        /// <summary>
        /// 让玩家从提供的卡牌定义中选择一张，创建相应卡牌并置入手牌中
        /// </summary>
        /// <param name="player"></param>
        /// <param name="game"></param>
        /// <param name="cards"></param>
        /// <param name="count"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static async Task<DiscoverEventArg> discover(this Player player, CardEngine game, IEnumerable<CardDefine> cardDefines, int count = 3,
            string title = null)
        {
            if (!cardDefines.Any())
            {
                return null;
            }
            var eventArg = await game.triggers.doEvent(new DiscoverEventArg()
            {
                player = player
            }, async arg =>
            {
                player = arg.player;
                cardDefines = cardDefines.randomTake(game, count);
                var selected = await askSelect(player, game, cardDefines, count, title);
                arg.discovered = await player.addCardToHand(game, selected);
                arg.defines = cardDefines.ToArray();
            });
            return eventArg;
        }
        
        public class DiscoverEventArg : EventArg, IDescribableEventArg
        {
            /// <summary>
            /// 发现的玩家
            /// </summary>
            public Player player;
            /// <summary>
            /// 发现的卡
            /// </summary>
            public Card discovered;
            /// <summary>
            /// 提供的选择
            /// </summary>
            public CardDefine[] defines;
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return viewer == player ? discovered : null;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return (game as CardEngine).createCards(defines);
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}玩家，{1}卡")]
            const string TEMPLATE_SELF = "{0}发现{1}";

            [MukioI18n.TranslateMsg("{0}玩家")]
            const string TEMPLATE_OTHER = "{0}发现了一张牌";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (!isCanceled)
                {
                    return viewer == player ? TEMPLATE_SELF : TEMPLATE_OTHER;
                }
                else
                {
                    return "";
                }
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    discovered.getFormatString()
                };
            }
        }

        public static async Task<HearthStoneRule.ChooseOneEventArg> chooseOne(this Player player,CardEngine game,IEnumerable<Card> options, bool isShow = true ,string title = "选择一项")
        {
            game.logger.log(player + "从" + string.Join("，", options) + "中选择一项");
            //var task = game.answers.ask(id, new DiscoverRequest(options.Select(c => c.id).ToArray(), title));
        
            var eventArg = await game.triggers.doEvent(new HearthStoneRule.ChooseOneEventArg() { player = player , options = options , choose = null , isShow = isShow }, async arg =>
            {
                var task = game.answers.ask(player.id, new DiscoverRequest(options.Select(c => c.id).ToArray(), title),float.MaxValue);
                await task;
                arg.choose = options.First(c => c.id == (task.Result as DiscoverResponse).cardId);
                game.logger.log(player + "选择" + arg.choose);
            });
            return eventArg;
        }
        
        public static async Task<HearthStoneRule.SearchPileEventArg> searchPile(this Player player,CardEngine game, IEnumerable<Card> cards, bool isShow = false,  int count = 3,string title = "选择一项")
        {
            game.logger.log(player + "从" + string.Join("，", cards) + "中选择一张");
            if (cards.Count() < 1)
                return null;
            if (count > 0)
                cards = cards.randomTake(game, count);
            //var task = game.answers.ask(id, new DiscoverRequest(options.Select(c => c.id).ToArray(), title));
            var eventArg = await game.triggers.doEvent(new HearthStoneRule.SearchPileEventArg() { player = player, cards = cards, choose = null, isShow = isShow }, async arg =>
            {             
                var task = game.answers.ask(player.id, new DiscoverRequest(cards.Select(c => c.id).ToArray(), title),float.MaxValue);
                await task;
                arg.choose = cards.First(c => c.id == (task.Result as DiscoverResponse).cardId);
                game.logger.log(player + "选择" + arg.choose);
            });
            return eventArg;
        }

        public static Task<HearthStoneRule.EnterFieldEventArg> tryEnterField(this Player player,CardEngine game, Pile from, Card card, int position)
        {
            if (player.getField().isFull)//没位置了
                return Task.FromResult(default(HearthStoneRule.EnterFieldEventArg));
            return game.triggers.doEvent(new HearthStoneRule.EnterFieldEventArg() { player = player, from = from, card = card, position = position }, onEnterField);
            async Task onEnterField(HearthStoneRule.EnterFieldEventArg arg)
            {
                player = arg.player;
                from = arg.from;
                card = arg.card;
                position = arg.position;
                if (from != null)
                    game.logger.log(player + "将" + arg.card + "从" + arg.from + "置入战场，位于" + arg.position);
                else
                    game.logger.log(player + "将" + arg.card + "置入战场，位于" + arg.position);
                if (card.define is ServantCardDefine servant)//设置随从状态
                {
                    await card.setDead(game, false);
                    await card.setCurrentLife(game, card.getLife(game));
                    await card.setReady(game, false);
                }
                if (from != null)
                    await from.moveTo(game, arg.card, player.getField(), arg.position);//从某pile召唤随从
                else
                    await player.getField().insert(game, card, position);//虚空生成随从
                arg.attack = card.getAttack(game);
                arg.life = card.getLife(game);
            }
        }
        public static async Task<bool> tryEnterHead(this Player player,CardEngine game, Pile from, Card card)
        {
            if (player.getHead().isFull)
                return false;
            await from.moveTo(game, card, player.getHead());
            return true;
        }

        public static Task<HearthStoneRule.CreateTokenEventArg> createToken(this Player player,CardEngine game, CardDefine define, int position = -1)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (define == null)
                throw new ArgumentNullException(nameof(define));
            if (player.getField().count >= player.getField().maxCount)
                return null;
            if (position < 0)
                position = player.getField().count;
            HearthStoneRule.CreateTokenEventArg eventArg = new HearthStoneRule.CreateTokenEventArg() { player = player, define = define, position = position };
            return game.triggers.doEvent(eventArg, async arg =>
            {
                player = arg.player;
                define = arg.define;
                position = arg.position;
                if (player.getField().count >= player.getField().maxCount)
                    return;
                game.logger.log(player + "召唤" + define.GetType().Name + "位于" + position);
                arg.card = game.createCard(define);
                await player.tryEnterField(game, null, arg.card, position);
            });
        }
        public static async Task<HearthStoneRule.CreateTokenEventArg[]> createToken(this Player player,CardEngine game, int count, CardDefine define, int position = -1)
        {
            List<HearthStoneRule.CreateTokenEventArg> list = new List<HearthStoneRule.CreateTokenEventArg>();
            for (int i = 0; i < count; i++)
            {
                list.Add(await player.createToken(game, define, position));
            }
            return list.ToArray();
        }

        public static async Task<HearthStoneRule.RebornEventArg> reborn(this Player player,CardEngine game, Card card, Pile pile, int position = -1)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (pile.count >= pile.maxCount)
                return null;
            if (position < 0)
                position = pile.count;
            if (card == null)
                return null;
            if (card.pile.name != PILE.GRAVE)
            {
                return null;
            }
            HearthStoneRule.RebornEventArg eventArg = new HearthStoneRule.RebornEventArg() { player = player, card = card, pile = pile, position = position };
            await game.triggers.doEvent(eventArg, async arg =>
            {
                if (pile.count >= pile.maxCount)
                    return;
                game.logger.log(arg.player + "复活" + arg.card.define.GetType().Name + "位于" + position);
                await arg.player.tryEnterField(game,arg.player.getGrave(),arg.card,arg.position);
                //亡语随从在墓地与场上移动时不会进行效果的激活与禁用
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

        public static int getSpellDamage(this Player player,CardEngine game, int baseDamage)
        {
            int result = baseDamage;
            foreach (var card in player.getField())
            {
                result += card.getSpellDamage(game);
            }
            return result;
        }
        public static Task shuffleCardToDeck(this Player player,CardEngine game, CardDefine define)
        {
            Card card = game.createCard(define);
            return player.shuffleCardToDeck(game, card);
        }
        public static Task shuffleCardToDeck(this Player player,CardEngine game, Card card)
        {
            return player.getDeck().insert(game, card, game.randomInt(0, player.getDeck().count));
        }
        public static Task<Card> addCardToHand<T>(this Player player, CardEngine game) where T : CardDefine
        {
            return player.addCardToHand(game, game.getDefine<T>());
        }
        public async static Task<Card> addCardToHand(this Player player,CardEngine game, CardDefine cardDefine)
        {
            if (player.getHand().isFull)
                return null;
            Card card = game.createCard(cardDefine);
            await player.getHand().add(game, card);
            return card;
        }
        public static Task<Card> addRandomCardToHand(this Player player,CardEngine game, IEnumerable<CardDefine> pool)
        {
            return player.addCardToHand(game, pool.random(game));
        }
        public static Task randomDiscard(this Player player,CardEngine game, int count = 1, Func<Card, bool> filter = null)
        {
            var cards = filter == null ? player.getHand() : player.getHand().Where(c => filter(c));
            if (cards.Count() < 1)
                return Task.CompletedTask;
            else if (cards.Count() <= count)
                return player.discard(game, cards);
            else
                return player.discard(game, cards.randomTake(game, count));
        }
        public static Task discard(this Player player,CardEngine game, Card card)
        {
            return player.discard(game, new Card[] { card });
        }
        public static Task discard(this Player player,CardEngine game, IEnumerable<Card> cards)
        {
            return game.triggers.doEvent(new HearthStoneRule.DiscardEventArg() { player = player, cards = cards.ToArray() }, arg =>
            {
                player = arg.player;
                cards = arg.cards;
                return player.getHand().moveTo(game, cards, player.getGrave());
            });
        }

        public static bool isFriend(this Player player,Card card)
        {
            if (card.getOwner() == player)
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
        public static Card[] findCardsWherever(this Player player,CardEngine game, Func<Card, bool> filter, PileFlag pile = PileFlag.self | PileFlag.field | PileFlag.hand | PileFlag.grave | PileFlag.deck)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));
            List<Card> cardList = new List<Card>();
            foreach (var p in pile.getPiles(game, player))
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

        public static async Task<Card> Reveal(this Player player,CardEngine game, Pile cards, int define = 0)
        {
            Card show = null;

            switch (define)
            {
                case 0:
                    if (cards.count > 0)
                    {
                        show = cards.random(game);
                        await game.triggers.doEvent(new HearthStoneRule.RevealArg() { player = player, card = show }, async arg =>
                        {
                            game.logger.log("揭示了{card:" + show.define.id + "}");
                        });
                        return show;
                    }
                    else
                    {
                        await game.triggers.doEvent(new HearthStoneRule.RevealArg() { player = player, card = null }, async arg =>
                        {
                            game.logger.log("揭示了null");
                        });
                        return show;
                    }
                case 1:
                    if (cards.Count(c => c.isSpell()) > 0)
                    {
                        show = cards.Where(c => c.isSpell()).random(game);
                        await game.triggers.doEvent(new HearthStoneRule.RevealArg() { player = player, card = show }, async arg =>
                        {
                            game.logger.log("揭示了{card:" + show.define.id + "}");
                        });
                        return show;

                    }
                    else
                    {
                        await game.triggers.doEvent(new HearthStoneRule.RevealArg() { player = player, card = null }, async arg =>
                        {
                            game.logger.log("揭示了null");
                        });
                        return show;
                    }
                case 2:
                    if (cards.Count(c => c.isServant()) > 0)
                    {
                        show = cards.Where(c => c.isServant()).random(game);
                        await game.triggers.doEvent(new HearthStoneRule.RevealArg() { player = player, card = show }, async arg =>
                        {
                            game.logger.log("揭示了{card:" + show.define.id + "}");
                        });
                        return show;

                    }
                    else
                    {
                        await game.triggers.doEvent(new HearthStoneRule.RevealArg() { player = player, card = null }, async arg =>
                        {
                            game.logger.log("揭示了null");
                        });
                        return show;
                    }
                case 3:
                    if (cards.Count(c => c.isItem()) > 0)
                    {
                        show = cards.Where(c => c.isItem()).random(game);
                        await game.triggers.doEvent(new HearthStoneRule.RevealArg() { player = player, card = show }, async arg =>
                        {
                            game.logger.log("揭示了{card:" + show.define.id + "}");
                        });
                        return show;

                    }
                    else
                    {
                        await game.triggers.doEvent(new HearthStoneRule.RevealArg() { player = player, card = null }, async arg =>
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

        public delegate bool PoolRange(CardEngine game , CardDefine define);
        public delegate bool isInRange(CardEngine game, CardDefine define);

        /// <summary>
        /// 获取随机池，并从中选count张卡
        /// </summary>
        /// <param name="isInRange">随机池范围</param>
        /// <param name="game"></param>
        /// <param name="count">从随机池中获取牌的数量，默认3</param>
        /// <param name="isFull">是否获取全随机池,默认否(即随机池只包含本角色牌+中立牌)</param>
        /// <returns></returns>
        public static List<Card> getNormalRandomPool(this Player player,isInRange isInRange,CardEngine game,int count = 3, bool isFull = false)
        {
            

            int MasterID = player.getMaster().id;
            List<Card> cards = new List<Card>();
            IEnumerable<THHCardDefine> defines;
            if (isFull)
            {
                defines = ((IEnumerable<THHCardDefine>)game.getDefinesOf(CATEGORY.CHARACTER_ALL))
                    .Where(define => !define.isToken && isInRange(game, define)).randomTake(game, count).ToArray();
                return game.createCards(defines.ToArray()).ToList();
            }
            else
            {
                defines = ((IEnumerable<THHCardDefine>)game.getDefinesOf(new int[] {MasterID, CATEGORY.CHARACTER_NEUTRAL}))
                    .Where(define => !define.isToken && isInRange(game, define)).randomTake(game, count).ToArray();
                return game.createCards(defines.ToArray()).ToList();
            }
        }

        #region Command
        public static Task cmdInitReplace(this Player player,CardEngine game, params Card[] cards)
        {
            InitReplaceResponse response = new InitReplaceResponse()
            {
                playerId = player.id,
                cardsId = cards.Select(c => c.id).ToArray()
            };
            game.logPlayerAction(player.id, response);
            return game.answers.answer(player.id, response);
        }
        public static Task cmdUse(this Player player,CardEngine game, Card card, int position = 0, params Card[] targets)
        {
            var response = new UseResponse()
            {
                cardId = card.id,
                position = position,
                targetsId = targets.Select(c => c.id).ToArray()
            };
            game.logPlayerAction(player.id, response);
            return game.answers.answer(player.id, response);
        }
        public static Task cmdTurnEnd(this Player player,CardEngine game)
        {
            var resp = new TurnEndResponse() { };
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        public static Task cmdAttack(this Player player,CardEngine game, Card card, Card target)
        {
            var resp = new AttackResponse()
            {
                cardId = card.id,
                targetId = target.id
            };
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        public static Task cmdSurrender(this Player player,CardEngine game)
        {
            game.logPlayerAction(player.id, new SurrenderResponse());
            return game.surrender(player);
        }
        public static Task cmdDiscover(this Player player,CardEngine game, int cardId)
        {
            var resp = new DiscoverResponse()
            {
                cardId = cardId
            };
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        #endregion
    }
    
    public partial class HearthStoneRule
    {
        public class SetGemEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
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
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家名称，{1}是灵力值")]
            const string TEMPLATE = "{0}的灵力值变为{1}";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    value
                };
            }
        }

        public class DrawEventArg : EventArg, IDescribableEventArg
        {
            /// <summary>
            /// 抽卡的玩家
            /// </summary>
            public Player player;
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
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}玩家，{1}卡")]
            const string TEMPLATE_SELF = "{0}抽{1}";

            [MukioI18n.TranslateMsg("{0}玩家")]
            const string TEMPLATE_OTHER = "抽了一张牌";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (!isCanceled)
                {
                    return viewer == player ? TEMPLATE_SELF : TEMPLATE_OTHER;
                }
                else
                {
                    return "";
                }
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    card.getFormatString()
                };
            }
        }

        public class FatigueEventArg : EventArg, IDescribableEventArg
        {
            public Player player;

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
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家")]
            const string TEMPLATE = "{0}已经无牌可抽，即将受到疲劳伤害";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] { player.name };
            }
        }

        /// <summary>
        /// 检视事件，指从特定牌堆随机几张卡选择一张。
        /// 卡面描述：检视某牌堆X张牌
        /// </summary>
        public class SearchPileEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
            public IEnumerable<Card> cards;
            public Card choose;
            public bool isShow = true;

            public ICard getCard(IGame game, IPlayer viewer)
            {
                return choose;
            }

            public ICard[] getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是选项。")]
            const string TEMPLATE = "{0} , 选择了 {1}";
            [MukioI18n.TranslateMsg("{0}是玩家")]
            const string TEMPLATE_NotShow = "{0} ，进行了选择";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (isShow)
                    return TEMPLATE;
                else
                    return TEMPLATE_NotShow;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] 
                {
                    player.name,
                    choose?.getFormatString()
                };
            }

        }

        public class ActiveEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
            public Card card;
            public object[] targets;
            public ActiveEventArg(Player player, Card card, object[] targets)
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
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡，{2}是目标列表")]
            const string TEMPLATE_HAS_TARGET = "{0} 对 {2} 发动 {1} 的效果";

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡")]
            const string TEMPLATE_NO_TARGET = "{0} 发动了 {1} 的效果";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (targets == null || targets.Length < 1)
                    return TEMPLATE_NO_TARGET;
                else
                    return TEMPLATE_HAS_TARGET;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    card.getFormatString(),
                    string.Join(", ", targets.Select(t => t is Card c ? c.getFormatString() : t.ToString()))
                };
            }
        }

        public class CreateTokenEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
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
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡")]
            const string TEMPLATE = "{0} 召唤了 {1}";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    card.getFormatString()
                };
            }
        }

        public class DiscardEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
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
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡牌列表")]
            const string TEMPLATE = "{0} 丢弃 {1}";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    string.Join("，", cards.Select(c => c.getFormatString()))
                };
            }
        }

        
    }
}