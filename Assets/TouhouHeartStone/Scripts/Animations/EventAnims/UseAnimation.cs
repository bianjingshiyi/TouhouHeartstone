using TouhouHeartstone;
using BJSYGameCore;
using UnityEngine;
using UI;
using BJSYGameCore.UI;
using System.Linq;
using TouhouCardEngine;
using System.Collections;

namespace Game
{
    using Arg = HearthStoneRule.UseEventArg;
    class UseAnimation : EventAnimation<Arg>
    {
        CardSnapshot _card;
        public override IEnumerator update(TableManager table, EventRecord record, Arg arg)
        {
            while (!record.isCompleted)
            {
                yield return null;
            }
            yield return updateCard(table, record, arg);

            var player = arg.player;
            if (table.game.answers.getRequests(player.id).FirstOrDefault() is DiscoverRequest && player != table.player)
                table.ui.Waiting.SetActive(true);
        }
        private IEnumerator updateCard(TableManager table, EventRecord record, Arg arg)
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
        private IEnumerator updateServant(TableManager table, EventRecord record, Arg arg)
        {
            var player = arg.player;
            var isCanceled = record.isCanceled;
            var position = arg.position;
            if (player == table.player)
            {
                if (!isCanceled)
                {
                    if (!table.ui.ServantPlaceHolder.isDisplaying)
                    {
                        HandListItem item = table.getHand(_card);
                        var moveAnim = new HandToFieldAnim(table, item, table.ui.SelfFieldList, position);
                        while (!moveAnim.update(table, _time))
                            yield return null;
                    }
                    //table.ui.SelfHandList.removeItem(table.getHand(_card));
                    //将使用随从的移除手牌转移至TryEnterFiledAnim
                    yield return tryTargetedAnim(table, table.ui.ServantPlaceHolder, record, arg);
                    table.ui.addChild(table.ui.ServantPlaceHolder.rectTransform);
                    table.ui.ServantPlaceHolder.hide();
                }
                else
                {
                    HandListItem item = table.getHand(_card);
                    if (table.ui.ServantPlaceHolder.isDisplaying)
                    {
                        table.ui.addChild(table.ui.ServantPlaceHolder.rectTransform);
                        table.ui.ServantPlaceHolder.hide();
                    }

                    item.Card.display();
                    table.setCard(item.Card, _card, true);
                    var moveAnim = new CardTransAnim(item.Card, item.rectTransform.position, table.handToFieldCurve);

                    while (!moveAnim.update(table, _time))
                        yield return null;
                }
            }
            else
            {
                table.tryGetHand(_card, out var hand);

                yield return playEnemyUseCardAnim(table, hand, record);

                if (isCanceled)
                {
                    cancelEnemyUseCard(table, record);
                }
                table.ui.addChild(table.ui.ServantPlaceHolder.rectTransform);
                table.ui.ServantPlaceHolder.hide();
            }
        }
        #endregion

        #region Spell
        private IEnumerator updateSpell(TableManager table, EventRecord record, Arg arg)
        {
            
            var player = arg.player;
            var isCanceled = record.isCanceled;
            if (player == table.player)
            {
                if (!isCanceled)
                {
                    if (table.tryGetHand(_card, out var hand))
                    {
                        AnimAnim useAnim = null;
                        while (!SimpleAnimHelper.update(table, ref useAnim, hand.onSelfUse, hand.animator, _time))
                            yield return null;
                        table.removeHand(_card);
                    }
                    table.getManager<SEManager>().playSE(table.getSkin(_card).sound_onUse);
                    yield return tryTargetedAnim(table, table.ui.SelfMaster, record, arg);
                }
                else
                {
                    HandListItem item = table.getHand(_card);
                    item.Card.display();
                    table.setCard(item.Card, _card, true);
                    var moveAnim = new CardTransAnim(item.Card, item.rectTransform.position, table.handToFieldCurve);
                    while (!moveAnim.update(table, _time))
                        yield return null;
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
                    yield return tryTargetedAnim(table, table.ui.EnemyMaster, record, arg);
                }
                else
                {
                    cancelEnemyUseCard(table, record);
                }
            }
        }
        #endregion

