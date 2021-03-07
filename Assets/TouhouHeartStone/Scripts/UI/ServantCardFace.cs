using UnityEngine.UI;
using System;
using TouhouHeartstone;
using UnityEngine.Events;
using UnityEngine;

namespace UI
{
    partial class ServantCardFace
    {
        [SerializeField]
        private Text _raceNameText;
        public Text raceNameText => _raceNameText;

        public int Charactertype
        {
            set
            {
                switch (value)
                {
                    case CATEGORY.CHARACTER_NEUTRAL:
                        break;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            init();
        }
    }
}
