using UI;
using BJSYGameCore;

namespace Game
{
    class WaitAnim : TableAnimation
    {
        Timer _timer;
        public WaitAnim(float time)
        {
            _timer = new Timer() { duration = time };
        }
        public override bool update(TableManager table, float time)
        {
            if (!_timer.isStarted)
                _timer.start();
            if (!_timer.isExpired())
                return false;
            return true;
        }
    }
}
