using Game.BattleTable;
using UnityEditor;

namespace TouhouHeartstone
{
    [CustomEditor(typeof(ParticleSystemColor))]
    public class ParticleSystemColorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var comp = target as ParticleSystemColor;

            var hue = comp.hue;
            var sat = comp.saturation;
            var value = comp.value;
            var alpha = comp.alpha;
            comp.hue = EditorGUILayout.Slider("Hue", hue, -180, 180);
            comp.saturation = EditorGUILayout.Slider("Saturation", sat, -100, 100);
            comp.value = EditorGUILayout.Slider("Value", value, -100, 100);
            comp.alpha = EditorGUILayout.Slider("Alpha", alpha, -100, 100);
            if (comp.hue != hue || comp.saturation != sat || comp.value != value || comp.alpha != alpha)
            {
                comp.updateColor();
            }
        }
    }
}
