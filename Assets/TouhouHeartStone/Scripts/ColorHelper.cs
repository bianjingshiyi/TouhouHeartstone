using Game.Animations;
using UnityEngine;

namespace Game
{
    public static class ColorHelper
    {
        public static Color modifyHSV(this Color color, float hue, float saturation, float value, float alpha)
        {
            Color.RGBToHSV(color, out var h, out var s, out var v);
            h += hue / 360;
            h = h < 0 ? 1 + h : h;
            s = Mathf.Clamp01(s + saturation / 100);
            v = Mathf.Clamp01(v + value / 100);
            color = Color.HSVToRGB(h, s, v);
            color.a = Mathf.Clamp01(color.a + alpha);
            return color;
        }
    }
}
