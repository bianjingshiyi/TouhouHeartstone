using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using BJSYGameCore;
using TouhouHeartstone;
using Game;
public class ChooseOneAnim : EventAnimation<THHPlayer.ChooseOneEventArg>
{

    Card _ChooseOne;
    TableAnimation _anim;

    public override bool update(TableManager table, THHPlayer.ChooseOneEventArg eventArg, float time)
    {
        //自己打出的效果不展示
        if (eventArg.player == table.player)
            return true;
        if (table.ui.Waiting.activeSelf)
            table.ui.Waiting.SetActive(false);
        if (_ChooseOne == null && eventArg.choose != null && eventArg.isShow)
        {
            _ChooseOne = table.createInvokeCard(eventArg.choose, table.ui.LeftDisplayImage.rectTransform.position, table.cardPrefab.rectTransform.rect.height*table.ui.SelfHandList.rectTransform.localScale.y * 1.2f, eventArg.isShow);
            _ChooseOne.asAnimator.SetBool("isFaceUp", eventArg.isShow);
        }
        if (!SimpleAnimHelper.update(table, ref _anim, _ChooseOne.ShowAnim, _ChooseOne.animator, time))
            return false;
        if (_ChooseOne != null)
            Object.Destroy(_ChooseOne.gameObject);
        return true;
    }
}
