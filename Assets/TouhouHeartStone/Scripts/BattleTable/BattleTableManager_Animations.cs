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
using BJSYGameCore;
using TreeNode = BJSYGameCore.TreeNode<(TouhouCardEngine.EventArg eventArg, Game.BattleTable.EventAnimation anim, Game.Animations.EventEffectAnim skinAnim, bool isFinished)>;
using System.Collections;

namespace Game.BattleTable
{
    partial class BattleTableManager
    {
        public TreeNode generateAnimForEvent(EventArg eventArg)
        {
            TreeNode node = new TreeNode();
            node.value.eventArg = eventArg;
            // 事件前
            foreach (EventArg beforeEvent in eventArg.beforeChildrenEvents)
            {
                TreeNode childNode = generateAnimForEvent(beforeEvent);
                childNode.parent = node;
                node.children.Add(childNode);
            }
            // 本体
            if (tryGenerateSkinAnimForEvent(game.currentFlow, eventArg, out EventEffectAnim skinAnim))
            {
                node.value.skinAnim = skinAnim;
            }
            EventAnimation anim = createEventAnim(eventArg);
            if (anim != null)
            {
                node.value.anim = anim;
            }
            // 子事件
            foreach (EventArg childEvent in eventArg.logicChildrenEvents)
            {
                TreeNode childNode = generateAnimForEvent(childEvent);
                childNode.parent = node;
                node.children.Add(childNode);
            }
            // 事件后
            foreach (EventArg afterEvent in eventArg.afterChildrenEvents)
            {
                TreeNode childNode = generateAnimForEvent(afterEvent);
                childNode.parent = node;
                node.children.Add(childNode);
            }
            return node;
        }
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
                if (baseType.GetGenericTypeDefinition() != typeof(EventAnimation<>))
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
            var name = getCardName(cardDefine);
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
        public bool hasBuffEffect(int cardId, DefineReference animRef)
        {
            if (cardBuffEffects.TryGetValue(cardId, out var effectDict))
            {
                return effectDict.Any(p => p.Value.define.getDefineRef() == animRef);
            }
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
        private EventEffectAnim createSkinEffectAnim(AnimationDefine animDef, EventArg arg, SkinAnimationRef animRef, Flow flow, Card card)
        {
            EventEffectAnim anim = new EventEffectAnim(animDef, arg, animRef, card);
            if (animRef.sourcePosition == VisualEffectPosition.TEMP_VAR)
            {
                object tempVar = flow.getFlowVariable(animRef.sourcePositionVarName);
                if (FlowConvert.tryConvertType(flow, tempVar, out Card varCard))
                {
                    anim.sourceTempVarCardId = varCard?.id ?? -1;
                }
            }
            if (animRef.targetPosition == VisualEffectPosition.TEMP_VAR)
            {
                object tempVar = flow.getFlowVariable(animRef.targetPositionVarName);
                if (FlowConvert.tryConvertType(flow, tempVar, out Card varCard))
                {
                    anim.targetTempVarCardId = varCard?.id ?? -1;
                }
            }
            return anim;
        }
        private void addSkinEffectAnim(AnimationDefine animationDef, EventArg arg, SkinAnimationRef skinAnimRef, Flow flow, Card card)
        {
            EventEffectAnim anim = createSkinEffectAnim(animationDef, arg, skinAnimRef, flow, card);
            addEventAnim(arg, anim);
        }
        //private void checkCardSkinEffectAnimations(Flow flow, EventArg arg)
        //{
        //    if (arg.define is IEffectEventDefine)
        //    {
        //        checkCardSkinEffectEntryAnimation(flow, arg);
        //        return;
        //    }
        //    EventArg effectTriggerArg = getEffectEventTriggerArg(arg);
        //    if (effectTriggerArg == null)
        //        return;
        //    SkinAnimationRef skinAnimRef = getEventSkinAnimRef(arg, effectTriggerArg);
        //    if (skinAnimRef == null)
        //        return;
        //    AnimationDefine animationDef = getAnimationDefine(skinAnimRef.animRef);
        //    if (animationDef != null)
        //    {
        //        addSkinEffectAnim(animationDef, arg, effectTriggerArg, skinAnimRef, flow);
        //    }
        //}
        /// <summary>
        /// 根据卡牌皮肤上记载的事件动画信息创建动画
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="eventArg"></param>
        /// <param name="anim"></param>
        /// <returns></returns>
        private bool tryGenerateSkinAnimForEvent(Flow flow, EventArg eventArg, out EventEffectAnim anim)
        {
            if (eventArg == null)
            {
                anim = null;
                return false;
            }

            if (eventArg.define is EffectActivationEventDefine || eventArg.define is EffectTriggerEventDefine)
            {
                return tryGenerateAnimForEffectEvent(flow, eventArg, out anim);
            }
            EventArg effectTriggerArg = getEffectEventTriggerArg(eventArg);
            if (effectTriggerArg == null)
            {
                anim = null;
                return false;
            }
            Effect effect = null;
            Card card = null;
            if (effectTriggerArg.define is EffectActivationEventDefine)
            {
                effect = effectTriggerArg.getVar<Effect>(EffectActivationEventDefine.VAR_EFFECT);
                card = effectTriggerArg.getVar<Card>(EffectActivationEventDefine.VAR_CARD);
            }
            else if (effectTriggerArg.define is EffectTriggerEventDefine)
            {
                effect = effectTriggerArg.getVar<Effect>(EffectTriggerEventDefine.VAR_EFFECT);
                card = effectTriggerArg.getVar<Card>(EffectTriggerEventDefine.VAR_CARD);
            }
            else
            {
                anim = null;
                return false;
            }
            SkinAnimationRef skinAnimRef = getEventSkinAnimRef(eventArg, effect);
            if (skinAnimRef == null)
            {
                anim = null;
                return false;
            }
            AnimationDefine animationDef = getAnimationDefine(skinAnimRef.animRef);
            if (animationDef == null)
            {
                anim = null;
                return false;
            }
            anim = createSkinEffectAnim(animationDef, eventArg, skinAnimRef, flow, card);
            return true;
        }
        private bool tryGenerateAnimForEffectEvent(Flow flow, EventArg arg, out EventEffectAnim anim)
        {
            if (arg == null)
            {
                anim = null;
                return false;
            }

            SkinAnimationRef skinAnimRef = getEffectEntrySkinAnimRef(arg);
            if (skinAnimRef == null)
            {
                anim = null;
                return false;
            }
            AnimationDefine animDef = getAnimationDefine(skinAnimRef.animRef);
            if (animDef == null)
            {
                anim = null;
                return false;
            }
            Card card = null;
            if (arg.define is EffectActivationEventDefine active)
            {
                card = arg.getVar<Card>(EffectActivationEventDefine.VAR_CARD);
            }
            else if (arg.define is EffectTriggerEventDefine trigger)
            {
                card = arg.getVar<Card>(EffectTriggerEventDefine.VAR_CARD);
            }
            else
            {
                anim = null;
                return false;
            }
            anim = createSkinEffectAnim(animDef, arg, skinAnimRef, flow, card);
            return true;
        }
        //private void checkCardSkinEffectEntryAnimation(Flow flow, EventArg arg)
        //{
        //    if (arg == null)
        //        return;
        //    SkinAnimationRef skinAnimRef = getEffectEntrySkinAnimRef(arg);
        //    if (skinAnimRef == null)
        //        return;
        //    AnimationDefine animationDef = getAnimationDefine(skinAnimRef.animRef);
        //    if (animationDef != null)
        //    {
        //        addSkinEffectAnim(animationDef, arg, arg, skinAnimRef, flow);
        //    }
        //}
        public EventArg getEffectEventTriggerArg(EventArg arg)
        {
            EventArg seekArg = arg;
            for (int i = 0; i < 100; i++)
            {
                if (seekArg == null)
                    return null;
                if (seekArg.define is EffectActivationEventDefine || seekArg.define is EffectTriggerEventDefine)
                    return seekArg;
                seekArg = seekArg.parent as EventArg;
            }
            throw new OverflowException();
        }
        public SkinAnimationRef getEventSkinAnimRef(EventArg arg, Effect effect)
        {
            BuffReference buffDefineRef = effect.buffDefineRef;
            DefineReference cardDefineRef = effect.cardDefineRef;
            if (buffDefineRef != null)
            {
                BuffDefine buffDefine = game.getBuffDefine(buffDefineRef);
                BuffSkin skin = getBuffSkin(buffDefine);
                if (effect is not GeneratedEffect genEffect) return null; // 不是自定义效果
                if (skin == null) return null; // 没有皮肤

                Node node = genEffect.graph.findNodeById(arg.flowNodeId);
                return getAnimRefOfEventNode(skin, arg, node);
            }
            else if (cardDefineRef != null)
            {
                CardDefine cardDefine = game.getDefine(cardDefineRef);
                (_, CardSkinData skin) = getCardSkin(cardDefine);
                if (effect is not GeneratedEffect genEffect) return null; // 不是自定义效果
                if (skin == null) return null; // 没有皮肤

                Node node = genEffect.graph.findNodeById(arg.flowNodeId);
                return getAnimRefOfEventNode(skin, arg, node);
            }
            return null;
        }
        public SkinAnimationRef getEffectEntrySkinAnimRef(EventArg effectArg)
        {
            var effectDefine = effectArg.define as IEffectEventDefine;
            var effect = effectDefine.getEffect(effectArg);
            var buffDefineRef = effect.buffDefineRef;
            var cardDefineRef = effect.cardDefineRef;
            ISkin skin = null;
            if (buffDefineRef != null)
            {
                var buffDefine = game.getBuffDefine(cardDefineRef);
                skin = getBuffSkin(buffDefine);
            }
            else if (cardDefineRef != null)
            {
                var cardDefine = game.getDefine(cardDefineRef);
                (_, skin) = getCardSkin(cardDefine);
            }
            if (effect is not IEntryNodeEffect entryNodeEffect) return null; //效果不正确
            if (skin == null) return null; // 没有皮肤

            var node = entryNodeEffect.entryNode;
            if (effectDefine is EffectActivationEventDefine)
            {
                if (!effectArg.getVar<bool>(EffectActivationEventDefine.VAR_ENABLE))
                    // 失效动作不播放特效。
                    return null;
                if (node.GetType() != typeof(PassiveEntryNode))
                    // 非被动效果的启用/禁用效果不播放特效。
                    return null;
                var passiveEntry = node as PassiveEntryNode;
                if (passiveEntry.getTags().Contains(EFFECTTAG.PASSIVE_LASTWORD))
                    // 遗言效果的启用/禁用效果不播放特效。
                    return null;
            }

            return getAnimRefOfEntryNode(skin, node);
        }
        public TableArea getEventSkinAnimArea(EventArg arg, int area)
        {
            return TableArea.AllField;
        }
        private SkinAnimationRef getAnimRefOfEntryNode(ISkin skin, Node node)
        {
            if (node == null || skin == null) return null; // 节点不存在

            var key = SkinAnimationRef.getNodeKey(node);
            return skin.getAnimRef(key);
        }
        private SkinAnimationRef getAnimRefOfEventNode(ISkin skin, IEventArg arg, Node node)
        {
            if (node == null || skin == null) return null; // 节点不存在
            string key = SkinAnimationRef.getNodeKey(node);
            SkinAnimationRef animRef = skin.getAnimRef(key);

            if (!isCorrectEventForAnimRef(arg, animRef, node)) return null; // 事件不正确

            return animRef;
        }
        private bool isCorrectEventForAnimRef(IEventArg arg, SkinAnimationRef animRef, Node node)
        {
            if (animRef == null)
                return false;
            EventReference eventRef = animRef.eventReference;
            if (eventRef == null) // 不存在事件名，可能是老版本的引用，获取之前设置的默认的事件
                eventRef = eventManager.getDefaultNodeEvent(node);

            if (eventRef == null) // 还是不存在，获取该节点可能事件的第一个。
            {
                EventReference[] eventRefs = animManager.getNodeVisualEffectEventRefs(node, getCardPoolSet());
                // 该节点的所有事件可能也会因为是入口节点导致是null，那就不改
                if (eventRefs != null)
                    eventRef = eventRefs.FirstOrDefault();
            }
            EventReference argRef = CardManager.getEventReferenceOfEventArg(arg);
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
        private bool addInsertedAnimOfParent(IEventArg arg, TableAnimation anim)
        {
            var parent = arg.parent;
            while (parent != null)
            {
                if (parent.state == EventState.Before)
                {
                    var anims = _animationCache.Concat(_animationQueue).OfType<EventAnimation>().Where(e => e.eventArg == parent);
                    if (anims.Count() > 0)
                    {
                        foreach (var a in anims)
                        {
                            var childGroup = new AnimationGroup();
                            childGroup.addToGroup(anim);
                            a.insertedAnims.Add(childGroup);
                        }
                        return true;
                    }
                }
                parent = parent.parent;
            }
            return false;
        }

        #endregion
        public void addEventAnim(IEventArg arg, TableAnimation anim)
        {
            bool addedAsChild = addInsertedAnimOfParent(arg, anim);
            if (addedAsChild)
            {
                return;
            }
            addAnim(anim);
        }
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
        /// <summary>
        /// 入队动画
        /// </summary>
        public void queueAnims()
        {
            //if (_animationCache.Count < 1)
            //    return;

            //bool canAddToQueue = true;
            ////if (game.triggers.currentEvent == null)
            ////    canAddToQueue = true;
            //List<TableAnimation> insertingAnims = new List<TableAnimation>();
            //for (int i = 0; i < _animationCache.Count; i++)
            //{
            //    TableAnimation anim = _animationCache[i];
            //    //if (anim is RequestEventAnim) // 询问必定显示
            //    //    canAddToQueue = true;
            //    if (canAddToQueue)
            //    {
            //        insertingAnims.Add(anim);
            //        _animationCache.RemoveAt(i);
            //        i--;
            //    }
            //}
            //_animationQueue.AddRange(insertingAnims);
        }
        public AnimationGroup[] getAllPlayingAnimationGroups()
        {
            List<AnimationGroup> groups = new List<AnimationGroup>();
            getAllPlayingAnimationGroupsNonAlloc(_animationGroups, groups);
            return groups.ToArray();
        }
        private void getAllPlayingAnimationGroupsNonAlloc(List<AnimationGroup> source, List<AnimationGroup> results)
        {
            foreach (var group in source)
            {
                results.Add(group);
                foreach (var anim in group)
                {
                    getAllPlayingAnimationGroupsNonAlloc(anim.insertedAnims, results);
                }
            }
        }
        /// <summary>
        /// 更新动画
        /// </summary>
        public void updateAnim()
        {
            //while (_animationQueue.Count > 0)
            //{
            //    // 动画队列出队
            //    TableAnimation first = _animationQueue[0];
            //    _animationQueue.RemoveAt(0);
            //    // 创建动画组
            //    AnimationGroup group = new AnimationGroup();
            //    group.addToGroup(first);
            //    foreach (TableAnimation groupedAnim in first.getGroupedAnimations(_animationQueue))
            //    {
            //        group.addToGroup(groupedAnim);
            //    }
            //    _animationGroups.Add(group);
            //    foreach (TableAnimation anim in group)
            //    {
            //        _animationQueue.Remove(anim);
            //    }
            //}
            //if (_animationGroups.Count > 0)
            //{
            //    for (int i = 0; i < _animationGroups.Count; i++)
            //    {
            //        AnimationGroup group = _animationGroups[i];
            //        // 检查动画组是否被之前的动画组阻挡
            //        bool blocked = false;
            //        for (int j = 0; j < i; j++)
            //        {
            //            AnimationGroup prevGroup = _animationGroups[j];
            //            if (!prevGroup.continueNextGroup(group))
            //            {
            //                blocked = true;
            //                break;
            //            }
            //        }
            //        if (blocked)
            //            break;
            //        // 更新动画组
            //        group.update(this, Time.deltaTime);
            //        if (group.Count <= 0)
            //        {
            //            _animationGroups.RemoveAt(i);
            //            i--;
            //        }
            //    }
            //    if (_animationGroups.Count <= 0)
            //        onAllAnimsFinished?.Invoke();
            //}
            if (_animationTrees.Count < 1)
                return;

            for (int i = 0; i < _animationTrees.Count; i++)
            {
                TreeNode root = _animationTrees[i];
                TreeNode node = root;
                bool blocked = false;
                foreach (TableAnimation playingAnim in _playingAnimList)
                {
                    if (!playingAnim.continueNextGroup(new TableAnimation[] { node.value.anim }))
                    {
                        blocked = true;
                        break;
                    }
                }
                if (blocked)
                    break; // 动画受阻，退出以等待正在播放的动画播放完毕。
                bool isAnyUnfinished = false;

            }
            if (_animationTrees.Count < 1)
                onAllAnimsFinished?.Invoke();
        }
        /// <summary>
        /// 更新单棵树的动画
        /// </summary>
        /// <param name="node"></param>
        /// <param name="playingAnimList"></param>
        private void updateAnimInTree(AnimationTreeNode node)
        {
            // 本动画播放
            EventAnimation anim = node.anim;
            bool isBlocked = false;
            if (_playingAnimList.Count > 0)
            {
                //第一个动画永远不被阻挡
                for (int i = 0; i < _playingAnimList.Count; i++)
                {
                    TableAnimation prevAnim = _playingAnimList[i];
                    if (prevAnim.blockAnimInGroup(anim)) // 判断动画是否被正在播放的动画阻挡
                    {
                        isBlocked = true;
                        break;
                    }
                }
            }
            if (!isBlocked)
            {
                try
                {
                    if (node.state == EventAnimState.NotStarted)
                    {
                        if (node.eventArg.state == EventState.Before)
                        {
                            // 事件没有结束，常见于等待玩家回应的情况，播放事件的开始部分动画。
                            node.state = EventAnimState.Started;
                            node.coroutine = StartCoroutine(updateStartAnimCoroutine(node));
                        }
                        else if (hasUnfinishedChildAnim(node))
                        {
                            // 事件有子动画，播放事件的开始部分动画。
                            node.state = EventAnimState.Started;
                            node.coroutine = StartCoroutine(updateStartAnimCoroutine(node));
                        }
                        else
                        {
                            // 事件已经完毕的一般情况。
                            node.state = EventAnimState.Started;
                            node.coroutine = StartCoroutine(updateAnimCoroutine(node));
                        }
                    }
                    else if (node.state == EventAnimState.StartFinished)
                    {
                        if (node.eventArg.isCanceled)
                        {
                            // 事件被取消了，播放取消的动画
                            node.state = EventAnimState.Canceled;
                            node.coroutine = StartCoroutine(updateCancelAnimCoroutine(node));
                        }
                        else
                        {
                            // 事件正常结束，播放结束的动画
                            node.state = EventAnimState.Logic;
                            node.coroutine = StartCoroutine(updateFinishAnimCoroutine(node));
                        }
                    }
                }
                catch (Exception e)
                {
                    node.state = EventAnimState.Finished;
                    Debug.LogError($"动画{anim}播放异常被跳过：{e}");
                }
            }
            // 发生前子事件播放
            for (int i = 0; i < node.beforeChildren.Count; i++)
            {
                AnimationTreeNode childNode = node.beforeChildren[i];
                updateAnimInTree(childNode);
            }
            // 发生后子事件播放
            for (int i = 0; i < node.afterChildren.Count; i++)
            {
                AnimationTreeNode childNode = node.afterChildren[i];
                updateAnimInTree(childNode);
            }
        }
        private IEnumerator updateAnimCoroutine(AnimationTreeNode node)
        {
            if (node.skinAnim != null)
            {
                _playingAnimList.Add(node.skinAnim);
                yield return node.skinAnim.update(this);
                _playingAnimList.Remove(node.skinAnim);
            }
            if (node.anim != null)
            {
                _playingAnimList.Add(node.anim);
                yield return node.anim.startUpdate(this);
                node.state = EventAnimState.StartFinished;
                yield return node.anim.logicUpdate(this);
                _playingAnimList.Remove(node.anim);
            }
            node.state = EventAnimState.Finished;
            node.coroutine = null;
        }
        /// <summary>
        /// 播放动画的开始部分，然后停住。常见于陷阱，事件前等后发先至的效果的动画。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private IEnumerator updateStartAnimCoroutine(AnimationTreeNode node)
        {
            if (node.skinAnim != null)
            {
                _playingAnimList.Add(node.skinAnim);
                yield return node.skinAnim.update(this);
                _playingAnimList.Remove(node.skinAnim);
            }
            if (node.anim != null)
            {
                _playingAnimList.Add(node.anim);
                yield return node.anim.startUpdate(this);
                _playingAnimList.Remove(node.anim);
            }
            node.state = EventAnimState.StartFinished;
            node.coroutine = null;
        }
        /// <summary>
        /// 播放动画开始之后的部分，用于调用完播放开始部分之后，结束整个动画。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private IEnumerator updateFinishAnimCoroutine(AnimationTreeNode node)
        {
            if (node.anim != null)
            {
                _playingAnimList.Add(node.anim);
                yield return node.anim.logicUpdate(this);
                _playingAnimList.Remove(node.anim);
            }
            node.state = EventAnimState.Finished;
            node.coroutine = null;
        }
        private IEnumerator updateCancelAnimCoroutine(AnimationTreeNode node)
        {
            if (node.anim != null)
            {
                _playingAnimList.Add(node.anim);
                yield return node.anim.canceledUpdate(this);
                _playingAnimList.Remove(node.anim);
            }
            node.state = EventAnimState.Finished;
            node.coroutine = null;
        }
        /// <summary>
        /// 检查给定节点是否有未完成的前序子动画。不检查给定的节点本身。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool hasUnfinishedChildAnim(AnimationTreeNode node)
        {
            foreach (AnimationTreeNode childNode in node.beforeChildren)
            {
                if (childNode.anim == null && childNode.skinAnim == null)
                    continue;
                if (childNode.state != EventAnimState.Finished || hasUnfinishedChildAnim(childNode))
                    return true;
            }
            return false;
        }
        public event Action onAllAnimsFinished;
        #region 属性字段
        /// <summary>
        /// 当前回合的玩家（仅限动画显示）。
        /// </summary>
        public Player currentAnimPlayer { get; set; }
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
        private readonly List<TreeNode> _animationTrees = new List<TreeNode>();
        private readonly List<TableAnimation> _playingAnimList = new List<TableAnimation>();
        Dictionary<int, HandToFieldAnim> _servantPreviewAnimDict = new Dictionary<int, HandToFieldAnim>();
        Dictionary<int, Transform> deadCardAnchors = new Dictionary<int, Transform>();
        Dictionary<int, Dictionary<int, TableEffect>> cardBuffEffects = new Dictionary<int, Dictionary<int, TableEffect>>();
        #endregion
    }
    class AnimationTreeNode
    {
        public EventArg eventArg;
        public EventAnimation anim;
        public EventEffectAnim skinAnim;
        public EventAnimState state;
        public AnimationTreeNode parent;
        public readonly List<AnimationTreeNode> beforeChildren = new List<AnimationTreeNode>();
        public readonly List<AnimationTreeNode> afterChildren = new List<AnimationTreeNode>();
        public Coroutine coroutine;
    }
}
