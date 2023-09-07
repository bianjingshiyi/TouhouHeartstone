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
            switch (obj)
            {
                case HearthStoneRule.UseEventArg _:
                //case THHPlayer.SetGemEventArg _:
                //case THHPlayer.SetMaxGemEventArg _:
                case HearthStoneRule.BurnEventArg _:
                case HearthStoneRule.FatigueEventArg _:
                case THHCard.DiscardEventArg _:
                case HearthStoneRule.CreateTokenEventArg _:
                case HearthStoneRule.RebornEventArg _:
                case THHCard.AttackEventArg _:
                case THHCard.DamageEventArg _:
                case THHCard.SetArmorEventArg _:
                case THHCard.HealEventArg _:
                case THHCard.SilenceEventArg _:
                case THHCard.DeathEventArg _:
                case THHPlayer.DiscoverEventArg _:
                case THHPlayer.RevealDuelEventArg _:
                case THHCard.SecretTriggerEventArg _:
                    addLogItem(obj);
                    break;
                case HearthStoneRule.DrawEventArg _:
                    if (obj is IDescribableEventArg describable)
                    {
                        // string info = describable.toString(_game, localPlayer);
                        string info = lang._(
                            describable.localizeTemplateString(_game, _localPlayer),
                            describable.localizeStringArgs(_game, _localPlayer)
                        );
                        if (info != null)
                        {
                            addLogItem(obj);
                        }
                    }
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
        private void onUpdateLogListItem(int index, RectTransform rect)
        {
            var item = rect.GetComponent<LogListItem>();
            var logItem = logItems[index];
            var obj = logItem.arg;
            item.SetLevel(logItem.level);
            switch (obj)
            {
                case HearthStoneRule.DrawEventArg draw:
                    item.IconImage.sprite = item.drawIcon;
                    item.Text.text = lang._(GAME_MANAGER_DRAW);
                    item.Image.sprite = tableManager.getSkin(draw.player == _localPlayer ? draw.card : draw.player.getMaster()).image;
                    item.BorderImage.sprite = draw.player == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case HearthStoneRule.BurnEventArg burn:

                    item.IconImage.sprite = item.burnIcon;
                    item.Text.text = lang._(GAME_MANAGER_BURN);
                    item.Image.sprite = tableManager.getSkin(burn.card).image;
                    item.BorderImage.sprite = burn.player == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case HearthStoneRule.FatigueEventArg fatigue:
                    item.IconImage.sprite = item.fatigueIcon;
                    item.Text.text = lang._(GAME_MANAGER_FATIGUE);
                    item.Image.sprite = tableManager.getSkin(fatigue.player.getMaster()).image;
                    item.BorderImage.sprite = fatigue.player == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.DiscardEventArg discard:
                    item.IconImage.sprite = item.discardIcon;
                    item.Text.text = lang._(GAME_MANAGER_DISCARD);
                    item.Image.sprite = tableManager.getSkin(discard.card).image;
                    item.BorderImage.sprite = discard.player == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case HearthStoneRule.UseEventArg use:
                    item.IconImage.sprite = item.useIcon;
                    item.Text.text = lang._(GAME_MANAGER_USE);
                    item.Image.sprite = use.card.isTrap(_game) ?
                        tableManager.getSkin(use.card.getOwner().getMaster()).image :
                        tableManager.getSkin(use.card).image;
                    item.BorderImage.sprite = use.player == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case HearthStoneRule.CreateTokenEventArg token:
                    item.IconImage.sprite = item.rebornIcon;
                    item.Text.text = lang._(GAME_MANAGER_TOKEN);
                    item.Image.sprite = tableManager.getSkin(token.card).image;
                    item.BorderImage.sprite = token.player == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case HearthStoneRule.RebornEventArg reborn:
                    item.IconImage.sprite = item.rebornIcon;
                    item.Text.text = lang._(GAME_MANAGER_REBORN);
                    item.Image.sprite = tableManager.getSkin(reborn.card).image;
                    item.BorderImage.sprite = reborn.player == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.AttackEventArg attack:
                    item.IconImage.sprite = item.attackIcon;
                    item.Text.text = lang._(GAME_MANAGER_ATTACK);
                    item.Image.sprite = tableManager.getSkin(attack.card).image;
                    item.BorderImage.sprite = attack.card.getOwner() == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.DamageEventArg damage:
                    item.IconImage.sprite = item.damageIcon;
                    item.Text.text = "-" + damage.damagedValue;
                    item.Image.sprite = tableManager.getSkin(damage.target).image;
                    item.BorderImage.sprite = damage.target.getOwner() == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.SetArmorEventArg armor:
                    item.IconImage.sprite = item.armorIcon;
                    item.Text.text = "=>" + armor.value;
                    item.Image.sprite = tableManager.getSkin(armor.card).image;
                    item.BorderImage.sprite = armor.card.getOwner() == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.HealEventArg heal:
                    var card = heal.card;
                    var healedValue = heal.healedValue;
                    item.IconImage.sprite = item.healIcon;
                    item.Text.text = "+" + healedValue;
                    item.Image.sprite = tableManager.getSkin(card).image;
                    item.BorderImage.sprite = card.getOwner() == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.SilenceEventArg silence:
                    item.IconImage.sprite = item.silenceIcon;
                    item.Text.text = lang._(GAME_MANAGER_SILENCE);
                    item.Image.sprite = tableManager.getSkin(silence.card).image;
                    item.BorderImage.color = silence.card.getOwner() == _localPlayer ? item.selfColor : item.oppoColor;
                    break;
                case THHCard.SecretTriggerEventArg secret:
                    item.IconImage.sprite = item.secretIcon;
                    item.Text.text = lang._(GAME_MANAGER_SECRET);
                    item.Image.sprite = tableManager.getSkin(secret.card).image;
                    item.BorderImage.sprite = secret.card.getOwner() == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHCard.DeathEventArg death:
                    item.IconImage.sprite = item.deathIcon;
                    item.Text.text = lang._(GAME_MANAGER_DEATH);
                    item.Image.sprite = tableManager.getSkin(death.card).image;
                    item.BorderImage.sprite = death.player == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                case THHPlayer.DiscoverEventArg discover:
                    item.Text.text = lang._(GAME_MANAGER_DISCOVER);
                    break;
                case THHPlayer.RevealDuelEventArg revealDuel:
                    item.Text.text = lang._(GAME_MANAGER_REVEAL_DUEL);
                    item.BorderImage.sprite = revealDuel.player == _localPlayer ? item.selfsprite : item.opposprite;
                    break;
                default:
                    break;
            }
        }
        private async void onLogListItemEnter(LogListItem item)
        {
            int itemIndex = _logScrollView.LogList.getLogItemIndex(item.rectTransform);
            var logItem = logItems[itemIndex];
            var arg = logItem.arg;
            if (arg is IDescribableEventArg describable)
            {
                var logInfoHori = _logScrollView.LogInfoHorizantal;
                logInfoHori.display();
                string info = lang._(describable.localizeTemplateString(_game, _localPlayer), describable.localizeStringArgs(_game, _localPlayer));
                info = Regex.Replace(info, @"{card:(?<poolId>-?\d+),(?<id>-?\d+)}",
                    m =>
                    {
                        if (int.TryParse(m.Groups["id"].Value, out var id) && long.TryParse(m.Groups["poolId"].Value, out var poolId))
                        {
                            var cardPoolRef = cards.getCardPoolReferenceInSet(poolId, _game?.getOption()?.cardPools);
                            return cards.getDefaultSkin(cardPoolRef, id).name;
                        }
                        return "未知卡牌";
                    });
                logInfoHori.Text.text = info;

                var eventCard = describable.getCard(_game, _localPlayer);
                if (eventCard != null)
                {
                    logInfoHori.InfoCard.display();
                    await setCardUIByEvent(logInfoHori.InfoCard.Card, eventCard, arg);
                }
                else
                    logInfoHori.InfoCard.hide();

                logInfoHori.TargetGridList.clearItems();
                var targets = describable.getTargets(_game, _localPlayer);
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
        private async Task setCardUIByEvent(UICard ui, ICard card, IEventArg arg)
        {
            if (ui == null || card == null)
                return;
            var cardObj = card as Card;
            if (cardObj != null)
            {
                EventRecord record = _game.triggers.getEventRecord(arg);
                if (record != null)
                {
                    CardSnapshot cardSnapshot = _game.getCardSnapshotOfEvent(cardObj, record);
                    if (cardSnapshot != null)
                    {
                        var descriptor = new CardDescriptor(_game, cardSnapshot);
                        await ui.setCard(descriptor, _main.card);
                        return;
                    }
                }
            }
            await ui.setCardDefine(card.define, _main.card);
        }

        public CardManager cards => _main.card;
        public LangManager lang => _main.lang;
        public BattleTableManager tableManager => _battleTableManager;

        [TranslateMsg]
        private const string GAME_MANAGER_DEATH = "死亡";
        [TranslateMsg]
        private const string GAME_MANAGER_SECRET = "陷阱";
        [TranslateMsg]
        private const string GAME_MANAGER_SILENCE = "封印";
        [TranslateMsg]
        private const string GAME_MANAGER_ATTACK = "攻击";
        [TranslateMsg]
        private const string GAME_MANAGER_REBORN = "复活";
        [TranslateMsg]
        private const string GAME_MANAGER_TOKEN = "召唤";
        [TranslateMsg]
        private const string GAME_MANAGER_USE = "使用";
        [TranslateMsg]
        private const string GAME_MANAGER_DISCARD = "弃牌";
        [TranslateMsg]
        private const string GAME_MANAGER_FATIGUE = "疲劳";
        [TranslateMsg]
        private const string GAME_MANAGER_BURN = "爆牌";
        [TranslateMsg]
        private const string GAME_MANAGER_DRAW = "抽牌";
        [TranslateMsg]
        private const string GAME_MANAGER_DISCOVER = "发现";
        [TranslateMsg]
        private const string GAME_MANAGER_REVEAL_DUEL = "信仰对决";
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
    }
}
