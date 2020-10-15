using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using Boo.Lang;
using System.Runtime.InteropServices;

namespace TouhouHeartstone.Builtin
{
    [IgnoreCardDefine]
    /// <summary>
    /// 东风谷早苗
    /// </summary>
    public class Sanae : MasterCardDefine
    {
        public const int ID = 0x00300000 | CATEGORY.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = WindStopMiracle.ID;
        public override IEffect[] effects { get; set; } = new Effect[0];

        /// <summary>
        /// 揭示对决
        /// 双方揭示牌库里的一张牌，法力消耗大的胜利
        /// </summary>
        /// <param name="game"></param>
        /// <param name="define">设定揭示的类别:0是所有类别；1是法术牌；2是随从牌；3是武器牌</param>
        /// <returns></returns>
        public static async Task<RevealDuelArg> revealDuel(THHPlayer player, THHGame game, int define = 0)
        {
            Card youCard = await player.Reveal(game, player.deck, define);
            Card oppoCard = await game.getOpponent(player).Reveal(game, game.getOpponent(player).deck, define);
            bool result = false;

            if (youCard.getCost(game) > oppoCard.getCost(game))
            {
                result = true;
            }

            var eventArg = await game.triggers.doEvent(new RevealDuelArg() { player = player, youCard = youCard, oppoCard = oppoCard, result = result }, async arg =>
            {
                game.logger.log(player + "发动了对决，她揭示了：" + youCard + "。她的对手揭示了：" + oppoCard + "。结果是" + result);
            });
            return eventArg;
        }

        public class RevealDuelArg : EventArg, IDescribableEventArg
        {
            /// <summary>
            /// 对决发起方
            /// </summary>
            public THHPlayer player;  
            /// <summary>
            /// 发起方揭示的牌
            /// </summary>
            public Card youCard;  
            /// <summary>
            /// 另一方揭示的牌
            /// </summary>
            public Card oppoCard; 
            /// <summary>
            /// 是否胜利
            /// </summary>
            public bool result;   
            public ICard getCard(IGame game, IPlayer viewer)
            {
                return null;
            }

