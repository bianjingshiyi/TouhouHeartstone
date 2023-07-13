using System.Collections.Generic;

namespace Game.UndoRedo
{
    public class OperationRecordList
    {
        public void addBreakpoint()
        {
            _breakpoints.Add(index);
        }
        public void addOperation(OperationRecord record)
        {
            _breakpoints.RemoveWhere(i => i > index);
            _records.RemoveRange(index, _records.Count - index);
            _records.Add(record);
            index = _records.Count;
        }
        public void undo()
        {
            if (!canUndo())
                return;

            while (index > 0)
            {
                index--;
                var record = _records[index];
                record.undo();
                if (_breakpoints.Contains(index))
                {
                    break;
                }
            }
        }
        public void redo()
        {
            if (!canRedo())
                return;

            while (index < _records.Count)
            {
                var record = _records[index];
                record.redo();
                index++;
                if (_breakpoints.Contains(index))
                {
                    break;
                }
            }
        }
        public bool canUndo()
        {
            return index > 0;
        }
        public bool canRedo()
        {
            return index < _records.Count;
        }
        public void clear()
        {
            _records.Clear();
            index = 0;
        }
        private int index = 0;
        private HashSet<int> _breakpoints = new HashSet<int>();
        private List<OperationRecord> _records = new List<OperationRecord>();
    }
    public abstract class OperationRecord
    {
        public abstract void undo();
        public abstract void redo();
    }
}
