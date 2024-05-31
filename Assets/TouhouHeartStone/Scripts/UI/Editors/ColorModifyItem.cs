using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ColorModifyItem : MonoBehaviour
    {
        public void update(ColorModifyViewData viewData)
        {
            _image.color = viewData.color;
        }
        private void Awake()
        {
            _button.onClick.AddListener(() => onClick?.Invoke(this));
        }
        public event Action<ColorModifyItem> onClick;
        [SerializeField]
        private Button _button;
        [SerializeField]
        private Image _image;
    }
    public struct ColorModifyViewData
    {
        public Color color;
    }
}
