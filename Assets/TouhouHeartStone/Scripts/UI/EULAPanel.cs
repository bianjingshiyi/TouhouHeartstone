using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EULAPanel : MonoBehaviour
    {
        [SerializeField]
        Text _content;

        public void Show(string content)
        {
            _content.text = content;
            gameObject.SetActive(true);
        }
    }
}
