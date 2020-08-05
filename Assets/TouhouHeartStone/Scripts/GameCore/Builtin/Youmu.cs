using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    public class KonpakuYoumu : MasterCardDefine
    {
        public const int ID = 0x00600000 | CardCategory.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = StartdustBullet.ID;
        public override IEffect[] effects { get; set; } = new Effect[0];

        public class AccumulateTrigggerEffectBefore : THHEffectBefore<THHGame.TurnEndEventArg> 
        {
            public AccumulateTrigggerEffectBefore()
                : base(PileName.HAND, (g1, c1, a1) =>
                {
                    return c1.getOwner().gem > c1.getCost(g1)&&c1.getOwner()==a1.player;
                }, null, async (g, c, a) =>
                {
                    c.setAccumulate(true);
                    await g.updateDeath();
                })
            {
            }
        }
    }
    /// <summary>
    /// 2 妖梦技能：奇妙半身 叠甲过
    /// </summary>
    public class Wonderfulhalfbody : SkillCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SKILL | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]{
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            card.getOwner().master.setArmor(2);
        }
    }
    /// <summary>
    /// 1 现世斩 使你的英雄在本回合内获得攻击力+2，蓄力：改为攻击力+3
    /// </summary>
    public class CutInTheWorld : SpellCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SPELL | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect),
           new KonpakuYoumu.AccumulateTrigggerEffectBefore()
        };
        static async Task effect(THHGame game, Card card)
        {
            int attack;
            if (!card.isAccumulate(game))
                attack = 2;
            else
                attack = 3;
            await card.getOwner().master.addBuff(game,
                new GeneratedBuff(ID, new AttackModifier(attack),
                new RemoveBuffBefore<THHGame.TurnEndEventArg>(PileName.MASTER, ID)));
        }
    }
    /// <summary>
    /// 1 冥想斩 抽一张道具牌，蓄力：使其获得攻击力+1
    /// </summary>
    public class MeditationSlash : SpellCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SPELL | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override string[] keywords { get; set; } = new string[] { Keyword.ACCUMULATE };
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {

        }
    }
    /// <summary>
    /// 1 悟入幻想 获得6点护甲，并在你的下个回合开始时失去6点护甲，蓄力：下个回合开始时不再失去6点护甲。
    /// </summary>
    public class Enlightenment : SpellCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SPELL | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override string[] keywords { get; set; } = new string[] { Keyword.ACCUMULATE };
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {

        }
    }
    /// <summary>
    /// 1 半灵猛击 对目标随从造成相当于护甲值的伤害。
    /// </summary>
    public class HalfSpiritSlam : SpellCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SPELL | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.damage(game,card,card.getOwner().master.getArmor(game));
            })
        };
    }

}

