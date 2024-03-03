using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UnityEngine;
namespace Game.BattleTable
{
    using Arg = MoveCardEventDefine;
    class MoveCardAnim : EventAnimation<Arg>
    {
        #region 公有方法
        public override void init(BattleTableManager table)
        {
            base.init(table);
            _movingCardId = startData.getVar<Card>(EVENTVAR.CARD).id;
        }
        public override IEnumerator startUpdate(BattleTableManager table)
        {
            evaluateBreakDurations(table);

            var cardSnapshot = table.getEventCardSnapshot(startData, EVENTVAR.CARD);
            var from = startData.getVar<Pile>(Arg.VAR_FROM_PILE);
            var to = startData.getVar<Pile>(Arg.VAR_TO_PILE);
            var position = startData.getVar<int>(Arg.VAR_POSITION);

            var startVisible = table.canSeeCard(cardSnapshot.id);
            var cardDesc = table.updateCardDescriptor(cardSnapshot);
            EventData data = new EventData
            {
                startVisible = startVisible,
                card = cardDesc,
                from = from,
                to = to,
                position = position
            };

            var info = getCardMoveInfo(table, cardDesc.id, from);
            _movingCardId = cardDesc.id;
            yield return moveToAnimStart(table, data, info);
        }
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(eventArg, EVENTVAR.CARD);
            if (card == null)
                yield break;
            var from = eventArg.getVar<Pile>(Arg.VAR_FROM_PILE);
            var to = eventArg.getVar<Pile>(Arg.VAR_TO_PILE);
            var position = eventArg.getVar<int>(Arg.VAR_POSITION);

            var startVisible = table.canSeeCard(card.id);
            var cardDesc = table.updateCardDescriptor(card);
            EventData data = new EventData
            {
                startVisible = startVisible,
                card = cardDesc,
                from = from,
                to = to,
                position = position
            };


            var info = getCardMoveInfo(table, card.id, from);
            _movingCardId = card.id;
            yield return moveToAnim(table, data, info);

            if (to != from)
            {
                if (to?.name == PILE.DECK || to?.name == PILE.GRAVE)
                {
                    table.addCardToDeckOrGrave(to, card.id);
                }
            }
        }
        public override IEnumerator canceledUpdate(BattleTableManager table)
        {
            var card = table.getEventCardSnapshot(eventArg, EVENTVAR.CARD);
            if (card == null)
                yield break;
            var from = eventArg.getVar<Pile>(Arg.VAR_FROM_PILE);
            var to = eventArg.getVar<Pile>(Arg.VAR_TO_PILE);
            var startVisible = table.canSeeCard(card);
            var cardDesc = table.updateCardDescriptor(card);
            EventData data = new EventData
            {
                startVisible = startVisible,
                card = cardDesc,
                from = to, // 反过来
                to = from,
                position = card.position
            };

            var info = getCardMoveInfo(table, card.id, to);
            _movingCardId = card.id;
            yield return moveCanceledAnim(table, data, info);
        }
        public override bool continueNextGroup(AnimationGroup nextGroup)
        {
            var parents = getAllParents(eventArg);
            if (_duration >= _breakDuration && nextGroup.All(a => canAnimContinue(a, parents)))
            {
                return true;
            }
            return base.continueNextGroup(nextGroup);
        }
        #endregion

        #region 私有方法

