using System.Collections;
using TouhouCardEngine;
using TouhouCardEngine.Histories;
using TouhouHeartstone;
using UnityEngine;
using ActorNotFoundException = UI.ActorNotFoundException;
using CardSkinData = UI.CardSkinData;
using SimpleAnim = UI.SimpleAnim;
using Card = TouhouCardEngine.Card;

namespace Game.BattleTable
{
    using Arg = UseEventDefine;
    class UseAnimation : EventAnimation<Arg>
    {
        #region 公有方法
        public override IEnumerator startUpdate(BattleTableManager table)
        {
            _card = table.getEventCardSnapshot(startData, EVENTVAR.CARD);
            var player = startData.getVar<Player>(EVENTVAR.PLAYER);

            bool isEnemy = player != table.player;
            bool isReplay = table.canSeeAllCards;
            bool isServant = _card.isServant();
            bool showPlayedCard = isEnemy || isReplay;
            if (isServant)
            {
                yield return playServantToFieldAnim(table);
            }
            else if (showPlayedCard)
            {
                yield return playUseCardAnim(table);
            }
        }
        public override IEnumerator logicUpdate(BattleTableManager table)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            if (player == table.player)
                // 自己使用
                yield return selfUseCard(table);
            else
                // 对手使用
                yield return enemyUseCard(table);
        }
        public override IEnumerator canceledUpdate(BattleTableManager table)
        {
            // 被取消
            yield return cancelUseCard(table);
        }
        #endregion

        #region 私有方法

        #region 成功使用卡牌
        IEnumerator successUseCard(BattleTableManager table, Transform sourceTransform)
        {
            if (_card.isSkill())
            {
                if (table.tryGetSkill(_card, out var skillUI))
                {
                    table.setSkill(skillUI, _card);
                }
            }

            // 播放音效。
            CardSkinData skin = table.getCardSkin(_card);
            yield return loadAndPlaySound(table, skin?.soundPath_onUse);
            // 播放指向目标动画。
            yield return tryTargetedAnim(table, sourceTransform);
        }
        IEnumerator selfUseCard(BattleTableManager table)
        {
            var sourceTransform = table.table.SelfMaster.transform;
            if (_card.isServant())
            {
                // 自己成功使用仆从牌。
                if (table.tryGetServant(_card.id, out Servant servant))
                {
                    sourceTransform = servant.transform;
                }
            }
            else if (_card.isSpell() || _card.isItem())
            {
                // 自己成功使用法术牌/道具牌。
                if (table.tryGetHand(_card.id, out var hand))
                {
                    AnimAnim useAnim = null;
                    while (!SimpleAnimHelper.update(table, ref useAnim, hand.onSelfUse, hand.animator, _deltaTime))
                        yield return null;
                    table.removeHand(_card);
                }
            }
            yield return successUseCard(table, sourceTransform);
        }
        IEnumerator enemyUseCard(BattleTableManager table)
        {
            yield return successUseCard(table, table.table.EnemyMaster.transform);
        }
        #endregion

