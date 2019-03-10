using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

internal interface ICardAppearance
{
    int id { get; set; }
    string name { get; set; }
    Sprite image { get; set; }
    Sprite style { get; set; }
    string desc { get; set; }
    string[] category { get; set; }
    CardAnimation[] animations { get; set; }
}