using System.Threading.Tasks;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
namespace TouhouHeartstone.Builtin
{
    public class Reimu : MasterCardDefine
    {
        public const int ID = 0x00100000 | CardCategory.MASTER | 0x000;
        public override int id { get; set; } = 1000;
        public override int life { get; } = 30;
        public override int skillID { get; } = TotematicCall.ID;
        public override IEffect[] effects { get; set; } = new Effect[0];
    }
    public class TotematicCall : SkillCardDefine
    {
        public const int ID = 1001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect(new On<THHPlayer.ActiveEventArg>(),"Skill",(game,player,card,vars)=>
            {
                return player.field.count<player.field.maxCount;
            },(game,player,card,targets)=>
            {
                return true;
            },async (game,player,card,vars,targets)=>
            {
                CardDefine[] totemDefines = new CardDefine[]
                {
                    game.getCardDefine<HealTotem>(),
                    game.getCardDefine<FireTotem>(),
                    game.getCardDefine<ManaTotem>(),
                    game.getCardDefine<TauntTotem>()
                };
                if(totemDefines.All(d=>player.field.Any(c=>c.define==d)) ||//场上四种类型的图腾都有
                    !totemDefines.Any(d=>player.field.Any(c=>c.define==d)))//或者都没有
                {
                    //随便召唤一种类型的图腾
                    CardDefine totemDefine = totemDefines[game.randomInt(0,3)];
                    await player.createToken(game,totemDefine,player.field.count);
                }
                else
                {
                    //召唤一种场上没有的图腾
                    CardDefine totemDefine = totemDefines.Where(d=>!player.field.Any(c=>c.define==d)).shuffle(game).First();
                    await player.createToken(game,totemDefine,player.field.count);
                }
            })
        };
    }
    public class HealTotem : ServantCardDefine
    {
        public const int ID = Reimu.ID | CardCategory.SERVANT | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHGame.TurnEndEventArg>(PileName.FIELD,(game,player,card,arg)=>
            {
                if(arg.player!=player)//不是自己的回合
                    return false;
                return true;
            },(game,player,card,targets)=>
            {
                return true;
            },async (game,player,card,arg)=>
            {
                await THHCard.heal(player.field,game,1);
            })
        };
    }
    public class FireTotem : ServantCardDefine
    {
        public const int ID = Reimu.ID | CardCategory.SERVANT | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    public class ManaTotem : ServantCardDefine
    {
        public const int ID = Reimu.ID | CardCategory.SERVANT | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 2;
        public override int spellDamage { get; set; } = 1;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    public class TauntTotem : ServantCardDefine
    {
        public const int ID = Reimu.ID | CardCategory.SERVANT | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { Keyword.TAUNT };
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 梦想封印 灵梦普通法术 3费 随机对3个敌方随从造成2点伤害。
    /// </summary>
    public class FantasySeal : SpellCardDefine
    {
        public const int ID = Reimu.ID | CardCategory.SPELL | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,player,card,arg)=>
            {
                return true;
            },(game,player,card,targets)=>
            {
                return true;
            },async (game,player,card,arg,targets)=>
            {
                THHPlayer opponent = game.getOpponent(arg.player);
                if(opponent.field.count<4)
                    await opponent.field.damage(game, arg.player.getSpellDamage(2));
                else
                    await opponent.field.randomTake(game,3).damage(game, arg.player.getSpellDamage(2));
            })
        };
    }
}