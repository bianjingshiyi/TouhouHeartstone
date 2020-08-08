using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone.Builtin
{
    public class Probe : MasterCardDefine
    {
        public const int ID = 0x04200000 | CardCategory.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 15;
        public override int skillID { get; set; } = ProbeShoot.ID;//英雄技能的ID
        public override IEffect[] effects { get; set; } = new IEffect[0];
    }

    /// <summary>
    /// 技能：射箭
    /// </summary>
    [Obsolete]
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
    /// 召唤系法术
    /// </summary>
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
    public class Pylon : ServantCardDefine
    {
        public const int ID = Probe.ID | CardCategory.SERVANT | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2; 
        public override int attack { get; set; } = 0;//随从的攻击力
        public override int life { get; set; } = 6;//随从的血量上限
        public override bool isToken { get; set; } = true;//是衍生物吗？（当然）
        //public override string[] tags { get; set; } = new string[];//种族标签
        public override string[] keywords { get; set; } = new string[] {Keyword.TAUNT};//关键词标签：
    }

    /// <summary>
    /// 粒子光束
    /// 2费，造成3点伤害
    /// 指向型直伤系法术
    /// </summary>
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
    /// 无指向型aoe法术
    /// </summary>
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
            await game.getAllCharacters().damage(game, card, card.getOwner().getSpellDamage(game,3));
        }
    }

    /// <summary>
    /// 水晶塔过载
    /// 4费，召唤一个水晶塔，再使所有水晶塔获得+3攻击力
    /// 召唤Buff复合型
    /// </summary>
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
                if(servant.define.id == Pylon.ID){
                    await servant.addBuff(game,new GeneratedBuff(ID,new AttackModifier(3)));
                }
                
            }
        }
        
    }

    /// <summary>
    /// 机械哨兵
    /// 4/2/4，战吼：召唤一个0/2具有嘲讽的幻象
    /// 战吼召唤系随从
    /// </summary>
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
    public class Vision : ServantCardDefine
    {
        public const int ID = Probe.ID | CardCategory.SERVANT | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0; 
        public override int attack { get; set; } = 0;//随从的攻击力
        public override int life { get; set; } = 2;//随从的血量上限
        public override bool isToken { get; set; } = true;//是衍生物吗？
        //public override string[] tags { get; set; } = new string[];//种族标签
        public override string[] keywords { get; set; } = new string[] {Keyword.TAUNT};//关键词标签：
        
    }

    /// <summary>
    /// 先知机器人
    /// 4/3/3，战吼：抽一张法力消耗最高的牌
    /// 战吼定向检索随从
    /// </summary>
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
            foreach(Card c in card.getOwner().deck)
            {
                if(c.getCost(game) > _card.getCost(game)){
                    _card = c;
                }
            }
            await card.getOwner().draw( game, _card);
        }
    }

    /// <summary>
    /// 巨像
    /// 8/8/8，唯一，回合结束时，对所有敌方角色造成3点伤害
    /// 回合结束扳机随从
    /// </summary>
    public class Colossus : ServantCardDefine
    {
        public const int ID = Probe.ID | CardCategory.SERVANT | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 8; 
        public override int attack { get; set; } = 8;//随从的攻击力
        public override int life { get; set; } = 8;//随从的血量上限
        public override bool isToken { get; set; } = false;//是衍生物吗？
        public override string[] tags { get; set; } = new string[] { CardTag.MACHINE };//种族标签
        public override string[] keywords { get; set; } = new string[] {Keyword.UNIQUE};//关键词标签：
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
    /// 指向型特殊直伤法术
    /// </summary>
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
    /// </summary>
    public class Protoss : ServantCardDefine
    {
        public const int ID = Probe.ID | CardCategory.SERVANT | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5; 
        public override int attack { get; set; } = 0;//随从的攻击力
        public override int life { get; set; } = 3;//随从的血量上限
        public override bool isToken { get; set; } = false;//是衍生物吗？
        public override string[] tags { get; set; } = new string[] { CardTag.MACHINE };//种族标签
        public override string[] keywords { get; set; } = new string[] {Keyword.UNIQUE};//关键词标签：
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            new NoTargetEffect(effect),
            new Halo(new GeneratedBuff(ID,new AttackModifier(2)), PileFlag.self | PileFlag.field)
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().createToken(game, game.getCardDefine<Pylon>(), card.pile.indexOf(card)+1 );
            await card.getOwner().createToken(game, game.getCardDefine<Pylon>(), card.pile.indexOf(card)-1 );
        }
    }


    /// <summary>
    /// 重构光线
    /// 0费，完全修好一个友方机械。(警告:不要对机械以外的随从使用！)
    /// </summary>
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
    /// </summary>
    public class Imitation  : SpellCardDefine
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
            await theReborned.addBuff(game,new GeneratedBuff(ID,new AttackModifier(3,true),new LifeModifier(3,true)));

        }
    }

    /// <summary>
    /// 守护之壳
    /// 3费，使一个随从获得+3生命值，如果它是机械，再获得+4攻击力
    /// </summary>
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
    /// 西塞尔
    /// 7/2/2，唯一，战吼：获得一个机械的控制权，然后将其变为3/3
    /// </summary>
    public class Sissel : ServantCardDefine
    {
        public const int ID = Probe.ID | CardCategory.SERVANT | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 7; 
        public override int attack { get; set; } = 2;//随从的攻击力
        public override int life { get; set; } = 2;//随从的血量上限
        public override bool isToken { get; set; } = false;//是衍生物吗？
        //public override string[] tags { get; set; } = new string[] {  };//种族标签
        public override string[] keywords { get; set; } = new string[] {Keyword.UNIQUE};//关键词标签：
        public override IEffect[] effects { get; set; } = new IEffect[]
        {
            //存在目标的法术
            new LambdaSingleTargetEffect(async (game,card,target)=>
            {
                await target.addBuff(game,new GeneratedBuff(ID,new LifeModifier(3)));
                if(((IList)target.getTags(game)).Contains(CardTag.MACHINE)){
                    await target.addBuff(game,new GeneratedBuff(ID,new AttackModifier(4)));
                }
            }, PileFlag.oppo | PileFlag.field , CardTag.MACHINE )//后缀表示只能指向随从
        };
        static async Task effect(THHGame game, Card card)
        {
            await card.getOwner().createToken(game, game.getCardDefine<Pylon>(), card.pile.indexOf(card)+1 );
            await card.getOwner().createToken(game, game.getCardDefine<Pylon>(), card.pile.indexOf(card)-1 );
        }
    }
}