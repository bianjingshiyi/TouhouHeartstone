using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Game;

public class MissionTriggerAnim : EventAnimation<THHCard.MissionCompleteArg>
{
    AnimAnim _anim;
    public override bool update(TableManager table, THHCard.MissionCompleteArg eventArg, float time)
    {
        if (table.tryGetSecret(eventArg.card, out var secret))
        {
            if (_anim == null)
            {
                secret.getChild("DummyTarget").GetComponent<RectTransform>().position = table.ui.CenterDisplayImage.rectTransform.position;
            }
            if (!SimpleAnimHelper.update(table, ref _anim, secret.onTrigger, secret.animator, time))
            {
                table.setCard(secret.Card, eventArg.card, true);
                return false;
            }
        }
        return true;
    }
}
