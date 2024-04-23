using System.Collections.Generic;
using System.Linq;
using BJSYGameCore;
using Game.BattleTable;
using UnityEngine;
using TargetColorPair = System.Tuple<Game.Animations.ColorTarget, UnityEngine.Color>;

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
            anim.colorTargets = colorTargets;
            var comp = GetComponent<TableEffect>();
            anim.prefab = comp;
            switch (comp)
            {
                case AreaEffect:
                    anim.type = AnimationType.Area;
                    break;
                case CameraEffect:
                    anim.type = AnimationType.Camera;
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
        public ColorTargetGroup[] getPossibleColorTargets(float aberration = 0)
        {
            var meshes = GetComponentsInChildren<MeshRenderer>(true);
            var lights = GetComponentsInChildren<Light>(true);
            var particles = GetComponentsInChildren<ParticleSystem>(true);

            List<TargetColorPair> targets = new List<TargetColorPair>();
            foreach (var mesh in meshes)
            {
                var meshColorController = mesh.GetComponent<MeshRendererController>();
                var animator = meshColorController.GetComponent<MeshRendererAnimator>();
                if (animator)
                {
                    for (int i = 0; i < animator.gradient.colorKeys.Length; i++)
                    {
                        var colorKey = animator.gradient.colorKeys[i];
                        targets.Add(new TargetColorPair(ColorTarget.FromMeshAnimator(transform.getChildPath(mesh.transform), i), colorKey.color));
                    }
                }
                else
                {
                    Color meshColor = meshColorController ? meshColorController.color : Color.white;
                    targets.Add(new TargetColorPair(ColorTarget.FromMesh(transform.getChildPath(mesh.transform)), meshColor));
                }
            }
            foreach (var light in lights)
            {
                targets.Add(new TargetColorPair(ColorTarget.FromLight(transform.getChildPath(light.transform)), light.color));
            }
            foreach (var particle in particles)
            {
                targets.AddRange(getParticleColorTargets(particle));
            }

            // 按照颜色的相似度进行分组。
            var resultGroups = new List<ColorTargetGroup>();

            var groupingTargets = new List<TargetColorPair>(targets);
            while (groupingTargets.Count > 0)
            {
                var colorGroups = new Dictionary<int, List<int>>();
                for (int i1 = 0; i1 < groupingTargets.Count; i1++)
                {
                    var pair1 = groupingTargets[i1];
                    var group = new List<int>();
                    colorGroups.Add(i1, group);
                    for (int i2 = 0; i2 < groupingTargets.Count; i2++)
                    {
                        if (i1 == i2)
                            continue;
                        var pair2 = groupingTargets[i2];
                        if (pair1.Item2.getColorDifferenceRGB(pair2.Item2) <= aberration)
                        {
                            group.Add(i2);
                        }
                    }
                }
                // 找到相似色彩最多的那个颜色。
                var mostColorTargetPair = colorGroups.OrderByDescending(g => g.Value.Count).FirstOrDefault();
                var mostColorTarget = groupingTargets[mostColorTargetPair.Key];
                var mostColor = mostColorTarget.Item2;
                mostColor.a = 1f;
                var mostColorTargetGroupedTargets = mostColorTargetPair.Value.Select(i => groupingTargets[i].Item1).Prepend(mostColorTarget.Item1).ToArray();
                resultGroups.Add(new ColorTargetGroup()
                {
                    color = mostColor,
                    targets = mostColorTargetGroupedTargets
                });
                // 去掉被包括的这些颜色。
                groupingTargets.RemoveAll(pair => mostColorTargetGroupedTargets.Contains(pair.Item1));
            }
            return resultGroups.ToArray();
        }
        public IEnumerable<TargetColorPair> getParticleColorTargets(ParticleSystem particle)
        {
            var main = particle.main;
            var colorOverLifetime = particle.colorOverLifetime;
            var colorbySpeed = particle.colorBySpeed;
            var trails = particle.trails;

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
            if (trails.enabled)
            {
                foreach (var target in GetMinMaxGradientColorTargets(path, ParticleColorTarget.TrailColorOverLifetime, trails.colorOverLifetime))
                {
                    yield return target;
                }
                foreach (var target in GetMinMaxGradientColorTargets(path, ParticleColorTarget.ColorOverTrail, trails.colorOverTrail))
                {
                    yield return target;
                }
            }
        }
        public IEnumerable<TargetColorPair> GetMinMaxGradientColorTargets(string path, ParticleColorTarget target, ParticleSystem.MinMaxGradient gradient)
        {
            switch (gradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    yield return new TargetColorPair(ColorTarget.FromParticle(path, target, 0), gradient.color);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    yield return new TargetColorPair(ColorTarget.FromParticle(path, target, 0), gradient.colorMin);
                    yield return new TargetColorPair(ColorTarget.FromParticle(path, target, 1), gradient.colorMax);
                    break;
                case ParticleSystemGradientMode.Gradient:
                case ParticleSystemGradientMode.RandomColor:
                    for (int i = 0; i < gradient.gradient.colorKeys.Length; i++)
                    {
                        var colorKey = gradient.gradient.colorKeys[i];
                        yield return new TargetColorPair(ColorTarget.FromParticle(path, target, i), colorKey.color);
                    }
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    for (int i = 0; i < gradient.gradientMin.colorKeys.Length; i++)
                    {
                        var colorKey = gradient.gradientMin.colorKeys[i];
                        yield return new TargetColorPair(ColorTarget.FromParticle(path, target, i), colorKey.color);
                    }
                    for (int i = 0; i < gradient.gradientMax.colorKeys.Length; i++)
                    {
                        var colorKey = gradient.gradientMax.colorKeys[i];
                        yield return new TargetColorPair(ColorTarget.FromParticle(path, target, i + gradient.gradientMin.colorKeys.Length), colorKey.color);
                    }
                    break;
            }
        }
        public string defineName;
        public float duration;
        public float blockTime;
        public bool isPersistent;
        public ColorTargetGroup[] colorTargets;
    }
}
