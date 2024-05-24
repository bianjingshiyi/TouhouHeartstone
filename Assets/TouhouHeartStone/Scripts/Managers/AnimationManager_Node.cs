using System;
using System.Collections.Generic;
using System.Linq;
using Game.Animations;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UI;
using UnityEngine;

namespace Game
{
    public partial class AnimationManager
    {
        public AnimationDefine[] getAnimationDefinesForNode()
        {
            return getAllAnimationDefines(false);
        }
        public string[] getAnimationEditorNamesForNode()
        {
            return getAnimationDefinesForNode().Select(d => main.lang._(d.name)).Prepend(main.lang._(StringTable.NONE)).ToArray();
        }
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
        public VisualEffectDialogViewData getNodeAnimationViewData(Node node, ISkin skin)
        {
            var defines = getAnimationDefinesForNode();
            var key = getOrInitNodeAnimationKey(node, skin);
            SkinAnimationRef animRef = skin.getAnimRef(key);

            var eventRefs = getLocalNodeVisualEffectEventRefs(node);
            var eventIndex = 0;
            int animIndex = -1;
            int sourcePosition = 0;
            int targetPosition = 0;
            string sourcePositionVarName = string.Empty;
            string targetPositionVarName = string.Empty;
            ColorModifyViewData[] colorModifies = null;
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

                    var possibleTargets = define.getColorTargetGroups();
                    colorModifies = new ColorModifyViewData[possibleTargets.Length];
                    for (int i = 0; i < colorModifies.Length; i++)
                    {
                        var targetWithColor = possibleTargets[i];
                        var targets = targetWithColor.targets;
                        var modify = animRef.colorModifies?.FirstOrDefault(m => m.targetIndex == i);

                        var modifiedColor = targetWithColor.color;
                        if (modify != null)
                        {
                            modifiedColor = modify.getColor();
                        }
                        colorModifies[i] = new ColorModifyViewData()
                        {
                            color = modifiedColor
                        };
                    }
                }
                sourcePosition = animRef.sourcePosition;
                targetPosition = animRef.targetPosition;
                sourcePositionVarName = animRef.sourcePositionVarName;
                targetPositionVarName = animRef.targetPositionVarName;
            }

            var eventViewData = new VisualEffectDialogViewData()
            {
                eventIndex = eventIndex,
                animIndex = animIndex + 1,
                sourcePosition = sourcePosition,
                targetPosition = targetPosition,
                sourcePositionVarName = sourcePositionVarName,
                targetPositionVarName = targetPositionVarName,
                sourcePositionVarNameVisible = sourcePosition == VisualEffectPosition.TEMP_VAR,
                targetPositionVarNameVisible = targetPosition == VisualEffectPosition.TEMP_VAR,
                colorModifies = colorModifies
            };
            return eventViewData;
        }

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
        public string getOrInitNodeAnimationKey(Node node, ISkin skin)
        {
            var key = SkinAnimationRef.getNodeKey(node);
            if (string.IsNullOrEmpty(key))
            {
                var animRefs = skin.getAllAnimRefs();
                key = $"AnimRef{animRefs.Length}_{node.id}";
                SkinAnimationRef.setNodeKey(node, key);
            }
            return key;
        }
        public SkinAnimationRef getOrInitNodeAnimationRef(Node node, ISkin skin)
        {
            var key = getOrInitNodeAnimationKey(node, skin);
            var animRef = skin.getAnimRef(key);
            if (animRef == null)
            {
                var defines = getAnimationDefinesForNode();
                var defaultDefine = defines.FirstOrDefault();
                var defRef = new DefineReference(defaultDefine?.cardPoolId ?? 0, defaultDefine?.id ?? 1);
                animRef = new SkinAnimationRef(key, defRef);
                skin.addAnimRef(animRef);
            }
            return animRef;
        }
        public void updateNodeAnimRef(Node node, ISkin skin, VisualEffectDialogInfo info)
        {
            var animRef = getOrInitNodeAnimationRef(node, skin);
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
        [TranslateMsg("默认的特效名称")]
        public const string VFX_DEFAULT_NAME = "特效";
    }
}
