using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using Boo.Lang;
using System.Runtime.InteropServices;
using System;
using System.Security.Policy;

namespace TouhouHeartstone.Builtin
{
    /// <summary>
    /// 东风谷早苗
    /// </summary>
    public class Sanae : MasterCardDefine
    {
        public const int ID = 0x00300000 | CATEGORY.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = WindStopMiracle.ID;
        public override IEffect[] effects { get; set; } = new Effect[0];

        /// <summary>
        /// 揭示对决
        /// 双方揭示牌库里的一张牌，法力消耗大的胜利
        /// </summary>
        /// <param name="game"></param>
        /// <param name="define">设定揭示的类别:0是所有类别；1是法术牌；2是随从牌；3是武器牌</param>
        /// <returns></returns>
        public static Task<RevealDuelArg> revealDuel(THHPlayer player, THHGame game, int define = 0)
        {
            return game.triggers.doEvent(new RevealDuelArg() { player = player }, async arg =>
            {
                THHPlayer oppo = game.getOpponent(player);
                arg.youCard = await player.Reveal(game, player.deck, define);
                arg.yourCardSnapshot = arg.youCard != null ? new CardSnapshot(game, arg.youCard) : null;
                arg.oppoCard = await oppo.Reveal(game, oppo.deck, define);
                arg.oppoCardSnapshot = arg.oppoCard != null ? new CardSnapshot(game, arg.oppoCard) : null;
                arg.result = false;

                if (arg.youCard != null && arg.oppoCard == null)
                    arg.result = true;
                else if (arg.youCard != null && arg.youCard.getCost(game) > arg.oppoCard.getCost(game))
                    arg.result = true;
                if (arg.youCard != null)
                    game.logger.log(player + "发动了对决，她揭示了：" + arg.youCard + "。她的对手揭示了：" + arg.oppoCard + "。结果是" + arg.result);
                else
                    game.logger.log(player + "揭示了个寂寞，输掉了对决。");
            });
        }

        public class RevealDuelArg : EventArg, IDescribableEventArg
        {
            /// <summary>
            /// 对决发起方
            /// </summary>
            public THHPlayer player;
            /// <summary>
            /// 发起方揭示的牌
            /// </summary>
            public Card youCard;
            /// <summary>
            /// 你的揭示牌快照
            /// </summary>
            public CardSnapshot yourCardSnapshot;
            /// <summary>
            /// 另一方揭示的牌
            /// </summary>
            public Card oppoCard;
            /// <summary>
            /// 对方的揭示牌快照
            /// </summary>
            public CardSnapshot oppoCardSnapshot;
            /// <summary>
            /// 是否胜利
            /// </summary>
            public bool result;
            public ICard getCard(IGame game, IPlayer viewer)
            {
                return null;
            }

