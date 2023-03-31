using System.Collections;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Game
{
    class RevealAnim : EventAnimation<HearthStoneRule.RevealEventArg>
    {
        Card _card;
        TableAnimation _moveAnim;
        AnimAnim _flipFaceAnim;
        AnimAnim _flipBackAnim;
        TableAnimation _moveBackAnim;
        float _time = 0;
        Coroutine _coroutine;
        WaitAnim _waitAnim;
        bool completed = false;

        public override bool update(TableManager table, HearthStoneRule.RevealEventArg eventArg, float time)
        {
            _time = time;
            if (_coroutine == null)
            {
                _coroutine = table.StartCoroutine(coroutine(table, eventArg));
            }
            if (table.tryGetHand(eventArg.card, out var handItem))
            {
                handItem.alpha = completed ? 1 : 0;
            }
            return completed;
        }
        private IEnumerator coroutine(TableManager table, HearthStoneRule.RevealEventArg eventArg)
        {
            if (!eventArg.shouldAnimate || eventArg.card == null)
            {
                completed = true;
                yield break;
            }
            //在对应的卡组上创建卡牌实例

            // 获取卡牌源位置。
            var targetTransform = getCardTargetTransfrom(table, eventArg);
            var startHeight = getCardStartHeight(table, eventArg);

            // 如果该牌在你的手牌里，起始正面朝上。
            bool startFaceUp = eventArg.player == table.player && eventArg.card.pile.name == PILE.HAND;
            if (_card == null)
            {
                _card = table.createInvokeCard(eventArg.card, targetTransform?.position ?? Vector3.zero, -1, startFaceUp);
            }
            var startScale = Vector3.one * startHeight / _card.rectTransform.rect.height;
            _card.transform.localScale = startScale;


            //播放位移曲线动画到对应的位置
            RectTransform targetRect;
            if (_moveAnim == null)
            {
                targetRect = table.ui.LeftDisplayImage.rectTransform;
                _moveAnim = new CardTransAnim(_card, targetRect, Vector3.one * targetRect.rect.height / _card.rectTransform.rect.height, table.cardShowCurve);
            }
            while (!_moveAnim.update(table, _time))
            {
                yield return null;
            }

            //翻转揭示
            if (!startFaceUp)
            {
                if (_flipFaceAnim == null)
                {
                    _flipFaceAnim = new AnimAnim(_card.animator, _card._flipToFaceAnimName);
                    table.setCard(_card, eventArg.card);
                }
                while (!_flipFaceAnim.update(table, _time))
                {
                    yield return null;
                }
            }

            // 等待1秒。
            if (_waitAnim == null)
                _waitAnim = new WaitAnim(1);
            while (!_waitAnim.update(table, _time))
            {
                yield return null;
            }

            //重新翻回
            if (!startFaceUp)
            {
                if (_flipBackAnim == null)
                {
                    _card.asAnimator.SetBool("isFaceUp", false);
                    _flipBackAnim = new AnimAnim(_card.animator, _card._flipToBackAnimName);
                }
                while (!_flipBackAnim.update(table, _time))
                {
                    yield return null;
                }
            }

            targetTransform = getCardTargetTransfrom(table, eventArg);
            //放回去
            if (_moveBackAnim == null)
            {
                _moveBackAnim = new CardTransAnim(_card, targetTransform, startScale, table.cardShowCurve);
            }
            while (!_moveBackAnim.update(table, _time))
            {
                yield return null;
            }


            //销毁实例
            if (_card != null)
                UObject.Destroy(_card.gameObject);
            completed = true;
            yield break;
        }

        private Transform getCardTargetTransfrom(TableManager table, HearthStoneRule.RevealEventArg eventArg)
        {
            Transform trans = null;
            var pile = eventArg.card.pile;
            if (pile != null)
            {
                if (pile.name == PILE.DECK)
                {
                    trans = (eventArg.player == table.player ? table.ui.SelfDeck : table.ui.EnemyDeck).transform;
                }
                else if (pile.name == PILE.HAND)
                {
                    if (table.tryGetHand(eventArg.card, out var handItem))
                    {
                        trans = handItem.transform;
                    }
                    else
                    {
                        trans = (eventArg.player == table.player ? table.ui.SelfHandList : table.ui.EnemyHandList).transform;
                    }
                }
                else if (pile.name == PILE.GRAVE)
                {
                    trans = (eventArg.player == table.player ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck).transform;
                }
            }
            return trans;
        }

        private float getCardStartHeight(TableManager table, HearthStoneRule.RevealEventArg eventArg)
        {
            float height = 1;
            var pile = eventArg.card.pile;
            if (pile != null)
            {
                if (pile.name == PILE.DECK)
                {
                    height = (eventArg.player == table.player ? table.ui.SelfDeck : table.ui.EnemyDeck).getCardHeight();
                }
                else if (pile.name == PILE.HAND)
                {
                    if (table.tryGetHand(eventArg.card, out var handItem))
                    {
                        height = handItem.rectTransform.rect.height * handItem.transform.lossyScale.y;
                    }
                    else
                    {
                        height = (eventArg.player == table.player ? table.ui.SelfHandList : table.ui.EnemyHandList).rectTransform.rect.height;
                    }
                }
                else if (pile.name == PILE.GRAVE)
                {
                    height = (eventArg.player == table.player ? table.ui.SelfGraveDeck : table.ui.EnemyGraveDeck).getCardHeight();
                }
            }
            return height;
        }
    }
}
