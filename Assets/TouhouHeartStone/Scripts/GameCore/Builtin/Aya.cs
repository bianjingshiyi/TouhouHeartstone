using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;



namespace TouhouHeartstone.Builtin
{
    [IgnoreCardDefine]
    public class Aya : MasterCardDefine
    {
        public const int ID = 0x00a00000 | CATEGORY.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = SpareFilm.ID;//英雄技能的ID
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    [IgnoreCardDefine]
    /// <summary>
    /// 技能：备用胶卷
    /// 下一张牌灵力值消耗减少1点
    /// </summary>
    public class SpareFilm : SkillCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1; //技能花费
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().master.addBuff(game, new GeneratedBuff(ID,
                new Halo(new GeneratedBuff(ID, new CostModifier(-1)), PileFlag.self | PileFlag.hand, PileFlag.self | PileFlag.master, (g1, c1) =>
                {
                    return !c1.isSkill();
                }),
                new RemoveBuffBefore<THHPlayer.UseEventArg>(PILE.MASTER, (g, c, a) =>
                {
                    return a.player == card.getOwner() && !a.card.isSkill();
                }, ID)));
        }
    }

    [IgnoreCardDefine]
    /// <summary>
    /// 乌鸦仆从
    /// 1/2/1,亡语：随机一张手牌灵力值消耗减少(1)点
    /// </summary>
    public class Crow : ServantCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SERVANT | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;//随从的攻击力
        public override int life { get; set; } = 1;//随从的血量上限
        public override bool isToken { get; set; } = false;//是衍生物吗？
        public override string[] tags { get; set; } = new string[] { TAG.BEAST };//种族标签
        //public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };//关键词标签：
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID,
                async(g,c,p)=>
                {
                    await c.getOwner().hand.random(g).addBuff(g,new GeneratedBuff(ID, new CostModifier(-1)));
                })
        };
    }

    [IgnoreCardDefine]
    /// <summary>
    /// 报纸派送
    /// 1费，造成2点伤害，本回合能使用多次，但不能对同一目标使用。
    /// </summary>
    public class NewspaperDistribution : SpellCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SPELL | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1; //花费
        public override string[] keywords { get; set; } = new string[] { KEYWORD._RETURNHEAD };//关键词标签：
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.damage(game,card,card.getOwner().getSpellDamage(game,2));
                await card.addBuff(game,new GeneratedBuff(ID,
                    new THHEffectAfter<THHGame.TurnEndEventArg>(PILE.HAND,
                                                                (g,c,a)=>
                                                                {
                                                                    return a.player == c.getOwner();
                                                                },null,
                                                                async(g,c,a)=>
                                                                {
                                                                    THHPlayer you = c.getOwner();
                                                                    await you.hand.moveTo(g,c,you.grave);
                                                                })));
            }, PileFlag.both | PileFlag.character,null,
                (game,card,target)=>
                {
                    if(!game.triggers.getRecordedEvents().
                        skipUntilLast(e=> e is THHGame.TurnStartEventArg turnStart)
                       .OfType<THHPlayer.UseEventArg>()
                       .Any(u => u.player == card.getOwner() &&
                                 u.card == card &&
                                 u.targets.Any(t => t==target)))
                    {
                        return true;
                    }else
                    {
                        return false;
                    }
                })
        };

    }

    [IgnoreCardDefine]
    /// <summary>
    /// 独家新闻
    /// 下一张牌灵力值消耗减少2点
    /// </summary>
    public class Scoop : SpellCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SPELL | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().master.addBuff(game, new GeneratedBuff(ID,
                new Halo(new GeneratedBuff(ID, new CostModifier(-2)), PileFlag.self | PileFlag.hand, PileFlag.self | PileFlag.master, (g1, c1) =>
                {
                    return !c1.isSkill();
                }),
                new RemoveBuffBefore<THHPlayer.UseEventArg>(PILE.MASTER, (g, c, a) =>
                {
                    return a.player == card.getOwner() && !a.card.isSkill();
                }, ID)));
        }
    }

    [IgnoreCardDefine]
    public class LittleAya : ServantCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SERVANT | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;//随从的攻击力
        public override int life { get; set; } = 3;//随从的血量上限
        public override bool isToken { get; set; } = false;//是衍生物吗？
        public override string[] tags { get; set; } = new string[] { TAG.BEAST };//种族标签
        //public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };//关键词标签：
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHPlayer.UseEventArg>(PILE.FIELD, (g, c, a) =>
                                                                    {
                                                                        return c.getOwner() == a.player && a.card.isSkill();
                                                                    },null,
                                                                    async(g,c,a)=>
                                                                    {
                                                                        a.action = e1 =>
                                                                        {
                                                                            return c.getOwner().hand.random(g).addBuff(g,new GeneratedBuff(ID,new CostModifier(-1)));
                                                                        };
                                                                    })
        };


    }

    [IgnoreCardDefine]
    /// <summary>
    /// 文文。新闻
    /// 抽一张牌，如果你在本回合使用这张牌，重复此效果。
    /// </summary>
    public class BunbunmaruNews : SpellCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SPELL | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().draw(game, out var draw);
            Card drawCard = draw.card;
            await card.getOwner().master.addBuff(game, new GeneratedBuff(ID, new THHEffectAfter<THHPlayer.UseEventArg>(PILE.MASTER, (g, c, a) =>
                                                                                                                                    {
                                                                                                                                        return a.card == drawCard;
                                                                                                                                    }, null,
                                                                                                                                   async (g, c, a) =>
                                                                                                                                   {
                                                                                                                                       await effect(g, c);
                                                                                                                                   }),
                                                                                                                                   new RemoveBuffAfter<THHPlayer.UseEventArg>(PILE.MASTER,
                                                                                                                                   (g, c, a) =>
                                                                                                                                   {
                                                                                                                                       return a.card == drawCard;
                                                                                                                                   }, ID),
                                                                                                                                   new RemoveBuffAfter<THHGame.TurnEndEventArg>(PILE.MASTER, ID)));
        }
    }

    [IgnoreCardDefine]
    /// <summary>
    /// 枫扇风
    /// 对三个随机敌方随从造成伤害，数值等同于本回合使用过的牌数量。
    /// </summary>
    public class MapleFansWind : SpellCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SPELL | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            int dam = game.triggers.getRecordedEvents().skipUntilLast(e => e is THHGame.TurnStartEventArg TurnStart).
                        OfType<THHPlayer.UseEventArg>().
                        Where(e2 => e2.player == card.getOwner() && !e2.card.isSkill()).Count() + 1;

            await game.getOpponent(card.getOwner()).field.damageByRandomCount(game, card, card.getOwner().getSpellDamage(game, dam), 3);
        }
    }

    [IgnoreCardDefine]
    /// <summary>
    /// 风神一扇
    /// 造成2点伤害，如果你本回合使用过灵力值与其原始灵力值消耗不同的牌，改为造成4点伤害。
    /// </summary>
    public class WindGodFan : SpellCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                bool x = game.triggers.getRecordedEvents().skipUntilLast(e => e is THHGame.TurnStartEventArg TurnStart).
                        OfType<THHPlayer.UseEventArg>().Any( u => u.player == card.getOwner() &&
                                                                    u.cost != u.card.define.getCost() &&
                                                                    u.card != card);
                if (x)
                {
                    await target.damage(game,card,card.getOwner().getSpellDamage(game, 4));
                }
                else
                {
                    await target.damage(game,card,card.getOwner().getSpellDamage(game, 2));
                }
            },PileFlag.both | PileFlag.character)
        };
    }

    [IgnoreCardDefine]
    /// <summary>
    /// 相机
    /// 3/0/4,每当对手抽一张牌，你获得这张牌的原始复制
    /// </summary>
    public class ReporterCamera : ItemCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.ITEM | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };//关键词标签：
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<THHPlayer.DrawEventArg>(PILE.ITEM,(g,c,a)=>
            {
                return a.player == g.getOpponent(c.getOwner());
            },null,async(g,c,a)=>
            {
                await c.getOwner().hand.add(g,g.createCard(a.card.define));
                await c.damage(g,c,1);
            })
        };
    }

    [IgnoreCardDefine]
    /// <summary>
    /// 09 果
    /// 抽牌，数量等同于这张卡的原始灵力值消耗与你使用这张卡时实际灵力值消耗之差。
    /// </summary>
    public class HimekaidouHatate : ServantCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SERVANT | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;//随从的攻击力
        public override int life { get; set; } = 3;//随从的血量上限
        public override bool isToken { get; set; } = false;//是衍生物吗？
        //public override string[] tags { get; set; } = new string[] { TAG.BEAST };//种族标签
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };//关键词标签：
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            int CurrentCost = game.triggers.getRecordedEvents().skipUntilLast(e => e is THHGame.TurnStartEventArg TurnStart).
                                OfType<THHPlayer.UseEventArg>().Last(u => u.player == card.getOwner()).cost;
            int baseCost = card.define.getCost();

            for (int i = 0; i < Math.Abs(CurrentCost - baseCost); i++)
            {
                await card.getOwner().draw(game);
            }
        }


    }

    [IgnoreCardDefine]
    /// <summary>
    /// 10 文花帖
    /// 每当你攻击时，使随机一张手牌灵力值消耗减少1点
    /// </summary>
    public class Bunkachou : ItemCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.ITEM | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        //public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };//关键词标签：
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<THHCard.AttackEventArg>(PILE.MASTER,(g,c,a)=>
            {
                return a.card == c.getOwner().master;
            },null,async(g,c,a)=>
            {
                await c.getOwner().hand.random(g).addBuff(g ,new GeneratedBuff(ID,new CostModifier(-1)));
            })
        };
    }

    [IgnoreCardDefine]
    /// <summary>
    /// 11 风神少女
    /// 抽一张牌，你本回合每使用一张牌，它的灵力值消耗就减少一点。
    /// </summary>
    public class WindGodGirl : SpellCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SPELL | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            int cost = game.triggers.getRecordedEvents().skipUntilLast(e => e is THHGame.TurnStartEventArg TurnStart).
                        OfType<THHPlayer.UseEventArg>().
                        Where(e2 => e2.player == card.getOwner() && !e2.card.isSkill()).Count() + 1;

            await card.getOwner().draw(game, out var draw);
            Card drawCard = draw.card;

            await card.addBuff(game, new GeneratedBuff(ID, new CostModifier(-cost)));
        }
    }

    [IgnoreCardDefine]
    /// <summary>
    /// 12
    /// </summary>
    public class LeafHiding : SpellCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SPELL | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            IEnumerable<Card> cards = game.createCards(game.getDefines().Where(e => (e.id >> 5) == (Aya.ID >> 5)).ToArray()).Where(q => q.isSpell());

            await card.getOwner().discover(game, cards);

            bool x = game.triggers.getRecordedEvents().skipUntilLast(e => e is THHGame.TurnStartEventArg TurnStart).
                        OfType<THHPlayer.UseEventArg>().
                        Any(e2 => e2.player == card.getOwner() && !e2.card.isSkill());
            if (x)
            {
                cards = game.createCards(game.getDefines().Where(e => (e.id >> 5) == (Aya.ID >> 5)).ToArray()).Where(q => q.isSpell());
                await card.getOwner().discover(game, cards);
            }
        }
    }

    /// <summary>
    /// 白木
    /// 使用技能后，使手牌随机三张牌灵力消耗减一
    /// 就真想印大帝？
    /// </summary>
    public class Aya13 : ServantCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SERVANT | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 4;//随从的攻击力
        public override int life { get; set; } = 4;//随从的血量上限
        public override bool isToken { get; set; } = false;//是衍生物吗？
        //public override string[] tags { get; set; } = new string[] { TAG.BEAST };//种族标签
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };//关键词标签：
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
             new THHEffectAfter<THHPlayer.UseEventArg>(PILE.FIELD,
              (g,c,a)=>{
                            return a.player == c.getOwner() && a.card.isSkill();
                       }, null, async(g, c, a) =>
                       {
                           var cards = c.getOwner().hand.randomTake(g,3);
                           foreach(var ca in cards)
                           {
                               await ca.addBuff(g,new GeneratedBuff(ID,new CostModifier(-1)));
                           }
                       }
                       )
        };

    }

    /// <summary>
    /// 鸦符【暗夜昼魔】
    /// </summary>
    public class Aya14 : SpellCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SPELL | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {

                for(int i = 0; i<3; i++)
                {
                    var createToken = await card.getOwner().createToken(game, game.getDefine<Crow>(), card.pile.indexOf(card) + 1);
                    var c = createToken.card;
                    if (!target.isDead(game))
                    {
                        await c.forceAttack(game,card.getOwner(),target);
                        await game.updateDeath(game);
                    }

                }

            })
        };
    }

    /// <summary>
    /// 暴力取材
    /// </summary>
    public class Aya15 : SpellCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SPELL | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().master.damage(game, card, 4);
            await game.getOpponent(card.getOwner()).field.random(game).damage(game, card, 4);
            await card.getOwner().draw(game);
            await card.getOwner().draw(game);
            await card.getOwner().draw(game);
        }
    }

    /// <summary>
    /// 椛
    /// </summary>
    public class Aya16 : ServantCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SERVANT | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        //public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };//关键词标签：
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<THHPlayer.UseEventArg>(PILE.FIELD, (g, c, a) =>
            {
                return a.player == g.getOpponent(c.getOwner()) && a.card.isServant();
            },null,async(g,c,a)=>
            {
                await c.forceAttack(g,c.getOwner(),a.card);
                await c.getOwner().createToken(g, g.getDefine<Aya17>(), c.pile.indexOf(c) + 1);
            })
        };
    }

    /// <summary>
    /// 直属卫兵
    /// </summary>
    public class Aya17 : ServantCardDefine
    {
        public const int ID = Aya.ID | CATEGORY.SERVANT | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;//随从的攻击力
        public override int life { get; set; } = 2;//随从的血量上限
        public override bool isToken { get; set; } = true;//是衍生物吗？
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };//关键词标签：
    }
}
