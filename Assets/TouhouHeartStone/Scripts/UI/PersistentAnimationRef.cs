using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Animations;
using TouhouCardEngine;

namespace UI
{
    public class PersistentAnimationRef : IAnimationRef, ICloneable
    {
        public PersistentAnimationRef() : this(new DefineReference(0, 0))
        {
        }
        /// <summary>
        /// 构造一个持续特效引用。
        /// </summary>
        public PersistentAnimationRef(DefineReference animRef)
        {
            this.animRef = animRef;
        }
        public PersistentAnimationRef(PersistentAnimationRef other)
        {
            animRef = other.animRef;
            colorModifies = other.colorModifies?.ToList();
        }
        public ColorModifyData[] getColorModifies()
        {
            return colorModifies.ToArray();
        }

        public ColorModifyData getColorModify(int index)
        {
            return colorModifies[index];
        }

        public void clearColorModify(DefineReference animRef)
        {
            colorModifies.RemoveAll(m => m.animationRef == animRef);
        }
        public void addColorModify(ColorModifyData modify)
        {
            colorModifies.Add(modify);
        }
        public PersistentAnimationRef Clone()
        {
            PersistentAnimationRef newRef = new PersistentAnimationRef();
            newRef.animRef = animRef.Clone();
            if (colorModifies != null && colorModifies.Count > 0)
            {
                foreach (var modify in colorModifies)
                {
                    newRef.colorModifies.Add(modify.Clone());
                }
            }
            return newRef;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        public DefineReference animRef;
        public List<ColorModifyData> colorModifies = new List<ColorModifyData>();
        DefineReference IAnimationRef.animRef { get => animRef; set => animRef = value; }
    }
}
