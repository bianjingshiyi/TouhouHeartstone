using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone.Builtin
{
    public class KonpakuYoumu : MasterCardDefine
    {
        public const int ID = 0x00600000 | CardCategory.MASTER | 0x000;
        public override int id { get; set; } = ID;
        public override int life { get; set; } = 30;
        public override int skillID { get; set; } = StartdustBullet.ID;
        public override IEffect[] effects { get; set; } = new Effect[0];
    }
}
