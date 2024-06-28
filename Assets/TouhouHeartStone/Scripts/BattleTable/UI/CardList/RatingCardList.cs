using System;

namespace Game.BattleTable
{
    public class RatingCardList : CardUIList<RatingCardItem>
    {
        protected override void addItemEvents(RatingCardItem item)
        {
            base.addItemEvents(item);
            item.onVoteClick += onVoteClickCallback;
        }
        protected override void removeItemEvents(RatingCardItem item)
        {
            base.removeItemEvents(item);
            item.onVoteClick -= onVoteClickCallback;
        }
        private void onVoteClickCallback(RatingCardItem item, bool value)
        {
            onVoteClick?.Invoke(indexOf(item), value);
        }
        public event Action<int, bool> onVoteClick;
    }
}
