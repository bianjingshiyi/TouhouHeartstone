using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using UI;
using Card = TouhouCardEngine.Card;

namespace Tests
{
    public class ResisenCardTests
    {

        //测试卡，力量的代价
        //这东西是怎么放在这里的。。。。
        [Test]
        public void XWavegTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<PowrOverwhelming>(), 3)));

            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());

            game.skipUntilCanUse<PowrOverwhelming>(you);
            you.cmdUse(game, you.getHand().getCard<PowrOverwhelming>(), targets: you.getField()[0]);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        //测试卡，维伦
        //。。。。我在搞毛啊
        [Test]
        public void VelenTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<TestHeal>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<Velen>(), 1)));

            game.skipUntilCanUse<Velen>(you);
            you.cmdUse(game, you.getHand().getCard<Velen>());
            you.cmdUse(game, you.getHand().getCard<TestHeal>(), targets: you.getMaster());

            game.Dispose();
        }

        //超短脑波
        //2费，奥秘,当敌方角色攻击时，取消攻击并使其受到相当于其攻击力的伤害。
        [Test]
        public void ReisenXWavegTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<XWave>(), 1)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());

            game.skipUntilCanUse<XWave>(you);
            you.cmdUse(game, you.getHand().getCard<XWave>());
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, oppo.getField()[0], you.getMaster());

            game.Dispose();
        }

        //生神停止 
        //2,奥秘，当敌方使用随从一张随从牌时，取消它的使用。
        //测试似乎过了，但希望人没事
        [Test]
        public void ReisenMindStopperTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MindStopper.ID, 3)
            );
            game.skipUntilCanUse<MindStopper>(you);
            you.cmdUse(game, you.getHand().getCard<MindStopper>());
            you.cmdTurnEnd(game);
            Assert.AreEqual(3, oppo.getGem());
            oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());
            Assert.AreEqual(0, oppo.getField().count);
            Assert.AreEqual(2, oppo.getGem());

        }


        //113 野兔兽妖 当你使用英雄技能后，攻击力变为3

        [Test]
        public void WildYoukaiRabbitTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<WildYoukaiRabbit>() as CardDefine, 1));

            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 4);
            you.cmdUse(game, you.getHand().getCard<WildYoukaiRabbit>());
            Assert.AreEqual(1, you.getField()[0].getAttack(game));
            you.cmdUse(game, you.getSkill(), targets: oppo.getMaster());
            Assert.AreEqual(3, you.getField()[0].getAttack(game));
        }

        // 赤眼催眠
        // 2费，奥秘,当敌方不低于2攻随从攻击时，获得他的控制权。
        [Test]
        public void MineShakerTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 10));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 7).Concat(Enumerable.Repeat(game.getDefine<MindShaker>(), 1)));

            game.skipUntilHasGem(oppo, 10);

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());

            while (you.getField().count < 7)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            }

            game.skipUntilCanUse<MindShaker>(you);
            you.cmdUse(game, you.getHand().getCard<MindShaker>());
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, oppo.getField()[0], you.getMaster());

            game.Dispose();
        }
        // 错乱之眼
        // 2费，奥秘：当随从攻击时，召唤3个0/2且具有嘲讽的幻象并使其中随机一个成为攻击目标。
        [Test]
        public void DisorderEyeTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<DisorderEye>(), 1)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.getHand().getCard<DefaultServant>());

            game.skipUntilCanUse<DisorderEye>(you);
            you.cmdUse(game, you.getHand().getCard<DisorderEye>());
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, oppo.getField()[0], you.getMaster());

            game.Dispose();
        }

        // 幻视调律
        // 2费，奥秘：当敌方对一个目标使用法术时，使其目标变为其他随机目标。
        [Test]
        public void VisionaryTuningTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<TestFreezeBrain>(), 1)));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<VisionaryTuning>(), 1)));


            for (int i = 0; i < 2; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            }

            game.skipUntilCanUse<VisionaryTuning>(you);
            you.cmdUse(game, you.getHand().getCard<VisionaryTuning>());
            you.cmdTurnEnd(game);

            oppo.cmdUse(game, oppo.getHand().getCard<TestFreezeBrain>(), targets: you.getField()[0]);
            Assert.False(you.getField()[0].isFreeze(game));
            game.Dispose();
        }

        // 紫外线场
        // 1费，复原你的技能并使你的技能在下一次使用前法力值消耗变为0。
        [Test]
        public void FieldUltraVioletTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<FieldUltraViolet>(), 1)));

            game.skipUntilHasGem(you, 3);
            Assert.False(you.getSkill().isUsed(game));
            you.cmdUse(game, you.getSkill());
            Assert.True(you.getSkill().isUsed(game));
            you.cmdUse(game, you.getHand().getCard<FieldUltraViolet>());
            Assert.False(you.getSkill().isUsed(game));
            you.cmdUse(game, you.getSkill());
            Assert.True(you.getSkill().isUsed(game));

            game.Dispose();
        }

        // 精神投弹
        //  1费,复原你的技能并使你的技能在下一次使用前能以随从作为目标。
        [Test]
        public void MindDroppingTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<MindDropping>(), 1)));
            game.skipUntilCanUse<DefaultServant>(oppo);
            Card servant = oppo.getHand().getCard<DefaultServant>();
            oppo.cmdUse(game, servant);
            oppo.cmdTurnEnd(game);
            game.skipUntilCanUse<MindDropping>(you);
            you.setGem(game, 10);
            you.cmdUse(game, you.getSkill());
            you.cmdUse(game, you.getHand().getCard<MindDropping>());
            Assert.True(you.getSkill().define is LunaticShoot_MindDropping);
            you.cmdUse(game, you.getSkill(), targets: oppo.getField()[0]);
            Assert.True(you.getSkill().define is LunaticShoot);
            Assert.AreEqual(5, oppo.getField()[0].getCurrentLife(game));

        }

        // 因幡帝
        // 抽一张牌，如果是奥秘则置入战场并重复此战吼
        [Test]
        public void TewiInabaTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<FreezingTrap>() as CardDefine, 9).Concat(Enumerable.Repeat(game.getDefine<TewiInaba>(), 1)));

            game.skipUntilCanUse<TewiInaba>(you);
            Assert.AreEqual(0, you.getHead().count);
            int handCount = you.getHand().count; 
            you.cmdUse(game, you.getHand().getCard<TewiInaba>());
            Assert.AreEqual(1, you.getHead().count);
            Assert.AreEqual(handCount,you.getHand().count);
            game.Dispose();
        }

        // 国士无双之药
        // 1，使一个友方随从获得+3/+3，下回合开始时消灭它（警告：不要过量嗑药！）
        [Test]
        public void PeerlessPatriotsElixirTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 3).Concat(Enumerable.Repeat(game.getDefine<PeerlessPatriotsElixir>(), 4)));

            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 4);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<PeerlessPatriotsElixir>(), targets: you.getField()[0]);
            game.skipTurnUntil(() => game.getCurrentPlayer() == you && you.getGem() >= 5);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<PeerlessPatriotsElixir>(), targets: you.getField()[0]);
            you.cmdUse(game, you.getHand().getCard<PeerlessPatriotsElixir>(), targets: you.getField()[0]);
            game.Dispose();
        }
        
        [Test]
        public void LunaticGunTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<FreezingTrap>() as CardDefine, 9).Concat(Enumerable.Repeat(game.getDefine<LunaticGun>(), 1)));

            game.skipUntilCanUse<LunaticGun>(you);
            you.cmdUse(game, you.getHand().getCard<LunaticGun>());
            game.skipUntilCanUse<FreezingTrap>(you);
            you.cmdUse(game, you.getHand().getCard<FreezingTrap>());
            game.Dispose();
        }
        
        [Test]
        public void ReisebMindExplosionTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MindExplosion.ID, 3)
            );
            game.skipUntilCanUse<MindExplosion>(you);
            you.cmdUse(game, you.getHand().getCard<MindExplosion>());

            game.Dispose();
        }

        // 永琳
        // 治疗与英雄技能数值翻倍
        [Test]
        public void ReisenEirin_TheMasterTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<TestHeal>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<Eirin_TheMaster>(), 1)));

            game.skipUntilCanUse<Eirin_TheMaster>(you);
            you.cmdUse(game, you.getHand().getCard<Eirin_TheMaster>());
            you.cmdUse(game, you.getHand().getCard<TestHeal>(), targets: you.getMaster());

            game.skipUntilCanUse<LunaticShoot>(you);
            you.cmdUse(game, you.getSkill());

            game.Dispose();
        }

        [Test]
        public void ReisenDefectedAgentTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(DefectedAgent.ID, 3)
            );

            game.skipUntilCanUse<DefectedAgent>(you);
            you.cmdUse(game, you.getHand().getCard<DefectedAgent>());


            game.skipUntilCanUse<LunaticShoot>(you);
            you.cmdUse(game, you.getSkill());

            game.Dispose();
        }

        [Test]
        public void ReisenDisbeliefAspectTest()
        {
            TestGameflow.createGame<FireBall,DefaultServant>(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(DisbeliefAspect.ID, 3)
            );

            game.skipUntilCanUse<DisbeliefAspect>(you);
            var count = you.getHand().count;
            you.cmdUse<DisbeliefAspect>(game);
            Assert.AreEqual(count-1,you.getHand().count);
            game.skipUntilCanUse<FireBall>(you);
            var fireBall = you.getHand().getCard<FireBall>();
            you.cmdUse(game,fireBall,0,oppo.getMaster());
            count = you.getHand().count;
            you.cmdUse<DisbeliefAspect>(game);
            Assert.AreEqual(count,you.getHand().count);
            Assert.True(you.getHand()[count-1].define is FireBall);

            game.Dispose();
        }
        [Test]
        public void medicineSellmanTest()
        {
            TestGameflow.createGame(out var game, out var you, out var _,
                new KeyValuePair<int, int>(MedicineSellman.ID, 1));
            game.skipUntilCanUse(you, game.getDefine<DefaultServant>(), game.getDefine<MedicineSellman>());
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.getField().getCard<DefaultServant>().damage(game, null, 6);
            Assert.AreEqual(1, you.getField().getCard<DefaultServant>().getCurrentLife(game));
            you.cmdUse(game, you.getHand().getCard<MedicineSellman>(), 0, you.getField().getCard<DefaultServant>());
            Assert.AreEqual(5, you.getField().getCard<DefaultServant>().getCurrentLife(game));
        }


        [Test]
        public void FieldUltraRedTest()
        {
            CardEngine game = TestGameflow.initGameWithoutPlayers(null, new HearthstoneOption()
            {
                shuffle = false
            });
            Player oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            Player you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<FieldUltraRed>(), 3)));

            game.skipUntilHasGem(you, 10);
            you.cmdUse(game, you.getHand().getCard<FieldUltraRed>());
            you.cmdUse(game, you.getHand().getCard<FieldUltraRed>());
            you.cmdUse(game, you.getSkill());
            you.getSkill().setUsed(game, false);
            you.cmdUse(game, you.getSkill());

            game.Dispose();
        }
        [Test]
        public void caseTooMuchUltraRedTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Reisen.ID,
                new KeyValuePair<int, int>(FieldUltraRed.ID, 2));
            game.skipUntilCanUse(you, game.getDefine<FieldUltraRed>(), game.getDefine<FieldUltraRed>(), game.getDefine<LunaticShoot>());
            you.cmdUse<FieldUltraRed>(game);
            you.cmdUse<FieldUltraRed>(game);
            you.cmdUse(game, you.getSkill());
            Assert.AreEqual(8, game.triggers.getRecordedEvents().OfType<THHCard.DamageEventArg>().Last().infoDic.First().Value.damagedValue);
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.getSkill());
            Assert.AreEqual(2, game.triggers.getRecordedEvents().OfType<THHCard.DamageEventArg>().Last().infoDic.First().Value.damagedValue);
        }
    }
}