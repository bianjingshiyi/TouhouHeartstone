using System;
using System.Runtime.InteropServices;

namespace TouhouHeartstone.Frontend.WitnessHandler
{
    public class WitnessHandler
    {
        public bool Exec(Witness witness, Frontend.Manager.FrontendManager frontend)
        {
            var type = witness.GetType();
            var types = typeof(WitnessHandler).Assembly.GetTypes();

            foreach (var item in types)
            {
                if (item.IsSubclassOf(typeof(WitnessHandlerBase)))
                {
                    var args = item.BaseType.GetGenericArguments();
                    if (args.Length == 1 && args[0] == type)
                    {
                        var handler = Activator.CreateInstance(item) as WitnessHandlerBase;
                        handler.SetFrontendManager(frontend);
                        handler.Exec(witness);
                        return handler.HasAnimation;
                    }
                }
            }

            DebugUtils.LogWarning($"Witness {type} 没有对应的处理类.");
            return false;
        }
    }

    public abstract class WitnessHandlerBase
    {
        protected Frontend.Manager.FrontendManager frontend;
        public abstract void Exec(Witness witness);

        public abstract bool HasAnimation { get; }

        public void SetFrontendManager(Frontend.Manager.FrontendManager frontendManager)
        {
            frontend = frontendManager;
        }
    }

    public abstract class WitnessHandlerBase<T> : WitnessHandlerBase where T : Witness
    {
        public override void Exec(Witness witness)
        {
            Exec(witness as T);
        }

        public abstract void Exec(T witness);
    }
}
