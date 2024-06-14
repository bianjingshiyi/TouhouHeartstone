using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Game.Animations;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Card = TouhouCardEngine.Card;
using UICard = Game.BattleTable.TableCard;
using Timer = BJSYGameCore.Timer;

namespace Game.BattleTable
{
    public partial class BattleTableManager
    {
        #region 公有方法
        public void cancelTargetSelect()
        {
            // TODO: 目前只能取消宣言，因为目前只有宣言的操作是需要点两下，攻击需要拖动。
            if (isSelectingTargetForBattlecry())
            {
                cancelBattlecry(targetSelectingCard);
            }
            finishTargetSelect();
        }
        public void finishTargetSelect()
        {
            targetSelectingCard = null;
            _servantUsingPosition = 0;
            targetSelectMode = TargetSelectMode.None;
            updateAllControlable();
            hideArrow();
        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 高亮所有指定的目标（标记为可选择）
        /// </summary>
        /// <param name="targets"></param>
        private void highlightTargets(Card[] targets)
        {
            foreach (var target in targets)
            {
                if (tryGetMaster(target, out var master))
                {
                    master.setHighlight(true, true);
                }
                else if (tryGetServant(target, out var servant))
                {
                    servant.setHighlight(true, true);
                }
            }
        }

        /// <summary>
        /// 判断鼠标是否在某张卡上，并返回卡
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        private bool isOnTarget(PointerEventData pointer, out Card card)
        {
            if (pointer.pointerCurrentRaycast.gameObject != null)
            {
                if (pointer.pointerCurrentRaycast.gameObject.GetComponentInParent<Servant>() is Servant targetServant)
                {
                    card = getCard(targetServant);
                    return card != null;
                }
                else if (pointer.pointerCurrentRaycast.gameObject.GetComponentInParent<Master>() is Master targetMaster)
                {
                    card = getCard(targetMaster);
                    return card != null;
                }
            }
            card = null;
            return false;
        }

        /// <summary>
        /// 显示箭头
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void displayArrow(Transform source, Vector3 from, Vector3 to)
        {
            table.AttackArrowImage.setActive(true);
            table.AttackArrowImage.connect(this, from, to);
        }
        /// <summary>
        /// 隐藏箭头
        /// </summary>
        private void hideArrow()
        {
            table.AttackArrowImage.setActive(false);
        }

        #region 法术
        private void startSelectingTargetForSpell(Card card)
        {
            if (isSelectingTarget())
                return;
            if (tryGetHand(card, out var hand))
            {
                hand.setSpellReady(true);
            }
            enterTargetSelectMode(TargetSelectMode.Use, card, card.getAvaliableTargets(game));
            hideAllHands();//手牌缩小
        }
        private void playSpellCard(Card card, PointerEventData pointer)
        {
            if (card.isNeedTarget(game))
            {
                //获取鼠标停留的那张卡，就是目标
                if (isOnTarget(pointer, out var target))
                {
                    playSpellToTarget(card, target);
                }
                else
                {
                    resetUse();
                }
            }
            else
            {
                //使用无目标法术牌
                player.cmdUse(game, card, 0);
                resetUse(resetHandItem: false);
            }
        }
        private void playSpellToTarget(Card card, Card target)
        {
            if (card.isValidTarget(game, target, out string invalidMsg))
            {
                //合法目标，生效
                player.cmdUse(game, card, 0, target);
                resetUse(resetHandItem: false, finishSelectTarget: true);
            }
            else
            {
                //非法目标，跳出非法信息
                showErrorTip(invalidMsg);
                resetUse();
            }
        }
        #endregion

        /// <summary>
        /// 在目标选择期间，更新箭头。
        /// </summary>
        /// <param name="position"></param>
        private void showArrowWhileUsing(Vector3 position)
        {
            var sourceTransform = table.SelfMaster.transform;
            var source = targetSelectingCard;
            if (source != null)
            {
                if (source.isSpell())
                {
                    if (tryGetMaster(source, out Master master))
                    {
                        sourceTransform = master.transform;
                    }
                }
                else if (tryGetMovingCard(source, out var movingCard))
                {
                    sourceTransform = movingCard.transform;
                }
                else
                {
                    var trans = getCardTransform(source);
                    if (trans != null)
                    {
                        sourceTransform = trans;
                    }
                }
            }
            //拖出箭头
            var sourcePos = WorldToTablePosition(sourceTransform.position + Vector3.up * 10);
            displayArrow(sourceTransform, sourcePos, position);
        }
        /// <summary>
        /// 取消宣言目标选择。
        /// </summary>
        /// <param name="card">触发宣言的卡牌。</param>
        /// <returns>是否成功取消。</returns>
        private bool cancelBattlecry(Card card)
        {
            if (card == null)
                return false;
            // 取消随从的入场预览动画。
            return cancelServantPreviewAnim(card);
        }
        /// <summary>
        /// 尝试确定选择目标。
        /// </summary>
        /// <param name="target">目标卡牌。</param>
        /// <returns>是否成功。</returns>
        private bool trySelectTargetForBattlecry(Card target)
        {
            if (!isSelectingTargetForBattlecry())
                return false;
            //获取鼠标停留的那张卡，就是目标
            if (target != null)
            {
                var source = targetSelectingCard;
                if (source.isValidTarget(game, target, out string invalidMsg))
                {
                    //合法目标，生效
                    player.cmdUse(game, source, _servantUsingPosition, target);
                    resetUse(resetHandItem: false, finishSelectTarget: true);
                    return true;
                }
                else
                {
                    //非法目标，跳出非法信息
                    showErrorTip(invalidMsg);
                }
            }
            return false;
        }

        private void enterTargetSelectMode(TargetSelectMode mode, Card card, Card[] targets)
        {
            targetSelectingCard = card;
            targetSelectMode = mode;
            //高亮标记所有目标
            updateAllControlable();
            highlightTargets(targets);
        }
        private bool isSelectingTarget()
        {
            return targetSelectMode != TargetSelectMode.None;
        }
        private bool isSelectingTargetForOtherCards(Card card)
        {
            return targetSelectingCard != null && !isSelectingTargetForCard(card);
        }
        private bool isSelectingTargetForCard(Card card)
        {
            return targetSelectingCard == card;
        }
        private bool isSelectingTargetForBattlecry()
        {
            if (targetSelectMode != TargetSelectMode.Use)
                return false;
            return targetSelectingCard != null && (targetSelectingCard.isServant() || targetSelectingCard.isItem());
        }

        #region 仆从使用预览动画
        private HandToFieldAnim addServantPreviewAnim(Card card, int index, Vector3 pos)
        {
            cancelServantPreviewAnim(card);
            _servantUsingPosition = index;
            var fieldAnim = new HandToFieldAnim(getCardDescriptor(card), pos, player.getHand(), player.getField(), index, canSeeCard(card));
            addAnim(fieldAnim);
            _servantPreviewAnimDict.Add(card.id, fieldAnim);
            return fieldAnim;
        }
        private bool cancelServantPreviewAnim(Card card)
        {
            if (card.pile == player.getHand())
            {
                finishServantPreviewAnim(card.id);
                var handPosition = card.pile.indexOf(card);
                // 如果没有手牌。
                if (!tryGetHand(card, out HandItem hand))
                {
                    // 如果有这个随从，播放回手动画。
                    if (tryGetServant(card, out _))
                    {
                        updateCardDescriptor(card);
                        addAnim(new FieldToHandAnim(card.id, player.getField(), player.getHand(), handPosition));
                    }
                    else //没有这个随从，直接创建手牌。
                    {
                        createHand(getCardDescriptor(card), handPosition);
                    }
                }
                else // 有这张手牌。
                {
                    hand.ignoreAlign = false;
                }
                return true;
            }
            return false;
        }
        private bool finishServantPreviewAnim(int cardId)
        {
            if (_servantPreviewAnimDict.TryGetValue(cardId, out var anim))
            {
                if (anim.isStarted() && !anim.isFinished())
                {
                    anim.forceFinish(this);
                }
                removeAnim(anim);
                _servantPreviewAnimDict.Remove(cardId);
                return true;
            }
            return false;
        }
        #endregion

        #endregion

        #region 属性字段
        [TranslateMsg]
        private const string TAB_THIS_INVALIDATION = "这不是一个有效的目标！";
        /// <summary>
        /// 当前正在使用/攻击的卡片的引用
        /// </summary>
        public Card targetSelectingCard { get; set; } = null;
        /// <summary>
        /// 目标选择模式
        /// </summary>
        public TargetSelectMode targetSelectMode { get; set; }
        #endregion
    }

    public enum TargetSelectMode
    {
        None,
        Use,
        Attack
    }
}
