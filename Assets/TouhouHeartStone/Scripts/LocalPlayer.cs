using UnityEngine;

namespace TouhouHeartstone
{
    public class LocalPlayer : Player
    {
        public static LocalPlayer create(PlayerManager manager, int id)
        {
            LocalPlayer player = new GameObject("LocalPlayer (" + id + ")").AddComponent<LocalPlayer>();
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