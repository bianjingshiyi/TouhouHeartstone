using Game;
using System.Collections;
using System.Collections.Generic;
using TouhouHeartstone;
using UI;
using UnityEngine;

public class SelectAnim : EventAnimation<THHPlayer.SelectEventArg>
{
    public override bool update(TableManager table, THHPlayer.SelectEventArg eventArg, float time)
    {
        if (table.ui.Waiting.activeSelf)
            table.ui.Waiting.SetActive(false);
        return true;
    }
}
