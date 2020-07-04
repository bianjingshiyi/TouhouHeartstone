using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    public class Marisa : MasterCardDefine
    {
        public const int ID = 0x00200000 | CardCategory.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = StartdustBullet.ID;
        public override IEffect[] effects { get; set; } = new Effect[0];
    }
    public class StartdustBullet : SkillCardDefine
    {
        public const int ID = Marisa.ID | CardCategory.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 极限火花 造成7点伤害。
    /// </summary>
    public class MasterSpark : SpellCardDefine
    {
        public const int ID = Marisa.ID | CardCategory.SPELL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                if(targets[0] is Card target && (target.pile.name == PileName.MASTER || target.pile.name == PileName.FIELD))
                    return true;
                return false;
            },async (game,card,arg,targets)=>
            {
                if(targets[0] is Card target)
                    await target.damage(game,card, arg.player.getSpellDamage(7));
            })
        };
    }
    /// <summary>
    /// 迷你八卦炉 我也不知道是什么，反正很厉害就对了。
    /// </summary>
    public class MiniHakkero : ItemCardDefine
    {
        public const int ID = Marisa.ID | CardCategory.ITEM | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 3;
    }
    /// <summary>
    /// 星尘幻想 随机对敌方角色造成4点伤害。
    /// </summary>
    public class StardustReverie : SpellCardDefine
    {
        public const int ID = Marisa.ID | CardCategory.SPELL | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async (game,card,arg,targets)=>
            {
                for (int i = 0; i < arg.player.getSpellDamage(4); i++)
                {
                    await game.getAllEnemies(arg.player).randomTake(game,1).damage(game,card,1);
                }
            })
        };
    }
    /// <summary>
    /// 法术剽窃 发现一张你的对手墓地中的法术牌，将其置入你的手牌并使其花费变为0。
    /// </summary>
    public class MagicEspionage : SpellCardDefine
    {
        public const int ID = Marisa.ID | CardCategory.SPELL | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            //new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,player,card,arg)=>
            //{
            //    return true;
            //},(game,player,card,targets)=>
            //{
            //    return false;
            //},async (game,player,card,arg,targets)=>
            //{
            //    for (int i = 0; i < arg.player.getSpellDamage(4); i++)
            //    {
            //        await game.getAllEnemies(arg.player).randomTake(game,1).damage(game,1);
            //    }
            //})
        };
    }
    /// <summary>
    /// 魔法扫帚 如果你具有法术强度，则攻击力+1
    /// </summary>
    public class MagicBroom : ItemCardDefine
    {
        public const int ID = Marisa.ID | CardCategory.ITEM | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;
    }
    /// <summary>
    /// 彗星 对一个随从造成2点伤害，抽2张牌
    /// </summary>
    public class BlazingStar : SpellCardDefine
    {
        public const int ID = Marisa.ID | CardCategory.SPELL | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                if(targets[0] is Card target && target.pile == game.getOpponent(card.owner).field)
                    return true;
                return false;
            },async (game,card,arg,targets)=>
            {
                if(targets[0] is Card target)
                    await target.damage(game,card,arg.player.getSpellDamage(2));
                for (int i = 0; i < 2; i++)
                {
                    await arg.player.draw(game);
                }
            })
        };
    }
    /// <summary>
    /// 魔理沙的大帽子 使一个随从+1/1并获得法术强度+1
    /// </summary>
    public class MarisasBigHat : SpellCardDefine
    {
        public const int ID = Marisa.ID | CardCategory.SPELL | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,card,arg)=>
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
                    target.addBuff(game,new MarisasBigHatBuff());
                }
            })
        };
    }
    public class MarisasBigHatBuff : Buff
    {
        public override int id { get; } = MarisasBigHat.ID;
        public override PropModifier[] modifiers { get; } = new PropModifier[]
        {
            new AttackModifier(1),
            new LifeModifier(1),
            new SpellDamageModifier(1)
        };
        public override Buff clone()
        {
            return new MarisasBigHatBuff();
        }
    }
}
