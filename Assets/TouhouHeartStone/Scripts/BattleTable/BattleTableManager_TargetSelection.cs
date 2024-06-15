using System.Collections.Generic;
using System.Linq;
using Game.BattleTable.UI;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.EventSystems;
using Card = TouhouCardEngine.Card;

namespace Game.BattleTable
{
    public partial class BattleTableManager
    {
        #region 公有方法
        public void cancelTargetSelect()
        {
            if (isSelectingTargetForServantBattlecry())
            {
                // 取消随从宣言的入场预览动画。
                tryCancelServantPreviewAnim(targetSelectingCard);
            }
            finishTargetSelect();
        }
        public void finishTargetSelect()
        {
            targetSelectingCard = null;
            _servantUsingPosition = 0;
            targetSelectType = TargetSelectType.None;
            targetSelectMode = TargetSelectMode.Click;
            _selectableTargets.Clear();
            _selectedTargets.Clear();
            _selectingBuffEffects.Clear();
            hideAllSelectionTargets();
            updateAllHighlight();
            hideArrow();
        }
        /// <summary>
        /// 将使用卡牌状态重置到什么也没做的状态。
        /// </summary>
        /// <param name="resetHandItem">是否重置当前正在拖动的手牌</param>
        /// <param name="resetServantPreview">是否重置随从占位</param>
        /// <param name="finishSelectTarget">是否要完成目标选择而非取消目标选择</param>
        public void resetUse(bool resetHandItem = true, bool resetServantPreview = true, bool finishSelectTarget = false)
        {
            // 使用中卡牌
            if (resetHandItem)
            {
                resetHand(usingCard);
            }
            // 选择目标
            if (finishSelectTarget)
                finishTargetSelect();
            else
                cancelTargetSelect();
            // 仆从预览
            if (resetServantPreview)
            {
                table.SelfField.setPreviewPosition(-1);
                _servantPreviewAnimDict.Clear();
            }
            usingCard = null;
        }
        #endregion

        #region 私有方法

        #region 开始选择
        private void startSelectingTargetForSpell(Card card, TargetSelectMode mode)
        {
            if (isSelectingTarget())
                return;
            if (tryGetHand(card, out var hand))
            {
                hand.setSpellReady(true);
            }
            var fullEffects = card.getFullTargetActiveEffects(game);
            _selectingBuffEffects.Clear();
            _selectingBuffEffects.AddRange(fullEffects);
            enterTargetSelectMode(TargetSelectType.Use, mode, card, card.getAvaliableTargets(game, _selectingBuffEffects));
        }
        private void startSelectingTargetForServantBattlecry(Card card, int index, Vector3 tablePos, TargetSelectMode mode)
        {
            if (isSelectingTarget())
                return;
            var fullEffects = card.getFullTargetActiveEffects(game);
            _selectingBuffEffects.Clear();
            _selectingBuffEffects.AddRange(fullEffects);
            enterTargetSelectMode(TargetSelectType.ServantBattlecry, mode, card, card.getAvaliableTargets(game, _selectingBuffEffects));
            addServantPreviewAnim(card, index, tablePos);
        }
        private void startSelectingTargetForUse(Card card, TargetSelectMode mode)
        {
            if (isSelectingTarget())
                return;
            var fullEffects = card.getFullTargetActiveEffects(game);
            _selectingBuffEffects.Clear();
            _selectingBuffEffects.AddRange(fullEffects);
            enterTargetSelectMode(TargetSelectType.Use, mode, card, card.getAvaliableTargets(game, _selectingBuffEffects));
        }
        private void startSelectingTargetForAttack(Card card, Card[] targets, TargetSelectMode mode)
        {
            if (isSelectingTarget())
                return;
            enterTargetSelectMode(TargetSelectType.Attack, mode, card, targets);
        }

        private void enterTargetSelectMode(TargetSelectType type, TargetSelectMode mode, Card card, Card[] targets)
        {
            targetSelectingCard = card;
            targetSelectType = type;
            targetSelectMode = mode;
            setSelectableTargets(targets);
            _selectedTargets.Clear();
            //高亮标记所有目标
            updateAllHighlight();
            hideAllHands();//手牌缩小
        }
        private void setSelectableTargets(Card[] targets)
        {
            _selectableTargets.Clear();
            _selectableTargets.AddRange(targets);
        }
        #endregion

