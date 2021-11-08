using UnityEngine;
using UI;
using TouhouHeartstone;
using UnityEngine.EventSystems;
using Card = TouhouCardEngine.Card;
using BJSYGameCore.UI;

namespace Game
{
    partial class TableManager
    {
        public bool tryGetSkill(Card card, out Skill skill)
        {
            if (card == player.getSkill())
            {
                skill = ui.SelfSkill;
                return true;
            }
            else if (card == game.getOpponent(player).getSkill())
            {
                skill = ui.EnemySkill;
                return true;
            }
            else
            {
                skill = null;
                return false;
            }
        }
        /// <summary>
        /// 设置Skill的UI
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="card"></param>
        public async void setSkill(Skill skill, Card card)
        {
            CardSkinData skin = getSkin(card);
            skill.costPropNumber.text.text = card.getCost(game).ToString();
            if (card.isUsed(game))
            {
                // skill.IsUsedController = Skill.IsUsed.True;
                skill.isUsed = true;
            }
            else
            {
                // skill.IsUsedController = Skill.IsUsed.False;
                skill.isUsed = false;
            }
            if (card.isUsable(game, player, out _) &&//技能是可用的
                !isSelectingTarget &&//没有在选择目标
                canControl//是自己的回合
                )
            {
                // skill.IsUsableController = Skill.IsUsable.True;
                skill.isUsable = true;
                skill.asButton.interactable = true;
            }
            else
            {
                // skill.IsUsableController = Skill.IsUsable.False;
                skill.isUsable = false;
                skill.asButton.interactable = false;
            }
            if (skin != null && skin.image != null)
                skill.image.sprite = skin.image;
            else
                skill.image.sprite = await getManager<CardManager>().loadCardImage(card.define.id);
        }
        /// <summary>
        /// 技能点击处理
        /// </summary>
        void onClickSkill()
        {
            if (!canControl)
                return;
            if (tryCancelUse())
                return;
            if (usingCard != null)//已经在用别的牌了，不能点技能
                return;
            if (player.getSkill().isUsable(game, player, out var info))
            {
                if (!player.getSkill().isNeedTarget(game, out _))
                    player.cmdUse(game, player.getSkill());
            }
            else
            {
                showTip(info);
            }
        }
        /// <summary>
        /// 技能拖拽
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="pointer"></param>
        void onDragSkill(Skill skill, PointerEventData pointer)
        {
            //TODO:按帧记录Replay
            if (!canControl)
                return;
            // Stage2: 当前正在拖拽，判断距离以决定是否退出目标选择模式
            if (usingCard == player.getSkill())
            {
                if (Vector3.Distance(skill.rectTransform.position, pointer.position) < skillDragThreshold)
                    cancelSkill();
                else
                    displayArrow(skill.rectTransform, skill.rectTransform.position, pointer.position);
                return;
            }
            // Stage1: 当前没有拖拽，进入目标选择模式
            if (usingCard != null)
                return;//已经在用别的牌了
            if (Vector3.Distance(skill.rectTransform.position, pointer.position) < skillDragThreshold)
                return;
            if (player.getSkill().isUsable(game, player, out var info))
            {
                if (player.getSkill().isNeedTarget(game, out var targets))
                {
                    usingCard = player.getSkill();
                    highlightTargets(targets);
                    displayArrow(skill.rectTransform, skill.rectTransform.position, pointer.position);
                }
            }
            else
                showTip(info);
        }
        /// <summary>
        /// 技能结束拖拽
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="pointer"></param>
        void onDragSkillEnd(Skill skill, PointerEventData pointer)
        {
            if (!canControl)
            {
                cancelSkill();
                return;
            }
            if (usingCard != null && usingCard != player.getSkill())
                return;//在用其他卡片
            if (usingCard != player.getSkill())
            {
                cancelSkill();
                return;
            }
            if (isOnTarget(pointer, out var target))
            {
                if (player.getSkill().isUsable(game, player, out var info) && player.getSkill().isValidTarget(game, target))
                    player.cmdUse(game, player.getSkill(), 0, target);
                else
                    showTip(info);
            }
            cancelSkill();
        }
        private void onSkillPointerExit(Skill arg1, PointerEventData arg2)
        {
            ui.TipBox.hide();
        }

        private void onSelfSkillPointerEnter(Skill skill, PointerEventData pointer)
        {
            ui.TipBox.display(getSkin(player.getSkill()).desc, pointer.position + _tipBoxOffset);
        }

        private void onEnemySkillPointerEnter(Skill skill, PointerEventData pointer)
        {
            ui.TipBox.display(getSkin(game.getOpponent(player).getSkill()).desc, pointer.position + _tipBoxOffset);
        }

        private void onSkillPointerMove(Skill arg1, Vector2 pointer)
        {
            ui.TipBox.updatePos(pointer);
        }
        /// <summary>
        /// 取消正在执行的技能UI
        /// </summary>
        private void cancelSkill()
        {
            usingCard = null;
            removeHighlights();
            hideArrow();
        }
        /// <summary>
        /// 显示箭头
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
    }
}
