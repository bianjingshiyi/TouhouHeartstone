using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouhouHeartstone.Backend.Builtin
{
    public class BudFairy : ServantCardDefine
    {
        public override int id
        {
            get { return 1; }
        }
        public override int cost
        {
            get { return 0; }
        }
        public override int attack
        {
            get { return 1; }
        }
        public override int life
        {
            get { return 1; }
        }
        public override int category
        {
            get { return 2; }
        }
    }
    public class Reimu : MasterCardDefine
    {
        public override int id
        {
            get { return 1000; }
        }
        public override int category
        {
            get { return 1000; }
        }
    }
    public class Marisa : MasterCardDefine
    {
        public override int id
        {
            get { return 2000; }
        }
        public override int category
        {
            get { return 2000; }
        }
    }
}