        #region 取消使用卡牌
        IEnumerator playServantToFieldAnim(BattleTableManager table)
        {
            if (table.tryGetHand(_card, out var hand))
            {
                var player = startData.getVar<Player>(EVENTVAR.PLAYER);
                var fieldIndex = startData.getVar<int>(EVENTVAR.POSITION);
                var pos = table.WorldToTablePosition(hand.transform.position);
                var moveAnim = new HandToFieldAnim(table.getCardDescriptor(_card.id), pos, player.getHand(), player.getField(), fieldIndex);
                while (!moveAnim.update(table, _deltaTime))
                    yield return null;
            }
        }
        IEnumerator cancelUseCard(BattleTableManager table)
        {
            // 使用卡牌被取消。
            if (table.tryGetHand(_card, out _))
                // 手上有这张牌，重置手牌显示。
                table.resetHand(_card);
            else
                // 没有这张手牌，回手。
                yield return returnToHand(table);
        }
        IEnumerator returnToHand(BattleTableManager table)
        {
            var handPosition = _card.position;
            if (_card.isServant())
            {
                // 使用仆从牌被取消。
                // 防止卡牌进入错误的玩家手牌中，先更新卡牌描述器。
                table.updateCardDescriptor(_card);
                // 如果有这个随从，播放回手动画。
                if (table.tryGetServant(_card, out _))
                {
                    var moveAnim = new FieldToHandAnim(_card.id, _card.owner.getField(), _card.owner.getHand(), handPosition);
                    while (!moveAnim.update(table, _deltaTime))
                        yield return null;
                }
                else
                {
                    // 直接创建手牌。
                    table.createHand(table.getCardDescriptor(_card.id), handPosition);
                }
            }
            else if (_card.isSpell() || _card.isSpell())
            {
                // 使用法术牌/道具牌被取消。
                // 直接创建手牌。
                table.createHand(table.getCardDescriptor(_card.id), handPosition);
            }
        }
        #endregion
        IEnumerator playUseCardAnim(BattleTableManager table)
        {
            // 播放使用动画。
            if (table.tryGetHand(_card, out var hand))
            {
                table.setCard(hand.card, _card);
                hand.card.setFaceup(!_card.isTrap(table.game) || table.canSeeCardOfOwner(_card.owner));
                hand.positionLerp.setStartPositionToCurrent();
                hand.positionLerp.setTargetTransform(table.table.SpellDisplayTransform);
                AnimAnim useAnim = null;
                while (!SimpleAnimHelper.update(table, ref useAnim, hand.onEnemyUse, hand.animator, _deltaTime))
                    yield return null;
                table.removeHand(_card);
            }
        }
        IEnumerator loadAndPlaySound(BattleTableManager table, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                AudioClip sound;
                var loadSoundTask = table.resources.loadAudio(path);
                while (!loadSoundTask.IsCompleted)
                    yield return null;

                sound = loadSoundTask.Result;
                table.sounds.playSE(sound);
            }
        }
        IEnumerator tryTargetedAnim(BattleTableManager table, Transform source)
        {
            var targets = eventArg.getVar<Card[]>(EVENTVAR.TARGETS);
            if (targets != null && targets.Length > 0 && targets[0] is var targetCard)
            {
                var card = targetCard;
                if (table.getCardSkin(_card) is var skin && !string.IsNullOrEmpty(skin.projectilePath))
                {
                    var loadProjectileTask = table.resources.loadObject<Projectile>(skin.projectilePath);

                    while (!loadProjectileTask.IsCompleted)
                        yield return null;

                    var projectile = loadProjectileTask.Result;
                    if (projectile)
                    {
                        Transform target;
                        if (table.tryGetMaster(card, out var targetMaster))
                            target = targetMaster.transform;
                        else if (table.tryGetServant(card, out var targetServant))
                            target = targetServant.transform;
                        else
                            throw new ActorNotFoundException(card);

                        var targetedAnim = new ProjectileAnimation(
                            table.createProjectile(projectile, source, target),
                            target);

                        while (!targetedAnim.update(table, _deltaTime))
                            yield return null;
                    }
                }
                else
                {
                    SimpleAnim simpleAnim = null;
                    Animator animator = null;
                    if (table.tryGetMaster(card, out var targetMaster))
                    {
                        simpleAnim = targetMaster.onTargeted;
                        animator = targetMaster.animator;
                    }
                    else if (table.tryGetServant(card, out var targetServant))
                    {
                        simpleAnim = targetServant.onTargeted;
                        animator = targetServant.animator;
                    }

                    if (animator != null)
                    {
                        AnimAnim targetedAnim = null;
                        while (!SimpleAnimHelper.update(table, ref targetedAnim, simpleAnim, animator, _deltaTime, next => false))
                            yield return null;
                    }
                }
            }
        }
        #endregion

        #region 属性字段
        CardSnapshot _card;
        #endregion
    }
}
