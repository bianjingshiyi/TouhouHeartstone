using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Scripting;

using TouhouCardEngine;

namespace TouhouHeartstone.Backend.Builtin
{
    public class FairyTwins : ServantCardDefine
    {
        public FairyTwins(IGameEnvironment env)
        {
            effects = new Effect[]
            {
                new GeneratedEffect("Field","onUse",
                "return true",
                "engine.createToken(player, card.define, player[\"Field\"].indexOf(card) + 1);")
            };
        }
        public override int id
        {
            get { return 2; }
        }
        public override int cost
        {
            get { return 1; }
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
        public override Effect[] effects { get; }
    }
}