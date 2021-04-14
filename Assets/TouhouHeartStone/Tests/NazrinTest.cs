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
using System.Threading.Tasks;
using UI;
using Card = TouhouCardEngine.Card;

namespace Tests
{
    public class NazrinTests
    {
        [Test]
        public void Nazrin_01Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(Nazrin_01.ID, 1));
            game.skipUntilHasGem(you, 3);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<Nazrin_01>(),targets: you.getField()[0]);

            Assert.True(you.getField()[0].hasKeyword(game, KEYWORD.SHIELD));
            game.Dispose();
        }

        [Test]
        public void Nazrin_02Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(Nazrin_02.ID, 1));
            game.skipUntilCanUse<Nazrin_02>(you);
            you.cmdUse(game, you.getHand().getCard<Nazrin_02>());
            you.draw(game);
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            Assert.NotNull(discoverRequest);
            int id = discoverRequest.cardIdArray[0];
            you.cmdDiscover(game, id);

            game.Dispose();
        }

        [Test]
        public void Nazrin_03Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(Nazrin_03.ID, 1));
            game.skipUntilCanUse<Nazrin_03>(you);
            you.cmdUse(game, you.getHand().getCard<Nazrin_03>());
            
            you.cmdTurnEnd(game);

            game.Dispose();
        }

        [Test]
        public void Nazrin_04Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Rin.ID,
                new KeyValuePair<int, int>(Nazrin_04.ID, 1));
            game.skipUntilCanUse<Nazrin_04>(you);
            you.cmdUse(game, you.getHand().getCard<Nazrin_04>());
            Assert.True(you.getHead().Any(c => c.define is Nazrin_04));

            for(int i=0; i<15; i++)
            {
                game.skipUntilCanUse<DefaultServant>(you);
                you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            }

            Assert.True(you.getHand().Any(c => c.define is Nazrin_05));

        }


    }


}