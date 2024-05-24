using System.Collections.Generic;
using System.Linq;
using BJSYGameCore;
using Game.BattleTable;
using UnityEngine;

namespace Game.Animations
{
    public class BuiltinEffect : MonoBehaviour
    {
        public UnityAnimationDefine toAnimationDefine()
        {
            var anim = new UnityAnimationDefine(0, 0, defineName);
            anim.duration = duration;
            anim.blockTime = blockTime;
            anim.isPersistent = isPersistent;
            anim.colorTargets = getPossibleColorTargets();
            var comp = GetComponent<TableEffect>();
            anim.prefab = comp;
            switch (comp)
            {
                case AreaEffect:
                    anim.type = AnimationType.Area;
                    break;
                case CardEffect:
                    anim.type = AnimationType.Card;
                    break;
                case VectorEffect:
                    anim.type = AnimationType.Vector;
                    break;
                case ProjectileEffect:
                    anim.type = AnimationType.Projectile;
                    break;
            }
            return anim;
        }
        public ColorTargetGroup[] getPossibleColorTargets()
        {
            var meshes = GetComponentsInChildren<MeshRenderer>(true);
            var lights = GetComponentsInChildren<Light>(true);
            var particles = GetComponentsInChildren<ParticleSystem>(true);

            List<(ColorTarget target, Color color)> targets = new List<(ColorTarget target, Color color)>();
            foreach (var mesh in meshes)
            {
                var meshColorController = mesh.GetComponent<MeshRendererController>();
                var meshColor = meshColorController ? meshColorController.color : Color.white;
                targets.Add((ColorTarget.FromMesh(transform.getChildPath(mesh.transform)), meshColor));
            }
            foreach (var light in lights)
            {
                targets.Add((ColorTarget.FromLight(transform.getChildPath(light.transform)), light.color));
            }
            foreach (var particle in particles)
            {
                targets.AddRange(getParticleColorTargets(particle));
            }

            // 按照颜色的相似度进行分组。
            var groups = targets.GroupBy(t => t.color, t => t.target).Select(t => new ColorTargetGroup(t.ToArray(), t.Key)).ToArray();

            return groups;
        }
        public IEnumerable<(ColorTarget target, Color color)> getParticleColorTargets(ParticleSystem particle)
        {
            var main = particle.main;
            var colorOverLifetime = particle.colorOverLifetime;
            var colorbySpeed = particle.colorBySpeed;

            var path = transform.getChildPath(particle.transform);
            foreach (var target in GetMinMaxGradientColorTargets(path, ParticleColorTarget.StartColor, main.startColor))
            {
                yield return target;
            }
            if (colorOverLifetime.enabled)
            {
                foreach (var target in GetMinMaxGradientColorTargets(path, ParticleColorTarget.ColorOverLifetime, colorOverLifetime.color))
                {
                    yield return target;
                }
            }
            if (colorbySpeed.enabled)
            {
                foreach (var target in GetMinMaxGradientColorTargets(path, ParticleColorTarget.ColorBySpeed, colorbySpeed.color))
                {
                    yield return target;
                }
            }
        }
        public IEnumerable<(ColorTarget target, Color color)> GetMinMaxGradientColorTargets(string path, ParticleColorTarget target, ParticleSystem.MinMaxGradient gradient)
        {
            switch (gradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    yield return (ColorTarget.FromParticle(path, target, 0), gradient.color);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    yield return (ColorTarget.FromParticle(path, target, 0), gradient.colorMin);
                    yield return (ColorTarget.FromParticle(path, target, 1), gradient.colorMax);
                    break;
                case ParticleSystemGradientMode.Gradient:
                case ParticleSystemGradientMode.RandomColor:
                    for (int i = 0; i < gradient.gradient.colorKeys.Length; i++)
                    {
                        var colorKey = gradient.gradient.colorKeys[i];
                        yield return (ColorTarget.FromParticle(path, target, i), colorKey.color);
                    }
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    for (int i = 0; i < gradient.gradientMin.colorKeys.Length; i++)
                    {
                        var colorKey = gradient.gradientMin.colorKeys[i];
                        yield return (ColorTarget.FromParticle(path, target, i), colorKey.color);
                    }
                    for (int i = 0; i < gradient.gradientMax.colorKeys.Length; i++)
                    {
                        var colorKey = gradient.gradientMax.colorKeys[i];
                        yield return (ColorTarget.FromParticle(path, target, i + gradient.gradientMin.colorKeys.Length), colorKey.color);
                    }
                    break;
            }
        }
        public string defineName;
        public float duration;
        public float blockTime;
        public bool isPersistent;
    }
}