        #region Skill
        private IEnumerator updateSkill(TableManager table, EventRecord record, Arg arg)
        {
            
            var player = arg.player;
            var isCanceled = record.isCanceled;
            if (isCanceled)
                yield break;

            Skill sourceActor = player == table.player ? table.ui.SelfSkill : table.ui.EnemySkill;

            table.setSkill(sourceActor, _card);

            CardSkinData skin = table.getSkin(_card);
            yield return loadAndPlaySound(table, skin?.soundPath_onUse);

            yield return tryTargetedAnim(table, sourceActor, record, arg);
        }
        #endregion

        #region Item
        private IEnumerator updateItem(TableManager table, EventRecord record, Arg arg)
        {
            
            var player = arg.player;
            var isCanceled = record.isCanceled;
            if (player == table.player)
            {
                if (!isCanceled)
                {
                    if (table.tryGetHand(_card, out var hand))
                    {
                        AnimAnim useAnim = null;
                        while (!SimpleAnimHelper.update(table, ref useAnim, hand.onSelfUse, hand.animator, _time))
                            yield return null;
                        table.removeHand(_card);
                    }
                    yield return tryTargetedAnim(table, table.ui.SelfItem, record, arg);
                }
                else
                {
                    HandListItem item = table.getHand(_card);
                    item.Card.display();
                    table.setCard(item.Card, _card, true);
                    var moveAnim = new CardTransAnim(item.Card, item.rectTransform.position, table.handToFieldCurve);
                    while (!moveAnim.update(table, _time))
                        yield return null;
                }
            }
            else
            {
                table.tryGetHand(_card, out var hand);

                yield return playEnemyUseCardAnim(table, hand, record);

                if (!isCanceled)
                {
                    yield return tryTargetedAnim(table, table.ui.EnemyItem, record, arg);
                }
                else
                {
                    cancelEnemyUseCard(table, record);
                }
            }
        }
        #endregion

        IEnumerator playEnemyUseCardAnim(TableManager table, HandListItem hand, EventRecord record)
        {
            
            // 播放使用动画。
            if (hand)
            {
                table.setCard(hand.Card, _card, !_card.isTrap(table.game));
                hand.GetComponentInChildren<PositionLerp>().setTarget(table.ui.getChild("SpellDisplay"));
                AnimAnim useAnim = null;
                while (!SimpleAnimHelper.update(table, ref useAnim, hand.onEnemyUse, hand.animator, _time))
                    yield return null;
                table.removeHand(_card);
            }
        }
        void cancelEnemyUseCard(TableManager table, EventRecord record)
        {
            
            table.createHand(_card, _card.position);
        }
        IEnumerator loadAndPlaySound(TableManager table, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                AudioClip sound;
                var loadSoundTask = table.getManager<THHResourceManager>().loadAudio(path);
                while (!loadSoundTask.IsCompleted)
                    yield return null;

                sound = loadSoundTask.Result;
                table.getManager<SEManager>().playSE(sound);
            }
        }
        IEnumerator tryTargetedAnim(TableManager table, UIObject sourceActor, EventRecord record, Arg arg)
        {
            
            var targets = arg.targets;
            if (targets != null && targets.Length > 0 && targets[0] is var targetCard)
            {
                if (table.getSkin(_card) is var skin && !string.IsNullOrEmpty(skin.projectilePath))
                {
                    var loadProjectileTask = table.getManager<THHResourceManager>().loadObject<Projectile>(skin.projectilePath);

                    while (!loadProjectileTask.IsCompleted)
                        yield return null;

                    UIObject targetActor;
                    if (table.tryGetMaster(targetCard, out var targetMaster))
                        targetActor = targetMaster;
                    else if (table.tryGetServant(targetCard, out var targetServant))
                        targetActor = targetServant;
                    else
                        throw new ActorNotFoundException(targetCard);

                    var targetedAnim = new ProjectileAnimation(
                        table.createProjectile(loadProjectileTask.Result, sourceActor.rectTransform, targetActor.rectTransform),
                        targetActor);

                    while (!targetedAnim.update(table, _time))
                        yield return null;
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
                        animator = targetServant.servantAnimator;
                    }
                    else
                        throw new ActorNotFoundException(targetCard);

                    AnimAnim targetedAnim = null;
                    while (!SimpleAnimHelper.update(table, ref targetedAnim, simpleAnim, animator, _time, next => false))
                        yield return null;
                }
            }
        }

        public override bool blockAnim(UIAnimation nextAnim)
        {
            if (!record.isCompleted)
            {
                return false;
            }
            return base.blockAnim(nextAnim);
        }
    }
}
