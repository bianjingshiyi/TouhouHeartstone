using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;

namespace TouhouHeartstone.Backend
{
    public class WitnessManager : MonoBehaviour, IEnumerable<IWitness>
    {
        public void add(IWitness witness)
        {
            if (witness == null || witness.number < _witnessed.Count)
                return;
            if (witness.number == _witnessed.Count)
            {
                _witnessed.Add(witness);
                Debug.Log("玩家" + game.network.localPlayerId + "收到：" + witness, this);
                onWitnessAdded.Invoke(witness);
                if (_hungup.Count > 0)
                {
                    IWitness next = _hungup.FirstOrDefault(e => { return e.number == _witnessed.Count; });
                    if (next != null)
                    {
                        _hungup.Remove(next);
                        add(next);
                    }
                }
            }
            else
                _hungup.Add(witness);
        }
        public IWitness getWitness(int number)
        {
            return _witnessed[number];
        }
        public int hungupCount
        {
            get { return _hungup.Count; }
        }
        public void getMissingRange(out int min, out int max)
        {
            min = _witnessed.Count;
            max = _hungup.Min(e => { return e.number; }) - 1;
        }
        [SerializeField]
        List<IWitness> _hungup = new List<IWitness>();
        public int count
        {
            get { return _witnessed.Count; }
        }
        public IWitness this[int index]
        {
            get { return _witnessed[index]; }
        }
        public IEnumerator<IWitness> GetEnumerator()
        {
            return ((IEnumerable<IWitness>)_witnessed).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IWitness>)_witnessed).GetEnumerator();
        }
        [SerializeField]
        List<IWitness> _witnessed = new List<IWitness>();
        public WitnessEvent onWitnessAdded
        {
            get { return _onWitnessAdded; }
        }
        [SerializeField]
        WitnessEvent _onWitnessAdded = new WitnessEvent();
        public GameContainer game
        {
            get
            {
                if (_game == null)
                    _game = GetComponentInParent<GameContainer>();
                return _game;
            }
        }
        [SerializeField]
        GameContainer _game;
    }
}