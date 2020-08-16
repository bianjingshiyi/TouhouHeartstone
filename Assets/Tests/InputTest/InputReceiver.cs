using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BJSYGameCore.UI;
using UnityEngine.UI;
using BJSYGameCore;
namespace Tests
{
    public class InputReceiver : MonoBehaviour
    {
        [SerializeField]
        GameObject _target;
        void Update()
        {
            _target.SetActive(this.findInstance<InputManager>().isPointerOver(GetComponent<RectTransform>()));
        }
    }
}