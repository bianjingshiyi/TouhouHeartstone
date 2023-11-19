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
            _effects = Array.Empty<IEffect>();
        }

        public override PropModifier[] getModifiers(CardEngine game)
        {
            return _modifiers;
        }

        public override IEffect[] getEffects(CardEngine game)
        {
            return _effects;
        }

        public override Buff clone()
        {
            return new MasterItemAttackBuff(this);
        }
        private MasterItemAttackBuff(MasterItemAttackBuff other) : base(other)
        {
            _effects = other._effects.ToArray();
        }
        public override int id => 0;
        public const string buffPropName = "itemAttack";
        private readonly PropModifier[] _modifiers = new PropModifier[]
        {
            new AttackModifier(0) { relatedPropName = buffPropName }
        };
        private IEffect[] _effects;
    }
}
