using System;
using System.Linq;
using TouhouCardEngine;
using System.Threading.Tasks;
using TouhouCardEngine.Interfaces;
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
            new THHEffectAfter<THHCard.DeathEventArg>(PileName.GRAVE,(game,card,arg)=>
            {
                return arg.infoDic.Any(p=>p.Key==card);
            },(game,card,targets)=>
            {
                return true;
            },async (game,card,arg)=>
            {
                await arg.infoDic[card].player.createToken(game,game.getCardDefine<LostSpecter>(),arg.infoDic[card].position);
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
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.FIELD,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                if(targets[0] is Card target && target.pile.name == PileName.FIELD)
                    return true;
                return false;
            },async (game,card,arg,targets)=>
            {
                if(targets[0] is Card target)
                {
                    //target.getNearbyCards().setFreeze(true);
                }
            })
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
}