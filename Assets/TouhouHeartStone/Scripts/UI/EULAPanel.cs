using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IGensoukyo;

namespace UI
{
    public class EULAPanel : MonoBehaviour
    {
        [SerializeField]
        HTMLRenderer _content;

        public void Show(string content)
        {
            _content.Html = content;
            gameObject.SetActive(true);
        }
    }
}
