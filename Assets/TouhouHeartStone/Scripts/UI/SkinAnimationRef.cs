using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Animations;
using TouhouCardEngine;

namespace UI
{
    public class SkinAnimationRef : IAnimationRef, ICloneable
    {
        /// <summary>
        /// 构造一个特效引用。
        /// </summary>
        public SkinAnimationRef(string key, DefineReference animRef)
        {
            this.key = key;
            this.animRef = animRef;
        }
        public SkinAnimationRef(SkinAnimationRef other)
        {
            key = other.key;
            animRef = other.animRef;
            sourcePosition = other.sourcePosition;
            targetPosition = other.targetPosition;
            sourcePositionVarName = other.sourcePositionVarName;
            targetPositionVarName = other.targetPositionVarName;
            colorModifies = other.colorModifies?.ToList();
            eventReference = other.eventReference;
        }
        public static string getNodeKey(Node node)
        {
            return node.getProp<string>(NODE_CONST_KEY);
        }
        public static void setNodeKey(Node node, string value)
        {
            node.setProp(NODE_CONST_KEY, value);
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
        public SkinAnimationRef Clone()
        {
            SkinAnimationRef newRef = new SkinAnimationRef(key, animRef?.Clone())
            {
                sourcePosition = sourcePosition,
                targetPosition = targetPosition,
                sourcePositionVarName = sourcePositionVarName,
                targetPositionVarName = targetPositionVarName,
                eventReference = eventReference?.Clone()
            };
            if (colorModifies != null && colorModifies.Count > 0)
            {
                foreach (ColorModifyData modify in colorModifies)
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
        public string key;
        public int sourcePosition;
        public int targetPosition;
        public string sourcePositionVarName;
        public string targetPositionVarName;
        public EventReference eventReference;
        public DefineReference animRef;
        public List<ColorModifyData> colorModifies = new List<ColorModifyData>();
        public const string NODE_CONST_KEY = "EDITOR_ANIMATION_REF";
        DefineReference IAnimationRef.animRef { get => animRef; set => animRef = value; }
    }
}
