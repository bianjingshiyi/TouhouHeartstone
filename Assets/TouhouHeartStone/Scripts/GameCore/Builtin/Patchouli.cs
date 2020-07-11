using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    public class Patchouli : MasterCardDefine
    {
        public const int ID = 0x00400000 | CardCategory.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = SummerFire.ID;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 知识引流
    /// </summary>
    [Obsolete]
    public class KnowledgeTap : SkillCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>("Skill",(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async (game,card,arg,targets)=>
            {
                await arg.player.master.damage(game,card, 2);
                await arg.player.draw(game);
            })
        };
    }
    /// <summary>
    /// 2 夏之火 造成1点伤害
    /// </summary>
    public class SummerFire : SkillCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SKILL | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((game,card,target)=>
            {
                return target.damage(game, card, 1);
            })
        };
    }
    /// <summary>
    /// 3 秘藏魔法 本回合内你使用的下一张法术牌花费-5
    /// </summary>
    /// <remarks>
    /// 这张卡的效果实际上应当是给使用者的master加上一个buff，这个buff附带一个光环效果，使玩家手牌中的所有法术-5费
    /// 当拥有者释放一个法术时，这个buff消失，当拥有者的回合结束时，这个buff也消失。
    /// 这些机制会由边境来提供，如果没有，就先跳过。
    /// </remarks>
    public class BestMagic : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
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

            })
        };
    }
    /// <summary>
    /// 777 魔神机甲 如果你在本回合中使用过花费大于0的法术牌，花费-2
    /// </summary>
    [Obsolete]
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
    /// 4 贤者之石 05 每当你使用一张法术，将一张随机元素法术置入你的手牌并使耐久-1。战吼：将一张随机元素法术置入你的手牌。
    /// </summary>
    [Obsolete]
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
    /// 5 火神之光 元素法术，对目标造成7点伤害
    /// </summary>
    public class AgniShine : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect((game,card,target)=>
            {
                target.damage(game, card, card.getOwner().getSpellDamage(7));
                return Task.CompletedTask;
            })
        };
    }
    /// <summary>
    /// 5 水精公主 元素法术，使所有友方角色获得减伤+3直到你的下个回合开始
    /// </summary>
    public class PrincessUndine : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 5 风灵角笛 元素法术，使目标随从获得+3/+6
    /// </summary>
    public class SylphyHorn : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE, (game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                if(targets[0] is Card target && target.pile.name == PileName.FIELD)
                    return true;
                return false;
            },(game,card,arg,targets)=>
            {
                if (targets[0] is Card target)
                {
                    card.addBuff(game,new GeneratedBuff(ID,new AttackModifier(3),new LifeModifier(6)));
                }
                return Task.CompletedTask;
            })
        };
    }
    /// <summary>
    /// 5 巨石震怒 元素法术，召唤一个3/9具有嘲讽且无法攻击的巨石
    /// </summary>
    public class TrilithonShake : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[]{
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async (game,card,arg,targets)=>
            {
                await arg.player.createToken(game, game.getCardDefine<Boulder>(), arg.player.field.count);
            })
        };
    }
    /// <summary>
    /// 巨石 衍生物
    /// </summary>
    public class Boulder : ServantCardDefine
    {
        public const int ID = CardCategory.CHARACTER_NEUTRAL | CardCategory.SERVANT | 0x657;
        public override int id { get; set; } = ID;
        public override bool isToken { get; set; } = true;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 9;
        public override string[] keywords { get; set; } = new string[] { Keyword.TAUNT };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHGame.TurnStartEventArg>(PileName.NONE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async(game,card,arg)=>
            {
                card.setReady(false);
            })
        };
    }

    /// <summary>
    /// 5 金属疲劳 元素法术，对所有敌方随从造成3点伤害
    /// </summary>
    public class MetalFatigue : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override IEffect[] effects { get; set; } = new IEffect[]{
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async (game,card,arg,targets)=>
            {
                foreach (Card target in game.getOpponent(card.owner as THHPlayer).field)
                {
                    await target.damage(game, card, arg.player.getSpellDamage(3));
                }
            })
        };
    }
    /// <summary>
    /// 666 小恶魔 战吼：发现一张墓地中的法术并置入你的手牌。
    /// </summary>
    public class Koakuma : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override int attack { get; set; } = 6;
        public override int life { get; set; } = 6;
        public override string[] tags { get; set; } = new string[] { CardTag.DEMON };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            var discovered = await card.getOwner().discover(game, card.getOwner().grave.Where(c => c.isSpell()));
            if (!card.getOwner().hand.isFull)
                await card.getOwner().grave.moveTo(game, discovered, card.getOwner().hand);
        }
    }
    /// <summary>
    /// 12 皇家烈焰 对敌方英雄造成15点伤害。
    /// </summary>
    public class RoyalFlare : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 12;
        public override IEffect[] effects { get; set; } = new IEffect[]{
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async(game,card,arg,targets)=>
            {
                await game.getOpponent(card.owner as THHPlayer).master.damage(game, card, 15);
            })
        };
    }
    /// <summary>
    /// 12 沉静月神 沉默并消灭所有敌方随从。
    /// </summary>
    public class SilentSelene : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 12;
        public override IEffect[] effects { get; set; } = new IEffect[]{
            new THHEffect<THHPlayer.ActiveEventArg>(PileName.NONE,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async(game,card,arg,targets)=>
            {
               foreach(Card target in game.getOpponent(card.owner as THHPlayer).field)
                {
                    target.define.effects = new IEffect[0];
                }
            })
        };
    }
    /// <summary>
    /// 111 元素精灵 战吼：将一张随机0费元素法术置入手牌
    /// </summary>
    public class ElementSprite : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x015;
        public override int cost { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int attack { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int life { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
    /// <summary>
    /// 2 大图书馆 发现一张牌库中的法术，抽这张牌并使其花费-1
    /// </summary>
    public class TheGreatLibrary : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x016;
        public override int cost { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
    /// <summary>
    /// 331 图书馆保卫者 战吼：你手牌中每有一张法术牌，便获得+1生命值
    /// </summary>
    public class LibraryProtector : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x017;
        public override int cost { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int attack { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int life { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
    /// <summary>
    /// 4 奥术知识 抽两张法术牌并使其花费-1
    /// </summary>
    public class ArcaneKnowledge : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x018;
        public override int cost { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}