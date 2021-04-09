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
        public Card getMaster() => getPile(PILE.MASTER)[0];

        public Card getSkill() => getPile(PILE.SKILL)[0];
        public Pile getDeck() => getPile(PILE.DECK);
        public Pile getInit() => getPile(PILE.INIT);
        public Pile getHand() => getPile(PILE.HAND);
        public Pile getField() => getPile(PILE.FIELD);
        public Pile getGrave() => getPile(PILE.GRAVE);

        /// <summary>
        /// 正在使用的法术卡都会被置入亚空间
        /// </summary>
        public Pile getWarp() => getPile(PILE.WARP);

        /// <summary>
        /// 头顶的五个玩意
        /// </summary>
        public Pile getHead() => getPile(PILE.HEAD);

        public Card getItem()
        {
            return this[PILE.ITEM].Any() ? this[PILE.ITEM][0] : null;
        }

        public void setIsPrepared(bool value) => setProp(PLAYER.ISPREPARED, value);
        public bool getIsPrepared() => getProp<bool>(PLAYER.ISPREPARED);
        public int getGem() => getProp<int>(PLAYER.GEM);
        public int getMaxGem() => getProp<int>(PLAYER.MAXGEM);

        private void setFatigue(int value) => setProp(PLAYER.FATIGUE, value);
        public int getFatigue() => getProp<int>(PLAYER.FATIGUE);

        public THHPlayer(THHGame game, int id, string name, MasterCardDefine master, IEnumerable<CardDefine> deck) : base(id, name)
        {
            addPile(new Pile(game, PILE.MASTER, 1));
            addPile(new Pile(game, PILE.SKILL, 1));
            addPile(new Pile(game, PILE.DECK));
            addPile(new Pile(game, PILE.HAND, maxCount: 10));
            addPile(new Pile(game, PILE.INIT, maxCount: 4));
            addPile(new Pile(game, PILE.FIELD, maxCount: 7));
            addPile(new Pile(game, PILE.GRAVE));
            addPile(new Pile(game, PILE.WARP));
            addPile(new Pile(game, PILE.HEAD, maxCount: 5));
            addPile(new Pile(game, PILE.ITEM, 1));
            this[PILE.MASTER].add(game, game.createCard(master));
            this[PILE.SKILL].add(game, game.createCard(game.getDefine(master.skillID)));
            this[PILE.DECK].add(game, deck.Select(d => game.createCard(d)).ToArray());
            setIsPrepared(false);
            setGem(game, 0);
            setMaxGem(game, 0);
            setFatigue(0);
        }
        internal async Task initReplace(THHGame game, params Card[] cards)
        {
            await game.triggers.doEvent(new InitReplaceEventArg() { player = this, cards = cards }, onInitReplace);
            async Task onInitReplace(InitReplaceEventArg arg)
            {
                arg.replacedCards = await arg.player.getInit().replaceByRandom(game, arg.cards, arg.player.getDeck());
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
                arg.player.setProp(PLAYER.GEM, arg.value);
                if (arg.player.getGem() < 0)
                    arg.player.setProp(PLAYER.GEM, 0);
                if (arg.player.getGem() > 10)
                    arg.player.setProp(PLAYER.GEM, 10);
                game.logger.log(arg.player + "的法力水晶变为" + arg.player.getGem());
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
        public Task setMaxGem(THHGame game, int value)
        {
            return game.triggers.doEvent(new SetMaxGemEventArg() { player = this, value = value }, arg =>
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
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}:玩家，{1}值")]
            const string TEMPLATE = "{0}的最大灵力值变为{1}";
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
        public Task draw(THHGame game, out DrawEventArg drawEvent, out BurnEventArg burnEvent, out FatigueEventArg fatigueEvent)
        {
            if (getDeck().count < 1)//无牌可抽，疲劳！
            {
                drawEvent = null;
                burnEvent = null;
                fatigueEvent = new FatigueEventArg() { player = this };
                return game.triggers.doEvent(fatigueEvent, onFatigue);
                async Task onFatigue(FatigueEventArg arg)
                {
                    THHPlayer player = arg.player;
                    player.setFatigue(player.getFatigue() + 1);
                    game.logger.log(player + "已经没有卡牌了，当前疲劳值：" + player.getFatigue());
                    var damage = await player.getMaster().damage(game, null, player.getFatigue());
                    await game.updateDeath();
                }
            }
            else
            {
                fatigueEvent = null;
                return draw(game, getDeck().top, out drawEvent, out burnEvent);
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
            if (!getDeck().Contains(card))
            {
                drawEvent = null;
                burnEvent = null;
                return Task.CompletedTask;
            }
            if (getHand().count >= getHand().maxCount)
            {
                drawEvent = null;
                burnEvent = new BurnEventArg() { player = this, card = card };
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
                drawEvent = new DrawEventArg() { player = this, card = card };
                burnEvent = null;
                return game.triggers.doEvent(drawEvent, async arg =>
                {
                    THHPlayer player = arg.player;
                    card = arg.card;
                    if (card.define is SpellCardDefine spell && card.define.hasKeyword(KEYWORD.AUTOCAST))
                    {
                        await player.getDeck().moveTo(game, card, player.getWarp());
                        game.logger.log("debug");
                        await card.activeEffect(game, player, new Card[0]);
                        await player.getWarp().moveTo(game, card, player.getGrave());
                        await game.updateDeath();
                        await draw(game);
                    }
                    else
                    {
                        await player.getDeck().moveTo(game, card, player.getHand(), player.getHand().count);
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
            var cards = getDeck().Where(filter);
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
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是手牌")]
            const string TEMPLATE = "{0}抽{1}，但是由于手牌已满它被送入墓地";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] { player.name, card.getFormatString() };
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
            await setGem(game, getGem() - card.getCost(game));//消耗法力值
            await game.triggers.doEvent(new UseEventArg() { player = this, card = card, position = position, cost = card.getCost(game), targets = targets }, async arg =>
             {
                 THHPlayer player = arg.player;
                 card = arg.card;
                 targets = arg.targets;
                 game.logger.log(arg.player + "使用" + arg.card + (targets.Length > 0 ? "，目标：" + string.Join<Card>("，", targets) : null));
                 if (arg.card.define is ServantCardDefine || (card.define is GeneratedCardDefine && (card.define as GeneratedCardDefine).type == DEFINE.SERVANT))
                 {
                     await tryEnterField(game, arg.player.getHand(), arg.card, arg.position);//随从卡，将卡置入战场
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
                     await player.getHand().moveTo(game, card, player.getWarp());
                     if (card.hasKeyword(game, KEYWORD.SECRET))
                         await tryEnterHead(game, getWarp(), card);
                     else
                     {
                         await card.activeEffect(game, player, targets);
                         await card.resetCard(game);
                         if (card.hasKeyword(game, KEYWORD._RETURNHEAD))
                         {
                             await player.getWarp().moveTo(game, card, player.getHand());
                         }
                         await player.getWarp().moveTo(game, card, player.getGrave());//如果是普通法术，则送入墓地
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
            await getHand().moveTo(game, item, this[PILE.ITEM]);
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

        public async Task<ChooseOneEventArg> chooseOne(THHGame game,IEnumerable<Card> options, bool isShow = true ,string title = "选择一项")
        {
            game.logger.log(this + "从" + string.Join("，", options) + "中选择一项");
            //var task = game.answers.ask(id, new DiscoverRequest(options.Select(c => c.id).ToArray(), title));
            var eventArg = await game.triggers.doEvent(new ChooseOneEventArg() { player = this , options = options , choose = null , isShow = isShow }, async arg =>
            {
                var task = game.answers.ask(id, new DiscoverRequest(options.Select(c => c.id).ToArray(), title));
                await task;
                arg.choose = options.First(c => c.id == (task.Result as DiscoverResponse).cardId);
                game.logger.log(this + "选择" + arg.choose);
            });
            return eventArg;
        }

        /// <summary>
        /// 选择事件，一般指从几个选项中选择一项
        /// 卡面描述："选择一项"
        /// </summary>
        public class ChooseOneEventArg : EventArg, IDescribableEventArg
        {
            public THHPlayer player;
            public IEnumerable<Card> options;
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
            const string TEMPLATE = "{0} 选择 {1}";
            [MukioI18n.TranslateMsg("{0}是玩家")]
            const string TEMPLATE_NotShow = "{0} 进行了选择";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (isShow)
                    return TEMPLATE;
                else
                    return TEMPLATE_NotShow;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    choose?.getFormatString()
                };
            }
            
        }

        public async Task<SearchPileEventArg> searchPile(THHGame game, IEnumerable<Card> cards, bool isShow = false,  int count = 3,string title = "选择一项")
        {
            game.logger.log(this + "从" + string.Join("，", cards) + "中选择一张");
            if (cards.Count() < 1)
                return null;
            if (count > 0)
                cards = cards.randomTake(game, count);
            //var task = game.answers.ask(id, new DiscoverRequest(options.Select(c => c.id).ToArray(), title));
            var eventArg = await game.triggers.doEvent(new SearchPileEventArg() { player = this, cards = cards, choose = null, isShow = isShow }, async arg =>
            {             
                var task = game.answers.ask(id, new DiscoverRequest(cards.Select(c => c.id).ToArray(), title));
                await task;
                arg.choose = cards.First(c => c.id == (task.Result as DiscoverResponse).cardId);
                game.logger.log(this + "选择" + arg.choose);
            });
            return eventArg;
        }

        /// <summary>
        /// 检视事件，指从特定牌堆随机几张卡选择一张。
        /// 卡面描述：检视某牌堆X张牌
        /// </summary>
        public class SearchPileEventArg : EventArg, IDescribableEventArg
        {
            public THHPlayer player;
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
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡。")]
            const string TEMPLATE_NO_TARGET = "{0} 使用 {1}";
            [MukioI18n.TranslateMsg("{0}是玩家，使用的是陷阱")]
            const string TEMPLATE_SECRET_OTHER = "{0} 使用了一张陷阱牌";
            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡，{2}是目标")]
            const string TEMPLATE_HAS_TARGET = "{0} 对 {2} 使用了 {1}";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (card.hasKeyword(game, KEYWORD.SECRET))
                {
                    return viewer == player ? TEMPLATE_NO_TARGET : TEMPLATE_SECRET_OTHER;
                }
                if (targets == null || targets.Length < 1)
                    return TEMPLATE_NO_TARGET;
                else
                    return TEMPLATE_HAS_TARGET;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    card?.getFormatString(),
                    string.Join("，", targets?.Select(c => "{card:" + c.define.id + "}"))
                };
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
        public Task<EnterFieldEventArg> tryEnterField(THHGame game, Pile from, Card card, int position)
        {
            if (getField().count >= getField().maxCount)//没位置了
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
                    await from.moveTo(game, arg.card, arg.player.getField(), arg.position);//从某pile召唤随从
                else
                    await player.getField().insert(game, card, position);//虚空生成随从
                arg.attack = card.getAttack(game);
                arg.life = card.getLife(game);
            }
        }
        public async Task<bool> tryEnterHead(THHGame game, Pile from, Card card)
        {
            if (getHead().isFull)
                return false;
            await from.moveTo(game, card, getHead());
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
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡，{2}是位置")]
            const string TEMPLATE = "{0} 将{1} 置入战场，位于 {2}";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    card.getFormatString(),
                    position
                };
            }
        }
        public Task<CreateTokenEventArg> createToken(THHGame game, CardDefine define, int position = -1)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (define == null)
                throw new ArgumentNullException(nameof(define));
            if (getField().count >= getField().maxCount)
                return null;
            if (position < 0)
                position = getField().count;
            CreateTokenEventArg eventArg = new CreateTokenEventArg() { player = this, define = define, position = position };
            return game.triggers.doEvent(eventArg, async arg =>
            {
                THHPlayer player = arg.player;
                define = arg.define;
                position = arg.position;
                if (getField().count >= getField().maxCount)
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
            if (card.pile.name != PILE.GRAVE)
            {
                return null;
            }
            RebornEventArg eventArg = new RebornEventArg() { player = this, card = card, pile = pile, position = position };
            await game.triggers.doEvent(eventArg, async arg =>
            {
                THHPlayer player = arg.player;
                position = arg.position;
                if (pile.count >= pile.maxCount)
                    return;
                game.logger.log(player + "复活" + arg.card.define.GetType().Name + "位于" + position);
                await arg.player.getGrave().moveTo(game, arg.card, arg.pile, position);
                //亡语随从在墓地与场上移动时不会进行效果的激活与禁用
                foreach (IPassiveEffect effect in arg.card.define.effects.OfType<DeathRattle>())
                {
                    if (effect.piles.Contains(pile.name))
                    {
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
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡")]
            const string TEMPLATE = "{0} 复活了 {1}";
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
        public int getSpellDamage(THHGame game, int baseDamage)
        {
            int result = baseDamage;
            foreach (var card in getField())
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
            return getDeck().insert(game, card, game.randomInt(0, getDeck().count));
        }
        public Task addCardToHand<T>(THHGame game) where T : CardDefine
        {
            if (getHand().isFull)
                return Task.CompletedTask;
            Card card = game.createCard<T>();
            return getHand().add(game, card);
        }
        public async Task<bool> tryAddCardToHand(THHGame game, Card card)
        {
            if (getHand().isFull)
                return false;
            game.logger.log("将" + card + "置入" + this + "的手牌");
            await getHand().add(game, card);
            return true;
        }
        public Task addRandomCardToHand(THHGame game, IEnumerable<CardDefine> pool)
        {
            if (getHand().isFull)//手牌满了
                return Task.CompletedTask;
            Card card = game.createCardByRandom(pool);
            return getHand().add(game, card);
        }
        public Task randomDiscard(THHGame game, int count = 1, Func<Card, bool> filter = null)
        {
            var cards = filter == null ? getHand() : getHand().Where(c => filter(c));
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
                return player.getHand().moveTo(game, cards, player.getGrave());
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

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡")]
            const string TEMPLATE_VALID = "{0} 揭示了 {1}";

            [MukioI18n.TranslateMsg("{0}是玩家")]
            const string TEMPLATE_INVALID = "{0} 揭示了个寂寞";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return card == null ? TEMPLATE_INVALID : TEMPLATE_VALID;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    card?.getFormatString()
                };
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