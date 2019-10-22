using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using TouhouCardEngine;
using TouhouHeartstone;

namespace Tests
{
    public class GameflowTests
    {
        [Test]
        public void initTest()
        {
            Game game = new Game(new UnitTestGameEnv());
            TestFrontend[] frontends = new TestFrontend[2];
            frontends[0] = new TestFrontend();
            frontends[1] = new TestFrontend();
            game.addPlayer(frontends[0], new int[] { 1000, 1, 1, 1, 1, 1 });
            game.addPlayer(frontends[1], new int[] { 2000, 1, 1, 1, 1, 1 });
            game.init();

            Assert.AreEqual(1, frontends[0].witnessList.Count);
            EventWitness witness = frontends[0].witnessList[0];
            Assert.AreEqual("onInit", witness.eventName);
            Assert.AreEqual(1000, witness.getVar<int[]>("masterCardsDID")[0]);
            Assert.AreEqual(2000, witness.getVar<int[]>("masterCardsDID")[1]);
            Assert.AreEqual(2, witness.getVar<int[]>("sortedPlayersIndex").Length);
            bool isFirstPlayer = witness.getVar<int[]>("sortedPlayersIndex")[0] == 0;
            Assert.AreEqual(2, witness.getVar<int[][]>("initCardsRID").Length);
            Assert.AreEqual(isFirstPlayer ? 3 : 4, witness.getVar<int[][]>("initCardsRID")[0].Length);
            Assert.AreEqual(isFirstPlayer ? 3 : 4, witness.getVar<int[]>("initCardsDID").Length);
            Assert.AreEqual(isFirstPlayer ? 2 : 1, witness.getVar<int[]>("deck").Length);
            Assert.AreEqual(1, frontends[1].witnessList.Count);
            witness = frontends[1].witnessList[0];
            Assert.AreEqual("onInit", witness.eventName);
            Assert.AreEqual(1000, witness.getVar<int[]>("masterCardsDID")[0]);
            Assert.AreEqual(2000, witness.getVar<int[]>("masterCardsDID")[1]);
            Assert.AreEqual(2, witness.getVar<int[]>("sortedPlayersIndex").Length);
            isFirstPlayer = witness.getVar<int[]>("sortedPlayersIndex")[0] == 1;
            Assert.AreEqual(2, witness.getVar<int[][]>("initCardsRID").Length);
            Assert.AreEqual(isFirstPlayer ? 3 : 4, witness.getVar<int[][]>("initCardsRID")[1].Length);
            Assert.AreEqual(isFirstPlayer ? 3 : 4, witness.getVar<int[]>("initCardsDID").Length);
            Assert.AreEqual(isFirstPlayer ? 2 : 1, witness.getVar<int[]>("deck").Length);
        }
        [Test]
        public void initReplaceTest()
        {
            Game game = new Game(new UnitTestGameEnv());
            TestFrontend[] frontends = new TestFrontend[2];
            frontends[0] = new TestFrontend();
            frontends[1] = new TestFrontend();
            game.addPlayer(frontends[0], new int[] { 1000, 1, 1, 1, 1, 1 });
            game.addPlayer(frontends[1], new int[] { 2000, 1, 1, 1, 1, 1 });
            game.init();
            int p0c0 = frontends[0].witnessList[0].getVar<int[][]>("initCardsRID")[0][0];
            game.initReplace(0, new int[] { p0c0 });
            int p1c0 = frontends[1].witnessList[0].getVar<int[][]>("initCardsRID")[1][0];
            int p1c1 = frontends[1].witnessList[0].getVar<int[][]>("initCardsRID")[1][1];
            game.initReplace(1, new int[] { p1c0, p1c1 });

            Assert.AreEqual(5, frontends[0].witnessList.Count);
            EventWitness witness = frontends[0].witnessList[1];
            Assert.AreEqual("onInitReplace", witness.eventName);
            Assert.AreEqual(0, witness.getVar<int>("playerIndex"));
            Assert.AreEqual(1, witness.getVar<int[]>("replacedCardsRID").Length);
            witness = frontends[0].witnessList[2];
            Assert.AreEqual("onInitReplace", witness.eventName);
            Assert.AreEqual(1, witness.getVar<int>("playerIndex"));
            Assert.AreEqual(2, witness.getVar<int[]>("replacedCardsRID").Length);
            witness = frontends[0].witnessList[3];
            Assert.AreEqual("onStart", witness.eventName);
            //玩家回合开始
            witness = frontends[0].witnessList[4];
            Assert.AreEqual("onTurnStart", witness.eventName);
            int firstPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[0];
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int>("playerIndex"));
            //增加法力水晶并充满
            Assert.AreEqual("onMaxGemChange", witness.child[0].eventName);
            Assert.AreEqual(1, witness.child[0].getVar<int>("value"));
            Assert.AreEqual("onGemChange", witness.child[1].eventName);
            Assert.AreEqual(1, witness.child[1].getVar<int>("value"));
            //抽一张卡
            Assert.AreEqual("onDraw", witness.child[2].eventName);
            Assert.AreEqual(firstPlayerIndex, witness.child[2].getVar<int>("playerIndex"));

