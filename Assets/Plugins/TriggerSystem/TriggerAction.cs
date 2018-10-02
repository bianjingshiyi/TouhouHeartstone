using System.Collections;

namespace TriggerSystem
{
    public abstract class TriggerAction
    {
        public abstract void execute(ReflectedTriggerScope scope);
    }
}