using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    public class Youmu : MasterCardDefine
    {
        public const int ID = 0x00500000 | CardCategory.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = StrangeHalfbody.ID;
        public override IEffect[] effects { get; set; } = new Effect[0];
        public const string PREHEAT = "preheat";
        public const string IS_PREHEATED = "isPreheated";
        /// <summary>
        /// 蓄力：当回合结束时，如果你的最大法力值大于等于这张卡的花费，那么将这张卡标记为蓄力。在使用一张牌的时候，如果这张牌具有蓄力效果且处于蓄力状态，那么触发该效果。
        /// </summary>
        public class PreHeat : THHEffectBefore<THHGame.TurnEndEventArg>
        {
            public PreHeat() : base(PileName.HAND, (g1, c1, a1) =>
            {
                return c1.getOwner() == a1.player && c1.getOwner().maxGem >= c1.getCost(g1) && !isPreHeated(g1, c1);
            }, null, (g, c, a) =>
            {
                return setPreHeated(g, c, true);
            })
            {
            }
        }
        public class PreHeatEventArg : EventArg
        {
            public Card card;
            public bool value;
        }
        public static bool isPreHeated(IGame game, Card card)
        {
            return card.getProp<bool>(game, IS_PREHEATED);
        }
        public static Task setPreHeated(THHGame game, Card card, bool value)
        {
            PreHeatEventArg eventArg = new PreHeatEventArg() { card = card, value = value };
            return game.triggers.doEvent(eventArg, arg =>
            {
                card = arg.card;
                value = arg.value;
                game.logger.log(card + "蓄力 => " + value);
                card.setProp(IS_PREHEATED, value);
                return Task.CompletedTask;
            });
        }
    }
    /// <summary>
    /// 2 妖梦技能：奇妙半身 叠甲过
    /// </summary>
    public class StrangeHalfbody : SkillCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SKILL | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().master.gainArmor(game, 2);
        }
    }
    /// <summary>
    /// 1 现世斩 使你的英雄在本回合内获得攻击力+2，蓄力：改为攻击力+3
    /// </summary>
    public class SlashOfPresentWorld : SpellCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SPELL | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { Youmu.PREHEAT };
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect),
           new Youmu.PreHeat()
        };
        static async Task effect(THHGame game, Card card)
        {
            int attack;
            if (!Youmu.isPreHeated(game, card))
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
    public class Meditation : SpellCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SPELL | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect),
           new Youmu.PreHeat()
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().draw(game, c => c.isItem(), out var drawEvent);
            var item = drawEvent.card;
            if (Youmu.isPreHeated(game, card))
                await item.addBuff(game, new GeneratedBuff(ID, new AttackModifier(1)));
        }
    }
    /// <summary>
    /// 1 悟入幻想 获得6点护甲，并在你的下个回合开始时失去6点护甲，蓄力：下个回合开始时不再失去6点护甲。
    /// </summary>
    public class FantasyOfEnteringEnlightenment : SpellCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SPELL | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { Youmu.PREHEAT };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect),
            new Youmu.PreHeat()
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().master.gainArmor(game, 6);
            if (!Youmu.isPreHeated(game, card))
            {
                await card.getOwner().master.addBuff(game, new GeneratedBuff(ID,
                    new BeforeTurnStart(async (g, c, a) =>
                    {
                        await c.loseArmor(g, 6);
                        await c.removeBuff(game, ID);

                    }, (g, c, a) =>
                    {
                        return a.player == c.getOwner();
                    })));
            }
        }
    }
    /// <summary>
    /// 1 半灵猛击 对目标随从造成相当于护甲值的伤害。
    /// </summary>
    public class HalfPhantonSlam : SpellCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SPELL | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.damage(game,card,card.getOwner().master.getArmor(game));
            })
        };
    }
    /// <summary>
    /// 121 冥界住人 亡语：将一个1/1的幽灵置入你的手牌
    /// </summary>
    public class NetherworldInhabitant : ServantCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SERVANT | 0x006;
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
        public const int ID = Youmu.ID | CardCategory.SERVANT | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override bool isToken { get; set; } = true;
    }
    /// <summary>
    /// 232 楼观剑 蓄力：当你的英雄消灭一个角色之后，获得一次额外的攻击机会
    /// </summary>
    public class Roukanken : ItemCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.ITEM | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new Youmu.PreHeat(),
            new THHEffectAfter<THHCard.AttackEventArg>(PileName.ITEM,(g,c,a)=>
            {
                return a.card == c.getOwner().master && Youmu.isPreHeated(g,c);
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
    public class TwoHundredYojanaOneSlash : SpellCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SPELL | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { Youmu.PREHEAT };
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect),
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().draw(game);
            if (card.getOwner().master.getArmor(game) > 0)
                await card.getOwner().draw(game);
        }
    }
    /// <summary>
    /// 2 樱花闪闪 使你的英雄对所有敌方随从进行攻击，生命值越低的越优先。
    /// </summary>
    public class CherryBlossomFlashing : SpellCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SPELL | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { Youmu.PREHEAT };
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect),
        };
        static Task effect(THHGame game, Card card)
        {
            THHPlayer player = card.getOwner();
            Card[] servants = game.getOpponent(player).field.OrderBy(c => c.getCurrentLife(game)).ToArray();
            foreach (Card target in servants)
            {
                if (player.master.getAttack(game) > 0)
                    player.master.forceAttack(game, player, target);
            }
            return Task.CompletedTask;
        }
    }
    /// <summary>
    /// 4 幽明的苦轮 使你的英雄在本回合内获得等同于护甲值的攻击力，失去所有护甲。
    /// </summary>
    public class WheelOfPainOfTheLivingAndDead : SpellCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SPELL | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { Youmu.PREHEAT };
        public override IEffect[] effects { get; set; } = new IEffect[] {
           new NoTargetEffect(effect),
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().master.addBuff(game, new GeneratedBuff(ID, new AttackModifier(card.getOwner().master.getArmor(game)),
                new RemoveBuffBefore<THHGame.TurnEndEventArg>(PileName.MASTER, ID)));
            await card.getOwner().master.loseArmor(game, card.getOwner().master.getArmor(game));
        }
    }
    /// <summary>
    /// 5 天人亦五衰 使你的英雄在本回合内获得攻击力+5，蓄力：改为攻击力+7
    /// </summary>
    public class FiveSignsOfTheDyingDeva : SpellCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SPELL | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override string[] keywords { get; set; } = new string[] { Youmu.PREHEAT };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new Youmu.PreHeat(),
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            int attack;
            if (!Youmu.isPreHeated(game, card))
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
    public class KonpakuYouki : ServantCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SERVANT | 0x013;
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
    public class YoukaiSmith : ServantCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SERVANT | 0x014;
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
    public class ProcessingPhantoms : ServantCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SERVANT | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 3;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new BeforeYourTurnEnd(async(game,card,arg)=>
            {
                if(card.getOwner().master.getArmor(game)>0)
                    await card.getOwner().createToken(game, game.getCardDefine<Ghost>());
            })
        };
    }
    /// <summary>
    /// 411 半幽灵 具有等同于护甲值的额外属性值
    /// </summary>
    public class HalfPhantom : ServantCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.SERVANT | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new IntPropFixer<THHCard.SetArmorEventArg>(PileFlag.field,
                new IntPropFixer<THHCard.SetArmorEventArg>.ModifierFixer(
                    new AttackModifier(0),(g,c,b,a)=>
                    {
                        return c.getOwner().master.getArmor(g);
                    }),
                new IntPropFixer<THHCard.SetArmorEventArg>.ModifierFixer(
                    new LifeModifier(0),(g,c,b,a)=>
                    {
                        return c.getOwner().master.getArmor(g);
                    }))
        };
    }
    /// <summary>
    /// 232 白楼剑 如果你已经装备另外一个道具，则不装备此道具，改为使其获得此道具的耐久度。
    /// </summary>
    public class Hakurouken : ItemCardDefine
    {
        public const int ID = Youmu.ID | CardCategory.ITEM | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 3;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            //new NoTargetEffect(async(g1,c1)=>
            //{
            //    if(c1.getOwner()[PileName.ITEM].isFull)
            //    {
            //        await c1.getOwner().item.addBuff(g1, new GeneratedBuff(ID, new LifeModifier(1)));
            //        await c1.die(g1);
            //    }
            //})
            new THHEffectBefore<THHPlayer.UseEventArg>(PileName.HAND,
                (g1,c1,a1)=>
                {
                    return a1.card == c1 && c1.getOwner().item != null;
                },null,
                (g1,c1,a1)=>
                {
                    a1.action = async arg =>
                    {
                        THHPlayer player = c1.getOwner();
                        await player.item.addBuff(g1,new GeneratedBuff(ID,new LifeModifier(c1.getLife(g1))));
                        await player.hand.moveTo(g1,c1,player.grave);
                    };
                    return Task.CompletedTask;
                })
        };

    }
}

