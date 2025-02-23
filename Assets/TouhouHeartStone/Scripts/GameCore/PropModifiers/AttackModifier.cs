using System;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
namespace TouhouHeartstone
{
    public class AttackModifier : IntPropModifier
    {
        public AttackModifier(int value) : base(DEFINE.ATTACK, value)
        {
        }
        public AttackModifier(int value, bool isSet) : base(DEFINE.ATTACK, value, isSet)
        {
        }
        public override PropModifier Clone()
        {
            return new AttackModifier(defaultValue, isSet);
        }
    }
}