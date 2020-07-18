using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
namespace Tests
{
    public static class TestGameflow
    {
        public static void createGame(out THHGame game, out THHPlayer you, out THHPlayer oppo, params KeyValuePair<int, int>[] yourDeck)
        {
            createGame(out game, out you, out oppo, new GameOption() { shuffle = false }, yourDeck);
        }
        public static void createGame(out THHGame game, out THHPlayer you, out THHPlayer oppo, GameOption option, params KeyValuePair<int, int>[] yourDeck)
        {
            game = initGameWithoutPlayers(null, option);
            IEnumerable<CardDefine> yourTrueDeck = yourDeck.Length > 0 ?
 Enumerable.Repeat(game.getCardDefine(yourDeck[0].Key), yourDeck[0].Value) :
 Enumerable.Repeat(game.getCardDefine(DefaultServant.ID), 30);
            for (int i = 1; i < yourDeck.Length; i++)
            {
                yourTrueDeck = yourTrueDeck.Concat(Enumerable.Repeat(game.getCardDefine(yourDeck[i].Key), yourDeck[i].Value));
            }
            int count = yourTrueDeck.Count();
            if (count < 30)
                yourTrueDeck = yourTrueDeck.Concat(Enumerable.Repeat(game.getCardDefine<DefaultServant>(), 30 - count));
            yourTrueDeck = yourTrueDeck.Reverse();
            you = game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster>(), yourTrueDeck);
            oppo = game.createPlayer(2, "玩家2", game.getCardDefine<TestMaster>(), Enumerable.Repeat(game.getCardDefine<DefaultServant>() as CardDefine, 30));
        }
        public static void createGameWithoutDeck(out THHGame game, out THHPlayer you, out THHPlayer oppo, GameOption option = null)
        {
            game = initGameWithoutPlayers(null, option);
            you = game.createPlayer(1, "玩家1", game.getCardDefine<TestMaster>(), null);
            oppo = game.createPlayer(2, "玩家2", game.getCardDefine<TestMaster>(), null);
        }
        public static THHGame initStandardGame(string name = null, int deckCount = 30, int[] playersId = null, GameOption option = null)
        {
            return initStandardGame(name, playersId,
                Enumerable.Repeat(new TestMaster(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new TestServant(), deckCount).ToArray(), 2).ToArray(),
                option);
        }
        public static THHGame initStandardGame(string name, int[] playersId, MasterCardDefine[] masters, CardDefine[][] decks, GameOption option)
        {
            THHGame game = initGameWithoutPlayers(name, option);
            if (playersId == null)
                playersId = new int[] { 1, 2 };
            if (masters == null)
                masters = Enumerable.Repeat(game.getCardDefine<TestMaster>(), playersId.Length).ToArray();
            if (decks == null)
                decks = Enumerable.Repeat(Enumerable.Repeat(game.getCardDefine<TestServant>(), 30).ToArray(), playersId.Length).ToArray();
            if (option == null)
                option = GameOption.Default;
            for (int i = 0; i < playersId.Length; i++)
            {
                game.createPlayer(playersId[i], "玩家" + playersId[i], masters[i], decks[i]);
            }
            return game;
        }
        public static THHGame initGameWithoutPlayers(string name, GameOption option)
        {
            TaskExceptionHandler.register();
            ULogger logger = new ULogger(name) { blackList = new List<string>() { "Load" } };
            THHGame game = new THHGame(option != null ? option : GameOption.Default, CardHelper.getCardDefines(logger))
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                logger = logger
            };
            game.answers.game = game;
            (game.triggers as TriggerManager).logger = game.logger;
            return game;
        }
    }
}