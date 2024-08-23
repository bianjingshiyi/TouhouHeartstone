using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BattleTable
{
    public class AnimationGroup : List<TableAnimation>
    {
        public void update(BattleTableManager table, float deltaTime)
        {
            for (int i = 0; i < Count; i++)
            {
                TableAnimation anim = this[i];
                bool isBlocked = false;
                if (i != 0)
                {
                    //第一个永远不被阻挡
                    for (int j = 0; j < i; j++)
                    {
                        TableAnimation prevAnim = this[j];
                        if (prevAnim.blockAnimInGroup(anim))
                        {
                            isBlocked = true;
                            break;
                        }
                    }
                }
                if (isBlocked)
                    continue;
                try
                {
                    if (anim.update(table, deltaTime))
                    {
                        RemoveAt(i);
                        i--;
                    }
                }
                catch (Exception e)
                {
                    RemoveAt(i);
                    i--;
                    Debug.LogError($"动画{anim}播放异常被跳过：{e}");
                }
            }
        }
        public void addToGroup(TableAnimation anim)
        {
            Add(anim);
            anim.group = this;
        }
        public bool continueNextGroup(AnimationGroup nextGroup)
        {
            foreach (TableAnimation anim in this)
            {
                if (!anim.continueNextGroup(nextGroup))
                {
                    return false;
                }
            }
            return true;
        }
        public override string ToString()
        {
            return string.Join(",", this);
        }
    }
}
