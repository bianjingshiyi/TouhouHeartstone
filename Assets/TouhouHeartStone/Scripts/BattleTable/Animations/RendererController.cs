using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.BattleTable
{
    [ExecuteInEditMode]
    public class RendererController : MonoBehaviour
    {
        private void Awake()
        {
            _tmp = GetComponent<TextMeshPro>();
        }
        private void OnWillRenderObject()
        {
            var col = color;
            foreach (var group in _parentGroups)
            {
                col = col * group.color;
            }
            if (textMesh)
            {
                textMesh.color = col;
            }
            else
            {
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(_colorName, col);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }
        private void OnTransformParentChanged()
        {
            updateParents();
        }
        private void OnEnable()
        {
            updateParents();
        }
        private void OnDestroy()
        {
            _propertyBlock = null;
        }
        private void updateParents()
        {
            _parentGroups.Clear();
            transform.GetComponentsInParent(false, _parentGroups);
        }
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
        public new Renderer renderer
        {
            get
            {
                if (!_renderer)
                {
                    _renderer = GetComponent<Renderer>();
                }
                return _renderer;
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
        [SerializeField]
        private string _colorName = "_Color";
        private TextMeshPro _tmp;
        [SerializeField]
        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;
        private List<RendererGroup> _parentGroups = new List<RendererGroup>();
    }
}
