using UnityEngine;

namespace TouhouHeartstone
{
    public class FakeHostManager : NetworkManager
    {
        public override bool isClient
        {
            get { return false; }
        }
        public void sendObject()
        {

        }
        float lag
        {
            get { return _lag; }
        }
        [SerializeField]
        float _lag;
    }
}