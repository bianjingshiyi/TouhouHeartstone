using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UDeck = Game.BattleTable.Deck;
namespace Game.BattleTable
{
    using Arg = THHPile.MoveCardEventArg;
    class MoveCardAnim : EventAnimation<Arg>
    {
        #region 公有方法
        public override IEnumerator logicUpdate(BattleTableManager table, EventRecord record, Arg arg)
        {
            evaluateBreakDurations(table);
            var card = table.getRecordCardSnapshot(record, EVENTVAR.CARD);
            var from = arg.from;
            var to = arg.to;
            var position = arg.position;
            EventData eventData = new EventData
            {
                card = card,
                from = from,
                to = to,
                position = position
            };

            string toName = to?.name;

            if (card == null)
                yield break;

            var info = getCardMoveInfo(table, card, from);
            yield return moveToAnim(table, eventData, info);

            if (toName == PILE.DECK)
                table.addCardToDeck(card);
            else if (toName == PILE.GRAVE)
                table.addCardToGrave(card);
        }
        public override bool blockAnimInGroup(TableAnimation other)
        {
            return _duration < _breakDuration;
        }
        public override IEnumerable<TableAnimation> getGroupedAnimations(IEnumerable<TableAnimation> following)
        {
            return following.OfType<MoveCardAnim>().Where(a => a.tEventArg.parent == tEventArg.parent);
        }
        #endregion

        #region 私有方法

