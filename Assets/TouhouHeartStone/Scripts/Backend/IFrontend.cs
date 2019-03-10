using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouhouHeartstone
{
    public interface IFrontend
    {
        int id { get; }
        void sendWitness(EventWitness witness);
    }
}