using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin.Proble;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using static TouhouHeartstone.Builtin.Proble.Probe;

namespace Tests
{
    public class ProbeCardTests
    {
        /// <summary>
        /// 粒子光束测试
        /// </summary>
        [Test]
        public void ProbeParticulateLaserTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ParticulateLaser.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= game.getDefine<ParticulateLaser>().cost);
            you.cmdUse(game, you.hand.getCard<ParticulateLaser>(), targets: oppo.master);
            game.Dispose();
        }

        /// <summary>
        /// 召唤水晶塔测试
        /// </summary>
        [Test]
        public void ProbeBulidPylonTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(BulidPylon.ID, 1)
            );
            // 快进到可以使用此卡
            game.skipUntilCanUse<BulidPylon>(you);
            // 我方使用召唤水晶塔
            you.cmdUse(game, you.hand.getCard<BulidPylon>());
            // 回合结束
            you.cmdTurnEnd(game);
            // 快进到你的回合
            //game.skipTurnUntil(() => game.currentPlayer == you);
            game.Dispose();
        }

        /// <summary>
        /// 水晶塔过载测试
        /// </summary>
        [Test]
        public void ProbePylonOverloadTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(BulidPylon.ID, 1),
                new KeyValuePair<int, int>(PylonOverload.ID, 1)
            );
            game.skipUntilCanUse<BulidPylon>(you);
            // 我方使用召唤水晶塔
            you.cmdUse(game, you.hand.getCard<BulidPylon>());
            // 回合结束
            you.cmdTurnEnd(game);
            game.skipUntilCanUse<PylonOverload>(you);
            you.cmdUse(game, you.hand.getCard<PylonOverload>());
            Debug.Log("第一个水晶塔是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game));
            Debug.Log("第二个水晶塔是" + you.field[1].getAttack(game) + "/" + you.field[1].getLife(game));
            game.Dispose();
        }

        /// <summary>
        /// 普罗托斯测试
        /// </summary>
        [Test]
        public void ProbeProtossTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(BulidPylon.ID, 1),
                new KeyValuePair<int, int>(Protoss.ID, 1)

            );
            game.skipUntilCanUse<BulidPylon>(you);
            // 回合结束
            you.cmdTurnEnd(game);
            game.skipUntilCanUse<Protoss>(you);
            you.cmdUse(game, you.hand.getCard<Protoss>());
            Debug.Log("第一个是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game));
            Debug.Log("第二个是" + you.field[1].getAttack(game) + "/" + you.field[1].getLife(game));
            Debug.Log("第三个是" + you.field[2].getAttack(game) + "/" + you.field[2].getLife(game));
            you.field[1].die(game);
            Debug.Log("第一个是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game));
            Debug.Log("第二个是" + you.field[1].getAttack(game) + "/" + you.field[1].getLife(game));
            //Debug.Log( "第三个是" + you.field[2].getAttack(game) + "/" + you.field[2].getLife(game) );
            game.Dispose();
        }

        /// <summary>
        /// 先知机器人测试
        /// </summary>
        [Test]
        public void ProbeProphetRobotTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ProphetRobot.ID, 15),
                new KeyValuePair<int, int>(Protoss.ID, 15)

            );
            //game.skipUntilCanUse<ProphetRobot>(you);
            // 回合结束
            //you.cmdTurnEnd(game);    
            game.skipUntilCanUse<ProphetRobot>(you);
            you.cmdUse(game, you.hand.getCard<ProphetRobot>());

            game.Dispose();
        }


        /// <summary>
        /// 哨兵测试
        /// </summary>
        [Test]
        public void ProbeSentryTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Sentry.ID, 1)

            );
            game.skipUntilCanUse<Sentry>(you);
            // 回合结束
            // you.cmdTurnEnd(game);    
            // game.skipUntilCanUse<Sentry>(you);
            you.cmdUse(game, you.hand.getCard<Sentry>());
            Debug.Log("第一个是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game));
            Debug.Log("第二个是" + you.field[1].getAttack(game) + "/" + you.field[1].getLife(game));
            //Debug.Log( "第三个是" + you.field[2].getAttack(game) + "/" + you.field[2].getLife(game) );
            game.Dispose();
        }

        /// <summary>
        /// 灵能震荡测试
        /// </summary>
        [Test]
        public void ProbePsionicShockTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(PsionicShock.ID, 1),
                new KeyValuePair<int, int>(Sentry.ID, 1)
            );
            game.skipUntilCanUse<Sentry>(you);
            you.cmdUse(game, you.hand.getCard<Sentry>());
            you.cmdTurnEnd(game);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<PsionicShock>());
            game.Dispose();
        }

        /// <summary>
        /// 能量反馈测试
        /// </summary>
        [Test]
        public void ProbeEnergyAutophagyTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(EnergyAutophagy.ID, 1)
            );
            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            oppo.cmdTurnEnd(game);
            game.skipUntilCanUse<EnergyAutophagy>(you);
            you.cmdUse(game, you.hand.getCard<EnergyAutophagy>(), targets: oppo.field[0]);
            game.Dispose();
        }


        /// <summary>
        /// 巨像测试
        /// </summary>
        [Test]
        public void ProbeColossusTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Colossus.ID, 1)

            );
            game.skipUntilCanUse<Colossus>(you);
            you.cmdUse(game, you.hand.getCard<Colossus>());
            you.cmdTurnEnd(game);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            oppo.cmdTurnEnd(game);
            you.cmdTurnEnd(game);
            //Debug.Log( "第三个是" + you.field[2].getAttack(game) + "/" + you.field[2].getLife(game) );
            game.Dispose();
        }

        /// <summary>
        /// 重构光线测试
        /// 0费，完全修好一个友方机械。(警告:不要对机械以外的随从使用！)
        /// </summary>
        [Test]
        public void ProbeReconstructRayTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ReconstructRay.ID, 2),
                new KeyValuePair<int, int>(Sentry.ID, 1),
                new KeyValuePair<int, int>(EnergyAutophagy.ID, 1)
            );
            game.skipUntilCanUse<Sentry>(you);
            you.cmdUse(game, you.hand.getCard<Sentry>());
            you.cmdTurnEnd(game);
            game.skipUntilCanUse<EnergyAutophagy>(you);
            you.cmdUse(game, you.hand.getCard<EnergyAutophagy>(), targets: you.field[0]);
            you.cmdUse(game, you.hand.getCard<ReconstructRay>(), targets: you.field[0]);
            you.cmdUse(game, you.hand.getCard<ReconstructRay>(), targets: you.field[1]);
            you.cmdTurnEnd(game);

            game.Dispose();
        }

        /// <summary>
        /// 山寨版本
        /// 3费，发现墓地里一张机械牌，召唤它并使其变为3/3
        /// </summary>
        [Test]
        public void ProbeImitationTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Imitation.ID, 1),
                new KeyValuePair<int, int>(Sentry.ID, 3)
            );
            game.skipUntilCanUse<Sentry>(you);
            //召唤3个哨兵，然后把哨兵和幻象都消灭
            you.cmdUse(game, you.hand.getCard<Sentry>());
            you.field[0].die(game);
            you.field[0].die(game);
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Sentry>());
            you.field[0].die(game);
            you.field[0].die(game);
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<Sentry>());
            you.field[0].die(game);
            you.field[0].die(game);
            you.cmdTurnEnd(game);

            //快进到能使用山寨产品
            game.skipUntilCanUse<Imitation>(you);
            you.cmdUse(game, you.hand.getCard<Imitation>());
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            int cardId = discoverRequest.cardIdArray[0];
            Card card = you.grave.getCard(cardId);//随便选一张
            you.cmdDiscover(game, cardId);//选
            Debug.Log("第一个是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game) + "的" + you.field[0].define.id);

            game.Dispose();
        }


        /// <summary>
        /// 守护之壳测试
        /// 3费，使一个随从获得+3生命值，如果它是机械，再获得+4攻击力
        /// </summary>
        [Test]
        public void ProbeProtectiveShellTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(ProtectiveShell.ID, 2),
                new KeyValuePair<int, int>(Sentry.ID, 1)
            );
            game.skipUntilCanUse<Sentry>(you);
            you.cmdUse(game, you.hand.getCard<Sentry>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);

            game.skipUntilCanUse<ProtectiveShell>(you);
            you.cmdUse(game, you.hand.getCard<ProtectiveShell>(), targets: you.field[0]);
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            you.cmdUse(game, you.hand.getCard<ProtectiveShell>(), targets: you.field[1]);

            Debug.Log("第一个是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game) + "的" + you.field[0].define.id);
            Debug.Log("第二个是" + you.field[1].getAttack(game) + "/" + you.field[1].getLife(game) + "的" + you.field[1].define.id);

            game.Dispose();
        }

        /// <summary>
        /// 西塞尔测试
        /// 7/2/2，唯一，战吼：获得一个机械的控制权，然后将其变为3/3
        /// </summary>
        [Test]
        public void ProbeSisselTest()
        {
            /*
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Sissel.ID, 1),
                new KeyValuePair<int, int>(Sentry.ID, 1)
            );

            game.skipUntilCanUse<Sentry>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<Sentry>());
            oppo.cmdTurnEnd(game);
            
            game.skipUntilCanUse<Sissel>(you);
            Assert.True(you.tryUse(game, you.hand.getCard<Sissel>(), 0, targets: oppo.field[0]).Result);
            Assert.False(you.tryUse( game, you.hand.getCard<Sissel>() , 0 ,targets: oppo.field[1]).Result);
            you.cmdUse(game, you.hand.getCard<Sissel>(),targets: oppo.field[0]);

            Debug.Log( "第一个是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game) + "的" + you.field[0].define.id );
            Debug.Log( "第二个是" + you.field[1].getAttack(game) + "/" + you.field[1].getLife(game) + "的" + you.field[1].define.id );   
            */
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 28).Concat(Enumerable.Repeat(game.getDefine<Sentry>(), 2)));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 28).Concat(Enumerable.Repeat(game.getDefine<Sissel>(), 2)));

            game.skipUntilCanUse<Sentry>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<Sentry>());
            oppo.cmdTurnEnd(game);

            game.skipUntilCanUse<Sissel>(you);
            Card card = you.hand.getCard<Sissel>();
            Assert.True(card.isValidTarget(game, oppo.field[0]));
            you.cmdUse(game, you.hand.getCard<Sissel>(), targets: oppo.field[0]);
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            Assert.False(card.isValidTarget(game, oppo.field[0]));

            //you.cmdUse(game, you.hand.getCard<Sissel>(), targets: oppo.field[0]);

            // Debug.Log("第一个是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game) + "的" + you.field[0].define.id);
            // Debug.Log("第二个是" + you.field[1].getAttack(game) + "/" + you.field[1].getLife(game) + "的" + you.field[1].define.id);
            game.Dispose();
        }

        /// <summary>
        /// 沉默的计牌器测试
        /// 4/3/3，每当你的对手使用一张牌时，自身获得+1/+2
        /// </summary>
        [Test]
        public void ProbeSilentCardCounterTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 28).Concat(Enumerable.Repeat(game.getDefine<ParticulateLaser>(), 2)));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 28).Concat(Enumerable.Repeat(game.getDefine<SilentCardCounter>(), 2)));

            game.skipUntilCanUse<SilentCardCounter>(you);
            you.cmdUse(game, you.hand.getCard<SilentCardCounter>());
            you.cmdTurnEnd(game);
            oppo.cmdUse(game, oppo.hand.getCard<ParticulateLaser>(), targets: you.field[0]);
            //Debug.Log( "第一个是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game) );
            game.Dispose();
        }

        /// <summary>
        /// 流水线测试
        /// 3/0/3，每当你使用一张牌时，召唤一个1/1的迷你机器人
        /// 它被关在工厂里一万年了...
        /// </summary>
        [Test]
        public void ProbeAssemblyLineTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(AssemblyLine.ID, 2),
                new KeyValuePair<int, int>(FireBall.ID, 2),
                new KeyValuePair<int, int>(DefaultServant.ID, 2)
            );

            game.skipUntilCanUse<AssemblyLine>(you);
            you.cmdUse(game, you.hand.getCard<AssemblyLine>());
            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());
            game.skipUntilCanUse<FireBall>(you);
            you.cmdUse(game, you.hand.getCard<FireBall>(),targets : oppo.master);
            //oppo.cmdUse(game, oppo.hand.getCard<ParticulateLaser>(), targets: you.field[0]);
            //Debug.Log( "第一个是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game) );
            game.Dispose();
        }

        /// <summary>
        /// 化敌为友
        /// 2费，选择一个随从，使其对他的相邻随从造成等同其攻击力的伤害
        /// 
        /// </summary>
        [Test]
        public void ProbeColludeWithTheEnemyTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<ColludeWithTheEnemy>(), 3)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<ColludeWithTheEnemy>(you);
            you.cmdUse(game, you.hand.getCard<ColludeWithTheEnemy>(), targets: oppo.field[0]);

            game.skipUntilCanUse<ColludeWithTheEnemy>(you);
            you.cmdUse(game, you.hand.getCard<ColludeWithTheEnemy>(), targets: oppo.field[1]);

            game.skipUntilCanUse<ColludeWithTheEnemy>(you);
            you.cmdUse(game, you.hand.getCard<ColludeWithTheEnemy>(), targets: oppo.field[2]);

            
            game.Dispose();
        }

        /// <summary>
        /// 腐蚀测试(格拉摩根伯爵-黑谷山女)
        /// 1费，选择一个随从，下回合开始时消灭他
        /// 
        /// </summary>
        [Test]
        public void ProbeCorrosionTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<Corrosion>(), 3)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<Corrosion>(you);
            you.cmdUse(game, you.hand.getCard<Corrosion>(), targets: oppo.field[0]);
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            game.Dispose();
        }

        [Test]
        public void ProbeCablewayGuardTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(CablewayGuard.ID, 2)
            );

            game.skipUntilCanUse<CablewayGuard>(you);
            you.cmdUse(game, you.hand.getCard<CablewayGuard>());
            game.skipUntilCanUse<CablewayGuard>(you);
            you.cmdUse(game, you.hand.getCard<CablewayGuard>());
            
            //oppo.cmdUse(game, oppo.hand.getCard<ParticulateLaser>(), targets: you.field[0]);
            //Debug.Log( "第一个是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game) );
            game.Dispose();
        }

        [Test]
        public void ProbeCirculationOfGossipTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<CirculationOfGossip_1>(), 3)));

            do {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            } while (!oppo.field.isFull);

            game.skipUntilCanUse<CirculationOfGossip_1>(you);
            you.cmdUse(game, you.hand.getCard<CirculationOfGossip_1>());

            

            while (!oppo.field.isFull)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }

            game.skipUntilCanUse<CirculationOfGossip_2>(you);
            you.cmdUse(game, you.hand.getCard<CirculationOfGossip_2>());

            while (!oppo.field.isFull)
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            }

            game.skipUntilCanUse<CirculationOfGossip_3>(you);
            you.cmdUse(game, you.hand.getCard<CirculationOfGossip_3>());

            //oppo.cmdUse(game, oppo.hand.getCard<ParticulateLaser>(), targets: you.field[0]);
            //Debug.Log( "第一个是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game) );
            game.Dispose();
        }

        [Test]
        public void ProbeGoustFromTheBarrowTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(GoustFromTheBarrow.ID, 2)
            );

            game.skipUntilCanUse<GoustFromTheBarrow>(you);
            you.cmdUse(game, you.hand.getCard<GoustFromTheBarrow>());


            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse(game, you.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<GoustFromTheBarrow>(you);
            you.cmdUse(game, you.hand.getCard<GoustFromTheBarrow>(),targets: you.field[0]);

            //oppo.cmdUse(game, oppo.hand.getCard<ParticulateLaser>(), targets: you.field[0]);
            //Debug.Log( "第一个是" + you.field[0].getAttack(game) + "/" + you.field[0].getLife(game) );
            game.Dispose();
        }

        [Test]
        public void ProbeGeralt_IgniTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<Geralt_Igni>(), 3)));

            do
            {
                game.skipUntilCanUse<DefaultServant>(oppo);
                oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            } while (!oppo.field.isFull);

            game.skipUntilCanUse<Geralt_Igni>(you);
            you.cmdUse(game, you.hand.getCard<Geralt_Igni>());
         
            game.Dispose();
        }

        [Test]
        public void ProbeFastReload()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<FastReload>(), 3)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());
            game.skipUntilHasGem(you, 3);
            you.cmdUse(game, you.skill,targets:oppo.field[0]);
            Assert.True(you.skill.isUsed(game));
            you.cmdUse(game, you.hand.getCard<FastReload>(), targets: oppo.master);
            Assert.False(you.skill.isUsed(game));
            you.cmdUse(game, you.skill,targets:oppo.field[0]);

            game.Dispose();
        }

        [Test]
        public void ProbeFencingCoachTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<FencingCoach>(), 3)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilHasGem(you, 5);
            you.cmdUse(game, you.hand.getCard<FencingCoach>());
            Assert.False(you.skill.isUsed(game));
            you.cmdUse(game, you.skill, targets: oppo.field[0]);
            Assert.False(you.skill.isUsed(game));
            you.cmdUse(game, you.skill, targets: oppo.field[0]);
            Assert.False(you.skill.isUsed(game));

            game.Dispose();
        }

        [Test]
        public void ProbeMirrorTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 2));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 1).Concat(Enumerable.Repeat(game.getDefine<Mirror>(), 1)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.skipUntilCanUse<Mirror>(you);
            you.cmdUse(game, you.hand.getCard<Mirror>());
            you.cmdTurnEnd(game);

            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            game.Dispose();
        }

        [Test]
        public void ProbePersevereTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 2));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 6).Concat(Enumerable.Repeat(game.getDefine<Persevere>(), 1)));

            game.skipUntilCanUse<DefaultServant>(oppo);
            oppo.cmdUse(game, oppo.hand.getCard<DefaultServant>());

            for (int i=0 ; i<6; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse(game, you.hand.getCard<DefaultServant>());
            }
            game.skipUntilCanUse<Persevere>(you);
            you.cmdUse(game, you.hand.getCard<Persevere>());
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, oppo.field[0], you.master);

            game.Dispose();
        }
    }

    
}