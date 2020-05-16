using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    public class Patchouli : MasterCardDefine
    {
        public const int ID = 0x00400000 | CardCategory.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = KnowledgeTap.ID;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 知识引流
    /// </summary>
    public class KnowledgeTap : SkillCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>("Skill",(game,player,card,arg)=>
            {
                return true;
            },(game,player,card,targets)=>
            {
                return false;
            },async (game,player,card,arg,targets)=>
            {
                await arg.player.master.damage(game, 2);
                await arg.player.draw(game);
            })
        };
    }
    /// <summary>
    /// 秘藏魔法 本回合内你使用的下一张法术牌费用减半。
    /// </summary>
    public class BestMagic : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,player,card,arg)=>
            {
                return true;
            },(game,player,card,targets)=>
            {
                return false;
            },async (game,player,card,arg,targets)=>
            {
                
            })
        };
    }
    /// <summary>
    /// 魔神机甲 88 战吼：选择你的3张手牌丢弃。
    /// </summary>
    public class MegaReaver : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 贤者之石 05 每当你使用一张法术，将一张随机元素法术置入你的手牌并使耐久-1。战吼：将一张随机元素法术置入你的手牌。
    /// </summary>
    public class PhilosopherStone : ItemCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.ITEM | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 火神之光 元素法术，全场打4。
    /// </summary>
    public class AgniShine : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 水精公主 元素法术，召唤一个0/8并能使你所受到的所有法术伤害变为1的屏障。
    /// </summary>
    public class PrincessUndine : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 风灵角笛 元素法术，将一个敌方随从洗回对手的牌库。
    /// </summary>
    public class SylphyHorn : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 巨石震怒 元素法术，召唤3个无法进行攻击并具有嘲讽的2/4的巨石。
    /// </summary>
    public class TrilithonShake : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 金属疲劳 元素法术，将所有随从的属性变为1/1。
    /// </summary>
    public class MetalFatigue : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 小恶魔 66 战吼：发现一张墓地中的法术并置入你的手牌。
    /// </summary>
    public class Koakuma : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 6;
        public override int life { get; set; } = 6;
        public override string[] tags { get; set; } = new string[] { CardTag.DEMON };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 皇家烈焰 对敌方英雄造成15点伤害。
    /// </summary>
    public class RoyalFlare : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 12;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 沉静月神 沉默并消灭所有敌方随从。
    /// </summary>
    public class SilentSelene : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 12;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
}