using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LoadingCircle : MonoBehaviour
    {
        #region 公共方法
        public void LoadingUpdate(float loading)
        {
            circle.fillAmount = loading;
        }
        public void Display()
        {
            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        #endregion

        #region 生命周期
        private void Awake()
        {
            circle = transform.Find("Circle").GetComponent<Image>();
        }
        #endregion

        #region 属性字段
        private Image circle;
        #endregion
    }
}


