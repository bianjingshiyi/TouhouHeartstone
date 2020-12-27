using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    partial class PropNumber
    {
        protected override void Awake()
        {
            base.Awake();
            init();
        }
        public SimpleAnim RedText;
        public SimpleAnim WhiteText;
        public SimpleAnim GreenText;
    }
}

