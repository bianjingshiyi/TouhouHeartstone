using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;


namespace TouhouHeartstone.Builtin
{
    public class Reisen : MasterCardDefine
    {
        public const int ID = CATEGORY.CHARACTER_REISEN | CATEGORY.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = LunaticShoot.ID; //英雄技能的ID

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
    /// 技能：开火
    /// 对敌方英雄造成2点伤害
    /// </summary>
    public class LunaticShoot : SkillCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SKILL | 0x000;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2; //技能花费

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect) //这不是个指向性技能
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
            await game.getOpponent(card.owner as Player).getMaster().damage(game, card, 2);
        }
    }

    /// <summary>
    /// 超短脑波
    /// 2费，奥秘,当敌方角色攻击时，取消攻击并使其对自身造成等同于其攻击力的伤害。
    /// </summary>
    public class XWave : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x001;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };

        private IEffect[] effects1 = new IEffect[]
        {
            new SecretTriggerBefore<THHCard.AttackEventArg>(
                (g, c, b, a) => { return a.card.getOwner() != c.getOwner() && a.card.canAttackTest(g); },
                async (g, c, b, a) =>
                {
                    a.isCanceled = true; //取消攻击
                    await a.card.damage(g, a.card, a.card.getAttack(g));
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
    /// 113 野兔兽妖 激励：攻击力变为3+
    /// </summary>
    public class WildYoukaiRabbit : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x002;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;
        public override int attack { get; set; } = 1;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = { TAG.BEAST };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectAfter<HearthStoneRule.UseEventArg>(PileFlag.field,
                (g, c, a) => { return a.player == c.getOwner() && a.card.isSkill(); },
                async (g, c, a) => { await c.addBuff(g, new CodeBuff(ID, new AttackModifier(3, true))); }
            )
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
    /// 2 奥秘：生神停止 当敌方使用随从一张随从牌时，取消它的使用。
    /// </summary>
    public class MindStopper : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x003;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };

        private IEffect[] effects1 = new IEffect[]
        {
            new SecretTriggerBefore<HearthStoneRule.UseEventArg>(
                (g, c, b, a) => { return a.card.getOwner() != c.getOwner() && a.card.isServant(); }, (g, c, b, a) =>
                {
                    a.isCanceled = true;
                    return Task.CompletedTask;
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
    /// 赤眼催眠
    /// 2费，奥秘,当敌方不低于2攻随从攻击时，获得他的控制权。
    /// </summary>
    public class MindShaker : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x004;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };

        private IEffect[] effects1 = new IEffect[]
        {
            new SecretTriggerBefore<THHCard.AttackEventArg>(
                (g, c, b, a) =>
                {
                    return a.card.getOwner() != c.getOwner() && a.card.canAttackTest(g) && a.card.isServant() &&
                           a.card.getAttack(g) <= 2 && c.getOwner().getField().count < 7;
                }, async (g, c, b, a) =>
                {
                    a.isCanceled = true; //取消攻击
                    Player you = c.getOwner();
                    Player oppo = a.card.getOwner();
                    await a.card.takeControl(g, you); //交换控制权
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
    /// 错乱之眼
    /// 2费，奥秘：当敌方角色攻击时，召唤3个0/2且具有嘲讽的幻象并使其中随机一个成为攻击目标。
    /// </summary>
    public class DisorderEye : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x005;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };

        private IEffect[] effects1 = new IEffect[]
        {
            new SecretTriggerBefore<THHCard.AttackEventArg>(
                (g, c, b, a) =>
                {
                    return a.card.getOwner() != c.getOwner() && a.card.canAttackTest(g) &&
                           c.getOwner().getField().count <= 4;
                }, async (g, c, b, a) =>
                {
                    List<Card> tokens = new List<Card>();
                    for (int i = 0; i < 3; i++)
                    {
                        Card token;
                        if (i != 1)
                            token = (await c.getOwner().createToken(g, g.getDefine<AfterImage>(),
                                c.getOwner().getField().count)).card;
                        else
                            token = (await c.getOwner().createToken(g, g.getDefine<AfterImage_Main>(),
                                c.getOwner().getField().count)).card;
                        tokens.Add(token);
                    }

                    await a.changeEventVar(g, EVENTVAR.TARGET_CARD, tokens.random(g));
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
    /// 残像
    /// 0/0/2
    /// 衍生物
    /// </summary>
    public class AfterImage : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x006;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };
    }

    /// <summary>
    /// 幻视调律
    /// 2费，奥秘：当敌方对一个目标使用法术时，使其目标变为其他随机目标。
    /// </summary>
    public class VisionaryTuning : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x007;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.SECRET };

        private IEffect[] effects1 = new IEffect[]
        {
            new SecretTriggerBefore<HearthStoneRule.UseEventArg>(
                (g, c, b, a) =>
                {
                    return a.card.getOwner() != c.getOwner() && a.card.isSpell() &&
                           a.card.define.getActiveEffect() is ITargetEffect &&
                           a.card.getAvaliableTargets(g).Where(target => !target.Equals(a.targets)) != null;
                }, (g, c, b, a) =>
                {
                    Card[] range = a.card.getAvaliableTargets(g);
                    Card newTarget = range.Where(target => !target.Equals(a.targets)).random(g);
                    Card[] targets = new Card[] {newTarget};
                    a.changeEventVar(g, EVENTVAR.TARGETS, targets);
                    return Task.CompletedTask;
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
    /// 紫外线场
    /// 1费，复原你的技能并使你的技能在下一次使用前法力值消耗变为0。
    /// </summary>
    public class FieldUltraViolet : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x008;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;

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
            await card.getOwner().getSkill().addBuff(game, new CodeBuff(ID, new CostModifier(0, true),
                new RemoveBuffAfter<HearthStoneRule.UseEventArg>(PileFlag.skill,
                    (g, c, b, a) => { return a.player == c.getOwner() && a.card.isSkill(); }, ID)));
            if (card.getOwner().getSkill().isUsed(game))
            {
                await card.getOwner().getSkill().setUsed(game, false);
            }
        }
    }

    /// <summary>
    /// 国士无双之药
    /// 1，使一个随从获得+3/+3，下回合开始时消灭它（警告：不要过量嗑药！）
    /// </summary>
    public class PeerlessPatriotsElixir : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x009;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (game, card, target) =>
            {
                if (!target.containBuff(ID))
                {
                    await target.addBuff(game, new CodeBuff(ID, new AttackModifier(3), new LifeModifier(3)));
                    await target.addBuff(game, new CodeBuff(ID,
                        new THHEffectAfter<HearthStoneRule.TurnStartEventArg>(
                            PileFlag.field,
                            (g1, c1, a1) => a1.player == card.getOwner(),
                            async (g1, c1, a1) =>
                            {
                                //起效部分，效果是消灭随从
                                await c1.setDead(game, true);
                            }
                        )));
                }
                else
                {
                    await target.addBuff(game, new CodeBuff(ID, new AttackModifier(3), new LifeModifier(3)));
                    await target.setDead(game, true);
                    await game.getAllServants().damage(game, target, target.getAttack(game));
                }
            }, PileFlag.self | PileFlag.field)
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
    /// 幻象震爆
    /// 
    /// </summary>
    public class MindExplosion : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x010;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;

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
            int damage = card.getOwner().getSpellDamage(game, 5,card);
            await game.getOpponent(card.getOwner()).getMaster().damage(game, card, damage);
        }
    }

    /// <summary>
    /// 永琳
    /// 英雄技能造成的伤害翻倍,治疗翻倍
    /// </summary>
    public class Eirin_TheMaster : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x011;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 6;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 8;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE }; //关键词标签：

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectBefore<THHCard.DamageEventArg>(PileFlag.field,
                (g, c, a) =>
                {
                    return a.source.getOwner() == c.getOwner() && a.source.isSkill();
                }, (g, c, a) =>
                {
                    var value = a.damageValue * 2;
                    a.changeEventVar(g, THHCard.DamageEventArg.VAR_DAMAGE_VALUE, value);
                    return Task.CompletedTask;
                }),
            new THHEffectBefore<THHCard.HealEventArg>(PileFlag.field,
                (g, c, a) => { return a.source.getOwner() == c.getOwner(); },
                (g, c, a) =>
                {
                    var value = a.value * 2;
                    a.changeEventVar(g, EVENTVAR.VALUE, value);
                    return Task.CompletedTask;
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
    /// 0费 精神投弹 复原你的技能并使你的技能在下一次使用前能以随从作为目标。
    /// </summary>
    public class MindDropping : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x012;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect) //这不是个指向性技能
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
            var player = card.getOwner();
            var skill = player.getSkill();

            if (skill.isUsed(game))
            {
                await skill.setUsed(game, false);
            }

            // 默认月狂射击
            if (skill.define == game.getDefine<LunaticShoot>())
            {
                var arg = await THHCard.setDefine(skill, game, game.getDefine<LunaticShoot_MindDropping>(), false);
                await skill.addBuff(game, new CodeBuff(ID,
                    new THHEffectAfter<HearthStoneRule.UseEventArg>(
                        PileFlag.skill,
                        (g, c, a) => 
                        { 
                            return a.card == skill; 
                        },
                        async (g, c, a) =>
                        {
                            await THHCard.setDefine(skill, game, arg.beforeDefine, false);
                            await skill.removeBuff(game, skill.getBuffs().Where(b => b.id == ID));
                        }
                    )));
            }
        }
    }

    public class LunaticShoot_MindDropping : SkillCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SKILL | 0x013;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2; //技能花费

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (g, c, t) => { await t.damage(g, c, 2); },
                PileFlag.both | PileFlag.character,
                onCheckTarget: (g, c, t) => !(t.isMaster() && t.getOwner() == c.getOwner()))
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
    /// 334 贩药货郎 战吼：回复4点生命值
    /// </summary>
    public class MedicineSellman : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x014;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 4;
        public override string[] tags { get; set; } = { TAG.BEAST };

        private IEffect[] effects1 = new IEffect[]
        {
            new LambdaSingleTargetEffect(async (g, c, t) => { await t.heal(g, c, 4); },
                PileFlag.both | PileFlag.character)
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
    /// 343 叛逃特工 每当你使用英雄技能时，对随机敌人造成2点伤害
    /// </summary>
    public class DefectedAgent : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x015;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 3;
        public override string[] tags { get; set; } = { TAG.BEAST };

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectAfter<HearthStoneRule.UseEventArg>(PileFlag.field,
                (g, c, a) => { return a.player == c.getOwner() && a.card.isSkill(); },
                async (g, c, a) => { await g.getAllEnemies(c.getOwner()).random(g).damage(g, c, 2); })
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
    /// 因幡帝
    /// 抽一张牌，如果是奥秘则置入战场并重复此战吼
    /// </summary>
    public class TewiInaba : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x016;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 5;
        public override int attack { get; set; } = 4;
        public override int life { get; set; } = 4;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.UNIQUE, KEYWORD.STEALTH }; //关键词标签：
        public override string[] tags { get; set; } = { TAG.BEAST };

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
            Card topCard = card.getOwner().getDeck().top;

            while (topCard != null &&
                   topCard.isTrap(game) &&
                   topCard.isSpell() &&
                   card.getOwner().getHead().count < card.getOwner().getHead().maxCount &&
                   card.getOwner().getHead().All(c => c.define != topCard.define))
            {
                await card.getOwner().draw(game);
                await card.getOwner().getHand().moveTo(game, topCard, card.getOwner().getHead());
                topCard = card.getOwner().getDeck().top;
            }

            await card.getOwner().draw(game);
        }
    }

    /// <summary>
    /// 1费 红外线场 使你的技能下一次造成的伤害翻倍。
    /// </summary>
    public class FieldUltraRed : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x017;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 1;

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect(effect, name: "下次技能伤害翻倍")
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
            var player = card.getOwner();
            await player.getMaster().addBuff(game, new CodeBuff(ID,
                new THHEffectBefore<THHCard.DamageEventArg>(
                    PileFlag.master,
                    (g, c, b, a) =>
                    {
                        return player.getSkill() == a.source;
                    }, 
                    (g, c, b, a) =>
                    {
                        var value = a.damageValue * 2;
                        a.changeEventVar(g, THHCard.DamageEventArg.VAR_DAMAGE_VALUE, value);
                        return Task.CompletedTask;
                    }),
                new RemoveBuffAfter<HearthStoneRule.UseEventArg>(
                    PileFlag.master,
                    (g, c, b, a) =>
                    {
                        return player.getSkill() == a.card;
                    }, 
                    ID)));
        }
    }

    /// <summary>
    /// 332 狂月枪
    /// 每当你将一个奥秘置入战场，便获得+1耐久度。
    /// </summary>
    public class LunaticGun : ItemCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.ITEM | 0x018;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 3;
        public override int attack { get; set; } = 3;
        public override int life { get; set; } = 2;

        private IEffect[] effects1 = new IEffect[]
        {
            new THHEffectAfter<THHPile.MoveCardEventArg>(PileFlag.item,
                (g, c, a) =>
                {
                    return a.from.owner == c.getOwner() &&
                           a.to == c.getOwner().getHead() &&
                           a.card.isSpell() &&
                           a.card.isTrap(g);
                },
                async (g, c, a) => { await c.addBuff(g, new CodeBuff(ID, new LifeModifier(1))); })
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
    /// 虚假幻象 2 将你本回合中使用的上一张法术牌的复制置入你的手牌。
    /// </summary>
    public class DisbeliefAspect : SpellCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SPELL | 0x019;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 2;

        private IEffect[] effects1 = new IEffect[]
        {
            new NoTargetEffect((g, c) =>
            {
                var spellsUsedInTurn = g.triggers.getRecordedEvents()
                    .skipUntilLast(e => e is HearthStoneRule.TurnStartEventArg _).OfType<HearthStoneRule.UseEventArg>()
                    .Select(e => e.card).Where(c1 => c1.isSpell()).ToArray();

                if (spellsUsedInTurn.Length > 0)
                {
                    return c.getOwner().getHand()
                        .add(g, g.createCard(spellsUsedInTurn[spellsUsedInTurn.Length - 1].define));
                }

                return Task.CompletedTask;
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

    public class AfterImage_Main : ServantCardDefine
    {
        public const int ID = Reisen.ID | CATEGORY.SERVANT | 0x020;
        public override int id { get; set; } = ID;
        public override int cost { get; set; } = 0;
        public override int attack { get; set; } = 0;
        public override int life { get; set; } = 2;
        public override bool isToken { get; set; } = true;
        public override string[] keywords { get; set; } = new string[] { KEYWORD.TAUNT };
    }
}