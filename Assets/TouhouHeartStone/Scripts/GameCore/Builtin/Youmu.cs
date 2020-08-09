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
                    return c1.getOwner().gem > c1.getCost(g1) && c1.getOwner() == a1.player;
                }, null, async (g, c, a) =>
                {
                    g.logger.log(c + "蓄力完成");
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
            await card.getOwner().changeArmor(game, 2);
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
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect),
           new KonpakuYoumu.AccumulateTrigggerEffectBefore()
        };
        static async Task effect(THHGame game, Card card)
        {
            Card item = card.getOwner().deck.First(c => c.define is ItemCardDefine);
            await card.getOwner().draw(game, item);
            if (card.isAccumulate(game))
                await item.addBuff(game, new GeneratedBuff(ID, new AttackModifier(1)));
        }
    }
    /// <summary>
    /// 1 悟入幻想 获得6点护甲，并在你的下个回合开始时失去6点护甲，蓄力：下个回合开始时不再失去6点护甲。
    /// </summary>
    public class Enlightenment : SpellCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SPELL | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect),
           new KonpakuYoumu.AccumulateTrigggerEffectBefore()
        };
        static async Task effect(THHGame game, Card card)
        {
            if (!card.isAccumulate(game))
            {
                await card.getOwner().changeArmor(game, 6);
                await card.getOwner().master.addBuff(game, new GeneratedBuff(ID, new ArmorEffectBefore<THHGame.TurnStartEventArg>((g1, c1, a1) =>
                {
                    return a1.player == c1.getOwner();
                }, (g2, c2, a2) =>
                {
                    c2.getOwner().changeArmor(game, -6);
                    return c2.getOwner().master.removeBuff(game, c2.getOwner().master.getBuffs().Where(b => b.id == ID));
                })));
            }
            else
                await card.getOwner().changeArmor(game, 6);
        }
        class ArmorEffectBefore<T> : THHEffectBefore<T> where T : IEventArg
        {
            public ArmorEffectBefore(CheckConditionDelegate onCheckCondition, ExecuteDelegate onExecute)
                : base(PileName.MASTER, onCheckCondition, null, async (g, c, a) =>
                {
                    await onExecute?.Invoke(g, c, a);
                })
            {
            }
        }
    }
    /// <summary>
    /// 1 半灵猛击 对目标随从造成相当于护甲值的伤害。
    /// </summary>
    public class HalfSpiritSlam : SpellCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SPELL | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.damage(game,card,card.getOwner().armor);
            })
        };
    }
    /// <summary>
    /// 121 冥界住人 亡语：将一个1/1的幽灵置入你的手牌
    /// </summary>
    public class Underworlddweller : ServantCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SERVANT | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID,(g1,c1,p)=>
            {
                return c1.getOwner().addCardToHand<Ghost>(g1);
            })
        };
    }
    /// <summary>
    /// 111 衍生物 幽灵
    /// </summary>
    public class Ghost : ServantCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SERVANT | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override bool isToken { get; set; } = true;
    }
    /// <summary>
    /// 232 楼观剑 蓄力：当你的英雄消灭一个角色之后，获得一次额外的攻击机会
    /// </summary>
    public class LouGuanjian : ItemCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.ITEM | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new KonpakuYoumu.AccumulateTrigggerEffectBefore(),
            new THHEffectAfter<THHCard.AttackEventArg>(PileName.ITEM,(g,c,a)=>
            {
                return a.card == c.getOwner().master && c.isAccumulate(g);
            },null,(g,c,a)=>
            {
                if(a.target.isDead(g))
                {
                    c.getOwner().master.setAttackTimes(c.getOwner().master.getAttackTimes(g)-1);
                }
                return Task.CompletedTask;
            })
        };
    }
    /// <summary>
    /// 2 二百由旬一闪 抽一张牌，如果你拥有护甲，则再抽一张牌
    /// </summary>
    public class TwoHundredYushunInAFlash : SpellCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SPELL | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { Keyword.ACCUMULATE };
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect),
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().draw(game);
            if (card.getOwner().armor > 0)
                await card.getOwner().draw(game);
        }
    }
    /// <summary>
    /// 2 樱花闪闪 使你的英雄对所有敌方随从进行攻击，生命值越低的越优先。
    /// </summary>
    public class CherryBlossom : SpellCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SPELL | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { Keyword.ACCUMULATE };
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect),
        };
        static async Task effect(THHGame game, Card card)
        {
            IEnumerable<Card> servants = game.getOpponent(card.getOwner()).field.OrderBy(c => c.getCurrentLife(game));
            foreach (Card target in servants)
                card.getOwner().cmdAttack(game, card.getOwner().master, target);
        }
    }
    /// <summary>
    /// 4 幽明的苦轮 使你的英雄在本回合内获得等同于护甲值的攻击力，失去所有护甲。
    /// </summary>
    public class FaintBitterWheel : SpellCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SPELL | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { Keyword.ACCUMULATE };
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect),
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().master.addBuff(game, new GeneratedBuff(ID, new AttackModifier(card.getOwner().armor),
                new RemoveBuffBefore<THHGame.TurnEndEventArg>(PileName.MASTER, ID)));
            await card.getOwner().changeArmor(game, 0, true);
        }
    }
    /// <summary>
    /// 5 天人亦五衰 使你的英雄在本回合内获得攻击力+5，蓄力：改为攻击力+7
    /// </summary>
    public class HeavenAndManAlsoFail : SpellCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SPELL | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new KonpakuYoumu.AccumulateTrigggerEffectBefore(),
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            int attack;
            if (!card.isAccumulate(game))
                attack = 5;
            else
                attack = 7;
            await card.getOwner().master.addBuff(game, new GeneratedBuff(ID, new AttackModifier(attack),
                new RemoveBuffBefore<THHGame.TurnEndEventArg>(PileName.MASTER, ID)));
        }
    }
    /// <summary>
    /// 324 魂魄妖忌 每当你的英雄攻击时，使其装备的道具获得攻击力+1
    /// </summary>
    public class YoukiKonpaku : ServantCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SERVANT | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHCard.AttackEventArg>(PileName.FIELD,(g,c,a)=>
            {
                return a.card == c.getOwner().master;
            },null,async(g,c,a)=>
            {
                if(c.getOwner()[PileName.ITEM].isFull)
                    await c.getOwner().item.addBuff(g, new GeneratedBuff(ID, new AttackModifier(1)));
            })
        };
    }
    /// <summary>
    /// 332 妖怪铁匠 战吼：使你的武器在本回合内获得免疫。
    /// </summary>
    public class MonsterBlacksmith : ServantCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SERVANT | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().item.addBuff(game, new GeneratedBuff(ID,
                new KeywordsModifier(KeywordsModifier.ModifyType.add, Keyword.IMMUNITY),
                new RemoveBuffBefore<THHGame.TurnEndEventArg>(PileName.ITEM, ID)));
        }
    }
    /// <summary>
    /// 323 列队游魂 当回合结束时，如果你的护甲值大于0，那么召唤一个1/1的幽灵。
    /// </summary>
    public class LinedUp : ServantCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SERVANT | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 3;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new BeforeYourTurnEnd(async(game,card,arg)=>
            {
                if(card.getOwner().armor>0)
                    await card.getOwner().createToken(game, game.getCardDefine<Ghost>());
            })
        };
    }
    /// <summary>
    /// 411 半幽灵 具有等同于护甲值的额外属性值
    /// </summary>
    public class HalfGhost : ServantCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.SERVANT | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(async(g1,c1)=>
            {
                await c1.addBuff(g1, new GeneratedBuff(ID,new AttackModifier(c1.getOwner().armor+c1.define.getAttack(),true)));
                await c1.addBuff(g1, new GeneratedBuff(ID,new LifeModifier(c1.getOwner().armor+c1.define.getLife(),true)));
            }),
            new THHEffectAfter<THHPlayer.ChangeArmorEventArg>(PileName.FIELD,(g,c,a)=>
            {
                return a.player == c.getOwner();
            },null,async(g,c,a)=>
            {
                await c.addBuff(g, new GeneratedBuff(ID,new AttackModifier(c.getOwner().armor+c.define.getAttack(),true)));
                await c.addBuff(g, new GeneratedBuff(ID,new LifeModifier(c.getOwner().armor+c.define.getLife(),true)));
            })
        };
    }
    /// <summary>
    /// 232 白楼剑 战吼：如果你已经装备另外一个道具，则不装备此道具，改为使其获得此道具的耐久度。
    /// </summary>
    public class Bailoujian : ItemCardDefine
    {
        public const int ID = KonpakuYoumu.ID | CardCategory.ITEM | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(async(g1,c1)=>
            {
                if(c1.getOwner()[PileName.ITEM].isFull)
                {
                    await c1.getOwner().item.addBuff(g1, new GeneratedBuff(ID, new LifeModifier(1)));
                    await c1.die(g1);
                }
            })
        };
    }
}

