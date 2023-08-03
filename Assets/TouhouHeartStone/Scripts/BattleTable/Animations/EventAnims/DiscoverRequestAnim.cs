using TouhouHeartstone;
namespace Game.BattleTable
{
    class DiscoverRequestAnim : RequestAnimation<DiscoverRequest>
    {
        bool _isOpened = false;
        public override bool display(BattleTableManager table, DiscoverRequest request)
        {
            if (!_isOpened)
            {
                _isOpened = true;
                if (request.cardRefArray != null)
                    table.displayDiscoverDialog(request.cardRefArray, request.title);
                else
                    table.displayDiscoverDialog(request.cardIdArray, request.title);
            }
            if (table.table.Discover.isShowing)
                return false;
            return true;
        }
    }
}
