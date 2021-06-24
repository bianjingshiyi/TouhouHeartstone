using UI;
using BJSYGameCore;
using UnityEngine.SceneManagement;
using Game.Replays;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using Game;
using MongoDB.Bson.Serialization;

namespace Controllers
{
    class ReplayPlayerCtrl : IController<MainCtrl>
    {
        public ReplayPlayerCtrl(IAppManager app, MainCtrl main, ReplayPlayerPanel ui)
        {
            this.app = app;
            this.main = main;
            _ui = ui;
        }
        public MainCtrl main { get; }
        public IAppManager app { get; }
        ReplayPlayerPanel _ui;
    }
}
