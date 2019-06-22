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
        CardImageResources images = null;

        [SerializeField]
        CardTextResources texts = null;

        public CardImageResource GetCardImageResource(int id) { return GetCardImageResource(id.ToString()); }
        public CardTextResource GetCardTextResource(int id) { return GetCardTextResource(id.ToString()); }

        public CardImageResource GetCardImageResource(string id)
        {
            return images.Get(id, "zh-CN");
        }

        public CardTextResource GetCardTextResource(string id)
        {
            return texts.Get(id, "zh-CN");
        }
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
