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
    /// 国士无双药
    /// 2费，重置你的英雄技能。在本回合，你的英雄技能法力值消耗变为0，技能伤害+2
    /// </summary>
    //[IgnoreCardDefine]
    //public class Analeptic : SpellCardDefine
    //{
    //    public const int ID = Reisen.ID | CATEGORY.SPELL | 0x001;
    //    public override int id { get; set; } = ID;
    //    public override int cost { get; set; } = 2; //花费
    //    public override IEffect[] effects { get; set; } = new IEffect[]
    //    {
    //        new NoTargetEffect(effect)

    //    };

    //    static async Task effect(THHGame game, Card card)
    //    {

    //        await card.getOwner().skill.addBuff(game,new GeneratedBuff(ID, new CostModifier(0, true), 
    //                                                 new RemoveBuffAfter<THHGame.TurnEndEventArg>(PILE.SKILL, ID))
    //                                                 );

    //        await card.getOwner().skill.addBuff(game, new GeneratedBuff(ID,
    //                                                  new THHEffectBefore<THHCard.DamageEventArg>(PILE.SKILL,
    //                                                      (g, c, a) =>
    //                                                      {
    //                                                          return a.source == c && c.isSkill();
    //                                                      }, null, (g, c, a) =>
    //                                                      {
    //                                                          a.value += 2;
    //                                                          return Task.CompletedTask;
    //                                                      }
    //                                                      ),
    //                                                      new RemoveBuffAfter<THHGame.TurnEndEventArg>(PILE.SKILL, ID)));

    //    if (card.getOwner().skill.isUsed(game) == true)
    //    {
    //        await card.getOwner().skill.setUsed(game, false);
    //    }

    //    }

    //}

    ///// <summary>
    ///// 叛逃月兔
    ///// 2/2/1，战吼：对随机敌方角色造成2点伤害。你每使用一次英雄技能，触发此战吼。
    ///// </summary>
    //public class Marin : ServantCardDefine
    //{
    //    public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x002;
    //    public override int id { get; set; } = ID;
    //    public override int cost { get; set; } = 2;
    //    public override int attack { get; set; } = 2;//随从的攻击力
    //    public override int life { get; set; } = 1;//随从的血量上限
    //    public override bool isToken { get; set; } = false;//是衍生物吗？
    //    public override IEffect[] effects { get; set; } = new IEffect[]
    //    {
    //        new NoTargetEffect(effect),

    //        new THHEffectAfter<THHPlayer.UseEventArg>(PILE.FIELD,
    //            (g,c,a)=>{
    //                return a.player == c.getOwner() && a.card.isSkill();
    //            }, null, (g, c, a) =>
    //            {
    //                g.getAllEnemies(c.getOwner()).random(g).damage(g, c, 2);
    //                return Task.CompletedTask;
    //            }
    //            )
    //    };
    //    static async Task effect(THHGame game, Card card)
    //    {
    //        await game.getAllEnemies(card.getOwner()).random(game).damage(game, card, 2);
    //    }
    //}

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
    /// 2 奥秘：生神停止 当敌方使用随从时，取消它的使用。
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
                return a.card.getOwner() != c.getOwner() && a.card != c;
            },async(g,c,a)=>
            {
                a.isCanceled = true;
            })
        };
    }
}