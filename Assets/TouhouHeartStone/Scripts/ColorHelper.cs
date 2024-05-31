using Game.Animations;
using UnityEngine;

namespace Game
{
    public static class ColorHelper
    {
        public static float getColorDifferenceRGB(this Color color1, Color color2)
        {
            var colorDiff = color1 - color2;
            var sqrR = Mathf.Pow(colorDiff.r, 2);
            var sqrG = Mathf.Pow(colorDiff.g, 2);
            var sqrB = Mathf.Pow(colorDiff.b, 2);
            var diffFloat = Mathf.Sqrt(sqrR + sqrG + sqrB);
            return diffFloat * 255 / 3;
        }
        public static float getColorDifferenceRGBA(this Color color1, Color color2)
        {
            var colorDiff = color1 - color2;
            var sqrR = Mathf.Pow(colorDiff.r, 2);
            var sqrG = Mathf.Pow(colorDiff.g, 2);
            var sqrB = Mathf.Pow(colorDiff.b, 2);
            var sqrA = Mathf.Pow(colorDiff.a, 2);
            var diffFloat = Mathf.Sqrt(sqrR + sqrG + sqrB + sqrA);
            return diffFloat * 255 / 4;
        }
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
