using MessagePack;
using System;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    public abstract class GeneratedRuleLogic
    {
        public ActionGraph graph { get; protected set; }
        public long cardPoolId;
        public int id;
        public string name;
    }
}
