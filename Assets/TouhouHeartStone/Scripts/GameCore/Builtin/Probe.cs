using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;



namespace TouhouHeartstone.Builtin.Proble
{
    [IgnoreCardDefine]
    public class Probe : MasterCardDefine
    {
        public const int ID = 0x04200000 | CardCategory.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 15;
        public override int skillID { get; set; } = ProbeShoot.ID;//英雄技能的ID
        public override IEffect[] effects { get; set; } = new IEffect[0];
        
        
        public static bool isJoinForce(Pile field, Card card)
        {
            int i = 0;
            foreach (var servant in field)
            {
                if (servant.define.id == card.define.id)
                {
                    i++;
                }
            }

            if (i > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        

    }
        /// <summary>
        /// 技能：射箭
        /// </summary>
        [Obsolete]
        [IgnoreCardDefine]
        public class ProbeShoot : SkillCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SKILL | 0x000;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 2; //技能花费
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            new NoTargetEffect(effect)//这不是个指向性技能
            };
            static async Task effect(THHGame game, Card card)
            {
                await game.getOpponent(card.owner as THHPlayer).master.damage(game, card, 2);
            }
        }

        /// <summary>
        /// 召唤水晶塔，法术
        /// 2费，召唤一个0/6,带有嘲讽的水晶塔
        /// 不仅为周围建筑供电，还提供WiFi
        /// </summary>
        [IgnoreCardDefine]
        public class BulidPylon : SpellCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SPELL | 0x001;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 2; //花费
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            new NoTargetEffect(effect)
            };

            static async Task effect(THHGame game, Card card)
            {
                await card.getOwner().createToken(game, game.getCardDefine<Pylon>(), card.getOwner().field.count);
            }

        }

        /// <summary>
        /// 水晶塔，随从
        /// 2/0/6,嘲讽
        /// </summary>
        [IgnoreCardDefine]
        public class Pylon : ServantCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SERVANT | 0x002;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 2;
            public override int attack { get; set; } = 0;//随从的攻击力
            public override int life { get; set; } = 6;//随从的血量上限
            public override bool isToken { get; set; } = true;//是衍生物吗？（当然）
                                                              //public override string[] tags { get; set; } = new string[];//种族标签
            public override string[] keywords { get; set; } = new string[] { Keyword.TAUNT };//关键词标签：
        }

        /// <summary>
        /// 粒子光束
        /// 2费，造成3点伤害
        /// 对矿物有加成
        /// </summary>
        [IgnoreCardDefine]
        public class ParticulateLaser : SpellCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SPELL | 0x003;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 2;
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            //存在目标的法术
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.damage(game, card, card.getOwner().getSpellDamage(game, 3));//对目标造成3点伤害
            })
            };
        }

        /// <summary>
        /// 灵能震荡
        /// 4费，对所有角色造成3点伤害
        /// Power Overflowing
        /// </summary>
        [IgnoreCardDefine]
        public class PsionicShock : SpellCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SPELL | 0x004;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 4;
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            new NoTargetEffect(effect)
            };

            static async Task effect(THHGame game, Card card)
            {
                //双向法术这块必须整完！
                await game.getAllCharacters().damage(game, card, card.getOwner().getSpellDamage(game, 3));
            }
        }

        /// <summary>
        /// 水晶塔过载
        /// 4费，召唤一个水晶塔，再使所有水晶塔获得+3攻击力
        /// 就连我的建筑在战斗都比你有用！
        /// </summary>
        [IgnoreCardDefine]
        public class PylonOverload : SpellCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SPELL | 0x005;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 4; //花费
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            new NoTargetEffect(effect)
            };

            static async Task effect(THHGame game, Card card)
            {
                //召唤水晶塔
                await card.getOwner().createToken(game, game.getCardDefine<Pylon>(), card.getOwner().field.count);

                //buff
                foreach (var servant in card.getOwner().field)
                {
                    if (servant.define.id == Pylon.ID)
                    {
                        await servant.addBuff(game, new GeneratedBuff(ID, new AttackModifier(3)));
                    }

                }
            }

        }

        /// <summary>
        /// 机械哨兵
        /// 4/2/4，战吼：召唤一个0/2具有嘲讽的幻象
        /// “星灵的凤凰战机比地上跑的虫子都多” ——倒霉的机枪兵
        /// </summary>
        [IgnoreCardDefine]
        public class Sentry : ServantCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SERVANT | 0x006;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 4;
            public override int attack { get; set; } = 2;//随从的攻击力
            public override int life { get; set; } = 4;//随从的血量上限
            public override bool isToken { get; set; } = false;//是衍生物吗？
            public override string[] tags { get; set; } = new string[] { CardTag.MACHINE };//种族标签
                                                                                           //public override string[] keywords { get; set; } = new string[] {Keyword.TAUNT};//关键词标签：
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            new NoTargetEffect(effect)
            };
            static async Task effect(THHGame game, Card card)
            {
                await card.getOwner().createToken(game, game.getCardDefine<Vision>(), card.getOwner().field.count);
            }
        }

        /// <summary>
        /// 幻象
        /// 0/0/2
        /// 衍生物
        /// </summary>
        [IgnoreCardDefine]
        public class Vision : ServantCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SERVANT | 0x007;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 0;
            public override int attack { get; set; } = 0;//随从的攻击力
            public override int life { get; set; } = 2;//随从的血量上限
            public override bool isToken { get; set; } = true;//是衍生物吗？
                                                              //public override string[] tags { get; set; } = new string[];//种族标签
            public override string[] keywords { get; set; } = new string[] { Keyword.TAUNT };//关键词标签：

        }

        /// <summary>
        /// 先知机器人
        /// 4/3/3，战吼：抽一张法力消耗最高的牌
        /// 它一直担心太阳会在某一天熄灭
        /// </summary>
        [IgnoreCardDefine]
        public class ProphetRobot : ServantCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SERVANT | 0x008;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 4;
            public override int attack { get; set; } = 3;//随从的攻击力
            public override int life { get; set; } = 3;//随从的血量上限
            public override bool isToken { get; set; } = false;//是衍生物吗？
            public override string[] tags { get; set; } = new string[] { CardTag.MACHINE };//种族标签
                                                                                           //public override string[] keywords { get; set; } = new string[] {Keyword.TAUNT};//关键词标签：
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            new NoTargetEffect(effect)
            };
            static async Task effect(THHGame game, Card card)
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
        }

        /// <summary>
        /// 巨像
        /// 8/8/8，唯一，回合结束时，对所有敌方角色造成3点伤害
        /// 3台巨像能轻易毁灭一个文明，但不一定能A得死人
        /// </summary>
        [IgnoreCardDefine]
        public class Colossus : ServantCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SERVANT | 0x009;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 8;
            public override int attack { get; set; } = 8;//随从的攻击力
            public override int life { get; set; } = 8;//随从的血量上限
            public override bool isToken { get; set; } = false;//是衍生物吗？
            public override string[] tags { get; set; } = new string[] { CardTag.MACHINE };//种族标签
            public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };//关键词标签：
            public override IEffect[] effects { get; set; } = new IEffect[]{
            //回合结束扳机
            new BeforeYourTurnEnd(async(game,card,arg)=>
            {
                await game.getAllEnemies(card.getOwner()).damage(game, card, 3);
            })
        };
        }

        /// <summary>
        /// 能量反馈
        /// 1费，对一个随从造成等同其攻击力的伤害
        /// 请不要随便给自己加能量条。
        /// </summary>
        [IgnoreCardDefine]
        public class EnergyAutophagy : SpellCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SPELL | 0x010;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 1;
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            //存在目标的法术
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                int dam = target.getAttack(game);
                await target.damage(game, card, card.getOwner().getSpellDamage(game, dam));
            }, PileFlag.both | PileFlag.field)//后缀表示只能指向随从
            };
        }

        /// <summary>
        /// 普罗托斯
        /// 5/0/3，唯一，所有随从获得+2攻击力，战吼：召唤2个水晶塔
        /// 征战无数的传奇探机最终还是逃不过死亡的命运：被投入坟头枢纽
        /// </summary>
        [IgnoreCardDefine]
        public class Protoss : ServantCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SERVANT | 0x011;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 5;
            public override int attack { get; set; } = 0;//随从的攻击力
            public override int life { get; set; } = 3;//随从的血量上限
            public override bool isToken { get; set; } = false;//是衍生物吗？
            public override string[] tags { get; set; } = new string[] { CardTag.MACHINE };//种族标签
            public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };//关键词标签：
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            new NoTargetEffect(effect),
            new Halo(new GeneratedBuff(ID,new AttackModifier(2)), PileFlag.self | PileFlag.field)
            };
            static async Task effect(THHGame game, Card card)
            {
                await card.getOwner().createToken(game, game.getCardDefine<Pylon>(), card.pile.indexOf(card) + 1);
                await card.getOwner().createToken(game, game.getCardDefine<Pylon>(), card.pile.indexOf(card) - 1);
            }
        }


        /// <summary>
        /// 重构光线
        /// 0费，完全修好一个友方机械。(警告:不要对机械以外的随从使用！)
        /// “只要给够钱，我连黄金舰队都能修。”——SCV
        /// </summary>
        [IgnoreCardDefine]
        public class ReconstructRay : SpellCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SPELL | 0x012;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 0;
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            //存在目标的法术
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                int heal = target.getLife(game);
                if(((IList)target.getTags(game)).Contains(CardTag.MACHINE)){
                    await target.heal(game,heal);
                }else{
                    await target.damage(game, card, card.getOwner().getSpellDamage(game, heal));
                }

            }, PileFlag.self | PileFlag.field)//后缀表示只能指向随从
            };
        }

        /// <summary>
        /// 山寨版本
        /// 3费，发现墓地里一张机械牌，召唤它并使其变为3/3
        /// 靠着偷、抢、抄，塔达林居然拼凑出了死亡舰队。
        /// </summary>
        [IgnoreCardDefine]
        public class Imitation : SpellCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SPELL | 0x013;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 0;
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            new NoTargetEffect(effect)
            };
            static async Task effect(THHGame game, Card card)
            {
                var discovered = await card.getOwner().discover(game, card.getOwner().grave.Where(c => c.isServant() && ((IList)c.getTags(game)).Contains(CardTag.MACHINE)));
                var theReborned = (await card.getOwner().reborn(game, discovered, card.getOwner().field)).card;
                await theReborned.addBuff(game, new GeneratedBuff(ID, new AttackModifier(3, true), new LifeModifier(3, true)));

            }
        }

        /// <summary>
        /// 守护之壳
        /// 3费，使一个随从获得+3生命值，如果它是机械，再获得+4攻击力
        /// 只是强化护盾，既不守护你的脸，又不是壳。
        /// </summary>
        [IgnoreCardDefine]
        public class ProtectiveShell : SpellCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SPELL | 0x014;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 3;
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            //存在目标的法术
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.addBuff(game,new GeneratedBuff(ID,new LifeModifier(3)));
                if(((IList)target.getTags(game)).Contains(CardTag.MACHINE)){
                    await target.addBuff(game,new GeneratedBuff(ID,new AttackModifier(4)));
                }
            }, PileFlag.self | PileFlag.field)//后缀表示只能指向随从
            };
        }

        /// <summary>
        /// 幽灵猫西塞尔
        /// 7/2/2，唯一，战吼：获得一个机械的控制权，然后将其攻击力与生命值变为3
        /// “为什么幻想乡没有电话？”
        /// </summary>
        [IgnoreCardDefine]
        public class Sissel : ServantCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SERVANT | 0x015;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 7;
            public override int attack { get; set; } = 2;//随从的攻击力
            public override int life { get; set; } = 2;//随从的血量上限
            public override bool isToken { get; set; } = false;//是衍生物吗？
                                                               //public override string[] tags { get; set; } = new string[] {  };//种族标签
            public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };//关键词标签：
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            //存在目标的法术
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.getOwner().field.moveTo(game, target, card.getOwner().field);
                await target.addBuff(game , new GeneratedBuff(ID,new AttackModifier(3,true),new LifeModifier(3,true)));
            }, PileFlag.oppo | PileFlag.field,onCheckTarget:(g,c,t)=> t.define.hasTag(CardTag.MACHINE))//后缀表示只能指向随从
            };
        }

        /// <summary>
        /// 沉默的计牌器
        /// 4/3/3，每当你的对手使用一张牌时，自身获得+1/+2
        /// “......你刚刚伏笔了”
        /// </summary>
        [IgnoreCardDefine]
        public class SilentCardCounter : ServantCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SERVANT | 0x016;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 4;
            public override int attack { get; set; } = 3;//随从的攻击力
            public override int life { get; set; } = 3;//随从的血量上限
            public override bool isToken { get; set; } = false;//是衍生物吗？
            public override string[] tags { get; set; } = new string[] { CardTag.MACHINE };//种族标签
                                                                                           //public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };//关键词标签：
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            //测试表示：这张牌的扳机是使用卡牌生效后。
            new AddBuffAfter<THHPlayer.UseEventArg>(PileName.FIELD,(g,c,a)=>
            {
                return a.player == g.getOpponent(c.getOwner());
            },new GeneratedBuff(ID,new AttackModifier(1),new LifeModifier(2)))
            };
        }

        /// <summary>
        /// 流水线
        /// 3/0/3，每当你使用一张牌时，召唤一个1/1的迷你机器人
        /// 它被关在工厂里一万年了...
        /// </summary>
        [IgnoreCardDefine]
        public class AssemblyLine : ServantCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SERVANT | 0x017;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 3;
            public override int attack { get; set; } = 0;//随从的攻击力
            public override int life { get; set; } = 3;//随从的血量上限
            public override bool isToken { get; set; } = false;//是衍生物吗？
                                                               //public override string[] tags { get; set; } = new string[] { CardTag.MACHINE };//种族标签
                                                               //public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };//关键词标签：
            public override IEffect[] effects { get; set; } = new IEffect[]
            {

            new THHEffectBefore<THHPlayer.UseEventArg>(PileName.FIELD,
                (g,c,a) =>
                {
                    return a.player == c.getOwner();
                },null,(g,c,a)=>
                {
                    c.getOwner().createToken(g, g.getCardDefine<MiniRobot>(), c.getOwner().field.count);
                    return Task.CompletedTask;
                }
            )

            };
        }

        /// <summary>
        /// 迷你机器人，随从
        /// 1/1/1
        /// </summary>
        [IgnoreCardDefine]
        public class MiniRobot : ServantCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SERVANT | 0x018;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 1;
            public override int attack { get; set; } = 1;//随从的攻击力
            public override int life { get; set; } = 1;//随从的血量上限
            public override bool isToken { get; set; } = true;//是衍生物吗？（当然）
                                                              //public override string[] tags { get; set; } = new string[];//种族标签
                                                              //public override string[] keywords { get; set; } = new string[] { Keyword.TAUNT };//关键词标签：
        }

        /// <summary>
        /// 化敌为友(格拉摩根伯爵-水桥帕露西)
        /// 2费，选择一个随从，使其对他的相邻随从造成等同其攻击力的伤害
        /// 
        /// </summary>
        [IgnoreCardDefine]
        public class ColludeWithTheEnemy : SpellCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SPELL | 0x019;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 2;
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            //存在目标的法术
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                int dam = target.getAttack(game);
                int left = target.pile.indexOf(target) - 1;
                int right = target.pile.indexOf(target) + 1;
                if(left >= 0)
                {
                    await game.getOpponent(card.getOwner()).field[left].damage(game,card,dam);
                }

                if(game.getOpponent(card.getOwner()).field.right != target)
                {
                    await game.getOpponent(card.getOwner()).field[right].damage(game,card,dam);
                }

            }, PileFlag.oppo | PileFlag.field)//后缀表示只能指向随从
            };
        }

        /// <summary>
        /// 腐蚀(格拉摩根伯爵-黑谷山女)
        /// 1费，选择一个随从，下回合开始时消灭他
        /// 
        /// </summary>
        [IgnoreCardDefine]
        public class Corrosion : SpellCardDefine
        {
            public const int ID = Probe.ID | CardCategory.SPELL | 0x020;
            public override int id { get; set; } = ID;
            public override int cost { get; set; } = 1;
            public override IEffect[] effects { get; set; } = new IEffect[]
            {
            //存在目标的法术
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.addBuff(game,new GeneratedBuff(ID,new THHEffectBefore<THHGame.TurnStartEventArg>(
                    PileName.FIELD,
                    (g1,c1,a1) =>
                    {
                        //判断是敌人的回合还是我的回合
                        return a1.player == card.getOwner();
                    },
                    null,
                    (g1,c1,a1) =>
                    {
                        //起效部分，效果是消灭随从
                        c1.setDead(true);
                        //由于回合开始不会进行死亡结算，所以这里要强制进行一次。
                        return g1.updateDeath();
                    }
                    )) );
            }, PileFlag.oppo | PileFlag.field)//后缀表示只能指向随从
            };
        }


    /// <summary>
    /// 自强型机器人（格拉摩根伯爵-星熊勇仪）
    /// 4/4/5，两回合后，自身获得+4/+4
    /// 它被关在工厂里一万年了...
    /// </summary>
    //[IgnoreCardDefine]
    //public class HoshigumaYugi : ServantCardDefine
    //{
    //    public const int ID = Probe.ID | CardCategory.SERVANT | 0x021;
    //    public override int id { get; set; } = ID;
    //    public override int cost { get; set; } = 4;
    //    public override int attack { get; set; } = 4;//随从的攻击力
    //    public override int life { get; set; } = 5;//随从的血量上限
    //    public override bool isToken { get; set; } = false;//是衍生物吗？
    //    public override string[] tags { get; set; } = new string[] { CardTag.MACHINE };//种族标签
    //                                                                                   //public override string[] keywords { get; set; } = new string[] { Keyword.UNIQUE };//关键词标签：
    //    public override IEffect[] effects { get; set; } = new IEffect[]
    //    {

    //    new THHEffectBefore<THHGame.TurnEndEventArg>(PileName.FIELD,
    //        (g,c,a) =>
    //        {
    //            return a.player == c.getOwner();
    //        },null,(g,c,a)=>
    //        {

    //            return Task.CompletedTask;
    //        }
    //        )
    //    };
    //}

    /// <summary>
    /// 索道护卫（格拉摩根伯爵-早苗-索道护卫）
    /// 554，会师：抽三张牌
    /// 当场上有同名牌时，触发会师
    /// </summary>
    [IgnoreCardDefine]
    public class CablewayGuard : ServantCardDefine
    {
        public const int ID = Probe.ID | CardCategory.SPELL | 0x021;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 5;//随从的攻击力
        public override int life { get; set; } = 4;//随从的血量上限
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            if (Probe.isJoinForce(card.getOwner().field, card))
            {
                for(int i = 0 ; i<3; i++) { 
                    await card.getOwner().draw(game);
                }
            }
        }
    }

    /// <summary>
    /// 捕风捉影
    /// 5费，对一个敌方随机随从打3，召唤一个3/2的鸦天狗
    /// 
    /// </summary>
    [IgnoreCardDefine]
    public class CirculationOfGossip_1 : SpellCardDefine
    {
        public const int ID = Probe.ID | CardCategory.SPELL | 0x022;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            //存在目标的法术
            new NoTargetEffect(effect)
        };

        static async Task effect(THHGame game, Card card)
        {
            
        }
    }
}