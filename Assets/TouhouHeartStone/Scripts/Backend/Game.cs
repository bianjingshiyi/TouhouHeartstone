using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouhouHeartstone.Backend
{
    [Serializable]
    class Game
    {
        public Game(int randomSeed)
        {
            random = new Random(randomSeed);
        }
        Random random
        {
            get { return _random; }
            set { _random = value; }
        }
        Random _random;
        public void start()
        {

        }
    }
}