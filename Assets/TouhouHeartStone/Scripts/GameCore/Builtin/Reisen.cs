using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;



namespace TouhouHeartstone.Builtin
{
    public class Reisen : MasterCardDefine
    {
        public const int ID = 0x00600000 | CATEGORY.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = OpenFire.ID;//英雄技能的ID
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 技能：开火
    /// 对敌方英雄造成2点伤害
    /// </summary>
    [Obsolete]
    public class OpenFire : SkillCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2; //技能花费
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)//这不是个指向性技能
        };
        static async Task effect(THHGame game, Card card)
        {
            await game.getOpponent(card.owner as THHPlayer).master.damage(game, card, 2);
        }
    }



    /// <summary>
    /// 超短脑波
    /// 2费，奥秘,当敌方角色攻击时，取消攻击并使其受到相当于其攻击力的伤害。
    /// </summary>
    public class XWave : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x001;
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
                 a.isCanceled = true;   //取消攻击
                 await a.card.damage(g,a.card,a.card.getAttack(g));
            })
        };
    }

    /// <summary>
    /// 113 野兔兽妖 激励：攻击力变为3
    /// </summary>
    public class HareMonster : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = { TAG.BEAST };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<THHPlayer.UseEventArg>(PILE.FIELD,
              (g,c,a)=>{
                    return a.player == c.getOwner() && a.card.isSkill();
                }, null, async(g, c, a) =>
                {
                    await c.addBuff(g, new GeneratedBuff(ID,new AttackModifier(3,true)));
                }
                )
        };
    }

    /// <summary>
    /// 2 奥秘：生神停止 当敌方使用随从一张随从牌时，取消它的使用。
    /// </summary>
    public class MindStopper : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new SecretTriggerBefore<THHPlayer.UseEventArg>((g,c,a)=>
            {
                return a.card.getOwner() != c.getOwner() && a.card.isServant();
            },async(g,c,a)=>
            {
                a.isCanceled = true;
            })
        };
    }

    /// <summary>
    /// 赤眼催眠
    /// 2费，奥秘,当敌方不低于2攻随从攻击时，获得他的控制权。
    /// </summary>
    public class MineShaker : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new SecretTriggerBefore<THHCard.AttackEventArg>((g,c,a)=>
            {
                return a.card.getOwner() != c.getOwner() && a.card.canAttackTest(g) && a.card.isServant() && a.card.getAttack(g) <= 2;
            },async(g,c,a)=>
            {
                 a.isCanceled = true;   //取消攻击
                 await a.card.getOwner().field.moveTo(g,a.card,c.getOwner().field);//交换控制权
                 
            })
        };
    }

    /// <summary>
    /// 错乱之眼
    /// 2费，奥秘：当随从攻击时，召唤3个0/2且具有嘲讽的幻象并使其中随机一个成为攻击目标。
    /// </summary>
    public class DisorderEye : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new SecretTriggerBefore<THHCard.AttackEventArg>((g,c,a)=>
            {
                return a.card.getOwner() != c.getOwner() && a.card.canAttackTest(g) && a.card.isServant() && c.getOwner().field.count<=4;
            },async(g,c,a)=>
            {
                 List<Card> tokens = new List<Card>();
                for(int i=0 ; i<3; i++)
                {
                    var token = await c.getOwner().createToken(g,g.getDefine<Vision>(),c.getOwner().field.count);
                    tokens.Add(token.card);
                }
                a.target = tokens.random(g);
            })
        };
    }

    /// <summary>
    /// 幻象
    /// 0/0/2
    /// 衍生物
    /// </summary>
    public class Vision : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override int attack { get; set; } = 0;//随从的攻击力
        public override int life { get; set; } = 2;//随从的血量上限
        public override bool isToken { get; set; } = true;//是衍生物吗？
                                                          //public override string[] tags { get; set; } = new string[];//种族标签
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };//关键词标签：
    }

    /// <summary>
    /// 幻视调律
    /// 2费，奥秘：当敌方对一个目标使用法术时，使其目标变为其他随机目标。
    /// </summary>
    public class VisionaryTuning : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new SecretTriggerBefore<THHPlayer.UseEventArg>((g,c,a)=>
            {
                return a.card.getOwner() != c.getOwner()  && a.card.isSpell() && a.card.define.getActiveEffect() is ITargetEffect;
            },async(g,c,a)=>
            {
                Card[] range = a.card.getAvaliableTargets(g);
                Card[] targets = range.Where(target => target.Equals(a.targets)).random(g);
                a.targets = targets;
            })
        };
    }

    /// <summary>
    /// 幻视调律
    /// 1费，复原你的技能并使你的技能在下一次使用前法力值消耗变为0。
    /// </summary>
    public class MindStarMine : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        //public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
           new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().skill.addBuff(game, new GeneratedBuff(ID, new CostModifier(0, true), new RemoveBuffAfter<THHPlayer.UseEventArg>(PILE.SKILL, (g, c, a) =>
                                                                                                                                                               {
                                                                                                                                                                   return a.player == c.getOwner() && a.card.isSkill();
                                                                                                                                                               }, ID)));
            if (card.getOwner().skill.isUsed(game))
            {
               await  card.getOwner().skill.setUsed(game,false);
            }
        }
    }

    /// <summary>
    /// 国士无双之药
    /// 1，使一个友方随从获得+3/+3，下回合开始时消灭它
    /// </summary>
    public class PowrOverwhelming : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                await target.addBuff(game,new GeneratedBuff(ID,new AttackModifier(3),new LifeModifier(3)));
                await target.addBuff(game,new GeneratedBuff(ID,new THHEffectBefore<THHGame.TurnStartEventArg>(
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
                        await g1.updateDeath(g1);
                    }
                    )));
            },PileFlag.self | PileFlag.field)

            
        };
    }

    /// <summary>
    /// 幻象震爆
    /// 2,对敌方英雄打5
    /// </summary>
    public class MindExplosion : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            await game.getOpponent(card.getOwner()).master.damage(game, card, 5);
        }
    }

    /// <summary>
    /// 永琳
    /// 英雄技能造成的伤害翻倍,治疗翻倍
    /// </summary>
    public class EirinYagokoro : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x023;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 8;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHCard.DamageEventArg>(PILE.FIELD,
                (g, c, a) =>
                {
                    return a.source.getOwner() == c.getOwner() && a.source.isSkill();
                },null,(g,c,a)=>
                {
                    a.value *= 2;
                    return Task.CompletedTask;
                }),
            
            new THHEffectBefore<THHCard.HealEventArg>(PILE.FIELD,
                (g, c, a) =>
                {
                    return a.source.getOwner() == c.getOwner();
                },null,(g,c,a)=>
                {
                    a.value *= 2;
                    return Task.CompletedTask;
                })
        };
    }
}