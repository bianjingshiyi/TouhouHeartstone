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
    public class VisionaryTuning : SpellCardDefine
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
    [IgnoreCardDefine]
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
}