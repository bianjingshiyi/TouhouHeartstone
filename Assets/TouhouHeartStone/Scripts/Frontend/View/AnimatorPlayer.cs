using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TouhouHeartstone.Frontend.View
{
    [RequireComponent(typeof(Animator))]
    class AnimatorPlayer : MonoBehaviour
    {
        Animator animator => GetComponent<Animator>();

        [SerializeField]
        string animateName = "";


        GenericAction playCallback;
        public void OnPlayFinish()
        {
            playCallback?.Invoke(this, null);
        }

        /// <summary>
        /// 播放一个预定义的动画
        /// </summary>
        /// <param name="callback"></param>
        public void Play(GenericAction callback = null)
        {
            playCallback = callback;
            animator.Play(animateName);
        }

        Dictionary<string, GenericAction> callbackDict = new Dictionary<string, GenericAction>();

        public void OnPlayFinish(string name)
        {
            if (callbackDict.ContainsKey(name))
            {
                callbackDict[name]?.Invoke(this, null);
            }
        }

        /// <summary>
        /// 播放指定名字的动画
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        public void Play(string name, GenericAction callback = null)
        {
            callbackDict[name] = callback;
            animator.Play(name);
        }
    }
}