        #region 移动到目标牌堆
        private IEnumerator moveToAnim(BattleTableManager table, EventData data, CardMoveInfo info)
        {
            var from = data.from;
            var to = data.to;
            string fromName = from?.name;
            string toName = to?.name;

            if ((fromName == PILE.DECK || toName == PILE.GRAVE) && from == to)
                // 单个牌库内移动时不播放动画
                yield break;

            // 播放特殊移动动画
            yield return playSpecialMoveAnims(table, data);
            if (_specialAnimPlayed)
                yield break;

            // 移除来源的桌面元素。
            yield return removeSourceTableElement(table, data);

            // 不应该播放移动动画。
            if (shouldNotPlayAnim(from, to))
                yield break;

            // 播放移动动画。
            bool known = isMoveKnown(table, from, to);
            switch (toName)
            {
                case PILE.DECK: // 任意牌堆-->牌库
                case PILE.GRAVE: // 任意牌堆-->墓地
                    yield return moveToDeckOrGrave(table, data, info, known);
                    break;
                case PILE.HAND: // 任意牌堆-->手牌
                    yield return moveToHand(table, data, info);
                    break;
                case PILE.FIELD: // 任意牌堆-->战场
                    yield return moveToField(table, data, info);
                    break;
                case PILE.HEAD: // 任意牌堆-->延迟
                    yield return moveToHead(table, data);
                    break;
                case PILE.ITEM: // 任意牌堆-->道具
                    yield return moveToItem(table, data);
                    break;
            }
        }
        private IEnumerator moveToDeckOrGrave(BattleTableManager table, EventData data, CardMoveInfo info, bool known)
        {
            var card = data.card;
            var to = data.to;

            var targetDeck = getDeckUI(table, to);
            var targetTransform = targetDeck.HoverTransform;
            var targetHeight = targetDeck.getCardHeight();


            var startPos = info.startPos;
            var startHeight = info.cardHeight;
            //创建卡牌
            var cardUI = table.createMovingCard(card, startPos, startHeight, known);

            // 播放音效
            table.sounds.playSE(table.table.playCardSound);

            //播放动画
            var targetLocalScale = Vector3.one * (targetHeight / cardUI.height);
            var anim = new CardTransAnim(cardUI, targetTransform, targetLocalScale, table.fieldToDeckCurve);
            while (!anim.update(table, _deltaTime))//没放完动画就让它接着放
                yield return null;
            //摧毁卡牌
            table.removeMovingCard(card);
            //放完了
        }
        private IEnumerator moveToHand(BattleTableManager table, EventData data, CardMoveInfo info)
        {
            ICardData card = data.card;
            if (!table.tryGetHand(card, out _))
            {
                // 创建移动手牌。
                var hand = table.createHand(card);
                var positionLerp = hand.positionLerp;
                var worldPos = table.TableToWorldPosition(info.startPos);
                hand.transform.position = worldPos;
                positionLerp.setStartPosition(worldPos);
                positionLerp.setPosition(worldPos);
                positionLerp.setTargetTransform(hand.transform);

                // 播放音效
                table.sounds.playSE(table.table.playCardSound);

                // 播放手牌移动动画。
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, _deltaTime))
                {
                    yield return null;
                }
            }
        }
        private IEnumerator moveToField(BattleTableManager table, EventData data, CardMoveInfo info)
        {
            ICardData card = data.card;
            Pile from = data.from;
            int position = data.position;
            if (table.tryGetServant(card, out _))
                yield break;

            string fromName = from?.name;
            // 从手牌进入。
            if (fromName == PILE.HAND)
            {
                // 播放从手牌入场动画。
                var desc = table.updateCardDescriptor(card);
                var moveAnim = new HandToFieldAnim(info.startPos, desc, position);
                while (!moveAnim.update(table, _deltaTime))
                    yield return null;
            }
            else
            {
                // 创建仆从。
                var servant = table.createServant(card, position);

                // 播放召唤动画。
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, servant.onSummon, servant.animator, _deltaTime))
                {
                    yield return null;
                }
            }
        }
        /// <summary>
        /// 奥秘入场
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private IEnumerator moveToHead(BattleTableManager table, EventData data)
        {
            ICardData card = data.card;
            // 创建延迟区物件。
            if (!table.tryGetSecret(card, out var secret))
                secret = table.createSecret(card);
            table.setSecret(secret, card);

            // 播放入场动画。
            SimpleAnim enterAnim = card.define.hasKeyword(KEYWORD.MISSION) ? secret.onMissionEnter : secret.onSecretEnter;
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, enterAnim, secret.animator, _deltaTime))
                yield return null;
        }
        private IEnumerator moveToItem(BattleTableManager table, EventData data)
        {
            var card = data.card;
            var to = data.to;
            // 创建道具。
            var item = getItem(table, to);
            item.setExist(true);
            item.setDeath(false);
            table.setItem(item, card);

            // 播放入场动画。
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, item.onEquip, item.animator, _deltaTime))
            {
                yield return null;
            }
        }
        #endregion

        #region 加速
        private void evaluateBreakDurations(BattleTableManager table)
        {
            if (_breakDuration <= _minBreakDuration || _breakDurationEvaluated)
                return;

            var anims = table.getAnimGroupOfAnim(this);
            var index = anims.IndexOf(this);
            var moveAnims = anims.Where((a, i) => i > index).OfType<MoveCardAnim>().ToArray();

            var targetBreakTime = getBreakTimeByCount(moveAnims.Length);
            foreach (var move in moveAnims)
            {
                move._breakDurationEvaluated = true;
                move._breakDuration = Mathf.Min(move._breakDuration, targetBreakTime);
            }
        }
        private float getBreakTimeByCount(int count)
        {
            return Mathf.Max(_minBreakDuration, 2f / count);
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
        private CardMoveInfo getCardMoveInfo(BattleTableManager table, ICardData card, Pile from)
        {
            var fromName = from?.name;
            switch (fromName)
            {
                case PILE.DECK:
                case PILE.GRAVE:
                    {
                        var deck = getDeckUI(table, from);
                        Transform deckTransform = deck.HoverTransform;

                        Vector3 startPos = table.WorldToTablePosition(deckTransform.position);
                        var scale = table.WorldToTableScale(deckTransform.lossyScale);
                        float startHeight = deck.getCardHeight() * scale.z;

                        return new CardMoveInfo(startPos, startHeight);
                    }
                case PILE.HAND:
                    {
                        if (table.tryGetHand(card, out var hand))
                            return getTableUnitMoveInfo(table, hand);
                        else if (table.tryGetServant(card, out var servant))
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
                        if (table.tryGetServant(card, out var servant))
                            return getTableUnitMoveInfo(table, servant);
                    }
                    break;
                case PILE.ITEM:
                    if (table.tryGetItem(card, out var item))
                        return getTableUnitMoveInfo(table, item);
                    break;
                case PILE.HEAD:
                    if (table.tryGetSecret(card, out var secret))
                        return getTableUnitMoveInfo(table, secret);
                    break;
            }
            return new CardMoveInfo(table.WorldToTablePosition(table.table.CenterDisplayTransform.position), 0);
        }
        #endregion


        #region 获取牌堆UI
        private UDeck getDeckUI(BattleTableManager table, Pile pile)
        {
            if (pile?.name == PILE.GRAVE)
            {
                return pile.owner == table.player ? table.table.SelfGrave : table.table.EnemyGrave;
            }
            return pile.owner == table.player ? table.table.SelfDeck : table.table.EnemyDeck;
        }
        private Field getField(BattleTableManager table, Pile pile)
        {
            return pile.owner == table.player ? table.table.SelfField : table.table.EnemyField;
        }
        private Item getItem(BattleTableManager table, Pile pile)
        {
            return pile.owner == table.player ? table.table.SelfItem : table.table.EnemyItem;
        }
        #endregion

        #region 已知信息
        private bool isPileKnown(Pile pile)
        {
            string name = pile?.name;
            return name == PILE.HAND || name == PILE.DECK || name == PILE.GRAVE || name == PILE.HEAD;
        }
        private bool isMoveKnown(BattleTableManager table, Pile from, Pile to)
        {
            string fromName = from?.name;
            bool fromSelf = from?.owner == table.player;
            bool toSelf = to?.owner == table.player;
            bool onlySelfKnows = fromSelf && isPileKnown(from); //只有自己能掌握的信息
            bool sendToSelf = toSelf && isPileKnown(to); //转移之后自己可以掌握的信息
            bool sharedInfo = fromName == PILE.FIELD || fromName == PILE.ITEM; // 双方玩家共享的信息。
            return onlySelfKnows || sendToSelf || sharedInfo || table.canSeeAllCards;
        }
        #endregion

        #region 移动动画逻辑
        private IEnumerator playSpecialMoveAnims(BattleTableManager table, EventData data)
        {
            ICardData card = data.card;
            Pile from = data.from;
            Pile to = data.to;
            int position = data.position;
            string fromName = from?.name;
            string toName = to?.name;
            switch (fromName)
            {
                case PILE.INIT:
                    //初始手牌
                    if (toName == PILE.HAND)
                    {
                        table.createHand(card, position);
                        _specialAnimPlayed = true;
                    }
                    break;
                case PILE.FIELD:
                    switch (toName)
                    {
                        case PILE.FIELD: // 战场-->战场
                            {
                                //获得控制权
                                TableAnimation anim = new FieldMoveAnim(card.id, getField(table, to), position);
                                while (anim != null && !anim.update(table, _deltaTime))
                                    yield return null;
                                _specialAnimPlayed = true;
                            }
                            break;
                        case PILE.HAND: // 战场-->手牌
                            {
                                // 回手
                                table.updateCardDescriptor(card);
                                TableAnimation anim = new FieldToHandAnim(card.id, position);
                                while (anim != null && !anim.update(table, _deltaTime))
                                    yield return null;
                                _specialAnimPlayed = true;
                            }
                            break;
                    }
                    break;
            }
        }
        private IEnumerator removeSourceTableElement(BattleTableManager table, EventData data)
        {
            ICardData card = data.card;
            Pile from = data.from;
            Pile to = data.to;
            string fromName = from?.name;
            string toName = to?.name;
            switch (fromName)
            {
                case PILE.DECK:
                    table.removeCardFromDeck(card);
                    break;
                case PILE.GRAVE:
                    table.removeCardFromGrave(card);
                    break;
                case PILE.HAND:
                    if (table.tryGetHand(card, out _))
                    {
                        table.removeHand(card);
                    }
                    else if (table.tryGetServant(card, out _) && toName != PILE.FIELD)
                    {
                        // 如果能获取到这张手牌对应的仆从，就获取仆从。
                        // 使用卡牌动画在使用仆从时，会先将仆从置入战场，然后才会播放插入的事件动画。
                        table.removeServant(card);
                    }
                    break;
                case PILE.FIELD:
                    {
                        if (table.tryGetServant(card, out _))
                        {
                            table.removeServant(card);
                        }
                    }
                    break;
                case PILE.ITEM:
                    {
                        if (table.tryGetItem(card, out _))
                        {
                            table.removeItem(card);
                        }
                    }
                    break;
                case PILE.HEAD:
                    {
                        if (table.tryGetSecret(card, out var secret))
                        {
                            SimpleAnim exitAnim = card.define.hasKeyword(KEYWORD.MISSION) ? secret.onMissionExit : secret.onSecretExit;
                            AnimAnim anim = null;
                            while (!SimpleAnimHelper.update(table, ref anim, secret.onSecretExit, secret.animator, _deltaTime))
                                yield return null;
                            table.removeSecret(card);
                        }
                    }
                    break;
            }
        }
        private bool shouldNotPlayAnim(Pile from, Pile to)
        {
            string fromName = from?.name;
            string toName = to?.name;
            if (toName == PILE.GRAVE)
            {
                if ((fromName == PILE.FIELD || fromName == PILE.ITEM) && tEventArg.parent is THHCard.DeathEventArg) // 阻止死亡/摧毁后播放置入墓地的动画
                    return true;
                if (fromName == PILE.HEAD && tEventArg.parent is THHCard.SecretTriggerEventArg) // 阻止陷阱触发后播放动画
                    return true;
                if (fromName == PILE.HAND && tEventArg.parent is THHCard.DiscardEventArg) // 阻止弃牌后播放移动动画。
                    return true;
                if (fromName == PILE.WARP) // 不播放法术从暂存区置入墓地的动画
                    return true;
            }
            return false;
        }
        #endregion

        #endregion

        #region 属性字段
        private const float _minBreakDuration = 0.05f;
        private float _breakDuration = 2f;
        private bool _breakDurationEvaluated;
        private bool _specialAnimPlayed;
        #endregion

        #region 内嵌类
        private struct EventData
        {
            public CardSnapshot card;
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
