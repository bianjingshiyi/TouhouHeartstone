using TouhouHeartstone;
using UI;
namespace Game
{
    class DiscoverRequestAnim : RequestAnimation<DiscoverRequest>
    {
        public override void display(TableManager table, DiscoverRequest request)
        {
            table.displayDiscoverDialog(request.cardIdArray, request.title);
        }
    }
}
