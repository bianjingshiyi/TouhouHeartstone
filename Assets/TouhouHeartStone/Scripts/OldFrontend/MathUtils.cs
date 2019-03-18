
using UnityEngine;

namespace TouhouHeartstone.OldFrontend
{
    public class MathUtils
    {
        public static Vector2 xzy2xy(Vector3 pos)
        {
            return new Vector2(pos.x, pos.z);
        }

        public static Vector3 xzy2xyz(Vector3 pos)
        {
            return new Vector3(pos.x, pos.z, pos.y);
        }
    }
}
