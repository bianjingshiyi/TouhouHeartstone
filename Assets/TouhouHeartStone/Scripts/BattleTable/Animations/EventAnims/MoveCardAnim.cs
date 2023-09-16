using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Object = UnityEngine.Object;
using UDeck = Game.BattleTable.Deck;
namespace Game.BattleTable
{
    using Arg = THHPile.MoveCardEventArg;
    class MoveCardAnim : EventAnimation<Arg>
    {
        CardSnapshot _card;
        Pile _from;
        Pile _to;
        int _position;
        float _breakDuration = 2f;
        bool _breakDurationEvaluated;
        private const float _minBreakDuration = 0.05f;
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            evaluateBreakDurations(table);
            yield return pauseWhileNotCompleted();
            yield return stopWhenCanceled();


            _from = arg.from;
            _to = arg.to;
            _position = arg.position;

            _card = table.getRecordCardSnapshot(record, EVENTVAR.CARD);
            string fromName = _from?.name;
            string toName = _to?.name;

            if (_card == null)
                yield break;

            switch (fromName)
            {
                case PILE.INIT:
                    if (toName == PILE.HAND)
                    {
                        //初始手牌
                        table.createHand(_card, _position);
                    }
                    break;
                case PILE.DECK:
                    yield return moveFromDeck(table);
                    break;
                case PILE.HAND:
                    yield return moveFromHand(table);
                    break;
                case PILE.GRAVE:
                    yield return moveFromGrave(table);
                    break;
                case PILE.FIELD:
                    yield return moveFromField(table, arg);
                    break;
                case PILE.ITEM:
                    yield return moveFromItem(table, arg);
                    break;
                case PILE.HEAD:
                    yield return moveFromHead(table, arg);
                    break;
                case PILE.WARP:
                    yield return moveFromWarp(table);
                    break;
                default:
                    yield return moveFromNull(table);
                    break;
            }
        }

