using UnityEngine;
using BJSYGameCore;
using UI;
using TouhouHeartstone;
using System;
using System.Collections.Generic;
using TouhouCardEngine.Interfaces;
using System.Reflection;
namespace Game
{
    public class TableManager : Manager
    {
        public THHGame game { get; private set; } = null;
        public THHPlayer player { get; private set; } = null;
        [SerializeField]
        Table _table;
        Table table
        {
            get { return _table; }
            set { _table = value; }
        }
        protected override void onAwake()
        {
            base.onAwake();
            if (table == null)
                table = this.findInstance<Table>();
            loadAnim(GetType().Assembly);
        }
        protected void Update()
        {
            updateAnim();
        }
        public void setGame(THHGame game, THHPlayer player)
        {
            table.InitReplaceDialog.hide();
            table.TurnTipImage.hide();
            table.SelfHandList.clearItems();
            table.SelfFieldList.clearItems();
            table.EnemyFieldList.clearItems();
            table.EnemyHandList.clearItems();
            table.AttackArrowImage.hide();
            table.Fatigue.hide();
            _animationQueue.Clear();

            if (game != null)
            {
                game.triggers.onEventBefore -= onEventBefore;
                game.triggers.onEventAfter -= onEventAfter;
            }
            this.game = game;
            if (game != null)
            {
                game.triggers.onEventBefore += onEventBefore;
                game.triggers.onEventAfter += onEventAfter;
            }
            if (player != null)
            {
                table.SelfSkill.asButton.onClick.RemoveAllListeners();
                table.TurnEndButton.onClick.RemoveAllListeners();
            }
            this.player = player;
            //if (player != null)
            //{
            //    table.SelfSkill.asButton.onClick.AddListener(() =>
            //    {
            //        if (selectableTargets != null)
            //            return;
            //        player.cmdUse(game, SelfSkill.card, 0);
            //    });
            //    table.TurnEndButton.onClick.AddListener(() =>
            //    {
            //        player.cmdTurnEnd(game);

            //        //SelfHandList.stopPlacing(true);
            //        resetUse(true, true);
            //        selectableTargets = null;
            //    });
            //}
        }

        private void onEventBefore(IEventArg arg)
        {
            UIAnimation anim = null;
            switch (arg)
            {
                case THHPlayer.InitReplaceEventArg initReplace:
                    anim = new InitReplaceAnimation(initReplace);
                    break;
                case THHGame.StartEventArg start:
                    anim = new StartAnimation(start);
                    break;
                case THHGame.TurnStartEventArg turnStart:
                    anim = new TurnStartAnimation(turnStart);
                    break;
                case THHPlayer.DrawEventArg draw:
                    anim = new DrawAnimation(draw);
                    break;
                case THHPlayer.SetGemEventArg setGem:
                    anim = new SetGemAnimation(setGem);
                    break;
                case THHPlayer.SetMaxGemEventArg setMaxGem:
                    anim = new SetMaxGemAnimation(setMaxGem);
                    break;
                case THHPlayer.MoveEventArg move:
                    anim = new MoveServantAnimation(move);
                    break;
                case THHPlayer.UseEventArg use:
                    if (use.card.define.type == CardDefineType.SERVANT)
                        anim = new UseServantAnimation(use);
                    else if (use.card.define.type == CardDefineType.SPELL)
                        anim = new UseSpellAnimation(use);
                    break;
                case THHCard.HealEventArg heal:
                    anim = new HealAnimation(heal);
                    break;
                //case THHPlayer.CreateTokenEventArg createToken:
                //    anim = new CreateTokenAnimation(createToken);
                //    break;
                case THHCard.AttackEventArg attack:
                    anim = new ServantAttackAnimation(attack);
                    break;
                case THHCard.DamageEventArg damage:
                    anim = new DamageAnimation(damage);
                    break;
                case THHCard.DeathEventArg death:
                    anim = new DeathAnimation(death);
                    break;
                case THHPlayer.ActiveEventArg active:
                    foreach (var target in active.targets)
                    {
                        if (target is TouhouCardEngine.Card card)
                        {
                            anim = new SelectTargetAnimation(active);
                        }
                    }
                    break;
                case THHGame.TurnEndEventArg turnEnd:
                    anim = new TurnEndAnimation(turnEnd);
                    break;
                case THHGame.GameEndEventArg gameEnd:
                    anim = new GameEndAnimation(gameEnd);
                    break;
                default:
                    anim = getEventAnim(arg);
                    break;
            }
            if (anim != null)
                addAnim(anim);
        }

        private void onEventAfter(IEventArg arg)
        {

        }
        #region UI
        public void setMaster(Master master)
        {

        }
        #endregion
        #region Animation
        Dictionary<Type, ConstructorInfo> animConstructorDic { get; } = new Dictionary<Type, ConstructorInfo>();
        public void loadAnim(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;
                Type baseType = type.BaseType;
                if (!baseType.IsGenericType)
                    continue;
                if (baseType.GetGenericTypeDefinition() != typeof(UIAnimation<>))
                    continue;
                Type paraType = baseType.GetGenericArguments()[0];
                if (!paraType.IsSubclassOf(typeof(EventArg)))
                    continue;
                setEventAnim(paraType, type);
            }
        }
        public void setEventAnim(Type eventType, Type animType)
        {
            foreach (ConstructorInfo constructor in animType.GetConstructors())
            {
                var args = constructor.GetParameters();
                if (args.Length < 1 || (args.Length == 1 && args[0].ParameterType == eventType))
                {
                    animConstructorDic.Add(eventType, constructor);
                    break;
                }
            }
        }
        public void setEventAnim<TEvent, TAnim>() where TEvent : IEventArg where TAnim : UIAnimation
        {
            setEventAnim(typeof(TEvent), typeof(TAnim));
        }
        public UIAnimation getEventAnim(IEventArg eventArg)
        {
            Type type = eventArg.GetType();
            if (animConstructorDic.ContainsKey(type))
            {
                if (animConstructorDic[type].GetParameters().Length == 0)
                    return animConstructorDic[type].Invoke(new object[0]) as UIAnimation;
                else if (animConstructorDic[type].GetParameters().Length == 1)
                    return animConstructorDic[type].Invoke(new object[] { eventArg }) as UIAnimation;
                return null;
            }
            else
                return null;
        }
        [SerializeField]
        List<UIAnimation> _animationQueue = new List<UIAnimation>();
        public void addAnim(UIAnimation anim)
        {
            _animationQueue.Add(anim);
        }
        public UIAnimation[] getAnimQueue()
        {
            return _animationQueue.ToArray();
        }
        public void updateAnim()
        {
            if (_animationQueue.Count > 0)
            {
                for (int i = 0; i < _animationQueue.Count; i++)
                {
                    UIAnimation anim = _animationQueue[i];
                    bool isBlocked = false;
                    if (i == 0)
                    {
                        //第一个永远不被阻挡
                    }
                    else
                    {
                        for (int j = 0; j < i; j++)
                        {
                            UIAnimation prevAnim = _animationQueue[j];
                            if (prevAnim.blockAnim(anim))
                            {
                                isBlocked = true;
                                break;
                            }
                        }
                    }
                    if (isBlocked)
                        continue;
                    if (anim.update(table))
                    {
                        _animationQueue.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        #endregion
    }
}