using UnityEngine;

namespace BJSYGameCore.Animations
{
    [ExecuteInEditMode]
    public abstract class MatPropCtrl : MonoBehaviour
    {
        Material _originMaterial;
        Material _cloneMaterial;
        public abstract Material material { get; set; }
        protected void OnEnable()
        {
            _originMaterial = material;
            _cloneMaterial = Instantiate(_originMaterial);
            material = _cloneMaterial;
        }
        protected abstract void Update();
        protected void OnDisable()
        {
            DestroyImmediate(_cloneMaterial);
            _cloneMaterial = null;
            material = _originMaterial;
            _originMaterial = null;
        }
        [ContextMenu(nameof(showMatPropNames))]
        public void showMatPropNames()
        {
            if (material != null)
            {
                foreach (string name in material.GetTexturePropertyNames())
                {
                    Debug.Log(name);
                }
            }
        }
    }
}
