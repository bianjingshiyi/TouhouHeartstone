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
        public override int skillID { get; set; } = LunaticShoot.ID;//英雄技能的ID
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 技能：开火
    /// 对敌方英雄造成2点伤害
    /// </summary>
    public class LunaticShoot : SkillCardDefine
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
    /// 2费，奥秘,当敌方角色攻击时，取消攻击并使其受到相当于其攻击力的伤害。+
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
    /// 113 野兔兽妖 激励：攻击力变为3+
    /// </summary>
    public class WildYoukaiRabbit : ServantCardDefine
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
            },(g,c,a)=>
            {
                a.isCanceled = true;
                return Task.CompletedTask;
            })
        };
    }

    /// <summary>
    /// 赤眼催眠
    /// 2费，奥秘,当敌方不低于2攻随从攻击时，获得他的控制权。
    /// </summary>
    public class MindShaker : SpellCardDefine
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
                    var token = await c.getOwner().createToken(g,g.getDefine<AfterImage>(),c.getOwner().field.count);
                    tokens.Add(token.card);
                }
                a.target = tokens.random(g);
            })
        };
    }

    /// <summary>
    /// 残像
    /// 0/0/2
    /// 衍生物
    /// </summary>
    public class AfterImage : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };
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
                return a.card.getOwner() != c.getOwner()  && a.card.isSpell() && a.card.define.getActiveEffect() is ITargetEffect ;
            },(g,c,a)=>
            {
                Card[] range = a.card.getAvaliableTargets(g);
                Card newTarget = range.Where(target => !target.Equals(a.targets)).random(g);
                Card[] targets = new Card[]{newTarget};
                a.targets = targets;
                return Task.CompletedTask;
            })
        };
    }

    /// <summary>
    /// 紫外线场
    /// 1费，复原你的技能并使你的技能在下一次使用前法力值消耗变为0。
    /// </summary>
    public class FieldUltraViolet : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
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
                await card.getOwner().skill.setUsed(game, false);
            }
        }
    }

    /// <summary>
    /// 国士无双之药
    /// 1，使一个友方随从获得+3/+3，下回合开始时消灭它（警告：不要过量嗑药！）
    /// </summary>
    public class PeerlessPatriotsElixir : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                if(!target.getBuffs().Any(b => b.id == ID))
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
                }
                else
                {
                    await target.setDead(game, true);
                }
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
    public class Eirin_TheMaster : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 8;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };//关键词标签：
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

    /// <summary>
    /// 1费 精神投弹 复原你的技能并使你的技能在下一次使用前能以随从作为目标。
    /// </summary>
    public class MindDropping : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)//这不是个指向性技能
        };
        static async Task effect(THHGame game, Card card)
        {
            if (card.getOwner().skill.isUsed(game))
            {
                await card.getOwner().skill.setUsed(game, false);
            }
            var arg = await THHCard.setDefine(card.getOwner().skill, game, game.getDefine<LunaticShoot_MindDropping>());
            await card.getOwner().skill.addBuff(game, new GeneratedBuff(ID,
                new THHEffectAfter<THHPlayer.UseEventArg>(PILE.FIELD,
              (g, c, a) =>
              {
                  return a.player == c.getOwner() && a.card.isSkill();
              }, null, async (g, c, a) =>
              {
                  await THHCard.setDefine(card.getOwner().skill, game, arg.beforeDefine);
              }
             )));
        }
    }
    public class LunaticShoot_MindDropping : SkillCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SKILL | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2; //技能花费
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async(g, c, t) =>
            {
                await t.damage(g,c,2);
            },PileFlag.both | PileFlag.character,onCheckTarget:(g,c,t)=> !(t.isMaster() && t.getOwner() == c.getOwner()))
        };

    }

    /// <summary>
    /// 334 贩药货郎 战吼：回复4点生命值
    /// </summary>
    public class MedicineSellman : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = { TAG.BEAST };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async(g,c,t)=>
            {
                await t.heal(g,c,6);
            },PileFlag.both | PileFlag.character)
        };
    }

    /// <summary>
    /// 434 叛逃特工 每当你使用英雄技能时，对随机敌人造成3点伤害
    /// </summary>
    public class DefectedAgent : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = { TAG.BEAST };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<THHPlayer.UseEventArg>(PILE.FIELD,
            (g, c, a) =>
            {
                return a.player == c.getOwner() && a.card.isSkill();
            },null,
            async(g,c,a)=>{
                await g.getAllEnemies(c.getOwner()).random(g).damage(g,c,2);
            })
        };
    }

    /// <summary>
    /// 因幡帝
    /// 抽一张牌，如果是奥秘则置入战场并重复此战吼
    /// </summary>
    public class TewiInaba : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE, KEYWORD.STEALTH };//关键词标签：
        public override string[] tags { get; set; } = { TAG.BEAST };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().draw(game);
            if (card.getOwner().deck.count >= 0)
            {
                var drawed = game.triggers.getRecordedEvents().OfType<THHPlayer.DrawEventArg>().Last().card;
                if (drawed.hasKeyword(game, KEYWORD.SECRET) &&
                    drawed.define is SpellCardDefine &&
                    card.getOwner().head.count < card.getOwner().head.maxCount &&
                    card.getOwner().head.All(c => c.define != drawed.define))
                {
                    await card.getOwner().hand.moveTo(game, drawed, card.getOwner().head);
                    await effect(game, card);
                }
            }
        }
    }

    /// <summary>
    /// 1费 红外线场 使你的技能下一次造成的伤害翻倍。
    /// </summary>
    public class FieldUltraRed : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            if (card.getOwner().skill.isUsed(game))
            {
                await card.getOwner().skill.setUsed(game, false);
            }
            await card.getOwner().master.addBuff(game, new GeneratedBuff(ID, new THHEffectBefore<THHCard.DamageEventArg>(PILE.FIELD,
            (g, c, a) =>
            {
                return a.source.getOwner() == c.getOwner() && a.source.isSkill();
            }, null, (g, c, a) =>
            {
                a.value *= 2;
                return Task.CompletedTask;
            }),
            new RemoveBuffAfter<THHPlayer.UseEventArg>(PILE.SKILL, (g, c, a) =>
            {
                return a.player == c.getOwner() && a.card.isSkill();
            }, ID)));
        }
    }

    /// <summary>
    /// 322 狂月枪
    /// 每当你将一个奥秘置入战场，便获得+1耐久度。
    /// </summary>
    public class LunaticGun : ItemCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.ITEM | 0x018;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<Pile.MoveCardEventArg>(PILE.ITEM,
            (g,c,a)=>{
                return a.from.owner == c.getOwner() &&
                a.to == c.getOwner().head &&
                a.card.define is SpellCardDefine &&
                a.card.hasKeyword(g,KEYWORD.SECRET);
            },null,
            async(g,c,a)=>{
                await c.addBuff(g,new GeneratedBuff(ID,new LifeModifier(1)));
            })
        };
    }
    /// <summary>
    /// 虚假幻象 1 将你本回合中使用的上一张法术牌的复制置入你的手牌。
    /// </summary>
    public class DisbeliefAspect : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x019;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect((g,c)=>
            {
                var lastSpellUse = g.triggers.getRecordedEvents()
                    .skipUntilLast(e=> e is THHGame.TurnStartEventArg turnStart)
                    .OfType<THHPlayer.UseEventArg>()
                    .LastOrDefault(u=> u.player==c.getOwner()&&u.card.isSpell());
                if (lastSpellUse != null)
                    return c.getOwner().hand.add(g,g.createCard(lastSpellUse.card.define));
                else
                    return Task.CompletedTask;
            })
        };
    }
}