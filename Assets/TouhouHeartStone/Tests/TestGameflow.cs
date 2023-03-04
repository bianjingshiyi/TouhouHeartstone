using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using BJSYGameCore;
using NitoriNetwork.Common;
using System.Reflection;
using Game;
using System.Threading.Tasks;
using TouhouCardEngine.Shared;

namespace Tests
{
    public static class TestGameflow
    {
        public static void createGame(out CardEngine game, out Player you, out Player oppo, params KeyValuePair<int, int>[] yourDeck)
        {
            createGame(out game, out you, out oppo, new HearthstoneOption() { shuffle = false }, yourDeck);
        }
        public static void createGame(out CardEngine game, out Player you, out Player oppo, KeyValuePair<int, int>[] yourDeck, KeyValuePair<int, int>[] oppoDeck)
        {
            createGame(out game, out you, out oppo, new HearthstoneOption() { shuffle = false }, TestMaster.ID, yourDeck, TestMaster.ID, oppoDeck);
        }
        public static void createGame(out CardEngine game, out Player you, out Player oppo, int yourMaster)
        {
            createGame(out game, out you, out oppo, new HearthstoneOption() { shuffle = false }, yourMaster);
        }
        public static void createGame(out CardEngine game, out Player you, out Player oppo, HearthstoneOption option, params KeyValuePair<int, int>[] yourDeck)
        {
            createGame(out game, out you, out oppo, option, TestMaster.ID, yourDeck);
        }
        public static void createGame(out CardEngine game, out Player you, out Player oppo, HearthstoneOption option, int yourMaster, params KeyValuePair<int, int>[] yourDeck)
        {
            createGame(out game, out you, out oppo, option, yourMaster, yourDeck, TestMaster.ID, new KeyValuePair<int, int>[0]);
        }
        public static void createGame(out CardEngine game, out Player you, out Player oppo, int yourMaster, params KeyValuePair<int, int>[] yourDeck)
        {
            createGame(out game, out you, out oppo, new HearthstoneOption() { shuffle = false }, yourMaster, yourDeck, TestMaster.ID, new KeyValuePair<int, int>[0]);
        }
        public static void createGame(out CardEngine game, out Player you, out Player oppo, HearthstoneOption option,
            int yourMaster, KeyValuePair<int, int>[] yourDeck,
            int oppoMaster, KeyValuePair<int, int>[] oppoDeck)
        {
            game = initGameWithoutPlayers(null, option);
            IEnumerable<CardDefine> yourTrueDeck = createDeck(game, yourDeck);
            IEnumerable<CardDefine> oppoTrueDeck = createDeck(game, oppoDeck);
            you = game.createPlayer(1, "玩家1", game.getDefine<MasterCardDefine>(0, yourMaster), yourTrueDeck);
            oppo = game.createPlayer(2, "玩家2", game.getDefine<MasterCardDefine>(0, oppoMaster), oppoTrueDeck);
        }
        public static async Task<(CardEngine game, Player you, Player oppo)> createGameWithEditorCards(HearthstoneOption option,
            int yourMaster, KeyValuePair<int, int>[] yourDeck,
            int oppoMaster, KeyValuePair<int, int>[] oppoDeck)
        {
            CardEngine game = await initGameWithoutPlayers(null, option, true);
            IEnumerable<CardDefine> yourTrueDeck = createDeck(game, yourDeck);
            IEnumerable<CardDefine> oppoTrueDeck = createDeck(game, oppoDeck);
            Player you = game.createPlayer(1, "玩家1", game.getDefine(0, yourMaster), yourTrueDeck);
            Player oppo = game.createPlayer(2, "玩家2", game.getDefine(0, oppoMaster), oppoTrueDeck);
            return (game, you, oppo);
        }
        /// <summary>
        /// 使用模板分别填充你和对手的牌库
        /// </summary>
        /// <param name="game"></param>
        /// <param name="you"></param>
        /// <param name="oppo"></param>
        /// <param name="option"></param>
        /// <param name="yourMaster"></param>
        /// <param name="yourDeck"></param>
        /// <param name="oppoMaster"></param>
        /// <param name="oppoDeck"></param>
        /// <typeparam name="T"></typeparam>
        public static void createGame<YourDeck, OppoDeck>(out CardEngine game, out Player you, out Player oppo, HearthstoneOption option,
            int yourMaster, KeyValuePair<int, int>[] yourDeck,
            int oppoMaster, KeyValuePair<int, int>[] oppoDeck) where YourDeck : CardDefine where OppoDeck : CardDefine
        {
            game = initGameWithoutPlayers(null, option);
            IEnumerable<CardDefine> yourTrueDeck = createDeck<YourDeck>(game, yourDeck);
            IEnumerable<CardDefine> oppoTrueDeck = createDeck<OppoDeck>(game, oppoDeck);
            you = game.createPlayer(1, "玩家1", game.getDefine<MasterCardDefine>(0, yourMaster), yourTrueDeck);
            oppo = game.createPlayer(2, "玩家2", game.getDefine<MasterCardDefine>(0, oppoMaster), oppoTrueDeck);
        }

