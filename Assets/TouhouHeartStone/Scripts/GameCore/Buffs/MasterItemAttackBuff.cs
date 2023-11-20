using System;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class MasterItemAttackBuff : Buff, IInnateBuff
    {
        public MasterItemAttackBuff() : base()
        {
            _modifiers.Add(new AttackModifier(0) { relatedPropName = buffPropName });
        }

        public override Buff clone()
        {
            return new MasterItemAttackBuff(this);
        }
        private MasterItemAttackBuff(MasterItemAttackBuff other) : base(other)
        {
        }
        public override int id => 0;
        public const string buffPropName = "itemAttack";
    }
}
