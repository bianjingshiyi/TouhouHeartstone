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
                textMesh.ForceMeshUpdate();
            }
            else
            {
                meshRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_Color", col);
                meshRenderer.SetPropertyBlock(propertyBlock);
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
        private List<MeshRendererGroup> _parentGroups = new List<MeshRendererGroup>();
    }
}
