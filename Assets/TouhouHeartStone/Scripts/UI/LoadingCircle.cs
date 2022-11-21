using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LoadingCircle : MonoBehaviour
    {
        #region 私有方法
        public void OnLoadingUpdate(float loading)
        {
            circle.fillAmount = loading;
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


