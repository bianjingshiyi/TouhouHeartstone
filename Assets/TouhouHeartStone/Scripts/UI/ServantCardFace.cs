using UnityEngine.UI;
using System;
using TouhouHeartstone;
using UnityEngine.Events;
using UnityEngine;

namespace UI
{
    partial class ServantCardFace : MonoBehaviour
    {
        public int Charactertype
        {
            set
            {
                switch (value)
                {
                    case CATEGORY.CHARACTER_NEUTRAL:
                        asAnimator.SetBool(ANIM_PARAM_ISRIN, false);
                        asAnimator.SetBool(ANIM_PARAM_ISYOUMU, false);
                        asAnimator.SetBool(ANIM_PARAM_ISSANAE, false);
                        asAnimator.SetBool(ANIM_PARAM_ISPATCHOULI, false);
                        asAnimator.SetBool(ANIM_PARAM_ISDEFINE, true);
                        asAnimator.SetBool(ANIM_PARAM_ISREISEN, false);
                        _onCharaterControllerNEUTRAL.Invoke();
                        break;
                    case CATEGORY.CHARACTER_RIN:
                        asAnimator.SetBool(ANIM_PARAM_ISRIN, true);
                        asAnimator.SetBool(ANIM_PARAM_ISYOUMU,false);
                        asAnimator.SetBool(ANIM_PARAM_ISSANAE, false);
                        asAnimator.SetBool(ANIM_PARAM_ISPATCHOULI, false);
                        asAnimator.SetBool(ANIM_PARAM_ISDEFINE, false);
                        asAnimator.SetBool(ANIM_PARAM_ISREISEN, false);
                        _onCharaterControllerRIN.Invoke();
                        break;
                    case CATEGORY.CHARACTER_PATCHOULI:
                        asAnimator.SetBool(ANIM_PARAM_ISRIN, false);
                        asAnimator.SetBool(ANIM_PARAM_ISYOUMU, false);
                        asAnimator.SetBool(ANIM_PARAM_ISSANAE, false);
                        asAnimator.SetBool(ANIM_PARAM_ISPATCHOULI, true);
                        asAnimator.SetBool(ANIM_PARAM_ISDEFINE, false);
                        asAnimator.SetBool(ANIM_PARAM_ISREISEN, false);
                        _onCharaterControllerPATCHOULI.Invoke();
                        break;
                    case CATEGORY.CHARACTER_REISEN:
                        asAnimator.SetBool(ANIM_PARAM_ISRIN, false);
                        asAnimator.SetBool(ANIM_PARAM_ISYOUMU, false);
                        asAnimator.SetBool(ANIM_PARAM_ISSANAE, false);
                        asAnimator.SetBool(ANIM_PARAM_ISPATCHOULI, false);
                        asAnimator.SetBool(ANIM_PARAM_ISDEFINE, false);
                        asAnimator.SetBool(ANIM_PARAM_ISREISEN, true);
                        _onCharaterControllerREISEN.Invoke();
                        break;
                    case CATEGORY.CHARACTER_SANAE:
                        asAnimator.SetBool(ANIM_PARAM_ISRIN, false);
                        asAnimator.SetBool(ANIM_PARAM_ISYOUMU, false);
                        asAnimator.SetBool(ANIM_PARAM_ISSANAE, true);
                        asAnimator.SetBool(ANIM_PARAM_ISPATCHOULI, false);
                        asAnimator.SetBool(ANIM_PARAM_ISDEFINE, false);
                        asAnimator.SetBool(ANIM_PARAM_ISREISEN, false);
                        _onCharaterControllerSANAE.Invoke();
                        break;
                    case CATEGORY.CHARACTER_YOUMU:
                        asAnimator.SetBool(ANIM_PARAM_ISRIN, false);
                        asAnimator.SetBool(ANIM_PARAM_ISYOUMU, true);
                        asAnimator.SetBool(ANIM_PARAM_ISSANAE, false);
                        asAnimator.SetBool(ANIM_PARAM_ISPATCHOULI, false);
                        asAnimator.SetBool(ANIM_PARAM_ISDEFINE, false);
                        asAnimator.SetBool(ANIM_PARAM_ISREISEN, false);
                        _onCharaterControllerYOUMU.Invoke();
                        break;
                }
            }
        }

        [SerializeField]
        private UnityEvent _onCharaterControllerNEUTRAL;
        [SerializeField]
        private UnityEvent _onCharaterControllerRIN;
        [SerializeField]
        private UnityEvent _onCharaterControllerYOUMU;
        [SerializeField]
        private UnityEvent _onCharaterControllerSANAE;
        [SerializeField]
        private UnityEvent _onCharaterControllerPATCHOULI;
        [SerializeField]
        private UnityEvent _onCharaterControllerREISEN;


        protected void Awake()
        {
            autoInit();
        }
    }
}
