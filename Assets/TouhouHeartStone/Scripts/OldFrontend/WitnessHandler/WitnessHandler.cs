using System;
using System.Runtime.InteropServices;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.OldFrontend.WitnessHandler
{
    [Obsolete]
    public class WitnessHandler
    {
        public bool Exec(IWitness witness, OldFrontend.Manager.FrontendManager frontend)
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

            DebugUtils.Warning($"Witness {type} 没有对应的处理类.");
            return false;
        }
    }

    [Obsolete]
    public abstract class WitnessHandlerBase
    {
        protected OldFrontend.Manager.FrontendManager frontend;
        public abstract void Exec(IWitness witness);

        public abstract bool HasAnimation { get; }

        public void SetFrontendManager(OldFrontend.Manager.FrontendManager frontendManager)
        {
            frontend = frontendManager;
        }
    }

    [Obsolete]
    public abstract class WitnessHandlerBase<T> : WitnessHandlerBase where T : IWitness
    {
        public override void Exec(IWitness witness)
        {
            Exec((T)witness);
        }

        public abstract void Exec(T witness);
    }
}
