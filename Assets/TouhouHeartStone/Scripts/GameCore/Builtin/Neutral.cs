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
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override string[] keywords { get; set; } = new string[] { Keyword.CHARGE };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 人类村落守卫，325，嘲讽
    /// </summary>
    public class HumanVillageGuard : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { Keyword.TAUNT };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 铁炮猎人 中立普通卡 322 战吼：造成2点伤害
    /// </summary>
    public class RifleHunter : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((game,card,target)=>
            {
                return target.damage(game,card,2);
            })
        };
    }
    /// <summary>
    /// 迷失亡灵 中立普通卡 541 亡语：召唤一个4/3的亡灵
    /// </summary>
    public class MissingSpecter : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID,(g1,c1,p)=>
            {
                return c1.getOwner().createToken(g1,g1.getCardDefine<LostSpecter>(),p);
            })
        };
    }
    /// <summary>
    /// 失落亡灵 中立衍生物 543
    /// </summary>
    public class LostSpecter : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x005;
        public override int id { get; set; } = ID;
        public override bool isToken { get; set; } = true;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 3;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 梅雨元凶 法强+1
    /// </summary>
    public class DrizzleFairy : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 2;
        public override int spellDamage { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    public class LuckyCoin : SpellCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect((game,card)=>
            {
                return card.getOwner().setGem(game,card.getOwner().gem + 1);
            })
        };
    }
    /// <summary>
    /// 炎之妖精 战吼：对一个随机敌方随从造成1点伤害
    /// </summary>
    public class FlameFairy : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.FIELD,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async (game,card,arg,targets)=>
            {
                THHPlayer opponent = game.getOpponent(arg.player);
                if(opponent.field.count>0)
                {
                    await opponent.field.randomTake(game,1).damage(game,card,1);
                }
            })
        };
    }
    /// <summary>
    /// 桑尼·缪尔可 潜行
    /// </summary>
    public class SunnyMilk : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { Keyword.STEALTH };
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 露娜·查尔德 战吼：使一个随从获得潜行
    /// </summary>
    public class LunaChild : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.FIELD,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                if(targets[0] is Card target && target.pile.name == PileName.FIELD)
                    return true;
                return false;
            },(game,card,arg,targets)=>
            {
                if(targets[0] is Card target)
                    target.setStealth(true);
                return Task.CompletedTask;
            })
        };
    }
    /// <summary>
    /// 斯塔·萨菲亚 如果你的战场上同时存在桑尼·缪尔可和露娜·查尔德，则你的所有随从获得潜行
    /// </summary>
    public class StarSphere : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            //TODO:潜行光环
        };
    }
    /// <summary>
    /// 酒之妖精 战吼：将一个友方随从置入你的手牌
    /// </summary>
    public class BeerFairy : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((game,card,target)=>
            {
                return target.backToHand(game);
            }, PileFlag.self | PileFlag.field)
        };
    }
    /// <summary>
    /// 太阳花领军 你的其他妖精的攻击力+2
    /// </summary>
    public class SunflowerWarleader : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
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
    }
    /// <summary>
    /// 琪露诺 战吼：随机冰冻3个角色（包括自己！）
    /// </summary>
    public class Cirno : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 5;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.FIELD,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async (game,card,arg,targets)=>
            {
                foreach (var character in game.getAllCharacters().randomTake(game,3))
                {
                    //character.setFreeze(true);
                }
            })
        };
    }
    /// <summary>
    /// 大妖精 冲锋，亡语：你的对手获得一个法力水晶
    /// </summary>
    public class Daiyousei : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { Keyword.CHARGE };
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<THHCard.DeathEventArg>(PileName.GRAVE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async (game,card,arg)=>
            {
                THHPlayer opponent = game.getOpponent(card.owner as THHPlayer);
                await opponent.setMaxGem(game, opponent.maxGem + 1);
                await opponent.setGem(game, opponent.gem + 1);
            })
        };
    }
    /// <summary>
    /// 琪露诺（冰瀑） 战吼：冰冻目标随从两侧的随从
    /// </summary>
    public class Cirno_IceFall : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 5;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((g,c,t)=>
            {
                t.getNearbyCards().setFreeze(true);
                return Task.CompletedTask;
            }, PileFlag.both| PileFlag.field)
        };
    }
    /// <summary>
    /// 克劳恩皮丝 战吼：所有随从的攻击力+2
    /// </summary>
    public class Clownpiece : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.FIELD,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async (game,card,arg,targets)=>
            {
                foreach (var servant in game.getAllServants())
                {
                    servant.addBuff(game, new ClownpieceBuff());
                }
            })
        };
    }
    public class ClownpieceBuff : Buff
    {
        public const int ID = Clownpiece.ID;
        public override int id { get; } = ID;
        public override PropModifier[] modifiers { get; } = new PropModifier[]
        {
            new AttackModifier(2)
        };
        public override IPassiveEffect[] effects { get; }
        public override int instanceID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x018;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 8;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
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
    }
    /// <summary>
    /// 爱塔尼媞·拉尔瓦 战吼：使一个妖精获得剧毒
    /// </summary>
    public class EternityLarva : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x019;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 2;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.FIELD,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                if(targets[0] is Card target && target.pile.name == PileName.FIELD && target.hasTag(game,CardTag.FAIRY))
                    return true;
                return false;
            },async (game,card,arg,targets)=>
            {
                if(targets[0] is Card target)
                {
                    //target.setPoision(true);
                }
            })
        };
    }
    /// <summary>
    /// 莉莉·怀特 战吼：你和你的对手的英雄恢复4点生命值，抽2张牌
    /// </summary>
    public class LilyWhite : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x020;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.FIELD,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async (game,card,arg,targets)=>
            {
                await game.players.Select(p=>p.master).heal(game,4);
                foreach (var p in game.players)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        await p.draw(game);
                    }
                }
            })
        };
    }

    /// <summary>
    /// 111 大妖精 突袭，亡语：在本回合获得一个法力水晶
    /// </summary>
    public class Daiyousei_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x021;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { Keyword.RUSH };
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID,(g1,c1,p)=>
            {
                return c1.getOwner().setGem(g1, c1.getOwner().gem + 1);
            })
        };
    }
    /// <summary>
    /// 121 琪露诺 战吼：冻结一个敌方角色。
    /// </summary>
    public class Cirno_N_YYM : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x022;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((game,card,target)=>
            {
                target.setFreeze(true);
                return Task.CompletedTask;
            }, PileFlag.oppo | PileFlag.character)
        };
    }
    /// <summary>
    /// 222 琪露诺 战吼：冻结目标随从相邻的随从。
    /// </summary>
    public class Cirno_N_HMX : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x023;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((game,card,target)=>
            {
                target.getNearbyCards().setFreeze(true);
                return Task.CompletedTask;
            }, PileFlag.both | PileFlag.field)
        };
    }
    /// <summary>
    /// 214 蕾蒂 双方使用的第一张牌法力值消耗增加1点。
    /// </summary>
    public class LettyWhiterock_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x024;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new ConditionalPassiveEffect(new Halo(new GeneratedBuff(ID,new CostModifier(1)), PileFlag.both | PileFlag.hand),
                (g,c,b)=>g.triggers.getRecordedEvents().skipUntilLast(e=>e is THHGame.TurnStartEventArg).OfType<THHPlayer.UseEventArg>().Count()<1)
            //new THHEffectBefore<THHGame.TurnStartEventArg>(PileName.FIELD,null,null,async (g,c,a)=>
            //{
            //    foreach (var player in g.players)
            //    {
            //        foreach (var card in player.hand)
            //        {
            //            await card.addBuff(g,new GeneratedBuff(ID,new CostModifier(1),
            //                new THHEffectAfter<THHPlayer.UseEventArg>(PileName.HAND,(g2,c2,a2)=>
            //                {
            //                    return a2.player == c2.getOwner();
            //                },null,async (g3,c3,a3)=>
            //                {
            //                    await c3.removeBuff(g3,ID);
            //                    await c3.getOwner().hand.removeBuff(g3,ID);
            //                })));
            //        }
            //    }
            //})
        };
        public class ConditionalPassiveEffect : PassiveEffect
        {
            Func<THHGame, Card, Buff, bool> condition { get; }
            PassiveEffect effect { get; }
            public ConditionalPassiveEffect(PassiveEffect effect, Func<THHGame, Card, Buff, bool> condition)
            {
                this.condition = condition;
                this.effect = effect;
            }
            public override string[] piles => effect.piles;
            public override void onEnable(THHGame game, Card card, Buff buff)
            {
                Trigger<THHGame.TurnStartEventArg> enableTrig = new Trigger<THHGame.TurnStartEventArg>(arg =>
                {
                    if (condition == null || condition(game, card, buff))
                        effect.onEnable(game, card, buff);
                    return Task.CompletedTask;
                });
                card.setProp(nameof(ConditionalPassiveEffect) + GetHashCode() + "enableTrig", enableTrig);
                game.triggers.registerAfter(enableTrig);
                Trigger<THHPlayer.UseEventArg> disableTrig = new Trigger<THHPlayer.UseEventArg>(arg =>
                {
                    if (condition != null && !condition(game, card, buff))
                        effect.onDisable(game, card, buff);
                    return Task.CompletedTask;
                });
                card.setProp(nameof(ConditionalPassiveEffect) + GetHashCode() + "disableTrig", disableTrig);
                game.triggers.registerAfter(disableTrig);
                if (condition == null || condition(game, card, buff))
                    effect.onEnable(game, card, buff);
            }
            public override void onDisable(THHGame game, Card card, Buff buff)
            {
                game.triggers.removeAfter(card.getProp<Trigger<THHGame.TurnStartEventArg>>(game, nameof(ConditionalPassiveEffect) + GetHashCode() + "enableTrig"));
                game.triggers.removeAfter(card.getProp<Trigger<THHPlayer.UseEventArg>>(game, nameof(ConditionalPassiveEffect) + GetHashCode() + "disableTrig"));
                effect.onDisable(game, card, buff);
            }
        }
    }
    /// <summary>
    /// 212 橙 战吼：本回合获得+2攻击力和突袭
    /// </summary>
    public class Chen_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x025;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect((game,card)=>
            {
                return card.addBuff(game,new GeneratedBuff(ID,
                    new AttackModifier(2),
                    new KeywordsModifier(KeywordsModifier.ModifyType.add,Keyword.RUSH)));
            })
        };
    }
    /// <summary>
    /// 333 小恶魔 战吼：随机将一张墓地中的法术并置入你的手牌。
    /// </summary>
    public class Koakuma_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x026;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { CardTag.DEMON };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            THHPlayer player = card.getOwner();
            if (!player.hand.isFull)
                await player.grave.moveTo(game, player.grave.Where(c => c.isSpell()).random(game), player.hand);
        }
    }
    /// <summary>
    /// 324 爱丽丝·玛格特洛依德 当你的回合开始时，召唤一个1/1的人形。
    /// </summary>
    public class AliceMargatroid_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x027;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]{
            new BeforeTurnStart(async(game,card,arg)=>
            {
                await card.getOwner().createToken(game, game.getCardDefine<Doll_N>());
            },(g,c,a)=>
            {
                return a.player==c.getOwner();
            })
        };
    }
    /// <summary>
    /// 衍生物 111 人形
    /// </summary>
    public class Doll_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x028;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override bool isToken { get; set; } = true;
    }
    /// <summary>
    /// 433 帕秋莉·诺蕾姬 唯一，战吼：发现两张元素法术，将它们融合并置入手牌。
    /// </summary>
    public class PatchouliKnowledge_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x029;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 3;
        public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(async (g1,c1)=>
            {
                List<Card> elementCards = new List<Card>(g1.createCards(Patchouli.getElementCards(g1)));
                Card a = await c1.getOwner().discover(g1,elementCards,title:"选择一张元素法术作为融合素材");
                elementCards.Remove(a);
                Card b = await c1.getOwner().discover(g1,elementCards,title:"选择另一张元素法术作为融合素材");
                await Patchouli.tryMix(g1,c1.getOwner(),a,b);
            })
        };
    }
    /// <summary>
    /// 423 莉莉白 当你的回合开始时，使你的其它随从获得+1/+1。
    /// </summary>
    public class LilyWhite_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x030;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]{
            new BeforeTurnStart(async(game,card,arg)=>
            {
                foreach(var servant in card.getOwner().field.Where(c=>c!=card))
                    await servant.addBuff(game,new GeneratedBuff(ID,new AttackModifier(1),new LifeModifier(1)));
            },(game, card, arg) =>
            {
                return arg.player == card.getOwner();
            })
        };
    }
    /// <summary>
    /// 534 魂魄妖梦 突袭，风怒。
    /// </summary>
    public class KonpakuYoumu_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x031;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { Keyword.RUSH, Keyword.WINDFURY };
    }
    /// <summary>
    /// 646 蕾米莉亚·斯卡蕾特 唯一，战吼：弃三张牌，抽三张牌。
    /// </summary>
    public class RemiliaScarlet_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x032;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 6;
        public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };
        public override IEffect[] effects { get; set; } = new IEffect[]{
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().randomDiscard(game, 3);
            for (int i = 0; i < 3; i++)
            {
                await card.getOwner().draw(game);
            }
        }
    }
    /// <summary>
    /// 6/4/6 芙兰朵露·斯卡蕾特 唯一，战吼：弃一张牌，随机消灭一个敌方随从。
    /// </summary>
    public class FlandreScarlet_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x033;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 6;
        public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };
        public override IEffect[] effects { get; set; } = new IEffect[]{
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().randomDiscard(game, 1);
            game.getOpponent(card.getOwner()).field.random(game).setDead(true);
        }
    }
    /// <summary>
    /// 6/2/2 十六夜咲夜 战吼：抽一张牌，使其本回合法力值消耗变为0点。
    /// </summary>
    public class IzayoiSakuya_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x037;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().draw(game, out var draw);
            var drawed = draw.card;
            await drawed.addBuff(game, new GeneratedBuff(ID, new CostModifier(0, true),
              new RemoveBuffBefore<THHGame.TurnEndEventArg>(PileName.HAND, ID)));
        }
    }
    /// <summary>
    /// 7/4/5 八云蓝 战吼：召唤橙，并使其获得+3/+1和突袭。
    /// </summary>
    public class YakumoRan_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x038;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 7;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            var createToken = await card.getOwner().createToken(game, game.getCardDefine<Chen_N>());
            var chen = createToken.card;
            await chen.addBuff(game, new GeneratedBuff(ID,
                new AttackModifier(3),
                new LifeModifier(1),
                new KeywordsModifier(KeywordsModifier.ModifyType.add, Keyword.RUSH)));
        }
    }
    /// <summary>
    /// 8/8/8 西行寺幽幽子 唯一，战吼：吃掉一个随从。
    /// </summary>
    public class SaigyoujiYuyuko_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x039;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 8;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((game,card,target)=>
            {
                target.setDead(true);
                return Task.CompletedTask;
            })
        };
    }
    /// <summary>
    /// 10/8/8 紫妈 战吼：将其它随从移回其拥有者的手牌。
    /// </summary>
    public class YakumoYukari_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x040;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            foreach (var player in game.players)
            {
                await player.field.Where(c => c != card).backToHand(game);
            }
        }
    }
    /// <summary>
    /// 122 露米娅 潜行，使用骰子决定攻击目标
    /// </summary>
    public class Rumia_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x041;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { Keyword.STEALTH };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHCard.AttackEventArg>(PileName.FIELD,(g,c,a)=>
            {
                return a.card == c;
            },null,(g,c,a)=>
            {
                var oppo = g.getOpponent(c.getOwner());
                Card[] targets = new Card[]{ oppo.master }.Concat(oppo.field).ToArray();
                a.target = targets[g.dice(targets.Length)];
                return Task.CompletedTask;
            })
        };
    }
    /// <summary>
    /// 露娜萨·普莉兹姆利巴 622 战吼：乐队集合！（无论其他成员在哪里！）
    /// </summary>
    public class LunasaPrismriver_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x042;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect((g1,c1)=>
            {
                return bandAssemble(g1,c1.getOwner(),c1.pile.indexOf(c1));
            })
        };
        public static async Task bandAssemble(THHGame game, THHPlayer player, int position)
        {
            var lunasa = player.field.FirstOrDefault(c => c.define.id == LunasaPrismriver_N.ID);
            if (lunasa == null)
            {
                lunasa = player.findCardsWherever(game, c => c.define.id == LunasaPrismriver_N.ID, PileFlag.self | PileFlag.hand | PileFlag.grave | PileFlag.deck).random(game);
                if (lunasa != null)
                    await player.tryEnterField(game, lunasa.pile, lunasa, position);
            }
            var merlin = player.field.FirstOrDefault(c => c.define.id == MerlinPrismriver_N.ID);
            if (merlin == null)
            {
                merlin = player.findCardsWherever(game, c => c.define.id == MerlinPrismriver_N.ID, PileFlag.self | PileFlag.hand | PileFlag.grave | PileFlag.deck).random(game);
                if (merlin != null)
                    await player.tryEnterField(game, merlin.pile, merlin, lunasa.pile.indexOf(lunasa) + 1);
            }
            var lyrica = player.field.FirstOrDefault(c => c.define.id == LyricaPrismriver_N.ID);
            if (lyrica == null)
            {
                lyrica = player.findCardsWherever(game, c => c.define.id == LyricaPrismriver_N.ID, PileFlag.self | PileFlag.hand | PileFlag.grave | PileFlag.deck).random(game);
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
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x043;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { Keyword.TAUNT };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect((g1,c1)=>
            {
                return LunasaPrismriver_N.bandAssemble(g1,c1.getOwner(),c1.pile.indexOf(c1));
            })
        };
    }
    /// <summary>
    /// 莉莉卡·普莉兹姆利巴 621 圣盾，战吼：乐队集合！（无论其他成员在哪里！）
    /// </summary>
    public class LyricaPrismriver_N : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x044;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { Keyword.SHIELD };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect((g1,c1)=>
            {
                return LunasaPrismriver_N.bandAssemble(g1,c1.getOwner(),c1.pile.indexOf(c1));
            })
        };
    }
}