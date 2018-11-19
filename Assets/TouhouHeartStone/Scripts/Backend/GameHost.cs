using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace TouhouHeartstone.Backend
{
    public class GameHost : GameContainer
    {
        protected override void onAwake()
        {
            base.onAwake();
            game = new Game((int)DateTime.Now.ToBinary());
            game.records.onWitness += onHostWitness;
        }
        protected override void onStart()
        {
            base.onStart();
            game.start(network.playersId);
        }
        protected override void onInitReplace(int[] cards)
        {
            game.initReplace(network.localPlayerId, cards.Select(e => { return new CardInstance(e, 0); }).ToArray());
        }
        protected override void onUse(int instance, int position, int target)
        {
            game.use(network.localPlayerId, instance, position, target);
        }
        protected override void onTurnEnd()
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
        protected override void onReceiveObject(int senderId, object obj)
        {
            if (obj is GetMissingWitnessRequest)
            {
                GetMissingWitnessRequest request = obj as GetMissingWitnessRequest;
                for (int i = request.min; i <= request.max; i++)
                {
                    network.sendObject(senderId, _dicWitnessed[senderId].Find(e => { return e.number == i; }));
                }
            }
            else if (obj is InitReplaceRequest)
            {
                InitReplaceRequest request = obj as InitReplaceRequest;
                game.initReplace(senderId, request.cards.Select(e => { return new CardInstance(e, 0); }).ToArray());
            }
            else if (obj is UseRequest)
            {
                UseRequest request = obj as UseRequest;
                game.use(senderId, request.instance, request.position, request.target);
            }
            else if (obj is TurnEndRequest)
            {
                TurnEndRequest request = obj as TurnEndRequest;
                game.turnEnd(senderId);
            }
        }
        Dictionary<int, List<Witness>> _dicWitnessed = new Dictionary<int, List<Witness>>();
        Game game { get; set; } = null;
    }
}