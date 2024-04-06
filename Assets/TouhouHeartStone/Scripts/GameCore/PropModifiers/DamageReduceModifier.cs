using MongoDB.Bson.Serialization.Attributes;
using System;
using TouhouCardEngine;
namespace TouhouHeartstone
{
    public class DamageReduceModifier : IntPropModifier
    {
        public DamageReduceModifier(int value) : base(DEFINE.DAMAGE_REDUCE, value)
        {
        }
        public DamageReduceModifier(int value, bool isSet) : base(DEFINE.DAMAGE_REDUCE, value, isSet)
        {
        }
    }
}