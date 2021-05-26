using UI;

namespace Controllers
{
    public class ReplayPlayerCtrl : IController<MainCtrl>
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
