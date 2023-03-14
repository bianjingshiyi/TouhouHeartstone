using TouhouHeartstone;
using BJSYGameCore;
using UnityEngine;
using UI;
using BJSYGameCore.UI;
using System.Linq;
using System.Threading.Tasks;

namespace Game
{
    class UseAnimation : EventAnimation<HearthStoneRule.UseEventArg>
    {
        TableAnimation _moveAnim;
        AnimAnim _useAnim;
        TableAnimation _targetedAnim;
        bool _enemyAnimPlayed;
        Task<AudioClip> _loadSoundTask;
        Task<Projectile> _loadProjectileTask;


        public override bool update(TableManager table, HearthStoneRule.UseEventArg eventArg, float time)
        {
            if (!updateCard(table, eventArg, time))
            {
                return false;
            }
            if (table.game.answers.getRequests(eventArg.player.id).FirstOrDefault() is DiscoverRequest && eventArg.player != table.player)
                table.ui.Waiting.SetActive(true);
            return true;
        }
        private bool updateCard(TableManager table, HearthStoneRule.UseEventArg eventArg, float time)
        {
            if (eventArg.card.isServant())
            {
                return updateServant(table, eventArg, time);
            }
            else if (eventArg.card.isSpell())
            {
                return updateSpell(table, eventArg, time);
            }
            else if (eventArg.card.isSkill())
            {
                return updateSkill(table, eventArg, time);
            }
            else if (eventArg.card.isItem())
            {
                return updateItem(table, eventArg, time);
            }
            return true;
        }
        #region Servant
        private bool updateServant(TableManager table, HearthStoneRule.UseEventArg eventArg, float time)
        {
            if (eventArg.player == table.player)
            {
                if (!eventArg.isCanceled)
                {
                    if (!table.ui.ServantPlaceHolder.isDisplaying)
                    {
                        HandListItem item = table.getHand(eventArg.card);
                        if (_moveAnim == null)
                            _moveAnim = new HandToFieldAnim(table, item, table.ui.SelfFieldList, eventArg.position);
                        if (!_moveAnim.update(table, time))
                            return false;
                    }
                    //table.ui.SelfHandList.removeItem(table.getHand(eventArg.card));
                    //将使用随从的移除手牌转移至TryEnterFiledAnim
                    if (tryTargetedAnim(table, table.ui.ServantPlaceHolder, eventArg, time))
                        return false;
                    table.ui.addChild(table.ui.ServantPlaceHolder.rectTransform);
                    table.ui.ServantPlaceHolder.hide();
                }
                else
                {
                    HandListItem item = table.getHand(eventArg.card);
                    if (item == null)
                        throw new ActorNotFoundException(eventArg.card);
                    if (table.ui.ServantPlaceHolder.isDisplaying)
                    {
                        table.ui.addChild(table.ui.ServantPlaceHolder.rectTransform);
                        table.ui.ServantPlaceHolder.hide();
                    }
                    if (_moveAnim == null)
                    {
                        item.Card.display();
                        table.setCard(item.Card, eventArg.card, true);
                        _moveAnim = new CardTransAnim(item.Card, item.rectTransform.position, table.handToFieldCurve);
                    }
                    if (!_moveAnim.update(table, time))
                        return false;
                }
            }
            else
            {
                table.tryGetHand(eventArg.card, out var hand);

                if (!playEnemyUseCardAnim(table, hand, eventArg, time))
                {
                    return false;
                }
                else
                {
                    if (!eventArg.isCanceled)
                    {
                        if (hand != null)
                        {
                            if (_moveAnim == null)
                                _moveAnim = new HandToFieldAnim(table, hand, table.ui.EnemyFieldList, eventArg.position);
                            if (!_moveAnim.update(table, time))
                                return false;
                        }
                        if (tryTargetedAnim(table, table.ui.ServantPlaceHolder, eventArg, time))
                            return false;
                    }
                    else
                    {
                        cancelEnemyUseCard(table, eventArg);
                    }
                }
                table.ui.addChild(table.ui.ServantPlaceHolder.rectTransform);
                table.ui.ServantPlaceHolder.hide();
            }
            return true;
        }
        #endregion

