using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouhouHeartstone
{
    public interface IFrontend
    {
        [Obsolete("请使用playerIndex替代")]
        int id { get; }
        void sendWitness(EventWitness witness);
    }
}