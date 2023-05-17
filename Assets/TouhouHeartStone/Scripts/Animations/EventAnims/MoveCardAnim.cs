
using System.Collections;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
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
                    yield return moveFromItem(table);
                    break;
                case PILE.HEAD:
                    yield return moveFromHead(table, _card);
                    break;
                case PILE.WARP:
                    if (toName == PILE.HEAD)
                    {
                        yield return moveToHead(table, _card);
                    }
                    break;
                default:
                    yield return moveFromNull(table);
                    break;
            }
        }
        public IEnumerator moveFromDeck(TableManager table)
        {
            string toName = _to?.name;
            bool fromSelf = _from == table.player.getDeck();
            UDeck deck = fromSelf ? table.ui.SelfDeck : table.ui.EnemyDeck;
            RectTransform deckTransform = deck.rectTransform;
            if (toName == PILE.HAND)
            {
                //抽牌
                if (!table.tryGetHand(_card, out var hand))
                {
                    hand = table.createHand(_card);
                    hand.rectTransform.position = deckTransform.position;
                    hand.GetComponentInChildren<PositionLerp>().setTarget(hand.rectTransform);
                }
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, _time))
                {
                    yield return null;
                }
            }
            else if (toName == PILE.WARP)
            {
                // 牌库-->暂存区
                if (!table.tryGetHand(_card, out var hand))
                {
                    hand = table.createHand(_card);
                    hand.rectTransform.position = deckTransform.position;
                    hand.GetComponentInChildren<PositionLerp>().setTarget(hand.rectTransform);
                }
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, _time))
                {
                    yield return null;
                }
                anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, hand.onDiscard, hand.animator, _time))
                {
                    yield return null;
                }
                table.ui.SelfHandList.removeItem(hand);
                table.ui.EnemyHandList.removeItem(hand);
            }
            else if (toName == PILE.HEAD)
            {
                // 牌库-->延迟区
                yield return moveToHead(table, _card);
            }
            else if (toName == PILE.GRAVE)
            {
                // 牌库-->墓地
                var toSelf = _to?.owner == table.player;
                var grave = toSelf ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck;
                var targetTransform = grave.rectTransform;
                yield return moveCard(table, _card, deckTransform.position, targetTransform, _time);
            }
            else if (toName == PILE.FIELD)
            {
                // 牌库-->战场
                yield return moveToField(table);
            }
        }
        private IEnumerator moveFromHand(TableManager table)
        {
            string fromName = _from?.name;
            string toName = _to?.name;
            if (toName == PILE.ITEM)
            {
                var item = _to.owner == table.player ? table.ui.SelfItem : table.ui.EnemyItem;
                table.setItem(item, _card);
                item.display();
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, item.onEquip, item.animator, _time))
                {
                    yield return null;
                }
                updateItemMaster(table);
            }
            else if (toName == PILE.GRAVE)
            {
                if (table.tryGetHand(_card, out var hand))
                {
                    //弃牌
                    hand.GetComponentInChildren<PositionLerp>().resetPosition();
                    hand.GetComponentInChildren<PositionLerp>().setTarget(hand.getChild("GameObject").GetComponent<RectTransform>());

                    bool onBlockAnim(UIAnimation next)
                    {
                        if (next is MoveCardAnim moveCard &&
                            moveCard.tEventArg.card.id != _card.id)
                            return true;
                        return false;
                    }
                    AnimAnim anim = null;
                    while (!SimpleAnimHelper.update(table, ref anim, hand.onDiscard, hand.animator, _time, onBlockAnim))
                    {
                        yield return null;
                    }
                    table.ui.SelfHandList.removeItem(hand);
                    table.ui.EnemyHandList.removeItem(hand);
                }
            }
            else if (toName == PILE.HEAD)
            {
                yield return moveToHead(table, _card);
            }
            else if (toName == PILE.DECK)
            {
                if (table.tryGetHand(_card, out var hand))
                {
                    var DeckRectTransform = table.player == _card.owner ? table.ui.SelfDeck.rectTransform : table.ui.EnemyDeck.rectTransform;
                    hand.GetComponentInChildren<PositionLerp>().resetPosition();
                    hand.GetComponentInChildren<PositionLerp>().setTarget(DeckRectTransform);

                    bool onBlockAnim(UIAnimation next)
                    {
                        if (next is MoveCardAnim moveCard &&
                            moveCard.tEventArg.card.id != _card.id)
                            return true;
                        return false;
                    }
                    AnimAnim anim = null;
                    while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, _time, onBlockAnim))
                    {
                        yield return null;
                    }
                    table.ui.SelfHandList.removeItem(hand);
                    table.ui.EnemyHandList.removeItem(hand);
                }
            }
            else if (toName == PILE.FIELD)
            {
                yield return moveToField(table);
            }
        }
        private IEnumerator moveFromGrave(TableManager table)
        {
            string toName = _to?.name;
            bool fromSelf = _from == table.player.getGrave();
            UDeck grave = fromSelf ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck;
            RectTransform graveTransform = grave.rectTransform;
            if (toName == PILE.DECK)
            {
                // 墓地-->牌库
                bool toSelf = _to?.owner == table.player;
                var deck = toSelf ? table.ui.SelfDeck : table.ui.EnemyDeck;
                var targetTransform = deck.rectTransform;
                yield return moveCard(table, _card, graveTransform.position, targetTransform, _time);
            }
            if (toName == PILE.GRAVE)
            {
                // 墓地-->墓地
                bool toSelf = _to?.owner == table.player;
                var targetGrave = toSelf ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck;
                var targetTransform = targetGrave.rectTransform;
                yield return moveCard(table, _card, graveTransform.position, targetTransform, _time);
            }
            if (toName == PILE.HAND)
            {
                //从墓地抽牌
                if (!table.tryGetHand(_card, out var hand))
                {
                    hand = table.createHand(_card);
                    hand.rectTransform.position = graveTransform.position;
                    hand.GetComponentInChildren<PositionLerp>().setTarget(hand.rectTransform);
                }
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, _time))
                {
                    yield return null;
                }
            }
            else if (toName == PILE.HEAD)
            {
                yield return moveToHead(table, _card);
            }
            else if (toName == PILE.FIELD)
            {
                yield return moveToField(table);
            }
        }
        private IEnumerator moveFromField(TableManager table, Arg arg)
        {
            string toName = _to?.name;
            RectTransform fieldTransform = _from == table.player.getField() ? table.ui.SelfFieldList.rectTransform : table.ui.SelfFieldList.rectTransform;
            switch (toName)
            {
                case PILE.HAND:
                    {
                        //回手
                        Vector2 sourcePosition = fieldTransform.position;
                        if (table.tryGetServant(_card, out var servant))//尝试获取将要回手的随从
                        {
                            sourcePosition = servant.transform.position;
                            table.ui.SelfFieldList.removeItem(servant);
                            table.ui.EnemyFieldList.removeItem(servant);
                        }
                        if (!table.tryGetHand(_card, out var hand))
                        {
                            hand = table.createHand(_card);
                            hand.rectTransform.position = sourcePosition;
                            hand.GetComponentInChildren<PositionLerp>().setTarget(hand.rectTransform);
                        }
                        AnimAnim anim = null;
                        while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, _time))
                        {
                            yield return null;
                        }
                    }
                    break;
                case PILE.FIELD:
                    {
                        //获得控制权
                        TableAnimation anim = null;
                        if (table.tryGetServant(_card, out var servant))
                            anim = new FieldMoveAnim(table, servant, _to == table.player.getField() ? table.ui.SelfFieldList : table.ui.EnemyFieldList, _position);

                        while (anim != null && !anim.update(table, _time))
                            yield return null;
                    }
                    break;
                case PILE.DECK:
                    {
                        //洗回牌库
                        Vector2 sourcePosition = fieldTransform.position;
                        if (table.tryGetServant(_card, out var servant))
                        {
                            sourcePosition = servant.transform.position;
                            table.ui.SelfFieldList.removeItem(servant);
                            table.ui.EnemyFieldList.removeItem(servant);
                        }
                        //创建卡牌
                        var cardUI = table.createCard(_card, sourcePosition, servant.rectTransform.rect.height);
                        //播放动画
                        UDeck targetDeck = _to == table.player.getDeck() ? table.ui.SelfDeck : table.ui.EnemyDeck;
                        var anim = new CardTransAnim(cardUI,
                            targetDeck.transform,
                            targetDeck.bottom.rect.height * targetDeck.bottom.lossyScale / cardUI.rectTransform.rect.height,
                            table.fieldToDeckCurve);

                        while (!anim.update(table, _time))//没放完动画就让它接着放
                            yield return null;
                        //摧毁卡牌
                        Object.Destroy(cardUI.gameObject);
                        //放完了
                    }
                    break;
                case PILE.GRAVE:
                    {
                        //置入墓地
                        if (arg.parent is THHCard.DeathEventArg) // 阻止死亡触发该动画
                        {
                            yield break;
                        }
                        if (table.tryGetServant(_card, out var servant)) // 必须要场上有这个仆从才行
                        {
                            Vector2 sourcePosition = servant.transform.position;
                            table.ui.SelfFieldList.removeItem(servant);
                            table.ui.EnemyFieldList.removeItem(servant);
                            //创建卡牌
                            var cardUI = table.createCard(_card, sourcePosition, servant.rectTransform.rect.height);
                            //播放动画
                            UDeck targetDeck = _to == table.player.getGrave() ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck;
                            var anim = new CardTransAnim(cardUI,
                                targetDeck.transform,
                                targetDeck.bottom.rect.height * targetDeck.bottom.lossyScale / cardUI.rectTransform.rect.height,
                                table.fieldToDeckCurve);

                            while (!anim.update(table, _time))//没放完动画就让它接着放
                                yield return null;
                            //摧毁卡牌
                            Object.Destroy(cardUI.gameObject);
                            //放完了
                        }
                    }
                    break;
            }
        }
        private IEnumerator moveFromItem(TableManager table)
        {
            string toName = _to?.name;
            switch (toName)
            {
                case PILE.HAND:
                    {
                        //回手
                        if (table.tryGetItem(_card, out var item))//尝试获取将要回手的武器
                        {
                            item.hide();
                        }
                        if (!table.tryGetHand(_card, out var hand))
                        {
                            hand = table.createHand(_card);
                            hand.rectTransform.position = item.rectTransform.position;
                            hand.GetComponentInChildren<PositionLerp>().setTarget(hand.rectTransform);
                        }
                        AnimAnim anim = null;
                        while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, _time))
                        {
                            yield return null;
                        }
                    }
                    break;
                case PILE.ITEM:
                    {
                        //获得控制权
                        if (table.tryGetItem(_card, out var item))
                        {
                            item.hide();

                            var target = _to == table.player.getPile(PILE.ITEM) ? table.ui.SelfItem : table.ui.EnemyItem;
                            table.setItem(target, _card);
                        }
                    }
                    break;
                case PILE.DECK:
                    {
                        //洗回牌库
                        Vector3 itemPosition = Vector3.zero;
                        float itemHeight = -1;
                        if (table.tryGetItem(_card, out var item))
                        {
                            item.hide();
                            itemPosition = item.transform.position;
                            itemHeight = item.rectTransform.rect.height;
                        }
                        //创建卡牌
                        var cardUI = table.createCard(_card, itemPosition, itemHeight);
                        //播放动画
                        UDeck targetDeck = _to == table.player.getDeck() ? table.ui.SelfDeck : table.ui.EnemyDeck;
                        var anim = new CardTransAnim(cardUI,
                            targetDeck.transform,
                            targetDeck.bottom.rect.height * targetDeck.bottom.lossyScale / cardUI.rectTransform.rect.height,
                            table.fieldToDeckCurve);

                        while (!anim.update(table, _time))//没放完动画就让它接着放
                            yield return null;
                        //摧毁卡牌
                        Object.Destroy(cardUI.gameObject);
                        //放完了
                    }
                    break;
            }
            updateItemMaster(table);
        }
        private IEnumerator moveFromHead(TableManager table, CardSnapshot _card)
        {
            if (table.tryGetSecret(_card, out var secret))
            {
                AnimAnim anim = null;
                while (SimpleAnimHelper.update(table, ref anim, secret.onExit, secret.animator, _time))
                    yield return null;
                table.removeSecret(_card);
            }
        }
        private IEnumerator moveFromNull(TableManager table)
        {
            string toName = _to?.name;
            if (toName == PILE.HAND)
            {
                //发现，印卡
                if (!table.tryGetHand(_card, out var hand))
                    hand = table.createHand(_card);
                AnimAnim anim = null;
                while (!SimpleAnimHelper.update(table, ref anim, hand.onCreate, hand.CardAnimator, _time))
                    yield return null;
            }
            else if (toName == PILE.HEAD)
            {
                yield return moveToHead(table, _card);
            }
            else if (toName == PILE.FIELD)
            {
                yield return moveToField(table);
            }
        }
        private IEnumerator moveToField(TableManager table)
        {
            string fromName = _from?.name;
            string toName = _to?.name;
            if (fromName == PILE.HAND && table.tryGetHand(_card, out var hand))
            {
                table.ui.SelfHandList.removeItem(hand);//移除手牌
            }

            if (!table.tryGetServant(_card, out var servant))
                servant = table.createServant(_card, _position);
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, servant.onSummon, servant.servantAnimator, _time))
            {
                yield return null;
            }
        }
        /// <summary>
        /// 奥秘入场
        /// </summary>
        /// <param name="table"></param>
        /// <param name="_card"></param>
        /// <returns></returns>
        private IEnumerator moveToHead(TableManager table, CardSnapshot _card)
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
        private IEnumerator moveCard(TableManager table, ICardData _card, Vector3 sourcePos, RectTransform targetTrans, float time)
        {
            var hand = table.createHand(_card);
            hand.rectTransform.position = sourcePos;
            hand.GetComponentInChildren<PositionLerp>().setTarget(targetTrans);
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, time))
                yield return null;
            table.removeHand(_card);
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
            //if (nextAnim is EnterFieldAnim enter)
            //{
            //    var nextCard = enter.tEventArg._card;
            //    var _card = tEventArg._card;
            //    if (nextCard != _card)
            //    {
            //        return false;
            //    }
            //}
            return base.blockAnim(nextAnim);
        }
    }
}
