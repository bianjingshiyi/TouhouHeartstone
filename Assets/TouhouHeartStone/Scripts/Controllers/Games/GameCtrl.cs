using System;
using UI;

namespace Controllers
{
    [Serializable]
    public class CardPoolInfo
    {
        public string cardPoolName;
        public string imageResPath;
    }
    public class GameCtrl : IController<MainCtrl>
    {
        public GameCtrl(IAppManager app, MainCtrl main, UI.Game ui)
        {
            this.app = app;
            this.main = main;
            _ui = ui;
            replayPlayerCtrl = new ReplayPlayerCtrl(app, main, _ui.transform.Find("LeftButtonGroup/ReplayPlayerPanel").GetComponent<ReplayPlayerPanel>());
        }
        public ReplayPlayerCtrl replayPlayerCtrl { get; }
        public MainCtrl main { get; }
        public IAppManager app { get; }
        UI.Game _ui;
    }
}
