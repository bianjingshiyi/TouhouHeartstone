using System;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
namespace TouhouHeartstone
{
    public class CostModifier : IntPropModifier
    {
        public CostModifier(int value) : base(DEFINE.COST, value)
        {
        }
        public CostModifier(int value, bool isSet) : base(DEFINE.COST, value, isSet)
        {
        }
    }
}