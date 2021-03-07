using UnityEngine.UI;
using System;
using UnityEngine;

namespace UI
{
    partial class ServantCardFace
    {
        [SerializeField]
        private Text _raceNameText;
        public Text raceNameText => _raceNameText;

        protected override void Awake()
        {
            base.Awake();
            init();
        }
    }
}