        public static void createGame<YourDeck, OppoDeck>(out CardEngine game, out Player you, out Player oppo,
            params KeyValuePair<int, int>[] yourDeck) where YourDeck : CardDefine where OppoDeck : CardDefine
        {
            createGame<YourDeck, OppoDeck>(out game, out you, out oppo, new HearthstoneOption() { shuffle = false }, TestMaster.ID, yourDeck, TestMaster.ID, new KeyValuePair<int, int>[0]);
        }

        private static IEnumerable<CardDefine> createDeck(CardEngine game, KeyValuePair<int, int>[] yourDeck)
        {
            return createDeck<DefaultServant>(game, yourDeck);
        }

        /// <summary>
        /// 使用模板填充牌库
        /// </summary>
        /// <param name="game"></param>
        /// <param name="yourDeck"></param>
        /// <typeparam name="_default"></typeparam>
        /// <returns></returns>
        private static IEnumerable<CardDefine> createDeck<_default>(CardEngine game, KeyValuePair<int, int>[] yourDeck) where _default : CardDefine
        {
            IEnumerable<CardDefine> yourTrueDeck = yourDeck.Length > 0 ?
                Enumerable.Repeat(game.getDefine(0, yourDeck[0].Key), yourDeck[0].Value) :
                Enumerable.Repeat(game.getDefine<_default>(), 30);
            for (int i = 1; i < yourDeck.Length; i++)
            {
                yourTrueDeck = yourTrueDeck.Concat(Enumerable.Repeat(game.getDefine(0, yourDeck[i].Key), yourDeck[i].Value));
            }
            int count = yourTrueDeck.Count();
            if (count < 30)
                yourTrueDeck = yourTrueDeck.Concat(Enumerable.Repeat(game.getDefine<_default>(), 30 - count));
            yourTrueDeck = yourTrueDeck.Reverse();
            return yourTrueDeck;
        }

