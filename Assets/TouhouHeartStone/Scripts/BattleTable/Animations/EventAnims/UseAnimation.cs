using System.Collections;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using ActorNotFoundException = UI.ActorNotFoundException;
using CardSkinData = UI.CardSkinData;
using SimpleAnim = UI.SimpleAnim;

namespace Game.BattleTable
{
    using Arg = HearthStoneRule.UseEventArg;
    class UseAnimation : EventAnimation<Arg>
    {
        CardSnapshot _card;
        public override IEnumerator logicUpdate(BattleTableManager table, EventRecord record, Arg arg)
        {
            yield return pauseWhileNotCompleted();
            yield return updateCard(table, record, arg);
        }
        private IEnumerator updateCard(BattleTableManager table, EventRecord record, Arg arg)
        {
            _card = table.getRecordCardSnapshot(record, EVENTVAR.CARD);
            if (_card.isServant())
            {
                yield return updateServant(table, record, arg);
            }
            else if (_card.isSpell())
            {
                yield return updateSpell(table, record, arg);
            }
            else if (_card.isSkill())
            {
                yield return updateSkill(table, record, arg);
            }
            else if (_card.isItem())
            {
                yield return updateItem(table, record, arg);
            }
        }
        #region Servant
        private IEnumerator updateServant(BattleTableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;
            var isCanceled = record.isCanceled;
            if (player == table.player)
            {
                // 自己使用
                if (!isCanceled)
                {
                    // 成功使用。

                    // 播放目标被指向的动画。
                    var transform = table.transform;
                    if (table.tryGetServant(_card.id, out Servant servant))
                    {
                        transform = servant.transform;
                    }
                    yield return tryTargetedAnim(table, transform, record, arg);
                }
                else
                {
                    // 被取消。
                    if (table.tryGetHand(_card, out var item)) // 如果有这张手牌
                    {
                        // 重置手牌显示。
                        table.resetHand(_card);
                    }
                    else // 没有这张手牌。
                    {
                        var handPosition = _card.position;
                        table.updateCardDescriptor(_card);
                        // 如果有这个随从，播放回手动画。
                        if (table.tryGetServant(_card, out _))
                        {
                            var moveAnim = new FieldToHandAnim(_card.id, handPosition);
                            while (!moveAnim.update(table, _deltaTime))
                                yield return null;
                        }
                        else //没有这个随从，直接创建手牌。
                        {
                            table.createHand(table.getCardDescriptor(_card.id), handPosition);
                        }
                    }
                }
            }
            else
            {
                if (isCanceled)
                {
                    table.tryGetHand(_card, out var hand);

                    yield return playEnemyUseCardAnim(table, hand, record);

                    cancelEnemyUseCard(table, record);
                }
            }
        }
        #endregion

        #region Spell
        private IEnumerator updateSpell(BattleTableManager table, EventRecord record, Arg arg)
        {

            var player = arg.player;
            var isCanceled = record.isCanceled;
            if (player == table.player && !table.canSeeAllCards)
            {
                if (!isCanceled)
                {
                    if (table.tryGetHand(_card.id, out var hand))
                    {
                        AnimAnim useAnim = null;
                        while (!SimpleAnimHelper.update(table, ref useAnim, hand.onSelfUse, hand.animator, _deltaTime))
                            yield return null;
                        table.removeHand(_card);
                    }
                    table.sounds.playSE(table.getSkin(_card).sound_onUse);
                    yield return tryTargetedAnim(table, table.table.SelfMaster.transform, record, arg);
                }
                else
                {
                    var handPosition = _card.position;
                    if (table.tryGetHand(_card, out _)) // 手上有这张牌
                    {
                        // 重置手牌显示。
                        table.resetHand(_card);
                    }
                    else //没有这个手牌，直接创建手牌。
                    {
                        table.createHand(table.getCardDescriptor(_card.id), handPosition);
                    }
                }
            }
            else
            {
                table.tryGetHand(_card, out var hand);

                yield return playEnemyUseCardAnim(table, hand, record);

                if (!isCanceled)
                {
                    // 播放音效。
                    CardSkinData skin = table.getSkin(_card);
                    yield return loadAndPlaySound(table, skin?.soundPath_onUse);
                    yield return tryTargetedAnim(table, table.table.EnemyMaster.transform, record, arg);
                }
                else
                {
                    cancelEnemyUseCard(table, record);
                }
            }
        }
        #endregion

