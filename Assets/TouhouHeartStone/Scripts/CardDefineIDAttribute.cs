using System;
using UnityEngine;
namespace Game
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class CardDefineIDAttribute : PropertyAttribute
    {
    }
}
