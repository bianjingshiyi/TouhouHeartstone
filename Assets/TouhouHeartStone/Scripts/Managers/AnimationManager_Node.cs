using System;
using System.Collections.Generic;
using System.Linq;
using Game.Animations;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Game
{
    public partial class AnimationManager
    {
        #region 节点的可用特效列表
        public AnimationDefine[] getAnimationDefinesForNode()
        {
            return getAllAnimationDefines(false);
        }
        public string[] getAnimationEditorNamesForNode()
        {
            return getAnimationDefinesForNode().Select(d => main.lang._(d.name)).Prepend(main.lang._(StringTable.NONE)).ToArray();
        }
        public AnimationDefine findAnimationDefineForNode(DefineReference defRef)
        {
            if (defRef == null)
                return null;
            var defines = getAnimationDefinesForNode();
            return defines.FirstOrDefault(d => d.cardPoolId == defRef.cardPoolId && d.id == defRef.defineId);
        }
        public AnimationDefine findAnimationDefineForBuff(DefineReference defRef)
        {
            if (defRef == null)
                return null;
            var defines = getAnimationDefinesForBuff();
            return defines.FirstOrDefault(d => d.cardPoolId == defRef.cardPoolId && d.id == defRef.defineId);
        }
        #endregion

        #region 特效选项
        public VisualEffectDialogViewData getNodeAnimationViewData(Node node, ISkin skin)
        {
            var defines = getAnimationDefinesForNode();
            SkinAnimationRef animRef = node.getOrInitNodeAnimationRef(skin);

            var eventRefs = getLocalNodeVisualEffectEventRefs(node);
            var eventIndex = 0;
            int animIndex = -1;
            int sourcePosition = 0;
            int targetPosition = 0;
            string sourcePositionVarName = string.Empty;
            string targetPositionVarName = string.Empty;
            if (animRef != null)
            {
                var animEventRef = animRef.eventReference;
                if (animEventRef == null)
                {
                    animEventRef = main.eventManager.getDefaultNodeEvent(node);
                }
                if (eventRefs != null && animEventRef != null)
                {
                    eventIndex = Array.IndexOf(eventRefs, animEventRef);
                    if (eventIndex < 0)
                        eventIndex = 0;
                }

                var animDefRef = animRef.animRef;
                if (animDefRef != null)
                {
                    var define = defines.FirstOrDefault(d => d.cardPoolId == animDefRef.cardPoolId && d.id == animDefRef.defineId);
                    animIndex = Array.IndexOf(defines, define);
                }
                sourcePosition = animRef.sourcePosition;
                targetPosition = animRef.targetPosition;
                sourcePositionVarName = animRef.sourcePositionVarName;
                targetPositionVarName = animRef.targetPositionVarName;
            }

            VFXDialogHideFlags hideFlags = VFXDialogHideFlags.None;
            hideFlags |= eventRefs != null ? 0 : VFXDialogHideFlags.Event;
            hideFlags |= sourcePosition == VisualEffectPosition.TEMP_VAR ? 0 : VFXDialogHideFlags.SourcePositionVarName;
            hideFlags |= targetPosition == VisualEffectPosition.TEMP_VAR ? 0 : VFXDialogHideFlags.TargetPositionVarName;
            var eventViewData = new VisualEffectDialogViewData()
            {
                eventIndex = eventIndex,
                animIndex = animIndex + 1,
                sourcePosition = sourcePosition,
                targetPosition = targetPosition,
                sourcePositionVarName = sourcePositionVarName,
                targetPositionVarName = targetPositionVarName,
                hideFlags = hideFlags
            };
            return eventViewData;
        }
        public void updateNodeAnimRef(Node node, ISkin skin, VisualEffectDialogInfo info)
        {
            var animRef = node.getOrInitNodeAnimationRef(skin);
            var defines = getAnimationDefinesForNode();
            var eventRefs = getLocalNodeVisualEffectEventRefs(node);
            var animIndex = info.animIndex;
            if (animIndex < 0)
            {
                animRef.animRef = new DefineReference(0, 0);
            }
            else
            {
                var define = defines[animIndex];
                animRef.animRef = new DefineReference(define.cardPoolId, define.id);
            }
            animRef.eventReference = eventRefs == null || info.eventIndex < 0 ? null : eventRefs[info.eventIndex];
            animRef.sourcePosition = info.sourcePosition;
            animRef.targetPosition = info.targetPosition;
            animRef.sourcePositionVarName = info.sourcePositionVarName;
            animRef.targetPositionVarName = info.targetPositionVarName;
        }
        #endregion

        #region 节点的可用事件类型
        public EventReference[] getNodeVisualEffectEventRefs(Node node, IEnumerable<ICardPoolReference> cardPoolset)
        {
            if (node is ActionNode action)
            {
                var actionRef = action.defineRef;
                return main.card.getActionBindedEventsRecursive(actionRef, cardPoolset);
            }
            return null;
        }
        public EventReference[] getLocalNodeVisualEffectEventRefs(Node node)
        {
            return getNodeVisualEffectEventRefs(node, main.card.getLocalCardPoolReferences());
        }
        public string[] getNodeVisualEffectEventEditorNames(Node node)
        {
            var eventRefs = getLocalNodeVisualEffectEventRefs(node);
            if (eventRefs == null)
                return null;
            return eventRefs.Select(r =>
            {
                var def = main.card.getLocalEventDefine(r);
                return main.eventManager.translateEventDefineNameForEditor(def);
            }).ToArray();
        }
        #endregion


        #region 调色
        public ColorModifyViewData[] getAnimationColorViewData(AnimationDefine define, IEnumerable<ColorModifyData> colorModifies)
        {
            if (define == null)
                return Array.Empty<ColorModifyViewData>();
            var animDefRef = define.getDefineRef();
            var possibleTargets = define.getColorTargetGroups();
            ColorModifyViewData[] viewData = new ColorModifyViewData[possibleTargets.Length];
            for (int i = 0; i < viewData.Length; i++)
            {
                var targetWithColor = possibleTargets[i];
                var targets = targetWithColor.targets;
                var modify = colorModifies?.FirstOrDefault(m => m.animationRef == animDefRef && m.targetIndex == i);

                var modifiedColor = targetWithColor.color;
                if (modify != null)
                {
                    modifiedColor = modify.getColor();
                }
                viewData[i] = new ColorModifyViewData()
                {
                    color = modifiedColor
                };
            }
            return viewData;
        }
        public ColorModifyViewData[] getAnimationColorViewDataForNode(Node node, ISkin skin)
        {
            SkinAnimationRef animRef = node.getOrInitNodeAnimationRef(skin);
            return getAnimationColorViewDataForNode(animRef);
        }
        public ColorModifyViewData[] getAnimationColorViewDataForNode(IAnimationRef animRef)
        {
            if (animRef == null)
                return Array.Empty<ColorModifyViewData>();

            var animDefRef = animRef.animRef;
            if (animDefRef == null)
                return Array.Empty<ColorModifyViewData>();

            var defines = getAnimationDefinesForNode();
            var define = defines.FirstOrDefault(d => d.cardPoolId == animDefRef.cardPoolId && d.id == animDefRef.defineId);
            return getAnimationColorViewData(define, animRef.getColorModifies());
        }
        public Color getAnimationColorForNode(Node node, ISkin skin, int index)
        {
            SkinAnimationRef animRef = node.getOrInitNodeAnimationRef(skin);
            return getAnimationColorForNode(animRef, index);
        }
        public Color getAnimationColorForNode(IAnimationRef animRef, int index)
        {
            if (animRef == null)
                return Color.red;

            var animDefRef = animRef.animRef;
            if (animDefRef == null)
                return Color.red;

            var defines = getAnimationDefinesForNode();
            var define = defines.FirstOrDefault(d => d.cardPoolId == animDefRef.cardPoolId && d.id == animDefRef.defineId);
            return getAnimationColor(define, animRef.getColorModifies(), index);
        }
        public Color getAnimationColor(AnimationDefine define, IEnumerable<ColorModifyData> modifies, int index)
        {
            if (define == null)
                return Color.red;
            var animDefRef = define.getDefineRef();
            var possibleTargets = define.getColorTargetGroups();
            var targetWithColor = possibleTargets[index];
            var modify = modifies?.FirstOrDefault(m => m.animationRef == animDefRef && m.targetIndex == index);
            return modify != null ? modify.getColor() : targetWithColor.color;
        }
        public void resetNodeAnimColor(Node node, ISkin skin)
        {
            var animRef = node.getOrInitNodeAnimationRef(skin);
            resetAnimColor(animRef);
        }
        public void resetAnimColor(IAnimationRef animRef)
        {
            if (animRef == null)
                return;
            animRef.clearColorModify(animRef.animRef);
        }
        public void setNodeAnimColor(Node node, ISkin skin, int index, Color color)
        {
            var animRef = node.getOrInitNodeAnimationRef(skin);
            setAnimationColor(animRef, index, color);
        }
        public void setAnimationColor(IAnimationRef animRef, int index, Color color)
        {
            if (animRef == null)
                return;
            var colorModifies = animRef.getColorModifies();
            var colorModify = Array.Find(colorModifies, m => m.animationRef == animRef.animRef && m.targetIndex == index);
            if (colorModify == null)
            {
                colorModify = new ColorModifyData()
                {
                    animationRef = animRef.animRef,
                    targetIndex = index
                };
                animRef.addColorModify(colorModify);
            }
            colorModify.r = color.r;
            colorModify.g = color.g;
            colorModify.b = color.b;
        }
        #endregion

        public static void cleanUnusedVisualEffects(ISkin skin, IEnumerable<GeneratedEffect> effects)
        {
            var allAnimRefs = skin.getAllAnimRefs();
            foreach (var animRef in allAnimRefs)
            {
                var key = animRef.key;
                var keys = effects.Where(e => e != null).SelectMany(e => e.graph.nodes).Select(n => SkinAnimationRef.getNodeKey(n));
                if (keys == null || !keys.Contains(key))
                {
                    skin.removeAnimRef(key);
                }
            }
        }


        [TranslateMsg("默认的特效名称")]
        public const string VFX_DEFAULT_NAME = "特效";
    }
}
