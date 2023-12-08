using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class MasterItemAttackBuff : Buff, IInnateBuff
    {
        public MasterItemAttackBuff() : base()
        {
            _modifiers.Add(new AttackModifier(0) { relatedPropName = buffPropName });
            canClone = false;
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
