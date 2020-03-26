using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    public class TotematicCall : SkillCardDefine
    {
        public const int ID = 1001;
        public override int id { get; set; } = ID;
        public override int cost
        {
            get { return 2; }
        }
        public override IEffect[] effects { get; } = new IEffect[]
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
        public override int cost { get; } = 1;
        public override int attack { get; } = 0;
        public override int life { get; } = 2;
        public override IEffect[] effects { get; } = new IEffect[]
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
        public override int cost { get; } = 1;
        public override int attack { get; } = 1;
        public override int life { get; } = 1;
        public override IEffect[] effects { get; } = new IEffect[0];
    }
    public class ManaTotem : ServantCardDefine
    {
        public const int ID = Reimu.ID | CardCategory.SERVANT | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; } = 1;
        public override int attack { get; } = 0;
        public override int life { get; } = 2;
        public override int spellDamage { get; } = 1;
        public override IEffect[] effects { get; } = new IEffect[0];
    }
    public class TauntTotem : ServantCardDefine
    {
        public const int ID = Reimu.ID | CardCategory.SERVANT | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; } = 1;
        public override int attack { get; } = 0;
        public override int life { get; } = 2;
        public override string[] keywords { get; } = new string[] { Keyword.TAUNT };
        public override IEffect[] effects { get; } = new IEffect[0];
    }
}
