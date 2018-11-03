using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;

namespace TouhouHeartstone.Backend
{
    public class WitnessManager : MonoBehaviour, IEnumerable<Witness>
    {
        public void add(Witness witness)
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
                    Witness next = _hungup.FirstOrDefault(e => { return e.number == _witnessed.Count; });
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
        public Witness getWitness(int number)
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
        List<Witness> _hungup = new List<Witness>();
        public int count
        {
            get { return _witnessed.Count; }
        }
        public Witness this[int index]
        {
            get { return _witnessed[index]; }
        }
        public IEnumerator<Witness> GetEnumerator()
        {
            return ((IEnumerable<Witness>)_witnessed).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Witness>)_witnessed).GetEnumerator();
        }
        [SerializeField]
        List<Witness> _witnessed = new List<Witness>();
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