        #region 源牌堆
        private IEnumerator moveFromDeck(BattleTableManager table)
        {
            if (_from == _to) // 移动到自己的牌库时不播放动画
            {
                yield break;
            }
            bool fromSelf = _from == table.player.getDeck();
            UDeck deck = fromSelf ? table.table.SelfDeck : table.table.EnemyDeck;
            Transform deckTransform = deck.HoverTransform;

            Vector3 startPos = table.WorldToTablePosition(deckTransform.position);
            var lossyScale = table.WorldToTableScale(deckTransform.lossyScale);
            float startHeight = deck.getCardHeight() * lossyScale.z;
            yield return moveToAnim(table, startPos, startHeight);
        }
        private IEnumerator moveFromHand(BattleTableManager table)
        {
            bool fromSelf = _from?.owner == table.player;

            float startHeight;
            Vector3 worldPos;
            if (table.tryGetHand(_card, out var h))
            {
                var lossyScale = table.WorldToTableScale(h.transform.lossyScale);
                startHeight = h.card.height * lossyScale.z;
                worldPos = h.transform.position;
                // 移除原先手牌UI。
                table.removeHand(_card);
            }
            else
            {
                startHeight = -1;
                worldPos = (fromSelf ? table.table.SelfHandList : table.table.EnemyHandList).transform.position;
            }
            var startPos = table.WorldToTablePosition(worldPos);
            // 为目标牌堆创建UI。
            yield return moveToAnim(table, startPos, startHeight);
        }
        private IEnumerator moveFromGrave(BattleTableManager table)
        {
            if (_from == _to) // 移动到自己的墓地时不播放动画
            {
                yield break;
            }
            bool fromSelf = _from == table.player.getGrave();
            UDeck grave = fromSelf ? table.table.SelfGrave : table.table.EnemyGrave;
            Transform graveTransform = grave.HoverTransform;

            Vector3 startPos = table.WorldToTablePosition(graveTransform.position);
            var lossyScale = table.WorldToTableScale(graveTransform.lossyScale);
            float startHeight = grave.getCardHeight() * lossyScale.z;

            yield return moveToAnim(table, startPos, startHeight);
        }
        private IEnumerator moveFromField(BattleTableManager table, Arg arg)
        {
            string toName = _to?.name;
            bool fromSelf = _from?.owner == table.player;
            bool toSelf = _to?.owner == table.player;
            switch (toName)
            {
                case PILE.FIELD: // 战场-->战场
                    {
                        //获得控制权
                        TableAnimation anim = new FieldMoveAnim(_card.id, toSelf ? table.table.SelfField : table.table.EnemyField, _position);
                        while (anim != null && !anim.update(table, _deltaTime))
                            yield return null;
                    }
                    break;
                case PILE.HAND: // 战场-->手牌
                    {
                        //获得控制权
                        TableAnimation anim = new FieldToHandAnim(_card.id, _position);
                        while (anim != null && !anim.update(table, _deltaTime))
                            yield return null;
                    }
                    break;
                default:
                    {
                        if (toName == PILE.GRAVE && arg.parent is THHCard.DeathEventArg) // 阻止死亡时播放置入墓地的动画
                        {
                            break;
                        }
                        Transform fieldTransform = fromSelf ? table.table.SelfField.transform : table.table.SelfField.transform;
                        Vector3 worldPos = fieldTransform.position;
                        float startHeight = -1;
                        if (table.tryGetServant(_card, out var servant))
                        {
                            worldPos = servant.transform.position;
                            var lossyScale = table.WorldToTableScale(servant.transform.lossyScale);
                            startHeight = servant.getHeight() * lossyScale.z;
                            table.removeServant(_card);
                        }
                        var startPos = table.WorldToTablePosition(worldPos);
                        yield return moveToAnim(table, startPos, startHeight);
                    }
                    break;
            }
        }
        private IEnumerator moveFromItem(BattleTableManager table, Arg arg)
        {
            string toName = _to?.name;
            table.updateItemMaster(_card, record);
            if (toName == PILE.GRAVE && arg.parent is THHCard.DeathEventArg) // 阻止摧毁后播放置入墓地的动画
            {
                yield break;
            }
            bool fromSelf = _from?.owner == table.player;

            var item = fromSelf ? table.table.SelfItem : table.table.EnemyItem;
            Vector3 startPos = table.WorldToTablePosition(item.transform.position);
            var lossyScale = table.WorldToTableScale(item.transform.lossyScale);
            float startHeight = item.getHeight() * lossyScale.z;

            if (table.tryGetItem(_card, out _))
            {
                table.removeItem(_card);
            }

            yield return moveToAnim(table, startPos, startHeight);
        }
        private IEnumerator moveFromHead(BattleTableManager table, Arg arg)
        {
            string toName = _to?.name;
            if (toName == PILE.GRAVE && arg.parent is THHCard.SecretTriggerEventArg) // 阻止陷阱触发后播放动画
            {
                if (table.tryGetSecret(_card, out _))
                {
                    table.removeSecret(_card);
                }
                yield break;
            }
            bool fromSelf = _from?.owner == table.player;

            Vector3 worldPos = (fromSelf ? table.table.SelfMaster : table.table.EnemyMaster).transform.position;
            float startHeight = 0;
            if (table.tryGetSecret(_card, out var secret))
            {
                worldPos = secret.transform.position;
                SimpleAnim exitAnim = secret.onSecretExit;
                if (_card.define.hasKeyword(KEYWORD.MISSION))
                {
                    exitAnim = secret.onMissionExit;
                }
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, secret.onSecretExit, secret.animator, _deltaTime))
                    yield return null;
                table.removeSecret(_card);
            }
            var startPos = table.WorldToTablePosition(worldPos);
            yield return moveToAnim(table, startPos, startHeight);
        }
        private IEnumerator moveFromWarp(BattleTableManager table)
        {
            string toName = _to?.name;
            if (toName == PILE.GRAVE) // 不播放置入墓地的动画
            {
                yield break;
            }
            var startPos = table.WorldToTablePosition(table.table.transform.position);
            yield return moveToAnim(table, startPos, 0);
        }
        private IEnumerator moveFromNull(BattleTableManager table)
        {
            var startPos = table.WorldToTablePosition(table.table.transform.position);
            yield return moveToAnim(table, startPos, 0);
        }
        #endregion
        #region 目标牌堆
        private IEnumerator moveToAnim(BattleTableManager table, Vector3 startPos, float startHeight)
        {
            string fromName = _from?.name;
            string toName = _to?.name;
            bool fromSelf = _from?.owner == table.player;
            bool toSelf = _to?.owner == table.player;
            bool onlySelfKnows = fromSelf && (fromName == PILE.HAND || fromName == PILE.DECK || fromName == PILE.GRAVE || fromName == PILE.HEAD); //只有自己能掌握的信息
            bool sendToSelf = toSelf && (toName == PILE.HAND || toName == PILE.DECK || toName == PILE.GRAVE || toName == PILE.HEAD); //转移之后自己可以掌握的信息
            bool sharedInfo = fromName == PILE.FIELD || fromName == PILE.ITEM; // 双方玩家共享的信息。
            bool known = onlySelfKnows || sendToSelf || sharedInfo;
            switch (toName)
            {
                case PILE.DECK: // 任意牌堆-->牌库
                    {
                        var deck = toSelf ? table.table.SelfDeck : table.table.EnemyDeck;
                        var targetTransform = deck.HoverTransform;
                        var targetHeight = deck.getCardHeight();
                        yield return createCardToDeck(table, _card, startPos, targetTransform, targetHeight, startHeight, known);
                    }
                    break;
                case PILE.HAND: // 任意牌堆-->手牌
                    yield return moveToHand(table, startPos);
                    break;
                case PILE.HEAD: // 任意牌堆-->延迟
                    yield return moveToHead(table);
                    break;
                case PILE.FIELD: // 任意牌堆-->战场
                    yield return moveToField(table, startPos);
                    break;
                case PILE.ITEM: // 任意牌堆-->道具
                    yield return moveToItem(table, record);
                    break;
                case PILE.GRAVE: // 任意牌堆-->墓地
                    {
                        var targetGrave = toSelf ? table.table.SelfGrave : table.table.EnemyGrave;
                        var targetTransform = targetGrave.HoverTransform;
                        var targetHeight = targetGrave.getCardHeight();
                        yield return createCardToDeck(table, _card, startPos, targetTransform, targetHeight, startHeight, known);
                    }
                    break;
            }
        }
        private IEnumerator moveToField(BattleTableManager table, Vector3 startPos)
        {
            if (table.tryGetServant(_card, out _))
                yield break;
            string fromName = _from?.name;
            int position = _position;
            bool toSelf = _to?.owner == table.player;
            if (fromName == PILE.HAND)
            {
                var desc = table.updateCardDescriptor(_card);
                var moveAnim = new HandToFieldAnim(startPos, desc, position);
                while (!moveAnim.update(table, _deltaTime))
                    yield return null;
            }
            else
            {
                if (!table.tryGetServant(_card, out var servant))
                    servant = table.createServant(_card, _position);
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, servant.onSummon, servant.animator, _deltaTime))
                {
                    yield return null;
                }
            }
        }
        private IEnumerator moveToItem(BattleTableManager table, EventRecord record)
        {
            var item = _to.owner == table.player ? table.table.SelfItem : table.table.EnemyItem;
            item.setExist(true);
            item.setDeath(false);
            table.setItem(item, _card);
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, item.onEquip, item.animator, _deltaTime))
            {
                yield return null;
            }
            table.updateItemMaster(_card, record);
        }
        /// <summary>
        /// 奥秘入场
        /// </summary>
        /// <param name="table"></param>
        /// <param name="_card"></param>
        /// <returns></returns>
        private IEnumerator moveToHead(BattleTableManager table)
        {
            if (!table.tryGetSecret(_card, out var secret))
            {
                secret = table.createSecret(_card);
            }

            SimpleAnim enterAnim = secret.onSecretEnter;
            if (_card.define.hasKeyword(KEYWORD.MISSION))
            {
                enterAnim = secret.onMissionEnter;
            }
            table.setSecret(secret, _card);
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, enterAnim, secret.animator, _deltaTime))
                yield return null;
        }
        private IEnumerator moveToHand(BattleTableManager table, Vector3? startPos)
        {
            if (!table.tryGetHand(_card, out _))
            {
                var hand = table.createHand(_card);
                var positionLerp = hand.positionLerp;
                if (startPos.HasValue)
                {
                    var worldPos = table.TableToWorldPosition(startPos.Value);
                    hand.transform.position = worldPos;
                    positionLerp.setStart(worldPos);
                    positionLerp.setPosition(worldPos);
                }
                else
                {
                    positionLerp.setStart();
                }
                positionLerp.setTarget(hand.transform);
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, _deltaTime))
                {
                    yield return null;
                }
            }
        }
        #endregion
        private IEnumerator createCardToDeck(BattleTableManager table, CardSnapshot card, Vector3 startPos, Transform target, float targetHeight, float startHeight = -1, bool isFaceUp = true)
        {
            //创建卡牌
            var cardUI = table.createCard(card, startPos, startHeight, isFaceUp);
            //播放动画
            var targetLocalScale = Vector3.one * (targetHeight / cardUI.height);
            var anim = new CardTransAnim(cardUI, target, targetLocalScale, table.fieldToDeckCurve);
            table.sounds.playSE(table.table.playCardSound);
            while (!anim.update(table, _deltaTime))//没放完动画就让它接着放
                yield return null;
            //摧毁卡牌
            Object.Destroy(cardUI.gameObject);
            //放完了
        }

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

        public override bool blockAnimInGroup(TableAnimation other)
        {
            return _duration < _breakDuration;
        }
        public override IEnumerable<TableAnimation> getGroupedAnimations(IEnumerable<TableAnimation> following)
        {
            return following.OfType<MoveCardAnim>().Where(a => a.tEventArg.parent == tEventArg.parent);
        }
    }
}