        #region 移动到目标牌堆
        private IEnumerator moveToAnimStart(BattleTableManager table, EventData data, CardMoveInfo info)
        {
            var card = data.card;
            var from = data.from;
            var to = data.to;
            var startVisible = data.startVisible;
            string toName = to?.name;

            // 不应该播放移动动画。
            if (shouldNotPlayAnim(from, to))
                yield break;

            Vector3 startPos = info.startPos;
            if (_moveAnim != null)
            {
                startPos = _moveAnim.getCurrentPosition(table);
            }
            // 播放移动动画。
            CardMoveAnim anim;
            switch (toName)
            {
                case PILE.DECK: // 任意牌堆-->牌库
                case PILE.GRAVE: // 任意牌堆-->墓地
                    anim = new MoveToDeckAnim(card, startPos, from, to, info.cardHeight, startVisible);
                    break;
                case PILE.HAND: // 任意牌堆-->手牌
                    anim = new MoveToHandAnim(card, startPos, from, to, data.position, startVisible);
                    break;
                case PILE.FIELD: // 任意牌堆-->战场
                    anim = new MoveToFieldAnim(card, startPos, from, to, data.position, info.cardHeight, startVisible);
                    break;
                case PILE.HEAD: // 任意牌堆-->延迟
                    anim = new MoveToHeadAnim(card, startPos, from, to, data.position, startVisible);
                    break;
                case PILE.ITEM: // 任意牌堆-->道具
                    anim = new MoveToItemAnim(card, startPos, from, to, startVisible);
                    break;
                default: // 任意牌堆-->无牌堆
                    anim = new MoveToNullAnim(card, startPos, from, to, startVisible);
                    break;
            }
            if (anim != null)
            {
                _moveAnim = anim;
                yield return _moveAnim.startUpdate(table, _deltaTime);
            }
        }
        private IEnumerator moveToAnim(BattleTableManager table, EventData data, CardMoveInfo info)
        {
            var card = data.card;
            var from = data.from;
            var to = data.to;
            var startVisible = data.startVisible;
            string toName = to?.name;

            Vector3 startPos = info.startPos;
            if (_moveAnim != null)
            {
                startPos = _moveAnim.getCurrentPosition(table);
            }
            // 不应该播放移动动画。
            if (shouldNotPlayAnim(from, to))
                yield break;

            // 播放移动动画。
            CardMoveAnim anim = null;
            switch (toName)
            {
                case PILE.DECK: // 任意牌堆-->牌库
                case PILE.GRAVE: // 任意牌堆-->墓地
                    if (_moveAnim is not MoveToDeckAnim)
                    {
                        anim = new MoveToDeckAnim(card, startPos, from, to, info.cardHeight, startVisible);
                    }
                    break;
                case PILE.HAND: // 任意牌堆-->手牌
                    if (_moveAnim is not MoveToHandAnim)
                    {
                        anim = new MoveToHandAnim(card, startPos, from, to, data.position, startVisible);
                    }
                    break;
                case PILE.FIELD: // 任意牌堆-->战场
                    if (_moveAnim is not MoveToFieldAnim)
                    {
                        anim = new MoveToFieldAnim(card, startPos, from, to, data.position, info.cardHeight, startVisible);
                    }
                    break;
                case PILE.HEAD: // 任意牌堆-->延迟
                    if (_moveAnim is not MoveToHeadAnim)
                    {
                        anim = new MoveToHeadAnim(card, startPos, from, to, data.position, startVisible);
                    }
                    break;
                case PILE.ITEM: // 任意牌堆-->道具
                    if (_moveAnim is not MoveToItemAnim)
                    {
                        anim = new MoveToItemAnim(card, startPos, from, to, startVisible);
                    }
                    break;
                default: // 任意牌堆-->无牌堆
                    if (_moveAnim is not MoveToNullAnim)
                    {
                        anim = new MoveToNullAnim(card, startPos, from, to, startVisible);
                    }
                    break;
            }
            if (anim != null)
            {
                _moveAnim = anim;
            }
            if (_moveAnim != null)
            {
                yield return _moveAnim.logicUpdate(table, _deltaTime);
            }
        }
        private IEnumerator moveCanceledAnim(BattleTableManager table, EventData data, CardMoveInfo info)
        {
            var card = data.card;
            var from = data.from;
            var to = data.to;
            var startVisible = data.startVisible;
            string toName = to?.name;

            // 不应该播放移动动画。
            if (shouldNotPlayAnim(from, to))
                yield break;

            Vector3 startPos = info.startPos;
            if (_moveAnim != null)
            {
                startPos = _moveAnim.getCurrentPosition(table);
            }
            // 播放移动动画。
            CardMoveAnim anim;
            switch (toName)
            {
                case PILE.DECK: // 牌库-->任意牌堆
                case PILE.GRAVE: // 墓地-->任意牌堆
                    anim = new MoveToDeckAnim(card, startPos, from, to, info.cardHeight, startVisible);
                    break;
                case PILE.HAND: // 手牌-->任意牌堆
                    anim = new MoveToHandAnim(card, startPos, from, to, data.position, startVisible);
                    break;
                case PILE.FIELD: // 战场-->任意牌堆
                    anim = new MoveToFieldAnim(card, startPos, from, to, data.position, info.cardHeight, startVisible);
                    break;
                case PILE.HEAD: // 延迟-->任意牌堆
                    anim = new MoveToHeadAnim(card, startPos, from, to, data.position, startVisible);
                    break;
                case PILE.ITEM: // 道具-->任意牌堆
                    anim = new MoveToItemAnim(card, startPos, from, to, startVisible);
                    break;
                default: // 无牌堆-->任意牌堆
                    anim = new MoveToNullAnim(card, startPos, from, to, startVisible);
                    break;
            }
            if (anim != null)
            {
                _moveAnim = anim;
                yield return _moveAnim.startUpdate(table, _deltaTime);
                yield return _moveAnim.logicUpdate(table, _deltaTime);
            }
        }
        #endregion

        #region 加速
        private void evaluateBreakDurations(BattleTableManager table)
        {
            if (_breakDuration <= _minBreakDuration || _breakDurationEvaluated)
                return;

            var groups = table.getAllAnimGroups();
            var group = table.getAnimGroupOfAnim(this);
            var index = Array.IndexOf(groups, group);
            var moveAnims = groups.Where((g, i) => i >= index).SelectMany(g => g.OfType<MoveCardAnim>()).ToArray();

            var targetBreakTime = getBreakTimeByCount(moveAnims.Length);
            foreach (var move in moveAnims)
            {
                move._breakDurationEvaluated = true;
                move._breakDuration = Mathf.Min(move._breakDuration, targetBreakTime);
            }
        }
        private float getBreakTimeByCount(int count)
        {
            return Mathf.Max(_minBreakDuration, 1f / count);
        }
        #endregion

