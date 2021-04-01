using UI;

namespace Controllers
{
    public class ReplayPlayerCtrl : IController<MainCtrl>
    {
        public ReplayPlayerCtrl(IController<MainCtrl> parent, ReplayPlayerPanel ui)
        {
            _parent = parent;
            main = parent.main;
            app = parent.app;
            _ui = ui;
        }
        public MainCtrl main { get; }
        public IController parent => _parent;
        public IAppManager app { get; }
        IController<MainCtrl> _parent;
        ReplayPlayerPanel _ui;
    }
}
