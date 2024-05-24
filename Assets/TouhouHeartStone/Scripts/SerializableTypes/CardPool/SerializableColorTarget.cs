using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Animations;
using MessagePack;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableColorTarget
    {
        public SerializableColorTarget(ColorTarget colorTarget)
        {
            type = (int)colorTarget.type;
            particleType = (int)colorTarget.particleType;
            index = colorTarget.index;
            path = colorTarget.path;
        }
        public ColorTarget deserialize()
        {
            return new ColorTarget()
            {
                type = (ColorTargetType)type,
                particleType = (ParticleColorTarget)particleType,
                index = index,
                path = path
            };
        }
        [Key(0)]
        public string path;
        [Key(1)]
        public int type;
        [Key(2)]
        public int particleType;
        [Key(3)]
        public int index;
    }
}
