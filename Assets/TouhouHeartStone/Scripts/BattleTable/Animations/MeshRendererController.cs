using UnityEngine;

namespace Game.BattleTable
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    public class MeshRendererController : MonoBehaviour
    {
        public void setColor(string name, Color color)
        {
            meshRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(name, color);
            meshRenderer.SetPropertyBlock(_propertyBlock);
        }
        public void setTexture(string name, Texture texture)
        {
            meshRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetTexture(name, texture);
            meshRenderer.SetPropertyBlock(_propertyBlock);
        }
        protected void Reset()
        {
            enabled = false;
        }
        protected void Update()
        {
            if (_propertyBlock != null)
            {
                meshRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_Color", color);
                meshRenderer.SetPropertyBlock(_propertyBlock);
            }
        }
        private void OnEnable()
        {
            _propertyBlock = new MaterialPropertyBlock();
        }
        private void OnDisable()
        {
            _propertyBlock = null;
        }

        [SerializeField]
        Texture _texture;
        [SerializeField]
        Color _color = Color.white;

        public Color color
        {
            get => _color;
            set => _color = value;
        }
        public MeshRenderer meshRenderer
        {
            get
            {
                if (!_meshRenderer)
                {
                    _meshRenderer = GetComponent<MeshRenderer>();
                }
                return _meshRenderer;
            }
        }
        private MaterialPropertyBlock _propertyBlock;
        private MeshRenderer _meshRenderer;
    }
}
