using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BJSYGameCore;
using TouhouHeartstone;
using UI;
using Game;
using System.Linq;

public class SearchAnim : EventAnimation<THHPlayer.SearchPileEventArg>
{
    Card _ChooseOne;
    TableAnimation _ChooseAnim;
    public override bool update(TableManager table, THHPlayer.SearchPileEventArg eventArg, float time)
    {
        //自己打出的效果不展示
        if (eventArg.player == table.player)
            return true;
        if (table.ui.Waiting.activeSelf)
            table.ui.Waiting.SetActive(false);
        if (_ChooseOne == null && eventArg.choose != null && eventArg.isShow)
        {
            _ChooseOne = table.createInvokeCard(eventArg.choose, table.ui.LeftDisplayImage.rectTransform.position, table.ui.SelfDeck.rectTransform.rect.height, eventArg.isShow);
            _ChooseOne.asAnimator.SetBool("isFaceUp", eventArg.isShow);
        }
        if (_ChooseAnim == null && eventArg.choose != null)
            _ChooseAnim = (TableAnimation)new WaitAnim(0.5f);
        if (_ChooseAnim != null && !_ChooseAnim.update(table, time))
            return false;
        if (_ChooseOne != null)
            Object.Destroy(_ChooseOne.gameObject);
        return true;
    }
}
