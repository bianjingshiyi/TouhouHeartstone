using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using UnityEngine;

namespace TouhouHeartstone
{
    public static class THHPlayer
    {
        #region 公有方法

        #region 牌堆
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
        public static bool removePile(this Player player, Pile pile)
        {
            if (pile?.name == null || THHPile.vanillaPileNames.Contains(pile?.name))
                return false;
            return player.removePile(pile);
        }
        #endregion

        #region 设置玩家属性
        public static void setIsPrepared(this Player player, CardEngine game, bool value) => player.setProp(game, PLAYER.ISPREPARED, value);
        public static bool getIsPrepared(this Player player) => player.getProp<bool>(PLAYER.ISPREPARED);
        public static int getGem(this Player player) => player.getProp<int>(PLAYER.GEM);
        public static int getMaxGem(this Player player) => player.getProp<int>(PLAYER.MAXGEM);
        public static Task<HearthStoneRule.SetGemEventArg> setGem(this Player player, CardEngine game, int value)
        {
            return game.triggers.doEvent(new HearthStoneRule.SetGemEventArg() { player = player, value = value }, arg =>
            {
                arg.value = Mathf.Clamp(arg.value, 0, 10);
                var argPlayer = arg.player;
                var argValue = arg.value;
                argPlayer.setProp(game, PLAYER.GEM, argValue);
                game.logger.logTrace($"{argPlayer}的灵力值变为{argValue}");
                return Task.CompletedTask;
            });
        }
        public static Task<HearthStoneRule.SetMaxGemEventArg> setMaxGem(this Player player, CardEngine game, int value)
        {
            return game.triggers.doEvent(new HearthStoneRule.SetMaxGemEventArg() { player = player, value = value }, arg =>
            {
                arg.player.setProp(game, PLAYER.MAXGEM, arg.value);
                if (arg.player.getMaxGem() < 0)
                    arg.player.setProp(game, PLAYER.MAXGEM, 0);
                if (arg.player.getMaxGem() > 10)
                    arg.player.setProp(game, PLAYER.MAXGEM, 10);
                game.logger.logTrace($"{arg.player}的法力水晶上限变为{arg.player.getMaxGem()}");
                return Task.CompletedTask;
            });
        }
        public static void setFatigue(this Player player, CardEngine game, int value) => player.setProp(game, PLAYER.FATIGUE, value);
        public static int getFatigue(this Player player) => player.getProp<int>(PLAYER.FATIGUE);
        #endregion

