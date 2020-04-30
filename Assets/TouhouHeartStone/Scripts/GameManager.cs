using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TouhouHeartstone;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;
using BJSYGameCore;
using UI;
using ExcelLibrary.SpreadSheet;
namespace Game
{
    public class GameManager : Manager
    {
        [SerializeField]
        GameOption _option = new GameOption();
        [SerializeField]
        Main _ui;
        public THHGame game { get; private set; } = null;
        Task gameTask { get; set; } = null;
        protected override void onAwake()
        {
            base.onAwake();
            _ui = this.findInstance<Main>();
            _ui.game = this;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }
        [CardDefineID]
        [SerializeField]
        int[] _deck = new int[31];
        public int[] deck
        {
            get { return _deck; }
            set { _deck = value; }
        }
        public CardManager cards
        {
            get { return getManager<CardManager>(); }
        }
        private void Start()
        {
#if !UNITY_EDITOR
            tryLoadDeckFromPrefs();
#endif
        }
        public void startGame()
        {
            game = new THHGame(_option, getManager<CardManager>().GetCardDefines())
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                time = new GameObject(nameof(TimeManager)).AddComponent<TimeManager>(),
                logger = new UnityLogger()
            };
            (game.answers as AnswerManager).game = game;
            THHPlayer localPlayer = game.createPlayer(1, "本地玩家", game.getCardDefine(_deck[0]) as MasterCardDefine,
                _deck.Skip(1).Select(id => game.getCardDefine(id)));
            THHPlayer aiPlayer = game.createPlayer(2, "AI", game.getCardDefine(_deck[0]) as MasterCardDefine,
                _deck.Skip(1).Select(id => game.getCardDefine(id)));
            //本地玩家用UI
            _ui.display(_ui.Game);
            _ui.Game.Table.setGame(game, localPlayer);
            //AI玩家用AI
            new AI(game, aiPlayer);
            game.triggers.onEventAfter += onEventAfter;
            gameTask = game.run();
        }

        private void onEventAfter(TouhouCardEngine.Interfaces.IEventArg obj)
        {
            if (obj is THHGame.GameEndEventArg)
            {
                game.Dispose();
                _ui.display(_ui.MainMenu);
            }
        }

        void tryLoadDeckFromPrefs()
        {
            if (!PlayerPrefs.HasKey("DeckCount"))
                return;
            int count = PlayerPrefs.GetInt("DeckCount");
            for (int i = 1; i < count; i++)
            {
                _deck[i] = PlayerPrefs.GetInt("Deck" + i);
            }
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Debug.LogError(e);
        }
    }
}
