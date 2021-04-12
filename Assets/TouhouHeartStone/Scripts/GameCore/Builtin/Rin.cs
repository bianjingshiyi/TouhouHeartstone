using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone.Builtin
{
    public class Rin : MasterCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_RIN | CATEGORY.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = Nekomatarize.ID;
    }

    /// <summary>
    /// 技能_猫又形态
    /// </summary>
    public class Nekomatarize : SkillCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().getMaster().gainArmor(game, 1);
            await card.getOwner().getMaster().addBuff(game,
                    new GeneratedBuff(ID, new AttackModifier(1),
                    new RemoveBuffAfter<HearthStoneRule.TurnEndEventArg>(PILE.MASTER, ID)));
        }
    }

    /// <summary>
    /// 丧尸妖精
    /// </summary>
    public class ZombieFairy : ServantCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SERVANT | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { KEYWORD._DeathRattle };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID,async(g1,c1,p)=>
            {
                await c1.getOwner().createToken(g1,g1.getDefine<GhostOfHatred>());
            })
        };
    }

    /// <summary>
    /// 怨灵,衍生物
    /// </summary>
    public class GhostOfHatred : ServantCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SERVANT | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[0];
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {

        };
    }

    /// <summary>
    /// 烈焰车轮
    /// </summary>
    public class BlazingWheel : ServantCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SERVANT | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 5;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new Halo(new GeneratedBuff(ID, new KeywordsModifier(0,KEYWORD.CHARGE)),
                 PileFlag.self | PileFlag.field)
        };
    }

    /// <summary>
    /// 磷火
    /// </summary>
    public class GhostFire : ServantCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SERVANT | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { KEYWORD._DeathRattle };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID,async(g1,c1,p)=>
            {
                await g1.getOpponent(c1.getOwner()).getMaster().damage(g1,c1,2);
                await g1.updateDeath();
            })
        };
    }

    /// <summary>
    /// 旧地狱的针山
    /// </summary>
    public class NeedleMountainOfFormerHell : SpellCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SPELL | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            foreach (Card c1 in card.getOwner().getField())
            {
                await c1.addBuff(game, new GeneratedBuff(ID, new DeathRattle(ID, async (g, c, b, p) =>
                {
                    await g.getOpponent(c.getOwner()).getMaster().damage(g, c, 1);
                    await g.updateDeath();
                })));
            }
        }
    }

    /// <summary>
    /// 死灰复燃
    /// </summary>
    public class RekindlingOfDeadAshes : SpellCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SPELL | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            Card[] field = card.getOwner().getField().ToArray();
            await field.setDead(game, true);
            await game.updateDeath();
            foreach (var c in field)
            {
                await card.getOwner().reborn(game, c, c.getOwner().getField());
            }
            // int count = card.getOwner().field.count;
            // for (int i = 0; i < count; i++)
            // {
            //     Card c = card.getOwner().field[0];
            //     await c.setDead(game, true);
            //     await game.updateDeath();
            //     await c.getOwner().reborn(game, c, c.getOwner().field, count);
            // }

        }
    }

    /// <summary>
    /// 尸体旅行
    /// </summary>
    public class CorpseVoyage : SpellCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            for (int i = 0; i < 3; i++)
            {
                await card.getOwner().reborn(game, card.getOwner().getGrave().Where(c => c.getCost(game) <= 3 && c.isServant()).random(game), card.getOwner().getField());
                //card.getOwner().grave.Where(c => c.getCost(game) <= 3 && c.isServant()).random(game);
            };
        }
    }

    /// <summary>
    /// （不太会）恰人的怨灵
    /// </summary>
    public class VengefulCannibalSpirit : ServantCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SERVANT | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD._DeathRattle };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID,async(g1,c1,p)=>
            {
                await c1.getOwner().createToken(g1,g1.getDefine<GhostOfHatred>());
                await c1.getOwner().createToken(g1,g1.getDefine<GhostOfHatred>());
            }),
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                await target.setDead(game,true);
                await game.updateDeath();
            },PileFlag.self | PileFlag.field)
        };
    }

    /// <summary>
    /// 抬棺者
    /// </summary>
    public class CoffinCarrier : ServantCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SERVANT | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD._DeathRattle, KEYWORD.RUSH };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID,async(g1,c1,p)=>
            {
                await c1.getOwner().draw(g1,c1.getOwner().getDeck().Where(c => c.hasDeathRattle(g1)).random(g1));
            }),
        };
    }

    /// <summary>
    /// 猫步
    /// </summary>
    public class CatWalk : SpellCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SPELL | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect((g1,c1)=>
            {
                return c1.getOwner().getMaster().addBuff(g1,
                    new GeneratedBuff(ID, new AttackModifier(3),
                        new THHEffectAfter<THHCard.AttackEventArg>(PileFlag.master | PileFlag.self,(g2,c2,b1,a1)=>
                        {
                            return a1.card == c2.getOwner().getMaster();
                        },(g2,c2,b1,a1)=>
                        {
                            string propName = nameof(CatWalk)+b1.instanceID+"Effected";
                            Card master = c2.getOwner().getMaster();
                            if(a1.target.isDead(g2) && !master.getProp<bool>(g2,propName))
                            {
                                master.setProp(g2,propName,true);
                                return master.setAttackTimes(g2,master.getAttackTimes(g2)-1);
                            }
                            return Task.CompletedTask;
                        }),
                        new RemoveBuffAfter<HearthStoneRule.TurnEndEventArg>(PILE.MASTER, ID)));
            })
        };
    }

    /// <summary>
    /// 鬼灵小镇==
    /// </summary>
    public class GhostTown : SpellCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SPELL | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            var arg = await card.getOwner().searchPile(game,
                card.getOwner().getGrave().Where(c => c.isServant() && c.hasDeathRattle(game)), 
                true,
                3,
                "尼科之咒！");
            Card choice = arg.choose;
            if (choice != null)
            {
                if (!card.getOwner().getHand().isFull)
                    await card.getOwner().getGrave().moveTo(game, choice, card.getOwner().getHand());
                await choice.triggerDeathRattles(game);
            }
            
        }
    }

    /// <summary>
    /// 不死族的攻城单位
    /// </summary>
    public class CorpseTruck : ServantCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SERVANT | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<THHCard.AttackEventArg>(PILE.FIELD, (g, c, a) =>
                    {
                         return a.card == c.getOwner().getMaster();
                    }, null, async (g, c, a) =>
                    {
                         await c.getOwner().draw(g,c.getOwner().getDeck().Where(c1=>c1.isServant()).random(g));
                    })
        };
    }

    /// <summary>
    /// 脾脏蛀食者
    /// </summary>
    public class SpleenEater : ServantCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SERVANT | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<THHCard.DeathEventArg>(PILE.FIELD,(g,c,a)=>
            {
                return !a.infoDic.Any(c1 => c1.Key == c);
            }, null, async (g, c, a) =>
            {
                foreach(var death in a.infoDic)
                {
                    if(c.getOwner().getHand().count<c.getOwner().getHand().maxCount && death.Key.getOwner() == c.getOwner())
                    {
                        await c.getOwner().addCardToHand<GhostOfHatred>(g);
                    }
                }
            })
        };
    }

    /// <summary>
    /// 尸体繁华街
    /// </summary>
    public class CorpseShoppingDistrict : SpellCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SPELL | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            for (int i = 0; i < 3; i++)
            {
                var reborn = await card.getOwner().reborn(game, card.getOwner().getGrave().Where(c => c.hasDeathRattle(game) && c.isServant()).random(game), card.getOwner().getField());
                if (reborn != null)
                {
                    await reborn.card.addBuff(game, new GeneratedBuff(ID, new THHEffectAfter<HearthStoneRule.TurnStartEventArg>(PILE.FIELD, (g, c, a) =>
                    {
                        return a.player == c.getOwner() && c.pile.name == PILE.FIELD;
                    }, null, async (g, c, a) =>
                    {
                        await c.setDead(g, true);
                        await g.updateDeath();
                    })));
                }
            };
        }
    }

    /// <summary>
    /// 醉猫乱步
    /// </summary>
    public class CatRandomWalk : SpellCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SPELL | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            for (int i = 0; i < 3; i++)
            {
                await card.getOwner().getDeck().moveTo(game, card.getOwner().getDeck().Where(c => c.isServant()).random(game), card.getOwner().getGrave());
            };
            await card.getOwner().getMaster().addBuff(game,
                    new GeneratedBuff(ID, new AttackModifier(3),
                    new RemoveBuffAfter<HearthStoneRule.TurnEndEventArg>(PILE.MASTER, ID)));
        }
    }

    /// <summary>
    /// 地狱祸轮
    /// </summary>
    public class HellTrafficAccident : SpellCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SPELL | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            for (int i = 0; i < 3; i++)
            {
                if(game.getOpponent(card.getOwner()).getGrave().Any(c => c.isServant()))
                {
                    await game.getOpponent(card.getOwner()).getGrave().moveTo(game, game.getOpponent(card.getOwner()).getGrave().Where(c => c.isServant()).Last(), card.getOwner().getGrave());
                }
            }

        }
    }
}