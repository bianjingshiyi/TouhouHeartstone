using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
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
        public const int ID = TestMaster.ID | CardCategory.SKILL | 0x000;
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
            new THHEffectBefore<THHGame.TurnEndEventArg>(PileName.FIELD,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return true;
            },(game,card,arg)=>
            {
                card.setProp("TestResult",true);
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
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE, (game,card,arg)=>
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
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE, (game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return true;
            },(game,card,arg,targets)=>
            {
                card.addBuff(game,new TestBuff());
                return Task.CompletedTask;
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
        public override string[] keywords { get; set; } = new string[] { Keyword.RUSH };
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
        public override string[] keywords { get; set; } = new string[] { Keyword.SHIELD };
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
        public override string[] keywords { get; set; } = new string[] { Keyword.STEALTH };
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
        public override string[] keywords { get; set; } = new string[] { Keyword.DRAIN };
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
        public override string[] keywords { get; set; } = new string[] { Keyword.POISONOUS };
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
        public override string[] keywords { get; set; } = new string[] { Keyword.ELUSIVE };
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
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.SKILL,(game,card,arg)=>
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
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,card,arg)=>
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
                    await target.damage(game, card, arg.player.getSpellDamage(1));
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
            new CostFixer()
        };
        class CostFixer : PassiveEffect
        {
            public override string[] piles { get; } = new string[] { PileName.HAND };
            CostModifier _modifier = new CostModifier();
            public override void onEnable(THHGame game, Card card)
            {
                card.addModifier(game, _modifier);
            }
            public override void onDisable(THHGame game, Card card)
            {
                card.removeModifier(game, _modifier);
            }
            class CostModifier : PropModifier<int>
            {
                public override string propName { get; } = nameof(ServantCardDefine.cost);
                public override int calc(Card card, int value)
                {
                    return value - card.getOwner().hand.count + 1;
                }

                public override PropModifier clone()
                {
                    return new CostModifier();
                }
            }
        }
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
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return true;
            },(game,card,arg,targets)=>
            {
                foreach(Card target in targets)
                {
                    target.setFreeze(true);
                }
                return Task.CompletedTask;
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
        //    public override void onEnable(THHGame game, Card card)
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
        public const int ID = TestMaster.ID | CardCategory.SPELL | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((game,card,target)=>
            {
                target.damage(game, card, card.getOwner().getSpellDamage(6));
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
        public override string[] keywords { get; set; } = new string[] { Keyword.POISONOUS };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHGame.TurnEndEventArg>(PileName.FIELD,(game,card,arg)=>
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
}