        #region 移动信息
        private CardMoveInfo getTableUnitMoveInfo(BattleTableManager table, TableUnit unit)
        {
            var lossyScale = table.WorldToTableScale(unit.transform.lossyScale);
            var startHeight = unit.getHeight() * lossyScale.z;
            var worldPos = unit.transform.position;
            var startPos = table.WorldToTablePosition(worldPos);
            return new CardMoveInfo(startPos, startHeight);
        }
        private CardMoveInfo getCardMoveInfo(BattleTableManager table, int cardId, Pile from)
        {
            var fromName = from?.name;
            switch (fromName)
            {
                case PILE.DECK:
                case PILE.GRAVE:
                    {
                        var deck = table.getDeckUI(from);
                        Transform deckTransform = deck.HoverTransform;

                        Vector3 startPos = table.WorldToTablePosition(deckTransform.position);
                        var scale = table.WorldToTableScale(deckTransform.lossyScale);
                        float startHeight = deck.getCardHeight() * scale.z;

                        return new CardMoveInfo(startPos, startHeight);
                    }
                case PILE.HAND:
                    {
                        if (table.tryGetHand(cardId, out var hand))
                            return getTableUnitMoveInfo(table, hand);
                        else if (table.tryGetServant(cardId, out var servant))
                            // 如果能获取到这张手牌对应的仆从，就获取仆从。
                            // 使用卡牌动画在使用仆从时，会先将仆从置入战场，然后才会播放插入的事件动画。
                            return getTableUnitMoveInfo(table, servant);

                        var handList = from.owner == table.player ? table.table.SelfHandList : table.table.EnemyHandList;
                        var worldPos = handList.transform.position;
                        var startPos = table.WorldToTablePosition(worldPos);
                        return new CardMoveInfo(startPos, -1);
                    }
                case PILE.FIELD:
                    {
                        if (table.tryGetServant(cardId, out var servant))
                            return getTableUnitMoveInfo(table, servant);
                    }
                    break;
                case PILE.ITEM:
                    if (table.tryGetItem(cardId, out var item))
                        return getTableUnitMoveInfo(table, item);
                    break;
                case PILE.HEAD:
                    if (table.tryGetSecret(cardId, out var secret))
                        return getTableUnitMoveInfo(table, secret);
                    break;
            }
            return new CardMoveInfo(table.WorldToTablePosition(table.table.CenterDisplayTransform.position), 0);
        }
        #endregion

        private bool shouldNotPlayAnim(Pile from, Pile to)
        {
            string fromName = from?.name;
            string toName = to?.name;
            if (toName == PILE.GRAVE)
            {
                var parentDefine = eventArg.parent?.define;
                if ((fromName == PILE.FIELD || fromName == PILE.ITEM) && parentDefine is DeathEventDefine) // 阻止死亡/摧毁后播放置入墓地的动画
                    return true;
                if (fromName == PILE.HEAD && parentDefine is SecretTriggerEventDefine) // 阻止陷阱触发后播放动画
                    return true;
                if (fromName == PILE.HAND && parentDefine is DiscardEventDefine) // 阻止弃牌后播放移动动画。
                    return true;
                if (fromName == PILE.WARP) // 不播放法术从暂存区置入墓地的动画
                    return true;
            }
            return false;
        }
        private bool canAnimContinue(TableAnimation anim, IEnumerable<IEventArg> parents)
        {
            if (anim is MoveCardAnim move)
            {
                if (move._movingCardId == _movingCardId) // 不能加速当前卡牌的移动。
                {
                    return false;
                }
                IEventArg parent = move.eventArg;
                while (parent != null)
                {
                    if (parents.Contains(parent))
                        return true;
                    parent = parent.parent;
                }
            }
            else if (anim.isInstantAnimation())
            {
                return true;
            }
            return false;
        }
        private IEventArg[] getAllParents(IEventArg arg)
        {
            List<IEventArg> parents = new List<IEventArg>();
            var parent = arg;
            while (parent != null)
            {
                parents.Add(parent);
                parent = parent.parent;
            }
            return parents.ToArray();
        }
        #endregion

        #region 属性字段
        private const float _minBreakDuration = 0.05f;
        private float _breakDuration = 2f;
        private bool _breakDurationEvaluated;
        private int _movingCardId;
        private CardMoveAnim _moveAnim;
        #endregion

        #region 内嵌类
        private struct EventData
        {
            public bool startVisible;
            public CardDescriptor card;
            public Pile from;
            public Pile to;
            public int position;
        }
        private struct CardMoveInfo
        {
            public CardMoveInfo(Vector3 startPos, float cardHeight)
            {
                this.startPos = startPos;
                this.cardHeight = cardHeight;
            }
            public Vector3 startPos;
            public float cardHeight;
        }
        #endregion
    }
}
