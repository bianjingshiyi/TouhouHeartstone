using System;
using System.Collections.Generic;
using System.Linq;
using MukioI18n;
using TouhouCardEngine;
using UI;

namespace Game
{
    public partial class AnimationManager
    {
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
        public VisualEffectDialogInfo getNodeAnimationViewData(Node node, ISkin skin)
        {
            var defines = getAllAnimationDefines();
            var key = getOrInitNodeAnimationKey(node, skin);
            SkinAnimationRef animRef = skin.getAnimRef(key);

            var eventRefs = getNodeVisualEffectEventRefs(node);
            var eventIndex = 0;
            int animIndex = -1;
            int sourcePosition = 0;
            int targetPosition = 0;
            if (animRef != null)
            {
                if (eventRefs != null && animRef.eventReference != null)
                {
                    eventIndex = Array.IndexOf(eventRefs, animRef.eventReference);
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
            }

            var eventViewData = new VisualEffectDialogInfo()
            {
                eventIndex = eventIndex,
                animIndex = animIndex + 1,
                sourcePosition = sourcePosition,
                targetPosition = targetPosition
            };
            return eventViewData;
        }

        public EventReference[] getNodeVisualEffectEventRefs(Node node)
        {
            if (node is ActionNode action)
            {
                var actionRef = action.defineRef;
                return main.card.getAnyActionBindedEventsRecursive(actionRef);
            }
            return null;
        }
        public string[] getNodeVisualEffectEventEditorNames(Node node)
        {
            var eventRefs = getNodeVisualEffectEventRefs(node);
            if (eventRefs == null)
                return null;
            return eventRefs.Select(r =>
            {
                var def = main.card.getEventDefine(r);
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
                var defines = getAllAnimationDefines();
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
            var defines = getAllAnimationDefines();
            var eventRefs = getNodeVisualEffectEventRefs(node);
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
        }
        [TranslateMsg("默认的特效名称")]
        public const string VFX_DEFAULT_NAME = "特效";
    }
}
