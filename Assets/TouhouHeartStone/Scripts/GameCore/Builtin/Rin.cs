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
        public const int ID = 0x00700000 | CATEGORY.MASTER | 0x000;
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
            await card.getOwner().master.gainArmor(game, 1);
            await card.getOwner().master.addBuff(game,
                    new GeneratedBuff(ID, new AttackModifier(1),
                    new RemoveBuffAfter<THHGame.TurnEndEventArg>(PILE.MASTER, ID)));
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
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new Halo(new GeneratedBuff(ID, new KeywordsModifier(0,KEYWORD.CHARGE)),
                 PileFlag.self | PileFlag.field,
                 PileFlag.self | PileFlag.field,
                 (g1, c1) =>
                 {
                     return true;
                 })
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
                await g1.getOpponent(c1.getOwner()).master.damage(g1,c1,3);
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
            foreach (Card c1 in card.getOwner().field)
            {
                await c1.addBuff(game, new GeneratedBuff(ID, new DeathRattle(ID, async (g, c, b, p) =>
                {
                    await g.getOpponent(c.getOwner()).master.damage(g, c, 1);
                    await g.updateDeath(game);
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
            int count = card.getOwner().field.count;
            for (int i = 0; i < count; i++)
            {
                Card c = card.getOwner().field[0];
                await c.setDead(game, true);
                await game.updateDeath(game);
                await c.getOwner().reborn(game, c, c.getOwner().field, count);
            }

        }
    }

    /// <summary>
    /// 尸体旅行
    /// </summary>
    public class CorpseVoyage : SpellCardDefine
    {
        public const int ID = Rin.ID | CATEGORY.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            for (int i = 0; i < 3; i++)
            {
                await card.getOwner().reborn(game, card.getOwner().grave.Where(c => c.getCost(game) <= 3 && c.isServant()).random(game), card.getOwner().field);
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
        public override int attack { get; set; } = 3;
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
                await game.updateDeath(game);
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
                await c1.getOwner().draw(g1,c1.getOwner().deck.Where(c => c.hasDeathRattle(g1)).random(g1));
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
                return c1.getOwner().master.addBuff(g1,
                    new GeneratedBuff(ID, new AttackModifier(3),
                        new THHEffectAfter<THHCard.AttackEventArg>(PileFlag.master | PileFlag.self,(g2,c2,b1,a1)=>
                        {
                            return a1.card == c2.getOwner().master;
                        },(g2,c2,b1,a1)=>
                        {
                            string propName = nameof(CatWalk)+b1.instanceID+"Effected";
                            Card master = c2.getOwner().master;
                            if(a1.target.isDead(g2) && !master.getProp<bool>(g2,propName))
                            {
                                master.setProp(g2,propName,true);
                                return master.setAttackTimes(g2,master.getAttackTimes(g2)-1);
                            }
                            return Task.CompletedTask;
                        }),
                        new RemoveBuffAfter<THHGame.TurnEndEventArg>(PILE.MASTER, ID)));
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
            Card choice = await card.getOwner().discover(game, card.getOwner().grave.Where(c=>c.isServant() && c.hasDeathRattle(game)), 3);
            if (!card.getOwner().hand.isFull)
                await card.getOwner().grave.moveTo(game, choice, card.getOwner().hand);
            await choice.triggerDeathRattles(game);
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
                         return a.card == c.getOwner().master;
                    }, null, async (g, c, a) =>
                    {
                         await c.getOwner().draw(g,c.getOwner().deck.Where(c1=>c1.isServant()).random(g));
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
                    if(c.getOwner().hand.count<c.getOwner().hand.maxCount)
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
                var reborn = await card.getOwner().reborn(game, card.getOwner().grave.Where(c => c.hasKeyword(game, KEYWORD._DeathRattle) && c.isServant()).random(game), card.getOwner().field);
                if (reborn != null)
                {
                    await reborn.card.addBuff(game, new GeneratedBuff(ID, new THHEffectAfter<THHGame.TurnStartEventArg>(PILE.FIELD, (g, c, a) =>
                    {
                        return a.player == c.getOwner();
                    }, null, async (g, c, a) =>
                    {
                        await c.setDead(g, true);
                        await g.updateDeath(g);
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
                await card.getOwner().deck.moveTo(game, card.getOwner().deck.Where(c => c.isServant()).random(game), card.getOwner().grave);
            };
            await card.getOwner().master.addBuff(game,
                    new GeneratedBuff(ID, new AttackModifier(3),
                    new RemoveBuffAfter<THHGame.TurnEndEventArg>(PILE.MASTER, ID)));
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
                await game.getOpponent(card.getOwner()).grave.moveTo(game, game.getOpponent(card.getOwner()).grave.top, card.getOwner().grave);
            }

        }
    }
}