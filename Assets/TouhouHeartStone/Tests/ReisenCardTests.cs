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

namespace Tests
{
    public class ResisenCardTests
    {
        
        //测试卡，力量的代价
        //这东西是怎么放在这里的。。。。
        [Test]
        public void XWavegTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<PowrOverwhelming>(), 3)));

            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<PowrOverwhelming>(you);
            you.cmdUse(game, you.hand.getCard<PowrOverwhelming>(),targets: you.field[0]);
            you.cmdTurnEnd(game);
            game.Dispose();
        }

        //测试卡，维伦
        //。。。。我在搞毛啊
        [Test]
        public void VelenTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<TestHeal>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<Velen>(), 1)));

            game.skipUntilCanUse<Velen>(you);
            you.cmdUse(game, you.hand.getCard<Velen>());
            you.cmdUse(game, you.hand.getCard<TestHeal>(),targets: you.master);

            game.Dispose();
        }

        //超短脑波
        //2费，奥秘,当敌方角色攻击时，取消攻击并使其受到相当于其攻击力的伤害。
        [Test]
        public void ReisenXWavegTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<XWave>(), 1)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<XWave>(you);
            you.cmdUse(game, you.hand.getCard<XWave>());
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, oppo.field[0], you.master);

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
            you.cmdUse(game, you.hand.getCard<MindStopper>());
            you.cmdTurnEnd(game);
            Assert.AreEqual(3, oppo.gem);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            Assert.AreEqual(0, oppo.field.count);
            Assert.AreEqual(2, oppo.gem);

        }

        
        //113 野兔兽妖 当你使用英雄技能后，攻击力变为3
       
        [Test]
        public void WildYoukaiRabbitTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<WildYoukaiRabbit>() as CardDefine, 1));

            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            you.cmdUse(game, you.hand.getCard<WildYoukaiRabbit>());
            Assert.AreEqual(1, you.field[0].getAttack(game));
            you.cmdUse(game, you.skill,targets:oppo.master);
            Assert.AreEqual(3, you.field[0].getAttack(game));
        }

        // 赤眼催眠
        // 2费，奥秘,当敌方不低于2攻随从攻击时，获得他的控制权。
        [Test]
        public void MineShakerTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<MindShaker>(), 1)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<MindShaker>(you);
            you.cmdUse(game, you.hand.getCard<MindShaker>());
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, oppo.field[0], you.master);
            
            game.Dispose();
        }
        // 错乱之眼
        // 2费，奥秘：当随从攻击时，召唤3个0/2且具有嘲讽的幻象并使其中随机一个成为攻击目标。
        [Test]
        public void DisorderEyeTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<DisorderEye>(), 1)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<DisorderEye>(you);
            you.cmdUse(game, you.hand.getCard<DisorderEye>());
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, oppo.field[0], you.master);

            game.Dispose();
        }

        // 幻视调律
        // 2费，奥秘：当敌方对一个目标使用法术时，使其目标变为其他随机目标。
        [Test]
        public void VisionaryTuningTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<TestFreezeBrain>(), 1)));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<VisionaryTuning>(), 1)));
            

            for (int i = 0; i < 2; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse(game, you.hand.getCard<DefaultServant>());
            }

            game.skipUntilCanUse<VisionaryTuning>(you);
            you.cmdUse(game, you.hand.getCard<VisionaryTuning>());
            you.cmdTurnEnd(game);

            oppo.cmdUse(game, oppo.hand.getCard<TestFreezeBrain>(), targets: you.field[0]);
            Assert.False(you.field[0].isFreeze(game));
            game.Dispose();
        }

        // 紫外线场
        // 1费，复原你的技能并使你的技能在下一次使用前法力值消耗变为0。
        [Test]
        public void FieldUltraVioletTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<FieldUltraViolet>(), 1)));

            game.skipUntilHasGem(you, 3);
            Assert.False(you.skill.isUsed(game));
            you.cmdUse(game, you.skill);
            Assert.True(you.skill.isUsed(game));
            you.cmdUse(game, you.hand.getCard<FieldUltraViolet>());
            Assert.False(you.skill.isUsed(game));
            you.cmdUse(game, you.skill);
            Assert.True(you.skill.isUsed(game));

            game.Dispose();
        }

        // 精神投弹
        //  1费,复原你的技能并使你的技能在下一次使用前能以随从作为目标。
        [Test]
        public void MindDroppingTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<MindDropping>(), 1)));
            game.skipUntilCanUse<DefaultServant>(oppo);
            Card servant = oppo.hand.getCard<DefaultServant>();
            oppo.cmdUse(game, servant);
            oppo.cmdTurnEnd(game);
            game.skipUntilCanUse<MindDropping>(you);
            you.setGem(game,10);
            you.cmdUse(game, you.skill);
            you.cmdUse(game, you.hand.getCard<MindDropping>());
            Assert.True(you.skill.define is LunaticShoot_MindDropping);
            you.cmdUse(game, you.skill,targets:oppo.field[0]);
            Assert.True(you.skill.define is LunaticShoot);
            Assert.AreEqual(5, oppo.field[0].getCurrentLife(game));

        }

        // 因幡帝
        // 抽一张牌，如果是奥秘则置入战场并重复此战吼
        [Test]
        public void TewiInabaTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<FreezingTrap>() as CardDefine, 9).Concat(Enumerable.Repeat(game.getDefine<TewiInaba>(), 1)));

            game.skipUntilCanUse<TewiInaba>(you);
            Assert.AreEqual(0, you.head.count);
            you.cmdUse(game, you.hand.getCard<TewiInaba>());
            Assert.AreEqual(1, you.head.count);
            game.Dispose();
        }

        // 国士无双之药
        // 1，使一个友方随从获得+3/+3，下回合开始时消灭它（警告：不要过量嗑药！）
        [Test]
        public void PeerlessPatriotsElixirTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 3).Concat(Enumerable.Repeat(game.getDefine<PeerlessPatriotsElixir>(), 4)));

            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<PeerlessPatriotsElixir>(),targets:you.field[0]);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 5);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            you.cmdUse(game, you.hand.getCard<PeerlessPatriotsElixir>(), targets: you.field[0]);
            you.cmdUse(game, you.hand.getCard<PeerlessPatriotsElixir>(), targets: you.field[0]);
            game.Dispose();
        }

        // 332 狂月枪
        // 每当你将一个奥秘置入战场，便获得+1耐久度。
        [Test]
        public void LunaticGunTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<FreezingTrap>() as CardDefine, 9).Concat(Enumerable.Repeat(game.getDefine<LunaticGun>(), 1)));

            game.skipUntilCanUse<LunaticGun>(you);          
            you.cmdUse(game, you.hand.getCard<LunaticGun>());
            game.skipUntilCanUse<FreezingTrap>(you);
            you.cmdUse(game, you.hand.getCard<FreezingTrap>());
            game.Dispose();
        }

        // 幻象震爆
        // 2,对敌方英雄打5
        [Test]
        public void ReisebMindExplosionTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MindExplosion.ID, 3)
            );
            game.skipUntilCanUse<MindExplosion>(you);
            you.cmdUse(game, you.hand.getCard<MindExplosion>());

            game.Dispose();
        }

        // 永琳
        // 治疗与英雄技能数值翻倍
        [Test]
        public void ReisenEirin_TheMasterTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<TestHeal>() as CardDefine, 29).Concat(Enumerable.Repeat(game.getDefine<Eirin_TheMaster>(), 1)));

            game.skipUntilCanUse<Eirin_TheMaster>(you);
            you.cmdUse(game, you.hand.getCard<Eirin_TheMaster>());
            you.cmdUse(game, you.hand.getCard<TestHeal>(), targets: you.master);

            game.skipUntilCanUse<LunaticShoot>(you);
            you.cmdUse(game, you.skill);

            game.Dispose();
        }

        [Test]
        public void ReisenDefectedAgentTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(DefectedAgent.ID, 3)
            );

            game.skipUntilCanUse<DefectedAgent>(you);
            you.cmdUse(game, you.hand.getCard<DefectedAgent>());


            game.skipUntilCanUse<LunaticShoot>(you);
            you.cmdUse(game, you.skill);

            game.Dispose();
        }

        [Test]
        public void ReisenDisbeliefAspectTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(DisbeliefAspect.ID, 3)
            );

            game.skipUntilHasGem(you, 2);
            you.cmdUse(game, you.skill);

            game.skipUntilHasGem(you, 3);
            you.cmdUse(game, you.hand.getCard<DisbeliefAspect>());
            you.cmdUse(game, you.skill);

            game.Dispose();
        }
    }
}