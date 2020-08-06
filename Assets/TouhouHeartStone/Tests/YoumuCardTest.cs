using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;
using System.Collections.Generic;
namespace Tests
{
    public class YoumuCardTest
    {
        [Test]
        public void WonderfulhalfbodyTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Wonderfulhalfbody.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 2);
            you.cmdUse(game, you.hand.First());
            you.master.damage(game, null, 1);
            you.master.damage(game, null, 4);
        }

        [Test]
        public void MasterAttackTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Wonderfulhalfbody.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 1);
            oppo.cmdUse(game, oppo.hand.First());//对手拍一张白板随从
            oppo.cmdTurnEnd(game);
            you.master.setAttack(5);
            you.cmdAttack(game, you.master, target: oppo.field[0]);
        }

        [Test]
        public void CutInTheWorldTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(CutInTheWorld.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            you.cmdUse(game, you.hand.First());
            you.cmdUse(game, you.hand.Last());
        }

        [Test]
        public void MeditationSlashTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(MeditationSlash.ID, 8),
                new KeyValuePair<int, int>(PhilosopherStone.ID,8)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 5);
            you.cmdUse(game, you.hand.getCard<MeditationSlash>());
            Card card = game.triggers.getRecordedEvents().OfType<THHPlayer.DrawEventArg>().Last().card;
            Assert.Equals(1, card.getAttack(game));
        }

        [Test]
        public void EnlightenmentTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Enlightenment.ID, 8)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 1);
            you.cmdUse(game, you.hand.Last());
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 3);
            you.cmdUse(game, you.hand.First());
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
        }

        [Test]
        public void HalfSpiritSlamTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(HalfSpiritSlam.ID, 3),
                new KeyValuePair<int, int>(Enlightenment.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 1);
            oppo.cmdUse(game, oppo.hand.First());//对手拍一张白板随从
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 2);
            you.cmdUse(game, you.hand.getCard<Enlightenment>());
            you.cmdUse(game, you.hand.getCard<HalfSpiritSlam>(),targets:oppo.field[0]);
            Assert.True(game.triggers.getRecordedEvents().OfType<THHCard.DamageEventArg>().Last().value == 6);
        }

        [Test]
        public void UnderworlddwellerTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(Underworlddweller.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 2);
            you.cmdUse(game, you.hand.getCard<Underworlddweller>());
            you.field[0].die(game);
            you.cmdUse(game, you.hand.getCard<Ghost>());
        }

        [Test]
        public void LouGuanjianTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LouGuanjian.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 2);
            oppo.cmdUse(game, oppo.hand.First(),0);//对手拍一张白板随从
            oppo.cmdUse(game, oppo.hand.First(),1);//对手拍一张白板随从
            oppo.cmdTurnEnd(game);

            you.master.setAttack(5);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 4);
            you.cmdUse(game, you.hand.First());
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            you.cmdAttack(game, you.master, target: oppo.master);
        }

        [Test]
        public void MonsterBlacksmithTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LouGuanjian.ID, 3),
                new KeyValuePair<int, int>(MonsterBlacksmith.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 2);
            oppo.cmdUse(game, oppo.hand.First(), 0);//对手拍一张白板随从
            oppo.cmdUse(game, oppo.hand.First(), 1);//对手拍一张白板随从
            oppo.cmdTurnEnd(game);

            you.master.setAttack(5);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 5);
            //you.cmdUse(game, you.hand.getCard<MonsterBlacksmith>());
            you.cmdUse(game, you.hand.First(c=>c.define is LouGuanjian));
            you.item.setImmunity(true);
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            you.cmdAttack(game, you.master, target: oppo.master);
        }
        
        [Test]
        public void YoukiKonpakuTest()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo,
                new KeyValuePair<int, int>(LouGuanjian.ID, 3),
                new KeyValuePair<int, int>(YoukiKonpaku.ID, 3)
            );
            game.skipTurnUntil(() => game.currentPlayer == oppo && oppo.gem >= 2);
            oppo.cmdUse(game, oppo.hand.First(), 0);//对手拍一张白板随从
            oppo.cmdUse(game, oppo.hand.First(), 1);//对手拍一张白板随从
            oppo.cmdTurnEnd(game);

            you.master.setAttack(5);
            game.skipTurnUntil(() => game.currentPlayer == you && you.gem >= 5);
            you.cmdUse(game, you.hand.getCard<YoukiKonpaku>());
            you.cmdUse(game, you.hand.First(c => c.define is LouGuanjian));
            //you.item.setImmunity(true);
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            you.cmdAttack(game, you.master, target: oppo.field[0]);
            you.cmdAttack(game, you.master, target: oppo.master);
        }
    }
}

