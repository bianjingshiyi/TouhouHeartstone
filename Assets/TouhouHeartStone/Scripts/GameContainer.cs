using System;

using UnityEngine;

namespace TouhouHeartstone
{
    public class GameContainer : MonoBehaviour
    {
        [SerializeField]
        Backend.Game _game;
        protected void Start()
        {
            _game = new Backend.Game((int)DateTime.Now.ToBinary());
            _game.start();
        }
    }
}