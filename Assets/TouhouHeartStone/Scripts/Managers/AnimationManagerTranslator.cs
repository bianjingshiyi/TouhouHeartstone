using System.Collections.Generic;
using System.Linq;
using MukioI18n;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(AnimationManager))]
    public class AnimationManagerTranslator : TranslateComponent<AnimationManager>
    {
        [SerializeField]
        string comment;

        [SerializeField]
        string context;

        public override void Translate()
        {
        }

        protected override string getCommentInner()
        {
            return comment;
        }

        protected override string getContextInner()
        {
            return context;
        }

        protected override string getKeyInner()
        {
            return null;
        }
        protected override IEnumerable<string> getKeysInner()
        {
            return Component.getBuiltinEffectNames();
        }
    }
}
