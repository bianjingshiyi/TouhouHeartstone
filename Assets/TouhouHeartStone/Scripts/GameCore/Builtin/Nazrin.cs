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
        public const int ID = 0x00800000 | CATEGORY.MASTER | 0x000;
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
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().createToken(game,game.getDefine<Nazrin_00>());
        }
    }

    /// <summary>
    /// 老鼠
    /// </summary>
    public class Nazrin_00 : ServantCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SERVANT | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 1;
        public override string[] tags { get; set; } = { TAG.BEAST };
        public override bool isToken { get; set; } = true;

    }

    /// <summary>
    /// 灵摆防御
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
                await target.addKeywords(game,KEYWORD.SHIELD);
            },PileFlag.both | PileFlag.field)
        };
       
    }

    /// <summary>
    /// 探知棒
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
            new THHEffectBefore<HearthStoneRule.DrawEventArg>(PILE.ITEM, (game, card, arg) =>
            {
                return arg.parent is HearthStoneRule.TurnStartEventArg && (arg.player == arg.card.owner);
            },null,
            async (game, card, arg) =>
            {
                //这是什么，把arg里的card换成选中的牌不就行了吗？
                arg.isCanceled = true;
                await card.getOwner().searchPile(game,card.getOwner().getDeck(),false,3);
            }),
            
        };
    }

    /// <summary>
    /// 搜寻鼠
    /// </summary>
    public class Nazrin_03 : ServantCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SERVANT | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = { TAG.BEAST };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<HearthStoneRule.TurnStartEventArg>(PILE.FIELD, (g, c, a) =>
            {
                return a.player == c.getOwner();
            },null,
            async (g,c,a)=>
            {
                await c.getOwner().addCardToHand<LuckyCoin>(g);
            })
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
        public override string[] keywords { get; set; } = new string[] { KEYWORD.MISSION }; 
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new MissionTrigger<HearthStoneRule.EnterFieldEventArg>((game,card,arg)=>
            {
                return arg.player == card.getOwner() && arg.card.getOwner() == card.getOwner();
            },async(game,card,arg)=>
            {
                await card.getOwner().addCardToHand<Nazrin_05>(game);
            },15)
        };
    }

    /// <summary>
    /// 宝塔
    /// </summary>
    public class Nazrin_05 : ItemCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.ITEM | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            int i = 0;
            foreach(Card c in card.getOwner().getHand())
            {
                i = c.getCost(game);
                await c.setDefine(game, game.getDefine<Nazrin_06>());
                await c.addModifier(game, new CostModifier(i - game.getDefine<Nazrin_06>().cost));
            }
        }
    }

    /// <summary>
    /// 宝藏
    /// </summary>
    public class Nazrin_06 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override bool isToken { get; set; } = true;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            CardDefine[] defines = game.getDefines().Where(c => c.hasKeyword(KEYWORD.UNIQUE) && c is ServantCardDefine s && !s.isToken).ToArray();
            Card[] unqueCards = game.createCards(defines);
            var arg = await card.getOwner().searchPile(game, unqueCards);
            await card.getOwner().tryAddCardToHand(game, arg.choose);
            arg.choose.addBuff(game, new GeneratedBuff(ID, new CostModifier(-arg.choose.define.getCost())));
        }
    }

    /// <summary>
    /// 洞穴寻宝
    /// </summary>
    public class Nazrin_07 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override bool isToken { get; set; } = true;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            CardDefine[] defines = game.getDefines().Where(c => ((c.id >> 5) == (Nazrin.ID >> 5) || (c.id >> 5) == (CATEGORY.CHARACTER_NEUTRAL >> 5)) && c is ServantCardDefine s && !s.isToken).ToArray();
            Card[] servantCards = game.createCards(defines);
            var arg1 = await card.getOwner().searchPile(game, servantCards,title: "探索地下洞穴");           
            await card.getOwner().tryAddCardToHand(game,arg1.choose);
            await arg1.choose.addBuff(game, new GeneratedBuff(ID, new CostModifier(5, true), new LifeModifier(5, true), new AttackModifier(5, true)));
        }
    }

    /// <summary>
    /// 稀有金属探测器
    /// </summary>
    public class Nazrin_08 : ServantCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SERVANT | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] keywords { get; set; } = { KEYWORD.CANTATTACK };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<HearthStoneRule.EnterFieldEventArg>(PILE.FIELD, (g, c, a) =>
            {
                return a.player == c.getOwner() && a.card.getOwner() == c.getOwner();
            },null,
            async (g,c,a)=>
            {
                await g.getAllEnemies(c.getOwner()).random(g).damage(g,c,1);
            })
        };
    }

    /// <summary>
    /// 贪食老鼠
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
            new DeathRattle(ID, async (game, card, p) =>
            {
                await card.getOwner().addCardToHand<Nazrin_10>(game);
            })
        };
    }

    /// <summary>
    /// 奶酪
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
                await target.heal(game,card,4);
                if(target.getCurrentLife(game) == target.getLife(game))
                {
                    await card.getOwner().createToken(game,game.getDefine<Nazrin_00>());
                }
            })
        };       
    }

    /// <summary>
    /// 鼠群呼唤
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

        static async Task effect(THHGame game, Card card)
        {
           await card.getOwner().tryEnterField(game, card.getOwner().getDeck(), card.getOwner().getDeck().Where(c=>c.getCost(game) <= 2 && c.isServant()).random(game),card.getOwner().getField().count);
           await card.getOwner().tryEnterField(game, card.getOwner().getDeck(), card.getOwner().getDeck().Where(c => c.getCost(game) <= 2 && c.isServant()).random(game), card.getOwner().getField().count);
        }
    }

    /// <summary>
    /// 鼠潮
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

        static async Task effect(THHGame game, Card card)
        {
            for(int i = 0; i < 2; i++)
            {
                var arg1 = await card.getOwner().createToken(game, game.getDefine<Nazrin_00>());
                await arg1.card.addBuff(game, new GeneratedBuff(ID, new AttackModifier(2)));
                await arg1.card.addKeywords(game, KEYWORD.RUSH);
            }
        }
    }

    /// <summary>
    /// 淘金热
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

        static async Task effect(THHGame game, Card card)
        {
            foreach(Card c in card.getOwner().getField())
            {
                await card.getOwner().addCardToHand<LuckyCoin>(game);
            }
        }
    }

    /// <summary>
    /// 寻宝探物
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

        static async Task effect(THHGame game, Card card)
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
    /// 便携鼠窝
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
            new THHEffectAfter<HearthStoneRule.EnterFieldEventArg>(PILE.FIELD, (game, card, arg) =>
            {
                return arg.player == card.getOwner() && arg.card.getOwner() == card.getOwner();
            },null,async(game,card,arg)=>
            {
                await card.getOwner().createToken(game,game.getDefine<Nazrin_00>());
            })
        };
    }

    /// <summary>
    /// 奶酪派对
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

        static async Task effect(THHGame game, Card card)
        {
            for(int i=0; i<3; i++)
            {
                CardDefine[] defines = game.getDefines().Where(c => ((c.id >> 5) == (Nazrin.ID >> 5) || (c.id >> 5) == (CATEGORY.CHARACTER_NEUTRAL >> 5)) && c is ServantCardDefine s && !s.isToken).ToArray();
                Card[] servantCards = game.createCards(defines);
                var arg1 = await card.getOwner().searchPile(game, servantCards);
                await card.getOwner().tryAddCardToHand(game, arg1.choose);               
            }
        }
    }

    /// <summary>
    /// 食物的诱惑
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

        static async Task effect(THHGame game, Card card)
        {
            foreach(Card c in card.getOwner().getField())
            {
                await c.addBuff(game, new GeneratedBuff(ID, new AttackModifier(2), new RemoveBuffAfter<HearthStoneRule.TurnEndEventArg>(PILE.MASTER, ID)));
                if(!c.hasKeyword(game,KEYWORD.RUSH))
                    await c.addKeywords(game, KEYWORD.RUSH);
            }
        }
    }

    /// <summary>
    /// 棒符【娜兹玲棒】
    /// </summary>
    public class Nazrin_18 : SpellCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SPELL | 0x018;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override bool isToken { get; set; } = true;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                await target.setDead(game,true);
                await card.getOwner().createToken(game,game.getDefine<Nazrin_00>());
                await game.updateDeath();
                
            })
        };
    }

    public class Nazrin_19 : ServantCardDefine
    {
        public const int ID = Nazrin.ID | CATEGORY.SERVANT | 0x019;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 7;
        public override int attack { get; set; } = 5;
        public override int life { get; set; } = 7;

        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<HearthStoneRule.UseEventArg>(PILE.FIELD, (game, card, arg) =>
            {
                return arg.player == card.getOwner() && arg.card.getOwner() == card.getOwner() && arg.card.hasKeyword(game,KEYWORD.UNIQUE) && arg.card.isServant();
            },null,async(game,card,arg)=>
            {
                await card.getOwner().tryAddCardToHand(game,arg.card);
                await arg.card.addBuff(game, new GeneratedBuff(ID, new CostModifier(5), new AttackModifier(5), new LifeModifier(5)));
            })
        };
    }
}