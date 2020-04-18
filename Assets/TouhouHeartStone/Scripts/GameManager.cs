using System.IO;
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
    class GameManager : Manager
    {
        [SerializeField]
        GameOption _option = new GameOption();
        [SerializeField]
        Table _table = null;
        public THHGame game { get; private set; } = null;
        Task gameTask { get; set; } = null;
        protected override void onAwake()
        {
            base.onAwake();
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }
        [SerializeField]
        string[] _externalCardPaths = new string[0];
        [CardDefineID]
        [SerializeField]
        int _master;
        [CardDefineID]
        [SerializeField]
        int[] _deck;
        private void Start()
        {
            Dictionary<Workbook, string> workbooks = new Dictionary<Workbook, string>();
            foreach (var path in _externalCardPaths)
            {
                if (Directory.Exists(Application.streamingAssetsPath + "/" + path))
                {
                    foreach (var filePath in Directory.GetFiles(Application.streamingAssetsPath + "/" + path, "*.xls", SearchOption.AllDirectories))
                    {
                        using (FileStream stream = new FileStream(filePath, FileMode.Open))
                        {
                            workbooks.Add(Workbook.Load(stream), filePath);
                        }
                    }
                }
                else if (File.Exists(Application.streamingAssetsPath + "/" + path))
                {
                    using (FileStream stream = new FileStream(Application.streamingAssetsPath + "/" + path, FileMode.Open))
                    {
                        workbooks.Add(Workbook.Load(stream), Application.streamingAssetsPath + "/" + path);
                    }
                }
            }
            CardDefine[] cards = CardImporter.GetCardDefines(workbooks, out var skins);
            _table.addSkins(skins);
            game = new THHGame(_option, cards)
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                logger = new UnityLogger()
            };
            (game.answers as AnswerManager).game = game;
            THHPlayer localPlayer = game.createPlayer(1, "本地玩家", game.getCardDefine(_master) as MasterCardDefine,
                _deck.Select(id => game.getCardDefine(id)));
            THHPlayer aiPlayer = game.createPlayer(2, "AI", game.getCardDefine(_master) as MasterCardDefine,
                _deck.Select(id => game.getCardDefine(id)));
            //本地玩家用UI
            _table.setGame(game, localPlayer);
            //AI玩家用AI
            new AI(game, aiPlayer);
            gameTask = game.run();
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Debug.LogError(e);
        }
    }
}
