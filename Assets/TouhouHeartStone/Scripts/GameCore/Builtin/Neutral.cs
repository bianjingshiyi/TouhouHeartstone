﻿using System;
using System.Linq;
using TouhouCardEngine;
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
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.FIELD,(game,player,card,vars)=>
            {
                return true;
            },(game,player,card,targets)=>
            {
                return true;
            },async (game,player,card,vars,targets)=>
            {
                Card target = targets[0] as Card;
                await target.damage(game,2);
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
            new THHEffectAfter<THHCard.DeathEventArg>(PileName.GRAVE,(game,player,card,arg)=>
            {
                return arg.infoDic.Any(p=>p.Key==card);
            },(game,player,card,targets)=>
            {
                return true;
            },async (game,player,card,arg)=>
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
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.HAND,(game,player,card,arg)=>
            {
                return true;
            },(game,player,card,targets)=>
            {
                return false;
            },async (game,player,card,arg,targets)=>
            {
                await arg.player.setGem(game,arg.player.gem + 1);
            })
        };
    }

    /// <summary>
    /// 一个会突袭的随从
    /// </summary>
    public class RushServant : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override string[] keywords { get; set; } = new string[] { Keyword.RUSH };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    /// <summary>
    /// 一个会圣盾的随从
    /// </summary>
    public class ShieldServant : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override string[] keywords { get; set; } = new string[] { Keyword.SHIELD };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    /// <summary>
    /// 会潜行的随从
    /// </summary>
    public class StealthServant : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x00B;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override string[] keywords { get; set; } = new string[] { Keyword.STEALTH };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    
    /// <summary>
    /// 会吸血的随从
    /// </summary>
    public class SuckingServant : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x00B;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override string[] keywords { get; set; } = new string[] { };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    /// <summary>
    /// 剧毒随从
    /// </summary>
    public class PoisonServant : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x00B;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override string[] keywords { get; set; } = new string[] { };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    /// <summary>
    /// 一只白板的挨打用随从
    /// </summary>
    public class DefaultServant : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x00A;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 7;
        public override string[] tags { get; set; } = new string[] { CardTag.FAIRY };
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
}