        #region 抽牌
        public static Task draw(this Player player, CardEngine game, out HearthStoneRule.DrawEventArg drawEvent, out HearthStoneRule.BurnEventArg burnEvent, out HearthStoneRule.FatigueEventArg fatigueEvent)
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
                    var damage = p.getFatigue() + 1;
                    p.setFatigue(game, damage);
                    game.logger.logTrace($"{p}已经没有卡牌了，当前疲劳值：{damage}");
                    arg.damage = damage;
                    await p.getMaster().damage(game, null, damage);
                }
            }
            else
            {
                fatigueEvent = null;
                return player.draw(game, player.getDeck().top, out drawEvent, out burnEvent);
            }
        }
        public static Task draw(this Player player, CardEngine game, out HearthStoneRule.DrawEventArg drawEvent)
        {
            return player.draw(game, out drawEvent, out _, out _);
        }
        public static Task draw(this Player player, CardEngine game, out HearthStoneRule.DrawEventArg drawEvent, out HearthStoneRule.BurnEventArg burnEvent)
        {
            return player.draw(game, out drawEvent, out burnEvent, out _);
        }
        public static Task draw(this Player player, CardEngine game)
        {
            return player.draw(game, out _, out _, out _);
        }
        public static Task draw(this Player player, CardEngine game, Card card, out HearthStoneRule.DrawEventArg drawEvent, out HearthStoneRule.BurnEventArg burnEvent)
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
                    game.logger.logTrace($"{arg.player}的手牌已经满了，{card}被送入墓地");
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
                    if (card.isSpell() && card.define.hasKeyword(KEYWORD.AUTOCAST))
                    {
                        await autocast(game, p, card);
                    }
                    else
                    {
                        await p.getDeck().moveTo(game, card, p.getHand(), p.getHand().count);
                        game.logger.logTrace($"{p}抽{card}");
                    }
                });
            }
        }
        public static Task draw(this Player player, CardEngine game, Card card)
        {
            return player.draw(game, card, out _, out _);
        }
        public static Task draw(this Player player, CardEngine game, Card card, out HearthStoneRule.DrawEventArg drawEvent)
        {
            return player.draw(game, card, out drawEvent, out _);
        }
        /// <summary>
        /// 抽一张符合条件的牌，如果没有符合条件的牌则抽牌不会发生，返回事件为空。
        /// </summary>
        /// <param name="game"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Task draw(this Player player, CardEngine game, Func<Card, bool> filter, out HearthStoneRule.DrawEventArg drawEvent)
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
        #endregion

        #region 自动施放
        public static Task<AutocastEventArg> autocast(CardEngine game, Player player, Card card)
        {
            var targets = new Card[] { card.getAvaliableTargets(game).random(game) };
            return game.triggers.doEvent(new AutocastEventArg() { player = player, card = card, targets = targets }, async arg =>
            {
                var argPlayer = arg.player;
                var argCard = arg.card;
                var argTargets = arg.targets;

                await argCard.pile.moveTo(game, card, argPlayer.getWarp());
                game.logger.logTrace("debug");
                await argCard.activeEffect(game, argPlayer, argTargets);
                await argCard.pile.moveTo(game, card, argPlayer.getGrave());
                await game.updateGameState();
                await argPlayer.draw(game);
            });
        }
        #endregion

        #region 选择卡牌 
        /// <summary>
        /// 让玩家从提供的卡牌定义中选择一张，创建相应卡牌并置入手牌中
        /// </summary>
        /// <param name="player"></param>
        /// <param name="game"></param>
        /// <param name="count"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static async Task<Card> discover(this Player player, CardEngine game, IEnumerable<CardDefine> cardDefines, int count = 3,
            string title = null, bool isShow = false)
        {
            if (!cardDefines.Any())
            {
                return null;
            }
            var eventArg = await game.triggers.doEvent(new HearthStoneRule.SelectCardDefineEventArg()
            {
                player = player,
                isShow = isShow,
                options = cardDefines.ToArray()
            }, async arg =>
            {
                cardDefines = arg.options;
                player = arg.player;
                cardDefines = cardDefines.randomTake(game, count);
                var selected = await game.askSelect(player, cardDefines, count, title);
                arg.chosen = selected;
            });
            if (eventArg.chosen != null)
            {
                var card = await player.addCardToHand(game, eventArg.chosen);
                return card;
            }
            return null;
        }
        public static async Task<HearthStoneRule.SelectCardDefineEventArg> chooseOne(this Player player, CardEngine game, IEnumerable<CardDefine> options, bool isShow = true, string title = "选择一项")
        {
            if (!options.Any())
            {
                return null;
            }
            game.logger.logTrace($"{player}从{string.Join("，", options)}中选择一项");
            //var task = game.answers.ask(id, new DiscoverRequest(options.Select(c => c.id).ToArray(), title));

            var eventArg = await game.triggers.doEvent(new HearthStoneRule.SelectCardDefineEventArg() { player = player, options = options.ToArray(), chosen = null, isShow = isShow }, async arg =>
            {
                var selected = await game.askSelect(player, options, options.Count(), title);
                arg.chosen = selected;
            });
            return eventArg;
        }
        public static async Task<HearthStoneRule.SelectCardEventArg> searchPile(this Player player, CardEngine game, IEnumerable<Card> cards, bool isShow = false, int count = 3, string title = "选择一项")
        {
            game.logger.logTrace($"{player}从{string.Join("，", cards)}中选择一张");
            if (cards.Count() < 1)
                return null;
            if (count > 0)
                cards = cards.randomTake(game, count);
            //var task = game.answers.ask(id, new DiscoverRequest(options.Select(c => c.id).ToArray(), title));
            var eventArg = await game.triggers.doEvent(new HearthStoneRule.SelectCardEventArg() { player = player, options = cards.ToArray(), chosen = null, isShow = isShow }, async arg =>
            {
                var selected = await game.askSelect(player, cards, count, title);
                arg.chosen = selected;
            });
            return eventArg;
        }


        #endregion

        #region 指令
        public static Task cmdInitReplace(this Player player, CardEngine game, params Card[] cards)
        {
            InitReplaceResponse response = new InitReplaceResponse()
            {
                playerId = player.id,
                cardsId = cards.Select(c => c.id).ToArray()
            };
            game.logPlayerAction(player.id, response);
            return game.answers.answer(player.id, response);
        }
        public static Task cmdUse(this Player player, CardEngine game, Card card, int position = 0, params Card[] targets)
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
        public static Task cmdTurnEnd(this Player player, CardEngine game)
        {
            var resp = new TurnEndResponse() { };
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        public static Task cmdAttack(this Player player, CardEngine game, Card card, Card target)
        {
            var resp = new AttackResponse()
            {
                cardId = card.id,
                targetId = target.id
            };
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        public static Task cmdSurrender(this Player player, CardEngine game)
        {
            var resp = new SurrenderResponse();
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        public static Task cmdDiscover(this Player player, CardEngine game, int cardId)
        {
            var resp = new DiscoverResponse()
            {
                cardId = cardId
            };
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        public static Task cmdDiscover(this Player player, CardEngine game, DefineReference cardDefRef)
        {
            var resp = new DiscoverResponse() { cardDefRef = cardDefRef };
            game.logPlayerAction(player.id, resp);
            return game.answers.answer(player.id, resp);
        }
        #endregion

        #region 道具
        public static Task<HearthStoneRule.EquipItemEventArg> equip(this Player player, CardEngine game, Pile from, Card item)
        {
            if (from != null && !from.Contains(item)) // 源牌堆不包含该卡牌。
                return null;
            return game.triggers.doEvent(new HearthStoneRule.EquipItemEventArg() { player = player, from = from, card = item }, onEquipItem);
            async Task onEquipItem(HearthStoneRule.EquipItemEventArg arg)
            {
                var argPlayer = arg.player;
                var argFrom = arg.from;
                var argCard = arg.card;
                if (argPlayer[PILE.ITEM].isFull)
                {
                    game.logger.logTrace($"由于{argPlayer}的道具栏已满，摧毁当前道具");
                    await argPlayer.getItem().destroy(game);
                }
                var moveEvent = await argFrom.moveTo(game, argCard, argPlayer[PILE.ITEM]);
                if (moveEvent == null || !moveEvent.success)
                {
                    arg.isCanceled = true;
                    return;
                }
                game.logger.logTrace($"{argPlayer}从{arg.from}装备了{arg.card}");
            }
        }
        #endregion

        #region 移动卡牌
        public static Task<HearthStoneRule.EnterFieldEventArg> tryEnterField(this Player player, CardEngine game, Pile from, Card card, int position)
        {
            if (card == null)
                return Task.FromResult(default(HearthStoneRule.EnterFieldEventArg));
            if (!card.isServant())
                return Task.FromResult(default(HearthStoneRule.EnterFieldEventArg));
            if (!Pile.canMove(card, from, player.getField()))//没位置了
                return Task.FromResult(default(HearthStoneRule.EnterFieldEventArg));
            return game.triggers.doEvent(new HearthStoneRule.EnterFieldEventArg() { player = player, from = from, card = card, position = position }, onEnterField);
            async Task onEnterField(HearthStoneRule.EnterFieldEventArg arg)
            {
                var argPlayer = arg.player;
                var argFrom = arg.from;
                var argCard = arg.card;
                var argPosition = arg.position;
                if (argFrom != null)
                    game.logger.logTrace($"{argPlayer}将{argCard}从{argFrom}置入战场，位于{argPosition}");//从某pile召唤随从
                else
                    game.logger.logTrace($"{argPlayer}将{argCard}置入战场，位于{argPosition}");//虚空生成随从

                var field = argPlayer.getField();
                if (position < 0)
                    position = field.count;
                var moveEvent = await card.pile.moveTo(game, card, field, position);
                if (moveEvent == null || !moveEvent.success)
                {
                    arg.isCanceled = true;
                    return;
                }
            }
        }
        public static async Task<bool> tryEnterHead(this Player player, CardEngine game, Pile from, Card card)
        {
            if (player.getHead().isFull)
                return false;
            await from.moveTo(game, card, player.getHead());
            return true;
        }
        public static async Task<HearthStoneRule.EnterFieldEventArg> createToken(this Player player, CardEngine game, CardDefine define, int position = -1)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (define == null)
                throw new ArgumentNullException(nameof(define));
            if (player.getField().count >= player.getField().maxCount)
                return null;
            if (position < 0)
                position = player.getField().count;
            var card = game.createCard(define);
            return await player.tryEnterField(game, null, card, position);
        }
        [Obsolete]
        public static async Task<Card> createTokenObsolete(this Player player, CardEngine game, CardDefine define, int position = -1)
        {
            var arg = await player.createToken(game, define, position);
            return arg?.card;
        }
        public static async Task<Card[]> createToken(this Player player, CardEngine game, int count, CardDefine define, int position = -1)
        {
            List<Card> cards = new List<Card>();
            for (int i = 0; i < count; i++)
            {
                var arg = await player.createToken(game, define, position);
                if (arg?.card != null)
                    cards.Add(arg?.card);
            }
            return cards.ToArray();
        }
        /// <summary>
        /// 复生，墓地到场上
        /// </summary>
        /// <param name="player"></param>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<HearthStoneRule.RebornEventArg> reborn(this Player player, CardEngine game, Card card, int position = -1)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (position < 0)
                position = player.getField().count;
            if (card == null)
                return null;
            if (card.pile.name != PILE.GRAVE)
            {
                return null;
            }
            HearthStoneRule.RebornEventArg eventArg = new HearthStoneRule.RebornEventArg() { player = player, card = card, position = position };
            eventArg = await game.triggers.doEvent(eventArg, async arg =>
            {
                game.logger.logTrace($"{arg.player}复活{arg.card.define.GetType().Name}位于{position}");
                await arg.player.tryEnterField(game, arg.player.getGrave(), arg.card, arg.position);
                //亡语随从在墓地与场上移动时不会进行效果的激活与禁用
                //foreach (IEffect effect in arg.card.define.getDeathRattles())
                //{
                //    if (effect is IPassiveEffect passive)
                //    {
                //        await passive.onDisable(game, card, null);
                //        await passive.onEnable(game, card, null);
                //    }
                //}
            });
            return eventArg;
        }
        public static async Task<Card> shuffleCardToDeck(this Player player, CardEngine game, CardDefine define)
        {
            Card card = game.createCard(define);
            await player.getDeck().insert(game, card, game.randomInt(0, player.getDeck().count));
            return card;
        }
        public static Task<Card> shuffleCardToDeck<T>(this Player player, CardEngine game) where T : CardDefine
        {
            return shuffleCardToDeck(player, game, game.getDefine<T>());
        }
        public static Task<Card> addCardToHand<T>(this Player player, CardEngine game) where T : CardDefine
        {
            return player.addCardToHand(game, game.getDefine<T>());
        }
        public async static Task<Card> addCardToHand(this Player player, CardEngine game, CardDefine cardDefine)
        {
            if (player.getHand().isFull)
                return null;
            Card card = game.createCard(cardDefine);
            await player.getHand().add(game, card);
            return card;
        }
        public static Task<Card> addRandomCardToHand(this Player player, CardEngine game, IEnumerable<CardDefine> pool)
        {
            return player.addCardToHand(game, pool.random(game));
        }
        public static Task<MassDiscardEventArg> randomDiscard(this Player player, CardEngine game, int count = 1, Func<Card, bool> filter = null)
        {
            var cards = filter == null ? player.getHand() : player.getHand().Where(c => filter(c));
            if (cards.Count() < 1)
                return default;
            var discards = cards;
            if (cards.Count() > count)
                discards = cards.randomTake(game, count);
            return discards.discard(game);
        }
        #endregion

        #region 信仰对决
        /// <summary>
        /// 揭示对决
        /// 双方揭示牌库里的一张牌，法力消耗大的胜利
        /// </summary>
        /// <param name="game"></param>
        /// <param name="define">设定揭示的类别:0是所有类别；1是法术牌；2是随从牌；3是武器牌</param>
        /// <returns></returns>
        public static Task<RevealDuelEventArg> revealDuel(this Player player, CardEngine game, int define)
        {
            return player.revealDuel(game, getRevealFilterByDefine(define));
        }

        /// <summary>
        /// 揭示对决
        /// 双方揭示牌库里的一张牌，法力消耗大的胜利
        /// </summary>
        /// <param name="game"></param>
        /// <param name="define">设定揭示的类别:0是所有类别；1是法术牌；2是随从牌；3是武器牌</param>
        /// <returns></returns>
        public static Task<RevealDuelEventArg> revealDuel(this Player player, CardEngine game, Predicate<Card> filter = null)
        {
            return game.triggers.doEvent(new RevealDuelEventArg() { player = player }, async arg =>
            {
                var duelPlayer = arg.player;
                Player oppo = game.getOpponent(duelPlayer);
                arg.youCard = await game.Reveal(duelPlayer.getDeck(), filter, false);
                arg.oppoCard = await game.Reveal(oppo.getDeck(), filter, false);
                arg.result = false;

                if (arg.youCard != null && arg.oppoCard == null)
                    arg.result = true;
                else if (arg.youCard != null && arg.youCard.getCost(game) > arg.oppoCard.getCost(game))
                    arg.result = true;
                if (arg.youCard != null)
                    game.logger.log($"{duelPlayer}发动了对决，她揭示了：{arg.youCard}。她的对手揭示了：{arg.oppoCard}。结果是{arg.result}");
                else
                    game.logger.log($"{duelPlayer}揭示了个寂寞，输掉了对决。");
            });
        }
        private static Predicate<Card> getRevealFilterByDefine(int define)
        {
            bool filter(Card card)
            {
                switch (define)
                {
                    case 0:
                        return true;
                    case 1:
                        return card.isSpell();
                    case 2:
                        return card.isServant();
                    case 3:
                        return card.isItem();
                    default:
                        return false;
                }
            }
            return filter;
        }

        #endregion 信仰对决

        #region 法术伤害
        public static int getSpellDamage(this Player player, CardEngine game, int baseDamage, Card spell)
        {
            return player.getSpellDamage(game, baseDamage) + (spell?.getSpellDamage(game) ?? 0);
        }
        public static int getSpellDamage(this Player player, CardEngine game, int baseDamage)
        {
            int result = baseDamage;
            foreach (var servant in player.getField())
            {
                result += servant.getSpellDamage(game);
            }
            result += player.getItem()?.getSpellDamage(game) ?? 0;
            result += player.getSkill()?.getSpellDamage(game) ?? 0;
            result += player.getMaster()?.getSpellDamage(game) ?? 0;
            return result;
        }
        #endregion

        #region 杂项
        public static async Task updateSecrets(this Player player, CardEngine game)
        {
            foreach (var secret in player.getHead()) // 位于延迟区。
            {
                await secret.updateSecret(game);
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
        public static async Task<bool> tryUse(this Player player, CardEngine game, Card card, int position, params Card[] targets)
        {
            if (!card.isUsable(game, player, out _))
                //卡牌不可用
                return false;
            if (card.isSkill())
            {
                //使用技能，设置技能使用状态为“已使用”
                await card.setUsed(game, true);
            }
            await player.setGem(game, player.getGem() - card.getCost(game));//消耗法力值
            await game.triggers.doEvent(new HearthStoneRule.UseEventArg(player, card, position, targets, card.getCost(game)), async arg =>
            {
                Player p = arg.player;
                card = arg.card;
                targets = arg.targets;
                var targetLog = targets.Length > 0 ? $"，目标：{string.Join<Card>("，", targets)}" : null;
                game.logger.logTrace($"{p}使用{arg.card}{targetLog}");
                if (arg.card.isServant())
                {
                    await player.tryEnterField(game, p.getHand(), arg.card, arg.position);//随从卡，将卡置入战场
                    //await card.ComeOnStage(game, p, targets);//触发战吼事件
                    await card.activeEffect(game, p, targets);//执行随从效果
                }
                else if (card.isSkill())
                {
                    await card.activeEffect(game, p, targets);
                }
                else if (card.isSpell())
                {
                    //法术卡,释放效果
                    await p.getHand().moveTo(game, card, p.getWarp());
                    if (card.isTrap(game) || card.hasKeyword(game, KEYWORD.MISSION))
                    {
                        await player.tryEnterHead(game, player.getWarp(), card);
                        await card.updateSecret(game);
                    }
                    else
                    {
                        await card.activeEffect(game, p, targets);
                        await p.getWarp().moveTo(game, card, p.getGrave());//送入墓地
                        if (card.hasKeyword(game, KEYWORD._RETURNHEAD))
                        {
                            await card.returnToHand(game, p);
                        }
                    }
                }
                else if (card.isItem())
                {
                    //物品卡，置入物品栏
                    await p.equip(game, p.getHand(), card);
                    await card.activeEffect(game, p, targets);
                }
            });
            return true;
        }
        public static Card getMaster(this Player player) => player.getPile(PILE.MASTER)[0];
        /// <summary>
        /// 找到符合条件的卡片，无论它在哪里。
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <remarks>但是其实并不会真的哪里都去找啦</remarks>
        public static Card[] findCardsWherever(this Player player, CardEngine game, Func<Card, bool> filter, PileFlag pile = PileFlag.self | PileFlag.field | PileFlag.hand | PileFlag.grave | PileFlag.deck)
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
        public static int getTurnCount(this Player player)
        {
            return player.getProp<int>(PLAYER.TURNCOUNT);
        }
        #endregion

        #endregion

        #region 私有方法
        internal static async Task initReplace(this Player player, CardEngine game, params Card[] cards)
        {
            await game.triggers.doEvent(new HearthStoneRule.InitReplaceEventArg() { player = player, cards = cards }, onInitReplace);
            async Task onInitReplace(HearthStoneRule.InitReplaceEventArg arg)
            {
                arg.replacedCards = await arg.player.getInit().replaceByRandom(game, arg.cards, arg.player.getDeck());
                var cardStrings = string.Join("，", arg.cards.Select(c => c.ToString()));
                var newCardStrings = string.Join("，", arg.replacedCards.Select(c => c.ToString()));
                game.logger.logTrace($"{arg.player}替换卡牌：{cardStrings}=>{newCardStrings}");
            }
        }
        #endregion

        #region 委托
        public delegate bool PoolRange(CardEngine game, CardDefine define);
        public delegate bool isInRange(CardEngine game, CardDefine define);
        #endregion

        #region 内嵌类
        public class RevealDuelEventArg : EventArg, IDescribableEventArg
        {
            /// <summary>
            /// 对决发起方
            /// </summary>
            public Player player
            {
                get => getVar<Player>(EVENTVAR.PLAYER);
                set => setVar(EVENTVAR.PLAYER, value);
            }
            /// <summary>
            /// 发起方揭示的牌
            /// </summary>
            public Card youCard
            {
                get => getVar<Card>(VAR_SELF_CARD);
                set => setVar(VAR_SELF_CARD, value);
            }
            /// <summary>
            /// 另一方揭示的牌
            /// </summary>
            public Card oppoCard
            {
                get => getVar<Card>(VAR_OPPO_CARD);
                set => setVar(VAR_OPPO_CARD, value);
            }
            /// <summary>
            /// 是否胜利
            /// </summary>
            public bool result
            {
                get => getVar<bool>(VAR_RESULT);
                set => setVar(VAR_RESULT, value);
            }

            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return result ? youCard : oppoCard;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return new DescribableCard[] { youCard, oppoCard };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setCardState(VAR_SELF_CARD, youCard);
                record.setCardState(VAR_OPPO_CARD, oppoCard);
                record.setVar(VAR_RESULT, result);
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return string.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是自己的卡，{2}是对手的卡，{3}是结果")]
            const string TEMPLATE = "{0}发动了对决，揭示了:{1}。 对手揭示了：{2}。 结果是 {3}";

            [MukioI18n.TranslateMsg("{0}是玩家")]
            const string TEMPLATE_NULL = "{0}发动了对决 揭示了个寂寞，输掉了对决。";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return youCard != null ? TEMPLATE : TEMPLATE_NULL;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    youCard?.getFormatString(),
                    oppoCard?.getFormatString(),
                    result?"胜利":"失败"
                };
            }
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
                    new EventVariableInfo() { name = VAR_SELF_CARD, type = typeof(Card) },
                    new EventVariableInfo() { name = VAR_OPPO_CARD, type = typeof(Card) },
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo() { name = EVENTVAR.PLAYER, type = typeof(Player) },
                    new EventVariableInfo() { name = VAR_SELF_CARD, type = typeof(Card) },
                    new EventVariableInfo() { name = VAR_OPPO_CARD, type = typeof(Card) },
                    new EventVariableInfo() { name = VAR_RESULT, type = typeof(bool) },
                };
            }
            public const string VAR_SELF_CARD = "己方卡牌";
            public const string VAR_OPPO_CARD = "对方卡牌";
            public const string VAR_RESULT = "是否胜利";
        }
        #endregion
    }

    public partial class HearthStoneRule
    {
        #region 内嵌类
        public class SetGemEventArg : EventArg, IDescribableEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return null;
            }
            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return string.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
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

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.VALUE,type=typeof(int)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.VALUE,type=typeof(int)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setVar(EVENTVAR.VALUE, value);
            }
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
            public int value
            {
                get { return getVar<int>(EVENTVAR.VALUE); }
                set { setVar(EVENTVAR.VALUE, value); }
            }
        }
        [EventChildren(typeof(THHPile.MoveCardEventArg))]
        public class DrawEventArg : EventArg, IDescribableEventArg, ICardEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return viewer == player ? card : null;
            }
            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            ICard ICardEventArg.getCard()
            {
                return card;
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
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setCardState(EVENTVAR.CARD, card);
            }
            /// <summary>
            /// 抽卡的玩家
            /// </summary>
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
            /// <summary>
            /// 抽到的卡
            /// </summary>
            public Card card
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
        }
        [EventChildren(typeof(THHCard.DamageEventArg))]
        public class FatigueEventArg : EventArg, IDescribableEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return null;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
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

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setVar(VAR_DAMAGE, damage);
            }
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
            public int damage
            {
                get { return getVar<int>(VAR_DAMAGE); }
                set { setVar(VAR_DAMAGE, value); }
            }
            public const string VAR_DAMAGE = "伤害";
        }
        /// <summary>
        /// 选择卡牌事件
        /// </summary>
        public class SelectCardEventArg : EventArg, IDescribableEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return chosen;
            }
            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setCardStates(VAR_OPTIONS, options);
                record.setCardState(EVENTVAR.CHOSEN_CARD, chosen);
                record.setVar(VAR_CARD_SEEN, isShow);
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return string.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

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
                    chosen?.getFormatString()
                };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = VAR_OPTIONS,type=typeof(Card[])}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = VAR_OPTIONS,type=typeof(Card[])},
                    new EventVariableInfo(){name = EVENTVAR.CHOSEN_CARD,type=typeof(Card)}
                };
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是选项。")]
            const string TEMPLATE = "{0} , 选择了 {1}";
            [MukioI18n.TranslateMsg("{0}是玩家")]
            const string TEMPLATE_NotShow = "{0} ，进行了选择";

            public const string VAR_OPTIONS = "选项";
            public const string VAR_CARD_SEEN = "明牌";
            public Player player
            {
                get => getVar<Player>(EVENTVAR.PLAYER);
                set => setVar(EVENTVAR.PLAYER, value);
            }
            public Card[] options
            {
                get => getVar<Card[]>(VAR_OPTIONS);
                set => setVar(VAR_OPTIONS, value);
            }
            public Card chosen
            {
                get => getVar<Card>(EVENTVAR.CHOSEN_CARD);
                set => setVar(EVENTVAR.CHOSEN_CARD, value);
            }
            public bool isShow
            {
                get => getVar<bool>(VAR_CARD_SEEN);
                set => setVar(VAR_CARD_SEEN, value);
            }

        }
        public class ActiveEventArg : EventArg, IDescribableEventArg, ICardEventArg
        {
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){ name = EVENTVAR.CARD, type = typeof(Card)},
                    new EventVariableInfo() { name = EVENTVAR.PLAYER,type = typeof(Player)},
                    new EventVariableInfo() { name = EVENTVAR.TARGET_CARD,type = typeof(Card[])}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){ name = EVENTVAR.CARD, type = typeof(Card)},
                    new EventVariableInfo() { name = EVENTVAR.PLAYER,type = typeof(Player)},
                    new EventVariableInfo() { name = EVENTVAR.TARGET_CARD,type = typeof(Card[])}
                };
            }
            public ActiveEventArg(Player player, Card card, Card[] targets)
            {
                this.player = player;
                this.card = card;
                this.targets = targets ?? Array.Empty<Card>();
            }
            public ActiveEventArg(): this(null, null, new Card[0])
            {
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setCardState(EVENTVAR.CARD, card);
                record.setCardStates(EVENTVAR.TARGET_CARD, targets.OfType<Card>());
            }

            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return targets.OfType<Card>().Select(t => (DescribableCard)t).ToArray();
            }
            ICard ICardEventArg.getCard()
            {
                return card;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return string.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
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
            public Player player
            {
                get => getVar<Player>(EVENTVAR.PLAYER);
                set => setVar(EVENTVAR.PLAYER, value);
            }
            public Card card
            {
                get => getVar<Card>(EVENTVAR.CARD);
                set => setVar(EVENTVAR.CARD, value);
            }
            public Card[] targets
            {
                get => getVar<Card[]>(EVENTVAR.TARGET_CARD);
                set => setVar(EVENTVAR.TARGET_CARD, value);
            }
        }
        public class LastwordEventArg : EventArg, IDescribableEventArg
        {
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){ name = "卡牌", type = typeof(Card)},
                    new EventVariableInfo() { name = "玩家",type = typeof(Player)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){ name = "卡牌", type = typeof(Card)},
                    new EventVariableInfo() { name = "玩家",type = typeof(Player)}
                };
            }
            public LastwordEventArg(Player player, Card card)
            {
                this.player = player;
                this.card = card;
            }
            public LastwordEventArg() : this(null, null)
            {
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(EVENTVAR.PLAYER, player);
                record.setCardState(EVENTVAR.CARD, card);
            }
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return string.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡")]
            const string TEMPLATE_NO_TARGET = "{0} 的 {1} 触发遗言";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE_NO_TARGET;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    card.getFormatString()
                };
            }
            public Player player
            {
                get => getVar<Player>(EVENTVAR.PLAYER);
                set => setVar(EVENTVAR.PLAYER, value);
            }
            public Card card
            {
                get => getVar<Card>(EVENTVAR.CARD);
                set => setVar(EVENTVAR.CARD, value);
            }
        }
        #endregion
    }
}