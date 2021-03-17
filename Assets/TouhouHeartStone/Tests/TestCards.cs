using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
// using UnityEditor.UIElements;

namespace Tests
{
    class TestMaster : MasterCardDefine
    {
        public const int ID = 0x0FF00000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = TestSkill.ID;
        public override IEffect[] effects { get; set; } = new LambdaSingleTargetEffect[0];
    }
    class TestSkill : SkillCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new LambdaSingleTargetEffect[0];
    }
    class TestServant : ServantCardDefine
    {
        public const int ID = 0x00110001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new LambdaSingleTargetEffect[0];
    }
    class TestServant_TurnEndEffect : ServantCardDefine
    {
        public const int ID = 0x00110002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHGame.TurnEndEventArg>(PILE.FIELD,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return true;
            },(game,card,arg)=>
            {
                card.setProp(game, "TestResult", true);
                return Task.CompletedTask;
            })
        };
    }
    class TestServant_ZeroAttack : ServantCardDefine
    {
        public const int ID = 0x00110003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
        };
    }
    class TestServant_Reverse : ServantCardDefine
    {
        public const int ID = 0x00110004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PILE.NONE, (game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return true;
            },(game,card,arg,targets)=>
            {
                return Task.CompletedTask;
            })
        };
    }
    class TestServant_Buff : ServantCardDefine
    {
        public const int ID = 0x00110005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PILE.NONE, (game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return true;
            },(game,card,arg,targets)=>
            {
                return card.addBuff(game,new TestBuff());
            })
        };
        class TestBuff : Buff
        {
            public const int ID = 0x001;
            public override int id { get; } = ID;
            public override PropModifier[] modifiers { get; } = new PropModifier[]
            {
                new AttackModifier(1),
                new LifeModifier(1)
            };
            public override IPassiveEffect[] effects { get; }
            public override int instanceID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override Buff clone()
            {
                return new TestBuff();
            }
        }
    }
    /// <summary>
    /// 一个会突袭的随从
    /// </summary>
    public class RushServant : ServantCardDefine
    {
        public const int ID = 0x00110006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[0];
        public override string[] keywords { get; set; } = new string[] { KEYWORD.RUSH };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 一个会圣盾的随从
    /// </summary>
    public class ShieldServant : ServantCardDefine
    {
        public const int ID = 0x00110007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[0];
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SHIELD };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 会潜行的随从
    /// </summary>
    public class StealthServant : ServantCardDefine
    {
        public const int ID = 0x00110008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[0];
        public override string[] keywords { get; set; } = new string[] { KEYWORD.STEALTH };
        public override IEffect[] effects { get; set; } = new IEffect[0];

        //public override IEffect[] effects { get; set; } = new IEffect[]
        //{
        //    new THHEffectBefore<THHGame.TurnEndEventArg>(PileName.FIELD,(game,card,arg)=>
        //    {
        //        return true;
        //    },(game,card,targets)=>
        //    {
        //        return true;
        //    },(game,card,arg)=>
        //    {

        //        game.getPlayerForNextTurn(arg.player).master.damage(game, card, 1);
        //        return Task.CompletedTask;
        //    })
        //};

    }

    /// <summary>
    /// 会吸血的随从
    /// </summary>
    public class DrainServant : ServantCardDefine
    {
        public const int ID = 0x00110009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.DRAIN };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    /// <summary>
    /// 剧毒随从
    /// </summary>
    public class PoisonousServant : ServantCardDefine
    {
        public const int ID = 0x0011000A;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.POISONOUS };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    /// <summary>
    /// 魔免随从
    /// </summary>
    public class ElusiveServant : ServantCardDefine
    {
        public const int ID = 0x0011000B;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.ELUSIVE };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    /// <summary>
    /// 拥有指定单个敌人攻击的技能的Master
    /// </summary>
    public class TestMaster2 : MasterCardDefine
    {
        public const int ID = 0x0011000C;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = TestDamageSkill.ID;
        public override IEffect[] effects { get; set; } = new LambdaSingleTargetEffect[0];
    }
    class TestDamageSkill : SkillCardDefine
    {
        public const int ID = 0x0011000D;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PILE.SKILL,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                if(targets[0] is Card target)
                    return true;
                return false;
            },async (game,card,arg,targets)=>
            {
                if(targets[0] is Card target)
                    await target.damage(game,card,1);
            })
        };

    }

    /// <summary>
    /// 单体指向型攻击的spellcard
    /// </summary>
    public class TestSpellCard : SpellCardDefine
    {
        public const int ID = 0x0011000E;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PILE.NONE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                if(targets[0] is Card target)
                    return true;
                return false;
            },async (game,card,arg,targets)=>
            {
                if(targets[0] is Card target)
                    await target.damage(game, card, arg.player.getSpellDamage(game,1));
            })
        };
    }

    /// <summary>
    /// 一只白板的挨打用随从
    /// </summary>
    public class DefaultServant : ServantCardDefine
    {
        public const int ID = 0x0011FFFF;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 7;
        public override string[] tags { get; set; } = new string[0];
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    public class MountainGaint : ServantCardDefine
    {
        public const int ID = 0x00110010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 12;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new IntPropFixer<Pile.MoveCardEventArg>(PileFlag.hand,new IntPropFixer<Pile.MoveCardEventArg>.ModifierFixer(new CostModifier(0),(g,c,b,a)=>
            {
                int v = -c.getOwner().hand.Where(c1=>c1!=c).Count();
                g.logger.log(c+"的花费=>"+v);
                return v;
            }))
        };
    }
    /// <summary>
    /// 冰环法术
    /// </summary>
    public class TestFreeze : SpellCardDefine
    {
        public const int ID = 0x0011000F;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PILE.NONE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return true;
            },async(game,card,arg,targets)=>
            {
                foreach(Card target in targets)
                {
                    await target.setFreeze(game,true);
                }
            })
        };
    }
    /// <summary>
    /// 巫师学徒
    /// </summary>
    class SorcererApprentice : ServantCardDefine
    {
        public const int ID = 0x00110011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new Halo(new GeneratedBuff(ID, new CostModifier(-1)), PileFlag.self | PileFlag.hand,filter:(game,card)=>
            {
                return card.define is SpellCardDefine;
            })
        };
        //class Halo : PassiveEffect
        //{
        //    public override string[] piles { get; } = new string[] { PileName.FIELD };
        //    Dictionary<Card, Buff> buffDic { get; } = new Dictionary<Card, Buff>();
        //    Trigger<Pile.MoveCardEventArg> onCardEnterHandTrigger { get; set; } = null;
        //    public override Task onEnable(THHGame game, Card card)
        //    {
        //        if (onCardEnterHandTrigger == null)
        //        {
        //            onCardEnterHandTrigger = new Trigger<Pile.MoveCardEventArg>(arg =>
        //            {
        //                if (arg.to == card.getOwner().hand &&
        //                    arg.card.define is SpellCardDefine)
        //                {
        //                    HaloBuff buff = new HaloBuff();
        //                    arg.card.addBuff(game, buff);
        //                    buffDic.Add(arg.card, buff);
        //                }
        //                else if (arg.from == card.getOwner().hand &&
        //                    arg.card.define is SpellCardDefine)
        //                {
        //                    if (buffDic.ContainsKey(arg.card))
        //                        arg.card.removeBuff(game, buffDic[arg.card]);
        //                }
        //                return Task.CompletedTask;
        //            });
        //            game.triggers.registerAfter(onCardEnterHandTrigger);
        //        }
        //        foreach (var target in card.getOwner().hand.Where(c => c.define is SpellCardDefine))
        //        {
        //            HaloBuff buff = new HaloBuff();
        //            target.addBuff(game, buff);
        //            buffDic.Add(target, buff);
        //        }
        //    }
        //    public override void onDisable(THHGame game, Card card)
        //    {
        //        if (onCardEnterHandTrigger != null)
        //        {
        //            game.triggers.removeAfter(onCardEnterHandTrigger);
        //            onCardEnterHandTrigger = null;
        //        }
        //        foreach (var pair in buffDic)
        //        {
        //            pair.Key.removeBuff(game, pair.Value);
        //        }
        //        buffDic.Clear();
        //    }
        //    class HaloBuff : Buff
        //    {
        //        public override int id { get; } = 0;
        //        public override PropModifier[] modifiers { get; } = new PropModifier[]
        //        {
        //            new HaloBuffModifier()
        //        };
        //        class HaloBuffModifier : PropModifier<int>
        //        {
        //            public override string propName { get; } = nameof(ServantCardDefine.cost);
        //            public override int calc(Card card, int value)
        //            {
        //                return value - 1;
        //            }
        //        }
        //        public override Buff clone()
        //        {
        //            return new HaloBuff();
        //        }
        //    }
        //}
    }
    class FireBall : SpellCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SPELL | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((game,card,target)=>
            {
                target.damage(game, card, card.getOwner().getSpellDamage(game, 6));
                return Task.CompletedTask;
            })
        };
    }
    /// <summary>
    /// 剧毒女祭师
    /// </summary>
    public class Priestess : ServantCardDefine
    {
        public const int ID = 0x001100AA;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 6;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.POISONOUS };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHGame.TurnEndEventArg>(PILE.FIELD,(game,card,arg)=>
            {
                if(arg.player!=game.currentPlayer)
                return false;
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async(game,card,arg)=>
            {
                for(int i=0;i<6;i++)
                {
                    foreach (var character in game.getOpponent(card.owner as THHPlayer).field.randomTake(game, 1))
                    {
                        await character.damage(game, card, 1);
                    }
                }
            })
        };
    }
    public class DiscoverCopy : SpellCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SPELL | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(async (game,card)=>
            {
                var discovered = await card.getOwner().discover(game,card.getOwner().hand);
                await card.getOwner().hand.add(game,game.createCard(discovered.define));
            })
        };
    }
    public class TestWeapon : ItemCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.ITEM | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
    }
    /// <summary>
    /// 1 腐蚀术 选择一个敌方随从，在你的回合开始时，消灭该随从。
    /// </summary>
    public class Corruption : SpellCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SPELL | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((g,c,t)=>
            {
                return t.addBuff(g,new GeneratedBuff(ID,
                    new THHEffectBefore<THHGame.TurnStartEventArg>(PILE.FIELD,(g1,c1,a1)=>
                    {
                        return a1.player == c.getOwner();
                    },null,async (g1,c1,a1)=>
                    {
                        await c1.setDead(g,true);
                        await g1.updateDeath();
                    })));
            }, PileFlag.field,null,(g,c,t)=>
            {
                return t.getOwner() != c.getOwner();
            })
        };
    }

    /// <summary>
    /// 测试：冷静头脑
    /// 冻结一个角色
    /// </summary>
    public class TestFreezeBrain : SpellCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SPELL | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((g, c, t) =>
            {
                t.setFreeze(g,true);
                return Task.CompletedTask;
            },PileFlag.both | PileFlag.character)
        };
    }
    public class Lightspawn : ServantCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SERVANT | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new PropModifyPassiveEffect(PileFlag.field, new PropHook<int>(PROPERTY.ATTACK,PROPERTY.CURRENT_LIFE))
        };
        class PropModifyPassiveEffect : PassiveEffect
        {
            public override string[] piles { get; }
            public PropModifier[] modifiers { get; }
            public PropModifyPassiveEffect(PileFlag pile, params PropModifier[] modifiers)
            {
                piles = pile.getPiles();
                this.modifiers = modifiers;
            }
            public override async Task onEnable(THHGame game, Card card, Buff buff)
            {
                foreach (var modifier in modifiers)
                {
                    await card.addModifier(game, modifier);
                }
            }
            public override async Task onDisable(THHGame game, Card card, Buff buff)
            {
                foreach (var modifier in modifiers)
                {
                    await card.removeModifier(game, modifier);
                }
            }
        }



    }

    /// <summary>
    /// 维伦
    /// 法术和英雄技能造成的伤害和治疗翻倍
    /// </summary>
    public class Velen : ServantCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SERVANT | 0x023;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 7;
        public override int attack { get; set; } = 7;
        public override int life { get; set; } = 7;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHCard.DamageEventArg>(PILE.FIELD,
                (g, c, a) =>
                {
                    return a.infoDic.Any(p=>p.Value.source.getOwner() == c.getOwner() && (p.Value.source.isSkill() || p.Value.source.isSpell()));
                },null,(g,c,a)=>
                {
                    foreach (var p in a.infoDic)
                    {
                        if(p.Value.source.getOwner() == c.getOwner() && (p.Value.source.isSkill() || p.Value.source.isSpell()))
                            p.Value.damageValue *= 2;
                    }
                    return Task.CompletedTask;
                }),
            new THHEffectBefore<THHCard.HealEventArg>(PILE.FIELD,
                (g, c, a) =>
                {
                    return a.source.getOwner() == c.getOwner() && a.source.isSpell();
                },null,(g,c,a)=>
                {
                    a.value *= 2;
                    return Task.CompletedTask;
                })
        };
    }


    /// <summary>
    /// 力量的代价
    /// 1，使一个友方随从获得+4/+4，回合结束消灭它
    /// </summary>
    public class PowrOverwhelming : SpellCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SPELL | 0x018;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                await target.addBuff(game,new GeneratedBuff(ID,new AttackModifier(4),new LifeModifier(4)));
                await target.addBuff(game,new GeneratedBuff(ID,new THHEffectBefore<THHGame.TurnEndEventArg>(
                    PILE.FIELD,
                    (g1, c1, a1) =>
                    {
                        return a1.player == c1.getOwner();
                    },
                    null,
                    async (g1,c1,a1) =>
                    {
                    //起效部分，效果是消灭随从
                    await c1.setDead(game, true);
                    //由于回合开始不会进行死亡结算，所以这里要强制进行一次。
                    await g1.updateDeath();
                    }
                    )));
            },PileFlag.self | PileFlag.field)
        };
    }
    /// <summary>
    /// 测试用奥秘
    /// 2费，释放奥秘：冰冻陷阱
    /// </summary>
    public class FreezingTrap : SpellCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SPELL | 0x019;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new SecretTriggerBefore<THHCard.AttackEventArg>((g,c,a)=>
            {
                return a.card.getOwner() != c.getOwner() && a.card.canAttackTest(g);
            },async(g,c,a)=>
            {
                 await a.card.fieldBackToHand(g);
                 await a.card.addBuff(g, new GeneratedBuff(ID,new CostModifier(2)));
            })
        };
    }
    /// <summary>
    /// 测试用奥秘
    /// 2费，释放奥秘：蒸发
    /// </summary>
    public class Vaporize : SpellCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SPELL | 0x020;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new SecretTriggerBefore<THHCard.AttackEventArg>((g,c,a)=>
            {
                return a.card.getOwner() != c.getOwner() && a.card.canAttackTest(g);
            },async(g,c,a)=>
            {
                 await a.card.setDead(g,true);
                 await g.updateDeath();//死亡结算
            })
        };
    }
    public class ChargeServant : ServantCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SERVANT | 0x021;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.CHARGE };
    }

    /// <summary>
    /// 治疗
    /// 0，治疗3
    /// </summary>
    public class TestHeal : SpellCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SPELL | 0x022;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
              new LambdaSingleTargetEffect((g, c, t) =>
                {
                    return t.heal(g,c,3);
                },PileFlag.both | PileFlag.character)
        };
    }

    /// <summary>
    /// 变形机制测试
    /// 4费  变羊术
    /// </summary>
    public class Polymorph : SpellCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SPELL | 0x023;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async(g, c, t) =>
            {
                await THHCard.setDefine(t, g,g.getDefine<RushServant>());//没有羊
            },PileFlag.both | PileFlag.field)
        };
    }
    public class EnemyOnlyBattleCry : ServantCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SPELL | 0x024;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((g,c,t)=>
            {
                return t.damage(g,c,1);
            }, PileFlag.oppo | PileFlag.character)
        };
    }
    /// <summary>
    /// 恐惧战马 411 亡语：将这张牌从墓地置入战场。
    /// </summary>
    public class Dreadsteed : ServantCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SERVANT | 0x025;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID,(g,c,b,p)=>
            {
                THHPlayer player = c.getOwner();
                return player.reborn(g,c,player.field,p);
            })
        };
    }
    public class Anubarak : ServantCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SERVANT | 0x026;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 9;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID,(g,c,b,p)=>
            {
                THHPlayer player = c.getOwner();
                player.createToken(g,g.getDefine<Nerubian>(),p);
                return c.fieldBackToHand(g);
            })
        };
    }
    public class Nerubian : ServantCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SERVANT | 0x027;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 4;
    }
    public class BlessingOfMight : SpellCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SPELL | 0x28;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((g,c,t)=>
            {
                return t.addBuff(g,new GeneratedBuff(ID, new AttackModifier(3)));
            }, PileFlag.field | PileFlag.both)
        };
    }
    /// <summary>
    /// 祈求牌，不触发任何效果，只是提供一次祈求。
    /// </summary>
    public class InvokeCard : SpellCardDefine
    {
        public const int ID = TestMaster.ID | CATEGORY.SPELL | 0x29;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect((g,c)=>
            {
                return Sanae.revealDuel(c.getOwner(),g);
            })
        };
    }
}