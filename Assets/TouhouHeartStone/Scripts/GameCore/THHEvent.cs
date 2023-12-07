using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public static class THHEvent
    {
        public static Task<EventArg> changeEventVar(this IEventArg eventArg, CardEngine game, string varName, object value)
        {
            return SetEventVarEventDefine.doEvent(game, eventArg, varName, value);
        }
    }
}