        public static void createGameWithoutDeck(out CardEngine game, out Player you, out Player oppo, HearthstoneOption option = null)
        {
            game = initGameWithoutPlayers(null, option);
            you = game.createPlayer(1, "玩家1", game.getDefine<TestMaster>(), null);
            oppo = game.createPlayer(2, "玩家2", game.getDefine<TestMaster>(), null);
        }
        public static CardEngine initStandardGame(string name = null, int deckCount = 30, int[] playersId = null, HearthstoneOption option = null)
        {
            return initStandardGame(name, playersId,
                Enumerable.Repeat(new TestMaster(), 2).ToArray(),
                Enumerable.Repeat(Enumerable.Repeat(new TestServant(), deckCount).ToArray(), 2).ToArray(),
                option);
        }
        public static CardEngine initStandardGame(string name, int[] playersId, MasterCardDefine[] masters, CardDefine[][] decks, HearthstoneOption option)
        {
            CardEngine game = initGameWithoutPlayers(name, option);
            if (playersId == null)
                playersId = new int[] { 1, 2 };
            if (masters == null)
                masters = Enumerable.Repeat(game.getDefine<TestMaster>(), playersId.Length).ToArray();
            if (decks == null)
                decks = Enumerable.Repeat(Enumerable.Repeat(game.getDefine<TestServant>(), 30).ToArray(), playersId.Length).ToArray();
            if (option == null)
                option = HearthstoneOption.Default;
            for (int i = 0; i < playersId.Length; i++)
            {
                game.createPlayer(playersId[i], "玩家" + playersId[i], masters[i], decks[i]);
            }
            return game;
        }
        public static CardEngine initGameWithoutPlayers(string name, HearthstoneOption option)
        {
            TaskExceptionHandler.register();
            ULogger logger = new ULogger(name) { blackList = new List<string>() { "Load" } };
            CardEngine game = new CardEngine()
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                logger = logger
            };
            game.answers.game = game;
            (game.triggers as TriggerManager).logger = game.logger;
            CardDefine[] cardDefines = CardHelper.getCardDefines(logger);
            game.init(
                new Assembly[]
                {
                    typeof(CardEngine).Assembly,
                    typeof(HearthStoneRule).Assembly,
                    typeof(EffectEditorLibrary).Assembly
                },
                new HearthStoneRule(new Dictionary<long, Dictionary<int, CardDefine>>() { { 0, cardDefines.ToDictionary(c => c.id) } }, null),
                option,
                new RoomPlayerData[0]).Wait();
            return game;
        }
        public static async Task<CardEngine> initGameWithoutPlayers(string name, HearthstoneOption option, bool withEditorCards)
        {
            TaskExceptionHandler.register();
            ULogger logger = new ULogger(name) { blackList = new List<string>() { "Load" } };
            CardEngine game = new CardEngine()
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                logger = logger
            };
            game.answers.game = game;
            (game.triggers as TriggerManager).logger = game.logger;
            Dictionary<long, Dictionary<int, CardDefine>> cardDefines = new Dictionary<long, Dictionary<int, CardDefine>>()
            {
                { 0, CardHelper.getCardDefines(logger).ToDictionary(c => c.id) }
            };
            Dictionary<long, Dictionary<int, BuffDefine>> buffDefines = new Dictionary<long, Dictionary<int, BuffDefine>>();
            if (withEditorCards)
            {
                MainManager main = new GameObject(nameof(MainManager)).AddComponent<MainManager>();
                main.enabled = false;
                CardManager cardManager = new GameObject(nameof(CardManager)).AddComponent<CardManager>();
                cardManager.game = main;
                var result = await cardManager.loadCardPoolInfosAsync();
                foreach (var pair in result)
                {
                    await cardManager.loadCardPool(pair.Key);
                    cardDefines.Add(pair.Value.cardPoolId, cardManager.getCardDefines(pair.Value.ToReference()));
                    buffDefines.Add(pair.Value.cardPoolId, cardManager.getBuffDefines(pair.Value.ToReference()));
                }
            }
            game.init(new Assembly[]
            {
                typeof(CardEngine).Assembly,
                typeof(HearthStoneRule).Assembly,
                typeof(EffectEditorLibrary).Assembly
            },
            new HearthStoneRule(cardDefines, buffDefines),
            option,
            new RoomPlayerData[0]).Wait();
            return game;
        }
        //public static CardEngine initSyncStandardGame(int[] playersId = null, IEnumerable<CardDefine> cardDefines = null)
        //{
        //    TaskExceptionHandler.register();
        //    ULogger logger = new ULogger(null) { blackList = new List<string>() { "Load" } };
        //    if (cardDefines == null)
        //    {
        //        cardDefines = new CardDefine[]
        //        {
        //            new TestMaster(),
        //            new TestSkill(),
        //            new TestServant(),
        //        };
        //    }
        //    CardEngine game = new CardEngine(new HearthStoneRule(new Dictionary<long, Dictionary<int, CardDefine>>()
        //    {
        //        { 0, cardDefines.ToDictionary(c => c.id) }
        //    }, null))
        //    {
        //        answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
        //        triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
        //        time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
        //        logger = logger
        //    };
        //    game.answers.game = game;
        //    (game.triggers as TriggerManager).logger = game.logger;
        //    if (playersId == null)
        //        playersId = new int[] { 1, 2 };
        //    Player[] players = new Player[playersId.Length];
        //    for (int i = 0; i < players.Length; i++)
        //    {
        //        players[i] = new Player(i, "玩家" + i);
        //        int[] deck = new int[31];
        //        deck[0] = TestMaster.ID;
        //        for (int j = 0; j < 30; j++)
        //        {
        //            deck[j + 1] = TestServant.ID;
        //        }
        //        players[i].setProp(DEFINE.PLAYER_PROP_DECK, deck);
        //    }
        //    _ = game.startGame(HearthstoneOption.Default, players);
        //    return game;
        //}
    }
}