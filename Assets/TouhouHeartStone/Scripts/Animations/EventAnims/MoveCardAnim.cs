
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
                    yield return moveFromHead(table, arg);
                    break;
                case PILE.WARP:
                    if (toName == PILE.HEAD)
                    {
                        yield return moveToHead(table);
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
            
            switch (toName)
            {
                case PILE.DECK: // 牌库-->牌库
                    {
                        if (_from == _to) // 移动到自己的牌库时不播放动画
                        {
                            break;
                        }
                        var toSelf = _to?.owner == table.player;
                        var targetDeck = toSelf ? table.ui.SelfDeck : table.ui.EnemyDeck;
                        var targetTransform = targetDeck.rectTransform;
                        yield return moveCard(table, _card, deckTransform.position, targetTransform, _time);
                    }
                    break;
                case PILE.HAND: // 牌库-->手牌
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
                    break;
                case PILE.HEAD: // 牌库-->延迟区
                    {
                        yield return moveToHead(table);
                    }
                    break;
                case PILE.GRAVE: // 牌库-->墓地
                    {
                        var toSelf = _to?.owner == table.player;
                        var grave = toSelf ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck;
                        var targetTransform = grave.rectTransform;
                        yield return moveCard(table, _card, deckTransform.position, targetTransform, _time);
                    }
                    break;
                case PILE.FIELD: // 牌库-->战场
                    {
                        yield return moveToField(table);
                    }
                    break;
                case PILE.ITEM: // 牌库-->道具
                    {
                        yield return moveToItem(table);
                    }
                    break;
            }
        }
        private IEnumerator moveFromHand(TableManager table)
        {
            string fromName = _from?.name;
            string toName = _to?.name;

            // 移除原先手牌UI。
            switch (toName)
            {
                case PILE.DECK: // 手牌-->牌库
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
                            table.removeHand(_card);
                        }
                    }
                    break;
                case PILE.HAND: // 手牌-->手牌
                    {
                        if (!table.tryGetHand(_card, out var hand)) // 不存在手牌UI。
                        {
                            // 创建一个
                            hand = table.createHand(_card);
                        }
                        else
                        {
                            // 记录之前的位置。
                            var posBefore = hand.transform.position;

                            // 移除旧的手牌UI。
                            table.removeHand(_card);

                            // 创建一个新的。
                            hand = table.createHand(_card);

                            // 移动到新的位置。
                            var positionLerp = hand.GetComponentInChildren<PositionLerp>();
                            positionLerp.startPosition = posBefore;
                            positionLerp.setPosition(posBefore);
                            positionLerp.setTarget(hand.rectTransform);

                            AnimAnim anim = null;
                            while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, _time))
                            {
                                yield return null;
                            }
                        }
                    }
                    break;
                case PILE.GRAVE: // 手牌-->墓地
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
                            table.removeHand(_card);
                        }
                    }
                    break;
                default: // 手牌-->其他牌堆
                    {
                        if (table.tryGetHand(_card, out var hand))
                        {
                            table.removeHand(_card);
                        }
                    }
                    break;
            }

            // 为目标牌堆创建UI。
            switch (toName)
            {
                case PILE.FIELD: // 手牌-->战场
                    yield return moveToField(table);
                    break;
                case PILE.ITEM: // 手牌-->道具
                    yield return moveToItem(table);
                    break;
                case PILE.HEAD: // 手牌-->延迟区
                    yield return moveToHead(table);
                    break;
            }
        }
        private IEnumerator moveFromGrave(TableManager table)
        {
            string toName = _to?.name;
            bool fromSelf = _from == table.player.getGrave();
            UDeck grave = fromSelf ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck;
            RectTransform graveTransform = grave.rectTransform;
            switch (toName)
            {
                case PILE.DECK: // 墓地-->牌库
                    {
                        bool toSelf = _to?.owner == table.player;
                        var deck = toSelf ? table.ui.SelfDeck : table.ui.EnemyDeck;
                        var targetTransform = deck.rectTransform;
                        yield return moveCard(table, _card, graveTransform.position, targetTransform, _time);
                    }
                    break;
                case PILE.GRAVE: // 墓地-->墓地
                    {
                        if (_from == _to)// 移动到自己的墓地时不播放动画
                        {
                            break;
                        }
                        bool toSelf = _to?.owner == table.player;
                        var targetGrave = toSelf ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck;
                        var targetTransform = targetGrave.rectTransform;
                        yield return moveCard(table, _card, graveTransform.position, targetTransform, _time);
                    }
                    break;
                case PILE.HAND: // 墓地-->手牌
                    {
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
                    break;
                case PILE.HEAD: // 墓地-->延迟
                    {
                        yield return moveToHead(table);
                    }
                    break;
                case PILE.FIELD: // 墓地-->战场
                    {
                        yield return moveToField(table);
                    }
                    break;
                case PILE.ITEM: // 墓地-->道具
                    {
                        yield return moveToItem(table);
                    }
                    break;
            }
        }
        private IEnumerator moveFromField(TableManager table, Arg arg)
        {
            string toName = _to?.name;
            RectTransform fieldTransform = _from == table.player.getField() ? table.ui.SelfFieldList.rectTransform : table.ui.SelfFieldList.rectTransform;
            switch (toName)
            {
                case PILE.HAND: // 战场-->手牌
                    {
                        //回手
                        Vector2 sourcePosition = fieldTransform.position;
                        if (table.tryGetServant(_card, out var servant))//尝试获取将要回手的随从
                        {
                            sourcePosition = servant.transform.position;
                            table.removeServant(_card);
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
                case PILE.FIELD: // 战场-->战场
                    {
                        //获得控制权
                        TableAnimation anim = null;
                        if (table.tryGetServant(_card, out var servant))
                            anim = new FieldMoveAnim(table, servant, _to == table.player.getField() ? table.ui.SelfFieldList : table.ui.EnemyFieldList, _position);

                        while (anim != null && !anim.update(table, _time))
                            yield return null;
                    }
                    break;
                case PILE.DECK: // 战场-->牌库
                    {
                        //洗回牌库
                        Vector2 sourcePosition = fieldTransform.position;
                        if (table.tryGetServant(_card, out var servant))
                        {
                            sourcePosition = servant.transform.position;
                            table.removeServant(_card);
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
                case PILE.GRAVE: // 战场-->墓地
                    {
                        //置入墓地
                        if (arg.parent is THHCard.DeathEventArg) // 阻止死亡触发该动画
                        {
                            break;
                        }
                        if (table.tryGetServant(_card, out var servant)) // 必须要场上有这个仆从才行
                        {
                            Vector2 sourcePosition = servant.transform.position;
                            table.removeServant(_card);
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
                default: // 战场-->其他牌堆
                    {
                        if (table.tryGetServant(_card, out _)) // 必须要场上有这个仆从才行
                        {
                            table.removeServant(_card);
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
                case PILE.HAND: // 道具-->手牌
                    {
                        //回手
                        if (table.tryGetItem(_card, out var item))//尝试获取将要回手的武器
                        {
                            table.removeItem(_card);
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
                case PILE.ITEM: // 道具-->道具
                    {
                        //获得控制权
                        if (table.tryGetItem(_card, out var item))
                        {
                            table.removeItem(_card);

                            var target = _to == table.player.getPile(PILE.ITEM) ? table.ui.SelfItem : table.ui.EnemyItem;
                            table.setItem(target, _card);
                        }
                    }
                    break;
                case PILE.DECK: // 道具-->牌库
                    {
                        //洗回牌库
                        Vector3 itemPosition = Vector3.zero;
                        float itemHeight = -1;
                        if (table.tryGetItem(_card, out var item))
                        {
                            table.removeItem(_card);
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
                default: // 道具-->其他牌堆
                    {
                        //获得控制权
                        if (table.tryGetItem(_card, out _))
                        {
                            table.removeItem(_card);
                        }
                    }
                    break;
            }
            updateItemMaster(table);
        }
        private IEnumerator moveFromHead(TableManager table, Arg arg)
        {
            string toName = _to?.name;
            bool fromSelf = _from?.owner == table.player;

            Vector2? secretPos = null;
            if (table.tryGetSecret(_card, out var secret))
            {
                secretPos = secret.transform.position;
                AnimAnim anim = null;
                while (SimpleAnimHelper.update(table, ref anim, secret.onExit, secret.animator, _time))
                    yield return null;
                table.removeSecret(_card);
            }

            switch (toName)
            {
                case PILE.HAND: // 延迟-->手牌
                    {
                        if (!table.tryGetHand(_card, out var hand))
                        {
                            hand = table.createHand(_card);
                            hand.rectTransform.position = secretPos ?? hand.rectTransform.position;
                            hand.GetComponentInChildren<PositionLerp>().setTarget(hand.rectTransform);
                        }
                        AnimAnim anim = null;
                        while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, _time))
                        {
                            yield return null;
                        }
                    }
                    break;
                case PILE.HEAD: // 延迟-->延迟
                    {
                        if (!table.tryGetSecret(_card, out var hand))
                        {
                            hand = table.createSecret(_card);;
                        }
                        AnimAnim anim = null;
                        while (!SimpleAnimHelper.update(table, ref anim, hand.onEnter, hand.animator, _time))
                        {
                            yield return null;
                        }
                    }
                    break;
                case PILE.DECK: // 延迟-->牌库
                    {
                        if (secretPos != null)
                        {
                            //创建卡牌
                            var cardUI = table.createCard(_card, secretPos.Value, 0, fromSelf);
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
                    }
                    break;
                case PILE.GRAVE: // 延迟-->墓地
                    {
                        //置入墓地
                        if (arg.parent is THHCard.SecretTriggerEventArg) // 阻止奥秘触发时播放该动画
                        {
                            break;
                        }
                        if (secretPos != null)
                        {
                            //创建卡牌
                            var cardUI = table.createCard(_card, secretPos.Value, 0, fromSelf);
                            //播放动画
                            UDeck targetGrave = _to == table.player.getGrave() ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck;
                            var anim = new CardTransAnim(cardUI,
                                targetGrave.transform,
                                targetGrave.bottom.rect.height * targetGrave.bottom.lossyScale / cardUI.rectTransform.rect.height,
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
        private IEnumerator moveFromNull(TableManager table)
        {
            string toName = _to?.name;
            switch (toName)
            {

                case PILE.HAND: //空-->手牌
                    {
                        //发现，印卡
                        if (!table.tryGetHand(_card, out var hand))
                            hand = table.createHand(_card);
                        AnimAnim anim = null;
                        while (!SimpleAnimHelper.update(table, ref anim, hand.onCreate, hand.CardAnimator, _time))
                            yield return null;
                    }
                    break;
                case PILE.HEAD: //空-->延迟区
                    {
                        yield return moveToHead(table);
                    }
                    break;
                case PILE.FIELD: //空-->战场
                    {
                        yield return moveToField(table);
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
            item.display();
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
        private IEnumerator moveCard(TableManager table, ICardData card, Vector3 sourcePos, RectTransform targetTrans, float time)
        {
            var hand = table.createHand(card);
            hand.rectTransform.position = sourcePos;
            hand.GetComponentInChildren<PositionLerp>().setTarget(targetTrans);
            AnimAnim anim = null;
            while (!SimpleAnimHelper.update(table, ref anim, hand.onDraw, hand.animator, time))
                yield return null;
            table.removeHand(card);
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
