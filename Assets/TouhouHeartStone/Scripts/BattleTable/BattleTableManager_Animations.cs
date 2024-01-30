using System;
using System.Collections.Generic;
using System.Reflection;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using Game.Animations;
using UnityEngine;
using Card = TouhouCardEngine.Card;
using UI;
using System.Linq;

namespace Game.BattleTable
{
    partial class BattleTableManager
    {
        #region Builtin
        Dictionary<Type, ConstructorInfo> animConstructorDic { get; } = new Dictionary<Type, ConstructorInfo>();
        /// <summary>
        /// 加载动画
        /// </summary>
        /// <param name="assembly"></param>
        public void loadAnim(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;
                Type baseType = type.BaseType;
                if (!baseType.IsGenericType)
                    continue;
                if (baseType.GetGenericTypeDefinition() != typeof(EventAnimation<>) &&
                    baseType.GetGenericTypeDefinition() != typeof(RequestAnimation<>))
                    continue;
                Type paraType = baseType.GetGenericArguments()[0];
                if (!paraType.IsSubclassOf(typeof(EventDefine)) &&
                    !paraType.IsSubclassOf(typeof(Request)))
                    continue;
                setAnim(paraType, type);
            }
        }
        /// <summary>
        /// 设置动画
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="animType"></param>
        public void setAnim(Type eventType, Type animType)
        {
            foreach (ConstructorInfo constructor in animType.GetConstructors())
            {
                var args = constructor.GetParameters();
                if (args.Length < 1 || (args.Length == 1 && args[0].ParameterType == eventType))
                {
                    if (animConstructorDic.ContainsKey(eventType))
                    {
                        UberDebug.LogWarningChannel("UI", $"存在冲突的动画{animType}");
                        break;
                    }
                    animConstructorDic.Add(eventType, constructor);
                    break;
                }
            }
        }
        /// <summary>
        /// 获取事件对应的动画
        /// </summary>
        /// <param name="eventArg"></param>
        /// <returns></returns>
        public EventAnimation createEventAnim(IEventArg eventArg)
        {
            Type type = eventArg.define.GetType();
            if (animConstructorDic.ContainsKey(type))
            {
                EventAnimation anim = null;
                if (animConstructorDic[type].GetParameters().Length == 0)
                    anim = animConstructorDic[type].Invoke(new object[0]) as EventAnimation;
                else if (animConstructorDic[type].GetParameters().Length == 1)
                    anim = animConstructorDic[type].Invoke(new object[] { eventArg }) as EventAnimation;
                anim.eventArg = eventArg;

                var eventData = new EventAnimStartData(eventArg);
                anim.startData = eventData;

                anim.init(this);
                return anim;
            }
            else
                return null;
        }
        public RequestAnimation createRequestAnim(IRequest request)
        {
            Type type = request.GetType();
            if (animConstructorDic.ContainsKey(type))
            {
                RequestAnimation anim = null;
                if (animConstructorDic[type].GetParameters().Length == 0)
                {
                    anim = animConstructorDic[type].Invoke(new object[0]) as RequestAnimation;
                    anim.request = request;
                }
                return anim;
            }
            else
                return null;
        }
        public EventAnimation getAnimOfEvent(IEventArg eventArg)
        {
            var anim = _animationCache.OfType<EventAnimation>().FirstOrDefault(e => e.eventArg == eventArg);
            if (anim != null)
                return anim;
            anim = _animationQueue.OfType<EventAnimation>().FirstOrDefault(e => e.eventArg == eventArg);
            if (anim != null)
                return anim;
            anim = _animationGroups.SelectMany(g => g).OfType<EventAnimation>().FirstOrDefault(e => e.eventArg == eventArg);
            if (anim != null)
                return anim;
            return null;
        }

        #region 死亡仆从锚点
        public Transform createDeadCardAnchor(int cardId, Vector3 position)
        {
            var card = game.getCard(cardId);
            var cardDefine = card.define;
            var name = getCardSkin(cardDefine)?.name ?? card.ToString();
            Transform transform;
            if (deadCardAnchors.ContainsKey(cardId))
            {
                transform = deadCardAnchors[cardId];
            }
            else
            {
                transform = new GameObject().transform;
                transform.parent = table.DeadCardAnchorsTransform;
                deadCardAnchors.Add(cardId, transform);
            }
            transform.position = position;
            transform.name = name;
            return transform;
        }
        public bool tryGetDeadCardAnchor(int cardId, out Transform transform)
        {
            return deadCardAnchors.TryGetValue(cardId, out transform);
        }
        public void clearDeadCardAnchors()
        {
            foreach (var pair in deadCardAnchors)
            {
                Destroy(pair.Value.gameObject);
            }
            deadCardAnchors.Clear();
        }
        #endregion

