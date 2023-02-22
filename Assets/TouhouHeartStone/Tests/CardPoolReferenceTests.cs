using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;
using Game;
using TouhouCardEngine;
using Controllers;

namespace Tests
{
    public class CardPoolReferenceTests
    {
        [Test]
        public void cardPoolReference_SerializationTest()
        {
            var cardPoolReference = new CardPoolReference(114514, 1919);
            Debug.Log(cardPoolReference);
            string json = cardPoolReference.toJson();
            Debug.Log(json);
            Debug.Log(JsonHelper.fromJson<CardPoolReference>(json));
        }
    }
}