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
                new KeyValuePair<int, int>(DefaultMystery.ID, 4),
                new KeyValuePair<int, int>(DefaultMystery_zf.ID, 4)
            );

            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 3);
            oppo.cmdUse(game, oppo.hand.First(),0);
            oppo.cmdUse(game, oppo.hand.First(),1);
            oppo.cmdUse(game, oppo.hand.First(),2);
            Card card = oppo.field[0];
            Card card2 = oppo.field[1];
            oppo.cmdTurnEnd(game);


            Card secret = you.hand.getCard<DefaultMystery>();
            you.cmdUse(game, secret);
            Pile.MoveCardEventArg movecardevent = game.triggers.getRecordedEvents().OfType<Pile.MoveCardEventArg>().LastOrDefault(c=>c.to == you.head);
            Assert.True(movecardevent.card == secret);//奥秘挂上了(移动至Head牌堆)
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, card, you.master);
            THHCard.SecretEventArg secretevent = game.triggers.getRecordedEvents().OfType<THHCard.SecretEventArg>().Last();
            Assert.True(secretevent.card == secret);//奥秘触发了
            Pile.MoveCardEventArg movecardevent2 = game.triggers.getRecordedEvents().OfType<Pile.MoveCardEventArg>().LastOrDefault(c => c.from == you.head);
            Assert.True(movecardevent2.card == secret);//奥秘送入墓地
            Assert.True(oppo.hand.ToArray().Contains(card));//回到手牌

            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            you.cmdUse(game, you.hand.getCard<DefaultMystery_zf>());
            you.cmdUse(game, you.hand.getCard<DefaultMystery_zf>());//不能使用相同的奥秘
            you.cmdUse(game, you.hand.getCard<DefaultMystery>());//挂一个蒸发
            you.cmdTurnEnd(game);

            oppo.cmdAttack(game, card2, you.master);//触发蒸发，不触发冰冻
            Assert.True(you.head.Any(c => c.define == game.getDefine<DefaultMystery>()));
            oppo.cmdAttack(game, oppo.field[0], you.master);//触发冰冻
        }
    }
}