        #region 确认目标
        private bool tryConfirmTargetByPointer(PointerEventData pointer)
        {
            if (!isOnTargetCard(pointer, out var target))
                return false;

            return tryConfirmTarget(target);
        }
        private bool tryConfirmTarget(Card target)
        {
            switch (targetSelectType)
            {
                case TargetSelectType.Use:
                case TargetSelectType.ServantBattlecry:
                    return tryConfirmTargetForUse(target);
                case TargetSelectType.Attack:
                    return tryConfirmTargetForAttack(target);
            }
            return false;
        }
        /// <summary>
        /// 尝试确定选择目标。
        /// </summary>
        /// <param name="target">目标卡牌。</param>
        /// <returns>是否成功。</returns>
        private bool tryConfirmTargetForUse(Card target)
        {
            if (target == null)
                return false;

            int targetIndex = _selectedTargets.Count;
            var source = targetSelectingCard;
            var selectedCards = _selectedTargets.ToArray();
            if (!source.isValidTargetForEffects(game, _selectingBuffEffects, target, out string invalidMsg, selectedCards))
            {
                //非法目标，跳出非法信息
                showErrorTip(invalidMsg);
                return false;
            }
            //合法目标，选中
            _selectedTargets.Add(target);
            selectedCards = _selectedTargets.ToArray();

            var filteredEffects = source.filterTargetEffects(game, _selectingBuffEffects, selectedCards);
            if (filteredEffects.Count() > 0)
            {
                // 没有选择完目标，继续选择
                _selectingBuffEffects.Clear();
                _selectingBuffEffects.AddRange(filteredEffects);
                targetSelectMode = TargetSelectMode.Click;
                setSelectableTargets(source.getAvaliableTargets(game, filteredEffects, selectedCards));
                updateAllHighlight();
                displaySelectionTarget(target, targetIndex);
            }
            else
            {
                // 目标选择完毕，使用卡牌
                player.cmdUse(game, source, _servantUsingPosition, _selectedTargets.ToArray());
                resetUse(resetHandItem: false, finishSelectTarget: true);
            }
            return true;
        }
        private bool tryConfirmTargetForAttack(Card target)
        {
            if (target == null)
                return false;

            var source = targetSelectingCard;
            if (!source.isAttackable(game, target, out var tip))
            {
                //非法目标，跳出非法信息
                showErrorTip(tip);
                return false;
            }
            //合法目标，生效
            player.cmdAttack(game, source, target);
            resetUse(finishSelectTarget: true);
            return true;
        }
        #endregion

        #region 打出
        private void playHand(Card card, PointerEventData pointer, TargetSelectMode mode)
        {
            // 随从牌。
            if (card.isServant())
            {
                playServantHand(card, pointer);
            }
            // 道具牌。
            else if (card.isItem())
            {
                playItemHand(card);
            }
            // 法术牌。
            else if (card.isSpell())
            {
                playSpellHand(card, pointer, mode);
            }
        }
        private void playServantHand(Card card, PointerEventData pointer)
        {
            var screenPos = pointer.position;
            var worldPos = ScreenToWorldPosition(screenPos);
            var tablePos = WorldToTablePosition(worldPos);

            int index = calcIndexInField(tablePos.x);
            table.SelfField.setPreviewPosition(-1);
            if (card.hasFullTargetActiveEffect(game))
            {
                //进入选择目标状态，固定手牌到占位上，高亮可以选择的目标
                startSelectingTargetForServantBattlecry(card, index, tablePos, TargetSelectMode.Click);
            }
            else
            {
                //使用无目标随从牌
                player.cmdUse(game, card, index);
                resetUse(resetHandItem: false);
            }
        }
        private void playItemHand(Card card)
        {
            if (card.hasFullTargetActiveEffect(game))
            {
                //进入选择目标状态，固定手牌到占位上，高亮可以选择的目标
                startSelectingTargetForUse(card, TargetSelectMode.Click);
            }
            else
            {
                //使用无目标道具牌
                player.cmdUse(game, card, 0);
                resetUse(resetHandItem: false);
            }
        }
        private void playSpellHand(Card card, PointerEventData pointer, TargetSelectMode mode)
        {
            if (card.isNeedTarget(game))
            {
                if (isSelectingTargetMode(mode))
                {
                    if (!tryConfirmTargetByPointer(pointer) && mode == TargetSelectMode.Drag)
                    {
                        // 拖动选择目标失败后取消选择。
                        resetUse();
                    }
                }
            }
            else
            {
                //使用无目标法术牌
                player.cmdUse(game, card, 0);
                resetUse(resetHandItem: false);
            }
        }
        #endregion

