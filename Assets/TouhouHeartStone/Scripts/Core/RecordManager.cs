using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    public class RecordManager : IEnumerable<Record>
    {
        public RecordManager(Game game)
        {
            this.game = game;
        }
        Game game { get; set; }
        internal void addRecord(Record record)
        {
            if (record == null)
                return;
            _recoderList.Add(record);
            onWitness?.Invoke(record.apply(game));
        }
        internal Record this[int index]
        {
            get { return _recoderList[index]; }
        }
        public int count
        {
            get { return _recoderList.Count; }
        }
        IEnumerator<Record> IEnumerable<Record>.GetEnumerator()
        {
            return ((IEnumerable<Record>)_recoderList).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Record>)_recoderList).GetEnumerator();
        }
        List<Record> _recoderList = new List<Record>();
        public event Action<Dictionary<int, Witness>> onWitness;
    }
}