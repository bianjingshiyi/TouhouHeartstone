using UnityEngine;
using UnityEngine.UI;

namespace BJSYGameCore.Animations
{
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
}
