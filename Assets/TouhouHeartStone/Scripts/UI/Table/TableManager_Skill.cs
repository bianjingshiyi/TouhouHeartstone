using UnityEngine;
using UI;
using TouhouHeartstone;
using UnityEngine.EventSystems;
using Card = TouhouCardEngine.Card;
namespace Game
{
    partial class TableManager
    {
        public bool tryGetSkill(Card card, out Skill skill)
        {
            if (card == player.skill)
            {
                skill = ui.SelfSkill;
                return true;
            }
            else if (card == game.getOpponent(player).skill)
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
        public void setSkill(Skill skill, Card card)
        {
            CardSkinData skin = getSkin(card);
            skill.image.sprite = skin.image;
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
            if (player.skill.isUsable(game, player, out var info))
            {
                if (!player.skill.isNeedTarget(game, out _))
                    player.cmdUse(game, player.skill);
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
            if (usingCard == player.skill)
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
            if (player.skill.isUsable(game, player, out var info))
            {
                if (player.skill.isNeedTarget(game, out var targets))
                {
                    usingCard = player.skill;
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
            if (usingCard != null && usingCard != player.skill)
                return;//在用其他卡片
            if (usingCard != player.skill)
            {
                cancelSkill();
                return;
            }
            if (isOnTarget(pointer, out var target))
            {
                if (player.skill.isUsable(game, player, out var info) && player.skill.isValidTarget(game, target))
                    player.cmdUse(game, player.skill, 0, target);
                else
                    showTip(info);
            }
            cancelSkill();
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
