using UnityEngine;

namespace Game.Animations
{
    public class ColorTarget
    {
        public string path;
        public ColorTargetType type;
        public ParticleColorTarget particleType;
        public int index;
        public override bool Equals(object obj)
        {
            if (obj is ColorTarget other)
            {
                return path == other.path && type == other.type && particleType == other.particleType && index == other.index;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            var hash = path.GetHashCode();
            hash = hash * 31 + type.GetHashCode();
            hash = hash * 31 + particleType.GetHashCode();
            hash = hash * 31 + index.GetHashCode();
            return hash;
        }
        public static bool operator ==(ColorTarget lhs, ColorTarget rhs)
        {
            if (lhs is null)
                return rhs is null;
            return lhs.Equals(rhs);
        }
        public static bool operator !=(ColorTarget lhs, ColorTarget rhs)
        {
            return !(lhs == rhs);
        }
        public static ColorTarget FromMesh(string path)
        {
            return new ColorTarget()
            {
                path = path,
                type = ColorTargetType.MeshRenderer
            };
        }
        public static ColorTarget FromLight(string path)
        {
            return new ColorTarget()
            {
                path = path,
                type = ColorTargetType.Light
            };
        }
        public static ColorTarget FromParticle(string path, ParticleColorTarget particleType, int index)
        {
            return new ColorTarget()
            {
                path = path,
                type = ColorTargetType.ParticleSystem,
                particleType = particleType,
                index = index
            };
        }
    }
    public struct ColorTargetGroup
    {
        public ColorTarget[] targets;
        public Color color;
        public ColorTargetGroup(ColorTarget[] target, Color color)
        {
            this.targets = target;
            this.color = color;
        }
    }
    public class ColorModifyData
    {
        public int targetIndex;
        public float r;
        public float g;
        public float b;
        public float a;
        public Color getColor()
        {
            return new Color(r, g, b, a);
        }
    }
    public enum ColorTargetType
    {
        MeshRenderer,
        Light,
        ParticleSystem
    }
    public enum ParticleColorTarget
    {
        StartColor,
        ColorOverLifetime,
        ColorBySpeed
    }
}
