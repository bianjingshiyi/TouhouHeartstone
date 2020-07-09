using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleToCanvas : MonoBehaviour
{
    [SerializeField]
    RectTransform _particle;
    [SerializeField]
    RectTransform _from;
    [SerializeField]
    RectTransform _to;
    //尝试做点什么，让粒子能从_from移动到_to，并且能在Canvas上渲染出来！

    [SerializeField]
    float step = 0.01f;

    bool dir = false;

    float Step => dir ? step : -step;

    float val = 0;

    private void Update()
    {
        if (val <= 0) dir = true;
        if (val >= 1) dir = false;

        val += Step;

        _particle.position = Vector3.Slerp(_from.position, _to.position, val);
    }
}
