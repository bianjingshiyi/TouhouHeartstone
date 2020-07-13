using UI;
using System;
namespace Game
{
    class CodeAnim : TableAnimation
    {
        Action _action;
        public CodeAnim(Action action)
        {
            _action = action;
        }
        public override bool update(TableManager table)
        {
            _action();
            return true;
        }
    }
}
