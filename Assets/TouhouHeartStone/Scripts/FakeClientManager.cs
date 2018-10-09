namespace TouhouHeartstone
{
    public class FakeClientManager : NetworkManager
    {
        public override bool isClient
        {
            get { return true; }
        }
    }
}