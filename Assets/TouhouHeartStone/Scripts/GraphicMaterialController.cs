using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
namespace Game
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Graphic))]
    public class GraphicMaterialController : MonoBehaviour
    {
        Graphic _graphic = null;
        Graphic graphic
        {
            get
            {
                if (_graphic == null)
                    _graphic = GetComponent<Graphic>();
                return _graphic;
            }
        }
        [Serializable]
        struct MatPropData
        {
            public string name;
            public float floatValue;
        }
        [SerializeField]
        List<MatPropData> _matPropList = new List<MatPropData>();
        protected void Update()
        {
            foreach (var prop in _matPropList)
            {
                graphic.material.SetFloat(prop.name, prop.floatValue);
            }
        }
    }
}
