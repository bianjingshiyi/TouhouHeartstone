using System;
using System.Linq;
using TouhouCardEngine;
using System.Threading.Tasks;
using TouhouCardEngine.Interfaces;
using System.Collections.Generic;

namespace TouhouHeartstone.Builtin
{
    /// <summary>
    /// 鲁莽的妖精，111，冲锋
    /// </summary>
    public class RashFairy : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.CHARGE };

        private IEffect[] effects1 = new IEffect[0];

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 人类村落守卫
    /// </summary>
    public class HumanVillageGuard : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };

        private IEffect[] effects1 = new IEffect[0];

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 铁炮猎人 中立普通卡 322 战吼：造成2点伤害
    /// </summary>
    public class RifleHunter : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[0];

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect((game, card, target) => { return target.damage(game, card, 2); },
                PileFlag.both | PileFlag.character)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 迷失亡灵 中立普通卡 541 亡语：召唤一个4/3的亡灵
    /// </summary>
    public class MissingSpecter : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.DEATHRATTLE };

        private IEffect[] effects1 = new IEffect[]
        {
            new DeathRattle(ID,
                (g1, c1, p) => { return c1.getOwner().createToken(g1, g1.getDefine<LostSpecter>(), p); })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 失落亡灵 中立衍生物 543
    /// </summary>
    public class LostSpecter : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x005;
        public override int id { get; set; } = ID;
        public override bool isToken { get; set; } = true;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 3;
        public override string[] keywords { get; set; } = new string[0];

        private IEffect[] effects1 = new IEffect[0];

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 梅雨元凶 法强+1
    /// </summary>
    public class DrizzleFairy : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 2;
        public override int spellDamage { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };
        public override string[] keywords { get; set; } = new string[0];

        private IEffect[] effects1 = new IEffect[0];

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    public class LuckyCoin : SpellCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect((game, card) => { return card.getOwner().setGem(game, card.getOwner().getGem() + 1); })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 炎之妖精 战吼：对一个随机敌方随从造成1点伤害
    /// </summary>
    public class FlameFairy : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            Player oppo = game.getOpponent(card.getOwner());
            await oppo.getField().random(game).damage(game, card, 1);
        }
    }

    /// <summary>
    /// 桑尼·缪尔可 潜行
    /// </summary>
    public class SunnyMilk : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.STEALTH };
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[0];

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 露娜·查尔德 战吼：使一个随从获得潜行
    /// </summary>
    public class LunaChild : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (g, c, t) => { await t.addKeywords(g, KEYWORD.STEALTH, KEYWORD.HAS_STEALTH); },
                PileFlag.both | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 斯塔·萨菲亚 如果你的战场上同时存在桑尼·缪尔可和露娜·查尔德，则你的所有随从获得潜行
    /// </summary>
    public class StarSphere : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            //TODO:潜行光环
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 酒之妖精 战吼：将一个友方随从置入你的手牌
    /// </summary>
    public class BeerFairy : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect((game, card, target) => { return target.fieldToHand(game); },
                PileFlag.self | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 太阳花领军 你的其他妖精的攻击力+2
    /// </summary>
    public class SunflowerWarleader : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            //new THHEffect<THHPlayer.ActiveEventArg>(PileName.FIELD,(game,player,card,arg)=>
            //{
            //    return true;
            //},(game,player,card,targets)=>
            //{
            //    if(targets[0] is Card target && target.owner == player && target.pile.name == PileName.FIELD)
            //        return true;
            //    return false;
            //},async (game,player,card,arg,targets)=>
            //{
            //    THHPlayer opponent = game.getOpponent(arg.player);
            //    if(opponent.field.count>0)
            //    {
            //        await opponent.field.randomTake(game,1).damage(game,1);
            //    }
            //})
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 琪露诺 战吼：随机冰冻3个角色（包括自己！）
    /// </summary>
    public class Cirno : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 5;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectOn<HearthStoneRule.ActiveEventArg>(PileFlag.field, (game, card, arg) => { return true; },
                (game, card, targets) => { return false; }, async (game, card, arg, targets) =>
                {
                    foreach (var character in game.getAllCharacters().randomTake(game, 3))
                    {
                        character.setFreeze(game, true);
                    }
                })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 大妖精 冲锋，亡语：你的对手获得一个法力水晶
    /// </summary>
    public class Daiyousei : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.CHARGE };
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            new DeathRattle(ID, async (game, card, arg) =>
                {
                    Player opponent = game.getOpponent(card.owner as Player);
                    await opponent.setMaxGem(game, opponent.getMaxGem() + 1);
                    await opponent.setGem(game, opponent.getGem() + 1);
                })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 琪露诺（冰瀑） 战吼：冰冻目标随从两侧的随从
    /// </summary>
    public class Cirno_IceFall : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 5;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                foreach (Card t in target.getNearbyCards())
                {
                    await t.setFreeze(game, true);
                }
            }, PileFlag.both | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 克劳恩皮丝 战吼：所有随从的攻击力+2
    /// </summary>
    public class Clownpiece : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectOn<HearthStoneRule.ActiveEventArg>(PileFlag.field, (game, card, arg) => { return true; },
                (game, card, targets) => { return false; }, async (game, card, arg, targets) =>
                {
                    foreach (var servant in game.getAllServants())
                    {
                        await servant.addBuff(game, new ClownpieceBuff());
                    }
                })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    public class ClownpieceBuff : Buff
    {
        public const int ID = Clownpiece.ID;
        public override int id { get; } = ID;

        private readonly PropModifier[] modifiers1 = new PropModifier[]
        {
            new AttackModifier(2)
        };

        public override PropModifier[] getPropertyModifiers(CardEngine game)
        {
            return modifiers1;
        }

        private readonly IPassiveEffect[] effects1;

        public override IPassiveEffect[] getEffects(CardEngine game)
        {
            return effects1;
        }

        public override int instanceID
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }


        public override Buff clone()
        {
            return new ClownpieceBuff();
        }
    }

    /// <summary>
    /// 森之妖精 每当一个妖精阵亡后，花费-1
    /// </summary>
    public class ForestFairy : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x018;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 8;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            //new THHEffect<THHPlayer.ActiveEventArg>(PileName.FIELD,(game,player,card,arg)=>
            //{
            //    return true;
            //},(game,player,card,targets)=>
            //{
            //    return false;
            //},async (game,player,card,arg,targets)=>
            //{
            //    foreach (var servant in game.getAllServants())
            //    {
            //        servant.addBuff(game, new ClownpieceBuff());
            //    }
            //})
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 爱塔尼媞·拉尔瓦 战吼：使一个妖精获得剧毒
    /// </summary>
    public class EternityLarva : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x019;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 2;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectOn<HearthStoneRule.ActiveEventArg>(PileFlag.field, (game, card, arg) => { return true; },
                (game, card, targets) =>
                {
                    if (targets[0] is Card target && target.pile.name == PILE.FIELD && target.hasTag(game, TAG.FAIRY))
                        return true;
                    return false;
                }, (game, card, arg, targets) =>
                {
                    if (targets[0] is Card target)
                    {
                        target.setPoisonous(game, true);
                    }

                    return Task.CompletedTask;
                })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 莉莉·怀特 战吼：你和你的对手的英雄恢复4点生命值，抽2张牌
    /// </summary>
    public class LilyWhite : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x020;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectOn<HearthStoneRule.ActiveEventArg>(PileFlag.field, (game, card, arg) => { return true; },
                (game, card, targets) => { return false; }, async (game, card, arg, targets) =>
                {
                    await game.players.Select(p => p.getMaster()).heal(game, card, 4);
                    foreach (var p in game.players)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            await p.draw(game);
                        }
                    }
                })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 121 大妖精 速攻
    /// </summary>
    public class Daiyousei_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x021;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.RUSH };
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            //new DeathRattle(ID, (g1, c1, p) => { return c1.getOwner().setGem(g1, c1.getOwner().getGem() + 1); })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 121 琪露诺 战吼：冻结一个敌方角色。
    /// </summary>
    public class Cirno_N_YYM : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x022;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) => { await target.setFreeze(game, true); },
                PileFlag.oppo | PileFlag.character)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 222 琪露诺 战吼：冻结目标随从相邻的随从。
    /// </summary>
    public class Cirno_N_HMX : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x023;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                foreach (Card t in target.getNearbyCards())
                {
                    await t.setFreeze(game, true);
                }
            }, PileFlag.both | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 213 蕾蒂 双方使用的第一张牌法力值消耗增加1点。
    /// </summary>
    public class LettyWhiterock_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x024;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;

        private IEffect[] effects1 = new IEffect[]
        {
            new ConditionalPassiveEffect(
                new Halo(new CodeBuff(ID, new CostModifier(1)), PileFlag.both | PileFlag.hand),
                (g, c, b) => g.triggers.getRecordedEvents()
                    .skipUntilLast(e => e is HearthStoneRule.TurnStartEventArg)
                    .OfType<HearthStoneRule.UseEventArg>()
                    .Where(u => u.card.pile.name != PILE.SKILL).Count() < 1)

        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 212 橙 战吼：获得+2攻击力和突袭
    /// </summary>
    public class Chen_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x025;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 2;
        public override string[] tags { get; set; } = new string[] { TAG.BEAST };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(async (game, card) =>
            {
                await card.addKeywords(game, KEYWORD.RUSH);
                await card.addBuff(game, new CodeBuff(ID, new AttackModifier(2)));
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 333 小恶魔 战吼：随机将一张墓地中的法术并置入你的手牌。
    /// </summary>
    public class Koakuma_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x026;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { TAG.DEMON };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            Player player = card.getOwner();
            if (!player.getHand().isFull)
                await player.getGrave().moveTo(game, player.getGrave().Where(c => c.isSpell()).random(game), player.getHand());
            // await card.getOwner().reborn(game, player.grave.Where(c => c.isSpell()).random(game), player.hand);
        }
    }

    /// <summary>
    /// 324 爱丽丝·玛格特洛依德 唯一，当你的回合开始时，召唤一个1/1的人形。
    /// </summary>
    public class AliceMargatroid_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x027;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new AfterTurnStart(PileFlag.field, async (game, card, arg) =>
            {
                await card.getOwner().createToken(game, game.getDefine<Doll_N>());
            }, (game, card, arg) => arg.player == card.getOwner())
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 衍生物 111 人形
    /// </summary>
    public class Doll_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x028;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override bool isToken { get; set; } = true;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };
    }

    /// <summary>
    /// 433 帕秋莉·诺蕾姬 唯一，战吼：发现两张元素法术，将它们融合并置入手牌。
    /// </summary>
    public class PatchouliKnowledge_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x029;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 3;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        public static async Task effect(CardEngine game, Card card)
        {
            var elementCards = Patchouli.getElementCards(game);
            var selectedCard1 = await card.getOwner().chooseOne(game, elementCards, isShow: false, title: "投入魔法");
            var selectedCard2 = await card.getOwner()
                .chooseOne(game, elementCards.Where(define => define != selectedCard1.choose), isShow: false, title: "投入魔法");
            var result = Patchouli.getMixCards(game, selectedCard1.choose, selectedCard2.choose);
            await card.getOwner().discover(game, result, title: "元素融合");
        }
    }

    /// <summary>
    /// 423 莉莉白 当你的回合开始时，使你的其它随从获得+1/+1。
    /// </summary>
    public class LilyWhite_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x030;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { TAG.FAIRY };

        private IEffect[] effects1 = new IEffect[]
        {
            new AfterTurnStart(PileFlag.field, async (game, card, arg) =>
            {
                foreach (var servant in card.getOwner().getField().Where(c => c != card))
                    await servant.addBuff(game, new CodeBuff(ID, new AttackModifier(1), new LifeModifier(1)));
            }, (game, card, arg) => { return arg.player == card.getOwner(); })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 534 魂魄妖梦 突袭，风怒。
    /// </summary>
    public class KonpakuYoumu_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x031;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.RUSH, KEYWORD.WINDFURY };
    }

    /// <summary>
    /// 646 蕾米莉亚·斯卡蕾特 唯一，战吼：弃三张牌，抽三张牌。
    /// </summary>
    public class RemiliaScarlet_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x032;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 6;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await card.getOwner().randomDiscard(game, 3);
            for (int i = 0; i < 3; i++)
            {
                await card.getOwner().draw(game);
            }
        }
    }

    /// <summary>
    /// 6/6/4 芙兰朵露·斯卡蕾特 唯一，战吼：弃一张牌，随机消灭一个敌方随从。
    /// </summary>
    public class FlandreScarlet_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x033;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 6;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await card.getOwner().randomDiscard(game, 1);
            await game.getOpponent(card.getOwner()).getField().random(game).setDead(game, true);
        }
    }

    /// <summary>
    /// 6/3/1 十六夜咲夜 战吼：抽一张牌，使其本回合法力值消耗变为0点。
    /// </summary>
    public class IzayoiSakuya_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x037;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 1;

        public override string[] keywords { get; set; } = new[] { KEYWORD.RUSH };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await card.getOwner().draw(game, out var draw);
            var drawed = draw.card;
            await drawed.addBuff(game, new CodeBuff(ID, new CostModifier(0, true),
                new RemoveBuffBefore<HearthStoneRule.TurnEndEventArg>(PileFlag.hand, ID)));
        }
    }

    /// <summary>
    /// 7/4/5 八云蓝 突袭，战吼：召唤橙，并使其获得+3/+1和突袭。
    /// </summary>
    public class YakumoRan_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x038;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 7;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 5;
        public override string[] keywords { get; set; } = new[] { KEYWORD.RUSH };
        public override string[] tags { get; set; } = new string[] { TAG.BEAST };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            var createToken =
                await card.getOwner().createToken(game, game.getDefine<Chen_N>(), card.pile.indexOf(card) + 1);
            var chen = createToken.card;
            await chen.addBuff(game, new CodeBuff(ID,
                new AttackModifier(3),
                new LifeModifier(1)));
            await chen.addKeywords(game, KEYWORD.RUSH);
        }
    }

    /// <summary>
    /// 8/8/8 西行寺幽幽子 唯一，战吼：吃掉一个随从。
    /// </summary>
    public class SaigyoujiYuyuko_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x039;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 8;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect((game, card, target) => { return target.setDead(game, true); },
                PileFlag.both | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 10/8/8 紫妈 战吼：将其它随从移回其拥有者的手牌。
    /// </summary>
    public class YakumoYukari_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x040;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            foreach (var player in game.players)
            {
                await player.getField().Where(c => c != card).fieldToHand(game);
            }
        }
    }

    /// <summary>
    /// 122 露米娅 潜行，使用骰子决定攻击目标
    /// </summary>
    public class Rumia_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x041;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.STEALTH };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectBefore<THHCard.AttackEventArg>(PileFlag.field, (g, c,b, a) => { return a.card == c; },
                (g, c,b, a) =>
                {
                    var oppo = g.getOpponent(c.getOwner());
                    Card[] targets = (new Card[] {oppo.getMaster()}).Concat(oppo.getField()).ToArray();
                    int dice = g.dice(targets.Length);
                    a.target = targets[dice - 1];
                    return Task.CompletedTask;
                })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 露娜萨·普莉兹姆利巴 622 战吼：乐队集合！（无论其他成员在哪里！）
    /// </summary>
    public class LunasaPrismriver_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x042;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect((g1, c1) => { return bandAssemble(g1, c1.getOwner(), c1.pile.indexOf(c1)); })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        public static async Task bandAssemble(CardEngine game, Player player, int position)
        {
            var lunasa = player.getField().FirstOrDefault(c => c.define.id == LunasaPrismriver_N.ID);
            if (lunasa == null)
            {
                lunasa = player.findCardsWherever(game, c => c.define.id == LunasaPrismriver_N.ID,
                    PileFlag.self | PileFlag.hand | PileFlag.grave | PileFlag.deck).random(game);
                if (lunasa != null)
                    await player.tryEnterField(game, lunasa.pile, lunasa, position);
            }

            var merlin = player.getField().FirstOrDefault(c => c.define.id == MerlinPrismriver_N.ID);
            if (merlin == null)
            {
                merlin = player.findCardsWherever(game, c => c.define.id == MerlinPrismriver_N.ID,
                    PileFlag.self | PileFlag.hand | PileFlag.grave | PileFlag.deck).random(game);
                if (merlin != null)
                    await player.tryEnterField(game, merlin.pile, merlin, lunasa.pile.indexOf(lunasa) + 1);
            }

            var lyrica = player.getField().FirstOrDefault(c => c.define.id == LyricaPrismriver_N.ID);
            if (lyrica == null)
            {
                lyrica = player.findCardsWherever(game, c => c.define.id == LyricaPrismriver_N.ID,
                    PileFlag.self | PileFlag.hand | PileFlag.grave | PileFlag.deck).random(game);
                if (lyrica != null)
                    await player.tryEnterField(game, lyrica.pile, lyrica, merlin.pile.indexOf(merlin) + 1);
            }
        }
    }

    /// <summary>
    /// 梅露兰·普莉兹姆利巴 612 嘲讽，战吼：乐队集合！（无论其他成员在哪里！）
    /// </summary>
    public class MerlinPrismriver_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x043;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect((g1, c1) =>
            {
                return LunasaPrismriver_N.bandAssemble(g1, c1.getOwner(), c1.pile.indexOf(c1));
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 莉莉卡·普莉兹姆利巴 621 圣盾，战吼：乐队集合！（无论其他成员在哪里！）
    /// </summary>
    public class LyricaPrismriver_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x044;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SHIELD };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect((g1, c1) =>
            {
                return LunasaPrismriver_N.bandAssemble(g1, c1.getOwner(), c1.pile.indexOf(c1));
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    //永远亭系列

    /// <summary>
    /// 莉格露·奈特巴格 1/1/2 亡语：将一个小虫置入手牌
    /// </summary>
    public class WriggleNightbug_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x045;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.DEATHRATTLE };
        public override string[] tags { get; set; } = { TAG.BEAST };

        private IEffect[] effects1 = new IEffect[]
        {
            new DeathRattle(ID,
                async (g1, c1, p) =>
                {
                    await c1.getOwner().addRandomCardToHand(g1,
                        g1.getDefines(new DefineReference[]
                        {
                            new DefineReference(0, Beetle_N.ID),
                            new DefineReference(0, Moth_N.ID),
                            new DefineReference(0, Spider_N.ID)
                        }));
                })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 甲虫（莉格露·奈特巴格） 1/1/1 嘲讽
    /// </summary>
    public class Beetle_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x046;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = { TAG.BEAST, TAG.BUG };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };

        private IEffect[] effects1 = new IEffect[] { };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 飞蛾（莉格露·奈特巴格） 1/1/1 突袭
    /// </summary>
    public class Moth_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x047;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = { TAG.BEAST, TAG.BUG };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.RUSH };

        private IEffect[] effects1 = new IEffect[] { };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 蜘蛛（莉格露·奈特巴格） 1/1/1 剧毒
    /// </summary>
    public class Spider_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x048;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = { TAG.BEAST, TAG.BUG };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.POISONOUS };

        private IEffect[] effects1 = new IEffect[] { };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 米斯蒂娅·萝拉蕾 2/2/2 战吼：直到你的回合开始，使目标随从的攻击力变为1
    /// </summary>
    public class MystiaLorelei_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x049;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;

        public override string[] tags { get; set; } = { TAG.BEAST };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (g, c, t) =>
            {
                await t.addBuff(g, new CodeBuff(ID, new AttackModifier(1, true),
                    new RemoveBuffAfter<HearthStoneRule.TurnStartEventArg>(PileFlag.field,
                        (g1, c1, b,a) => { return a.player == c.getOwner(); }, ID)));
            }, PileFlag.both | PileFlag.field)
        };

        //  public override string[] keywords { get; set; } = new string[] { KEYWORD.POISONOUS };
        public override IEffect[] getEffects()
        {
            return effects1;
        }

        //  public override string[] keywords { get; set; } = new string[] { KEYWORD.POISONOUS };
        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 铃仙·优昙华院·因幡 5/4/4 战吼：对敌方英雄造成2点伤害2次
    /// </summary>
    public class ReisenUdongeinInaba_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x050;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = { TAG.BEAST };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        //public override string[] tags { get; set; } = { TAG.BEAST };
        //  public override string[] keywords { get; set; } = new string[] { KEYWORD.POISONOUS };
        public override IEffect[] getEffects()
        {
            return effects1;
        }

        //public override string[] tags { get; set; } = { TAG.BEAST };
        //  public override string[] keywords { get; set; } = new string[] { KEYWORD.POISONOUS };
        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            for (int i = 0; i < 2; i++)
            {
                await game.getOpponent(card.getOwner()).getMaster().damage(game, card, 2);
            }
        }
    }

    /// <summary>
    /// 因幡帝 3/4/2 战吼：发现一张牌库里的牌，将它置顶
    /// </summary>
    public class TewiInaba_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x051;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 2;
        public override string[] tags { get; set; } = { TAG.BEAST };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            var discover = await card.getOwner().searchPile(game, card.getOwner().getDeck(), false, 3, "选择一张牌置于牌组顶");
            Card c = discover.choose;
            await card.getOwner().getDeck().moveTo(game, c, card.getOwner().getDeck(),
                card.getOwner().getDeck().indexOf(card.getOwner().getDeck().top));
        }
    }

    /// <summary>
    /// 上白泽慧音A 3/2/4 当你的回合结束时若位于手牌中则变形为半兽。战吼:将一张墓地中花费最高的牌洗入牌库。
    /// </summary>
    public class KamishirasawaKeine_Human_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x052;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 4;

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect),
            new THHEffectAfter<HearthStoneRule.TurnEndEventArg>(PileFlag.hand,
                (g, c, a) => { return a.player == c.getOwner() && c.pile == c.getOwner().getHand(); },
                async (g, c, a) => { await THHCard.setDefine(c, g, g.getDefine<KamishirasawaKeine_Beast_N>()); })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            Player player = card.getOwner();
            var topCostCard = player.getGrave().OrderByDescending(c => c.getCost(game)).FirstOrDefault();
            if (topCostCard != null)
                await player.getGrave().moveTo(game, topCostCard, player.getDeck(), game.randomInt(0, player.getDeck().count));
        }
    }


    /// <summary>
    /// 上白泽慧音B 4/4/4 当你的回合结束时若位于手牌中则变形为半人。战吼:将三张墓地中花费最低的牌送进墓地。
    /// </summary>
    public class KamishirasawaKeine_Beast_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x053;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 4;
        public override bool isToken { get; set; } = true;

        public override string[] tags { get; set; } = { TAG.BEAST };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect),
            new THHEffectAfter<HearthStoneRule.TurnEndEventArg>(PileFlag.hand,
                (g, c, a) => { return a.player == c.getOwner() && c.pile == c.getOwner().getHand(); },
                async (g, c, a) => { await THHCard.setDefine(c, g, g.getDefine<KamishirasawaKeine_Human_N>()); })
        };

        //  public override string[] keywords { get; set; } = new string[] { KEYWORD.POISONOUS };
        public override IEffect[] getEffects()
        {
            return effects1;
        }

        //  public override string[] keywords { get; set; } = new string[] { KEYWORD.POISONOUS };
        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            Player player = card.getOwner();
            var lowCostCards = player.getDeck().OrderBy(c => c.getCost(game)).Take(3);
            foreach (var c in lowCostCards)
            {
                await player.getDeck().moveTo(game, c, player.getGrave(), game.randomInt(0, player.getGrave().count));
            }
        }
    }

    /// <summary>
    /// 藤原妹红 8/7/3 亡语：并对所有随从造成2点伤害，返回牌库。
    /// </summary>
    public class HuziwaraNoMokou_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x054;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 8;
        public override int attack { get; set; } = 7;
        public override int life { get; set; } = 3;

        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE, KEYWORD.RUSH, KEYWORD.DEATHRATTLE };

        private IEffect[] effects1 = new IEffect[]
        {
            new DeathRattle(ID, async (game, card, arg) =>
            {
                await game.getAllServants().damage(game, card, 2);
                await card.graveToDeck(game, true);
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    // 伪抉择：发现提供两张假卡作为选项，然后根据选项触发效果

    /// <summary>
    /// 八意永琳 10/8/8 部署：选择一个英雄，然后选择一项：使她剩余生命值翻倍；或使她剩余生命值减半
    /// </summary>
    public class YagokoroEirin_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x055;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (g, c, t) =>
            {
                CardDefine[] options = g.getDefines().Where(c1 => c1 is GameOfLife_Rise_N ||
                                                                c1 is GameOfLife_Fall_N ).ToArray();
                var choice = await c.getOwner().chooseOne(g,options);
                switch (choice.choose.id)
                {
                    case GameOfLife_Rise_N.ID:
                        if (t.getCurrentLife(g) <= t.getLife(g) / 2)
                        {
                            await t.setCurrentLife(g, t.getCurrentLife(g) * 2);
                        }
                        else
                        {
                            await t.setCurrentLife(g, t.getLife(g));
                        }
                        break;
                    case GameOfLife_Fall_N.ID:
                        await t.setCurrentLife(g, t.getCurrentLife(g) / 2);
                        break;
                }
            }, PileFlag.both | PileFlag.master)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 生命游戏-苏活：假卡，无实际效果。
    /// 使其剩余生命值翻倍
    /// </summary>
    public class GameOfLife_Rise_N : SpellCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SPELL | 0x056;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override bool isToken { get; set; } = true;

        private IEffect[] effects1 = new IEffect[]
        {
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 生命游戏-衰亡：假卡，无实际效果。
    /// 使其剩余生命值翻倍
    /// </summary>
    public class GameOfLife_Fall_N : SpellCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SPELL | 0x057;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override bool isToken { get; set; } = true;

        private IEffect[] effects1 = new IEffect[]
        {
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 蓬莱山辉夜 10/5/10 部署：选择一项：每个玩家禁止抽牌两个回合；或每个玩家抽3张牌。
    /// </summary>
    public class HouraisanKaguya_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x058;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override int attack { get; set; } = 5;
        public override int life { get; set; } = 10;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine g, Card c)
        {
            CardDefine[] options = g.getDefines().Where(c1 => c1 is ReturnOfImperishableNight_Eternity_N ||
                                                                c1 is EndOfImperishableNight_Instant_N).ToArray();
            var choice = await c.getOwner().chooseOne(g, options, title: "选择一项");
            if (choice.choose is ReturnOfImperishableNight_Eternity_N)
            {
                await c.getOwner().getMaster().addBuff(g, new CodeBuff(ID,
                    new THHEffectBefore<HearthStoneRule.DrawEventArg>(
                        PileFlag.master,
                       (g1, c1, b1, a1) => { return a1.player == c1.getOwner(); },
                        async (g1, c1, b1, a1) => { a1.isCanceled = true; }),
                    new THHEffectBefore<HearthStoneRule.BurnEventArg>(
                       PileFlag.master,
                       (g1, c1, b1, a1) => { return a1.player == c1.getOwner(); },
                       async (g1, c1, b1, a1) => { a1.isCanceled = true; }),
                    new THHEffectBefore<HearthStoneRule.FatigueEventArg>(
                        PileFlag.master,
                        (g1, c1, b1, a1) => { return a1.player == c1.getOwner(); },
                        async (g1, c1, b1, a1) => { a1.isCanceled = true; }),
                    new RemoveBuffAfterTimeUp<HearthStoneRule.TurnEndEventArg>(PileFlag.master, 2,
                       (g1, c1, b1, a1) => { return a1.player == c1.getOwner(); })));

                await g.getOpponent(c.getOwner()).getMaster().addBuff(g, new CodeBuff(ID,
                    new THHEffectBefore<HearthStoneRule.DrawEventArg>(PileFlag.master,
                        (g1, c1, b1, a1) => { return a1.player == c1.getOwner(); },
                        async (g1, c1, b1, a1) => { a1.isCanceled = true; }),
                    new THHEffectBefore<HearthStoneRule.BurnEventArg>(
                        PileFlag.master,
                        (g1, c1, b1, a1) => { return a1.player == c1.getOwner(); },
                        async (g1, c1, b1, a1) => { a1.isCanceled = true; }),
                    new THHEffectBefore<HearthStoneRule.FatigueEventArg>(
                        PileFlag.master,
                        (g1, c1, b1, a1) => { return a1.player == c1.getOwner(); },
                        async (g1, c1, b1, a1) => { a1.isCanceled = true; }),
                    new RemoveBuffAfterTimeUp<HearthStoneRule.TurnEndEventArg>(PileFlag.master, 2,
                        (g1, c1, b1, a1) => { return a1.player == c1.getOwner(); })));
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    await c.getOwner().draw(g);
                }

                for (int i = 0; i < 5; i++)
                {
                    await g.getOpponent(c.getOwner()).draw(g);
                }
            }
        }
    }

    /// <summary>
    /// 永夜归-永恒：假卡，无实际效果。
    /// 
    /// </summary>
    public class ReturnOfImperishableNight_Eternity_N : SpellCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SPELL | 0x059;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override bool isToken { get; set; } = true;

        private IEffect[] effects1 = new IEffect[]
        {
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 永夜返-须臾：假卡，无实际效果。
    /// 
    /// </summary>
    public class EndOfImperishableNight_Instant_N : SpellCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SPELL | 0x060;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override bool isToken { get; set; } = true;

        private IEffect[] effects1 = new IEffect[]
        {
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 红美铃 407 嘲讽 当该随从受到伤害后攻击力+3
    /// </summary>
    public class HoanMeirin_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x061;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 7;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectAfter<THHCard.DamageEventArg>(PileFlag.field,
                (g, c, a) => { return c == a.target; }, (g, c, a) => { return c.addBuff(g, new CodeBuff(ID, new AttackModifier(3))); })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    //风

    /// <summary>
    /// 秋静叶
    /// </summary>
    public class AkiSizuha_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x062;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = { TAG.GOD };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectAfter<HearthStoneRule.TurnEndEventArg>(PileFlag.field,
                (game, card, arg) => { return arg.player == card.getOwner(); },
                async (game, card, arg) => { await game.getOpponent(card.getOwner()).addCardToHand<Neutral_73>(game); })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 秋穰子
    /// </summary>
    public class AkiMinoriko_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x063;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = { TAG.GOD };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectAfter<HearthStoneRule.TurnEndEventArg>(PileFlag.field,
                (game, card, arg) => { return arg.player == card.getOwner(); },
                async (game, card, arg) => { await card.getOwner().addCardToHand<Neutral_74>(game); })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 键山雏
    /// 部署:沉默相邻的友方随从
    /// </summary>
    public class KagiyamaHina_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x064;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = { TAG.GOD };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await card.getNearbyCards().silence(game);
        }
    }

    /// <summary>
    /// 河城荷取
    /// 护盾，潜行
    /// </summary>
    public class KawasiroNitori_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x065;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SHIELD, KEYWORD.STEALTH };

        private IEffect[] effects1 = new IEffect[] { };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }


    /// <summary>
    /// 犬走椛
    /// 嘲讽，部署:如果对手控制了至少2个随从，抽一张牌
    /// </summary>
    public class InubashiriMomizi_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x066;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 5;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            Player oppo = game.getOpponent(card.getOwner());
            if (oppo.getField().count >= 2)
            {
                await card.getOwner().draw(game);
            }
        }
    }

    /// <summary>
    /// 寒光天狗（划掉）射命丸文
    /// 突袭，部署:你和你的对手抽一张牌
    /// </summary>
    public class ShameimaruAya_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x067;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 6;
        public override int life { get; set; } = 3;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.CHARGE, KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await game.getOpponent(card.getOwner()).draw(game);
            await card.getOwner().draw(game);
        }
    }

    /// <summary>
    /// 洩矢诹访子
    /// </summary>
    public class MoriyaSuwako_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x068;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.RUSH, KEYWORD.UNIQUE, KEYWORD.DEATHRATTLE };
        public override string[] tags { get; set; } = { TAG.GOD };

        private IEffect[] effects1 = new IEffect[]
        {
            new DeathRattle(ID,
                async (game, card, p) =>
                {
                    await card.getOwner().createToken(game, game.getOpponent(card.getOwner()).getField().count,
                        game.getDefine<Frog_N>());
                })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 青蛙
    /// 衍生卡
    /// </summary>
    public class Frog_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x069;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.RUSH };
        public override string[] tags { get; set; } = { TAG.BEAST };
    }

    /// <summary>
    /// 早苗
    /// 抽牌，直到牌库顶的牌法力值消耗小于刚抽的牌才停下
    /// </summary>
    public class Sanae_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x070;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 7;
        public override int attack { get; set; } = 6;
        public override int life { get; set; } = 6;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };
        public override string[] tags { get; set; } = { TAG.GOD };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            Card top;
            Card draw;
            do
            {
                await card.getOwner().draw(game, out var arg);
                draw = arg.card;
                top = card.getOwner().getDeck().top;
            } while (top.getCost(game) > draw.getCost(game));
        }
    }

    /// <summary>
    /// 中立神妈
    /// </summary>
    public class YasakaKanako_N : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x071;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };
        public override string[] tags { get; set; } = { TAG.GOD };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            var you = card.getOwner();
            var cards = you.getHand().Where(c => c.getCost(game) >= 5);
            var enumerable = cards.ToList();
            foreach (var t in enumerable)
            {
                await card.getOwner().getHand().moveTo(game, t, card.getOwner().getDeck());
            }

            for (int i = 0; i < enumerable.Count; i++)
            {
                await card.getOwner().createToken(game, game.getDefine<Onbashira>());
            }
        }
    }


    /// <summary>
    /// 衍生物 枫叶
    /// </summary>
    public class Neutral_73 : SpellCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SPELL | 0x073;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;

        public override bool isToken { get; set; } = true;

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        //  public override string[] keywords { get; set; } = new string[] { KEYWORD._RevealDuel };
        public override IEffect[] getEffects()
        {
            return effects1;
        }

        //  public override string[] keywords { get; set; } = new string[] { KEYWORD._RevealDuel };
        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
        }
    }

    /// <summary>
    /// 红薯
    /// </summary>
    public class Neutral_74 : SpellCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SPELL | 0x074;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;

        public override bool isToken { get; set; } = true;

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(
                async (game, card, target) => { await target.heal(game, card, 2); }, PileFlag.both | PileFlag.field)
        };

        //  public override string[] keywords { get; set; } = new string[] { KEYWORD._RevealDuel };
        public override IEffect[] getEffects()
        {
            return effects1;
        }

        //  public override string[] keywords { get; set; } = new string[] { KEYWORD._RevealDuel };
        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 琪斯美 2/2/2
    /// 在对手使用一张花费大于或等于4的仆从后，将该仆从从你的手牌置入战场。
    /// </summary>
    public class Kisume : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x075;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = { KEYWORD.TAUNT };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectAfter<HearthStoneRule.UseEventArg>(PileFlag.hand,
                (game, card, arg) =>
                    card.getOwner() != arg.player && arg.cost >= 4 && arg.card.isServant(), (game, card, arg) => card.getOwner().tryEnterField(game, card.pile, card, card.getOwner().getField().count))
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 黑谷山女 2/1/4
    /// 该仆从对其他仆从造成伤害后，若受伤仆从仍然存活则将其生命值转变为1。
    /// </summary>
    public class KurodaniYamame : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x076;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = { TAG.BEAST };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectAfter<THHCard.DamageEventArg>(PileFlag.field,
                (game, card, arg) => arg.source == card && !arg.target.isDead(game) && !arg.target.isMaster(), onExecute)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        public static Task onExecute(CardEngine game, Card card, THHCard.DamageEventArg arg)
        {
            arg.target.setCurrentLife(game, 1);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 水桥帕露西 3/2/4
    /// 宣言：如果场上存在攻击力大于该仆从的角色，获得增益：攻击力+2
    /// </summary>
    public class MizuhashiParsee : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x077;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;

        public override int life { get; set; } = 4;

        private IEffect[] effects1 = new IEffect[]
        {
            // new NoTargetEffect((game, card) => card.addBuff(game, new GeneratedBuff(ID, new AttackModifier(2))),
            //     (game, card) => game.getAllServants()
            //         .Any(s => s.getAttack(game) > card.getAttack(game)))
            new NoTargetEffect((game, card) =>
            {
                if (game.getAllServants().Any(s => s.getAttack(game) > card.getAttack(game)))
                {
                    card.addBuff(game, new CodeBuff(ID, new AttackModifier(2)));
                }

                return Task.CompletedTask;
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 星熊勇仪 9/7/7
    /// 对手所控制的最高攻击力仆从每有一点攻击力，该仆从便减少一点花费。
    /// </summary>
    public class HoshigumaYugi : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x078;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 9;
        public override int attack { get; set; } = 7;

        public override int life { get; set; } = 7;

        private IEffect[] effects1 = new IEffect[]
        {
            // new IntPropFixer<Card.PropChangeEventArg>(PileFlag.hand,
            //     new IntPropFixer<Card.PropChangeEventArg>.ModifierFixer(new CostModifier(0),
            //         (game, card, buff, arg) =>
            //         {
            //             var oppoField = (game as THHGame).getOpponent(card.getOwner()).field;
            //             return -(oppoField.count > 0 ? oppoField.Select(s => s.getAttack(game)).Max() : 0);
            //         })),
            new EventTriggeredModifierFixer<int>(PileFlag.hand, new CostModifier(0), null,
                new EventTriggeredModifierFixer<int>.ModifierFixer<Card.PropChangeEventArg>(
                    (game, card, buff, arg) => { return arg.propName == nameof(ServantCardDefine.attack); },
                    (game, card, buff, arg) =>
                    {
                        var oppoField = (game as CardEngine).getOpponent(card.getOwner()).getField();
                        return -(oppoField.count > 0 ? oppoField.Select(s => s.getAttack(game)).Max() : 0);
                    }), new EventTriggeredModifierFixer<int>.ModifierFixer<Pile.MoveCardEventArg>(
                    (game, card, buff, arg) => arg.from?.name == PILE.FIELD || arg.to?.name == PILE.FIELD,
                    (game, card, buff, arg) =>
                    {
                        var oppoField = (game as CardEngine).getOpponent(card.getOwner()).getField();
                        return -(oppoField.count > 0 ? oppoField.Select(s => s.getAttack(game)).Max() : 0);
                    }))
        };

        // public override string[] keywords { get; set; } = {};
        // public override string type { get; set; }
        // public override bool isToken { get; set; }
        // public override bool isActive { get; set; }
        // public override int spellDamage { get; set; }
        // public override int damageReduce { get; set; }
        // public override string[] tags { get; set; }

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        // public override string[] keywords { get; set; } = {};
        // public override string type { get; set; }
        // public override bool isToken { get; set; }
        // public override bool isActive { get; set; }
        // public override int spellDamage { get; set; }
        // public override int damageReduce { get; set; }
        // public override string[] tags { get; set; }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 古明地觉 3/1/4
    /// 宣言：从对手上个回合内使用过的三张随机牌中选择一张并将其复制置入你的手牌。
    /// </summary>
    public class KomeijiSatori : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x079;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 1;

        public override int life { get; set; } = 4;

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            var recorded = game.triggers.getRecordedEvents();
            var usedCards = new List<Card>();
            bool count = false;
            for (int i = recorded.Length - 1; i >= 0; i--)
            {
                if (!count)
                {
                    if (recorded[i] is HearthStoneRule.TurnEndEventArg)
                    {
                        count = true;
                    }
                }
                else
                {
                    if (recorded[i] is HearthStoneRule.UseEventArg arg &&
                        !(arg.card.isSkill()))
                    {
                        usedCards.Add(arg.card);
                    }

                    if (recorded[i] is HearthStoneRule.TurnEndEventArg)
                    {
                        break;
                    }
                }
            }

            await card.getOwner().discover(game, usedCards.Select(c => c.define), title: "想起【......】");

        }
    }

    /// <summary>
    /// 火焰猫燐 3/2/3
    /// 宣言：本回合内每死亡过一个仆从，便获得增益：攻击力+1
    /// </summary>
    public class KaenbyouRin : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x080;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;

        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = { TAG.BEAST };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            var recorded = game.triggers.getRecordedEvents();
            int count = 0;
            for (int i = recorded.Length - 1; i > 0; i--)
            {
                if (recorded[i] is THHCard.DeathEventArg)
                {
                    count++;//+= (recorded[i] as THHCard.DeathEventArg).infoDic.Count;
                }

                if (recorded[i] is HearthStoneRule.TurnStartEventArg)
                {
                    break;
                }
            }

            await card.addBuff(game, new CodeBuff(ID, new AttackModifier(count)));
        }
    }

    /// <summary>
    /// 灵乌路空 6/6/6
    /// 无法攻击，每当你的回合结束时对其他所有角色造成3点伤害。
    /// </summary>
    public class ReiujiUtsuho : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x081;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 6;
        public override int life { get; set; } = 6;
        public override string[] keywords { get; set; } = { KEYWORD.CANTATTACK };
        public override string[] tags { get; set; } = { TAG.BEAST, TAG.GOD };

        private IEffect[] effects1 = new IEffect[]
        {
            new BeforeYourTurnEnd(PileFlag.field, (game, card,buff, arg) =>
            {
                var targets = game.getAllCharacters().ToList();
                targets.Remove(card);
                targets.damage(game, card, 3);
                return Task.CompletedTask;
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }

    /// <summary>
    /// 古明地恋 5/1/4
    /// 唯一，永久隐匿，所有目标选择都会改为选择该仆从以外的随机目标。
    /// </summary>
    public class KomeijiKoishi : ServantCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NEUTRAL | CATEGORY.SERVANT | 0x082;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 1;

        public override int life { get; set; } = 4;

        public override string[] keywords { get; set; } = { KEYWORD.PERMANENTSTEALTH, KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            // new THHEffectAfter<THHCard.DamageEventArg>(PileFlag.field,
            //     (game, card, arg) => arg.infoDic.Values.Select(info => info.source).Contains(card), null,
            //     async (game, card, arg) => card.setStealth(game, true)),
            new THHEffectBefore<HearthStoneRule.UseEventArg>(PileFlag.field,
                (game, card, arg) => arg.targets.Length > 0,
                (game, card, arg) =>
                {
                    var count = arg.targets.Count();
                    var availableTargets = arg.card.getAvaliableTargets(game).Where(c => c != card);
                    var newTargets = availableTargets.randomTake(game, count).ToArray();
                    arg.setVar(HearthStoneRule.UseEventArg.VAR_TARGETS,newTargets);
                    return Task.CompletedTask;
                }),
            new THHEffectBefore<THHCard.AttackEventArg>(PileFlag.field,
                null,
                (game, card, arg) =>
                {
                    var oppo = game.getOpponent(arg.card.getOwner());
                    var availableTargets = oppo.getField().ToList().Append(oppo.getMaster())
                        .Where(c => !c.isStealth(game)).ToList();
                    if (availableTargets.Contains(card))
                    {
                        availableTargets.Remove(card);
                    }

                    arg.target = availableTargets.random(game);
                    return Task.CompletedTask;
                })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
}