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
        public GameCtrl(IController<MainCtrl> parent, UI.Game ui)
        {
            _parent = parent;
            main = parent.main;
            app = parent.app;
            _ui = ui;
            replayPlayerCtrl = new ReplayPlayerCtrl(this, _ui.transform.Find("LeftButtonGroup/ReplayPlayerPanel").GetComponent<ReplayPlayerPanel>());
        }
        public ReplayPlayerCtrl replayPlayerCtrl { get; }
        public MainCtrl main { get; }
        public IController parent => _parent;
        public IAppManager app { get; }
        IController<MainCtrl> _parent;
        UI.Game _ui;
    }
}
