using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TouhouCardEngine;

namespace TouhouHeartstone
{
    public interface IFrontend
    {
        void sendWitness(EventWitness witness);
    }
}