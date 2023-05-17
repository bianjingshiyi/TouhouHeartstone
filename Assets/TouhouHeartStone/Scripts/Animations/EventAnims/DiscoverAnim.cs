using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BJSYGameCore;
using TouhouHeartstone;
using UI;
using Game;
using System.Linq;

public class DiscoverAnim : EventAnimation<THHPlayer.DiscoverEventArg>
{
    Card _Discover;
    TableAnimation _anim;
    public override bool forceQueueAnimations => true;
    public override bool update(TableManager table, THHPlayer.DiscoverEventArg eventArg, float time)
    {
        if (eventArg.player == table.player)
            return true;
        if (table.ui.Waiting.activeSelf)
            table.ui.Waiting.SetActive(false);
        if (_Discover == null && eventArg.discovered != null && eventArg.isShow)
        {
            _Discover = table.createInvokeCard(eventArg.discovered, table.ui.LeftDisplayImage.rectTransform.position, table.cardPrefab.rectTransform.rect.height * table.ui.SelfHandList.rectTransform.localScale.y * 1.2f, eventArg.isShow);
            _Discover.asAnimator.SetBool("isFaceUp", eventArg.isShow);
        }
        if (!SimpleAnimHelper.update(table, ref _anim, _Discover.ShowAnim, _Discover.animator, time))
            return false;
        if (_Discover != null)
            Object.Destroy(_Discover.gameObject);
        return true;
    }
}
