using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using Game.BattleTable;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Card = TouhouCardEngine.Card;
using UICard = UI.Card;

namespace Controllers
{
    public class GameLogCtrl : MonoBehaviour
    {
        public void setGame(CardEngine game, MainManager main, BattleTableManager battleTableManager, Player localPlayer)
        {
            _game = game;
            _main = main;
            _battleTableManager = battleTableManager;
            _localPlayer = localPlayer;
            game.triggers.onEventBefore += t1 => gameTriggerEventCache.cache(onEventBefore, t1);
            game.triggers.onEventAfter += t1 => gameTriggerEventCache.cache(onEventAfter, t1);
        }
        public void reset()
        {
            logItems.Clear();
            _logPanel.shrink();
            _logScrollView.LogList.updateLogListCount(0);
        }
        public void pollEvents()
        {
            gameTriggerEventCache.poll();
        }
        #region 生命周期
        private void Awake()
        {
            _logScrollView.LogList.onEnableLogListItem += onEnableLogListItem;
            _logScrollView.LogList.onDisableLogListItem += onDisableLogListItem;
            _logScrollView.LogList.onUpdateLogListItem += onUpdateLogListItem;
        }
        #endregion
        #region 事件回调
        void onEventBefore(IEventArg obj)
        {
            var define = obj.define;
            switch (define)
            {
                case UseEventDefine _:
                case BurnEventDefine _:
                case FatigueEventDefine _:
                case DiscardEventDefine _:
                case EnterFieldEventDefine _:
                case RebornEventDefine _:
                case AttackEventDefine _:
                case DamageEventDefine _:
                case SetArmorEventDefine _:
                case HealEventDefine _:
                case SilenceEventDefine _:
                case ShackleEventDefine _:
                case DeathEventDefine _:
                case RevealDuelEventDefine _:
                case SecretTriggerEventDefine _:
                    addLogItem(obj);
                    break;
                case DrawEventDefine _:
                    if (define is IDescribableEventDefine describable)
                    {
                        // string info = describable.toString(_game, localPlayer);
                        string info = lang._(
                            describable.localizeTemplateString(_game, obj as EventArg, _localPlayer),
                            describable.localizeStringArgs(_game, obj as EventArg, _localPlayer)
                        );
                        if (info != null)
                        {
                            addLogItem(obj);
                        }
                    }
                    break;
                case RevealEventDefine reveal:
                    if (obj.getVar<Card>(RevealEventDefine.VAR_CARD) != null)
                    {
                        addLogItem(obj);
                    }
                    break;
                case RevealCardDefineEventDefine revealCardDefine:
                    if (obj.getVar<CardDefine>(RevealCardDefineEventDefine.VAR_CARD_DEFINE) == null)
                        break;
                    bool isSelf = obj.getVar<Player>(RevealCardDefineEventDefine.VAR_PLAYER) == _localPlayer;
                    if (isSelf && obj.getVar<bool>(RevealCardDefineEventDefine.VAR_INVISIBLE_FOR_SELF))
                        break;
                    if (!isSelf && obj.getVar<bool>(RevealCardDefineEventDefine.VAR_INVISIBLE_FOR_OPPO))
                        break;
                    addLogItem(obj);
                    break;
                default:
                    break;
            }
        }
        void onEventAfter(IEventArg obj)
        {
            var item = logItems.FirstOrDefault(i => i.arg == obj);
            if (item == null)
                return;
            foreach (var parentItem in logItems)
            {
                int depth = 0;
                if (isChildOf(parentItem.arg, item.arg, ref depth))
                {
                    item.level = depth;
                    break;
                }
            }
            bool isChildOf(IEventArg parent, IEventArg child, ref int depth)
            {
                if (depth > 100)
                    throw new StackOverflowException();
                if (child == null)
                    return false;
                if (parent == null)
                    return false;
                if (child.parent == null)
                    return false;
                if (child.parent == parent)
                {
                    depth++;
                    return true;
                }
                else
                {
                    depth++;
                    return isChildOf(parent, child.parent, ref depth);
                }
            }
            _logScrollView.asScrollRect.verticalNormalizedPosition = 0;
        }
        private void onEnableLogListItem(int index, RectTransform rect)
        {
            var item = rect.GetComponent<LogListItem>();
            item.onPointerEnter.add(onLogListItemEnter);
            item.onPointerExit.add(onLogListItemExit);
        }
        private void onDisableLogListItem(int index, RectTransform rect)
        {
            var item = rect.GetComponent<LogListItem>();
            item.onPointerEnter.remove(onLogListItemEnter);
            item.onPointerExit.remove(onLogListItemExit);
        }
        private async void onUpdateLogListItem(int index, RectTransform rect)
        {
            var item = rect.GetComponent<LogListItem>();
            var logItem = logItems[index];
            var obj = logItem.arg;
            item.SetLevel(logItem.level);

            Sprite icon = item.defaultIcon;
            string text = string.Empty;
            Player player = null;
            var eventDefine = obj.define;

            DescribableCard descCard = null;
            if (eventDefine is IDescribableEventDefine descriable)
            {
                descCard = descriable.getCard(tableManager.game, obj as EventArg, _localPlayer);

                player = descriable.getPlayer(tableManager.game, obj as EventArg);

                var nameTemplate = descriable.localizeLogItemNameTemplate(tableManager.game, obj as EventArg);
                var nameArgs = descriable.localizeLogItemNameArgs(tableManager.game, obj as EventArg);
                if (nameTemplate != null)
                {
                    if (nameArgs != null)
                        text = lang._(nameTemplate, nameArgs);
                    else
                        text = lang._(nameTemplate);
                }
            }
            switch (eventDefine)
            {
                case DrawEventDefine:
                    icon = item.drawIcon;
                    break;
                case BurnEventDefine:
                    icon = item.burnIcon;
                    break;
                case FatigueEventDefine:
                    icon = item.fatigueIcon;
                    break;
                case DiscardEventDefine:
                    icon = item.discardIcon;
                    break;
                case UseEventDefine:
                    icon = item.useIcon;
                    break;
                case EnterFieldEventDefine:
                    icon = item.tokenIcon;
                    break;
                case RebornEventDefine:
                    icon = item.rebornIcon;
                    break;
                case AttackEventDefine:
                    icon = item.attackIcon;
                    break;
                case DamageEventDefine:
                    icon = item.damageIcon;
                    break;
                case SetArmorEventDefine:
                    icon = item.armorIcon;
                    break;
                case HealEventDefine:
                    icon = item.healIcon;
                    break;
                case SilenceEventDefine:
                    icon = item.silenceIcon;
                    break;
                case SecretTriggerEventDefine:
                    icon = item.secretIcon;
                    break;
                case DeathEventDefine:
                    icon = item.deathIcon;
                    break;
            }

            item.IconImage.sprite = icon;
            item.Text.text = text;
            item.BorderImage.sprite = player == _localPlayer ? item.selfsprite : item.opposprite;
            item.Image.sprite = await getCardImageByEvent(descCard, obj);
        }
        private async void onLogListItemEnter(LogListItem item)
        {
            int itemIndex = _logScrollView.LogList.getLogItemIndex(item.rectTransform);
            var logItem = logItems[itemIndex];
            var arg = logItem.arg as EventArg;
            var eventDefine = arg.define;
            if (eventDefine is IDescribableEventDefine describable)
            {
                var logInfoHori = _logScrollView.LogInfoHorizantal;
                logInfoHori.display();
                string info = lang._(describable.localizeTemplateString(_game, arg, _localPlayer), describable.localizeStringArgs(_game, arg, _localPlayer));
                info = processLogInfo(info, arg);
                logInfoHori.Text.text = info;

                var eventCard = describable.getCard(_game, arg, _localPlayer);
                if (eventCard != null)
                {
                    logInfoHori.InfoCard.display();
                    await setCardUIByEvent(logInfoHori.InfoCard.Card, eventCard, arg);
                }
                else
                    logInfoHori.InfoCard.hide();

                logInfoHori.TargetGridList.clearItems();
                var targets = describable.getTargets(_game, arg, _localPlayer);
                if (targets != null && targets.Length > 0)
                {
                    logInfoHori.TargetGridList.gameObject.SetActive(true);
                    foreach (var target in targets)
                    {
                        if (target == null)
                            continue;
                        var targetItem = logInfoHori.TargetGridList.addItem();

                        await setCardUIByEvent(targetItem.Card, target, arg);
                    }
                }
                else
                {
                    logInfoHori.TargetGridList.gameObject.SetActive(false);
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(logInfoHori.TargetGridList.rectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(logInfoHori.LogInfoVerticalVerticalLayoutGroup.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(logInfoHori.rectTransform);
            }
        }
        void onLogListItemExit(LogListItem item)
        {
            _logScrollView.LogInfoHorizantal.hide();
        }
        #endregion

        private void addLogItem(IEventArg arg)
        {
            var item = new LogItem()
            {
                arg = arg
            };
            logItems.Add(item);
            _logScrollView.LogList.updateLogListCount(logItems.Count);
        }
        private string processLogInfo(string info, EventArg arg)
        {
            info = Regex.Replace(info, cardDefinePattern,
                m =>
                {
                    if (int.TryParse(m.Groups["id"].Value, out var id) && long.TryParse(m.Groups["poolId"].Value, out var poolId))
                    {
                        return tableManager.getCardSkin(poolId, id).name;
                    }
                    return "未知卡牌";
                });
            info = Regex.Replace(info, cardPattern,
                m =>
                {
                    if (int.TryParse(m.Groups["id"].Value, out var id))
                    {
                        var card = tableManager.game.getCard(id);
                        CardSkinData skin = null;
                        if (card != null)
                        {
                            var cardSnapshot = getCardSnapshotOfEvent(card, arg);
                            var define = cardSnapshot?.define ?? card.define;
                            skin = tableManager.getCardSkin(define);
                        }
                        if (skin != null)
                        {
                            return skin.name;
                        }
                    }
                    return "未知卡牌";
                });
            return info;
        }
        private CardSnapshot getCardSnapshotOfEvent(Card card, IEventArg arg)
        {
            return _game.getCardSnapshotBeforeEvent(card, arg);
        }
        private CardDefine getCardDefineByEvent(DescribableCard card, IEventArg arg)
        {
            if (card == null)
                return null;
            if (card.card is Card cardObj)
            {
                if (arg != null)
                {
                    return getCardSnapshotOfEvent(cardObj, arg)?.define;
                }
                return cardObj.define;
            }
            else if (card.define is CardDefine cardDefine)
            {
                return cardDefine;
            }
            return null;
        }
        private async Task<Sprite> getCardImageByEvent(DescribableCard card, IEventArg arg)
        {
            var cardDefine = getCardDefineByEvent(card, arg);
            if (cardDefine == null)
                return _main.card.getDefaultSprite();
            return await tableManager.loadCardImage(cardDefine);
        }
        private async Task setCardUIByEvent(UICard ui, DescribableCard card, IEventArg arg)
        {
            if (ui == null || card == null)
                return;
            if (card.card is Card cardObj)
            {
                if (arg != null)
                {
                    CardSnapshot cardSnapshot = getCardSnapshotOfEvent(cardObj, arg);
                    if (cardSnapshot != null)
                    {
                        var descriptor = new CardDescriptor(_game, cardSnapshot);
                        await ui.setCard(descriptor, tableManager);
                        return;
                    }
                }
                await ui.setCardDefine(cardObj.define, tableManager);
            }
            else if (card.define is CardDefine cardDefine)
            {
                await ui.setCardDefine(cardDefine, tableManager);
            }
        }
        public LangManager lang => _main.lang;
        public BattleTableManager tableManager => _battleTableManager;

        [SerializeField]
        private LogPanel _logPanel;
        [SerializeField]
        private LogScrollView _logScrollView;
        private MainManager _main;
        private BattleTableManager _battleTableManager;
        private CardEngine _game;
        private Player _localPlayer;
        ActionEventCache gameTriggerEventCache { get; } = new ActionEventCache();
        private List<LogItem> logItems = new List<LogItem>();
        private const string cardDefinePattern = @"{cardDefine:(?<poolId>-?\d+),(?<id>-?\d+)}";
        private const string cardPattern = @"{card:(?<id>-?\d+)}";
    }
}
