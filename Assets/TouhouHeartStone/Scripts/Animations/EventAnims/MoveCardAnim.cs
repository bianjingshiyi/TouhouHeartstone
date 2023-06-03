
using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Object = UnityEngine.Object;
using UDeck = UI.Deck;
namespace Game
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

        public override void postEvent(TableManager table, Arg eventArg)
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
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            while (!record.isCompleted)
                yield return null;
            if (record.isCanceled)
                yield break;


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
        private IEnumerator moveFromDeck(TableManager table)
        {
            if (_from == _to) // 移动到自己的牌库时不播放动画
            {
                yield break;
            }
            bool fromSelf = _from == table.player.getDeck();
            UDeck deck = fromSelf ? table.ui.SelfDeck : table.ui.EnemyDeck;
            RectTransform deckTransform = deck.rectTransform;

            Vector3 startPos = deckTransform.position;
            float startHeight = deck.getCardHeight();
            yield return moveToAnim(table, startPos, startHeight);
        }
        private IEnumerator moveFromHand(TableManager table)
        {
            bool fromSelf = _from?.owner == table.player;

            float startHeight;
            Vector3 startPos;
            if (table.tryGetHand(_card, out var h))
            {
                startHeight = h.rectTransform.rect.height * h.rectTransform.lossyScale.y;
                startPos = h.transform.position;
                // 移除原先手牌UI。
                table.removeHand(_card);
            }
            else
            {
                startHeight = -1;
                startPos = (fromSelf ? table.ui.SelfHandList : table.ui.EnemyHandList).rectTransform.position;
            }

            // 为目标牌堆创建UI。
            yield return moveToAnim(table, startPos, startHeight);
        }
        private IEnumerator moveFromGrave(TableManager table)
        {
            if (_from == _to) // 移动到自己的墓地时不播放动画
            {
                yield break;
            }
            bool fromSelf = _from == table.player.getGrave();
            UDeck grave = fromSelf ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck;
            RectTransform graveTransform = grave.rectTransform;

            Vector3 startPos = graveTransform.position;
            float startHeight = grave.getCardHeight();

            yield return moveToAnim(table, startPos, startHeight);
        }
        private IEnumerator moveFromField(TableManager table, Arg arg)
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
                            anim = new FieldMoveAnim(table, servant, toSelf ? table.ui.SelfFieldList : table.ui.EnemyFieldList, _position);

                        while (anim != null && !anim.update(table, _time))
                            yield return null;
                    }
                    break;
                default:
                    {
                        if (toName == PILE.GRAVE && arg.parent is THHCard.DeathEventArg) // 阻止死亡时播放置入墓地的动画
                        {
                            break;
                        }
                        RectTransform fieldTransform = fromSelf ? table.ui.SelfFieldList.rectTransform : table.ui.SelfFieldList.rectTransform;
                        Vector2 sourcePosition = fieldTransform.position;
                        float startHeight = -1;
                        if (table.tryGetServant(_card, out var servant))
                        {
                            sourcePosition = servant.transform.position;
                            startHeight = servant.rectTransform.rect.height * servant.rectTransform.lossyScale.y;
                            table.removeServant(_card);
                        }
                        yield return moveToAnim(table, sourcePosition, startHeight);
                    }
                    break;
            }
        }
        private IEnumerator moveFromItem(TableManager table, Arg arg)
        {
            string toName = _to?.name;
            bool fromSelf = _from?.owner == table.player;

            var item = fromSelf ? table.ui.SelfItem : table.ui.EnemyItem;
            Vector3 startPos = item.rectTransform.position;
            float startHeight = item.rectTransform.rect.height * item.transform.lossyScale.y;

            if (table.tryGetItem(_card, out _))
            {
                table.removeItem(_card);
            }

            if (toName == PILE.GRAVE && arg.parent is THHCard.DeathEventArg) // 阻止摧毁后播放置入墓地的动画
            {
                yield break;
            }
            yield return moveToAnim(table, startPos, startHeight);
            updateItemMaster(table);
        }
        private IEnumerator moveFromHead(TableManager table, Arg arg)
        {
            string toName = _to?.name;
            bool fromSelf = _from?.owner == table.player;

            Vector2 startPos = (fromSelf ? table.ui.SelfMaster : table.ui.EnemyMaster).transform.position;
            float startHeight = 0;
            if (table.tryGetSecret(_card, out var secret))
            {
                startPos = secret.transform.position;
                AnimAnim anim = null;
                while (SimpleAnimHelper.update(table, ref anim, secret.onExit, secret.animator, _time))
                    yield return null;
                table.removeSecret(_card);
            }

            if (toName == PILE.GRAVE && arg.parent is THHCard.SecretTriggerEventArg) // 阻止陷阱触发后播放置入墓地的动画
            {
                yield break;
            }
            yield return moveToAnim(table, startPos, startHeight);
        }
        private IEnumerator moveFromWarp(TableManager table)
        {
            string toName = _to?.name;
            if (toName == PILE.GRAVE) // 不播放置入墓地的动画
            {
                yield break;
            }
            yield return moveToAnim(table, table.ui.transform.position, 0);
        }
        private IEnumerator moveFromNull(TableManager table)
        {
            yield return moveToAnim(table, table.ui.transform.position, 0);
        }
        #endregion
        #region 目标牌堆
        private IEnumerator moveToAnim(TableManager table, Vector3 startPos, float startHeight)
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
                        var deck = toSelf ? table.ui.SelfDeck : table.ui.EnemyDeck;
                        var targetTransform = deck.rectTransform;
                        var targetSize = deck.bottom.rect.height * deck.bottom.lossyScale;
                        yield return createCardToDeck(table, _card, startPos, targetTransform, targetSize, startHeight, known);
                    }
                    break;
                case PILE.HAND: // 任意牌堆-->手牌
                    yield return moveToHand(table, startPos);
                    break;
                case PILE.HEAD: // 任意牌堆-->延迟
                    yield return moveToHead(table);
                    break;
                case PILE.FIELD: // 任意牌堆-->战场
                    yield return moveToField(table);
                    break;
                case PILE.ITEM: // 任意牌堆-->道具
                    yield return moveToItem(table);
                    break;
                case PILE.GRAVE: // 任意牌堆-->墓地
                    {
                        var targetGrave = toSelf ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck;
                        var targetTransform = targetGrave.rectTransform;
                        var targetSize = targetGrave.bottom.rect.height * targetGrave.bottom.lossyScale;
                        yield return createCardToDeck(table, _card, startPos, targetTransform, targetSize, startHeight, known);
                    }
                    break;
            }
        }
        private IEnumerator moveToField(TableManager table)
        {
            if (!table.tryGetServant(_card, out var servant))
                servant = table.createServant(_card, _position);
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, servant.onSummon, servant.servantAnimator, _time))
            {
                yield return null;
            }
        }
        private IEnumerator moveToItem(TableManager table)
        {
            var item = _to.owner == table.player ? table.ui.SelfItem : table.ui.EnemyItem;
            table.setItem(item, _card);
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, item.onEquip, item.animator, _time))
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
        private IEnumerator moveToHead(TableManager table)
        {
            if (!table.tryGetSecret(_card, out var secret))
            {
                secret = table.createSecret(_card);
            }
            if (_card.define.hasKeyword(KEYWORD.MISSION))
            {
                secret.Image.gameObject.SetActive(false);
                secret.MissionImage.gameObject.SetActive(true);
            }
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, secret.onEnter, secret.animator, _time))
                yield return null;
        }
        private IEnumerator moveToHand(TableManager table, Vector3? startPos)
        {
            if (!table.tryGetHand(_card, out _))
            {
                var hand = table.createHand(_card);
                var positionLerp = hand.GetComponentInChildren<PositionLerp>();

                if (startPos.HasValue)
                {
                    var pos = startPos.Value;
                    hand.rectTransform.position = pos;
                    positionLerp.startPosition = pos;
                    positionLerp.setPosition(pos);
                }
                positionLerp.setTarget(hand.rectTransform);
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, _time))
                {
                    yield return null;
                }
            }
        }
        #endregion
        private IEnumerator createCardToDeck(TableManager table, CardSnapshot card, Vector3 startPos, Transform target, Vector3 targetSize, float startHeight = -1, bool isFaceUp = true)
        {
            //创建卡牌
            var cardUI = table.createCard(card, startPos, startHeight, isFaceUp);
            //播放动画
            var anim = new CardTransAnim(cardUI, target, targetSize / cardUI.rectTransform.rect.height, table.fieldToDeckCurve);
            cardUI.playSound(table.playCardSound);
            while (!anim.update(table, _time))//没放完动画就让它接着放
                yield return null;
            //摧毁卡牌
            Object.Destroy(cardUI.gameObject);
            //放完了
        }
        

        void updateItemMaster(TableManager table)
        {
            if (_fromMaster != null)
            {
                var masterUI = _fromMaster.getOwner() == table.player ? table.ui.SelfMaster : table.ui.EnemyMaster;
                table.setMaster(masterUI, _fromMaster);
                _fromMaster = null;
            }
            if (_toMaster != null)
            {
                var masterUI = _toMaster.getOwner() == table.player ? table.ui.SelfMaster : table.ui.EnemyMaster;
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
