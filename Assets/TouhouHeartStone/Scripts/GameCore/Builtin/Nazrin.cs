using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone.Builtin
{
    public class Nazrin : MasterCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_NAZRIN | CATEGORY.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = Nazrin_Skill.ID;
    }

    public class Nazrin_Skill : SkillCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect, (game, card) => !card.getOwner().getField().isFull)
        };

        static async Task effect(CardEngine game, Card card)
        {
            await card.getOwner().createToken(game, game.getDefine<Nazrin_00>());
        }
    }

    /// <summary>
    /// 老鼠 1/1/1
    /// </summary>
    public class Nazrin_00 : ServantCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SERVANT | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = {TAG.BEAST};
        public override bool isToken { get; set; } = true;
    }

    /// <summary>
    /// 灵摆防御 使一个角色获得<b>护盾</b>。
    /// </summary>
    public class Nazrin_01 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                await target.addKeywords(game, KEYWORD.SHIELD);
            }, PileFlag.both|PileFlag.character)
        };
    }

    /// <summary>
    /// 探知棒 2/1/3 你回合开始时的抽牌改为从牌库三张随机牌中选择一张，每次触发消耗1点耐久。
    /// </summary>
    public class Nazrin_02 : ItemCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.ITEM | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectBefore<HearthStoneRule.DrawEventArg>(PILE.ITEM,
                (game, card, arg) =>
                {
                    return arg.parent is HearthStoneRule.TurnStartEventArg && (arg.player == card.owner);
                }, null,
                async (game, card, arg) =>
                {
                    var selected = await card.getOwner().askSelect(game, card.getOwner().getDeck(), title: "广告招租");
                    arg.card = selected;
                    await card.damage(game, card, 1);
                    await game.updateDeath();
                }),
        };
    }

    /// <summary>
    /// 搜寻鼠 3/2/4 每当你的回合开始时，将一张幸运铜钱置入你的手牌。
    /// </summary>
    public class Nazrin_03 : ServantCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SERVANT | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = {TAG.BEAST};

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<HearthStoneRule.TurnStartEventArg>(PILE.FIELD,
                (g, c, a) => { return a.player == c.getOwner(); }, null,
                async (g, c, a) => { await c.getOwner().addCardToHand<LuckyCoin>(g); })
        };
    }

    /// <summary>
    /// 宝塔搜寻队 
    /// </summary>
    public class Nazrin_04 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override string[] keywords { get; set; } = new string[] {KEYWORD.MISSION, KEYWORD.UNIQUE};

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new MissionTrigger<HearthStoneRule.EnterFieldEventArg>(
                (game, card, arg) =>
                {
                    return arg.player == card.getOwner() && arg.card.getOwner() == card.getOwner();
                }, async (game, card, arg) => { await card.getOwner().addCardToHand<Nazrin_05>(game); }, 15)
        };
    }

    /// <summary>
    /// 宝塔 <b>宣言</b>：将你所有手牌变形为宝藏。（保留原来的灵力值消耗。）
    /// </summary>
    public class Nazrin_05 : ItemCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.ITEM | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(CardEngine game, Card card)
        {
            int i = 0;
            foreach (Card c in card.getOwner().getHand())
            {
                i = c.getCost(game);
                await c.setDefine(game, game.getDefine<Nazrin_06>());
                await c.addBuff(game, new GeneratedBuff(ID, new CostModifier(i, true)));
            }
        }
    }

    /// <summary>
    /// 宝藏 从三张随机<b>唯一</b>仆从牌中选择一张，将其灵力值消耗为(0)点的复制置入你的手牌。
    /// </summary>
    public class Nazrin_06 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override bool isToken { get; set; } = true;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(CardEngine game, Card card)
        {
            CardDefine[] defines = game.getDefines().Where(c => c.hasKeyword(KEYWORD.UNIQUE) && c is ServantCardDefine s && !s.isToken).ToArray();
            Card discovered = await card.getOwner().discover(game, defines, title: "广告招租");
            await discovered.addBuff(game, new GeneratedBuff(ID, new CostModifier(0, true)));
            // List<Card> unqueCards = card.getOwner().getNormalRandomPool((g, d) =>
            // {
            //     return d is ServantCardDefine && d.hasKeyword(KEYWORD.UNIQUE);
            // }, game, 3, true).ToList();
            // var arg = await card.getOwner().searchPile(game, unqueCards);
            // await card.getOwner().tryAddCardToHand(game, arg.choose);
            // await arg.choose.addBuff(game, new GeneratedBuff(ID, new CostModifier(0, true)));
        }
    }

    /// <summary>
    /// 洞穴寻宝 从三张随机仆从牌中选择一张，将其5/5的复制置入你的手牌并使其灵力值消耗变为(5)点。
    /// </summary>
    public class Nazrin_07 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
     
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(CardEngine game, Card card)
        {
            var discovered = await card.getOwner().discover(game,
                game.getDefinesOf(card.getOwner().getMaster().id,CATEGORY.CHARACTER_NEUTRAL).Where(define => define is ServantCardDefine s && !s.isToken));
            await discovered.addBuff(game,
                new GeneratedBuff(ID, new CostModifier(5, true), new LifeModifier(5, true),
                    new AttackModifier(5, true)));
            // List<Card> servantCards = card.getOwner().getNormalRandomPool((g, d) =>
            // {
            //     return d is ServantCardDefine;
            // },game, 3);
            // var arg1 = await card.getOwner().searchPile(game, servantCards, title: "探索地下洞穴");
            // await card.getOwner().tryAddCardToHand(game, arg1.choose);
            // await arg1.choose.addBuff(game,
            //     new GeneratedBuff(ID, new CostModifier(5, true), new LifeModifier(5, true),
            //         new AttackModifier(5, true)));
        }
    }

    /// <summary>
    /// 稀有金属探测器 <b>无法攻击</b>每当你召唤一个仆从后，对敌方随机角色造成1点伤害。
    /// </summary>
    public class Nazrin_08 : ServantCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SERVANT | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] keywords { get; set; } = {KEYWORD.CANTATTACK};

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<HearthStoneRule.EnterFieldEventArg>(PILE.FIELD,
                (g, c, a) => { return a.player == c.getOwner() && a.card.getOwner() == c.getOwner() && a.card != c; }, null,
                async (g, c, a) => { await g.getAllEnemies(c.getOwner()).random(g).damage(g, c, 1); })
        };
    }

    /// <summary>
    /// 贪食老鼠 2/2/3 <b>遗言</b>:将一张奶酪置入你的手牌。
    /// </summary>
    public class Nazrin_09 : ServantCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SERVANT | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override int attack { get; set; } = 2;

        public override int life { get; set; } = 3;

        //public override string[] keywords { get; set; } = { KEYWORD.CANTATTACK };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID, async (game, card, p) => { await card.getOwner().addCardToHand<Nazrin_10>(game); })
        };
    }

    /// <summary>
    /// 奶酪 恢复4点生命值，如果恢复了所有生命值，则召唤一个1/1的老鼠。
    /// </summary>
    public class Nazrin_10 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override bool isToken { get; set; } = true;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                await target.heal(game, card, 4);
                if (target.getCurrentLife(game) == target.getLife(game))
                {
                    await card.getOwner().createToken(game, game.getDefine<Nazrin_00>());
                }
            })
        };
    }

    /// <summary>
    /// 鼠群呼唤 从你的牌库将2张灵力值消耗小于或等于2的仆从置入战场。
    /// </summary>
    public class Nazrin_11 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(CardEngine game, Card card)
        {
            await card.getOwner().tryEnterField(game, card.getOwner().getDeck(),
                card.getOwner().getDeck().Where(c => c.getCost(game) <= 2 && c.isServant()).random(game),
                card.getOwner().getField().count);
            await card.getOwner().tryEnterField(game, card.getOwner().getDeck(),
                card.getOwner().getDeck().Where(c => c.getCost(game) <= 2 && c.isServant()).random(game),
                card.getOwner().getField().count);
        }
    }

    /// <summary>
    /// 鼠潮 召唤2只1/1的老鼠并使其获得增益：+1攻击力和<b>速攻</b>。
    /// </summary>
    public class Nazrin_12 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(CardEngine game, Card card)
        {
            for (int i = 0; i < 2; i++)
            {
                var arg1 = await card.getOwner().createToken(game, game.getDefine<Nazrin_00>());
                await arg1.card.addBuff(game,
                    new GeneratedBuff(ID, new AttackModifier(1),
                        new KeywordsModifier(KeywordsModifier.ModifyType.add, KEYWORD.RUSH)));
            }
        }
    }

    /// <summary>
    /// 淘金热 你每控制一个仆从，便将一张幸运铜钱置入你的手牌。
    /// </summary>
    public class Nazrin_13 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(CardEngine game, Card card)
        {
            foreach (Card c in card.getOwner().getField())
            {
                await card.getOwner().addCardToHand<LuckyCoin>(game);
            }
        }
    }

    /// <summary>
    /// 寻宝探物 抽2张灵力值消耗最低的牌。
    /// </summary>
    public class Nazrin_14 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(CardEngine game, Card card)
        {
            Player player = card.getOwner();
            var lowCostCards = player.getDeck().OrderBy(c => c.getCost(game)).Take(2);
            foreach (var c in lowCostCards)
            {
                await player.draw(game, c);
            }
        }
    }

    /// <summary>
    /// 便携鼠窝 每当你使用一张仆从牌后，召唤一只1/1的老鼠。
    /// </summary>
    public class Nazrin_15 : ServantCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SERVANT | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 5;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<HearthStoneRule.UseEventArg>(PILE.FIELD,
                (game, card, arg) =>
                {
                    return arg.player == card.getOwner() && arg.card.getOwner() == card.getOwner() &&
                           arg.card.isServant() && arg.card != card;
                }, null,
                async (game, card, arg) => { await card.getOwner().createToken(game, game.getDefine<Nazrin_00>()); })
        };
    }

    /// <summary>
    /// 奶酪派对 从三张随机仆从牌中选择一张，重复2次。
    /// </summary>
    public class Nazrin_16 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(CardEngine game, Card card)
        {
            for (int i = 0; i < 2; i++)
            {
                await card.getOwner().discover(game,
                    game.getDefines().Where(define => define is ServantCardDefine s && !s.isToken));
            }
            
            // List<Card> servantCards = new List<Card>();
            // for (int i = 0; i < 3; i++)
            // {
            //     servantCards = card.getOwner().getNormalRandomPool((g, d) =>
            //     {
            //         return d is ServantCardDefine;
            //     }, game, 3);
            //     var arg1 = await card.getOwner().searchPile(game, servantCards);
            //     await card.getOwner().tryAddCardToHand(game, arg1.choose);
            // }
        }
    }

    /// <summary>
    /// 食物的诱惑 使你所有仆从获得增益：+2攻击力和<b>速攻</b>。
    /// </summary>
    public class Nazrin_17 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(CardEngine game, Card card)
        {
            foreach (Card c in card.getOwner().getField())
            {
                await c.addBuff(game,
                    new GeneratedBuff(ID, new AttackModifier(2),new KeywordsModifier(KeywordsModifier.ModifyType.add, KEYWORD.RUSH)));
            }
        }
    }

    /// <summary>
    /// 棒符【娜兹玲棒】 消灭一个随从并召唤一只老鼠
    /// </summary>
    public class Nazrin_18 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x018;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                await target.setDead(game, true);
                await game.updateDeath();
                await card.getOwner().createToken(game, game.getDefine<Nazrin_00>());
            }, PileFlag.both | PileFlag.field)
        };
    }

    /// <summary>
    /// 寅丸星 <b>唯一</b>每当你使用一张<b>唯一</b>仆从牌后，将其5/5的复制置入你的手牌并使其灵力值消耗变为(5)点。
    /// </summary>
    public class Nazrin_19 : ServantCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SERVANT | 0x019;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 7;
        public override int attack { get; set; } = 5;
        public override int life { get; set; } = 7;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<HearthStoneRule.UseEventArg>(PILE.FIELD,
                (game, card, arg) =>
                {
                    return arg.player == card.getOwner() && arg.card.getOwner() == card.getOwner() &&
                           arg.card.hasKeyword(game, KEYWORD.UNIQUE) && arg.card.isServant() && arg.card != card;
                }, null, async (game, card, arg) =>
                {
                    var c = game.createCard(arg.card.define);
                    await card.getOwner().tryAddCardToHand(game,c);
                    await c.addBuff(game,
                        new GeneratedBuff(ID, new CostModifier(5, true), new AttackModifier(5, true),
                            new LifeModifier(5, true)));
                })
        };
    }
}