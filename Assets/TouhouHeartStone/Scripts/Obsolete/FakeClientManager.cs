using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace TouhouHeartstone
{
    public class FakeClientManager : NetworkManager
    {
        public override bool isClient
        {
            get { return true; }
        }
        public override int id
        {
            get { return _id; }
        }
        [SerializeField]
        int _id;
        protected override void onReceiveObject(int senderId, object obj)
        {
            if (obj is Witness)
            {
                game.witness.add(obj as Witness);
                if (game.witness.hungupCount > 0)
                {
                    int min, max;
                    game.witness.getMissingRange(out min, out max);
                    sendObject(senderId, new GetMissingWitnessRequest(min, max));
                }
            }
        }
    }
}