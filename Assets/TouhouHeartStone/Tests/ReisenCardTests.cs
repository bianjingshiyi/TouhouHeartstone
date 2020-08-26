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
using static TouhouHeartstone.Builtin.Proble.Probe;

namespace Tests
{
    public class ResisenCardTests
    {
        [Test]
        public void ResisenAnalepticTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<Analeptic>(), 3)));

            game.skipUntilHasGem(you, 2);
            you.cmdUse(game, you.skill);

            game.skipUntilHasGem(you, 8);
            you.cmdUse(game, you.skill);
            you.cmdUse(game, you.hand.getCard<Analeptic>());

            you.cmdUse(game, you.skill);
            you.cmdUse(game, you.hand.getCard<Analeptic>());

            you.cmdUse(game, you.skill);
            you.cmdUse(game, you.hand.getCard<Analeptic>());

            you.cmdUse(game, you.skill);
            game.Dispose();
        }

        [Test]
        public void ResisenMarinTest()
        {
            THHGame game = TestGameflow.initGameWithoutPlayers(null, new GameOption()
            {
                shuffle = false
            });
            THHPlayer oppo = game.createPlayer(0, "玩家0", game.getDefine<TestMaster2>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 30));
            THHPlayer you = game.createPlayer(1, "玩家1", game.getDefine<Reisen>(),
                Enumerable.Repeat(game.getDefine<DefaultServant>() as CardDefine, 27).Concat(Enumerable.Repeat(game.getDefine<Marin>(), 3)));
   
            game.skipUntilHasGem(you, 4);
            you.cmdUse(game, you.hand.getCard<Marin>());

            you.cmdUse(game, you.skill);
            
            game.Dispose();
        }

        [Test]
        public void PowrOverwhelmingTest()
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

        [Test]
        public void DefaultMysteryTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(DefaultMystery.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 2);
            oppo.cmdUse(game, oppo.hand.First());
            Card card = oppo.field[0];
            oppo.cmdTurnEnd(game);

            you.cmdUse(game, you.hand.getCard<DefaultMystery>());
            THHCard.SecretEventArg secretevent = game.triggers.getRecordedEvents().OfType<THHCard.SecretEventArg>().Last();
            Assert.True(secretevent.card == you.master);//奥秘挂上了
            Assert.AreEqual(1,you.master.getSecret(game));//挂了1个奥秘
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, card, you.master);
            THHCard.SecretEventArg secretevent2 = game.triggers.getRecordedEvents().OfType<THHCard.SecretEventArg>().LastOrDefault(c => c.type == THHCard.SecretEventArg.TriggerSecret);
            Assert.True(secretevent2.card == you.master);//奥秘触发了
            Assert.AreEqual(0, you.master.getSecret(game));//没有奥秘了
            Assert.True(oppo.hand.ToArray().Contains(card));//回到手牌
        }    }
}