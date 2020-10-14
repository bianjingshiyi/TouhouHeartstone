using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System.Threading.Tasks;
using System.Linq;

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
        public const int ID = Sanae.ID | CATEGORY.SKILL | 0x001;
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
        public const int ID = Sanae.ID | CATEGORY.SKILL | 0x002;
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
        public const int ID = Sanae.ID | CATEGORY.SKILL | 0x003;
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
        public const int ID = Sanae.ID | CATEGORY.SKILL | 0x004;
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
        public const int ID = Sanae.ID | CATEGORY.SKILL | 0x005;
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
        public const int ID = Sanae.ID | CATEGORY.SKILL | 0x006;
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
}