        #region Skill
        private IEnumerator updateSkill(BattleTableManager table, EventRecord record, Arg arg)
        {

            var player = arg.player;
            var isCanceled = record.isCanceled;
            if (isCanceled)
                yield break;

            Skill sourceActor = player == table.player ? table.table.SelfSkill : table.table.EnemySkill;

            table.setSkill(sourceActor, _card);

            CardSkinData skin = table.getSkin(_card);
            yield return loadAndPlaySound(table, skin?.soundPath_onUse);

            yield return tryTargetedAnim(table, sourceActor.transform, record, arg);
        }
        #endregion

        #region Item
        private IEnumerator updateItem(BattleTableManager table, EventRecord record, Arg arg)
        {

            var player = arg.player;
            var isCanceled = record.isCanceled;
            if (player == table.player && !table.canSeeAllCards)
            {
                if (!isCanceled)
                {
                    if (table.tryGetHand(_card, out var hand))
                    {
                        AnimAnim useAnim = null;
                        while (!SimpleAnimHelper.update(table, ref useAnim, hand.onSelfUse, hand.animator, _deltaTime))
                            yield return null;
                        table.removeHand(_card);
                    }
                    yield return tryTargetedAnim(table, table.table.SelfItem.transform, record, arg);
                }
                else
                {
                    var handPosition = _card.position;
                    if (table.tryGetHand(_card, out _)) // 手上有这张牌
                    {
                        // 重置手牌显示。
                        table.resetHand(_card);
                    }
                    else //没有这个手牌，直接创建手牌。
                    {
                        table.createHand(table.getCardDescriptor(_card.id), handPosition);
                    }
                }
            }
            else
            {
                table.tryGetHand(_card, out var hand);

                yield return playEnemyUseCardAnim(table, hand, record);

                if (!isCanceled)
                {
                    yield return tryTargetedAnim(table, table.table.EnemyItem.transform, record, arg);
                }
                else
                {
                    cancelEnemyUseCard(table, record);
                }
            }
        }
        #endregion

        IEnumerator playEnemyUseCardAnim(BattleTableManager table, HandItem hand, EventRecord record)
        {

            // 播放使用动画。
            if (hand)
            {
                table.setCard(hand.card, _card, !_card.isTrap(table.game) || table.canSeeCardOfOwner(_card.owner));
                hand.positionLerp.setStartPositionToCurrent();
                hand.positionLerp.setTargetTransform(table.table.SpellDisplayTransform);
                AnimAnim useAnim = null;
                while (!SimpleAnimHelper.update(table, ref useAnim, hand.onEnemyUse, hand.animator, _deltaTime))
                    yield return null;
                table.removeHand(_card);
            }
        }
        void cancelEnemyUseCard(BattleTableManager table, EventRecord record)
        {
            table.createHand(_card, _card.position);
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
        IEnumerator tryTargetedAnim(BattleTableManager table, Transform source, EventRecord record, Arg arg)
        {
            var targets = arg.targets;
            if (targets != null && targets.Length > 0 && targets[0] is var targetCard)
            {
                if (table.getSkin(_card) is var skin && !string.IsNullOrEmpty(skin.projectilePath))
                {
                    var loadProjectileTask = table.resources.loadObject<Projectile>(skin.projectilePath);

                    while (!loadProjectileTask.IsCompleted)
                        yield return null;

                    var projectile = loadProjectileTask.Result;
                    if (projectile)
                    {
                        Transform target;
                        if (table.tryGetMaster(targetCard, out var targetMaster))
                            target = targetMaster.transform;
                        else if (table.tryGetServant(targetCard, out var targetServant))
                            target = targetServant.transform;
                        else
                            throw new ActorNotFoundException(targetCard);

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
                    if (table.tryGetMaster(targetCard, out var targetMaster))
                    {
                        simpleAnim = targetMaster.onTargeted;
                        animator = targetMaster.animator;
                    }
                    else if (table.tryGetServant(targetCard, out var targetServant))
                    {
                        simpleAnim = targetServant.onTargeted;
                        animator = targetServant.animator;
                    }
                    else
                        throw new ActorNotFoundException(targetCard);

                    AnimAnim targetedAnim = null;
                    while (!SimpleAnimHelper.update(table, ref targetedAnim, simpleAnim, animator, _deltaTime, next => false))
                        yield return null;
                }
            }
        }
    }
}
