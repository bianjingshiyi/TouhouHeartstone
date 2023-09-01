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
        private void checkCardSkinEffectAnimations(IEventArg arg)
        {
            if (arg is EffectTriggerEventArg)
                return;
            var effectTriggerArg = getEffectEventTriggerArg(arg);
            if (effectTriggerArg == null)
                return;
            var skinAnimRef = getEventSkinAnimRef(arg, effectTriggerArg);
            if (skinAnimRef == null)
                return;
            var animationDef = gameManager.getAnimationDefine(skinAnimRef.animRef);
            var sourceCard = getEventSkinAnimCard(effectTriggerArg, arg, skinAnimRef.sourcePosition);
            var targetCard = getEventSkinAnimCard(effectTriggerArg, arg, skinAnimRef.targetPosition);
            if (animationDef != null)
            {
                addAnim(new GameTableAnim(animationDef, sourceCard, targetCard));
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
        public EffectNodeAnimationRef getEventSkinAnimRef(IEventArg arg, EffectTriggerEventArg effectArg)
        {
            var card = effectArg.card;
            var buff = effectArg.buff;
            EffectNodeAnimationRef animRef = null;
            if (buff is GeneratedBuff genBuff)
            {
                var define = game.getBuffDefine(genBuff.defineRef);
                var skin = cardManager.getBuffSkin(define);
                if (define is GeneratedBuffDefine genBuffDef && effectArg.effect is GeneratedEffect genEffect)
                {
                    var effectIndex = genBuffDef.effectList.IndexOf(genEffect);
                    var node = genEffect.graph.findNodeById(arg.flowNodeId);
                    if (node != null && isCorrectEventForNode(arg, node))
                    {
                        animRef = skin.getAnimRef(effectIndex, arg.flowNodeId);
                    }
                }
            }
            else if (card != null)
            {
                var define = card.define;
                var skin = cardManager.getCardSkin(define);
                if (effectArg.effect is GeneratedEffect genEffect)
                {
                    var effectIndex = Array.IndexOf(define.getGeneratedEffects(), genEffect);
                    var node = genEffect.graph.findNodeById(arg.flowNodeId);
                    if (node != null && isCorrectEventForNode(arg, node))
                    {
                        animRef = skin.getAnimRef(effectIndex, arg.flowNodeId);
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
                    if (arg is ITargetEventArg targetEventArg)
                    {
                        return targetEventArg.getTarget() as Card;
                    }
                    break;
            }
            return null;
        }
        private bool isCorrectEventForNode(IEventArg arg, Node node)
        {
            if (node is ActionNode action)
            {
                return EffectEditorLibrary.getActionBindedEventType(action.defineRef).IsAssignableFrom(arg.GetType());
            }
            return false;
        }
        #endregion
        [SerializeField]
        List<TableAnimation> _animationCache = new List<TableAnimation>();
        [SerializeField]
        List<TableAnimation> _animationQueue = new List<TableAnimation>();
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
        public TableAnimation[] getAnimQueue()
        {
            return _animationQueue.ToArray();
        }
        public void updateAnim()
        {
            if (_animationQueue.Count > 0)
            {
                for (int i = 0; i < _animationQueue.Count; i++)
                {
                    TableAnimation anim = _animationQueue[i];
                    bool isBlocked = false;
                    if (i == 0)
                    {
                        //第一个永远不被阻挡
                    }
                    else
                    {
                        for (int j = 0; j < i; j++)
                        {
                            TableAnimation prevAnim = _animationQueue[j];
                            if (prevAnim.blockAnim(anim))
                            {
                                isBlocked = true;
                                break;
                            }
                        }
                    }
                    if (isBlocked)
                        //continue;
                        break; // 一个动画被阻挡之后，之后的都肯定会被阻挡。
                    try
                    {
                        if (anim.update(this, Time.deltaTime))
                        {
                            _animationQueue.RemoveAt(i);
                            i--;
                        }
                    }
                    catch (Exception e)
                    {
                        _animationQueue.RemoveAt(i);
                        i--;
                        Debug.LogError("动画" + anim + "播放异常被跳过：" + e);
                    }
                }
                if (_animationQueue.Count < 1)
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
        #endregion
    }
}
