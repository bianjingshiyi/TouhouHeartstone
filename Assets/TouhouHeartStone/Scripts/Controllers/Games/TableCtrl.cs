using UI;

namespace Controllers
{
    public class TableCtrl : IController<MainCtrl>
    {
        public TableCtrl(IAppManager app, MainCtrl main, Table ui)
        {
            this.app = app;
            this.main = main;
            _ui = ui;
        }
        public MainCtrl main { get; }
        public IAppManager app { get; }
        Table _ui;
    }
}
