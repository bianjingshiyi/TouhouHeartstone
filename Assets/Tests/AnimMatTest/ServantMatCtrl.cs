using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

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
    [RequireComponent(typeof(Graphic))]
    public abstract class GraphMatPropCtrl : MatPropCtrl
    {
        Graphic _graphic;
        public Graphic graphic
        {
            get
            {
                if (_graphic == null)
                    _graphic = GetComponent<Graphic>();
                return _graphic;
            }
        }
        public override Material material
        {
            get { return graphic.material; }
            set { graphic.material = value; }
        }
    }
    public class ServantMatCtrl : GraphMatPropCtrl
    {
        [SerializeField]
        float _Gray;
        [SerializeField]
        Color _Color;
        protected override void Update()
        {
            material.SetFloat(nameof(_Gray), _Gray);
            material.SetColor(nameof(_Color), _Color);
        }
        private void Reset()
        {
            _Gray = material.GetFloat(nameof(_Gray));
            _Color = material.GetColor(nameof(_Color));
        }
    }
}
