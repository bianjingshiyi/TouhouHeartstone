
using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using Object = UnityEngine.Object;
using UDeck = Game.BattleTable.Deck;
namespace Game.BattleTable
{
    using Arg = Pile.MoveCardEventArg;
    class MoveCardAnim : EventAnimation<Arg>
    {
        CardSnapshot _card;
        Pile _from;
        Pile _to;
        int _position;

        CardSnapshot _fromMaster;
        CardSnapshot _toMaster;

        public override void postEvent(BattleTableManager table, Arg eventArg)
        {
            base.postEvent(table, eventArg);

            if (eventArg.from?.name == PILE.ITEM)
            {
                _fromMaster = snapshotCard(table, eventArg.from.owner.getMaster());
            }
            if (eventArg.to?.name == PILE.ITEM)
            {
                _toMaster = snapshotCard(table, eventArg.to.owner.getMaster());
            }
        }
        public override IEnumerator update(BattleTableManager table, EventRecord record, Arg arg)
        {
            yield return pauseWhileNotCompleted();
            yield return stopWhenCanceled();


            _from = arg.from;
            _to = arg.to;
            _position = arg.position;

            _card = table.getRecordCardSnapshot(record, Arg.VAR_CARD);
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

            Vector3 startPos = deckTransform.position;
            float startHeight = deck.getCardHeight();
            yield return moveToAnim(table, startPos, startHeight);
        }
        private IEnumerator moveFromHand(BattleTableManager table)
        {
            bool fromSelf = _from?.owner == table.player;

            float startHeight;
            Vector3 startPos;
            if (table.tryGetHand(_card, out var h))
            {
                startHeight = h.card.height * h.transform.lossyScale.y;
                startPos = h.transform.position;
                // 移除原先手牌UI。
                table.removeHand(_card);
            }
            else
            {
                startHeight = -1;
                startPos = (fromSelf ? table.table.SelfHandList : table.table.EnemyHandList).transform.position;
            }

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

            Vector3 startPos = graveTransform.position;
            float startHeight = grave.getCardHeight();

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
                        TableAnimation anim = null;
                        if (table.tryGetServant(_card, out var servant))
                            anim = new FieldMoveAnim(table, servant, toSelf ? table.table.SelfField : table.table.EnemyField, _position);

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
                        Vector2 sourcePosition = fieldTransform.position;
                        float startHeight = -1;
                        if (table.tryGetServant(_card, out var servant))
                        {
                            sourcePosition = servant.transform.position;
                            startHeight = servant.getHeight() * servant.transform.lossyScale.y;
                            table.removeServant(_card);
                        }
                        yield return moveToAnim(table, sourcePosition, startHeight);
                    }
                    break;
            }
        }
        private IEnumerator moveFromItem(BattleTableManager table, Arg arg)
        {
            string toName = _to?.name;
            if (toName == PILE.GRAVE && arg.parent is THHCard.DeathEventArg) // 阻止摧毁后播放置入墓地的动画
            {
                yield break;
            }
            bool fromSelf = _from?.owner == table.player;

            var item = fromSelf ? table.table.SelfItem : table.table.EnemyItem;
            Vector3 startPos = item.transform.position;
            float startHeight = item.getHeight() * item.transform.lossyScale.y;

            if (table.tryGetItem(_card, out _))
            {
                table.removeItem(_card);
            }

            yield return moveToAnim(table, startPos, startHeight);
            updateItemMaster(table);
        }
        private IEnumerator moveFromHead(BattleTableManager table, Arg arg)
        {
            string toName = _to?.name;
            bool fromSelf = _from?.owner == table.player;

            Vector2 startPos = (fromSelf ? table.table.SelfMaster : table.table.EnemyMaster).transform.position;
            float startHeight = 0;
            if (table.tryGetSecret(_card, out var secret))
            {
                startPos = secret.transform.position;
                AnimAnim anim = null;
                while (SimpleAnimHelper.update(table, ref anim, secret.onExit, secret.animator, _deltaTime))
                    yield return null;
                table.removeSecret(_card);
            }

            if (toName == PILE.GRAVE && arg.parent is THHCard.SecretTriggerEventArg) // 阻止陷阱触发后播放置入墓地的动画
            {
                yield break;
            }
            yield return moveToAnim(table, startPos, startHeight);
        }
        private IEnumerator moveFromWarp(BattleTableManager table)
        {
            string toName = _to?.name;
            if (toName == PILE.GRAVE) // 不播放置入墓地的动画
            {
                yield break;
            }
            yield return moveToAnim(table, table.table.transform.position, 0);
        }
        private IEnumerator moveFromNull(BattleTableManager table)
        {
            yield return moveToAnim(table, table.table.transform.position, 0);
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
                        yield return createCardToDeck(table, _card, startPos, targetTransform, deck.getCardHeight(), startHeight, known);
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
                    yield return moveToItem(table);
                    break;
                case PILE.GRAVE: // 任意牌堆-->墓地
                    {
                        var targetGrave = toSelf ? table.table.SelfGrave : table.table.EnemyGrave;
                        var targetTransform = targetGrave.HoverTransform;
                        yield return createCardToDeck(table, _card, startPos, targetTransform, targetGrave.getCardHeight(), startHeight, known);
                    }
                    break;
            }
        }
        private IEnumerator moveToField(BattleTableManager table, Vector3 startPos)
        {
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
        private IEnumerator moveToItem(BattleTableManager table)
        {
            var item = _to.owner == table.player ? table.table.SelfItem : table.table.EnemyItem;
            table.setItem(item, _card);
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, item.onEquip, item.animator, _deltaTime))
            {
                yield return null;
            }
            updateItemMaster(table);
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
            if (_card.define.hasKeyword(KEYWORD.MISSION))
            {
                secret.setMission(true);
            }
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, secret.onEnter, secret.animator, _deltaTime))
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
                    var pos = startPos.Value;
                    hand.transform.position = pos;
                    positionLerp.setStart(pos);
                    positionLerp.setPosition(pos);
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


        void updateItemMaster(BattleTableManager table)
        {
            if (_fromMaster != null)
            {
                var masterUI = _fromMaster.getOwner() == table.player ? table.table.SelfMaster : table.table.EnemyMaster;
                table.setMaster(masterUI, _fromMaster);
                _fromMaster = null;
            }
            if (_toMaster != null)
            {
                var masterUI = _toMaster.getOwner() == table.player ? table.table.SelfMaster : table.table.EnemyMaster;
                table.setMaster(masterUI, _toMaster);
                _toMaster = null;
            }
        }
        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (nextAnim is MoveCardAnim moveCard)
            {
                var from = tEventArg.from;
                var to = tEventArg.to;
                var nextFrom = moveCard.tEventArg.from;
                var nextTo = moveCard.tEventArg.to;
                if (from == nextFrom && to == nextTo && _duration >= 0.1f) // 加速所有来源和目标相同的牌堆移动。
                {
                    return false;
                }
            }
            return base.blockAnim(nextAnim);
        }
    }
}
