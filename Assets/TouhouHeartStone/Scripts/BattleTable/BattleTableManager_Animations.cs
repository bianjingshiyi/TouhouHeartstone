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
using Coffee.UIExtensions;

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
                if (baseType.GetGenericTypeDefinition() != typeof(UIAnimation<>) &&
                    baseType.GetGenericTypeDefinition() != typeof(EventAnimation<>) &&
                    baseType.GetGenericTypeDefinition() != typeof(RequestAnimation<>))
                    continue;
                Type paraType = baseType.GetGenericArguments()[0];
                if (!paraType.IsSubclassOf(typeof(EventArg)) &&
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
                        UberDebug.LogWarningChannel("UI", "存在冲突的动画" + animType);
                        break;
                    }
                    animConstructorDic.Add(eventType, constructor);
                    break;
                }
            }
        }
        public void setEventAnim<TEvent, TAnim>() where TEvent : IEventArg where TAnim : UIAnimation
        {
            setAnim(typeof(TEvent), typeof(TAnim));
        }
        public void setRequestAnim<TRequest, TAnim>() where TRequest : IRequest where TAnim : UIAnimation
        {
            setAnim(typeof(TRequest), typeof(TAnim));
        }
        /// <summary>
        /// 获取事件对应的动画
        /// </summary>
        /// <param name="eventArg"></param>
        /// <returns></returns>
        public TableAnimation getEventAnim(IEventArg eventArg)
        {
            Type type = eventArg.GetType();
            if (animConstructorDic.ContainsKey(type))
            {
                TableAnimation anim = null;
                if (animConstructorDic[type].GetParameters().Length == 0)
                    anim = animConstructorDic[type].Invoke(new object[0]) as TableAnimation;
                else if (animConstructorDic[type].GetParameters().Length == 1)
                    anim = animConstructorDic[type].Invoke(new object[] { eventArg }) as TableAnimation;
                if (anim is EventAnimation tAnim)
                {
                    tAnim.eventArg = eventArg;
                }
                anim.init(this);
                return anim;
            }
            else
                return null;
        }
        public RequestAnimation getRequestAnim(IRequest request)
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
        public void createDeadCardAnchor(Card card, Vector3 position)
        {
            var name = cardManager.getCardSkin(card.define)?.name ?? card.ToString();
            var cardId = card.id;
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
            transform.name = name;
            transform.position = position;
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
        private void checkCardSkinEffectAnimations(IEventArg arg)
        {
            if (arg is EffectTriggerEventArg effectTriggerEventArg)
            {
                checkCardSkinEffectEntryAnimation(effectTriggerEventArg);
                return;
            }
            var effectTriggerArg = getEffectEventTriggerArg(arg);
            if (effectTriggerArg == null)
                return;
            var skinAnimRef = getEventSkinAnimRef(arg, effectTriggerArg);
            if (skinAnimRef == null)
                return;
            var animationDef = gameManager.getAnimationDefine(skinAnimRef.animRef);
            if (animationDef != null)
            {
                addAnim(new EventEffectAnim(animationDef, arg, effectTriggerArg, skinAnimRef));
            }
        }
        private void checkCardSkinEffectEntryAnimation(EffectTriggerEventArg arg)
        {
            if (arg == null)
                return;
            var skinAnimRef = getEffectEntrySkinAnimRef(arg);
            if (skinAnimRef == null)
                return;
            var animationDef = gameManager.getAnimationDefine(skinAnimRef.animRef);
            if (animationDef != null)
            {
                addAnim(new EventEffectAnim(animationDef, arg, arg, skinAnimRef));
            }
        }
        public EffectTriggerEventArg getEffectEventTriggerArg(IEventArg arg)
        {
            var seekArg = arg;
            while (seekArg != null)
            {
                if (seekArg is EffectTriggerEventArg effectArg)
                {
                    return effectArg;
                }
                seekArg = seekArg.parent;
            }
            return null;
        }
        public SkinAnimationRef getEventSkinAnimRef(IEventArg arg, EffectTriggerEventArg effectArg)
        {
            var card = effectArg.card;
            var buff = effectArg.buff;
            SkinAnimationRef animRef = null;
            if (buff is GeneratedBuff genBuff)
            {
                var define = game.getBuffDefine(genBuff.defineRef);
                var skin = cardManager.getBuffSkin(define);
                if (define is GeneratedBuffDefine genBuffDef && effectArg.effect is GeneratedEffect genEffect)
                {
                    var node = genEffect.graph.findNodeById(arg.flowNodeId);
                    if (node != null && isCorrectEventForNode(arg, node))
                    {
                        var key = node.getConst<string>(SkinAnimationRef.NODE_CONST_KEY);
                        animRef = skin.getAnimRef(key);
                    }
                }
            }
            else if (card != null)
            {
                var define = card.define;
                var skin = cardManager.getCardSkin(define);
                if (effectArg.effect is GeneratedEffect genEffect)
                {
                    var node = genEffect.graph.findNodeById(arg.flowNodeId);
                    if (node != null && isCorrectEventForNode(arg, node))
                    {
                        var key = node.getConst<string>(SkinAnimationRef.NODE_CONST_KEY);
                        animRef = skin.getAnimRef(key);
                    }
                }
            }
            return animRef;
        }
        public SkinAnimationRef getEffectEntrySkinAnimRef(EffectTriggerEventArg effectArg)
        {
            var card = effectArg.card;
            var buff = effectArg.buff;
            SkinAnimationRef animRef = null;
            if (buff is GeneratedBuff genBuff)
            {
                var define = game.getBuffDefine(genBuff.defineRef);
                var skin = cardManager.getBuffSkin(define);
                if (define is GeneratedBuffDefine && effectArg.effect is IEntryNodeEffect entryNodeEffect)
                {
                    Node node = entryNodeEffect.entryNode;
                    if (node != null)
                    {
                        var key = node.getConst<string>(SkinAnimationRef.NODE_CONST_KEY);
                        animRef = skin.getAnimRef(key);
                    }
                }
            }
            else if (card != null)
            {
                var define = card.define;
                var skin = cardManager.getCardSkin(define);
                if (effectArg.effect is IEntryNodeEffect entryNodeEffect)
                {
                    var node = entryNodeEffect.entryNode;
                    if (node != null)
                    {
                        var key = node.getConst<string>(SkinAnimationRef.NODE_CONST_KEY);
                        animRef = skin.getAnimRef(key);
                    }
                }
            }
            return animRef;
        }
        public Card getEventSkinAnimCard(EffectTriggerEventArg effectArg, IEventArg arg, int position)
        {
            switch (position)
            {
                case VisualEffectPosition.SELF_MASTER:
                    return effectArg.card.getOwner().getMaster();
                case VisualEffectPosition.ENEMY_MASTER:
                    return game.getOpponent(effectArg.card.getOwner()).getMaster();
                case VisualEffectPosition.THIS_CARD:
                    return effectArg.card;
                case VisualEffectPosition.TARGET_CARD:
                    if (arg is ICardEventArg targetEventArg)
                    {
                        return targetEventArg.getCard() as Card;
                    }
                    break;
            }
            return null;
        }
        private bool isCorrectEventForNode(IEventArg arg, Node node)
        {
            if (node is ActionNode action)
            {
                var type = EffectEditorLibrary.getActionBindedEventType(action.defineRef);
                if (type == null)
                    return false;
                return type.IsAssignableFrom(arg.GetType());
            }
            return false;
        }
        #endregion
        [SerializeField]
        List<TableAnimation> _animationCache = new List<TableAnimation>();
        [SerializeField]
        List<TableAnimation> _animationQueue = new List<TableAnimation>();
        [SerializeField]
        List<AnimationGroup> _animationGroups = new List<AnimationGroup>();
        List<TableAnimation> insertingAnims = new List<TableAnimation>();
        public void addAnim(TableAnimation anim)
        {
            _animationCache.Add(anim);
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
                if (anim is EventAnimation eventAnim)
                {
                    var record = game.triggers.getEventRecord(eventAnim.eventArg, true, true);
                    if (record == null)
                        continue;
                    eventAnim.record = record;
                }
                insertingAnims.Insert(0, anim);
                _animationCache.RemoveAt(i);
            }
            if (insertingAnims.Count > 0)
            {
                _animationQueue.AddRange(insertingAnims);
                insertingAnims.Clear();
            }
        }
        public bool isPlayingAnimation()
        {
            return _animationGroups.Count > 0;
        }
        public AnimationGroup getAnimGroupOfAnim(TableAnimation anim)
        {
            return _animationGroups.FirstOrDefault(g => g.Contains(anim));
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
        HandToFieldAnim _servantPreviewAnim = null;
        Dictionary<int, Transform> deadCardAnchors = new Dictionary<int, Transform>();
        #endregion
    }
}
