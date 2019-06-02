using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TouhouHeartstone
{
    public interface IFrontend
    {
        void sendWitness(EventWitness witness);
    }
}