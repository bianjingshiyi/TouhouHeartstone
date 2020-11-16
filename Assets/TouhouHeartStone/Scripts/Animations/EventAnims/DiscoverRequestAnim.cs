using TouhouHeartstone;
using UI;
namespace Game
{
    class DiscoverRequestAnim : RequestAnimation<DiscoverRequest>
    {
        bool _isOpened = false;
        public override bool display(TableManager table, DiscoverRequest request)
        {
            if (!_isOpened)
            {
                _isOpened = true;
                table.displayDiscoverDialog(request.cardIdArray, request.title);
            }
            if (table.ui.Discover.isDisplaying)
                return false;
            return true;
        }
    }
}
