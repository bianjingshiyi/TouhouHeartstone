using UnityEngine;

namespace TouhouHeartstone
{
    public class RemotePlayer : Player
    {
        public static RemotePlayer create(PlayerManager manager, int id)
        {
            RemotePlayer player = new GameObject("RemotePlayer (" + id + ")").AddComponent<RemotePlayer>();
            player._id = id;
            player.transform.parent = manager.transform;
            return player;
        }
        public override int id
        {
            get { return _id; }
        }
        [SerializeField]
        int _id;
    }
}