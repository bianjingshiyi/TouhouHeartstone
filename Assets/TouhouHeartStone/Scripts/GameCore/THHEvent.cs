using System.Threading.Tasks;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public static class THHEvent
    {
        public static Task<SetEventVarEventArg> changeEventVar(this IEventArg eventArg, IGame game, string varName, object value)
        {
            return game.triggers.doEvent(new SetEventVarEventArg(eventArg, varName, value), arg =>
            {
                var argEvent = arg.eventArg;
                var argName = arg.varName;
                var argValue = arg.afterValue;
                argEvent.setVar(argName, argValue);
                return Task.CompletedTask;
            });
        }
    }
}