        #region Spell
        private bool updateSpell(TableManager table, HearthStoneRule.UseEventArg eventArg, float time)
        {
            if (eventArg.player == table.player)
            {
                if (!eventArg.isCanceled)
                {
                    if (table.tryGetHand(eventArg.card, out var hand))
                    {
                        if (!SimpleAnimHelper.update(table, ref _useAnim, hand.onSelfUse, hand.animator, time))
                            return false;
                        table.removeHand(eventArg.card);
                    }
                    table.getManager<SEManager>().playSE(table.getSkin(eventArg.card).sound_onUse);
                    if (tryTargetedAnim(table, table.ui.SelfMaster, eventArg, time))
                        return false;
                }
                else
                {
                    HandListItem item = table.getHand(eventArg.card);
                    if (item == null)
                        throw new ActorNotFoundException(eventArg.card);
                    if (_moveAnim == null)
                    {
                        item.Card.display();
                        table.setCard(item.Card, eventArg.card, true);
                        _moveAnim = new CardTransAnim(item.Card, item.rectTransform.position, table.handToFieldCurve);
                    }
                    if (!_moveAnim.update(table, time))
                        return false;
                }
            }
            else
            {
                table.tryGetHand(eventArg.card, out var hand);

                if (!playEnemyUseCardAnim(table, hand, eventArg, time))
                {
                     return false;
                }
                else
                {
                    if (!eventArg.isCanceled)
                    {
                        // 播放音效。
                        CardSkinData skin = table.getSkin(eventArg.card);
                        bool soundLoaded = loadAndPlaySound(table, skin?.soundPath_onUse);
                        if (!soundLoaded)
                        {
                            return false;
                        }

                        if (tryTargetedAnim(table, table.ui.EnemyMaster, eventArg, time))
                            return false;
                    }
                    else
                    {
                        cancelEnemyUseCard(table, eventArg);
                    }
                }
            }
            return true;
        }
        #endregion

        #region Skill
        private bool updateSkill(TableManager table, HearthStoneRule.UseEventArg eventArg, float time)
        {
            if (eventArg.isCanceled)
            {
                return true;
            }

            Skill sourceActor = eventArg.player == table.player ? table.ui.SelfSkill : table.ui.EnemySkill;

            table.setSkill(sourceActor, eventArg.card);

            CardSkinData skin = table.getSkin(eventArg.card);
            loadAndPlaySound(table, skin?.soundPath_onUse);

            if (tryTargetedAnim(table, sourceActor, eventArg, time))
                return false;
            return true;
        }
        #endregion

        #region Item
        private bool updateItem(TableManager table, HearthStoneRule.UseEventArg eventArg, float time)
        {
            if (eventArg.player == table.player)
            {
                if (!eventArg.isCanceled)
                {
                    if (table.tryGetHand(eventArg.card, out var hand))
                    {
                        if (!SimpleAnimHelper.update(table, ref _useAnim, hand.onSelfUse, hand.animator, time))
                            return false;
                        table.removeHand(eventArg.card);
                    }
                    table.setMaster(table.ui.SelfMaster, table.player.getMaster());
                    if (tryTargetedAnim(table, table.ui.SelfItem, eventArg, time))
                        return false;
                }
                else
                {
                    HandListItem item = table.getHand(eventArg.card);
                    if (item == null)
                        throw new ActorNotFoundException(eventArg.card);
                    if (_moveAnim == null)
                    {
                        item.Card.display();
                        table.setCard(item.Card, eventArg.card, true);
                        _moveAnim = new CardTransAnim(item.Card, item.rectTransform.position, table.handToFieldCurve);
                    }
                    if (!_moveAnim.update(table, time))
                        return false;
                }
            }
            else
            {
                table.tryGetHand(eventArg.card, out var hand);
                if (!playEnemyUseCardAnim(table, hand, eventArg, time))
                {
                    return false;
                }
                else
                {
                    if (!eventArg.isCanceled)
                    {
                        table.setMaster(table.ui.EnemyMaster, table.game.getOpponent(table.player).getMaster());
                        if (tryTargetedAnim(table, table.ui.EnemyItem, eventArg, time))
                            return false;
                    }
                    else
                    {
                        cancelEnemyUseCard(table, eventArg);
                    }
                }
            }
            return true;
        }
        #endregion

