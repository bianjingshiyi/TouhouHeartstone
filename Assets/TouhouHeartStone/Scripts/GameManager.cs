using System;
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
        private void Start()
        {
            game = new THHGame(_option,
            new Reimu(), new TotematicCall(),
            new Marisa(), new StartdustBullet(),
            new RashFairy())
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                logger = new UnityLogger()
            };
            (game.answers as AnswerManager).game = game;
            THHPlayer localPlayer = game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat<CardDefine>(game.getCardDefine<RashFairy>(), 30));
            THHPlayer aiPlayer = game.createPlayer(2, "玩家2", game.getCardDefine<Reimu>(), Enumerable.Repeat<CardDefine>(game.getCardDefine<RashFairy>(), 30));
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
