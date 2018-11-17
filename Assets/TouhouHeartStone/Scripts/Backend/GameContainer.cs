using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using TouhouHeartstone.Frontend.Manager;

namespace TouhouHeartstone.Backend
{
    public class GameContainer : MonoBehaviour
    {
        protected void Awake()
        {
            network.onReceiveObject += onReceiveObject;
            if (!network.isClient)
            {
                game = new Game((int)DateTime.Now.ToBinary());
                game.records.onWitness += onHostWitness;
            }
        }
        protected void Start()
        {
            if (frontendEvents != null)
            {
                frontendEvents.ReplaceInitDrawAction += onInitReplace;
                frontendEvents.EndRoundEventAction += onTurnEnd;
            }
            if (!network.isClient)
            {
                game.start(network.playersId);
            }
        }
        private void onInitReplace(int[] cards)
        {
            game.initReplace(network.localPlayerId, cards.Select(e => { return new CardInstance(e, 0); }).ToArray());
        }
        private void onUse(int instance, int position, int target)
        {
            game.use(network.localPlayerId, instance, position, target);
        }
        private void onTurnEnd()
        {
            game.turnEnd(network.localPlayerId);
        }
        private void onHostWitness(Dictionary<int, Witness> dicWitness)
        {
            if (dicWitness == null)
                return;
            //添加给自己
            Witness witness = dicWitness[network.localPlayerId];
            witness.number = this.witness.count;
            this.witness.add(witness);
            //发送给其他玩家
            for (int i = 0; i < network.playersId.Length; i++)
            {
                if (network.playersId[i] != network.localPlayerId)
                {
                    int playerId = network.playersId[i];
                    if (!_dicWitnessed.ContainsKey(playerId))
                        _dicWitnessed.Add(playerId, new List<Witness>());
                    witness = dicWitness[playerId];
                    witness.number = _dicWitnessed[playerId].Count;
                    _dicWitnessed[playerId].Add(witness);
                    network.sendObject(playerId, witness);
                }
            }
        }
        private void onReceiveObject(int senderId, object obj)
        {
            if (network.isClient)
            {
                if (obj is Witness)
                {
                    witness.add(obj as Witness);
                    if (witness.hungupCount > 0)
                    {
                        int min, max;
                        witness.getMissingRange(out min, out max);
                        network.sendObject(senderId, new GetMissingWitnessRequest(min, max));
                    }
                }
            }
            else
            {
                if (obj is GetMissingWitnessRequest)
                {
                    GetMissingWitnessRequest request = obj as GetMissingWitnessRequest;
                    for (int i = request.min; i <= request.max; i++)
                    {
                        network.sendObject(senderId, _dicWitnessed[senderId].Find(e => { return e.number == i; }));
                    }
                }
            }
        }
        Dictionary<int, List<Witness>> _dicWitnessed = new Dictionary<int, List<Witness>>();
        FrontendWitnessEventDispatcher frontendEvents
        {
            get
            {
                if (_frontendEvents == null)
                {
                    foreach (GameObject obj in gameObject.scene.GetRootGameObjects())
                    {
                        _frontendEvents = obj.GetComponentInChildren<FrontendWitnessEventDispatcher>();
                        if (_frontendEvents != null)
                            break;
                    }
                }
                return _frontendEvents;
            }
        }
        [SerializeField]
        FrontendWitnessEventDispatcher _frontendEvents;
        public NetworkManager network
        {
            get
            {
                if (_network == null)
                    _network = GetComponentInChildren<NetworkManager>();
                return _network;
            }
        }
        [SerializeField]
        NetworkManager _network;
        public WitnessManager witness
        {
            get
            {
                if (_witness == null)
                    _witness = GetComponentInChildren<WitnessManager>();
                return _witness;
            }
        }
        [SerializeField]
        WitnessManager _witness;
        Game game { get; set; } = null;
    }
}