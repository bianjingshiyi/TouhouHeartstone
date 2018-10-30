using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace TouhouHeartstone
{
    public class FakeHostManager : NetworkManager
    {
        public override bool isClient
        {
            get { return false; }
        }
        public override int id
        {
            get { return _id; }
        }
        [SerializeField]
        int _id;
        protected void Start()
        {
            game.game.records.onWitness += onWitness;
        }
        void onWitness(Dictionary<int, Witness> dicWitness)
        {
            if (dicWitness == null)
                return;
            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i] != this)
                {
                    int playerId = connections[i].id;
                    if (!_dicWitnessed.ContainsKey(playerId))
                        _dicWitnessed.Add(playerId, new List<Witness>());
                    Witness witness = dicWitness[playerId];
                    witness.number = _dicWitnessed[playerId].Count;
                    _dicWitnessed[playerId].Add(witness);
                    sendObject(playerId, witness);
                }
            }
        }
        Dictionary<int, List<Witness>> _dicWitnessed = new Dictionary<int, List<Witness>>();
        protected override void onReceiveObject(int senderId, object obj)
        {
            if (obj is GetMissingWitnessRequest)
            {
                GetMissingWitnessRequest request = obj as GetMissingWitnessRequest;
                for (int i = request.min; i <= request.max; i++)
                {
                    sendObject(senderId, _dicWitnessed[senderId].Find(e => { return e.number == i; }));
                }
            }
        }
    }
}