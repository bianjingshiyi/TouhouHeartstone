﻿using System.Linq;
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
        public override int cost { get; } = 1;
        public override int attack { get; } = 1;
        public override int life { get; } = 1;
        public override string[] keywords { get; } = new string[] { Keyword.CHARGE };
        public override IEffect[] effects { get; } = new Effect[0];
    }
    /// <summary>
    /// 人类村落守卫，325，嘲讽
    /// </summary>
    public class HumanVillageGuard : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; } = 3;
        public override int attack { get; } = 2;
        public override int life { get; } = 4;
        public override string[] keywords { get; } = new string[] { Keyword.TAUNT };
        public override IEffect[] effects { get; } = new IEffect[0];
    }
    /// <summary>
    /// 铁炮猎人 中立普通卡 322 战吼：造成2点伤害
    /// </summary>
    public class RifleHunter : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; } = 3;
        public override int attack { get; } = 2;
        public override int life { get; } = 2;
        public override string[] keywords { get; } = new string[0];
        public override IEffect[] effects { get; } = new IEffect[]
        {
            new THHEffect<THHPlayer.BattleCryEventArg>(PileName.FIELD,(game,player,card,vars)=>
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
        public override int cost { get; } = 5;
        public override int attack { get; } = 4;
        public override int life { get; } = 1;
        public override string[] keywords { get; } = new string[0];
        public override IEffect[] effects { get; } = new IEffect[]
        {
            new THHEffectAfter<THHCard.DeathEventArg>(PileName.GRAVE,(game,player,card,arg)=>
            {
                return arg.cards.Contains(card);
            },(game,player,card,targets)=>
            {
                return true;
            },async (game,player,card,arg)=>
            {
                await player.createToken(game,game.getCardDefine<LostSpecter>(),arg.position);
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
        public override bool isToken { get; } = true;
        public override int cost { get; } = 5;
        public override int attack { get; } = 4;
        public override int life { get; } = 3;
        public override string[] keywords { get; } = new string[0];
        public override IEffect[] effects { get; } = new IEffect[0];
    }
}