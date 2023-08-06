using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.BattleTable
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    public class MeshRendererController : MonoBehaviour
    {
        private void Awake()
        {
            _tmp = GetComponent<TextMeshPro>();
        }
        protected void Update()
        {
            Transform t = transform;
            Color col = color;
            while (t)
            {
                t.GetComponents(_parentComponents);
                foreach (var comp in _parentComponents)
                {
                    if (comp is MeshRendererGroup meshRendererGroup)
                    {
                        col *= meshRendererGroup.color;
                    }
                }
                t = t.parent;
            }
            _parentComponents.Clear();
            if (textMesh)
            {
                textMesh.color = col;
            }
            else
            {
                meshRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_Color", col);
                meshRenderer.SetPropertyBlock(propertyBlock);
            }
        }
        private void OnDestroy()
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
        public TextMeshPro textMesh
        {
            get
            {
                return _tmp;
            }
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
        public MaterialPropertyBlock propertyBlock
        {
            get
            {
                if (_propertyBlock == null)
                {
                    _propertyBlock = new MaterialPropertyBlock();
                }
                return _propertyBlock;
            }
        }
        private TextMeshPro _tmp;
        private MeshRenderer _meshRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private List<Component> _parentComponents = new List<Component>();
    }
}