            Assert.AreEqual(5, frontends[1].witnessList.Count);
            witness = frontends[1].witnessList[1];
            Assert.AreEqual("onInitReplace", witness.eventName);
            Assert.AreEqual(0, witness.getVar<int>("playerIndex"));
            Assert.AreEqual(1, witness.getVar<int[]>("replacedCardsRID").Length);
            witness = frontends[1].witnessList[2];
            Assert.AreEqual("onInitReplace", witness.eventName);
            Assert.AreEqual(1, witness.getVar<int>("playerIndex"));
            Assert.AreEqual(2, witness.getVar<int[]>("replacedCardsRID").Length);
            witness = frontends[1].witnessList[3];
            Assert.AreEqual("onStart", witness.eventName);
            //玩家回合开始
            witness = frontends[1].witnessList[4];
            Assert.AreEqual("onTurnStart", witness.eventName);
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int>("playerIndex"));
            //增加法力水晶并充满
            Assert.AreEqual("onMaxGemChange", witness.child[0].eventName);
            Assert.AreEqual(1, witness.child[0].getVar<int>("value"));
            Assert.AreEqual("onGemChange", witness.child[1].eventName);
            Assert.AreEqual(1, witness.child[1].getVar<int>("value"));
            //抽一张卡
            Assert.AreEqual("onDraw", witness.child[2].eventName);
            Assert.AreEqual(firstPlayerIndex, witness.child[2].getVar<int>("playerIndex"));
        }
        [Test]
        public void useTest()
        {
            Game game = new Game(new UnitTestGameEnv());
            TestFrontend[] frontends = new TestFrontend[2];
            frontends[0] = new TestFrontend();
            frontends[1] = new TestFrontend();
            game.addPlayer(frontends[0], new int[] { 1000, 1, 1, 1, 1, 1 });
            game.addPlayer(frontends[1], new int[] { 2000, 1, 1, 1, 1, 1 });
            game.init();
            int firstPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[0];
            game.initReplace(0, new int[0]);
            game.initReplace(1, new int[0]);
            int p0c0 = frontends[0].witnessList[0].getVar<int[][]>("initCardsRID")[firstPlayerIndex][0];
            game.use(firstPlayerIndex, p0c0, 0, new int[0]);

            EventWitness witness = frontends[0].witnessList.Find(e => { return e.eventName == "onUse"; });
            Assert.IsNotNull(witness);
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int>("playerIndex"));
            Assert.AreEqual(p0c0, witness.getVar<int>("cardRID"));
            Assert.AreEqual(1, witness.getVar<int>("cardDID"));
            Assert.AreEqual(0, witness.getVar<int>("targetPosition"));
            Assert.AreEqual(0, witness.getVar<int[]>("targetCardsRID").Length);
            Assert.AreEqual("onGemChange", witness.child[0].eventName);
            Assert.AreEqual(1, witness.child[0].getVar<int>("value"));
            Assert.AreEqual("onSummon", witness.child[1].eventName);
            Assert.AreEqual(firstPlayerIndex, witness.child[1].getVar<int>("playerIndex"));
            Assert.AreEqual(1, witness.child[1].getVar<int>("cardDID"));
            Assert.AreEqual(0, witness.child[1].getVar<int>("position"));

            witness = frontends[1].witnessList.Find(e => { return e.eventName == "onUse"; });
            Assert.IsNotNull(witness);
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int>("playerIndex"));
            Assert.AreEqual(p0c0, witness.getVar<int>("cardRID"));
            Assert.AreEqual(1, witness.getVar<int>("cardDID"));
            Assert.AreEqual(0, witness.getVar<int>("targetPosition"));
            Assert.AreEqual(0, witness.getVar<int[]>("targetCardsRID").Length);
            Assert.AreEqual("onGemChange", witness.child[0].eventName);
            Assert.AreEqual(1, witness.child[0].getVar<int>("value"));
            Assert.AreEqual("onSummon", witness.child[1].eventName);
            Assert.AreEqual(firstPlayerIndex, witness.child[1].getVar<int>("playerIndex"));
            Assert.AreEqual(1, witness.child[1].getVar<int>("cardDID"));
            Assert.AreEqual(0, witness.child[1].getVar<int>("position"));
        }
        [Test]
        public void turnEndTest()
        {
            Game game = new Game(new UnitTestGameEnv());
            TestFrontend[] frontends = new TestFrontend[2];
            frontends[0] = new TestFrontend();
            frontends[1] = new TestFrontend();
            game.addPlayer(frontends[0], new int[] { 1000, 1, 1, 1, 1, 1 });
            game.addPlayer(frontends[1], new int[] { 2000, 1, 1, 1, 1, 1 });
            game.init();
            int firstPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[0];
            int secondPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[1];
            game.initReplace(0, new int[0]);
            game.initReplace(1, new int[0]);
            game.turnEnd(firstPlayerIndex);

            EventWitness witness = frontends[0].witnessList.Find(e => { return e.eventName == "onTurnEnd"; });
            Assert.IsNotNull(witness);
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int>("playerIndex"));
            witness = frontends[0].witnessList[frontends[0].witnessList.IndexOf(witness) + 1];
            Assert.AreEqual("onTurnStart", witness.eventName);
            Assert.AreEqual(secondPlayerIndex, witness.getVar<int>("playerIndex"));
        }
        [Test]
        public void burnTest()
        {
            Game game = new Game(new UnitTestGameEnv());
            TestFrontend[] frontends = new TestFrontend[2];
            frontends[0] = new TestFrontend();
            frontends[1] = new TestFrontend();
            game.addPlayer(frontends[0], new int[] { 1000, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            game.addPlayer(frontends[1], new int[] { 2000, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            game.init();
            int firstPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[0];
            int secondPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[1];
            game.initReplace(0, new int[0]);
            game.initReplace(1, new int[0]);
            for (int i = 0; i < 7; i++)
            {
                game.turnEnd(firstPlayerIndex);
                game.turnEnd(secondPlayerIndex);
            }

            EventWitness witness = frontends[0].witnessList[frontends[0].witnessList.Count - 1].child[2];
            Assert.AreEqual("onBurn", witness.eventName);
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int>("playerIndex"));
            Assert.AreEqual(1, witness.getVar<int>("cardDID"));
            Assert.IsTrue(witness.getVar<int>("cardRID") > 0);
        }
        [Test]
        public void tiredTest()
        {
            Game game = new Game(new UnitTestGameEnv());
            TestFrontend[] frontends = new TestFrontend[2];
            frontends[0] = new TestFrontend();
            frontends[1] = new TestFrontend();
            game.addPlayer(frontends[0], new int[] { 1000, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            game.addPlayer(frontends[1], new int[] { 2000, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            game.init();
            int firstPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[0];
            int secondPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[1];
            game.initReplace(0, new int[0]);
            game.initReplace(1, new int[0]);
            for (int i = 0; i < 8; i++)
            {
                game.turnEnd(firstPlayerIndex);
                game.turnEnd(secondPlayerIndex);
            }

            EventWitness witness = frontends[0].witnessList[frontends[0].witnessList.Count - 1].child[2];
            Assert.AreEqual("onTired", witness.eventName);
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int>("playerIndex"));
            witness = witness.child[0];
            Assert.AreEqual("onDamage", witness.eventName);
            Assert.AreEqual(1, witness.getVar<int[]>("amounts")[0]);
        }
        [Test]
        public void attackTest()
        {
            Game game = new Game(new UnitTestGameEnv());
            TestFrontend[] frontends = new TestFrontend[2];
            frontends[0] = new TestFrontend();
            frontends[1] = new TestFrontend();
            game.addPlayer(frontends[0], new int[] { 1000, 1, 1, 1, 1, 1 });
            game.addPlayer(frontends[1], new int[] { 2000, 1, 1, 1, 1, 1 });
            game.init();
            int firstPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[0];
            int secondPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[1];
            int p0c0 = frontends[0].witnessList[0].getVar<int[][]>("initCardsRID")[firstPlayerIndex][0];
            int p1c0 = frontends[1].witnessList[0].getVar<int[][]>("initCardsRID")[secondPlayerIndex][0];
            game.initReplace(0, new int[0]);
            game.initReplace(1, new int[0]);
            game.use(firstPlayerIndex, p0c0, 0, new int[0]);
            game.turnEnd(firstPlayerIndex);
            game.use(secondPlayerIndex, p1c0, 0, new int[0]);
            game.turnEnd(secondPlayerIndex);
            game.attack(firstPlayerIndex, p0c0, p1c0);

            EventWitness witness = frontends[0].witnessList.Find(e => { return e.eventName == "onAttack"; });
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int>("playerIndex"));
            Assert.AreEqual(p0c0, witness.getVar<int>("cardRID"));
            Assert.AreEqual(p1c0, witness.getVar<int>("targetCardRID"));
            witness = witness.child[0];
            Assert.AreEqual("onDamage", witness.eventName);
            Assert.AreEqual(p0c0, witness.getVar<int[]>("cardsRID")[0]);
            Assert.AreEqual(p1c0, witness.getVar<int[]>("cardsRID")[1]);
            Assert.AreEqual(1, witness.getVar<int[]>("amounts")[0]);
            Assert.AreEqual(1, witness.getVar<int[]>("amounts")[1]);
            witness = witness.child[0];
            Assert.AreEqual("onDeath", witness.eventName);
            Assert.AreEqual(p0c0, witness.getVar<int[]>("cardsRID")[0]);
            Assert.AreEqual(p1c0, witness.getVar<int[]>("cardsRID")[1]);
        }
        [Test]
        public void winTest()
        {
            Game game = new Game(new UnitTestGameEnv());
            TestFrontend[] frontends = new TestFrontend[2];
            frontends[0] = new TestFrontend();
            frontends[1] = new TestFrontend();
            game.addPlayer(frontends[0], new int[] { 1000, 1, 1, 1, 1, 1 });
            game.addPlayer(frontends[1], new int[] { 2000, 1, 1, 1, 1, 1 });
            game.init();
            int firstPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[0];
            int secondPlayerIndex = frontends[0].witnessList[0].getVar<int[]>("sortedPlayersIndex")[1];
            int cardRID = frontends[0].witnessList[0].getVar<int[][]>("initCardsRID")[firstPlayerIndex][0];
            int targetCardRID = frontends[1].witnessList[0].getVar<int[]>("masterCardsRID")[secondPlayerIndex];
            game.initReplace(0, new int[0]);
            game.initReplace(1, new int[0]);
            game.use(firstPlayerIndex, cardRID, 0, new int[0]);
            for (int i = 0; i < 30; i++)
            {
                game.turnEnd(firstPlayerIndex);
                game.turnEnd(secondPlayerIndex);
                game.attack(firstPlayerIndex, cardRID, targetCardRID);
            }

            EventWitness witness = frontends[0].witnessList[frontends[0].witnessList.Count - 1];
            Assert.AreEqual("onAttack", witness.eventName);
            witness = witness.child[0];
            Assert.AreEqual("onDamage", witness.eventName);
            witness = witness.child[0];
            Assert.AreEqual("onDeath", witness.eventName);
            witness = witness.child[0];
            Assert.AreEqual("onGameEnd", witness.eventName);
            Assert.AreEqual(1, witness.getVar<int[]>("winnerPlayersIndex").Length);
            Assert.AreEqual(firstPlayerIndex, witness.getVar<int[]>("winnerPlayersIndex")[0]);
        }
    }
}
