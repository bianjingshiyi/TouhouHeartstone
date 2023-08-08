using UnityEngine;

namespace Game.BattleTable
{
    public class MeshRendererGroup : MonoBehaviour
    {
        [SerializeField]
        Color _color = Color.white;

        public Color color
        {
            get => _color;
            set => _color = value;
        }
    }
}