            public ICard[] getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            public string toString(IGame game, IPlayer viewer)
            {
                return youCard != null ? player.name + "发动了对决，她揭示了:{ card:" + youCard.define.id + "} " + "。她的对手揭示了：{ card:" + oppoCard + "}。结果是" + result : player + "揭示了个寂寞，输掉了对决。";
            }
        }
    }
    /// <summary>
    /// 风止奇迹：治愈2点生命值。
    /// </summary>
    public class WindStopMiracle : SkillCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
                                                {
                                                    await target.heal(game,card,2);
                                                },PileFlag.both | PileFlag.character)
        };
    }

    /// <summary>
    /// 星之仪式
    /// 
    /// </summary>
    public class StarRitual : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { KEYWORD._RevealDuel };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            THHPlayer you = card.getOwner();
            var duel = await Sanae.revealDuel(you, game);
            if (duel.result)
            {
                Card _card = duel.youCard;
                bool x = true;
                foreach (Card c in you.deck)
                {
                    if (c.getCost(game) > _card.getCost(game))
                    {
                        x = false;
                        break;
                    }
                }
                if (x)
                {
                    await card.getOwner().draw(game, _card);
                }
                else
                {
                    await you.addCardToHand<StarRitual>(game);
                }
                ////遍历牌库寻找法力消耗最高的牌
                //Card _card = card.getOwner().deck.top;
                //foreach (Card c in card.getOwner().deck)
                //{
                //    if (c.getCost(game) > _card.getCost(game))
                //    {
                //        _card = c;
                //    }
                //}
                //await card.getOwner().draw(game, _card);
            }
            else
            {
                await you.addCardToHand<StarRitual>(game);
            }
        }
    }

    /// <summary>
    /// 灰色秘术
    /// 信仰对决
    /// 治愈8点生命值。
    /// 本回合中,如果你打出一张信仰对决牌，再为它治愈4点生命值。
    /// </summary>
    public class GrayThaumaturgy : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                await target.heal(game,card,8);
                await target.addBuff(game,new GeneratedBuff(ID,new THHEffectAfter<THHPlayer.UseEventArg>(PILE.FIELD,(g,c,a)=>
                      {
                          return a.player == c.getOwner() && a.card.define.hasKeyword(KEYWORD._RevealDuel);
                      },null,async(g,c,a)=>
                      {
                            await target.heal(game,card,4);
                      }),
                      new RemoveBuffAfter<THHPlayer.UseEventArg>(PILE.FIELD,(g,c,a)=>
                      {
                          return a.player == c.getOwner() && a.card.define.hasKeyword(KEYWORD._RevealDuel);
                      },ID),
                      new RemoveBuffAfter<THHPlayer.UseEventArg>(PILE.MASTER,(g,c,a)=>
                      {
                          return a.player == c.getOwner() && a.card.define.hasKeyword(KEYWORD._RevealDuel);
                      },ID),
                      new RemoveBuffAfter<THHGame.TurnEndEventArg>(PILE.FIELD,(g,c,a)=>
                      {
                          return a.player == c.getOwner();
                      },ID),
                      new RemoveBuffAfter<THHGame.TurnEndEventArg>(PILE.MASTER,(g,c,a)=>
                      {
                          return a.player == c.getOwner();
                      },ID)));
            })
        };

    }

    /// <summary>
    /// 商业繁荣祈愿
    /// 信仰对决（随从牌） 对目标随从造成3点伤害；获胜时再治愈你的英雄4点生命值。
    /// </summary>
    [IgnoreCardDefine]
    public class CharmOfGoodCommerce : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD._RevealDuel };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                THHPlayer you = card.getOwner();
                var duel = await Sanae.revealDuel(you, game,2);
                if (duel.result)
                {
                    await you.master.heal(game,card,4);
                }
                await target.damage(game,card,3);

            },PileFlag.both | PileFlag.field)
        };
    }

    /// <summary>
    /// 五谷丰穰
    /// 治愈5点生命值，分配到所有友方角色身上。
    /// </summary>
    public class BumperCrop : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            for (int i = 0; i < 5; i++)
            {
                await game.getAllAllys(card.getOwner()).Where(e => e.isDamage(game)).random(game).heal(game, card, 1);
            }
        }
    }

    /// <summary>
    /// 客星之奇迹
    /// 信仰对决（随从牌） 对所有敌方随从造成2点伤害；获胜时再抽一张牌。
    /// </summary>
    public class GuestStarsMiracle : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { KEYWORD._RevealDuel };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            THHPlayer you = card.getOwner();
            var duel = await Sanae.revealDuel(you, game);
            await game.getOpponent(you).field.damage(game, card, 2);
            if (duel.result)
            {
                await you.draw(game);
            }

        }

    }

    /// <summary>
    /// 开海之奇迹
    /// 信仰对决（随从牌）对目标随从相邻的随从造成4点伤害；获胜时再对目标随从造成2点伤害。
    /// </summary>
    public class OpenSeaMiracle : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { KEYWORD._RevealDuel };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                THHPlayer you = card.getOwner();
                var duel = await Sanae.revealDuel(you, game,2);
                await target.getNearbyCards().damage(game,card,you.getSpellDamage(game,4));
                if (duel.result)
                    await target.damage(game,card,you.getSpellDamage(game, 2));
            },PileFlag.both | PileFlag.field)
        };
    }

    /// <summary>
    /// 飞蛇
    /// 突袭， 每当该随从受到治疗时，获得+1攻击力。
    /// </summary>
    public class FlyingSnake : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 3;
        public override string[] keywords { get; set; } = new string[] { KEYWORD._RevealDuel, KEYWORD.RUSH };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<THHCard.HealEventArg>(PILE.FIELD, (game, card, arg) =>
            {
                return arg != null && arg.cards.Any(e => e == card);
            },null,async(game,card,arg)=>
            {
                await card.addBuff(game,new GeneratedBuff(ID,new AttackModifier(1)));
            })
        };
    }


    /// <summary>
    /// 神明招来
    /// 从牌库中随机召唤一张神明牌，并使其获得突袭.回合结束时，他会返回牌库。
    /// </summary>
    public class GodSummon : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            THHPlayer you = card.getOwner();
            Card god = you.deck.Where(c => c.hasTag(game, TAG.GOD)).random(game);
            if (god != null)
            {
                await you.tryEnterField(game, you.deck, god, you.field.count);
                await god.addKeywords(game, KEYWORD.RUSH);
                await god.addBuff(game, new GeneratedBuff(ID, new THHEffectAfter<THHGame.TurnEndEventArg>(PILE.FIELD, (g, c, a) =>
                {
                    return a.player == c.getOwner();
                }, null, async (g, c, a) =>
                {
                    int index = game.randomInt(0, c.getOwner().deck.count);
                    await god.resetCard(game);
                    await c.getOwner().field.moveTo(game, god, c.getOwner().deck, index);
                })));
            }

        }
    }

    ///// <summary>
    ///// 神明招来 盾
    ///// 召唤手牌中的一张随机神明随从牌的复制，并使其获得圣盾和嘲讽。
    ///// </summary>
    //public class GodSummon_Shield : SpellCardDefine
    //{
    //    public const int ID = Sanae.ID | CATEGORY.SPELL | 0x009;
    //    public override int id { get; set; } = ID;
    //    public override int cost { get; set; } = 6;
    //    public override IEffect[] effects { get; set; } = new IEffect[]
    //    {
    //        new NoTargetEffect(effect)
    //    };

    //    static async Task effect(THHGame game, Card card)
    //    {
    //        THHPlayer you = card.getOwner();
    //        Card god = you.hand.Where(c => c.hasTag(game, TAG.GOD)).random(game);
    //        if(god != null)
    //        {
    //            var servant = await you.createToken(game, god.define);
    //            await servant.card.addKeywords(game, KEYWORD.SHIELD,KEYWORD.TAUNT);
    //            //await servant.card.addBuff(game, new GeneratedBuff(ID, new KeywordsModifier(KeywordsModifier.ModifyType.add, KEYWORD.SHIELD), new KeywordsModifier(KeywordsModifier.ModifyType.add, KEYWORD.TAUNT)));
    //        }

    //    }
    //}

    /// <summary>
    /// 被操纵的巨蛙
    /// 嘲讽，亡语：本局对战你每揭示过一次，对所有随从造成1点伤害。
    /// 我真不知道这东西该如何起名。。。。。。。
    /// </summary>

    public class ManipulatedGaintFrog : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 5;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT, KEYWORD._DeathRattle };
        public override string[] tags { get; set; } = { TAG.BEAST };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID, async(game, card,p) =>
            {
                int count = game.triggers.getRecordedEvents().
                skipUntilLast(e=> e is THHGame.StartEventArg GameStart).
                OfType<Sanae.RevealDuelArg>().
                Where(e2=>e2.player == card.getOwner()).Count();

                await game.getAllServants().damage(game,card,count);
                await game.updateDeath(game);

            })
        };
    }

    /// <summary>
    /// 妖怪信众
    /// 信仰对决(法术牌) 获胜时抽一张牌
    /// </summary>

    //public class YoukaiBelievers : ServantCardDefine
    //{
    //    public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x011;
    //    public override int id { get; set; } = ID;
    //    public override int cost { get; set; } = 5;
    //    public override int attack { get; set; } = 4;
    //    public override int life { get; set; } = 6;
    //    //public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };
    //    //public override string[] tags { get; set; } = { TAG.BEAST };
    //    public override IEffect[] effects { get; set; } = new IEffect[]
    //    {
    //        new NoTargetEffect(effect)
    //    };

    //    static async Task effect(THHGame game, Card card)
    //    {
    //        THHPlayer you = card.getOwner();
    //        var duel = await Sanae.revealDuel(you, game,1);
    //        if (duel.result)
    //        {
    //            await you.draw(game);
    //        }

    //    }
    //}

    /// <summary>
    /// 神代大蛇
    /// 
    /// </summary>

    public class OrochiOfAncientTimes : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 9;
        public override int attack { get; set; } = 6;
        public override int life { get; set; } = 7;
        //public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };
        public override string[] tags { get; set; } = { TAG.BEAST };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
             new AddBuffAfter<Sanae.RevealDuelArg>(PILE.HAND,(g,c,a)=>
             {
                return a.player == c.getOwner() && !c.containBuff(ID);
             },new GeneratedBuff(ID,new CostModifier(-4),
                new RemoveBuffBefore<THHGame.TurnEndEventArg>(PILE.HAND,ID))),
             new LambdaSingleTargetEffect(async(g,c,t)=>
                                         {
                                             await t.setDead(g,true);
                                             await g.updateDeath(g);
                                         },PileFlag.oppo | PileFlag.field)

        };


    }


    /// <summary>
    /// 八坂神奈子
    /// 唯一
    /// 战吼：选择一种神迹并释放。
    /// </summary>

    public class YasakaKanako : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };
        public override string[] tags { get; set; } = { TAG.GOD };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
             new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            List<Card> options = new List<Card> { game.createCard<GodMiracleOfRain>(), game.createCard<GodMiracleOfOnbashira>(), game.createCard<GodMiracleOfWar>(), game.createCard<GodMiracleOfRevolution>() };
            Card choice = await card.getOwner().discover(game, options, 4, "见证神迹！");
            int count = game.triggers.getRecordedEvents().
                skipUntilLast(e => e is THHGame.StartEventArg GameStart).
                OfType<Sanae.RevealDuelArg>().
                Where(e2 => e2.player == card.getOwner()).Count();

            if (choice.define == game.getDefine<GodMiracleOfRain>())
            {
                await game.getOpponent(card.getOwner()).field.damage(game, card, 3 + count);
            }
            else if (choice.define == game.getDefine<GodMiracleOfOnbashira>())
            {
                for (int i = 0; i < count + 1; i++)
                {
                    await card.getOwner().createToken(game, game.getDefine<Onbashira>());
                }
            }
            else if (choice.define == game.getDefine<GodMiracleOfWar>())
            {
                await game.getOpponent(card.getOwner()).master.damage(game, card, 6 + count);
            }
            else
            {
                for (int i = 0; i < count + 2; i++)
                {
                    await card.getOwner().draw(game);
                    if (card.getOwner().hand.isFull)
                    {
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 风雨之神迹
    /// 假卡，对所有敌方随从造成3点伤害。
    /// </summary>
    public class GodMiracleOfRain : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {

        };
    }

    /// <summary>
    /// 御柱之神迹
    /// 假卡，召唤2个3/3并具有嘲讽的御柱。
    /// </summary>
    public class GodMiracleOfOnbashira : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {

        };
    }

    /// <summary>
    /// 战争之神迹
    /// 假卡，对敌方英雄造成6点伤害。
    /// </summary>
    public class GodMiracleOfWar : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {

        };
    }

    /// <summary>
    /// 革新之神迹
    /// 假卡，抽2张牌。
    /// </summary>
    public class GodMiracleOfRevolution : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {

        };
    }

    /// <summary>
    /// 御柱
    /// 衍生卡，3/3/3嘲讽
    /// </summary>
    public class Onbashira : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x018;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 4;
        public override bool isToken { get; set; } = true;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };
    }

    /// <summary>
    /// 泄矢诹访子
    /// 唯一，突袭
    /// 位于手牌中时，本局对战你每揭示过一次法力值消耗减少（1）点。
    /// 亡语：随机释放一场灾祸。
    /// </summary>

    public class MoriyaSuwako : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x019;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE, KEYWORD.RUSH, KEYWORD._DeathRattle };
        public override string[] tags { get; set; } = { TAG.GOD };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            //new THHEffectAfter<Sanae.RevealDuelArg>(PILE.HAND,
            //    (game, card, arg) =>
            //    {
            //        return arg.player == card.getOwner();
            //    },null,async(game,card,arg)=>
            //    {
            //        await card.addBuff(game,new GeneratedBuff(ID,new CostModifier(-1)));
            //    }),
            new DeathRattle(ID, async (game, card, p) =>
            {
                int index = game.randomInt(1,4);
                THHPlayer you = card.getOwner();
                THHPlayer oppo = game.getOpponent(you);
                switch (index)
                {
                    case 1:
                        await card.getOwner().addCardToHand<PlagueOfMishaguchi>(game);
                        break;
                    case 2:
                        await card.getOwner().addCardToHand<PlagueOfFrogs>(game);
                        break;
                    case 3:
                        await card.getOwner().addCardToHand<PlagueOfCrops>(game);
                        break;
                    default:
                        await card.getOwner().addCardToHand<PlagueOfHibernation>(game);
                        break;
                }
            }),
            new IntPropFixer<Sanae.RevealDuelArg>(PileFlag.hand,new IntPropFixer<Sanae.RevealDuelArg>.ModifierFixer(new CostModifier(0),(g,c,b,a)=>
            {
                int v = g.triggers.getRecordedEvents().
                        skipUntilLast(e=> e is THHGame.StartEventArg GameStart).
                        OfType<Sanae.RevealDuelArg>().
                        Where(e2=>e2.player == c.getOwner()).Count();
                g.logger.log(c+"的花费=>"+v);
                return -v;
            }))

        };


    }

    /// <summary>
    /// 青蛙
    /// 衍生卡，2/2/2嘲讽
    /// </summary>
    public class Frog : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x020;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };
    }

    /// <summary>
    /// 赤口之灾祸
    /// 
    /// </summary>
    public class PlagueOfMishaguchi : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x021;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async(game,card,target)=>
            {
                await target.setDead(game,true);
                await card.getOwner().master.damage(game,card,card.getOwner().getSpellDamage(game,4));
                await game.updateDeath(game);
            })
            
        };
    }

    /// <summary>
    /// 青蛙之灾祸
    /// 
    /// </summary>
    public class PlagueOfFrogs : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x022;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            //
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().createToken(game, game.getOpponent(card.getOwner()).field.count, game.getDefine<Frog>());
        }
    }

    /// <summary>
    /// 荒芜之灾祸
    /// 
    /// </summary>
    public class PlagueOfCrops : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x023;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            //game.getOpponent(card.getOwner()).hand
            foreach(Card c in game.getOpponent(card.getOwner()).hand)
            {
                if(c.isServant())
                    await c.addBuff(game, new GeneratedBuff(ID, new CostModifier(5)));
            }
        }
    }

    /// <summary>
    /// 休眠之灾祸
    /// 
    /// </summary>
    public class PlagueOfHibernation : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x024;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            if(card.getOwner().grave.Any(c=>c.define == game.getDefine<MoriyaSuwako>()))
            {
                Card card1 = card.getOwner().grave.Where(c => c.define == game.getDefine<MoriyaSuwako>()).random(game);
                await card.getOwner().grave.moveTo(game, card1, card.getOwner().hand);
                await card1.addBuff(game, new GeneratedBuff(ID, new AttackModifier(2), new LifeModifier(2)));
            }
        }
    }

    public class GodWind : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x025;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        // public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            //new THHEffectAfter<Sanae.RevealDuelArg>(PILE.HAND,
            //    (game, card, arg) =>
            //    {
            //        return arg.player == card.getOwner();
            //    },null,async(game,card,arg)=>
            //    {
            //        await card.addBuff(game,new GeneratedBuff(ID,new CostModifier(-1)));
            //    }),
            new NoTargetEffect(effect),
            new IntPropFixer<Sanae.RevealDuelArg>(PileFlag.hand,new IntPropFixer<Sanae.RevealDuelArg>.ModifierFixer(new CostModifier(0),(g,c,b,a)=>
            {
                int v = g.triggers.getRecordedEvents().
                        skipUntilLast(e=> e is THHGame.StartEventArg GameStart).
                        OfType<Sanae.RevealDuelArg>().
                        Where(e2=>e2.player == c.getOwner()).Count();
                g.logger.log(c+"的花费=>"+v);
                return -v;
            }))
        };
        static async Task effect(THHGame game, Card card)
        {
            THHPlayer you = card.getOwner();
            THHPlayer oppo = game.getOpponent(you);
            int index = 0;

            while (you.field.count != 0)
            {
                index = game.randomInt(0, oppo.deck.count);
                await you.field[0].resetCard(game);
                await you.field.moveTo(game, you.field[0], oppo.deck, index);
            }

            while (oppo.field.count != 0)
            {
                index = game.randomInt(0, oppo.deck.count);
                await oppo.field[0].resetCard(game);
                await oppo.field.moveTo(game, oppo.field[0], oppo.deck, index);
            }

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class YoukaiPowerSpoiler : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x026;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        //public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async(g,c,t)=>
            {
                await t.silence(g);
                await c.getOwner().master.heal(g,c,t.define.getCost());
            },PileFlag.both | PileFlag.field)
        };
    }

    //27
    /// <summary>
    /// 
    /// </summary>
    public class LittleShrine : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x027;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 2;

        //public override string[] tags { get; set; } = { TAG.BEAST };
        // public override string[] keywords { get; set; } = new string[] { KEYWORD._RevealDuel };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<THHGame.TurnEndEventArg>(PILE.FIELD, (g, c, a) =>
                                                                    {
                                                                       return a.player == c.getOwner();
                                                                    }, null,
                                                                    async (g, c, a) =>
                                                                    {
                                                                        var duel = await Sanae.revealDuel(c.getOwner(),g);
                                                                        if (duel.result)
                                                                        {
                                                                            await c.getOwner().draw(g);
                                                                        }
                                                                    })
        };
    }

    public class ForgottenRecrption : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x028;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(async(game,card)=>
            {
                Card c = card.getOwner().grave.Where(e => e.define.hasTag(TAG.GOD)).random(game);
                await card.getOwner().grave.moveTo(game,c,card.getOwner().hand);
            })
        };
    }


    public class OmikujiBomb : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x029;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().addRandomCardToHand(game, game.getDefines(new int[] { Bomb1.ID, Bomb2.ID, Bomb3.ID, Bomb4.ID }));
        }
    }

    public class Bomb1 : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x030;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async(g, c, t) =>
            {
                await t.damage(g,c,c.getOwner().getSpellDamage(g,6));
            },PileFlag.both | PileFlag.field)
        };
    }

    public class Bomb2 : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x031;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async(g, c, t) =>
            {
                await t.damage(g,c,c.getOwner().getSpellDamage(g,4));
            },PileFlag.both | PileFlag.field)
        };

    }

    public class Bomb3 : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x032;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            await game.getOpponent(card.getOwner()).field.random(game).damage(game, card, card.getOwner().getSpellDamage(game, 4));
        }
    }

    public class Bomb4 : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x033;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            await game.getAllServants().damageByRandom(game, card, 6);
        }
    }
}