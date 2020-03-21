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

namespace Game
{
    class GameManager : Manager
    {
        public THHGame game { get; private set; } = null;
        [SerializeField]
        GameOption _option = new GameOption();
        protected override void onAwake()
        {
            base.onAwake();
            game = new THHGame(_option,
            new Reimu(), new TotematicCall(),
            new Marisa(), new StartdustBullet())
            {
                answers = new GameObject(nameof(AnswerManager)).AddComponent<AnswerManager>(),
                triggers = new GameObject(nameof(TriggerManager)).AddComponent<TriggerManager>(),
                logger = new UnityLogger()
            };
            (game.answers as AnswerManager).game = game;
            game.createPlayer(1, "玩家1", game.getCardDefine<Reimu>(), Enumerable.Repeat<CardDefine>(null, 0));
            game.createPlayer(2, "玩家2", game.getCardDefine<Reimu>(), Enumerable.Repeat<CardDefine>(null, 0));
            _ = game.init();
        }
    }
}