            public ICard[] getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            public string toString(IGame game, IPlayer viewer)
            {
                return player.name + "发动了对决，她揭示了:{ card:"  + youCard.define.id + "} " + "。她的对手揭示了：{ card:" + oppoCard + "}。结果是" + result;
            }
        }
    }
    /// <summary>
    /// 风止奇迹：治愈2点生命值。
    /// </summary>
    public class WindStopMiracle : SkillCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
                                                {
                                                    await target.heal(game,card,2);
                                                },PileFlag.both | PileFlag.character)
        };
    }

    /// <summary>
    /// 星之仪式
    /// 信仰对决 将一张全新的“星之仪式”置入手牌；获胜时改为抽一张法力值消耗最高的牌
    /// </summary>
    public class StarCeremony : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            THHPlayer you = card.getOwner();
            var duel = await Sanae.revealDuel(you,game);
            if (duel.result)
            {
                //遍历牌库寻找法力消耗最高的牌
                Card _card = card.getOwner().deck.top;
                foreach (Card c in card.getOwner().deck)
                {
                    if (c.getCost(game) > _card.getCost(game))
                    {
                        _card = c;
                    }
                }
                await card.getOwner().draw(game, _card);
            }
            else
            {
                await you.addCardToHand<StarCeremony>(game);
            }
        }
    }

    /// <summary>
    /// 灰色秘术
    /// 治愈2点生命值。本回合你每揭示过一次，重复一遍。
    /// </summary>
    public class GrayThaumaturgy : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                int count = game.triggers.getRecordedEvents().skipUntilLast(e=> e is THHGame.TurnStartEventArg turnStart)
                    .OfType<THHPlayer.RevealArg>().Count();

                await target.heal(game,card,2);
                for(int i=0; i < count; i++)
                {
                    await target.heal(game,card,2);
                }
            })
        };
       
    }

    /// <summary>
    /// 商业繁荣祈愿
    /// 信仰对决（随从牌） 对目标随从造成3点伤害；获胜时再治愈你的英雄4点生命值。
    /// </summary>
    public class CharmofGoodCommerce : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                THHPlayer you = card.getOwner();
                var duel = await Sanae.revealDuel(you, game,2);
                if (duel.result)
                {
                    await you.master.heal(game,card,4);
                }
                await target.damage(game,card,3);
                
            })
        };    
    }

    /// <summary>
    /// 五谷丰穰
    /// 治愈10点生命值，分配到所有友方角色身上。
    /// </summary>
    public class BumperCropRiceShower : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            for(int i=0; i<10; i++)
            {
                await game.getAllAllys(card.getOwner()).Where(e => e.isDamage(game)).random(game).heal(game, card, 1);
            }
        }
    }

    /// <summary>
    /// 客星之奇迹
    /// 信仰对决（随从牌） 对所有敌方随从造成2点伤害；获胜时再抽一张牌。
    /// </summary>
    public class DaytimeGuestStars : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            THHPlayer you = card.getOwner();
            var duel = await Sanae.revealDuel(you, game);
            await game.getOpponent(you).field.damage(game, card, 2);
            if (duel.result)
            {
                await you.draw(game);
            }
            
        }

    }

    /// <summary>
    /// 开海之奇迹
    /// 信仰对决（随从牌）对目标随从相邻的随从造成4点伤害；获胜时再对目标随从造成2点伤害。
    /// </summary>
    public class TheDayTheSeaSplit : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                THHPlayer you = card.getOwner();
                var duel = await Sanae.revealDuel(you, game,2);

                foreach( Card t in target.getNearbyCards())
                {
                    await t.damage(game,card,4);
                }
                if (duel.result)
                {
                    await target.damage(game,card,2);
                }
                

            })
        };
    }

    /// <summary>
    /// 虔诚的参拜客
    /// 每当一个友方角色获得治疗后，攻击力+1。
    /// </summary>
    public class DevoutPilgrim : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 4;
        public override int attack { get; set; } = 2;
        public override int life { get; set; } = 6;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new THHEffectAfter<THHCard.HealEventArg>(PILE.FIELD, (game, card, arg) =>
            {
                return arg.source.getOwner() == card.getOwner() && arg != null;
            },null,async(game,card,arg)=>
            {
                foreach(Card c in arg.cards)
                {
                    if (c.getOwner() == card.getOwner())
                    {
                        await card.addBuff(game,new GeneratedBuff(ID,new AttackModifier(1)));
                    }
                }
            })
        };
    }


    /// <summary>
    /// 神明招来 突
    /// 召唤手牌中的一张随机神明随从牌的复制，并使其获得突袭。
    /// </summary>
    public class SummonTakeminakata_Rush : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            THHPlayer you = card.getOwner();
            Card c = you.hand.Where(c => c.hasTag(game, TAG.GOD)).random(game);
            var servant = await you.createToken(game, c.define);
            await servant.card.addBuff(game, new GeneratedBuff(ID, new KeywordsModifier(KeywordsModifier.ModifyType.add, KEYWORD.RUSH)));
        }
    }

    /// <summary>
    /// 神明招来 盾
    /// 召唤手牌中的一张随机神明随从牌的复制，并使其获得圣盾和嘲讽。
    /// </summary>
    public class SummonTakeminakata_Shield : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            THHPlayer you = card.getOwner();
            Card c = you.hand.Where(c => c.hasTag(game, TAG.GOD)).random(game);
            var servant = await you.createToken(game, c.define);
            await servant.card.addBuff(game, new GeneratedBuff(ID, new KeywordsModifier(KeywordsModifier.ModifyType.add, KEYWORD.SHIELD), new KeywordsModifier(KeywordsModifier.ModifyType.add, KEYWORD.TAUNT)));
        }
    }

    /// <summary>
    /// 被操纵的巨蛙
    /// 嘲讽，亡语：本局对战你每揭示过一次，对所有随从造成1点伤害。
    /// 我真不知道这东西该如何起名。。。。。。。
    /// </summary>
    
    public class BigFrog : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 5;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };
        public override string[] tags { get; set; } = { TAG.BEAST };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new DeathRattle(ID, async(game, card,p) =>
            {
                int count = game.triggers.getRecordedEvents().skipUntilLast(e=> e is THHGame.StartEventArg GameStart)
                    .OfType<THHPlayer.RevealArg>().Count();

                for(int i=0; i<count; i++)
                {
                    await game.getAllServants().damage(game,card,1);
                }
            })
        };
    }

    /// <summary>
    /// 妖怪信众
    /// 信仰对决(法术牌) 获胜时抽一张牌
    /// </summary>

    public class YoukaiDisciple : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 6;
        //public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };
        //public override string[] tags { get; set; } = { TAG.BEAST };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            THHPlayer you = card.getOwner();
            var duel = await Sanae.revealDuel(you, game,1);
            if (duel.result)
            {
                await you.draw(game);
            }
            
        }
    }

    /// <summary>
    /// 神代大蛇
    /// 战吼：若你本回合揭示过，则随机消灭一个攻击力小于等于3的敌方随从。
    /// </summary>

    public class OrochiOfAncientTimes : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 7;
        public override int attack { get; set; } = 6;
        public override int life { get; set; } = 7;
        //public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };
        public override string[] tags { get; set; } = { TAG.BEAST };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
             new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            bool isRevealed = (game.triggers.getRecordedEvents().skipUntilLast(e => e is THHGame.TurnStartEventArg TurnStart).OfType<THHPlayer.RevealArg>().Count() != 0);
            if (isRevealed)
            {
                await game.getOpponent(card.getOwner()).field.Where(c => c.getAttack(game) <= 3).random(game).setDead(game,true);
                await game.updateDeath(game);
            }
        }

    }


    /// <summary>
    /// 八坂神奈子
    /// 唯一
    /// 战吼：选择一种神迹并释放。从。
    /// </summary>

    public class KanakoYasaka : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override int attack { get; set; } = 8;
        public override int life { get; set; } = 8;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE };
        public override string[] tags { get; set; } = { TAG.GOD };
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
             new NoTargetEffect(effect)
        };
        static async Task effect(THHGame game, Card card)
        {
            List<Card> options = new List<Card> { game.createCard<KanakoYasaka_A>(), game.createCard<KanakoYasaka_B>(), game.createCard<KanakoYasaka_C>(), game.createCard<KanakoYasaka_D>() };
            Card choice = await card.getOwner().discover(game,options, 4, "见证神迹！");

            if(choice.define == game.getDefine<KanakoYasaka_A>())
            {
                await game.getOpponent(card.getOwner()).field.damage(game, card, 3);
            }else if (choice.define == game.getDefine<KanakoYasaka_B>())
            {
                await card.getOwner().createToken(game, game.getDefine<Onbashira>());
                await card.getOwner().createToken(game, game.getDefine<Onbashira>());
            }
            else if (choice.define == game.getDefine<KanakoYasaka_C>())
            {
                await game.getOpponent(card.getOwner()).master.damage(game, card, 6);
            }
            else
            {
                await card.getOwner().draw(game);
                await card.getOwner().draw(game);
            }
        }
    }

    /// <summary>
    /// 风雨之神迹
    /// 假卡，对所有敌方随从造成3点伤害。
    /// </summary>
    public class KanakoYasaka_A : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {

        };
    }

    /// <summary>
    /// 御柱之神迹
    /// 假卡，召唤2个3/3并具有嘲讽的御柱。
    /// </summary>
    public class KanakoYasaka_B : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {

        };
    }

    /// <summary>
    /// 战争之神迹
    /// 假卡，对敌方英雄造成6点伤害。
    /// </summary>
    public class KanakoYasaka_C : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {

        };
    }

    /// <summary>
    /// 革新之神迹
    /// 假卡，抽2张牌。
    /// </summary>
    public class KanakoYasaka_D : SpellCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SPELL | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 10;
        public override bool isToken { get; set; } = true;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {

        };
    }

    /// <summary>
    /// 御柱
    /// 衍生卡，3/3/3嘲讽
    /// </summary>
    public class Onbashira : ServantCardDefine
    {
        public const int ID = Sanae.ID | CATEGORY.SERVANT | 0x018;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 3;
        public override bool isToken { get; set; } = true;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };
    }
}