        #region 检测
        private bool isSelectingTarget()
        {
            return targetSelectType != TargetSelectType.None;
        }
        private bool isSelectingTargetForUse()
        {
            return targetSelectType == TargetSelectType.Use;
        }
        private bool isSelectingTargetForServantBattlecry()
        {
            return targetSelectType == TargetSelectType.ServantBattlecry;
        }
        private bool isSelectingTargetForOtherCards(Card card)
        {
            return isSelectingTarget() && !isSelectingTargetForCard(card);
        }
        private bool isSelectingTargetForCard(Card card)
        {
            return targetSelectingCard == card;
        }
        private bool isSelectingTargetMode(TargetSelectMode mode)
        {
            return targetSelectMode == mode;
        }
        #endregion

        #region 仆从使用预览动画
        private HandToFieldAnim addServantPreviewAnim(Card card, int index, Vector3 pos)
        {
            tryCancelServantPreviewAnim(card);
            _servantUsingPosition = index;
            var fieldAnim = new HandToFieldAnim(getCardDescriptor(card), pos, player.getHand(), player.getField(), index, canSeeCard(card));
            addAnim(fieldAnim);
            _servantPreviewAnimDict.Add(card.id, fieldAnim);
            return fieldAnim;
        }
        private bool tryCancelServantPreviewAnim(Card card)
        {
            if (card == null)
                return false;
            if (card.pile != player.getHand())
                return false;

            tryFinishServantPreviewAnim(card.id);

            var handPosition = card.pile.indexOf(card);
            // 有这张手牌。
            if (tryGetHand(card, out HandItem hand))
            {
                hand.ignoreAlign = false;
                return true;
            }

            // 没有这张手牌。
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
            return true;
        }
        private bool tryFinishServantPreviewAnim(int cardId)
        {
            if (!_servantPreviewAnimDict.TryGetValue(cardId, out var anim))
                return false;

            if (anim.isStarted() && !anim.isFinished())
            {
                anim.forceFinish(this);
            }
            removeAnim(anim);
            _servantPreviewAnimDict.Remove(cardId);
            return true;
        }
        #endregion

        #region 箭头
        /// <summary>
        /// 在目标选择期间，更新箭头。
        /// </summary>
        /// <param name="position"></param>
        private void updateArrowWhileSelection(Vector3 position)
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
        #endregion

        #region 锁定准星
        private void displaySelectionTarget(Card card, int targetIndex)
        {
            Transform transform = null;
            if (tryGetMaster(card, out var masterUI))
            {
                transform = masterUI.transform;
            }
            else if (tryGetServant(card, out var servantUI))
            {
                transform = servantUI.transform;
            }
            var tablePos = WorldToTablePosition(transform.position) + _selectionTargetOffset;
            var target = Instantiate(table.SelectionTargetTemplate, TableToWorldPosition(tablePos), Quaternion.identity, transform).GetComponent<SelectionTarget>();
            _selectionTargetDict.Add(card.id, target);
        }
        private void hideAllSelectionTargets()
        {
            foreach (var pair in _selectionTargetDict)
            {
                Destroy(pair.Value.gameObject);
            }
            _selectionTargetDict.Clear();
        }
        #endregion

        /// <summary>
        /// 判断鼠标是否在某张卡上，并返回卡
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        private bool isOnTargetCard(PointerEventData pointer, out Card card)
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

        #endregion

        #region 属性字段
        [TranslateMsg]
        private const string TAB_THIS_INVALIDATION = "这不是一个有效的目标！";
        /// <summary>
        /// 当前正在使用/攻击的卡片的引用
        /// </summary>
        public Card targetSelectingCard { get; set; } = null;
        /// <summary>
        /// 目标选择类型
        /// </summary>
        public TargetSelectType targetSelectType { get; set; }
        /// <summary>
        /// 目标选择模式
        /// </summary>
        public TargetSelectMode targetSelectMode { get; set; }

        [SerializeField]
        private Vector3 _selectionTargetOffset = new Vector3(0, 500, 0);

        private List<Card> _selectableTargets = new List<Card>();
        private List<Card> _selectedTargets = new List<Card>();
        private List<BuffEffectPair> _selectingBuffEffects = new List<BuffEffectPair>();
        private Dictionary<int, SelectionTarget> _selectionTargetDict = new Dictionary<int, SelectionTarget>();
        #endregion
    }

    public enum TargetSelectType
    {
        None,
        Use,
        ServantBattlecry,
        Attack
    }
    public enum TargetSelectMode
    {
        Click,
        Drag
    }
}
