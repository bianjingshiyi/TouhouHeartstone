using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnEndClock : MonoBehaviour
{
    [SerializeField]
    RectTransform IndLong;

    [SerializeField]
    RectTransform IndShort;

    int turnCounter = 0;

    bool lastSelf = false;

    /// <summary>
    /// 设置回合
    /// </summary>
    /// <param name="turn"></param>
    public void SetTurn(int turn)
    {
        IndShort.transform.rotation = Quaternion.Euler(0, 0, - turn * 360 / 12);
        turnCounter = turn;
    }

    /// <summary>
    /// 使用内置的计数器设置回合状态
    /// </summary>
    /// <param name="self"></param>
    public void SetTurnState(bool self)
    {
        if (lastSelf != self)
        {
            lastSelf = self;
            if (self)
            {
                SetTurn(turnCounter + 1);
            }
        }
    }

    /// <summary>
    /// 设置倒计时
    /// </summary>
    /// <param name="sec"></param>
    public void SetCountdown(float sec)
    {
        if (sec > 60) 
            sec = 60;
        IndLong.transform.rotation = Quaternion.Euler(0, 0, sec * 360 / 60);
    }
}