        bool playEnemyUseCardAnim(TableManager table, HandListItem hand, HearthStoneRule.UseEventArg eventArg, float time)
        {
            if (!_enemyAnimPlayed)
            {
                // 播放使用动画。
                if (hand != null)
                {
                    if (eventArg.card.hasKeyword(table.game, KEYWORD.SECRET))
                        table.setCard(hand.Card, eventArg.card, false);
                    else
                        table.setCard(hand.Card, eventArg.card, true);
                    hand.GetComponentInChildren<PositionLerp>().setTarget(table.ui.getChild("SpellDisplay"));
                    if (!SimpleAnimHelper.update(table, ref _useAnim, hand.onEnemyUse, hand.animator, time))
                        return false;
                    table.removeHand(eventArg.card);
                }
                _enemyAnimPlayed = true;
            }
            return true;
        }
        void cancelEnemyUseCard(TableManager table, HearthStoneRule.UseEventArg eventArg)
        {
            var card = eventArg.card;
            table.createHand(card, card.pile.indexOf(card));
        }
        bool loadAndPlaySound(TableManager table, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                AudioClip sound;
                if (_loadSoundTask == null)
                {
                    _loadSoundTask = table.getManager<THHResourceManager>().loadAudio(path);
                }
                if (!_loadSoundTask.IsCompleted)
                    return false;
                else
                {
                    sound = _loadSoundTask.Result;
                    _loadSoundTask = null;
                }
                table.getManager<SEManager>().playSE(sound);
            }
            return true;
        }
        bool tryTargetedAnim(TableManager table, UIObject sourceActor, HearthStoneRule.UseEventArg eventArg, float time)
        {
            if (eventArg.targets != null && eventArg.targets.Length > 0 && eventArg.targets[0] is var targetCard)
            {
                if (table.getSkin(eventArg.card) is var skin && !string.IsNullOrEmpty(skin.projectilePath))
                {
                    if (_loadProjectileTask == null)
                    {
                        _loadProjectileTask = table.getManager<THHResourceManager>().loadObject<Projectile>(skin.projectilePath);
                    }
                    if (!_loadProjectileTask.IsCompleted)
                        return true;
                    else
                    {
                        UIObject targetActor;
                        if (table.tryGetMaster(targetCard, out var targetMaster))
                            targetActor = targetMaster;
                        else if (table.tryGetServant(targetCard, out var targetServant))
                            targetActor = targetServant;
                        else
                            throw new ActorNotFoundException(targetCard);
                        if (_targetedAnim == null)
                        {
                            _targetedAnim = new ProjectileAnimation(
                                table.createProjectile(_loadProjectileTask.Result, sourceActor.rectTransform, targetActor.rectTransform),
                                targetActor);
                        }
                        if (!_targetedAnim.update(table, time))
                            return true;
                    }
                }
                else
                {
                    SimpleAnim simpleAnim = null;
                    Animator animator = null;
                    if (_targetedAnim == null)
                    {
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
                    }
                    if (!SimpleAnimHelper.update(table, ref _targetedAnim, simpleAnim, animator, time, next => false))
                        return true;
                }
            }
            return false;
        }
    }
}
