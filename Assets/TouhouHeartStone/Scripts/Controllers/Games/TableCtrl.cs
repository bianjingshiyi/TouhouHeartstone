using UI;

namespace Controllers
{
    public class TableCtrl : IController<MainCtrl>
    {
        public TableCtrl(IController<MainCtrl> parent, Table ui)
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
        Table _ui;
    }
}