        #region 增益特效
        public void addCardBuffEffect(int cardId, int buffInstanceId, TableEffect effect)
        {
            if (!cardBuffEffects.TryGetValue(cardId, out var effectDict))
            {
                effectDict = new Dictionary<int, TableEffect>();
                cardBuffEffects[cardId] = effectDict;
            }
            effectDict.Add(buffInstanceId, effect);
        }
        public bool removeCardBuffEffect(int cardId, int buffInstanceId)
        {
            if (cardBuffEffects.TryGetValue(cardId, out var effectDict))
            {
                return effectDict.Remove(buffInstanceId);
            }
            return false;
        }
        public bool tryGetCardBuffEffect(int cardId, int buffInstanceId, out TableEffect effect)
        {
            if (cardBuffEffects.TryGetValue(cardId, out var effectDict))
            {
                return effectDict.TryGetValue(buffInstanceId, out effect);
            }
            effect = null;
            return false;
        }
        public void clearCardBuffEffects()
        {
            foreach (var cardPair in cardBuffEffects)
            {
                foreach (var buffPair in cardPair.Value)
                {
                    Destroy(buffPair.Value.gameObject);
                }
            }
            cardBuffEffects.Clear();
        }
        #endregion

        #region 特效动画
        public AnimationDefine getAnimationDefine(DefineReference animRef)
        {
            return gameManager.getAnimationDefine(animRef);
        }
        private void addSkinEffectAnim(AnimationDefine animationDef, EventArg arg, EventArg effectTriggerArg, SkinAnimationRef skinAnimRef, Flow flow)
        {
            var anim = new EventEffectAnim(animationDef, arg, effectTriggerArg, skinAnimRef);
            if (skinAnimRef.sourcePosition == VisualEffectPosition.TEMP_VAR)
            {
                var tempVar = flow.getFlowVariable(skinAnimRef.sourcePositionVarName);
                if (FlowConvert.tryConvertType(flow, tempVar, out Card card))
                {
                    anim.sourceTempVarCardId = card?.id ?? -1;
                }
            }
            if (skinAnimRef.targetPosition == VisualEffectPosition.TEMP_VAR)
            {
                var tempVar = flow.getFlowVariable(skinAnimRef.targetPositionVarName);
                if (FlowConvert.tryConvertType(flow, tempVar, out Card card))
                {
                    anim.targetTempVarCardId = card?.id ?? -1;
                }
            }
            addAnim(anim);
            addToInsertedAnimsOfParent(arg, anim);
        }
        private void checkCardSkinEffectAnimations(Flow flow, EventArg arg)
        {
            if (arg is EventArg eventArg && eventArg.define is EffectTriggerEventDefine)
            {
                checkCardSkinEffectEntryAnimation(flow, eventArg);
                return;
            }
            var effectTriggerArg = getEffectEventTriggerArg(arg);
            if (effectTriggerArg == null)
                return;
            var skinAnimRef = getEventSkinAnimRef(arg, effectTriggerArg);
            if (skinAnimRef == null)
                return;
            var animationDef = getAnimationDefine(skinAnimRef.animRef);
            if (animationDef != null)
            {
                addSkinEffectAnim(animationDef, arg, effectTriggerArg, skinAnimRef, flow);
            }
        }
        private void checkCardSkinEffectEntryAnimation(Flow flow, EventArg arg)
        {
            if (arg == null)
                return;
            var skinAnimRef = getEffectEntrySkinAnimRef(flow, arg);
            if (skinAnimRef == null)
                return;
            var animationDef = getAnimationDefine(skinAnimRef.animRef);
            if (animationDef != null)
            {
                addSkinEffectAnim(animationDef, arg, arg, skinAnimRef, flow);
            }
        }
        public EventArg getEffectEventTriggerArg(IEventArg arg)
        {
            var seekArg = arg;
            while (seekArg != null)
            {
                if (seekArg is EventArg effectArg && seekArg.define is EffectTriggerEventDefine)
                {
                    return effectArg;
                }
                seekArg = seekArg.parent;
            }
            return null;
        }
        public SkinAnimationRef getEventSkinAnimRef(EventArg arg, EventArg effectArg)
        {
            var effectRef = effectArg.getVar<EffectReference>(EffectTriggerEventDefine.VAR_EFFECT_REF);
            var effect = effectRef.getEffect();
            var buff = effectRef.buff;
            var cardDefine = effectRef.cardDefine;
            if (buff != null)
            {
                var define = buff.define;
                var skin = getBuffSkin(define);
                if (effect is not GeneratedEffect genEffect) return null; // 不是自定义效果

                var node = genEffect.graph.findNodeById(arg.flowNodeId);
                return getAnimRefOfEventNode(skin, arg, node);

            }
            else if (cardDefine != null)
            {
                var skin = getCardSkin(cardDefine);
                if (effect is not GeneratedEffect genEffect) return null; // 不是自定义效果

                var node = genEffect.graph.findNodeById(arg.flowNodeId);
                return getAnimRefOfEventNode(skin, arg, node);
            }
            return null;
        }
        public SkinAnimationRef getEffectEntrySkinAnimRef(Flow flow, EventArg effectArg)
        {
            var effectRef = effectArg.getVar<EffectReference>(EffectTriggerEventDefine.VAR_EFFECT_REF);
            var effect = effectRef.getEffect();
            var buff = effectRef.buff;
            var cardDefine = effectRef.cardDefine;
            if (buff != null)
            {
                var define = buff.define;
                var skin = getBuffSkin(define);
                if (effect is not IEntryNodeEffect entryNodeEffect) return null; //效果不正确

                var node = entryNodeEffect.entryNode;
                return getAnimRefOfEntryNode(skin, node);
            }
            else if (cardDefine != null)
            {
                var skin = getCardSkin(cardDefine);
                if (effect is not IEntryNodeEffect entryNodeEffect) return null; //效果不正确

                var node = entryNodeEffect.entryNode;
                return getAnimRefOfEntryNode(skin, node);
            }
            return null;
        }
        public Card getEventSkinAnimCard(EventArg effectArg, IEventArg arg, int position)
        {
            var card = effectArg.getVar<Card>(EffectTriggerEventDefine.VAR_CARD);
            switch (position)
            {
                case VisualEffectPosition.SELF_MASTER:
                    return card.getOwner().getMaster();
                case VisualEffectPosition.ENEMY_MASTER:
                    return game.getOpponent(card.getOwner()).getMaster();
                case VisualEffectPosition.THIS_CARD:
                    return card;
                case VisualEffectPosition.TARGET_CARD:
                    if (arg.define is ICardEventDefine targetEventArg)
                    {
                        return targetEventArg.getCard(arg) as Card;
                    }
                    break;
            }
            return null;
        }
        public TableArea getEventSkinAnimArea(EventArg effectArg, IEventArg arg, int area)
        {
            return TableArea.AllField;
        }
        private SkinAnimationRef getAnimRefOfEntryNode(ISkin skin, Node node)
        {
            if (node == null) return null; // 节点不存在

            var key = SkinAnimationRef.getNodeKey(node);
            return skin.getAnimRef(key);
        }
        private SkinAnimationRef getAnimRefOfEventNode(ISkin skin, IEventArg arg, Node node)
        {
            if (node == null) return null; // 节点不存在
            var key = SkinAnimationRef.getNodeKey(node);
            var animRef = skin.getAnimRef(key);

            if (!isCorrectEventForAnimRef(arg, animRef, node)) return null; // 事件不正确

            return animRef;
        }
        private bool isCorrectEventForAnimRef(IEventArg arg, SkinAnimationRef animRef, Node node)
        {
            if (animRef == null)
                return false;
            var eventRef = animRef.eventReference;
            if (eventRef == null) // 不存在事件名，可能是老版本的引用，获取之前设置的默认的事件
                eventRef = eventManager.getDefaultNodeEvent(node);

            if (eventRef == null) // 还是不存在，获取该节点可能事件的第一个。
            {
                var cardPoolSet = getCardPoolSet();
                var eventRefs = animManager.getNodeVisualEffectEventRefs(node, cardPoolSet);
                // 该节点的所有事件可能也会因为是入口节点导致是null，那就不改
                if (eventRefs != null)
                    eventRef = eventRefs.FirstOrDefault();
            }
            var argRef = CardManager.getEventReferenceOfEventArg(arg);
            return eventRef == argRef;
        }
        #endregion

