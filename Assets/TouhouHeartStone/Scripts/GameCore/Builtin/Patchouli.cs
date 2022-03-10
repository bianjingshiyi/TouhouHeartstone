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
        public const int ID = CATEGORY.CHARACTER_PATCHOULI | CATEGORY.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = SummerFire.ID;

        private IEffect[] effects1 = new IEffect[0];

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

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
        public static CardDefine[] getElementCards(CardEngine game)
        {
            return game.getDefines().Where(c => getElements(c).Length == 1 && c is SpellCardDefine spell && !spell.isToken).ToArray();
        }
        public static CardDefine[] getMixCards(CardEngine game)
        {
            return game.getDefines().Where(c => getElements(c).Length > 1).ToArray();
        }
        public static CardDefine[] getMixCards(CardEngine game, CardDefine a, CardDefine b)
        {
            var elements = getElements(a).Concat(getElements(b)).Distinct().ToArray();
            return getMixCards(game).Where(c => elements.isSubset(getElements(c)) && c.id != a.id && c.id != b.id).ToArray();
        }


        public static async Task<ElementMixEventArg> tryMix(CardEngine game, Card card)
        {
            Player player = card.getOwner();
            var mixableCards = player.getHand().Where(c => getMixCards(game, card.define, c.define).Length > 0);
            if (mixableCards.Count() < 0)
            {
                return null;
            }
            var eventArg = await game.triggers.doEvent(new ElementMixEventArg()
            {
                player = player,
            }, async args =>
             {
                 Card mixCard = await game.askSelect(player, mixableCards, title: "投入魔法");
                 var mixedResults = getMixCards(game, card.define, mixCard.define);
                 var mixedResult = await game.askSelect(player, mixedResults, title: "元素融合");
                 await mixCard.setDefine(game, mixedResult);
                 await mixCard.resetCard(game);
             });

            return eventArg;
        }


        public class ElementMixEventArg : EventArg, IDescribableEventArg
        {
            public Player player;
            public CardDefine define1;
            public CardDefine define2;
            public Card results;

            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return viewer == player ? results : null;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return results;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}玩家，{1}卡，{2}卡，{3}卡")]
            const string TEMPLATE_SELF = "{0} 将 {1} 与 {2} 融合,获得{3}";

            [MukioI18n.TranslateMsg("{0} 玩家")]
            const string TEMPLATE_OTHER = "{0} 执行了元素融合";

            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (!isCanceled)
                {
                    return viewer == player ? TEMPLATE_SELF : TEMPLATE_OTHER;
                }
                else
                {
                    return "";
                }
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name,
                    define1.GetType().Name,
                    define2.GetType().Name,
                    results.getFormatString()
                };
            }
        }

    }

    /// <summary>
    /// 2 夏之火 造成1点伤害
    /// </summary>
    public class SummerFire : SkillCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SKILL | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect((game,card,target)=>
            {
                return target.damage(game, card, 1);
            },PileFlag.both | PileFlag.character)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
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
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
            //new GeneratedEffect(new ActionNode("AddBuff", new ActionValueRef[]
            //{
            //    new ActionValueRef(new ActionNode("GetMaster", new ActionValueRef[]
            //    {
            //        new ActionValueRef(new ActionNode("GetOwner", new ActionValueRef[]
            //        {
            //            new ActionValueRef(new ActionNode("GetVariable", new object[] { "Card" }))
            //        }))
            //    })),
            //    new ActionValueRef(new ActionNode("GetBuffDefine"))
            //}) ,new string[0])
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            //if (game.getBuffDefine(haloBuffDefine.id) == null)
            //    game.rule.addBuffDefine(haloBuffDefine);
            //if (game.getBuffDefine(buffDefine.id) == null)
            //    game.rule.addBuffDefine(buffDefine);
            //await card.getOwner().getMaster().addBuff(game, new GeneratedBuff(game.getBuffDefine(haloBuffDefine.id)));
            await card.getOwner().getMaster().addBuff(game, new CodeBuff(ID,
                new Halo(new CodeBuff(ID, new CostModifier(-5)), PileFlag.self | PileFlag.hand, PileFlag.self | PileFlag.master, (g1, c1) =>
                {
                    return c1.isSpell();
                }),
                new RemoveBuffBefore<HearthStoneRule.UseEventArg>(PileFlag.master, (g, c, b, a) =>
                {
                    return a.player == card.getOwner() && a.card.isSpell();
                }, ID),
                new RemoveBuffBefore<HearthStoneRule.TurnEndEventArg>(PileFlag.master, (g2, c2, b2, a2) =>
                {
                    return a2.player == card.getOwner();
                }, ID)));
        }
        //static GeneratedBuffDefine haloBuffDefine = new GeneratedBuffDefine(ID << 1, null, new GeneratedEffect[]
        //{
        //    new GeneratedHalo(ID<<1+1,new string[]{ PILE.MASTER },new string[]{ PILE.HAND }, new ActionValueRef(new ActionNode("IsSpell",new ActionValueRef[]
        //    {
        //        new ActionValueRef(new ActionNode("ThisCard"))
        //    }))),
        //    new GeneratedEffect(new string[] { DEFINE.MASTER },
        //        new TriggerGraph(
        //            "BeforeUseEvent",
        //            new ActionValueRef(new ActionNode("LogicOperation", new ActionValueRef[]
        //            {
        //                new ActionValueRef(new ActionNode("Compare",new ActionValueRef[]
        //                {
        //                    new ActionValueRef(new ActionNode("GetVariable", "Player")),
        //                    new ActionValueRef(new ActionNode("GetOwner",new ActionValueRef[]
        //                    {
        //                        new ActionValueRef(new ActionNode("ThisCard"))
        //                    }))
        //                },new object[]{ CompareOperator.equals })),
        //                new ActionValueRef(new ActionNode("IsSpell", new ActionValueRef[]
        //                {
        //                    new ActionValueRef(new ActionNode("GetVariable", "Card"))
        //                }))
        //            },  new object[]{ LogicOperator.and })),
        //            new ActionNode("RemoveBuff", new ActionValueRef[]
        //            {
        //                new ActionValueRef(new ActionNode("ThisCard")),
        //                new ActionValueRef(new ActionNode("ThisBuff"))
        //            }))),
        //    new GeneratedEffect(new string[] { DEFINE.MASTER },
        //        new TriggerGraph(
        //            "BeforeTurnEndEvent",
        //            new ActionValueRef(new ActionNode("Compare",new ActionValueRef[]
        //            {
        //                new ActionValueRef(new ActionNode("GetVariable", "Player")),
        //                new ActionValueRef(new ActionNode("GetOwner",new ActionValueRef[]
        //                {
        //                    new ActionValueRef(new ActionNode("ThisCard"))
        //                }))
        //            },new object[]{ CompareOperator.equals })),
        //            new ActionNode("RemoveBuff", new ActionValueRef[]
        //            {
        //                new ActionValueRef(new ActionNode("ThisCard")),
        //                new ActionValueRef(new ActionNode("ThisBuff"))
        //            }))),
        //});
        //static GeneratedBuffDefine buffDefine = new GeneratedBuffDefine(ID << 1 + 1, new PropModifier[]
        //{
        //    new CostModifier(-5)
        //});
    }
    /// <summary>
    /// 777 机械帕秋莉 如果你在本回合中使用过法术牌，花费-2
    /// </summary>
    public class MegaReaver : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SERVANT | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 7;
        public override int attack { get; set; } = 7;
        public override int life { get; set; } = 7;
        public override string[] keywords { get; set; } = new string[0];

        private IEffect[] effects1 = new IEffect[]
        {
            new AddBuffAfter<HearthStoneRule.UseEventArg>(PileFlag.hand,(g,c,a)=>
            {
                return a.player == c.getOwner() && a.card.isSpell() && !c.containBuff(ID);
            },new CodeBuff(ID,new CostModifier(-2),
                new RemoveBuffBefore<HearthStoneRule.TurnEndEventArg>(PileFlag.hand,ID)))
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 4 贤者之石 05 每当回合结束，随机将一张花费大于0的元素法术置入你的手牌，耐久-1。战吼：将一张随机元素法术置入你的手牌。
    /// </summary>
    public class PhilosopherStone : ItemCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.ITEM | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 5;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect((g1,c1)=>
            {
                c1.damage(g1,c1,1);
                return c1.getOwner().addRandomCardToHand(g1,g1.getDefines(new int[]{ MetalFatigue.ID,SylphyHorn.ID,PrincessUndine.ID,AgniShine.ID,TrilithonShake.ID }));

            }),
            new ItemTrigggerEffectBefore<HearthStoneRule.TurnEndEventArg>((g1,c1,b1,a1)=>
            {
                return a1.player==c1.getOwner();
            },(g2,c2,b2,a2)=>
            {
                return c2.getOwner().addRandomCardToHand(g2,g2.getDefines(new int[]{ MetalFatigue.ID,SylphyHorn.ID,PrincessUndine.ID,AgniShine.ID,TrilithonShake.ID }));
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        class ItemTrigggerEffectAfter<T> : THHEffectAfter<T> where T : IEventArg
        {
            public ItemTrigggerEffectAfter(CheckConditionNoBuffDelegate onCheckCondition, ExecuteNoBuffNoTargetDelegate onExecute)
                : base(PileFlag.item, onCheckCondition, async (g, c, a) =>
                {
                    await onExecute?.Invoke(g, c, a);
                    await c.damage(g, c, 1);
                    await g.updateDeath();
                })
            {
            }
        }
        class ItemTrigggerEffectBefore<T> : THHEffectBefore<T> where T : IEventArg
        {
            public ItemTrigggerEffectBefore(CheckConditionDelegate onCheckCondition, ExecuteNoTargetDelegate onExecute)
                : base(PileFlag.item, onCheckCondition, async (g, c, b, a) =>
                {
                    await onExecute?.Invoke(g, c, b, a);
                    await c.damage(g, c, 1);
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
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override string[] tags { get; set; } = new string[] { Patchouli.FIRE };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.damage(game, card, card.getOwner().getSpellDamage(game, 7));
                await Patchouli.tryMix(game,card);
            },PileFlag.both | PileFlag.character)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 5 水精公主 元素法术，使所有友方角色获得减伤+3直到你的下个回合开始
    /// </summary>
    public class PrincessUndine : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override string[] tags { get; set; } = new string[] { Patchouli.WATER };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(async (game,card)=>
            {
                await card.getOwner().getMaster().addBuff(game,new CodeBuff(ID,
                    new DamageReduceModifier(3),
                    new RemoveBuffBefore<HearthStoneRule.TurnStartEventArg>(PileFlag.field,(g,c,b,a)=>
                    {
                        return a.player == card.getOwner();
                    },ID)));
                foreach (var servant in card.getOwner().getField())
                {
                    await servant.addBuff(game,new CodeBuff(ID,
                        new DamageReduceModifier(3),
                        new RemoveBuffBefore<HearthStoneRule.TurnStartEventArg>(PileFlag.field,(g,c,b,a)=>
                        {
                            return a.player == card.getOwner();
                        },ID)));
                }
                await Patchouli.tryMix(game,card);
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 5 风灵角笛 元素法术，使目标随从获得+3/+6
    /// </summary>
    public class SylphyHorn : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { Patchouli.WOOD };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async(game,card,target)=>
            {
                await target.addBuff(game,new CodeBuff(ID,new AttackModifier(3),new LifeModifier(6)));
                await Patchouli.tryMix(game,card);
            }, PileFlag.both | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 5 巨石震怒 元素法术，召唤一个3/9具有嘲讽且无法攻击的巨石
    /// </summary>
    public class TrilithonShake : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override string[] tags { get; set; } = new string[] { Patchouli.EARTH };

        private IEffect[] effects1 = new IEffect[]{
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await card.getOwner().createToken(game, game.getDefine<RockElement>(), card.getOwner().getField().count);
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 岩石元素 衍生随从，无法攻击
    /// </summary>
    public class RockElement : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SERVANT | 0x657;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 9;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { TAG.ELEMENT };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT, KEYWORD.CANTATTACK };
    }
    /// <summary>
    /// 5 金属疲劳 元素法术，对所有敌方随从造成3点伤害
    /// </summary>
    public class MetalFatigue : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override string[] tags { get; set; } = new string[] { Patchouli.METAL };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await game.getOpponent(card.getOwner()).getField().damage(game, card, card.getOwner().getSpellDamage(game, 3));
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 666 小恶魔 战吼：发现一张墓地中的法术并置入你的手牌。
    /// </summary>
    public class Koakuma : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SERVANT | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 6;
        public override int life { get; set; } = 6;
        public override string[] tags { get; set; } = new string[] { TAG.DEMON };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            Player player = card.getOwner();
            var discovered = await player.searchPile(game, player.getGrave().Where(c => c.isSpell()), title: "选择一张墓地中的法术加入手牌");
            if (!player.getHand().isFull)
            {
                await player.getGrave().moveTo(game, discovered.choose, player.getHand());
                await discovered.choose.addBuff(game, new CodeBuff(ID, new CostModifier(-3)));
            }
        }
    }
    /// <summary>
    /// 12 皇家烈焰 对敌方英雄造成15点伤害。
    /// </summary>
    public class RoyalFlare : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 12;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await game.getOpponent(card.owner as Player).getMaster().damage(game, card, card.owner.getSpellDamage(game, 15));
        }
    }
    /// <summary>
    /// 12 沉静月神 沉默并消灭所有敌方随从。
    /// </summary>
    public class SilentSelene : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 12;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await game.getOpponent(card.getOwner()).getField().silence(game);
            await game.getOpponent(card.getOwner()).getField().die(game);
        }
    }
    /// <summary>
    /// 111 元素精灵 战吼：将一张随机0费元素法术置入手牌
    /// </summary>
    public class ElementSprite : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SERVANT | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = new string[] { TAG.ELEMENT };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static Task effect(CardEngine game, Card card)
        {
            return card.getOwner().addRandomCardToHand(game, game.getDefines(new int[] { AutumnEdge.ID, SpringWind.ID, WinterElement.ID, SummerRed.ID, DoyouSpear.ID }));
        }
    }
    /// <summary>
    /// 2 大图书馆 从牌库5张牌中选择一张，抽这张牌
    /// </summary>
    public class TheGreatLibrary : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;

        private IEffect[] effects1 = new IEffect[] {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            //var discovered = await card.getOwner().discover(game, card.getOwner().deck, 5, "查询图书");
            var discover = await card.getOwner().searchPile(game, card.getOwner().getDeck(), false, 5, "书海里寻找智慧");
            await card.getOwner().draw(game, discover.choose);

        }
    }
    /// <summary>
    /// 331 图书馆保卫者 战吼：你手牌中每有一张法术牌，便获得+1生命值
    /// </summary>
    public class LibraryProtector : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SERVANT | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await card.addBuff(game, new CodeBuff(ID, new LifeModifier(card.getOwner().getHand().Where(c => c.isSpell()).Count())));
        }
    }
    /// <summary>
    /// 4 奥术知识 从牌库5张牌中选择一张牌，重复2次，然后抽取所有被选中的牌。
    /// </summary>
    public class ArcaneKnowledge : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x018;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;

        private IEffect[] effects1 = new IEffect[] {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            for (int i = 0; i < 2; i++)
            {
                var discovered = await card.getOwner().searchPile(game, card.getOwner().getDeck(), false, 5, "知识中寻求方法");
                await card.getOwner().draw(game, discovered.choose);
            }
        }
    }
    /// <summary>
    /// 0 火符【夏之红】 使目标随从在本回合内获得攻击力+2
    /// </summary>
    public class SummerRed : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x019;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.FIRE };

        private IEffect[] effects1 = new IEffect[] {

            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                new BuffFixer().onEnable(game,target);
                await Patchouli.tryMix(game,card);
            },PileFlag.both | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        class BuffFixer
        {
            Trigger<HearthStoneRule.TurnEndEventArg> TurnEndTrigger { get; set; } = null;
            Buff buff = new CodeBuff(ID, new AttackModifier(2));
            public async void onEnable(CardEngine game, Card card)
            {
                await card.addBuff(game, buff);
                if (TurnEndTrigger == null)
                {
                    TurnEndTrigger = new Trigger<HearthStoneRule.TurnEndEventArg>(async arg =>
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
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x020;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.WATER };

        private IEffect[] effects1 = new IEffect[] {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.setFreeze(game, true);
                await Patchouli.tryMix(game,card);
            },PileFlag.both | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 0 木符【春之风】 使目标随从获得生命值+2
    /// </summary>
    public class SpringWind : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x021;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.WOOD };

        private IEffect[] effects1 = new IEffect[] {
            new LambdaSingleTargetEffect(async(game,card,target)=>
            {
                await target.addBuff(game,new CodeBuff(ID,new LifeModifier(2)));
                await Patchouli.tryMix(game,card);
            },PileFlag.both | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 0 金符【秋之刃】 随机对一个敌方随从造成2点伤害
    /// </summary>
    public class AutumnEdge : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x022;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.METAL };

        private IEffect[] effects1 = new IEffect[] {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await game.getOpponent(card.getOwner()).getField().random(game).damage(game, card, 2);
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 0 土符【土用之枪】 召唤一个1/1的宝石长枪
    /// </summary>
    public class DoyouSpear : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x023;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.EARTH };

        private IEffect[] effects1 = new IEffect[] {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await card.getOwner().createToken(game, game.getDefine<GemSpear>(), card.getOwner().getField().count);
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 111 衍生随从 宝石长枪
    /// </summary>
    public class GemSpear : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SERVANT | 0x658;
        public override int id { get; set; } = ID;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { TAG.ELEMENT };
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;

        private IEffect[] effects1 = new IEffect[0];

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 4 金木符【元素收割者】 对所有敌方随从造成2点伤害，每消灭一个敌方随从使一个随机友方随从获得+1/1
    /// </summary>
    public class ElementalHarvester : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x024;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.METAL, Patchouli.WOOD };

        private IEffect[] effects1 = new IEffect[] {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            Player player = card.getOwner();
            Player opponent = game.getOpponent(player);
            await opponent.getField().damage(game, card, player.getSpellDamage(game, 2));
            foreach (Card target in opponent.getField())
            {
                if (target.isDead(game))
                {
                    if (player.getField().count > 0)
                        await player.getField().random(game).addBuff(game, new CodeBuff(ID, new AttackModifier(1), new LifeModifier(1)));
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
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x025;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.WATER, Patchouli.METAL };

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(async (game,card)=>
            {
                Player opponent = game.getOpponent(card.getOwner());
                foreach(Card target in opponent.getField())
                {
                    await target.addBuff(game,
                        new CodeBuff(ID,new AttackModifier(-3),
                        new RemoveBuffBefore<HearthStoneRule.TurnStartEventArg>(PileFlag.field,
                        (g,c,b,a)=>{ return a.player == card.getOwner(); }, ID)));
                    if(target.getAttack(game)<=0)
                        await target.setDead(game,true);
                }
                await Patchouli.tryMix(game, card);
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 6 木火符【森林大火】 使所有友方随从获得+2/2，你每控制一个随从便对一个敌方随从造成1点伤害
    /// </summary>
    public class ForestBlaze : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x026;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.FIRE, Patchouli.WOOD };

        private IEffect[] effects1 = new IEffect[] {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                foreach(Card buffcard in card.getOwner().getField())
                {
                    await buffcard.addBuff(game,new CodeBuff(ID,new AttackModifier(2),new LifeModifier(2)));
                }
                await target.damage(game,card,card.getOwner().getField().count);
                await Patchouli.tryMix(game, card);
            },PileFlag.oppo | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 4 木土符【抽芽行尸】 使目标随从获得亡语：召唤一个目标随从的复制并使其获得+3+3
    /// </summary>
    public class BurgeoningRise : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x027;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.EARTH, Patchouli.WOOD };

        private IEffect[] effects1 = new IEffect[] {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.addKeywords(game,KEYWORD.DEATHRATTLE);
                await target.addBuff(game, new CodeBuff(ID,new DeathRattle(ID,async (g,c,b,p)=>
                {
                    var createToken = await target.getOwner().createToken(game,c.define,c.getOwner().getField().count);
                    if (createToken != null)
                        await createToken.card.addBuff(game,new CodeBuff(ID+1,new AttackModifier(3),new LifeModifier(3)));

                })));
                await Patchouli.tryMix(game, card);
            },PileFlag.both | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 6 水木符【水精灵】 使目标随从获得+3/6和嘲讽，并回复其所有生命值
    /// </summary>
    public class WaterElf : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x028;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.WATER, Patchouli.WOOD };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.addBuff(game,new CodeBuff(ID,new LifeModifier(6),new AttackModifier(3)));
                await target.addKeywords(game,KEYWORD.TAUNT);
                await target.heal(game,card,target.getLife(game)-target.getCurrentLife(game));
                await Patchouli.tryMix(game, card);
            }, PileFlag.both | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 4 水火符【燃素之雨】 造成7点伤害，随机分配给所有敌方角色
    /// </summary>
    public class PhlogisticRain : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x029;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.WATER, Patchouli.FIRE };

        private IEffect[] effects1 = new IEffect[] {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await game.getAllEnemies(card.getOwner()).damageByRandom(game, card, card.getOwner().getSpellDamage(game, 7));
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 火金符【圣爱尔摩火柱】 元素法术，对目标随从和敌方英雄造成4点伤害。
    /// </summary>
    public class StElmoPillar : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x030;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.METAL, Patchouli.FIRE };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await new Card[] { target, game.getOpponent(card.getOwner()).getMaster() }.damage(game,card,card.getOwner().getSpellDamage(game,4));
                await Patchouli.tryMix(game, card);
            }, PileFlag.both | PileFlag.field)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 6 火土符【环状熔岩带】 召唤2个2/2具有嘲讽并能在回合结束对随机敌方角色造成2点伤害的熔岩元素。
    /// </summary>
    public class LavaCromlech : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x031;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.FIRE, Patchouli.EARTH };

        private IEffect[] effects1 = new IEffect[] {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            for (int i = 0; i < 2; i++)
                await card.getOwner().createToken(game, game.getDefine<LavaElement>());
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 衍生随从 熔岩元素 当你的回合结束时对随机敌方角色造成2点伤害
    /// </summary>
    public class LavaElement : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SERVANT | 0x659;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { TAG.ELEMENT };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };

        private IEffect[] effects1 = new IEffect[]{
            new BeforeYourTurnEnd(PileFlag.field, async(game,card,buff,arg)=>
            {
                await game.getAllEnemies(card.getOwner()).random(game).damage(game, card, 2);
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 6 土金符【淡黄阵风】 每有一个敌方随从，便召唤一个3/1具有突袭的沙尘元素
    /// </summary>
    public class GingerGust : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x032;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.METAL, Patchouli.EARTH };

        private IEffect[] effects1 = new IEffect[] {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await card.getOwner().createToken(game, game.getOpponent(card.getOwner()).getField().count, game.getDefine<DustElement>());
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 衍生随从 沙尘元素 31突袭
    /// </summary>
    public class DustElement : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SERVANT | 0x660;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 1;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { TAG.ELEMENT };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.RUSH };

        private IEffect[] effects1 = new IEffect[0];

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 土水符【诺亚洪水】 沉默所有随从，然后召唤1个2/2并具有嘲讽的洪水元素
    /// </summary>
    public class NoachianDeluge : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x033;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { Patchouli.EARTH, Patchouli.WATER };

        private IEffect[] effects1 = new IEffect[] {
            new NoTargetEffect(effect)
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }

        static async Task effect(CardEngine game, Card card)
        {
            await game.getAllServants().silence(game);
            await card.getOwner().createToken(game, game.getDefine<FloodElement>());
            await Patchouli.tryMix(game, card);
        }
    }
    /// <summary>
    /// 衍生随从 洪水元素 嘲讽
    /// </summary>
    public class FloodElement : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SERVANT | 0x661;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override string[] tags { get; set; } = new string[] { TAG.ELEMENT };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };

        private IEffect[] effects1 = new IEffect[0];

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 454 噬法触手 突袭，当你的回合开始时随机弃一张法术牌
    /// </summary>
    public class BiteTentacle : ServantCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SERVANT | 0x034;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 5;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = new string[] { TAG.DEMON };
        public override string[] keywords { get; set; } = new string[] { KEYWORD.RUSH };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectBefore<HearthStoneRule.TurnStartEventArg>(PileFlag.field,(g1,c1,a1)=>
            {
                return a1.player==c1.getOwner();
            },(g1,c1,a1)=>
            {
                return c1.getOwner().randomDiscard(g1,1,c2=>c2.isSpell());
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 多重施法 1 随机使一张手牌中的法术牌花费-2，将一张伤害递增的“哮喘”洗入牌库，将另一张“多重施法”置入你的手牌。
    /// </summary>
    public class MultiCast : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x035;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(async (game,card)=>
            {
                Card target = card.getOwner().getHand().Where(c=>c.isSpell()&&c.getCost(game)>0).random(game);
                if(target!=null)
                    await target.addBuff(game, new CodeBuff(ID, new CostModifier(-2)));
                card.getOwner().setProp(nameof(Asthma),card.getOwner().getProp<int>(nameof(Asthma))+1);
                Card asthma = await card.owner.shuffleCardToDeck<Asthma>(game);
                await asthma.setProp(game,"damage",card.getOwner().getProp<int>(nameof(Asthma)));
                await card.getOwner().addCardToHand<MultiCast>(game);
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
    /// <summary>
    /// 哮喘 1 抽到自动释放，对你的英雄造成{card:damage}点伤害
    /// </summary>
    public class Asthma : SpellCardDefine
    {
        public const int ID = Patchouli.ID | CATEGORY.SPELL | 0x036;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.AUTOCAST };
        public override bool isToken { get; set; } = true;

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect((g1,c1)=>
            {
                return c1.getOwner().getMaster().damage(g1,c1,c1.getProp<int>(g1,"damage"));
            })
        };

        public override IEffect[] getEffects()
        {
            return effects1;
        }

        public override void setEffects(IEffect[] value)
        {
            effects1 = value;
        }
    }
}