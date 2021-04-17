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
            TestGameflow.createGame(out var game, out var you, out var oppo, Nazrin.ID,
                new KeyValuePair<int, int>(Nazrin_01.ID, 1));
            game.skipUntilHasGem(you, 3);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.cmdUse(game, you.getHand().getCard<Nazrin_01>(), targets: you.getField()[0]);

            Assert.True(you.getField()[0].hasKeyword(game, KEYWORD.SHIELD));
            game.Dispose();
        }

        [Test]
        public void Nazrin_02Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Nazrin.ID,
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
            TestGameflow.createGame(out var game, out var you, out var oppo, Nazrin.ID,
                new KeyValuePair<int, int>(Nazrin_03.ID, 1));
            game.skipUntilCanUse<Nazrin_03>(you);
            you.cmdUse(game, you.getHand().getCard<Nazrin_03>());

            you.cmdTurnEnd(game);

            game.Dispose();
        }

        [Test]
        public void Nazrin_04Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Nazrin.ID,
                new KeyValuePair<int, int>(Nazrin_04.ID, 1));
            game.skipUntilCanUse<Nazrin_04>(you);
            you.cmdUse(game, you.getHand().getCard<Nazrin_04>());
            Assert.True(you.getHead().Any(c => c.define is Nazrin_04));
            Card card = you.getHead()[0];
            for (int i = 0; i < 15; i++)
            {
                //这方法居然不能推进进度？？？
                //you.createToken(game, game.getDefine<DefaultServant>());
                //you.getField()[0].setDead(game, true);
                //game.updateDeath();
                if (!you.getHead().Any(c => c.define is DefaultServant))
                {
                    you.tryAddCardToHand(game, game.createCard<DefaultServant>());
                }

                if (you.getGem() == 0)
                {
                    you.setGem(game, you.getMaxGem());
                }
                you.cmdUse(game, you.getHand().getCard<DefaultServant>());
                you.getField()[0].setDead(game, true);
                game.updateDeath();
            }
            Debug.Log("宝塔数量 ： " + you.getHand().Where(c => c.define is Nazrin_05).Count());


            //宝塔测试

            game.skipUntilCanUse<Nazrin_05>(you);

            int count = you.getHand().Count();
            int j = 0;

            while (!you.getHand().isFull)
            {
                you.tryAddCardToHand(game, game.createCard<DefaultServant>());
            }

            foreach (Card c in you.getHand())
            {
                j++;
                Debug.Log("手牌 " + j + " 费用为 " + c.getCost(game));
            }

            you.cmdUse(game, you.getHand().getCard<Nazrin_05>());
            Assert.True(you.getHand().Any(c => c.define is Nazrin_06));

            j = 0;
            foreach (Card c in you.getHand())
            {
                j++;
                Debug.Log("手牌 " + j + " 费用为 " + c.getCost(game));
            }

            //宝藏测试
            game.skipUntilCanUse<Nazrin_06>(you);
            you.cmdUse(game, you.getHand().getCard<Nazrin_06>());
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            int cardId = discoverRequest.cardIdArray[0];
            you.cmdDiscover(game, cardId);

            game.Dispose();
        }

        [Test]
        public void Nazrin_07Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Nazrin.ID,
                new KeyValuePair<int, int>(Nazrin_07.ID, 1));

            game.skipUntilCanUse<Nazrin_07>(you);
            you.cmdUse(game, you.getHand().getCard<Nazrin_07>());
            DiscoverRequest discoverRequest = game.answers.getRequest<DiscoverRequest>(you.id);
            int cardId = discoverRequest.cardIdArray[0];
            you.cmdDiscover(game, cardId);

            game.Dispose();
        }

        [Test]
        public void Nazrin_08Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Nazrin.ID,
                new KeyValuePair<int, int>(Nazrin_08.ID, 1));
            game.skipUntilCanUse<Nazrin_08>(you);
            you.cmdUse(game, you.getHand().getCard<Nazrin_08>());
            you.cmdTurnEnd(game);
            oppo.cmdTurnEnd(game);
            Assert.False(you.getField()[0].canAttack(game));
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());
            you.createToken(game, game.getDefine<DefaultServant>());

            game.Dispose();

        }

        [Test]
        public void Nazrin_09Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Nazrin.ID,
                new KeyValuePair<int, int>(Nazrin_09.ID, 1));
            game.skipUntilCanUse<Nazrin_09>(you);
            you.cmdUse(game, you.getHand().getCard<Nazrin_09>());
            you.getField()[0].setDead(game, true);
            game.updateDeath();

            Assert.True(you.getHand().Any(c => c.define is Nazrin_10));

            //奶酪测试
            you.tryAddCardToHand(game, game.createCard<Nazrin_10>());
            game.skipUntilCanUse<Nazrin_10>(you);
            you.cmdUse(game, you.getHand().getCard<Nazrin_10>(),targets: you.getMaster());
            game.skipUntilCanUse<Nazrin_10>(you);
            you.getMaster().damage(game, null, 5);
            you.cmdUse(game, you.getHand().getCard<Nazrin_10>(), targets: you.getMaster());

            game.Dispose();
        }

        [Test]
        public void Nazrin_15Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Nazrin.ID,
                new KeyValuePair<int, int>(Nazrin_15.ID, 1));
            game.skipUntilCanUse<Nazrin_15>(you);
            you.cmdUse(game, you.getHand().getCard<Nazrin_15>(), targets: you.getMaster());

            game.skipUntilCanUse<DefaultServant>(you);
            you.cmdUse(game, you.getHand().getCard<DefaultServant>());

            game.Dispose();
        }

        [Test]
        public void Nazrin_19Test()
        {
            TestGameflow.createGame(out var game, out var you, out var oppo, Nazrin.ID,
                new KeyValuePair<int, int>(Nazrin_19.ID, 1));
            game.skipUntilCanUse<Nazrin_19>(you);

            you.cmdUse(game, you.getHand().getCard<Nazrin_19>());
            you.tryAddCardToHand(game, game.createCard<UniqueServant>());
            you.cmdUse(game, you.getHand().getCard<UniqueServant>());

            game.Dispose();

        }
    }
}