        #endregion

        #region 私有方法
        private void updateCardBuffEffects(float deltaTime)
        {
            foreach (var cardPair in cardBuffEffects)
            {
                var cardId = cardPair.Key;
                var effectDict = cardPair.Value;
                var buffInstanceIds = effectDict.Keys.ToList();
                foreach (var buffInstanceId in buffInstanceIds)
                {
                    var effect = effectDict[buffInstanceId];
                    if (!effect.sourceTransform)
                    {
                        effect.sourceTransform = getCardTransform(cardId, true);
                    }
                    effect.update(deltaTime);
                    if (effect.isFinished())
                    {
                        Destroy(effect.gameObject);
                        effectDict.Remove(buffInstanceId);
                    }
                }
            }
        }
        private void addToInsertedAnimsOfParent(IEventArg arg, TableAnimation anim)
        {
            var parent = arg.parent;
            while (parent != null)
            {
                if (parent.state == EventState.Before)
                {
                    var anims = _animationCache.Concat(_animationQueue).OfType<EventAnimation>().Where(e => e.eventArg == parent);
                    foreach (var a in anims)
                    {
                        a.insertedAnims.Add(anim);
                    }
                }
                parent = parent.parent;
            }
        }

        #endregion
        /// <summary>
        /// 等待被加入到待播放队列中的动画。
        /// </summary>
        [SerializeField]
        List<TableAnimation> _animationCache = new List<TableAnimation>();
        /// <summary>
        /// 待播放队列，会在所有事件都结束，或者需要玩家进行操作时才会加入动画组进行播放。
        /// </summary>
        [SerializeField]
        List<TableAnimation> _animationQueue = new List<TableAnimation>();
        /// <summary>
        /// 正在播放的动画组列表。
        /// </summary>
        [SerializeField]
        List<AnimationGroup> _animationGroups = new List<AnimationGroup>();
        /// <summary>
        /// 正在被加入待播放队列中的动画列表。
        /// </summary>
        List<TableAnimation> insertingAnims = new List<TableAnimation>();
        public void addAnim(TableAnimation anim)
        {
            _animationCache.Add(anim);
        }
        public bool removeAnim(TableAnimation anim)
        {
            if (_animationCache.Remove(anim) || _animationQueue.Remove(anim))
                return true;
            foreach (var group in _animationGroups)
            {
                if (group.Remove(anim))
                    return true;
            }
            return false;
        }
        public void queueAnims()
        {
            bool canAddToQueue = false;
            if (game.triggers.currentEvent == null)
            {
                canAddToQueue = true;
            }
            insertingAnims.Clear();
            for (int i = _animationCache.Count - 1; i >= 0; i--)
            {
                var anim = _animationCache[i];
                if (anim is TableAnimation tableAnim && tableAnim.forceQueueAnimations)
                {
                    canAddToQueue = true;
                }
                if (!canAddToQueue)
                    continue;
                insertingAnims.Insert(0, anim);
                _animationCache.RemoveAt(i);
            }
            _animationQueue.AddRange(insertingAnims);
            insertingAnims.Clear();
        }
        public bool isPlayingAnimation()
        {
            return _animationGroups.Count + _animationQueue.Count > 0;
        }
        public AnimationGroup getAnimGroupOfAnim(TableAnimation anim)
        {
            return _animationGroups.FirstOrDefault(g => g.Contains(anim));
        }
        public AnimationGroup[] getAllAnimGroups()
        {
            return _animationGroups.ToArray();
        }
        public AnimationGroup getNextGroupOfAnimGroup(AnimationGroup anim)
        {
            var index = _animationGroups.IndexOf(anim);
            if (index < 0) 
                return null;
            var nextIndex = index + 1;
            if (nextIndex >= _animationGroups.Count)
                return null;
            return _animationGroups[nextIndex];
        }
        public void updateAnim()
        {
            while (_animationQueue.Count > 0)
            {
                var first = _animationQueue.FirstOrDefault();
                if (first == null)
                {
                    _animationQueue.RemoveAt(0);
                    continue;
                }
                var group = new AnimationGroup
                {
                    first
                };
                group.AddRange(first.getGroupedAnimations(_animationQueue.Skip(1)).Where(a => a != null));
                _animationGroups.Add(group);
                _animationQueue.RemoveAll(t => group.Contains(t));
            }
            if (_animationGroups.Count > 0)
            {
                for (int i = 0; i < _animationGroups.Count; i++)
                {
                    var group = _animationGroups[i];
                    bool blocked = false;
                    for (int j = 0; j < i; j++)
                    {
                        var prevGroup = _animationGroups[j];
                        if (!prevGroup.continueNextGroup(group))
                        {
                            blocked = true;
                            break;
                        }
                    }
                    if (blocked)
                        break;
                    group.update(this, Time.deltaTime);
                    if (group == null || group.Count <= 0)
                    {
                        _animationGroups.RemoveAt(i);
                        i--;
                    }
                }
                if (_animationGroups.Count <= 0)
                    onAllAnimsFinished?.Invoke();
            }
        }
        public event Action onAllAnimsFinished;
        #region 属性字段
        /// <summary>
        /// 当前回合的玩家（仅限动画显示）。
        /// </summary>
        public Player currentAnimPlayer { get; set; }
        Dictionary<int, HandToFieldAnim> _servantPreviewAnimDict = new Dictionary<int, HandToFieldAnim>();
        Dictionary<int, Transform> deadCardAnchors = new Dictionary<int, Transform>();
        Dictionary<int, Dictionary<int, TableEffect>> cardBuffEffects = new Dictionary<int, Dictionary<int, TableEffect>>();
        #endregion
    }
}
