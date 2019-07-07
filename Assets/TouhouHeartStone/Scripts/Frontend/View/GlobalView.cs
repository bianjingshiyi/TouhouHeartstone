using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace TouhouHeartstone.Frontend.View
{
    public class GlobalView : MonoBehaviour
    {
        [SerializeField]
        public CardPositionCalculator CardPositionCalculator = null;

        [SerializeField]
        public DamageView damagePrefab = null;

        [SerializeField]
        CardAppearResourcesLoader AppearResLoader = null;

        [System.Obsolete("使用CardAppearResouces替代")]
        [SerializeField]
        CardImageResources images = null;

        [System.Obsolete("使用CardAppearResouces替代")]
        [SerializeField]
        CardTextResources texts = null;

        [System.Obsolete("使用CardAppearResouces替代")]
        public CardImageResource GetCardImageResource(int id) { return GetCardImageResource(id.ToString()); }
        [System.Obsolete("使用CardAppearResouces替代")]
        public CardTextResource GetCardTextResource(int id) { return GetCardTextResource(id.ToString()); }

        [System.Obsolete("使用CardAppearResouces替代")]
        public CardImageResource GetCardImageResource(string id)
        {
            return images.Get(id, "zh-CN");
        }
        [System.Obsolete("使用CardAppearResouces替代")]
        public CardTextResource GetCardTextResource(string id)
        {
            return texts.Get(id, "zh-CN");
        }

        public CardAppearResouces GetCardAppearence(string id)
        {
            return AppearResLoader.Get(id, "zh-Hans");
        }

        public CardAppearResouces GetCardAppearence(int id) { return GetCardAppearence(id.ToString()); }
    }
    public struct PositionWithRotation
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public PositionWithRotation(Vector3 pos, Vector3 rot)
        {
            Position = pos;
            Rotation = rot;
        }
        public PositionWithRotation(Vector3 pos) : this(pos, Vector3.zero) { }
    }
}
