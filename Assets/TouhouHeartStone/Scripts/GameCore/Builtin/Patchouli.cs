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
        public const string METAL = "Metal";
        public const string WOOD = "Wood";
        public const string WATER = "Water";
        public const string FIRE = "Fire";
        public const string EARTH = "Earth";
        public static string[] getElements(CardDefine define)
        {
            List<string> list = new List<string>();
            if (define.hasTag(METAL))
                list.Add(METAL);
            if (define.hasTag(WOOD))
                list.Add(WOOD);
            if (define.hasTag(WATER))
                list.Add(WATER);
            if (define.hasTag(FIRE))
                list.Add(FIRE);
            if (define.hasTag(EARTH))
                list.Add(EARTH);
            return list.ToArray();
        }
        public static CardDefine[] getMixCards(THHGame game)
        {
            return game.getCardDefines().Where(c => getElements(c).Length > 1).ToArray();
        }
        public static CardDefine[] getMixCards(THHGame game, CardDefine a, CardDefine b)
        {
            var elements = getElements(a).Concat(getElements(b)).Distinct().ToArray();
            return getMixCards(game).Where(c => elements.isSubset(getElements(c)) && c.id != a.id && c.id != b.id).ToArray();
        }
        public static async Task tryMix(THHGame game, Card card)
        {
            THHPlayer player = card.getOwner();
            var mixableCards = player.hand.Where(c => getMixCards(game, card.define, c.define).Length > 0).ToArray();//检查手牌中有没有可以融合的牌
            if (mixableCards.Length > 0)
            {
                var mixCard = await player.discover(game, mixableCards, -1, "选择一张卡片进行元素融合");
                var mixedResults = getMixCards(game, card.define, mixCard.define).Select(define => game.createCard(define)).ToArray();
                var mixedResult = await player.discover(game, mixedResults, -1, "选择一张融合元素法术加入你的手牌");
                await player.discard(game, mixCard);
                await player.tryAddCardToHand(game, mixedResult);
            }
        }
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
        public override int cost { get; set; } = 2;
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
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().master.addBuff(game, new GeneratedBuff(ID,
                new Halo(new GeneratedBuff(ID, new CostModifier(-5)), PileFlag.self | PileFlag.hand, PileFlag.self | PileFlag.master, (g1, c1) =>
                 {
                     return c1.isSpell();
                 }),
                new RemoveBuffBefore<THHPlayer.UseEventArg>(PileName.MASTER, (g, c, a) =>
                {
                    return a.player == card.getOwner() && a.card.isSpell();
                }, ID),
                new RemoveBuffBefore<THHGame.TurnEndEventArg>(PileName.MASTER, (g2, c2, a2) =>
                {
                    return a2.player == card.getOwner();
                }, ID)));
        }
    }
    /// <summary>
    /// 777 帕秋莉机器人 如果你在本回合中使用过花费大于0的法术牌，花费-2
    /// </summary>
    public class MegaReaver : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 7;
        public override int attack { get; set; } = 7;
        public override int life { get; set; } = 7;
        public override string[] keywords { get; set; } = new string[0];
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new AddBuffAfter<THHPlayer.UseEventArg>(PileName.HAND,(g,c,a)=>
            {
                return a.player == c.getOwner() && a.card.isSpell() && a.card.getCost(g) > 0;
            },new GeneratedBuff(ID,new CostModifier(-2),
                new RemoveBuffBefore<THHGame.TurnEndEventArg>(PileName.HAND,ID)))
        };
    }
    /// <summary>
    /// 4 贤者之石 05 每当你使用一张法术，将一张随机元素法术置入你的手牌并使耐久-1。战吼：将一张随机元素法术置入你的手牌。
    /// </summary>
    public class PhilosopherStone : ItemCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.ITEM | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 5;
        public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect((g1,c1)=>
            {
                return c1.getOwner().addRandomCardToHand(g1,g1.getCardDefines(new int[]{ MetalFatigue.ID,SylphyHorn.ID,PrincessUndine.ID,AgniShine.ID,TrilithonShake.ID }));
            }),
            new ItemTrigggerEffectAfter<THHPlayer.UseEventArg>((g1,c1,a1)=>
            {
                return a1.player==c1.getOwner() && a1.card.isSpell();
            },(g2,c2,a2)=>
            {
                return c2.getOwner().addRandomCardToHand(g2,g2.getCardDefines(new int[]{ MetalFatigue.ID,SylphyHorn.ID,PrincessUndine.ID,AgniShine.ID,TrilithonShake.ID }));
            })
        };
        class ItemTrigggerEffectAfter<T> : THHEffectAfter<T> where T : IEventArg
        {
            public ItemTrigggerEffectAfter(CheckConditionDelegate onCheckCondition, ExecuteDelegate onExecute)
                : base(PileName.ITEM, onCheckCondition, null, async (g, c, a) =>
                {
                    await onExecute?.Invoke(g, c, a);
                    c.setCurrentLife(c.getCurrentLife(g) - 1);
                    await g.updateDeath();
                })
            {
            }
        }
    }
    /// <summary>
    /// 5 火神之光 元素法术，对目标造成7点伤害
    /// </summary>
    public class AgniShine : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override string[] tags { get; set; } = new string[] { Patchouli.FIRE };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.damage(game, card, card.getOwner().getSpellDamage(game, 7));
                await Patchouli.tryMix(game,card);
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
        public override string[] tags { get; set; } = new string[] { CardTag.WATER };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(async (game,card)=>
            {
                await card.getOwner().master.addBuff(game,new GeneratedBuff(ID,
                    new DamageReduceModifier(3),
                    new RemoveBuffBefore<THHGame.TurnStartEventArg>(PileName.MASTER,(g,c,a)=>
                    {
                        return a.player == card.getOwner();
                    },ID)));
                foreach (var servant in card.getOwner().field)
                {
                    await servant.addBuff(game,new GeneratedBuff(ID,
                        new DamageReduceModifier(3),
                        new RemoveBuffBefore<THHGame.TurnStartEventArg>(PileName.FIELD,(g,c,a)=>
                        {
                            return a.player == card.getOwner();
                        },ID)));
                }
                await Patchouli.tryMix(game,card);
            })
        };
    }
    /// <summary>
    /// 5 风灵角笛 元素法术，使目标随从获得+3/+6
    /// </summary>
    public class SylphyHorn : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { CardTag.WOOD };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async(game,card,target)=>
            {
                await target.addBuff(game,new GeneratedBuff(ID,new AttackModifier(3),new LifeModifier(6)));
                await Patchouli.tryMix(game,card);
            }, PileFlag.both | PileFlag.field)
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
        public override string[] tags { get; set; } = new string[] { CardTag.EARTH };
        public override IEffect[] effects { get; set; } = new IEffect[]{
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().createToken(game, game.getCardDefine<RockElement>(), card.getOwner().field.count);
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 岩石元素 衍生随从，无法攻击
    /// </summary>
    public class RockElement : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x657;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 9;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.ELEMENT };
        public override string[] keywords { get; set; } = new string[] { Keyword.TAUNT };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            //TODO:只需要一个关键字就可以了，不需要写的这么复杂
            new THHEffectAfter<THHGame.TurnStartEventArg>(PileName.FIELD,(game,card,arg)=>
            {
                return true;
            },(game,card,targets)=>
            {
                return false;
            },(game,card,arg)=>
            {
                card.setReady(false);
                return Task.CompletedTask;
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
        public override string[] tags { get; set; } = new string[] { CardTag.METAL };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await game.getOpponent(card.getOwner()).field.damage(game, card, card.getOwner().getSpellDamage(game, 3));
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 666 小恶魔 战吼：发现一张墓地中的法术并置入你的手牌。
    /// </summary>
    public class Koakuma : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 6;
        public override int life { get; set; } = 6;
        public override string[] tags { get; set; } = new string[] { CardTag.DEMON };
        public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };
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
        public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await game.getOpponent(card.owner as THHPlayer).master.damage(game, card, 15);
        }
    }
    /// <summary>
    /// 12 沉静月神 沉默并消灭所有敌方随从。
    /// </summary>
    public class SilentSelene : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 12;
        public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await game.getOpponent(card.getOwner()).field.silence(game);
            await game.getOpponent(card.getOwner()).field.die(game);
        }
    }
    /// <summary>
    /// 111 元素精灵 战吼：将一张随机0费元素法术置入手牌
    /// </summary>
    public class ElementSprite : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { CardTag.ELEMENT };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static Task effect(THHGame game, Card card)
        {
            return card.getOwner().addRandomCardToHand(game, game.getCardDefines(new int[] { AutumnEdge.ID, SpringWind.ID, WinterElement.ID, SummerRed.ID, DoyouSpear.ID }));
        }
    }
    /// <summary>
    /// 2 大图书馆 发现一张牌库中的法术，抽这张牌并使其花费-1
    /// </summary>
    public class TheGreatLibrary : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            var discovered = await card.getOwner().discover(game, card.getOwner().deck.Where(c => c.isSpell()));
            if (!card.getOwner().hand.isFull)
            {
                await card.getOwner().draw(game, discovered);
                await discovered.addBuff(game, new GeneratedBuff(ID, new CostModifier(-1)));
            }
        }
    }
    /// <summary>
    /// 331 图书馆保卫者 战吼：你手牌中每有一张法术牌，便获得+1生命值
    /// </summary>
    public class LibraryProtector : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { Keyword.TAUNT };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.addBuff(game, new GeneratedBuff(ID, new LifeModifier(card.getOwner().hand.Where(c => c.isSpell()).Count() - 1)));
        }
    }
    /// <summary>
    /// 4 奥术知识 抽两张法术牌并使其花费-1
    /// </summary>
    public class ArcaneKnowledge : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x018;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            for (int i = 0; i < 2; i++)
            {
                await card.getOwner().draw(game);
                var drawed = game.triggers.getRecordedEvents().OfType<THHPlayer.DrawEventArg>().Last().card;
                await drawed.addBuff(game, new GeneratedBuff(ID, new CostModifier(-1)));
            }
        }
    }
    /// <summary>
    /// 0 火符【夏之红】 使目标随从在本回合内获得攻击力+2
    /// </summary>
    public class SummerRed : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x019;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.METAL };
        public override IEffect[] effects { get; set; } = new IEffect[] {

            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                new BuffFixer().onEnable(game,target);
                await Patchouli.tryMix(game,card);
            },PileFlag.both | PileFlag.field)
        };//TODO:这个Buff，不需要这么复杂。
        class BuffFixer
        {
            Trigger<THHGame.TurnEndEventArg> TurnEndTrigger { get; set; } = null;
            Buff buff = new GeneratedBuff(ID, new AttackModifier(2));
            public async void onEnable(THHGame game, Card card)
            {
                await card.addBuff(game, buff);
                if (TurnEndTrigger == null)
                {
                    TurnEndTrigger = new Trigger<THHGame.TurnEndEventArg>(async arg =>
                    {
                        await card.removeBuff(game, buff);
                        game.triggers.removeAfter(TurnEndTrigger);
                        TurnEndTrigger = null;
                    });
                    game.triggers.registerAfter(TurnEndTrigger);
                }
            }
        }
    }
    /// <summary>
    /// 0 水符【冬之素】 使目标随从冻结
    /// </summary>
    public class WinterElement : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x020;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.WOOD };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                target.setFreeze(true);
                await Patchouli.tryMix(game,card);
            })
        };
    }
    /// <summary>
    /// 0 木符【春之风】 使目标随从获得生命值+2
    /// </summary>
    public class SpringWind : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x021;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.WATER };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new LambdaSingleTargetEffect(async(game,card,target)=>
            {
                await target.addBuff(game,new GeneratedBuff(ID,new LifeModifier(2)));
                await Patchouli.tryMix(game,card);
            },PileFlag.both | PileFlag.field)
        };
    }
    /// <summary>
    /// 0 金符【秋之刃】 随机对一个敌方随从造成2点伤害
    /// </summary>
    public class AutumnEdge : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x022;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.FIRE };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            THHPlayer opponent = game.getOpponent(card.getOwner());
            if (opponent.field.count > 0)
                return;
            await opponent.field.randomTake(game, 1).damage(game, card, 2);
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 0 土符【土用之枪】 召唤一个1/1的宝石长枪
    /// </summary>
    public class DoyouSpear : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x023;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.EARTH };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().createToken(game, game.getCardDefine<GemSpear>(), card.getOwner().field.count);
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 111 衍生随从 宝石长枪
    /// </summary>
    public class GemSpear : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x658;
        public override int id { get; set; } = ID;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.ELEMENT };
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 4 金木符【元素收割者】 对所有敌方随从造成2点伤害，每消灭一个敌方随从使一个随机友方随从获得+1/1
    /// </summary>
    public class ElementalHarvester : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x024;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.METAL, Patchouli.WOOD };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            THHPlayer player = card.getOwner();
            THHPlayer opponent = game.getOpponent(player);
            await opponent.field.damage(game, card, player.getSpellDamage(game, 2));
            foreach (Card target in opponent.field)
            {
                if (target.isDead(game))
                {
                    if (player.field.count > 0)
                        await player.field.random(game).addBuff(game, new GeneratedBuff(ID, new AttackModifier(1), new LifeModifier(1)));
                }
            }
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 6 金水符【水银之毒】 使所有敌方随从攻击力-3直到你的回合开始，若其攻击力被减为0，则将其消灭
    /// </summary>
    public class MercuryPoison : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x025;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.WATER, CardTag.METAL };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(async (game,card)=>
            {
                THHPlayer opponent = game.getOpponent(card.getOwner());
                foreach(Card target in opponent.field)
                {
                    await target.addBuff(game,
                        new GeneratedBuff(ID,new AttackModifier(-3),
                        new RemoveBuffBefore<THHGame.TurnStartEventArg>(PileName.FIELD,ID)));
                    if(target.getAttack(game)<=0)
                        target.setDead(true);
                }
                await Patchouli.tryMix(game, card);
            })
        };
    }
    /// <summary>
    /// 6 木火符【森林大火】 使所有友方随从获得+2/2，你每控制一个随从便对目标造成1点伤害
    /// </summary>
    public class ForestBlaze : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x026;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.FIRE, CardTag.WOOD };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                foreach(Card buffcard in card.getOwner().field)
                {
                    await buffcard.addBuff(game,new GeneratedBuff(ID,new AttackModifier(2),new LifeModifier(2)));
                }
                await target.damage(game,card,card.getOwner().field.count);
                await Patchouli.tryMix(game, card);
            },PileFlag.both | PileFlag.field)
        };
    }
    /// <summary>
    /// 4 木土符【抽芽行尸】 使目标随从获得亡语：召唤一个目标随从的复制并使其获得+3+3
    /// </summary>
    public class BurgeoningRise : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x027;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.EARTH, CardTag.WOOD };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.addBuff(game, new GeneratedBuff(ID,new DeathRattle(ID,async (g,c,p)=>
                {
                    var createToken = await card.getOwner().createToken(game,c.define,p);
                    if(createToken!=null)
                        await createToken.card.addBuff(game,new GeneratedBuff(ID+1,new AttackModifier(3),new LifeModifier(3)));
                })));
                await Patchouli.tryMix(game, card);
            },PileFlag.both | PileFlag.field)
        };
        private static IEffect[] addeffect = new IEffect[]{new THHEffectAfter<THHCard.DeathEventArg>(PileName.GRAVE, (game, card, arg) =>
        {
            return arg.infoDic.Any(p => p.Key == card);
        }, (game, card, targets) =>
        {
            return true;
        }, async (game, card, arg) =>
        {
            await arg.infoDic[card].player.createToken(game, game.getCardDefine(card.define.id), arg.infoDic[card].position);
        })
        };
    }
    /// <summary>
    /// 6 水木符【水精灵】 使目标随从获得+3/6和嘲讽，并回复其所有生命值
    /// </summary>
    public class WaterElf : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x028;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.WATER, CardTag.WOOD };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.addBuff(game,new GeneratedBuff(ID,new LifeModifier(6),new AttackModifier(3)));
                await target.heal(game,target.getLife(game)-target.getCurrentLife(game));
                await Patchouli.tryMix(game, card);
            }, PileFlag.both | PileFlag.field)};
    }
    /// <summary>
    /// 4 水火符【燃素之雨】 造成点6伤害，随机分配给所有敌方角色
    /// </summary>
    public class PhlogisticRain : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x029;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.WATER, CardTag.FIRE };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            Card[] target;
            for (int i = 0; i < 6; i++)
            {
                do
                {
                    target = game.getAllEnemies(card.getOwner()).randomTake(game, 1).ToArray();
                }
                while (target[0].getCurrentLife(game) == 0);
                await target[0].damage(game, card, 1);
            }
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 火金符【圣爱尔摩火柱】 对目标随从造成6点伤害，并对其相邻的随从造成2点伤害
    /// </summary>
    public class StElmoPillar : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x030;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.METAL, CardTag.FIRE };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.damage(game,card,6);
                await target.getNearbyCards().damage(game,card,2);
                await Patchouli.tryMix(game, card);
            }, PileFlag.both | PileFlag.field)
        };
    }
    /// <summary>
    /// 6 火土符【环状熔岩带】 召唤3个2/2并能在回合结束对随机敌方角色造成2点伤害的熔岩元素
    /// </summary>
    public class LavaCromlech : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x031;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.FIRE, CardTag.EARTH };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            for (int i = 0; i < 3; i++)
                await card.getOwner().createToken(game, game.getCardDefine<LavaElement>(), card.getOwner().field.count);
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 衍生随从 熔岩元素 回合结束对随机敌方角色造成2点伤害
    /// </summary>
    public class LavaElement : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x659;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.ELEMENT };
        public override string[] keywords { get; set; } = new string[] { Keyword.TAUNT };
        public override IEffect[] effects { get; set; } = new IEffect[]{
            new THHEffectBefore<THHGame.TurnEndEventArg>(PileName.FIELD,(game,card,arg)=>
            {
                if(card.getOwner() != game.currentPlayer)
                    return false;
                return true;
            },(game,card,targets)=>
            {
                return false;
            },async(game,card,arg)=>
            {
                await game.getAllEnemies(card.getOwner()).randomTake(game, 1).damage(game, card, 2);
            })
        };
    }
    /// <summary>
    /// 6 土金符【淡黄阵风】 每有一个敌方随从，便召唤一个3/1具有突袭的沙尘元素
    /// </summary>
    public class GingerGust : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x032;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.METAL, CardTag.EARTH };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            THHPlayer opponent = game.getOpponent(card.getOwner());
            for (int i = 0; i < opponent.field.count; i++)
                await card.getOwner().createToken(game, game.getCardDefine<DustElement>(), card.getOwner().field.count);
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 衍生随从 沙尘元素 31突袭
    /// </summary>
    public class DustElement : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x660;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 1;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.ELEMENT };
        public override string[] keywords { get; set; } = new string[] { Keyword.RUSH };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 土水符【诺亚洪水】 沉默所有随从，然后召唤1个2/2并具有嘲讽的洪水元素
    /// </summary>
    public class NoachianDeluge : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x033;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.EARTH, CardTag.WATER };
        public override IEffect[] effects { get; set; } = new IEffect[] {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            foreach (Card servant in game.getAllServants())
            {
                servant.define.effects = new IEffect[0];
            }
            await card.getOwner().createToken(game, game.getCardDefine<FloodElement>(), card.getOwner().field.count);
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 衍生随从 洪水元素 嘲讽
    /// </summary>
    public class FloodElement : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x661;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { CardTag.ELEMENT };
        public override string[] keywords { get; set; } = new string[] { Keyword.TAUNT };
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }
    /// <summary>
    /// 464 噬法触手 突袭，当你的回合开始时随机弃一张法术牌
    /// </summary>
    public class BiteTentacle : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SERVANT | 0x034;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 6;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = new string[] { CardTag.DEMON };
        public override string[] keywords { get; set; } = new string[] { Keyword.RUSH };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<THHGame.TurnStartEventArg>(PileName.FIELD,(g1,c1,a1)=>
            {
                return a1.player==c1.getOwner();
            },null,(g1,c1,a1)=>
            {
                return c1.getOwner().randomDiscard(g1,1,c2=>c2.isSpell());
            })
        };
    }
    /// <summary>
    /// 多重施法 1 随机使一张手牌中的法术牌花费-2，将一张伤害递增的“哮喘”洗入牌库，将另一张“多重施法”置入你的手牌。
    /// </summary>
    public class MultiCast : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CardCategory.SPELL | 0x035;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(async (g1,c1)=>
            {
                await c1.getOwner().hand.random(g1).addBuff(g1,new GeneratedBuff(ID,new CostModifier(-2)));
                c1.getOwner().setProp(nameof(Asthma),c1.getOwner().getProp<int>(nameof(Asthma))+1);
                Card asthma = g1.createCard<Asthma>();
                asthma.setProp("damage",c1.getOwner().getProp<int>(nameof(Asthma)));
                await c1.getOwner().shuffleCardToDeck(g1,asthma);
                await c1.getOwner().addCardToHand<MultiCast>(g1);
            })
        };
    }
    /// <summary>
    /// 哮喘 1 抽到自动释放，对你的英雄造成{card:damage}点伤害
    /// </summary>
    public class Asthma : SpellCardDefine
    {
        public const int ID = MultiCast.ID | CardCategory.TOKEN | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { Keyword.AUTOCAST };
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect((g1,c1)=>
            {
                return c1.getOwner().master.damage(g1,c1,c1.getProp<int>(g1,"damage"));
            })
        };
    }
}